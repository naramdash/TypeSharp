namespace TypeSharp.Compiler.Diagnostics;

public sealed record DiagnosticDescriptor(
    string Code,
    string Title,
    DiagnosticSeverity DefaultSeverity,
    DiagnosticCategory Category,
    string MessageTemplate,
    string Explanation,
    string SuggestedAction);
