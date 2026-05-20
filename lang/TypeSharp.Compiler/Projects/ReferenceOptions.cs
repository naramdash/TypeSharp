namespace TypeSharp.Compiler.Projects;

public sealed record ReferenceOptions(
    IReadOnlyList<string> Assemblies,
    IReadOnlyList<string> Paths,
    IReadOnlyList<string> Packages)
{
    public static ReferenceOptions Empty { get; } = new([], [], []);
}
