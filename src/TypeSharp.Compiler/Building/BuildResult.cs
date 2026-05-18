using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Building;

public sealed record BuildResult(
    IReadOnlyList<GeneratedCSharpFile> GeneratedFiles,
    GeneratedCSharpProject? GeneratedProject,
    GeneratedAssembly? GeneratedAssembly,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
