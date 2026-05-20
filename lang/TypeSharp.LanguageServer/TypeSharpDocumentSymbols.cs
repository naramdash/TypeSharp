using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Semantics;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentSymbols
{
    public static TypeSharpDocumentSymbol? FindSymbolAt(string text, string fileName, LspPosition position)
    {
        var model = TypeSharpSemanticModel.AnalyzeText(text, fileName);
        var symbol = model.FindSymbolAt(new SourcePosition(position.Line + 1, position.Character + 1));
        if (symbol is null)
        {
            return null;
        }

        return new TypeSharpDocumentSymbol(
            symbol.Name,
            symbol.Kind,
            symbol.File,
            symbol.SymbolSpan,
            symbol.TargetSpan,
            symbol.IsBuiltIn);
    }
}

public sealed record TypeSharpDocumentSymbol(
    string Name,
    BoundSymbolKind? Kind,
    string File,
    SourceSpan SymbolSpan,
    SourceSpan TargetSpan,
    bool IsBuiltIn);
