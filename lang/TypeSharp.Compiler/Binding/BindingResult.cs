using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Binding;

public sealed record BindingResult(
    IReadOnlyList<BoundSymbol> Symbols,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
