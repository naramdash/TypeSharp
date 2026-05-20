namespace TypeSharp.Compiler.Projects;

public sealed record TypeSharpManifest(
    string ManifestPath,
    string ProjectDirectory,
    ProjectOptions Project,
    LanguageOptions Language,
    ReferenceOptions References,
    ToolingOptions Tooling);
