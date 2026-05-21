using System.Text;
using TypeSharp.Compiler.Lowering;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Backend;

public static class CSharpSourceBackend
{
    public static string Emit(SyntaxNode root, string? defaultNamespace = null)
    {
        return Emit(root, defaultNamespace, "Module");
    }

    public static string Emit(SyntaxNode root, string? defaultNamespace, string moduleContainerName)
    {
        return Emit(root, defaultNamespace, moduleContainerName, new Dictionary<string, CSharpSourceImportTarget>(StringComparer.Ordinal));
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports)
    {
        return Emit(root, defaultNamespace, moduleContainerName, sourceImports, [], [], []);
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
        IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
    {
        return Emit(root, defaultNamespace, moduleContainerName, sourceImports, [], [], functionReExports);
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
        IReadOnlyList<CSharpSourceFunctionImportAlias> functionImportAliases,
        IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
    {
        return Emit(root, defaultNamespace, moduleContainerName, sourceImports, [], functionImportAliases, functionReExports);
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
        IReadOnlyList<CSharpSourceValueImportAlias> valueImportAliases,
        IReadOnlyList<CSharpSourceFunctionImportAlias> functionImportAliases,
        IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
    {
        return Emit(root, defaultNamespace, moduleContainerName, sourceImports, valueImportAliases, [], functionImportAliases, functionReExports);
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
        IReadOnlyList<CSharpSourceValueImportAlias> valueImportAliases,
        IReadOnlyList<CSharpSourceValueReExport> valueReExports,
        IReadOnlyList<CSharpSourceFunctionImportAlias> functionImportAliases,
        IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
    {
        return Emit(root, defaultNamespace, moduleContainerName, sourceImports, valueImportAliases, valueReExports, [], functionImportAliases, functionReExports);
    }

    public static string Emit(
        SyntaxNode root,
        string? defaultNamespace,
        string moduleContainerName,
        IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
        IReadOnlyList<CSharpSourceValueImportAlias> valueImportAliases,
        IReadOnlyList<CSharpSourceValueReExport> valueReExports,
        IReadOnlyList<CSharpSourceEnumImportShape> enumImportShapes,
        IReadOnlyList<CSharpSourceFunctionImportAlias> functionImportAliases,
        IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
    {
        var loweredRoot = TypeSharpLoweringPipeline.Default.Lower(root);
        var emitter = new Emitter();
        return emitter.Emit(loweredRoot, defaultNamespace, moduleContainerName, sourceImports, valueImportAliases, valueReExports, enumImportShapes, functionImportAliases, functionReExports);
    }

    private sealed class Emitter
    {
        private readonly StringBuilder _builder = new();
        private readonly HashSet<string> _constructorCandidateNames = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RecordShape> _records = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RecordShape> _structuralShapes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TypeLevelUnionShape> _typeLevelUnions = new(StringComparer.Ordinal);
        private readonly HashSet<string> _compileTimeOnlyTypeAliases = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _compileTimeOnlyTypeAliasRuntimeTypes = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EnumShape> _enums = new(StringComparer.Ordinal);
        private readonly Dictionary<string, UnionShape> _unions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _unionCaseFactories = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _unionCaseValues = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CSharpFunctionSignature> _functionSignatures = new(StringComparer.Ordinal);
        private HashSet<string> _localExportedNames = new(StringComparer.Ordinal);
        private Dictionary<string, string> _valueTypes = new(StringComparer.Ordinal);
        private int _temporaryIndex;

        private sealed record CSharpSourceLiteralExportAlias(string ExportedName, SyntaxNode Literal);

        private sealed record CSharpSourceValueExportAlias(string ExportedName, SyntaxNode Value);

        private readonly record struct CollectionSegment(bool IsSpread, IReadOnlyList<SyntaxNode> Elements, SyntaxNode? Expression);

        public string Emit(
            SyntaxNode root,
            string? defaultNamespace,
            string moduleContainerName,
            IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports,
            IReadOnlyList<CSharpSourceValueImportAlias> valueImportAliases,
            IReadOnlyList<CSharpSourceValueReExport> valueReExports,
            IReadOnlyList<CSharpSourceEnumImportShape> enumImportShapes,
            IReadOnlyList<CSharpSourceFunctionImportAlias> functionImportAliases,
            IReadOnlyList<CSharpSourceFunctionReExport> functionReExports)
        {
            var explicitNamespace = root.Children.FirstOrDefault(child => child.Kind == SyntaxKind.NamespaceDeclaration) is { } namespaceDeclaration
                ? GetQualifiedName(namespaceDeclaration)
                : string.Empty;
            var namespaceName = explicitNamespace.Length > 0
                ? explicitNamespace
                : NormalizeNamespace(defaultNamespace);
            _localExportedNames = CollectLocalExportedNames(root);

            var imports = CollectImports(root, sourceImports);
            var literals = root.Children
                .Where(child => child.Kind == SyntaxKind.LiteralDeclaration && !IsAmbientDeclaration(child))
                .ToArray();
            var literalExportAliases = CollectLocalLiteralExportAliases(root);
            var values = root.Children
                .Where(child => child.Kind == SyntaxKind.ValueDeclaration && !IsAmbientDeclaration(child) && CanLowerTopLevelValueDeclaration(child))
                .ToArray();
            var valueExportAliases = CollectLocalValueExportAliases(root);

            var functions = root.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var typeAliases = root.Children
                .Where(child => child.Kind == SyntaxKind.TypeAliasDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var modules = root.Children
                .Where(child => child.Kind == SyntaxKind.ModuleDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var records = root.Children
                .Where(child => child.Kind == SyntaxKind.RecordDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var enums = root.Children
                .Where(child => child.Kind == SyntaxKind.EnumDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var unions = root.Children
                .Where(child => child.Kind == SyntaxKind.UnionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var classes = root.Children
                .Where(child => child.Kind == SyntaxKind.ClassDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var interfaces = root.Children
                .Where(child => child.Kind == SyntaxKind.InterfaceDeclaration && !IsAmbientDeclaration(child))
                .ToArray();
            var extensionDeclarations = root.Children
                .Where(child => child.Kind == SyntaxKind.ExtensionDeclaration && !IsAmbientDeclaration(child))
                .Concat(modules.SelectMany(module => module.Children.Where(child => child.Kind == SyntaxKind.ExtensionDeclaration && !IsAmbientDeclaration(child))))
                .ToArray();

            foreach (var importedName in imports.ImportedNames)
            {
                _constructorCandidateNames.Add(importedName);
            }

            foreach (var record in records)
            {
                RegisterRecord(record);
            }

            foreach (var typeAlias in typeAliases)
            {
                RegisterTypeAlias(typeAlias);
            }

            foreach (var union in unions)
            {
                RegisterUnion(union);
            }

            foreach (var enumDeclaration in enums)
            {
                RegisterEnum(enumDeclaration);
            }

            foreach (var enumImportShape in enumImportShapes)
            {
                if (!_enums.ContainsKey(enumImportShape.LocalName))
                {
                    _enums[enumImportShape.LocalName] = new EnumShape(enumImportShape.LocalName, enumImportShape.Members);
                }
            }

            foreach (var function in functions)
            {
                RegisterFunctionSignature(function);
            }

            foreach (var value in values)
            {
                RegisterTopLevelValue(value);
            }

            foreach (var valueImportAlias in valueImportAliases)
            {
                _valueTypes[valueImportAlias.LocalName] = valueImportAlias.Type;
            }

            _builder.AppendLine("// <auto-generated />");
            _builder.AppendLine();
            foreach (var import in imports.Usings)
            {
                _builder.AppendLine($"using {import};");
            }

            foreach (var alias in imports.Aliases)
            {
                _builder.AppendLine($"using {alias.LocalName} = {alias.QualifiedName};");
            }

            var needsRuntime = unions.Length > 0 || ContainsNode(root, SyntaxKind.MatchExpression);
            if (needsRuntime && !imports.Usings.Contains("TypeSharp.Runtime", StringComparer.Ordinal))
            {
                _builder.AppendLine("using TypeSharp.Runtime;");
            }

            foreach (var import in imports.StaticUsings)
            {
                _builder.AppendLine($"using static {import};");
            }

            if (imports.Usings.Count > 0 || imports.Aliases.Count > 0 || imports.StaticUsings.Count > 0 || needsRuntime)
            {
                _builder.AppendLine();
            }

            _builder.AppendLine($"namespace {namespaceName}");
            _builder.AppendLine("{");
            _builder.AppendLine($"    public static class {moduleContainerName}");
            _builder.AppendLine("    {");

            foreach (var literal in literals)
            {
                EmitLiteralDeclaration(literal);
            }

            if (values.Length > 0)
            {
                if (literals.Length > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < values.Length; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitTopLevelValueDeclaration(values[index]);
                }
            }

            if (literalExportAliases.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < literalExportAliases.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitLiteralExportAlias(literalExportAliases[index]);
                }
            }

            if (valueExportAliases.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < valueExportAliases.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitValueExportAlias(valueExportAliases[index]);
                }
            }

            if (valueImportAliases.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0 || valueExportAliases.Count > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < valueImportAliases.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitValueImportAlias(valueImportAliases[index]);
                }
            }

            if (valueReExports.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0 || valueExportAliases.Count > 0 || valueImportAliases.Count > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < valueReExports.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitValueReExport(valueReExports[index]);
                }
            }

            if (functionImportAliases.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0 || valueExportAliases.Count > 0 || valueImportAliases.Count > 0 || valueReExports.Count > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < functionImportAliases.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitFunctionImportAlias(functionImportAliases[index]);
                }
            }

            if ((literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0 || valueExportAliases.Count > 0 || valueImportAliases.Count > 0 || valueReExports.Count > 0 || functionImportAliases.Count > 0) && functions.Length > 0)
            {
                _builder.AppendLine();
            }

            for (var index = 0; index < functions.Length; index++)
            {
                if (index > 0)
                {
                    _builder.AppendLine();
                }

                EmitFunction(functions[index], isStatic: true);
            }

            if (functionReExports.Count > 0)
            {
                if (literals.Length > 0 || values.Length > 0 || literalExportAliases.Count > 0 || valueExportAliases.Count > 0 || valueImportAliases.Count > 0 || valueReExports.Count > 0 || functionImportAliases.Count > 0 || functions.Length > 0)
                {
                    _builder.AppendLine();
                }

                for (var index = 0; index < functionReExports.Count; index++)
                {
                    if (index > 0)
                    {
                        _builder.AppendLine();
                    }

                    EmitFunctionReExport(functionReExports[index]);
                }
            }

            _builder.AppendLine("    }");

            foreach (var module in modules)
            {
                _builder.AppendLine();
                EmitModuleDeclaration(module);
            }

            foreach (var recordDeclaration in records)
            {
                _builder.AppendLine();
                EmitRecordDeclaration(recordDeclaration);
            }

            foreach (var enumDeclaration in enums)
            {
                _builder.AppendLine();
                EmitEnumDeclaration(enumDeclaration);
            }

            foreach (var unionDeclaration in unions)
            {
                _builder.AppendLine();
                EmitUnionDeclaration(unionDeclaration);
            }

            foreach (var classDeclaration in classes)
            {
                _builder.AppendLine();
                EmitClassDeclaration(classDeclaration);
            }

            foreach (var interfaceDeclaration in interfaces)
            {
                _builder.AppendLine();
                EmitInterfaceDeclaration(interfaceDeclaration);
            }

            for (var index = 0; index < extensionDeclarations.Length; index++)
            {
                _builder.AppendLine();
                EmitExtensionDeclaration(extensionDeclarations[index], index);
            }

            _builder.AppendLine("}");
            return _builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private void RegisterRecord(SyntaxNode node)
        {
            var name = GetRecordDeclarationName(node);
            if (name.Length == 0)
            {
                return;
            }

            var parameters = GetParameters(node);
            _records[name] = new RecordShape(name, parameters);
            _constructorCandidateNames.Add(name);
        }

        private void RegisterTypeAlias(SyntaxNode node)
        {
            var name = GetTypeAliasDeclarationName(node);
            var target = node.Children.LastOrDefault(child => !child.IsToken);
            if (name.Length == 0 || target is null)
            {
                return;
            }

            if (target.Kind is SyntaxKind.UnionType or SyntaxKind.RecordShapeType or SyntaxKind.IntersectionType or SyntaxKind.KeyofType or SyntaxKind.IndexedAccessType or SyntaxKind.LiteralType)
            {
                _compileTimeOnlyTypeAliases.Add(name);
            }

            if (target.Kind == SyntaxKind.RecordShapeType)
            {
                _structuralShapes[name] = GetStructuralShape(name, target);
            }
            else if (target.Kind == SyntaxKind.IntersectionType &&
                TryGetIntersectionStructuralShape(name, target, out var intersectionShape))
            {
                _structuralShapes[name] = intersectionShape;
            }

            if (target.Kind == SyntaxKind.KeyofType)
            {
                _compileTimeOnlyTypeAliasRuntimeTypes[name] = "string";
            }
            else if (target.Kind == SyntaxKind.IndexedAccessType)
            {
                _compileTimeOnlyTypeAliasRuntimeTypes[name] = MapType(target);
            }
            else if (target.Kind == SyntaxKind.LiteralType)
            {
                _compileTimeOnlyTypeAliasRuntimeTypes[name] = MapType(target);
            }

            if (target.Kind != SyntaxKind.UnionType)
            {
                return;
            }

            var members = new List<TypeLevelUnionMemberShape>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in target.Children.Where(child => !child.IsToken))
            {
                var sourceType = GetSourceTypeName(member);
                if (sourceType.Length == 0 || !seen.Add(sourceType))
                {
                    continue;
                }

                members.Add(new TypeLevelUnionMemberShape(sourceType, MapType(member)));
            }

            if (members.Count >= 2)
            {
                _typeLevelUnions[name] = new TypeLevelUnionShape(name, members);
            }
        }

        private void RegisterUnion(SyntaxNode node)
        {
            var unionName = GetUnionDeclarationName(node);
            if (unionName.Length == 0 || GetTypeParameterList(node).Length > 0)
            {
                return;
            }

            var cases = GetUnionCases(node);
            _unions[unionName] = new UnionShape(unionName, cases);
            foreach (var unionCase in cases)
            {
                if (unionCase.Parameters.Count == 0)
                {
                    _unionCaseValues[unionCase.Name] = $"{unionName}.{unionCase.Name}";
                }
                else
                {
                    _unionCaseFactories[unionCase.Name] = $"{unionName}.{unionCase.Name}";
                }
            }
        }

        private void RegisterEnum(SyntaxNode node)
        {
            var name = GetEnumDeclarationName(node);
            if (name.Length == 0)
            {
                return;
            }

            _enums[name] = new EnumShape(name, GetEnumMembers(node));
        }

        private static CSharpImports CollectImports(
            SyntaxNode root,
            IReadOnlyDictionary<string, CSharpSourceImportTarget> sourceImports)
        {
            var usings = new List<string>();
            var aliases = new List<CSharpImportAlias>();
            var staticUsings = new List<string>();
            var importedNames = new List<string>();
            var seenUsings = new HashSet<string>(StringComparer.Ordinal);
            var seenAliases = new HashSet<string>(StringComparer.Ordinal);
            var seenStaticUsings = new HashSet<string>(StringComparer.Ordinal);
            var seenImportedNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var child in root.Children)
            {
                if (child.Kind is SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration)
                {
                    var moduleSpecifier = child.Children.FirstOrDefault(grandchild => grandchild.IsToken && grandchild.Kind == SyntaxKind.StringLiteralToken);
                    if (!TryUnquoteStringLiteral(moduleSpecifier?.Text, out var namespaceName))
                    {
                        continue;
                    }

                    var isSourceImport = sourceImports.TryGetValue(namespaceName, out var sourceImport);
                    if (isSourceImport)
                    {
                        var resolvedSourceImport = sourceImport!;
                        if (seenUsings.Add(resolvedSourceImport.NamespaceName))
                        {
                            usings.Add(resolvedSourceImport.NamespaceName);
                        }

                        if (child.Kind == SyntaxKind.ImportNamedDeclaration &&
                            seenStaticUsings.Add(resolvedSourceImport.QualifiedModuleContainer))
                        {
                            staticUsings.Add(resolvedSourceImport.QualifiedModuleContainer);
                        }
                    }
                    else if (seenUsings.Add(namespaceName))
                    {
                        usings.Add(namespaceName);
                    }

                    foreach (var specifier in GetNamedImportSpecifiers(child))
                    {
                        if (!isSourceImport && seenImportedNames.Add(specifier.LocalName))
                        {
                            importedNames.Add(specifier.LocalName);
                        }

                        var aliasSourceImport = isSourceImport ? sourceImport : null;
                        var shouldEmitRelativeTypeAlias = isSourceImport &&
                            aliasSourceImport is not null &&
                            (child.Kind == SyntaxKind.ImportTypeDeclaration
                                ? specifier.IsAlias || aliasSourceImport.HasExportedTypeTarget(specifier.ImportedName)
                                : specifier.IsAlias && aliasSourceImport.HasExportedTypeTarget(specifier.ImportedName));
                        var shouldEmitRelativeModuleAlias = isSourceImport &&
                            aliasSourceImport is not null &&
                            child.Kind == SyntaxKind.ImportNamedDeclaration &&
                            specifier.IsAlias &&
                            aliasSourceImport.HasExportedModuleTarget(specifier.ImportedName);
                        if (namespaceName.Length > 0 &&
                            ((!isSourceImport && specifier.IsAlias) || shouldEmitRelativeTypeAlias || shouldEmitRelativeModuleAlias) &&
                            seenAliases.Add(specifier.LocalName))
                        {
                            var qualifiedName = shouldEmitRelativeTypeAlias
                                ? aliasSourceImport!.ResolveExportedTypeQualifiedName(specifier.ImportedName)
                                : shouldEmitRelativeModuleAlias
                                    ? aliasSourceImport!.ResolveExportedModuleQualifiedName(specifier.ImportedName)
                                : $"{namespaceName}.{specifier.ImportedName}";
                            aliases.Add(new CSharpImportAlias(specifier.LocalName, qualifiedName));
                        }
                    }

                    continue;
                }

                if (child.Kind == SyntaxKind.ImportNamespaceDeclaration)
                {
                    var moduleSpecifier = child.Children.FirstOrDefault(grandchild => grandchild.IsToken && grandchild.Kind == SyntaxKind.StringLiteralToken);
                    if (!TryUnquoteStringLiteral(moduleSpecifier?.Text, out var namespaceName) ||
                        !TryGetNamespaceImportAlias(child, out var alias))
                    {
                        continue;
                    }

                    var qualifiedName = namespaceName;
                    if (sourceImports.TryGetValue(namespaceName, out var sourceImport))
                    {
                        qualifiedName = sourceImport.QualifiedModuleContainer;
                    }

                    if (seenAliases.Add(alias))
                    {
                        aliases.Add(new CSharpImportAlias(alias, qualifiedName));
                        if (seenImportedNames.Add(alias))
                        {
                            importedNames.Add(alias);
                        }
                    }

                    continue;
                }

                if (child.Kind == SyntaxKind.ImportStaticDeclaration)
                {
                    var typeName = child.Children.FirstOrDefault(grandchild => grandchild.Kind == SyntaxKind.TypeName);
                    var qualifiedName = GetQualifiedName(typeName);
                    if (qualifiedName.Length > 0 && seenStaticUsings.Add(qualifiedName))
                    {
                        staticUsings.Add(qualifiedName);
                    }
                }

                if (child.Kind == SyntaxKind.OpenDeclaration)
                {
                    var namespaceName = GetQualifiedName(child);
                    if (namespaceName.Length > 0 && seenUsings.Add(namespaceName))
                    {
                        usings.Add(namespaceName);
                    }
                }
            }

            return new CSharpImports(usings, aliases, staticUsings, importedNames);
        }

        private void EmitModuleDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var partialModifier = GetPartialModifier(node);
            var name = GetModuleDeclarationName(node);

            _builder.AppendLine($"    {visibility} static{partialModifier} class {name}");
            _builder.AppendLine("    {");

            var literals = node.Children
                .Where(child => child.Kind == SyntaxKind.LiteralDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            var functions = node.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            foreach (var literal in literals)
            {
                EmitLiteralDeclaration(literal);
            }

            if (literals.Length > 0 && functions.Length > 0)
            {
                _builder.AppendLine();
            }

            for (var index = 0; index < functions.Length; index++)
            {
                if (index > 0)
                {
                    _builder.AppendLine();
                }

                EmitFunction(functions[index], isStatic: true);
            }

            _builder.AppendLine("    }");
        }

        private void EmitRecordDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var partialModifier = GetPartialModifier(node);
            var name = GetRecordDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameters(node);

            _builder.AppendLine($"    {visibility} sealed{partialModifier} class {name}{typeParameters}");
            EmitWhereClauses(node, "    ");
            _builder.AppendLine("    {");

            if (parameters.Count > 0)
            {
                _builder.AppendLine($"        public {name}({FormatParameters(parameters)})");
                _builder.AppendLine("        {");
                foreach (var parameter in parameters)
                {
                    _builder.AppendLine($"            this.{parameter.Name} = {parameter.Name};");
                }

                _builder.AppendLine("        }");
                _builder.AppendLine();
            }

            foreach (var parameter in parameters)
            {
                _builder.AppendLine($"        public {parameter.Type} {parameter.Name} {{ get; }}");
            }

            if (parameters.Count > 0)
            {
                _builder.AppendLine();
            }

            EmitRecordEquals(name, parameters);
            _builder.AppendLine();
            EmitRecordGetHashCode(parameters);

            _builder.AppendLine("    }");
        }

        private void EmitEnumDeclaration(SyntaxNode node)
        {
            var name = GetEnumDeclarationName(node);
            var visibility = GetVisibility(node);
            var underlyingType = GetEnumUnderlyingType(node);
            var members = node.Children
                .Where(child => child.Kind == SyntaxKind.EnumMember)
                .Select(GetEnumMemberShape)
                .Where(member => member.Name.Length > 0)
                .ToArray();
            EmitAttributeLists(node, "    ");
            _builder.AppendLine($"    {visibility} enum {name}{underlyingType}");
            _builder.AppendLine("    {");
            for (var index = 0; index < members.Length; index++)
            {
                EmitAttributeLists(members[index].Node, "        ");
                var valueSuffix = members[index].ExplicitValue is { Length: > 0 } value
                    ? $" = {value}"
                    : string.Empty;
                var suffix = index == members.Length - 1 ? string.Empty : ",";
                _builder.AppendLine($"        {members[index].Name}{valueSuffix}{suffix}");
            }

            _builder.AppendLine("    }");
        }

        private string GetEnumUnderlyingType(SyntaxNode node)
        {
            var annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            if (annotation is null)
            {
                return string.Empty;
            }

            var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
            var type = MapType(typeNode);
            return string.IsNullOrWhiteSpace(type) ? string.Empty : $" : {type}";
        }

        private void EmitAttributeLists(SyntaxNode node, string indent)
        {
            foreach (var attributeList in node.Children.Where(child => child.Kind == SyntaxKind.AttributeList))
            {
                var attributes = attributeList.Children
                    .Where(child => child.Kind == SyntaxKind.Attribute)
                    .Select(EmitAttribute)
                    .Where(attribute => attribute.Length > 0)
                    .ToArray();

                if (attributes.Length > 0)
                {
                    _builder.AppendLine($"{indent}[{string.Join(", ", attributes)}]");
                }
            }
        }

        private string EmitAttribute(SyntaxNode node)
        {
            var nameNode = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
            var name = GetQualifiedName(nameNode);
            if (name.Length == 0)
            {
                return string.Empty;
            }

            var call = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.CallExpression);
            if (call is null)
            {
                return name;
            }

            var callee = call.Children.FirstOrDefault(child => !child.IsToken);
            var arguments = call.Children
                .SkipWhile(child => !ReferenceEquals(child, callee))
                .Skip(1)
                .Where(child => !child.IsToken)
                .Select(child => EmitExpression(child));
            return $"{name}({string.Join(", ", arguments)})";
        }

        private void EmitUnionDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var partialModifier = GetPartialModifier(node);
            var name = GetUnionDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var cases = GetUnionCases(node);
            var baseType = $"{name}{typeParameters}";

            _builder.AppendLine($"    {visibility} abstract{partialModifier} class {baseType}");
            _builder.AppendLine("    {");
            _builder.AppendLine($"        private {name}()");
            _builder.AppendLine("        {");
            _builder.AppendLine("        }");

            foreach (var unionCase in cases)
            {
                _builder.AppendLine();
                EmitUnionCaseFactory(baseType, unionCase);
            }

            foreach (var unionCase in cases)
            {
                _builder.AppendLine();
                EmitUnionCaseClass(baseType, unionCase);
            }

            _builder.AppendLine("    }");
        }

        private void EmitUnionCaseFactory(string baseType, UnionCaseShape unionCase)
        {
            if (unionCase.Parameters.Count == 0)
            {
                _builder.AppendLine($"        public static {baseType} {unionCase.Name}");
                _builder.AppendLine("        {");
                _builder.AppendLine("            get");
                _builder.AppendLine("            {");
                _builder.AppendLine($"                return {unionCase.ClassName}.Instance;");
                _builder.AppendLine("            }");
                _builder.AppendLine("        }");
                return;
            }

            _builder.AppendLine($"        public static {baseType} {unionCase.Name}({FormatParameters(unionCase.Parameters)})");
            _builder.AppendLine("        {");
            _builder.AppendLine($"            return new {unionCase.ClassName}({FormatArgumentNames(unionCase.Parameters)});");
            _builder.AppendLine("        }");
        }

        private void EmitUnionCaseClass(string baseType, UnionCaseShape unionCase)
        {
            _builder.AppendLine($"        public sealed class {unionCase.ClassName} : {baseType}, ITypeSharpUnionCase");
            _builder.AppendLine("        {");
            if (unionCase.Parameters.Count == 0)
            {
                EmitPayloadlessUnionCaseBody(unionCase);
            }
            else
            {
                EmitPayloadUnionCaseBody(unionCase);
            }

            _builder.AppendLine("        }");
        }

        private void EmitPayloadlessUnionCaseBody(UnionCaseShape unionCase)
        {
            _builder.AppendLine($"            public static readonly {unionCase.ClassName} Instance = new {unionCase.ClassName}();");
            _builder.AppendLine();
            _builder.AppendLine($"            private {unionCase.ClassName}()");
            _builder.AppendLine("            {");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            EmitUnionMetadataProperties(unionCase, hasPayload: false);
            _builder.AppendLine();
            _builder.AppendLine("            public object Payload");
            _builder.AppendLine("            {");
            _builder.AppendLine("                get");
            _builder.AppendLine("                {");
            _builder.AppendLine("                    throw new System.InvalidOperationException(\"Union case has no payload.\");");
            _builder.AppendLine("                }");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            _builder.AppendLine("            public override bool Equals(object obj)");
            _builder.AppendLine("            {");
            _builder.AppendLine($"                return obj is {unionCase.ClassName};");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            _builder.AppendLine("            public override int GetHashCode()");
            _builder.AppendLine("            {");
            _builder.AppendLine("                return TypeSharpUnion.CombineHash(Tag, null);");
            _builder.AppendLine("            }");
        }

        private void EmitPayloadUnionCaseBody(UnionCaseShape unionCase)
        {
            _builder.AppendLine($"            public {unionCase.ClassName}({FormatParameters(unionCase.Parameters)})");
            _builder.AppendLine("            {");
            foreach (var parameter in unionCase.Parameters)
            {
                _builder.AppendLine($"                this.{parameter.Name} = {parameter.Name};");
            }

            _builder.AppendLine("            }");
            _builder.AppendLine();
            foreach (var parameter in unionCase.Parameters)
            {
                _builder.AppendLine($"            public {parameter.Type} {parameter.Name} {{ get; }}");
            }

            _builder.AppendLine();
            EmitUnionMetadataProperties(unionCase, hasPayload: true);
            _builder.AppendLine();
            _builder.AppendLine("            public object Payload");
            _builder.AppendLine("            {");
            _builder.AppendLine("                get");
            _builder.AppendLine("                {");
            _builder.AppendLine($"                    return {FormatPayloadExpression(unionCase.Parameters)};");
            _builder.AppendLine("                }");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            EmitUnionCaseEquals(unionCase);
            _builder.AppendLine();
            EmitUnionCaseGetHashCode(unionCase);
        }

        private void EmitUnionMetadataProperties(UnionCaseShape unionCase, bool hasPayload)
        {
            _builder.AppendLine("            public int Tag");
            _builder.AppendLine("            {");
            _builder.AppendLine("                get");
            _builder.AppendLine("                {");
            _builder.AppendLine($"                    return {unionCase.Tag};");
            _builder.AppendLine("                }");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            _builder.AppendLine("            public string CaseName");
            _builder.AppendLine("            {");
            _builder.AppendLine("                get");
            _builder.AppendLine("                {");
            _builder.AppendLine($"                    return \"{unionCase.Name}\";");
            _builder.AppendLine("                }");
            _builder.AppendLine("            }");
            _builder.AppendLine();
            _builder.AppendLine("            public bool HasPayload");
            _builder.AppendLine("            {");
            _builder.AppendLine("                get");
            _builder.AppendLine("                {");
            _builder.AppendLine($"                    return {hasPayload.ToString().ToLowerInvariant()};");
            _builder.AppendLine("                }");
            _builder.AppendLine("            }");
        }

        private void EmitUnionCaseEquals(UnionCaseShape unionCase)
        {
            _builder.AppendLine("            public override bool Equals(object obj)");
            _builder.AppendLine("            {");
            _builder.AppendLine($"                var other = obj as {unionCase.ClassName};");
            var comparisons = string.Join(" && ", unionCase.Parameters.Select(parameter => $"object.Equals({parameter.Name}, other.{parameter.Name})"));
            _builder.AppendLine($"                return other != null && {comparisons};");
            _builder.AppendLine("            }");
        }

        private void EmitUnionCaseGetHashCode(UnionCaseShape unionCase)
        {
            _builder.AppendLine("            public override int GetHashCode()");
            _builder.AppendLine("            {");
            if (unionCase.Parameters.Count == 1)
            {
                _builder.AppendLine($"                return TypeSharpUnion.CombineHash(Tag, {unionCase.Parameters[0].Name});");
            }
            else
            {
                _builder.AppendLine($"                return TypeSharpEquality.CombineHash(Tag, {FormatArgumentNames(unionCase.Parameters)});");
            }

            _builder.AppendLine("            }");
        }

        private void EmitClassDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var partialModifier = GetPartialModifier(node);
            var name = GetClassDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);

            _builder.AppendLine($"    {visibility}{partialModifier} class {name}{typeParameters}");
            EmitWhereClauses(node, "    ");
            _builder.AppendLine("    {");

            var functions = node.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            for (var index = 0; index < functions.Length; index++)
            {
                if (index > 0)
                {
                    _builder.AppendLine();
                }

                EmitFunction(functions[index], isStatic: false);
            }

            _builder.AppendLine("    }");
        }

        private void EmitInterfaceDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var partialModifier = GetPartialModifier(node);
            var name = GetInterfaceDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);

