using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceFunctionReExport(
    string QualifiedModuleContainer,
    string ExportedName,
    SyntaxNode Function);
