using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.TypeChecking;

public static class TypeSharpTypeChecker
{
    public static TypeCheckResult Check(SyntaxNode root, string file)
    {
        var checker = new Checker(file);
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
        private readonly List<Diagnostic> _diagnostics = [];
        private readonly TypeSharpInferenceEngine _inference = new();

        public Checker(string file)
        {
            _file = file;
        }

        public TypeCheckResult Check(SyntaxNode root)
        {
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
                        foreach (var importName in GetNamedImportIdentifiers(child))
                        {
                            scope.DeclareType(importName.Text ?? string.Empty);
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

                                if (TryGetTypeLevelUnion(aliasName, aliasTarget, out var typeLevelUnion))
                                {
                                    scope.DeclareTypeLevelUnion(aliasName, typeLevelUnion.Members);
                                }

                                if (TryGetStructuralShape(aliasName, aliasTarget, out var structuralShape))
                                {
                                    scope.DeclareStructuralShape(aliasName, structuralShape.Members);
                                }
                            }
                        }

                        break;

                    case SyntaxKind.RecordDeclaration:
                    case SyntaxKind.UnionDeclaration:
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
                        }

                        break;

                    case SyntaxKind.FunctionDeclaration:
                        if (TryGetDeclarationName(child, out var functionName) &&
                            TryGetDirectTypeAnnotation(child, out var functionReturnTypeNode) &&
                            TryGetType(functionReturnTypeNode, out var functionReturnType))
                        {
                            scope.DeclareFunction(functionName, functionReturnType);
                        }

                        break;

                    case SyntaxKind.ValueDeclaration:
                    case SyntaxKind.LiteralDeclaration:
                        if (!TryGetDeclarationName(child, out var valueName))
                        {
                            break;
                        }

                        if (TryGetDirectTypeAnnotation(child, out var valueTypeNode) &&
                            TryGetType(valueTypeNode, out var valueType))
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
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.DelegateDeclaration:
                    CheckGenericConstraints(node);
                    foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
                    {
                        CheckGenericConstraints(function);
                    }
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

            var scope = new TypeScope(parentScope);
            foreach (var parameter in node.Children.Where(child => child.Kind == SyntaxKind.ParameterList).SelectMany(child => child.Children).Where(child => child.Kind == SyntaxKind.Parameter))
            {
                if (TryGetFirstIdentifier(parameter, out var parameterIdentifier) &&
                    TryGetDirectTypeAnnotation(parameter, out var parameterTypeNode) &&
                    TryGetType(parameterTypeNode, out var parameterType))
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
                TryGetType(returnTypeNode, out expectedReturnType);
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
                TryGetType(typeNode, out expectedType);

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

            var receiverType = CheckExpression(receiver, scope);
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

        private SimpleType InferIndexer(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                return SimpleType.Unknown;
            }

            var receiverType = CheckExpression(expressions[0], scope);
            CheckExpression(expressions[1], scope);
            if (!receiverType.IsKnown || receiverType.IsNull || !receiverType.Name.EndsWith("[]", StringComparison.Ordinal))
            {
                return SimpleType.Unknown;
            }

            var elementTypeName = receiverType.Name[..^2];
            return elementTypeName.Length == 0 ? SimpleType.Unknown : SimpleType.Named(elementTypeName);
        }

