namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceValueImportAlias(
    string QualifiedModuleContainer,
    string LocalName,
    string TargetMemberName,
    string Type,
    bool IsMutable);
