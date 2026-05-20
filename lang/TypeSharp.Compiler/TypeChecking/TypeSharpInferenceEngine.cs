using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.TypeChecking;

internal sealed class TypeSharpInferenceEngine
{
    public bool TryInferExpression(
        SyntaxNode node,
        ITypeSharpInferenceScope scope,
        Func<SyntaxNode, SimpleType> inferNested,
        out SimpleType type)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(inferNested);

        type = node.Kind switch
        {
            SyntaxKind.LiteralExpression => InferLiteral(node),
            SyntaxKind.NameofExpression => SimpleType.Named("string"),
            SyntaxKind.CheckedExpression => InferCheckedExpression(node, inferNested),
            SyntaxKind.ParenthesizedExpression => InferParenthesizedExpression(node, inferNested),
            SyntaxKind.IdentifierExpression => InferIdentifier(node, scope),
            SyntaxKind.CallExpression => InferCall(node, scope, inferNested),
            SyntaxKind.BinaryExpression => InferBinary(node, scope, inferNested),
            _ => SimpleType.Unknown
        };

        return type.IsKnown || node.Kind is
            SyntaxKind.LiteralExpression or
            SyntaxKind.NameofExpression or
            SyntaxKind.CheckedExpression or
            SyntaxKind.ParenthesizedExpression or
            SyntaxKind.IdentifierExpression or
            SyntaxKind.CallExpression or
            SyntaxKind.BinaryExpression;
    }

    public SimpleType InferLiteral(SyntaxNode node)
    {
        var token = node.Children.FirstOrDefault(child => child.IsToken);
        return token?.Kind switch
        {
            SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => SimpleType.Named("string"),
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => SimpleType.Named("bool"),
            SyntaxKind.NullKeyword => SimpleType.Null,
            SyntaxKind.NumericLiteralToken => InferNumericLiteral(token.Text ?? string.Empty),
            _ => SimpleType.Unknown
        };
    }

    private static SimpleType InferIdentifier(SyntaxNode node, ITypeSharpInferenceScope scope)
    {
        if (!TryGetFirstIdentifier(node, out var identifier))
        {
            return SimpleType.Unknown;
        }

        return scope.ResolveValue(identifier.Text ?? string.Empty, out var type)
            ? type
            : SimpleType.Unknown;
    }

    private static SimpleType InferCheckedExpression(
        SyntaxNode node,
        Func<SyntaxNode, SimpleType> inferNested)
    {
        var expression = node.Children.FirstOrDefault(child => !child.IsToken);
        return expression is null ? SimpleType.Unknown : inferNested(expression);
    }

    private static SimpleType InferParenthesizedExpression(
        SyntaxNode node,
        Func<SyntaxNode, SimpleType> inferNested)
    {
        var expression = node.Children.FirstOrDefault(child => !child.IsToken);
        return expression is null ? SimpleType.Unknown : inferNested(expression);
    }

    private static SimpleType InferCall(
        SyntaxNode node,
        ITypeSharpInferenceScope scope,
        Func<SyntaxNode, SimpleType> inferNested)
    {
        if (node.Children.Count == 0)
        {
            return SimpleType.Unknown;
        }

        var callee = node.Children[0];
        if (callee.Kind != SyntaxKind.IdentifierExpression)
        {
            inferNested(callee);
        }

        foreach (var argument in node.Children.Skip(1).Where(child => !child.IsToken))
        {
            inferNested(argument);
        }

        if (callee.Kind != SyntaxKind.IdentifierExpression || !TryGetFirstIdentifier(callee, out var identifier))
        {
            return SimpleType.Unknown;
        }

        var name = identifier.Text ?? string.Empty;
        if (scope.ResolveFunction(name, out var returnType))
        {
            return returnType;
        }

        return scope.ResolveType(name) ? SimpleType.Named(name) : SimpleType.Unknown;
    }

    private static SimpleType InferBinary(
        SyntaxNode node,
        ITypeSharpInferenceScope scope,
        Func<SyntaxNode, SimpleType> inferNested)
    {
        var children = node.Children;
        var expressions = children.Where(child => !child.IsToken).ToArray();
        if (IsCompositionExpression(children))
        {
            foreach (var child in expressions)
            {
                inferNested(child);
            }

            return SimpleType.Unknown;
        }

        if (children.Any(child => child.IsToken && child.Kind == SyntaxKind.PipeGreaterToken) &&
            expressions.Length >= 2)
        {
            inferNested(expressions[0]);
            return InferPipelineTarget(expressions[1], scope, inferNested);
        }

        foreach (var child in expressions)
        {
            inferNested(child);
        }

        if (children.Any(child => child.IsToken && child.Kind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken))
        {
            return SimpleType.Named("bool");
        }

        if (children.Any(child => child.IsToken && child.Kind == SyntaxKind.NullCoalescingToken))
        {
            var right = children.Where(child => !child.IsToken).Skip(1).FirstOrDefault();
            return right is null ? SimpleType.Unknown : inferNested(right);
        }

        return SimpleType.Unknown;
    }

    private static SimpleType InferPipelineTarget(
        SyntaxNode target,
        ITypeSharpInferenceScope scope,
        Func<SyntaxNode, SimpleType> inferNested)
    {
        if (target.Kind == SyntaxKind.CallExpression)
        {
            return InferCall(target, scope, inferNested);
        }

        if (target.Kind == SyntaxKind.IdentifierExpression &&
            TryGetFirstIdentifier(target, out var identifier) &&
            scope.ResolveFunction(identifier.Text ?? string.Empty, out var returnType))
        {
            return returnType;
        }

        inferNested(target);
        return SimpleType.Unknown;
    }

    private static bool IsCompositionExpression(IReadOnlyList<SyntaxNode> children)
    {
        for (var index = 0; index < children.Count - 1; index++)
        {
            if (children[index].IsToken &&
                children[index + 1].IsToken &&
                ((children[index].Kind == SyntaxKind.GreaterToken && children[index + 1].Kind == SyntaxKind.GreaterToken) ||
                 (children[index].Kind == SyntaxKind.LessToken && children[index + 1].Kind == SyntaxKind.LessToken)))
            {
                return true;
            }
        }

        return false;
    }

    private static SimpleType InferNumericLiteral(string text)
    {
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            return SimpleType.Named("decimal");
        }

        return text.Contains('.', StringComparison.Ordinal) ? SimpleType.Named("double") : SimpleType.Named("int");
    }

    private static bool TryGetFirstIdentifier(SyntaxNode node, out SyntaxNode identifier)
    {
        identifier = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)!;
        return identifier is not null;
    }
}
