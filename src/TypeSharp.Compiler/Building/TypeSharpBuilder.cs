using System.Diagnostics;
using System.Text;
using TypeSharp.Compiler.Backend;
using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Interop;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;
using TypeSharp.Compiler.TypeChecking;

namespace TypeSharp.Compiler.Building;

public static class TypeSharpBuilder
{
    public static BuildResult Build(string manifestPath, string configuration = "Debug", string? targetFrameworkOverride = null)
    {
        configuration = NormalizeBuildConfiguration(configuration);
        targetFrameworkOverride = NormalizeTargetFrameworkOverride(targetFrameworkOverride);
        var diagnostics = new List<Diagnostic>();
        var generatedFiles = new List<GeneratedCSharpFile>();
        GeneratedCSharpProject? generatedProject = null;
        GeneratedAssembly? generatedAssembly = null;
        var backend = CSharpSourceBackendAdapter.Instance;
        var parsedSources = new List<(SourceFile SourceFile, SyntaxNode Root)>();

        var manifestResult = TypeSharpManifestLoader.Load(manifestPath);
        diagnostics.AddRange(manifestResult.Diagnostics);
        if (manifestResult.Manifest is null)
        {
            return new BuildResult([], null, null, diagnostics);
        }

        var sourceDiscovery = SourceDiscovery.Discover(manifestResult.Manifest);
        diagnostics.AddRange(sourceDiscovery.Diagnostics);

        var referenceResult = TypeSharpReferenceResolver.Resolve(manifestResult.Manifest);
        var metadataResult = TypeSharpMetadataReader.Read(referenceResult);
        diagnostics.AddRange(metadataResult.Diagnostics);
        if (metadataResult.HasErrors)
        {
            return new BuildResult([], null, null, diagnostics);
        }

        foreach (var sourceFile in sourceDiscovery.SourceFiles)
        {
            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
            diagnostics.AddRange(parseResult.Diagnostics);
            if (parseResult.HasErrors || parseResult.Root is null)
            {
                continue;
            }

            diagnostics.AddRange(TypeSharpInteropValidator.Validate(
                parseResult.Root,
                metadataResult.Assemblies,
                sourceFile.RelativePath,
                manifestResult.Manifest.Language.Nullable));
            var bindingResult = TypeSharpBinder.Bind(parseResult.Root, sourceFile.RelativePath);
            diagnostics.AddRange(bindingResult.Diagnostics);
            if (!bindingResult.HasErrors)
            {
                diagnostics.AddRange(TypeSharpTypeChecker.Check(parseResult.Root, sourceFile.RelativePath).Diagnostics);
            }

            parsedSources.Add((sourceFile, parseResult.Root));
        }

        if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return new BuildResult([], null, null, diagnostics);
        }

        if (IsExecutable(manifestResult.Manifest.Project.OutputType) &&
            !ValidateExecutableEntryPoint(manifestResult.Manifest, parsedSources, diagnostics))
        {
            return new BuildResult([], null, null, diagnostics);
        }

        var outputRoot = Path.GetFullPath(Path.Combine(
            manifestResult.Manifest.ProjectDirectory,
            manifestResult.Manifest.Project.GeneratedOutputRoot));

