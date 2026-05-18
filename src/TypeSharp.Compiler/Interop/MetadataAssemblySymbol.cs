namespace TypeSharp.Compiler.Interop;

public sealed record MetadataAssemblySymbol(
    string Identity,
    ResolvedReferenceKind ReferenceKind,
    string OriginalText,
    string? Path,
    string? RelativePath)
{
    public bool IsFrameworkAssembly => ReferenceKind == ResolvedReferenceKind.FrameworkAssembly;

    public bool IsLocalAssembly => ReferenceKind == ResolvedReferenceKind.LocalAssembly;
}
