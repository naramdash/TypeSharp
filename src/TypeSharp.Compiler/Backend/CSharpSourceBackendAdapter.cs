using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public sealed class CSharpSourceBackendAdapter : ITypeSharpBackend
{
    public static CSharpSourceBackendAdapter Instance { get; } = new();

    private CSharpSourceBackendAdapter()
    {
    }

    public string Name => "csharp";

    public string GeneratedSourceExtension => ".g.cs";

    public string Emit(SyntaxNode root) => CSharpSourceBackend.Emit(root);
}
