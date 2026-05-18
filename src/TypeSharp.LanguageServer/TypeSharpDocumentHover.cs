using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentHover
{
    private static readonly HashSet<string> BuiltInTypes = new(StringComparer.Ordinal)
    {
        "bool",
        "byte",
        "char",
        "decimal",
        "double",
        "dynamic",
        "float",
        "int",
        "long",
        "never",
        "object",
        "sbyte",
        "short",
        "string",
        "uint",
        "ulong",
        "unit",
        "unknown",
        "ushort",
        "void"
    };

    public static LspHover? GetHover(string text, string fileName, LspPosition position)
    {
        var parseResult = TypeSharpParser.ParseText(text, fileName);
        if (parseResult.Root is null)
        {
            return null;
        }

        var target = FindIdentifierAt(parseResult.Root, position);
        if (target is null || string.IsNullOrWhiteSpace(target.Text))
        {
            return null;
        }

        var bindingResult = TypeSharpBinder.Bind(parseResult.Root, fileName);
        var symbol = FindBestSymbol(bindingResult.Symbols, target, fileName);
        if (symbol is not null)
        {
            return CreateHover(ToDisplayKind(symbol.Kind), symbol.Name, symbol.File, symbol.Span, target.Span);
        }

        if (BuiltInTypes.Contains(target.Text))
        {
            return CreateHover("type", target.Text, fileName, target.Span, target.Span);
        }

        return null;
    }

    private static SyntaxNode? FindIdentifierAt(SyntaxNode node, LspPosition position)
    {
        if (node.IsToken)
        {
            return node.Kind == SyntaxKind.IdentifierToken && Contains(node.Span, position)
                ? node
                : null;
        }

        foreach (var child in node.Children)
        {
            var match = FindIdentifierAt(child, position);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static BoundSymbol? FindBestSymbol(
        IReadOnlyList<BoundSymbol> symbols,
        SyntaxNode target,
        string fileName)
    {
        var name = target.Text ?? string.Empty;
        var candidates = symbols
            .Where(symbol => string.Equals(symbol.Name, name, StringComparison.Ordinal)
                && string.Equals(symbol.File, fileName, StringComparison.Ordinal))
            .ToArray();

        return candidates.FirstOrDefault(symbol => symbol.Span.Equals(target.Span))
            ?? candidates
                .Where(symbol => Compare(symbol.Span.Start, target.Span.Start) <= 0)
                .OrderByDescending(symbol => symbol.Span.Start.Line)
                .ThenByDescending(symbol => symbol.Span.Start.Column)
                .FirstOrDefault()
            ?? candidates.FirstOrDefault();
    }

    private static LspHover CreateHover(
        string kind,
        string name,
        string file,
        SourceSpan symbolSpan,
        SourceSpan targetSpan)
    {
        var value = $"**{kind}** `{name}`\n\nDefined in `{file}:{symbolSpan.Start.Line}:{symbolSpan.Start.Column}`.";
        return new LspHover(
            new LspMarkupContent("markdown", value),
            LspDiagnosticMapper.ToLspRange(targetSpan));
    }

    private static string ToDisplayKind(BoundSymbolKind kind) =>
        kind switch
        {
            BoundSymbolKind.Namespace => "namespace",
            BoundSymbolKind.Import => "import",
            BoundSymbolKind.Type => "type",
            BoundSymbolKind.Function => "function",
            BoundSymbolKind.Value => "value",
            BoundSymbolKind.Parameter => "parameter",
            BoundSymbolKind.Local => "local",
            _ => "symbol"
        };

    private static bool Contains(SourceSpan span, LspPosition position)
    {
        var sourcePosition = new SourcePosition(position.Line + 1, position.Character + 1);
        return Compare(sourcePosition, span.Start) >= 0 && Compare(sourcePosition, span.End) < 0;
    }

    private static int Compare(SourcePosition left, SourcePosition right)
    {
        var line = left.Line.CompareTo(right.Line);
        return line != 0 ? line : left.Column.CompareTo(right.Column);
    }
}

public sealed record LspHover(LspMarkupContent Contents, LspRange? Range);

public sealed record LspMarkupContent(string Kind, string Value);
