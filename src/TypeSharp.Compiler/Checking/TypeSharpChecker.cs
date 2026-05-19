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

        var sourceDiscovery = SourceDiscovery.Discover(manifestResult.Manifest);
        diagnostics.AddRange(sourceDiscovery.Diagnostics);

        var metadataResult = TypeSharpMetadataReader.Read(TypeSharpReferenceResolver.Resolve(manifestResult.Manifest));
        diagnostics.AddRange(metadataResult.Diagnostics);

        var parsedSources = new List<SourceModule>();
        foreach (var sourceFile in sourceDiscovery.SourceFiles)
        {
            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
            diagnostics.AddRange(parseResult.Diagnostics);
            if (parseResult.HasErrors || parseResult.Root is null)
            {
                continue;
            }

            parsedSources.Add(new SourceModule(sourceFile, parseResult.Root));
        }

        diagnostics.AddRange(SourceModuleGraph.Build(parsedSources).Diagnostics);

        foreach (var parsedSource in parsedSources)
        {
            diagnostics.AddRange(TypeSharpInteropValidator.Validate(
                parsedSource.Root,
                metadataResult.Assemblies,
                parsedSource.SourceFile.RelativePath,
                manifestResult.Manifest.Language.Nullable));
            var bindingResult = TypeSharpBinder.Bind(parsedSource.Root, parsedSource.SourceFile.RelativePath);
            diagnostics.AddRange(bindingResult.Diagnostics);
            if (!bindingResult.HasErrors)
            {
                diagnostics.AddRange(TypeSharpTypeChecker.Check(parsedSource.Root, parsedSource.SourceFile.RelativePath).Diagnostics);
            }
        }

        return new CheckResult(sourceDiscovery.SourceFiles, diagnostics);
    }
}
