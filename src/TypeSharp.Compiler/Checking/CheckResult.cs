using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Projects;

namespace TypeSharp.Compiler.Checking;

public sealed record CheckResult(
    IReadOnlyList<SourceFile> SourceFiles,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
