using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;

namespace TypeSharp.Compiler.Binding;

public static class TypeSharpBinder
{
    public static BindingResult Bind(
        SyntaxNode root,
        string file,
        IReadOnlyList<SourceAliasOption>? sourceAliases = null)
    {
        var binder = new Binder(file, sourceAliases ?? []);
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
        private readonly IReadOnlyList<SourceAliasOption> _sourceAliases;
        private readonly HashSet<string> _localExportedNames = new(StringComparer.Ordinal);
        private bool _hasStaticImport;

        public Binder(string file, IReadOnlyList<SourceAliasOption> sourceAliases)
        {
            _file = file;
            _sourceAliases = sourceAliases;
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

                    case SyntaxKind.ImportNamespaceDeclaration:
                        if (TryGetNamespaceImportAlias(child, out var importAlias))
                        {
                            AddSymbol(scope, importAlias.Text ?? string.Empty, BoundSymbolKind.Import, importAlias.Span, declareValue: true, declareType: true);
                        }

                        break;

                    case SyntaxKind.ImportStaticDeclaration:
                        _hasStaticImport = true;
                        break;

                    case SyntaxKind.ExportNamedDeclaration:
                    case SyntaxKind.ExportTypeDeclaration:
                        if (IsUnsupportedExportSpecifierDeclaration(child))
                        {
                            ReportUnsupportedExportForwarding(child);
                        }

                        break;

                    case SyntaxKind.ExportStarDeclaration:
                        if (!IsSupportedSourceReExport(child))
                        {
                            ReportUnsupportedExportForwarding(child);
                        }

                        break;

                    case SyntaxKind.TypeAliasDeclaration:
                    case SyntaxKind.RecordDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.DelegateDeclaration:
                        if (TryGetDeclarationName(child, out var typeName, out var typeSpan))
                        {
                            AddSymbol(scope, typeName, BoundSymbolKind.Type, typeSpan, declareValue: false, declareType: true);
                        }

                        break;

                    case SyntaxKind.UnionDeclaration:
                        if (TryGetDeclarationName(child, out var unionName, out var unionSpan))
                        {
                            AddSymbol(scope, unionName, BoundSymbolKind.Type, unionSpan, declareValue: false, declareType: true);
                        }

                        CollectUnionCaseSymbols(child, scope);
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
                            var isFunctionValue = child.Kind == SyntaxKind.ValueDeclaration && IsFunctionValueDeclaration(child);
                            AddSymbol(
                                scope,
                                valueName,
                                BoundSymbolKind.Value,
                                valueSpan,
                                declareValue: true,
                                declareType: false,
                                declareLiteral: child.Kind == SyntaxKind.LiteralDeclaration,
                                declareExportableValue: !isFunctionValue || HasFunctionTypeAnnotation(child) || HasLambdaInitializer(child));
                        }

                        break;
                }
            }
        }

        private void CollectUnionCaseSymbols(SyntaxNode unionDeclaration, BindingScope scope)
        {
            foreach (var unionCase in unionDeclaration.Children.Where(child => child.Kind == SyntaxKind.UnionCase))
            {
                var identifier = unionCase.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
                if (identifier is null)
                {
                    continue;
                }

                var kind = unionCase.Children.Any(child => child.Kind == SyntaxKind.ParameterList)
                    ? BoundSymbolKind.Function
                    : BoundSymbolKind.Value;
                AddSymbol(scope, identifier.Text ?? string.Empty, kind, identifier.Span, declareValue: true, declareType: false, declareFunction: false);
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
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.DelegateDeclaration:
                    BindTypeDeclaration(node, scope);
                    break;

                case SyntaxKind.ExtensionDeclaration:
                    BindExtensionDeclaration(node, scope);
                    break;

                case SyntaxKind.ValueDeclaration:
                case SyntaxKind.LiteralDeclaration:
                    BindTypeAnnotations(node, scope);
                    BindInitializers(node, scope);
                    break;

                case SyntaxKind.ExportNamedDeclaration:
                    BindLocalExportNamedDeclaration(node, scope);
                    break;

                case SyntaxKind.ExportTypeDeclaration:
                    BindLocalExportTypeDeclaration(node, scope);
                    break;
            }
        }

        private void BindLocalExportNamedDeclaration(SyntaxNode node, BindingScope scope)
        {
            if (IsSupportedSourceReExport(node))
            {
                return;
            }

            if (IsUnsupportedExportSpecifierDeclaration(node))
            {
                return;
            }

            if (HasExportAlias(node))
            {
                BindLocalValueExportAliases(node, scope);
                return;
            }

            foreach (var exportName in GetExportedIdentifiers(node))
            {
                var name = exportName.Text ?? string.Empty;
                CheckDuplicateExport(exportName, name);
                if (!scope.ResolveValue(name) && !scope.ResolveType(name))
                {
                    ReportUnresolved(exportName, name);
                }
            }
        }

        private void BindLocalExportTypeDeclaration(SyntaxNode node, BindingScope scope)
        {
            if (IsSupportedSourceReExport(node))
            {
                return;
            }

            if (IsUnsupportedExportSpecifierDeclaration(node))
            {
                return;
            }

            if (HasExportAlias(node))
            {
                foreach (var exportSpecifier in GetNamedExportSpecifiers(node))
                {
                    CheckDuplicateExport(exportSpecifier.ExportedIdentifier, exportSpecifier.ExportedName);
                    if (!scope.ResolveType(exportSpecifier.TargetName))
                    {
                        ReportUnresolved(exportSpecifier.TargetIdentifier, exportSpecifier.TargetName);
                    }
                }

                return;
            }

            foreach (var exportName in GetExportedIdentifiers(node))
            {
                var name = exportName.Text ?? string.Empty;
                CheckDuplicateExport(exportName, name);
                if (!scope.ResolveType(name))
                {
                    ReportUnresolved(exportName, name);
                }
            }
        }

        private void BindLocalValueExportAliases(SyntaxNode node, BindingScope scope)
        {
            foreach (var exportSpecifier in GetNamedExportSpecifiers(node))
            {
                CheckDuplicateExport(exportSpecifier.ExportedIdentifier, exportSpecifier.ExportedName);
                if (!scope.ResolveValue(exportSpecifier.TargetName) && !scope.ResolveType(exportSpecifier.TargetName))
                {
                    ReportUnresolved(exportSpecifier.TargetIdentifier, exportSpecifier.TargetName);
                    continue;
                }

                if (!scope.ResolveFunction(exportSpecifier.TargetName) &&
                    !scope.ResolveLiteral(exportSpecifier.TargetName) &&
                    !scope.ResolveExportableValue(exportSpecifier.TargetName))
                {
                    ReportUnsupportedExportForwarding(exportSpecifier.TargetIdentifier);
                }
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

        private void BindTypeDeclaration(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            DeclareTypeParameters(node, scope);
            BindTypeAnnotations(node, scope);
            BindInitializers(node, scope);

            foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
            {
                BindFunctionDeclaration(function, scope);
            }
        }

        private void BindExtensionDeclaration(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            if (node.Children.FirstOrDefault(child => IsTypeSyntax(child.Kind)) is { } receiverType)
            {
                BindTypeNode(receiverType, scope);
            }

            foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
            {
                BindFunctionDeclaration(function, scope);
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
                    AddSymbol(scope, parameterToken.Text ?? string.Empty, BoundSymbolKind.Parameter, parameterToken.Span, declareValue: true, declareType: false);
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

                case SyntaxKind.NameofExpression:
                    BindNameofExpression(node, scope);
                    break;

                case SyntaxKind.SatisfiesExpression:
                    BindSatisfiesExpression(node, scope);
                    break;

                case SyntaxKind.YieldExpression:
                    foreach (var child in node.Children.Where(child => !child.IsToken))
                    {
                        BindNode(child, scope);
                    }

                    break;

                case SyntaxKind.LockStatement:
                    BindLockStatement(node, scope);
                    break;

                case SyntaxKind.ForExpression:
                    BindForExpression(node, scope);
                    break;

                case SyntaxKind.MatchExpression:
                    BindMatchExpression(node, scope);
                    break;

                case SyntaxKind.RecordExpression:
                    foreach (var child in node.Children.Where(child => child.Kind is SyntaxKind.RecordField or SyntaxKind.RecordSpreadField))
                    {
                        if (child.Kind == SyntaxKind.RecordField)
                        {
                            BindRecordField(child, scope);
                        }
                        else
                        {
                            foreach (var expression in child.Children.Where(grandchild => !grandchild.IsToken))
                            {
                                BindNode(expression, scope);
                            }
                        }
                    }

                    break;

                case SyntaxKind.TypeAnnotation:
                case SyntaxKind.TypeName:
                case SyntaxKind.ArrayType:
                case SyntaxKind.NullableType:
                case SyntaxKind.FunctionType:
                case SyntaxKind.UnionType:
                case SyntaxKind.IntersectionType:
                case SyntaxKind.KeyofType:
                case SyntaxKind.IndexedAccessType:
                case SyntaxKind.LiteralType:
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
                AddSymbol(scope, name, BoundSymbolKind.Local, span, declareValue: true, declareType: false);
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

        private void BindSatisfiesExpression(SyntaxNode node, BindingScope scope)
        {
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            if (children.Length > 0)
            {
                BindNode(children[0], scope);
            }

            if (children.Length > 1)
            {
                BindTypeNode(children[^1], scope);
            }
        }

        private void BindLockStatement(SyntaxNode node, BindingScope scope)
        {
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            if (children.Length > 0)
            {
                BindNode(children[0], scope);
            }

            foreach (var block in children.Skip(1).Where(child => child.Kind == SyntaxKind.BlockExpression))
            {
                BindNode(block, scope);
            }
        }

        private void BindLambdaExpression(SyntaxNode node, BindingScope parentScope)
        {
            var scope = new BindingScope(parentScope);
            if (node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken) is { } parameter)
            {
                AddSymbol(scope, parameter.Text ?? string.Empty, BoundSymbolKind.Parameter, parameter.Span, declareValue: true, declareType: false);
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
                AddSymbol(scope, node.Text ?? string.Empty, BoundSymbolKind.Local, node.Span, declareValue: true, declareType: false);
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

                AddSymbol(scope, name, BoundSymbolKind.Type, identifier.Span, declareValue: false, declareType: true);
            }
        }

        private void BindTypeAnnotations(SyntaxNode node, BindingScope scope)
        {
            foreach (var child in node.Children.Where(child => child.Kind == SyntaxKind.TypeAnnotation))
            {
                BindTypeNode(child, scope);
            }

            foreach (var child in node.Children.Where(child => !child.IsToken
                && child.Kind != SyntaxKind.TypeAnnotation
                && child.Kind != SyntaxKind.Initializer
                && child.Kind != SyntaxKind.FunctionBody
                && child.Kind != SyntaxKind.FunctionDeclaration))
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

        private void BindNameofExpression(SyntaxNode node, BindingScope scope)
        {
            var target = node.Children.FirstOrDefault(child => !child.IsToken);
            if (target is null || !TryGetNameReferenceRootIdentifier(target, out var identifier))
            {
                return;
            }

            var name = identifier.Text ?? string.Empty;
            if (scope.ResolveValue(name) || scope.ResolveType(name))
            {
                return;
            }

            ReportUnresolved(identifier, name);
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

        private void ReportDuplicateSymbol(SourceSpan span, string name)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.DuplicateSymbol.Code,
                DiagnosticDescriptors.DuplicateSymbol.DefaultSeverity,
                $"Duplicate symbol '{name}' in the same scope.",
                _file,
                span));
        }

        private void ReportUnsupportedExportForwarding(SyntaxNode node)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.UnsupportedExportForwarding.Code,
                DiagnosticDescriptors.UnsupportedExportForwarding.DefaultSeverity,
                DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate,
                _file,
                node.Span));
        }

        private void CheckDuplicateExport(SyntaxNode identifier, string name)
        {
            if (!_localExportedNames.Add(name))
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.DuplicateExport.Code,
                    DiagnosticDescriptors.DuplicateExport.DefaultSeverity,
                    $"Duplicate export '{name}'.",
                    _file,
                    identifier.Span));
            }
        }

        private bool IsUnsupportedExportSpecifierDeclaration(SyntaxNode node) =>
            HasFromSpecifier(node) && !IsSupportedSourceReExport(node);

        private bool IsSupportedSourceReExport(SyntaxNode node) =>
            node.Kind is SyntaxKind.ExportNamedDeclaration or SyntaxKind.ExportTypeDeclaration or SyntaxKind.ExportStarDeclaration &&
            HasFromSpecifier(node) &&
            HasSourceFromSpecifier(node);

        private static bool HasFromSpecifier(SyntaxNode node) =>
            node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.FromKeyword);

        private bool HasSourceFromSpecifier(SyntaxNode node)
        {
            for (var index = 0; index < node.Children.Count - 1; index++)
            {
                if (node.Children[index].Kind != SyntaxKind.FromKeyword ||
                    node.Children[index + 1].Kind != SyntaxKind.StringLiteralToken)
                {
                    continue;
                }

                var specifier = Unquote(node.Children[index + 1].Text ?? string.Empty);
                return specifier == "." ||
                    specifier == ".." ||
                    specifier.StartsWith("./", StringComparison.Ordinal) ||
                    specifier.StartsWith("../", StringComparison.Ordinal) ||
                    MatchesSourceAlias(specifier);
            }

            return false;
        }

        private bool MatchesSourceAlias(string specifier) =>
            _sourceAliases.Any(alias => SourceAliasPatternMatches(alias.Pattern.Trim(), specifier));

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

        private static string Unquote(string text)
        {
            if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
            {
                return text[1..^1];
            }

            return text;
        }

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

        private static bool HasModifier(SyntaxNode node, SyntaxKind modifierKind) =>
            node.Children.Any(child => child.Kind == modifierKind);

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

        private static bool HasLambdaInitializer(SyntaxNode node) =>
            node.Children
                .Where(child => child.Kind == SyntaxKind.Initializer)
                .SelectMany(child => child.Children)
                .Any(child => child.Kind == SyntaxKind.LambdaExpression);

        private static bool HasFunctionTypeAnnotation(SyntaxNode node) =>
            node.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)?
                .Children
                .Any(child => child.Kind == SyntaxKind.FunctionType) == true;

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

        private static IEnumerable<SyntaxNode> GetExportedIdentifiers(SyntaxNode node)
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
                        yield return new NamedExportSpecifier(name, alias, child, node.Children[index + 2]);
                        index += 2;
                    }
                    else
                    {
                        yield return new NamedExportSpecifier(name, name, child, child);
                    }
                }
            }
        }

        private void AddSymbol(
            BindingScope scope,
            string name,
            BoundSymbolKind kind,
            SourceSpan span,
            bool declareValue,
            bool declareType,
            bool declareFunction = true,
            bool declareLiteral = false,
            bool declareExportableValue = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if ((declareValue && scope.HasLocalValue(name)) || (declareType && scope.HasLocalType(name)))
            {
                ReportDuplicateSymbol(span, name);
                return;
            }

            if (declareValue)
            {
                scope.DeclareValue(name);
            }

            if (kind == BoundSymbolKind.Function && declareFunction)
            {
                scope.DeclareFunction(name);
            }

            if (declareLiteral)
            {
                scope.DeclareLiteral(name);
            }

            if (declareExportableValue)
            {
                scope.DeclareExportableValue(name);
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

        private static bool TryGetNameReferenceRootIdentifier(SyntaxNode node, out SyntaxNode identifier)
        {
            if (node.Kind == SyntaxKind.UnboundGenericNameExpression &&
                node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.UnboundGenericArityList) is { } target)
            {
                return TryGetNameReferenceRootIdentifier(target, out identifier);
            }

            if (node.Kind == SyntaxKind.MemberAccessExpression &&
                node.Children.FirstOrDefault(child => !child.IsToken) is { } receiver)
            {
                return TryGetNameReferenceRootIdentifier(receiver, out identifier);
            }

            return TryGetFirstIdentifier(node, out identifier);
        }

        private static IEnumerable<SyntaxNode> GetNamedImportIdentifiers(SyntaxNode node)
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

                if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    if (index + 2 < node.Children.Count &&
                        node.Children[index + 1].IsToken &&
                        node.Children[index + 1].Kind == SyntaxKind.AsKeyword &&
                        node.Children[index + 2].IsToken &&
                        node.Children[index + 2].Kind == SyntaxKind.IdentifierToken)
                    {
                        yield return node.Children[index + 2];
                        index += 2;
                    }
                    else
                    {
                        yield return child;
                    }
                }
            }
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

        private readonly record struct NamedExportSpecifier(
            string TargetName,
            string ExportedName,
            SyntaxNode TargetIdentifier,
            SyntaxNode ExportedIdentifier);
    }

    private sealed class BindingScope
    {
        private readonly BindingScope? _parent;
        private readonly HashSet<string> _values = new(StringComparer.Ordinal);
        private readonly HashSet<string> _types = new(StringComparer.Ordinal);
        private readonly HashSet<string> _functions = new(StringComparer.Ordinal);
        private readonly HashSet<string> _literals = new(StringComparer.Ordinal);
        private readonly HashSet<string> _exportableValues = new(StringComparer.Ordinal);

        public BindingScope(BindingScope? parent)
        {
            _parent = parent;
        }

        public bool HasLocalValue(string name) => _values.Contains(name);

        public bool HasLocalType(string name) => _types.Contains(name);

        public void DeclareValue(string name) => _values.Add(name);

        public void DeclareType(string name) => _types.Add(name);

        public void DeclareFunction(string name) => _functions.Add(name);

        public void DeclareLiteral(string name) => _literals.Add(name);

        public void DeclareExportableValue(string name) => _exportableValues.Add(name);

        public bool ResolveValue(string name) => _values.Contains(name) || (_parent?.ResolveValue(name) ?? false);

        public bool ResolveType(string name) => _types.Contains(name) || (_parent?.ResolveType(name) ?? false);

        public bool ResolveFunction(string name) => _functions.Contains(name) || (_parent?.ResolveFunction(name) ?? false);

        public bool ResolveLiteral(string name) => _literals.Contains(name) || (_parent?.ResolveLiteral(name) ?? false);

        public bool ResolveExportableValue(string name) => _exportableValues.Contains(name) || (_parent?.ResolveExportableValue(name) ?? false);
    }
}
