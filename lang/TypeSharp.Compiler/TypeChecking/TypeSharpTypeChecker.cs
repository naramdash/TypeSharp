using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Interop;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;

namespace TypeSharp.Compiler.TypeChecking;

public static class TypeSharpTypeChecker
{
    public static TypeCheckResult Check(SyntaxNode root, string file)
    {
        var checker = new Checker(file, [], [], new HashSet<string>(StringComparer.Ordinal));
        return checker.Check(root);
    }

    public static TypeCheckResult Check(
        SyntaxNode root,
        string file,
        IReadOnlyList<MetadataAssemblySymbol> metadataAssemblies)
    {
        var checker = new Checker(file, metadataAssemblies, [], new HashSet<string>(StringComparer.Ordinal));
        return checker.Check(root);
    }

    public static TypeCheckResult Check(
        SyntaxNode root,
        string file,
        IReadOnlyList<MetadataAssemblySymbol> metadataAssemblies,
        IReadOnlyList<SourceAliasOption> sourceAliases,
        IReadOnlySet<string>? sourceModuleSpecifiers = null)
    {
        var checker = new Checker(file, metadataAssemblies, sourceAliases, sourceModuleSpecifiers ?? new HashSet<string>(StringComparer.Ordinal));
        return checker.Check(root);
    }

    private sealed class Checker
    {
        private static readonly HashSet<string> BuiltInTypes = new(StringComparer.Ordinal)
        {
            "bool",
            "byte",
            "char",
            "decimal",
            "double",
            "dynamic",
            "float",
            "int",
            "long",
            "never",
            "object",
            "sbyte",
            "short",
            "string",
            "uint",
            "ulong",
            "unit",
            "unknown",
            "ushort",
            "void"
        };

        private readonly string _file;
        private readonly IReadOnlyList<MetadataAssemblySymbol> _metadataAssemblies;
        private readonly IReadOnlyList<SourceAliasOption> _sourceAliases;
        private readonly IReadOnlySet<string> _sourceModuleSpecifiers;
        private readonly List<Diagnostic> _diagnostics = [];
        private readonly TypeSharpInferenceEngine _inference = new();
        private HashSet<string> _localExportedNames = new(StringComparer.Ordinal);

        public Checker(
            string file,
            IReadOnlyList<MetadataAssemblySymbol> metadataAssemblies,
            IReadOnlyList<SourceAliasOption> sourceAliases,
            IReadOnlySet<string> sourceModuleSpecifiers)
        {
            _file = file;
            _metadataAssemblies = metadataAssemblies;
            _sourceAliases = sourceAliases;
            _sourceModuleSpecifiers = sourceModuleSpecifiers;
        }

        public TypeCheckResult Check(SyntaxNode root)
        {
            _localExportedNames = CollectLocalExportedNames(root);
            var scope = new TypeScope(null);
            foreach (var type in BuiltInTypes)
            {
                scope.DeclareType(type);
            }

            CollectTopLevelTypesAndFunctions(root, scope);
            foreach (var child in root.Children)
            {
                CheckTopLevelDeclaration(child, scope);
            }

            return new TypeCheckResult(_diagnostics);
        }

        private void CollectTopLevelTypesAndFunctions(SyntaxNode root, TypeScope scope)
        {
            foreach (var child in root.Children)
            {
                switch (child.Kind)
                {
                    case SyntaxKind.ImportNamedDeclaration:
                    case SyntaxKind.ImportTypeDeclaration:
                        var hasModuleSpecifier = TryGetModuleSpecifier(child, out var moduleSpecifier);
                        var isSourceImport = hasModuleSpecifier && IsSourceModuleSpecifier(moduleSpecifier);
                        foreach (var importSpecifier in GetNamedImportSpecifiers(child))
                        {
                            var name = importSpecifier.LocalName;
                            scope.DeclareType(name);
                            if (child.Kind == SyntaxKind.ImportNamedDeclaration && isSourceImport)
                            {
                                scope.DeclareFunction(name, SimpleType.Unknown);
                            }

                            if (!isSourceImport &&
                                hasModuleSpecifier &&
                                TryFindImportedEnumMembers(moduleSpecifier, importSpecifier.ImportedName, out var importedEnumMembers))
                            {
                                scope.DeclareEnum(name, importedEnumMembers);
                            }
                        }

                        break;

                    case SyntaxKind.ImportNamespaceDeclaration:
                        if (TryGetNamespaceImportAlias(child, out var importAlias))
                        {
                            scope.DeclareType(importAlias.Text ?? string.Empty);
                        }

                        break;

                    case SyntaxKind.ModuleDeclaration:
                        if (TryGetDeclarationName(child, out var moduleName))
                        {
                            scope.DeclareType(moduleName);
                        }

                        break;

                    case SyntaxKind.TypeAliasDeclaration:
                        if (TryGetDeclarationName(child, out var aliasName))
                        {
                            scope.DeclareType(aliasName);
                            if (TryGetTypeAliasTarget(child, out var aliasTarget))
                            {
                                if (TryGetCompileTimeOnlyType(aliasTarget, scope, out var aliasKind))
                                {
                                    scope.DeclareCompileTimeOnlyType(aliasName, aliasKind);
                                }

                                var declaredTypeLevelUnion = false;
                                if (TryGetIndexedAccessTypeLevelUnion(aliasName, aliasTarget, scope, out var indexedAccessUnion))
                                {
                                    scope.DeclareTypeLevelUnion(aliasName, indexedAccessUnion.Members);
                                    declaredTypeLevelUnion = true;
                                }
                                else if (TryGetKeyofTypeLevelUnion(aliasName, aliasTarget, scope, out var keyofUnion))
                                {
                                    scope.DeclareTypeLevelUnion(aliasName, keyofUnion.Members);
                                    declaredTypeLevelUnion = true;
                                }
                                else if (TryGetTypeLevelUnion(aliasName, aliasTarget, out var typeLevelUnion))
                                {
                                    scope.DeclareTypeLevelUnion(aliasName, typeLevelUnion.Members);
                                    declaredTypeLevelUnion = true;
                                }

                                if (!declaredTypeLevelUnion &&
                                    aliasTarget.Kind == SyntaxKind.IndexedAccessType &&
                                    TryGetIndexedAccessType(aliasTarget, scope, out var indexedAccessType) &&
                                    indexedAccessType.IsKnown)
                                {
                                    scope.DeclareTypeAlias(aliasName, indexedAccessType);
                                }

                                if (TryGetStructuralShape(aliasName, aliasTarget, scope, out var structuralShape))
                                {
                                    scope.DeclareStructuralShape(aliasName, structuralShape.Members);
                                }
                            }
                        }

                        break;

                    case SyntaxKind.RecordDeclaration:
                    case SyntaxKind.UnionDeclaration:
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.DelegateDeclaration:
                        if (TryGetDeclarationName(child, out var typeName))
                        {
                            scope.DeclareType(typeName);
                            if (child.Kind == SyntaxKind.UnionDeclaration)
                            {
                                scope.DeclareUnion(typeName, GetUnionCases(child));
                            }

                            if (child.Kind == SyntaxKind.RecordDeclaration)
                            {
                                scope.DeclareRecordShape(typeName, GetRecordShape(typeName, child).Members);
                            }

                            if (child.Kind == SyntaxKind.EnumDeclaration)
                            {
                                scope.DeclareEnum(typeName, GetEnumMembers(child));
                            }
                        }

                        break;

                    case SyntaxKind.FunctionDeclaration:
                        if (TryGetDeclarationName(child, out var functionName) &&
                            TryGetDirectTypeAnnotation(child, out var functionReturnTypeNode) &&
                            TryGetType(functionReturnTypeNode, scope, out var functionReturnType))
                        {
                            scope.DeclareFunction(functionName, functionReturnType, GetFunctionCapabilities(child));
                        }

                        break;

                    case SyntaxKind.ValueDeclaration:
                    case SyntaxKind.LiteralDeclaration:
                        if (!TryGetDeclarationName(child, out var valueName))
                        {
                            break;
                        }

                        if (TryGetDirectTypeAnnotation(child, out var valueTypeNode) &&
                            TryGetType(valueTypeNode, scope, out var valueType))
                        {
                            scope.DeclareValue(valueName, valueType);
                            if (TryGetFunctionReturnType(valueTypeNode, out var valueFunctionReturnType))
                            {
                                scope.DeclareFunction(valueName, valueFunctionReturnType);
                            }

                            break;
                        }

                        if (child.Kind == SyntaxKind.LiteralDeclaration &&
                            child.Children.FirstOrDefault(grandchild => grandchild.Kind == SyntaxKind.Initializer) is { } initializer &&
                            initializer.Children.FirstOrDefault(grandchild => !grandchild.IsToken) is { Kind: SyntaxKind.LiteralExpression } literalExpression)
                        {
                            var inferredType = _inference.InferLiteral(literalExpression);
                            if (inferredType.IsKnown)
                            {
                                scope.DeclareValue(valueName, inferredType);
                            }
                        }

                        break;
                }
            }
        }

        private void CheckTopLevelDeclaration(SyntaxNode node, TypeScope scope)
        {
            switch (node.Kind)
            {
                case SyntaxKind.TypeAliasDeclaration:
                    CheckTypeAliasDeclaration(node, scope);
                    break;

                case SyntaxKind.FunctionDeclaration:
                    CheckFunction(node, scope);
                    break;

                case SyntaxKind.RecordDeclaration:
                case SyntaxKind.EnumDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.DelegateDeclaration:
                    CheckGenericConstraints(node);
                    foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
                    {
                        CheckGenericConstraints(function);
                    }
                    break;

                case SyntaxKind.ExtensionDeclaration:
                    CheckExtensionDeclaration(node, scope);
                    break;

                case SyntaxKind.ModuleDeclaration:
                    CheckModuleDeclaration(node, scope);
                    break;

                case SyntaxKind.ValueDeclaration:
                case SyntaxKind.LiteralDeclaration:
                    CheckPublicValueBoundary(node, scope);
                    CheckValueDeclaration(node, scope);
                    break;
            }
        }

        private void CheckModuleDeclaration(SyntaxNode node, TypeScope parentScope)
        {
            var scope = new TypeScope(parentScope);
            CollectTopLevelTypesAndFunctions(node, scope);

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                CheckTopLevelDeclaration(child, scope);
            }
        }

        private void CheckTypeAliasDeclaration(SyntaxNode node, TypeScope scope)
        {
            if (!IsPublicBoundaryDeclaration(node) || !TryGetTypeAliasTarget(node, out var target))
            {
                return;
            }

            ReportPublicBoundaryLeaks(target, scope);
        }

        private void CheckFunction(SyntaxNode node, TypeScope parentScope)
        {
            CheckGenericConstraints(node);

            var scope = new TypeScope(parentScope, allowedCapabilities: GetFunctionCapabilities(node));
            CheckFunctionDynamicCapability(node, scope);
            foreach (var parameter in node.Children.Where(child => child.Kind == SyntaxKind.ParameterList).SelectMany(child => child.Children).Where(child => child.Kind == SyntaxKind.Parameter))
            {
                if (TryGetFirstIdentifier(parameter, out var parameterIdentifier) &&
                    TryGetDirectTypeAnnotation(parameter, out var parameterTypeNode) &&
                    TryGetType(parameterTypeNode, scope, out var parameterType))
                {
                    scope.DeclareValue(parameterIdentifier.Text ?? string.Empty, parameterType);
                }
            }

            if (IsPublicBoundaryDeclaration(node))
            {
                CheckFunctionPublicBoundary(node, scope);
            }

            var expectedReturnType = SimpleType.Unknown;
            var expectedReturnTypeKnown =
                TryGetDirectTypeAnnotation(node, out var returnTypeNode) &&
                TryGetType(returnTypeNode, scope, out expectedReturnType);
            var comparisonReturnType = expectedReturnType;
            if (expectedReturnTypeKnown && IsAsyncFunction(node) && TryGetTaskResultType(expectedReturnType, out var asyncResultType))
            {
                comparisonReturnType = asyncResultType;
            }

            foreach (var body in node.Children.Where(child => child.Kind == SyntaxKind.FunctionBody))
            {
                var actualReturnType = CheckFunctionBody(body, scope, expectedReturnTypeKnown ? comparisonReturnType : null);
                if (expectedReturnTypeKnown && actualReturnType.IsKnown && IsNullabilityViolation(comparisonReturnType, actualReturnType))
                {
                    ReportNullabilityViolation(
                        body,
                        actualReturnType.IsNull
                            ? $"Cannot return null from function returning non-null type '{comparisonReturnType}'."
                            : $"Cannot return nullable expression of type '{actualReturnType}' from function returning non-null type '{comparisonReturnType}'.");
                }
                else if (expectedReturnTypeKnown && actualReturnType.IsKnown && TryGetStructuralAssignmentDiagnostic(scope, comparisonReturnType, actualReturnType, out var structuralMessage))
                {
                    ReportMismatch(body, structuralMessage);
                }
                else if (expectedReturnTypeKnown && actualReturnType.IsKnown && !CanAssign(scope, comparisonReturnType, actualReturnType))
                {
                    ReportMismatch(
                        body,
                        $"Cannot return expression of type '{actualReturnType}' from function returning '{expectedReturnType}'.");
                }
            }
        }

        private void CheckExtensionDeclaration(SyntaxNode node, TypeScope scope)
        {
            var receiverTypeNode = node.Children.FirstOrDefault(child => IsTypeSyntax(child.Kind));
            _ = receiverTypeNode is not null && TryGetType(receiverTypeNode, scope, out var receiverType);

            foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
            {
                var firstParameter = function.Children
                    .Where(child => child.Kind == SyntaxKind.ParameterList)
                    .SelectMany(child => child.Children)
                    .FirstOrDefault(child => child.Kind == SyntaxKind.Parameter);

                if (receiverTypeNode is null || !TryGetType(receiverTypeNode, scope, out receiverType))
                {
                    ReportMismatch(node, "Extension declaration requires a receiver type.");
                }
                else if (firstParameter is null ||
                    !TryGetDirectTypeAnnotation(firstParameter, out var parameterTypeNode) ||
                    !TryGetType(parameterTypeNode, scope, out var parameterType))
                {
                    ReportMismatch(function, $"Extension method requires a first receiver parameter of type '{receiverType}'.");
                }
                else if (parameterType.IsKnown &&
                    receiverType.IsKnown &&
                    (!string.Equals(parameterType.Name, receiverType.Name, StringComparison.Ordinal) ||
                        parameterType.IsNullable != receiverType.IsNullable ||
                        parameterType.IsNull != receiverType.IsNull))
                {
                    ReportMismatch(function, $"Extension method first parameter must match receiver type '{receiverType}', but found '{parameterType}'.");
                }

                CheckFunction(function, scope);
            }
        }

