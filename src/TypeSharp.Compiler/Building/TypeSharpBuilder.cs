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
    public static BuildResult Build(string manifestPath)
    {
        var diagnostics = new List<Diagnostic>();
        var generatedFiles = new List<GeneratedCSharpFile>();
        GeneratedCSharpProject? generatedProject = null;
        GeneratedAssembly? generatedAssembly = null;
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

        var outputRoot = Path.GetFullPath(Path.Combine(
            manifestResult.Manifest.ProjectDirectory,
            manifestResult.Manifest.Project.GeneratedOutputRoot));

        foreach (var (sourceFile, root) in parsedSources)
        {
            var relativePath = ToGeneratedRelativePath(sourceFile.RelativePath);
            var outputPath = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? outputRoot);
            File.WriteAllText(outputPath, CSharpSourceBackend.Emit(root));
            generatedFiles.Add(new GeneratedCSharpFile(outputPath, relativePath));
        }

        var projectRelativePath = $"{SanitizeFileName(manifestResult.Manifest.Project.Name)}.Generated.csproj";
        var projectPath = Path.Combine(outputRoot, projectRelativePath);
        Directory.CreateDirectory(outputRoot);
        File.WriteAllText(projectPath, EmitGeneratedProject(manifestResult.Manifest, referenceResult.References, outputRoot));
        File.WriteAllText(Path.Combine(outputRoot, "NuGet.config"), EmitOfflineNuGetConfig());
        generatedProject = new GeneratedCSharpProject(projectPath, projectRelativePath);

        var projectBuild = BuildGeneratedProject(outputRoot, generatedProject.RelativePath);
        if (projectBuild.ExitCode != 0)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.GeneratedProjectBuildFailed,
                $"Generated C# project build failed for '{generatedProject.RelativePath}' with exit code {projectBuild.ExitCode}.",
                manifestResult.Manifest.ManifestPath));
            return new BuildResult(generatedFiles, generatedProject, null, diagnostics);
        }

        var assemblyRelativePath = ToGeneratedAssemblyRelativePath(manifestResult.Manifest);
        var assemblyPath = Path.Combine(outputRoot, assemblyRelativePath.Replace('/', Path.DirectorySeparatorChar));
        generatedAssembly = new GeneratedAssembly(assemblyPath, assemblyRelativePath);

        return new BuildResult(generatedFiles, generatedProject, generatedAssembly, diagnostics);
    }

    private static string ToGeneratedRelativePath(string sourceRelativePath)
    {
        var normalized = sourceRelativePath.Replace('\\', '/');
        var withoutExtension = normalized.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? normalized[..^".tysh".Length]
            : normalized;

        return $"{withoutExtension}.g.cs";
    }

    private static string EmitGeneratedProject(
        TypeSharpManifest manifest,
        IReadOnlyList<ResolvedReference> references,
        string outputRoot)
    {
        var targetFramework = string.IsNullOrWhiteSpace(manifest.Project.TargetFramework)
            ? TypeSharpCompilerInfo.DefaultTargetFramework
            : manifest.Project.TargetFramework;
        var outputType = string.Equals(manifest.Project.OutputType, "exe", StringComparison.OrdinalIgnoreCase)
            ? "Exe"
            : "Library";
        var assemblyName = XmlEscape(manifest.Project.Name);
        var rootNamespace = XmlEscape(manifest.Project.RootNamespace ?? manifest.Project.Name);

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

    private static GeneratedProjectBuildResult BuildGeneratedProject(string outputRoot, string projectRelativePath)
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

    private static string ToGeneratedAssemblyRelativePath(TypeSharpManifest manifest)
    {
        var configuration = "Debug";
        var targetFramework = string.IsNullOrWhiteSpace(manifest.Project.TargetFramework)
            ? TypeSharpCompilerInfo.DefaultTargetFramework
            : manifest.Project.TargetFramework;
        return $"bin/{configuration}/{targetFramework}/{manifest.Project.Name}.dll";
    }

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
