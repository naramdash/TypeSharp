namespace TypeSharp.Compiler.Interop;

public sealed record ResolvedReference(
    ResolvedReferenceKind Kind,
    string Identity,
    string OriginalText,
    string? Path,
    string? RelativePath);
