using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public sealed class CSharpSourceBackendAdapter : ITypeSharpBackend
{
    public static CSharpSourceBackendAdapter Instance { get; } = new();

    private CSharpSourceBackendAdapter()
    {
    }

    public string Name => "csharp";

    public TypeSharpBackendArtifactKind ArtifactKind => TypeSharpBackendArtifactKind.SourceText;

    public string GeneratedArtifactExtension => ".g.cs";

    public TypeSharpBackendArtifact Emit(SyntaxNode root) =>
        TypeSharpBackendArtifact.SourceText(GeneratedArtifactExtension, CSharpSourceBackend.Emit(root));

    public TypeSharpBackendArtifact Emit(SyntaxNode root, string? defaultNamespace) =>
        TypeSharpBackendArtifact.SourceText(GeneratedArtifactExtension, CSharpSourceBackend.Emit(root, defaultNamespace));

    public TypeSharpBackendArtifact Emit(SyntaxNode root, string? defaultNamespace, string moduleContainerName) =>
        TypeSharpBackendArtifact.SourceText(GeneratedArtifactExtension, CSharpSourceBackend.Emit(root, defaultNamespace, moduleContainerName));
}
