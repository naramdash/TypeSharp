using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public sealed record ManifestPathResult(
    string? ManifestPath,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
