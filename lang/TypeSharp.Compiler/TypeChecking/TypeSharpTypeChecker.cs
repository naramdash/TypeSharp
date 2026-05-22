using System.Globalization;
using System.Numerics;
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
            CollectExtensionProperties(root, scope);
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
                            scope.DeclareFunction(
                                functionName,
                                functionReturnType,
                                GetFunctionCapabilities(child),
                                GetFunctionParameterTypes(child, scope),
                                GetFunctionParameterNames(child),
                                GetTypeParameterNames(child),
                                GetFunctionParamsParameterIndex(child, scope),
                                GetFunctionOptionalParameterFlags(child));
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
                            scope.DeclareValue(valueName, valueType, IsMutableValueDeclaration(child));
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

        private void CollectExtensionProperties(SyntaxNode root, TypeScope scope)
        {
            foreach (var extension in root.Children.Where(child => child.Kind == SyntaxKind.ExtensionDeclaration))
            {
                var receiverTypeNode = extension.Children.FirstOrDefault(child => IsTypeSyntax(child.Kind));
                if (receiverTypeNode is null || !TryGetType(receiverTypeNode, scope, out var receiverType))
                {
                    continue;
                }

                ReportExtensionPropertyHelperNameCollisions(extension, scope, receiverType);

                foreach (var property in extension.Children.Where(child => child.Kind == SyntaxKind.ValueDeclaration))
                {
                    if (!TryGetDeclarationName(property, out var propertyName) ||
                        !TryGetDirectTypeAnnotation(property, out var propertyTypeNode) ||
                        !TryGetType(propertyTypeNode, scope, out var propertyType))
                    {
                        continue;
                    }

                    if (!CanCollectExtensionProperty(extension, property, receiverType, propertyName, propertyType))
                    {
                        continue;
                    }

                    if (TryGetExtensionPropertyPrecedenceConflict(scope, receiverType, propertyName, out var conflictMessage))
                    {
                        ReportMismatch(property, conflictMessage);
                        continue;
                    }

                    if (!scope.TryDeclareExtensionProperty(receiverType, propertyName, propertyType))
                    {
                        ReportMismatch(property, $"Extension property '{propertyName}' is already declared for receiver type '{receiverType}'.");
                    }
                }
            }
        }

        private void ReportExtensionPropertyHelperNameCollisions(SyntaxNode extension, TypeScope scope, SimpleType receiverType)
        {
            var methodNames = extension.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsExternalSignatureFunction(child))
                .Select(child => TryGetDeclarationName(child, out var methodName) ? methodName : string.Empty)
                .Where(name => name.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            var generatedHelpers = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var property in extension.Children.Where(child => child.Kind == SyntaxKind.ValueDeclaration))
            {
                if (!TryGetDeclarationName(property, out var propertyName) ||
                    !TryGetDirectTypeAnnotation(property, out var propertyTypeNode) ||
                    !TryGetType(propertyTypeNode, scope, out var propertyType) ||
                    !CanCollectExtensionProperty(extension, property, receiverType, propertyName, propertyType))
                {
                    continue;
                }

                var helperName = GetExtensionPropertyHelperName(propertyName);
                if (methodNames.Contains(helperName))
                {
                    ReportMismatch(
                        property,
                        $"Extension property '{propertyName}' generates helper method '{helperName}', which conflicts with extension method '{helperName}' in the same extension declaration.");
                }

                if (generatedHelpers.TryGetValue(helperName, out var existingPropertyName))
                {
                    ReportMismatch(
                        property,
                        $"Extension property '{propertyName}' generates helper method '{helperName}', which conflicts with extension property '{existingPropertyName}' in the same extension declaration.");
                }
                else
                {
                    generatedHelpers.Add(helperName, propertyName);
                }
            }
        }

        private static bool CanCollectExtensionProperty(
            SyntaxNode extension,
            SyntaxNode property,
            SimpleType receiverType,
            string propertyName,
            SimpleType propertyType) =>
            GetExtensionReceiverIdentifier(extension) is not null &&
            !IsMutableValueDeclaration(property) &&
            HasInitializer(property) &&
            !property.Children.Any(child => child.Kind == SyntaxKind.AccessorBlock) &&
            receiverType.IsKnown &&
            !receiverType.IsNull &&
            !receiverType.IsNullable &&
            propertyName.Length > 0 &&
            propertyType.IsKnown;

        private bool TryGetExtensionPropertyPrecedenceConflict(
            TypeScope scope,
            SimpleType receiverType,
            string propertyName,
            out string message)
        {
            message = string.Empty;
            if (!receiverType.IsKnown ||
                receiverType.IsNull ||
                receiverType.IsNullable ||
                propertyName.Length == 0)
            {
                return false;
            }

            if (TryFindImportedMemberAccessType(
                    FindMetadataTypes(receiverType.Name),
                    propertyName,
                    isStatic: false,
                    requireWritable: false,
                    out _))
            {
                message = FormatExtensionPropertyPrecedenceConflict(receiverType, propertyName);
                return true;
            }

            if (scope.ResolveShape(receiverType.Name, out var shape) &&
                shape.Members.Any(member => string.Equals(member.Name, propertyName, StringComparison.Ordinal)))
            {
                message = FormatExtensionPropertyPrecedenceConflict(receiverType, propertyName);
                return true;
            }

            return false;
        }

        private static string FormatExtensionPropertyPrecedenceConflict(SimpleType receiverType, string propertyName) =>
            $"Extension property '{propertyName}' conflicts with existing member '{propertyName}' on receiver type '{receiverType}'. Ordinary and structural members take precedence over extension properties.";

        private static string FormatNullableExtensionPropertyReceiverMessage(SimpleType receiverType)
        {
            var nonNullReceiverType = receiverType with { IsNullable = false };
            return $"Extension property receiver type '{receiverType}' is nullable; nullable extension-property receivers are not supported in this slice. Use non-null receiver type '{nonNullReceiverType}' until nullable receiver lifting is implemented.";
        }

        private static string GetExtensionPropertyHelperName(string propertyName) =>
            string.IsNullOrWhiteSpace(propertyName) ? "GetValue" : $"Get{propertyName}";

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

                case SyntaxKind.EnumDeclaration:
                    CheckGenericConstraints(node);
                    CheckEnumDeclaration(node);
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
            CollectExtensionProperties(node, scope);

            foreach (var child in node.Children.Where(child => !child.IsToken))
            {
                CheckTopLevelDeclaration(child, scope);
            }
        }

        private void CheckEnumDeclaration(SyntaxNode node)
        {
            var annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            var underlyingTypeName = "int";
            if (annotation is not null)
            {
                if (!TryGetType(annotation, out var underlyingType))
                {
                    return;
                }

                if (!IsValidEnumUnderlyingType(underlyingType.Name))
                {
                    ReportMismatch(
                        annotation,
                        $"Enum underlying type must be one of 'byte', 'sbyte', 'short', 'ushort', 'int', 'uint', 'long', or 'ulong', but found '{underlyingType}'.");
                    return;
                }

                underlyingTypeName = underlyingType.Name;
            }

            CheckEnumMemberInitializers(node, underlyingTypeName);
            CheckEnumMemberReferences(node);
        }

        private void CheckEnumMemberInitializers(SyntaxNode node, string underlyingTypeName)
        {
            if (!TryGetEnumUnderlyingRange(underlyingTypeName, out var minimum, out var maximum))
            {
                return;
            }

            foreach (var member in node.Children.Where(child => child.Kind == SyntaxKind.EnumMember))
            {
                var initializer = member.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Initializer);
                if (initializer is null)
                {
                    continue;
                }

                foreach (var literalText in GetEnumInitializerLiteralTexts(initializer))
                {
                    if (!BigInteger.TryParse(literalText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
                    {
                        ReportMismatch(
                            initializer,
                            $"Enum member value '{literalText}' must be an integer literal for underlying type '{underlyingTypeName}'.");
                        continue;
                    }

                    if (value < minimum || value > maximum)
                    {
                        ReportMismatch(
                            initializer,
                            $"Enum member value '{literalText}' is outside the range of underlying type '{underlyingTypeName}'.");
                    }
                }
            }
        }

        private void CheckEnumMemberReferences(SyntaxNode node)
        {
            var declaredMembers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in node.Children.Where(child => child.Kind == SyntaxKind.EnumMember))
            {
                var memberName = member.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                var initializer = member.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Initializer);
                if (initializer is not null)
                {
                    var isAlias = IsSingleEnumAliasInitializer(initializer);
                    foreach (var target in GetEnumInitializerIdentifierOperands(initializer))
                    {
                        var targetName = target.Text ?? string.Empty;
                        if (declaredMembers.Contains(targetName))
                        {
                            continue;
                        }

                        ReportMismatch(
                            target,
                            isAlias
                                ? $"Enum member alias '{memberName}' must reference a previously declared member of the same enum, but found '{targetName}'."
                                : $"Enum member initializer '{memberName}' must reference only previously declared members of the same enum, but found '{targetName}'.");
                    }
                }

                if (memberName.Length > 0)
                {
                    declaredMembers.Add(memberName);
                }
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
            CheckParamsParameterDeclarations(node, scope);
            CheckDefaultParameterDeclarations(node, scope);
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

        private void CheckParamsParameterDeclarations(SyntaxNode node, TypeScope scope)
        {
            var parameters = GetParameterNodes(node).ToArray();
            var paramsIndexes = parameters
                .Select((parameter, index) => (Parameter: parameter, Index: index))
                .Where(item => IsParamsParameter(item.Parameter))
                .ToArray();

            if (paramsIndexes.Length == 0)
            {
                return;
            }

            if (paramsIndexes.Length > 1)
            {
                foreach (var duplicate in paramsIndexes.Skip(1))
                {
                    ReportMismatch(duplicate.Parameter, "A parameter list can declare only one params parameter.");
                }
            }

            var firstParams = paramsIndexes[0];
            if (firstParams.Index != parameters.Length - 1)
            {
                ReportMismatch(firstParams.Parameter, "The params parameter must be the final parameter.");
            }

            foreach (var item in paramsIndexes)
            {
                if (!TryGetDirectTypeAnnotation(item.Parameter, out var annotation) ||
                    !TryGetType(annotation, scope, out var parameterType) ||
                    !TryGetArrayElementTypeName(parameterType, out _))
                {
                    var name = item.Parameter.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? "parameter";
                    ReportMismatch(item.Parameter, $"Params parameter '{name}' must have an array type.");
                }
            }
        }

        private void CheckDefaultParameterDeclarations(SyntaxNode node, TypeScope scope)
        {
            var parameters = GetParameterNodes(node).ToArray();
            var defaultedParameters = parameters
                .Select((parameter, index) => (Parameter: parameter, Index: index))
                .Where(item => IsDefaultedParameter(item.Parameter))
                .ToArray();

            if (defaultedParameters.Length == 0)
            {
                return;
            }

            var typeParameterNames = new HashSet<string>(GetTypeParameterNames(node), StringComparer.Ordinal);

            if (IsExternalSignatureFunction(node))
            {
                foreach (var item in defaultedParameters)
                {
                    var name = GetParameterName(item.Parameter) ?? "parameter";
                    ReportMismatch(
                        item.Parameter,
                        $"Defaulted parameter '{name}' is supported only on TypeSharp-owned function declarations.");
                }
            }

            if (parameters.Any(IsParamsParameter))
            {
                ReportMismatch(
                    defaultedParameters[0].Parameter,
                    "Defaulted parameters cannot be combined with params parameters in the initial optional-parameter slice.");
            }

            var defaultedSuffixStarted = false;
            foreach (var parameter in parameters)
            {
                if (IsDefaultedParameter(parameter))
                {
                    defaultedSuffixStarted = true;
                    CheckDefaultParameterInitializer(parameter, scope, typeParameterNames);
                    continue;
                }

                if (defaultedSuffixStarted)
                {
                    var name = GetParameterName(parameter) ?? "parameter";
                    ReportMismatch(parameter, $"Required parameter '{name}' cannot follow a defaulted parameter.");
                }
            }
        }

        private void CheckDefaultParameterInitializer(
            SyntaxNode parameter,
            TypeScope scope,
            IReadOnlySet<string> containingTypeParameterNames)
        {
            var name = GetParameterName(parameter) ?? "parameter";
            if (!TryGetDirectTypeAnnotation(parameter, out var annotation) ||
                !TryGetType(annotation, scope, out var parameterType) ||
                !parameterType.IsKnown)
            {
                ReportMismatch(parameter, $"Defaulted parameter '{name}' must declare an explicit type.");
                return;
            }

            if (containingTypeParameterNames.Count > 0 &&
                GenericTypeReferencesAny(parameterType, containingTypeParameterNames, containingTypeParameterNames))
            {
                ReportMismatch(
                    parameter,
                    $"Defaulted parameter '{name}' cannot use generic type parameter type '{parameterType}'.");
                return;
            }

            if (IsParamsParameter(parameter))
            {
                ReportMismatch(parameter, $"Params parameter '{name}' cannot declare a default value.");
            }

            var initializerExpression = GetParameterInitializerExpression(parameter);
            if (initializerExpression is null)
            {
                return;
            }

            if (!IsSupportedDefaultParameterExpression(initializerExpression))
            {
                ReportMismatch(
                    initializerExpression,
                    $"Defaulted parameter '{name}' must use a string, numeric, bool, or null literal.");
                return;
            }

            var initializerType = CheckExpressionWithExpected(initializerExpression, scope, parameterType);
            if (!initializerType.IsKnown)
            {
                return;
            }

            if (IsNullabilityViolation(parameterType, initializerType))
            {
                ReportMismatch(
                    initializerExpression,
                    initializerType.IsNull
                        ? $"Default value for parameter '{name}' cannot be null because it expects non-null type '{parameterType}'."
                        : $"Default value for parameter '{name}' expects non-null type '{parameterType}', but found nullable type '{initializerType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, parameterType, initializerType, out var structuralMessage))
            {
                ReportMismatch(initializerExpression, structuralMessage);
            }
            else if (!CanAssign(scope, parameterType, initializerType))
            {
                ReportMismatch(
                    initializerExpression,
                    $"Default value for parameter '{name}' expects '{parameterType}', but found '{initializerType}'.");
            }
        }

        private void CheckExtensionDeclaration(SyntaxNode node, TypeScope scope)
        {
            var receiverTypeNode = node.Children.FirstOrDefault(child => IsTypeSyntax(child.Kind));
            var receiverType = SimpleType.Unknown;
            var hasReceiverType = receiverTypeNode is not null && TryGetType(receiverTypeNode, scope, out receiverType);
            var receiverIdentifier = GetExtensionReceiverIdentifier(node);
            var propertyScope = new TypeScope(scope);
            if (hasReceiverType && receiverIdentifier is not null)
            {
                propertyScope.DeclareValue(receiverIdentifier.Text ?? string.Empty, receiverType);
            }

            foreach (var function in node.Children.Where(child => child.Kind == SyntaxKind.FunctionDeclaration))
            {
                var firstParameter = function.Children
                    .Where(child => child.Kind == SyntaxKind.ParameterList)
                    .SelectMany(child => child.Children)
                    .FirstOrDefault(child => child.Kind == SyntaxKind.Parameter);

                if (!hasReceiverType)
                {
                    ReportMismatch(node, "Extension declaration requires a receiver type.");
                }
                else if (firstParameter is null ||
                    !TryGetDirectTypeAnnotation(firstParameter, out var parameterTypeNode) ||
                    !TryGetType(parameterTypeNode, scope, out var parameterType))
                {
                    ReportMismatch(function, $"Extension method requires a first receiver parameter of type '{receiverType}'.");
                }
                else if (IsParamsParameter(firstParameter))
                {
                    ReportMismatch(firstParameter, "Extension method receiver parameter cannot be a params parameter.");
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

            foreach (var property in node.Children.Where(child => child.Kind == SyntaxKind.ValueDeclaration))
            {
                if (!hasReceiverType)
                {
                    ReportMismatch(node, "Extension declaration requires a receiver type.");
                }

                if (receiverIdentifier is null)
                {
                    ReportMismatch(property, "Extension property requires a receiver name in the extension declaration.");
                }

                if (receiverType.IsKnown && receiverType.IsNullable)
                {
                    ReportMismatch(
                        property,
                        FormatNullableExtensionPropertyReceiverMessage(receiverType));
                }

                if (IsMutableValueDeclaration(property))
                {
                    ReportMismatch(property, "Extension property cannot be mutable.");
                }

                if (!TryGetDirectTypeAnnotation(property, out _))
                {
                    ReportMismatch(property, "Extension property requires an explicit type annotation.");
                }

                if (!HasInitializer(property))
                {
                    ReportMismatch(property, "Extension property requires an initializer expression.");
                }

                if (property.Children.Any(child => child.Kind == SyntaxKind.AccessorBlock))
                {
                    ReportMismatch(property, "Extension property accessor blocks are not supported in this slice; use an initializer expression.");
                }

                CheckPublicValueBoundary(property, scope);
                CheckValueDeclaration(property, propertyScope);
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
            var hasTypeAnnotation = TryGetDirectTypeAnnotation(node, out var typeNode);
            var annotationKnown =
                hasTypeAnnotation &&
                TryGetType(typeNode, scope, out expectedType);
            var annotationParameterType = SimpleType.Unknown;
            var annotationReturnType = SimpleType.Unknown;
            var functionAnnotationKnown =
                hasTypeAnnotation &&
                TryGetUnaryFunctionType(typeNode, scope, out annotationParameterType, out annotationReturnType);
            if ((annotationKnown || functionAnnotationKnown) && !scope.AllowsDynamic && ContainsDynamicType(typeNode))
            {
                ReportDynamicCapabilityRequired(typeNode);
            }

            var initializerType = SimpleType.Unknown;
            SyntaxNode? initializerExpression = null;
            if (node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Initializer) is { } initializer)
            {
                initializerExpression = initializer.Children.FirstOrDefault(child => !child.IsToken);
                initializerType = initializerExpression is null
                    ? SimpleType.Unknown
                    : CheckExpressionWithExpected(initializerExpression, scope, annotationKnown ? expectedType : null);
            }

            if (!functionAnnotationKnown &&
                initializerExpression is not null &&
                IsCompositionExpression(initializerExpression) &&
                !IsShiftLikeCompositionExpression(initializerExpression, scope) &&
                IsPublicBoundaryDeclaration(node))
            {
                ReportMismatch(
                    node,
                    "Public direct composition values require an explicit function type annotation.");
            }

            if (functionAnnotationKnown && initializerExpression is not null)
            {
                CheckCompositionFunctionTypeAnnotation(
                    node,
                    scope,
                    initializerExpression,
                    annotationParameterType,
                    annotationReturnType);
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
                    scope.DeclareValue(name, declaredType, IsMutableValueDeclaration(node));
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

            if (IsBitwiseExpression(node))
            {
                return CheckBitwiseExpression(node, scope);
            }

            if (node.Kind == SyntaxKind.AssignmentExpression)
            {
                return CheckAssignmentExpression(node, scope);
            }

            if (IsLogicalUnsignedShiftExpression(node))
            {
                return CheckLogicalUnsignedShiftExpression(node, scope);
            }

            if (IsCompositionExpression(node))
            {
                return CheckCompositionExpression(node, scope);
            }

            if (IsPipelineExpression(node))
            {
                return CheckPipelineExpression(node, scope);
            }

            if (node.Kind == SyntaxKind.CallExpression &&
                TryGetDirectCallName(node, out var directCallName) &&
                scope.ResolveFunctionInfo(directCallName, out var directFunction))
            {
                return CheckDirectFunctionCallExpression(node, scope, directCallName, directFunction);
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
                SyntaxKind.NullConditionalMemberAccessExpression => CheckNullConditionalMemberAccess(node, scope),
                SyntaxKind.AwaitExpression => InferAwait(node, scope),
                SyntaxKind.IndexerExpression => InferIndexer(node, scope),
                SyntaxKind.NullConditionalIndexerExpression => CheckNullConditionalIndexerAccess(node, scope),
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

        private SimpleType CheckAssignmentExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                foreach (var expression in expressions)
                {
                    CheckExpression(expression, scope);
                }

                return SimpleType.Unknown;
            }

            var target = expressions[0];
            var value = expressions[1];
            var operatorKind = node.Children.FirstOrDefault(child => child.IsToken && IsAssignmentOperatorKind(child.Kind))?.Kind ?? SyntaxKind.UnknownToken;

            if (target.Kind == SyntaxKind.NullConditionalMemberAccessExpression)
            {
                return CheckNullConditionalMemberAssignment(node, target, value, scope, operatorKind);
            }

            if (target.Kind == SyntaxKind.NullConditionalIndexerExpression)
            {
                return CheckNullConditionalIndexerAssignment(node, target, value, scope, operatorKind);
            }

            if (TryGetAssignmentTargetIdentifier(target, out var targetIdentifier))
            {
                var targetName = targetIdentifier.Text ?? string.Empty;
                if (!scope.ResolveValueInfo(targetName, out var targetInfo))
                {
                    CheckExpression(target, scope);
                    CheckExpression(value, scope);
                    return SimpleType.Unknown;
                }

                if (!targetInfo.IsMutable)
                {
                    ReportMismatch(targetIdentifier, $"Cannot assign to immutable binding '{targetName}'. Use 'let mut' when mutation is intended.");
                }

                return operatorKind switch
                {
                    SyntaxKind.EqualsToken => CheckSimpleAssignmentValue(node, value, scope, targetInfo.Type),
                    SyntaxKind.PipeEqualsToken or SyntaxKind.AmpersandEqualsToken or SyntaxKind.CaretEqualsToken =>
                        CheckBitwiseCompoundAssignmentValue(node, value, scope, targetInfo.Type, operatorKind),
                    SyntaxKind.LessLessEqualsToken or SyntaxKind.GreaterGreaterEqualsToken or SyntaxKind.LogicalUnsignedShiftEqualsToken =>
                        CheckShiftCompoundAssignmentValue(node, value, scope, targetInfo.Type, operatorKind),
                    _ => CheckCompoundAssignmentValue(value, scope, targetInfo.Type)
                };
            }

            if (TryGetNullableExtensionPropertyAssignmentTarget(target, scope, out var nullableAssignmentExtensionProperty, out var nullableReceiverType))
            {
                CheckExpression(value, scope);
                ReportMismatch(target, FormatNullableExtensionPropertyAccessMessage(nullableReceiverType, nullableAssignmentExtensionProperty));
                return nullableAssignmentExtensionProperty.Type;
            }

            if (TryGetExtensionPropertyAssignmentTarget(target, scope, out var extensionProperty))
            {
                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    $"Extension property '{extensionProperty.Name}' is getter-only and cannot be assigned; extension property setters are not supported in this slice.");
                return extensionProperty.Type;
            }

            if (IsImportedAssignmentTargetCandidate(target))
            {
                if (operatorKind == SyntaxKind.LogicalUnsignedShiftEqualsToken)
                {
                    return CheckImportedLogicalUnsignedShiftAssignment(node, target, value, scope);
                }

                var targetType = CheckExpression(target, scope);
                CheckExpression(value, scope);
                return targetType;
            }

            ReportMismatch(target, "Assignment target must be a mutable local binding or a supported imported C# member, indexer, or event target.");
            CheckExpression(target, scope);
            CheckExpression(value, scope);
            return SimpleType.Unknown;
        }

        private bool TryGetExtensionPropertyAssignmentTarget(
            SyntaxNode target,
            TypeScope scope,
            out ExtensionPropertyInfo extensionProperty)
        {
            extensionProperty = default;
            if (target.Kind != SyntaxKind.MemberAccessExpression ||
                !TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            if (TryGetImportedStaticMemberAccessType(receiver, memberName, scope, requireWritable: false, out _))
            {
                return false;
            }

            var receiverType = CheckExpression(receiver, scope);
            if (IsUnknownType(receiverType) ||
                TryGetImportedInstanceMemberAccessType(receiverType, memberName, requireWritable: false, out _))
            {
                return false;
            }

            if (receiverType.IsKnown && scope.ResolveShape(receiverType.Name, out var shape) &&
                shape.Members.Any(member => string.Equals(member.Name, memberName, StringComparison.Ordinal)))
            {
                return false;
            }

            return scope.ResolveExtensionProperty(receiverType, memberName, out extensionProperty);
        }

        private bool TryGetNullableExtensionPropertyAssignmentTarget(
            SyntaxNode target,
            TypeScope scope,
            out ExtensionPropertyInfo extensionProperty,
            out SimpleType receiverType)
        {
            extensionProperty = default;
            receiverType = SimpleType.Unknown;
            if (target.Kind != SyntaxKind.MemberAccessExpression ||
                !TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            if (TryGetImportedStaticMemberAccessType(receiver, memberName, scope, requireWritable: false, out _))
            {
                return false;
            }

            receiverType = CheckExpression(receiver, scope);
            if (IsUnknownType(receiverType))
            {
                return false;
            }

            var nonNullReceiverType = receiverType with { IsNullable = false };
            if (TryGetImportedInstanceMemberAccessType(nonNullReceiverType, memberName, requireWritable: false, out _))
            {
                return false;
            }

            if (receiverType.IsKnown && scope.ResolveShape(receiverType.Name, out var shape) &&
                shape.Members.Any(member => string.Equals(member.Name, memberName, StringComparison.Ordinal)))
            {
                return false;
            }

            return TryGetNullableExtensionPropertyAccess(receiverType, memberName, scope, out extensionProperty);
        }

        private SimpleType CheckSimpleAssignmentValue(SyntaxNode assignment, SyntaxNode value, TypeScope scope, SimpleType targetType)
        {
            var valueType = CheckExpressionWithExpected(value, scope, targetType.IsKnown ? targetType : null);
            if (!targetType.IsKnown || !valueType.IsKnown)
            {
                return targetType.IsKnown ? targetType : valueType;
            }

            if (IsNullabilityViolation(targetType, valueType))
            {
                ReportNullabilityViolation(
                    assignment,
                    valueType.IsNull
                        ? $"Cannot assign null to non-null type '{targetType}'."
                        : $"Cannot assign nullable expression of type '{valueType}' to non-null type '{targetType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, targetType, valueType, out var structuralMessage))
            {
                ReportMismatch(assignment, structuralMessage);
            }
            else if (!CanAssign(scope, targetType, valueType))
            {
                ReportMismatch(assignment, $"Cannot assign expression of type '{valueType}' to '{targetType}'.");
            }

            return targetType;
        }

        private SimpleType CheckNullConditionalMemberAssignment(
            SyntaxNode assignment,
            SyntaxNode target,
            SyntaxNode value,
            TypeScope scope,
            SyntaxKind operatorKind)
        {
            if (IsAdditiveAssignmentOperatorKind(operatorKind))
            {
                if (TryGetNullConditionalImportedMemberAssignmentTargetType(target, scope, out var additiveTargetType))
                {
                    return CheckAdditiveCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        additiveTargetType,
                        operatorKind);
                }

                if (TryGetNullConditionalExtensionPropertyTarget(
                        target,
                        scope,
                        out var additiveExtensionProperty,
                        out var additiveReceiverType))
                {
                    CheckExpression(value, scope);
                    ReportMismatch(target, FormatNullConditionalExtensionPropertyAssignmentMessage(additiveReceiverType, additiveExtensionProperty));
                    return additiveExtensionProperty.Type;
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    "Null-conditional additive compound assignment '?.' is supported only for readable and writable metadata-backed imported C# instance field/property targets.");
                return SimpleType.Unknown;
            }

            if (IsBitwiseAssignmentOperatorKind(operatorKind))
            {
                if (TryGetNullConditionalImportedMemberAssignmentTargetType(target, scope, out var bitwiseTargetType))
                {
                    return CheckBitwiseCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        bitwiseTargetType,
                        operatorKind);
                }

                if (TryGetNullConditionalExtensionPropertyTarget(
                        target,
                        scope,
                        out var bitwiseExtensionProperty,
                        out var bitwiseReceiverType))
                {
                    CheckExpression(value, scope);
                    ReportMismatch(target, FormatNullConditionalExtensionPropertyAssignmentMessage(bitwiseReceiverType, bitwiseExtensionProperty));
                    return bitwiseExtensionProperty.Type;
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    "Null-conditional bitwise compound assignment '?.' is supported only for readable and writable metadata-backed imported C# instance field/property targets.");
                return SimpleType.Unknown;
            }

            if (operatorKind == SyntaxKind.LogicalUnsignedShiftEqualsToken)
            {
                if (TryGetNullConditionalImportedMemberAssignmentTargetType(target, scope, out var logicalShiftTargetType))
                {
                    return CheckShiftCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        logicalShiftTargetType,
                        SyntaxKind.LogicalUnsignedShiftEqualsToken);
                }

                if (TryGetNullConditionalExtensionPropertyTarget(
                        target,
                        scope,
                        out var logicalShiftExtensionProperty,
                        out var logicalShiftReceiverType))
                {
                    CheckExpression(value, scope);
                    ReportMismatch(target, FormatNullConditionalExtensionPropertyAssignmentMessage(logicalShiftReceiverType, logicalShiftExtensionProperty));
                    return logicalShiftExtensionProperty.Type;
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    "Null-conditional logical unsigned shift assignment '?.' is supported only for readable and writable metadata-backed imported C# instance field/property targets.");
                return SimpleType.Unknown;
            }

            if (operatorKind != SyntaxKind.EqualsToken)
            {
                CheckNullConditionalReceiver(target, scope);
                CheckExpression(value, scope);
                ReportMismatch(
                    assignment,
                    "Null-conditional assignment '?.' supports only simple '=', bounded additive compound '+=', '-=', bounded bitwise compound '|=', '&=', '^=', or bounded logical unsigned shift '>>>=' over metadata-backed imported C# instance field/property targets; other compound assignment, increment, decrement, indexer, event, static, and TypeSharp-owned targets are not supported.");
                return SimpleType.Unknown;
            }

            if (TryGetNullConditionalImportedMemberAssignmentTargetType(target, scope, out var targetType))
            {
                return CheckSimpleAssignmentValue(assignment, value, scope, targetType);
            }

            if (TryGetNullConditionalExtensionPropertyTarget(
                    target,
                    scope,
                    out var extensionProperty,
                    out var receiverType))
            {
                CheckExpression(value, scope);
                ReportMismatch(target, FormatNullConditionalExtensionPropertyAssignmentMessage(receiverType, extensionProperty));
                return extensionProperty.Type;
            }

            CheckExpression(value, scope);
            ReportMismatch(
                target,
                "Null-conditional assignment '?.' is supported only for writable metadata-backed imported C# instance field/property targets.");
            return SimpleType.Unknown;
        }

        private SimpleType CheckNullConditionalMemberAccess(SyntaxNode node, TypeScope scope)
        {
            if (TryGetNullConditionalImportedMemberReadType(node, scope, out var targetType))
            {
                return targetType.AsNullable();
            }

            if (TryGetNullConditionalExtensionPropertyTarget(
                    node,
                    scope,
                    out var extensionProperty,
                    out var receiverType))
            {
                ReportMismatch(node, FormatNullConditionalExtensionPropertyAccessMessage(receiverType, extensionProperty));
                return extensionProperty.Type;
            }

            CheckNullConditionalReceiver(node, scope);
            ReportMismatch(
                node,
                "Null-conditional member access '?.' is supported only for readable metadata-backed imported C# instance field/property targets in this slice; invocation, chains, events, static, local, and TypeSharp-owned targets are not supported.");
            return SimpleType.Unknown;
        }

        private SimpleType CheckNullConditionalIndexerAssignment(
            SyntaxNode assignment,
            SyntaxNode target,
            SyntaxNode value,
            TypeScope scope,
            SyntaxKind operatorKind)
        {
            if (IsBitwiseAssignmentOperatorKind(operatorKind))
            {
                if (TryGetNullConditionalImportedIndexerAssignmentTargetType(target, scope, out var bitwiseTargetType))
                {
                    return CheckBitwiseCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        bitwiseTargetType,
                        operatorKind);
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    "Null-conditional bitwise compound assignment '?[]' is supported only for readable and writable metadata-backed imported C# instance indexer targets with a matching public getter and setter.");
                return SimpleType.Unknown;
            }

            if (operatorKind == SyntaxKind.LogicalUnsignedShiftEqualsToken)
            {
                if (TryGetNullConditionalImportedIndexerAssignmentTargetType(target, scope, out var logicalShiftTargetType))
                {
                    return CheckShiftCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        logicalShiftTargetType,
                        SyntaxKind.LogicalUnsignedShiftEqualsToken);
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    target,
                    "Null-conditional logical unsigned shift assignment '?[]' is supported only for readable and writable metadata-backed imported C# instance indexer targets with a matching public getter and setter.");
                return SimpleType.Unknown;
            }

            if (operatorKind != SyntaxKind.EqualsToken)
            {
                CheckNullConditionalIndexerParts(target, scope);
                CheckExpression(value, scope);
                ReportMismatch(
                    assignment,
                    "Null-conditional assignment '?[]' supports only simple '=', bounded bitwise compound '|=', '&=', '^=', or bounded logical unsigned shift '>>>=' over metadata-backed imported C# instance indexer targets; other compound assignment, increment, decrement, member, event, static, and TypeSharp-owned targets are not supported.");
                return SimpleType.Unknown;
            }

            if (TryGetNullConditionalImportedIndexerAssignmentTargetType(target, scope, out var targetType))
            {
                return CheckSimpleAssignmentValue(assignment, value, scope, targetType);
            }

            CheckExpression(value, scope);
            ReportMismatch(
                target,
                "Null-conditional assignment '?[]' is supported only for writable metadata-backed imported C# instance indexer targets with a matching public getter and setter.");
            return SimpleType.Unknown;
        }

        private SimpleType CheckNullConditionalIndexerAccess(SyntaxNode node, TypeScope scope)
        {
            if (TryGetNullConditionalImportedIndexerReadType(node, scope, out var targetType))
            {
                return targetType.AsNullable();
            }

            CheckNullConditionalIndexerParts(node, scope);
            ReportMismatch(
                node,
                "Null-conditional indexer access '?[]' is supported only for readable metadata-backed imported C# instance indexer targets with a matching public getter.");
            return SimpleType.Unknown;
        }

        private void CheckNullConditionalReceiver(SyntaxNode target, TypeScope scope)
        {
            if (TryGetMemberAccessParts(target, out var receiver, out _))
            {
                CheckExpression(receiver, scope);
            }
        }

        private void CheckNullConditionalIndexerParts(SyntaxNode target, TypeScope scope)
        {
            if (!TryGetIndexerAccessParts(target, out var receiver, out var arguments))
            {
                return;
            }

            CheckExpression(receiver, scope);
            foreach (var argument in arguments)
            {
                CheckExpression(argument, scope);
            }
        }

        private SimpleType CheckCompoundAssignmentValue(SyntaxNode value, TypeScope scope, SimpleType targetType)
        {
            CheckExpression(value, scope);
            return targetType;
        }

        private SimpleType CheckImportedLogicalUnsignedShiftAssignment(
            SyntaxNode assignment,
            SyntaxNode target,
            SyntaxNode value,
            TypeScope scope)
        {
            if (target.Kind == SyntaxKind.MemberAccessExpression &&
                TryGetImportedMemberAssignmentTargetType(target, scope, out var targetType))
            {
                return CheckShiftCompoundAssignmentValue(
                    assignment,
                    value,
                    scope,
                    targetType,
                    SyntaxKind.LogicalUnsignedShiftEqualsToken);
            }

            if (target.Kind == SyntaxKind.IndexerExpression)
            {
                if (TryGetImportedIndexerAssignmentTargetType(target, scope, out var indexerTargetType))
                {
                    return CheckShiftCompoundAssignmentValue(
                        assignment,
                        value,
                        scope,
                        indexerTargetType,
                        SyntaxKind.LogicalUnsignedShiftEqualsToken);
                }

                CheckExpression(value, scope);
                ReportMismatch(
                    assignment,
                    "Logical unsigned shift assignment '>>>=' is supported only for mutable local bindings or readable and writable metadata-backed imported C# field/property/indexer targets; indexer targets require a matching public getter and setter.");
                return SimpleType.Unknown;
            }

            var fallbackTargetType = CheckExpression(target, scope);
            CheckExpression(value, scope);
            ReportMismatch(
                assignment,
                "Logical unsigned shift assignment '>>>=' is supported only for mutable local bindings or readable and writable metadata-backed imported C# field/property/indexer targets; event and unresolved imported member targets are not supported.");
            return fallbackTargetType.IsKnown ? fallbackTargetType : SimpleType.Unknown;
        }

        private SimpleType CheckShiftCompoundAssignmentValue(
            SyntaxNode assignment,
            SyntaxNode value,
            TypeScope scope,
            SimpleType targetType,
            SyntaxKind operatorKind)
        {
            var valueType = CheckExpression(value, scope);
            if (!targetType.IsKnown || !valueType.IsKnown)
            {
                return targetType.IsKnown ? targetType : SimpleType.Unknown;
            }

            var operatorText = operatorKind switch
            {
                SyntaxKind.LessLessEqualsToken => "<<=",
                SyntaxKind.GreaterGreaterEqualsToken => ">>=",
                SyntaxKind.LogicalUnsignedShiftEqualsToken => ">>>=",
                _ => "?="
            };

            if (!TryGetBinaryIntegralShiftResultType(targetType, valueType, out _))
            {
                ReportMismatch(
                    assignment,
                    $"Shift assignment '{operatorText}' operands must be non-null primitive integral values with an int-compatible shift count, but found '{targetType}' and '{valueType}'.");
                return targetType;
            }

            return targetType;
        }

        private SimpleType CheckAdditiveCompoundAssignmentValue(
            SyntaxNode assignment,
            SyntaxNode value,
            TypeScope scope,
            SimpleType targetType,
            SyntaxKind operatorKind)
        {
            var valueType = CheckExpression(value, scope);
            if (!targetType.IsKnown || !valueType.IsKnown)
            {
                return targetType.IsKnown ? targetType : SimpleType.Unknown;
            }

            var operatorText = operatorKind switch
            {
                SyntaxKind.PlusEqualsToken => "+=",
                SyntaxKind.MinusEqualsToken => "-=",
                _ => "?="
            };

            if (!TryGetBinaryIntegralAdditiveResultType(targetType, valueType, out var resultType))
            {
                ReportMismatch(
                    assignment,
                    $"Additive compound assignment '{operatorText}' operands must be non-null primitive integral numeric values of a supported type, but found '{targetType}' and '{valueType}'.");
                return targetType;
            }

            if (resultType.IsKnown && !CanAssign(scope, targetType, resultType))
            {
                ReportMismatch(assignment, $"Cannot assign additive compound assignment result of type '{resultType}' to '{targetType}'.");
            }

            return targetType;
        }

        private SimpleType CheckBitwiseCompoundAssignmentValue(
            SyntaxNode assignment,
            SyntaxNode value,
            TypeScope scope,
            SimpleType targetType,
            SyntaxKind operatorKind)
        {
            var valueType = CheckExpression(value, scope);
            if (!targetType.IsKnown || !valueType.IsKnown)
            {
                return targetType.IsKnown ? targetType : SimpleType.Unknown;
            }

            var operatorText = operatorKind switch
            {
                SyntaxKind.PipeEqualsToken => "|=",
                SyntaxKind.AmpersandEqualsToken => "&=",
                SyntaxKind.CaretEqualsToken => "^=",
                _ => "?="
            };

            if (TryGetBitwiseCompoundResultType(scope, targetType, valueType, operatorText, assignment, out var resultType) &&
                resultType.IsKnown &&
                !CanAssign(scope, targetType, resultType))
            {
                ReportMismatch(assignment, $"Cannot assign compound assignment result of type '{resultType}' to '{targetType}'.");
            }

            return targetType;
        }

        private bool TryGetBitwiseCompoundResultType(
            TypeScope scope,
            SimpleType targetType,
            SimpleType valueType,
            string operatorText,
            SyntaxNode node,
            out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            var targetIsEnum = IsKnownEnumType(scope, targetType);
            var valueIsEnum = IsKnownEnumType(scope, valueType);
            if (targetIsEnum || valueIsEnum)
            {
                if (!targetIsEnum || !valueIsEnum || !MetadataTypeNameMatches(targetType.Name, valueType.Name))
                {
                    ReportMismatch(node, $"Enum compound assignment '{operatorText}' operands must be enum values of the same type, but found '{targetType}' and '{valueType}'.");
                    return false;
                }

                resultType = targetType;
                return true;
            }

            if (TryGetBinaryIntegralBitwiseResultType(targetType, valueType, out resultType) ||
                TryGetBinaryBooleanBitwiseResultType(targetType, valueType, out resultType))
            {
                return true;
            }

            if (IsKnownNonNullableBoolType(targetType) || IsKnownNonNullableBoolType(valueType))
            {
                ReportMismatch(node, $"Boolean compound assignment '{operatorText}' operands must both be 'bool', but found '{targetType}' and '{valueType}'.");
                return false;
            }

            ReportMismatch(node, $"Bitwise compound assignment '{operatorText}' operands must be integral numeric values or boolean values of a supported primitive type, but found '{targetType}' and '{valueType}'.");
            return false;
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

            if (TryGetImportedStaticMemberAccessType(receiver, member.Text, scope, requireWritable: false, out var staticMemberType))
            {
                return staticMemberType;
            }

            var receiverType = CheckExpression(receiver, scope);
            if (IsUnknownType(receiverType))
            {
                ReportUnknownAccessRequiresNarrowing(node);
                return SimpleType.Unknown;
            }

            if (TryGetImportedInstanceMemberAccessType(receiverType, member.Text, requireWritable: false, out var importedMemberType))
            {
                return importedMemberType;
            }

            var memberName = member.Text;
            if (receiverType.IsKnown && scope.ResolveShape(receiverType.Name, out var shape))
            {
                var shapeMember = shape.Members.FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.Ordinal));
                if (shapeMember.Name is not null)
                {
                    return shapeMember.IsOptional ? shapeMember.Type.AsNullable() : shapeMember.Type;
                }

                if (scope.ResolveExtensionProperty(receiverType, memberName, out var shapeExtensionProperty))
                {
                    return shapeExtensionProperty.Type;
                }

                if (TryGetNullableExtensionPropertyAccess(receiverType, memberName, scope, out var nullableShapeExtensionProperty))
                {
                    ReportMismatch(node, FormatNullableExtensionPropertyAccessMessage(receiverType, nullableShapeExtensionProperty));
                    return nullableShapeExtensionProperty.Type;
                }

                ReportMismatch(node, $"Type '{receiverType}' does not contain member '{memberName}'.");
                return SimpleType.Unknown;
            }

            if (scope.ResolveExtensionProperty(receiverType, memberName, out var extensionProperty))
            {
                return extensionProperty.Type;
            }

            if (TryGetNullableExtensionPropertyAccess(receiverType, memberName, scope, out var nullableExtensionProperty))
            {
                ReportMismatch(node, FormatNullableExtensionPropertyAccessMessage(receiverType, nullableExtensionProperty));
                return nullableExtensionProperty.Type;
            }

            return SimpleType.Unknown;
        }

        private static string FormatNullableExtensionPropertyAccessMessage(SimpleType receiverType, ExtensionPropertyInfo extensionProperty) =>
            $"Extension property '{extensionProperty.Name}' requires non-null receiver type '{extensionProperty.ReceiverType}', but receiver expression has nullable type '{receiverType}'; nullable extension-property receiver access is not supported in this slice. Narrow the receiver before accessing the property until nullable receiver lifting is implemented.";

        private static string FormatNullConditionalExtensionPropertyAccessMessage(SimpleType receiverType, ExtensionPropertyInfo extensionProperty) =>
            $"Extension property '{extensionProperty.Name}' cannot be accessed with null-conditional member access '?.' for receiver type '{receiverType}'; TypeSharp-owned null-conditional extension-property access is not supported in this slice. Use ordinary member access after narrowing nullable receivers until nullable receiver lifting is implemented.";

        private static string FormatNullConditionalExtensionPropertyAssignmentMessage(SimpleType receiverType, ExtensionPropertyInfo extensionProperty) =>
            $"Extension property '{extensionProperty.Name}' cannot be assigned with null-conditional member access '?.' for receiver type '{receiverType}'; extension properties are getter-only and TypeSharp-owned null-conditional extension-property assignment is not supported in this slice. Use ordinary member access after narrowing nullable receivers until nullable receiver lifting and setters are implemented.";

        private bool TryGetNullableExtensionPropertyAccess(
            SimpleType receiverType,
            string memberName,
            TypeScope scope,
            out ExtensionPropertyInfo extensionProperty)
        {
            extensionProperty = default;
            if (!receiverType.IsKnown ||
                receiverType.IsNull ||
                !receiverType.IsNullable ||
                memberName.Length == 0)
            {
                return false;
            }

            var nonNullReceiverType = receiverType with { IsNullable = false };
            return scope.ResolveExtensionProperty(nonNullReceiverType, memberName, out extensionProperty);
        }

        private bool TryGetNullConditionalExtensionPropertyTarget(
            SyntaxNode target,
            TypeScope scope,
            out ExtensionPropertyInfo extensionProperty,
            out SimpleType receiverType)
        {
            extensionProperty = default;
            receiverType = SimpleType.Unknown;
            if (target.Kind != SyntaxKind.NullConditionalMemberAccessExpression ||
                !TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            receiverType = CheckExpression(receiver, scope);
            if (IsUnknownType(receiverType) || memberName.Length == 0)
            {
                return false;
            }

            var lookupType = receiverType with { IsNullable = false };
            if (scope.ResolveExtensionProperty(lookupType, memberName, out extensionProperty))
            {
                return true;
            }

            return false;
        }

        private bool TryGetImportedMemberAssignmentTargetType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            targetType = SimpleType.Unknown;
            if (!TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            if (TryGetImportedStaticMemberAccessType(receiver, memberName, scope, requireWritable: true, out targetType))
            {
                return true;
            }

            var receiverType = CheckExpression(receiver, scope);
            return TryGetImportedInstanceMemberAccessType(receiverType, memberName, requireWritable: true, out targetType);
        }

        private bool TryGetNullConditionalImportedMemberAssignmentTargetType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            targetType = SimpleType.Unknown;
            if (!TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            var receiverType = CheckExpression(receiver, scope);
            return TryGetImportedNullConditionalInstanceMemberAccessType(receiverType, memberName, requireWritable: true, out targetType);
        }

        private bool TryGetNullConditionalImportedMemberReadType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            targetType = SimpleType.Unknown;
            if (!TryGetMemberAccessParts(target, out var receiver, out var memberName))
            {
                return false;
            }

            var receiverType = CheckExpression(receiver, scope);
            return TryGetImportedNullConditionalInstanceMemberAccessType(receiverType, memberName, requireWritable: false, out targetType);
        }

        private bool TryGetImportedStaticMemberAccessType(
            SyntaxNode receiver,
            string memberName,
            TypeScope scope,
            bool requireWritable,
            out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (receiver.Kind != SyntaxKind.IdentifierExpression ||
                !TryGetFirstIdentifier(receiver, out var identifier) ||
                identifier.Text is not { Length: > 0 } receiverName ||
                scope.ResolveValue(receiverName, out _) ||
                scope.ResolveFunctionInfo(receiverName, out _))
            {
                return false;
            }

            return TryFindImportedMemberAccessType(
                FindMetadataTypes(receiverName),
                memberName,
                isStatic: true,
                requireWritable,
                out type);
        }

        private bool TryGetImportedInstanceMemberAccessType(
            SimpleType receiverType,
            string memberName,
            bool requireWritable,
            out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (!receiverType.IsKnown || receiverType.IsNull || receiverType.IsNullable)
            {
                return false;
            }

            return TryFindImportedMemberAccessType(
                FindMetadataTypes(receiverType.Name),
                memberName,
                isStatic: false,
                requireWritable,
                out type);
        }

        private bool TryGetImportedNullConditionalInstanceMemberAccessType(
            SimpleType receiverType,
            string memberName,
            bool requireWritable,
            out SimpleType type)
        {
            type = SimpleType.Unknown;
            if (!receiverType.IsKnown || receiverType.IsNull)
            {
                return false;
            }

            var lookupType = receiverType with { IsNullable = false };
            var receiverTypes = FindMetadataTypes(lookupType.Name);
            if (!receiverTypes.Any(candidate => !candidate.IsValueType))
            {
                return false;
            }

            return TryFindImportedMemberAccessType(
                receiverTypes,
                memberName,
                isStatic: false,
                requireWritable,
                out type);
        }

        private static bool TryFindImportedMemberAccessType(
            IReadOnlyList<MetadataTypeSymbol> receiverTypes,
            string memberName,
            bool isStatic,
            bool requireWritable,
            out SimpleType type)
        {
            type = SimpleType.Unknown;
            foreach (var receiverType in receiverTypes)
            {
                var property = receiverType.Properties.FirstOrDefault(candidate =>
                    candidate.IsStatic == isStatic &&
                    !candidate.IsIndexer &&
                    candidate.HasPublicGetter &&
                    (!requireWritable || candidate.HasPublicSetter) &&
                    string.Equals(candidate.Name, memberName, StringComparison.Ordinal));
                if (property is not null)
                {
                    type = SimpleType.Named(NormalizePrimitiveTypeName(property.Type));
                    return true;
                }

                var field = receiverType.Fields.FirstOrDefault(candidate =>
                    candidate.IsStatic == isStatic &&
                    !candidate.IsLiteral &&
                    (!requireWritable || !candidate.IsReadOnly) &&
                    string.Equals(candidate.Name, memberName, StringComparison.Ordinal));
                if (field is not null)
                {
                    type = SimpleType.Named(NormalizePrimitiveTypeName(field.Type));
                    return true;
                }
            }

            return false;
        }

        private bool TryGetImportedIndexerAssignmentTargetType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            targetType = SimpleType.Unknown;
            if (!TryGetIndexerAccessParts(target, out var receiver, out var arguments) ||
                arguments.Count == 0)
            {
                return false;
            }

            var receiverType = CheckExpression(receiver, scope);
            var argumentTypes = arguments
                .Select(argument => GetIndexerArgumentType(argument, scope))
                .ToArray();

            if (IsUnknownType(receiverType))
            {
                ReportUnknownAccessRequiresNarrowing(target);
                return false;
            }

            if (!receiverType.IsKnown || receiverType.IsNull || receiverType.IsNullable)
            {
                return false;
            }

            if (!TrySelectImportedIndexerProperty(
                    FindMetadataTypes(receiverType.Name),
                    arguments,
                    argumentTypes,
                    requireWritable: true,
                    out var property))
            {
                return false;
            }

            targetType = SimpleType.Named(NormalizePrimitiveTypeName(property.Type));
            return true;
        }

        private bool TryGetNullConditionalImportedIndexerAssignmentTargetType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            return TryGetNullConditionalImportedIndexerTargetType(
                target,
                scope,
                requireWritable: true,
                out targetType);
        }

        private bool TryGetNullConditionalImportedIndexerReadType(
            SyntaxNode target,
            TypeScope scope,
            out SimpleType targetType)
        {
            return TryGetNullConditionalImportedIndexerTargetType(
                target,
                scope,
                requireWritable: false,
                out targetType);
        }

        private bool TryGetNullConditionalImportedIndexerTargetType(
            SyntaxNode target,
            TypeScope scope,
            bool requireWritable,
            out SimpleType targetType)
        {
            targetType = SimpleType.Unknown;
            if (!TryGetIndexerAccessParts(target, out var receiver, out var arguments) ||
                arguments.Count == 0)
            {
                return false;
            }

            var receiverType = CheckExpression(receiver, scope);
            var argumentTypes = arguments
                .Select(argument => GetIndexerArgumentType(argument, scope))
                .ToArray();

            if (IsUnknownType(receiverType))
            {
                ReportUnknownAccessRequiresNarrowing(target);
                return false;
            }

            if (!receiverType.IsKnown || receiverType.IsNull)
            {
                return false;
            }

            var lookupType = receiverType with { IsNullable = false };
            var receiverTypes = FindMetadataTypes(lookupType.Name);
            if (!receiverTypes.Any(type => !type.IsValueType))
            {
                return false;
            }

            if (!TrySelectImportedIndexerProperty(
                    receiverTypes,
                    arguments,
                    argumentTypes,
                    requireWritable,
                    out var property))
            {
                return false;
            }

            targetType = SimpleType.Named(NormalizePrimitiveTypeName(property.Type));
            return true;
        }

        private IndexerArgumentType GetIndexerArgumentType(SyntaxNode argument, TypeScope scope)
        {
            var expression = UnwrapParenthesizedExpression(argument);
            var type = CheckExpression(argument, scope);
            var numericLiteralText = TryGetNumericLiteralText(expression, out var literalText)
                ? literalText
                : null;
            var isNullLiteral = IsNullLiteralExpression(expression);
            return new IndexerArgumentType(type, numericLiteralText, isNullLiteral);
        }

        private bool TrySelectImportedIndexerProperty(
            IReadOnlyList<MetadataTypeSymbol> receiverTypes,
            IReadOnlyList<SyntaxNode> arguments,
            IReadOnlyList<IndexerArgumentType> argumentTypes,
            bool requireWritable,
            out MetadataPropertySymbol property)
        {
            property = default!;
            var candidates = receiverTypes
                .SelectMany(type => type.Properties)
                .Where(candidate =>
                    !candidate.IsStatic &&
                    candidate.IsIndexer &&
                    candidate.HasPublicGetter &&
                    (!requireWritable || candidate.HasPublicSetter) &&
                    candidate.ParameterCount == arguments.Count)
                .ToArray();
            if (candidates.Length == 0)
            {
                return false;
            }

            var scoredCandidates = candidates
                .Select(candidate => TryScoreIndexerArguments(candidate, argumentTypes, out var score)
                    ? new IndexerCandidateScore(candidate, score)
                    : (IndexerCandidateScore?)null)
                .Where(score => score is not null)
                .Select(score => score!.Value)
                .ToArray();
            if (scoredCandidates.Length == 0)
            {
                if (candidates.Length == 1 && argumentTypes.Any(argument => !argument.Type.IsKnown))
                {
                    property = candidates[0];
                    return true;
                }

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

            property = bestCandidates[0];
            return true;
        }

        private bool TryScoreIndexerArguments(
            MetadataPropertySymbol property,
            IReadOnlyList<IndexerArgumentType> argumentTypes,
            out int score)
        {
            score = 0;
            if (property.ParameterTypes.Count != argumentTypes.Count)
            {
                return false;
            }

            for (var index = 0; index < argumentTypes.Count; index++)
            {
                if (!TryScoreIndexerArgument(argumentTypes[index], property.ParameterTypes[index], out var argumentScore))
                {
                    return false;
                }

                score += argumentScore;
            }

            return true;
        }

        private bool TryScoreIndexerArgument(
            IndexerArgumentType argument,
            string parameterType,
            out int score)
        {
            score = 0;
            var normalizedParameterType = NormalizePrimitiveTypeName(parameterType);
            if (!argument.Type.IsKnown)
            {
                score = 10_000;
                return true;
            }

            if (argument.IsNullLiteral)
            {
                if (string.Equals(normalizedParameterType, "object", StringComparison.Ordinal) ||
                    FindMetadataTypes(parameterType).Any(type => !type.IsValueType))
                {
                    score = string.Equals(normalizedParameterType, "object", StringComparison.Ordinal) ? 1000 : 100;
                    return true;
                }

                return false;
            }

            var normalizedArgumentType = NormalizePrimitiveTypeName(argument.Type.Name);
            if (TypeNamesMatch(normalizedArgumentType, normalizedParameterType))
            {
                score = 0;
                return true;
            }

            if (argument.NumericLiteralText is not null &&
                CanPassNumericLiteralToIndexerParameter(argument.NumericLiteralText, normalizedParameterType))
            {
                score = 10;
                return true;
            }

            if (CanAssignMetadataType(SimpleType.Named(parameterType), argument.Type))
            {
                score = 100;
                return true;
            }

            if (string.Equals(normalizedParameterType, "object", StringComparison.Ordinal) &&
                !argument.Type.IsNull)
            {
                score = 1000;
                return true;
            }

            return false;
        }

        private static bool TypeNamesMatch(string actual, string expected) =>
            string.Equals(actual, expected, StringComparison.Ordinal) ||
            MetadataTypeNameMatches(actual, expected);

        private static bool CanPassNumericLiteralToIndexerParameter(string literalText, string parameterType)
        {
            if (!TryParseIntegerLiteral(literalText, out var value))
            {
                return false;
            }

            return parameterType switch
            {
                "byte" => value >= byte.MinValue && value <= byte.MaxValue,
                "sbyte" => value >= sbyte.MinValue && value <= sbyte.MaxValue,
                "short" => value >= short.MinValue && value <= short.MaxValue,
                "ushort" => value >= ushort.MinValue && value <= ushort.MaxValue,
                "int" => value >= int.MinValue && value <= int.MaxValue,
                "uint" => value >= uint.MinValue && value <= uint.MaxValue,
                "long" => value >= long.MinValue && value <= long.MaxValue,
                "ulong" => value >= ulong.MinValue && value <= ulong.MaxValue,
                _ => false
            };
        }

        private static bool TryParseIntegerLiteral(string literalText, out BigInteger value)
        {
            literalText = literalText.Trim();
            if (literalText.Length == 0 ||
                literalText.Contains('.', StringComparison.Ordinal))
            {
                value = default;
                return false;
            }

            return BigInteger.TryParse(literalText, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetIndexerAccessParts(
            SyntaxNode node,
            out SyntaxNode receiver,
            out IReadOnlyList<SyntaxNode> arguments)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (node.Kind is not (SyntaxKind.IndexerExpression or SyntaxKind.NullConditionalIndexerExpression) ||
                expressions.Length < 2)
            {
                receiver = default!;
                arguments = [];
                return false;
            }

            receiver = expressions[0];
            arguments = expressions.Skip(1).ToArray();
            return true;
        }

        private static SyntaxNode UnwrapParenthesizedExpression(SyntaxNode node)
        {
            var current = node;
            while (current.Kind == SyntaxKind.ParenthesizedExpression &&
                current.Children.FirstOrDefault(child => !child.IsToken) is { } inner)
            {
                current = inner;
            }

            return current;
        }

        private static bool TryGetNumericLiteralText(SyntaxNode expression, out string text)
        {
            text = string.Empty;
            expression = UnwrapParenthesizedExpression(expression);
            if (expression.Kind == SyntaxKind.LiteralExpression)
            {
                var token = expression.Children.FirstOrDefault(child => child.IsToken);
                if (token?.Kind == SyntaxKind.NumericLiteralToken &&
                    token.Text is { Length: > 0 } literalText)
                {
                    text = literalText;
                    return true;
                }
            }

            if (expression.Kind == SyntaxKind.BinaryExpression &&
                expression.Children.FirstOrDefault(child => child.IsToken) is { Kind: SyntaxKind.MinusToken } &&
                expression.Children.Count(child => !child.IsToken) == 1 &&
                TryGetNumericLiteralText(expression.Children.First(child => !child.IsToken), out var operandText))
            {
                text = "-" + operandText;
                return true;
            }

            return false;
        }

        private static bool IsNullLiteralExpression(SyntaxNode expression)
        {
            expression = UnwrapParenthesizedExpression(expression);
            var token = expression.Kind == SyntaxKind.LiteralExpression
                ? expression.Children.FirstOrDefault(child => child.IsToken)
                : null;
            return token?.Kind == SyntaxKind.NullKeyword;
        }

        private SimpleType CheckBitwiseExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            var operatorText = node.Children.FirstOrDefault(child =>
                child.IsToken &&
                child.Kind is SyntaxKind.PipeToken or SyntaxKind.AmpersandToken or SyntaxKind.CaretToken or SyntaxKind.TildeToken)?.Text ?? "?";

            if (operatorText == "~")
            {
                if (expressions.Length != 1)
                {
                    foreach (var expression in expressions)
                    {
                        CheckExpression(expression, scope);
                    }

                    return SimpleType.Unknown;
                }

                var operandType = CheckExpression(expressions[0], scope);
                if (!operandType.IsKnown)
                {
                    return SimpleType.Unknown;
                }

                if (IsKnownEnumType(scope, operandType))
                {
                    return operandType;
                }

                if (TryGetUnaryIntegralBitwiseResultType(operandType, out var unaryIntegralResultType))
                {
                    return unaryIntegralResultType;
                }

                if (operandType.IsKnown)
                {
                    ReportMismatch(node, $"Bitwise operator '~' operand must be an integral numeric value or enum value, but found '{operandType}'.");
                }

                return SimpleType.Unknown;
            }

            if (expressions.Length != 2)
            {
                foreach (var expression in expressions)
                {
                    CheckExpression(expression, scope);
                }

                return SimpleType.Unknown;
            }

            var leftType = CheckExpression(expressions[0], scope);
            var rightType = CheckExpression(expressions[1], scope);
            if (!leftType.IsKnown || !rightType.IsKnown)
            {
                return SimpleType.Unknown;
            }

            var leftIsEnum = IsKnownEnumType(scope, leftType);
            var rightIsEnum = IsKnownEnumType(scope, rightType);
            if (leftIsEnum || rightIsEnum)
            {
                if (!leftIsEnum || !rightIsEnum)
                {
                    ReportMismatch(node, $"Enum value '{operatorText}' operands must be enum values of the same type, but found '{leftType}' and '{rightType}'.");
                    return SimpleType.Unknown;
                }

                if (!string.Equals(leftType.Name, rightType.Name, StringComparison.Ordinal))
                {
                    ReportMismatch(node, $"Enum value '{operatorText}' operands must have the same enum type, but found '{leftType}' and '{rightType}'.");
                    return SimpleType.Unknown;
                }

                return leftType;
            }

            if (TryGetBinaryIntegralBitwiseResultType(leftType, rightType, out var integralResultType))
            {
                return integralResultType;
            }

            if (TryGetBinaryBooleanBitwiseResultType(leftType, rightType, out var boolResultType))
            {
                return boolResultType;
            }

            if (IsKnownNonNullableBoolType(leftType) || IsKnownNonNullableBoolType(rightType))
            {
                ReportMismatch(node, $"Boolean bitwise operator '{operatorText}' operands must both be 'bool', but found '{leftType}' and '{rightType}'.");
                return SimpleType.Unknown;
            }

            ReportMismatch(node, $"Bitwise operator '{operatorText}' operands must be integral numeric values or boolean values of a supported primitive type, but found '{leftType}' and '{rightType}'.");
            return SimpleType.Unknown;
        }

        private static bool IsBitwiseExpression(SyntaxNode node) =>
            node.Kind == SyntaxKind.BinaryExpression &&
            node.Children.Any(child => child.IsToken && child.Kind is SyntaxKind.PipeToken or SyntaxKind.AmpersandToken or SyntaxKind.CaretToken or SyntaxKind.TildeToken);

        private SimpleType CheckDirectFunctionCallExpression(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            FunctionInfo function)
        {
            var arguments = GetCallArguments(node).ToArray();
            var hasNamedArguments = arguments.Any(IsNamedArgument);
            var explicitTypeArguments = GetDirectCallTypeArguments(node, scope);
            if (function.TypeParameters.Count > 0 || explicitTypeArguments.Count > 0)
            {
                if (hasNamedArguments)
                {
                    return CheckDirectGenericNamedFunctionCallExpression(
                        node,
                        scope,
                        functionName,
                        function,
                        arguments,
                        explicitTypeArguments);
                }

                return CheckDirectGenericFunctionCallExpression(
                    node,
                    scope,
                    functionName,
                    function,
                    arguments,
                    explicitTypeArguments);
            }

            if (function.ParameterTypes is not { } parameterTypes)
            {
                if (hasNamedArguments)
                {
                    ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for function '{functionName}'.");
                }

                CheckArgumentExpressions(arguments, scope);
                return function.ReturnType;
            }

            if (hasNamedArguments)
            {
                if (TryGetParamsParameter(parameterTypes, function.ParamsParameterIndex, out _, out _, out _))
                {
                    ReportMismatch(
                        node,
                        $"Named arguments cannot be used with TypeSharp params parameter function '{functionName}' in this slice.");
                    CheckArgumentExpressions(arguments, scope);
                    return function.ReturnType;
                }

                return CheckDirectNamedFunctionCallExpression(
                    node,
                    scope,
                    functionName,
                    function.ReturnType,
                    parameterTypes,
                    function.ParameterNames,
                    function.OptionalParameterFlags,
                    arguments);
            }

            if (TryGetParamsParameter(parameterTypes, function.ParamsParameterIndex, out var paramsIndex, out var paramsArrayType, out var paramsElementType))
            {
                return CheckDirectFunctionParamsCallExpression(
                    node,
                    scope,
                    functionName,
                    function.ReturnType,
                    parameterTypes,
                    arguments,
                    paramsIndex,
                    paramsArrayType,
                    paramsElementType);
            }

            ReportDirectFunctionArityMismatch(
                node,
                functionName,
                parameterTypes,
                function.OptionalParameterFlags,
                arguments.Length);

            for (var index = 0; index < arguments.Length; index++)
            {
                var argument = arguments[index];
                if (index >= parameterTypes.Count || !parameterTypes[index].IsKnown)
                {
                    CheckExpression(argument, scope);
                    continue;
                }

                CheckDirectFunctionArgument(node, scope, functionName, argument, index, parameterTypes[index]);
            }

            return function.ReturnType;
        }

        private SimpleType CheckDirectFunctionParamsCallExpression(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            SimpleType returnType,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<SyntaxNode> arguments,
            int paramsIndex,
            SimpleType paramsArrayType,
            SimpleType paramsElementType)
        {
            if (arguments.Count < paramsIndex)
            {
                ReportMismatch(
                    node,
                    $"Function '{functionName}' expects at least {FormatArgumentCount(paramsIndex)}, but call supplies {FormatArgumentCount(arguments.Count)}.");
            }

            for (var index = 0; index < Math.Min(arguments.Count, paramsIndex); index++)
            {
                if (!parameterTypes[index].IsKnown)
                {
                    CheckExpression(arguments[index], scope);
                    continue;
                }

                CheckDirectFunctionArgument(node, scope, functionName, arguments[index], index, parameterTypes[index]);
            }

            if (arguments.Count < paramsIndex)
            {
                return returnType;
            }

            if (arguments.Count == parameterTypes.Count)
            {
                var paramsArgument = arguments[paramsIndex];
                if (paramsArgument.Kind == SyntaxKind.CollectionExpression)
                {
                    CheckDirectFunctionArgument(node, scope, functionName, paramsArgument, paramsIndex, paramsArrayType);
                    return returnType;
                }

                var argumentType = CheckExpression(paramsArgument, scope);
                if (!argumentType.IsKnown || CanPassToExpectedSilently(scope, paramsArrayType, argumentType))
                {
                    ValidateDirectFunctionArgumentType(node, scope, functionName, paramsIndex, paramsArrayType, argumentType);
                    return returnType;
                }

                ValidateDirectFunctionArgumentType(node, scope, functionName, paramsIndex, paramsElementType, argumentType);
                return returnType;
            }

            for (var index = paramsIndex; index < arguments.Count; index++)
            {
                CheckDirectFunctionArgument(node, scope, functionName, arguments[index], index, paramsElementType);
            }

            return returnType;
        }

        private SimpleType CheckDirectNamedFunctionCallExpression(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            SimpleType returnType,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<string>? parameterNames,
            IReadOnlyList<bool>? optionalParameterFlags,
            IReadOnlyList<SyntaxNode> arguments)
        {
            if (!TryBuildParameterIndex(parameterNames, parameterTypes.Count, out var parameterIndexes))
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for function '{functionName}'.");
                CheckArgumentExpressions(arguments, scope);
                return returnType;
            }

            var knownParameterNames = parameterNames!;
            var suppliedParameters = new bool[parameterTypes.Count];
            var boundArguments = new List<BoundCallArgument>();
            var nextPositionalParameter = 0;
            var sawNamedArgument = false;

            foreach (var argument in arguments)
            {
                if (TryGetNamedArgumentName(argument, out var argumentName))
                {
                    sawNamedArgument = true;
                    var argumentExpression = GetArgumentExpression(argument);
                    if (!parameterIndexes.TryGetValue(argumentName, out var parameterIndex))
                    {
                        ReportMismatch(argument, $"Function '{functionName}' has no parameter named '{argumentName}'.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (suppliedParameters[parameterIndex])
                    {
                        ReportMismatch(argument, $"Function '{functionName}' parameter '{argumentName}' is supplied more than once.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    suppliedParameters[parameterIndex] = true;
                    boundArguments.Add(new BoundCallArgument(argument, parameterIndex));
                    continue;
                }

                if (sawNamedArgument)
                {
                    ReportMismatch(argument, $"Function '{functionName}' positional arguments cannot follow named arguments.");
                }

                var positionalIndex = nextPositionalParameter++;
                if (positionalIndex >= parameterTypes.Count)
                {
                    ReportMismatch(
                        argument,
                        $"Function '{functionName}' expects {FormatArgumentCount(parameterTypes.Count)}, but call supplies more arguments.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                if (suppliedParameters[positionalIndex])
                {
                    var parameterName = knownParameterNames[positionalIndex];
                    ReportMismatch(argument, $"Function '{functionName}' parameter '{parameterName}' is supplied more than once.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                suppliedParameters[positionalIndex] = true;
                boundArguments.Add(new BoundCallArgument(argument, positionalIndex));
            }

            ReportMissingNamedArguments(node, functionName, parameterTypes, knownParameterNames, optionalParameterFlags, suppliedParameters);
            foreach (var boundArgument in boundArguments)
            {
                var expectedType = parameterTypes[boundArgument.ParameterIndex];
                var expression = GetArgumentExpression(boundArgument.Argument);
                if (!expectedType.IsKnown)
                {
                    CheckExpression(expression, scope);
                    continue;
                }

                CheckDirectFunctionArgument(node, scope, functionName, expression, boundArgument.ParameterIndex, expectedType);
            }

            return returnType;
        }

        private SimpleType CheckDirectGenericNamedFunctionCallExpression(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            FunctionInfo function,
            IReadOnlyList<SyntaxNode> arguments,
            IReadOnlyList<SimpleType> explicitTypeArguments)
        {
            var substitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);
            var typeParameterNames = new HashSet<string>(function.TypeParameters, StringComparer.Ordinal);

            if (explicitTypeArguments.Count > 0)
            {
                if (explicitTypeArguments.Count != function.TypeParameters.Count)
                {
                    ReportMismatch(
                        node,
                        $"Function '{functionName}' expects {FormatGenericTypeArgumentCount(function.TypeParameters.Count)}, but call supplies {FormatGenericTypeArgumentCount(explicitTypeArguments.Count)}.");
                    CheckArgumentExpressions(arguments, scope);
                    return SimpleType.Unknown;
                }

                for (var index = 0; index < explicitTypeArguments.Count; index++)
                {
                    substitutions[function.TypeParameters[index]] = explicitTypeArguments[index];
                }
            }

            var returnType = SubstituteGenericType(function.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            if (function.ParameterTypes is not { } parameterTypes)
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for function '{functionName}'.");
                CheckArgumentExpressions(arguments, scope);
                return returnType;
            }

            if (TryGetParamsParameter(parameterTypes, function.ParamsParameterIndex, out _, out _, out _))
            {
                ReportMismatch(
                    node,
                    $"Named arguments cannot be used with TypeSharp params parameter function '{functionName}' in this generic slice.");
                CheckArgumentExpressions(arguments, scope);
                return returnType;
            }

            if (!TryBuildParameterIndex(function.ParameterNames, parameterTypes.Count, out var parameterIndexes))
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for function '{functionName}'.");
                CheckArgumentExpressions(arguments, scope);
                return returnType;
            }

            var knownParameterNames = function.ParameterNames!;
            var suppliedParameters = new bool[parameterTypes.Count];
            var boundArguments = new List<BoundCallArgument>();
            var nextPositionalParameter = 0;
            var sawNamedArgument = false;

            foreach (var argument in arguments)
            {
                if (TryGetNamedArgumentName(argument, out var argumentName))
                {
                    sawNamedArgument = true;
                    var argumentExpression = GetArgumentExpression(argument);
                    if (!parameterIndexes.TryGetValue(argumentName, out var parameterIndex))
                    {
                        ReportMismatch(argument, $"Function '{functionName}' has no parameter named '{argumentName}'.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (suppliedParameters[parameterIndex])
                    {
                        ReportMismatch(argument, $"Function '{functionName}' parameter '{argumentName}' is supplied more than once.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    suppliedParameters[parameterIndex] = true;
                    boundArguments.Add(new BoundCallArgument(argument, parameterIndex));
                    continue;
                }

                if (sawNamedArgument)
                {
                    ReportMismatch(argument, $"Function '{functionName}' positional arguments cannot follow named arguments.");
                }

                var positionalIndex = nextPositionalParameter++;
                if (positionalIndex >= parameterTypes.Count)
                {
                    ReportMismatch(
                        argument,
                        $"Function '{functionName}' expects {FormatArgumentCount(parameterTypes.Count)}, but call supplies more arguments.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                if (suppliedParameters[positionalIndex])
                {
                    var parameterName = knownParameterNames[positionalIndex];
                    ReportMismatch(argument, $"Function '{functionName}' parameter '{parameterName}' is supplied more than once.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                suppliedParameters[positionalIndex] = true;
                boundArguments.Add(new BoundCallArgument(argument, positionalIndex));
            }

            ReportMissingNamedArguments(node, functionName, parameterTypes, knownParameterNames, function.OptionalParameterFlags, suppliedParameters);
            if (explicitTypeArguments.Count > 0)
            {
                foreach (var boundArgument in boundArguments)
                {
                    var expression = GetArgumentExpression(boundArgument.Argument);
                    var expectedType = SubstituteGenericType(
                        parameterTypes[boundArgument.ParameterIndex],
                        substitutions,
                        typeParameterNames,
                        unresolvedTypeParameterIsUnknown: true);
                    if (!expectedType.IsKnown)
                    {
                        CheckExpression(expression, scope);
                        continue;
                    }

                    CheckDirectFunctionArgument(node, scope, functionName, expression, boundArgument.ParameterIndex, expectedType);
                }

                return returnType;
            }

            var argumentTypes = new SimpleType[parameterTypes.Count];
            foreach (var boundArgument in boundArguments)
            {
                argumentTypes[boundArgument.ParameterIndex] = CheckExpression(GetArgumentExpression(boundArgument.Argument), scope);
            }

            var inconsistentTypeParameters = InferDirectGenericFunctionTypeArguments(
                node,
                $"Function '{functionName}'",
                parameterTypes,
                parameterTypes.Count,
                argumentTypes,
                typeParameterNames,
                substitutions);

            foreach (var boundArgument in boundArguments)
            {
                var parameterType = parameterTypes[boundArgument.ParameterIndex];
                if (GenericTypeReferencesAny(parameterType, inconsistentTypeParameters, typeParameterNames))
                {
                    continue;
                }

                var expectedType = SubstituteGenericType(
                    parameterType,
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown: true);
                if (!expectedType.IsKnown)
                {
                    continue;
                }

                ValidateDirectFunctionArgumentType(
                    node,
                    scope,
                    functionName,
                    boundArgument.ParameterIndex,
                    expectedType,
                    argumentTypes[boundArgument.ParameterIndex]);
            }

            return SubstituteGenericType(function.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
        }

        private SimpleType CheckDirectGenericFunctionCallExpression(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            FunctionInfo function,
            IReadOnlyList<SyntaxNode> arguments,
            IReadOnlyList<SimpleType> explicitTypeArguments)
        {
            var substitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);
            var typeParameterNames = new HashSet<string>(function.TypeParameters, StringComparer.Ordinal);

            if (explicitTypeArguments.Count > 0)
            {
                if (explicitTypeArguments.Count != function.TypeParameters.Count)
                {
                    ReportMismatch(
                        node,
                        $"Function '{functionName}' expects {FormatGenericTypeArgumentCount(function.TypeParameters.Count)}, but call supplies {FormatGenericTypeArgumentCount(explicitTypeArguments.Count)}.");

                    foreach (var argument in arguments)
                    {
                        CheckExpression(argument, scope);
                    }

                    return SimpleType.Unknown;
                }

                for (var index = 0; index < explicitTypeArguments.Count; index++)
                {
                    substitutions[function.TypeParameters[index]] = explicitTypeArguments[index];
                }
            }

            if (function.ParameterTypes is not { } parameterTypes)
            {
                foreach (var argument in arguments)
                {
                    CheckExpression(argument, scope);
                }

                return SubstituteGenericType(function.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            }

            var hasParams = TryGetParamsParameter(parameterTypes, function.ParamsParameterIndex, out var paramsIndex, out _, out _);
            if (hasParams && arguments.Count < paramsIndex)
            {
                ReportMismatch(
                    node,
                    $"Function '{functionName}' expects at least {FormatArgumentCount(paramsIndex)}, but call supplies {FormatArgumentCount(arguments.Count)}.");
            }
            else if (!hasParams)
            {
                ReportDirectFunctionArityMismatch(
                    node,
                    functionName,
                    parameterTypes,
                    function.OptionalParameterFlags,
                    arguments.Count);
            }

            if (explicitTypeArguments.Count > 0)
            {
                var effectiveParameterTypes = GetEffectiveParamsParameterTypes(
                    scope,
                    parameterTypes,
                    arguments,
                    argumentTypes: null,
                    function.ParamsParameterIndex,
                    substitutions,
                    typeParameterNames);

                for (var index = 0; index < arguments.Count; index++)
                {
                    var argument = arguments[index];
                    if (index >= effectiveParameterTypes.Count)
                    {
                        CheckExpression(argument, scope);
                        continue;
                    }

                    var expectedType = SubstituteGenericType(effectiveParameterTypes[index], substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
                    if (!expectedType.IsKnown)
                    {
                        CheckExpression(argument, scope);
                        continue;
                    }

                    CheckDirectFunctionArgument(node, scope, functionName, argument, index, expectedType);
                }

                return SubstituteGenericType(function.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            }

            var argumentTypes = new SimpleType[arguments.Count];
            for (var index = 0; index < arguments.Count; index++)
            {
                argumentTypes[index] = CheckExpression(arguments[index], scope);
            }

            var inferredParameterTypes = GetEffectiveParamsParameterTypes(
                scope,
                parameterTypes,
                arguments,
                argumentTypes,
                function.ParamsParameterIndex,
                substitutions: null,
                typeParameterNames);

            var inconsistentTypeParameters = InferDirectGenericFunctionTypeArguments(
                node,
                $"Function '{functionName}'",
                inferredParameterTypes,
                arguments.Count,
                argumentTypes,
                typeParameterNames,
                substitutions);

            for (var index = 0; index < arguments.Count; index++)
            {
                if (index >= inferredParameterTypes.Count)
                {
                    continue;
                }

                var parameterType = inferredParameterTypes[index];
                if (GenericTypeReferencesAny(parameterType, inconsistentTypeParameters, typeParameterNames))
                {
                    continue;
                }

                var expectedType = SubstituteGenericType(parameterType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
                if (!expectedType.IsKnown)
                {
                    continue;
                }

                ValidateDirectFunctionArgumentType(node, scope, functionName, index, expectedType, argumentTypes[index]);
            }

            return SubstituteGenericType(function.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
        }

        private HashSet<string> InferDirectGenericFunctionTypeArguments(
            SyntaxNode node,
            string subjectDescription,
            IReadOnlyList<SimpleType> parameterTypes,
            int argumentCount,
            IReadOnlyList<SimpleType> argumentTypes,
            IReadOnlySet<string> typeParameterNames,
            Dictionary<string, SimpleType> substitutions)
        {
            var inferredFromArgument = new Dictionary<string, int>(StringComparer.Ordinal);
            var inconsistentTypeParameters = new HashSet<string>(StringComparer.Ordinal);
            var limit = Math.Min(argumentCount, parameterTypes.Count);
            for (var index = 0; index < limit; index++)
            {
                if (!TryInferDirectGenericArgument(
                    parameterTypes[index],
                    argumentTypes[index],
                    typeParameterNames,
                    out var typeParameterName,
                    out var inferredArgumentType))
                {
                    continue;
                }

                if (!substitutions.TryGetValue(typeParameterName, out var inferredType))
                {
                    substitutions[typeParameterName] = inferredArgumentType;
                    inferredFromArgument[typeParameterName] = index;
                    continue;
                }

                if (SameSimpleType(inferredType, inferredArgumentType))
                {
                    continue;
                }

                inconsistentTypeParameters.Add(typeParameterName);
                var previousIndex = inferredFromArgument.TryGetValue(typeParameterName, out var inferredIndex)
                    ? inferredIndex + 1
                    : 1;
                ReportMismatch(
                    node,
                    $"{subjectDescription} cannot infer generic type parameter '{typeParameterName}' consistently: argument {previousIndex} inferred '{inferredType}', but argument {index + 1} inferred '{inferredArgumentType}'.");
            }

            return inconsistentTypeParameters;
        }

        private IReadOnlyList<SimpleType> GetEffectiveParamsParameterTypes(
            TypeScope scope,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<SyntaxNode> arguments,
            IReadOnlyList<SimpleType>? argumentTypes,
            int? paramsParameterIndex,
            IReadOnlyDictionary<string, SimpleType>? substitutions,
            IReadOnlySet<string>? typeParameterNames)
        {
            if (!TryGetParamsParameter(parameterTypes, paramsParameterIndex, out var paramsIndex, out var paramsArrayType, out var paramsElementType) ||
                arguments.Count < paramsIndex)
            {
                return parameterTypes;
            }

            if (arguments.Count == parameterTypes.Count)
            {
                if (arguments[paramsIndex].Kind == SyntaxKind.CollectionExpression)
                {
                    return parameterTypes;
                }

                if (argumentTypes is not null)
                {
                    var comparisonArrayType = paramsArrayType;
                    if (substitutions is not null && typeParameterNames is not null)
                    {
                        comparisonArrayType = SubstituteGenericType(
                            comparisonArrayType,
                            substitutions,
                            typeParameterNames,
                            unresolvedTypeParameterIsUnknown: true);
                    }

                    var argumentType = argumentTypes[paramsIndex];
                    if (!argumentType.IsKnown ||
                        CanPassToExpectedSilently(scope, comparisonArrayType, argumentType) ||
                        (typeParameterNames is not null &&
                            TryInferDirectGenericArgument(paramsArrayType, argumentType, typeParameterNames, out _, out _)))
                    {
                        return parameterTypes;
                    }
                }
            }

            var effectiveParameterTypes = new List<SimpleType>();
            for (var index = 0; index < paramsIndex; index++)
            {
                effectiveParameterTypes.Add(parameterTypes[index]);
            }

            for (var index = paramsIndex; index < arguments.Count; index++)
            {
                effectiveParameterTypes.Add(paramsElementType);
            }

            return effectiveParameterTypes;
        }

        private IReadOnlyList<SimpleType> GetEffectiveParamsParameterTypesForArgumentTypes(
            TypeScope scope,
            IReadOnlyList<SimpleType> parameterTypes,
            int argumentCount,
            IReadOnlyList<SimpleType>? argumentTypes,
            int? paramsParameterIndex,
            IReadOnlyDictionary<string, SimpleType>? substitutions,
            IReadOnlySet<string>? typeParameterNames)
        {
            if (!TryGetParamsParameter(parameterTypes, paramsParameterIndex, out var paramsIndex, out var paramsArrayType, out var paramsElementType) ||
                argumentCount < paramsIndex)
            {
                return parameterTypes;
            }

            if (argumentCount == parameterTypes.Count && argumentTypes is not null)
            {
                var comparisonArrayType = paramsArrayType;
                if (substitutions is not null && typeParameterNames is not null)
                {
                    comparisonArrayType = SubstituteGenericType(
                        comparisonArrayType,
                        substitutions,
                        typeParameterNames,
                        unresolvedTypeParameterIsUnknown: true);
                }

                var argumentType = argumentTypes[paramsIndex];
                if (!argumentType.IsKnown ||
                    CanPassToExpectedSilently(scope, comparisonArrayType, argumentType) ||
                    (typeParameterNames is not null &&
                        TryInferDirectGenericArgument(paramsArrayType, argumentType, typeParameterNames, out _, out _)))
                {
                    return parameterTypes;
                }
            }

            var effectiveParameterTypes = new List<SimpleType>();
            for (var index = 0; index < paramsIndex; index++)
            {
                effectiveParameterTypes.Add(parameterTypes[index]);
            }

            for (var index = paramsIndex; index < argumentCount; index++)
            {
                effectiveParameterTypes.Add(paramsElementType);
            }

            return effectiveParameterTypes;
        }

        private static bool TryGetParamsParameter(
            IReadOnlyList<SimpleType> parameterTypes,
            int? paramsParameterIndex,
            out int paramsIndex,
            out SimpleType paramsArrayType,
            out SimpleType paramsElementType)
        {
            paramsIndex = paramsParameterIndex ?? -1;
            paramsArrayType = SimpleType.Unknown;
            paramsElementType = SimpleType.Unknown;
            if (paramsIndex < 0 || paramsIndex >= parameterTypes.Count)
            {
                return false;
            }

            paramsArrayType = parameterTypes[paramsIndex];
            return TryGetArrayElementType(paramsArrayType, out paramsElementType);
        }

        private bool CanPassToExpectedSilently(TypeScope scope, SimpleType expectedType, SimpleType argumentType)
        {
            return argumentType.IsKnown &&
                !IsNullabilityViolation(expectedType, argumentType) &&
                !TryGetStructuralAssignmentDiagnostic(scope, expectedType, argumentType, out _) &&
                CanAssign(scope, expectedType, argumentType);
        }

        private void ReportDirectFunctionArityMismatch(
            SyntaxNode node,
            string functionName,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<bool>? optionalParameterFlags,
            int suppliedArgumentCount)
        {
            var requiredCount = GetRequiredParameterCount(parameterTypes, optionalParameterFlags);
            if (suppliedArgumentCount < requiredCount)
            {
                var expects = requiredCount == parameterTypes.Count
                    ? FormatArgumentCount(parameterTypes.Count)
                    : $"at least {FormatArgumentCount(requiredCount)}";
                ReportMismatch(
                    node,
                    $"Function '{functionName}' expects {expects}, but call supplies {FormatArgumentCount(suppliedArgumentCount)}.");
                return;
            }

            if (suppliedArgumentCount > parameterTypes.Count)
            {
                var expects = requiredCount == parameterTypes.Count
                    ? FormatArgumentCount(parameterTypes.Count)
                    : $"at most {FormatArgumentCount(parameterTypes.Count)}";
                ReportMismatch(
                    node,
                    $"Function '{functionName}' expects {expects}, but call supplies {FormatArgumentCount(suppliedArgumentCount)}.");
            }
        }

        private void ReportPipelineArityMismatch(
            SyntaxNode node,
            string targetName,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<bool>? optionalParameterFlags,
            int suppliedArgumentCount)
        {
            var requiredCount = GetRequiredParameterCount(parameterTypes, optionalParameterFlags);
            if (suppliedArgumentCount < requiredCount)
            {
                var expects = requiredCount == parameterTypes.Count
                    ? FormatArgumentCount(parameterTypes.Count)
                    : $"at least {FormatArgumentCount(requiredCount)}";
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects {expects} after pipeline lowering, but pipeline supplies {FormatArgumentCount(suppliedArgumentCount)}.");
                return;
            }

            if (suppliedArgumentCount > parameterTypes.Count)
            {
                var expects = requiredCount == parameterTypes.Count
                    ? FormatArgumentCount(parameterTypes.Count)
                    : $"at most {FormatArgumentCount(parameterTypes.Count)}";
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects {expects} after pipeline lowering, but pipeline supplies {FormatArgumentCount(suppliedArgumentCount)}.");
            }
        }

        private static int GetRequiredParameterCount(
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<bool>? optionalParameterFlags)
        {
            if (optionalParameterFlags is null || optionalParameterFlags.Count != parameterTypes.Count)
            {
                return parameterTypes.Count;
            }

            for (var index = 0; index < optionalParameterFlags.Count; index++)
            {
                if (optionalParameterFlags[index])
                {
                    return index;
                }
            }

            return parameterTypes.Count;
        }

        private static bool IsOptionalParameter(int parameterIndex, IReadOnlyList<bool>? optionalParameterFlags) =>
            optionalParameterFlags is not null &&
            parameterIndex >= 0 &&
            parameterIndex < optionalParameterFlags.Count &&
            optionalParameterFlags[parameterIndex];

        private static bool TryBuildParameterIndex(
            IReadOnlyList<string>? parameterNames,
            int parameterCount,
            out IReadOnlyDictionary<string, int> parameterIndexes)
        {
            var indexes = new Dictionary<string, int>(StringComparer.Ordinal);
            parameterIndexes = indexes;
            if (parameterNames is null || parameterNames.Count != parameterCount)
            {
                return false;
            }

            for (var index = 0; index < parameterNames.Count; index++)
            {
                var name = parameterNames[index];
                if (string.IsNullOrWhiteSpace(name) || indexes.ContainsKey(name))
                {
                    return false;
                }

                indexes[name] = index;
            }

            return true;
        }

        private void ReportMissingNamedArguments(
            SyntaxNode node,
            string functionName,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<string> parameterNames,
            IReadOnlyList<bool>? optionalParameterFlags,
            IReadOnlyList<bool> suppliedParameters)
        {
            for (var index = 0; index < parameterTypes.Count; index++)
            {
                if (suppliedParameters[index] || IsOptionalParameter(index, optionalParameterFlags))
                {
                    continue;
                }

                ReportMismatch(node, $"Function '{functionName}' requires argument for parameter '{parameterNames[index]}'.");
            }
        }

        private void ReportMissingPipelineNamedArguments(
            SyntaxNode node,
            string targetName,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<string> parameterNames,
            IReadOnlyList<bool>? optionalParameterFlags,
            IReadOnlyList<bool> suppliedParameters)
        {
            for (var index = 0; index < parameterTypes.Count; index++)
            {
                if (suppliedParameters[index] || IsOptionalParameter(index, optionalParameterFlags))
                {
                    continue;
                }

                ReportMismatch(node, $"Pipeline target '{targetName}' requires argument for parameter '{parameterNames[index]}' after pipeline lowering.");
            }
        }

        private void CheckDirectFunctionArgument(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            SyntaxNode argument,
            int parameterIndex,
            SimpleType expectedType)
        {
            var argumentType = CheckExpressionWithExpected(argument, scope, expectedType);
            ValidateDirectFunctionArgumentType(node, scope, functionName, parameterIndex, expectedType, argumentType);
        }

        private void ValidateDirectFunctionArgumentType(
            SyntaxNode node,
            TypeScope scope,
            string functionName,
            int parameterIndex,
            SimpleType expectedType,
            SimpleType argumentType)
        {
            if (!argumentType.IsKnown)
            {
                return;
            }

            if (IsNullabilityViolation(expectedType, argumentType))
            {
                ReportMismatch(
                    node,
                    argumentType.IsNull
                        ? $"Function '{functionName}' argument {parameterIndex + 1} cannot be null because it expects non-null type '{expectedType}'."
                        : $"Function '{functionName}' argument {parameterIndex + 1} expects non-null type '{expectedType}', but found nullable type '{argumentType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, expectedType, argumentType, out var structuralMessage))
            {
                ReportMismatch(node, structuralMessage);
            }
            else if (!CanAssign(scope, expectedType, argumentType))
            {
                ReportMismatch(
                    node,
                    $"Function '{functionName}' argument {parameterIndex + 1} expects '{expectedType}', but found '{argumentType}'.");
            }
        }

        private static IEnumerable<SyntaxNode> GetCallArguments(SyntaxNode call) =>
            call.Kind == SyntaxKind.CallExpression
                ? call.Children.Skip(1).Where(child => !child.IsToken)
                : Enumerable.Empty<SyntaxNode>();

        private void CheckArgumentExpressions(IEnumerable<SyntaxNode> arguments, TypeScope scope)
        {
            foreach (var argument in arguments)
            {
                CheckExpression(GetArgumentExpression(argument), scope);
            }
        }

        private static bool IsNamedArgument(SyntaxNode argument) =>
            argument.Kind == SyntaxKind.NamedArgument;

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

        private static SyntaxNode GetArgumentExpression(SyntaxNode argument) =>
            argument.Kind == SyntaxKind.NamedArgument
                ? argument.Children.LastOrDefault(child => !child.IsToken) ?? argument
                : argument;

        private SimpleType CheckPipelineExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                foreach (var expression in expressions)
                {
                    CheckExpression(expression, scope);
                }

                return SimpleType.Unknown;
            }

            var input = expressions[0];
            var target = expressions[1];
            var inputType = CheckExpression(input, scope);
            if (!TryGetPipelineTargetFunctionName(target, out var targetName) ||
                !scope.ResolveFunctionInfo(targetName, out var targetFunction))
            {
                CheckExpression(target, scope);
                return SimpleType.Unknown;
            }

            return CheckPipelineFunctionApplication(node, scope, targetName, targetFunction, target, inputType);
        }

        private SimpleType CheckPipelineFunctionApplication(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            FunctionInfo targetFunction,
            SyntaxNode target,
            SimpleType inputType)
        {
            IReadOnlyList<SyntaxNode> targetArguments = GetPipelineTargetArguments(target).ToArray();
            var hasNamedArguments = targetArguments.Any(IsNamedArgument);
            if (targetFunction.TypeParameters.Count > 0)
            {
                if (hasNamedArguments)
                {
                    return CheckGenericPipelineNamedFunctionApplication(
                        node,
                        scope,
                        targetName,
                        targetFunction,
                        target,
                        inputType,
                        targetArguments);
                }

                return CheckGenericPipelineFunctionApplication(
                    node,
                    scope,
                    targetName,
                    targetFunction,
                    target,
                    inputType,
                    targetArguments);
            }

            if (targetFunction.ParameterTypes is not { } parameterTypes)
            {
                if (hasNamedArguments)
                {
                    ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for pipeline target '{targetName}'.");
                }

                CheckArgumentExpressions(targetArguments, scope);
                return targetFunction.ReturnType;
            }

            if (hasNamedArguments)
            {
                if (TryGetParamsParameter(parameterTypes, targetFunction.ParamsParameterIndex, out _, out _, out _))
                {
                    ReportMismatch(
                        node,
                        $"Named arguments cannot be used with TypeSharp params parameter pipeline target '{targetName}' in this slice.");
                    CheckArgumentExpressions(targetArguments, scope);
                    return targetFunction.ReturnType;
                }

                return CheckPipelineNamedFunctionApplication(
                    node,
                    scope,
                    targetName,
                    targetFunction.ReturnType,
                    inputType,
                    targetArguments,
                    parameterTypes,
                    targetFunction.ParameterNames,
                    targetFunction.OptionalParameterFlags);
            }

            if (TryGetParamsParameter(parameterTypes, targetFunction.ParamsParameterIndex, out var paramsIndex, out var paramsArrayType, out var paramsElementType))
            {
                return CheckPipelineParamsFunctionApplication(
                    node,
                    scope,
                    targetName,
                    targetFunction.ReturnType,
                    inputType,
                    targetArguments,
                    parameterTypes,
                    paramsIndex,
                    paramsArrayType,
                    paramsElementType);
            }

            var suppliedArgumentCount = targetArguments.Count + 1;
            if (parameterTypes.Count == 0)
            {
                ReportMismatch(node, $"Pipeline target '{targetName}' cannot receive a pipeline input because it declares no parameters.");
            }
            else
            {
                ReportPipelineArityMismatch(
                    node,
                    targetName,
                    parameterTypes,
                    targetFunction.OptionalParameterFlags,
                    suppliedArgumentCount);
            }

            if (inputType.IsKnown &&
                parameterTypes.Count > 0 &&
                parameterTypes[0].IsKnown &&
                !CanAssign(scope, parameterTypes[0], inputType))
            {
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects '{parameterTypes[0]}' for its first parameter, but pipeline input has type '{inputType}'.");
            }

            for (var index = 0; index < targetArguments.Count; index++)
            {
                var argument = targetArguments[index];
                var parameterIndex = index + 1;
                if (parameterIndex >= parameterTypes.Count || !parameterTypes[parameterIndex].IsKnown)
                {
                    CheckExpression(argument, scope);
                    continue;
                }

                CheckPipelineArgument(node, scope, targetName, argument, parameterIndex, parameterTypes[parameterIndex]);
            }

            return targetFunction.ReturnType;
        }

        private SimpleType CheckPipelineNamedFunctionApplication(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            SimpleType returnType,
            SimpleType inputType,
            IReadOnlyList<SyntaxNode> targetArguments,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyList<string>? parameterNames,
            IReadOnlyList<bool>? optionalParameterFlags)
        {
            if (!TryBuildParameterIndex(parameterNames, parameterTypes.Count, out var parameterIndexes))
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for pipeline target '{targetName}'.");
                CheckArgumentExpressions(targetArguments, scope);
                return returnType;
            }

            var knownParameterNames = parameterNames!;
            var suppliedParameters = new bool[parameterTypes.Count];
            var boundArguments = new List<BoundCallArgument>();
            if (parameterTypes.Count == 0)
            {
                ReportMismatch(node, $"Pipeline target '{targetName}' cannot receive a pipeline input because it declares no parameters.");
            }
            else
            {
                suppliedParameters[0] = true;
                if (parameterTypes[0].IsKnown)
                {
                    ValidatePipelineInputType(node, scope, targetName, parameterTypes[0], inputType);
                }
            }

            var nextPositionalParameter = 1;
            var sawNamedArgument = false;
            foreach (var argument in targetArguments)
            {
                if (TryGetNamedArgumentName(argument, out var argumentName))
                {
                    sawNamedArgument = true;
                    var argumentExpression = GetArgumentExpression(argument);
                    if (!parameterIndexes.TryGetValue(argumentName, out var parameterIndex))
                    {
                        ReportMismatch(argument, $"Pipeline target '{targetName}' has no parameter named '{argumentName}'.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (parameterIndex == 0)
                    {
                        ReportMismatch(
                            argument,
                            $"Pipeline target '{targetName}' already receives parameter '{argumentName}' from the pipeline input.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (suppliedParameters[parameterIndex])
                    {
                        ReportMismatch(argument, $"Pipeline target '{targetName}' parameter '{argumentName}' is supplied more than once.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    suppliedParameters[parameterIndex] = true;
                    boundArguments.Add(new BoundCallArgument(argument, parameterIndex));
                    continue;
                }

                if (sawNamedArgument)
                {
                    ReportMismatch(argument, $"Pipeline target '{targetName}' positional arguments cannot follow named arguments.");
                }

                var positionalIndex = nextPositionalParameter++;
                if (positionalIndex >= parameterTypes.Count)
                {
                    ReportMismatch(
                        argument,
                        $"Pipeline target '{targetName}' expects {FormatArgumentCount(parameterTypes.Count)} after pipeline lowering, but pipeline supplies more arguments.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                if (suppliedParameters[positionalIndex])
                {
                    var parameterName = knownParameterNames[positionalIndex];
                    ReportMismatch(argument, $"Pipeline target '{targetName}' parameter '{parameterName}' is supplied more than once.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                suppliedParameters[positionalIndex] = true;
                boundArguments.Add(new BoundCallArgument(argument, positionalIndex));
            }

            ReportMissingPipelineNamedArguments(node, targetName, parameterTypes, knownParameterNames, optionalParameterFlags, suppliedParameters);
            foreach (var boundArgument in boundArguments)
            {
                var expectedType = parameterTypes[boundArgument.ParameterIndex];
                var expression = GetArgumentExpression(boundArgument.Argument);
                if (!expectedType.IsKnown)
                {
                    CheckExpression(expression, scope);
                    continue;
                }

                CheckPipelineArgument(node, scope, targetName, expression, boundArgument.ParameterIndex, expectedType);
            }

            return returnType;
        }

        private SimpleType CheckGenericPipelineNamedFunctionApplication(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            FunctionInfo targetFunction,
            SyntaxNode target,
            SimpleType inputType,
            IReadOnlyList<SyntaxNode> targetArguments)
        {
            var substitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);
            var typeParameterNames = new HashSet<string>(targetFunction.TypeParameters, StringComparer.Ordinal);
            var explicitTypeArguments = GetDirectCallTypeArguments(target, scope);

            if (explicitTypeArguments.Count > 0)
            {
                if (explicitTypeArguments.Count != targetFunction.TypeParameters.Count)
                {
                    ReportMismatch(
                        node,
                        $"Pipeline target '{targetName}' expects {FormatGenericTypeArgumentCount(targetFunction.TypeParameters.Count)}, but pipeline supplies {FormatGenericTypeArgumentCount(explicitTypeArguments.Count)}.");
                    CheckArgumentExpressions(targetArguments, scope);
                    return SimpleType.Unknown;
                }

                for (var index = 0; index < explicitTypeArguments.Count; index++)
                {
                    substitutions[targetFunction.TypeParameters[index]] = explicitTypeArguments[index];
                }
            }

            var returnType = SubstituteGenericType(targetFunction.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            if (targetFunction.ParameterTypes is not { } parameterTypes)
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for pipeline target '{targetName}'.");
                CheckArgumentExpressions(targetArguments, scope);
                return returnType;
            }

            if (TryGetParamsParameter(parameterTypes, targetFunction.ParamsParameterIndex, out _, out _, out _))
            {
                ReportMismatch(
                    node,
                    $"Named arguments cannot be used with TypeSharp params parameter pipeline target '{targetName}' in this generic slice.");
                CheckArgumentExpressions(targetArguments, scope);
                return returnType;
            }

            if (!TryBuildParameterIndex(targetFunction.ParameterNames, parameterTypes.Count, out var parameterIndexes))
            {
                ReportMismatch(node, $"Named arguments require a known TypeSharp parameter list for pipeline target '{targetName}'.");
                CheckArgumentExpressions(targetArguments, scope);
                return returnType;
            }

            var knownParameterNames = targetFunction.ParameterNames!;
            var suppliedParameters = new bool[parameterTypes.Count];
            var boundArguments = new List<BoundCallArgument>();
            if (parameterTypes.Count == 0)
            {
                ReportMismatch(node, $"Pipeline target '{targetName}' cannot receive a pipeline input because it declares no parameters.");
            }
            else
            {
                suppliedParameters[0] = true;
            }

            var nextPositionalParameter = 1;
            var sawNamedArgument = false;
            foreach (var argument in targetArguments)
            {
                if (TryGetNamedArgumentName(argument, out var argumentName))
                {
                    sawNamedArgument = true;
                    var argumentExpression = GetArgumentExpression(argument);
                    if (!parameterIndexes.TryGetValue(argumentName, out var parameterIndex))
                    {
                        ReportMismatch(argument, $"Pipeline target '{targetName}' has no parameter named '{argumentName}'.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (parameterIndex == 0)
                    {
                        ReportMismatch(
                            argument,
                            $"Pipeline target '{targetName}' already receives parameter '{argumentName}' from the pipeline input.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    if (suppliedParameters[parameterIndex])
                    {
                        ReportMismatch(argument, $"Pipeline target '{targetName}' parameter '{argumentName}' is supplied more than once.");
                        CheckExpression(argumentExpression, scope);
                        continue;
                    }

                    suppliedParameters[parameterIndex] = true;
                    boundArguments.Add(new BoundCallArgument(argument, parameterIndex));
                    continue;
                }

                if (sawNamedArgument)
                {
                    ReportMismatch(argument, $"Pipeline target '{targetName}' positional arguments cannot follow named arguments.");
                }

                var positionalIndex = nextPositionalParameter++;
                if (positionalIndex >= parameterTypes.Count)
                {
                    ReportMismatch(
                        argument,
                        $"Pipeline target '{targetName}' expects {FormatArgumentCount(parameterTypes.Count)} after pipeline lowering, but pipeline supplies more arguments.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                if (suppliedParameters[positionalIndex])
                {
                    var parameterName = knownParameterNames[positionalIndex];
                    ReportMismatch(argument, $"Pipeline target '{targetName}' parameter '{parameterName}' is supplied more than once.");
                    CheckExpression(GetArgumentExpression(argument), scope);
                    continue;
                }

                suppliedParameters[positionalIndex] = true;
                boundArguments.Add(new BoundCallArgument(argument, positionalIndex));
            }

            ReportMissingPipelineNamedArguments(node, targetName, parameterTypes, knownParameterNames, targetFunction.OptionalParameterFlags, suppliedParameters);
            if (explicitTypeArguments.Count > 0)
            {
                if (parameterTypes.Count > 0)
                {
                    var expectedInputType = SubstituteGenericType(
                        parameterTypes[0],
                        substitutions,
                        typeParameterNames,
                        unresolvedTypeParameterIsUnknown: true);
                    if (expectedInputType.IsKnown)
                    {
                        ValidatePipelineInputType(node, scope, targetName, expectedInputType, inputType);
                    }
                }

                foreach (var boundArgument in boundArguments)
                {
                    var expression = GetArgumentExpression(boundArgument.Argument);
                    var expectedType = SubstituteGenericType(
                        parameterTypes[boundArgument.ParameterIndex],
                        substitutions,
                        typeParameterNames,
                        unresolvedTypeParameterIsUnknown: true);
                    if (!expectedType.IsKnown)
                    {
                        CheckExpression(expression, scope);
                        continue;
                    }

                    CheckPipelineArgument(node, scope, targetName, expression, boundArgument.ParameterIndex, expectedType);
                }

                return returnType;
            }

            var argumentTypes = new SimpleType[parameterTypes.Count];
            if (parameterTypes.Count > 0)
            {
                argumentTypes[0] = inputType;
            }

            foreach (var boundArgument in boundArguments)
            {
                argumentTypes[boundArgument.ParameterIndex] = CheckExpression(GetArgumentExpression(boundArgument.Argument), scope);
            }

            var inconsistentTypeParameters = InferDirectGenericFunctionTypeArguments(
                node,
                $"Pipeline target '{targetName}'",
                parameterTypes,
                parameterTypes.Count,
                argumentTypes,
                typeParameterNames,
                substitutions);

            for (var index = 0; index < parameterTypes.Count; index++)
            {
                if (index > 0 && !suppliedParameters[index])
                {
                    continue;
                }

                var parameterType = parameterTypes[index];
                if (GenericTypeReferencesAny(parameterType, inconsistentTypeParameters, typeParameterNames))
                {
                    continue;
                }

                var expectedType = SubstituteGenericType(
                    parameterType,
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown: true);
                if (!expectedType.IsKnown)
                {
                    continue;
                }

                if (index == 0)
                {
                    ValidatePipelineInputType(node, scope, targetName, expectedType, inputType);
                    continue;
                }

                ValidatePipelineArgumentType(node, scope, targetName, index, expectedType, argumentTypes[index]);
            }

            return SubstituteGenericType(targetFunction.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
        }

        private SimpleType CheckPipelineParamsFunctionApplication(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            SimpleType returnType,
            SimpleType inputType,
            IReadOnlyList<SyntaxNode> targetArguments,
            IReadOnlyList<SimpleType> parameterTypes,
            int paramsIndex,
            SimpleType paramsArrayType,
            SimpleType paramsElementType)
        {
            var suppliedArgumentCount = targetArguments.Count + 1;
            if (suppliedArgumentCount < paramsIndex)
            {
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects at least {FormatArgumentCount(paramsIndex)} after pipeline lowering, but pipeline supplies {FormatArgumentCount(suppliedArgumentCount)}.");
            }

            if (paramsIndex > 0 && parameterTypes[0].IsKnown)
            {
                ValidatePipelineInputType(node, scope, targetName, parameterTypes[0], inputType);
            }

            for (var index = 0; index < targetArguments.Count && index + 1 < paramsIndex; index++)
            {
                var parameterIndex = index + 1;
                if (!parameterTypes[parameterIndex].IsKnown)
                {
                    CheckExpression(targetArguments[index], scope);
                    continue;
                }

                CheckPipelineArgument(node, scope, targetName, targetArguments[index], parameterIndex, parameterTypes[parameterIndex]);
            }

            if (suppliedArgumentCount < paramsIndex)
            {
                return returnType;
            }

            if (suppliedArgumentCount == parameterTypes.Count)
            {
                if (paramsIndex == 0)
                {
                    ValidatePipelineInputType(
                        node,
                        scope,
                        targetName,
                        !inputType.IsKnown || CanPassToExpectedSilently(scope, paramsArrayType, inputType)
                            ? paramsArrayType
                            : paramsElementType,
                        inputType);
                    return returnType;
                }

                var paramsArgument = targetArguments[paramsIndex - 1];
                if (paramsArgument.Kind == SyntaxKind.CollectionExpression)
                {
                    CheckPipelineArgument(node, scope, targetName, paramsArgument, paramsIndex, paramsArrayType);
                    return returnType;
                }

                var argumentType = CheckExpression(paramsArgument, scope);
                if (!argumentType.IsKnown || CanPassToExpectedSilently(scope, paramsArrayType, argumentType))
                {
                    ValidatePipelineArgumentType(node, scope, targetName, paramsIndex, paramsArrayType, argumentType);
                    return returnType;
                }

                ValidatePipelineArgumentType(node, scope, targetName, paramsIndex, paramsElementType, argumentType);
                return returnType;
            }

            if (paramsIndex == 0)
            {
                ValidatePipelineInputType(node, scope, targetName, paramsElementType, inputType);
            }

            for (var index = Math.Max(0, paramsIndex - 1); index < targetArguments.Count; index++)
            {
                CheckPipelineArgument(node, scope, targetName, targetArguments[index], index + 1, paramsElementType);
            }

            return returnType;
        }

        private SimpleType CheckGenericPipelineFunctionApplication(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            FunctionInfo targetFunction,
            SyntaxNode target,
            SimpleType inputType,
            IReadOnlyList<SyntaxNode> targetArguments)
        {
            var substitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);
            var typeParameterNames = new HashSet<string>(targetFunction.TypeParameters, StringComparer.Ordinal);
            var explicitTypeArguments = GetDirectCallTypeArguments(target, scope);

            if (explicitTypeArguments.Count > 0)
            {
                if (explicitTypeArguments.Count != targetFunction.TypeParameters.Count)
                {
                    ReportMismatch(
                        node,
                        $"Pipeline target '{targetName}' expects {FormatGenericTypeArgumentCount(targetFunction.TypeParameters.Count)}, but pipeline supplies {FormatGenericTypeArgumentCount(explicitTypeArguments.Count)}.");

                    foreach (var argument in targetArguments)
                    {
                        CheckExpression(argument, scope);
                    }

                    return SimpleType.Unknown;
                }

                for (var index = 0; index < explicitTypeArguments.Count; index++)
                {
                    substitutions[targetFunction.TypeParameters[index]] = explicitTypeArguments[index];
                }
            }

            if (targetFunction.ParameterTypes is not { } parameterTypes)
            {
                foreach (var argument in targetArguments)
                {
                    CheckExpression(argument, scope);
                }

                return SubstituteGenericType(targetFunction.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            }

            var suppliedArgumentCount = targetArguments.Count + 1;
            var hasParams = TryGetParamsParameter(parameterTypes, targetFunction.ParamsParameterIndex, out var paramsIndex, out _, out _);
            if (parameterTypes.Count == 0)
            {
                ReportMismatch(node, $"Pipeline target '{targetName}' cannot receive a pipeline input because it declares no parameters.");
            }
            else if (hasParams && suppliedArgumentCount < paramsIndex)
            {
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects at least {FormatArgumentCount(paramsIndex)} after pipeline lowering, but pipeline supplies {FormatArgumentCount(suppliedArgumentCount)}.");
            }
            else if (!hasParams)
            {
                ReportPipelineArityMismatch(
                    node,
                    targetName,
                    parameterTypes,
                    targetFunction.OptionalParameterFlags,
                    suppliedArgumentCount);
            }

            if (explicitTypeArguments.Count > 0)
            {
                var effectiveParameterTypes = GetEffectiveParamsParameterTypesForArgumentTypes(
                    scope,
                    parameterTypes,
                    suppliedArgumentCount,
                    argumentTypes: null,
                    targetFunction.ParamsParameterIndex,
                    substitutions,
                    typeParameterNames);

                ValidateGenericPipelineArgumentsWithSubstitutions(
                    node,
                    scope,
                    targetName,
                    inputType,
                    targetArguments,
                    effectiveParameterTypes,
                    substitutions,
                    typeParameterNames);

                return SubstituteGenericType(targetFunction.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
            }

            var argumentTypes = new SimpleType[suppliedArgumentCount];
            argumentTypes[0] = inputType;
            for (var index = 0; index < targetArguments.Count; index++)
            {
                argumentTypes[index + 1] = CheckExpression(targetArguments[index], scope);
            }

            var inferredParameterTypes = GetEffectiveParamsParameterTypesForArgumentTypes(
                scope,
                parameterTypes,
                suppliedArgumentCount,
                argumentTypes,
                targetFunction.ParamsParameterIndex,
                substitutions: null,
                typeParameterNames);

            var inconsistentTypeParameters = InferDirectGenericFunctionTypeArguments(
                node,
                $"Pipeline target '{targetName}'",
                inferredParameterTypes,
                suppliedArgumentCount,
                argumentTypes,
                typeParameterNames,
                substitutions);

            for (var index = 0; index < suppliedArgumentCount; index++)
            {
                if (index >= inferredParameterTypes.Count)
                {
                    continue;
                }

                var parameterType = inferredParameterTypes[index];
                if (GenericTypeReferencesAny(parameterType, inconsistentTypeParameters, typeParameterNames))
                {
                    continue;
                }

                var expectedType = SubstituteGenericType(parameterType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
                if (!expectedType.IsKnown)
                {
                    continue;
                }

                if (index == 0)
                {
                    ValidatePipelineInputType(node, scope, targetName, expectedType, inputType);
                    continue;
                }

                ValidatePipelineArgumentType(node, scope, targetName, index, expectedType, argumentTypes[index]);
            }

            return SubstituteGenericType(targetFunction.ReturnType, substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
        }

        private void ValidateGenericPipelineArgumentsWithSubstitutions(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            SimpleType inputType,
            IReadOnlyList<SyntaxNode> targetArguments,
            IReadOnlyList<SimpleType> parameterTypes,
            IReadOnlyDictionary<string, SimpleType> substitutions,
            IReadOnlySet<string> typeParameterNames)
        {
            if (parameterTypes.Count > 0)
            {
                var expectedInputType = SubstituteGenericType(parameterTypes[0], substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
                if (expectedInputType.IsKnown)
                {
                    ValidatePipelineInputType(node, scope, targetName, expectedInputType, inputType);
                }
            }

            for (var index = 0; index < targetArguments.Count; index++)
            {
                var argument = targetArguments[index];
                var parameterIndex = index + 1;
                if (parameterIndex >= parameterTypes.Count)
                {
                    CheckExpression(argument, scope);
                    continue;
                }

                var expectedType = SubstituteGenericType(parameterTypes[parameterIndex], substitutions, typeParameterNames, unresolvedTypeParameterIsUnknown: true);
                if (!expectedType.IsKnown)
                {
                    CheckExpression(argument, scope);
                    continue;
                }

                CheckPipelineArgument(node, scope, targetName, argument, parameterIndex, expectedType);
            }
        }

        private void ValidatePipelineInputType(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            SimpleType expectedType,
            SimpleType inputType)
        {
            if (inputType.IsKnown &&
                !CanAssign(scope, expectedType, inputType))
            {
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' expects '{expectedType}' for its first parameter, but pipeline input has type '{inputType}'.");
            }
        }

        private void CheckPipelineArgument(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            SyntaxNode argument,
            int parameterIndex,
            SimpleType expectedType)
        {
            var argumentType = CheckExpressionWithExpected(argument, scope, expectedType);
            ValidatePipelineArgumentType(node, scope, targetName, parameterIndex, expectedType, argumentType);
        }

        private void ValidatePipelineArgumentType(
            SyntaxNode node,
            TypeScope scope,
            string targetName,
            int parameterIndex,
            SimpleType expectedType,
            SimpleType argumentType)
        {
            if (!argumentType.IsKnown)
            {
                return;
            }

            if (IsNullabilityViolation(expectedType, argumentType))
            {
                ReportMismatch(
                    node,
                    argumentType.IsNull
                        ? $"Pipeline target '{targetName}' argument {parameterIndex + 1} cannot be null because it expects non-null type '{expectedType}'."
                        : $"Pipeline target '{targetName}' argument {parameterIndex + 1} expects non-null type '{expectedType}', but found nullable type '{argumentType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, expectedType, argumentType, out var structuralMessage))
            {
                ReportMismatch(node, structuralMessage);
            }
            else if (!CanAssign(scope, expectedType, argumentType))
            {
                ReportMismatch(
                    node,
                    $"Pipeline target '{targetName}' argument {parameterIndex + 1} expects '{expectedType}', but found '{argumentType}'.");
            }
        }

        private static IEnumerable<SyntaxNode> GetPipelineTargetArguments(SyntaxNode target) =>
            target.Kind == SyntaxKind.CallExpression
                ? target.Children.Skip(1).Where(child => !child.IsToken)
                : Enumerable.Empty<SyntaxNode>();

        private static string FormatArgumentCount(int count) =>
            count == 1 ? "1 argument" : $"{count} arguments";

        private static string FormatGenericTypeArgumentCount(int count) =>
            count == 1 ? "1 generic type argument" : $"{count} generic type arguments";

        private static bool IsPipelineExpression(SyntaxNode node) =>
            node.Kind == SyntaxKind.BinaryExpression &&
            node.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.PipeGreaterToken);

        private SimpleType CheckCompositionExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length != 2)
            {
                foreach (var expression in expressions)
                {
                    CheckExpression(expression, scope);
                }

                return SimpleType.Unknown;
            }

            var operatorText = TryGetCompositionOperatorText(node.Children, out var compositionOperator)
                ? compositionOperator
                : ">>";
            var leftType = CheckExpression(expressions[0], scope);
            var rightType = CheckExpression(expressions[1], scope);
            if (TryGetBinaryIntegralShiftResultType(leftType, rightType, out var shiftResultType))
            {
                return shiftResultType;
            }

            if (ShouldReportShiftOperandDiagnostic(scope, leftType, rightType))
            {
                ReportMismatch(
                    node,
                    $"Shift operator '{operatorText}' operands must be non-null primitive integral values with an int-compatible shift count, but found '{leftType}' and '{rightType}'.");
                return SimpleType.Unknown;
            }

            CheckNamedFunctionComposition(node, scope, expressions[0], expressions[1], operatorText);
            return SimpleType.Unknown;
        }

        private SimpleType CheckLogicalUnsignedShiftExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length != 2)
            {
                foreach (var expression in expressions)
                {
                    CheckExpression(expression, scope);
                }

                return SimpleType.Unknown;
            }

            var leftType = CheckExpression(expressions[0], scope);
            var rightType = CheckExpression(expressions[1], scope);
            if (TryGetBinaryIntegralShiftResultType(leftType, rightType, out var shiftResultType))
            {
                return shiftResultType;
            }

            if (ShouldReportLogicalUnsignedShiftOperandDiagnostic(scope, expressions[0], leftType, expressions[1], rightType))
            {
                ReportMismatch(
                    node,
                    $"Shift operator '>>>' operands must be non-null primitive integral values with an int-compatible shift count, but found '{leftType}' and '{rightType}'.");
            }

            return SimpleType.Unknown;
        }

        private void CheckNamedFunctionComposition(
            SyntaxNode node,
            TypeScope scope,
            SyntaxNode leftExpression,
            SyntaxNode rightExpression,
            string operatorText)
        {
            if (!TryGetDirectIdentifierName(leftExpression, out var leftName) ||
                !TryGetDirectIdentifierName(rightExpression, out var rightName) ||
                !scope.ResolveFunctionInfo(leftName, out var leftFunction) ||
                !scope.ResolveFunctionInfo(rightName, out var rightFunction) ||
                !TryGetUnaryFunctionSignature(leftFunction, out var leftParameterType, out var leftReturnType) ||
                !TryGetUnaryFunctionSignature(rightFunction, out var rightParameterType, out var rightReturnType))
            {
                return;
            }

            var firstName = leftName;
            var firstFunction = leftFunction;
            var firstReturnType = leftReturnType;
            var secondName = rightName;
            var secondFunction = rightFunction;
            var secondParameterType = rightParameterType;
            if (operatorText == "<<")
            {
                firstName = rightName;
                firstFunction = rightFunction;
                firstReturnType = rightReturnType;
                secondName = leftName;
                secondFunction = leftFunction;
                secondParameterType = leftParameterType;
            }

            ResolveCompositionGenericEdgeTypes(
                firstFunction,
                firstReturnType,
                secondFunction,
                secondParameterType,
                out firstReturnType,
                out secondParameterType);

            if (!firstReturnType.IsKnown || !secondParameterType.IsKnown)
            {
                return;
            }

            if (CanAssign(scope, secondParameterType, firstReturnType))
            {
                return;
            }

            ReportMismatch(
                node,
                $"Composition operator '{operatorText}' cannot compose '{firstName}' with '{secondName}': '{firstName}' returns '{firstReturnType}', but '{secondName}' expects '{secondParameterType}'.");
        }

        private void CheckCompositionFunctionTypeAnnotation(
            SyntaxNode node,
            TypeScope scope,
            SyntaxNode expression,
            SimpleType annotationParameterType,
            SimpleType annotationReturnType)
        {
            if (!TryGetDirectCompositionSignature(
                scope,
                expression,
                out var firstName,
                out var firstParameterType,
                out var secondName,
                out var secondReturnType))
            {
                return;
            }

            if (firstParameterType.IsKnown && !CanAssign(scope, firstParameterType, annotationParameterType))
            {
                ReportMismatch(
                    node,
                    $"Function type annotation for composition supplies input '{annotationParameterType}', but '{firstName}' expects '{firstParameterType}'.");
            }

            if (!secondReturnType.IsKnown)
            {
                return;
            }

            if (IsNullabilityViolation(annotationReturnType, secondReturnType))
            {
                ReportMismatch(
                    node,
                    secondReturnType.IsNull
                        ? $"Function type annotation for composition expects non-null result '{annotationReturnType}', but '{secondName}' returns null."
                        : $"Function type annotation for composition expects non-null result '{annotationReturnType}', but '{secondName}' returns nullable type '{secondReturnType}'.");
            }
            else if (TryGetStructuralAssignmentDiagnostic(scope, annotationReturnType, secondReturnType, out var structuralMessage))
            {
                ReportMismatch(node, structuralMessage);
            }
            else if (!CanAssign(scope, annotationReturnType, secondReturnType))
            {
                ReportMismatch(
                    node,
                    $"Function type annotation for composition expects result '{annotationReturnType}', but '{secondName}' returns '{secondReturnType}'.");
            }
        }

        private static bool TryGetDirectCompositionSignature(
            TypeScope scope,
            SyntaxNode expression,
            out string firstName,
            out SimpleType firstParameterType,
            out string secondName,
            out SimpleType secondReturnType)
        {
            firstName = string.Empty;
            firstParameterType = SimpleType.Unknown;
            secondName = string.Empty;
            secondReturnType = SimpleType.Unknown;

            var expressions = expression.Children.Where(child => !child.IsToken).ToArray();
            if (!IsCompositionExpression(expression) ||
                expressions.Length != 2 ||
                !TryGetCompositionOperatorText(expression.Children, out var operatorText) ||
                !TryGetDirectIdentifierName(expressions[0], out var leftName) ||
                !TryGetDirectIdentifierName(expressions[1], out var rightName) ||
                !scope.ResolveFunctionInfo(leftName, out var leftFunction) ||
                !scope.ResolveFunctionInfo(rightName, out var rightFunction) ||
                !TryGetUnaryFunctionSignature(leftFunction, out var leftParameterType, out var leftReturnType) ||
                !TryGetUnaryFunctionSignature(rightFunction, out var rightParameterType, out var rightReturnType))
            {
                return false;
            }

            firstName = leftName;
            var firstFunction = leftFunction;
            firstParameterType = leftParameterType;
            var firstReturnType = leftReturnType;
            secondName = rightName;
            var secondFunction = rightFunction;
            var secondParameterType = rightParameterType;
            secondReturnType = rightReturnType;
            if (operatorText == "<<")
            {
                firstName = rightName;
                firstFunction = rightFunction;
                firstParameterType = rightParameterType;
                firstReturnType = rightReturnType;
                secondName = leftName;
                secondFunction = leftFunction;
                secondParameterType = leftParameterType;
                secondReturnType = leftReturnType;
            }

            ResolveCompositionGenericSignatureTypes(
                firstFunction,
                firstParameterType,
                firstReturnType,
                secondFunction,
                secondParameterType,
                secondReturnType,
                out firstParameterType,
                out _,
                out _,
                out secondReturnType);

            return true;
        }

        private static void ResolveCompositionGenericEdgeTypes(
            FunctionInfo firstFunction,
            SimpleType firstReturnType,
            FunctionInfo secondFunction,
            SimpleType secondParameterType,
            out SimpleType resolvedFirstReturnType,
            out SimpleType resolvedSecondParameterType)
        {
            ResolveCompositionGenericSignatureTypes(
                firstFunction,
                SimpleType.Unknown,
                firstReturnType,
                secondFunction,
                secondParameterType,
                SimpleType.Unknown,
                out _,
                out resolvedFirstReturnType,
                out resolvedSecondParameterType,
                out _);
        }

        private static void ResolveCompositionGenericSignatureTypes(
            FunctionInfo firstFunction,
            SimpleType firstParameterType,
            SimpleType firstReturnType,
            FunctionInfo secondFunction,
            SimpleType secondParameterType,
            SimpleType secondReturnType,
            out SimpleType resolvedFirstParameterType,
            out SimpleType resolvedFirstReturnType,
            out SimpleType resolvedSecondParameterType,
            out SimpleType resolvedSecondReturnType)
        {
            var firstTypeParameterNames = new HashSet<string>(firstFunction.TypeParameters, StringComparer.Ordinal);
            var secondTypeParameterNames = new HashSet<string>(secondFunction.TypeParameters, StringComparer.Ordinal);
            var firstSubstitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);
            var secondSubstitutions = new Dictionary<string, SimpleType>(StringComparer.Ordinal);

            TryInferCompositionTypeArgument(
                firstReturnType,
                secondParameterType,
                firstTypeParameterNames,
                secondTypeParameterNames,
                firstSubstitutions);
            TryInferCompositionTypeArgument(
                secondParameterType,
                firstReturnType,
                secondTypeParameterNames,
                firstTypeParameterNames,
                secondSubstitutions);

            resolvedFirstParameterType = SubstituteGenericType(firstParameterType, firstSubstitutions, firstTypeParameterNames, unresolvedTypeParameterIsUnknown: true);
            resolvedFirstReturnType = SubstituteGenericType(firstReturnType, firstSubstitutions, firstTypeParameterNames, unresolvedTypeParameterIsUnknown: true);
            resolvedSecondParameterType = SubstituteGenericType(secondParameterType, secondSubstitutions, secondTypeParameterNames, unresolvedTypeParameterIsUnknown: true);
            resolvedSecondReturnType = SubstituteGenericType(secondReturnType, secondSubstitutions, secondTypeParameterNames, unresolvedTypeParameterIsUnknown: true);
        }

        private static void TryInferCompositionTypeArgument(
            SimpleType genericShape,
            SimpleType sourceType,
            IReadOnlySet<string> typeParameterNames,
            IReadOnlySet<string> oppositeTypeParameterNames,
            Dictionary<string, SimpleType> substitutions)
        {
            if (typeParameterNames.Count == 0 ||
                !TryInferDirectGenericArgument(
                    genericShape,
                    sourceType,
                    typeParameterNames,
                    out var typeParameterName,
                    out var inferredType) ||
                GenericTypeReferencesAny(inferredType, oppositeTypeParameterNames, oppositeTypeParameterNames))
            {
                return;
            }

            substitutions[typeParameterName] = inferredType;
        }

        private static bool TryGetUnaryFunctionSignature(
            FunctionInfo function,
            out SimpleType parameterType,
            out SimpleType returnType)
        {
            parameterType = SimpleType.Unknown;
            returnType = function.ReturnType;
            if (!returnType.IsKnown ||
                function.ParameterTypes is not { Count: 1 } parameterTypes ||
                !parameterTypes[0].IsKnown)
            {
                return false;
            }

            parameterType = parameterTypes[0];
            return true;
        }

        private static bool TryGetDirectIdentifierName(SyntaxNode node, out string name)
        {
            name = string.Empty;
            if (node.Kind != SyntaxKind.IdentifierExpression || !TryGetFirstIdentifier(node, out var identifier))
            {
                return false;
            }

            name = identifier.Text ?? string.Empty;
            return name.Length > 0;
        }

        private static bool IsCompositionExpression(SyntaxNode node) =>
            node.Kind == SyntaxKind.BinaryExpression &&
            TryGetCompositionOperatorText(node.Children, out _);

        private static bool IsLogicalUnsignedShiftExpression(SyntaxNode node) =>
            node.Kind == SyntaxKind.BinaryExpression &&
            TryGetLogicalUnsignedShiftOperatorText(node.Children, out _);

        private static bool TryGetCompositionOperatorText(IReadOnlyList<SyntaxNode> children, out string operatorText)
        {
            if (TryGetLogicalUnsignedShiftOperatorText(children, out _))
            {
                operatorText = string.Empty;
                return false;
            }

            for (var index = 0; index + 1 < children.Count; index++)
            {
                if (!children[index].IsToken || !children[index + 1].IsToken)
                {
                    continue;
                }

                if (children[index].Kind == SyntaxKind.GreaterToken && children[index + 1].Kind == SyntaxKind.GreaterToken)
                {
                    operatorText = ">>";
                    return true;
                }

                if (children[index].Kind == SyntaxKind.LessToken && children[index + 1].Kind == SyntaxKind.LessToken)
                {
                    operatorText = "<<";
                    return true;
                }
            }

            operatorText = string.Empty;
            return false;
        }

        private static bool TryGetLogicalUnsignedShiftOperatorText(IReadOnlyList<SyntaxNode> children, out string operatorText)
        {
            for (var index = 0; index + 2 < children.Count; index++)
            {
                if (children[index].IsToken &&
                    children[index + 1].IsToken &&
                    children[index + 2].IsToken &&
                    children[index].Kind == SyntaxKind.GreaterToken &&
                    children[index + 1].Kind == SyntaxKind.GreaterToken &&
                    children[index + 2].Kind == SyntaxKind.GreaterToken)
                {
                    operatorText = ">>>";
                    return true;
                }
            }

            operatorText = string.Empty;
            return false;
        }

        private bool IsShiftLikeCompositionExpression(SyntaxNode node, TypeScope scope)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (!IsCompositionExpression(node) || expressions.Length != 2)
            {
                return false;
            }

            var leftType = InferExpressionForCompositionClassification(expressions[0], scope);
            var rightType = InferExpressionForCompositionClassification(expressions[1], scope);
            return IsShiftOperandCandidate(scope, leftType) || IsShiftOperandCandidate(scope, rightType);
        }

        private SimpleType InferExpressionForCompositionClassification(SyntaxNode node, TypeScope scope)
        {
            return _inference.TryInferExpression(
                node,
                scope,
                child => InferExpressionForCompositionClassification(child, scope),
                out var type)
                ? type
                : SimpleType.Unknown;
        }

        private static bool ShouldReportShiftOperandDiagnostic(TypeScope scope, SimpleType leftType, SimpleType rightType)
        {
            if (IsKnownFunctionLikeType(leftType) || IsKnownFunctionLikeType(rightType))
            {
                return false;
            }

            return IsShiftOperandCandidate(scope, leftType) ||
                IsShiftOperandCandidate(scope, rightType) ||
                (leftType.IsKnown && rightType.IsKnown);
        }

        private static bool ShouldReportLogicalUnsignedShiftOperandDiagnostic(
            TypeScope scope,
            SyntaxNode leftExpression,
            SimpleType leftType,
            SyntaxNode rightExpression,
            SimpleType rightType) =>
            ShouldReportShiftOperandDiagnostic(scope, leftType, rightType) ||
            IsDirectKnownFunctionReference(scope, leftExpression) ||
            IsDirectKnownFunctionReference(scope, rightExpression);

        private static bool IsDirectKnownFunctionReference(TypeScope scope, SyntaxNode expression) =>
            TryGetDirectIdentifierName(expression, out var name) &&
            scope.ResolveFunctionInfo(name, out _);

        private static bool IsShiftOperandCandidate(TypeScope scope, SimpleType type)
        {
            if (!type.IsKnown || IsKnownFunctionLikeType(type))
            {
                return false;
            }

            return type.IsNull ||
                type.IsNullable ||
                IsPrimitiveCompositionValueType(type.Name) ||
                scope.ResolveEnum(type.Name, out _) ||
                scope.ResolveShape(type.Name, out _) ||
                scope.ResolveRecordShape(type.Name, out _) ||
                scope.ResolveType(type.Name);
        }

        private static bool IsKnownFunctionLikeType(SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            (type.Name.Contains("->", StringComparison.Ordinal) ||
             type.Name.StartsWith("System.Func<", StringComparison.Ordinal) ||
             type.Name.StartsWith("System.Action<", StringComparison.Ordinal) ||
             string.Equals(type.Name, "System.Action", StringComparison.Ordinal));

        private static bool IsKnownCompositionValueOperand(TypeScope scope, SimpleType type)
        {
            if (!type.IsKnown)
            {
                return false;
            }

            if (type.IsNull)
            {
                return true;
            }

            return IsPrimitiveCompositionValueType(type.Name) ||
                scope.ResolveEnum(type.Name, out _);
        }

        private static bool IsPrimitiveCompositionValueType(string typeName) =>
            typeName is
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
                "string" or
                "uint" or
                "ulong" or
                "ushort";

        private static bool IsAssignmentOperatorKind(SyntaxKind kind) =>
            kind is SyntaxKind.EqualsToken
                or SyntaxKind.PlusEqualsToken
                or SyntaxKind.MinusEqualsToken
                or SyntaxKind.PipeEqualsToken
                or SyntaxKind.AmpersandEqualsToken
                or SyntaxKind.CaretEqualsToken
                or SyntaxKind.LessLessEqualsToken
                or SyntaxKind.GreaterGreaterEqualsToken
                or SyntaxKind.LogicalUnsignedShiftEqualsToken;

        private static bool IsBitwiseAssignmentOperatorKind(SyntaxKind kind) =>
            kind is SyntaxKind.PipeEqualsToken
                or SyntaxKind.AmpersandEqualsToken
                or SyntaxKind.CaretEqualsToken;

        private static bool IsAdditiveAssignmentOperatorKind(SyntaxKind kind) =>
            kind is SyntaxKind.PlusEqualsToken
                or SyntaxKind.MinusEqualsToken;

        private static bool TryGetAssignmentTargetIdentifier(SyntaxNode node, out SyntaxNode identifier)
        {
            identifier = node;
            return node.Kind == SyntaxKind.IdentifierExpression && TryGetFirstIdentifier(node, out identifier);
        }

        private static bool IsImportedAssignmentTargetCandidate(SyntaxNode node) =>
            node.Kind is SyntaxKind.MemberAccessExpression or SyntaxKind.IndexerExpression;

        private static bool TryGetMemberAccessParts(
            SyntaxNode node,
            out SyntaxNode receiver,
            out string memberName)
        {
            receiver = default!;
            memberName = string.Empty;
            if (node.Kind is not (SyntaxKind.MemberAccessExpression or SyntaxKind.NullConditionalMemberAccessExpression))
            {
                return false;
            }

            receiver = node.Children.FirstOrDefault(child => !child.IsToken)!;
            memberName = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            return receiver is not null && memberName.Length > 0;
        }

        private static bool IsKnownEnumType(TypeScope scope, SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            !type.IsNullable &&
            (scope.ResolveEnum(type.Name, out _) ||
             scope.ResolveEnum(GetUnqualifiedTypeName(type.Name), out _));

        private static bool TryGetUnaryIntegralBitwiseResultType(SimpleType operandType, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!IsKnownNonNullableIntegralType(operandType))
            {
                return false;
            }

            return NormalizePrimitiveTypeName(operandType.Name) switch
            {
                "byte" or "sbyte" or "short" or "ushort" => SetResult(SimpleType.Named("int"), out resultType),
                "int" or "uint" or "long" or "ulong" => SetResult(SimpleType.Named(NormalizePrimitiveTypeName(operandType.Name)), out resultType),
                _ => false
            };
        }

        private static bool TryGetBinaryIntegralBitwiseResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!IsKnownNonNullableIntegralType(leftType) ||
                !IsKnownNonNullableIntegralType(rightType) ||
                !TryPromoteIntegralBinaryType(leftType.Name, rightType.Name, out var promotedType))
            {
                return false;
            }

            resultType = SimpleType.Named(promotedType);
            return true;
        }

        private static bool TryGetBinaryIntegralAdditiveResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!IsKnownNonNullableIntegralType(leftType) ||
                !IsKnownNonNullableIntegralType(rightType) ||
                !TryPromoteIntegralBinaryType(leftType.Name, rightType.Name, out var promotedType))
            {
                return false;
            }

            resultType = SimpleType.Named(promotedType);
            return true;
        }

        private static bool TryGetBinaryIntegralShiftResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!IsKnownNonNullableIntegralType(leftType) ||
                !IsKnownNonNullableShiftCountType(rightType))
            {
                return false;
            }

            return NormalizePrimitiveTypeName(leftType.Name) switch
            {
                "byte" or "sbyte" or "short" or "ushort" => SetResult(SimpleType.Named("int"), out resultType),
                "int" or "uint" or "long" or "ulong" => SetResult(SimpleType.Named(NormalizePrimitiveTypeName(leftType.Name)), out resultType),
                _ => false
            };
        }

        private static bool TryGetBinaryBooleanBitwiseResultType(SimpleType leftType, SimpleType rightType, out SimpleType resultType)
        {
            resultType = SimpleType.Unknown;
            if (!IsKnownNonNullableBoolType(leftType) || !IsKnownNonNullableBoolType(rightType))
            {
                return false;
            }

            resultType = SimpleType.Named("bool");
            return true;
        }

        private static bool IsKnownNonNullableBoolType(SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            !type.IsNullable &&
            string.Equals(type.Name, "bool", StringComparison.Ordinal);

        private static bool IsKnownNonNullableIntegralType(SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            !type.IsNullable &&
            IsIntegralPrimitiveType(NormalizePrimitiveTypeName(type.Name));

        private static bool IsKnownNonNullableShiftCountType(SimpleType type) =>
            type.IsKnown &&
            !type.IsNull &&
            !type.IsNullable &&
            NormalizePrimitiveTypeName(type.Name) is "byte" or "sbyte" or "short" or "ushort" or "int";

        private static bool IsIntegralPrimitiveType(string typeName) =>
            NormalizePrimitiveTypeName(typeName) is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong";

        private static bool TryPromoteIntegralBinaryType(string left, string right, out string resultType)
        {
            resultType = string.Empty;
            left = NormalizePrimitiveTypeName(left);
            right = NormalizePrimitiveTypeName(right);
            if (!IsIntegralPrimitiveType(left) || !IsIntegralPrimitiveType(right))
            {
                return false;
            }

            if (left is "ulong" || right is "ulong")
            {
                if (CanImplicitlyPromoteToUnsignedLong(left) && CanImplicitlyPromoteToUnsignedLong(right))
                {
                    resultType = "ulong";
                    return true;
                }

                return false;
            }

            if (left is "long" || right is "long")
            {
                resultType = "long";
                return true;
            }

            if (left is "uint" || right is "uint")
            {
                resultType = left is "sbyte" or "short" or "int" || right is "sbyte" or "short" or "int"
                    ? "long"
                    : "uint";
                return true;
            }

            resultType = "int";
            return true;
        }

        private static bool CanImplicitlyPromoteToUnsignedLong(string type) =>
            NormalizePrimitiveTypeName(type) is "byte" or "ushort" or "uint" or "ulong";

        private static string NormalizePrimitiveTypeName(string typeName) =>
            typeName switch
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
                _ => typeName
            };

        private static bool SetResult(SimpleType value, out SimpleType result)
        {
            result = value;
            return true;
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

        private static bool IsValidEnumUnderlyingType(string typeName) =>
            typeName is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong";

        private static bool TryGetEnumUnderlyingRange(string typeName, out BigInteger minimum, out BigInteger maximum)
        {
            switch (typeName)
            {
                case "sbyte":
                    minimum = sbyte.MinValue;
                    maximum = sbyte.MaxValue;
                    return true;
                case "byte":
                    minimum = byte.MinValue;
                    maximum = byte.MaxValue;
                    return true;
                case "short":
                    minimum = short.MinValue;
                    maximum = short.MaxValue;
                    return true;
                case "ushort":
                    minimum = ushort.MinValue;
                    maximum = ushort.MaxValue;
                    return true;
                case "int":
                    minimum = int.MinValue;
                    maximum = int.MaxValue;
                    return true;
                case "uint":
                    minimum = uint.MinValue;
                    maximum = uint.MaxValue;
                    return true;
                case "long":
                    minimum = long.MinValue;
                    maximum = long.MaxValue;
                    return true;
                case "ulong":
                    minimum = BigInteger.Zero;
                    maximum = ulong.MaxValue;
                    return true;
                default:
                    minimum = BigInteger.Zero;
                    maximum = BigInteger.Zero;
                    return false;
            }
        }

        private static IEnumerable<string> GetEnumInitializerLiteralTexts(SyntaxNode initializer)
        {
            var children = initializer.Children;
            for (var index = 0; index < children.Count; index++)
            {
                var child = children[index];
                if (!child.IsToken || child.Kind != SyntaxKind.NumericLiteralToken || string.IsNullOrEmpty(child.Text))
                {
                    continue;
                }

                var sign = index > 0 &&
                    children[index - 1].IsToken &&
                    children[index - 1].Kind is SyntaxKind.PlusToken or SyntaxKind.MinusToken
                        ? children[index - 1].Text ?? string.Empty
                        : string.Empty;
                yield return $"{sign}{child.Text}";
            }
        }

        private static IEnumerable<SyntaxNode> GetEnumInitializerIdentifierOperands(SyntaxNode initializer) =>
            initializer.Children.Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken && !string.IsNullOrEmpty(child.Text));

        private static bool IsSingleEnumAliasInitializer(SyntaxNode initializer) =>
            !initializer.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.PipeToken) &&
            GetEnumInitializerIdentifierOperands(initializer).Take(2).Count() == 1;

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
                node.Children.FirstOrDefault(child => !child.IsToken) is not { } callee ||
                !TryGetDirectCallTargetName(callee, out name))
            {
                return false;
            }

            return name.Length > 0;
        }

        private static bool TryGetDirectCallTargetName(SyntaxNode callee, out string name)
        {
            name = string.Empty;
            if (callee.Kind == SyntaxKind.IdentifierExpression && TryGetFirstIdentifier(callee, out var identifier))
            {
                name = identifier.Text ?? string.Empty;
                return name.Length > 0;
            }

            if (callee.Kind != SyntaxKind.GenericNameExpression)
            {
                return false;
            }

            var target = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
            if (target?.Kind != SyntaxKind.IdentifierExpression || !TryGetFirstIdentifier(target, out identifier))
            {
                return false;
            }

            name = identifier.Text ?? string.Empty;
            return name.Length > 0;
        }

        private static IReadOnlyList<SimpleType> GetDirectCallTypeArguments(SyntaxNode call, TypeScope scope)
        {
            if (call.Kind != SyntaxKind.CallExpression ||
                call.Children.FirstOrDefault(child => !child.IsToken) is not { Kind: SyntaxKind.GenericNameExpression } callee ||
                callee.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList) is not { } argumentList)
            {
                return [];
            }

            var arguments = new List<SimpleType>();
            foreach (var argument in argumentList.Children.Where(child => !child.IsToken))
            {
                if (!TryGetType(argument, scope, out var type))
                {
                    return [];
                }

                arguments.Add(type);
            }

            return arguments;
        }

        private static bool TryGetSimpleGenericParameterName(
            SimpleType type,
            IReadOnlySet<string> typeParameterNames,
            out string typeParameterName)
        {
            typeParameterName = type.Name;
            return type.IsKnown && !type.IsNull && typeParameterNames.Contains(type.Name);
        }

        private static bool TryInferDirectGenericArgument(
            SimpleType parameterType,
            SimpleType argumentType,
            IReadOnlySet<string> typeParameterNames,
            out string typeParameterName,
            out SimpleType inferredType)
        {
            typeParameterName = string.Empty;
            inferredType = SimpleType.Unknown;
            if (!parameterType.IsKnown || parameterType.IsNull || !argumentType.IsKnown || argumentType.IsNull)
            {
                return false;
            }

            if (TryGetSimpleGenericParameterName(parameterType, typeParameterNames, out typeParameterName))
            {
                inferredType = argumentType;
                return true;
            }

            if (TryGetArrayElementTypeName(parameterType, out var parameterElementTypeName) &&
                TryGetArrayElementTypeName(argumentType, out var argumentElementTypeName))
            {
                return TryInferDirectGenericArgument(
                    SimpleType.Named(parameterElementTypeName),
                    SimpleType.Named(argumentElementTypeName),
                    typeParameterNames,
                    out typeParameterName,
                    out inferredType);
            }

            if (TryGetSingleGenericArgument(parameterType.Name, out var parameterGenericName, out var parameterArgument) &&
                TryGetSingleGenericArgument(argumentType.Name, out var argumentGenericName, out var argumentArgument) &&
                MetadataTypeNameMatches(argumentGenericName, parameterGenericName))
            {
                return TryInferDirectGenericArgument(
                    GetTypeFromGenericArgument(parameterArgument),
                    GetTypeFromGenericArgument(argumentArgument),
                    typeParameterNames,
                    out typeParameterName,
                    out inferredType);
            }

            return false;
        }

        private static bool GenericTypeReferencesAny(
            SimpleType type,
            IReadOnlySet<string> candidates,
            IReadOnlySet<string> typeParameterNames)
        {
            foreach (var candidate in candidates)
            {
                if (GenericTypeReferencesParameter(type, candidate, typeParameterNames))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GenericTypeReferencesParameter(
            SimpleType type,
            string candidate,
            IReadOnlySet<string> typeParameterNames)
        {
            if (!type.IsKnown || type.IsNull)
            {
                return false;
            }

            if (TryGetSimpleGenericParameterName(type, typeParameterNames, out var typeParameterName))
            {
                return string.Equals(typeParameterName, candidate, StringComparison.Ordinal);
            }

            if (TryGetArrayElementTypeName(type, out var elementTypeName))
            {
                return GenericTypeReferencesParameter(SimpleType.Named(elementTypeName), candidate, typeParameterNames);
            }

            return TryGetSingleGenericArgument(type.Name, out _, out var argument) &&
                GenericTypeReferencesParameter(GetTypeFromGenericArgument(argument), candidate, typeParameterNames);
        }

        private static SimpleType SubstituteGenericType(
            SimpleType type,
            IReadOnlyDictionary<string, SimpleType> substitutions,
            IReadOnlySet<string> typeParameterNames,
            bool unresolvedTypeParameterIsUnknown)
        {
            if (!type.IsKnown || type.IsNull)
            {
                return type;
            }

            if (TryGetSimpleGenericParameterName(type, typeParameterNames, out var typeParameterName))
            {
                if (!substitutions.TryGetValue(typeParameterName, out var substitutedType))
                {
                    return unresolvedTypeParameterIsUnknown ? SimpleType.Unknown : type;
                }

                return type.IsNullable ? substitutedType.AsNullable() : substitutedType;
            }

            if (TryGetArrayElementTypeName(type, out var elementTypeName))
            {
                var substitutedElementType = SubstituteGenericType(
                    SimpleType.Named(elementTypeName),
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown);
                if (!substitutedElementType.IsKnown)
                {
                    return SimpleType.Unknown;
                }

                var substitutedArrayType = SimpleType.Named($"{substitutedElementType.Name}[]");
                return type.IsNullable ? substitutedArrayType.AsNullable() : substitutedArrayType;
            }

            if (TryGetSingleGenericArgument(type.Name, out var genericName, out var argument))
            {
                var substitutedArgument = SubstituteGenericType(
                    GetTypeFromGenericArgument(argument),
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown);
                if (!substitutedArgument.IsKnown)
                {
                    return SimpleType.Unknown;
                }

                var substitutedGenericType = SimpleType.Named($"{genericName}<{substitutedArgument}>");
                return type.IsNullable ? substitutedGenericType.AsNullable() : substitutedGenericType;
            }

            return type;
        }

        private static bool TryGetArrayElementTypeName(SimpleType type, out string elementTypeName)
        {
            elementTypeName = string.Empty;
            if (!type.IsKnown || type.IsNull || !type.Name.EndsWith("[]", StringComparison.Ordinal))
            {
                return false;
            }

            elementTypeName = type.Name[..^2];
            return elementTypeName.Length > 0;
        }

        private static bool TryGetArrayElementType(SimpleType type, out SimpleType elementType)
        {
            if (TryGetArrayElementTypeName(type, out var elementTypeName))
            {
                elementType = SimpleType.Named(elementTypeName);
                return true;
            }

            elementType = SimpleType.Unknown;
            return false;
        }

        private static bool IsParamsParameter(SyntaxNode parameter) =>
            parameter.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.ParamsKeyword);

        private static bool IsExternalSignatureFunction(SyntaxNode node) =>
            node.Children.Any(child => child.Kind is SyntaxKind.AmbientModifier or SyntaxKind.ExternModifier);

        private static bool IsDefaultedParameter(SyntaxNode parameter) =>
            parameter.Children.Any(child => child.Kind == SyntaxKind.Initializer);

        private static SyntaxNode? GetParameterInitializerExpression(SyntaxNode parameter) =>
            parameter.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
                .Children
                .FirstOrDefault(child => !child.IsToken);

        private static string? GetParameterName(SyntaxNode parameter) =>
            parameter.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text;

        private static bool IsSupportedDefaultParameterExpression(SyntaxNode node)
        {
            if (node.Kind != SyntaxKind.LiteralExpression)
            {
                return false;
            }

            var token = node.Children.FirstOrDefault(child => child.IsToken);
            return token?.Kind is SyntaxKind.StringLiteralToken
                or SyntaxKind.NumericLiteralToken
                or SyntaxKind.TrueKeyword
                or SyntaxKind.FalseKeyword
                or SyntaxKind.NullKeyword;
        }

        private static bool SameSimpleType(SimpleType left, SimpleType right) =>
            left.IsKnown == right.IsKnown &&
            left.IsNull == right.IsNull &&
            left.IsNullable == right.IsNullable &&
            string.Equals(left.Name, right.Name, StringComparison.Ordinal);

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

        private static bool TryGetUnaryFunctionType(
            SyntaxNode annotation,
            TypeScope scope,
            out SimpleType parameterType,
            out SimpleType returnType)
        {
            parameterType = SimpleType.Unknown;
            returnType = SimpleType.Unknown;

            if (annotation.Kind == SyntaxKind.TypeAnnotation)
            {
                var typeNode = annotation.Children.FirstOrDefault(child => !child.IsToken);
                return typeNode is not null && TryGetUnaryFunctionType(typeNode, scope, out parameterType, out returnType);
            }

            if (annotation.Kind != SyntaxKind.FunctionType)
            {
                return false;
            }

            var types = annotation.Children.Where(child => !child.IsToken).ToArray();
            if (types.Length != 2 ||
                !TryGetType(types[0], scope, out parameterType) ||
                !TryGetType(types[1], scope, out returnType))
            {
                parameterType = SimpleType.Unknown;
                returnType = SimpleType.Unknown;
                return false;
            }

            return parameterType.IsKnown && returnType.IsKnown;
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

        private static IReadOnlyList<SyntaxNode> GetParameterNodes(SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            return parameterList is null
                ? []
                : parameterList.Children.Where(child => child.Kind == SyntaxKind.Parameter).ToArray();
        }

        private static IReadOnlyList<SimpleType>? GetFunctionParameterTypes(SyntaxNode declaration, TypeScope scope)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return null;
            }

            var parameters = parameterList.Children.Where(child => child.Kind == SyntaxKind.Parameter).ToArray();
            var parameterTypes = new List<SimpleType>();
            foreach (var parameter in parameters)
            {
                if (!TryGetDirectTypeAnnotation(parameter, out var annotation) ||
                    !TryGetType(annotation, scope, out var type) ||
                    !type.IsKnown)
                {
                    return null;
                }

                parameterTypes.Add(type);
            }

            return parameterTypes;
        }

        private static IReadOnlyList<string>? GetFunctionParameterNames(SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return null;
            }

            return parameterList.Children
                .Where(child => child.Kind == SyntaxKind.Parameter)
                .Select(parameter => GetParameterName(parameter) ?? string.Empty)
                .ToArray();
        }

        private static int? GetFunctionParamsParameterIndex(SyntaxNode declaration, TypeScope scope)
        {
            var parameters = GetParameterNodes(declaration);
            var paramsIndexes = parameters
                .Select((parameter, index) => (Parameter: parameter, Index: index))
                .Where(item => IsParamsParameter(item.Parameter))
                .ToArray();

            if (paramsIndexes.Length != 1)
            {
                return null;
            }

            var item = paramsIndexes[0];
            if (item.Index != parameters.Count - 1 ||
                !TryGetDirectTypeAnnotation(item.Parameter, out var annotation) ||
                !TryGetType(annotation, scope, out var parameterType) ||
                !TryGetArrayElementTypeName(parameterType, out _))
            {
                return null;
            }

            return item.Index;
        }

        private static IReadOnlyList<bool>? GetFunctionOptionalParameterFlags(SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return null;
            }

            return parameterList.Children
                .Where(child => child.Kind == SyntaxKind.Parameter)
                .Select(IsDefaultedParameter)
                .ToArray();
        }

        private static IReadOnlyList<string> GetTypeParameterNames(SyntaxNode declaration)
        {
            var typeParameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeParameterList);
            if (typeParameterList is null)
            {
                return [];
            }

            return typeParameterList.Children
                .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                .Select(child => child.Text ?? string.Empty)
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
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

        private static bool IsMutableValueDeclaration(SyntaxNode node) =>
            node.Kind == SyntaxKind.ValueDeclaration &&
            node.Children.Any(child => child.Kind == SyntaxKind.MutKeyword);

        private static bool HasInitializer(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.Initializer);

        private static SyntaxNode? GetExtensionReceiverIdentifier(SyntaxNode node)
        {
            var seenReceiverType = false;
            foreach (var child in node.Children)
            {
                if (!seenReceiverType && IsTypeSyntax(child.Kind))
                {
                    seenReceiverType = true;
                    continue;
                }

                if (!seenReceiverType)
                {
                    continue;
                }

                if (child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child;
                }

                if (child.IsToken && child.Kind == SyntaxKind.OpenBraceToken)
                {
                    return null;
                }
            }

            return null;
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
        private readonly Dictionary<string, ValueInfo> _values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, FunctionInfo> _functions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimpleType> _typeAliases = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CompileTimeOnlyTypeKind> _compileTimeOnlyTypes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeLevelUnionInfo> _typeLevelUnions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UnionInfo> _unions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IReadOnlyList<string>> _enums = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _structuralShapes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ShapeInfo> _recordShapes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ExtensionPropertyInfo>> _extensionProperties = new(StringComparer.Ordinal);
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

        public void DeclareValue(string name, SimpleType type, bool isMutable = false) => _values[name] = new ValueInfo(type, isMutable);

        public void DeclareFunction(string name, SimpleType returnType) => _functions[name] = new FunctionInfo(returnType, default, null, null, [], null, null);

        public void DeclareFunction(string name, SimpleType returnType, FunctionCapabilities capabilities) => _functions[name] = new FunctionInfo(returnType, capabilities, null, null, [], null, null);

        public void DeclareFunction(
            string name,
            SimpleType returnType,
            FunctionCapabilities capabilities,
            IReadOnlyList<SimpleType>? parameterTypes) =>
            DeclareFunction(name, returnType, capabilities, parameterTypes, null, [], null, null);

        public void DeclareFunction(
            string name,
            SimpleType returnType,
            FunctionCapabilities capabilities,
            IReadOnlyList<SimpleType>? parameterTypes,
            IReadOnlyList<string>? parameterNames,
            IReadOnlyList<string> typeParameters,
            int? paramsParameterIndex,
            IReadOnlyList<bool>? optionalParameterFlags) =>
            _functions[name] = new FunctionInfo(returnType, capabilities, parameterTypes, parameterNames, typeParameters, paramsParameterIndex, optionalParameterFlags);

        public void DeclareType(string name) => _types.Add(name);

        public void DeclareTypeAlias(string name, SimpleType type) => _typeAliases[name] = type;

        public void DeclareCompileTimeOnlyType(string name, CompileTimeOnlyTypeKind kind) => _compileTimeOnlyTypes[name] = kind;

        public void DeclareTypeLevelUnion(string name, IReadOnlyList<TypeLevelUnionMemberInfo> members) => _typeLevelUnions[name] = new TypeLevelUnionInfo(name, members);

        public void DeclareUnion(string name, IReadOnlyList<UnionCaseInfo> cases) => _unions[name] = new UnionInfo(name, cases);

        public void DeclareEnum(string name, IReadOnlyList<string> members) => _enums[name] = members;

        public void DeclareStructuralShape(string name, IReadOnlyList<ShapeMemberInfo> members) => _structuralShapes[name] = new ShapeInfo(name, members);

        public void DeclareRecordShape(string name, IReadOnlyList<ShapeMemberInfo> members) => _recordShapes[name] = new ShapeInfo(name, members);

        public bool TryDeclareExtensionProperty(SimpleType receiverType, string name, SimpleType type)
        {
            if (!receiverType.IsKnown || receiverType.IsNull || receiverType.IsNullable || name.Length == 0 || !type.IsKnown)
            {
                return true;
            }

            if (ResolveExtensionProperty(receiverType, name, out _))
            {
                return false;
            }

            if (!_extensionProperties.TryGetValue(receiverType.Name, out var properties))
            {
                properties = [];
                _extensionProperties[receiverType.Name] = properties;
            }

            properties.Add(new ExtensionPropertyInfo(receiverType, name, type));
            return true;
        }

        public bool ResolveValue(string name, out SimpleType type)
        {
            if (_values.TryGetValue(name, out var value))
            {
                type = value.Type;
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

        public bool ResolveValueInfo(string name, out ValueInfo value)
        {
            if (_values.TryGetValue(name, out value))
            {
                return true;
            }

            if (_parent is not null)
            {
                return _parent.ResolveValueInfo(name, out value);
            }

            value = default;
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

        public bool ResolveExtensionProperty(SimpleType receiverType, string name, out ExtensionPropertyInfo property)
        {
            if (receiverType.IsKnown &&
                !receiverType.IsNull &&
                !receiverType.IsNullable &&
                _extensionProperties.TryGetValue(receiverType.Name, out var properties))
            {
                property = properties.FirstOrDefault(candidate =>
                    candidate.ReceiverType.Equals(receiverType) &&
                    string.Equals(candidate.Name, name, StringComparison.Ordinal));
                if (property.Name is not null)
                {
                    return true;
                }
            }

            if (_parent is not null)
            {
                return _parent.ResolveExtensionProperty(receiverType, name, out property);
            }

            property = default;
            return false;
        }

        public bool ResolveType(string name) => _types.Contains(name) || (_parent?.ResolveType(name) ?? false);
    }

    private readonly record struct UnionInfo(string Name, IReadOnlyList<UnionCaseInfo> Cases);

    private readonly record struct UnionCaseInfo(string Name, IReadOnlyList<ParameterInfo> Parameters);

    private readonly record struct ParameterInfo(string Name, string Type);

    private readonly record struct ValueInfo(SimpleType Type, bool IsMutable);

    private readonly record struct TypeLevelUnionInfo(string Name, IReadOnlyList<TypeLevelUnionMemberInfo> Members);

    private readonly record struct TypeLevelUnionMemberInfo(SimpleType Type);

    private readonly record struct ShapeInfo(string Name, IReadOnlyList<ShapeMemberInfo> Members);

    private readonly record struct ShapeMemberInfo(string Name, SimpleType Type, bool IsOptional);

    private readonly record struct ExtensionPropertyInfo(SimpleType ReceiverType, string Name, SimpleType Type);

    private readonly record struct RecordExpressionFieldInfo(SyntaxNode Node, SimpleType Type, bool IsOptional);

    private readonly record struct BranchNarrowing(string VariableName, SimpleType Type);

    private readonly record struct BoundCallArgument(SyntaxNode Argument, int ParameterIndex);

    private readonly record struct IndexerArgumentType(SimpleType Type, string? NumericLiteralText, bool IsNullLiteral);

    private readonly record struct IndexerCandidateScore(MetadataPropertySymbol Property, int Score);

    private readonly record struct FunctionInfo(
        SimpleType ReturnType,
        FunctionCapabilities Capabilities,
        IReadOnlyList<SimpleType>? ParameterTypes,
        IReadOnlyList<string>? ParameterNames,
        IReadOnlyList<string> TypeParameters,
        int? ParamsParameterIndex,
        IReadOnlyList<bool>? OptionalParameterFlags);

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
