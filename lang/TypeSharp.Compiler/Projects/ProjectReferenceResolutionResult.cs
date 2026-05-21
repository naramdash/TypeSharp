using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public sealed record ProjectReferenceResolutionResult(
    IReadOnlyList<ProjectReferenceInfo> DirectReferences,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public IReadOnlyList<ExternalSourceModule> ExternalSourceModules =>
        DirectReferences
            .SelectMany(reference => reference.Modules.Select(module => new ExternalSourceModule(
                module.Specifier,
                module.Specifier,
                module.Exports)))
            .ToArray();
}

public sealed record ProjectReferenceInfo(
    ProjectReferencePathOption Option,
    TypeSharpManifest Manifest,
    IReadOnlyList<ProjectReferenceModule> Modules);

public sealed record ProjectReferenceModule(
    string Specifier,
    string ModulePath,
    string NamespaceName,
    string ModuleContainerName,
    SourceModuleExports Exports)
{
    public string QualifiedModuleContainer => $"{NamespaceName}.{ModuleContainerName}";
}
