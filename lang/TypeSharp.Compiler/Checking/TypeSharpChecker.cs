using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Interop;
using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;
using TypeSharp.Compiler.TypeChecking;

namespace TypeSharp.Compiler.Checking;

public static class TypeSharpChecker
{
    public static CheckResult Check(string manifestPath)
    {
        var diagnostics = new List<Diagnostic>();
        var manifestResult = TypeSharpManifestLoader.Load(manifestPath);
        diagnostics.AddRange(manifestResult.Diagnostics);

        if (manifestResult.Manifest is null)
        {
            return new CheckResult([], diagnostics);
        }

        var projectReferences = TypeSharpProjectReferenceResolver.Resolve(manifestResult.Manifest);
        diagnostics.AddRange(projectReferences.Diagnostics);
        var externalSourceModules = projectReferences.ExternalSourceModules;
        var sourceModuleSpecifiers = externalSourceModules
            .Select(module => module.Specifier)
            .ToHashSet(StringComparer.Ordinal);

        var sourceDiscovery = SourceDiscovery.Discover(manifestResult.Manifest);
        diagnostics.AddRange(sourceDiscovery.Diagnostics);

        var metadataResult = TypeSharpMetadataReader.Read(TypeSharpReferenceResolver.Resolve(manifestResult.Manifest));
        diagnostics.AddRange(metadataResult.Diagnostics);

        var parsedSourceResults = sourceDiscovery.SourceFiles
            .AsParallel()
            .AsOrdered()
            .Select(ParseSource)
            .ToArray();
        var parsedSources = new List<SourceModule>(parsedSourceResults.Length);
        foreach (var parsedSourceResult in parsedSourceResults)
        {
            diagnostics.AddRange(parsedSourceResult.Diagnostics);
            if (parsedSourceResult.Module is not null)
            {
                parsedSources.Add(parsedSourceResult.Module);
            }
        }

        diagnostics.AddRange(SourceModuleGraph.Build(parsedSources, manifestResult.Manifest.Modules.Aliases, externalSourceModules).Diagnostics);

        var sourceDiagnostics = parsedSources
            .AsParallel()
            .AsOrdered()
            .Select(parsedSource => CheckSource(
                parsedSource,
                metadataResult.Assemblies,
                manifestResult.Manifest.Language.Nullable,
                manifestResult.Manifest.Modules.Aliases,
                sourceModuleSpecifiers))
            .ToArray();
        foreach (var sourceDiagnostic in sourceDiagnostics)
        {
            diagnostics.AddRange(sourceDiagnostic.Diagnostics);
        }

        return new CheckResult(sourceDiscovery.SourceFiles, diagnostics);
    }

    private static SourceParseResult ParseSource(SourceFile sourceFile)
    {
        var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
        if (parseResult.HasErrors || parseResult.Root is null)
        {
            return new SourceParseResult(null, parseResult.Diagnostics);
        }

        return new SourceParseResult(new SourceModule(sourceFile, parseResult.Root), parseResult.Diagnostics);
    }

    private static SourceDiagnostics CheckSource(
        SourceModule parsedSource,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string nullableMode,
        IReadOnlyList<SourceAliasOption> sourceAliases,
        IReadOnlySet<string> sourceModuleSpecifiers)
    {
        var diagnostics = new List<Diagnostic>();
        diagnostics.AddRange(TypeSharpInteropValidator.Validate(
            parsedSource.Root,
            assemblies,
            parsedSource.SourceFile.RelativePath,
            nullableMode,
            sourceAliases,
            sourceModuleSpecifiers));
        var bindingResult = TypeSharpBinder.Bind(parsedSource.Root, parsedSource.SourceFile.RelativePath, sourceAliases);
        diagnostics.AddRange(bindingResult.Diagnostics);
        if (!bindingResult.HasErrors)
        {
            diagnostics.AddRange(TypeSharpTypeChecker.Check(
                parsedSource.Root,
                parsedSource.SourceFile.RelativePath,
                assemblies,
                sourceAliases,
                sourceModuleSpecifiers).Diagnostics);
        }

        return new SourceDiagnostics(diagnostics);
    }

    private sealed record SourceParseResult(SourceModule? Module, IReadOnlyList<Diagnostic> Diagnostics);

    private sealed record SourceDiagnostics(IReadOnlyList<Diagnostic> Diagnostics);
}