        private void CheckGenericConstraints(SyntaxNode node)
        {
            foreach (var constraintItem in node.Children
                .Where(child => child.Kind == SyntaxKind.WhereClause)
                .SelectMany(child => child.Children)
                .Where(child => child.Kind == SyntaxKind.GenericConstraint)
                .SelectMany(child => child.Children)
                .Where(child => child.Kind == SyntaxKind.ConstraintItem))
            {
                var token = constraintItem.Children.FirstOrDefault(child =>
                    child.IsToken
                    && child.Kind == SyntaxKind.IdentifierToken
                    && string.Equals(child.Text, "notnull", StringComparison.Ordinal));
                if (token is null)
                {
                    continue;
                }

                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.UnsupportedGenericConstraint.Code,
                    DiagnosticDescriptors.UnsupportedGenericConstraint.DefaultSeverity,
                    "Generic constraint 'notnull' cannot be lowered by the C# 7.3 backend.",
                    _file,
                    token.Span));
            }
        }

        private void CheckFunctionPublicBoundary(SyntaxNode node, TypeScope scope)
        {
            foreach (var parameter in node.Children.Where(child => child.Kind == SyntaxKind.ParameterList).SelectMany(child => child.Children).Where(child => child.Kind == SyntaxKind.Parameter))
            {
                if (TryGetDirectTypeAnnotation(parameter, out var annotation))
                {
                    ReportPublicBoundaryLeaks(annotation, scope);
                }
            }

            foreach (var annotation in node.Children.Where(child => child.Kind == SyntaxKind.TypeAnnotation))
            {
                ReportPublicBoundaryLeaks(annotation, scope);
            }
        }

        private void CheckPublicValueBoundary(SyntaxNode node, TypeScope scope)
        {
            if (!IsPublicBoundaryDeclaration(node) || !TryGetDirectTypeAnnotation(node, out var annotation))
            {
                return;
            }

            ReportPublicBoundaryLeaks(annotation, scope);
        }

        private SimpleType CheckFunctionBody(SyntaxNode body, TypeScope scope, SimpleType? expectedType)
        {
            var expressionChildren = body.Children.Where(child => !child.IsToken).ToArray();
            if (expressionChildren.Length == 0)
            {
                return SimpleType.Unknown;
            }

            if (expressionChildren[0].Kind == SyntaxKind.BlockExpression)
            {
                return CheckBlock(expressionChildren[0], new TypeScope(scope), expectedType);
            }

            return CheckExpressionWithExpected(expressionChildren[^1], scope, expectedType);
        }

        private SimpleType CheckBlock(SyntaxNode node, TypeScope scope, SimpleType? expectedType = null)
        {
            var lastExpressionType = SimpleType.Unknown;
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            for (var index = 0; index < children.Length; index++)
            {
                var child = children[index];
                var isLast = index == children.Length - 1;

                if (child.Kind == SyntaxKind.ValueDeclaration)
                {
                    CheckValueDeclaration(child, scope);
                    continue;
                }

                if (child.Kind == SyntaxKind.YieldExpression)
                {
                    lastExpressionType = CheckYieldExpression(child, scope, expectedType);
                    continue;
                }

                if (child.Kind == SyntaxKind.LockStatement)
                {
                    CheckLockStatement(child, scope);
                    lastExpressionType = SimpleType.Unknown;
                    continue;
                }

                if (child.Kind == SyntaxKind.ExpressionStatement)
                {
                    var expression = child.Children.FirstOrDefault(grandchild => !grandchild.IsToken);
                    lastExpressionType = expression is null
                        ? SimpleType.Unknown
                        : CheckExpressionWithExpected(expression, scope, isLast ? expectedType : null);
                    continue;
                }

                lastExpressionType = CheckExpressionWithExpected(child, scope, isLast ? expectedType : null);
            }

            return lastExpressionType;
        }

        private void CheckValueDeclaration(SyntaxNode node, TypeScope scope)
        {
            var expectedType = SimpleType.Unknown;
            var annotationKnown =
                TryGetDirectTypeAnnotation(node, out var typeNode) &&
                TryGetType(typeNode, scope, out expectedType);
            if (annotationKnown && !scope.AllowsDynamic && ContainsDynamicType(typeNode))
            {
                ReportDynamicCapabilityRequired(typeNode);
            }

            var initializerType = SimpleType.Unknown;
            if (node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Initializer) is { } initializer)
            {
                initializerType = CheckInitializer(initializer, scope, annotationKnown ? expectedType : null);
            }

            if (annotationKnown && initializerType.IsKnown && IsNullabilityViolation(expectedType, initializerType))
            {
                ReportNullabilityViolation(
                    node,
                    initializerType.IsNull
                        ? $"Cannot assign null to non-null type '{expectedType}'."
                        : $"Cannot assign nullable expression of type '{initializerType}' to non-null type '{expectedType}'.");
            }
            else if (annotationKnown && initializerType.IsKnown && TryGetStructuralAssignmentDiagnostic(scope, expectedType, initializerType, out var structuralMessage))
            {
                ReportMismatch(node, structuralMessage);
            }
            else if (annotationKnown && initializerType.IsKnown && !CanAssign(scope, expectedType, initializerType))
            {
                ReportMismatch(
                    node,
                    $"Cannot assign expression of type '{initializerType}' to '{expectedType}'.");
            }

            if (TryGetDeclarationName(node, out var name))
            {
                var declaredType = annotationKnown ? expectedType : initializerType;
                if (declaredType.IsKnown)
                {
                    scope.DeclareValue(name, declaredType);
                    if (annotationKnown && TryGetFunctionReturnType(typeNode, out var functionReturnType))
                    {
                        scope.DeclareFunction(name, functionReturnType);
                    }
                }
            }
        }

        private SimpleType CheckInitializer(SyntaxNode node, TypeScope scope, SimpleType? expectedType = null)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return expression is null ? SimpleType.Unknown : CheckExpressionWithExpected(expression, scope, expectedType);
        }

        private SimpleType CheckExpressionWithExpected(SyntaxNode node, TypeScope scope, SimpleType? expectedType)
        {
            if (expectedType.HasValue &&
                expectedType.Value.IsKnown &&
                ((scope.ResolveTypeLevelUnion(expectedType.Value.Name, out var expectedUnion) &&
                    expectedUnion.Members.Any(member => TryGetLiteralRuntimeType(member.Type, out _))) ||
                    TryGetLiteralRuntimeType(expectedType.Value, out _)) &&
                TryGetLiteralExpressionType(node, out var literalType))
            {
                return literalType;
            }

            if (expectedType.HasValue && node.Kind == SyntaxKind.RecordExpression)
            {
                return CheckRecordExpression(node, scope, expectedType.Value);
            }

            if (expectedType.HasValue && node.Kind == SyntaxKind.CollectionExpression)
            {
                return InferCollection(node, scope, expectedType.Value);
            }

            return CheckExpression(node, scope);
        }

        private SimpleType CheckExpression(SyntaxNode node, TypeScope scope)
        {
            CheckCapabilityCall(node, scope);

            if (node.Kind == SyntaxKind.SatisfiesExpression)
            {
                return CheckSatisfiesExpression(node, scope);
            }

            if (_inference.TryInferExpression(node, scope, child => CheckExpression(child, scope), out var inferredType))
            {
                return inferredType;
            }

            return node.Kind switch
            {
                SyntaxKind.ExpressionStatement => node.Children.FirstOrDefault(child => !child.IsToken) is { } expression
                    ? CheckExpression(expression, scope)
                    : SimpleType.Unknown,
                SyntaxKind.BlockExpression => CheckBlock(node, new TypeScope(scope)),
                SyntaxKind.IfExpression => InferIf(node, scope),
                SyntaxKind.MatchExpression => InferMatch(node, scope),
                SyntaxKind.MemberAccessExpression => InferMemberAccess(node, scope),
                SyntaxKind.AwaitExpression => InferAwait(node, scope),
                SyntaxKind.IndexerExpression => InferIndexer(node, scope),
                SyntaxKind.LambdaExpression => SimpleType.Unknown,
                SyntaxKind.CollectionExpression => InferCollection(node, scope),
                SyntaxKind.SpreadElement => CheckSpreadElement(node, scope),
                SyntaxKind.RecordExpression => CheckRecordExpression(node, scope, SimpleType.Unknown),
                _ => CheckChildrenForSideEffects(node, scope)
            };
        }

        private SimpleType CheckChildrenForSideEffects(SyntaxNode node, TypeScope scope)
        {
            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                CheckExpression(child, scope);
            }

            return SimpleType.Unknown;
        }

        private SimpleType InferMemberAccess(SyntaxNode node, TypeScope scope)
        {
            var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
            var member = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            if (receiver is null || member?.Text is null)
            {
                return SimpleType.Unknown;
            }

            if (TryGetEnumMemberAccess(receiver, scope, out var enumName, out var enumMembers))
            {
                if (enumMembers.Contains(member.Text))
                {
                    return SimpleType.Named(enumName);
                }

                ReportMismatch(node, $"Enum '{enumName}' does not contain member '{member.Text}'.");
                return SimpleType.Unknown;
            }

            var receiverType = CheckExpression(receiver, scope);
            if (IsUnknownType(receiverType))
            {
                ReportUnknownAccessRequiresNarrowing(node);
                return SimpleType.Unknown;
            }

            if (!receiverType.IsKnown || !scope.ResolveShape(receiverType.Name, out var shape))
            {
                return SimpleType.Unknown;
            }

            var memberName = member.Text;
            var shapeMember = shape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.Ordinal));
            if (shapeMember.Name is null)
            {
                ReportMismatch(node, $"Type '{receiverType}' does not contain member '{memberName}'.");
                return SimpleType.Unknown;
            }

            return shapeMember.IsOptional ? shapeMember.Type.AsNullable() : shapeMember.Type;
        }

        private static bool TryGetEnumMemberAccess(
            SyntaxNode receiver,
            TypeScope scope,
            out string enumName,
            out IReadOnlyList<string> members)
        {
            enumName = string.Empty;
            members = [];
            if (receiver.Kind != SyntaxKind.IdentifierExpression ||
                !TryGetFirstIdentifier(receiver, out var identifier))
            {
                return false;
            }

            enumName = identifier.Text ?? string.Empty;
            return scope.ResolveEnum(enumName, out members);
        }

        private bool TryFindImportedEnumMembers(
            string moduleSpecifier,
            string importedName,
            out IReadOnlyList<string> members)
        {
            var enumType = _metadataAssemblies
                .SelectMany(assembly => assembly.Types)
                .FirstOrDefault(type =>
                    type.IsEnum &&
                    string.Equals(type.Namespace, moduleSpecifier, StringComparison.Ordinal) &&
                    MetadataTypeMatchesImportedName(type, importedName) &&
                    type.EnumMembers.Count > 0);
            if (enumType is not null)
            {
                members = enumType.EnumMembers;
                return true;
            }

            members = [];
            return false;
        }

        private SimpleType InferIndexer(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                return SimpleType.Unknown;
            }

            var receiverType = CheckExpression(expressions[0], scope);
            CheckExpression(expressions[1], scope);
            if (IsUnknownType(receiverType))
            {
                ReportUnknownAccessRequiresNarrowing(node);
                return SimpleType.Unknown;
            }

            if (!receiverType.IsKnown || receiverType.IsNull || !receiverType.Name.EndsWith("[]", StringComparison.Ordinal))
            {
                return SimpleType.Unknown;
            }

            var elementTypeName = receiverType.Name[..^2];
            return elementTypeName.Length == 0 ? SimpleType.Unknown : SimpleType.Named(elementTypeName);
        }

        private SimpleType CheckRecordExpression(SyntaxNode node, TypeScope scope, SimpleType expectedType)
        {
            ShapeInfo? expectedShape = null;
            if (expectedType.IsKnown && !expectedType.IsNull && scope.ResolveShape(expectedType.Name, out var resolvedShape))
            {
                expectedShape = resolvedShape;
            }

            var fields = GetRecordExpressionFieldTypes(node, scope, expectedShape);
            if (!expectedShape.HasValue)
            {
                return SimpleType.Unknown;
            }

            var shape = expectedShape.Value;
            foreach (var member in shape.Members.Where(member => !member.IsOptional))
            {
                if (!fields.TryGetValue(member.Name, out var field) || field.IsOptional)
                {
                    ReportMismatch(
                        node,
                        $"Record expression for '{expectedType}' is missing required field '{member.Name}'.");
                }
            }

            foreach (var field in fields)
            {
                var member = shape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, field.Key, StringComparison.Ordinal));
                if (member.Name is null)
                {
                    ReportMismatch(
                        field.Value.Node,
                        $"Type '{expectedType}' does not contain field '{field.Key}'.");
                    continue;
                }

                if (field.Value.Type.IsKnown && !CanAssign(scope, member.Type, field.Value.Type))
                {
                    ReportMismatch(
                        field.Value.Node,
                        $"Record expression field '{field.Key}' expects '{member.Type}' but found '{field.Value.Type}'.");
                }
            }

            return expectedType;
        }

        private Dictionary<string, RecordExpressionFieldInfo> GetRecordExpressionFieldTypes(SyntaxNode node, TypeScope scope, ShapeInfo? expectedShape)
        {
            var fields = new Dictionary<string, RecordExpressionFieldInfo>(StringComparer.Ordinal);
            foreach (var field in node.Children.Where(child => child.Kind is SyntaxKind.RecordField or SyntaxKind.RecordSpreadField))
            {
                if (field.Kind == SyntaxKind.RecordSpreadField)
                {
                    AddRecordSpreadFields(field, scope, expectedShape, fields);
                    continue;
                }

                var name = field.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                var expression = field.Children.LastOrDefault(child => !child.IsToken);
                var type = expression is null
                    ? scope.ResolveValue(name, out var valueType) ? valueType : SimpleType.Unknown
                    : CheckExpression(expression, scope);
                fields[name] = new RecordExpressionFieldInfo(field, type, IsOptional: false);
            }

            return fields;
        }

        private void AddRecordSpreadFields(
            SyntaxNode spreadField,
            TypeScope scope,
            ShapeInfo? expectedShape,
            Dictionary<string, RecordExpressionFieldInfo> fields)
        {
            var expression = spreadField.Children.FirstOrDefault(child => !child.IsToken);
            var spreadType = expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);
            if (!spreadType.IsKnown)
            {
                if (expectedShape.HasValue)
                {
                    ReportMismatch(spreadField, "Record spread expects a nominal record value, but found 'unknown'.");
                }

                return;
            }

            if (!scope.ResolveRecordShape(spreadType.Name, out var spreadShape))
            {
                if (expectedShape.HasValue)
                {
                    ReportMismatch(spreadField, $"Record spread expects a nominal record value, but found '{spreadType}'.");
                }

                return;
            }

            var expectedMembers = expectedShape.HasValue
                ? new HashSet<string>(expectedShape.Value.Members.Select(member => member.Name), StringComparer.Ordinal)
                : null;

            foreach (var member in spreadShape.Members)
            {
                if (expectedMembers is not null && !expectedMembers.Contains(member.Name))
                {
                    continue;
                }

                fields[member.Name] = new RecordExpressionFieldInfo(spreadField, member.Type, member.IsOptional);
            }
        }

        private SimpleType CheckSatisfiesExpression(SyntaxNode node, TypeScope scope)
        {
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            var expression = children.FirstOrDefault();
            var targetTypeNode = children.Length > 1 ? children[^1] : null;
            var expressionType = expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);

            if (targetTypeNode is null ||
                !TryGetType(targetTypeNode, scope, out var targetType) ||
                !targetType.IsKnown ||
                !expressionType.IsKnown)
            {
                return expressionType;
            }

            if (IsNullabilityViolation(targetType, expressionType))
            {
                ReportNullabilityViolation(
                    node,
                    $"Expression of type '{expressionType}' does not satisfy non-null type '{targetType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, targetType, expressionType, out var message))
            {
                ReportMismatch(node, message);
            }
            else if (!CanAssign(scope, targetType, expressionType))
            {
                ReportMismatch(node, $"Expression of type '{expressionType}' does not satisfy '{targetType}'.");
            }

            return expressionType;
        }

        private SimpleType CheckYieldExpression(SyntaxNode node, TypeScope scope, SimpleType? expectedType)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            var actualType = expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);
            if (!expectedType.HasValue)
            {
                return SimpleType.Unknown;
            }

            if (!TryGetIteratorElementType(expectedType.Value, out var elementType))
            {
                ReportMismatch(node, "Yield expression requires a function returning 'IEnumerable<T>' or 'IEnumerator<T>'.");
                return SimpleType.Unknown;
            }

            if (actualType.IsKnown && IsNullabilityViolation(elementType, actualType))
            {
                ReportNullabilityViolation(
                    node,
                    actualType.IsNull
                        ? $"Cannot yield null for iterator element type '{elementType}'."
                        : $"Cannot yield nullable expression of type '{actualType}' for iterator element type '{elementType}'.");
            }
            else if (actualType.IsKnown && TryGetStructuralAssignmentDiagnostic(scope, elementType, actualType, out var structuralMessage))
            {
                ReportMismatch(node, structuralMessage);
            }
            else if (actualType.IsKnown && !CanAssign(scope, elementType, actualType))
            {
                ReportMismatch(
                    node,
                    $"Yield expression of type '{actualType}' is not assignable to iterator element type '{elementType}'.");
            }

            return expectedType.Value;
        }

        private void CheckLockStatement(SyntaxNode node, TypeScope scope)
        {
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            var gateExpression = children.FirstOrDefault(child => child.Kind != SyntaxKind.BlockExpression);
            var body = children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
            var gateType = gateExpression is null ? SimpleType.Unknown : CheckExpression(gateExpression, scope);

            if (gateType.IsKnown && !IsLockableType(gateType))
            {
                ReportMismatch(node, $"Lock expression requires a non-null reference type, but found '{gateType}'.");
            }

            if (body is not null)
            {
                CheckBlock(body, new TypeScope(scope));
            }
        }

        private SimpleType InferCollection(SyntaxNode node, TypeScope scope, SimpleType? expectedType = null)
        {
            var elementTypes = new List<SimpleType>();
            foreach (var element in node.Children.Where(child => !child.IsToken))
            {
                elementTypes.Add(CheckCollectionElement(element, scope));
            }

            var knownElementTypes = elementTypes.Where(type => type.IsKnown).ToArray();
            if (knownElementTypes.Length == 0)
            {
                return expectedType.HasValue && TryGetCollectionElementType(expectedType.Value, out _)
                    ? expectedType.Value
                    : SimpleType.Unknown;
            }

            var elementType = knownElementTypes[0];
            foreach (var actual in knownElementTypes.Skip(1))
            {
                if (!string.Equals(elementType.Name, actual.Name, StringComparison.Ordinal) ||
                    elementType.IsNullable != actual.IsNullable ||
                    elementType.IsNull != actual.IsNull)
                {
                    ReportMismatch(
                        node,
                        $"Collection expression elements must have a consistent type. Expected '{elementType}' but found '{actual}'.");
                    return SimpleType.Unknown;
                }
            }

            if (elementType.IsNull)
            {
                return SimpleType.Unknown;
            }

            if (expectedType.HasValue &&
                TryGetCollectionElementType(expectedType.Value, out var expectedElementType))
            {
                if (IsNullabilityViolation(expectedElementType, elementType) ||
                    !CanAssign(scope, expectedElementType, elementType))
                {
                    ReportMismatch(
                        node,
                        $"Collection expression element expects '{expectedElementType}' but found '{elementType}'.");
                    return SimpleType.Unknown;
                }

                return expectedType.Value;
            }

            return SimpleType.Named($"{elementType.Name}[]");
        }

        private SimpleType CheckCollectionElement(SyntaxNode element, TypeScope scope) =>
            element.Kind == SyntaxKind.SpreadElement
                ? CheckSpreadElement(element, scope)
                : CheckExpression(element, scope);

        private SimpleType CheckSpreadElement(SyntaxNode node, TypeScope scope)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            var spreadType = expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);
            if (!spreadType.IsKnown)
            {
                return SimpleType.Unknown;
            }

            if (!TryGetCollectionElementType(spreadType, out var elementType))
            {
                ReportMismatch(node, $"Spread element expects an array or List<T>, but found '{spreadType}'.");
                return SimpleType.Unknown;
            }

            return elementType;
        }

        private static bool TryGetCollectionElementType(SimpleType collectionType, out SimpleType elementType)
        {
            elementType = SimpleType.Unknown;
            if (!collectionType.IsKnown || collectionType.IsNull)
            {
                return false;
            }

            if (collectionType.Name.EndsWith("[]", StringComparison.Ordinal))
            {
                elementType = SimpleType.Named(collectionType.Name[..^2]);
                return true;
            }

            if (TryGetSingleGenericArgument(collectionType.Name, out var typeName, out var argument) &&
                (string.Equals(typeName, "List", StringComparison.Ordinal) ||
                 string.Equals(typeName, "System.Collections.Generic.List", StringComparison.Ordinal)))
            {
                elementType = GetTypeFromGenericArgument(argument);
                return true;
            }

            return false;
        }

        private static bool TryGetIteratorElementType(SimpleType iteratorType, out SimpleType elementType)
        {
            elementType = SimpleType.Unknown;
            if (!iteratorType.IsKnown || iteratorType.IsNull)
            {
                return false;
            }

            if (!TryGetSingleGenericArgument(iteratorType.Name, out var typeName, out var argument))
            {
                return false;
            }

            var unqualifiedTypeName = GetUnqualifiedTypeName(typeName);
            if (!string.Equals(unqualifiedTypeName, "IEnumerable", StringComparison.Ordinal) &&
                !string.Equals(unqualifiedTypeName, "IEnumerator", StringComparison.Ordinal))
            {
                return false;
            }

            elementType = GetTypeFromGenericArgument(argument);
            return elementType.IsKnown;
        }

        private static SimpleType GetTypeFromGenericArgument(string argument)
        {
            var trimmed = argument.Trim();
            if (trimmed.Length == 0)
            {
                return SimpleType.Unknown;
            }

            if (trimmed.EndsWith("?", StringComparison.Ordinal))
            {
                return SimpleType.Named(trimmed[..^1]).AsNullable();
            }

            return SimpleType.Named(trimmed);
        }

        private static bool IsLockableType(SimpleType type)
        {
            if (!type.IsKnown || type.IsNull || type.IsNullable)
            {
                return false;
            }

            return type.Name is not (
                "bool" or
                "byte" or
                "char" or
                "decimal" or
                "double" or
                "float" or
                "int" or
                "long" or
                "sbyte" or
                "short" or
                "uint" or
                "ulong" or
                "ushort" or
                "unit" or
                "void");
        }

        private SimpleType InferAwait(SyntaxNode node, TypeScope scope)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            if (expression is null)
            {
                return SimpleType.Unknown;
            }

            var expressionType = CheckExpression(expression, scope);
            return TryGetTaskResultType(expressionType, out var resultType) ? resultType : SimpleType.Unknown;
        }

        private SimpleType InferIf(SyntaxNode node, TypeScope scope)
        {
            var condition = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.BlockExpression && child.Kind != SyntaxKind.ElseClause);
            BranchNarrowing? thenNarrowing = null;
            BranchNarrowing? elseNarrowing = null;
            if (condition is not null &&
                TryGetDiscriminantNarrowing(condition, scope, out var conditionThenNarrowing, out var conditionElseNarrowing))
            {
                thenNarrowing = conditionThenNarrowing;
                elseNarrowing = conditionElseNarrowing;
            }

            foreach (var child in node.Children.Where(child => !child.IsToken && child.Kind != SyntaxKind.BlockExpression && child.Kind != SyntaxKind.ElseClause))
            {
                CheckExpression(child, scope);
            }

            var branchTypes = new List<SimpleType>();
            foreach (var branch in node.Children.Where(child => child.Kind == SyntaxKind.BlockExpression))
            {
                branchTypes.Add(CheckBlock(branch, CreateBranchScope(scope, thenNarrowing)));
            }

            foreach (var elseClause in node.Children.Where(child => child.Kind == SyntaxKind.ElseClause))
            {
                foreach (var branch in elseClause.Children.Where(child => !child.IsToken))
                {
                    branchTypes.Add(CheckExpression(branch, CreateBranchScope(scope, elseNarrowing)));
                }
            }

            return MergeBranchTypes(branchTypes);
        }

        private static TypeScope CreateBranchScope(TypeScope parent, BranchNarrowing? narrowing)
        {
            var scope = new TypeScope(parent);
            if (narrowing.HasValue)
            {
                scope.DeclareValue(narrowing.Value.VariableName, narrowing.Value.Type);
            }

            return scope;
        }

        private bool TryGetDiscriminantNarrowing(
            SyntaxNode condition,
            TypeScope scope,
            out BranchNarrowing? thenNarrowing,
            out BranchNarrowing? elseNarrowing)
        {
            thenNarrowing = null;
            elseNarrowing = null;

            condition = UnwrapParenthesized(condition);
            if (condition.Kind != SyntaxKind.BinaryExpression)
            {
                return false;
            }

            var operatorToken = condition.Children.FirstOrDefault(child =>
                child.IsToken && child.Kind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken);
            if (operatorToken is null)
            {
                return false;
            }

            var expressions = condition.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length != 2)
            {
                return false;
            }

            if (!TryGetDiscriminantComparison(expressions[0], expressions[1], out var variableName, out var memberName, out var literalType) &&
                !TryGetDiscriminantComparison(expressions[1], expressions[0], out variableName, out memberName, out literalType))
            {
                return false;
            }

            if (!scope.ResolveValue(variableName, out var inputType) ||
                !inputType.IsKnown ||
                !scope.ResolveTypeLevelUnion(inputType.Name, out var union))
            {
                return false;
            }

            var matchedMembers = new List<TypeLevelUnionMemberInfo>();
            var unmatchedMembers = new List<TypeLevelUnionMemberInfo>();
            foreach (var member in union.Members)
            {
                if (!member.Type.IsKnown ||
                    !scope.ResolveShape(member.Type.Name, out var shape))
                {
                    return false;
                }

                var shapeMember = shape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.Ordinal));
                if (shapeMember.Name is null ||
                    shapeMember.IsOptional ||
                    !TryGetLiteralRuntimeType(shapeMember.Type, out _))
                {
                    return false;
                }

                if (shapeMember.Type.Name == literalType.Name)
                {
                    matchedMembers.Add(member);
                }
                else
                {
                    unmatchedMembers.Add(member);
                }
            }

            if (matchedMembers.Count == 0)
            {
                ReportMismatch(
                    condition,
                    $"Discriminant member '{memberName}' on type-level union '{union.Name}' has no member with literal type {literalType}.");
                return false;
            }

            if (unmatchedMembers.Count == 0)
            {
                return false;
            }

            var equalityCheck = operatorToken.Kind == SyntaxKind.EqualsEqualsToken;
            var trueMembers = equalityCheck ? matchedMembers : unmatchedMembers;
            var falseMembers = equalityCheck ? unmatchedMembers : matchedMembers;
            if (trueMembers.Count == 1)
            {
                thenNarrowing = new BranchNarrowing(variableName, trueMembers[0].Type);
            }

            if (falseMembers.Count == 1)
            {
                elseNarrowing = new BranchNarrowing(variableName, falseMembers[0].Type);
            }

            return thenNarrowing.HasValue || elseNarrowing.HasValue;
        }

        private static bool TryGetDiscriminantComparison(
            SyntaxNode memberCandidate,
            SyntaxNode literalCandidate,
            out string variableName,
            out string memberName,
            out SimpleType literalType)
        {
            literalType = SimpleType.Unknown;
            literalCandidate = UnwrapParenthesized(literalCandidate);
            return TryGetDiscriminantAccess(memberCandidate, out variableName, out memberName) &&
                TryGetLiteralExpressionType(literalCandidate, out literalType);
        }

        private static bool TryGetDiscriminantAccess(SyntaxNode node, out string variableName, out string memberName)
        {
            variableName = string.Empty;
            memberName = string.Empty;

            node = UnwrapParenthesized(node);
            if (node.Kind != SyntaxKind.MemberAccessExpression)
            {
                return false;
            }

            var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
            if (receiver is null ||
                UnwrapParenthesized(receiver).Kind != SyntaxKind.IdentifierExpression ||
                !TryGetFirstIdentifier(UnwrapParenthesized(receiver), out var variableIdentifier))
            {
                return false;
            }

            var memberIdentifier = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            variableName = variableIdentifier.Text ?? string.Empty;
            memberName = memberIdentifier?.Text ?? string.Empty;
            return variableName.Length > 0 && memberName.Length > 0;
        }

        private static SyntaxNode UnwrapParenthesized(SyntaxNode node)
        {
            while (node.Kind == SyntaxKind.ParenthesizedExpression &&
                node.Children.FirstOrDefault(child => !child.IsToken) is { } expression)
            {
                node = expression;
            }

            return node;
        }

        private SimpleType InferMatch(SyntaxNode node, TypeScope scope)
        {
            var input = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.MatchArm);
            var inputType = input is null ? SimpleType.Unknown : CheckExpression(input, scope);
            var branchTypes = new List<SimpleType>();

            if (IsKnownBoolType(inputType))
            {
                return InferBoolMatch(node, scope, branchTypes);
            }

            if (inputType.IsKnown && scope.ResolveUnion(inputType.Name, out var union))
            {
                var coveredCases = new HashSet<string>(StringComparer.Ordinal);
                var hasDiscardArm = false;
                foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
                {
                    var armScope = new TypeScope(scope);
                    SyntaxNode? expression = null;
                    var matchedArm = TryGetUnionArm(arm, union, out var unionCase, out var payloadName, out expression, out var isDiscard);
                    if (matchedArm)
                    {
                        if (!isDiscard)
                        {
                            if (payloadName.Length > 0 && unionCase.Parameters.Count == 1)
                            {
                                armScope.DeclareValue(payloadName, SimpleType.Named(unionCase.Parameters[0].Type));
                            }
                        }
                    }

                    var guard = GetMatchArmGuard(arm);
                    if (guard is not null)
                    {
                        CheckMatchGuard(guard, armScope);
                    }

                    if (matchedArm && guard is null)
                    {
                        if (isDiscard)
                        {
                            hasDiscardArm = true;
                        }
                        else
                        {
                            coveredCases.Add(unionCase.Name);
                        }
                    }

                    if (expression is not null)
                    {
                        branchTypes.Add(CheckExpression(expression, armScope));
                    }
                }

                var missingCases = hasDiscardArm
                    ? []
                    : union.Cases
                        .Where(unionCase => !coveredCases.Contains(unionCase.Name))
                        .Select(unionCase => unionCase.Name)
                        .ToArray();
                if (missingCases.Length > 0)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.NonExhaustiveMatch.Code,
                        DiagnosticDescriptors.NonExhaustiveMatch.DefaultSeverity,
                        $"Non-exhaustive match for union '{union.Name}'. Missing cases: {string.Join(", ", missingCases)}.",
                        _file,
                        node.Span));
                }

                return MergeBranchTypes(branchTypes);
            }

            if (inputType.IsKnown && scope.ResolveEnum(inputType.Name, out var enumMembers))
            {
                return InferEnumMatch(node, scope, inputType.Name, enumMembers, branchTypes);
            }

            if (inputType.IsKnown && scope.ResolveTypeLevelUnion(inputType.Name, out var typeLevelUnion))
            {
                if (IsLiteralTypeLevelUnion(typeLevelUnion))
                {
                    return InferLiteralTypeLevelUnionMatch(node, scope, typeLevelUnion, branchTypes);
                }

                var coveredMembers = new HashSet<string>(StringComparer.Ordinal);
                var hasDiscardArm = false;
                foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
                {
                    var armScope = new TypeScope(scope);
                    SyntaxNode? expression = null;
                    var matchedArm = TryGetTypeLevelUnionArm(arm, typeLevelUnion, out var member, out var variableName, out expression, out var isDiscard);
                    if (matchedArm)
                    {
                        if (!isDiscard)
                        {
                            if (variableName.Length > 0)
                            {
                                armScope.DeclareValue(variableName, member.Type);
                            }
                        }
                    }

                    var guard = GetMatchArmGuard(arm);
                    if (guard is not null)
                    {
                        CheckMatchGuard(guard, armScope);
                    }

                    if (matchedArm && guard is null)
                    {
                        if (isDiscard)
                        {
                            hasDiscardArm = true;
                        }
                        else
                        {
                            coveredMembers.Add(member.Type.Name);
                        }
                    }

                    if (expression is not null)
                    {
                        branchTypes.Add(CheckExpression(expression, armScope));
                    }
                }

                var missingMembers = hasDiscardArm
                    ? []
                    : typeLevelUnion.Members
                        .Where(member => !coveredMembers.Contains(member.Type.Name))
                        .Select(member => member.Type.ToString())
                        .ToArray();
                if (missingMembers.Length > 0)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.NonExhaustiveMatch.Code,
                        DiagnosticDescriptors.NonExhaustiveMatch.DefaultSeverity,
                        $"Non-exhaustive match for type-level union '{typeLevelUnion.Name}'. Missing members: {string.Join(", ", missingMembers)}.",
                        _file,
                        node.Span));
                }

                return MergeBranchTypes(branchTypes);
            }

            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                var armScope = new TypeScope(scope);
                if (TryGetTypePattern(arm, out var narrowedType, out var variableName) && variableName.Length > 0)
                {
                    armScope.DeclareValue(variableName, narrowedType);
                }

                var guard = GetMatchArmGuard(arm);
                if (guard is not null)
                {
                    CheckMatchGuard(guard, armScope);
                }

                var expression = GetMatchArmExpression(arm);
                if (expression is not null)
                {
                    branchTypes.Add(CheckExpression(expression, armScope));
                }
            }

            return MergeBranchTypes(branchTypes);
        }

        private SimpleType InferBoolMatch(SyntaxNode node, TypeScope scope, List<SimpleType> branchTypes)
        {
            var coveredCases = new HashSet<string>(StringComparer.Ordinal);
            var hasDiscardArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                var armScope = new TypeScope(scope);
                var guard = GetMatchArmGuard(arm);
                if (guard is not null)
                {
                    CheckMatchGuard(guard, armScope);
                }

                if (IsDiscardPattern(arm))
                {
                    if (guard is null)
                    {
                        hasDiscardArm = true;
                    }
                }
                else if (TryGetLiteralPatternType(arm, out var literalType, out var pattern))
                {
                    if (!TryGetLiteralRuntimeType(literalType, out var runtimeType) ||
                        !string.Equals(runtimeType.Name, "bool", StringComparison.Ordinal))
                    {
                        ReportMismatch(pattern, $"Match pattern of type '{runtimeType}' is not compatible with input type 'bool'.");
                    }
                    else if (guard is null)
                    {
                        coveredCases.Add(literalType.Name);
                    }
                }

                var expression = GetMatchArmExpression(arm);
                if (expression is not null)
                {
                    branchTypes.Add(CheckExpression(expression, armScope));
                }
            }

            var missingCases = hasDiscardArm
                ? []
                : new[] { "true", "false" }
                    .Where(value => !coveredCases.Contains(value))
                    .ToArray();
            if (missingCases.Length > 0)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.NonExhaustiveMatch.Code,
                    DiagnosticDescriptors.NonExhaustiveMatch.DefaultSeverity,
                    $"Non-exhaustive match for bool. Missing cases: {string.Join(", ", missingCases)}.",
                    _file,
                    node.Span));
            }

            return MergeBranchTypes(branchTypes);
        }

        private SimpleType InferLiteralTypeLevelUnionMatch(
            SyntaxNode node,
            TypeScope scope,
            TypeLevelUnionInfo typeLevelUnion,
            List<SimpleType> branchTypes)
        {
            var coveredMembers = new HashSet<string>(StringComparer.Ordinal);
            var memberNames = typeLevelUnion.Members.Select(member => member.Type.Name).ToArray();
            var memberSet = new HashSet<string>(memberNames, StringComparer.Ordinal);
            var hasDiscardArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                var armScope = new TypeScope(scope);
                var guard = GetMatchArmGuard(arm);
                if (guard is not null)
                {
                    CheckMatchGuard(guard, armScope);
                }

                if (IsDiscardPattern(arm))
                {
                    if (guard is null)
                    {
                        hasDiscardArm = true;
                    }
                }
                else if (TryGetLiteralPatternType(arm, out var literalType, out var pattern))
                {
                    if (!memberSet.Contains(literalType.Name))
                    {
                        ReportMismatch(pattern, $"Match pattern '{literalType}' is not part of type-level union '{typeLevelUnion.Name}'.");
                    }
                    else if (guard is null)
                    {
                        coveredMembers.Add(literalType.Name);
                    }
                }

                var expression = GetMatchArmExpression(arm);
                if (expression is not null)
                {
                    branchTypes.Add(CheckExpression(expression, armScope));
                }
            }

            var missingMembers = hasDiscardArm
                ? []
                : memberNames
                    .Where(member => !coveredMembers.Contains(member))
                    .ToArray();
            if (missingMembers.Length > 0)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.NonExhaustiveMatch.Code,
                    DiagnosticDescriptors.NonExhaustiveMatch.DefaultSeverity,
                    $"Non-exhaustive match for type-level union '{typeLevelUnion.Name}'. Missing members: {string.Join(", ", missingMembers)}.",
                    _file,
                    node.Span));
            }

            return MergeBranchTypes(branchTypes);
        }

        private SimpleType InferEnumMatch(
            SyntaxNode node,
            TypeScope scope,
            string enumName,
            IReadOnlyList<string> enumMembers,
            List<SimpleType> branchTypes)
        {
            var coveredMembers = new HashSet<string>(StringComparer.Ordinal);
            var memberSet = new HashSet<string>(enumMembers, StringComparer.Ordinal);
            var hasDiscardArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                var armScope = new TypeScope(scope);
                var guard = GetMatchArmGuard(arm);
                if (guard is not null)
                {
                    CheckMatchGuard(guard, armScope);
                }

                if (IsDiscardPattern(arm))
                {
                    if (guard is null)
                    {
                        hasDiscardArm = true;
                    }
                }
                else if (TryGetEnumPattern(arm, out var memberName, out var pattern))
                {
                    if (!memberSet.Contains(memberName))
                    {
                        ReportMismatch(pattern, $"Enum '{enumName}' does not contain member '{memberName}'.");
                    }
                    else if (guard is null)
                    {
                        coveredMembers.Add(memberName);
                    }
                }

                var expression = GetMatchArmExpression(arm);
                if (expression is not null)
                {
                    branchTypes.Add(CheckExpression(expression, armScope));
                }
            }

            var missingMembers = hasDiscardArm
                ? []
                : enumMembers
                    .Where(member => !coveredMembers.Contains(member))
                    .ToArray();
            if (missingMembers.Length > 0)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.NonExhaustiveMatch.Code,
                    DiagnosticDescriptors.NonExhaustiveMatch.DefaultSeverity,
                    $"Non-exhaustive match for enum '{enumName}'. Missing members: {string.Join(", ", missingMembers)}.",
                    _file,
                    node.Span));
            }

            return MergeBranchTypes(branchTypes);
        }

        private void CheckMatchGuard(SyntaxNode guard, TypeScope scope)
        {
            var guardType = CheckExpression(guard, scope);
            if (guardType.IsKnown &&
                (guardType.IsNull ||
                    guardType.IsNullable ||
                    !string.Equals(guardType.Name, "bool", StringComparison.Ordinal)))
            {
                ReportMismatch(guard, $"Match guard expression must be 'bool', but found '{guardType}'.");
            }
        }

        private static bool IsKnownBoolType(SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            !type.IsNullable &&
            string.Equals(type.Name, "bool", StringComparison.Ordinal);

        private static bool IsLiteralTypeLevelUnion(TypeLevelUnionInfo union) =>
            union.Members.Count > 0 &&
            union.Members.All(member => TryGetLiteralRuntimeType(member.Type, out _));

        private static SimpleType MergeBranchTypes(IReadOnlyList<SimpleType> branchTypes)
        {
            var known = branchTypes.Where(type => type.IsKnown).ToArray();
            if (known.Length == 0)
            {
                return SimpleType.Unknown;
            }

            var firstNonNull = known.FirstOrDefault(type => !type.IsNull);
            if (!firstNonNull.IsKnown)
            {
                return SimpleType.Null;
            }

            if (known.All(type => type.IsNull || type.Name == firstNonNull.Name))
            {
                return known.Any(type => type.IsNull)
                    ? firstNonNull.AsNullable()
                    : firstNonNull;
            }

            return SimpleType.Unknown;
        }

        private bool CanAssign(TypeScope scope, SimpleType expected, SimpleType actual) =>
            CanAssign(scope, expected, actual, new HashSet<string>(StringComparer.Ordinal));

        private bool CanAssign(TypeScope scope, SimpleType expected, SimpleType actual, HashSet<string> visited)
        {
            if (!expected.IsKnown || !actual.IsKnown)
            {
                return true;
            }

            if (expected.Name == "dynamic" || expected.Name == "unknown")
            {
                return true;
            }

            if (actual.IsNull)
            {
                return expected.IsNullable;
            }

            if (scope.ResolveTypeAlias(expected.Name, out var expectedAliasType))
            {
                var key = $"expected:{expected.Name}";
                if (visited.Add(key))
                {
                    var resolvedExpected = expected.IsNullable ? expectedAliasType.AsNullable() : expectedAliasType;
                    return CanAssign(scope, resolvedExpected, actual, visited);
                }
            }

            if (scope.ResolveTypeAlias(actual.Name, out var actualAliasType))
            {
                var key = $"actual:{actual.Name}";
                if (visited.Add(key))
                {
                    var resolvedActual = actual.IsNullable ? actualAliasType.AsNullable() : actualAliasType;
                    return CanAssign(scope, expected, resolvedActual, visited);
                }
            }

            if (TryGetLiteralRuntimeType(actual, out var actualLiteralRuntime) &&
                expected.Name == actualLiteralRuntime.Name)
            {
                return expected.IsNullable || !actual.IsNullable;
            }

            if (scope.ResolveTypeLevelUnion(expected.Name, out var typeLevelUnion) &&
                typeLevelUnion.Members.Any(member => CanAssign(scope, member.Type, actual, visited)))
            {
                return true;
            }

            if (scope.ResolveTypeLevelUnion(actual.Name, out var actualTypeLevelUnion) &&
                actualTypeLevelUnion.Members.All(member => CanAssign(scope, expected, member.Type, visited)))
            {
                return true;
            }

            if (scope.ResolveStructuralShape(expected.Name, out var expectedShape))
            {
                return CanAssignToShape(scope, expected, expectedShape, actual, visited);
            }

            if (CanAssignMetadataType(expected, actual))
            {
                return expected.IsNullable || !actual.IsNullable;
            }

            if (expected.Name != actual.Name)
            {
                return false;
            }

            return expected.IsNullable || !actual.IsNullable;
        }

        private bool CanAssignToShape(TypeScope scope, SimpleType expected, ShapeInfo expectedShape, SimpleType actual, HashSet<string> visited)
        {
            if (!actual.IsKnown || actual.IsNull || (actual.IsNullable && !expected.IsNullable))
            {
                return false;
            }

            if (!scope.ResolveShape(actual.Name, out var actualShape))
            {
                return false;
            }

            var key = $"{expectedShape.Name}<={actualShape.Name}";
            if (!visited.Add(key))
            {
                return true;
            }

            foreach (var expectedMember in expectedShape.Members)
            {
                var actualMember = actualShape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, expectedMember.Name, StringComparison.Ordinal));
                if (actualMember.Name is null)
                {
                    if (expectedMember.IsOptional)
                    {
                        continue;
                    }

                    return false;
                }

                if (!expectedMember.IsOptional && actualMember.IsOptional)
                {
                    return false;
                }

                if (!CanAssign(scope, expectedMember.Type, actualMember.Type, visited))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanAssignMetadataType(SimpleType expected, SimpleType actual)
        {
            if (_metadataAssemblies.Count == 0 ||
                expected.IsNull ||
                actual.IsNull ||
                (actual.IsNullable && !expected.IsNullable))
            {
                return false;
            }

            var expectedTypes = FindMetadataTypes(expected.Name);
            var actualTypes = FindMetadataTypes(actual.Name);
            if (expectedTypes.Count == 0 || actualTypes.Count == 0)
            {
                return false;
            }

            return actualTypes.Any(actualType => expectedTypes.Any(expectedType =>
                TypeSymbolSatisfiesMetadataRelation(
                    actualType,
                    expectedType,
                    new HashSet<string>(StringComparer.Ordinal))));
        }

        private bool TypeSymbolSatisfiesMetadataRelation(
            MetadataTypeSymbol actualType,
            MetadataTypeSymbol expectedType,
            HashSet<string> visited)
        {
            if (MetadataTypeNameMatches(actualType.FullName, expectedType.FullName) ||
                MetadataTypeNameMatches(actualType.Name, expectedType.FullName) ||
                MetadataTypeNameMatches(actualType.BaseTypeName, expectedType.FullName) ||
                actualType.InterfaceNames.Any(interfaceName => MetadataTypeNameMatches(interfaceName, expectedType.FullName)))
            {
                return true;
            }

            if (!visited.Add(actualType.FullName))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(actualType.BaseTypeName) &&
                FindMetadataTypes(actualType.BaseTypeName)
                    .Any(baseType => TypeSymbolSatisfiesMetadataRelation(baseType, expectedType, visited)))
            {
                return true;
            }

            foreach (var interfaceName in actualType.InterfaceNames)
            {
                if (FindMetadataTypes(interfaceName)
                    .Any(interfaceType => TypeSymbolSatisfiesMetadataRelation(interfaceType, expectedType, visited)))
                {
                    return true;
                }
            }

            return false;
        }

        private IReadOnlyList<MetadataTypeSymbol> FindMetadataTypes(string typeName) =>
            _metadataAssemblies
                .SelectMany(assembly => assembly.Types)
                .Where(type =>
                    string.Equals(type.FullName, typeName, StringComparison.Ordinal) ||
                    MetadataTypeNameMatches(type.FullName, typeName) ||
                    MetadataTypeNameMatches(type.Name, typeName))
                .ToArray();

        private static bool MetadataTypeNameMatches(string? actual, string expected)
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

        private static string StripGenericArity(string name)
        {
            var index = name.IndexOf('`', StringComparison.Ordinal);
            return index < 0 ? name : name[..index];
        }

        private bool TryGetStructuralAssignmentDiagnostic(TypeScope scope, SimpleType expected, SimpleType actual, out string message)
        {
            message = string.Empty;
            if (!expected.IsKnown ||
                !actual.IsKnown ||
                !scope.ResolveStructuralShape(expected.Name, out var expectedShape) ||
                !scope.ResolveShape(actual.Name, out var actualShape))
            {
                return false;
            }

            foreach (var expectedMember in expectedShape.Members)
            {
                var actualMember = actualShape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, expectedMember.Name, StringComparison.Ordinal));
                if (actualMember.Name is null)
                {
                    if (expectedMember.IsOptional)
                    {
                        continue;
                    }

                    message = $"Type '{actual}' is missing required member '{expectedMember.Name}' for structural type '{expected}'.";
                    return true;
                }

                if (!expectedMember.IsOptional && actualMember.IsOptional)
                {
                    message = $"Member '{expectedMember.Name}' on type '{actual}' is optional but structural type '{expected}' requires it.";
                    return true;
                }

                if (!CanAssign(scope, expectedMember.Type, actualMember.Type))
                {
                    message = $"Member '{expectedMember.Name}' on type '{actual}' has type '{actualMember.Type}', which is not assignable to structural member type '{expectedMember.Type}' on '{expected}'.";
                    return true;
                }
            }

            return false;
        }

        private static bool IsNullabilityViolation(SimpleType expected, SimpleType actual)
        {
            if (!expected.IsKnown || !actual.IsKnown || expected.Name is "dynamic" or "unknown")
            {
                return false;
            }

            if (actual.IsNull)
            {
                return !expected.IsNullable;
            }

            return expected.Name == actual.Name && actual.IsNullable && !expected.IsNullable;
        }

        private void ReportMismatch(SyntaxNode node, string message)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.TypeMismatch.Code,
                DiagnosticDescriptors.TypeMismatch.DefaultSeverity,
                message,
                _file,
                node.Span));
        }

        private void ReportNullabilityViolation(SyntaxNode node, string message)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.NullabilityContractViolation.Code,
                DiagnosticDescriptors.NullabilityContractViolation.DefaultSeverity,
                message,
                _file,
                node.Span));
        }

        private void ReportDynamicCallRequiresCapability(SyntaxNode node, string functionName)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.DynamicCallRequiresCapability.Code,
                DiagnosticDescriptors.DynamicCallRequiresCapability.DefaultSeverity,
                $"Call to dynamic function '{functionName}' requires a 'dynamic' function modifier on the containing function.",
                _file,
                node.Span));
        }

        private void ReportUnknownAccessRequiresNarrowing(SyntaxNode node)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnknownAccessRequiresNarrowing.Code,
                DiagnosticDescriptors.UnknownAccessRequiresNarrowing.DefaultSeverity,
                DiagnosticDescriptors.UnknownAccessRequiresNarrowing.MessageTemplate,
                _file,
                node.Span));
        }

        private void ReportCapabilityCallRequiresMarker(SyntaxNode node, string functionName, string capability)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.CapabilityCallRequiresMarker.Code,
                DiagnosticDescriptors.CapabilityCallRequiresMarker.DefaultSeverity,
                $"Call to {capability} function '{functionName}' requires a '{capability}' function modifier on the containing function.",
                _file,
                node.Span));
        }

        private void ReportDynamicCapabilityRequired(SyntaxNode node)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.DynamicCapabilityRequired.Code,
                DiagnosticDescriptors.DynamicCapabilityRequired.DefaultSeverity,
                DiagnosticDescriptors.DynamicCapabilityRequired.MessageTemplate,
                _file,
                node.Span));
        }

        private void CheckFunctionDynamicCapability(SyntaxNode node, TypeScope scope)
        {
            if (scope.AllowsDynamic)
            {
                return;
            }

            foreach (var parameter in node.Children.Where(child => child.Kind == SyntaxKind.ParameterList).SelectMany(child => child.Children).Where(child => child.Kind == SyntaxKind.Parameter))
            {
                if (TryGetDirectTypeAnnotation(parameter, out var annotation) && ContainsDynamicType(annotation))
                {
                    ReportDynamicCapabilityRequired(annotation);
                }
            }

            foreach (var annotation in node.Children.Where(child => child.Kind == SyntaxKind.TypeAnnotation))
            {
                if (ContainsDynamicType(annotation))
                {
                    ReportDynamicCapabilityRequired(annotation);
                }
            }
        }

        private void CheckCapabilityCall(SyntaxNode node, TypeScope scope)
        {
            if (node.Kind == SyntaxKind.CallExpression &&
                TryGetDirectCallName(node, out var callName) &&
                scope.ResolveFunctionInfo(callName, out var function))
            {
                ReportCapabilityMismatch(node, scope, callName, function.Capabilities);
                return;
            }

            if (node.Kind != SyntaxKind.BinaryExpression ||
                !node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.PipeGreaterToken))
            {
                return;
            }

            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2 ||
                !TryGetPipelineTargetFunctionName(expressions[1], out var targetName) ||
                !scope.ResolveFunctionInfo(targetName, out var targetFunction))
            {
                return;
            }

            ReportCapabilityMismatch(expressions[1], scope, targetName, targetFunction.Capabilities);
        }

        private void ReportCapabilityMismatch(SyntaxNode node, TypeScope scope, string functionName, FunctionCapabilities capabilities)
        {
            if (capabilities.RequiresDynamic && !scope.AllowsDynamic)
            {
                ReportDynamicCallRequiresCapability(node, functionName);
                return;
            }

            if (capabilities.RequiresReflect && !scope.AllowsReflect)
            {
                ReportCapabilityCallRequiresMarker(node, functionName, "reflect");
                return;
            }

            if (capabilities.RequiresInterop && !scope.AllowsInterop)
            {
                ReportCapabilityCallRequiresMarker(node, functionName, "interop");
                return;
            }

            if (capabilities.RequiresUnsafe && !scope.AllowsUnsafe)
            {
                ReportCapabilityCallRequiresMarker(node, functionName, "unsafe");
            }
        }

        private void ReportPublicBoundaryLeaks(SyntaxNode node, TypeScope scope)
        {
            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                foreach (var child in node.Children.Where(child => !child.IsToken))
                {
                    ReportPublicBoundaryLeaks(child, scope);
                }

                return;
            }

            if (TryGetCompileTimeOnlyType(node, scope, out _))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.PublicBoundaryTypeLeak.Code,
                    DiagnosticDescriptors.PublicBoundaryTypeLeak.DefaultSeverity,
                    DiagnosticDescriptors.PublicBoundaryTypeLeak.MessageTemplate,
                    _file,
                    node.Span));
                return;
            }

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                ReportPublicBoundaryLeaks(child, scope);
            }
        }

        private static bool TryGetCompileTimeOnlyType(SyntaxNode node, TypeScope scope, out CompileTimeOnlyTypeKind kind)
        {
            kind = CompileTimeOnlyTypeKind.None;
            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                var typeNode = node.Children.FirstOrDefault(child => !child.IsToken);
                return typeNode is not null && TryGetCompileTimeOnlyType(typeNode, scope, out kind);
            }

            if (node.Kind == SyntaxKind.UnionType)
            {
                kind = CompileTimeOnlyTypeKind.TypeLevelUnion;
                return true;
            }

            if (node.Kind == SyntaxKind.RecordShapeType)
            {
                kind = CompileTimeOnlyTypeKind.StructuralShape;
                return true;
            }

            if (node.Kind == SyntaxKind.IntersectionType)
            {
                kind = CompileTimeOnlyTypeKind.IntersectionType;
                return true;
            }

            if (node.Kind is SyntaxKind.KeyofType or SyntaxKind.IndexedAccessType or SyntaxKind.LiteralType)
            {
                kind = CompileTimeOnlyTypeKind.TypeLevelUnion;
                return true;
            }

            if (node.Kind == SyntaxKind.TypeName &&
                TryGetSimpleTypeName(node, out var name) &&
                scope.ResolveCompileTimeOnlyType(name, out kind))
            {
                return true;
            }

            return false;
        }

        private bool IsPublicBoundaryDeclaration(SyntaxNode node)
        {
            if (node.Children.Any(child => child.Kind is SyntaxKind.ExportModifier or SyntaxKind.PublicModifier))
            {
                return true;
            }

            return TryGetLocalExportableDeclarationName(node, out var name) &&
                _localExportedNames.Contains(name);
        }

        private static HashSet<string> CollectLocalExportedNames(SyntaxNode root)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var declaredFunctions = root.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && TryGetDeclarationName(child, out _))
                .Select(child => TryGetDeclarationName(child, out var name) ? name : string.Empty)
                .Where(name => name.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            var declaredValues = root.Children
                .Where(child =>
                    (child.Kind is SyntaxKind.ValueDeclaration or SyntaxKind.LiteralDeclaration) &&
                    (child.Kind != SyntaxKind.ValueDeclaration || !IsFunctionValueDeclaration(child) || HasFunctionTypeAnnotation(child) || HasLambdaInitializer(child)) &&
                    TryGetDeclarationName(child, out _))
                .Select(child => TryGetDeclarationName(child, out var name) ? name : string.Empty)
                .Where(name => name.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            var declaredTypes = root.Children
                .Where(child =>
                    (child.Kind is SyntaxKind.TypeAliasDeclaration or SyntaxKind.RecordDeclaration or SyntaxKind.UnionDeclaration or SyntaxKind.ClassDeclaration or SyntaxKind.InterfaceDeclaration or SyntaxKind.DelegateDeclaration) &&
                    TryGetDeclarationName(child, out _))
                .Select(child => TryGetDeclarationName(child, out var name) ? name : string.Empty)
                .Where(name => name.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var exportDeclaration in root.Children.Where(child => child.Kind is SyntaxKind.ExportNamedDeclaration or SyntaxKind.ExportTypeDeclaration))
            {
                if (HasFromSpecifier(exportDeclaration))
                {
                    continue;
                }

                if (HasExportAlias(exportDeclaration))
                {
                    if (exportDeclaration.Kind == SyntaxKind.ExportNamedDeclaration)
                    {
                        foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
                        {
                            if (exportSpecifier.IsAlias && declaredFunctions.Contains(exportSpecifier.TargetName))
                            {
                                names.Add(exportSpecifier.TargetName);
                            }
                            else if (exportSpecifier.IsAlias && declaredValues.Contains(exportSpecifier.TargetName))
                            {
                                names.Add(exportSpecifier.TargetName);
                            }
                        }
                    }
                    else if (exportDeclaration.Kind == SyntaxKind.ExportTypeDeclaration)
                    {
                        foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
                        {
                            if (exportSpecifier.IsAlias && declaredTypes.Contains(exportSpecifier.TargetName))
                            {
                                names.Add(exportSpecifier.TargetName);
                            }
                        }
                    }

                    continue;
                }

                foreach (var name in GetExportedIdentifierTexts(exportDeclaration))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private static bool TryGetLocalExportableDeclarationName(SyntaxNode node, out string name)
        {
            name = node.Kind switch
            {
                SyntaxKind.FunctionDeclaration => TryGetDeclarationName(node, out var functionName) ? functionName : string.Empty,
                SyntaxKind.ValueDeclaration => TryGetDeclarationName(node, out var valueName) ? valueName : string.Empty,
                SyntaxKind.LiteralDeclaration => TryGetDeclarationName(node, out var literalName) ? literalName : string.Empty,
                SyntaxKind.TypeAliasDeclaration => TryGetDeclarationName(node, out var aliasName) ? aliasName : string.Empty,
                SyntaxKind.RecordDeclaration => TryGetDeclarationName(node, out var recordName) ? recordName : string.Empty,
                SyntaxKind.UnionDeclaration => TryGetDeclarationName(node, out var unionName) ? unionName : string.Empty,
                SyntaxKind.EnumDeclaration => TryGetDeclarationName(node, out var enumName) ? enumName : string.Empty,
                SyntaxKind.ClassDeclaration => TryGetDeclarationName(node, out var className) ? className : string.Empty,
                SyntaxKind.InterfaceDeclaration => TryGetDeclarationName(node, out var interfaceName) ? interfaceName : string.Empty,
                _ => string.Empty
            };

            return name.Length > 0;
        }

        private static bool HasFromSpecifier(SyntaxNode node) =>
            node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.FromKeyword);

        private static bool HasExportAlias(SyntaxNode node)
        {
            var insideBraces = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
                {
                    insideBraces = true;
                    continue;
                }

                if (child.IsToken && child.Kind == SyntaxKind.CloseBraceToken)
                {
                    return false;
                }

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.AsKeyword)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFunctionValueDeclaration(SyntaxNode node)
        {
            var annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            if (annotation?.Children.Any(child => child.Kind == SyntaxKind.FunctionType) == true)
            {
                return true;
            }

            return node.Children
                .Where(child => child.Kind == SyntaxKind.Initializer)
                .SelectMany(child => child.Children)
                .Any(child => child.Kind == SyntaxKind.LambdaExpression);
        }

        private static bool HasFunctionTypeAnnotation(SyntaxNode node) =>
            node.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)?
                .Children
                .Any(child => child.Kind == SyntaxKind.FunctionType) == true;

        private static bool HasLambdaInitializer(SyntaxNode node) =>
            node.Children
                .Where(child => child.Kind == SyntaxKind.Initializer)
                .SelectMany(child => child.Children)
                .Any(child => child.Kind == SyntaxKind.LambdaExpression);

        private static IEnumerable<string> GetExportedIdentifierTexts(SyntaxNode node)
        {
            var insideBraces = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
                {
                    insideBraces = true;
                    continue;
                }

                if (child.IsToken && child.Kind == SyntaxKind.CloseBraceToken)
                {
                    yield break;
                }

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } name)
                {
                    yield return name;
                }
            }
        }

        private static IEnumerable<NamedExportSpecifier> GetNamedExportSpecifiers(SyntaxNode node)
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

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } name)
                {
                    if (index + 2 < node.Children.Count &&
                        node.Children[index + 1].IsToken &&
                        node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                        node.Children[index + 2].IsToken &&
                        node.Children[index + 2].Kind == SyntaxKind.IdentifierToken &&
                        node.Children[index + 2].Text is { Length: > 0 } alias)
                    {
                        yield return new NamedExportSpecifier(name, alias);
                        index += 2;
                    }
                    else
                    {
                        yield return new NamedExportSpecifier(name, name);
                    }
                }
            }
        }

        private readonly record struct NamedExportSpecifier(string TargetName, string ExportedName)
        {
            public bool IsAlias => !string.Equals(TargetName, ExportedName, StringComparison.Ordinal);
        }

        private static bool IsAsyncFunction(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.AsyncModifier);

        private static bool IsUnknownType(SimpleType type) =>
            type.IsKnown && !type.IsNull && string.Equals(type.Name, "unknown", StringComparison.Ordinal);

        private static bool HasFunctionModifier(SyntaxNode node, SyntaxKind modifier) =>
            node.Children.Any(child => child.Kind == modifier);

        private static FunctionCapabilities GetFunctionCapabilities(SyntaxNode node) =>
            new(
                RequiresDynamic: HasFunctionModifier(node, SyntaxKind.DynamicModifier),
                RequiresReflect: HasFunctionModifier(node, SyntaxKind.ReflectModifier),
                RequiresInterop: HasFunctionModifier(node, SyntaxKind.InteropModifier),
                RequiresUnsafe: HasFunctionModifier(node, SyntaxKind.UnsafeModifier));

        private static bool TryGetTypeAliasTarget(SyntaxNode node, out SyntaxNode target)
        {
            target = node.Children.LastOrDefault(child => !child.IsToken) ?? node;
            return target.Kind is not SyntaxKind.TypeAliasDeclaration;
        }

        private static bool TryGetDirectTypeAnnotation(SyntaxNode node, out SyntaxNode annotation)
        {
            annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation) ?? node;
            return annotation.Kind == SyntaxKind.TypeAnnotation;
        }

        private static bool IsTypeSyntax(SyntaxKind kind) =>
            kind is SyntaxKind.TypeName
                or SyntaxKind.ArrayType
                or SyntaxKind.NullableType
                or SyntaxKind.FunctionType
                or SyntaxKind.UnionType
                or SyntaxKind.IntersectionType
                or SyntaxKind.KeyofType
                or SyntaxKind.IndexedAccessType
                or SyntaxKind.LiteralType
                or SyntaxKind.RecordShapeType;

        private static bool TryGetType(SyntaxNode node, out SimpleType type)
        {
            return TryGetType(node, scope: null, out type);
        }

        private static bool TryGetType(SyntaxNode node, TypeScope? scope, out SimpleType type)
        {
            type = SimpleType.Unknown;

            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                var typeNode = node.Children.FirstOrDefault(child => !child.IsToken);
                return typeNode is not null && TryGetType(typeNode, scope, out type);
            }

            if (node.Kind == SyntaxKind.NullableType)
            {
                var inner = node.Children.FirstOrDefault(child => !child.IsToken);
                if (inner is not null && TryGetType(inner, scope, out var innerType))
                {
                    type = innerType.AsNullable();
                    return true;
                }
            }

            if (node.Kind == SyntaxKind.ArrayType)
            {
                var inner = node.Children.FirstOrDefault(child => !child.IsToken);
                if (inner is not null && TryGetType(inner, scope, out var innerType))
                {
                    type = SimpleType.Named($"{innerType.Name}[]");
                    return true;
                }
            }

            if (node.Kind == SyntaxKind.KeyofType)
            {
                var target = node.Children.FirstOrDefault(child => !child.IsToken);
                if (target is not null && TryGetType(target, scope, out var targetType) && targetType.IsKnown)
                {
                    var keyofName = GetKeyofTypeName(targetType.Name);
                    type = SimpleType.Named(keyofName);
                    if (scope is not null &&
                        !scope.ResolveTypeLevelUnion(keyofName, out _) &&
                        TryGetKeyofTypeLevelUnion(keyofName, node, scope, out var keyofUnion))
                    {
                        scope.DeclareType(keyofName);
                        scope.DeclareCompileTimeOnlyType(keyofName, CompileTimeOnlyTypeKind.TypeLevelUnion);
                        scope.DeclareTypeLevelUnion(keyofName, keyofUnion.Members);
                    }

                    return true;
                }
            }

            if (node.Kind == SyntaxKind.IndexedAccessType)
            {
                return TryGetIndexedAccessType(node, scope, out type);
            }

            if (node.Kind == SyntaxKind.LiteralType)
            {
                return TryGetLiteralType(node, out type);
            }

            if (node.Kind == SyntaxKind.TypeName)
            {
                if (TryGetGenericType(node, scope, out type))
                {
                    return true;
                }

                var identifiers = node.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken).ToArray();
                var hasDot = node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.DotToken);
                if (identifiers.Length == 1 && !hasDot)
                {
                    type = SimpleType.Named(identifiers[0].Text ?? string.Empty);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetLiteralExpressionType(SyntaxNode node, out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (node.Kind != SyntaxKind.LiteralExpression)
            {
                return false;
            }

            return TryGetLiteralTokenType(node.Children.FirstOrDefault(child => child.IsToken), out type);
        }

        private static bool TryGetLiteralType(SyntaxNode node, out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (node.Kind != SyntaxKind.LiteralType)
            {
                return false;
            }

            return TryGetLiteralTokenType(node.Children.FirstOrDefault(child => child.IsToken), out type);
        }

        private static bool TryGetLiteralTokenType(SyntaxNode? token, out SimpleType type)
        {
            type = token?.Kind switch
            {
                SyntaxKind.StringLiteralToken => SimpleType.Named(token.Text ?? "\"\""),
                SyntaxKind.NumericLiteralToken => SimpleType.Named(token.Text ?? "0"),
                SyntaxKind.TrueKeyword => SimpleType.Named("true"),
                SyntaxKind.FalseKeyword => SimpleType.Named("false"),
                _ => SimpleType.Unknown
            };

            return type.IsKnown;
        }

        private static bool TryGetLiteralRuntimeType(SimpleType literalType, out SimpleType runtimeType)
        {
            runtimeType = SimpleType.Unknown;
            if (!literalType.IsKnown || literalType.IsNull)
            {
                return false;
            }

            if (literalType.Name.StartsWith("\"", StringComparison.Ordinal) &&
                literalType.Name.EndsWith("\"", StringComparison.Ordinal))
            {
                runtimeType = SimpleType.Named("string");
                return true;
            }

            if (literalType.Name is "true" or "false")
            {
                runtimeType = SimpleType.Named("bool");
                return true;
            }

            if (literalType.Name.Length > 0 && char.IsDigit(literalType.Name[0]))
            {
                runtimeType = literalType.Name.Contains('.', StringComparison.Ordinal)
                    ? SimpleType.Named("double")
                    : SimpleType.Named("int");
                return true;
            }

            return false;
        }

        private static string GetKeyofTypeName(string targetTypeName) => $"keyof {targetTypeName}";

        private static bool ContainsDynamicType(SyntaxNode node)
        {
            if (node.Kind == SyntaxKind.TypeName &&
                TryGetSimpleTypeName(node, out var name) &&
                string.Equals(name, "dynamic", StringComparison.Ordinal))
            {
                return true;
            }

            return node.Children.Any(child => !child.IsToken && ContainsDynamicType(child));
        }

        private static bool TryGetDirectCallName(SyntaxNode node, out string name)
        {
            name = string.Empty;
            if (node.Kind != SyntaxKind.CallExpression ||
                node.Children.FirstOrDefault(child => !child.IsToken) is not { Kind: SyntaxKind.IdentifierExpression } callee ||
                !TryGetFirstIdentifier(callee, out var identifier))
            {
                return false;
            }

            name = identifier.Text ?? string.Empty;
            return name.Length > 0;
        }

        private static bool TryGetPipelineTargetFunctionName(SyntaxNode target, out string name)
        {
            name = string.Empty;
            if (target.Kind == SyntaxKind.IdentifierExpression && TryGetFirstIdentifier(target, out var identifier))
            {
                name = identifier.Text ?? string.Empty;
                return name.Length > 0;
            }

            return TryGetDirectCallName(target, out name);
        }

        private static bool TryGetTypeLevelUnion(string name, SyntaxNode node, out TypeLevelUnionInfo union)
        {
            union = default;
            if (node.Kind != SyntaxKind.UnionType)
            {
                return false;
            }

            var members = new List<TypeLevelUnionMemberInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var memberNode in node.Children.Where(child => !child.IsToken))
            {
                if (!TryGetType(memberNode, out var memberType) || !memberType.IsKnown || memberType.IsNull)
                {
                    return false;
                }

                if (seen.Add(memberType.Name))
                {
                    members.Add(new TypeLevelUnionMemberInfo(memberType));
                }
            }

            if (members.Count < 2)
            {
                return false;
            }

            union = new TypeLevelUnionInfo(name, members);
            return true;
        }

        private static bool TryGetKeyofTypeLevelUnion(string name, SyntaxNode node, TypeScope scope, out TypeLevelUnionInfo union)
        {
            union = default;
            if (node.Kind != SyntaxKind.KeyofType)
            {
                return false;
            }

            var target = node.Children.FirstOrDefault(child => !child.IsToken);
            if (target is null || !TryGetKeyofShape(target, scope, out var shape))
            {
                return false;
            }

            var members = shape.Members
                .Select(member => member.Name)
                .Where(memberName => memberName.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .Select(memberName => new TypeLevelUnionMemberInfo(SimpleType.Named(ToStringLiteralTypeName(memberName))))
                .ToArray();
            if (members.Length == 0)
            {
                return false;
            }

            union = new TypeLevelUnionInfo(name, members);
            return true;
        }

        private static bool TryGetIndexedAccessTypeLevelUnion(string name, SyntaxNode node, TypeScope scope, out TypeLevelUnionInfo union)
        {
            union = default;
            if (!TryGetIndexedAccessMemberTypes(node, scope, out var memberTypes))
            {
                return false;
            }

            var members = memberTypes
                .Distinct()
                .Select(type => new TypeLevelUnionMemberInfo(type))
                .ToArray();
            if (members.Length < 2)
            {
                return false;
            }

            union = new TypeLevelUnionInfo(name, members);
            return true;
        }

        private static bool TryGetIndexedAccessType(SyntaxNode node, TypeScope? scope, out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (scope is null || !TryGetIndexedAccessMemberTypes(node, scope, out var memberTypes))
            {
                return false;
            }

            var distinctTypes = memberTypes.Distinct().ToArray();
            if (distinctTypes.Length == 1)
            {
                type = distinctTypes[0];
                return true;
            }

            if (!TryGetIndexedAccessParts(node, out var targetTypeNode, out var keyTypeNode) ||
                !TryGetType(targetTypeNode, scope, out var targetType) ||
                !TryGetIndexedAccessKeyNames(keyTypeNode, scope, out var keyNames))
            {
                return false;
            }

            var indexedAccessName = GetIndexedAccessTypeName(targetType.Name, keyNames);
            type = SimpleType.Named(indexedAccessName);
            if (!scope.ResolveTypeLevelUnion(indexedAccessName, out _))
            {
                scope.DeclareType(indexedAccessName);
                scope.DeclareCompileTimeOnlyType(indexedAccessName, CompileTimeOnlyTypeKind.TypeLevelUnion);
                scope.DeclareTypeLevelUnion(
                    indexedAccessName,
                    distinctTypes.Select(memberType => new TypeLevelUnionMemberInfo(memberType)).ToArray());
            }

            return true;
        }

        private static bool TryGetIndexedAccessMemberTypes(SyntaxNode node, TypeScope scope, out IReadOnlyList<SimpleType> memberTypes)
        {
            memberTypes = [];
            if (!TryGetIndexedAccessParts(node, out var targetTypeNode, out var keyTypeNode) ||
                !TryGetKeyofShape(targetTypeNode, scope, out var shape) ||
                !TryGetIndexedAccessKeyNames(keyTypeNode, scope, out var keyNames))
            {
                return false;
            }

            var members = new List<SimpleType>();
            foreach (var keyName in keyNames.Distinct(StringComparer.Ordinal))
            {
                var member = shape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, keyName, StringComparison.Ordinal));
                if (member.Name.Length == 0 || !member.Type.IsKnown)
                {
                    return false;
                }

                members.Add(member.Type);
            }

            memberTypes = members;
            return members.Count > 0;
        }

        private static bool TryGetIndexedAccessParts(SyntaxNode node, out SyntaxNode targetTypeNode, out SyntaxNode keyTypeNode)
        {
            targetTypeNode = node;
            keyTypeNode = node;
            if (node.Kind != SyntaxKind.IndexedAccessType)
            {
                return false;
            }

            var typeNodes = node.Children.Where(child => !child.IsToken).ToArray();
            if (typeNodes.Length < 2)
            {
                return false;
            }

            targetTypeNode = typeNodes[0];
            keyTypeNode = typeNodes[1];
            return true;
        }

        private static bool TryGetIndexedAccessKeyNames(SyntaxNode node, TypeScope scope, out IReadOnlyList<string> keyNames)
        {
            var keys = new List<string>();
            if (node.Kind == SyntaxKind.UnionType)
            {
                foreach (var memberNode in node.Children.Where(child => !child.IsToken))
                {
                    if (!TryGetIndexedAccessKeyNames(memberNode, scope, out var memberKeys))
                    {
                        keyNames = [];
                        return false;
                    }

                    keys.AddRange(memberKeys);
                }

                keyNames = keys.Distinct(StringComparer.Ordinal).ToArray();
                return keys.Count > 0;
            }

            if (!TryGetType(node, scope, out var keyType) || !keyType.IsKnown)
            {
                keyNames = [];
                return false;
            }

            if (TryGetStringLiteralTypeValue(keyType, out var directKey))
            {
                keyNames = [directKey];
                return true;
            }

            if (scope.ResolveTypeLevelUnion(keyType.Name, out var typeLevelUnion))
            {
                foreach (var member in typeLevelUnion.Members)
                {
                    if (!TryGetStringLiteralTypeValue(member.Type, out var keyName))
                    {
                        keyNames = [];
                        return false;
                    }

                    keys.Add(keyName);
                }

                keyNames = keys.Distinct(StringComparer.Ordinal).ToArray();
                return keys.Count > 0;
            }

            keyNames = [];
            return false;
        }

        private static bool TryGetStringLiteralTypeValue(SimpleType type, out string value)
        {
            value = string.Empty;
            if (!type.IsKnown ||
                type.Name.Length < 2 ||
                !type.Name.StartsWith("\"", StringComparison.Ordinal) ||
                !type.Name.EndsWith("\"", StringComparison.Ordinal))
            {
                return false;
            }

            value = type.Name[1..^1];
            return value.Length > 0;
        }

        private static string GetIndexedAccessTypeName(string targetTypeName, IReadOnlyList<string> keyNames) =>
            $"{targetTypeName}[{string.Join("|", keyNames.Select(ToStringLiteralTypeName))}]";

        private static bool TryGetKeyofShape(SyntaxNode node, TypeScope scope, out ShapeInfo shape)
        {
            if (node.Kind == SyntaxKind.RecordShapeType)
            {
                return TryGetStructuralShape(GetKeyofTypeName("anonymous"), node, scope, out shape);
            }

            if (TryGetType(node, scope, out var targetType) &&
                targetType.IsKnown &&
                scope.ResolveShape(targetType.Name, out shape))
            {
                return true;
            }

            shape = default;
            return false;
        }

        private static string ToStringLiteralTypeName(string value) => $"\"{value}\"";

        private static bool TryGetStructuralShape(string name, SyntaxNode node, TypeScope scope, out ShapeInfo shape)
        {
            shape = default;
            if (node.Kind == SyntaxKind.IntersectionType)
            {
                return TryGetIntersectionStructuralShape(name, node, scope, out shape);
            }

            if (node.Kind != SyntaxKind.RecordShapeType)
            {
                return false;
            }

            var members = new List<ShapeMemberInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in node.Children.Where(child => child.Kind == SyntaxKind.ShapeMember))
            {
                var memberName = member.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (memberName.Length == 0 || !seen.Add(memberName))
                {
                    continue;
                }

                var typeNode = member.Children.LastOrDefault(child => !child.IsToken);
                if (typeNode is null || !TryGetType(typeNode, scope, out var memberType) || !memberType.IsKnown)
                {
                    return false;
                }

                var isOptional = member.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.QuestionToken);
                members.Add(new ShapeMemberInfo(memberName, memberType, isOptional));
            }

            shape = new ShapeInfo(name, members);
            return true;
        }

        private static bool TryGetIntersectionStructuralShape(string name, SyntaxNode node, TypeScope scope, out ShapeInfo shape)
        {
            shape = default;
            var members = new List<ShapeMemberInfo>();
            var memberIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var memberNode in node.Children.Where(child => !child.IsToken))
            {
                if (!TryGetType(memberNode, scope, out var memberType) ||
                    !memberType.IsKnown ||
                    !scope.ResolveShape(memberType.Name, out var memberShape))
                {
                    return false;
                }

                foreach (var member in memberShape.Members)
                {
                    if (memberIndexes.TryGetValue(member.Name, out var existingIndex))
                    {
                        var existing = members[existingIndex];
                        if (!string.Equals(existing.Type.Name, member.Type.Name, StringComparison.Ordinal) ||
                            existing.Type.IsNullable != member.Type.IsNullable ||
                            existing.Type.IsNull != member.Type.IsNull ||
                            existing.IsOptional != member.IsOptional)
                        {
                            return false;
                        }

                        continue;
                    }

                    memberIndexes[member.Name] = members.Count;
                    members.Add(member);
                }
            }

            if (members.Count == 0)
            {
                return false;
            }

            shape = new ShapeInfo(name, members);
            return true;
        }

        private static bool TryGetGenericType(SyntaxNode node, out SimpleType type)
        {
            return TryGetGenericType(node, scope: null, out type);
        }

        private static bool TryGetGenericType(SyntaxNode node, TypeScope? scope, out SimpleType type)
        {
            type = SimpleType.Unknown;
            var baseType = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
            var argumentList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
            if (baseType is null || argumentList is null || !TryGetType(baseType, scope, out var genericType))
            {
                return false;
            }

            var arguments = new List<string>();
            foreach (var argument in argumentList.Children.Where(child => !child.IsToken))
            {
                if (!TryGetType(argument, scope, out var argumentType))
                {
                    return false;
                }

                arguments.Add(argumentType.ToString());
            }

            if (arguments.Count == 0)
            {
                return false;
            }

            type = SimpleType.Named($"{genericType.Name}<{string.Join(",", arguments)}>");
            return true;
        }

        private static bool TryGetFunctionReturnType(SyntaxNode annotation, out SimpleType returnType)
        {
            returnType = SimpleType.Unknown;

            if (annotation.Kind == SyntaxKind.TypeAnnotation)
            {
                var typeNode = annotation.Children.FirstOrDefault(child => !child.IsToken);
                return typeNode is not null && TryGetFunctionReturnType(typeNode, out returnType);
            }

            if (annotation.Kind == SyntaxKind.FunctionType)
            {
                var right = annotation.Children.LastOrDefault(child => !child.IsToken);
                return right is not null && TryGetType(right, out returnType);
            }

            return false;
        }

        private static bool TryGetTaskResultType(SimpleType type, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!type.IsKnown || type.IsNull)
            {
                return false;
            }

            if (type.Name == "Task")
            {
                resultType = SimpleType.Named("unit");
                return true;
            }

            const string prefix = "Task<";
            if (!type.Name.StartsWith(prefix, StringComparison.Ordinal) || !type.Name.EndsWith(">", StringComparison.Ordinal))
            {
                return false;
            }

            var innerName = type.Name[prefix.Length..^1];
            if (innerName.Length == 0)
            {
                return false;
            }

            var isNullable = innerName.EndsWith("?", StringComparison.Ordinal);
            if (isNullable)
            {
                innerName = innerName[..^1];
            }

            resultType = isNullable ? SimpleType.Named(innerName).AsNullable() : SimpleType.Named(innerName);
            return true;
        }

        private static IReadOnlyList<UnionCaseInfo> GetUnionCases(SyntaxNode declaration)
        {
            var cases = new List<UnionCaseInfo>();
            foreach (var unionCase in declaration.Children.Where(child => child.Kind == SyntaxKind.UnionCase))
            {
                var name = unionCase.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                cases.Add(new UnionCaseInfo(name, GetParameters(unionCase)));
            }

            return cases;
        }

        private static IReadOnlyList<string> GetEnumMembers(SyntaxNode declaration) =>
            declaration.Children
                .Where(child => child.Kind == SyntaxKind.EnumMember)
                .Select(member => member.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty)
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        private static IReadOnlyList<ParameterInfo> GetParameters(SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return [];
            }

            var parameters = new List<ParameterInfo>();
            foreach (var parameter in parameterList.Children.Where(child => child.Kind == SyntaxKind.Parameter))
            {
                var name = parameter.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (TryGetDirectTypeAnnotation(parameter, out var annotation) &&
                    TryGetType(annotation, out var type) &&
                    name.Length > 0)
                {
                    parameters.Add(new ParameterInfo(name, type.Name));
                }
            }

            return parameters;
        }

        private static ShapeInfo GetRecordShape(string name, SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return new ShapeInfo(name, []);
            }

            var members = new List<ShapeMemberInfo>();
            foreach (var parameter in parameterList.Children.Where(child => child.Kind == SyntaxKind.Parameter))
            {
                var memberName = parameter.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (memberName.Length == 0 ||
                    !TryGetDirectTypeAnnotation(parameter, out var annotation) ||
                    !TryGetType(annotation, out var memberType) ||
                    !memberType.IsKnown)
                {
                    continue;
                }

                members.Add(new ShapeMemberInfo(memberName, memberType, IsOptional: false));
            }

            return new ShapeInfo(name, members);
        }

        private static bool TryGetUnionArm(
            SyntaxNode arm,
            UnionInfo union,
            out UnionCaseInfo unionCase,
            out string payloadName,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            unionCase = default;
            payloadName = string.Empty;
            expression = GetMatchArmExpression(arm);
            isDiscard = false;

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            var caseName = pattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (caseName == "_")
            {
                isDiscard = true;
                return true;
            }

            if (caseName.Length == 0)
            {
                return false;
            }

            var foundCase = union.Cases.FirstOrDefault(candidate => string.Equals(candidate.Name, caseName, StringComparison.Ordinal));
            if (foundCase.Name is null)
            {
                return false;
            }

            unionCase = foundCase;
            var argumentPattern = pattern?
                .Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.PatternArgumentList)?
                .Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            payloadName = argumentPattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            return true;
        }

        private static bool TryGetTypeLevelUnionArm(
            SyntaxNode arm,
            TypeLevelUnionInfo union,
            out TypeLevelUnionMemberInfo member,
            out string variableName,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            member = default;
            variableName = string.Empty;
            expression = GetMatchArmExpression(arm);
            isDiscard = false;

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            variableName = pattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (variableName == "_")
            {
                isDiscard = true;
                return true;
            }

            if (!TryGetTypePattern(arm, out var narrowedType, out variableName))
            {
                return false;
            }

            var foundMember = union.Members.FirstOrDefault(candidate => candidate.Type.Name == narrowedType.Name);
            if (!foundMember.Type.IsKnown)
            {
                return false;
            }

            member = foundMember;
            return true;
        }

        private static bool TryGetTypePattern(SyntaxNode arm, out SimpleType narrowedType, out string variableName)
        {
            narrowedType = SimpleType.Unknown;
            variableName = string.Empty;

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            if (pattern is null)
            {
                return false;
            }

            variableName = pattern.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (variableName == "_")
            {
                variableName = string.Empty;
                return false;
            }

            return TryGetDirectTypeAnnotation(pattern, out var annotation) &&
                TryGetType(annotation, out narrowedType) &&
                narrowedType.IsKnown;
        }

        private static SyntaxNode? GetMatchArmExpression(SyntaxNode arm) =>
            arm.Children.LastOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.Pattern && child.Kind != SyntaxKind.RecordPattern);

        private static bool IsDiscardPattern(SyntaxNode arm) =>
            GetMatchArmPattern(arm)?
                .Children
                .FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?
                .Text == "_";

        private static bool TryGetLiteralPatternType(SyntaxNode arm, out SimpleType type, out SyntaxNode pattern)
        {
            type = SimpleType.Unknown;
            pattern = GetMatchArmPattern(arm) ?? arm;
            if (pattern.Kind != SyntaxKind.Pattern)
            {
                return false;
            }

            return TryGetLiteralTokenType(pattern.Children.FirstOrDefault(child => child.IsToken), out type);
        }

        private static bool TryGetEnumPattern(SyntaxNode arm, out string memberName, out SyntaxNode pattern)
        {
            memberName = string.Empty;
            pattern = GetMatchArmPattern(arm) ?? arm;
            if (pattern.Kind != SyntaxKind.Pattern)
            {
                return false;
            }

            var identifier = pattern.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            memberName = identifier?.Text ?? string.Empty;
            return memberName.Length > 0 && memberName != "_";
        }

        private static SyntaxNode? GetMatchArmPattern(SyntaxNode arm) =>
            arm.Children.FirstOrDefault(child => child.Kind is SyntaxKind.Pattern or SyntaxKind.RecordPattern);

        private static SyntaxNode? GetMatchArmGuard(SyntaxNode arm)
        {
            var children = arm.Children;
            for (var index = 0; index < children.Count - 1; index++)
            {
                if (children[index].IsToken && children[index].Kind == SyntaxKind.WhenKeyword)
                {
                    return children[index + 1].IsToken ? null : children[index + 1];
                }
            }

            return null;
        }

        private static bool TryGetSimpleTypeName(SyntaxNode node, out string name)
        {
            name = string.Empty;
            if (node.Kind != SyntaxKind.TypeName || node.Children.Any(child => child.Kind == SyntaxKind.TypeArgumentList))
            {
                return false;
            }

            var identifiers = node.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken).ToArray();
            var hasDot = node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.DotToken);
            if (identifiers.Length != 1 || hasDot)
            {
                return false;
            }

            name = identifiers[0].Text ?? string.Empty;
            return name.Length > 0;
        }

        private static bool TryGetDeclarationName(SyntaxNode node, out string name)
        {
            name = string.Empty;
            var seenDeclarationKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind is SyntaxKind.FunKeyword or SyntaxKind.ModuleKeyword or SyntaxKind.TypeKeyword or SyntaxKind.RecordKeyword or SyntaxKind.UnionKeyword or SyntaxKind.EnumKeyword or SyntaxKind.ClassKeyword or SyntaxKind.DelegateKeyword or SyntaxKind.LetKeyword or SyntaxKind.LiteralKeyword)
                {
                    seenDeclarationKeyword = true;
                    continue;
                }

                if (seenDeclarationKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    name = child.Text ?? string.Empty;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetSingleGenericArgument(string typeName, out string genericName, out string argument)
        {
            genericName = string.Empty;
            argument = string.Empty;

            var open = typeName.IndexOf('<', StringComparison.Ordinal);
            var close = typeName.LastIndexOf('>');
            if (open <= 0 || close <= open + 1 || close != typeName.Length - 1)
            {
                return false;
            }

            var inner = typeName.Substring(open + 1, close - open - 1).Trim();
            if (inner.Length == 0 || inner.Contains(',', StringComparison.Ordinal))
            {
                return false;
            }

            genericName = typeName[..open].Trim();
            argument = inner;
            return genericName.Length > 0;
        }

        private static bool TryGetFirstIdentifier(SyntaxNode node, out SyntaxNode identifier)
        {
            identifier = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken) ?? node;
            return identifier.IsToken && identifier.Kind == SyntaxKind.IdentifierToken;
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

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } name)
                {
                    if (index + 2 < node.Children.Count &&
                        node.Children[index + 1].IsToken &&
                        node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                        node.Children[index + 2].IsToken &&
                        node.Children[index + 2].Kind == SyntaxKind.IdentifierToken &&
                        node.Children[index + 2].Text is { Length: > 0 } alias)
                    {
                        yield return new NamedImportSpecifier(name, alias);
                        index += 2;
                    }
                    else
                    {
                        yield return new NamedImportSpecifier(name, name);
                    }
                }
            }
        }

        private static bool MetadataTypeMatchesImportedName(MetadataTypeSymbol type, string importedName) =>
            string.Equals(type.Name, importedName, StringComparison.Ordinal) ||
            string.Equals(StripGenericArity(type.Name), importedName, StringComparison.Ordinal);

        private static bool TryGetModuleSpecifier(SyntaxNode node, out string specifier)
        {
            var token = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.StringLiteralToken);
            if (TryUnquoteStringLiteral(token?.Text, out specifier))
            {
                return true;
            }

            specifier = string.Empty;
            return false;
        }

        private static bool IsRelativeSpecifier(string specifier) =>
            specifier == "." ||
            specifier == ".." ||
            specifier.StartsWith("./", StringComparison.Ordinal) ||
            specifier.StartsWith("../", StringComparison.Ordinal);

        private bool MatchesSourceAlias(string specifier) =>
            _sourceAliases.Any(alias => SourceAliasPatternMatches(alias.Pattern.Trim(), specifier));

        private bool IsSourceModuleSpecifier(string specifier) =>
            IsRelativeSpecifier(specifier) ||
            MatchesSourceAlias(specifier) ||
            _sourceModuleSpecifiers.Contains(specifier);

        private static bool SourceAliasPatternMatches(string pattern, string specifier)
        {
            var wildcard = pattern.IndexOf('*');
            if (wildcard < 0)
            {
                return string.Equals(pattern, specifier, StringComparison.Ordinal);
            }

            var prefix = pattern[..wildcard];
            var suffix = pattern[(wildcard + 1)..];
            return specifier.Length >= prefix.Length + suffix.Length &&
                specifier.StartsWith(prefix, StringComparison.Ordinal) &&
                specifier.EndsWith(suffix, StringComparison.Ordinal);
        }

        private static bool TryUnquoteStringLiteral(string? text, out string value)
        {
            value = string.Empty;
            if (string.IsNullOrEmpty(text) || text.Length < 2 || text[0] != '"' || text[^1] != '"')
            {
                return false;
            }

            value = text[1..^1];
            return value.Length > 0;
        }

        private static bool TryGetNamespaceImportAlias(SyntaxNode node, out SyntaxNode alias)
        {
            for (var index = 0; index + 1 < node.Children.Count; index++)
            {
                if (node.Children[index].IsToken &&
                    node.Children[index].Kind == SyntaxKind.AsKeyword &&
                    node.Children[index + 1].IsToken &&
                    node.Children[index + 1].Kind == SyntaxKind.IdentifierToken)
                {
                    alias = node.Children[index + 1];
                    return true;
                }
            }

            alias = node;
            return false;
        }

        private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName);
    }

    private sealed class TypeScope : ITypeSharpInferenceScope
    {
        private readonly TypeScope? _parent;
        private readonly FunctionCapabilities _allowedCapabilities;
        private readonly Dictionary<string, SimpleType> _values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, FunctionInfo> _functions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimpleType> _typeAliases = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CompileTimeOnlyTypeKind> _compileTimeOnlyTypes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeLevelUnionInfo> _typeLevelUnions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UnionInfo> _unions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IReadOnlyList<string>> _enums = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _structuralShapes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _recordShapes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);

        public TypeScope(TypeScope? parent, FunctionCapabilities allowedCapabilities = default)
        {
            _parent = parent;
            _allowedCapabilities = allowedCapabilities;
        }

        public bool AllowsDynamic => _allowedCapabilities.RequiresDynamic || (_parent?.AllowsDynamic ?? false);

        public bool AllowsReflect => _allowedCapabilities.RequiresReflect || (_parent?.AllowsReflect ?? false);

        public bool AllowsInterop => _allowedCapabilities.RequiresInterop || (_parent?.AllowsInterop ?? false);

        public bool AllowsUnsafe => _allowedCapabilities.RequiresUnsafe || (_parent?.AllowsUnsafe ?? false);

        public void DeclareValue(string name, SimpleType type) => _values[name] = type;

        public void DeclareFunction(string name, SimpleType returnType) => _functions[name] = new FunctionInfo(returnType, default);

        public void DeclareFunction(string name, SimpleType returnType, FunctionCapabilities capabilities) => _functions[name] = new FunctionInfo(returnType, capabilities);

        public void DeclareType(string name) => _types.Add(name);

        public void DeclareTypeAlias(string name, SimpleType type) => _typeAliases[name] = type;

        public void DeclareCompileTimeOnlyType(string name, CompileTimeOnlyTypeKind kind) => _compileTimeOnlyTypes[name] = kind;

        public void DeclareTypeLevelUnion(string name, IReadOnlyList<TypeLevelUnionMemberInfo> members) => _typeLevelUnions[name] = new TypeLevelUnionInfo(name, members);

        public void DeclareUnion(string name, IReadOnlyList<UnionCaseInfo> cases) => _unions[name] = new UnionInfo(name, cases);

        public void DeclareEnum(string name, IReadOnlyList<string> members) => _enums[name] = members;

        public void DeclareStructuralShape(string name, IReadOnlyList<ShapeMemberInfo> members) => _structuralShapes[name] = new ShapeInfo(name, members);

        public void DeclareRecordShape(string name, IReadOnlyList<ShapeMemberInfo> members) => _recordShapes[name] = new ShapeInfo(name, members);

        public bool ResolveValue(string name, out SimpleType type)
        {
            if (_values.TryGetValue(name, out type))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveValue(name, out type);
            }

            type = SimpleType.Unknown;
            return false;
        }

        public bool ResolveFunction(string name, out SimpleType returnType)
        {
            if (_functions.TryGetValue(name, out var function))
            {
                returnType = function.ReturnType;
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveFunction(name, out returnType);
            }

            returnType = SimpleType.Unknown;
            return false;
        }

        public bool ResolveFunctionInfo(string name, out FunctionInfo function)
        {
            if (_functions.TryGetValue(name, out function))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveFunctionInfo(name, out function);
            }

            function = default;
            return false;
        }

        public bool ResolveCompileTimeOnlyType(string name, out CompileTimeOnlyTypeKind kind)
        {
            if (_compileTimeOnlyTypes.TryGetValue(name, out kind))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveCompileTimeOnlyType(name, out kind);
            }

            kind = CompileTimeOnlyTypeKind.None;
            return false;
        }

        public bool ResolveTypeAlias(string name, out SimpleType type)
        {
            if (_typeAliases.TryGetValue(name, out type))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveTypeAlias(name, out type);
            }

            type = SimpleType.Unknown;
            return false;
        }

        public bool ResolveTypeLevelUnion(string name, out TypeLevelUnionInfo union)
        {
            if (_typeLevelUnions.TryGetValue(name, out union))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveTypeLevelUnion(name, out union);
            }

            union = default;
            return false;
        }

        public bool ResolveUnion(string name, out UnionInfo union)
        {
            if (_unions.TryGetValue(name, out union))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveUnion(name, out union);
            }

            union = default;
            return false;
        }

        public bool ResolveEnum(string name, out IReadOnlyList<string> members)
        {
            if (_enums.TryGetValue(name, out var resolvedMembers))
            {
                members = resolvedMembers;
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveEnum(name, out members);
            }

            members = [];
            return false;
        }

        public bool ResolveStructuralShape(string name, out ShapeInfo shape)
        {
            if (_structuralShapes.TryGetValue(name, out shape))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveStructuralShape(name, out shape);
            }

            shape = default;
            return false;
        }

        public bool ResolveRecordShape(string name, out ShapeInfo shape)
        {
            if (_recordShapes.TryGetValue(name, out shape))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveRecordShape(name, out shape);
            }

            shape = default;
            return false;
        }

        public bool ResolveShape(string name, out ShapeInfo shape)
        {
            if (ResolveStructuralShape(name, out shape))
            {
                return true;
            }

            return ResolveRecordShape(name, out shape);
        }

        public bool ResolveType(string name) => _types.Contains(name) || (_parent?.ResolveType(name) ?? false);
    }

    private readonly record struct UnionInfo(string Name, IReadOnlyList<UnionCaseInfo> Cases);

    private readonly record struct UnionCaseInfo(string Name, IReadOnlyList<ParameterInfo> Parameters);

    private readonly record struct ParameterInfo(string Name, string Type);

    private readonly record struct TypeLevelUnionInfo(string Name, IReadOnlyList<TypeLevelUnionMemberInfo> Members);

    private readonly record struct TypeLevelUnionMemberInfo(SimpleType Type);

    private readonly record struct ShapeInfo(string Name, IReadOnlyList<ShapeMemberInfo> Members);

    private readonly record struct ShapeMemberInfo(string Name, SimpleType Type, bool IsOptional);

    private readonly record struct RecordExpressionFieldInfo(SyntaxNode Node, SimpleType Type, bool IsOptional);

    private readonly record struct BranchNarrowing(string VariableName, SimpleType Type);

    private readonly record struct FunctionInfo(SimpleType ReturnType, FunctionCapabilities Capabilities);

    private readonly record struct FunctionCapabilities(
        bool RequiresDynamic,
        bool RequiresReflect,
        bool RequiresInterop,
        bool RequiresUnsafe);

    private enum CompileTimeOnlyTypeKind
    {
        None,
        TypeLevelUnion,
        StructuralShape,
        IntersectionType
    }

}
