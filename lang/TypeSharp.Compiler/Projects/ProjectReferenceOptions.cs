namespace TypeSharp.Compiler.Projects;

public sealed record ProjectReferenceOptions(IReadOnlyList<ProjectReferencePathOption> Paths)
{
    public static ProjectReferenceOptions Empty { get; } = new([]);
}

public sealed record ProjectReferencePathOption(
    string Path,
    string File,
    int Line,
    int Column);
