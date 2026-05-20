using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Lowering;

public interface ITypeSharpLoweringPass
{
    string Name { get; }

    SyntaxNode Lower(SyntaxNode root);
}
