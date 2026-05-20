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

        var sourceAnalysisResults = sourceDiscovery.SourceFiles
            .AsParallel()
            .AsOrdered()
            .Select(sourceFile => AnalyzeSource(
                sourceFile,
                metadataResult.Assemblies,
                manifestResult.Manifest.Language.Nullable))
            .ToArray();
        foreach (var sourceAnalysisResult in sourceAnalysisResults)
        {
            diagnostics.AddRange(sourceAnalysisResult.Diagnostics);
            if (sourceAnalysisResult.Root is not null)
            {
                parsedSources.Add((sourceAnalysisResult.SourceFile, sourceAnalysisResult.Root));
            }
        }

        var sourceModules = parsedSources
            .Select(source => new SourceModule(source.SourceFile, source.Root))
            .ToArray();
        var sourceModuleGraph = SourceModuleGraph.Build(sourceModules);
        diagnostics.AddRange(sourceModuleGraph.Diagnostics);

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
        var sourceModuleTargets = BuildSourceModuleTargets(parsedSources, rootNamespace);
        foreach (var (sourceFile, root) in parsedSources)
        {
            var relativePath = ToGeneratedRelativePath(sourceFile.RelativePath, backend);
            var outputPath = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var moduleContainerName = GeneratedModuleContainerNaming.GetContainerName(sourceFile, parsedSources.Count);
            var sourceImports = BuildSourceImports(sourceFile, sourceModuleGraph, sourceModuleTargets);
            var valueImportAliases = BuildSourceValueImportAliases(sourceFile, root, sourceModuleGraph, sourceModuleTargets, parsedSources);
            var valueReExports = BuildSourceValueReExports(sourceFile, root, sourceModuleGraph, sourceModuleTargets, parsedSources);
            var functionImportAliases = BuildSourceFunctionImportAliases(sourceFile, root, sourceModuleGraph, sourceModuleTargets, parsedSources);
            var functionReExports = BuildLocalFunctionExportAliases(sourceFile, root, sourceModuleTargets)
                .Concat(BuildSourceFunctionReExports(sourceFile, root, sourceModuleGraph, sourceModuleTargets, parsedSources))
                .ToArray();
            var artifact = backend is CSharpSourceBackendAdapter csharpBackend
                ? csharpBackend.Emit(root, rootNamespace, moduleContainerName, sourceImports, valueImportAliases, valueReExports, functionImportAliases, functionReExports)
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
            var mainModuleContainerName = GetMainModuleContainerName(parsedSources, mainNamespace, mainMethod, rootNamespace);
            Directory.CreateDirectory(outputRoot);
            File.WriteAllText(outputPath, EmitEntryPoint(mainNamespace, mainModuleContainerName, mainMethod, mainInvocationArguments));
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

    private static string EmitEntryPoint(string namespaceName, string moduleContainerName, string mainMethod, string mainInvocationArguments)
    {
        return $$"""
        // <auto-generated />

        namespace {{namespaceName}}
        {
            internal static class Program
            {
                public static int Main(string[] args)
                {
                    object result = {{moduleContainerName}}.{{mainMethod}}({{mainInvocationArguments}});
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

    private static string GetMainModuleContainerName(
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        string namespaceName,
        string methodName,
        string defaultNamespace)
    {
        foreach (var (sourceFile, root) in parsedSources)
        {
            if (!string.Equals(GetNamespaceName(root, defaultNamespace), namespaceName, StringComparison.Ordinal))
            {
                continue;
            }

            var function = root.Children.FirstOrDefault(child =>
                child.Kind == SyntaxKind.FunctionDeclaration &&
                !IsAmbientDeclaration(child) &&
                string.Equals(GetDeclarationName(child), methodName, StringComparison.Ordinal));
            if (function is not null)
            {
                return GeneratedModuleContainerNaming.GetContainerName(sourceFile, parsedSources.Count);
            }
        }

        return "Module";
    }

    private static IReadOnlyDictionary<string, CSharpSourceImportTarget> BuildSourceImports(
        SourceFile sourceFile,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets)
    {
        var imports = new Dictionary<string, CSharpSourceImportTarget>(StringComparer.Ordinal);
        foreach (var dependency in sourceModuleGraph.Dependencies)
        {
            if (dependency.Kind != SourceModuleDependencyKind.Import ||
                !string.Equals(dependency.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) ||
                !sourceModuleTargets.TryGetValue(dependency.ToModulePath, out var target))
            {
                continue;
            }

            imports[dependency.Specifier] = target with { Specifier = dependency.Specifier };
        }

        return imports;
    }

    private static IReadOnlyList<CSharpSourceValueImportAlias> BuildSourceValueImportAliases(
        SourceFile sourceFile,
        SyntaxNode root,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources)
    {
        var importAliases = new List<CSharpSourceValueImportAlias>();
        foreach (var importDeclaration in root.Children.Where(IsRelativeNamedSourceImport))
        {
            if (!TryGetModuleSpecifier(importDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Import &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var importSpecifier in GetNamedImportSpecifiers(importDeclaration).Where(specifier => specifier.IsAlias))
            {
                if (TryResolveSourceValueExport(
                    dependency.ToModulePath,
                    importSpecifier.ImportedName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var valueTarget))
                {
                    importAliases.Add(new CSharpSourceValueImportAlias(
                        valueTarget.QualifiedModuleContainer,
                        importSpecifier.LocalName,
                        valueTarget.TargetMemberName,
                        valueTarget.Type,
                        valueTarget.IsMutable));
                }
            }
        }

        return importAliases;
    }

    private static IReadOnlyList<CSharpSourceValueReExport> BuildSourceValueReExports(
        SourceFile sourceFile,
        SyntaxNode root,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources)
    {
        var reExports = new List<CSharpSourceValueReExport>();
        foreach (var exportDeclaration in root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (TryResolveSourceValueExport(
                    dependency.ToModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var valueTarget))
                {
                    reExports.Add(new CSharpSourceValueReExport(
                        valueTarget.QualifiedModuleContainer,
                        exportSpecifier.ExportedName,
                        valueTarget.TargetMemberName,
                        valueTarget.Type,
                        valueTarget.IsMutable));
                }
            }
        }

        foreach (var exportDeclaration in root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportName in CollectSourceValueExportNames(
                dependency.ToModulePath,
                sourceModuleGraph,
                parsedSources,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
            {
                if (TryResolveSourceValueExport(
                    dependency.ToModulePath,
                    exportName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var valueTarget))
                {
                    reExports.Add(new CSharpSourceValueReExport(
                        valueTarget.QualifiedModuleContainer,
                        exportName,
                        valueTarget.TargetMemberName,
                        valueTarget.Type,
                        valueTarget.IsMutable));
                }
            }
        }

        return reExports
            .GroupBy(reExport => reExport.ExportedName, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    private static IReadOnlyList<CSharpSourceFunctionImportAlias> BuildSourceFunctionImportAliases(
        SourceFile sourceFile,
        SyntaxNode root,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources)
    {
        var importAliases = new List<CSharpSourceFunctionImportAlias>();
        foreach (var importDeclaration in root.Children.Where(IsRelativeNamedSourceImport))
        {
            if (!TryGetModuleSpecifier(importDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Import &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var importSpecifier in GetNamedImportSpecifiers(importDeclaration).Where(specifier => specifier.IsAlias))
            {
                if (TryResolveSourceFunctionExport(
                    dependency.ToModulePath,
                    importSpecifier.ImportedName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var functionTarget))
                {
                    importAliases.Add(new CSharpSourceFunctionImportAlias(
                        functionTarget.QualifiedModuleContainer,
                        importSpecifier.LocalName,
                        functionTarget.Function));
                }
            }
        }

        return importAliases;
    }

    private static IReadOnlyList<CSharpSourceFunctionReExport> BuildLocalFunctionExportAliases(
        SourceFile sourceFile,
        SyntaxNode root,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets)
    {
        if (!sourceModuleTargets.TryGetValue(sourceFile.ModulePath, out var target))
        {
            return [];
        }

        var exportAliases = new List<CSharpSourceFunctionReExport>();
        foreach (var exportDeclaration in root.Children.Where(IsLocalNamedExportAlias))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration).Where(specifier => specifier.IsAlias))
            {
                var targetFunction = root.Children.FirstOrDefault(child =>
                    child.Kind == SyntaxKind.FunctionDeclaration &&
                    !IsAmbientDeclaration(child) &&
                    string.Equals(GetDeclarationName(child), exportSpecifier.TargetName, StringComparison.Ordinal));
                if (targetFunction is not null)
                {
                    exportAliases.Add(new CSharpSourceFunctionReExport(
                        target.QualifiedModuleContainer,
                        exportSpecifier.ExportedName,
                        targetFunction));
                }
            }
        }

        return exportAliases;
    }

    private static IReadOnlyList<CSharpSourceFunctionReExport> BuildSourceFunctionReExports(
        SourceFile sourceFile,
        SyntaxNode root,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources)
    {
        var reExports = new List<CSharpSourceFunctionReExport>();
        foreach (var exportDeclaration in root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (TryResolveSourceFunctionExport(
                    dependency.ToModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var functionTarget))
                {
                    reExports.Add(new CSharpSourceFunctionReExport(
                        functionTarget.QualifiedModuleContainer,
                        exportSpecifier.ExportedName,
                        functionTarget.Function));
                }
            }
        }

        foreach (var exportDeclaration in root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, sourceFile.ModulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportName in CollectSourceFunctionExportNames(
                dependency.ToModulePath,
                sourceModuleGraph,
                parsedSources,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
            {
                if (TryResolveSourceFunctionExport(
                    dependency.ToModulePath,
                    exportName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    out var functionTarget))
                {
                    reExports.Add(new CSharpSourceFunctionReExport(
                        functionTarget.QualifiedModuleContainer,
                        exportName,
                        functionTarget.Function));
                }
            }
        }

        return reExports
            .GroupBy(reExport => reExport.ExportedName, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }

    private static IReadOnlyDictionary<string, CSharpSourceImportTarget> BuildSourceModuleTargets(
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        string defaultNamespace)
    {
        var targets = new Dictionary<string, CSharpSourceImportTarget>(StringComparer.OrdinalIgnoreCase);
        foreach (var (sourceFile, root) in parsedSources)
        {
            targets[sourceFile.ModulePath] = new CSharpSourceImportTarget(
                string.Empty,
                GetNamespaceName(root, defaultNamespace),
                GeneratedModuleContainerNaming.GetContainerName(sourceFile, parsedSources.Count));
        }

        var typeExportTargetCache = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var moduleExportTargetCache = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (sourceFile, _) in parsedSources)
        {
            targets[sourceFile.ModulePath] = targets[sourceFile.ModulePath] with
            {
                TypeExportTargets = CollectTypeExportTargets(
                    sourceFile.ModulePath,
                    targets,
                    parsedSources,
                    typeExportTargetCache,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)),
                ModuleExportTargets = CollectModuleExportTargets(
                    sourceFile.ModulePath,
                    targets,
                    parsedSources,
                    moduleExportTargetCache,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase))
            };
        }

        return targets;
    }

    private static IReadOnlyDictionary<string, string> CollectTypeExportTargets(
        string modulePath,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        Dictionary<string, IReadOnlyDictionary<string, string>> cache,
        HashSet<string> visiting)
    {
        if (cache.TryGetValue(modulePath, out var cached))
        {
            return cached;
        }

        var visitKey = $"{modulePath}\0<types>";
        if (!visiting.Add(visitKey))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null ||
            !sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            visiting.Remove(visitKey);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var typeExportTargets = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var exportTarget in CollectLocalTypeExportTargets(source.Root, target.NamespaceName))
        {
            typeExportTargets[exportTarget.Key] = exportTarget.Value;
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeTypeReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (TryResolveSourceTypeExport(
                    resolvedModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleTargets,
                    parsedSources,
                    cache,
                    visiting,
                    out var qualifiedTypeName))
                {
                    typeExportTargets[exportSpecifier.ExportedName] = qualifiedTypeName;
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            var targetTypeExportTargets = CollectTypeExportTargets(
                resolvedModulePath,
                sourceModuleTargets,
                parsedSources,
                cache,
                visiting);
            foreach (var exportTarget in targetTypeExportTargets)
            {
                typeExportTargets[exportTarget.Key] = exportTarget.Value;
            }
        }

        cache[modulePath] = typeExportTargets;
        visiting.Remove(visitKey);
        return typeExportTargets;
    }

    private static IReadOnlyDictionary<string, string> CollectLocalTypeExportTargets(SyntaxNode root, string namespaceName)
    {
        var declaredTypes = root.Children
            .Where(child => child.Kind is SyntaxKind.TypeAliasDeclaration or SyntaxKind.RecordDeclaration or SyntaxKind.UnionDeclaration or SyntaxKind.ClassDeclaration or SyntaxKind.InterfaceDeclaration or SyntaxKind.DelegateDeclaration)
            .Select(GetTypeDeclarationName)
            .Where(name => name.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
        var typeExportTargets = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var typeDeclaration in root.Children.Where(IsSourceTypeDeclaration))
        {
            var name = GetTypeDeclarationName(typeDeclaration);
            if (name.Length > 0 &&
                (HasModifier(typeDeclaration, SyntaxKind.ExportModifier) ||
                    GetLocalExportedIdentifiers(root).Contains(name, StringComparer.Ordinal)))
            {
                typeExportTargets[name] = $"{namespaceName}.{name}";
            }
        }

        foreach (var exportDeclaration in root.Children.Where(child => child.Kind == SyntaxKind.ExportTypeDeclaration && !TryGetModuleSpecifier(child, out _)))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration).Where(specifier => specifier.IsAlias))
            {
                if (declaredTypes.Contains(exportSpecifier.TargetName))
                {
                    typeExportTargets[exportSpecifier.ExportedName] = $"{namespaceName}.{exportSpecifier.TargetName}";
                }
            }
        }

        return typeExportTargets;
    }

    private static IReadOnlyDictionary<string, string> CollectModuleExportTargets(
        string modulePath,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        Dictionary<string, IReadOnlyDictionary<string, string>> cache,
        HashSet<string> visiting)
    {
        if (cache.TryGetValue(modulePath, out var cached))
        {
            return cached;
        }

        var visitKey = $"{modulePath}\0<modules>";
        if (!visiting.Add(visitKey))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null ||
            !sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            visiting.Remove(visitKey);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var moduleExportTargets = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var exportTarget in CollectLocalModuleExportTargets(source.Root, target.NamespaceName))
        {
            moduleExportTargets[exportTarget.Key] = exportTarget.Value;
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (TryResolveSourceModuleExport(
                    resolvedModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleTargets,
                    parsedSources,
                    cache,
                    visiting,
                    out var qualifiedModuleName))
                {
                    moduleExportTargets[exportSpecifier.ExportedName] = qualifiedModuleName;
                }
            }
        }

        cache[modulePath] = moduleExportTargets;
        visiting.Remove(visitKey);
        return moduleExportTargets;
    }

    private static IReadOnlyDictionary<string, string> CollectLocalModuleExportTargets(SyntaxNode root, string namespaceName)
    {
        var declaredModules = root.Children
            .Where(child => child.Kind == SyntaxKind.ModuleDeclaration)
            .Select(GetModuleDeclarationName)
            .Where(name => name.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
        var moduleExportTargets = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var moduleDeclaration in root.Children.Where(child => child.Kind == SyntaxKind.ModuleDeclaration))
        {
            var name = GetModuleDeclarationName(moduleDeclaration);
            if (name.Length > 0 &&
                (HasModifier(moduleDeclaration, SyntaxKind.ExportModifier) ||
                    GetLocalExportedIdentifiers(root).Contains(name, StringComparer.Ordinal)))
            {
                moduleExportTargets[name] = $"{namespaceName}.{name}";
            }
        }

        foreach (var exportDeclaration in root.Children.Where(child => child.Kind == SyntaxKind.ExportNamedDeclaration && !TryGetModuleSpecifier(child, out _)))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration).Where(specifier => specifier.IsAlias))
            {
                if (declaredModules.Contains(exportSpecifier.TargetName))
                {
                    moduleExportTargets[exportSpecifier.ExportedName] = $"{namespaceName}.{exportSpecifier.TargetName}";
                }
            }
        }

        return moduleExportTargets;
    }

    private static string GetTypeDeclarationName(SyntaxNode node)
    {
        var seenDeclarationKeyword = false;
        foreach (var child in node.Children)
        {
            if (child.IsToken && child.Kind is SyntaxKind.TypeKeyword or SyntaxKind.RecordKeyword or SyntaxKind.UnionKeyword or SyntaxKind.ClassKeyword or SyntaxKind.InterfaceKeyword or SyntaxKind.DelegateKeyword)
            {
                seenDeclarationKeyword = true;
                continue;
            }

            if (seenDeclarationKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            {
                return child.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static string GetModuleDeclarationName(SyntaxNode node)
    {
        var seenModuleKeyword = false;
        foreach (var child in node.Children)
        {
            if (child.IsToken && child.Kind == SyntaxKind.ModuleKeyword)
            {
                seenModuleKeyword = true;
                continue;
            }

            if (seenModuleKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            {
                return child.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static bool TryResolveSourceModuleExport(
        string modulePath,
        string exportedName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        Dictionary<string, IReadOnlyDictionary<string, string>> cache,
        HashSet<string> visiting,
        out string qualifiedModuleName)
    {
        qualifiedModuleName = string.Empty;
        var visitKey = $"{modulePath}\0<module>\0{exportedName}";
        if (!visiting.Add(visitKey))
        {
            return false;
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null ||
            !sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            visiting.Remove(visitKey);
            return false;
        }

        foreach (var localTarget in CollectLocalModuleExportTargets(source.Root, target.NamespaceName))
        {
            if (string.Equals(localTarget.Key, exportedName, StringComparison.Ordinal))
            {
                qualifiedModuleName = localTarget.Value;
                visiting.Remove(visitKey);
                return true;
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryResolveSourceModuleExport(
                    resolvedModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleTargets,
                    parsedSources,
                    cache,
                    visiting,
                    out qualifiedModuleName))
                {
                    visiting.Remove(visitKey);
                    return true;
                }
            }
        }

        if (cache.TryGetValue(modulePath, out var cached) &&
            cached.TryGetValue(exportedName, out var cachedQualifiedModuleName))
        {
            qualifiedModuleName = cachedQualifiedModuleName;
            visiting.Remove(visitKey);
            return true;
        }

        visiting.Remove(visitKey);
        return false;
    }

    private static bool TryResolveSourceTypeExport(
        string modulePath,
        string exportedName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        Dictionary<string, IReadOnlyDictionary<string, string>> cache,
        HashSet<string> visiting,
        out string qualifiedTypeName)
    {
        qualifiedTypeName = string.Empty;
        var visitKey = $"{modulePath}\0{exportedName}";
        if (!visiting.Add(visitKey))
        {
            return false;
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null ||
            !sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            visiting.Remove(visitKey);
            return false;
        }

        var directType = source.Root.Children.FirstOrDefault(child =>
            IsSourceTypeDeclaration(child) &&
            string.Equals(GetTypeDeclarationName(child), exportedName, StringComparison.Ordinal) &&
            IsDirectTypeExported(source.Root, child, exportedName));
        if (directType is not null)
        {
            qualifiedTypeName = $"{target.NamespaceName}.{exportedName}";
            visiting.Remove(visitKey);
            return true;
        }

        foreach (var localTarget in CollectLocalTypeExportTargets(source.Root, target.NamespaceName))
        {
            if (string.Equals(localTarget.Key, exportedName, StringComparison.Ordinal))
            {
                qualifiedTypeName = localTarget.Value;
                visiting.Remove(visitKey);
                return true;
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeTypeReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryResolveSourceTypeExport(
                    resolvedModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleTargets,
                    parsedSources,
                    cache,
                    visiting,
                    out qualifiedTypeName))
                {
                    visiting.Remove(visitKey);
                    return true;
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(modulePath, specifier);
            if (TryResolveSourceTypeExport(
                resolvedModulePath,
                exportedName,
                sourceModuleTargets,
                parsedSources,
                cache,
                visiting,
                out qualifiedTypeName))
            {
                visiting.Remove(visitKey);
                return true;
            }
        }

        if (cache.TryGetValue(modulePath, out var cached) &&
            cached.TryGetValue(exportedName, out var cachedQualifiedTypeName))
        {
            qualifiedTypeName = cachedQualifiedTypeName;
            visiting.Remove(visitKey);
            return true;
        }

        visiting.Remove(visitKey);
        return false;
    }

    private static bool TryResolveSourceFunctionExport(
        string modulePath,
        string exportedName,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        HashSet<string> visiting,
        out ResolvedSourceFunction functionTarget)
    {
        functionTarget = default;
        var visitKey = $"{modulePath}\0{exportedName}";
        if (!visiting.Add(visitKey))
        {
            return false;
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null)
        {
            visiting.Remove(visitKey);
            return false;
        }

        if (sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            var directFunction = source.Root.Children.FirstOrDefault(child =>
                child.Kind == SyntaxKind.FunctionDeclaration &&
                !IsAmbientDeclaration(child) &&
                string.Equals(GetDeclarationName(child), exportedName, StringComparison.Ordinal) &&
                IsDirectFunctionExported(source.Root, child, exportedName));
            if (directFunction is not null)
            {
                functionTarget = new ResolvedSourceFunction(target.QualifiedModuleContainer, directFunction);
                visiting.Remove(visitKey);
                return true;
            }
        }

        if (sourceModuleTargets.TryGetValue(modulePath, out var localAliasTarget))
        {
            foreach (var exportDeclaration in source.Root.Children.Where(IsLocalNamedExportAlias))
            {
                foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
                {
                    if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var targetFunction = source.Root.Children.FirstOrDefault(child =>
                        child.Kind == SyntaxKind.FunctionDeclaration &&
                        !IsAmbientDeclaration(child) &&
                        string.Equals(GetDeclarationName(child), exportSpecifier.TargetName, StringComparison.Ordinal));
                    if (targetFunction is not null)
                    {
                        functionTarget = new ResolvedSourceFunction(localAliasTarget.QualifiedModuleContainer, targetFunction);
                        visiting.Remove(visitKey);
                        return true;
                    }
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryResolveSourceFunctionExport(
                    dependency.ToModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    visiting,
                    out functionTarget))
                {
                    visiting.Remove(visitKey);
                    return true;
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            if (TryResolveSourceFunctionExport(
                dependency.ToModulePath,
                exportedName,
                sourceModuleGraph,
                sourceModuleTargets,
                parsedSources,
                visiting,
                out functionTarget))
            {
                visiting.Remove(visitKey);
                return true;
            }
        }

        visiting.Remove(visitKey);
        return false;
    }

    private static bool TryResolveSourceValueExport(
        string modulePath,
        string exportedName,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceModuleTargets,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        HashSet<string> visiting,
        out ResolvedSourceValue valueTarget)
    {
        valueTarget = default;
        var visitKey = $"{modulePath}\0{exportedName}";
        if (!visiting.Add(visitKey))
        {
            return false;
        }

        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null ||
            !sourceModuleTargets.TryGetValue(modulePath, out var target))
        {
            visiting.Remove(visitKey);
            return false;
        }

        var directValue = source.Root.Children.FirstOrDefault(child =>
            IsSourceValueImportAliasTarget(child) &&
            string.Equals(GetValueDeclarationName(child), exportedName, StringComparison.Ordinal) &&
            IsDirectValueExported(source.Root, child, exportedName));
        if (directValue is not null)
        {
            valueTarget = new ResolvedSourceValue(
                target.QualifiedModuleContainer,
                exportedName,
                GetValueAliasType(directValue),
                IsMutableValueDeclaration(directValue));
            visiting.Remove(visitKey);
            return true;
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsLocalNamedExportAlias))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                {
                    continue;
                }

                var localValue = source.Root.Children.FirstOrDefault(child =>
                    IsSourceValueImportAliasTarget(child) &&
                    string.Equals(GetValueDeclarationName(child), exportSpecifier.TargetName, StringComparison.Ordinal));
                if (localValue is not null)
                {
                    valueTarget = new ResolvedSourceValue(
                        target.QualifiedModuleContainer,
                        exportedName,
                        GetValueAliasType(localValue),
                        IsMutableValueDeclaration(localValue));
                    visiting.Remove(visitKey);
                    return true;
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (!string.Equals(exportSpecifier.ExportedName, exportedName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryResolveSourceValueExport(
                    dependency.ToModulePath,
                    exportSpecifier.TargetName,
                    sourceModuleGraph,
                    sourceModuleTargets,
                    parsedSources,
                    visiting,
                    out valueTarget))
                {
                    visiting.Remove(visitKey);
                    return true;
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            if (TryResolveSourceValueExport(
                dependency.ToModulePath,
                exportedName,
                sourceModuleGraph,
                sourceModuleTargets,
                parsedSources,
                visiting,
                out valueTarget))
            {
                visiting.Remove(visitKey);
                return true;
            }
        }

        visiting.Remove(visitKey);
        return false;
    }

    private static IReadOnlyList<string> CollectSourceFunctionExportNames(
        string modulePath,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        HashSet<string> visiting)
    {
        if (!visiting.Add(modulePath))
        {
            return [];
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null)
        {
            visiting.Remove(modulePath);
            return names.ToArray();
        }

        foreach (var function in source.Root.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child)))
        {
            var name = GetDeclarationName(function);
            if (name.Length > 0 && IsDirectFunctionExported(source.Root, function, name))
            {
                names.Add(name);
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsLocalNamedExportAlias))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (source.Root.Children.Any(child =>
                    child.Kind == SyntaxKind.FunctionDeclaration &&
                    !IsAmbientDeclaration(child) &&
                    string.Equals(GetDeclarationName(child), exportSpecifier.TargetName, StringComparison.Ordinal)))
                {
                    names.Add(exportSpecifier.ExportedName);
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                names.Add(exportSpecifier.ExportedName);
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var name in CollectSourceFunctionExportNames(dependency.ToModulePath, sourceModuleGraph, parsedSources, visiting))
            {
                names.Add(name);
            }
        }

        visiting.Remove(modulePath);
        return names.ToArray();
    }

    private static IReadOnlyList<string> CollectSourceValueExportNames(
        string modulePath,
        SourceModuleGraph sourceModuleGraph,
        IReadOnlyList<(SourceFile SourceFile, SyntaxNode Root)> parsedSources,
        HashSet<string> visiting)
    {
        if (!visiting.Add(modulePath))
        {
            return [];
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        var source = parsedSources.FirstOrDefault(candidate =>
            string.Equals(candidate.SourceFile.ModulePath, modulePath, StringComparison.OrdinalIgnoreCase));
        if (source.Root is null)
        {
            visiting.Remove(modulePath);
            return names.ToArray();
        }

        foreach (var value in source.Root.Children.Where(child => IsSourceValueImportAliasTarget(child)))
        {
            var name = GetValueDeclarationName(value);
            if (name.Length > 0 && IsDirectValueExported(source.Root, value, name))
            {
                names.Add(name);
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsLocalNamedExportAlias))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (source.Root.Children.Any(child =>
                    IsSourceValueImportAliasTarget(child) &&
                    string.Equals(GetValueDeclarationName(child), exportSpecifier.TargetName, StringComparison.Ordinal)))
                {
                    names.Add(exportSpecifier.ExportedName);
                }
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeNamedReExport))
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                names.Add(exportSpecifier.ExportedName);
            }
        }

        foreach (var exportDeclaration in source.Root.Children.Where(IsSupportedRelativeStarReExport))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifier))
            {
                continue;
            }

            var dependency = sourceModuleGraph.Dependencies.FirstOrDefault(candidate =>
                candidate.Kind == SourceModuleDependencyKind.Export &&
                string.Equals(candidate.FromModulePath, modulePath, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Specifier, specifier, StringComparison.Ordinal));
            if (dependency is null)
            {
                continue;
            }

            foreach (var name in CollectSourceValueExportNames(dependency.ToModulePath, sourceModuleGraph, parsedSources, visiting))
            {
                names.Add(name);
            }
        }

        visiting.Remove(modulePath);
        return names.ToArray();
    }

    private static bool IsDirectFunctionExported(SyntaxNode root, SyntaxNode function, string name) =>
        HasModifier(function, SyntaxKind.ExportModifier) ||
        GetLocalExportedIdentifiers(root).Contains(name, StringComparer.Ordinal);

    private static bool IsDirectValueExported(SyntaxNode root, SyntaxNode value, string name) =>
        HasModifier(value, SyntaxKind.ExportModifier) ||
        GetLocalExportedIdentifiers(root).Contains(name, StringComparer.Ordinal);

    private static bool IsDirectTypeExported(SyntaxNode root, SyntaxNode type, string name) =>
        HasModifier(type, SyntaxKind.ExportModifier) ||
        GetLocalExportedIdentifiers(root).Contains(name, StringComparer.Ordinal);

    private static IEnumerable<string> GetLocalExportedIdentifiers(SyntaxNode root)
    {
        foreach (var exportDeclaration in root.Children.Where(child => child.Kind is SyntaxKind.ExportNamedDeclaration or SyntaxKind.ExportTypeDeclaration))
        {
            if (TryGetModuleSpecifier(exportDeclaration, out _) ||
                GetNamedExportSpecifiers(exportDeclaration).Any(specifier => specifier.IsAlias))
            {
                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                yield return exportSpecifier.ExportedName;
            }
        }
    }

    private static bool IsSourceValueImportAliasTarget(SyntaxNode node) =>
        node.Kind == SyntaxKind.LiteralDeclaration ||
        (node.Kind == SyntaxKind.ValueDeclaration && (!IsFunctionValueDeclaration(node) || HasFunctionTypeAnnotation(node) || HasLambdaInitializer(node)));

    private static bool IsSourceTypeDeclaration(SyntaxNode node) =>
        node.Kind is
            SyntaxKind.TypeAliasDeclaration or
            SyntaxKind.RecordDeclaration or
            SyntaxKind.UnionDeclaration or
            SyntaxKind.ClassDeclaration or
            SyntaxKind.InterfaceDeclaration or
            SyntaxKind.DelegateDeclaration;

    private static bool IsFunctionValueDeclaration(SyntaxNode node)
    {
        var annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
        if (annotation?.Children.Any(child => child.Kind == SyntaxKind.FunctionType) == true)
        {
            return true;
        }

        return node.Children
            .Where(child => child.Kind == SyntaxKind.Initializer)
            .SelectMany(child => child.Children)
            .Any(child => child.Kind == SyntaxKind.LambdaExpression);
    }

    private static bool HasFunctionTypeAnnotation(SyntaxNode node) =>
        node.Children
            .FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)?
            .Children
            .Any(child => child.Kind == SyntaxKind.FunctionType) == true;

    private static bool HasLambdaInitializer(SyntaxNode node) =>
        node.Children
            .Where(child => child.Kind == SyntaxKind.Initializer)
            .SelectMany(child => child.Children)
            .Any(child => child.Kind == SyntaxKind.LambdaExpression);

    private static bool IsMutableValueDeclaration(SyntaxNode node) =>
        node.Children.Any(child => child.Kind == SyntaxKind.MutKeyword);

    private static string GetValueAliasType(SyntaxNode node)
    {
        if (TryGetDirectTypeAnnotation(node, out var annotation))
        {
            return MapType(annotation.Children.FirstOrDefault(child => !child.IsToken));
        }

        var initializer = GetInitializerExpression(node);
        if (TryInferLambdaFunctionType(initializer, out var lambdaType))
        {
            return lambdaType;
        }

        if (initializer?.Kind == SyntaxKind.CollectionExpression)
        {
            var elementType = InferCollectionElementType(initializer.Children.Where(child => !child.IsToken).ToArray());
            if (elementType.Length > 0)
            {
                return $"{elementType}[]";
            }
        }

        return InferLiteralType(initializer);
    }

    private static bool TryInferLambdaFunctionType(SyntaxNode? initializer, out string type)
    {
        type = string.Empty;
        if (initializer?.Kind != SyntaxKind.LambdaExpression)
        {
            return false;
        }

        type = $"System.Func<object, {InferLambdaReturnType(initializer)}>";
        return true;
    }

    private static string InferLambdaReturnType(SyntaxNode lambda)
    {
        var body = lambda.Children.LastOrDefault(child => !child.IsToken);
        return InferExpressionType(body);
    }

    private static string InferExpressionType(SyntaxNode? node)
    {
        if (node is null)
        {
            return "object";
        }

        if (node.Kind == SyntaxKind.LiteralExpression)
        {
            return InferLiteralType(node);
        }

        if (node.Kind == SyntaxKind.NameofExpression)
        {
            return "string";
        }

        if (node.Kind == SyntaxKind.CheckedExpression)
        {
            return InferExpressionType(node.Children.FirstOrDefault(child => !child.IsToken));
        }

        if (node.Kind == SyntaxKind.ParenthesizedExpression)
        {
            return InferExpressionType(node.Children.FirstOrDefault(child => !child.IsToken));
        }

        if (node.Kind == SyntaxKind.BlockExpression)
        {
            return InferExpressionType(GetBlockResultExpression(node));
        }

        if (node.Kind == SyntaxKind.IfExpression &&
            TryInferIfExpressionType(node, out var ifType))
        {
            return ifType;
        }

        if (IsUnaryLogicalNotExpression(node))
        {
            return "bool";
        }

        if (TryInferUnaryNumericExpressionType(node, out var unaryNumericType))
        {
            return unaryNumericType;
        }

        if (node.Kind == SyntaxKind.BinaryExpression &&
            node.Children.Any(child => child.IsToken && child.Kind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken))
        {
            return "bool";
        }

        return "object";
    }

    private static bool TryInferIfExpressionType(SyntaxNode node, out string type)
    {
        var branchTypes = GetIfBranchResultExpressions(node)
            .Select(InferExpressionType)
            .Where(candidate => !string.Equals(candidate, "object", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        type = branchTypes.Length == 1 ? branchTypes[0] : string.Empty;
        return type.Length > 0;
    }

    private static IEnumerable<SyntaxNode> GetIfBranchResultExpressions(SyntaxNode node)
    {
        var thenBlock = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
        if (GetBlockResultExpression(thenBlock) is { } thenExpression)
        {
            yield return thenExpression;
        }

        foreach (var elseClause in node.Children.Where(child => child.Kind == SyntaxKind.ElseClause))
        {
            if (elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.IfExpression) is { } nestedIf)
            {
                yield return nestedIf;
                continue;
            }

            var elseBlock = elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
            if (GetBlockResultExpression(elseBlock) is { } elseExpression)
            {
                yield return elseExpression;
            }
        }
    }

    private static SyntaxNode? GetBlockResultExpression(SyntaxNode? block)
    {
        var result = block?.Children.LastOrDefault(child => !child.IsToken);
        return result?.Kind == SyntaxKind.ExpressionStatement
            ? result.Children.FirstOrDefault(child => !child.IsToken)
            : result;
    }

    private static bool IsUnaryLogicalNotExpression(SyntaxNode node) =>
        node.Kind == SyntaxKind.BinaryExpression &&
        node.Children.FirstOrDefault(child => child.IsToken)?.Kind == SyntaxKind.BangToken &&
        node.Children.Count(child => !child.IsToken) == 1;

    private static bool TryInferUnaryNumericExpressionType(SyntaxNode node, out string type)
    {
        type = string.Empty;
        var operatorKind = node.Children.FirstOrDefault(child => child.IsToken)?.Kind ?? SyntaxKind.UnknownToken;
        var expression = node.Children.FirstOrDefault(child => !child.IsToken);
        if (node.Kind != SyntaxKind.BinaryExpression ||
            expression is null ||
            node.Children.Count(child => !child.IsToken) != 1 ||
            operatorKind is not (SyntaxKind.PlusToken or SyntaxKind.MinusToken))
        {
            return false;
        }

        return TryGetUnaryNumericResultType(InferExpressionType(expression), operatorKind, out type);
    }

    private static bool TryGetUnaryNumericResultType(string operandType, SyntaxKind operatorKind, out string resultType)
    {
        resultType = operandType switch
        {
            "byte" or "sbyte" or "short" or "ushort" => "int",
            "int" or "long" or "float" or "double" or "decimal" => operandType,
            "uint" or "ulong" when operatorKind == SyntaxKind.PlusToken => operandType,
            _ => string.Empty
        };
        return resultType.Length > 0;
    }

    private static bool TryGetDirectTypeAnnotation(SyntaxNode node, out SyntaxNode annotation)
    {
        annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)!;
        return annotation is not null;
    }

    private static SyntaxNode? GetInitializerExpression(SyntaxNode node) =>
        node.Children
            .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
            .Children
            .FirstOrDefault(child => !child.IsToken);

    private static string InferCollectionElementType(IReadOnlyList<SyntaxNode> elements)
    {
        if (elements.Any(element => element.Kind == SyntaxKind.SpreadElement))
        {
            return string.Empty;
        }

        var inferredTypes = elements
            .Select(InferLiteralType)
            .Where(type => type.Length > 0 && !string.Equals(type, "object", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return inferredTypes.Length == 1 ? inferredTypes[0] : string.Empty;
    }

    private static string InferLiteralType(SyntaxNode? initializer)
    {
        if (initializer?.Kind != SyntaxKind.LiteralExpression)
        {
            return "object";
        }

        var token = initializer.Children.FirstOrDefault(child => child.IsToken);
        return token?.Kind switch
        {
            SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => "string",
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => "bool",
            SyntaxKind.NumericLiteralToken => InferNumericLiteralType(token.Text ?? string.Empty),
            _ => "object"
        };
    }

    private static string InferNumericLiteralType(string text)
    {
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            return "decimal";
        }

        return text.Contains('.', StringComparison.Ordinal) ? "double" : "int";
    }

    private static string MapType(SyntaxNode? node)
    {
        if (node is null)
        {
            return "object";
        }

        if (node.Kind == SyntaxKind.TypeAnnotation)
        {
            return MapType(node.Children.FirstOrDefault(child => !child.IsToken));
        }

        if (node.Kind == SyntaxKind.NullableType)
        {
            return MapType(node.Children.FirstOrDefault(child => !child.IsToken));
        }

        if (node.Kind == SyntaxKind.ArrayType)
        {
            return $"{MapType(node.Children.FirstOrDefault(child => !child.IsToken))}[]";
        }

        if (node.Kind == SyntaxKind.FunctionType)
        {
            return MapFunctionType(node);
        }

        if (node.Kind == SyntaxKind.TypeName)
        {
            return GetQualifiedName(node) switch
            {
                "bool" => "bool",
                "byte" => "byte",
                "char" => "char",
                "decimal" => "decimal",
                "double" => "double",
                "float" => "float",
                "int" => "int",
                "long" => "long",
                "object" => "object",
                "sbyte" => "sbyte",
                "short" => "short",
                "string" => "string",
                "uint" => "uint",
                "ulong" => "ulong",
                "ushort" => "ushort",
                "void" or "unit" => "void",
                _ => GetQualifiedName(node)
            };
        }

        return "object";
    }

    private static string MapFunctionType(SyntaxNode node)
    {
        var types = node.Children.Where(child => !child.IsToken).ToArray();
        if (types.Length < 2)
        {
            return "System.Func<object>";
        }

        var parameterType = MapType(types[0]);
        var returnType = MapType(types[1]);
        if (string.Equals(parameterType, "void", StringComparison.Ordinal))
        {
            return string.Equals(returnType, "void", StringComparison.Ordinal)
                ? "System.Action"
                : $"System.Func<{returnType}>";
        }

        return string.Equals(returnType, "void", StringComparison.Ordinal)
            ? $"System.Action<{parameterType}>"
            : $"System.Func<{parameterType}, {returnType}>";
    }

    private static bool HasModifier(SyntaxNode node, SyntaxKind modifier) =>
        node.Children.Any(child => child.Kind == modifier);

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
                !IsAmbientDeclaration(child) &&
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
                !IsAmbientDeclaration(child) &&
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

    private static bool IsAmbientDeclaration(SyntaxNode node) =>
        node.Children.Any(child => child.Kind == SyntaxKind.AmbientModifier);

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

    private static SourceAnalysisResult AnalyzeSource(
        SourceFile sourceFile,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string nullableMode)
    {
        var diagnostics = new List<Diagnostic>();
        var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
        diagnostics.AddRange(parseResult.Diagnostics);
        if (parseResult.HasErrors || parseResult.Root is null)
        {
            return new SourceAnalysisResult(sourceFile, null, diagnostics);
        }

        diagnostics.AddRange(TypeSharpInteropValidator.Validate(
            parseResult.Root,
            assemblies,
            sourceFile.RelativePath,
            nullableMode));
        var bindingResult = TypeSharpBinder.Bind(parseResult.Root, sourceFile.RelativePath);
        diagnostics.AddRange(bindingResult.Diagnostics);
        if (!bindingResult.HasErrors)
        {
            diagnostics.AddRange(TypeSharpTypeChecker.Check(
                parseResult.Root,
                sourceFile.RelativePath,
                assemblies).Diagnostics);
        }

        return new SourceAnalysisResult(sourceFile, parseResult.Root, diagnostics);
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

    private static bool IsSupportedRelativeNamedReExport(SyntaxNode node) =>
        node.Kind == SyntaxKind.ExportNamedDeclaration &&
        TryGetModuleSpecifier(node, out var specifier) &&
        IsRelativeSpecifier(specifier);

    private static bool IsSupportedRelativeTypeReExport(SyntaxNode node) =>
        node.Kind == SyntaxKind.ExportTypeDeclaration &&
        TryGetModuleSpecifier(node, out var specifier) &&
        IsRelativeSpecifier(specifier);

    private static bool IsSupportedRelativeStarReExport(SyntaxNode node) =>
        node.Kind == SyntaxKind.ExportStarDeclaration &&
        TryGetModuleSpecifier(node, out var specifier) &&
        IsRelativeSpecifier(specifier);

    private static bool IsRelativeNamedSourceImport(SyntaxNode node) =>
        node.Kind == SyntaxKind.ImportNamedDeclaration &&
        TryGetModuleSpecifier(node, out var specifier) &&
        IsRelativeSpecifier(specifier);

    private static bool IsLocalNamedExportAlias(SyntaxNode node) =>
        node.Kind == SyntaxKind.ExportNamedDeclaration &&
        !TryGetModuleSpecifier(node, out _) &&
        GetNamedExportSpecifiers(node).Any(specifier => specifier.IsAlias);

    private static bool TryGetModuleSpecifier(SyntaxNode node, out string specifier)
    {
        for (var index = 0; index < node.Children.Count - 1; index++)
        {
            if (node.Children[index].Kind == SyntaxKind.FromKeyword &&
                node.Children[index + 1].Kind == SyntaxKind.StringLiteralToken &&
                TryUnquoteStringLiteral(node.Children[index + 1].Text, out specifier))
            {
                return true;
            }
        }

        specifier = string.Empty;
        return false;
    }

    private static bool TryUnquoteStringLiteral(string? text, out string value)
    {
        value = string.Empty;
        if (string.IsNullOrEmpty(text) || text.Length < 2 || text[0] != '"' || text[^1] != '"')
        {
            return false;
        }

        value = text[1..^1];
        return value.Length > 0;
    }

    private static bool IsRelativeSpecifier(string specifier) =>
        specifier == "." ||
        specifier == ".." ||
        specifier.StartsWith("./", StringComparison.Ordinal) ||
        specifier.StartsWith("../", StringComparison.Ordinal);

    private static string ResolveRelativeModulePath(string fromModulePath, string specifier)
    {
        var parts = new List<string>();
        var lastSlash = fromModulePath.LastIndexOf('/');
        if (lastSlash >= 0)
        {
            parts.AddRange(fromModulePath[..lastSlash].Split('/', StringSplitOptions.RemoveEmptyEntries));
        }

        foreach (var part in specifier.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                continue;
            }

            parts.Add(part);
        }

        var resolved = string.Join("/", parts);
        return resolved.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? resolved[..^".tysh".Length]
            : resolved;
    }

    private static IEnumerable<NamedExportSpecifier> GetNamedExportSpecifiers(SyntaxNode node)
    {
        var insideBraces = false;
        for (var index = 0; index < node.Children.Count; index++)
        {
            var child = node.Children[index];
            if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
            {
                insideBraces = true;
                continue;
            }

            if (child.IsToken && child.Kind == SyntaxKind.CloseBraceToken)
            {
                yield break;
            }

            if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } name)
            {
                if (index + 2 < node.Children.Count &&
                    node.Children[index + 1].IsToken &&
                    node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                    node.Children[index + 2].IsToken &&
                    node.Children[index + 2].Kind == SyntaxKind.IdentifierToken &&
                    node.Children[index + 2].Text is { Length: > 0 } alias)
                {
                    yield return new NamedExportSpecifier(name, alias);
                    index += 2;
                }
                else
                {
                    yield return new NamedExportSpecifier(name, name);
                }
            }
        }
    }

    private static IEnumerable<NamedImportSpecifier> GetNamedImportSpecifiers(SyntaxNode node)
    {
        var insideBraces = false;
        for (var index = 0; index < node.Children.Count; index++)
        {
            var child = node.Children[index];
            if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
            {
                insideBraces = true;
                continue;
            }

            if (child.IsToken && child.Kind == SyntaxKind.CloseBraceToken)
            {
                yield break;
            }

            if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } name)
            {
                if (index + 2 < node.Children.Count &&
                    node.Children[index + 1].IsToken &&
                    node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                    node.Children[index + 2].IsToken &&
                    node.Children[index + 2].Kind == SyntaxKind.IdentifierToken &&
                    node.Children[index + 2].Text is { Length: > 0 } alias)
                {
                    yield return new NamedImportSpecifier(name, alias);
                    index += 2;
                }
                else
                {
                    yield return new NamedImportSpecifier(name, name);
                }
            }
        }
    }

    private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName)
    {
        public bool IsAlias => !string.Equals(ImportedName, LocalName, StringComparison.Ordinal);
    }

    private readonly record struct NamedExportSpecifier(string TargetName, string ExportedName)
    {
        public bool IsAlias => !string.Equals(TargetName, ExportedName, StringComparison.Ordinal);
    }

    private readonly record struct ResolvedSourceFunction(string QualifiedModuleContainer, SyntaxNode Function);

    private readonly record struct ResolvedSourceValue(
        string QualifiedModuleContainer,
        string TargetMemberName,
        string Type,
        bool IsMutable);

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

    private static string GetValueDeclarationName(SyntaxNode node)
    {
        var seenValueKeyword = false;
        foreach (var child in node.Children)
        {
            if (child.IsToken && child.Kind is SyntaxKind.LetKeyword or SyntaxKind.LiteralKeyword)
            {
                seenValueKeyword = true;
                continue;
            }

            if (seenValueKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
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

    private sealed record SourceAnalysisResult(
        SourceFile SourceFile,
        SyntaxNode? Root,
        IReadOnlyList<Diagnostic> Diagnostics);
}
