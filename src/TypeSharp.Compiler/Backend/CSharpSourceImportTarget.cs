namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceImportTarget(
    string Specifier,
    string NamespaceName,
    string ModuleContainerName)
{
    public string QualifiedModuleContainer => $"{NamespaceName}.{ModuleContainerName}";
}
