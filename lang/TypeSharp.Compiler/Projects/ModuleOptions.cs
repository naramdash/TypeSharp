namespace TypeSharp.Compiler.Projects;

public sealed record ModuleOptions(IReadOnlyList<SourceAliasOption> Aliases)
{
    public static ModuleOptions Empty { get; } = new([]);
}

public sealed record SourceAliasOption(
    string Pattern,
    string Target,
    string File,
    int Line,
    int Column);
