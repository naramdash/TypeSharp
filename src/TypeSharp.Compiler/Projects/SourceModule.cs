using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Projects;

public sealed record SourceModule(SourceFile SourceFile, SyntaxNode Root);
