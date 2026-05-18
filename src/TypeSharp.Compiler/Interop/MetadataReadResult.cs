using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Interop;

public sealed record MetadataReadResult(
    IReadOnlyList<MetadataAssemblySymbol> Assemblies,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
