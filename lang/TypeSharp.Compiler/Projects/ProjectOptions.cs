namespace TypeSharp.Compiler.Projects;

public sealed record ProjectOptions(
    string Name,
    string TargetFramework,
    string OutputType,
    string? RootNamespace,
    IReadOnlyList<string> SourceRoots,
    string? Main,
    string GeneratedOutputRoot)
{
    public static ProjectOptions Default(string projectDirectory) => new(
        Path.GetFileName(projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
        TypeSharpCompilerInfo.DefaultTargetFramework,
        "library",
        RootNamespace: null,
        SourceRoots: ["src"],
        Main: null,
        GeneratedOutputRoot: "obj/generated");
}
