namespace TypeSharp.Compiler.Backend;

public sealed record CSharpSourceEnumImportShape(
    string LocalName,
    IReadOnlyList<string> Members);
