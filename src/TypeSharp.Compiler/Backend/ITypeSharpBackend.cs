using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public interface ITypeSharpBackend
{
    string Name { get; }

    string GeneratedSourceExtension { get; }

    string Emit(SyntaxNode root);
}
