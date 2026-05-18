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

        foreach (var sourceFile in sourceDiscovery.SourceFiles)
        {
            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
            diagnostics.AddRange(parseResult.Diagnostics);
            if (!parseResult.HasErrors && parseResult.Root is not null)
            {
                var bindingResult = TypeSharpBinder.Bind(parseResult.Root, sourceFile.RelativePath);
                diagnostics.AddRange(bindingResult.Diagnostics);
                if (!bindingResult.HasErrors)
                {
                    diagnostics.AddRange(TypeSharpTypeChecker.Check(parseResult.Root, sourceFile.RelativePath).Diagnostics);
                }
            }
        }

        return new CheckResult(sourceDiscovery.SourceFiles, diagnostics);
    }
}
