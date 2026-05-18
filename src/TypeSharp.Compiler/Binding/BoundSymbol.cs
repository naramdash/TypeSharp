using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Binding;

public sealed record BoundSymbol(
    string Name,
    BoundSymbolKind Kind,
    string File,
    SourceSpan Span);
