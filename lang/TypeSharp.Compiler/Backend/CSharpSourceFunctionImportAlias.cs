using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceFunctionImportAlias(
    string QualifiedModuleContainer,
    string LocalName,
    SyntaxNode Function);
