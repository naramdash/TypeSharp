using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Lowering;

public sealed class CSharpRuntimeImportLoweringPass : ITypeSharpLoweringPass
{
    private const string RuntimeNamespace = "TypeSharp.Runtime";

    public static CSharpRuntimeImportLoweringPass Instance { get; } = new();

    private CSharpRuntimeImportLoweringPass()
    {
    }

    public string Name => "csharp-runtime-import";

    public SyntaxNode Lower(SyntaxNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (!NeedsRuntimeHelpers(root) || HasRuntimeImport(root))
        {
            return root;
        }

        var children = root.Children.ToList();
        var insertAt = 0;
        while (insertAt < children.Count && IsHeaderNode(children[insertAt]))
        {
            insertAt++;
        }

        children.Insert(insertAt, CreateRuntimeImport(root));
        return WithChildren(root, children);
    }

    private static bool IsHeaderNode(SyntaxNode node) =>
        node.Kind is
            SyntaxKind.NamespaceDeclaration or
            SyntaxKind.OpenDeclaration or
            SyntaxKind.ImportNamedDeclaration or
            SyntaxKind.ImportTypeDeclaration or
            SyntaxKind.ImportStaticDeclaration;

    private static SyntaxNode CreateRuntimeImport(SyntaxNode root) =>
        new(
            SyntaxKind.ImportNamedDeclaration,
            root.Span,
            children:
            [
                new SyntaxNode(
                    SyntaxKind.StringLiteralToken,
                    root.Span,
                    $"\"{RuntimeNamespace}\"",
                    isToken: true)
            ]);

    private static SyntaxNode WithChildren(SyntaxNode node, IReadOnlyList<SyntaxNode> children) =>
        new(node.Kind, node.Span, node.Text, node.IsToken, node.IsMissing, children);

    private static bool HasRuntimeImport(SyntaxNode root)
    {
        foreach (var child in root.Children)
        {
            if (child.Kind is not (SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration))
            {
                continue;
            }

            var moduleSpecifier = child.Children.FirstOrDefault(grandchild =>
                grandchild.IsToken && grandchild.Kind == SyntaxKind.StringLiteralToken);

            if (string.Equals(moduleSpecifier?.Text, $"\"{RuntimeNamespace}\"", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool NeedsRuntimeHelpers(SyntaxNode node)
    {
        if (node.Kind is SyntaxKind.UnionDeclaration or SyntaxKind.MatchExpression)
        {
            return true;
        }

        return node.Children.Any(child => !child.IsToken && NeedsRuntimeHelpers(child));
    }
}
