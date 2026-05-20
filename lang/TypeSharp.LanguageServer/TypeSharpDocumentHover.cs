using TypeSharp.Compiler.Binding;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentHover
{
    public static LspHover? GetHover(string text, string fileName, LspPosition position)
    {
        var symbol = TypeSharpDocumentSymbols.FindSymbolAt(text, fileName, position);
        if (symbol is null)
        {
            return null;
        }

        var kind = symbol.Kind.HasValue ? ToDisplayKind(symbol.Kind.Value) : "type";
        var value = $"**{kind}** `{symbol.Name}`\n\nDefined in `{symbol.File}:{symbol.SymbolSpan.Start.Line}:{symbol.SymbolSpan.Start.Column}`.";
        return new LspHover(
            new LspMarkupContent("markdown", value),
            LspDiagnosticMapper.ToLspRange(symbol.TargetSpan));
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
}

public sealed record LspHover(LspMarkupContent Contents, LspRange? Range);

public sealed record LspMarkupContent(string Kind, string Value);
