using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public interface ITypeSharpBackend
{
    string Name { get; }

    TypeSharpBackendArtifactKind ArtifactKind { get; }

    string GeneratedArtifactExtension { get; }

    TypeSharpBackendArtifact Emit(SyntaxNode root);
}
