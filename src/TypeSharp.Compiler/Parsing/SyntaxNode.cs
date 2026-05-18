using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Parsing;

public sealed class SyntaxNode
{
    public SyntaxNode(
        SyntaxKind kind,
        SourceSpan span,
        string? text = null,
        bool isToken = false,
        bool isMissing = false,
        IReadOnlyList<SyntaxNode>? children = null)
    {
        Kind = kind;
        Span = span;
        Text = text;
        IsToken = isToken;
        IsMissing = isMissing;
        Children = children ?? [];
    }

    public SyntaxKind Kind { get; }
    public SourceSpan Span { get; }
    public string? Text { get; }
    public bool IsToken { get; }
    public bool IsMissing { get; }
    public IReadOnlyList<SyntaxNode> Children { get; }

    public static SyntaxNode Token(SyntaxToken token) =>
        new(token.Kind, token.Span, token.Text, isToken: true);

    public static SyntaxNode Missing(SyntaxKind kind, SourcePosition position) =>
        new(kind, new SourceSpan(position, position), isMissing: true);
}
