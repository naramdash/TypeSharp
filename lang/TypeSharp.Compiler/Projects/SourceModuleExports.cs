namespace TypeSharp.Compiler.Projects;

public sealed record SourceModuleExports(
    IReadOnlySet<string> ValueNames,
    IReadOnlySet<string> TypeNames,
    IReadOnlySet<string> ImportAliasValueNames,
    IReadOnlySet<string> FunctionNames,
    IReadOnlySet<string> ModuleNames)
{
    public static SourceModuleExports Empty { get; } = new(
        new HashSet<string>(StringComparer.Ordinal),
        new HashSet<string>(StringComparer.Ordinal),
        new HashSet<string>(StringComparer.Ordinal),
        new HashSet<string>(StringComparer.Ordinal),
        new HashSet<string>(StringComparer.Ordinal));
}

public sealed record ExternalSourceModule(
    string Specifier,
    string ModulePath,
    SourceModuleExports Exports);
