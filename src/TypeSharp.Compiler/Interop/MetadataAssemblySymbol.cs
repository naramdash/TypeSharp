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
    IReadOnlyList<MetadataPropertySymbol> Properties,
    IReadOnlyList<MetadataFieldSymbol> Fields)
{
    public string FullName => Namespace.Length == 0 ? Name : $"{Namespace}.{Name}";
}

public sealed record MetadataMethodSymbol(
    string Name,
    string ReturnType,
    MetadataNullabilityKind ReturnNullability,
    IReadOnlyList<MetadataParameterSymbol> Parameters);

public sealed record MetadataPropertySymbol(
    string Name,
    string Type);

public sealed record MetadataFieldSymbol(
    string Name,
    string Type,
    bool IsStatic,
    bool IsLiteral);

public sealed record MetadataParameterSymbol(
    string Name,
    string Type,
    MetadataByRefKind ByRefKind,
    bool IsParams,
    bool IsOptional);

public enum MetadataByRefKind
{
    None,
    Ref,
    Out,
    In
}

public enum MetadataNullabilityKind
{
    NotApplicable,
    Unknown,
    Annotated
}