        private SimpleType CheckRecordExpression(SyntaxNode node, TypeScope scope, SimpleType expectedType)
        {
            var fields = GetRecordExpressionFieldTypes(node, scope);
            if (!expectedType.IsKnown || expectedType.IsNull || !scope.ResolveShape(expectedType.Name, out var shape))
            {
                return SimpleType.Unknown;
            }

            foreach (var member in shape.Members.Where(member => !member.IsOptional))
            {
                if (!fields.ContainsKey(member.Name))
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

        private Dictionary<string, RecordExpressionFieldInfo> GetRecordExpressionFieldTypes(SyntaxNode node, TypeScope scope)
        {
            var fields = new Dictionary<string, RecordExpressionFieldInfo>(StringComparer.Ordinal);
            foreach (var field in node.Children.Where(child => child.Kind == SyntaxKind.RecordField))
            {
                var name = field.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                var expression = field.Children.LastOrDefault(child => !child.IsToken);
                var type = expression is null
                    ? scope.ResolveValue(name, out var valueType) ? valueType : SimpleType.Unknown
                    : CheckExpression(expression, scope);
                fields[name] = new RecordExpressionFieldInfo(field, type);
            }

            return fields;
        }

        private SimpleType InferCollection(SyntaxNode node, TypeScope scope, SimpleType? expectedType = null)
        {
            var elementTypes = new List<SimpleType>();
            foreach (var element in node.Children.Where(child => !child.IsToken))
            {
                elementTypes.Add(CheckExpression(element, scope));
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
                elementType = SimpleType.Named(argument);
                return true;
            }

            return false;
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
            foreach (var child in node.Children.Where(child => !child.IsToken && child.Kind != SyntaxKind.BlockExpression && child.Kind != SyntaxKind.ElseClause))
            {
                CheckExpression(child, scope);
            }

            var branchTypes = new List<SimpleType>();
            foreach (var branch in node.Children.Where(child => child.Kind == SyntaxKind.BlockExpression))
            {
                branchTypes.Add(CheckBlock(branch, new TypeScope(scope)));
            }

            foreach (var elseClause in node.Children.Where(child => child.Kind == SyntaxKind.ElseClause))
            {
                foreach (var branch in elseClause.Children.Where(child => !child.IsToken))
                {
                    branchTypes.Add(CheckExpression(branch, scope));
                }
            }

            return MergeBranchTypes(branchTypes);
        }

        private SimpleType InferMatch(SyntaxNode node, TypeScope scope)
        {
            var input = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.MatchArm);
            var inputType = input is null ? SimpleType.Unknown : CheckExpression(input, scope);
            var branchTypes = new List<SimpleType>();

            if (inputType.IsKnown && scope.ResolveUnion(inputType.Name, out var union))
            {
                var coveredCases = new HashSet<string>(StringComparer.Ordinal);
                foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
                {
                    var armScope = new TypeScope(scope);
                    if (TryGetUnionArm(arm, union, out var unionCase, out var payloadName, out var expression))
                    {
                        coveredCases.Add(unionCase.Name);
                        if (payloadName.Length > 0 && unionCase.Parameters.Count == 1)
                        {
                            armScope.DeclareValue(payloadName, SimpleType.Named(unionCase.Parameters[0].Type));
                        }
                    }

                    if (expression is not null)
                    {
                        branchTypes.Add(CheckExpression(expression, armScope));
                    }
                }

                var missingCases = union.Cases
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

            if (inputType.IsKnown && scope.ResolveTypeLevelUnion(inputType.Name, out var typeLevelUnion))
            {
                var coveredMembers = new HashSet<string>(StringComparer.Ordinal);
                var hasDiscardArm = false;
                foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
                {
                    var armScope = new TypeScope(scope);
                    if (TryGetTypeLevelUnionArm(arm, typeLevelUnion, out var member, out var variableName, out var expression, out var isDiscard))
                    {
                        if (isDiscard)
                        {
                            hasDiscardArm = true;
                        }
                        else
                        {
                            coveredMembers.Add(member.Type.Name);
                            if (variableName.Length > 0)
                            {
                                armScope.DeclareValue(variableName, member.Type);
                            }
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

                var expression = GetMatchArmExpression(arm);
                if (expression is not null)
                {
                    branchTypes.Add(CheckExpression(expression, armScope));
                }
            }

            return MergeBranchTypes(branchTypes);
        }

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

        private static bool CanAssign(TypeScope scope, SimpleType expected, SimpleType actual) =>
            CanAssign(scope, expected, actual, new HashSet<string>(StringComparer.Ordinal));

        private static bool CanAssign(TypeScope scope, SimpleType expected, SimpleType actual, HashSet<string> visited)
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

            if (scope.ResolveTypeLevelUnion(expected.Name, out var typeLevelUnion) &&
                typeLevelUnion.Members.Any(member => member.Type.Name == actual.Name && (member.Type.IsNullable || !actual.IsNullable)))
            {
                return true;
            }

            if (scope.ResolveStructuralShape(expected.Name, out var expectedShape))
            {
                return CanAssignToShape(scope, expected, expectedShape, actual, visited);
            }

            if (expected.Name != actual.Name)
            {
                return false;
            }

            return expected.IsNullable || !actual.IsNullable;
        }

        private static bool CanAssignToShape(TypeScope scope, SimpleType expected, ShapeInfo expectedShape, SimpleType actual, HashSet<string> visited)
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

        private static bool TryGetStructuralAssignmentDiagnostic(TypeScope scope, SimpleType expected, SimpleType actual, out string message)
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

            if (node.Kind == SyntaxKind.TypeName &&
                TryGetSimpleTypeName(node, out var name) &&
                scope.ResolveCompileTimeOnlyType(name, out kind))
            {
                return true;
            }

            return false;
        }

        private static bool IsPublicBoundaryDeclaration(SyntaxNode node) =>
            node.Children.Any(child => child.Kind is SyntaxKind.ExportModifier or SyntaxKind.PublicModifier);

        private static bool IsAsyncFunction(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.AsyncModifier);

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

        private static bool TryGetType(SyntaxNode node, out SimpleType type)
        {
            type = SimpleType.Unknown;

            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                var typeNode = node.Children.FirstOrDefault(child => !child.IsToken);
                return typeNode is not null && TryGetType(typeNode, out type);
            }

            if (node.Kind == SyntaxKind.NullableType)
            {
                var inner = node.Children.FirstOrDefault(child => !child.IsToken);
                if (inner is not null && TryGetType(inner, out var innerType))
                {
                    type = innerType.AsNullable();
                    return true;
                }
            }

            if (node.Kind == SyntaxKind.ArrayType)
            {
                var inner = node.Children.FirstOrDefault(child => !child.IsToken);
                if (inner is not null && TryGetType(inner, out var innerType))
                {
                    type = SimpleType.Named($"{innerType.Name}[]");
                    return true;
                }
            }

            if (node.Kind == SyntaxKind.TypeName)
            {
                if (TryGetGenericType(node, out type))
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

        private static bool TryGetStructuralShape(string name, SyntaxNode node, out ShapeInfo shape)
        {
            shape = default;
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
                if (typeNode is null || !TryGetType(typeNode, out var memberType) || !memberType.IsKnown)
                {
                    return false;
                }

                var isOptional = member.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.QuestionToken);
                members.Add(new ShapeMemberInfo(memberName, memberType, isOptional));
            }

            shape = new ShapeInfo(name, members);
            return true;
        }

        private static bool TryGetGenericType(SyntaxNode node, out SimpleType type)
        {
            type = SimpleType.Unknown;
            var baseType = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
            var argumentList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
            if (baseType is null || argumentList is null || !TryGetType(baseType, out var genericType))
            {
                return false;
            }

            var arguments = new List<string>();
            foreach (var argument in argumentList.Children.Where(child => !child.IsToken))
            {
                if (!TryGetType(argument, out var argumentType))
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
            out SyntaxNode? expression)
        {
            unionCase = default;
            payloadName = string.Empty;
            expression = GetMatchArmExpression(arm);

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            var caseName = pattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
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
                if (child.IsToken && child.Kind is SyntaxKind.FunKeyword or SyntaxKind.ModuleKeyword or SyntaxKind.TypeKeyword or SyntaxKind.RecordKeyword or SyntaxKind.UnionKeyword or SyntaxKind.ClassKeyword or SyntaxKind.DelegateKeyword or SyntaxKind.LetKeyword or SyntaxKind.LiteralKeyword)
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

        private static IEnumerable<SyntaxNode> GetNamedImportIdentifiers(SyntaxNode node)
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

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    yield return child;
                }
            }
        }
    }

