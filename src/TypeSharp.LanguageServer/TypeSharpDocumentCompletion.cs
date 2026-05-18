using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentCompletion
{
    private static readonly IReadOnlyList<string> Keywords =
    [
        "namespace",
        "import",
        "export",
        "fun",
        "let",
        "literal",
        "type",
        "record",
        "union",
        "match",
        "if",
        "else",
        "for",
        "async",
        "await"
    ];

    public static IReadOnlyList<LspCompletionItem> GetCompletions(string text, string fileName, LspPosition position)
    {
        var prefix = GetPrefix(text, position);
        var items = new Dictionary<string, LspCompletionItem>(StringComparer.Ordinal);
        var parseResult = TypeSharpParser.ParseText(text, fileName);
        if (parseResult.Root is not null)
        {
            var bindingResult = TypeSharpBinder.Bind(parseResult.Root, fileName);
            foreach (var symbol in bindingResult.Symbols)
            {
                AddItem(items, prefix, symbol.Name, ToCompletionKind(symbol.Kind), ToDisplayKind(symbol.Kind));
            }
        }

        foreach (var type in TypeSharpDocumentSymbols.BuiltInTypeNames)
        {
            AddItem(items, prefix, type, LspCompletionItemKind.Keyword, "built-in type");
        }

        foreach (var keyword in Keywords)
        {
            AddItem(items, prefix, keyword, LspCompletionItemKind.Keyword, "keyword");
        }

        return items.Values
            .OrderBy(item => item.Label, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddItem(
        IDictionary<string, LspCompletionItem> items,
        string prefix,
        string label,
        int kind,
        string detail)
    {
        if (string.IsNullOrWhiteSpace(label) || !MatchesPrefix(label, prefix) || items.ContainsKey(label))
        {
            return;
        }

        items[label] = new LspCompletionItem(label, kind, detail);
    }

    private static bool MatchesPrefix(string label, string prefix) =>
        prefix.Length == 0 || label.StartsWith(prefix, StringComparison.Ordinal);

    private static string GetPrefix(string text, LspPosition position)
    {
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        if (position.Line < 0 || position.Line >= lines.Length)
        {
            return string.Empty;
        }

        var line = lines[position.Line];
        var end = Math.Clamp(position.Character, 0, line.Length);
        var start = end;
        while (start > 0 && IsIdentifierPart(line[start - 1]))
        {
            start--;
        }

        return line[start..end];
    }

    private static bool IsIdentifierPart(char character) =>
        character == '_' || char.IsLetterOrDigit(character);

    private static int ToCompletionKind(BoundSymbolKind kind) =>
        kind switch
        {
            BoundSymbolKind.Namespace => LspCompletionItemKind.Module,
            BoundSymbolKind.Type => LspCompletionItemKind.Class,
            BoundSymbolKind.Function => LspCompletionItemKind.Function,
            BoundSymbolKind.Value => LspCompletionItemKind.Value,
            BoundSymbolKind.Parameter => LspCompletionItemKind.Variable,
            BoundSymbolKind.Local => LspCompletionItemKind.Variable,
            BoundSymbolKind.Import => LspCompletionItemKind.Reference,
            _ => LspCompletionItemKind.Text
        };

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

public sealed record LspCompletionItem(string Label, int Kind, string Detail);

public static class LspCompletionItemKind
{
    public const int Text = 1;
    public const int Function = 3;
    public const int Variable = 6;
    public const int Class = 7;
    public const int Module = 9;
    public const int Value = 12;
    public const int Keyword = 14;
    public const int Reference = 18;
}
