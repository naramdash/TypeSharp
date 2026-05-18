namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentDefinition
{
    public static LspLocation? GetDefinition(string text, string fileName, string documentUri, LspPosition position)
    {
        var symbol = TypeSharpDocumentSymbols.FindSymbolAt(text, fileName, position);
        if (symbol is null || symbol.IsBuiltIn)
        {
            return null;
        }

        return new LspLocation(documentUri, LspDiagnosticMapper.ToLspRange(symbol.SymbolSpan));
    }
}

public sealed record LspLocation(string Uri, LspRange Range);
