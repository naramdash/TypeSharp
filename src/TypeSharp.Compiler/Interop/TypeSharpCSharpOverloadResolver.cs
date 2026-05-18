using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpCSharpOverloadResolver
{
    public static CSharpOverloadResolution Resolve(
        IEnumerable<CSharpOverloadCandidate> candidates,
        IReadOnlyList<SyntaxNode> arguments)
    {
        var applicableCandidates = candidates
            .Where(candidate => IsApplicableArity(candidate.Method, arguments))
            .ToArray();

        var bestCandidates = SelectBestCandidates(applicableCandidates, arguments);
        return new CSharpOverloadResolution(applicableCandidates, bestCandidates);
    }

    public static MetadataParameterSymbol? GetParameterForArgument(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        int argumentIndex)
    {
        var parameterIndex = GetParameterIndexForArgument(method, arguments, argumentIndex);
        return parameterIndex is null ? null : method.Parameters[parameterIndex.Value];
    }

    public static MetadataByRefKind GetArgumentByRefKind(SyntaxNode argument)
    {
        if (argument.Kind == SyntaxKind.NamedArgument)
        {
            var expression = argument.Children.LastOrDefault(child => !child.IsToken);
            return expression is null ? MetadataByRefKind.None : GetArgumentByRefKind(expression);
        }

        return argument.Kind switch
        {
            SyntaxKind.RefArgument => MetadataByRefKind.Ref,
            SyntaxKind.OutArgument => MetadataByRefKind.Out,
            SyntaxKind.InArgument => MetadataByRefKind.In,
            _ => MetadataByRefKind.None
        };
    }

    private static CSharpOverloadCandidate[] SelectBestCandidates(
        CSharpOverloadCandidate[] candidates,
        IReadOnlyList<SyntaxNode> arguments)
    {
        if (candidates.Length <= 1)
        {
            return candidates;
        }

        var exactMatches = candidates
            .Where(candidate => IsExactMatch(candidate.Method, arguments))
            .ToArray();

        return exactMatches.Length > 0 ? exactMatches : candidates;
    }

    private static bool IsExactMatch(MetadataMethodSymbol method, IReadOnlyList<SyntaxNode> arguments)
    {
        if (!IsApplicableArity(method, arguments))
        {
            return false;
        }

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            var parameterIndex = GetParameterIndexForArgument(method, arguments, index);
            if (parameterIndex is null)
            {
                return false;
            }

            var parameter = method.Parameters[parameterIndex.Value];
            var positionalOrdinal = GetPositionalOrdinal(arguments, index);
            var expectedType = GetExpectedArgumentType(method, parameter, parameterIndex.Value, positionalOrdinal);

            if (GetArgumentByRefKind(argument) != parameter.ByRefKind ||
                !TryInferArgumentType(argument, out var argumentType) ||
                !string.Equals(argumentType, expectedType, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsApplicableArity(MetadataMethodSymbol method, IReadOnlyList<SyntaxNode> arguments)
    {
        var providedParameters = new bool[method.Parameters.Count];
        var positionalCount = 0;

        for (var index = 0; index < arguments.Count; index++)
        {
            var parameterIndex = GetParameterIndexForArgument(method, arguments, index);
            if (parameterIndex is null)
            {
                return false;
            }

            if (providedParameters[parameterIndex.Value] && !IsExpandedParamsArgument(method, parameterIndex.Value, GetPositionalOrdinal(arguments, index)))
            {
                return false;
            }

            providedParameters[parameterIndex.Value] = true;
            if (!TryGetNamedArgumentName(arguments[index], out _))
            {
                positionalCount++;
            }
        }

        if (!HasParamsParameter(method) && positionalCount > method.Parameters.Count)
        {
            return false;
        }

        for (var index = 0; index < method.Parameters.Count; index++)
        {
            var parameter = method.Parameters[index];
            if (!providedParameters[index] && !parameter.IsOptional && !parameter.IsParams)
            {
                return false;
            }
        }

        return true;
    }

    private static int? GetParameterIndexForArgument(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        int argumentIndex)
    {
        var argument = arguments[argumentIndex];
        if (TryGetNamedArgumentName(argument, out var name))
        {
            for (var index = 0; index < method.Parameters.Count; index++)
            {
                if (string.Equals(method.Parameters[index].Name, name, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return null;
        }

        var positionalOrdinal = GetPositionalOrdinal(arguments, argumentIndex);
        if (positionalOrdinal < method.Parameters.Count)
        {
            return positionalOrdinal;
        }

        return HasParamsParameter(method) ? method.Parameters.Count - 1 : null;
    }

    private static int GetPositionalOrdinal(IReadOnlyList<SyntaxNode> arguments, int argumentIndex)
    {
        var ordinal = 0;
        for (var index = 0; index < argumentIndex; index++)
        {
            if (!TryGetNamedArgumentName(arguments[index], out _))
            {
                ordinal++;
            }
        }

        return ordinal;
    }

    private static string GetExpectedArgumentType(
        MetadataMethodSymbol method,
        MetadataParameterSymbol parameter,
        int parameterIndex,
        int positionalOrdinal)
    {
        if (!IsExpandedParamsArgument(method, parameterIndex, positionalOrdinal))
        {
            return parameter.Type;
        }

        return parameter.Type.EndsWith("[]", StringComparison.Ordinal)
            ? parameter.Type.Substring(0, parameter.Type.Length - 2)
            : parameter.Type;
    }

    private static bool IsExpandedParamsArgument(MetadataMethodSymbol method, int parameterIndex, int positionalOrdinal)
    {
        if (!HasParamsParameter(method))
        {
            return false;
        }

        var fixedParameterCount = method.Parameters.Count - 1;
        return parameterIndex == fixedParameterCount && positionalOrdinal >= fixedParameterCount;
    }

    private static bool HasParamsParameter(MetadataMethodSymbol method) =>
        method.Parameters.Count > 0 && method.Parameters[method.Parameters.Count - 1].IsParams;

    private static bool TryInferArgumentType(SyntaxNode argument, out string type)
    {
        if (argument.Kind == SyntaxKind.NamedArgument)
        {
            var expression = argument.Children.LastOrDefault(child => !child.IsToken);
            if (expression is not null)
            {
                return TryInferArgumentType(expression, out type);
            }

            type = string.Empty;
            return false;
        }

        if (argument.Kind is SyntaxKind.RefArgument or SyntaxKind.OutArgument or SyntaxKind.InArgument)
        {
            var expression = argument.Children.FirstOrDefault(child => !child.IsToken);
            if (expression is not null)
            {
                return TryInferArgumentType(expression, out type);
            }

            type = string.Empty;
            return false;
        }

        if (argument.Kind == SyntaxKind.LiteralExpression)
        {
            var token = argument.Children.FirstOrDefault(child => child.IsToken);
            type = token?.Kind switch
            {
                SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => "string",
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => "bool",
                SyntaxKind.NumericLiteralToken => InferNumericType(token.Text ?? string.Empty),
                _ => string.Empty
            };

            return type.Length > 0;
        }

        type = string.Empty;
        return false;
    }

    private static string InferNumericType(string text)
    {
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            return "decimal";
        }

        return text.Contains('.', StringComparison.Ordinal) ? "double" : "int";
    }

    private static bool TryGetNamedArgumentName(SyntaxNode argument, out string name)
    {
        name = string.Empty;
        if (argument.Kind != SyntaxKind.NamedArgument)
        {
            return false;
        }

        name = argument.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
        return name.Length > 0;
    }
}
