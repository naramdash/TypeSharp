using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpInteropValidator
{
    public static IReadOnlyList<Diagnostic> Validate(
        SyntaxNode root,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        string nullableMode = "strict")
    {
        var diagnostics = new List<Diagnostic>();
        ValidateNode(root, assemblies, file, nullableMode, diagnostics);
        return diagnostics;
    }

    private static void ValidateNode(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        string nullableMode,
        List<Diagnostic> diagnostics)
    {
        if (node.Kind == SyntaxKind.CallExpression)
        {
            ValidateCall(node, assemblies, file, nullableMode, diagnostics);
        }

        foreach (var child in node.Children.Where(child => !child.IsToken))
        {
            ValidateNode(child, assemblies, file, nullableMode, diagnostics);
        }
    }

    private static void ValidateCall(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        string nullableMode,
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
            .SelectMany(type => type.Methods.Select(method => new CSharpOverloadCandidate(type, method)))
            .Where(candidate => string.Equals(candidate.Method.Name, methodName, StringComparison.Ordinal))
            .ToArray();

        var resolution = TypeSharpCSharpOverloadResolver.Resolve(candidates, arguments);
        if (resolution.IsAmbiguous)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.AmbiguousCSharpOverload.Code,
                DiagnosticDescriptors.AmbiguousCSharpOverload.DefaultSeverity,
                $"Call to C# method '{typeName}.{methodName}' matches {resolution.BestCandidates.Count} overload candidates. Add an explicit type annotation or make the call unambiguous.",
                file,
                node.Span));
            return;
        }

        if (resolution.SelectedCandidate is not { } selectedCandidate)
        {
            return;
        }

        var (metadataType, metadataMethod) = selectedCandidate;
        if (IsStrictNullableMode(nullableMode) &&
            metadataMethod.ReturnNullability == MetadataNullabilityKind.Unknown)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnknownCSharpNullability.Code,
                DiagnosticDescriptors.UnknownCSharpNullability.DefaultSeverity,
                $"Call to C# method '{metadataType.FullName}.{metadataMethod.Name}' returns reference type '{metadataMethod.ReturnType}' from metadata without nullable annotations.",
                file,
                node.Span));
        }

        for (var index = 0; index < arguments.Length; index++)
        {
            var argument = arguments[index];
            var parameter = TypeSharpCSharpOverloadResolver.GetParameterForArgument(metadataMethod, arguments, index);
            if (parameter is null)
            {
                continue;
            }

            var actual = TypeSharpCSharpOverloadResolver.GetArgumentByRefKind(argument);
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

    private static bool IsStrictNullableMode(string nullableMode) =>
        string.Equals(nullableMode, "strict", StringComparison.OrdinalIgnoreCase);

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
