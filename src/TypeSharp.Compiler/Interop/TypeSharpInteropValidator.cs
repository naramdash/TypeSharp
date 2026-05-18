using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpInteropValidator
{
    public static IReadOnlyList<Diagnostic> Validate(
        SyntaxNode root,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file)
    {
        var diagnostics = new List<Diagnostic>();
        ValidateNode(root, assemblies, file, diagnostics);
        return diagnostics;
    }

    private static void ValidateNode(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (node.Kind == SyntaxKind.CallExpression)
        {
            ValidateCall(node, assemblies, file, diagnostics);
        }

        foreach (var child in node.Children.Where(child => !child.IsToken))
        {
            ValidateNode(child, assemblies, file, diagnostics);
        }
    }

    private static void ValidateCall(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetStaticMemberCall(node, out var typeName, out var methodName))
        {
            return;
        }

        var arguments = node.Children.Skip(1).Where(child => !child.IsToken).ToArray();
        var candidates = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal) || string.Equals(type.FullName, typeName, StringComparison.Ordinal))
            .SelectMany(type => type.Methods.Select(method => (Type: type, Method: method)))
            .Where(candidate => string.Equals(candidate.Method.Name, methodName, StringComparison.Ordinal) &&
                IsApplicableArity(candidate.Method, arguments))
            .ToArray();

        var selectedCandidates = SelectBestCandidates(candidates, arguments);
        if (selectedCandidates.Length > 1)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.AmbiguousCSharpOverload.Code,
                DiagnosticDescriptors.AmbiguousCSharpOverload.DefaultSeverity,
                $"Call to C# method '{typeName}.{methodName}' matches {selectedCandidates.Length} overload candidates. Add an explicit type annotation or make the call unambiguous.",
                file,
                node.Span));
            return;
        }

        if (selectedCandidates.Length != 1)
        {
            return;
        }

        var (metadataType, metadataMethod) = selectedCandidates[0];
        for (var index = 0; index < arguments.Length; index++)
        {
            var argument = arguments[index];
            var parameter = GetParameterForArgument(metadataMethod, arguments, index);
            if (parameter is null)
            {
                continue;
            }

            var actual = GetArgumentByRefKind(argument);
            if (actual == parameter.ByRefKind)
            {
                continue;
            }

            var expectedText = FormatExpected(parameter.ByRefKind);
            var actualText = FormatActual(actual);
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.InvalidByRefInterop.Code,
                DiagnosticDescriptors.InvalidByRefInterop.DefaultSeverity,
                $"Call to C# method '{metadataType.FullName}.{metadataMethod.Name}' expects parameter '{parameter.Name}' to be passed {expectedText}, but the argument uses {actualText}.",
                file,
                argument.Span));
        }
    }

    private static (MetadataTypeSymbol Type, MetadataMethodSymbol Method)[] SelectBestCandidates(
        (MetadataTypeSymbol Type, MetadataMethodSymbol Method)[] candidates,
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
            if (parameter is null)
            {
                return false;
            }

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

    private static bool HasParamsParameter(MetadataMethodSymbol method) =>
        method.Parameters.Count > 0 && method.Parameters[method.Parameters.Count - 1].IsParams;

    private static MetadataParameterSymbol? GetParameterForArgument(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        int argumentIndex)
    {
        var parameterIndex = GetParameterIndexForArgument(method, arguments, argumentIndex);
        return parameterIndex is null ? null : method.Parameters[parameterIndex.Value];
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

    private static bool TryGetStaticMemberCall(SyntaxNode call, out string typeName, out string methodName)
    {
        typeName = string.Empty;
        methodName = string.Empty;

        var callee = call.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind != SyntaxKind.MemberAccessExpression)
        {
            return false;
        }

        var receiver = callee.Children.FirstOrDefault(child => !child.IsToken);
        if (receiver?.Kind != SyntaxKind.IdentifierExpression || !TryGetIdentifier(receiver, out typeName))
        {
            return false;
        }

        var member = callee.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
        methodName = member?.Text ?? string.Empty;
        return methodName.Length > 0;
    }

    private static bool TryGetIdentifier(SyntaxNode node, out string name)
    {
        name = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
        return name.Length > 0;
    }

    private static MetadataByRefKind GetArgumentByRefKind(SyntaxNode argument)
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

    private static string FormatExpected(MetadataByRefKind kind) =>
        kind switch
        {
            MetadataByRefKind.Ref => "with 'ref'",
            MetadataByRefKind.Out => "with 'out'",
            MetadataByRefKind.In => "with 'in'",
            _ => "without a byref modifier"
        };

    private static string FormatActual(MetadataByRefKind kind) =>
        kind switch
        {
            MetadataByRefKind.Ref => "'ref'",
            MetadataByRefKind.Out => "'out'",
            MetadataByRefKind.In => "'in'",
            _ => "no byref modifier"
        };
}
