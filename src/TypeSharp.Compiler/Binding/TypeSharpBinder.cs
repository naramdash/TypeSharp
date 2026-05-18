using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Binding;

public static class TypeSharpBinder
{
    public static BindingResult Bind(SyntaxNode root, string file)
    {
        var binder = new Binder(file);
        return binder.Bind(root);
    }

    private sealed class Binder
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
        private readonly List<BoundSymbol> _symbols = [];
        private bool _hasStaticImport;

        public Binder(string file)
        {
            _file = file;
        }

        public BindingResult Bind(SyntaxNode root)
        {
            var scope = new BindingScope(null);
            AddBuiltIns(scope);
            CollectTopLevelSymbols(root, scope);

            foreach (var child in root.Children)
            {
                BindTopLevelDeclaration(child, scope);
            }

            return new BindingResult(_symbols, _diagnostics);
        }

        private void AddBuiltIns(BindingScope scope)
        {
            foreach (var name in BuiltInTypes)
            {
                scope.DeclareType(name);
            }
        }

        private void CollectTopLevelSymbols(SyntaxNode root, BindingScope scope)
        {
            foreach (var child in root.Children)
            {
                switch (child.Kind)
                {
                    case SyntaxKind.NamespaceDeclaration:
                        if (TryGetQualifiedNameText(child, out var namespaceName))
                        {
                            AddSymbol(scope, namespaceName, BoundSymbolKind.Namespace, child.Span, declareValue: false, declareType: true);
                        }

                        break;

                    case SyntaxKind.ModuleDeclaration:
                        if (TryGetDeclarationName(child, out var moduleName, out var moduleSpan))
                        {
                            AddSymbol(scope, moduleName, BoundSymbolKind.Namespace, moduleSpan, declareValue: false, declareType: true);
                        }

                        break;

                    case SyntaxKind.ImportNamedDeclaration:
                    case SyntaxKind.ImportTypeDeclaration:
                        foreach (var importName in GetNamedImportIdentifiers(child))
                        {
                            AddSymbol(scope, importName.Text ?? string.Empty, BoundSymbolKind.Import, importName.Span, declareValue: true, declareType: true);
                        }

                        break;

                    case SyntaxKind.ImportStaticDeclaration:
                        _hasStaticImport = true;
                        break;

                    case SyntaxKind.TypeAliasDeclaration:
                    case SyntaxKind.RecordDeclaration:
                    case SyntaxKind.UnionDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.DelegateDeclaration:
                        if (TryGetDeclarationName(child, out var typeName, out var typeSpan))
                        {
                            AddSymbol(scope, typeName, BoundSymbolKind.Type, typeSpan, declareValue: false, declareType: true);
                        }

                        break;

                    case SyntaxKind.FunctionDeclaration:
                        if (TryGetDeclarationName(child, out var functionName, out var functionSpan))
                        {
                            AddSymbol(scope, functionName, BoundSymbolKind.Function, functionSpan, declareValue: true, declareType: false);
                        }

                        break;

                    case SyntaxKind.ValueDeclaration:
                    case SyntaxKind.LiteralDeclaration:
                        if (TryGetDeclarationName(child, out var valueName, out var valueSpan))
                        {
                            AddSymbol(scope, valueName, BoundSymbolKind.Value, valueSpan, declareValue: true, declareType: false);
                        }

                        break;
                }
            }
        }

        private void BindTopLevelDeclaration(SyntaxNode node, BindingScope scope)
        {
            switch (node.Kind)
            {
                case SyntaxKind.FunctionDeclaration:
                    BindFunctionDeclaration(node, scope);
                    break;

                case SyntaxKind.ModuleDeclaration:
                    BindModuleDeclaration(node, scope);
                    break;

                case SyntaxKind.TypeAliasDeclaration:
                case SyntaxKind.RecordDeclaration:
                case SyntaxKind.UnionDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.DelegateDeclaration:
                case SyntaxKind.ValueDeclaration:
                case SyntaxKind.LiteralDeclaration:
                    BindTypeAnnotations(node, scope);
                    BindInitializers(node, scope);
                    break;
            }
        }

