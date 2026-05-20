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
    IReadOnlyList<MetadataFieldSymbol> Fields,
    IReadOnlyList<MetadataEventSymbol> Events)
{
    public string FullName => Namespace.Length == 0 ? Name : $"{Namespace}.{Name}";

    public bool IsValueType { get; init; }

    public bool IsInterface { get; init; }

    public bool HasPublicParameterlessConstructor { get; init; }

    public IReadOnlyList<MetadataMethodSymbol> Constructors { get; init; } = [];

    public string? BaseTypeName { get; init; }

    public IReadOnlyList<string> InterfaceNames { get; init; } = [];
}

public sealed record MetadataMethodSymbol(
    string Name,
    string ReturnType,
    MetadataNullabilityKind ReturnNullability,
    IReadOnlyList<MetadataParameterSymbol> Parameters,
    bool IsStatic = true,
    int GenericParameterCount = 0,
    bool IsExtension = false)
{
    public IReadOnlyList<MetadataGenericParameterSymbol> GenericParameters { get; init; } = [];
}

public sealed record MetadataGenericParameterSymbol(
    string Name,
    bool HasReferenceTypeConstraint,
    bool HasNotNullableValueTypeConstraint,
    bool HasDefaultConstructorConstraint,
    IReadOnlyList<string> TypeConstraints);

public sealed record MetadataPropertySymbol(
    string Name,
    string Type,
    bool IsStatic = true,
    bool HasPublicGetter = true,
    bool HasPublicSetter = false,
    bool IsIndexer = false,
    int ParameterCount = 0)
{
    public IReadOnlyList<string> ParameterTypes { get; init; } = [];
}

public sealed record MetadataFieldSymbol(
    string Name,
    string Type,
    bool IsStatic,
    bool IsLiteral,
    bool IsReadOnly = false);

public sealed record MetadataEventSymbol(
    string Name,
    string Type,
    bool IsStatic,
    bool HasPublicAdder,
    bool HasPublicRemover);

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