    private sealed class TypeScope : ITypeSharpInferenceScope
    {
        private readonly TypeScope? _parent;
        private readonly Dictionary<string, SimpleType> _values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimpleType> _functions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CompileTimeOnlyTypeKind> _compileTimeOnlyTypes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeLevelUnionInfo> _typeLevelUnions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UnionInfo> _unions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _structuralShapes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _recordShapes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);

        public TypeScope(TypeScope? parent)
        {
            _parent = parent;
        }

        public void DeclareValue(string name, SimpleType type) => _values[name] = type;

        public void DeclareFunction(string name, SimpleType returnType) => _functions[name] = returnType;

        public void DeclareType(string name) => _types.Add(name);

        public void DeclareCompileTimeOnlyType(string name, CompileTimeOnlyTypeKind kind) => _compileTimeOnlyTypes[name] = kind;

        public void DeclareTypeLevelUnion(string name, IReadOnlyList<TypeLevelUnionMemberInfo> members) => _typeLevelUnions[name] = new TypeLevelUnionInfo(name, members);

        public void DeclareUnion(string name, IReadOnlyList<UnionCaseInfo> cases) => _unions[name] = new UnionInfo(name, cases);

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
            if (_functions.TryGetValue(name, out returnType))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveFunction(name, out returnType);
            }

            returnType = SimpleType.Unknown;
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

    private readonly record struct RecordExpressionFieldInfo(SyntaxNode Node, SimpleType Type);

    private enum CompileTimeOnlyTypeKind
    {
        None,
        TypeLevelUnion,
        StructuralShape
    }

}
