using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.TypeChecking;

namespace TypeSharp.Compiler.Semantics;

public sealed class TypeSharpSemanticModel
{
    public static IReadOnlyList<string> BuiltInTypeNames { get; } =
    [
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
    ];

    private static readonly HashSet<string> BuiltInTypes = new(BuiltInTypeNames, StringComparer.Ordinal);

    private TypeSharpSemanticModel(
        string file,
        SyntaxNode? root,
        IReadOnlyList<BoundSymbol> symbols,
        IReadOnlyList<Diagnostic> diagnostics)
    {
        File = file;
        Root = root;
        Symbols = symbols;
        Diagnostics = diagnostics;
    }

    public string File { get; }

    public SyntaxNode? Root { get; }

    public IReadOnlyList<BoundSymbol> Symbols { get; }

    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public static TypeSharpSemanticModel AnalyzeText(
        string text,
        string file,
        bool includeSymbolsForParseErrors = false)
    {
        var diagnostics = new List<Diagnostic>();
        var parseResult = TypeSharpParser.ParseText(text, file);
        diagnostics.AddRange(parseResult.Diagnostics);

        if (parseResult.Root is null || (parseResult.HasErrors && !includeSymbolsForParseErrors))
        {
            return new TypeSharpSemanticModel(file, parseResult.Root, [], diagnostics);
        }

        var bindingResult = TypeSharpBinder.Bind(parseResult.Root, file);
        diagnostics.AddRange(bindingResult.Diagnostics);
        if (!parseResult.HasErrors && !bindingResult.HasErrors)
        {
            diagnostics.AddRange(TypeSharpTypeChecker.Check(parseResult.Root, file).Diagnostics);
        }

        return new TypeSharpSemanticModel(file, parseResult.Root, bindingResult.Symbols, diagnostics);
    }

    public TypeSharpSemanticSymbol? FindSymbolAt(SourcePosition position)
    {
        if (Root is null)
        {
            return null;
        }

        var target = FindIdentifierAt(Root, position);
        if (target is null || string.IsNullOrWhiteSpace(target.Text))
        {
            return null;
        }

        var symbol = FindBestSymbol(target);
        if (symbol is not null)
        {
            return new TypeSharpSemanticSymbol(
                symbol.Name,
                symbol.Kind,
                symbol.File,
                symbol.Span,
                target.Span,
                IsBuiltIn: false);
        }

        if (BuiltInTypes.Contains(target.Text))
        {
            return new TypeSharpSemanticSymbol(
                target.Text,
                Kind: null,
                File,
                target.Span,
                target.Span,
                IsBuiltIn: true);
        }

        return null;
    }

    private SyntaxNode? FindIdentifierAt(SyntaxNode node, SourcePosition position)
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

    private BoundSymbol? FindBestSymbol(SyntaxNode target)
    {
        var name = target.Text ?? string.Empty;
        var candidates = Symbols
            .Where(symbol => string.Equals(symbol.Name, name, StringComparison.Ordinal)
                && string.Equals(symbol.File, File, StringComparison.Ordinal))
            .ToArray();

        return candidates.FirstOrDefault(symbol => symbol.Span.Equals(target.Span))
            ?? candidates
                .Where(symbol => Compare(symbol.Span.Start, target.Span.Start) <= 0)
                .OrderByDescending(symbol => symbol.Span.Start.Line)
                .ThenByDescending(symbol => symbol.Span.Start.Column)
                .FirstOrDefault()
            ?? candidates.FirstOrDefault();
    }

    private static bool Contains(SourceSpan span, SourcePosition position) =>
        Compare(position, span.Start) >= 0 && Compare(position, span.End) < 0;

    private static int Compare(SourcePosition left, SourcePosition right)
    {
        var line = left.Line.CompareTo(right.Line);
        return line != 0 ? line : left.Column.CompareTo(right.Column);
    }
}

public sealed record TypeSharpSemanticSymbol(
    string Name,
    BoundSymbolKind? Kind,
    string File,
    SourceSpan SymbolSpan,
    SourceSpan TargetSpan,
    bool IsBuiltIn);
