using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.LanguageServer;

public static class LspDiagnosticMapper
{
    public static LspDiagnostic ToLspDiagnostic(Diagnostic diagnostic) =>
        new(
            ToLspRange(diagnostic.Span),
            ToLspSeverity(diagnostic.Severity),
            "typesharp",
            diagnostic.Code,
            diagnostic.Message);

    public static IReadOnlyList<LspDiagnostic> ToLspDiagnostics(IReadOnlyList<Diagnostic> diagnostics) =>
        diagnostics.Select(ToLspDiagnostic).ToArray();

    public static LspRange ToLspRange(SourceSpan span) =>
        new(ToLspPosition(span.Start), ToLspPosition(span.End));

    public static LspPosition ToLspPosition(SourcePosition position) =>
        new(
            Math.Max(position.Line - 1, 0),
            Math.Max(position.Column - 1, 0));

    private static int ToLspSeverity(DiagnosticSeverity severity) =>
        severity switch
        {
            DiagnosticSeverity.Error => 1,
            DiagnosticSeverity.Warning => 2,
            DiagnosticSeverity.Info => 3,
            _ => 3
        };
}

public sealed record LspDiagnostic(
    LspRange Range,
    int Severity,
    string Source,
    string Code,
    string Message);

public sealed record LspRange(LspPosition Start, LspPosition End);

public sealed record LspPosition(int Line, int Character);
