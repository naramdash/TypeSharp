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
                            if (TryGetTypeAliasTarget(child, out var aliasTarget) &&
                                TryGetCompileTimeOnlyType(aliasTarget, scope, out var aliasKind))
                            {
                                scope.DeclareCompileTimeOnlyType(aliasName, aliasKind);
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
                            var inferredType = InferLiteral(literalExpression);
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

            foreach (var body in node.Children.Where(child => child.Kind == SyntaxKind.FunctionBody))
            {
                var actualReturnType = CheckFunctionBody(body, scope);
                if (expectedReturnTypeKnown && actualReturnType.IsKnown && !CanAssign(expectedReturnType, actualReturnType))
                {
                    ReportMismatch(
                        body,
                        $"Cannot return expression of type '{actualReturnType}' from function returning '{expectedReturnType}'.");
                }
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

        private SimpleType CheckFunctionBody(SyntaxNode body, TypeScope scope)
        {
            var expressionChildren = body.Children.Where(child => !child.IsToken).ToArray();
            if (expressionChildren.Length == 0)
            {
                return SimpleType.Unknown;
            }

            if (expressionChildren[0].Kind == SyntaxKind.BlockExpression)
            {
                return CheckBlock(expressionChildren[0], new TypeScope(scope));
            }

            return CheckExpression(expressionChildren[^1], scope);
        }

        private SimpleType CheckBlock(SyntaxNode node, TypeScope scope)
        {
            var lastExpressionType = SimpleType.Unknown;
            foreach (var child in node.Children)
            {
                if (child.IsToken)
                {
                    continue;
                }

                if (child.Kind == SyntaxKind.ValueDeclaration)
                {
                    CheckValueDeclaration(child, scope);
                    continue;
                }

                if (child.Kind == SyntaxKind.ExpressionStatement)
                {
                    var expression = child.Children.FirstOrDefault(grandchild => !grandchild.IsToken);
                    lastExpressionType = expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);
                    continue;
                }

                lastExpressionType = CheckExpression(child, scope);
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
                initializerType = CheckInitializer(initializer, scope);
            }

            if (annotationKnown && initializerType.IsKnown && !CanAssign(expectedType, initializerType))
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

        private SimpleType CheckInitializer(SyntaxNode node, TypeScope scope)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return expression is null ? SimpleType.Unknown : CheckExpression(expression, scope);
        }

        private SimpleType CheckExpression(SyntaxNode node, TypeScope scope)
        {
            return node.Kind switch
            {
                SyntaxKind.ExpressionStatement => node.Children.FirstOrDefault(child => !child.IsToken) is { } expression
                    ? CheckExpression(expression, scope)
                    : SimpleType.Unknown,
                SyntaxKind.LiteralExpression => InferLiteral(node),
                SyntaxKind.IdentifierExpression => InferIdentifier(node, scope),
                SyntaxKind.CallExpression => InferCall(node, scope),
                SyntaxKind.BlockExpression => CheckBlock(node, new TypeScope(scope)),
                SyntaxKind.IfExpression => InferIf(node, scope),
                SyntaxKind.BinaryExpression => InferBinary(node, scope),
                SyntaxKind.MemberAccessExpression or SyntaxKind.IndexerExpression => SimpleType.Unknown,
                SyntaxKind.LambdaExpression => SimpleType.Unknown,
                SyntaxKind.CollectionExpression => SimpleType.Unknown,
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

        private static SimpleType InferLiteral(SyntaxNode node)
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

        private SimpleType InferIdentifier(SyntaxNode node, TypeScope scope)
        {
            if (!TryGetFirstIdentifier(node, out var identifier))
            {
                return SimpleType.Unknown;
            }

            return scope.ResolveValue(identifier.Text ?? string.Empty, out var type)
                ? type
                : SimpleType.Unknown;
        }

        private SimpleType InferCall(SyntaxNode node, TypeScope scope)
        {
            if (node.Children.Count == 0)
            {
                return SimpleType.Unknown;
            }

            foreach (var argument in node.Children.Skip(1).Where(child => !child.IsToken))
            {
                CheckExpression(argument, scope);
            }

            var callee = node.Children[0];
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

        private SimpleType InferBinary(SyntaxNode node, TypeScope scope)
        {
            var children = node.Children;
            foreach (var child in children.Where(child => !child.IsToken))
            {
                CheckExpression(child, scope);
            }

            if (children.Any(child => child.IsToken && child.Kind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken))
            {
                return SimpleType.Named("bool");
            }

            if (children.Any(child => child.IsToken && child.Kind == SyntaxKind.NullCoalescingToken))
            {
                var right = children.Where(child => !child.IsToken).Skip(1).FirstOrDefault();
                return right is null ? SimpleType.Unknown : CheckExpression(right, scope);
            }

            return SimpleType.Unknown;
        }

        private static SimpleType InferNumericLiteral(string text)
        {
            if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                return SimpleType.Named("decimal");
            }

            return text.Contains('.', StringComparison.Ordinal) ? SimpleType.Named("double") : SimpleType.Named("int");
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

        private static bool CanAssign(SimpleType expected, SimpleType actual)
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

            if (expected.Name != actual.Name)
            {
                return false;
            }

            return expected.IsNullable || !actual.IsNullable;
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

    private sealed class TypeScope
    {
        private readonly TypeScope? _parent;
        private readonly Dictionary<string, SimpleType> _values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimpleType> _functions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CompileTimeOnlyTypeKind> _compileTimeOnlyTypes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);

        public TypeScope(TypeScope? parent)
        {
            _parent = parent;
        }

        public void DeclareValue(string name, SimpleType type) => _values[name] = type;

        public void DeclareFunction(string name, SimpleType returnType) => _functions[name] = returnType;

        public void DeclareType(string name) => _types.Add(name);

        public void DeclareCompileTimeOnlyType(string name, CompileTimeOnlyTypeKind kind) => _compileTimeOnlyTypes[name] = kind;

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

        public bool ResolveType(string name) => _types.Contains(name) || (_parent?.ResolveType(name) ?? false);
    }

    private enum CompileTimeOnlyTypeKind
    {
        None,
        TypeLevelUnion,
        StructuralShape
    }

    private readonly record struct SimpleType(string Name, bool IsNullable, bool IsKnown, bool IsNull)
    {
        public static SimpleType Unknown { get; } = new(string.Empty, IsNullable: false, IsKnown: false, IsNull: false);

        public static SimpleType Null { get; } = new("null", IsNullable: true, IsKnown: true, IsNull: true);

        public static SimpleType Named(string name) => new(name, IsNullable: false, IsKnown: true, IsNull: false);

        public SimpleType AsNullable() => this with { IsNullable = true };

        public override string ToString()
        {
            if (!IsKnown)
            {
                return "unknown";
            }

            if (IsNull)
            {
                return "null";
            }

            return IsNullable ? $"{Name}?" : Name;
        }
    }
}
