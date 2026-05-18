namespace TypeSharp.Compiler.Projects;

public sealed record LanguageOptions(
    string Version,
    bool Strict,
    string Nullable,
    IReadOnlyList<string> PreviewFeatures)
{
    public static LanguageOptions Default { get; } = new(
        "preview",
        Strict: true,
        Nullable: "strict",
        PreviewFeatures: []);
}
