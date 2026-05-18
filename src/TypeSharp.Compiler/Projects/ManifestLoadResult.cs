using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public sealed record ManifestLoadResult(
    TypeSharpManifest? Manifest,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
