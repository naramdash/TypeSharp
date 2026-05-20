using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Lowering;

public sealed class TypeSharpLoweringPipeline
{
    private readonly ITypeSharpLoweringPass[] _passes;

    public static TypeSharpLoweringPipeline Default { get; } = new(
    [
        CSharpRuntimeImportLoweringPass.Instance
    ]);

    public TypeSharpLoweringPipeline(IEnumerable<ITypeSharpLoweringPass> passes)
    {
        _passes = passes?.ToArray() ?? throw new ArgumentNullException(nameof(passes));
        if (_passes.Length == 0)
        {
            throw new ArgumentException("A lowering pipeline must contain at least one pass.", nameof(passes));
        }
    }

    public IReadOnlyList<ITypeSharpLoweringPass> Passes => _passes;

    public SyntaxNode Lower(SyntaxNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var current = root;
        foreach (var pass in _passes)
        {
            current = pass.Lower(current);
        }

        return current;
    }
}
