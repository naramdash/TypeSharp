using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public sealed record SourceDiscoveryResult(
    IReadOnlyList<SourceFile> SourceFiles,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
