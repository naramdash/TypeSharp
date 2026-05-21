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
        if (TryInferBinaryShiftExpression(children, expressions, inferNested, out var shiftType))
        {
            return shiftType;
        }

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

        if (IsUnaryLogicalNotExpression(children) && expressions.Length == 1)
        {
            var operandType = inferNested(expressions[0]);
            return IsBoolType(operandType) ? SimpleType.Named("bool") : SimpleType.Unknown;
        }

        if (TryInferUnaryNumericExpression(children, expressions, inferNested, out var unaryNumericType))
        {
            return unaryNumericType;
        }

        if (TryInferUnaryIntegralBitwiseExpression(children, expressions, inferNested, out var unaryBitwiseType))
        {
            return unaryBitwiseType;
        }

        if (TryInferBinaryBitwiseExpression(children, expressions, inferNested, out var binaryBitwiseType))
        {
            return binaryBitwiseType;
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

    private static bool IsUnaryLogicalNotExpression(IReadOnlyList<SyntaxNode> children) =>
        children.Count >= 2 &&
        children[0].IsToken &&
        children[0].Kind == SyntaxKind.BangToken &&
        children.Skip(1).Count(child => !child.IsToken) == 1;

    private static bool IsBoolType(SimpleType type) =>
        type.IsKnown &&
        !type.IsNull &&
        string.Equals(type.Name, "bool", StringComparison.Ordinal);

    private static bool TryInferUnaryNumericExpression(
        IReadOnlyList<SyntaxNode> children,
        IReadOnlyList<SyntaxNode> expressions,
        Func<SyntaxNode, SimpleType> inferNested,
        out SimpleType type)
    {
        type = SimpleType.Unknown;
        var operatorKind = children.FirstOrDefault(child => child.IsToken)?.Kind ?? SyntaxKind.UnknownToken;
        if (expressions.Count != 1 ||
            operatorKind is not (SyntaxKind.PlusToken or SyntaxKind.MinusToken))
        {
            return false;
        }

        var operandType = inferNested(expressions[0]);
        type = TryGetUnaryNumericResultType(operandType, operatorKind, out var resultType)
            ? resultType
            : SimpleType.Unknown;
        return true;
    }

    private static bool TryGetUnaryNumericResultType(SimpleType operandType, SyntaxKind operatorKind, out SimpleType resultType)
    {
        resultType = SimpleType.Unknown;
        if (!operandType.IsKnown || operandType.IsNull)
        {
            return false;
        }

        return operandType.Name switch
        {
            "byte" or "sbyte" or "short" or "ushort" => SetResult(SimpleType.Named("int"), out resultType),
            "int" or "long" or "float" or "double" or "decimal" => SetResult(SimpleType.Named(operandType.Name), out resultType),
            "uint" or "ulong" when operatorKind == SyntaxKind.PlusToken => SetResult(SimpleType.Named(operandType.Name), out resultType),
            _ => false
        };
    }

    private static bool TryInferUnaryIntegralBitwiseExpression(
        IReadOnlyList<SyntaxNode> children,
        IReadOnlyList<SyntaxNode> expressions,
        Func<SyntaxNode, SimpleType> inferNested,
        out SimpleType type)
    {
        type = SimpleType.Unknown;
        if (expressions.Count != 1 ||
            children.FirstOrDefault(child => child.IsToken)?.Kind != SyntaxKind.TildeToken)
        {
            return false;
        }

        var operandType = inferNested(expressions[0]);
        type = TryGetUnaryIntegralBitwiseResultType(operandType, out var resultType)
            ? resultType
            : SimpleType.Unknown;
        return true;
    }

    private static bool TryInferBinaryBitwiseExpression(
        IReadOnlyList<SyntaxNode> children,
        IReadOnlyList<SyntaxNode> expressions,
        Func<SyntaxNode, SimpleType> inferNested,
        out SimpleType type)
    {
        type = SimpleType.Unknown;
        var operatorKind = children.FirstOrDefault(child => child.IsToken)?.Kind ?? SyntaxKind.UnknownToken;
        if (expressions.Count != 2 ||
            operatorKind is not (SyntaxKind.PipeToken or SyntaxKind.AmpersandToken or SyntaxKind.CaretToken))
        {
            return false;
        }

        var leftType = inferNested(expressions[0]);
        var rightType = inferNested(expressions[1]);
        type = TryGetBinaryBooleanBitwiseResultType(leftType, rightType, out var boolResultType)
            ? boolResultType
            : TryGetBinaryIntegralBitwiseResultType(leftType, rightType, out var resultType)
            ? resultType
            : SimpleType.Unknown;
        return true;
    }

    private static bool TryInferBinaryShiftExpression(
        IReadOnlyList<SyntaxNode> children,
        IReadOnlyList<SyntaxNode> expressions,
        Func<SyntaxNode, SimpleType> inferNested,
        out SimpleType type)
    {
        type = SimpleType.Unknown;
        if (expressions.Count != 2 || !TryGetShiftOperatorText(children, out _))
        {
            return false;
        }

        var leftType = inferNested(expressions[0]);
        var rightType = inferNested(expressions[1]);
        type = TryGetBinaryIntegralShiftResultType(leftType, rightType, out var resultType)
            ? resultType
            : SimpleType.Unknown;
        return true;
    }

    private static bool TryGetUnaryIntegralBitwiseResultType(SimpleType operandType, out SimpleType resultType)
    {
        resultType = SimpleType.Unknown;
        if (!IsKnownNonNullableIntegralType(operandType))
        {
            return false;
        }

        return operandType.Name switch
        {
            "byte" or "sbyte" or "short" or "ushort" => SetResult(SimpleType.Named("int"), out resultType),
            "int" or "uint" or "long" or "ulong" => SetResult(SimpleType.Named(operandType.Name), out resultType),
            _ => false
        };
    }

    private static bool TryGetBinaryIntegralBitwiseResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
    {
        resultType = SimpleType.Unknown;
        if (!IsKnownNonNullableIntegralType(leftType) ||
            !IsKnownNonNullableIntegralType(rightType) ||
            !TryPromoteIntegralBinaryType(leftType.Name, rightType.Name, out var promotedType))
        {
            return false;
        }

        resultType = SimpleType.Named(promotedType);
        return true;
    }

    private static bool TryGetBinaryIntegralShiftResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
    {
        resultType = SimpleType.Unknown;
        if (!IsKnownNonNullableIntegralType(leftType) ||
            !IsKnownNonNullableShiftCountType(rightType))
        {
            return false;
        }

        return leftType.Name switch
        {
            "byte" or "sbyte" or "short" or "ushort" => SetResult(SimpleType.Named("int"), out resultType),
            "int" or "uint" or "long" or "ulong" => SetResult(SimpleType.Named(leftType.Name), out resultType),
            _ => false
        };
    }

    private static bool TryGetBinaryBooleanBitwiseResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
    {
        resultType = SimpleType.Unknown;
        if (!IsKnownNonNullableBoolType(leftType) || !IsKnownNonNullableBoolType(rightType))
        {
            return false;
        }

        resultType = SimpleType.Named("bool");
        return true;
    }

    private static bool IsKnownNonNullableBoolType(SimpleType type) =>
        type.IsKnown &&
        !type.IsNull &&
        !type.IsNullable &&
        string.Equals(type.Name, "bool", StringComparison.Ordinal);

    private static bool IsKnownNonNullableIntegralType(SimpleType type) =>
        type.IsKnown &&
        !type.IsNull &&
        !type.IsNullable &&
        IsIntegralPrimitiveType(type.Name);

    private static bool IsKnownNonNullableShiftCountType(SimpleType type) =>
        type.IsKnown &&
        !type.IsNull &&
        !type.IsNullable &&
        type.Name is "byte" or "sbyte" or "short" or "ushort" or "int";

    private static bool IsIntegralPrimitiveType(string typeName) =>
        typeName is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong";

    private static bool TryPromoteIntegralBinaryType(string left, string right, out string resultType)
    {
        resultType = string.Empty;
        if (!IsIntegralPrimitiveType(left) || !IsIntegralPrimitiveType(right))
        {
            return false;
        }

        if (left is "ulong" || right is "ulong")
        {
            if (CanImplicitlyPromoteToUnsignedLong(left) && CanImplicitlyPromoteToUnsignedLong(right))
            {
                resultType = "ulong";
                return true;
            }

            return false;
        }

        if (left is "long" || right is "long")
        {
            resultType = "long";
            return true;
        }

        if (left is "uint" || right is "uint")
        {
            resultType = left is "sbyte" or "short" or "int" || right is "sbyte" or "short" or "int"
                ? "long"
                : "uint";
            return true;
        }

        resultType = "int";
        return true;
    }

    private static bool CanImplicitlyPromoteToUnsignedLong(string type) =>
        type is "byte" or "ushort" or "uint" or "ulong";

    private static bool SetResult(SimpleType value, out SimpleType result)
    {
        result = value;
        return true;
    }

    private static bool IsCompositionExpression(IReadOnlyList<SyntaxNode> children)
    {
        if (TryGetLogicalUnsignedShiftOperatorText(children, out _))
        {
            return false;
        }

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

    private static bool TryGetShiftOperatorText(IReadOnlyList<SyntaxNode> children, out string operatorText)
    {
        if (TryGetLogicalUnsignedShiftOperatorText(children, out operatorText))
        {
            return true;
        }

        for (var index = 0; index < children.Count - 1; index++)
        {
            if (!children[index].IsToken || !children[index + 1].IsToken)
            {
                continue;
            }

            if (children[index].Kind == SyntaxKind.GreaterToken && children[index + 1].Kind == SyntaxKind.GreaterToken)
            {
                operatorText = ">>";
                return true;
            }

            if (children[index].Kind == SyntaxKind.LessToken && children[index + 1].Kind == SyntaxKind.LessToken)
            {
                operatorText = "<<";
                return true;
            }
        }

        operatorText = string.Empty;
        return false;
    }

    private static bool TryGetLogicalUnsignedShiftOperatorText(IReadOnlyList<SyntaxNode> children, out string operatorText)
    {
        for (var index = 0; index + 2 < children.Count; index++)
        {
            if (children[index].IsToken &&
                children[index + 1].IsToken &&
                children[index + 2].IsToken &&
                children[index].Kind == SyntaxKind.GreaterToken &&
                children[index + 1].Kind == SyntaxKind.GreaterToken &&
                children[index + 2].Kind == SyntaxKind.GreaterToken)
            {
                operatorText = ">>>";
                return true;
            }
        }

        operatorText = string.Empty;
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
