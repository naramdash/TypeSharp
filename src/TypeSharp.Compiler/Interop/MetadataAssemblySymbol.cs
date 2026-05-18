namespace TypeSharp.Compiler.Interop;

public sealed record MetadataAssemblySymbol(
    string Identity,
    ResolvedReferenceKind ReferenceKind,
    string OriginalText,
    string? Path,
    string? RelativePath)
{
    public IReadOnlyList<MetadataTypeSymbol> Types { get; init; } = [];

    public bool IsFrameworkAssembly => ReferenceKind == ResolvedReferenceKind.FrameworkAssembly;

    public bool IsLocalAssembly => ReferenceKind == ResolvedReferenceKind.LocalAssembly;
}

public sealed record MetadataTypeSymbol(
    string Namespace,
    string Name,
    IReadOnlyList<MetadataMethodSymbol> Methods,
    IReadOnlyList<MetadataPropertySymbol> Properties)
{
    public string FullName => Namespace.Length == 0 ? Name : $"{Namespace}.{Name}";
}

public sealed record MetadataMethodSymbol(
    string Name,
    string ReturnType,
    IReadOnlyList<MetadataParameterSymbol> Parameters);

public sealed record MetadataPropertySymbol(
    string Name);

public sealed record MetadataParameterSymbol(
    string Name,
    string Type,
    MetadataByRefKind ByRefKind);

public enum MetadataByRefKind
{
    None,
    Ref,
    Out,
    In
}
