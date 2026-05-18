namespace TypeSharp.Compiler.Diagnostics;

public sealed record Diagnostic(
    string Code,
    DiagnosticSeverity Severity,
    string Message,
    string File,
    SourceSpan Span)
{
    public string ToCliText()
    {
        var severity = Severity.ToString().ToLowerInvariant();
        return $"{File}({Span.Start.Line},{Span.Start.Column}): {severity} {Code}: {Message}";
    }
}
