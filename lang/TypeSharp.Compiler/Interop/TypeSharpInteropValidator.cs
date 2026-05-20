using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpInteropValidator
{
    private const int ExactIndexerMatchScore = 0;
    private const int NumericIndexerConversionScore = 10;
    private const int MetadataIndexerRelationScore = 100;
    private const int ObjectIndexerFallbackScore = 1000;

    public static IReadOnlyList<Diagnostic> Validate(
        SyntaxNode root,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        string nullableMode = "strict")
    {
        var diagnostics = new List<Diagnostic>();
        var extensionNamespaces = CollectExtensionMethodNamespaces(root);
        ValidateNode(
            root,
            assemblies,
            file,
            nullableMode,
            diagnostics,
            extensionNamespaces,
            new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(StringComparer.Ordinal),
            parent: null,
            grandParent: null);
        return diagnostics;
    }

    private static void ValidateNode(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        string nullableMode,
        List<Diagnostic> diagnostics,
        IReadOnlyCollection<string> extensionNamespaces,
        Dictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        SyntaxNode? parent,
        SyntaxNode? grandParent)
    {
        if (node.Kind == SyntaxKind.FunctionDeclaration)
        {
            var functionScope = new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(StringComparer.Ordinal);
            TrackFunctionParameters(node, assemblies, functionScope);
            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                ValidateNode(child, assemblies, file, nullableMode, diagnostics, extensionNamespaces, functionScope, node, parent);
            }

            return;
        }

        if (node.Kind == SyntaxKind.BlockExpression)
        {
            var blockScope = new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(localInstances, StringComparer.Ordinal);
            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                ValidateNode(child, assemblies, file, nullableMode, diagnostics, extensionNamespaces, blockScope, node, parent);
            }

            return;
        }

        if (node.Kind == SyntaxKind.LambdaExpression)
        {
            var lambdaScope = new Dictionary<string, IReadOnlyList<MetadataTypeSymbol>>(localInstances, StringComparer.Ordinal);
            TrackContextualLambdaParameters(node, parent, grandParent, assemblies, extensionNamespaces, localInstances, lambdaScope);
            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                ValidateNode(child, assemblies, file, nullableMode, diagnostics, extensionNamespaces, lambdaScope, node, parent);
            }

            return;
        }

        if (node.Kind is SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration)
        {
            ValidateNamedImport(node, assemblies, file, diagnostics);
        }

        if (node.Kind == SyntaxKind.MemberAccessExpression)
        {
            var isCallCallee = IsCallCallee(node, parent, grandParent);
            var isAssignmentTarget = IsAssignmentTarget(node, parent);
            if (!isCallCallee)
            {
                ValidateStaticMemberAccess(node, assemblies, file, diagnostics);
            }

            if (isAssignmentTarget)
            {
                ValidateStaticPropertySetterAccess(node, parent, assemblies, file, diagnostics);
                ValidateStaticFieldAssignmentAccess(node, parent, assemblies, file, diagnostics);
                ValidateInstanceEventAssignmentAccess(node, parent, localInstances, file, diagnostics);
                ValidateInstancePropertySetterAccess(node, parent, localInstances, file, diagnostics);
                ValidateInstanceFieldAssignmentAccess(node, parent, localInstances, file, diagnostics);
            }
            else
            {
                var callExpression = parent?.Kind == SyntaxKind.CallExpression ? parent : grandParent?.Kind == SyntaxKind.CallExpression ? grandParent : null;
                ValidateInstanceMemberAccess(node, assemblies, extensionNamespaces, localInstances, file, diagnostics, isCallCallee, callExpression);
            }
        }

        if (node.Kind == SyntaxKind.IndexerExpression)
        {
            ValidateInstanceIndexerAccess(node, assemblies, localInstances, file, diagnostics);
        }

        if (node.Kind == SyntaxKind.CallExpression)
        {
            ValidateCall(node, assemblies, extensionNamespaces, file, nullableMode, localInstances, diagnostics);
        }

        foreach (var child in node.Children.Where(child => !child.IsToken))
        {
            ValidateNode(child, assemblies, file, nullableMode, diagnostics, extensionNamespaces, localInstances, node, parent);
        }

        if (node.Kind == SyntaxKind.ValueDeclaration)
        {
            TrackLocalInstance(node, assemblies, localInstances);
        }

        if (node.Kind == SyntaxKind.AssignmentExpression)
        {
            TrackAssignedInstance(node, assemblies, localInstances);
        }
    }

    private static void ValidateNamedImport(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetModuleSpecifier(node, out _, out var moduleSpecifier) ||
            IsRelativeSpecifier(moduleSpecifier))
        {
            return;
        }

        var namespaceTypes = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => string.Equals(type.Namespace, moduleSpecifier, StringComparison.Ordinal))
            .ToArray();

        if (namespaceTypes.Length == 0)
        {
            return;
        }

        foreach (var specifier in GetNamedImportSpecifiers(node))
        {
            if (namespaceTypes.Any(type => MetadataTypeMatchesImportedName(type, specifier.ImportedName)))
            {
                continue;
            }

            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingCSharpType.Code,
                DiagnosticDescriptors.MissingCSharpType.DefaultSeverity,
                $"C# namespace '{moduleSpecifier}' does not contain a public type named '{specifier.ImportedName}'. Check the import, type name, or referenced assembly.",
                file,
                specifier.Span));
        }
    }

    private static void TrackContextualLambdaParameters(
        SyntaxNode lambda,
        SyntaxNode? parent,
        SyntaxNode? grandParent,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string> extensionNamespaces,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        Dictionary<string, IReadOnlyList<MetadataTypeSymbol>> lambdaScope)
    {
        var call = parent?.Kind == SyntaxKind.CallExpression
            ? parent
            : grandParent?.Kind == SyntaxKind.CallExpression
                ? grandParent
                : null;
        if (call is null ||
            !TryResolveStaticMethodCall(call, assemblies, extensionNamespaces, localInstances, out var selectedCandidate, out var arguments))
        {
            return;
        }

        var argumentIndex = -1;
        for (var index = 0; index < arguments.Count; index++)
        {
            if (ReferenceEquals(UnwrapArgumentExpression(arguments[index]), lambda))
            {
                argumentIndex = index;
                break;
            }
        }

        if (argumentIndex < 0)
        {
            return;
        }

        var parameter = TypeSharpCSharpOverloadResolver.GetParameterForArgument(selectedCandidate.Method, arguments, argumentIndex);
        if (parameter is null ||
            !TryGetKnownDelegateParameterTypes(parameter.Type, out var parameterTypes) ||
            !TryGetLambdaParameterNames(lambda, out var parameterNames) ||
            parameterNames.Count != parameterTypes.Count)
        {
            return;
        }

        for (var index = 0; index < parameterNames.Count; index++)
        {
            var metadataTypes = FindMetadataTypes(assemblies, parameterTypes[index]);
            if (metadataTypes.Count > 0)
            {
                lambdaScope[parameterNames[index]] = metadataTypes;
            }
            else
            {
                lambdaScope.Remove(parameterNames[index]);
            }
        }
    }

    private static bool TryResolveStaticMethodCall(
        SyntaxNode call,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string> extensionNamespaces,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out CSharpOverloadCandidate selectedCandidate,
        out IReadOnlyList<SyntaxNode> arguments)
    {
        selectedCandidate = default!;
        arguments = [];
        if (!TryGetStaticMemberCall(call, out var typeName, out var methodName, out var explicitGenericTypeArguments))
        {
            return false;
        }

        var metadataTypes = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal) || string.Equals(type.FullName, typeName, StringComparison.Ordinal))
            .ToArray();
        if (metadataTypes.Length == 0)
        {
            return false;
        }

        var candidates = metadataTypes
            .SelectMany(type => type.Methods.Select(method => new CSharpOverloadCandidate(type, method)))
            .Where(candidate => candidate.Method.IsStatic &&
                string.Equals(candidate.Method.Name, methodName, StringComparison.Ordinal))
            .ToArray();
        if (candidates.Length == 0)
        {
            return false;
        }

        arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
        var explicitGenericTypeArgumentCount = explicitGenericTypeArguments.Count == 0
            ? (int?)null
            : explicitGenericTypeArguments.Count;
        var resolution = TypeSharpCSharpOverloadResolver.Resolve(
            candidates,
            arguments,
            explicitGenericTypeArgumentCount,
            assemblies,
            localInstances,
            extensionNamespaces);

        if (resolution.IsAmbiguous || resolution.SelectedCandidate is not { } selected)
        {
            return false;
        }

        selectedCandidate = selected;
        return true;
    }

    private static bool TryGetKnownDelegateParameterTypes(string typeName, out IReadOnlyList<string> parameterTypes)
    {
        parameterTypes = [];
        if (!TryParseConstructedGenericTypeName(typeName, out var genericTypeName, out var typeArguments))
        {
            return TypeNameMatches(typeName, "System.Action");
        }

        var genericName = GetUnqualifiedTypeName(StripGenericArity(genericTypeName));
        if (string.Equals(genericName, "Func", StringComparison.Ordinal) && typeArguments.Count > 0)
        {
            parameterTypes = typeArguments.Take(typeArguments.Count - 1).ToArray();
            return true;
        }

        if (string.Equals(genericName, "Action", StringComparison.Ordinal))
        {
            parameterTypes = typeArguments;
            return true;
        }

        return false;
    }

    private static bool TryGetLambdaParameterNames(SyntaxNode lambda, out IReadOnlyList<string> names)
    {
        if (lambda.Kind != SyntaxKind.LambdaExpression)
        {
            names = [];
            return false;
        }

        names = lambda.Children
            .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            .Select(child => child.Text ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToArray();
        return true;
    }

    private static IReadOnlyCollection<string> CollectExtensionMethodNamespaces(SyntaxNode root)
    {
        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var child in root.Children)
        {
            if (child.Kind is SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration &&
                TryGetModuleSpecifier(child, out _, out var moduleSpecifier) &&
                !IsRelativeSpecifier(moduleSpecifier))
            {
                namespaces.Add(moduleSpecifier);
                continue;
            }

            if (child.Kind == SyntaxKind.OpenDeclaration)
            {
                var namespaceName = GetQualifiedName(child);
                if (namespaceName.Length > 0)
                {
                    namespaces.Add(namespaceName);
                }
            }
        }

        return namespaces;
    }

    private static void ValidateStaticMemberAccess(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetStaticMemberAccess(node, out var typeName, out var memberName))
        {
            return;
        }

        var metadataTypes = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal) || string.Equals(type.FullName, typeName, StringComparison.Ordinal))
            .ToArray();

        if (metadataTypes.Length == 0 ||
            metadataTypes.Any(type => HasPublicStaticMember(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpStaticMember.Code,
            DiagnosticDescriptors.MissingCSharpStaticMember.DefaultSeverity,
            $"C# type '{typeName}' does not contain a public static member named '{memberName}'. Check the member name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateInstanceMemberAccess(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string> extensionNamespaces,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        string file,
        List<Diagnostic> diagnostics,
        bool isCallCallee,
        SyntaxNode? callExpression)
    {
        if (!TryGetInstanceMemberAccess(node, out var receiverName, out var memberName) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes) ||
            receiverTypes.Count == 0)
        {
            return;
        }

        if (receiverTypes.Any(type => HasPublicInstanceMember(type, memberName, methodOnly: isCallCallee)))
        {
            return;
        }

        if (isCallCallee && callExpression is not null)
        {
            var extensionCandidates = receiverTypes
                .SelectMany(type => FindExtensionMethodCandidates(assemblies, extensionNamespaces, type, memberName))
                .ToArray();
            if (extensionCandidates.Length > 0)
            {
                var arguments = callExpression.Children.Skip(1).Where(child => !child.IsToken).ToArray();
                var receiverExpression = node.Children.FirstOrDefault(child => !child.IsToken);
                var extensionArguments = receiverExpression is null
                    ? arguments
                    : new[] { receiverExpression }.Concat(arguments).ToArray();
                var resolution = TypeSharpCSharpOverloadResolver.Resolve(
                    extensionCandidates,
                    extensionArguments,
                    assemblies: assemblies,
                    localInstances: localInstances,
                    extensionNamespaces: extensionNamespaces);

                if (resolution.IsAmbiguous)
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.AmbiguousCSharpOverload.Code,
                        DiagnosticDescriptors.AmbiguousCSharpOverload.DefaultSeverity,
                        $"Call to C# extension method '{receiverName}.{memberName}' matches {resolution.BestCandidates.Count} overload candidates. Add an explicit type annotation or make the call unambiguous.",
                        file,
                        node.Span));
                }
                else if (resolution.SelectedCandidate is null)
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.NoMatchingCSharpOverload.Code,
                        DiagnosticDescriptors.NoMatchingCSharpOverload.DefaultSeverity,
                        $"Call to C# extension method '{receiverName}.{memberName}' matches no overload candidate. Adjust the arguments, names, or byref modifiers.",
                        file,
                        node.Span));
                }

                return;
            }
        }

        var memberKind = isCallCallee ? "method" : "member";
        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpInstanceMember.Code,
            DiagnosticDescriptors.MissingCSharpInstanceMember.DefaultSeverity,
            $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' does not contain a public instance {memberKind} named '{memberName}'. Check the member name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateInstanceIndexerAccess(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetInstanceIndexerAccess(node, out var receiverName, out var arguments) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes) ||
            receiverTypes.Count == 0)
        {
            return;
        }

        var ambiguousCandidateCount = 0;
        foreach (var receiverType in receiverTypes)
        {
            var resolution = ResolvePublicInstanceIndexer(receiverType, arguments, assemblies, localInstances);
            if (resolution.IsApplicable)
            {
                return;
            }

            if (resolution.IsAmbiguous && ambiguousCandidateCount == 0)
            {
                ambiguousCandidateCount = resolution.CandidateCount;
            }
        }

        if (ambiguousCandidateCount > 0)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.AmbiguousCSharpOverload.Code,
                DiagnosticDescriptors.AmbiguousCSharpOverload.DefaultSeverity,
                $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' indexer access matches {ambiguousCandidateCount} indexer candidates. Add an explicit conversion or annotation to make the indexer access unambiguous.",
                file,
                node.Span));
            return;
        }

        var argumentCount = arguments.Count;
        var hasArityMatch = receiverTypes.Any(type => HasPublicInstanceIndexer(type, argumentCount));
        var message = hasArityMatch && TryFormatKnownIndexerArgumentTypes(arguments, assemblies, localInstances, out var argumentTypes)
            ? $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' does not contain a public instance indexer compatible with argument type(s) {argumentTypes}. Check the indexed value or referenced assembly."
            : $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' does not contain a public instance indexer with {argumentCount} argument(s). Check the indexed value or referenced assembly.";

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpInstanceIndexer.Code,
            DiagnosticDescriptors.MissingCSharpInstanceIndexer.DefaultSeverity,
            message,
            file,
            node.Span));
    }

    private static void ValidateStaticPropertySetterAccess(
        SyntaxNode node,
        SyntaxNode? assignment,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!IsSimpleAssignment(assignment) ||
            !TryGetStaticMemberAccess(node, out var typeName, out var memberName))
        {
            return;
        }

        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count == 0 ||
            metadataTypes.Any(type => HasPublicStaticPropertySetter(type, memberName)))
        {
            return;
        }

        if (!metadataTypes.Any(type => HasPublicStaticProperty(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpStaticPropertySetter.Code,
            DiagnosticDescriptors.MissingCSharpStaticPropertySetter.DefaultSeverity,
            $"C# type '{typeName}' does not contain a public static setter for property '{memberName}'. Check the property name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateStaticFieldAssignmentAccess(
        SyntaxNode node,
        SyntaxNode? assignment,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!IsSimpleAssignment(assignment) ||
            !TryGetStaticMemberAccess(node, out var typeName, out var memberName))
        {
            return;
        }

        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count == 0 ||
            metadataTypes.Any(type => HasWritablePublicStaticField(type, memberName)))
        {
            return;
        }

        if (!metadataTypes.Any(type => HasReadOnlyPublicStaticField(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.ReadOnlyCSharpStaticFieldAssignment.Code,
            DiagnosticDescriptors.ReadOnlyCSharpStaticFieldAssignment.DefaultSeverity,
            $"C# type '{typeName}' cannot assign to read-only static field '{memberName}'. Check the field name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateInstancePropertySetterAccess(
        SyntaxNode node,
        SyntaxNode? assignment,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!IsSimpleAssignment(assignment) ||
            !TryGetInstanceMemberAccess(node, out var receiverName, out var memberName) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes) ||
            receiverTypes.Count == 0)
        {
            return;
        }

        if (receiverTypes.Any(type => HasPublicInstancePropertySetter(type, memberName)))
        {
            return;
        }

        if (!receiverTypes.Any(type => HasPublicInstanceProperty(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpInstancePropertySetter.Code,
            DiagnosticDescriptors.MissingCSharpInstancePropertySetter.DefaultSeverity,
            $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' does not contain a public instance setter for property '{memberName}'. Check the property name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateInstanceEventAssignmentAccess(
        SyntaxNode node,
        SyntaxNode? assignment,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetEventAssignmentAction(assignment, out var action) ||
            !TryGetInstanceMemberAccess(node, out var receiverName, out var memberName) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes) ||
            receiverTypes.Count == 0)
        {
            return;
        }

        if (receiverTypes.Any(type => HasPublicInstanceEventAccessor(type, memberName, action)))
        {
            return;
        }

        if (receiverTypes.Any(type => HasPublicInstanceNonEventMember(type, memberName) &&
            !HasPublicInstanceEvent(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.MissingCSharpInstanceEvent.Code,
            DiagnosticDescriptors.MissingCSharpInstanceEvent.DefaultSeverity,
            $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' does not contain a public instance event named '{memberName}' with public {action} accessor. Check the event name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateInstanceFieldAssignmentAccess(
        SyntaxNode node,
        SyntaxNode? assignment,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        string file,
        List<Diagnostic> diagnostics)
    {
        if (!IsSimpleAssignment(assignment) ||
            !TryGetInstanceMemberAccess(node, out var receiverName, out var memberName) ||
            !localInstances.TryGetValue(receiverName, out var receiverTypes) ||
            receiverTypes.Count == 0)
        {
            return;
        }

        if (!receiverTypes.Any(type => HasReadOnlyPublicInstanceField(type, memberName)))
        {
            return;
        }

        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.ReadOnlyCSharpInstanceFieldAssignment.Code,
            DiagnosticDescriptors.ReadOnlyCSharpInstanceFieldAssignment.DefaultSeverity,
            $"C# value '{receiverName}' of imported type '{FormatMetadataTypes(receiverTypes)}' cannot assign to readonly instance field '{memberName}'. Check the field name or referenced assembly.",
            file,
            node.Span));
    }

    private static void ValidateCall(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string> extensionNamespaces,
        string file,
        string nullableMode,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        List<Diagnostic> diagnostics)
    {
        var arguments = node.Children.Skip(1).Where(child => !child.IsToken).ToArray();
        if (TryGetConstructorCall(node, out var constructorTypeName, out var explicitConstructorTypeArguments))
        {
            ValidateConstructorCall(
                node,
                constructorTypeName,
                explicitConstructorTypeArguments,
                arguments,
                assemblies,
                localInstances,
                extensionNamespaces,
                file,
                diagnostics);
            return;
        }

        if (!TryGetStaticMemberCall(node, out var typeName, out var methodName, out var explicitGenericTypeArguments))
        {
            return;
        }

        var explicitGenericTypeArgumentCount = explicitGenericTypeArguments.Count == 0
            ? (int?)null
            : explicitGenericTypeArguments.Count;
        var metadataTypes = assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal) || string.Equals(type.FullName, typeName, StringComparison.Ordinal))
            .ToArray();

        if (metadataTypes.Length == 0)
        {
            return;
        }

        var candidates = metadataTypes
            .SelectMany(type => type.Methods.Select(method => new CSharpOverloadCandidate(type, method)))
            .Where(candidate => candidate.Method.IsStatic &&
                string.Equals(candidate.Method.Name, methodName, StringComparison.Ordinal))
            .ToArray();

        if (candidates.Length == 0)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingCSharpMethod.Code,
                DiagnosticDescriptors.MissingCSharpMethod.DefaultSeverity,
                FormatMissingCSharpMethodMessage(typeName, methodName, explicitGenericTypeArgumentCount),
                file,
                node.Span));
            return;
        }

        var resolution = TypeSharpCSharpOverloadResolver.Resolve(
            candidates,
            arguments,
            explicitGenericTypeArgumentCount,
            assemblies,
            localInstances,
            extensionNamespaces);
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
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.NoMatchingCSharpOverload.Code,
                DiagnosticDescriptors.NoMatchingCSharpOverload.DefaultSeverity,
                FormatNoMatchingOverloadMessage(typeName, methodName, explicitGenericTypeArgumentCount),
                file,
                node.Span));
            return;
        }

        var (metadataType, metadataMethod) = selectedCandidate;
        var genericTypeArguments = explicitGenericTypeArguments.Count == 0
            ? InferGenericTypeArguments(metadataMethod, arguments, assemblies, localInstances)
            : explicitGenericTypeArguments;
        if (TryGetUnsatisfiedGenericConstraint(
            metadataMethod,
            genericTypeArguments,
            assemblies,
            out var constraintMessage,
            out var constraintSpan))
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnsatisfiedCSharpGenericConstraint.Code,
                DiagnosticDescriptors.UnsatisfiedCSharpGenericConstraint.DefaultSeverity,
                $"Call to C# method '{metadataType.FullName}.{metadataMethod.Name}' violates imported generic constraints. {constraintMessage}",
                file,
                constraintSpan));
            return;
        }

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

    private static void ValidateConstructorCall(
        SyntaxNode node,
        string typeName,
        IReadOnlyList<GenericTypeArgument> explicitTypeArguments,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        IReadOnlyCollection<string> extensionNamespaces,
        string file,
        List<Diagnostic> diagnostics)
    {
        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count == 0)
        {
            return;
        }

        var matchingArityTypes = metadataTypes
            .Where(type => GetTypeGenericParameterCount(type.Name) == explicitTypeArguments.Count)
            .ToArray();
        if (matchingArityTypes.Length == 0)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.NoMatchingCSharpOverload.Code,
                DiagnosticDescriptors.NoMatchingCSharpOverload.DefaultSeverity,
                FormatNoMatchingConstructorMessage(typeName, explicitTypeArguments.Count),
                file,
                node.Span));
            return;
        }

        var candidates = matchingArityTypes
            .SelectMany(type => CreateConstructorCandidates(type, explicitTypeArguments))
            .ToArray();
        if (candidates.Length == 0)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.NoMatchingCSharpOverload.Code,
                DiagnosticDescriptors.NoMatchingCSharpOverload.DefaultSeverity,
                FormatNoMatchingConstructorMessage(typeName, explicitTypeArguments.Count),
                file,
                node.Span));
            return;
        }

        var resolution = TypeSharpCSharpOverloadResolver.Resolve(
            candidates,
            arguments,
            assemblies: assemblies,
            localInstances: localInstances,
            extensionNamespaces: extensionNamespaces);
        if (resolution.IsAmbiguous)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.AmbiguousCSharpOverload.Code,
                DiagnosticDescriptors.AmbiguousCSharpOverload.DefaultSeverity,
                $"Call to C# constructor '{typeName}' matches {resolution.BestCandidates.Count} overload candidates. Add an explicit type annotation or make the call unambiguous.",
                file,
                node.Span));
            return;
        }

        if (resolution.SelectedCandidate is null)
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.NoMatchingCSharpOverload.Code,
                DiagnosticDescriptors.NoMatchingCSharpOverload.DefaultSeverity,
                FormatNoMatchingConstructorMessage(typeName, explicitTypeArguments.Count),
                file,
                node.Span));
        }
    }

    private static IReadOnlyList<CSharpOverloadCandidate> CreateConstructorCandidates(
        MetadataTypeSymbol type,
        IReadOnlyList<GenericTypeArgument> explicitTypeArguments)
    {
        var constructors = type.Constructors.ToList();
        if (type.IsValueType && constructors.All(constructor => constructor.Parameters.Count != 0))
        {
            constructors.Insert(
                0,
                new MetadataMethodSymbol(
                    ".ctor",
                    "void",
                    MetadataNullabilityKind.NotApplicable,
                    [],
                    IsStatic: false));
        }

        return constructors
            .Select(constructor => new CSharpOverloadCandidate(
                type,
                SubstituteConstructorTypeArguments(constructor, explicitTypeArguments)))
            .ToArray();
    }

    private static MetadataMethodSymbol SubstituteConstructorTypeArguments(
        MetadataMethodSymbol constructor,
        IReadOnlyList<GenericTypeArgument> explicitTypeArguments)
    {
        if (explicitTypeArguments.Count == 0)
        {
            return constructor;
        }

        return constructor with
        {
            Parameters = constructor.Parameters
                .Select(parameter => parameter with
                {
                    Type = SubstituteTypeGenericArguments(parameter.Type, explicitTypeArguments)
                })
                .ToArray()
        };
    }

    private static string SubstituteTypeGenericArguments(
        string typeName,
        IReadOnlyList<GenericTypeArgument> explicitTypeArguments)
    {
        var result = typeName;
        for (var index = 0; index < explicitTypeArguments.Count; index++)
        {
            result = result.Replace($"!{index}", explicitTypeArguments[index].Name, StringComparison.Ordinal);
        }

        return result;
    }

    private static IReadOnlyList<GenericTypeArgument> InferGenericTypeArguments(
        MetadataMethodSymbol method,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        if (method.GenericParameters.Count == 0)
        {
            return [];
        }

        var inferred = new GenericTypeArgument?[method.GenericParameters.Count];
        for (var index = 0; index < arguments.Count; index++)
        {
            var parameter = TypeSharpCSharpOverloadResolver.GetParameterForArgument(method, arguments, index);
            if (parameter is null)
            {
                continue;
            }

            if (!TryInferGenericTypeArgumentsForParameter(
                    parameter.Type,
                    parameter.IsParams,
                    arguments[index],
                    assemblies,
                    localInstances,
                    out var parameterInferences))
            {
                return [];
            }

            foreach (var parameterInference in parameterInferences)
            {
                if (parameterInference.Index < 0 || parameterInference.Index >= inferred.Length)
                {
                    return [];
                }

                var current = inferred[parameterInference.Index];
                if (current is not null && !TypeNameMatches(current.Value.Name, parameterInference.Argument.Name))
                {
                    return [];
                }

                inferred[parameterInference.Index] = parameterInference.Argument;
            }
        }

        return inferred.Any(argument => argument is null)
            ? []
            : inferred.Select(argument => argument!.Value).ToArray();
    }

    private static bool TryInferGenericTypeArgumentsForParameter(
        string parameterType,
        bool isParamsParameter,
        SyntaxNode argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out IReadOnlyList<InferredGenericTypeArgument> inferred)
    {
        inferred = [];
        var candidateParameterType = isParamsParameter && parameterType.EndsWith("[]", StringComparison.Ordinal)
            ? parameterType[..^2]
            : parameterType;

        if (TryGetGenericMethodParameterIndex(candidateParameterType, isParamsParameter: false, out var genericParameterIndex))
        {
            if (!TryInferGenericTypeArgument(argument, assemblies, localInstances, out var genericTypeArgument))
            {
                return false;
            }

            inferred = [new InferredGenericTypeArgument(genericParameterIndex, genericTypeArgument)];
            return true;
        }

        if (TryGetGenericMethodArrayParameterIndex(candidateParameterType, out genericParameterIndex))
        {
            if (!TryInferGenericArrayElementArgument(argument, assemblies, localInstances, out var genericTypeArgument))
            {
                return false;
            }

            inferred = [new InferredGenericTypeArgument(genericParameterIndex, genericTypeArgument)];
            return true;
        }

        if (!TryParseConstructedGenericTypeName(
                candidateParameterType,
                out var parameterGenericTypeName,
                out var parameterTypeArguments))
        {
            return true;
        }

        var genericParameterPositions = parameterTypeArguments
            .Select((typeArgument, index) => TryGetGenericMethodParameterIndex(typeArgument, isParamsParameter: false, out var parameterIndex)
                ? new GenericParameterPosition(index, parameterIndex)
                : (GenericParameterPosition?)null)
            .Where(position => position is not null)
            .Select(position => position!.Value)
            .ToArray();
        if (genericParameterPositions.Length == 0)
        {
            return true;
        }

        var expression = UnwrapArgumentExpression(argument);
        if (!TryInferConstructedGenericArgumentType(expression, assemblies, out var constructedArgument) ||
            constructedArgument.TypeArguments.Count != parameterTypeArguments.Count ||
            !TypeNameMatches(constructedArgument.Name, parameterGenericTypeName))
        {
            return false;
        }

        inferred = genericParameterPositions
            .Select(position => new InferredGenericTypeArgument(
                position.GenericParameterIndex,
                constructedArgument.TypeArguments[position.TypeArgumentIndex]))
            .ToArray();
        return true;
    }

    private static bool TryInferGenericTypeArgument(
        SyntaxNode argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out GenericTypeArgument genericTypeArgument)
    {
        var expression = UnwrapArgumentExpression(argument);
        if (TryInferLiteralGenericTypeArgument(expression, out genericTypeArgument))
        {
            return true;
        }

        if (TryInferConstructedGenericTypeArgument(expression, assemblies, out genericTypeArgument))
        {
            return true;
        }

        if (TryInferTrackedLocalGenericTypeArgument(expression, localInstances, out genericTypeArgument))
        {
            return true;
        }

        genericTypeArgument = default;
        return false;
    }

    private static bool TryInferGenericArrayElementArgument(
        SyntaxNode argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out GenericTypeArgument genericTypeArgument)
    {
        genericTypeArgument = default;
        var expression = UnwrapArgumentExpression(argument);
        if (expression.Kind != SyntaxKind.CollectionExpression)
        {
            return false;
        }

        var elements = expression.Children.Where(child => !child.IsToken).ToArray();
        if (elements.Length == 0 || elements.Any(element => element.Kind == SyntaxKind.SpreadElement))
        {
            return false;
        }

        foreach (var element in elements)
        {
            if (!TryInferGenericTypeArgument(element, assemblies, localInstances, out var elementArgument))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(genericTypeArgument.Name) &&
                !TypeNameMatches(genericTypeArgument.Name, elementArgument.Name))
            {
                return false;
            }

            genericTypeArgument = elementArgument;
        }

        return !string.IsNullOrEmpty(genericTypeArgument.Name);
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

    private static bool TryInferLiteralGenericTypeArgument(SyntaxNode expression, out GenericTypeArgument argument)
    {
        argument = default;
        if (expression.Kind != SyntaxKind.LiteralExpression)
        {
            return false;
        }

        var token = expression.Children.FirstOrDefault(child => child.IsToken);
        var typeName = token?.Kind switch
        {
            SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => "string",
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => "bool",
            SyntaxKind.NumericLiteralToken => InferNumericType(token.Text ?? string.Empty),
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return false;
        }

        argument = new GenericTypeArgument(typeName, expression);
        return true;
    }

    private static bool TryInferConstructedGenericTypeArgument(
        SyntaxNode expression,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out GenericTypeArgument argument)
    {
        argument = default;
        if (expression.Kind != SyntaxKind.CallExpression)
        {
            return false;
        }

        if (!TryGetConstructedCallTypeName(expression, out var typeName))
        {
            return false;
        }

        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count != 1)
        {
            return false;
        }

        argument = new GenericTypeArgument(metadataTypes[0].FullName, expression);
        return true;
    }

    private static bool TryInferConstructedGenericArgumentType(
        SyntaxNode expression,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out ConstructedGenericTypeArgument argument)
    {
        argument = default;
        if (expression.Kind != SyntaxKind.CallExpression ||
            !TryGetConstructedCallTypeName(expression, out var typeName))
        {
            return false;
        }

        var typeArguments = GetConstructedCallGenericTypeArguments(expression);
        if (typeArguments.Count == 0)
        {
            return false;
        }

        var metadataTypes = FindMetadataTypes(assemblies, typeName);
        if (metadataTypes.Count != 1)
        {
            return false;
        }

        argument = new ConstructedGenericTypeArgument(metadataTypes[0].FullName, typeArguments);
        return true;
    }

    private static IReadOnlyList<GenericTypeArgument> GetConstructedCallGenericTypeArguments(SyntaxNode callExpression)
    {
        var callee = callExpression.Children.FirstOrDefault(child => !child.IsToken);
        return callee?.Kind == SyntaxKind.GenericNameExpression
            ? GetExplicitGenericTypeArguments(callee)
            : [];
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

    private static bool TryInferTrackedLocalGenericTypeArgument(
        SyntaxNode expression,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out GenericTypeArgument argument)
    {
        argument = default;
        if (expression.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(expression, out var name) ||
            !localInstances.TryGetValue(name, out var metadataTypes) ||
            metadataTypes.Count != 1)
        {
            return false;
        }

        argument = new GenericTypeArgument(metadataTypes[0].FullName, expression);
        return true;
    }

    private static bool TryGetGenericMethodParameterIndex(
        string typeName,
        bool isParamsParameter,
        out int index)
    {
        index = 0;
        var candidate = isParamsParameter && typeName.EndsWith("[]", StringComparison.Ordinal)
            ? typeName[..^2]
            : typeName;

        return candidate.StartsWith("!!", StringComparison.Ordinal) &&
            int.TryParse(candidate[2..], out index);
    }

    private static bool TryGetGenericMethodArrayParameterIndex(string typeName, out int index)
    {
        index = 0;
        return typeName.EndsWith("[]", StringComparison.Ordinal) &&
            TryGetGenericMethodParameterIndex(typeName[..^2], isParamsParameter: false, out index);
    }

    private static string InferNumericType(string text)
    {
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
        {
            return "decimal";
        }

        return text.Contains('.', StringComparison.Ordinal) ? "double" : "int";
    }

    private static bool TryGetConstructorCall(
        SyntaxNode call,
        out string typeName,
        out IReadOnlyList<GenericTypeArgument> explicitTypeArguments)
    {
        typeName = string.Empty;
        explicitTypeArguments = [];

        var callee = call.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind == SyntaxKind.IdentifierExpression)
        {
            return TryGetIdentifier(callee, out typeName);
        }

        if (callee?.Kind != SyntaxKind.GenericNameExpression)
        {
            return false;
        }

        var target = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
        if (target?.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(target, out typeName))
        {
            return false;
        }

        explicitTypeArguments = GetExplicitGenericTypeArguments(callee);
        return explicitTypeArguments.Count > 0;
    }

    private static bool TryGetStaticMemberCall(
        SyntaxNode call,
        out string typeName,
        out string methodName,
        out IReadOnlyList<GenericTypeArgument> explicitGenericTypeArguments)
    {
        typeName = string.Empty;
        methodName = string.Empty;
        explicitGenericTypeArguments = [];

        var callee = call.Children.FirstOrDefault(child => !child.IsToken);
        if (callee?.Kind == SyntaxKind.GenericNameExpression)
        {
            explicitGenericTypeArguments = GetExplicitGenericTypeArguments(callee);
            callee = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
        }

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

    private static IReadOnlyList<GenericTypeArgument> GetExplicitGenericTypeArguments(SyntaxNode genericName)
    {
        var argumentList = genericName.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
        if (argumentList is null)
        {
            return [];
        }

        return argumentList.Children
            .Where(child => !child.IsToken)
            .Select(child => TryGetMetadataTypeName(child, out var name)
                ? new GenericTypeArgument(name, child)
                : new GenericTypeArgument(string.Empty, child))
            .Where(argument => argument.Name.Length > 0)
            .ToArray();
    }

    private static bool TryGetUnsatisfiedGenericConstraint(
        MetadataMethodSymbol method,
        IReadOnlyList<GenericTypeArgument> explicitGenericTypeArguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out string message,
        out SourceSpan span)
    {
        message = string.Empty;
        span = explicitGenericTypeArguments.Count == 0 ? default : explicitGenericTypeArguments[0].Node.Span;
        if (explicitGenericTypeArguments.Count == 0 ||
            method.GenericParameters.Count == 0 ||
            method.GenericParameters.Count != explicitGenericTypeArguments.Count)
        {
            return false;
        }

        for (var index = 0; index < method.GenericParameters.Count; index++)
        {
            var parameter = method.GenericParameters[index];
            var argument = explicitGenericTypeArguments[index];
            span = argument.Node.Span;

            if (parameter.HasReferenceTypeConstraint &&
                GetGenericTypeArgumentKind(argument, assemblies) != GenericTypeArgumentKind.Reference)
            {
                message = $"Type argument '{argument.Name}' for generic parameter '{parameter.Name}' must satisfy the C# 'class' constraint.";
                return true;
            }

            if (parameter.HasNotNullableValueTypeConstraint &&
                GetGenericTypeArgumentKind(argument, assemblies) != GenericTypeArgumentKind.Value)
            {
                message = $"Type argument '{argument.Name}' for generic parameter '{parameter.Name}' must satisfy the C# 'struct' constraint.";
                return true;
            }

            if (parameter.HasDefaultConstructorConstraint &&
                !HasPublicDefaultConstructor(argument, assemblies))
            {
                message = $"Type argument '{argument.Name}' for generic parameter '{parameter.Name}' must satisfy the C# 'new()' constraint.";
                return true;
            }

            foreach (var constraint in parameter.TypeConstraints)
            {
                if (SatisfiesTypeConstraint(argument, constraint, assemblies))
                {
                    continue;
                }

                message = $"Type argument '{argument.Name}' for generic parameter '{parameter.Name}' must satisfy the C# type constraint '{constraint}'.";
                return true;
            }
        }

        return false;
    }

    private static GenericTypeArgumentKind GetGenericTypeArgumentKind(
        GenericTypeArgument argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (IsKnownValueTypeName(argument.Name))
        {
            return GenericTypeArgumentKind.Value;
        }

        if (IsKnownReferenceTypeName(argument.Name))
        {
            return GenericTypeArgumentKind.Reference;
        }

        var metadataTypes = FindMetadataTypes(assemblies, argument.Name);
        if (metadataTypes.Count == 0)
        {
            return GenericTypeArgumentKind.Unknown;
        }

        return metadataTypes.Any(type => type.IsValueType)
            ? GenericTypeArgumentKind.Value
            : GenericTypeArgumentKind.Reference;
    }

    private static bool HasPublicDefaultConstructor(
        GenericTypeArgument argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (IsKnownValueTypeName(argument.Name))
        {
            return true;
        }

        if (string.Equals(argument.Name, "object", StringComparison.Ordinal) ||
            string.Equals(argument.Name, "System.Object", StringComparison.Ordinal))
        {
            return true;
        }

        var metadataTypes = FindMetadataTypes(assemblies, argument.Name);
        return metadataTypes.Any(type => type.IsValueType || type.HasPublicParameterlessConstructor);
    }

    private static bool SatisfiesTypeConstraint(
        GenericTypeArgument argument,
        string constraint,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (IsKnownValueTypeName(argument.Name) &&
            string.Equals(constraint, "System.ValueType", StringComparison.Ordinal))
        {
            return true;
        }

        if (TypeNameMatches(argument.Name, constraint))
        {
            return true;
        }

        var metadataTypes = FindMetadataTypes(assemblies, argument.Name);
        return metadataTypes.Any(type => TypeSymbolSatisfiesConstraint(
            type,
            constraint,
            assemblies,
            new HashSet<string>(StringComparer.Ordinal)));
    }

    private static bool TypeSymbolSatisfiesConstraint(
        MetadataTypeSymbol type,
        string constraint,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        HashSet<string> visited)
    {
        if (TypeNameMatches(type.FullName, constraint) ||
            TypeNameMatches(type.Name, constraint) ||
            TypeNameMatches(type.BaseTypeName, constraint) ||
            type.InterfaceNames.Any(interfaceName => TypeNameMatches(interfaceName, constraint)))
        {
            return true;
        }

        if (!visited.Add(type.FullName))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(type.BaseTypeName) &&
            FindMetadataTypes(assemblies, type.BaseTypeName)
                .Any(baseType => TypeSymbolSatisfiesConstraint(baseType, constraint, assemblies, visited)))
        {
            return true;
        }

        foreach (var interfaceName in type.InterfaceNames)
        {
            if (FindMetadataTypes(assemblies, interfaceName)
                .Any(interfaceType => TypeSymbolSatisfiesConstraint(interfaceType, constraint, assemblies, visited)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsKnownValueTypeName(string name) =>
        name is "bool" or "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal" or "char" ||
        name is "System.Boolean" or "System.Byte" or "System.SByte" or "System.Int16" or "System.UInt16" or "System.Int32" or "System.UInt32" or "System.Int64" or "System.UInt64" or "System.Single" or "System.Double" or "System.Decimal" or "System.Char";

    private static bool IsKnownReferenceTypeName(string name) =>
        name is "string" or "object" or "dynamic" ||
        name is "System.String" or "System.Object" ||
        name.EndsWith("[]", StringComparison.Ordinal);

    private static bool TypeNameMatches(string? actual, string expected)
    {
        if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        return string.Equals(actual, expected, StringComparison.Ordinal) ||
            string.Equals(GetUnqualifiedTypeName(actual), expected, StringComparison.Ordinal) ||
            string.Equals(actual, GetUnqualifiedTypeName(expected), StringComparison.Ordinal) ||
            string.Equals(StripGenericArity(actual), StripGenericArity(expected), StringComparison.Ordinal) ||
            string.Equals(GetUnqualifiedTypeName(StripGenericArity(actual)), GetUnqualifiedTypeName(StripGenericArity(expected)), StringComparison.Ordinal);
    }

    private static string GetUnqualifiedTypeName(string name)
    {
        var index = name.LastIndexOf('.');
        return index < 0 ? name : name[(index + 1)..];
    }

    private static bool TryGetStaticMemberAccess(
        SyntaxNode node,
        out string typeName,
        out string memberName)
    {
        typeName = string.Empty;
        memberName = string.Empty;

        if (node.Kind != SyntaxKind.MemberAccessExpression)
        {
            return false;
        }

        var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
        if (receiver?.Kind != SyntaxKind.IdentifierExpression || !TryGetIdentifier(receiver, out typeName))
        {
            return false;
        }

        var member = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
        memberName = member?.Text ?? string.Empty;
        return memberName.Length > 0;
    }

    private static bool TryGetInstanceMemberAccess(
        SyntaxNode node,
        out string receiverName,
        out string memberName)
    {
        receiverName = string.Empty;
        memberName = string.Empty;

        if (node.Kind != SyntaxKind.MemberAccessExpression)
        {
            return false;
        }

        var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
        if (receiver?.Kind != SyntaxKind.IdentifierExpression || !TryGetIdentifier(receiver, out receiverName))
        {
            return false;
        }

        var member = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
        memberName = member?.Text ?? string.Empty;
        return memberName.Length > 0;
    }

    private static bool TryGetInstanceIndexerAccess(
        SyntaxNode node,
        out string receiverName,
        out IReadOnlyList<SyntaxNode> arguments)
    {
        receiverName = string.Empty;
        arguments = [];

        if (node.Kind != SyntaxKind.IndexerExpression)
        {
            return false;
        }

        var expressions = node.Children.Where(child => !child.IsToken).ToArray();
        if (expressions.Length < 2 ||
            expressions[0].Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(expressions[0], out receiverName))
        {
            return false;
        }

        arguments = expressions.Skip(1).ToArray();
        return true;
    }

    private static bool IsCallCallee(SyntaxNode node, SyntaxNode? parent, SyntaxNode? grandParent)
    {
        if (parent?.Kind == SyntaxKind.CallExpression)
        {
            return ReferenceEquals(parent.Children.FirstOrDefault(child => !child.IsToken), node);
        }

        if (parent?.Kind == SyntaxKind.GenericNameExpression &&
            grandParent?.Kind == SyntaxKind.CallExpression &&
            ReferenceEquals(grandParent.Children.FirstOrDefault(child => !child.IsToken), parent))
        {
            return ReferenceEquals(parent.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList), node);
        }

        return false;
    }

    private static bool IsAssignmentTarget(SyntaxNode node, SyntaxNode? parent) =>
        parent?.Kind == SyntaxKind.AssignmentExpression &&
        ReferenceEquals(parent.Children.FirstOrDefault(child => !child.IsToken), node);

    private static bool IsSimpleAssignment(SyntaxNode? node) =>
        node?.Kind == SyntaxKind.AssignmentExpression &&
        node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.EqualsToken);

    private static bool TryGetEventAssignmentAction(SyntaxNode? node, out string action)
    {
        action = string.Empty;
        if (node?.Kind != SyntaxKind.AssignmentExpression)
        {
            return false;
        }

        if (node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.PlusEqualsToken))
        {
            action = "add";
            return true;
        }

        if (node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.MinusEqualsToken))
        {
            action = "remove";
            return true;
        }

        return false;
    }

    private static bool HasPublicStaticMember(MetadataTypeSymbol type, string memberName) =>
        type.Fields.Any(field => field.IsStatic && string.Equals(field.Name, memberName, StringComparison.Ordinal)) ||
        type.Properties.Any(property => property.IsStatic && string.Equals(property.Name, memberName, StringComparison.Ordinal)) ||
        type.Methods.Any(method => method.IsStatic && string.Equals(method.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicStaticProperty(MetadataTypeSymbol type, string memberName) =>
        type.Properties.Any(property =>
            property.IsStatic &&
            !property.IsIndexer &&
            string.Equals(property.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicStaticPropertySetter(MetadataTypeSymbol type, string memberName) =>
        type.Properties.Any(property =>
            property.IsStatic &&
            !property.IsIndexer &&
            property.HasPublicSetter &&
            string.Equals(property.Name, memberName, StringComparison.Ordinal));

    private static bool HasWritablePublicStaticField(MetadataTypeSymbol type, string memberName) =>
        type.Fields.Any(field =>
            field.IsStatic &&
            !field.IsLiteral &&
            !field.IsReadOnly &&
            string.Equals(field.Name, memberName, StringComparison.Ordinal));

    private static bool HasReadOnlyPublicStaticField(MetadataTypeSymbol type, string memberName) =>
        type.Fields.Any(field =>
            field.IsStatic &&
            (field.IsLiteral || field.IsReadOnly) &&
            string.Equals(field.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicInstanceMember(MetadataTypeSymbol type, string memberName, bool methodOnly)
    {
        if (type.Methods.Any(method => !method.IsStatic && string.Equals(method.Name, memberName, StringComparison.Ordinal)))
        {
            return true;
        }

        return !methodOnly &&
            (type.Fields.Any(field => !field.IsStatic && string.Equals(field.Name, memberName, StringComparison.Ordinal)) ||
            type.Properties.Any(property => !property.IsStatic && string.Equals(property.Name, memberName, StringComparison.Ordinal)));
    }

    private static IEnumerable<CSharpOverloadCandidate> FindExtensionMethodCandidates(
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyCollection<string> extensionNamespaces,
        MetadataTypeSymbol receiverType,
        string memberName)
    {
        foreach (var type in assemblies.SelectMany(assembly => assembly.Types).Where(type => extensionNamespaces.Contains(type.Namespace)))
        {
            foreach (var method in type.Methods)
            {
                if (method.IsStatic &&
                    method.IsExtension &&
                    string.Equals(method.Name, memberName, StringComparison.Ordinal) &&
                    method.Parameters.Count > 0 &&
                    ExtensionReceiverMatches(receiverType, method.Parameters[0].Type, assemblies))
                {
                    yield return new CSharpOverloadCandidate(type, method);
                }
            }
        }
    }

    private static bool ExtensionReceiverMatches(
        MetadataTypeSymbol receiverType,
        string extensionReceiverType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies) =>
        IsObjectExtensionReceiverParameter(extensionReceiverType) ||
        TypeNameMatches(receiverType.FullName, extensionReceiverType) ||
        TypeNameMatches(receiverType.Name, extensionReceiverType) ||
        TypeSymbolSatisfiesConstraint(
            receiverType,
            extensionReceiverType,
            assemblies,
            new HashSet<string>(StringComparer.Ordinal));

    private static bool IsObjectExtensionReceiverParameter(string receiverType) =>
        string.Equals(NormalizePrimitiveTypeName(receiverType), "object", StringComparison.Ordinal);

    private static bool HasPublicInstanceNonEventMember(MetadataTypeSymbol type, string memberName) =>
        type.Methods.Any(method => !method.IsStatic && string.Equals(method.Name, memberName, StringComparison.Ordinal)) ||
        type.Fields.Any(field => !field.IsStatic && string.Equals(field.Name, memberName, StringComparison.Ordinal)) ||
        type.Properties.Any(property => !property.IsStatic && string.Equals(property.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicInstanceEvent(MetadataTypeSymbol type, string memberName) =>
        type.Events.Any(eventSymbol =>
            !eventSymbol.IsStatic &&
            string.Equals(eventSymbol.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicInstanceEventAccessor(MetadataTypeSymbol type, string memberName, string action) =>
        type.Events.Any(eventSymbol =>
            !eventSymbol.IsStatic &&
            string.Equals(eventSymbol.Name, memberName, StringComparison.Ordinal) &&
            (string.Equals(action, "add", StringComparison.Ordinal)
                ? eventSymbol.HasPublicAdder
                : eventSymbol.HasPublicRemover));

    private static bool HasPublicInstanceIndexer(MetadataTypeSymbol type, int argumentCount) =>
        type.Properties.Any(property =>
            !property.IsStatic &&
            property.IsIndexer &&
            property.ParameterCount == argumentCount);

    private static IndexerResolution ResolvePublicInstanceIndexer(
        MetadataTypeSymbol type,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        var candidates = type.Properties
            .Where(property =>
                !property.IsStatic &&
                property.IsIndexer &&
                property.ParameterCount == arguments.Count)
            .ToArray();
        if (candidates.Length == 0)
        {
            return default;
        }

        if (!TryInferIndexerArgumentTypes(arguments, assemblies, localInstances, out var argumentTypes))
        {
            return candidates.Any(property => IndexerArgumentsMatch(property, arguments, assemblies, localInstances))
                ? new IndexerResolution(IsApplicable: true, IsAmbiguous: false, CandidateCount: 1)
                : default;
        }

        var scoredCandidates = candidates
            .Select(property => TryScoreIndexerArguments(property, argumentTypes, assemblies, out var score)
                ? new IndexerCandidateScore(property, score)
                : (IndexerCandidateScore?)null)
            .Where(score => score is not null)
            .Select(score => score!.Value)
            .ToArray();
        if (scoredCandidates.Length == 0)
        {
            return default;
        }

        var bestScore = scoredCandidates.Min(candidate => candidate.Score);
        var bestCandidates = scoredCandidates
            .Where(candidate => candidate.Score == bestScore)
            .Select(candidate => candidate.Property)
            .ToArray();
        bestCandidates = SelectMoreSpecificNullLiteralIndexerTargets(bestCandidates, arguments, assemblies);
        return bestCandidates.Length == 1
            ? new IndexerResolution(IsApplicable: true, IsAmbiguous: false, CandidateCount: 1)
            : new IndexerResolution(IsApplicable: false, IsAmbiguous: true, CandidateCount: bestCandidates.Length);
    }

    private static MetadataPropertySymbol[] SelectMoreSpecificNullLiteralIndexerTargets(
        MetadataPropertySymbol[] candidates,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (candidates.Length <= 1 || !arguments.Any(IsNullLiteralIndexerArgument))
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

                if (IsMoreSpecificForNullLiteralIndexerArguments(
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

    private static bool IsMoreSpecificForNullLiteralIndexerArguments(
        MetadataPropertySymbol better,
        MetadataPropertySymbol worse,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        var foundMoreSpecificTarget = false;
        for (var index = 0; index < arguments.Count; index++)
        {
            if (!IsNullLiteralIndexerArgument(arguments[index]))
            {
                continue;
            }

            var betterType = better.ParameterTypes[index];
            var worseType = worse.ParameterTypes[index];
            if (TypeNameMatches(betterType, worseType))
            {
                continue;
            }

            var betterIsMoreSpecific = IsMoreSpecificIndexerReferenceTarget(betterType, worseType, assemblies);
            var worseIsMoreSpecific = IsMoreSpecificIndexerReferenceTarget(worseType, betterType, assemblies);
            if (betterIsMoreSpecific && !worseIsMoreSpecific)
            {
                foundMoreSpecificTarget = true;
                continue;
            }

            return false;
        }

        return foundMoreSpecificTarget;
    }

    private static bool IsNullLiteralIndexerArgument(SyntaxNode argument)
    {
        var expression = UnwrapArgumentExpression(argument);
        var token = expression.Kind == SyntaxKind.LiteralExpression
            ? expression.Children.FirstOrDefault(child => child.IsToken)
            : null;
        return token?.Kind == SyntaxKind.NullKeyword;
    }

    private static bool IsMoreSpecificIndexerReferenceTarget(
        string candidateType,
        string otherType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (TypeNameMatches(candidateType, otherType))
        {
            return false;
        }

        if (IsObjectIndexerParameter(otherType) && !IsObjectIndexerParameter(candidateType))
        {
            return CanPassNullIndexerArgument(candidateType, assemblies);
        }

        return TryGetBestIndexerMetadataRelationshipDistance(candidateType, otherType, assemblies, out var distance) &&
            distance > 0;
    }

    private static bool IndexerArgumentsMatch(
        MetadataPropertySymbol property,
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        if (property.ParameterTypes.Count == 0)
        {
            return true;
        }

        if (property.ParameterTypes.Count != arguments.Count)
        {
            return false;
        }

        for (var index = 0; index < arguments.Count; index++)
        {
            if (!TryInferIndexerArgumentType(arguments[index], assemblies, localInstances, out var actualType))
            {
                return true;
            }

            if (!CanPassToIndexerParameter(actualType, property.ParameterTypes[index], assemblies))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryInferIndexerArgumentType(
        SyntaxNode argument,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out InferredIndexerArgumentType type)
    {
        var expression = UnwrapArgumentExpression(argument);
        if (TryInferLiteralIndexerArgumentType(expression, out type))
        {
            return true;
        }

        if (TryInferConstructedGenericTypeArgument(expression, assemblies, out var constructedArgument) ||
            TryInferTrackedLocalGenericTypeArgument(expression, localInstances, out constructedArgument))
        {
            type = new InferredIndexerArgumentType(constructedArgument.Name);
            return true;
        }

        type = default;
        return false;
    }

    private static bool TryInferIndexerArgumentTypes(
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out IReadOnlyList<InferredIndexerArgumentType> types)
    {
        var inferred = new List<InferredIndexerArgumentType>();
        foreach (var argument in arguments)
        {
            if (!TryInferIndexerArgumentType(argument, assemblies, localInstances, out var type))
            {
                types = [];
                return false;
            }

            inferred.Add(type);
        }

        types = inferred;
        return true;
    }

    private static bool TryInferLiteralIndexerArgumentType(SyntaxNode expression, out InferredIndexerArgumentType type)
    {
        type = default;
        if (expression.Kind != SyntaxKind.LiteralExpression)
        {
            return false;
        }

        var token = expression.Children.FirstOrDefault(child => child.IsToken);
        if (token is null)
        {
            return false;
        }

        type = token.Kind switch
        {
            SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => new InferredIndexerArgumentType("string"),
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => new InferredIndexerArgumentType("bool"),
            SyntaxKind.NumericLiteralToken => new InferredIndexerArgumentType(InferNumericType(token.Text ?? string.Empty), token.Text ?? string.Empty),
            SyntaxKind.NullKeyword => new InferredIndexerArgumentType("null", IsNullLiteral: true),
            _ => default
        };

        return !string.IsNullOrEmpty(type.Name);
    }

    private static bool CanPassToIndexerParameter(
        InferredIndexerArgumentType actualType,
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        if (actualType.IsNullLiteral)
        {
            return CanPassNullIndexerArgument(parameterType, assemblies);
        }

        if (IndexerTypeNameMatches(actualType.Name, parameterType))
        {
            return true;
        }

        if (IsObjectIndexerParameter(parameterType))
        {
            return true;
        }

        if (CanPassNumericIndexerArgumentType(actualType, parameterType))
        {
            return true;
        }

        return TryGetBestIndexerMetadataRelationshipDistance(
            actualType.Name,
            parameterType,
            assemblies,
            out _);
    }

    private static bool TryScoreIndexerArguments(
        MetadataPropertySymbol property,
        IReadOnlyList<InferredIndexerArgumentType> argumentTypes,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int score)
    {
        score = 0;
        if (property.ParameterTypes.Count == 0)
        {
            return true;
        }

        if (property.ParameterTypes.Count != argumentTypes.Count)
        {
            return false;
        }

        for (var index = 0; index < argumentTypes.Count; index++)
        {
            if (!TryScoreIndexerArgument(argumentTypes[index], property.ParameterTypes[index], assemblies, out var argumentScore))
            {
                score = 0;
                return false;
            }

            score += argumentScore;
        }

        return true;
    }

    private static bool TryScoreIndexerArgument(
        InferredIndexerArgumentType actualType,
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int score)
    {
        if (actualType.IsNullLiteral)
        {
            return TryScoreNullIndexerArgument(parameterType, assemblies, out score);
        }

        if (IndexerTypeNameMatches(actualType.Name, parameterType))
        {
            score = ExactIndexerMatchScore;
            return true;
        }

        if (CanPassNumericIndexerArgumentType(actualType, parameterType))
        {
            score = NumericIndexerConversionScore;
            return true;
        }

        if (IsObjectIndexerParameter(parameterType))
        {
            score = ObjectIndexerFallbackScore;
            return true;
        }

        if (TryGetBestIndexerMetadataRelationshipDistance(
                actualType.Name,
                parameterType,
                assemblies,
                out var distance))
        {
            score = MetadataIndexerRelationScore + distance;
            return true;
        }

        score = 0;
        return false;
    }

    private static bool TryGetBestIndexerMetadataRelationshipDistance(
        string actualTypeName,
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int distance)
    {
        distance = int.MaxValue;
        var found = false;
        foreach (var type in FindMetadataTypes(assemblies, actualTypeName))
        {
            if (TryGetIndexerMetadataRelationshipDistance(
                    type,
                    parameterType,
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

    private static bool TryGetIndexerMetadataRelationshipDistance(
        MetadataTypeSymbol type,
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        HashSet<string> visited,
        out int distance)
    {
        if (TypeNameMatches(type.FullName, parameterType) ||
            TypeNameMatches(type.Name, parameterType))
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
            if (TypeNameMatches(type.BaseTypeName, parameterType))
            {
                distance = Math.Min(distance, 1);
            }

            foreach (var baseType in FindMetadataTypes(assemblies, type.BaseTypeName))
            {
                if (TryGetIndexerMetadataRelationshipDistance(baseType, parameterType, assemblies, visited, out var baseDistance))
                {
                    distance = Math.Min(distance, baseDistance + 1);
                }
            }
        }

        foreach (var interfaceName in type.InterfaceNames)
        {
            if (TypeNameMatches(interfaceName, parameterType))
            {
                distance = Math.Min(distance, 1);
            }

            foreach (var interfaceType in FindMetadataTypes(assemblies, interfaceName))
            {
                if (TryGetIndexerMetadataRelationshipDistance(interfaceType, parameterType, assemblies, visited, out var interfaceDistance))
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

    private static bool IndexerTypeNameMatches(string actual, string expected) =>
        TypeNameMatches(actual, expected) ||
        string.Equals(NormalizePrimitiveTypeName(actual), NormalizePrimitiveTypeName(expected), StringComparison.Ordinal);

    private static bool IsObjectIndexerParameter(string parameterType) =>
        string.Equals(NormalizePrimitiveTypeName(parameterType), "object", StringComparison.Ordinal);

    private static bool TryFormatKnownIndexerArgumentTypes(
        IReadOnlyList<SyntaxNode> arguments,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out string formattedTypes)
    {
        var types = new List<string>();
        foreach (var argument in arguments)
        {
            if (!TryInferIndexerArgumentType(argument, assemblies, localInstances, out var typeName))
            {
                formattedTypes = string.Empty;
                return false;
            }

            types.Add($"'{typeName.Name}'");
        }

        formattedTypes = string.Join(", ", types);
        return true;
    }

    private static bool CanPassNullIndexerArgument(
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies) =>
        IsNullableIndexerValueType(parameterType) ||
        IsReferenceIndexerParameter(parameterType, assemblies);

    private static bool TryScoreNullIndexerArgument(
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out int score)
    {
        if (IsObjectIndexerParameter(parameterType))
        {
            score = ObjectIndexerFallbackScore;
            return true;
        }

        if (IsNullableIndexerValueType(parameterType) ||
            IsReferenceIndexerParameter(parameterType, assemblies))
        {
            score = MetadataIndexerRelationScore;
            return true;
        }

        score = 0;
        return false;
    }

    private static bool IsReferenceIndexerParameter(
        string parameterType,
        IReadOnlyList<MetadataAssemblySymbol> assemblies)
    {
        var normalized = NormalizePrimitiveTypeName(parameterType);
        if (IsObjectIndexerParameter(normalized) ||
            string.Equals(normalized, "string", StringComparison.Ordinal) ||
            normalized.EndsWith("[]", StringComparison.Ordinal))
        {
            return true;
        }

        if (IsKnownNonNullableValueTypeName(normalized))
        {
            return false;
        }

        var metadataTypes = FindMetadataTypes(assemblies, parameterType);
        if (metadataTypes.Count > 0)
        {
            return metadataTypes.Any(type => !type.IsValueType);
        }

        return normalized.Contains('.', StringComparison.Ordinal) ||
            normalized.Contains('<', StringComparison.Ordinal);
    }

    private static bool IsNullableIndexerValueType(string parameterType)
    {
        var normalized = StripGenericArity(NormalizePrimitiveTypeName(parameterType));
        var unqualified = GetUnqualifiedTypeName(normalized);
        return string.Equals(unqualified, "Nullable", StringComparison.Ordinal) ||
            normalized.StartsWith("System.Nullable<", StringComparison.Ordinal) ||
            normalized.StartsWith("Nullable<", StringComparison.Ordinal);
    }

    private static bool CanPassNumericIndexerArgumentType(InferredIndexerArgumentType actualType, string parameterType)
    {
        var actual = NormalizePrimitiveTypeName(actualType.Name);
        var parameter = NormalizePrimitiveTypeName(parameterType);
        if (!IsNumericType(actual) || !IsNumericType(parameter))
        {
            return false;
        }

        if (actual != "int")
        {
            return false;
        }

        if (parameter is "long" or "float" or "double" or "decimal")
        {
            return true;
        }

        return ulong.TryParse(actualType.NumericLiteralText, out var value) &&
            parameter switch
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

    private static bool IsNumericType(string type) =>
        NormalizePrimitiveTypeName(type) is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong" or "float" or "double" or "decimal";

    private static bool IsKnownNonNullableValueTypeName(string type)
    {
        var normalized = NormalizePrimitiveTypeName(type);
        return IsNumericType(normalized) ||
            normalized is "bool" or "char" or "void";
    }

    private static string NormalizePrimitiveTypeName(string type) =>
        type switch
        {
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
            "System.Boolean" or "Boolean" => "bool",
            "System.Char" or "Char" => "char",
            "System.String" or "String" => "string",
            "System.Object" or "Object" => "object",
            _ => type
        };

    private static bool HasPublicInstanceProperty(MetadataTypeSymbol type, string memberName) =>
        type.Properties.Any(property =>
            !property.IsStatic &&
            !property.IsIndexer &&
            string.Equals(property.Name, memberName, StringComparison.Ordinal));

    private static bool HasPublicInstancePropertySetter(MetadataTypeSymbol type, string memberName) =>
        type.Properties.Any(property =>
            !property.IsStatic &&
            !property.IsIndexer &&
            property.HasPublicSetter &&
            string.Equals(property.Name, memberName, StringComparison.Ordinal));

    private static bool HasReadOnlyPublicInstanceField(MetadataTypeSymbol type, string memberName) =>
        type.Fields.Any(field =>
            !field.IsStatic &&
            field.IsReadOnly &&
            string.Equals(field.Name, memberName, StringComparison.Ordinal));

    private static void TrackLocalInstance(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        Dictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        if (!TryGetValueDeclarationName(node, out var name))
        {
            return;
        }

        if (TryGetAnnotatedMetadataTypes(node, assemblies, out var metadataTypes) ||
            TryGetConstructedMetadataTypes(node, assemblies, out metadataTypes) ||
            TryGetAliasedMetadataTypes(node, localInstances, out metadataTypes))
        {
            localInstances[name] = metadataTypes;
        }
        else
        {
            localInstances.Remove(name);
        }
    }

    private static void TrackFunctionParameters(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        Dictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        foreach (var parameter in node.Children
            .Where(child => child.Kind == SyntaxKind.ParameterList)
            .SelectMany(child => child.Children)
            .Where(child => child.Kind == SyntaxKind.Parameter))
        {
            if (!TryGetParameterName(parameter, out var name))
            {
                continue;
            }

            if (TryGetAnnotatedMetadataTypes(parameter, assemblies, out var metadataTypes))
            {
                localInstances[name] = metadataTypes;
            }
            else
            {
                localInstances.Remove(name);
            }
        }
    }

    private static void TrackAssignedInstance(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        Dictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances)
    {
        if (!IsSimpleAssignment(node))
        {
            return;
        }

        var expressions = node.Children.Where(child => !child.IsToken).ToArray();
        if (expressions.Length < 2 ||
            expressions[0].Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(expressions[0], out var name))
        {
            return;
        }

        if (TryGetExpressionMetadataTypes(expressions[1], assemblies, localInstances, out var metadataTypes))
        {
            localInstances[name] = metadataTypes;
        }
        else
        {
            localInstances.Remove(name);
        }
    }

    private static bool TryGetParameterName(SyntaxNode node, out string name)
    {
        name = node.Children
            .FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?
            .Text ?? string.Empty;
        return name.Length > 0;
    }

    private static bool TryGetValueDeclarationName(SyntaxNode node, out string name)
    {
        name = node.Children
            .FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?
            .Text ?? string.Empty;
        return name.Length > 0;
    }

    private static bool TryGetConstructedMetadataTypes(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out IReadOnlyList<MetadataTypeSymbol> metadataTypes)
    {
        metadataTypes = [];
        var initializer = node.Children
            .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
            .Children
            .FirstOrDefault(child => !child.IsToken);
        if (initializer?.Kind != SyntaxKind.CallExpression)
        {
            return false;
        }

        if (!TryGetConstructedCallTypeName(initializer, out var typeName))
        {
            return false;
        }

        metadataTypes = FindMetadataTypes(assemblies, typeName);
        return metadataTypes.Count > 0;
    }

    private static bool TryGetAliasedMetadataTypes(
        SyntaxNode node,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out IReadOnlyList<MetadataTypeSymbol> metadataTypes)
    {
        metadataTypes = [];
        var initializer = node.Children
            .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
            .Children
            .FirstOrDefault(child => !child.IsToken);
        if (initializer?.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifier(initializer, out var aliasName) ||
            !localInstances.TryGetValue(aliasName, out var aliasTypes) ||
            aliasTypes.Count == 0)
        {
            return false;
        }

        metadataTypes = aliasTypes;
        return true;
    }

    private static bool TryGetExpressionMetadataTypes(
        SyntaxNode? expression,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        IReadOnlyDictionary<string, IReadOnlyList<MetadataTypeSymbol>> localInstances,
        out IReadOnlyList<MetadataTypeSymbol> metadataTypes)
    {
        metadataTypes = [];
        if (expression?.Kind == SyntaxKind.CallExpression)
        {
            if (TryGetConstructedCallTypeName(expression, out var typeName))
            {
                metadataTypes = FindMetadataTypes(assemblies, typeName);
                return metadataTypes.Count > 0;
            }
        }

        if (expression?.Kind == SyntaxKind.IdentifierExpression &&
            TryGetIdentifier(expression, out var aliasName) &&
            localInstances.TryGetValue(aliasName, out var aliasTypes) &&
            aliasTypes.Count > 0)
        {
            metadataTypes = aliasTypes;
            return true;
        }

        return false;
    }

    private static bool TryGetAnnotatedMetadataTypes(
        SyntaxNode node,
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        out IReadOnlyList<MetadataTypeSymbol> metadataTypes)
    {
        metadataTypes = [];
        var annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
        var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
        if (!TryGetMetadataTypeName(typeNode, out var typeName))
        {
            return false;
        }

        metadataTypes = FindMetadataTypes(assemblies, typeName);
        return metadataTypes.Count > 0;
    }

    private static bool TryGetMetadataTypeName(SyntaxNode? node, out string typeName)
    {
        typeName = string.Empty;
        if (node is null)
        {
            return false;
        }

        if (node.Kind == SyntaxKind.TypeAnnotation)
        {
            return TryGetMetadataTypeName(node.Children.FirstOrDefault(child => !child.IsToken), out typeName);
        }

        if (node.Kind == SyntaxKind.NullableType)
        {
            return TryGetMetadataTypeName(node.Children.FirstOrDefault(child => !child.IsToken), out typeName);
        }

        if (node.Kind != SyntaxKind.TypeName)
        {
            return false;
        }

        var genericBase = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
        if (genericBase is not null)
        {
            return TryGetMetadataTypeName(genericBase, out typeName);
        }

        var identifiers = node.Children
            .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            .Select(child => child.Text ?? string.Empty)
            .Where(text => text.Length > 0)
            .ToArray();
        if (identifiers.Length == 0)
        {
            return false;
        }

        typeName = string.Join(".", identifiers);
        return true;
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

    private static string GetQualifiedName(SyntaxNode? node)
    {
        if (node is null)
        {
            return string.Empty;
        }

        var identifiers = node.Kind == SyntaxKind.TypeName
            ? node.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
            : node.Children
                .Where(child => child.Kind == SyntaxKind.TypeName)
                .SelectMany(child => child.Children)
                .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);

        return string.Join(
            ".",
            identifiers
                .Select(identifier => identifier.Text ?? string.Empty)
                .Where(text => text.Length > 0));
    }

    private static IReadOnlyList<MetadataTypeSymbol> FindMetadataTypes(
        IReadOnlyList<MetadataAssemblySymbol> assemblies,
        string typeName) =>
        assemblies
            .SelectMany(assembly => assembly.Types)
            .Where(type =>
                string.Equals(type.FullName, typeName, StringComparison.Ordinal) ||
                MetadataTypeMatchesImportedName(type, typeName))
            .ToArray();

    private static string FormatMetadataTypes(IReadOnlyList<MetadataTypeSymbol> types) =>
        types.Count == 1
            ? types[0].FullName
            : string.Join(", ", types.Select(type => type.FullName).OrderBy(name => name, StringComparer.Ordinal));

    private static bool TryGetModuleSpecifier(SyntaxNode node, out SyntaxNode specifierToken, out string specifier)
    {
        for (var index = 0; index < node.Children.Count - 1; index++)
        {
            if (node.Children[index].Kind == SyntaxKind.FromKeyword &&
                node.Children[index + 1].Kind == SyntaxKind.StringLiteralToken)
            {
                specifierToken = node.Children[index + 1];
                specifier = Unquote(specifierToken.Text ?? string.Empty);
                return true;
            }
        }

        specifierToken = default!;
        specifier = string.Empty;
        return false;
    }

    private static bool IsRelativeSpecifier(string specifier) =>
        specifier == "." ||
        specifier == ".." ||
        specifier.StartsWith("./", StringComparison.Ordinal) ||
        specifier.StartsWith("../", StringComparison.Ordinal);

    private static string Unquote(string text)
    {
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
        {
            return text[1..^1];
        }

        return text;
    }

    private static IEnumerable<NamedImportSpecifier> GetNamedImportSpecifiers(SyntaxNode node)
    {
        var insideBraces = false;
        for (var index = 0; index < node.Children.Count; index++)
        {
            var child = node.Children[index];
            if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
            {
                insideBraces = true;
                continue;
            }

            if (child.IsToken && child.Kind == SyntaxKind.CloseBraceToken)
            {
                yield break;
            }

            if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } text)
            {
                if (index + 2 < node.Children.Count &&
                    node.Children[index + 1].IsToken &&
                    node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                    node.Children[index + 2].IsToken &&
                    node.Children[index + 2].Kind == SyntaxKind.IdentifierToken)
                {
                    yield return new NamedImportSpecifier(text, child.Span);
                    index += 2;
                }
                else
                {
                    yield return new NamedImportSpecifier(text, child.Span);
                }
            }
        }
    }

    private static bool MetadataTypeMatchesImportedName(MetadataTypeSymbol type, string importedName) =>
        string.Equals(type.Name, importedName, StringComparison.Ordinal) ||
        string.Equals(StripGenericArity(type.Name), importedName, StringComparison.Ordinal);

    private static int GetTypeGenericParameterCount(string metadataName)
    {
        var tickIndex = metadataName.IndexOf('`', StringComparison.Ordinal);
        if (tickIndex < 0 ||
            tickIndex == metadataName.Length - 1 ||
            !int.TryParse(metadataName[(tickIndex + 1)..], out var count))
        {
            return 0;
        }

        return count;
    }

    private static string StripGenericArity(string metadataName)
    {
        var tickIndex = metadataName.IndexOf('`', StringComparison.Ordinal);
        return tickIndex < 0 ? metadataName : metadataName[..tickIndex];
    }

    private static string FormatMissingCSharpMethodMessage(
        string typeName,
        string methodName,
        int? explicitGenericTypeArgumentCount)
    {
        var genericText = explicitGenericTypeArgumentCount is null
            ? string.Empty
            : $" with {explicitGenericTypeArgumentCount.Value} explicit generic type argument(s)";

        return $"C# type '{typeName}' does not contain a public static method named '{methodName}'{genericText}. Check the import, method name, or referenced assembly.";
    }

    private static bool TryGetIdentifier(SyntaxNode node, out string name)
    {
        name = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
        return name.Length > 0;
    }

    private static string FormatNoMatchingOverloadMessage(
        string typeName,
        string methodName,
        int? explicitGenericTypeArgumentCount)
    {
        var genericText = explicitGenericTypeArgumentCount is null
            ? string.Empty
            : $" with {explicitGenericTypeArgumentCount.Value} explicit generic type argument(s)";

        return $"Call to C# method '{typeName}.{methodName}'{genericText} matches no overload candidate. Adjust the arguments, generic type arguments, names, or byref modifiers.";
    }

    private static string FormatNoMatchingConstructorMessage(
        string typeName,
        int explicitGenericTypeArgumentCount)
    {
        var genericText = explicitGenericTypeArgumentCount == 0
            ? string.Empty
            : $" with {explicitGenericTypeArgumentCount} explicit generic type argument(s)";

        return $"Call to C# constructor '{typeName}'{genericText} matches no overload candidate. Adjust the constructor arguments or generic type arguments.";
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

    private readonly record struct NamedImportSpecifier(string ImportedName, SourceSpan Span);

    private readonly record struct GenericTypeArgument(string Name, SyntaxNode Node);

    private readonly record struct ConstructedGenericTypeArgument(string Name, IReadOnlyList<GenericTypeArgument> TypeArguments);

    private readonly record struct GenericParameterPosition(int TypeArgumentIndex, int GenericParameterIndex);

    private readonly record struct InferredGenericTypeArgument(int Index, GenericTypeArgument Argument);

    private readonly record struct InferredIndexerArgumentType(string Name, string? NumericLiteralText = null, bool IsNullLiteral = false);

    private readonly record struct IndexerCandidateScore(MetadataPropertySymbol Property, int Score);

    private readonly record struct IndexerResolution(bool IsApplicable, bool IsAmbiguous, int CandidateCount);

    private enum GenericTypeArgumentKind
    {
        Unknown,
        Reference,
        Value
    }
}
