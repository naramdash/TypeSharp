namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceValueReExport(
    string QualifiedModuleContainer,
    string ExportedName,
    string TargetMemberName,
    string Type,
    bool IsMutable);
