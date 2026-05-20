using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Interop;

public sealed record ReferenceResolutionResult(
    IReadOnlyList<ResolvedReference> References,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
