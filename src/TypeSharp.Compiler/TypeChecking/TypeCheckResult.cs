using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.TypeChecking;

public sealed record TypeCheckResult(IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
