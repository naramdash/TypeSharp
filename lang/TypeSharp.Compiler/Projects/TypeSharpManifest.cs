namespace TypeSharp.Compiler.Projects;

public sealed record TypeSharpManifest(
    string ManifestPath,
    string ProjectDirectory,
    ProjectOptions Project,
    LanguageOptions Language,
    ModuleOptions Modules,
    ProjectReferenceOptions ProjectReferences,
    ReferenceOptions References,
    ToolingOptions Tooling);
