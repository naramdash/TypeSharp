namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceImportTarget(
    string Specifier,
    string NamespaceName,
    string ModuleContainerName)
{
    public IReadOnlyDictionary<string, string> TypeExportTargets { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, string> ModuleExportTargets { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public string QualifiedModuleContainer => $"{NamespaceName}.{ModuleContainerName}";

    public bool HasExportedTypeTarget(string exportedName) =>
        TypeExportTargets.ContainsKey(exportedName);

    public string ResolveExportedTypeQualifiedName(string exportedName) =>
        TypeExportTargets.TryGetValue(exportedName, out var targetName)
            ? targetName
            : $"{NamespaceName}.{exportedName}";

    public bool HasExportedModuleTarget(string exportedName) =>
        ModuleExportTargets.ContainsKey(exportedName);

    public string ResolveExportedModuleQualifiedName(string exportedName) =>
        ModuleExportTargets.TryGetValue(exportedName, out var targetName)
            ? targetName
            : $"{NamespaceName}.{exportedName}";
}