        private void BindModuleDeclaration(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            CollectTopLevelSymbols(node, scope);

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                BindTopLevelDeclaration(child, scope);
            }
        }

        private void BindFunctionDeclaration(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            DeclareTypeParameters(node, scope);
            foreach (var parameter in node.Children.Where(child => child.Kind == SyntaxKind.ParameterList).SelectMany(child => child.Children).Where(child => child.Kind == SyntaxKind.Parameter))
            {
                BindTypeAnnotations(parameter, scope);
                if (TryGetFirstIdentifier(parameter, out var parameterToken))
                {
                    scope.DeclareValue(parameterToken.Text ?? string.Empty);
                    _symbols.Add(new BoundSymbol(parameterToken.Text ?? string.Empty, BoundSymbolKind.Parameter, _file, parameterToken.Span));
                }
            }

            foreach (var typeAnnotation in node.Children.Where(child => child.Kind == SyntaxKind.TypeAnnotation))
            {
                BindTypeNode(typeAnnotation, scope);
            }

            foreach (var body in node.Children.Where(child => child.Kind == SyntaxKind.FunctionBody))
            {
                BindNode(body, scope);
            }
        }

        private void BindNode(SyntaxNode node, BindingScope scope)
        {
            switch (node.Kind)
            {
                case SyntaxKind.FunctionBody:
                case SyntaxKind.ExpressionStatement:
                case SyntaxKind.Initializer:
                    foreach (var child in node.Children)
                    {
                        BindNode(child, scope);
                    }

                    break;

                case SyntaxKind.BlockExpression:
                    BindBlock(node, new BindingScope(scope));
                    break;

                case SyntaxKind.ValueDeclaration:
                    BindValueDeclaration(node, scope);
                    break;

                case SyntaxKind.IdentifierExpression:
                    BindIdentifierExpression(node, scope, allowTypeSymbol: false, allowExternalStaticCall: false);
                    break;

                case SyntaxKind.CallExpression:
                    BindCallExpression(node, scope);
                    break;

                case SyntaxKind.MemberAccessExpression:
                case SyntaxKind.IndexerExpression:
                    if (node.Children.Count > 0)
                    {
                        BindNode(node.Children[0], scope);
                    }

                    for (var index = 2; index < node.Children.Count; index++)
                    {
                        BindNode(node.Children[index], scope);
                    }

                    break;

                case SyntaxKind.NamedArgument:
                    if (node.Children.Count > 2)
                    {
                        BindNode(node.Children[2], scope);
                    }

                    break;

                case SyntaxKind.OutArgument:
                case SyntaxKind.InArgument:
                case SyntaxKind.RefArgument:
                    foreach (var child in node.Children.Where(child => !child.IsToken))
                    {
                        BindNode(child, scope);
                    }

                    break;

                case SyntaxKind.LambdaExpression:
                    BindLambdaExpression(node, scope);
                    break;

                case SyntaxKind.ForExpression:
                    BindForExpression(node, scope);
                    break;

                case SyntaxKind.MatchExpression:
                    BindMatchExpression(node, scope);
                    break;

                case SyntaxKind.RecordExpression:
                    foreach (var child in node.Children.Where(child => child.Kind == SyntaxKind.RecordField))
                    {
                        BindRecordField(child, scope);
                    }

                    break;

                case SyntaxKind.TypeAnnotation:
                case SyntaxKind.TypeName:
                case SyntaxKind.ArrayType:
                case SyntaxKind.NullableType:
                case SyntaxKind.FunctionType:
                case SyntaxKind.UnionType:
                case SyntaxKind.RecordShapeType:
                case SyntaxKind.ShapeMember:
                case SyntaxKind.TypeArgumentList:
                    BindTypeNode(node, scope);
                    break;

                default:
                    foreach (var child in node.Children.Where(child => !child.IsToken))
                    {
                        BindNode(child, scope);
                    }

                    break;
            }
        }

        private void BindBlock(SyntaxNode node, BindingScope scope)
        {
            foreach (var child in node.Children)
            {
                if (child.IsToken)
                {
                    continue;
                }

                BindNode(child, scope);
            }
        }

        private void BindValueDeclaration(SyntaxNode node, BindingScope scope)
        {
            BindTypeAnnotations(node, scope);
            BindInitializers(node, scope);

            if (TryGetDeclarationName(node, out var name, out var span))
            {
                scope.DeclareValue(name);
                _symbols.Add(new BoundSymbol(name, BoundSymbolKind.Local, _file, span));
            }
        }

        private void BindCallExpression(SyntaxNode node, BindingScope scope)
        {
            if (node.Children.Count == 0)
            {
                return;
            }

            var callee = node.Children[0];
            if (callee.Kind == SyntaxKind.IdentifierExpression)
            {
                BindIdentifierExpression(callee, scope, allowTypeSymbol: true, allowExternalStaticCall: _hasStaticImport);
            }
            else
            {
                BindNode(callee, scope);
            }

            foreach (var child in node.Children.Skip(1).Where(child => !child.IsToken))
            {
                BindNode(child, scope);
            }
        }

        private void BindLambdaExpression(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            if (node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken) is { } parameter)
            {
                scope.DeclareValue(parameter.Text ?? string.Empty);
                _symbols.Add(new BoundSymbol(parameter.Text ?? string.Empty, BoundSymbolKind.Parameter, _file, parameter.Span));
            }

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                BindNode(child, scope);
            }
        }

        private void BindForExpression(SyntaxNode node, BindingScope parentScope)
        {
            if (node.Children.FirstOrDefault(child => child.Kind != SyntaxKind.Pattern && !child.IsToken) is { } iterable)
            {
                BindNode(iterable, parentScope);
            }

            var scope = new BindingScope(parentScope);
            foreach (var pattern in node.Children.Where(child => child.Kind == SyntaxKind.Pattern))
            {
                AddPatternBindings(pattern, scope);
            }

            foreach (var block in node.Children.Where(child => child.Kind == SyntaxKind.BlockExpression))
            {
                BindNode(block, scope);
            }
        }

        private void BindMatchExpression(SyntaxNode node, BindingScope scope)
        {
            if (node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.MatchArm) is { } input)
            {
                BindNode(input, scope);
            }

            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                var armScope = new BindingScope(scope);
                foreach (var pattern in arm.Children.Where(child => child.Kind == SyntaxKind.Pattern || child.Kind == SyntaxKind.RecordPattern))
                {
                    AddPatternBindings(pattern, armScope);
                }

                foreach (var child in arm.Children.Where(child => !child.IsToken && child.Kind != SyntaxKind.Pattern && child.Kind != SyntaxKind.RecordPattern))
                {
                    BindNode(child, armScope);
                }
            }
        }

        private void BindRecordField(SyntaxNode node, BindingScope scope)
        {
            var nonTokenChildren = node.Children.Where(child => !child.IsToken).ToArray();
            if (nonTokenChildren.Length > 0)
            {
                foreach (var child in nonTokenChildren)
                {
                    BindNode(child, scope);
                }

                return;
            }

            if (TryGetFirstIdentifier(node, out var identifier))
            {
                ResolveValue(identifier, scope, allowTypeSymbol: false, allowExternalStaticCall: false);
            }
        }

        private void AddPatternBindings(SyntaxNode node, BindingScope scope)
        {
            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                BindTypeNode(node, scope);
                return;
            }

            if (node.IsToken && node.Kind == SyntaxKind.IdentifierToken)
            {
                scope.DeclareValue(node.Text ?? string.Empty);
                _symbols.Add(new BoundSymbol(node.Text ?? string.Empty, BoundSymbolKind.Local, _file, node.Span));
                return;
            }

            foreach (var child in node.Children)
            {
                AddPatternBindings(child, scope);
            }
        }

        private void DeclareTypeParameters(SyntaxNode node, BindingScope scope)
        {
            foreach (var identifier in GetTypeParameterIdentifiers(node))
            {
                var name = identifier.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                scope.DeclareType(name);
                _symbols.Add(new BoundSymbol(name, BoundSymbolKind.Type, _file, identifier.Span));
            }
        }

        private void BindTypeAnnotations(SyntaxNode node, BindingScope scope)
        {
            foreach (var child in node.Children.Where(child => child.Kind == SyntaxKind.TypeAnnotation))
            {
                BindTypeNode(child, scope);
            }

            foreach (var child in node.Children.Where(child => !child.IsToken && child.Kind != SyntaxKind.TypeAnnotation && child.Kind != SyntaxKind.Initializer && child.Kind != SyntaxKind.FunctionBody))
            {
                BindTypeAnnotations(child, scope);
            }
        }

        private void BindInitializers(SyntaxNode node, BindingScope scope)
        {
            foreach (var initializer in node.Children.Where(child => child.Kind == SyntaxKind.Initializer))
            {
                BindNode(initializer, scope);
            }
        }

        private void BindTypeNode(SyntaxNode node, BindingScope scope)
        {
            if (node.Kind == SyntaxKind.TypeName)
            {
                var directIdentifiers = node.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken).ToArray();
                var hasDot = node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.DotToken);
                if (directIdentifiers.Length == 1 && !hasDot)
                {
                    ResolveType(directIdentifiers[0], scope);
                }

                foreach (var child in node.Children.Where(child => !child.IsToken))
                {
                    BindTypeNode(child, scope);
                }

                return;
            }

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                BindTypeNode(child, scope);
            }
        }

        private void BindIdentifierExpression(SyntaxNode node, BindingScope scope, bool allowTypeSymbol, bool allowExternalStaticCall)
        {
            if (TryGetFirstIdentifier(node, out var identifier))
            {
                ResolveValue(identifier, scope, allowTypeSymbol, allowExternalStaticCall);
            }
        }

        private void ResolveValue(SyntaxNode identifier, BindingScope scope, bool allowTypeSymbol, bool allowExternalStaticCall)
        {
            var name = identifier.Text ?? string.Empty;
            if (scope.ResolveValue(name) || (allowTypeSymbol && scope.ResolveType(name)) || (allowExternalStaticCall && IsCallableName(name)))
            {
                return;
            }

            ReportUnresolved(identifier, name);
        }

        private void ResolveType(SyntaxNode identifier, BindingScope scope)
        {
            var name = identifier.Text ?? string.Empty;
            if (scope.ResolveType(name))
            {
                return;
            }

            ReportUnresolved(identifier, name);
        }

        private void ReportUnresolved(SyntaxNode identifier, string name)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnresolvedName.Code,
                DiagnosticDescriptors.UnresolvedName.DefaultSeverity,
                $"Unresolved name '{name}'.",
                _file,
                identifier.Span));
        }

        private void AddSymbol(
            BindingScope scope,
            string name,
            BoundSymbolKind kind,
            SourceSpan span,
            bool declareValue,
            bool declareType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (declareValue)
            {
                scope.DeclareValue(name);
            }

            if (declareType)
            {
                scope.DeclareType(name);
            }

            _symbols.Add(new BoundSymbol(name, kind, _file, span));
        }

        private static bool IsCallableName(string name) =>
            name.Length > 0 && char.IsUpper(name[0]);

        private static bool TryGetDeclarationName(SyntaxNode node, out string name, out SourceSpan span)
        {
            name = string.Empty;
            span = node.Span;
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
                    span = child.Span;
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

        private static IEnumerable<SyntaxNode> GetTypeParameterIdentifiers(SyntaxNode node)
        {
            var typeParameterList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeParameterList);
            if (typeParameterList is null)
            {
                yield break;
            }

            foreach (var child in typeParameterList.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    yield return child;
                }
            }
        }

        private static bool TryGetQualifiedNameText(SyntaxNode node, out string name)
        {
            var parts = node.Children
                .Where(child => child.Kind == SyntaxKind.TypeName)
                .SelectMany(child => child.Children)
                .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                .Select(child => child.Text ?? string.Empty)
                .ToArray();

            name = string.Join(".", parts);
            return parts.Length > 0;
        }
    }

    private sealed class BindingScope
    {
        private readonly BindingScope? _parent;
        private readonly HashSet<string> _values = new(StringComparer.Ordinal);
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);

        public BindingScope(BindingScope? parent)
        {
            _parent = parent;
        }

        public void DeclareValue(string name) => _values.Add(name);

        public void DeclareType(string name) => _types.Add(name);

        public bool ResolveValue(string name) => _values.Contains(name) || (_parent?.ResolveValue(name) ?? false);

        public bool ResolveType(string name) => _types.Contains(name) || (_parent?.ResolveType(name) ?? false);
    }
}
