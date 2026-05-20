using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpCSharpOverloadResolver
{
    private const int ExactMatchScore = 0;
    private const int NumericConversionScore = 10;
    private const int DelegateLambdaScore = 20;
    private const int MetadataRelationScore = 100;
    private const int ObjectFallbackScore = 1000;
    private const int GenericFallbackScore = 2000;

    public static CSharpOverloadResolution Resolve(
        IEnumerable<CSharpOverloadCandidate> candidates,
        IReadOnlyList<SyntaxNode> arguments,
        int? explicitGenericTypeArgumentCount = null,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies = null,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances = null,
        IReadOnlyCollection<string>? extensionNamespaces = null)
    {
        var applicableCandidates = candidates
            .Where(candidate => explicitGenericTypeArgumentCount is null ||
                candidate.Method.GenericParameterCount == explicitGenericTypeArgumentCount.Value)
            .Where(candidate => IsApplicableArity(candidate.Method, arguments))
            .Where(candidate => IsApplicableKnownArgumentTypes(candidate.Method, arguments, assemblies, localInstances, extensionNamespaces))
            .ToArray();

        var bestCandidates = SelectBestCandidates(applicableCandidates, arguments, assemblies, localInstances, extensionNamespaces);
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
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces)
    {
        if (candidates.Length <= 1)
        {
            return candidates;
        }

        var scoredCandidates = candidates
            .Select(candidate => TryScoreCandidate(candidate, arguments, assemblies, localInstances, extensionNamespaces, out var score)
                ? new CandidateScore(candidate, score)
                : (CandidateScore?)null)
            .Where(score => score is not null)
            .Select(score => score!.Value)
            .ToArray();
        if (scoredCandidates.Length == candidates.Length)
        {
            var bestScore = scoredCandidates.Min(candidate => candidate.Score);
            var bestCandidates = scoredCandidates
                .Where(candidate => candidate.Score == bestScore)
                .Select(candidate => candidate.Candidate)
                .ToArray();
            return SelectMoreSpecificNullLiteralTargets(bestCandidates, arguments, assemblies);
        }

        var exactMatches = candidates
            .Where(candidate => IsExactMatch(candidate.Method, arguments, assemblies, localInstances, extensionNamespaces))
            .ToArray();

        return exactMatches.Length > 0 ? exactMatches : candidates;
    }

    private static CSharpOverloadCandidate[] SelectMoreSpecificNullLiteralTargets(
        CSharpOverloadCandidate[] candidates,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies)
    {
        if (candidates.Length <= 1 ||
            assemblies is null ||
            !arguments.Any(IsNullLiteralArgument))
        {
            return candidates;
        }

        var lessSpecific = new bool[candidates.Length];
        for (var candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
        {
            for (var otherIndex = 0; otherIndex < candidates.Length; otherIndex++)
            {
                if (candidateIndex == otherIndex)
                {
                    continue;
                }

                if (IsMoreSpecificForNullLiteralArguments(
                        candidates[otherIndex],
                        candidates[candidateIndex],
                        arguments,
                        assemblies))
                {
                    lessSpecific[candidateIndex] = true;
                    break;
                }
            }
        }

        var selected = candidates
            .Where((_, index) => !lessSpecific[index])
            .ToArray();
        return selected.Length > 0 ? selected : candidates;
    }

    private static bool IsMoreSpecificForNullLiteralArguments(
        CSharpOverloadCandidate better,
        CSharpOverloadCandidate worse,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        var foundMoreSpecificTarget = false;
        for (var index = 0; index < arguments.Count; index++)
        {
            if (!IsNullLiteralArgument(arguments[index]))
            {
                continue;
            }

            var betterParameterIndex = GetParameterIndexForArgument(better.Method, arguments, index);
            var worseParameterIndex = GetParameterIndexForArgument(worse.Method, arguments, index);
            if (betterParameterIndex is null || worseParameterIndex is null)
            {
                return false;
            }

            var positionalOrdinal = GetPositionalOrdinal(arguments, index);
            var betterType = GetExpectedArgumentType(better.Method, better.Method.Parameters[betterParameterIndex.Value], betterParameterIndex.Value, positionalOrdinal);
            var worseType = GetExpectedArgumentType(worse.Method, worse.Method.Parameters[worseParameterIndex.Value], worseParameterIndex.Value, positionalOrdinal);
            if (TypeNamesEqual(betterType, worseType))
            {
                continue;
            }

            var betterIsMoreSpecific = IsMoreSpecificReferenceTarget(betterType, worseType, assemblies);
            var worseIsMoreSpecific = IsMoreSpecificReferenceTarget(worseType, betterType, assemblies);
            if (betterIsMoreSpecific && !worseIsMoreSpecific)
            {
                foundMoreSpecificTarget = true;
                continue;
            }

            return false;
        }

        return foundMoreSpecificTarget;
    }

    private static bool IsNullLiteralArgument(SyntaxNode argument)
    {
        if (argument.Kind == SyntaxKind.NamedArgument)
        {
            var expression = argument.Children.LastOrDefault(child => !child.IsToken);
            return expression is not null && IsNullLiteralArgument(expression);
        }

        var token = argument.Kind == SyntaxKind.LiteralExpression
            ? argument.Children.FirstOrDefault(child => child.IsToken)
            : null;
        return token?.Kind == SyntaxKind.NullKeyword;
    }

    private static bool IsMoreSpecificReferenceTarget(
        string candidateType,
        string otherType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (TypeNamesEqual(candidateType, otherType))
        {
            return false;
        }

        if (IsObjectType(otherType) && !IsObjectType(candidateType))
        {
            return CanPassNullLiteral(candidateType, assemblies);
        }

        return TryGetBestMetadataRelationshipDistance(candidateType, otherType, assemblies, out var distance) &&
            distance > 0;
    }

    private static bool IsExactMatch(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces)
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

            if (GetArgumentByRefKind(argument) != parameter.ByRefKind)
            {
                return false;
            }

            if (IsLambdaArgument(argument))
            {
                if (!CanPassLambdaArgument(argument, expectedType, assemblies, localInstances, extensionNamespaces))
                {
                    return false;
                }

                continue;
            }

            if (!TryInferArgumentType(argument, out var argumentType, assemblies, localInstances) ||
                !TypeNamesEqual(argumentType.Name, expectedType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsApplicableKnownArgumentTypes(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces)
    {
        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            var parameterIndex = GetParameterIndexForArgument(method, arguments, index);
            if (parameterIndex is null)
            {
                return false;
            }

            var parameter = method.Parameters[parameterIndex.Value];
            if (GetArgumentByRefKind(argument) != parameter.ByRefKind)
            {
                continue;
            }

            var positionalOrdinal = GetPositionalOrdinal(arguments, index);
            var expectedType = GetExpectedArgumentType(method, parameter, parameterIndex.Value, positionalOrdinal);
            if (IsLambdaArgument(argument))
            {
                if (!CanPassLambdaArgument(argument, expectedType, assemblies, localInstances, extensionNamespaces))
                {
                    return false;
                }

                continue;
            }

            if (!TryInferArgumentType(argument, out var argumentType, assemblies, localInstances))
            {
                continue;
            }

            if (!CanPassKnownArgumentType(argumentType, expectedType, assemblies))
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

    private static bool TryScoreCandidate(
        CSharpOverloadCandidate candidate,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces,
        out int score)
    {
        score = 0;
        if (!IsApplicableArity(candidate.Method, arguments))
        {
            return false;
        }

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            var parameterIndex = GetParameterIndexForArgument(candidate.Method, arguments, index);
            if (parameterIndex is null)
            {
                score = 0;
                return false;
            }

            var parameter = candidate.Method.Parameters[parameterIndex.Value];
            if (GetArgumentByRefKind(argument) != parameter.ByRefKind)
            {
                score = 0;
                return false;
            }

            var positionalOrdinal = GetPositionalOrdinal(arguments, index);
            var expectedType = GetExpectedArgumentType(candidate.Method, parameter, parameterIndex.Value, positionalOrdinal);
            if (IsLambdaArgument(argument))
            {
                if (!TryScoreLambdaArgument(argument, expectedType, assemblies, localInstances, extensionNamespaces, out var lambdaScore))
                {
                    score = 0;
                    return false;
                }

                score += lambdaScore;
                continue;
            }

            if (!TryInferArgumentType(argument, out var argumentType, assemblies, localInstances))
            {
                score = 0;
                return false;
            }

            if (!TryScoreKnownArgumentType(argumentType, expectedType, assemblies, out var argumentScore))
            {
                score = 0;
                return false;
            }

            score += argumentScore;
        }

        return true;
    }

    private static bool TryInferArgumentType(
        SyntaxNode argument,
        out InferredArgumentType type,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies = null,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances = null)
    {
        if (argument.Kind == SyntaxKind.NamedArgument)
        {
            var expression = argument.Children.LastOrDefault(child => !child.IsToken);
            if (expression is not null)
            {
                return TryInferArgumentType(expression, out type, assemblies, localInstances);
            }

            type = default;
            return false;
        }

        if (argument.Kind is SyntaxKind.RefArgument or SyntaxKind.OutArgument or SyntaxKind.InArgument)
        {
            var expression = argument.Children.FirstOrDefault(child => !child.IsToken);
            if (expression is not null)
            {
                return TryInferArgumentType(expression, out type, assemblies, localInstances);
            }

            type = default;
            return false;
        }

        if (argument.Kind == SyntaxKind.LiteralExpression)
        {
            var token = argument.Children.FirstOrDefault(child => child.IsToken);
            if (token is null)
            {
                type = default;
                return false;
            }

            type = token.Kind switch
            {
                SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => new InferredArgumentType("string"),
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => new InferredArgumentType("bool"),
                SyntaxKind.NumericLiteralToken => new InferredArgumentType(InferNumericType(token.Text ?? string.Empty), token.Text ?? string.Empty),
                SyntaxKind.NullKeyword => new InferredArgumentType("null", IsNullLiteral: true),
                _ => default
            };

            return !string.IsNullOrEmpty(type.Name);
        }

        if (argument.Kind == SyntaxKind.CallExpression &&
            assemblies is not null &&
            TryGetConstructedCallTypeName(argument, out var typeName))
        {
            var constructedTypes = FindMetadataTypes(assemblies, typeName);
            if (constructedTypes.Count == 1)
            {
                type = new InferredArgumentType(constructedTypes[0].FullName);
                return true;
            }
        }

        if (argument.Kind == SyntaxKind.IdentifierExpression &&
            localInstances is not null &&
            TryGetIdentifier(argument, out var name) &&
            localInstances.TryGetValue(name, out var metadataTypes) &&
            metadataTypes.Count == 1)
        {
            type = new InferredArgumentType(metadataTypes[0].FullName);
            return true;
        }

        if (argument.Kind == SyntaxKind.MemberAccessExpression &&
            assemblies is not null &&
            localInstances is not null &&
            TryInferLocalInstanceMemberAccessArgumentType(argument, assemblies, localInstances, out type))
        {
            return true;
        }

        type = default;
        return false;
    }

    private static bool TryInferLocalInstanceMemberAccessArgumentType(
        SyntaxNode argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out InferredArgumentType type)
    {
        type = default;
        if (!TryGetLambdaParameterMemberAccessPath(argument, out var receiverName, out var memberNames) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes))
        {
            return false;
        }

        var inferredTypes = new List<string>();
        foreach (var receiverType in receiverTypes)
        {
            var currentType = receiverType.FullName;
            foreach (var memberName in memberNames)
            {
                if (!TryGetInstanceMemberType(currentType, memberName, assemblies, out currentType))
                {
                    currentType = string.Empty;
                    break;
                }
            }

            if (currentType.Length > 0)
            {
                inferredTypes.Add(currentType);
            }
        }

        var distinctTypes = inferredTypes.Distinct(StringComparer.Ordinal).ToArray();
        if (distinctTypes.Length != 1)
        {
            return false;
        }

        type = new InferredArgumentType(distinctTypes[0]);
        return true;
    }

    private static string InferNumericType(string text)
    {
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            return "decimal";
        }

        return text.Contains('.', StringComparison.Ordinal) ? "double" : "int";
    }

    private static bool CanPassKnownArgumentType(
        InferredArgumentType argumentType,
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies)
    {
        if (argumentType.IsNullLiteral)
        {
            return CanPassNullLiteral(expectedType, assemblies);
        }

        if (TypeNamesEqual(argumentType.Name, expectedType) ||
            IsObjectType(expectedType) ||
            IsGenericParameterType(expectedType))
        {
            return true;
        }

        if (CanPassKnownNumericArgumentType(argumentType, expectedType))
        {
            return true;
        }

        return assemblies is not null &&
            FindMetadataTypes(assemblies, argumentType.Name).Any(type => TypeSymbolSatisfiesConstraint(
                type,
                expectedType,
                assemblies,
                new HashSet<string>(StringComparer.Ordinal)));
    }

    private static bool TryScoreKnownArgumentType(
        InferredArgumentType argumentType,
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        out int score)
    {
        if (argumentType.IsNullLiteral)
        {
            return TryScoreNullLiteral(expectedType, assemblies, out score);
        }

        if (TypeNamesEqual(argumentType.Name, expectedType))
        {
            score = ExactMatchScore;
            return true;
        }

        if (CanPassKnownNumericArgumentType(argumentType, expectedType))
        {
            score = NumericConversionScore;
            return true;
        }

        if (IsObjectType(expectedType))
        {
            score = ObjectFallbackScore;
            return true;
        }

        if (assemblies is not null &&
            TryGetBestMetadataRelationshipDistance(argumentType.Name, expectedType, assemblies, out var distance))
        {
            score = MetadataRelationScore + distance;
            return true;
        }

        if (IsGenericParameterType(expectedType))
        {
            score = GenericFallbackScore;
            return true;
        }

        score = 0;
        return false;
    }

    private static bool CanPassNullLiteral(string expectedType, IReadOnlyList<MetadataAssemblySymbol>? assemblies) =>
        IsNullableValueTypeName(expectedType) ||
        IsReferenceTypeName(expectedType, assemblies) ||
        IsGenericParameterType(expectedType);

    private static bool TryScoreNullLiteral(
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        out int score)
    {
        if (IsObjectType(expectedType))
        {
            score = ObjectFallbackScore;
            return true;
        }

        if (IsNullableValueTypeName(expectedType) ||
            IsReferenceTypeName(expectedType, assemblies))
        {
            score = MetadataRelationScore;
            return true;
        }

        if (IsGenericParameterType(expectedType))
        {
            score = GenericFallbackScore;
            return true;
        }

        score = 0;
        return false;
    }

    private static bool CanPassKnownNumericArgumentType(InferredArgumentType argumentType, string expectedType)
    {
        var actual = NormalizePrimitiveTypeName(argumentType.Name);
        var expected = NormalizePrimitiveTypeName(expectedType);
        if (!IsNumericType(actual) || !IsNumericType(expected))
        {
            return false;
        }

        if (actual != "int")
        {
            return false;
        }

        if (expected is "long" or "float" or "double" or "decimal")
        {
            return true;
        }

        return TryParseIntegralLiteral(argumentType.NumericLiteralText, out var value) &&
            expected switch
            {
                "byte" => value <= byte.MaxValue,
                "sbyte" => value <= (ulong)sbyte.MaxValue,
                "short" => value <= (ulong)short.MaxValue,
                "ushort" => value <= ushort.MaxValue,
                "uint" => value <= uint.MaxValue,
                "ulong" => true,
                _ => false
            };
    }

    private static bool TryParseIntegralLiteral(string? text, out ulong value) =>
        ulong.TryParse(text, out value);

    private static bool IsLambdaArgument(SyntaxNode argument) =>
        UnwrapArgumentExpression(argument).Kind == SyntaxKind.LambdaExpression;

    private static bool CanPassLambdaArgument(
        SyntaxNode argument,
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces)
    {
        if (!TryGetKnownDelegateSignature(expectedType, out var delegateSignature) ||
            !TryGetLambdaParameterCount(argument, out var lambdaParameterCount) ||
            lambdaParameterCount != delegateSignature.ParameterTypes.Count)
        {
            return false;
        }

        if (TypeNamesEqual(delegateSignature.ReturnType, "void") ||
            !TryGetLambdaBody(argument, out var body))
        {
            return true;
        }

        if (!TryInferLambdaBodyType(argument, body, delegateSignature, assemblies, localInstances, extensionNamespaces, out var bodyType))
        {
            return true;
        }

        return CanPassKnownArgumentType(bodyType, delegateSignature.ReturnType, assemblies);
    }

    private static bool TryScoreLambdaArgument(
        SyntaxNode argument,
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces,
        out int score)
    {
        score = 0;
        if (!TryGetKnownDelegateSignature(expectedType, out var delegateSignature) ||
            !TryGetLambdaParameterCount(argument, out var lambdaParameterCount) ||
            lambdaParameterCount != delegateSignature.ParameterTypes.Count)
        {
            return false;
        }

        if (TypeNamesEqual(delegateSignature.ReturnType, "void") ||
            !TryGetLambdaBody(argument, out var body))
        {
            score = DelegateLambdaScore;
            return true;
        }

        if (!TryInferLambdaBodyType(argument, body, delegateSignature, assemblies, localInstances, extensionNamespaces, out var bodyType))
        {
            score = DelegateLambdaScore;
            return true;
        }

        if (!TryScoreKnownArgumentType(bodyType, delegateSignature.ReturnType, assemblies, out var bodyReturnScore))
        {
            return false;
        }

        score = DelegateLambdaScore + bodyReturnScore;
        return true;
    }

    private static bool TryInferLambdaBodyType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces,
        out InferredArgumentType type)
    {
        if (TryInferLambdaParameterReferenceType(argument, body, delegateSignature, out type))
        {
            return true;
        }

        if (TryInferLambdaParameterMemberAccessType(argument, body, delegateSignature, assemblies, out type))
        {
            return true;
        }

        if (TryInferLambdaParameterMethodCallType(argument, body, delegateSignature, assemblies, extensionNamespaces, out type))
        {
            return true;
        }

        if (TryInferLambdaIndexerExpressionType(argument, body, delegateSignature, assemblies, localInstances, extensionNamespaces, out type))
        {
            return true;
        }

        if (TryInferLambdaBinaryExpressionType(body, out type))
        {
            return true;
        }

        if (TryInferLambdaNullCoalescingExpressionType(argument, body, delegateSignature, assemblies, localInstances, extensionNamespaces, out type))
        {
            return true;
        }

        if (TryInferLambdaStaticMethodCallType(argument, body, delegateSignature, assemblies, extensionNamespaces, out type))
        {
            return true;
        }

        return TryInferArgumentType(body, out type, assemblies, localInstances);
    }

    private static bool TryGetLambdaParameterCount(SyntaxNode argument, out int parameterCount)
    {
        if (!TryGetLambdaParameterNames(argument, out var parameterNames))
        {
            parameterCount = 0;
            return false;
        }

        parameterCount = parameterNames.Count;
        return true;
    }

    private static bool TryInferLambdaParameterReferenceType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        out InferredArgumentType type)
    {
        type = default;
        if (body.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(body, out var bodyName) ||
            !TryGetLambdaParameterNames(argument, out var parameterNames))
        {
            return false;
        }

        for (var index = 0; index < parameterNames.Count && index < delegateSignature.ParameterTypes.Count; index++)
        {
            if (string.Equals(parameterNames[index], bodyName, StringComparison.Ordinal))
            {
                type = new InferredArgumentType(delegateSignature.ParameterTypes[index]);
                return true;
            }
        }

        return false;
    }

    private static bool TryInferLambdaParameterMemberAccessType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        out InferredArgumentType type)
    {
        type = default;
        if (assemblies is null ||
            body.Kind != SyntaxKind.MemberAccessExpression ||
            !TryGetLambdaParameterNames(argument, out var parameterNames))
        {
            return false;
        }

        if (!TryGetLambdaParameterMemberAccessPath(body, out var receiverName, out var memberNames))
        {
            return false;
        }

        for (var index = 0; index < parameterNames.Count && index < delegateSignature.ParameterTypes.Count; index++)
        {
            if (!string.Equals(parameterNames[index], receiverName, StringComparison.Ordinal))
            {
                continue;
            }

            var currentType = delegateSignature.ParameterTypes[index];
            foreach (var memberName in memberNames)
            {
                if (!TryGetInstanceMemberType(currentType, memberName, assemblies, out currentType))
                {
                    currentType = string.Empty;
                    break;
                }
            }

            if (currentType.Length > 0)
            {
                type = new InferredArgumentType(currentType);
                return true;
            }
        }

        return false;
    }

    private static bool TryInferLambdaParameterMethodCallType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyCollection<string>? extensionNamespaces,
        out InferredArgumentType type)
    {
        type = default;
        if (assemblies is null ||
            body.Kind != SyntaxKind.CallExpression ||
            !TryGetLambdaParameterNames(argument, out var parameterNames))
        {
            return false;
        }

        var callee = body.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind != SyntaxKind.MemberAccessExpression ||
            !TryGetLambdaParameterMemberAccessPath(callee, out var receiverName, out var memberNames) ||
            memberNames.Count == 0)
        {
            return false;
        }

        var receiverMemberNames = memberNames.Take(memberNames.Count - 1).ToArray();
        var methodName = memberNames[^1];
        var callArguments = body.Children.Skip(1).Where(child => !child.IsToken).ToArray();
        for (var index = 0; index < parameterNames.Count && index < delegateSignature.ParameterTypes.Count; index++)
        {
            if (!string.Equals(parameterNames[index], receiverName, StringComparison.Ordinal))
            {
                continue;
            }

            var currentType = delegateSignature.ParameterTypes[index];
            foreach (var memberName in receiverMemberNames)
            {
                if (!TryGetInstanceMemberType(currentType, memberName, assemblies, out currentType))
                {
                    currentType = string.Empty;
                    break;
                }
            }

            if (currentType.Length == 0)
            {
                continue;
            }

            if (TryGetInstanceMethodReturnType(currentType, methodName, callArguments, assemblies, extensionNamespaces, out var returnType))
            {
                type = new InferredArgumentType(returnType);
                return true;
            }

            if (TryGetExtensionMethodReturnType(currentType, methodName, callArguments, assemblies, extensionNamespaces, out returnType))
            {
                type = new InferredArgumentType(returnType);
                return true;
            }
        }

        return false;
    }

    private static bool TryInferLambdaIndexerExpressionType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces,
        out InferredArgumentType type)
    {
        type = default;
        if (body.Kind != SyntaxKind.IndexerExpression)
        {
            return false;
        }

        var expressions = body.Children.Where(child => !child.IsToken).ToArray();
        if (expressions.Length < 2 ||
            !TryInferLambdaBodyType(argument, expressions[0], delegateSignature, assemblies, localInstances, extensionNamespaces, out var receiverType))
        {
            return false;
        }

        var indexArguments = expressions.Skip(1).ToArray();
        var lambdaInstances = assemblies is null
            ? localInstances
            : CreateLambdaParameterLocalInstances(argument, delegateSignature, assemblies, localInstances);
        if (TryGetArrayIndexerReturnType(receiverType.Name, indexArguments, assemblies, lambdaInstances, out var arrayElementType))
        {
            type = new InferredArgumentType(arrayElementType);
            return true;
        }

        if (assemblies is not null &&
            TryGetInstanceIndexerReturnType(receiverType.Name, indexArguments, assemblies, lambdaInstances, out var returnType))
        {
            type = new InferredArgumentType(returnType);
            return true;
        }

        return false;
    }

    private static bool TryInferLambdaBinaryExpressionType(
        SyntaxNode body,
        out InferredArgumentType type)
    {
        type = default;
        if (body.Kind != SyntaxKind.BinaryExpression ||
            !body.Children.Any(child => child.IsToken &&
                child.Kind is SyntaxKind.EqualsEqualsToken or
                    SyntaxKind.BangEqualsToken or
                    SyntaxKind.LessToken or
                    SyntaxKind.LessOrEqualsToken or
                    SyntaxKind.GreaterToken or
                    SyntaxKind.GreaterOrEqualsToken or
                    SyntaxKind.AmpersandAmpersandToken or
                    SyntaxKind.PipePipeToken))
        {
            return false;
        }

        type = new InferredArgumentType("bool");
        return true;
    }

    private static bool TryInferLambdaNullCoalescingExpressionType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        IReadOnlyCollection<string>? extensionNamespaces,
        out InferredArgumentType type)
    {
        type = default;
        if (body.Kind != SyntaxKind.BinaryExpression ||
            !body.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.NullCoalescingToken))
        {
            return false;
        }

        var expressions = body.Children.Where(child => !child.IsToken).ToArray();
        if (expressions.Length < 2)
        {
            return false;
        }

        if (TryInferLambdaBodyType(argument, expressions[1], delegateSignature, assemblies, localInstances, extensionNamespaces, out var rightType) &&
            !rightType.IsNullLiteral)
        {
            type = rightType;
            return true;
        }

        if (TryInferLambdaBodyType(argument, expressions[0], delegateSignature, assemblies, localInstances, extensionNamespaces, out var leftType))
        {
            type = leftType;
            return true;
        }

        if (rightType.Name.Length > 0)
        {
            type = rightType;
            return true;
        }

        return false;
    }

    private static bool TryInferLambdaStaticMethodCallType(
        SyntaxNode argument,
        SyntaxNode body,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyCollection<string>? extensionNamespaces,
        out InferredArgumentType type)
    {
        type = default;
        if (assemblies is null ||
            body.Kind != SyntaxKind.CallExpression ||
            !TryGetLambdaParameterNames(argument, out var parameterNames) ||
            !TryGetStaticMemberCall(body, out var typeName, out var methodName, out var explicitGenericTypeArgumentCount))
        {
            return false;
        }

        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count == 0)
        {
            return false;
        }

        var candidates = metadataTypes
            .SelectMany(typeSymbol => typeSymbol.Methods
                .Where(method => method.IsStatic &&
                    !method.IsExtension &&
                    string.Equals(method.Name, methodName, StringComparison.Ordinal))
                .Select(method => new CSharpOverloadCandidate(typeSymbol, method)))
            .ToArray();
        if (candidates.Length == 0)
        {
            return false;
        }

        var callArguments = body.Children.Skip(1).Where(child => !child.IsToken).ToArray();
        var lambdaInstances = CreateLambdaParameterLocalInstances(parameterNames, delegateSignature, assemblies);
        var resolution = Resolve(
            candidates,
            callArguments,
            explicitGenericTypeArgumentCount,
            assemblies,
            lambdaInstances,
            extensionNamespaces);

        type = new InferredArgumentType(resolution.SelectedCandidate?.Method.ReturnType ?? string.Empty);
        return type.Name.Length > 0;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> CreateLambdaParameterLocalInstances(
        SyntaxNode argument,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances)
    {
        var merged = localInstances is null
            ? new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(StringComparer.Ordinal)
            : new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(localInstances, StringComparer.Ordinal);

        if (!TryGetLambdaParameterNames(argument, out var parameterNames))
        {
            return merged;
        }

        foreach (var pair in CreateLambdaParameterLocalInstances(parameterNames, delegateSignature, assemblies))
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> CreateLambdaParameterLocalInstances(
        IReadOnlyList<string> parameterNames,
        KnownDelegateSignature delegateSignature,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        var instances = new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(StringComparer.Ordinal);
        for (var index = 0; index < parameterNames.Count && index < delegateSignature.ParameterTypes.Count; index++)
        {
            var metadataTypes = FindMetadataTypes(assemblies, delegateSignature.ParameterTypes[index]);
            if (metadataTypes.Count > 0)
            {
                instances[parameterNames[index]] = metadataTypes;
            }
        }

        return instances;
    }

    private static bool TryGetArrayIndexerReturnType(
        string receiverType,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol>? assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        out string elementType)
    {
        elementType = string.Empty;
        if (!receiverType.EndsWith("[]", StringComparison.Ordinal) ||
            arguments.Count != 1 ||
            !TryInferArgumentType(arguments[0], out var indexType, assemblies, localInstances) ||
            !CanPassKnownArgumentType(indexType, "int", assemblies))
        {
            return false;
        }

        elementType = receiverType[..^2];
        return elementType.Length > 0;
    }

    private static bool TryGetInstanceIndexerReturnType(
        string receiverType,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        out string returnType)
    {
        returnType = string.Empty;
        var candidates = FindMetadataTypes(assemblies, receiverType)
            .SelectMany(type => type.Properties
                .Where(property =>
                    !property.IsStatic &&
                    property.HasPublicGetter &&
                    property.IsIndexer &&
                    property.ParameterTypes.Count == arguments.Count))
            .ToArray();
        if (candidates.Length == 0 ||
            !TryInferIndexerArgumentTypes(arguments, assemblies, localInstances, out var argumentTypes))
        {
            return false;
        }

        var scoredCandidates = candidates
            .Select(property => TryScoreIndexerArguments(property, argumentTypes, assemblies, out var score)
                ? new IndexerCandidateScore(property, score)
                : (IndexerCandidateScore?)null)
            .Where(candidate => candidate is not null)
            .Select(candidate => candidate!.Value)
            .ToArray();
        if (scoredCandidates.Length == 0)
        {
            return false;
        }

        var bestScore = scoredCandidates.Min(candidate => candidate.Score);
        var bestCandidates = scoredCandidates
            .Where(candidate => candidate.Score == bestScore)
            .Select(candidate => candidate.Property)
            .ToArray();
        if (bestCandidates.Length != 1)
        {
            return false;
        }

        returnType = bestCandidates[0].Type;
        return returnType.Length > 0;
    }

    private static bool TryInferIndexerArgumentTypes(
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>>? localInstances,
        out IReadOnlyList<InferredArgumentType> argumentTypes)
    {
        var inferred = new List<InferredArgumentType>();
        foreach (var argument in arguments)
        {
            if (!TryInferArgumentType(argument, out var type, assemblies, localInstances))
            {
                argumentTypes = [];
                return false;
            }

            inferred.Add(type);
        }

        argumentTypes = inferred;
        return true;
    }

    private static bool TryScoreIndexerArguments(
        MetadataPropertySymbol property,
        IReadOnlyList<InferredArgumentType> argumentTypes,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int score)
    {
        score = 0;
        if (property.ParameterTypes.Count != argumentTypes.Count)
        {
            return false;
        }

        for (var index = 0; index < argumentTypes.Count; index++)
        {
            if (!TryScoreKnownArgumentType(argumentTypes[index], property.ParameterTypes[index], assemblies, out var argumentScore))
            {
                return false;
            }

            score += argumentScore;
        }

        return true;
    }

    private static bool TryGetLambdaParameterMemberAccessPath(
        SyntaxNode body,
        out string receiverName,
        out IReadOnlyList<string> memberNames)
    {
        receiverName = string.Empty;
        memberNames = [];
        if (body.Kind != SyntaxKind.MemberAccessExpression)
        {
            return false;
        }

        var names = new List<string>();
        var current = body;
        while (current.Kind == SyntaxKind.MemberAccessExpression)
        {
            var receiver = current.Children.FirstOrDefault(child => !child.IsToken);
            var member = current.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            if (receiver is null ||
                member?.Text is not { Length: > 0 } memberName)
            {
                return false;
            }

            names.Add(memberName);
            current = receiver;
        }

        if (current.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(current, out receiverName))
        {
            return false;
        }

        names.Reverse();
        memberNames = names;
        return memberNames.Count > 0;
    }

    private static bool TryGetInstanceMemberType(
        string receiverType,
        string memberName,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out string memberType)
    {
        var memberTypes = FindMetadataTypes(assemblies, receiverType)
            .SelectMany(type =>
                type.Properties
                    .Where(property => !property.IsStatic &&
                        property.HasPublicGetter &&
                        !property.IsIndexer &&
                        string.Equals(property.Name, memberName, StringComparison.Ordinal))
                    .Select(property => property.Type)
                    .Concat(type.Fields
                        .Where(field => !field.IsStatic &&
                            string.Equals(field.Name, memberName, StringComparison.Ordinal))
                        .Select(field => field.Type)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        memberType = memberTypes.Length == 1 ? memberTypes[0] : string.Empty;
        return memberType.Length > 0;
    }

    private static bool TryGetInstanceMethodReturnType(
        string receiverType,
        string methodName,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string>? extensionNamespaces,
        out string returnType)
    {
        var candidates = FindMetadataTypes(assemblies, receiverType)
            .SelectMany(type => type.Methods
                .Where(method => !method.IsStatic &&
                    string.Equals(method.Name, methodName, StringComparison.Ordinal))
                .Select(method => new CSharpOverloadCandidate(type, method)))
            .ToArray();
        if (candidates.Length == 0)
        {
            returnType = string.Empty;
            return false;
        }

        var resolution = Resolve(candidates, arguments, assemblies: assemblies, extensionNamespaces: extensionNamespaces);
        returnType = resolution.SelectedCandidate?.Method.ReturnType ?? string.Empty;
        return returnType.Length > 0;
    }

    private static bool TryGetExtensionMethodReturnType(
        string receiverType,
        string methodName,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string>? extensionNamespaces,
        out string returnType)
    {
        returnType = string.Empty;
        if (extensionNamespaces is null || extensionNamespaces.Count == 0)
        {
            return false;
        }

        var receiverArgumentType = new InferredArgumentType(receiverType);
        var scoredCandidates = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => extensionNamespaces.Contains(type.Namespace))
            .SelectMany(type => type.Methods
                .Where(method => method.IsStatic &&
                    method.IsExtension &&
                    string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                    method.Parameters.Count > 0)
                .Select(method => new CSharpOverloadCandidate(type, method)))
            .Select(candidate => TryScoreExtensionMethodCandidate(candidate, receiverArgumentType, arguments, assemblies, extensionNamespaces, out var score)
                ? new CandidateScore(candidate, score)
                : (CandidateScore?)null)
            .Where(score => score is not null)
            .Select(score => score!.Value)
            .ToArray();

        if (scoredCandidates.Length == 0)
        {
            return false;
        }

        var bestScore = scoredCandidates.Min(candidate => candidate.Score);
        var bestCandidates = scoredCandidates
            .Where(candidate => candidate.Score == bestScore)
            .Select(candidate => candidate.Candidate)
            .ToArray();
        if (bestCandidates.Length != 1)
        {
            return false;
        }

        returnType = bestCandidates[0].Method.ReturnType;
        return returnType.Length > 0;
    }

    private static bool TryScoreExtensionMethodCandidate(
        CSharpOverloadCandidate candidate,
        InferredArgumentType receiverArgumentType,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string>? extensionNamespaces,
        out int score)
    {
        score = 0;
        var receiverParameter = candidate.Method.Parameters[0];
        if (!TryScoreKnownArgumentType(receiverArgumentType, receiverParameter.Type, assemblies, out var receiverScore))
        {
            return false;
        }

        var methodWithoutReceiver = candidate.Method with
        {
            Parameters = candidate.Method.Parameters.Skip(1).ToArray()
        };
        if (!IsApplicableArity(methodWithoutReceiver, arguments) ||
            !IsApplicableKnownArgumentTypes(methodWithoutReceiver, arguments, assemblies, localInstances: null, extensionNamespaces: extensionNamespaces))
        {
            return false;
        }

        var reducedCandidate = new CSharpOverloadCandidate(candidate.Type, methodWithoutReceiver);
        score = receiverScore;
        if (TryScoreCandidate(reducedCandidate, arguments, assemblies, localInstances: null, extensionNamespaces: extensionNamespaces, out var argumentScore))
        {
            score += argumentScore;
        }
        else
        {
            score += GenericFallbackScore;
        }

        return true;
    }

    private static bool TryGetLambdaParameterNames(SyntaxNode argument, out IReadOnlyList<string> names)
    {
        var expression = UnwrapArgumentExpression(argument);
        if (expression.Kind != SyntaxKind.LambdaExpression)
        {
            names = [];
            return false;
        }

        names = expression.Children
            .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            .Select(child => child.Text ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToArray();
        return true;
    }

    private static bool TryGetLambdaBody(SyntaxNode argument, out SyntaxNode body)
    {
        var expression = UnwrapArgumentExpression(argument);
        if (expression.Kind == SyntaxKind.LambdaExpression)
        {
            var candidate = expression.Children.LastOrDefault(child => !child.IsToken);
            if (candidate is not null)
            {
                body = candidate;
                return true;
            }
        }

        body = argument;
        return false;
    }

    private static SyntaxNode UnwrapArgumentExpression(SyntaxNode argument)
    {
        if (argument.Kind == SyntaxKind.NamedArgument)
        {
            return argument.Children.LastOrDefault(child => !child.IsToken) ?? argument;
        }

        if (argument.Kind is SyntaxKind.RefArgument or SyntaxKind.OutArgument or SyntaxKind.InArgument)
        {
            return argument.Children.FirstOrDefault(child => !child.IsToken) ?? argument;
        }

        return argument;
    }

    private static bool TryGetKnownDelegateSignature(string typeName, out KnownDelegateSignature signature)
    {
        signature = default;
        if (!TryParseConstructedGenericTypeName(typeName, out var genericTypeName, out var typeArguments))
        {
            if (TypeNamesEqual(typeName, "System.Action"))
            {
                signature = new KnownDelegateSignature([], "void");
                return true;
            }

            return false;
        }

        var genericName = GetUnqualifiedTypeName(StripGenericArity(genericTypeName));
        if (string.Equals(genericName, "Func", StringComparison.Ordinal) && typeArguments.Count > 0)
        {
            signature = new KnownDelegateSignature(typeArguments.Take(typeArguments.Count - 1).ToArray(), typeArguments[^1]);
            return true;
        }

        if (string.Equals(genericName, "Action", StringComparison.Ordinal))
        {
            signature = new KnownDelegateSignature(typeArguments, "void");
            return true;
        }

        return false;
    }

    private static bool TryParseConstructedGenericTypeName(
        string typeName,
        out string genericTypeName,
        out IReadOnlyList<string> typeArguments)
    {
        genericTypeName = string.Empty;
        typeArguments = [];

        var openIndex = typeName.IndexOf('<');
        if (openIndex <= 0 || !typeName.EndsWith(">", StringComparison.Ordinal))
        {
            return false;
        }

        genericTypeName = typeName[..openIndex];
        var argumentText = typeName[(openIndex + 1)..^1];
        if (string.IsNullOrWhiteSpace(argumentText))
        {
            return false;
        }

        typeArguments = SplitTopLevelTypeArguments(argumentText);
        return typeArguments.Count > 0;
    }

    private static IReadOnlyList<string> SplitTopLevelTypeArguments(string text)
    {
        var arguments = new List<string>();
        var start = 0;
        var depth = 0;
        for (var index = 0; index < text.Length; index++)
        {
            var ch = text[index];
            if (ch == '<')
            {
                depth++;
            }
            else if (ch == '>')
            {
                depth--;
            }
            else if (ch == ',' && depth == 0)
            {
                arguments.Add(text[start..index].Trim());
                start = index + 1;
            }
        }

        arguments.Add(text[start..].Trim());
        return arguments.Where(argument => argument.Length > 0).ToArray();
    }

    private static bool TypeNamesEqual(string left, string right) =>
        string.Equals(NormalizePrimitiveTypeName(left), NormalizePrimitiveTypeName(right), StringComparison.Ordinal) ||
        string.Equals(GetUnqualifiedTypeName(NormalizePrimitiveTypeName(left)), NormalizePrimitiveTypeName(right), StringComparison.Ordinal) ||
        string.Equals(NormalizePrimitiveTypeName(left), GetUnqualifiedTypeName(NormalizePrimitiveTypeName(right)), StringComparison.Ordinal) ||
        string.Equals(StripGenericArity(NormalizePrimitiveTypeName(left)), StripGenericArity(NormalizePrimitiveTypeName(right)), StringComparison.Ordinal) ||
        string.Equals(GetUnqualifiedTypeName(StripGenericArity(NormalizePrimitiveTypeName(left))), GetUnqualifiedTypeName(StripGenericArity(NormalizePrimitiveTypeName(right))), StringComparison.Ordinal);

    private static bool IsObjectType(string type) =>
        string.Equals(NormalizePrimitiveTypeName(type), "object", StringComparison.Ordinal);

    private static bool IsGenericParameterType(string type)
    {
        if (type.Length < 2)
        {
            return false;
        }

        var index = type[0] == '!'
            ? (type.Length > 1 && type[1] == '!' ? 2 : 1)
            : 0;
        if (index == 0 || index >= type.Length)
        {
            return false;
        }

        for (; index < type.Length; index++)
        {
            if (!char.IsDigit(type[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNumericType(string type) =>
        NormalizePrimitiveTypeName(type) is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal";

    private static bool IsReferenceTypeName(string type, IReadOnlyList<MetadataAssemblySymbol>? assemblies)
    {
        var normalized = NormalizePrimitiveTypeName(type);
        if (IsObjectType(normalized) ||
            string.Equals(normalized, "string", StringComparison.Ordinal) ||
            normalized.EndsWith("[]", StringComparison.Ordinal))
        {
            return true;
        }

        if (IsKnownNonNullableValueTypeName(normalized))
        {
            return false;
        }

        if (assemblies is not null)
        {
            var metadataTypes = FindMetadataTypes(assemblies, type);
            if (metadataTypes.Count > 0)
            {
                return metadataTypes.Any(metadataType => !metadataType.IsValueType);
            }
        }

        return normalized.Contains('.', StringComparison.Ordinal) ||
            normalized.Contains('<', StringComparison.Ordinal);
    }

    private static bool IsNullableValueTypeName(string type)
    {
        var normalized = StripGenericArity(NormalizePrimitiveTypeName(type));
        var unqualified = GetUnqualifiedTypeName(normalized);
        return string.Equals(unqualified, "Nullable", StringComparison.Ordinal) ||
            normalized.StartsWith("System.Nullable<", StringComparison.Ordinal) ||
            normalized.StartsWith("Nullable<", StringComparison.Ordinal);
    }

    private static bool IsKnownNonNullableValueTypeName(string type)
    {
        var normalized = NormalizePrimitiveTypeName(type);
        return IsNumericType(normalized) ||
            normalized is "bool" or "char" or "void";
    }

    private static string NormalizePrimitiveTypeName(string type) =>
        type switch
        {
            "System.Boolean" or "Boolean" => "bool",
            "System.Byte" or "Byte" => "byte",
            "System.SByte" or "SByte" => "sbyte",
            "System.Int16" or "Int16" => "short",
            "System.UInt16" or "UInt16" => "ushort",
            "System.Int32" or "Int32" => "int",
            "System.UInt32" or "UInt32" => "uint",
            "System.Int64" or "Int64" => "long",
            "System.UInt64" or "UInt64" => "ulong",
            "System.Single" or "Single" => "float",
            "System.Double" or "Double" => "double",
            "System.Decimal" or "Decimal" => "decimal",
            "System.Char" or "Char" => "char",
            "System.String" or "String" => "string",
            "System.Object" or "Object" => "object",
            _ => type
        };

    private static IReadOnlyList<MetadataTypeSymbol> FindMetadataTypes(
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string typeName) =>
        assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type =>
                string.Equals(type.FullName, typeName, StringComparison.Ordinal) ||
                TypeNamesEqual(type.FullName, typeName) ||
                TypeNamesEqual(type.Name, typeName))
            .ToArray();

    private static bool TryGetBestMetadataRelationshipDistance(
        string argumentTypeName,
        string expectedType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int distance)
    {
        distance = int.MaxValue;
        var found = false;
        foreach (var type in FindMetadataTypes(assemblies, argumentTypeName))
        {
            if (TryGetMetadataRelationshipDistance(
                    type,
                    expectedType,
                    assemblies,
                    new HashSet<string>(StringComparer.Ordinal),
                    out var candidateDistance))
            {
                distance = Math.Min(distance, candidateDistance);
                found = true;
            }
        }

        return found;
    }

    private static bool TypeSymbolSatisfiesConstraint(
        MetadataTypeSymbol type,
        string constraint,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        HashSet<string> visited) =>
        TryGetMetadataRelationshipDistance(type, constraint, assemblies, visited, out _);

    private static bool TryGetMetadataRelationshipDistance(
        MetadataTypeSymbol type,
        string constraint,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        HashSet<string> visited,
        out int distance)
    {
        if (TypeNamesEqual(type.FullName, constraint) ||
            TypeNamesEqual(type.Name, constraint))
        {
            distance = 0;
            return true;
        }

        if (!visited.Add(type.FullName))
        {
            distance = 0;
            return false;
        }

        distance = int.MaxValue;
        if (!string.IsNullOrWhiteSpace(type.BaseTypeName))
        {
            if (TypeNamesEqual(type.BaseTypeName, constraint))
            {
                distance = Math.Min(distance, 1);
            }

            foreach (var baseType in FindMetadataTypes(assemblies, type.BaseTypeName))
            {
                if (TryGetMetadataRelationshipDistance(baseType, constraint, assemblies, visited, out var baseDistance))
                {
                    distance = Math.Min(distance, baseDistance + 1);
                }
            }
        }

        foreach (var interfaceName in type.InterfaceNames)
        {
            if (TypeNamesEqual(interfaceName, constraint))
            {
                distance = Math.Min(distance, 1);
            }

            foreach (var interfaceType in FindMetadataTypes(assemblies, interfaceName))
            {
                if (TryGetMetadataRelationshipDistance(interfaceType, constraint, assemblies, visited, out var interfaceDistance))
                {
                    distance = Math.Min(distance, interfaceDistance + 1);
                }
            }
        }

        if (distance != int.MaxValue)
        {
            return true;
        }

        distance = 0;
        return false;
    }

    private static string GetUnqualifiedTypeName(string name)
    {
        var index = name.LastIndexOf('.');
        return index < 0 ? name : name[(index + 1)..];
    }

    private static string StripGenericArity(string name)
    {
        var index = name.IndexOf('`', StringComparison.Ordinal);
        return index < 0 ? name : name[..index];
    }

    private static bool TryGetIdentifier(SyntaxNode node, out string name)
    {
        name = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
        return name.Length > 0;
    }

    private static bool TryGetConstructedCallTypeName(SyntaxNode callExpression, out string typeName)
    {
        typeName = string.Empty;
        if (callExpression.Kind != SyntaxKind.CallExpression)
        {
            return false;
        }

        var callee = callExpression.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind == SyntaxKind.IdentifierExpression)
        {
            return TryGetIdentifier(callee, out typeName);
        }

        if (callee?.Kind != SyntaxKind.GenericNameExpression)
        {
            return false;
        }

        var target = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
        return target?.Kind == SyntaxKind.IdentifierExpression &&
            TryGetIdentifier(target, out typeName);
    }

    private static bool TryGetStaticMemberCall(
        SyntaxNode call,
        out string typeName,
        out string methodName,
        out int? explicitGenericTypeArgumentCount)
    {
        typeName = string.Empty;
        methodName = string.Empty;
        explicitGenericTypeArgumentCount = null;
        if (call.Kind != SyntaxKind.CallExpression)
        {
            return false;
        }

        var callee = call.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind == SyntaxKind.GenericNameExpression)
        {
            explicitGenericTypeArgumentCount = CountGenericTypeArguments(callee);
            callee = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
        }

        if (callee?.Kind != SyntaxKind.MemberAccessExpression)
        {
            return false;
        }

        var receiver = callee.Children.FirstOrDefault(child => !child.IsToken);
        if (receiver?.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(receiver, out typeName))
        {
            return false;
        }

        methodName = callee.Children
            .LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            ?.Text ?? string.Empty;
        return methodName.Length > 0;
    }

    private static int? CountGenericTypeArguments(SyntaxNode genericName)
    {
        var argumentList = genericName.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
        var count = argumentList?.Children.Count(child => !child.IsToken) ?? 0;
        return count == 0 ? null : count;
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

    private readonly record struct InferredArgumentType(string Name, string? NumericLiteralText = null, bool IsNullLiteral = false);

    private readonly record struct KnownDelegateSignature(IReadOnlyList<string> ParameterTypes, string ReturnType);

    private readonly record struct CandidateScore(CSharpOverloadCandidate Candidate, int Score);

    private readonly record struct IndexerCandidateScore(MetadataPropertySymbol Property, int Score);
}
