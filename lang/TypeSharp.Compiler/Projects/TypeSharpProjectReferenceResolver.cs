using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Projects;

public static class TypeSharpProjectReferenceResolver
{
    public static ProjectReferenceResolutionResult Resolve(TypeSharpManifest manifest, string? targetFrameworkOverride = null)
    {
        var diagnostics = new List<Diagnostic>();
        var directReferences = ResolveDirectReferences(
            manifest,
            ResolveTargetFramework(manifest, targetFrameworkOverride),
            targetFrameworkOverride,
            diagnostics,
            [NormalizeFullPath(manifest.ManifestPath)],
            new Dictionary<string, TypeSharpManifest>(StringComparer.OrdinalIgnoreCase));

        ReportDuplicateProjectReferenceNames(directReferences, diagnostics);
        return new ProjectReferenceResolutionResult(directReferences, diagnostics);
    }

    public static string GetRootNamespace(TypeSharpManifest manifest)
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

    public static string ResolveTargetFramework(TypeSharpManifest manifest, string? targetFrameworkOverride)
    {
        if (!string.IsNullOrWhiteSpace(targetFrameworkOverride))
        {
            return targetFrameworkOverride;
        }

        return string.IsNullOrWhiteSpace(manifest.Project.TargetFramework)
            ? TypeSharpCompilerInfo.DefaultTargetFramework
            : manifest.Project.TargetFramework;
    }

    private static IReadOnlyList<ProjectReferenceInfo> ResolveDirectReferences(
        TypeSharpManifest manifest,
        string expectedTargetFramework,
        string? targetFrameworkOverride,
        List<Diagnostic> diagnostics,
        List<string> manifestStack,
        Dictionary<string, TypeSharpManifest> loadedManifests)
    {
        var references = new List<ProjectReferenceInfo>();
        foreach (var option in manifest.ProjectReferences.Paths)
        {
            var trimmed = option.Path.Trim();
            if (trimmed.Length == 0)
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestValue,
                    "Project reference path cannot be empty.",
                    option.File,
                    option.Line,
                    option.Column));
                continue;
            }

            var fullPath = NormalizeFullPath(Path.Combine(manifest.ProjectDirectory, trimmed));
            var cycleStart = manifestStack.FindIndex(path => string.Equals(path, fullPath, StringComparison.OrdinalIgnoreCase));
            if (cycleStart >= 0)
            {
                var cycle = manifestStack
                    .Skip(cycleStart)
                    .Concat([fullPath])
                    .Select(Path.GetFileName)
                    .ToArray();
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestValue,
                    $"Project reference cycle detected: {string.Join(" -> ", cycle)}.",
                    option.File,
                    option.Line,
                    option.Column));
                continue;
            }

            if (!File.Exists(fullPath))
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.ManifestNotFound,
                    $"Could not find referenced TypeSharp manifest '{trimmed}'.",
                    option.File,
                    option.Line,
                    option.Column));
                continue;
            }

            if (!loadedManifests.TryGetValue(fullPath, out var referencedManifest))
            {
                var loadResult = TypeSharpManifestLoader.Load(fullPath);
                diagnostics.AddRange(loadResult.Diagnostics);
                if (loadResult.Manifest is null)
                {
                    continue;
                }

                referencedManifest = loadResult.Manifest;
                loadedManifests[fullPath] = referencedManifest;
            }

            var referencedTargetFramework = ResolveTargetFramework(referencedManifest, targetFrameworkOverride);
            if (!string.Equals(referencedTargetFramework, expectedTargetFramework, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestValue,
                    $"Project reference '{trimmed}' targets '{referencedTargetFramework}', but the dependent project targets '{expectedTargetFramework}'.",
                    option.File,
                    option.Line,
                    option.Column));
            }

            manifestStack.Add(fullPath);
            var referencedDirectReferences = ResolveDirectReferences(
                referencedManifest,
                referencedTargetFramework,
                targetFrameworkOverride,
                diagnostics,
                manifestStack,
                loadedManifests);
            manifestStack.RemoveAt(manifestStack.Count - 1);
            ReportDuplicateProjectReferenceNames(referencedDirectReferences, diagnostics);

            references.Add(new ProjectReferenceInfo(
                option,
                referencedManifest,
                CreateModuleMetadata(
                    referencedManifest,
                    diagnostics,
                    CreateExternalSourceModules(referencedDirectReferences))));
        }

        return references;
    }

    private static IReadOnlyList<ProjectReferenceModule> CreateModuleMetadata(
        TypeSharpManifest manifest,
        List<Diagnostic> diagnostics,
        IReadOnlyList<ExternalSourceModule>? externalSourceModules = null)
    {
        var discovery = SourceDiscovery.Discover(manifest);
        diagnostics.AddRange(discovery.Diagnostics);

        var parsedModules = new List<SourceModule>();
        foreach (var sourceFile in discovery.SourceFiles)
        {
            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
            diagnostics.AddRange(parseResult.Diagnostics);
            if (parseResult.Root is not null && !parseResult.HasErrors)
            {
                parsedModules.Add(new SourceModule(sourceFile, parseResult.Root));
            }
        }

        var graph = SourceModuleGraph.Build(parsedModules, manifest.Modules.Aliases, externalSourceModules);
        diagnostics.AddRange(graph.Diagnostics);

        var defaultNamespace = GetRootNamespace(manifest);
        return parsedModules
            .Select(module =>
            {
                var namespaceName = GetNamespaceName(module.Root, defaultNamespace);
                var moduleContainerName = GeneratedModuleContainerNaming.GetContainerName(module.SourceFile, parsedModules.Count);
                var exports = graph.ExportsByModulePath.TryGetValue(module.SourceFile.ModulePath, out var moduleExports)
                    ? moduleExports
                    : SourceModuleExports.Empty;
                return new ProjectReferenceModule(
                    $"{manifest.Project.Name}/{module.SourceFile.ModulePath}",
                    module.SourceFile.ModulePath,
                    namespaceName,
                    moduleContainerName,
                    exports);
            })
            .OrderBy(module => module.Specifier, StringComparer.OrdinalIgnoreCase)
            .ThenBy(module => module.Specifier, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ExternalSourceModule> CreateExternalSourceModules(IReadOnlyList<ProjectReferenceInfo> references) =>
        references
            .SelectMany(reference => reference.Modules.Select(module => new ExternalSourceModule(
                module.Specifier,
                module.Specifier,
                module.Exports)))
            .ToArray();

    private static void ReportDuplicateProjectReferenceNames(
        IReadOnlyList<ProjectReferenceInfo> references,
        List<Diagnostic> diagnostics)
    {
        var seen = new Dictionary<string, ProjectReferenceInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var reference in references)
        {
            var name = reference.Manifest.Project.Name.Trim();
            if (name.Length == 0)
            {
                continue;
            }

            if (seen.ContainsKey(name))
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestValue,
                    $"Project reference name '{name}' is ambiguous. Direct TypeSharp project references must have unique project names.",
                    reference.Option.File,
                    reference.Option.Line,
                    reference.Option.Column));
                continue;
            }

            seen.Add(name, reference);
        }
    }

    private static string GetNamespaceName(SyntaxNode root, string defaultNamespace)
    {
        var namespaceDeclaration = root.Children.FirstOrDefault(child => child.Kind == SyntaxKind.NamespaceDeclaration);
        var namespaceName = GetQualifiedName(namespaceDeclaration);
        return namespaceName.Length > 0 ? namespaceName : defaultNamespace;
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

    private static string NormalizeFullPath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