            _builder.AppendLine($"    {visibility}{partialModifier} interface {name}{typeParameters}");
            EmitWhereClauses(node, "    ");
            _builder.AppendLine("    {");

            var functions = node.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            for (var index = 0; index < functions.Length; index++)
            {
                if (index > 0)
                {
                    _builder.AppendLine();
                }

                EmitInterfaceFunction(functions[index]);
            }

            _builder.AppendLine("    }");
        }

        private void EmitExtensionDeclaration(SyntaxNode node, int index)
        {
            var visibility = GetNamespaceTypeVisibility(node);
            var receiverTypeNode = GetExtensionReceiverTypeNode(node);
            var receiverType = MapType(receiverTypeNode);
            var className = GetExtensionClassName(receiverTypeNode, index);

            _builder.AppendLine($"    {visibility} static class {className}");
            _builder.AppendLine("    {");

            var functions = node.Children
                .Where(child => child.Kind == SyntaxKind.FunctionDeclaration && !IsAmbientDeclaration(child))
                .ToArray();

            for (var functionIndex = 0; functionIndex < functions.Length; functionIndex++)
            {
                if (functionIndex > 0)
                {
                    _builder.AppendLine();
                }

                EmitExtensionFunction(functions[functionIndex], receiverType);
            }

            _builder.AppendLine("    }");
        }

        private void EmitLiteralDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var name = GetLiteralDeclarationName(node);
            var initializer = GetInitializerExpression(node);
            var type = GetLiteralType(node, initializer);
            var storage = CanEmitConstLiteral(type, initializer) ? "const" : "static readonly";

            _builder.AppendLine($"        {visibility} {storage} {type} {name} = {EmitExpression(initializer)};");
        }

        private void EmitTopLevelValueDeclaration(SyntaxNode node)
        {
            var visibility = GetVisibility(node);
            var name = GetLocalDeclarationName(node);
            var initializer = GetInitializerExpression(node);
            var type = GetValueDeclarationType(node, initializer);
            var storage = IsMutableValueDeclaration(node) ? "static" : "static readonly";
            var value = initializer is null ? EmitDefaultValue(type) : EmitExpression(initializer, type);

            _builder.AppendLine($"        {visibility} {storage} {type} {name} = {value};");
        }

        private void EmitLiteralExportAlias(CSharpSourceLiteralExportAlias exportAlias)
        {
            var targetName = GetLiteralDeclarationName(exportAlias.Literal);
            var initializer = GetInitializerExpression(exportAlias.Literal);
            var type = GetLiteralType(exportAlias.Literal, initializer);
            var storage = CanEmitConstLiteral(type, initializer) ? "const" : "static readonly";

            _builder.AppendLine($"        public {storage} {type} {exportAlias.ExportedName} = {targetName};");
        }

        private void EmitValueExportAlias(CSharpSourceValueExportAlias exportAlias)
        {
            var targetName = GetLocalDeclarationName(exportAlias.Value);
            var initializer = GetInitializerExpression(exportAlias.Value);
            var type = GetValueDeclarationType(exportAlias.Value, initializer, allowDirectCompositionInference: false);

            _builder.AppendLine($"        public static {type} {exportAlias.ExportedName}");
            _builder.AppendLine("        {");
            _builder.AppendLine($"            get {{ return {targetName}; }}");
            if (IsMutableValueDeclaration(exportAlias.Value))
            {
                _builder.AppendLine($"            set {{ {targetName} = value; }}");
            }

            _builder.AppendLine("        }");
        }

        private void EmitValueImportAlias(CSharpSourceValueImportAlias importAlias)
        {
            var target = $"{importAlias.QualifiedModuleContainer}.{importAlias.TargetMemberName}";

            _builder.AppendLine($"        private static {importAlias.Type} {importAlias.LocalName}");
            _builder.AppendLine("        {");
            _builder.AppendLine($"            get {{ return {target}; }}");
            if (importAlias.IsMutable)
            {
                _builder.AppendLine($"            set {{ {target} = value; }}");
            }

            _builder.AppendLine("        }");
        }

        private void EmitValueReExport(CSharpSourceValueReExport reExport)
        {
            var target = $"{reExport.QualifiedModuleContainer}.{reExport.TargetMemberName}";

            _builder.AppendLine($"        public static {reExport.Type} {reExport.ExportedName}");
            _builder.AppendLine("        {");
            _builder.AppendLine($"            get {{ return {target}; }}");
            if (reExport.IsMutable)
            {
                _builder.AppendLine($"            set {{ {target} = value; }}");
            }

            _builder.AppendLine("        }");
        }

        private void EmitFunction(SyntaxNode node, bool isStatic)
        {
            var visibility = GetVisibility(node);
            var staticModifier = isStatic ? " static" : string.Empty;
            var asyncModifier = IsAsyncFunction(node) ? " async" : string.Empty;
            var name = GetDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameters(node);
            var returnType = GetReturnType(node);
            var body = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.FunctionBody);
            var expression = body?.Children.LastOrDefault(child => !child.IsToken);

            var previousValueTypes = _valueTypes;
            _valueTypes = new Dictionary<string, string>(previousValueTypes, StringComparer.Ordinal);
            foreach (var parameter in parameters)
            {
                _valueTypes[parameter.Name] = parameter.SourceType;
            }

            try
            {
                _builder.AppendLine($"        {visibility}{staticModifier}{asyncModifier} {returnType} {name}{typeParameters}({FormatParameters(parameters)})");
                EmitWhereClauses(node, "        ");
                _builder.AppendLine("        {");
                if (expression?.Kind == SyntaxKind.BlockExpression)
                {
                    EmitBlock(expression, returnType);
                }
                else
                {
                    _builder.AppendLine($"            return {EmitExpression(expression, returnType)};");
                }

                _builder.AppendLine("        }");
            }
            finally
            {
                _valueTypes = previousValueTypes;
            }
        }

        private void EmitRecordEquals(string name, IReadOnlyList<CSharpParameter> parameters)
        {
            _builder.AppendLine("        public override bool Equals(object obj)");
            _builder.AppendLine("        {");
            _builder.AppendLine($"            var other = obj as {name};");
            var comparisons = parameters.Count == 0
                ? "true"
                : string.Join(" && ", parameters.Select(parameter => $"object.Equals({parameter.Name}, other.{parameter.Name})"));
            _builder.AppendLine($"            return other != null && {comparisons};");
            _builder.AppendLine("        }");
        }

        private void EmitRecordGetHashCode(IReadOnlyList<CSharpParameter> parameters)
        {
            _builder.AppendLine("        public override int GetHashCode()");
            _builder.AppendLine("        {");
            _builder.AppendLine("            unchecked");
            _builder.AppendLine("            {");
            _builder.AppendLine("                var hash = 17;");
            foreach (var parameter in parameters)
            {
                _builder.AppendLine($"                hash = (hash * 397) ^ (object.Equals({parameter.Name}, null) ? 0 : {parameter.Name}.GetHashCode());");
            }

            _builder.AppendLine("                return hash;");
            _builder.AppendLine("            }");
            _builder.AppendLine("        }");
        }

        private void EmitInterfaceFunction(SyntaxNode node)
        {
            var name = GetDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameterList(node);
            var returnType = GetReturnType(node);
            var whereClauses = GetWhereClauses(node).ToArray();

            if (whereClauses.Length == 0)
            {
                _builder.AppendLine($"        {returnType} {name}{typeParameters}({parameters});");
                return;
            }

            _builder.AppendLine($"        {returnType} {name}{typeParameters}({parameters})");
            for (var index = 0; index < whereClauses.Length; index++)
            {
                var terminator = index == whereClauses.Length - 1 ? ";" : string.Empty;
                _builder.AppendLine($"            {whereClauses[index]}{terminator}");
            }
        }

        private string GetParameterList(SyntaxNode function)
        {
            var parameterList = function.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return string.Empty;
            }

            var parameters = parameterList.Children
                .Where(child => child.Kind == SyntaxKind.Parameter)
                .Select(EmitParameter);
            return string.Join(", ", parameters);
        }

        private IReadOnlyList<CSharpParameter> GetParameters(SyntaxNode declaration)
        {
            var parameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.ParameterList);
            if (parameterList is null)
            {
                return [];
            }

            return parameterList.Children
                .Where(child => child.Kind == SyntaxKind.Parameter)
                .Select(GetParameter)
                .ToArray();
        }

        private static string FormatParameters(IReadOnlyList<CSharpParameter> parameters) =>
            string.Join(", ", parameters.Select(FormatParameter));

        private static string FormatParameter(CSharpParameter parameter)
        {
            var paramsPrefix = parameter.IsParams ? "params " : string.Empty;
            var defaultSuffix = string.IsNullOrWhiteSpace(parameter.DefaultValue)
                ? string.Empty
                : $" = {parameter.DefaultValue}";
            return $"{paramsPrefix}{parameter.Type} {parameter.Name}{defaultSuffix}";
        }

        private static string FormatExtensionParameters(IReadOnlyList<CSharpParameter> parameters, string receiverType)
        {
            if (parameters.Count == 0)
            {
                return $"this {receiverType} receiver";
            }

            var first = parameters[0];
            var formatted = new List<string> { $"this {receiverType} {first.Name}" };
            formatted.AddRange(parameters.Skip(1).Select(FormatParameter));
            return string.Join(", ", formatted);
        }

        private static string FormatArgumentNames(IReadOnlyList<CSharpParameter> parameters) =>
            string.Join(", ", parameters.Select(parameter => parameter.Name));

        private static string FormatPayloadExpression(IReadOnlyList<CSharpParameter> parameters)
        {
            if (parameters.Count == 1)
            {
                return parameters[0].Name;
            }

            return $"new object[] {{ {FormatArgumentNames(parameters)} }}";
        }

        private IReadOnlyList<UnionCaseShape> GetUnionCases(SyntaxNode declaration)
        {
            var cases = new List<UnionCaseShape>();
            foreach (var unionCase in declaration.Children.Where(child => child.Kind == SyntaxKind.UnionCase))
            {
                var name = unionCase.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                cases.Add(new UnionCaseShape(
                    name,
                    $"{name}Case",
                    cases.Count,
                    GetParameters(unionCase)));
            }

            return cases;
        }

        private static string GetTypeParameterList(SyntaxNode declaration)
        {
            var typeParameterList = declaration.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeParameterList);
            if (typeParameterList is null)
            {
                return string.Empty;
            }

            var parameters = typeParameterList.Children
                .Where(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                .Select(child => child.Text ?? string.Empty)
                .Where(text => text.Length > 0)
                .ToArray();

            return parameters.Length == 0 ? string.Empty : $"<{string.Join(", ", parameters)}>";
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
                .Where(text => text.Length > 0)
                .ToArray();
        }

        private void EmitWhereClauses(SyntaxNode declaration, string indent)
        {
            foreach (var whereClause in GetWhereClauses(declaration))
            {
                _builder.AppendLine($"{indent}    {whereClause}");
            }
        }

        private IEnumerable<string> GetWhereClauses(SyntaxNode declaration)
        {
            foreach (var whereClause in declaration.Children.Where(child => child.Kind == SyntaxKind.WhereClause))
            {
                foreach (var constraint in whereClause.Children.Where(child => child.Kind == SyntaxKind.GenericConstraint))
                {
                    var parameterName = constraint.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                    if (parameterName.Length == 0)
                    {
                        continue;
                    }

                    var items = constraint.Children
                        .Where(child => child.Kind == SyntaxKind.ConstraintItem)
                        .Select(FormatConstraintItem)
                        .Where(item => item.Length > 0)
                        .ToArray();

                    if (items.Length == 0)
                    {
                        continue;
                    }

                    yield return $"where {parameterName} : {string.Join(", ", items)}";
                }
            }
        }

        private string FormatConstraintItem(SyntaxNode item)
        {
            var tokens = item.Children.Where(child => child.IsToken).ToArray();
            if (tokens.Length == 3
                && string.Equals(tokens[0].Text, "new", StringComparison.Ordinal)
                && tokens[1].Kind == SyntaxKind.OpenParenToken
                && tokens[2].Kind == SyntaxKind.CloseParenToken)
            {
                return "new()";
            }

            if (tokens.Length == 1
                && tokens[0].Text is { Length: > 0 } keyword
                && (string.Equals(keyword, "class", StringComparison.Ordinal)
                    || string.Equals(keyword, "struct", StringComparison.Ordinal)
                    || string.Equals(keyword, "notnull", StringComparison.Ordinal)))
            {
                return keyword;
            }

            var typeNode = item.Children.FirstOrDefault(child => !child.IsToken);
            return typeNode is null ? string.Empty : MapType(typeNode);
        }

        private string EmitParameter(SyntaxNode parameter)
        {
            var parameterInfo = GetParameter(parameter);
            return FormatParameter(parameterInfo);
        }

        private CSharpParameter GetParameter(SyntaxNode parameter)
        {
            var name = parameter.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text;
            var annotation = parameter.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
            return new CSharpParameter(
                MapType(typeNode),
                !string.IsNullOrWhiteSpace(name) ? name : "_",
                GetSourceTypeName(typeNode),
                IsParamsParameter(parameter),
                GetParameterDefaultValue(parameter));
        }

        private static bool IsParamsParameter(SyntaxNode parameter) =>
            parameter.Children.Any(child => child.IsToken && child.Kind == SyntaxKind.ParamsKeyword);

        private static string? GetParameterDefaultValue(SyntaxNode parameter)
        {
            var initializerExpression = parameter.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
                .Children
                .FirstOrDefault(child => !child.IsToken);
            return initializerExpression?.Kind == SyntaxKind.LiteralExpression
                ? EmitLiteral(initializerExpression)
                : null;
        }

        private void EmitBlock(
            SyntaxNode node,
            string? expectedType = null,
            string indent = "            ",
            bool returnsLastExpression = true)
        {
            var statements = node.Children.Where(child => !child.IsToken).ToArray();
            var hasYield = statements.Any(statement => statement.Kind == SyntaxKind.YieldExpression);
            for (var index = 0; index < statements.Length; index++)
            {
                var statement = statements[index];
                var isLast = index == statements.Length - 1;

                if (statement.Kind == SyntaxKind.ValueDeclaration)
                {
                    EmitLocalDeclaration(statement, indent);
                    continue;
                }

                if (statement.Kind == SyntaxKind.YieldExpression)
                {
                    EmitYieldExpression(statement, indent);
                    continue;
                }

                if (statement.Kind == SyntaxKind.LockStatement)
                {
                    EmitLockStatement(statement, indent);
                    continue;
                }

                var expression = statement.Kind == SyntaxKind.ExpressionStatement
                    ? statement.Children.FirstOrDefault(child => !child.IsToken)
                    : statement;

                if (isLast && returnsLastExpression && !hasYield)
                {
                    _builder.AppendLine($"{indent}return {EmitExpression(expression, expectedType)};");
                }
                else
                {
                    _builder.AppendLine($"{indent}{EmitExpression(expression)};");
                }
            }
        }

        private void EmitExtensionFunction(SyntaxNode node, string receiverType)
        {
            var visibility = GetVisibility(node);
            var asyncModifier = IsAsyncFunction(node) ? " async" : string.Empty;
            var name = GetDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameters(node);
            var returnType = GetReturnType(node);
            var body = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.FunctionBody);
            var expression = body?.Children.LastOrDefault(child => !child.IsToken);

            var previousValueTypes = _valueTypes;
            _valueTypes = new Dictionary<string, string>(previousValueTypes, StringComparer.Ordinal);
            foreach (var parameter in parameters)
            {
                _valueTypes[parameter.Name] = parameter.SourceType;
            }

            try
            {
                _builder.AppendLine($"        {visibility} static{asyncModifier} {returnType} {name}{typeParameters}({FormatExtensionParameters(parameters, receiverType)})");
                EmitWhereClauses(node, "        ");
                _builder.AppendLine("        {");
                if (expression?.Kind == SyntaxKind.BlockExpression)
                {
                    EmitBlock(expression, returnType);
                }
                else
                {
                    _builder.AppendLine($"            return {EmitExpression(expression, returnType)};");
                }

                _builder.AppendLine("        }");
            }
            finally
            {
                _valueTypes = previousValueTypes;
            }
        }

        private void EmitYieldExpression(SyntaxNode node, string indent)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            _builder.AppendLine($"{indent}yield return {EmitExpression(expression)};");
        }

        private void EmitLockStatement(SyntaxNode node, string indent)
        {
            var children = node.Children.Where(child => !child.IsToken).ToArray();
            var gateExpression = children.FirstOrDefault(child => child.Kind != SyntaxKind.BlockExpression);
            var body = children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);

            _builder.AppendLine($"{indent}lock ({EmitExpression(gateExpression)})");
            _builder.AppendLine($"{indent}{{");
            if (body is not null)
            {
                EmitBlock(body, indent: $"{indent}    ", returnsLastExpression: false);
            }

            _builder.AppendLine($"{indent}}}");
        }

        private void EmitLocalDeclaration(SyntaxNode node, string indent = "            ")
        {
            _builder.AppendLine($"{indent}{FormatLocalDeclaration(node)};");
            RegisterLocalDeclarationType(node);
        }

        private string FormatLocalDeclaration(SyntaxNode node)
        {
            var name = GetLocalDeclarationName(node);
            var expectedType = TryGetDirectTypeAnnotation(node, out var annotation)
                ? MapType(annotation.Children.FirstOrDefault(child => !child.IsToken))
                : null;
            var initializer = node.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
                .Children
                .FirstOrDefault(child => !child.IsToken);

            var declarationType = HasFunctionTypeAnnotation(node) && expectedType is not null
                ? expectedType
                : "var";
            return $"{declarationType} {name} = {EmitExpression(initializer, expectedType)}";
        }

        private void RegisterLocalDeclarationType(SyntaxNode node)
        {
            var name = GetLocalDeclarationName(node);
            if (TryGetDirectTypeAnnotation(node, out var sourceAnnotation))
            {
                _valueTypes[name] = GetSourceTypeName(sourceAnnotation);
            }
        }

        private void RegisterTopLevelValue(SyntaxNode node)
        {
            var name = GetLocalDeclarationName(node);
            if (name.Length == 0)
            {
                return;
            }

            _valueTypes[name] = GetValueDeclarationSourceType(node, GetInitializerExpression(node));
        }

        private void RegisterFunctionSignature(SyntaxNode node)
        {
            var name = GetDeclarationName(node);
            if (name.Length == 0)
            {
                return;
            }

            _functionSignatures[name] = new CSharpFunctionSignature(
                GetParameters(node).Select(parameter => parameter.Type).ToArray(),
                GetReturnType(node),
                GetTypeParameterNames(node));
        }

        private string EmitExpression(SyntaxNode? node, string? expectedType = null)
        {
            if (node is null)
            {
                return "default(object)";
            }

            return node.Kind switch
            {
                SyntaxKind.LiteralExpression => EmitLiteral(node),
                SyntaxKind.IdentifierExpression => EmitIdentifier(node),
                SyntaxKind.MemberAccessExpression => EmitMemberAccess(node),
                SyntaxKind.IndexerExpression => EmitIndexer(node),
                SyntaxKind.GenericNameExpression => EmitGenericName(node),
                SyntaxKind.CallExpression => EmitCall(node),
                SyntaxKind.NamedArgument => EmitNamedArgument(node),
                SyntaxKind.OutArgument => EmitOutArgument(node),
                SyntaxKind.InArgument => EmitInArgument(node),
                SyntaxKind.RefArgument => EmitRefArgument(node),
                SyntaxKind.LambdaExpression => EmitLambda(node, expectedType),
                SyntaxKind.SpreadElement => EmitSpreadElement(node),
                SyntaxKind.BinaryExpression => EmitBinary(node),
                SyntaxKind.AssignmentExpression => EmitAssignment(node),
                SyntaxKind.RecordExpression => EmitRecordExpression(node, expectedType),
                SyntaxKind.RecordUpdateExpression => EmitRecordUpdate(node),
                SyntaxKind.IfExpression => EmitIfExpression(node, expectedType),
                SyntaxKind.MatchExpression => EmitMatch(node, expectedType ?? "object"),
                SyntaxKind.AwaitExpression => EmitAwait(node),
                SyntaxKind.CollectionExpression => EmitCollection(node, expectedType),
                SyntaxKind.SatisfiesExpression => EmitSatisfiesExpression(node),
                SyntaxKind.NameofExpression => EmitNameof(node),
                SyntaxKind.CheckedExpression => EmitCheckedExpression(node),
                SyntaxKind.ParenthesizedExpression => EmitParenthesizedExpression(node),
                _ => "default(object)"
            };
        }

        private string EmitIdentifier(SyntaxNode node)
        {
            var identifier = GetIdentifierText(node);
            if (_unionCaseValues.TryGetValue(identifier, out var caseValue))
            {
                return caseValue;
            }

            return identifier.Length == 0 ? "default" : identifier;
        }

        private string EmitMemberAccess(SyntaxNode node)
        {
            var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
            var member = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            if (receiver is null || member?.Text is null)
            {
                return "default(object)";
            }

            return $"{EmitExpression(receiver)}.{member.Text}";
        }

        private string EmitIndexer(SyntaxNode node)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                return "default(object)";
            }

            return $"{EmitExpression(expressions[0])}[{EmitExpression(expressions[1])}]";
        }

        private string EmitNameof(SyntaxNode node)
        {
            var target = node.Children.FirstOrDefault(child => !child.IsToken);
            if (target?.Kind == SyntaxKind.UnboundGenericNameExpression)
            {
                return EmitStringLiteral(GetUnboundGenericNameofResult(target));
            }

            return target is null ? "nameof" : $"nameof({EmitExpression(target)})";
        }

        private static string GetUnboundGenericNameofResult(SyntaxNode node)
        {
            var target = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.UnboundGenericArityList);
            return GetNameReferenceTerminalIdentifier(target);
        }

        private static string GetNameReferenceTerminalIdentifier(SyntaxNode? node)
        {
            if (node is null)
            {
                return string.Empty;
            }

            if (node.Kind == SyntaxKind.MemberAccessExpression)
            {
                return node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            }

            if (node.Kind == SyntaxKind.UnboundGenericNameExpression)
            {
                var target = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.UnboundGenericArityList);
                return GetNameReferenceTerminalIdentifier(target);
            }

            return GetIdentifierText(node);
        }

        private static string EmitStringLiteral(string value) =>
            $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

        private string EmitSatisfiesExpression(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return EmitExpression(expression);
        }

        private string EmitCheckedExpression(SyntaxNode node)
        {
            var keyword = node.Children.FirstOrDefault(child => child.IsToken && child.Kind is SyntaxKind.CheckedKeyword or SyntaxKind.UncheckedKeyword)?.Text ?? "checked";
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"{keyword}({EmitExpression(expression)})";
        }

        private string EmitParenthesizedExpression(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"({EmitExpression(expression)})";
        }

        private string EmitIfExpression(SyntaxNode node, string? expectedType)
        {
            var returnType = string.IsNullOrWhiteSpace(expectedType) || string.Equals(expectedType, "var", StringComparison.Ordinal)
                ? InferExpressionType(node)
                : expectedType;
            if (string.IsNullOrWhiteSpace(returnType))
            {
                returnType = "object";
            }

            var builder = new StringBuilder();
            builder.Append($"(new System.Func<{returnType}>(() => ");
            AppendIfExpressionBody(builder, node, returnType);
            builder.Append("))()");
            return builder.ToString();
        }

        private void AppendIfExpressionBody(StringBuilder builder, SyntaxNode node, string returnType)
        {
            builder.Append("{ ");
            var condition = GetIfCondition(node);
            var thenBlock = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
            builder.Append($"if ({EmitExpression(condition)}) ");
            AppendInlineReturnBlock(builder, thenBlock, returnType);

            var hasExhaustiveElse = false;
            foreach (var elseClause in node.Children.Where(child => child.Kind == SyntaxKind.ElseClause))
            {
                var keywordKind = elseClause.Children.FirstOrDefault(child => child.IsToken)?.Kind;
                if (keywordKind == SyntaxKind.ElifKeyword)
                {
                    var elifCondition = elseClause.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.BlockExpression);
                    var elifBlock = elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
                    builder.Append($" else if ({EmitExpression(elifCondition)}) ");
                    AppendInlineReturnBlock(builder, elifBlock, returnType);
                    continue;
                }

                var nestedIf = elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.IfExpression);
                if (nestedIf is not null)
                {
                    builder.Append(" else ");
                    AppendIfExpressionBody(builder, nestedIf, returnType);
                    hasExhaustiveElse = true;
                    continue;
                }

                var elseBlock = elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
                builder.Append(" else ");
                AppendInlineReturnBlock(builder, elseBlock, returnType);
                hasExhaustiveElse = true;
            }

            if (!hasExhaustiveElse)
            {
                builder.Append($" return default({returnType});");
            }

            builder.Append(" }");
        }

        private void AppendInlineReturnBlock(StringBuilder builder, SyntaxNode? block, string returnType, bool returnsValue = true)
        {
            builder.Append("{ ");
            var statements = block?.Children.Where(child => !child.IsToken).ToArray() ?? [];
            if (statements.Length == 0)
            {
                if (returnsValue)
                {
                    builder.Append($"return default({returnType});");
                }

                builder.Append(" }");
                return;
            }

            for (var index = 0; index < statements.Length; index++)
            {
                var statement = statements[index];
                var isLast = index == statements.Length - 1;
                if (statement.Kind == SyntaxKind.ValueDeclaration)
                {
                    builder.Append(FormatLocalDeclaration(statement));
                    builder.Append("; ");
                    RegisterLocalDeclarationType(statement);
                    continue;
                }

                var expression = statement.Kind == SyntaxKind.ExpressionStatement
                    ? statement.Children.FirstOrDefault(child => !child.IsToken)
                    : statement;
                if (isLast && returnsValue)
                {
                    builder.Append($"return {EmitExpression(expression, returnType)};");
                }
                else
                {
                    builder.Append($"{EmitExpression(expression)}; ");
                }
            }

            builder.Append(" }");
        }

        private static SyntaxNode? GetIfCondition(SyntaxNode node) =>
            node.Children.FirstOrDefault(child => !child.IsToken &&
                child.Kind != SyntaxKind.BlockExpression &&
                child.Kind != SyntaxKind.ElseClause);

        private void EmitFunctionImportAlias(CSharpSourceFunctionImportAlias importAlias)
        {
            var node = importAlias.Function;
            var name = importAlias.LocalName;
            var targetName = GetDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameters(node);
            var returnType = GetReturnType(node);
            var invocation = $"{importAlias.QualifiedModuleContainer}.{targetName}{typeParameters}({FormatArgumentNames(parameters)})";

            _builder.AppendLine($"        private static {returnType} {name}{typeParameters}({FormatParameters(parameters)})");
            EmitWhereClauses(node, "        ");
            _builder.AppendLine("        {");
            if (string.Equals(returnType, "void", StringComparison.Ordinal))
            {
                _builder.AppendLine($"            {invocation};");
            }
            else
            {
                _builder.AppendLine($"            return {invocation};");
            }

            _builder.AppendLine("        }");
        }

        private void EmitFunctionReExport(CSharpSourceFunctionReExport reExport)
        {
            var node = reExport.Function;
            var name = reExport.ExportedName;
            var targetName = GetDeclarationName(node);
            var typeParameters = GetTypeParameterList(node);
            var parameters = GetParameters(node);
            var returnType = GetReturnType(node);
            var invocation = $"{reExport.QualifiedModuleContainer}.{targetName}{typeParameters}({FormatArgumentNames(parameters)})";

            _builder.AppendLine($"        public static {returnType} {name}{typeParameters}({FormatParameters(parameters)})");
            EmitWhereClauses(node, "        ");
            _builder.AppendLine("        {");
            if (string.Equals(returnType, "void", StringComparison.Ordinal))
            {
                _builder.AppendLine($"            {invocation};");
            }
            else
            {
                _builder.AppendLine($"            return {invocation};");
            }

            _builder.AppendLine("        }");
        }

        private string EmitGenericName(SyntaxNode node)
        {
            var target = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
            var argumentList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
            if (target is null || argumentList is null)
            {
                return EmitExpression(target);
            }

            var arguments = argumentList.Children
                .Where(child => !child.IsToken)
                .Select(MapType)
                .ToArray();

            return arguments.Length == 0
                ? EmitExpression(target)
                : $"{EmitExpression(target)}<{string.Join(", ", arguments)}>";
        }

        private string EmitCall(SyntaxNode node)
        {
            var callee = node.Children.FirstOrDefault(child => !child.IsToken);
            if (callee is null)
            {
                return "default(object)";
            }

            var arguments = node.Children
                .SkipWhile(child => !ReferenceEquals(child, callee))
                .Skip(1)
                .Where(child => !child.IsToken)
                .Select(child => EmitExpression(child));

            return EmitInvocation(callee, arguments);
        }

        private string EmitInvocation(SyntaxNode callee, IEnumerable<string> arguments)
        {
            var argumentList = string.Join(", ", arguments);
            if (TryGetCallTarget(callee, out var callTarget))
            {
                return $"{callTarget}({argumentList})";
            }

            if (TryGetConstructorName(callee, out var constructorName))
            {
                return $"new {constructorName}({argumentList})";
            }

            return $"{EmitExpression(callee)}({argumentList})";
        }

        private string EmitAwait(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"await {EmitExpression(expression)}";
        }

        private string EmitOutArgument(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"out {EmitExpression(expression)}";
        }

        private string EmitNamedArgument(SyntaxNode node)
        {
            var name = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            var expression = node.Children.LastOrDefault(child => !child.IsToken);
            return name.Length == 0
                ? EmitExpression(expression)
                : $"{name}: {EmitExpression(expression)}";
        }

        private string EmitInArgument(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"in {EmitExpression(expression)}";
        }

        private string EmitRefArgument(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return $"ref {EmitExpression(expression)}";
        }

        private string EmitLambda(SyntaxNode node, string? expectedType)
        {
            var parameter = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? "_";
            var body = node.Children.LastOrDefault(child => !child.IsToken);
            var returnType = TryGetDelegateSignature(expectedType, out var parameterTypes, out var delegateReturnType)
                ? delegateReturnType
                : InferExpressionType(body);
            if (string.IsNullOrWhiteSpace(returnType) || string.Equals(returnType, "var", StringComparison.Ordinal))
            {
                returnType = "object";
            }

            var previousValueTypes = _valueTypes;
            _valueTypes = new Dictionary<string, string>(previousValueTypes, StringComparer.Ordinal);
            if (parameterTypes.Count > 0 && parameter.Length > 0)
            {
                _valueTypes[parameter] = parameterTypes[0];
            }

            try
            {
                return body?.Kind == SyntaxKind.BlockExpression
                    ? $"{parameter} => {EmitLambdaBlockBody(body, returnType)}"
                    : $"{parameter} => {EmitExpression(body, string.Equals(returnType, "void", StringComparison.Ordinal) ? null : returnType)}";
            }
            finally
            {
                _valueTypes = previousValueTypes;
            }
        }

        private string EmitLambdaBlockBody(SyntaxNode block, string returnType)
        {
            var builder = new StringBuilder();
            AppendInlineReturnBlock(
                builder,
                block,
                string.Equals(returnType, "void", StringComparison.Ordinal) ? "object" : returnType,
                !string.Equals(returnType, "void", StringComparison.Ordinal));
            return builder.ToString();
        }

        private string EmitSpreadElement(SyntaxNode node)
        {
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            return EmitExpression(expression);
        }

        private string EmitCollection(SyntaxNode node, string? expectedType)
        {
            var elements = node.Children.Where(child => !child.IsToken).ToArray();
            if (elements.Any(element => element.Kind == SyntaxKind.SpreadElement))
            {
                return EmitSpreadCollection(elements, expectedType);
            }

            var initializer = FormatCollectionInitializer(elements);
            if (TryGetListCollectionType(expectedType, out var listType))
            {
                return $"new {listType} {initializer}";
            }

            var expectedElementType = GetArrayElementType(expectedType);
            if (expectedElementType.Length > 0)
            {
                return $"new {expectedElementType}[] {initializer}";
            }

            var inferredElementType = InferCollectionElementType(elements);
            if (inferredElementType.Length > 0)
            {
                return $"new {inferredElementType}[] {initializer}";
            }

            return elements.Length == 0
                ? "new object[] { }"
                : $"new[] {initializer}";
        }

        private string EmitSpreadCollection(IReadOnlyList<SyntaxNode> elements, string? expectedType)
        {
            var elementType = GetCollectionElementType(expectedType);
            if (elementType.Length == 0)
            {
                elementType = InferCollectionElementType(elements);
            }

            if (elementType.Length == 0)
            {
                elementType = "object";
            }

            var segments = BuildCollectionSegments(elements);
            var sequence = EmitCollectionSequence(segments, elementType);
            if (TryGetListCollectionType(expectedType, out var listType))
            {
                return $"new {listType}({sequence})";
            }

            return $"System.Linq.Enumerable.ToArray<{elementType}>({sequence})";
        }

        private string EmitCollectionSequence(IReadOnlyList<CollectionSegment> segments, string elementType)
        {
            if (segments.Count == 0)
            {
                return $"new {elementType}[] {{ }}";
            }

            var sequence = EmitCollectionSegment(segments[0], elementType);
            foreach (var segment in segments.Skip(1))
            {
                sequence = $"System.Linq.Enumerable.Concat<{elementType}>({sequence}, {EmitCollectionSegment(segment, elementType)})";
            }

            return sequence;
        }

        private string EmitCollectionSegment(CollectionSegment segment, string elementType) =>
            segment.IsSpread
                ? EmitExpression(segment.Expression)
                : $"new {elementType}[] {FormatCollectionInitializer(segment.Elements)}";

        private static IReadOnlyList<CollectionSegment> BuildCollectionSegments(IReadOnlyList<SyntaxNode> elements)
        {
            var segments = new List<CollectionSegment>();
            var run = new List<SyntaxNode>();
            foreach (var element in elements)
            {
                if (element.Kind == SyntaxKind.SpreadElement)
                {
                    AddCollectionElementRun(segments, run);
                    var expression = element.Children.FirstOrDefault(child => !child.IsToken);
                    segments.Add(new CollectionSegment(IsSpread: true, [], expression));
                    continue;
                }

                run.Add(element);
            }

            AddCollectionElementRun(segments, run);
            return segments;
        }

        private static void AddCollectionElementRun(List<CollectionSegment> segments, List<SyntaxNode> run)
        {
            if (run.Count == 0)
            {
                return;
            }

            segments.Add(new CollectionSegment(IsSpread: false, run.ToArray(), null));
            run.Clear();
        }

        private string EmitAssignment(SyntaxNode node)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            var operatorToken = node.Children.FirstOrDefault(child => child.IsToken);
            if (expressions.Length < 2 || operatorToken?.Text is null)
            {
                return "default(object)";
            }

            return $"{EmitExpression(expressions[0])} {operatorToken.Text} {EmitExpression(expressions[1])}";
        }

        private string EmitBinary(SyntaxNode node)
        {
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            var operatorToken = node.Children.FirstOrDefault(child => child.IsToken);
            if (operatorToken is null)
            {
                return "default(object)";
            }

            if (expressions.Length == 1)
            {
                return $"{operatorToken.Text}{EmitExpression(expressions[0])}";
            }

            if (expressions.Length < 2 || operatorToken.Text is null)
            {
                return "default(object)";
            }

            if (operatorToken.Kind == SyntaxKind.PipeGreaterToken)
            {
                return EmitPipeline(expressions[0], expressions[1]);
            }

            if (TryGetCompositionDirection(node, out var direction))
            {
                return EmitComposition(expressions[0], expressions[1], direction);
            }

            return $"{EmitExpression(expressions[0])} {operatorToken.Text} {EmitExpression(expressions[1])}";
        }

        private string EmitPipeline(SyntaxNode input, SyntaxNode target)
        {
            var pipedExpression = EmitExpression(input);
            if (target.Kind != SyntaxKind.CallExpression)
            {
                return $"{EmitExpression(target)}({pipedExpression})";
            }

            var callee = target.Children.FirstOrDefault(child => !child.IsToken);
            if (callee is null)
            {
                return "default(object)";
            }

            var arguments = target.Children
                .SkipWhile(child => !ReferenceEquals(child, callee))
                .Skip(1)
                .Where(child => !child.IsToken)
                .Select(child => EmitExpression(child))
                .Prepend(pipedExpression);

            return EmitInvocation(callee, arguments);
        }

        private string EmitComposition(SyntaxNode left, SyntaxNode right, CompositionDirection direction)
        {
            var parameterName = $"__compose{_temporaryIndex++}";
            var body = direction == CompositionDirection.Forward
                ? EmitComposedApplication(right, EmitComposedApplication(left, parameterName))
                : EmitComposedApplication(left, EmitComposedApplication(right, parameterName));

            return $"{parameterName} => {body}";
        }

        private string EmitComposedApplication(SyntaxNode target, string argument)
        {
            if (TryGetCompositionExpression(target, out var left, out var right, out var direction))
            {
                return direction == CompositionDirection.Forward
                    ? EmitComposedApplication(right, EmitComposedApplication(left, argument))
                    : EmitComposedApplication(left, EmitComposedApplication(right, argument));
            }

            return EmitInvocation(target, [argument]);
        }

        private static bool TryGetCompositionExpression(
            SyntaxNode node,
            out SyntaxNode left,
            out SyntaxNode right,
            out CompositionDirection direction)
        {
            left = null!;
            right = null!;
            direction = CompositionDirection.Forward;

            if (node.Kind != SyntaxKind.BinaryExpression ||
                !TryGetCompositionDirection(node, out direction))
            {
                return false;
            }

            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (expressions.Length < 2)
            {
                return false;
            }

            left = expressions[0];
            right = expressions[1];
            return true;
        }

        private static bool TryGetDirectIdentifierName(SyntaxNode node, out string name)
        {
            name = string.Empty;
            if (node.Kind != SyntaxKind.IdentifierExpression)
            {
                return false;
            }

            name = GetIdentifierText(node);
            return name.Length > 0;
        }

        private static bool TryGetCompositionDirection(SyntaxNode node, out CompositionDirection direction)
        {
            direction = CompositionDirection.Forward;
            var children = node.Children;
            for (var index = 0; index < children.Count - 1; index++)
            {
                if (!children[index].IsToken || !children[index + 1].IsToken)
                {
                    continue;
                }

                if (children[index].Kind == SyntaxKind.GreaterToken &&
                    children[index + 1].Kind == SyntaxKind.GreaterToken)
                {
                    direction = CompositionDirection.Forward;
                    return true;
                }

                if (children[index].Kind == SyntaxKind.LessToken &&
                    children[index + 1].Kind == SyntaxKind.LessToken)
                {
                    direction = CompositionDirection.Backward;
                    return true;
                }
            }

            return false;
        }

        private string EmitMatch(SyntaxNode node, string resultType)
        {
            var input = node.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.MatchArm);
            if (input is null)
            {
                return "default(object)";
            }

            if (!TryGetUnionShape(input, out var union))
            {
                if (TryGetExpressionType(input, out var inputTypeName))
                {
                    if (_enums.TryGetValue(inputTypeName, out var enumShape))
                    {
                        return EmitEnumMatch(node, input, enumShape, resultType);
                    }

                    if (string.Equals(inputTypeName, "bool", StringComparison.Ordinal))
                    {
                        return EmitLiteralMatch(node, input, resultType);
                    }
                }

                if (TryGetTypeLevelUnionShape(input, out var typeLevelUnion))
                {
                    return IsLiteralTypeLevelUnion(typeLevelUnion)
                        ? EmitLiteralMatch(node, input, resultType)
                        : EmitTypeLevelUnionMatch(node, input, typeLevelUnion, resultType);
                }

                return "default(object)";
            }

            var matchValueName = $"__match{_temporaryIndex++}";
            var builder = new StringBuilder();
            builder.AppendLine($"new System.Func<{resultType}>(delegate()");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {matchValueName} = {EmitExpression(input)};");

            var wroteArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                if (!TryGetMatchArm(arm, union, out var unionCase, out var payloadVariable, out var expression, out var isDiscard))
                {
                    continue;
                }

                var guard = GetMatchArmGuard(arm);
                var resultExpression = EmitExpression(expression);
                if (wroteArm)
                {
                    builder.AppendLine();
                }

                if (isDiscard)
                {
                    if (guard is null)
                    {
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                    else
                    {
                        builder.AppendLine($"                if ({EmitExpression(guard)})");
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                }
                else
                {
                    var predicate = unionCase.Parameters.Count == 0
                        ? "IsPayloadlessCase"
                        : "IsPayloadCase";
                    builder.AppendLine($"                if (TypeSharpPattern.{predicate}({matchValueName}, {unionCase.Tag}))");
                    builder.AppendLine("                {");
                    if (payloadVariable.Length > 0 && unionCase.Parameters.Count == 1)
                    {
                        builder.AppendLine($"                    var {payloadVariable} = TypeSharpPattern.RequirePayload<{unionCase.Parameters[0].Type}>({matchValueName}, {unionCase.Tag});");
                    }

                    if (guard is null)
                    {
                        builder.AppendLine($"                    return {resultExpression};");
                    }
                    else
                    {
                        builder.AppendLine($"                    if ({EmitExpression(guard)})");
                        builder.AppendLine("                    {");
                        builder.AppendLine($"                        return {resultExpression};");
                        builder.AppendLine("                    }");
                    }

                    builder.AppendLine("                }");
                }

                wroteArm = true;
            }

            if (wroteArm)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"                throw TypeSharpPattern.NoMatch({matchValueName});");
            builder.Append("            })()");
            return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private string EmitEnumMatch(SyntaxNode node, SyntaxNode input, EnumShape enumShape, string resultType)
        {
            var matchValueName = $"__match{_temporaryIndex++}";
            var builder = new StringBuilder();
            builder.AppendLine($"new System.Func<{resultType}>(delegate()");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {matchValueName} = {EmitExpression(input)};");

            var wroteArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                if (!TryGetEnumPatternMatchArm(arm, enumShape, out var memberName, out var expression, out var isDiscard))
                {
                    continue;
                }

                var guard = GetMatchArmGuard(arm);
                var resultExpression = EmitExpression(expression);
                if (wroteArm)
                {
                    builder.AppendLine();
                }

                if (isDiscard)
                {
                    if (guard is null)
                    {
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                    else
                    {
                        builder.AppendLine($"                if ({EmitExpression(guard)})");
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                }
                else
                {
                    builder.AppendLine($"                if (object.Equals({matchValueName}, {enumShape.Name}.{memberName}))");
                    builder.AppendLine("                {");
                    if (guard is null)
                    {
                        builder.AppendLine($"                    return {resultExpression};");
                    }
                    else
                    {
                        builder.AppendLine($"                    if ({EmitExpression(guard)})");
                        builder.AppendLine("                    {");
                        builder.AppendLine($"                        return {resultExpression};");
                        builder.AppendLine("                    }");
                    }

                    builder.AppendLine("                }");
                }

                wroteArm = true;
            }

            if (wroteArm)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"                throw TypeSharpPattern.NoMatch({matchValueName});");
            builder.Append("            })()");
            return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private string EmitLiteralMatch(SyntaxNode node, SyntaxNode input, string resultType)
        {
            var matchValueName = $"__match{_temporaryIndex++}";
            var builder = new StringBuilder();
            builder.AppendLine($"new System.Func<{resultType}>(delegate()");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {matchValueName} = {EmitExpression(input)};");

            var wroteArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                if (!TryGetLiteralPatternMatchArm(arm, out var literalExpression, out var expression, out var isDiscard))
                {
                    continue;
                }

                var guard = GetMatchArmGuard(arm);
                var resultExpression = EmitExpression(expression);
                if (wroteArm)
                {
                    builder.AppendLine();
                }

                if (isDiscard)
                {
                    if (guard is null)
                    {
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                    else
                    {
                        builder.AppendLine($"                if ({EmitExpression(guard)})");
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                }
                else
                {
                    builder.AppendLine($"                if (object.Equals({matchValueName}, {literalExpression}))");
                    builder.AppendLine("                {");
                    if (guard is null)
                    {
                        builder.AppendLine($"                    return {resultExpression};");
                    }
                    else
                    {
                        builder.AppendLine($"                    if ({EmitExpression(guard)})");
                        builder.AppendLine("                    {");
                        builder.AppendLine($"                        return {resultExpression};");
                        builder.AppendLine("                    }");
                    }

                    builder.AppendLine("                }");
                }

                wroteArm = true;
            }

            if (wroteArm)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"                throw TypeSharpPattern.NoMatch({matchValueName});");
            builder.Append("            })()");
            return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private string EmitTypeLevelUnionMatch(SyntaxNode node, SyntaxNode input, TypeLevelUnionShape union, string resultType)
        {
            var matchValueName = $"__match{_temporaryIndex++}";
            var builder = new StringBuilder();
            builder.AppendLine($"new System.Func<{resultType}>(delegate()");
            builder.AppendLine("            {");
            builder.AppendLine($"                var {matchValueName} = {EmitExpression(input)};");

            var wroteArm = false;
            foreach (var arm in node.Children.Where(child => child.Kind == SyntaxKind.MatchArm))
            {
                if (!TryGetTypePatternMatchArm(arm, union, out var member, out var variableName, out var expression, out var isDiscard))
                {
                    continue;
                }

                var guard = GetMatchArmGuard(arm);
                var resultExpression = EmitExpression(expression);
                if (wroteArm)
                {
                    builder.AppendLine();
                }

                if (isDiscard)
                {
                    if (guard is null)
                    {
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                    else
                    {
                        builder.AppendLine($"                if ({EmitExpression(guard)})");
                        builder.AppendLine("                {");
                        builder.AppendLine($"                    return {resultExpression};");
                        builder.AppendLine("                }");
                    }
                }
                else
                {
                    builder.AppendLine($"                if ({matchValueName} is {member.CSharpType} {variableName})");
                    builder.AppendLine("                {");
                    if (guard is null)
                    {
                        builder.AppendLine($"                    return {resultExpression};");
                    }
                    else
                    {
                        builder.AppendLine($"                    if ({EmitExpression(guard)})");
                        builder.AppendLine("                    {");
                        builder.AppendLine($"                        return {resultExpression};");
                        builder.AppendLine("                    }");
                    }

                    builder.AppendLine("                }");
                }

                wroteArm = true;
            }

            if (wroteArm)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"                throw TypeSharpPattern.NoMatch({matchValueName});");
            builder.Append("            })()");
            return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        private string EmitRecordUpdate(SyntaxNode node)
        {
            var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
            var update = node.Children.LastOrDefault(child => child.Kind == SyntaxKind.RecordExpression);
            if (receiver is null || update is null || !TryGetRecordShape(receiver, out var record))
            {
                return "default(object)";
            }

            var receiverExpression = EmitExpression(receiver);
            var updatedFields = GetRecordFieldExpressions(update, record);
            var arguments = record.Parameters
                .Select(parameter => updatedFields.TryGetValue(parameter.Name, out var updatedValue)
                    ? updatedValue
                    : $"{receiverExpression}.{parameter.Name}");

            return $"new {record.Name}({string.Join(", ", arguments)})";
        }

        private string EmitRecordExpression(SyntaxNode node, string? expectedType)
        {
            if (string.IsNullOrWhiteSpace(expectedType) || !_records.TryGetValue(expectedType, out var record))
            {
                return "default(object)";
            }

            var fields = GetRecordFieldExpressions(node, record);
            var arguments = record.Parameters.Select(parameter =>
                fields.TryGetValue(parameter.Name, out var expression)
                    ? expression
                    : $"default({parameter.Type})");

            return $"new {record.Name}({string.Join(", ", arguments)})";
        }

        private Dictionary<string, string> GetRecordFieldExpressions(SyntaxNode recordExpression, RecordShape? expectedRecord = null)
        {
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var field in recordExpression.Children.Where(child => child.Kind is SyntaxKind.RecordField or SyntaxKind.RecordSpreadField))
            {
                if (field.Kind == SyntaxKind.RecordSpreadField)
                {
                    AddRecordSpreadFieldExpressions(field, expectedRecord, fields);
                    continue;
                }

                var name = field.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                if (name.Length == 0)
                {
                    continue;
                }

                var expression = field.Children.LastOrDefault(child => !child.IsToken);
                fields[name] = expression is null ? name : EmitExpression(expression);
            }

            return fields;
        }

        private void AddRecordSpreadFieldExpressions(
            SyntaxNode spreadField,
            RecordShape? expectedRecord,
            Dictionary<string, string> fields)
        {
            if (!expectedRecord.HasValue)
            {
                return;
            }

            var expression = spreadField.Children.FirstOrDefault(child => !child.IsToken);
            if (expression is null)
            {
                return;
            }

            var spreadExpression = EmitExpression(expression);
            foreach (var parameter in expectedRecord.Value.Parameters)
            {
                fields[parameter.Name] = $"{spreadExpression}.{parameter.Name}";
            }
        }

        private bool TryGetRecordShape(SyntaxNode receiver, out RecordShape record)
        {
            record = default;
            if (!TryGetExpressionType(receiver, out var typeName))
            {
                return false;
            }

            return _records.TryGetValue(typeName, out record);
        }

        private bool TryGetUnionShape(SyntaxNode receiver, out UnionShape union)
        {
            union = default;
            if (!TryGetExpressionType(receiver, out var typeName))
            {
                return false;
            }

            return _unions.TryGetValue(typeName, out union);
        }

        private bool TryGetTypeLevelUnionShape(SyntaxNode receiver, out TypeLevelUnionShape union)
        {
            union = default;
            if (!TryGetExpressionType(receiver, out var typeName))
            {
                return false;
            }

            return _typeLevelUnions.TryGetValue(typeName, out union);
        }

        private static bool TryGetMatchArm(
            SyntaxNode arm,
            UnionShape union,
            out UnionCaseShape unionCase,
            out string payloadVariable,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            unionCase = default;
            payloadVariable = string.Empty;
            expression = null;
            isDiscard = false;

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            var caseName = pattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (caseName == "_")
            {
                expression = GetMatchArmExpression(arm);
                isDiscard = expression is not null;
                return isDiscard;
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
            payloadVariable = argumentPattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            expression = GetMatchArmExpression(arm);
            return expression is not null;
        }

        private static bool TryGetLiteralPatternMatchArm(
            SyntaxNode arm,
            out string literalExpression,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            literalExpression = string.Empty;
            expression = null;
            isDiscard = false;

            var pattern = GetMatchArmPattern(arm);
            var identifier = pattern?
                .Children
                .FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?
                .Text ?? string.Empty;
            if (identifier == "_")
            {
                expression = GetMatchArmExpression(arm);
                isDiscard = expression is not null;
                return isDiscard;
            }

            if (pattern is null || !TryGetLiteralPatternExpression(pattern, out literalExpression))
            {
                return false;
            }

            expression = GetMatchArmExpression(arm);
            return expression is not null;
        }

        private static bool TryGetEnumPatternMatchArm(
            SyntaxNode arm,
            EnumShape enumShape,
            out string memberName,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            memberName = string.Empty;
            expression = null;
            isDiscard = false;

            var pattern = GetMatchArmPattern(arm);
            var identifier = pattern?
                .Children
                .FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?
                .Text ?? string.Empty;
            if (identifier == "_")
            {
                expression = GetMatchArmExpression(arm);
                isDiscard = expression is not null;
                return isDiscard;
            }

            if (identifier.Length == 0 || !enumShape.Members.Contains(identifier))
            {
                return false;
            }

            memberName = identifier;
            expression = GetMatchArmExpression(arm);
            return expression is not null;
        }

        private bool TryGetTypePatternMatchArm(
            SyntaxNode arm,
            TypeLevelUnionShape union,
            out TypeLevelUnionMemberShape member,
            out string variableName,
            out SyntaxNode? expression,
            out bool isDiscard)
        {
            member = default;
            variableName = string.Empty;
            expression = null;
            isDiscard = false;

            var pattern = arm.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Pattern);
            variableName = pattern?.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (variableName == "_")
            {
                expression = GetMatchArmExpression(arm);
                isDiscard = expression is not null;
                return isDiscard;
            }

            var annotation = pattern?.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
            var sourceType = GetSourceTypeName(typeNode);
            var foundMember = union.Members.FirstOrDefault(candidate => candidate.SourceType == sourceType);
            if (foundMember.SourceType is null)
            {
                return false;
            }

            member = foundMember;
            expression = GetMatchArmExpression(arm);
            return variableName.Length > 0 && expression is not null;
        }

        private static SyntaxNode? GetMatchArmExpression(SyntaxNode arm) =>
            arm.Children.LastOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.Pattern && child.Kind != SyntaxKind.RecordPattern);

        private static SyntaxNode? GetMatchArmPattern(SyntaxNode arm) =>
            arm.Children.FirstOrDefault(child => child.Kind is SyntaxKind.Pattern or SyntaxKind.RecordPattern);

        private static bool TryGetLiteralPatternExpression(SyntaxNode pattern, out string expression)
        {
            var token = pattern.Children.FirstOrDefault(child => child.IsToken);
            expression = token?.Kind switch
            {
                SyntaxKind.StringLiteralToken => token.Text ?? "\"\"",
                SyntaxKind.NumericLiteralToken => token.Text ?? "0",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.FalseKeyword => "false",
                _ => string.Empty
            };

            return expression.Length > 0;
        }

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

        private static bool IsLiteralTypeLevelUnion(TypeLevelUnionShape union) =>
            union.Members.Count > 0 &&
            union.Members.All(member => IsLiteralSourceType(member.SourceType));

        private static bool IsLiteralSourceType(string sourceType) =>
            sourceType is "true" or "false" ||
            (sourceType.StartsWith("\"", StringComparison.Ordinal) && sourceType.EndsWith("\"", StringComparison.Ordinal)) ||
            (sourceType.Length > 0 && char.IsDigit(sourceType[0]));

        private bool TryGetExpressionType(SyntaxNode node, out string typeName)
        {
            typeName = string.Empty;
            if (node.Kind == SyntaxKind.ParenthesizedExpression)
            {
                var expression = node.Children.FirstOrDefault(child => !child.IsToken);
                return expression is not null && TryGetExpressionType(expression, out typeName);
            }

            if (node.Kind != SyntaxKind.IdentifierExpression)
            {
                return false;
            }

            var name = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            if (name.Length == 0)
            {
                return false;
            }

            if (!_valueTypes.TryGetValue(name, out var knownType) || knownType.Length == 0)
            {
                return false;
            }

            typeName = knownType;
            return true;
        }

        private static string EmitLiteral(SyntaxNode node)
        {
            var token = node.Children.FirstOrDefault(child => child.IsToken);
            return token?.Kind switch
            {
                SyntaxKind.StringLiteralToken => token.Text ?? "\"\"",
                SyntaxKind.InterpolatedStringLiteralToken => token.Text ?? "\"\"",
                SyntaxKind.NumericLiteralToken => token.Text ?? "0",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.NullKeyword => "null",
                _ => "default(object)"
            };
        }

        private string GetReturnType(SyntaxNode function)
        {
            var annotation = function.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
            return MapType(typeNode);
        }

        private string MapType(SyntaxNode? node)
        {
            if (node is null)
            {
                return "object";
            }

            if (node.Kind == SyntaxKind.TypeName)
            {
                if (TryGetGenericType(node, out var genericType))
                {
                    return genericType;
                }

                var name = GetQualifiedName(node);
                if (_compileTimeOnlyTypeAliasRuntimeTypes.TryGetValue(name, out var runtimeType))
                {
                    return runtimeType;
                }

                if (_typeLevelUnions.ContainsKey(name) || _compileTimeOnlyTypeAliases.Contains(name))
                {
                    return "object";
                }

                return name switch
                {
                    "bool" => "bool",
                    "byte" => "byte",
                    "char" => "char",
                    "decimal" => "decimal",
                    "double" => "double",
                    "float" => "float",
                    "int" => "int",
                    "long" => "long",
                    "object" => "object",
                    "sbyte" => "sbyte",
                    "short" => "short",
                    "string" => "string",
                    "uint" => "uint",
                    "ulong" => "ulong",
                    "ushort" => "ushort",
                    "void" or "unit" => "void",
                    _ => name
                };
            }

            if (node.Kind == SyntaxKind.ArrayType)
            {
                var elementType = node.Children.FirstOrDefault(child => !child.IsToken);
                return $"{MapType(elementType)}[]";
            }

            if (node.Kind == SyntaxKind.NullableType)
            {
                var innerType = MapType(node.Children.FirstOrDefault(child => !child.IsToken));
                return IsCSharpNonNullableValueType(innerType) ? $"{innerType}?" : innerType;
            }

            if (node.Kind == SyntaxKind.FunctionType)
            {
                return MapFunctionType(node);
            }

            if (node.Kind == SyntaxKind.LiteralType)
            {
                return MapLiteralRuntimeType(node);
            }

            if (node.Kind == SyntaxKind.KeyofType)
            {
                return "string";
            }

            if (node.Kind == SyntaxKind.IndexedAccessType)
            {
                return TryMapIndexedAccessType(node, out var indexedAccessType)
                    ? indexedAccessType
                    : "object";
            }

            if (node.Kind is SyntaxKind.UnionType or SyntaxKind.IntersectionType or SyntaxKind.RecordShapeType)
            {
                return "object";
            }

            return "object";
        }

        private static bool IsCSharpNonNullableValueType(string typeName) =>
            typeName is "bool"
                or "byte"
                or "char"
                or "decimal"
                or "double"
                or "float"
                or "int"
                or "long"
                or "sbyte"
                or "short"
                or "uint"
                or "ulong"
                or "ushort";

        private bool TryMapIndexedAccessType(SyntaxNode node, out string mappedType)
        {
            mappedType = "object";
            var typeNodes = node.Children.Where(child => !child.IsToken).ToArray();
            if (typeNodes.Length < 2)
            {
                return false;
            }

            var targetName = GetSourceTypeName(typeNodes[0]);
            if (!TryGetKnownShape(targetName, out var shape) ||
                !TryGetIndexedAccessKeyNames(typeNodes[1], out var keyNames))
            {
                return false;
            }

            var mappedTypes = new List<string>();
            foreach (var keyName in keyNames.Distinct(StringComparer.Ordinal))
            {
                var parameter = shape.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Name, keyName, StringComparison.Ordinal));
                if (parameter.Name.Length == 0 || parameter.Type.Length == 0)
                {
                    return false;
                }

                mappedTypes.Add(parameter.Type);
            }

            var distinctTypes = mappedTypes.Distinct(StringComparer.Ordinal).ToArray();
            if (distinctTypes.Length != 1)
            {
                return false;
            }

            mappedType = distinctTypes[0];
            return true;
        }

        private bool TryGetIndexedAccessKeyNames(SyntaxNode node, out IReadOnlyList<string> keyNames)
        {
            var keys = new List<string>();
            if (node.Kind == SyntaxKind.UnionType)
            {
                foreach (var memberNode in node.Children.Where(child => !child.IsToken))
                {
                    if (!TryGetIndexedAccessKeyNames(memberNode, out var memberKeys))
                    {
                        keyNames = [];
                        return false;
                    }

                    keys.AddRange(memberKeys);
                }

                keyNames = keys.Distinct(StringComparer.Ordinal).ToArray();
                return keys.Count > 0;
            }

            if (node.Kind == SyntaxKind.KeyofType)
            {
                var target = node.Children.FirstOrDefault(child => !child.IsToken);
                var targetName = GetSourceTypeName(target);
                if (!TryGetKnownShape(targetName, out var shape))
                {
                    keyNames = [];
                    return false;
                }

                keyNames = shape.Parameters.Select(parameter => parameter.Name).Where(name => name.Length > 0).Distinct(StringComparer.Ordinal).ToArray();
                return keyNames.Count > 0;
            }

            if (node.Kind == SyntaxKind.TypeName)
            {
                var name = GetQualifiedName(node);
                if (!_typeLevelUnions.TryGetValue(name, out var typeLevelUnion))
                {
                    keyNames = [];
                    return false;
                }

                foreach (var member in typeLevelUnion.Members)
                {
                    if (!TryUnquoteStringLiteral(member.SourceType, out var keyName))
                    {
                        keyNames = [];
                        return false;
                    }

                    keys.Add(keyName);
                }

                keyNames = keys.Distinct(StringComparer.Ordinal).ToArray();
                return keys.Count > 0;
            }

            if (node.Kind == SyntaxKind.LiteralType &&
                TryUnquoteStringLiteral(node.Children.FirstOrDefault(child => child.IsToken)?.Text, out var directKey))
            {
                keyNames = [directKey];
                return true;
            }

            keyNames = [];
            return false;
        }

        private bool TryGetKnownShape(string name, out RecordShape shape)
        {
            if (_records.TryGetValue(name, out shape))
            {
                return true;
            }

            return _structuralShapes.TryGetValue(name, out shape);
        }

        private RecordShape GetStructuralShape(string name, SyntaxNode node)
        {
            var members = new List<CSharpParameter>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in node.Children.Where(child => child.Kind == SyntaxKind.ShapeMember))
            {
                var memberName = member.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
                var memberType = member.Children.LastOrDefault(child => !child.IsToken);
                if (memberName.Length == 0 || memberType is null || !seen.Add(memberName))
                {
                    continue;
                }

                members.Add(new CSharpParameter(MapType(memberType), memberName, GetSourceTypeName(memberType), IsParams: false));
            }

            return new RecordShape(name, members);
        }

        private bool TryGetIntersectionStructuralShape(string name, SyntaxNode node, out RecordShape shape)
        {
            var members = new List<CSharpParameter>();
            var memberIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var memberNode in node.Children.Where(child => !child.IsToken))
            {
                var memberShapeName = GetSourceTypeName(memberNode);
                if (!TryGetKnownShape(memberShapeName, out var memberShape))
                {
                    shape = default;
                    return false;
                }

                foreach (var member in memberShape.Parameters)
                {
                    if (memberIndexes.TryGetValue(member.Name, out var existingIndex))
                    {
                        if (!string.Equals(members[existingIndex].Type, member.Type, StringComparison.Ordinal))
                        {
                            shape = default;
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
                shape = default;
                return false;
            }

            shape = new RecordShape(name, members);
            return true;
        }

        private string MapFunctionType(SyntaxNode node)
        {
            var types = node.Children.Where(child => !child.IsToken).ToArray();
            if (types.Length < 2)
            {
                return "System.Func<object>";
            }

            var parameterType = MapType(types[0]);
            var returnType = MapType(types[1]);
            if (string.Equals(parameterType, "void", StringComparison.Ordinal))
            {
                return string.Equals(returnType, "void", StringComparison.Ordinal)
                    ? "System.Action"
                    : $"System.Func<{returnType}>";
            }

            return string.Equals(returnType, "void", StringComparison.Ordinal)
                ? $"System.Action<{parameterType}>"
                : $"System.Func<{parameterType}, {returnType}>";
        }

        private static bool TryGetDelegateSignature(string? typeName, out IReadOnlyList<string> parameterTypes, out string returnType)
        {
            parameterTypes = [];
            returnType = string.Empty;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            var trimmed = typeName.Trim();
            if (string.Equals(trimmed, "System.Action", StringComparison.Ordinal) ||
                string.Equals(trimmed, "Action", StringComparison.Ordinal))
            {
                returnType = "void";
                return true;
            }

            if (!TryGetGenericArguments(trimmed, out var genericName, out var arguments))
            {
                return false;
            }

            if (string.Equals(genericName, "System.Action", StringComparison.Ordinal) ||
                string.Equals(genericName, "Action", StringComparison.Ordinal))
            {
                parameterTypes = arguments;
                returnType = "void";
                return true;
            }

            if (string.Equals(genericName, "System.Func", StringComparison.Ordinal) ||
                string.Equals(genericName, "Func", StringComparison.Ordinal))
            {
                returnType = arguments[^1];
                parameterTypes = arguments.Take(arguments.Count - 1).ToArray();
                return true;
            }

            return false;
        }

        private static bool TryGetGenericArguments(string typeName, out string genericName, out IReadOnlyList<string> arguments)
        {
            genericName = string.Empty;
            arguments = [];

            var open = typeName.IndexOf('<', StringComparison.Ordinal);
            var close = typeName.LastIndexOf('>');
            if (open <= 0 || close <= open + 1 || close != typeName.Length - 1)
            {
                return false;
            }

            genericName = typeName[..open].Trim();
            var inner = typeName.Substring(open + 1, close - open - 1);
            var parsed = new List<string>();
            var depth = 0;
            var start = 0;
            for (var index = 0; index < inner.Length; index++)
            {
                var current = inner[index];
                if (current == '<')
                {
                    depth++;
                    continue;
                }

                if (current == '>')
                {
                    depth--;
                    if (depth < 0)
                    {
                        return false;
                    }

                    continue;
                }

                if (current == ',' && depth == 0)
                {
                    var argument = inner.Substring(start, index - start).Trim();
                    if (argument.Length == 0)
                    {
                        return false;
                    }

                    parsed.Add(argument);
                    start = index + 1;
                }
            }

            if (depth != 0)
            {
                return false;
            }

            var last = inner[start..].Trim();
            if (last.Length == 0)
            {
                return false;
            }

            parsed.Add(last);
            arguments = parsed;
            return genericName.Length > 0;
        }

        private static string MapLiteralRuntimeType(SyntaxNode node)
        {
            var token = node.Children.FirstOrDefault(child => child.IsToken);
            return token?.Kind switch
            {
                SyntaxKind.StringLiteralToken => "string",
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => "bool",
                SyntaxKind.NumericLiteralToken => (token.Text ?? string.Empty).Contains('.', StringComparison.Ordinal) ? "double" : "int",
                _ => "object"
            };
        }

        private bool TryGetGenericType(SyntaxNode node, out string type)
        {
            type = string.Empty;
            var baseType = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
            var argumentList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
            if (baseType is null || argumentList is null)
            {
                return false;
            }

            var baseName = MapType(baseType);
            var arguments = argumentList.Children
                .Where(child => !child.IsToken)
                .Select(MapType)
                .ToArray();
            if (baseName.Length == 0 || arguments.Length == 0)
            {
                return false;
            }

            type = $"{baseName}<{string.Join(", ", arguments)}>";
            return true;
        }

        private string GetLiteralType(SyntaxNode literal, SyntaxNode? initializer)
        {
            var annotation = literal.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation);
            var typeNode = annotation?.Children.FirstOrDefault(child => !child.IsToken);
            if (typeNode is not null)
            {
                return MapType(typeNode);
            }

            return InferLiteralType(initializer);
        }

        private string GetValueDeclarationType(
            SyntaxNode value,
            SyntaxNode? initializer,
            bool allowDirectCompositionInference = true)
        {
            if (TryGetDirectTypeAnnotation(value, out var annotation))
            {
                return MapType(annotation.Children.FirstOrDefault(child => !child.IsToken));
            }

            if (TryInferLambdaFunctionType(initializer, out var lambdaType))
            {
                return lambdaType;
            }

            if (allowDirectCompositionInference &&
                !IsPublicDeclaration(value) &&
                TryInferDirectCompositionFunctionType(initializer, out var compositionType))
            {
                return compositionType;
            }

            if (initializer?.Kind == SyntaxKind.CollectionExpression)
            {
                var elementType = InferCollectionElementType(initializer.Children.Where(child => !child.IsToken).ToArray());
                if (elementType.Length > 0)
                {
                    return $"{elementType}[]";
                }
            }

            return InferLiteralType(initializer);
        }

        private string GetValueDeclarationSourceType(
            SyntaxNode value,
            SyntaxNode? initializer,
            bool allowDirectCompositionInference = true)
        {
            if (TryGetDirectTypeAnnotation(value, out var annotation))
            {
                return GetSourceTypeName(annotation);
            }

            if (TryInferLambdaFunctionType(initializer, out var lambdaType))
            {
                return lambdaType;
            }

            if (allowDirectCompositionInference &&
                !IsPublicDeclaration(value) &&
                TryInferDirectCompositionFunctionType(initializer, out var compositionType))
            {
                return compositionType;
            }

            if (initializer?.Kind == SyntaxKind.CollectionExpression)
            {
                var elementType = InferCollectionElementType(initializer.Children.Where(child => !child.IsToken).ToArray());
                if (elementType.Length > 0)
                {
                    return $"{elementType}[]";
                }
            }

            return InferLiteralType(initializer);
        }

        private bool TryInferDirectCompositionFunctionType(SyntaxNode? initializer, out string type)
        {
            type = string.Empty;
            if (initializer is null ||
                !TryGetCompositionExpression(initializer, out var leftExpression, out var rightExpression, out var direction) ||
                !TryGetDirectIdentifierName(leftExpression, out var leftName) ||
                !TryGetDirectIdentifierName(rightExpression, out var rightName) ||
                !_functionSignatures.TryGetValue(leftName, out var leftFunction) ||
                !_functionSignatures.TryGetValue(rightName, out var rightFunction) ||
                !TryGetUnaryFunctionSignature(leftFunction, out var leftParameterType, out var leftReturnType) ||
                !TryGetUnaryFunctionSignature(rightFunction, out var rightParameterType, out var rightReturnType))
            {
                return false;
            }

            var firstFunction = leftFunction;
            var firstParameterType = leftParameterType;
            var firstReturnType = leftReturnType;
            var secondFunction = rightFunction;
            var secondParameterType = rightParameterType;
            var secondReturnType = rightReturnType;
            if (direction == CompositionDirection.Backward)
            {
                firstFunction = rightFunction;
                firstParameterType = rightParameterType;
                firstReturnType = rightReturnType;
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
                out firstReturnType,
                out secondParameterType,
                out secondReturnType);

            return TypesMatch(firstReturnType, secondParameterType) &&
                TryBuildUnaryDelegateType(firstParameterType, secondReturnType, out type);
        }

        private static bool TryGetUnaryFunctionSignature(
            CSharpFunctionSignature function,
            out string parameterType,
            out string returnType)
        {
            parameterType = string.Empty;
            returnType = function.ReturnType;
            if (function.ParameterTypes.Count != 1 ||
                string.IsNullOrWhiteSpace(function.ParameterTypes[0]) ||
                string.IsNullOrWhiteSpace(returnType))
            {
                return false;
            }

            parameterType = function.ParameterTypes[0];
            return true;
        }

        private static void ResolveCompositionGenericSignatureTypes(
            CSharpFunctionSignature firstFunction,
            string firstParameterType,
            string firstReturnType,
            CSharpFunctionSignature secondFunction,
            string secondParameterType,
            string secondReturnType,
            out string resolvedFirstParameterType,
            out string resolvedFirstReturnType,
            out string resolvedSecondParameterType,
            out string resolvedSecondReturnType)
        {
            var firstTypeParameterNames = new HashSet<string>(firstFunction.TypeParameters, StringComparer.Ordinal);
            var secondTypeParameterNames = new HashSet<string>(secondFunction.TypeParameters, StringComparer.Ordinal);
            var firstSubstitutions = new Dictionary<string, string>(StringComparer.Ordinal);
            var secondSubstitutions = new Dictionary<string, string>(StringComparer.Ordinal);

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
            string genericShape,
            string sourceType,
            IReadOnlySet<string> typeParameterNames,
            IReadOnlySet<string> oppositeTypeParameterNames,
            Dictionary<string, string> substitutions)
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

        private static bool TryInferDirectGenericArgument(
            string parameterType,
            string argumentType,
            IReadOnlySet<string> typeParameterNames,
            out string typeParameterName,
            out string inferredType)
        {
            typeParameterName = string.Empty;
            inferredType = string.Empty;
            if (string.IsNullOrWhiteSpace(parameterType) || string.IsNullOrWhiteSpace(argumentType))
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
                    parameterElementTypeName,
                    argumentElementTypeName,
                    typeParameterNames,
                    out typeParameterName,
                    out inferredType);
            }

            if (TryGetSingleGenericArgument(parameterType, out var parameterGenericName, out var parameterArgument) &&
                TryGetSingleGenericArgument(argumentType, out var argumentGenericName, out var argumentArgument) &&
                TypesMatch(argumentGenericName, parameterGenericName))
            {
                return TryInferDirectGenericArgument(
                    parameterArgument,
                    argumentArgument,
                    typeParameterNames,
                    out typeParameterName,
                    out inferredType);
            }

            return false;
        }

        private static bool GenericTypeReferencesAny(
            string type,
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
            string type,
            string candidate,
            IReadOnlySet<string> typeParameterNames)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            if (TryGetSimpleGenericParameterName(type, typeParameterNames, out var typeParameterName))
            {
                return string.Equals(typeParameterName, candidate, StringComparison.Ordinal);
            }

            if (TryGetArrayElementTypeName(type, out var elementTypeName))
            {
                return GenericTypeReferencesParameter(elementTypeName, candidate, typeParameterNames);
            }

            return TryGetSingleGenericArgument(type, out _, out var argument) &&
                GenericTypeReferencesParameter(argument, candidate, typeParameterNames);
        }

        private static string SubstituteGenericType(
            string type,
            IReadOnlyDictionary<string, string> substitutions,
            IReadOnlySet<string> typeParameterNames,
            bool unresolvedTypeParameterIsUnknown)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return string.Empty;
            }

            if (TryGetSimpleGenericParameterName(type, typeParameterNames, out var typeParameterName))
            {
                if (!substitutions.TryGetValue(typeParameterName, out var substitutedType))
                {
                    return unresolvedTypeParameterIsUnknown ? string.Empty : type;
                }

                return substitutedType;
            }

            if (TryGetArrayElementTypeName(type, out var elementTypeName))
            {
                var substitutedElementType = SubstituteGenericType(
                    elementTypeName,
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown);
                return substitutedElementType.Length == 0 ? string.Empty : $"{substitutedElementType}[]";
            }

            if (TryGetSingleGenericArgument(type, out var genericName, out var argument))
            {
                var substitutedArgument = SubstituteGenericType(
                    argument,
                    substitutions,
                    typeParameterNames,
                    unresolvedTypeParameterIsUnknown);
                return substitutedArgument.Length == 0 ? string.Empty : $"{genericName}<{substitutedArgument}>";
            }

            return type;
        }

        private static bool TryBuildUnaryDelegateType(string parameterType, string returnType, out string type)
        {
            type = string.Empty;
            if (string.IsNullOrWhiteSpace(parameterType) ||
                string.IsNullOrWhiteSpace(returnType) ||
                string.Equals(parameterType, "void", StringComparison.Ordinal))
            {
                return false;
            }

            type = string.Equals(returnType, "void", StringComparison.Ordinal)
                ? $"System.Action<{parameterType}>"
                : $"System.Func<{parameterType}, {returnType}>";
            return true;
        }

        private static bool TryGetSimpleGenericParameterName(
            string type,
            IReadOnlySet<string> typeParameterNames,
            out string typeParameterName)
        {
            typeParameterName = type.Trim();
            return typeParameterNames.Contains(typeParameterName);
        }

        private static bool TryGetArrayElementTypeName(string type, out string elementTypeName)
        {
            elementTypeName = string.Empty;
            var trimmed = type.Trim();
            if (!trimmed.EndsWith("[]", StringComparison.Ordinal))
            {
                return false;
            }

            elementTypeName = trimmed[..^2];
            return elementTypeName.Length > 0;
        }

        private static bool TypesMatch(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            return string.Equals(left, right, StringComparison.Ordinal) ||
                string.Equals(GetUnqualifiedTypeName(left), right, StringComparison.Ordinal) ||
                string.Equals(left, GetUnqualifiedTypeName(right), StringComparison.Ordinal) ||
                string.Equals(StripGenericArity(left), StripGenericArity(right), StringComparison.Ordinal) ||
                string.Equals(GetUnqualifiedTypeName(StripGenericArity(left)), GetUnqualifiedTypeName(StripGenericArity(right)), StringComparison.Ordinal);
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

        private static bool TryInferLambdaFunctionType(SyntaxNode? initializer, out string type)
        {
            type = string.Empty;
            if (initializer?.Kind != SyntaxKind.LambdaExpression)
            {
                return false;
            }

            type = $"System.Func<object, {InferLambdaReturnType(initializer)}>";
            return true;
        }

        private static string InferLambdaReturnType(SyntaxNode lambda)
        {
            var body = lambda.Children.LastOrDefault(child => !child.IsToken);
            return InferExpressionType(body);
        }

        private static string InferExpressionType(SyntaxNode? node)
        {
            if (node is null)
            {
                return "object";
            }

            if (node.Kind == SyntaxKind.LiteralExpression)
            {
                return InferLiteralType(node);
            }

            if (node.Kind == SyntaxKind.NameofExpression)
            {
                return "string";
            }

            if (node.Kind == SyntaxKind.CheckedExpression)
            {
                return InferExpressionType(node.Children.FirstOrDefault(child => !child.IsToken));
            }

            if (node.Kind == SyntaxKind.ParenthesizedExpression)
            {
                return InferExpressionType(node.Children.FirstOrDefault(child => !child.IsToken));
            }

            if (node.Kind == SyntaxKind.BlockExpression)
            {
                return InferExpressionType(GetBlockResultExpression(node));
            }

            if (node.Kind == SyntaxKind.CollectionExpression)
            {
                var elementType = InferCollectionLiteralElementType(node.Children.Where(child => !child.IsToken).ToArray());
                return elementType.Length > 0 ? $"{elementType}[]" : "object";
            }

            if (node.Kind == SyntaxKind.IfExpression &&
                TryInferIfExpressionType(node, out var ifType))
            {
                return ifType;
            }

            if (IsUnaryLogicalNotExpression(node))
            {
                return "bool";
            }

            if (TryInferUnaryNumericExpressionType(node, out var unaryNumericType))
            {
                return unaryNumericType;
            }

            if (TryInferBitwiseExpressionType(node, out var bitwiseType))
            {
                return bitwiseType;
            }

            if (node.Kind == SyntaxKind.BinaryExpression &&
                node.Children.Any(child => child.IsToken && child.Kind is SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken))
            {
                return "bool";
            }

            return "object";
        }

        private static bool TryInferIfExpressionType(SyntaxNode node, out string type)
        {
            var branchTypes = GetIfBranchResultExpressions(node)
                .Select(InferExpressionType)
                .Where(candidate => !string.Equals(candidate, "object", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            type = branchTypes.Length == 1 ? branchTypes[0] : string.Empty;
            return type.Length > 0;
        }

        private static IEnumerable<SyntaxNode> GetIfBranchResultExpressions(SyntaxNode node)
        {
            var thenBlock = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
            if (GetBlockResultExpression(thenBlock) is { } thenExpression)
            {
                yield return thenExpression;
            }

            foreach (var elseClause in node.Children.Where(child => child.Kind == SyntaxKind.ElseClause))
            {
                if (elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.IfExpression) is { } nestedIf)
                {
                    yield return nestedIf;
                    continue;
                }

                var elseBlock = elseClause.Children.FirstOrDefault(child => child.Kind == SyntaxKind.BlockExpression);
                if (GetBlockResultExpression(elseBlock) is { } elseExpression)
                {
                    yield return elseExpression;
                }
            }
        }

        private static SyntaxNode? GetBlockResultExpression(SyntaxNode? block)
        {
            var result = block?.Children.LastOrDefault(child => !child.IsToken);
            return result?.Kind == SyntaxKind.ExpressionStatement
                ? result.Children.FirstOrDefault(child => !child.IsToken)
                : result;
        }

        private static string InferCollectionLiteralElementType(IReadOnlyList<SyntaxNode> elements)
        {
            if (elements.Count == 0 || elements.Any(element => element.Kind == SyntaxKind.SpreadElement))
            {
                return string.Empty;
            }

            var elementTypes = elements
                .Select(InferExpressionType)
                .Where(type => !string.Equals(type, "object", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return elementTypes.Length == 1 ? elementTypes[0] : string.Empty;
        }

        private static bool IsUnaryLogicalNotExpression(SyntaxNode node) =>
            node.Kind == SyntaxKind.BinaryExpression &&
            node.Children.FirstOrDefault(child => child.IsToken)?.Kind == SyntaxKind.BangToken &&
            node.Children.Count(child => !child.IsToken) == 1;

        private static bool TryInferUnaryNumericExpressionType(SyntaxNode node, out string type)
        {
            type = string.Empty;
            var operatorKind = node.Children.FirstOrDefault(child => child.IsToken)?.Kind ?? SyntaxKind.UnknownToken;
            var expression = node.Children.FirstOrDefault(child => !child.IsToken);
            if (node.Kind != SyntaxKind.BinaryExpression ||
                expression is null ||
                node.Children.Count(child => !child.IsToken) != 1 ||
                operatorKind is not (SyntaxKind.PlusToken or SyntaxKind.MinusToken))
            {
                return false;
            }

            return TryGetUnaryNumericResultType(InferExpressionType(expression), operatorKind, out type);
        }

        private static bool TryGetUnaryNumericResultType(string operandType, SyntaxKind operatorKind, out string resultType)
        {
            resultType = operandType switch
            {
                "byte" or "sbyte" or "short" or "ushort" => "int",
                "int" or "long" or "float" or "double" or "decimal" => operandType,
                "uint" or "ulong" when operatorKind == SyntaxKind.PlusToken => operandType,
                _ => string.Empty
            };
            return resultType.Length > 0;
        }

        private static bool TryInferBitwiseExpressionType(SyntaxNode node, out string type)
        {
            type = string.Empty;
            var operatorKind = node.Children.FirstOrDefault(child => child.IsToken)?.Kind ?? SyntaxKind.UnknownToken;
            var expressions = node.Children.Where(child => !child.IsToken).ToArray();
            if (node.Kind != SyntaxKind.BinaryExpression)
            {
                return false;
            }

            if (operatorKind == SyntaxKind.TildeToken && expressions.Length == 1)
            {
                return TryGetUnaryIntegralBitwiseResultType(InferExpressionType(expressions[0]), out type);
            }

            if (operatorKind is SyntaxKind.PipeToken or SyntaxKind.AmpersandToken or SyntaxKind.CaretToken &&
                expressions.Length == 2)
            {
                var leftType = InferExpressionType(expressions[0]);
                var rightType = InferExpressionType(expressions[1]);
                return TryGetBinaryBooleanBitwiseResultType(leftType, rightType, out type) ||
                    TryGetBinaryIntegralBitwiseResultType(leftType, rightType, out type);
            }

            return false;
        }

        private static bool TryGetUnaryIntegralBitwiseResultType(string operandType, out string resultType)
        {
            resultType = operandType switch
            {
                "byte" or "sbyte" or "short" or "ushort" => "int",
                "int" or "uint" or "long" or "ulong" => operandType,
                _ => string.Empty
            };
            return resultType.Length > 0;
        }

        private static bool TryGetBinaryBooleanBitwiseResultType(string left, string right, out string resultType)
        {
            resultType = string.Equals(left, "bool", StringComparison.Ordinal) &&
                string.Equals(right, "bool", StringComparison.Ordinal)
                ? "bool"
                : string.Empty;
            return resultType.Length > 0;
        }

        private static bool TryGetBinaryIntegralBitwiseResultType(string left, string right, out string resultType)
        {
            resultType = string.Empty;
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

        private static bool IsIntegralPrimitiveType(string typeName) =>
            typeName is "byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong";

        private static bool CanImplicitlyPromoteToUnsignedLong(string type) =>
            type is "byte" or "ushort" or "uint" or "ulong";

        private static string InferLiteralType(SyntaxNode? initializer)
        {
            if (initializer?.Kind != SyntaxKind.LiteralExpression)
            {
                return "object";
            }

            var token = initializer.Children.FirstOrDefault(child => child.IsToken);
            return token?.Kind switch
            {
                SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringLiteralToken => "string",
                SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword => "bool",
                SyntaxKind.NumericLiteralToken => InferNumericLiteralType(token.Text ?? string.Empty),
                _ => "object"
            };
        }

        private static string InferNumericLiteralType(string text)
        {
            if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                return "decimal";
            }

            return text.Contains('.', StringComparison.Ordinal) ? "double" : "int";
        }

        private string InferCollectionElementType(IReadOnlyList<SyntaxNode> elements)
        {
            var elementTypes = elements
                .Select(InferCollectionElementType)
                .Where(type => type.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return elementTypes.Length == 1 ? elementTypes[0] : string.Empty;
        }

        private string InferCollectionElementType(SyntaxNode element)
        {
            if (element.Kind == SyntaxKind.SpreadElement)
            {
                var expression = element.Children.FirstOrDefault(child => !child.IsToken);
                return expression is not null && TryGetExpressionType(expression, out var collectionTypeName)
                    ? GetCollectionElementType(MapSourceTypeName(collectionTypeName))
                    : string.Empty;
            }

            if (element.Kind == SyntaxKind.LiteralExpression)
            {
                return InferLiteralType(element);
            }

            return TryGetExpressionType(element, out var typeName)
                ? MapSourceTypeName(typeName)
                : string.Empty;
        }

        private string FormatCollectionInitializer(IReadOnlyList<SyntaxNode> elements)
        {
            if (elements.Count == 0)
            {
                return "{ }";
            }

            return $"{{ {string.Join(", ", elements.Select(element => EmitExpression(element)))} }}";
        }

        private static string GetArrayElementType(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName) || !typeName.EndsWith("[]", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return typeName[..^2];
        }

        private static string GetCollectionElementType(string? typeName)
        {
            var arrayElementType = GetArrayElementType(typeName);
            if (arrayElementType.Length > 0)
            {
                return arrayElementType;
            }

            return TryGetListCollectionElementType(typeName, out var listElementType)
                ? listElementType
                : string.Empty;
        }

        private static bool TryGetListCollectionType(string? typeName, out string listType)
        {
            listType = string.Empty;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            var trimmed = typeName.Trim();
            if (!TryGetSingleGenericArgument(trimmed, out var genericName, out _))
            {
                return false;
            }

            if (!string.Equals(genericName, "List", StringComparison.Ordinal) &&
                !string.Equals(genericName, "System.Collections.Generic.List", StringComparison.Ordinal))
            {
                return false;
            }

            listType = trimmed;
            return true;
        }

        private static bool TryGetListCollectionElementType(string? typeName, out string elementType)
        {
            elementType = string.Empty;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            var trimmed = typeName.Trim();
            if (!TryGetSingleGenericArgument(trimmed, out var genericName, out var argument))
            {
                return false;
            }

            if (!string.Equals(genericName, "List", StringComparison.Ordinal) &&
                !string.Equals(genericName, "System.Collections.Generic.List", StringComparison.Ordinal))
            {
                return false;
            }

            elementType = argument;
            return elementType.Length > 0;
        }

        private static string MapSourceTypeName(string sourceType) =>
            sourceType switch
            {
                "bool" => "bool",
                "byte" => "byte",
                "char" => "char",
                "decimal" => "decimal",
                "double" => "double",
                "float" => "float",
                "int" => "int",
                "long" => "long",
                "object" => "object",
                "sbyte" => "sbyte",
                "short" => "short",
                "string" => "string",
                "uint" => "uint",
                "ulong" => "ulong",
                "ushort" => "ushort",
                _ => sourceType
            };

        private static string NormalizeNamespace(string? namespaceName) =>
            string.IsNullOrWhiteSpace(namespaceName) ? "TypeSharp.Generated" : namespaceName.Trim();

        private static string GetSourceTypeName(SyntaxNode? node)
        {
            if (node is null)
            {
                return "object";
            }

            if (node.Kind == SyntaxKind.TypeAnnotation)
            {
                return GetSourceTypeName(node.Children.FirstOrDefault(child => !child.IsToken));
            }

            if (node.Kind == SyntaxKind.NullableType)
            {
                return GetSourceTypeName(node.Children.FirstOrDefault(child => !child.IsToken));
            }

            if (node.Kind == SyntaxKind.ArrayType)
            {
                return $"{GetSourceTypeName(node.Children.FirstOrDefault(child => !child.IsToken))}[]";
            }

            if (node.Kind == SyntaxKind.FunctionType)
            {
                return GetSourceFunctionTypeName(node);
            }

            if (node.Kind == SyntaxKind.LiteralType)
            {
                return node.Children.FirstOrDefault(child => child.IsToken)?.Text ?? "object";
            }

            if (node.Kind == SyntaxKind.TypeName)
            {
                if (TryGetSourceGenericType(node, out var genericType))
                {
                    return genericType;
                }

                return GetQualifiedName(node);
            }

            return "object";
        }

        private static string GetSourceFunctionTypeName(SyntaxNode node)
        {
            var types = node.Children.Where(child => !child.IsToken).ToArray();
            if (types.Length < 2)
            {
                return "System.Func<object>";
            }

            var parameterType = MapSourceTypeName(GetSourceTypeName(types[0]));
            var returnType = MapSourceTypeName(GetSourceTypeName(types[1]));
            if (string.Equals(parameterType, "void", StringComparison.Ordinal))
            {
                return string.Equals(returnType, "void", StringComparison.Ordinal)
                    ? "System.Action"
                    : $"System.Func<{returnType}>";
            }

            return string.Equals(returnType, "void", StringComparison.Ordinal)
                ? $"System.Action<{parameterType}>"
                : $"System.Func<{parameterType}, {returnType}>";
        }

        private static bool TryGetSourceGenericType(SyntaxNode node, out string type)
        {
            type = string.Empty;
            var baseType = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeName);
            var argumentList = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeArgumentList);
            if (baseType is null || argumentList is null)
            {
                return false;
            }

            var baseName = GetSourceTypeName(baseType);
            var arguments = argumentList.Children
                .Where(child => !child.IsToken)
                .Select(GetSourceTypeName)
                .ToArray();
            if (baseName.Length == 0 || arguments.Length == 0)
            {
                return false;
            }

            type = $"{baseName}<{string.Join(",", arguments)}>";
            return true;
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

        private static bool CanEmitConstLiteral(string type, SyntaxNode? initializer)
        {
            if (initializer?.Kind != SyntaxKind.LiteralExpression)
            {
                return false;
            }

            var token = initializer.Children.FirstOrDefault(child => child.IsToken);
            if (token is null || token.Kind == SyntaxKind.InterpolatedStringLiteralToken)
            {
                return false;
            }

            if (token.Kind == SyntaxKind.NullKeyword)
            {
                return string.Equals(type, "string", StringComparison.Ordinal);
            }

            return type is "bool"
                or "byte"
                or "char"
                or "decimal"
                or "double"
                or "float"
                or "int"
                or "long"
                or "sbyte"
                or "short"
                or "string"
                or "uint"
                or "ulong"
                or "ushort";
        }

        private static bool IsMutableValueDeclaration(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.MutKeyword);

        private static bool CanLowerTopLevelValueDeclaration(SyntaxNode node) =>
            !IsFunctionValueDeclaration(node) || HasFunctionTypeAnnotation(node) || HasLambdaInitializer(node);

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

        private static string EmitDefaultValue(string type) =>
            string.IsNullOrWhiteSpace(type) ? "default(object)" : $"default({type})";

        private static SyntaxNode? GetInitializerExpression(SyntaxNode node)
        {
            return node.Children
                .FirstOrDefault(child => child.Kind == SyntaxKind.Initializer)?
                .Children
                .FirstOrDefault(child => !child.IsToken);
        }

        private static bool TryGetDirectTypeAnnotation(SyntaxNode node, out SyntaxNode annotation)
        {
            annotation = node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)!;
            return annotation is not null;
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

        private string GetVisibility(SyntaxNode node)
        {
            if (node.Children.Any(child => child.Kind == SyntaxKind.PrivateModifier))
            {
                return "private";
            }

            if (node.Children.Any(child => child.Kind is SyntaxKind.ExportModifier or SyntaxKind.PublicModifier))
            {
                return "public";
            }

            return TryGetLocalExportableDeclarationName(node, out var name) &&
                _localExportedNames.Contains(name)
                ? "public"
                : "internal";
        }

        private string GetNamespaceTypeVisibility(SyntaxNode node) =>
            string.Equals(GetVisibility(node), "public", StringComparison.Ordinal)
                ? "public"
                : "internal";

        private bool IsPublicDeclaration(SyntaxNode node) =>
            string.Equals(GetVisibility(node), "public", StringComparison.Ordinal);

        private static HashSet<string> CollectLocalExportedNames(SyntaxNode root)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var declaredTypes = root.Children
                .Where(child => child.Kind is SyntaxKind.TypeAliasDeclaration or SyntaxKind.RecordDeclaration or SyntaxKind.UnionDeclaration or SyntaxKind.EnumDeclaration or SyntaxKind.ClassDeclaration or SyntaxKind.InterfaceDeclaration)
                .Select(GetLocalTypeDeclarationName)
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
                    if (exportDeclaration.Kind == SyntaxKind.ExportTypeDeclaration)
                    {
                        foreach (var exportSpecifier in GetNamedImportSpecifiers(exportDeclaration))
                        {
                            if (exportSpecifier.IsAlias && declaredTypes.Contains(exportSpecifier.ImportedName))
                            {
                                names.Add(exportSpecifier.ImportedName);
                            }
                        }
                    }

                    continue;
                }

                foreach (var name in GetExportedIdentifiers(exportDeclaration))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private static IReadOnlyList<CSharpSourceLiteralExportAlias> CollectLocalLiteralExportAliases(SyntaxNode root)
        {
            var declaredLiterals = root.Children
                .Where(child => child.Kind == SyntaxKind.LiteralDeclaration && !IsAmbientDeclaration(child))
                .Select(literal => new
                {
                    Name = GetLiteralDeclarationName(literal),
                    Literal = literal
                })
                .Where(literal => literal.Name.Length > 0)
                .ToDictionary(literal => literal.Name, literal => literal.Literal, StringComparer.Ordinal);
            if (declaredLiterals.Count == 0)
            {
                return [];
            }

            var aliases = new List<CSharpSourceLiteralExportAlias>();
            foreach (var exportDeclaration in root.Children.Where(child =>
                child.Kind == SyntaxKind.ExportNamedDeclaration &&
                !HasFromSpecifier(child) &&
                HasExportAlias(child)))
            {
                foreach (var exportSpecifier in GetNamedImportSpecifiers(exportDeclaration).Where(specifier => specifier.IsAlias))
                {
                    if (declaredLiterals.TryGetValue(exportSpecifier.ImportedName, out var literal))
                    {
                        aliases.Add(new CSharpSourceLiteralExportAlias(exportSpecifier.LocalName, literal));
                    }
                }
            }

            return aliases;
        }

        private static IReadOnlyList<CSharpSourceValueExportAlias> CollectLocalValueExportAliases(SyntaxNode root)
        {
            var declaredValues = root.Children
                .Where(child => child.Kind == SyntaxKind.ValueDeclaration && !IsAmbientDeclaration(child) && CanLowerTopLevelValueDeclaration(child))
                .Select(value => new
                {
                    Name = GetLocalDeclarationName(value),
                    Value = value
                })
                .Where(value => value.Name.Length > 0)
                .ToDictionary(value => value.Name, value => value.Value, StringComparer.Ordinal);
            if (declaredValues.Count == 0)
            {
                return [];
            }

            var aliases = new List<CSharpSourceValueExportAlias>();
            foreach (var exportDeclaration in root.Children.Where(child =>
                child.Kind == SyntaxKind.ExportNamedDeclaration &&
                !HasFromSpecifier(child) &&
                HasExportAlias(child)))
            {
                foreach (var exportSpecifier in GetNamedImportSpecifiers(exportDeclaration).Where(specifier => specifier.IsAlias))
                {
                    if (declaredValues.TryGetValue(exportSpecifier.ImportedName, out var value))
                    {
                        aliases.Add(new CSharpSourceValueExportAlias(exportSpecifier.LocalName, value));
                    }
                }
            }

            return aliases;
        }

        private static string GetLocalTypeDeclarationName(SyntaxNode node) =>
            node.Kind switch
            {
                SyntaxKind.TypeAliasDeclaration => GetTypeAliasDeclarationName(node),
                SyntaxKind.RecordDeclaration => GetRecordDeclarationName(node),
                SyntaxKind.UnionDeclaration => GetUnionDeclarationName(node),
                SyntaxKind.EnumDeclaration => GetEnumDeclarationName(node),
                SyntaxKind.ClassDeclaration => GetClassDeclarationName(node),
                SyntaxKind.InterfaceDeclaration => GetInterfaceDeclarationName(node),
                _ => string.Empty
            };

        private static bool TryGetLocalExportableDeclarationName(SyntaxNode node, out string name)
        {
            name = node.Kind switch
            {
                SyntaxKind.FunctionDeclaration => GetDeclarationName(node),
                SyntaxKind.ValueDeclaration => GetLocalDeclarationName(node),
                SyntaxKind.LiteralDeclaration => GetLiteralDeclarationName(node),
                SyntaxKind.ModuleDeclaration => GetModuleDeclarationName(node),
                SyntaxKind.TypeAliasDeclaration => GetTypeAliasDeclarationName(node),
                SyntaxKind.RecordDeclaration => GetRecordDeclarationName(node),
                SyntaxKind.UnionDeclaration => GetUnionDeclarationName(node),
                SyntaxKind.EnumDeclaration => GetEnumDeclarationName(node),
                SyntaxKind.ClassDeclaration => GetClassDeclarationName(node),
                SyntaxKind.InterfaceDeclaration => GetInterfaceDeclarationName(node),
                _ => string.Empty
            };

            return name.Length > 0;
        }

        private static string GetPartialModifier(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.PartialModifier) ? " partial" : string.Empty;

        private static bool IsAsyncFunction(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.AsyncModifier);

        private static bool IsAmbientDeclaration(SyntaxNode node) =>
            node.Children.Any(child => child.Kind == SyntaxKind.AmbientModifier);

        private static string GetDeclarationName(SyntaxNode node)
        {
            var seenFunctionKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.FunKeyword)
                {
                    seenFunctionKeyword = true;
                    continue;
                }

                if (seenFunctionKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static string GetTypeAliasDeclarationName(SyntaxNode node)
        {
            var seenTypeKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.TypeKeyword)
                {
                    seenTypeKeyword = true;
                    continue;
                }

                if (seenTypeKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static string GetLiteralDeclarationName(SyntaxNode node)
        {
            var seenLiteralKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.LiteralKeyword)
                {
                    seenLiteralKeyword = true;
                    continue;
                }

                if (seenLiteralKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Literal";
                }
            }

            return "Literal";
        }

        private static string GetModuleDeclarationName(SyntaxNode node)
        {
            var seenModuleKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.ModuleKeyword)
                {
                    seenModuleKeyword = true;
                    continue;
                }

                if (seenModuleKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Module";
                }
            }

            return "Module";
        }

        private static string GetClassDeclarationName(SyntaxNode node)
        {
            var seenClassKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.ClassKeyword)
                {
                    seenClassKeyword = true;
                    continue;
                }

                if (seenClassKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static string GetRecordDeclarationName(SyntaxNode node)
        {
            var seenRecordKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.RecordKeyword)
                {
                    seenRecordKeyword = true;
                    continue;
                }

                if (seenRecordKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static string GetUnionDeclarationName(SyntaxNode node)
        {
            var seenUnionKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.UnionKeyword)
                {
                    seenUnionKeyword = true;
                    continue;
                }

                if (seenUnionKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static string GetEnumDeclarationName(SyntaxNode node)
        {
            var seenEnumKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.EnumKeyword)
                {
                    seenEnumKeyword = true;
                    continue;
                }

                if (seenEnumKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static IReadOnlyList<string> GetEnumMembers(SyntaxNode node) =>
            GetEnumMemberShapes(node)
                .Select(member => member.Name)
                .ToArray();

        private static IReadOnlyList<EnumMemberShape> GetEnumMemberShapes(SyntaxNode node) =>
            node.Children
                .Where(child => child.Kind == SyntaxKind.EnumMember)
                .Select(GetEnumMemberShape)
                .Where(member => member.Name.Length > 0)
                .ToArray();

        private static EnumMemberShape GetEnumMemberShape(SyntaxNode member)
        {
            var name = member.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)?.Text ?? string.Empty;
            var initializer = member.Children.FirstOrDefault(child => child.Kind == SyntaxKind.Initializer);
            var explicitValue = GetEnumMemberExplicitValue(initializer);
            return new EnumMemberShape(member, name, explicitValue);
        }

        private static string? GetEnumMemberExplicitValue(SyntaxNode? initializer)
        {
            if (initializer is null)
            {
                return null;
            }

            var parts = new List<string>();
            for (var index = 0; index < initializer.Children.Count; index++)
            {
                var child = initializer.Children[index];
                if (!child.IsToken)
                {
                    continue;
                }

                switch (child.Kind)
                {
                    case SyntaxKind.EqualsToken:
                        break;
                    case SyntaxKind.PipeToken:
                        parts.Add("|");
                        break;
                    case SyntaxKind.PlusToken:
                    case SyntaxKind.MinusToken:
                        if (index + 1 < initializer.Children.Count &&
                            initializer.Children[index + 1].IsToken &&
                            initializer.Children[index + 1].Kind == SyntaxKind.NumericLiteralToken &&
                            initializer.Children[index + 1].Text is { Length: > 0 } signedValue)
                        {
                            parts.Add($"{child.Text}{signedValue}");
                            index++;
                        }

                        break;
                    case SyntaxKind.NumericLiteralToken:
                    case SyntaxKind.IdentifierToken:
                        if (child.Text is { Length: > 0 } text)
                        {
                            parts.Add(text);
                        }

                        break;
                }
            }

            return parts.Count == 0 ? null : string.Join(" ", parts);
        }

        private static string GetInterfaceDeclarationName(SyntaxNode node)
        {
            var seenInterfaceKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.InterfaceKeyword)
                {
                    seenInterfaceKeyword = true;
                    continue;
                }

                if (seenInterfaceKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "Generated";
                }
            }

            return "Generated";
        }

        private static SyntaxNode? GetExtensionReceiverTypeNode(SyntaxNode node) =>
            node.Children.FirstOrDefault(child => IsTypeSyntax(child.Kind));

        private static string GetExtensionClassName(SyntaxNode? receiverTypeNode, int index)
        {
            var sourceName = GetSourceTypeName(receiverTypeNode);
            var normalized = new string(sourceName.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = "Generated";
            }

            var prefix = char.ToUpperInvariant(normalized[0]) + normalized[1..];
            var suffix = index == 0 ? string.Empty : index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"{prefix}{suffix}Extensions";
        }

        private static string GetLocalDeclarationName(SyntaxNode node)
        {
            var seenLetKeyword = false;
            foreach (var child in node.Children)
            {
                if (child.IsToken && child.Kind == SyntaxKind.LetKeyword)
                {
                    seenLetKeyword = true;
                    continue;
                }

                if (seenLetKeyword && child.IsToken && child.Kind == SyntaxKind.IdentifierToken)
                {
                    return child.Text ?? "local";
                }
            }

            return "local";
        }

        private bool TryGetCallTarget(SyntaxNode callee, out string callTarget)
        {
            callTarget = string.Empty;
            if (callee.Kind != SyntaxKind.IdentifierExpression)
            {
                return false;
            }

            var identifier = GetIdentifierText(callee);
            if (!_unionCaseFactories.TryGetValue(identifier, out var target))
            {
                return false;
            }

            callTarget = target;
            return true;
        }

        private bool TryGetConstructorName(SyntaxNode callee, out string constructorName)
        {
            constructorName = string.Empty;
            if (callee.Kind == SyntaxKind.GenericNameExpression)
            {
                var target = callee.Children.FirstOrDefault(child => !child.IsToken && child.Kind != SyntaxKind.TypeArgumentList);
                if (target?.Kind != SyntaxKind.IdentifierExpression)
                {
                    return false;
                }

                var genericConstructorName = GetIdentifierText(target);
                if (!_constructorCandidateNames.Contains(genericConstructorName))
                {
                    return false;
                }

                constructorName = EmitGenericName(callee);
                return constructorName.Length > 0;
            }

            if (callee.Kind != SyntaxKind.IdentifierExpression)
            {
                return false;
            }

            constructorName = GetIdentifierText(callee);
            return _constructorCandidateNames.Contains(constructorName);
        }

        private static string GetIdentifierText(SyntaxNode node)
        {
            var identifier = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
            return identifier?.Text ?? string.Empty;
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

            var parts = identifiers.Select(identifier => identifier.Text ?? string.Empty).Where(text => text.Length > 0);
            return string.Join(".", parts);
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

        private static bool IsRelativeSpecifier(string specifier) =>
            specifier == "." ||
            specifier == ".." ||
            specifier.StartsWith("./", StringComparison.Ordinal) ||
            specifier.StartsWith("../", StringComparison.Ordinal);

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

        private static IEnumerable<string> GetExportedIdentifiers(SyntaxNode node)
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

        private static bool ContainsNode(SyntaxNode node, SyntaxKind kind)
        {
            if (node.Kind == kind)
            {
                return true;
            }

            return node.Children.Any(child => !child.IsToken && ContainsNode(child, kind));
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
                        node.Children[index + 2].Kind == SyntaxKind.IdentifierToken &&
                        node.Children[index + 2].Text is { Length: > 0 } alias)
                    {
                        yield return new NamedImportSpecifier(text, alias);
                        index += 2;
                    }
                    else
                    {
                        yield return new NamedImportSpecifier(text, text);
                    }
                }
            }
        }

        private static bool TryGetNamespaceImportAlias(SyntaxNode node, out string alias)
        {
            for (var index = 0; index + 1 < node.Children.Count; index++)
            {
                if (node.Children[index].IsToken &&
                    node.Children[index].Kind == SyntaxKind.AsKeyword &&
                    node.Children[index + 1].IsToken &&
                    node.Children[index + 1].Kind == SyntaxKind.IdentifierToken &&
                    node.Children[index + 1].Text is { Length: > 0 } text)
                {
                    alias = text;
                    return true;
                }
            }

            alias = string.Empty;
            return false;
        }

        private sealed record CSharpImports(
            IReadOnlyList<string> Usings,
            IReadOnlyList<CSharpImportAlias> Aliases,
            IReadOnlyList<string> StaticUsings,
            IReadOnlyList<string> ImportedNames);

        private readonly record struct CSharpImportAlias(string LocalName, string QualifiedName);

        private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName)
        {
            public bool IsAlias => !string.Equals(ImportedName, LocalName, StringComparison.Ordinal);
        }

        private readonly record struct CSharpParameter(string Type, string Name, string SourceType, bool IsParams, string? DefaultValue = null);

        private readonly record struct CSharpFunctionSignature(
            IReadOnlyList<string> ParameterTypes,
            string ReturnType,
            IReadOnlyList<string> TypeParameters);

        private readonly record struct RecordShape(string Name, IReadOnlyList<CSharpParameter> Parameters);

        private readonly record struct TypeLevelUnionShape(string Name, IReadOnlyList<TypeLevelUnionMemberShape> Members);

        private readonly record struct TypeLevelUnionMemberShape(string SourceType, string CSharpType);

        private readonly record struct EnumMemberShape(SyntaxNode Node, string Name, string? ExplicitValue);

        private readonly record struct EnumShape(string Name, IReadOnlyList<string> Members);

        private readonly record struct UnionShape(string Name, IReadOnlyList<UnionCaseShape> Cases);

        private readonly record struct UnionCaseShape(
            string Name,
            string ClassName,
            int Tag,
            IReadOnlyList<CSharpParameter> Parameters);

        private enum CompositionDirection
        {
            Forward,
            Backward
        }
    }
}
