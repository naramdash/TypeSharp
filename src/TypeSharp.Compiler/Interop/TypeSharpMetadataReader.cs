using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpMetadataReader
{
    public static MetadataReadResult Read(ReferenceResolutionResult resolutionResult)
    {
        var diagnostics = new List<Diagnostic>(resolutionResult.Diagnostics);
        var assemblies = ReadAssemblies(resolutionResult.References, diagnostics);
        return new MetadataReadResult(assemblies, diagnostics);
    }

    public static MetadataReadResult Read(IEnumerable<ResolvedReference> references)
    {
        var diagnostics = new List<Diagnostic>();
        var assemblies = ReadAssemblies(references, diagnostics);
        return new MetadataReadResult(assemblies, diagnostics);
    }

    private static List<MetadataAssemblySymbol> ReadAssemblies(
        IEnumerable<ResolvedReference> references,
        List<Diagnostic> diagnostics)
    {
        var assemblies = new List<MetadataAssemblySymbol>();

        foreach (var reference in references)
        {
            if (reference.Kind == ResolvedReferenceKind.LocalAssembly && !CanReadLocalReference(reference, diagnostics))
            {
                continue;
            }

            assemblies.Add(new MetadataAssemblySymbol(
                reference.Identity,
                reference.Kind,
                reference.OriginalText,
                reference.Path,
                reference.RelativePath));
        }

        return assemblies;
    }

    private static bool CanReadLocalReference(ResolvedReference reference, List<Diagnostic> diagnostics)
    {
        var displayPath = reference.RelativePath ?? reference.OriginalText;
        var diagnosticFile = reference.Path ?? displayPath;

        if (string.IsNullOrWhiteSpace(reference.Path) || !File.Exists(reference.Path))
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.MissingReference,
                $"Referenced assembly path '{displayPath}' does not exist.",
                diagnosticFile));
            return false;
        }

        try
        {
            using var stream = File.Open(reference.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.MissingReference,
                $"Referenced assembly path '{displayPath}' cannot be read.",
                diagnosticFile));
            return false;
        }
    }
}
