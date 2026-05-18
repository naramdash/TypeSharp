using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Parsing;

public sealed record SyntaxToken(
    SyntaxKind Kind,
    string Text,
    SourceSpan Span,
    bool IsMissing = false,
    string LeadingTriviaSummary = "");