        var rootNamespace = GetRootNamespace(manifestResult.Manifest);
        foreach (var (sourceFile, root) in parsedSources)
        {
            var relativePath = ToGeneratedRelativePath(sourceFile.RelativePath, backend);
            var outputPath = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var artifact = backend is CSharpSourceBackendAdapter csharpBackend
                ? csharpBackend.Emit(root, rootNamespace)
                : backend.Emit(root);
            if (artifact.Kind != TypeSharpBackendArtifactKind.SourceText)
            {
                throw new NotSupportedException($"Backend '{backend.Name}' emits '{artifact.Kind}' artifacts, but the current project builder expects generated source text.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? outputRoot);
            File.WriteAllText(outputPath, artifact.RequireText());
            generatedFiles.Add(new GeneratedCSharpFile(outputPath, relativePath));
        }

        if (IsExecutable(manifestResult.Manifest.Project.OutputType) &&
            TryGetMainEntryPoint(manifestResult.Manifest, out var mainNamespace, out var mainMethod))
        {
            const string relativePath = "Program.g.cs";
            var outputPath = Path.Combine(outputRoot, relativePath);
            var mainInvocationArguments = GetMainInvocationArguments(parsedSources, mainNamespace, mainMethod, rootNamespace);
            Directory.CreateDirectory(outputRoot);
            File.WriteAllText(outputPath, EmitEntryPoint(mainNamespace, mainMethod, mainInvocationArguments));
            generatedFiles.Add(new GeneratedCSharpFile(outputPath, relativePath));
        }

        var projectRelativePath = $"{SanitizeFileName(manifestResult.Manifest.Project.Name)}.Generated.csproj";
        var projectPath = Path.Combine(outputRoot, projectRelativePath);
        Directory.CreateDirectory(outputRoot);
        var targetFramework = ResolveTargetFramework(manifestResult.Manifest, targetFrameworkOverride);
        File.WriteAllText(projectPath, EmitGeneratedProject(manifestResult.Manifest, referenceResult.References, outputRoot, targetFramework));
        File.WriteAllText(Path.Combine(outputRoot, "NuGet.config"), EmitOfflineNuGetConfig());
        generatedProject = new GeneratedCSharpProject(projectPath, projectRelativePath);

        var projectBuild = BuildGeneratedProject(outputRoot, generatedProject.RelativePath, configuration);
        if (projectBuild.ExitCode != 0)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.GeneratedProjectBuildFailed,
                $"Generated C# project build failed for '{generatedProject.RelativePath}' with exit code {projectBuild.ExitCode}.",
                manifestResult.Manifest.ManifestPath));
            return new BuildResult(generatedFiles, generatedProject, null, diagnostics);
        }

        var assemblyRelativePath = ToGeneratedAssemblyRelativePath(manifestResult.Manifest, configuration, targetFramework);
        var assemblyPath = Path.Combine(outputRoot, assemblyRelativePath.Replace('/', Path.DirectorySeparatorChar));
        generatedAssembly = new GeneratedAssembly(assemblyPath, assemblyRelativePath);

        return new BuildResult(generatedFiles, generatedProject, generatedAssembly, diagnostics);
    }

    private static string ToGeneratedRelativePath(string sourceRelativePath, ITypeSharpBackend backend)
    {
        var normalized = sourceRelativePath.Replace('\\', '/');
        var withoutExtension = normalized.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? normalized[..^".tysh".Length]
            : normalized;

        return $"{withoutExtension}{backend.GeneratedArtifactExtension}";
    }

    private static string EmitGeneratedProject(
        TypeSharpManifest manifest,
        IReadOnlyList<ResolvedReference> references,
        string outputRoot,
        string targetFramework)
    {
        var outputType = IsExecutable(manifest.Project.OutputType)
            ? "Exe"
            : "Library";
        var assemblyName = XmlEscape(manifest.Project.Name);
        var rootNamespace = XmlEscape(GetRootNamespace(manifest));

        var builder = new StringBuilder();
        builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        builder.AppendLine("  <PropertyGroup>");
        builder.AppendLine($"    <TargetFramework>{XmlEscape(targetFramework)}</TargetFramework>");
        builder.AppendLine($"    <OutputType>{outputType}</OutputType>");
        builder.AppendLine("    <LangVersion>7.3</LangVersion>");
        builder.AppendLine("    <ImplicitUsings>false</ImplicitUsings>");
        builder.AppendLine("    <Nullable>disable</Nullable>");
        builder.AppendLine($"    <AssemblyName>{assemblyName}</AssemblyName>");
        builder.AppendLine($"    <RootNamespace>{rootNamespace}</RootNamespace>");
        builder.AppendLine("  </PropertyGroup>");

        if (references.Count > 0)
        {
            builder.AppendLine("  <ItemGroup>");
            foreach (var reference in references)
            {
                EmitReferenceItem(builder, reference, outputRoot);
            }

            builder.AppendLine("  </ItemGroup>");
        }

        builder.Append("</Project>");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void EmitReferenceItem(StringBuilder builder, ResolvedReference reference, string outputRoot)
    {
        if (reference.Kind == ResolvedReferenceKind.FrameworkAssembly)
        {
            builder.AppendLine($"    <Reference Include=\"{XmlEscape(reference.Identity)}\" />");
            return;
        }

        var hintPath = ToGeneratedProjectHintPath(outputRoot, reference.Path ?? reference.OriginalText);
        builder.AppendLine($"    <Reference Include=\"{XmlEscape(reference.Identity)}\">");
        builder.AppendLine($"      <HintPath>{XmlEscape(hintPath)}</HintPath>");
        builder.AppendLine("    </Reference>");
    }

    private static string ToGeneratedProjectHintPath(string outputRoot, string referencePath)
    {
        var hintPath = Path.IsPathFullyQualified(referencePath)
            ? Path.GetRelativePath(outputRoot, referencePath)
            : referencePath;

        return NormalizePath(hintPath);
    }

    private static string EmitOfflineNuGetConfig() =>
        """
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <packageSources>
            <clear />
          </packageSources>
        </configuration>
        """.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string EmitEntryPoint(string namespaceName, string mainMethod, string mainInvocationArguments)
    {
        return $$"""
        // <auto-generated />

        namespace {{namespaceName}}
        {
            internal static class Program
            {
                public static int Main(string[] args)
                {
                    object result = Module.{{mainMethod}}({{mainInvocationArguments}});
                    if (result is int exitCode)
                    {
                        return exitCode;
                    }

                    if (result != null)
                    {
                        System.Console.WriteLine(result);
                    }

                    return 0;
                }
            }
        }
        """.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string GetMainInvocationArguments(
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        string namespaceName,
        string methodName,
        string defaultNamespace)
    {
        foreach (var (_, root) in parsedSources)
        {
            if (!string.Equals(GetNamespaceName(root, defaultNamespace), namespaceName, StringComparison.Ordinal))
            {
                continue;
            }

            var mainFunction = root.Children.FirstOrDefault(child =>
                child.Kind == SyntaxKind.FunctionDeclaration &&
                string.Equals(GetDeclarationName(child), methodName, StringComparison.Ordinal));
            if (mainFunction is not null && HasSingleStringArrayParameter(mainFunction))
            {
                return "args";
            }
        }

        return string.Empty;
    }

    private static bool HasSingleStringArrayParameter(SyntaxNode function)
    {
        var parameterList = function.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
        var parameters = parameterList?.Children.Where(child => child.Kind == SyntaxKind.Parameter).ToArray() ?? [];
        if (parameters.Length != 1)
        {
            return false;
        }

        var annotation = parameters[0].Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
        var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
        return IsStringArrayType(typeNode);
    }

    private static bool IsStringArrayType(SyntaxNode? node)
    {
        if (node?.Kind != SyntaxKind.ArrayType)
        {
            return false;
        }

        var elementType = node.Children.FirstOrDefault(child => !child.IsToken);
        return elementType?.Kind == SyntaxKind.TypeName &&
            string.Equals(GetQualifiedName(elementType), "string", StringComparison.Ordinal);
    }

    private static bool ValidateExecutableEntryPoint(
        TypeSharpManifest manifest,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetMainEntryPoint(manifest, out var namespaceName, out var methodName))
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.UnsupportedExecutableEntryPoint,
                "Executable project main must be a non-empty function name or qualified function name.",
                manifest.ManifestPath));
            return false;
        }

        return ValidateExecutableEntryPoint(parsedSources, namespaceName, methodName, GetRootNamespace(manifest), manifest.ManifestPath, diagnostics);
    }

    private static bool ValidateExecutableEntryPoint(
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        string namespaceName,
        string methodName,
        string defaultNamespace,
        string manifestPath,
        List<Diagnostic> diagnostics)
    {
        var mainFunction = FindFunction(parsedSources, namespaceName, methodName, defaultNamespace);
        if (mainFunction is null)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.UnsupportedExecutableEntryPoint,
                $"Executable project main '{namespaceName}.{methodName}' was not found.",
                manifestPath));
            return false;
        }

        if (HasNoParameters(mainFunction) || HasSingleStringArrayParameter(mainFunction))
        {
            return true;
        }

        diagnostics.Add(DiagnosticFactory.Manifest(
            DiagnosticDescriptors.UnsupportedExecutableEntryPoint,
            $"Executable project main '{namespaceName}.{methodName}' must have no parameters or exactly one 'string[]' parameter.",
            manifestPath));
        return false;
    }

    private static SyntaxNode? FindFunction(
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        string namespaceName,
        string methodName,
        string defaultNamespace)
    {
        foreach (var (_, root) in parsedSources)
        {
            if (!string.Equals(GetNamespaceName(root, defaultNamespace), namespaceName, StringComparison.Ordinal))
            {
                continue;
            }

            var function = root.Children.FirstOrDefault(child =>
                child.Kind == SyntaxKind.FunctionDeclaration &&
                string.Equals(GetDeclarationName(child), methodName, StringComparison.Ordinal));
            if (function is not null)
            {
                return function;
            }
        }

        return null;
    }

    private static bool HasNoParameters(SyntaxNode function)
    {
        var parameterList = function.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
        return parameterList is null || !parameterList.Children.Any(child => child.Kind == SyntaxKind.Parameter);
    }

    private static GeneratedProjectBuildResult BuildGeneratedProject(string outputRoot, string projectRelativePath, string configuration)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = outputRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.StartInfo.ArgumentList.Add("build");
        process.StartInfo.ArgumentList.Add(projectRelativePath);
        process.StartInfo.ArgumentList.Add("--configuration");
        process.StartInfo.ArgumentList.Add(configuration);
        process.StartInfo.ArgumentList.Add("--nologo");
        process.StartInfo.ArgumentList.Add("--verbosity");
        process.StartInfo.ArgumentList.Add("quiet");
        process.StartInfo.ArgumentList.Add("--ignore-failed-sources");

        process.Start();
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(milliseconds: 120_000))
        {
            process.Kill(entireProcessTree: true);
            return new GeneratedProjectBuildResult(-1, standardOutput, standardError);
        }

        return new GeneratedProjectBuildResult(process.ExitCode, standardOutput, standardError);
    }

    private static string ToGeneratedAssemblyRelativePath(TypeSharpManifest manifest, string configuration, string targetFramework)
    {
        var extension = IsExecutable(manifest.Project.OutputType) ? "exe" : "dll";
        return $"bin/{configuration}/{targetFramework}/{manifest.Project.Name}.{extension}";
    }

    private static string ResolveTargetFramework(TypeSharpManifest manifest, string? targetFrameworkOverride)
    {
        if (!string.IsNullOrWhiteSpace(targetFrameworkOverride))
        {
            return targetFrameworkOverride;
        }

        return string.IsNullOrWhiteSpace(manifest.Project.TargetFramework)
            ? TypeSharpCompilerInfo.DefaultTargetFramework
            : manifest.Project.TargetFramework;
    }

    private static string NormalizeBuildConfiguration(string configuration)
    {
        if (string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase))
        {
            return "Debug";
        }

        if (string.Equals(configuration, "Release", StringComparison.OrdinalIgnoreCase))
        {
            return "Release";
        }

        throw new ArgumentException("Build configuration must be 'Debug' or 'Release'.", nameof(configuration));
    }

    private static string? NormalizeTargetFrameworkOverride(string? targetFrameworkOverride)
    {
        if (targetFrameworkOverride is null)
        {
            return null;
        }

        if (string.Equals(targetFrameworkOverride, TypeSharpCompilerInfo.DefaultTargetFramework, StringComparison.OrdinalIgnoreCase))
        {
            return TypeSharpCompilerInfo.DefaultTargetFramework;
        }

        throw new ArgumentException("Target framework override must be 'net48'.", nameof(targetFrameworkOverride));
    }

    private static bool TryGetMainEntryPoint(TypeSharpManifest manifest, out string namespaceName, out string methodName)
    {
        var main = string.IsNullOrWhiteSpace(manifest.Project.Main)
            ? "main"
            : manifest.Project.Main.Trim();
        var lastDot = main.LastIndexOf('.');
        if (lastDot < 0)
        {
            namespaceName = GetRootNamespace(manifest);
            methodName = main;
            return methodName.Length > 0;
        }

        namespaceName = main[..lastDot];
        methodName = main[(lastDot + 1)..];
        return namespaceName.Length > 0 && methodName.Length > 0;
    }

    private static string GetRootNamespace(TypeSharpManifest manifest)
    {
        if (!string.IsNullOrWhiteSpace(manifest.Project.RootNamespace))
        {
            return manifest.Project.RootNamespace.Trim();
        }

        if (!string.IsNullOrWhiteSpace(manifest.Project.Name))
        {
            return manifest.Project.Name.Trim();
        }

        return "TypeSharp.Generated";
    }

    private static string GetNamespaceName(SyntaxNode root, string defaultNamespace)
    {
        var namespaceDeclaration = root.Children.FirstOrDefault(child => child.Kind == SyntaxKind.NamespaceDeclaration);
        var namespaceName = GetQualifiedName(namespaceDeclaration);
        return namespaceName.Length > 0 ? namespaceName : defaultNamespace;
    }

    private static string GetDeclarationName(SyntaxNode node)
    {
        var seenFunctionKeyword = false;
        foreach (var child in node.Children)
        {
            if (child.IsToken && child.Kind == SyntaxKind.FunKeyword)
            {
                seenFunctionKeyword = true;
                continue;
            }

            if (seenFunctionKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            {
                return child.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static string GetQualifiedName(SyntaxNode? node)
    {
        if (node is null)
        {
            return string.Empty;
        }

        var identifiers = node.Kind == SyntaxKind.TypeName
            ? node.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            : node.Children
                .Where(child => child.Kind == SyntaxKind.TypeName)
                .SelectMany(child => child.Children)
                .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);

        var parts = identifiers.Select(identifier => identifier.Text ?? string.Empty).Where(text => text.Length > 0);
        return string.Join(".", parts);
    }

    private static bool IsExecutable(string outputType) =>
        string.Equals(outputType, "exe", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "TypeSharp.Generated" : sanitized;
    }

    private static string XmlEscape(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);

    private sealed record GeneratedProjectBuildResult(int ExitCode, string StandardOutput, string StandardError);
}
