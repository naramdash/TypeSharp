using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Projects;

public sealed record SourceModuleGraph(
    IReadOnlyList<SourceModule> Modules,
    IReadOnlyList<SourceModuleDependency> Dependencies,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public static SourceModuleGraph Build(IReadOnlyList<SourceModule> modules)
    {
        var diagnostics = new List<Diagnostic>();
        var dependencies = new List<SourceModuleDependency>();
        var moduleByPath = new Dictionary<string, SourceModule>(StringComparer.OrdinalIgnoreCase);
        var localExportsByPath = new Dictionary<string, SourceModuleExports>(StringComparer.OrdinalIgnoreCase);
        var exportsByPath = new Dictionary<string, SourceModuleExports>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            moduleByPath.TryAdd(module.SourceFile.ModulePath, module);
            localExportsByPath.TryAdd(module.SourceFile.ModulePath, CollectLocalExports(module.Root));
        }

        foreach (var module in modules)
        {
            CollectExports(module, moduleByPath, localExportsByPath, exportsByPath, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        foreach (var module in modules)
        {
            CollectDependencies(module, module.Root, moduleByPath, exportsByPath, dependencies, diagnostics);
            ValidateNamespaceImportMemberAccesses(module, moduleByPath, exportsByPath, diagnostics);
        }

        return new SourceModuleGraph(modules, dependencies, diagnostics);
    }

    private static void CollectDependencies(
        SourceModule module,
        SyntaxNode node,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath,
        List<SourceModuleDependency> dependencies,
        List<Diagnostic> diagnostics)
    {
        if (TryGetDependencyKind(node, out var kind) &&
            TryGetModuleSpecifier(node, out var specifierToken, out var specifier) &&
            IsRelativeSpecifier(specifier))
        {
            var resolvedModulePath = ResolveRelativeModulePath(module.SourceFile.ModulePath, specifier);
            if (!moduleByPath.ContainsKey(resolvedModulePath))
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.UnresolvedSourceModule.Code,
                    DiagnosticDescriptors.UnresolvedSourceModule.DefaultSeverity,
                    $"Source module specifier '{specifier}' could not be resolved from module '{module.SourceFile.ModulePath}'.",
                    module.SourceFile.RelativePath,
                    specifierToken.Span));
            }
            else
            {
                dependencies.Add(new SourceModuleDependency(
                    kind,
                    module.SourceFile.ModulePath,
                    resolvedModulePath,
                    specifier,
                    module.SourceFile.RelativePath,
                    specifierToken.Span));

                if (kind == SourceModuleDependencyKind.Import &&
                    exportsByPath.TryGetValue(resolvedModulePath, out var exports))
                {
                    ValidateSourceImportExports(
                        node,
                        exports,
                        specifier,
                        resolvedModulePath,
                        module.SourceFile.RelativePath,
                        diagnostics);
                }
                else if (kind == SourceModuleDependencyKind.Export &&
                    IsSupportedSourceReExport(node) &&
                    exportsByPath.TryGetValue(resolvedModulePath, out var reExportTargetExports))
                {
                    ValidateSourceReExportExports(
                        node,
                        reExportTargetExports,
                        specifier,
                        resolvedModulePath,
                        module.SourceFile.RelativePath,
                        diagnostics);
                }
            }
        }

        foreach (var child in node.Children)
        {
            CollectDependencies(module, child, moduleByPath, exportsByPath, dependencies, diagnostics);
        }
    }

    private static void ValidateSourceImportExports(
        SyntaxNode node,
        SourceModuleExports exports,
        string specifier,
        string resolvedModulePath,
        string sourceFile,
        List<Diagnostic> diagnostics)
    {
        if (node.Kind == SyntaxKind.ImportNamespaceDeclaration)
        {
            return;
        }

        if (node.Kind != SyntaxKind.ImportNamedDeclaration &&
            node.Kind != SyntaxKind.ImportTypeDeclaration)
        {
            return;
        }

        foreach (var importedName in GetNamedImportSpecifiers(node))
        {
            if (node.Kind == SyntaxKind.ImportTypeDeclaration)
            {
                if (exports.TypeNames.Contains(importedName.ImportedName))
                {
                    continue;
                }

                diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.MissingSourceModuleExport.Code,
                    DiagnosticDescriptors.MissingSourceModuleExport.DefaultSeverity,
                    $"Source module import '{specifier}' resolves to '{resolvedModulePath}', but exported type '{importedName.ImportedName}' was not found.",
                    sourceFile,
                    importedName.Span));
                continue;
            }

            if (importedName.IsAlias)
            {
                if (exports.FunctionNames.Contains(importedName.ImportedName) ||
                    exports.ImportAliasValueNames.Contains(importedName.ImportedName) ||
                    exports.ModuleNames.Contains(importedName.ImportedName) ||
                    (exports.TypeNames.Contains(importedName.ImportedName) &&
                        !exports.ValueNames.Contains(importedName.ImportedName)))
                {
                    continue;
                }

                if (exports.ValueNames.Contains(importedName.ImportedName) ||
                    exports.TypeNames.Contains(importedName.ImportedName))
                {
                    ReportUnsupportedSourceImportAlias(
                        specifier,
                        resolvedModulePath,
                        importedName,
                        sourceFile,
                        diagnostics);
                    continue;
                }
            }
            else if (exports.ValueNames.Contains(importedName.ImportedName) ||
                exports.TypeNames.Contains(importedName.ImportedName))
            {
                continue;
            }

            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingSourceModuleExport.Code,
                DiagnosticDescriptors.MissingSourceModuleExport.DefaultSeverity,
                $"Source module import '{specifier}' resolves to '{resolvedModulePath}', but exported name '{importedName.ImportedName}' was not found.",
                sourceFile,
                importedName.Span));
        }
    }

    private static void ReportUnsupportedSourceImportAlias(
        string specifier,
        string resolvedModulePath,
        NamedImportSpecifier importedName,
        string sourceFile,
        List<Diagnostic> diagnostics)
    {
        diagnostics.Add(new Diagnostic(
            DiagnosticDescriptors.UnsupportedSourceModuleImport.Code,
            DiagnosticDescriptors.UnsupportedSourceModuleImport.DefaultSeverity,
            $"Source module import '{specifier}' resolves to '{resolvedModulePath}', but alias '{importedName.ImportedName} as {importedName.LocalName}' does not target an exported function, top-level value, type, or module that the current backend can lower.",
            sourceFile,
            importedName.Span));
    }

    private static void ValidateSourceReExportExports(
        SyntaxNode node,
        SourceModuleExports exports,
        string specifier,
        string resolvedModulePath,
        string sourceFile,
        List<Diagnostic> diagnostics)
    {
        if (node.Kind == SyntaxKind.ExportStarDeclaration)
        {
            return;
        }

        foreach (var exportSpecifier in GetNamedExportSpecifiers(node))
        {
            if (node.Kind == SyntaxKind.ExportTypeDeclaration)
            {
                if (exports.TypeNames.Contains(exportSpecifier.TargetName))
                {
                    continue;
                }

                diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.MissingSourceModuleExport.Code,
                    DiagnosticDescriptors.MissingSourceModuleExport.DefaultSeverity,
                    $"Source module re-export '{specifier}' resolves to '{resolvedModulePath}', but exported type '{exportSpecifier.TargetName}' was not found.",
                    sourceFile,
                    exportSpecifier.Span));
                continue;
            }

            if (exports.FunctionNames.Contains(exportSpecifier.TargetName) ||
                exports.ImportAliasValueNames.Contains(exportSpecifier.TargetName) ||
                exports.ModuleNames.Contains(exportSpecifier.TargetName))
            {
                continue;
            }

            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingSourceModuleExport.Code,
                DiagnosticDescriptors.MissingSourceModuleExport.DefaultSeverity,
                $"Source module re-export '{specifier}' resolves to '{resolvedModulePath}', but exported function, top-level value, or module '{exportSpecifier.TargetName}' was not found.",
                sourceFile,
                exportSpecifier.Span));
        }
    }

    private static bool TryGetDependencyKind(SyntaxNode node, out SourceModuleDependencyKind kind)
    {
        switch (node.Kind)
        {
            case SyntaxKind.ImportNamedDeclaration:
            case SyntaxKind.ImportTypeDeclaration:
            case SyntaxKind.ImportNamespaceDeclaration:
                kind = SourceModuleDependencyKind.Import;
                return true;

            case SyntaxKind.ExportNamedDeclaration:
            case SyntaxKind.ExportTypeDeclaration:
            case SyntaxKind.ExportStarDeclaration:
                kind = SourceModuleDependencyKind.Export;
                return true;

            default:
                kind = default;
                return false;
        }
    }

    private static bool IsSupportedSourceReExport(SyntaxNode node) =>
        node.Kind is SyntaxKind.ExportNamedDeclaration or SyntaxKind.ExportTypeDeclaration or SyntaxKind.ExportStarDeclaration &&
        TryGetModuleSpecifier(node, out _, out var specifier) &&
        IsRelativeSpecifier(specifier);

    private static void ValidateNamespaceImportMemberAccesses(
        SourceModule module,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath,
        List<Diagnostic> diagnostics)
    {
        var namespaceImports = CollectRelativeNamespaceImports(module, moduleByPath, exportsByPath);
        if (namespaceImports.Count == 0)
        {
            return;
        }

        ValidateNamespaceImportMemberAccesses(module.Root, namespaceImports, module.SourceFile.RelativePath, diagnostics);
    }

    private static IReadOnlyDictionary<string, SourceModuleNamespaceImport> CollectRelativeNamespaceImports(
        SourceModule module,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath)
    {
        var imports = new Dictionary<string, SourceModuleNamespaceImport>(StringComparer.Ordinal);
        foreach (var child in module.Root.Children.Where(child => child.Kind == SyntaxKind.ImportNamespaceDeclaration))
        {
            if (!TryGetModuleSpecifier(child, out _, out var specifier) ||
                !IsRelativeSpecifier(specifier) ||
                !TryGetNamespaceImportAlias(child, out var alias))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(module.SourceFile.ModulePath, specifier);
            if (!moduleByPath.ContainsKey(resolvedModulePath) ||
                !exportsByPath.TryGetValue(resolvedModulePath, out var exports))
            {
                continue;
            }

            imports[alias] = new SourceModuleNamespaceImport(alias, specifier, resolvedModulePath, exports);
        }

        return imports;
    }

    private static void ValidateNamespaceImportMemberAccesses(
        SyntaxNode node,
        IReadOnlyDictionary<string, SourceModuleNamespaceImport> namespaceImports,
        string sourceFile,
        List<Diagnostic> diagnostics)
    {
        if (TryGetNamespaceImportMemberAccess(node, out var alias, out var memberToken) &&
            namespaceImports.TryGetValue(alias, out var sourceImport) &&
            memberToken.Text is { Length: > 0 } memberName &&
            !sourceImport.Exports.ValueNames.Contains(memberName) &&
            !sourceImport.Exports.TypeNames.Contains(memberName))
        {
            diagnostics.Add(new Diagnostic(
                DiagnosticDescriptors.MissingSourceModuleExport.Code,
                DiagnosticDescriptors.MissingSourceModuleExport.DefaultSeverity,
                $"Source module namespace import '{sourceImport.Specifier}' resolves to '{sourceImport.ResolvedModulePath}', but exported member '{memberName}' was not found.",
                sourceFile,
                memberToken.Span));
        }

        foreach (var child in node.Children.Where(child => !child.IsToken))
        {
            ValidateNamespaceImportMemberAccesses(child, namespaceImports, sourceFile, diagnostics);
        }
    }

    private static bool TryGetNamespaceImportMemberAccess(SyntaxNode node, out string alias, out SyntaxNode memberToken)
    {
        alias = string.Empty;
        if (node.Kind != SyntaxKind.MemberAccessExpression)
        {
            memberToken = default!;
            return false;
        }

        var receiver = node.Children.FirstOrDefault(child => !child.IsToken);
        if (receiver?.Kind != SyntaxKind.IdentifierExpression ||
            !TryGetIdentifierText(receiver, out alias))
        {
            memberToken = default!;
            return false;
        }

        memberToken = node.Children.LastOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken)!;
        return memberToken is not null;
    }

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

    private static string ResolveRelativeModulePath(string fromModulePath, string specifier)
    {
        var parts = new List<string>();
        var lastSlash = fromModulePath.LastIndexOf('/');
        if (lastSlash >= 0)
        {
            parts.AddRange(fromModulePath[..lastSlash].Split('/', StringSplitOptions.RemoveEmptyEntries));
        }

        foreach (var part in specifier.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                continue;
            }

            parts.Add(part);
        }

        var resolved = string.Join("/", parts);
        return resolved.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? resolved[..^".tysh".Length]
            : resolved;
    }

    private static string Unquote(string text)
    {
        if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
        {
            return text[1..^1];
        }

        return text;
    }

    private static bool TryGetNamespaceImportAlias(SyntaxNode node, out string alias)
    {
        alias = string.Empty;
        for (var index = 0; index < node.Children.Count - 1; index++)
        {
            if (node.Children[index].Kind == SyntaxKind.AsKeyword &&
                node.Children[index + 1].Kind == SyntaxKind.IdentifierToken &&
                node.Children[index + 1].Text is { Length: > 0 } text)
            {
                alias = text;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetIdentifierText(SyntaxNode node, out string text)
    {
        var identifier = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
        text = identifier?.Text ?? string.Empty;
        return text.Length > 0;
    }

    private static SourceModuleExports CollectExports(
        SourceModule module,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> localExportsByPath,
        Dictionary<string, SourceModuleExports> exportsByPath,
        HashSet<string> visiting)
    {
        if (exportsByPath.TryGetValue(module.SourceFile.ModulePath, out var cached))
        {
            return cached;
        }

        if (!localExportsByPath.TryGetValue(module.SourceFile.ModulePath, out var localExports))
        {
            localExports = new SourceModuleExports(
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal));
        }

        var exportedValues = new HashSet<string>(localExports.ValueNames, StringComparer.Ordinal);
        var exportedTypes = new HashSet<string>(localExports.TypeNames, StringComparer.Ordinal);
        var importAliasValues = new HashSet<string>(localExports.ImportAliasValueNames, StringComparer.Ordinal);
        var exportedFunctions = new HashSet<string>(localExports.FunctionNames, StringComparer.Ordinal);
        var exportedModules = new HashSet<string>(localExports.ModuleNames, StringComparer.Ordinal);
        if (!visiting.Add(module.SourceFile.ModulePath))
        {
            return new SourceModuleExports(exportedValues, exportedTypes, importAliasValues, exportedFunctions, exportedModules);
        }

        foreach (var exportDeclaration in module.Root.Children.Where(child => IsSupportedSourceReExport(child)))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out _, out var specifier) ||
                !IsRelativeSpecifier(specifier))
            {
                continue;
            }

            var resolvedModulePath = ResolveRelativeModulePath(module.SourceFile.ModulePath, specifier);
            if (!moduleByPath.TryGetValue(resolvedModulePath, out var targetModule))
            {
                continue;
            }

            var targetExports = CollectExports(targetModule, moduleByPath, localExportsByPath, exportsByPath, visiting);
            if (exportDeclaration.Kind == SyntaxKind.ExportStarDeclaration)
            {
                foreach (var exportedValue in targetExports.ValueNames)
                {
                    exportedValues.Add(exportedValue);
                }

                foreach (var exportedType in targetExports.TypeNames)
                {
                    exportedTypes.Add(exportedType);
                }

                foreach (var importAliasValue in targetExports.ImportAliasValueNames)
                {
                    importAliasValues.Add(importAliasValue);
                }

                foreach (var exportedFunction in targetExports.FunctionNames)
                {
                    exportedFunctions.Add(exportedFunction);
                }

                continue;
            }

            foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
            {
                if (exportDeclaration.Kind == SyntaxKind.ExportTypeDeclaration)
                {
                    if (targetExports.TypeNames.Contains(exportSpecifier.TargetName))
                    {
                        exportedTypes.Add(exportSpecifier.ExportedName);
                    }

                    continue;
                }

                if (targetExports.FunctionNames.Contains(exportSpecifier.TargetName))
                {
                    exportedValues.Add(exportSpecifier.ExportedName);
                    exportedFunctions.Add(exportSpecifier.ExportedName);
                }
                else if (targetExports.ImportAliasValueNames.Contains(exportSpecifier.TargetName))
                {
                    exportedValues.Add(exportSpecifier.ExportedName);
                    importAliasValues.Add(exportSpecifier.ExportedName);
                }
                else if (targetExports.ModuleNames.Contains(exportSpecifier.TargetName))
                {
                    exportedModules.Add(exportSpecifier.ExportedName);
                }
            }
        }

        visiting.Remove(module.SourceFile.ModulePath);
        var exports = new SourceModuleExports(exportedValues, exportedTypes, importAliasValues, exportedFunctions, exportedModules);
        exportsByPath[module.SourceFile.ModulePath] = exports;
        return exports;
    }

    private static SourceModuleExports CollectLocalExports(SyntaxNode root)
    {
        var declaredValues = new HashSet<string>(StringComparer.Ordinal);
        var declaredTypes = new HashSet<string>(StringComparer.Ordinal);
        var declaredFunctions = new HashSet<string>(StringComparer.Ordinal);
        var declaredLiterals = new HashSet<string>(StringComparer.Ordinal);
        var declaredImportAliasValues = new HashSet<string>(StringComparer.Ordinal);
        var declaredModules = new HashSet<string>(StringComparer.Ordinal);
        var exportedValues = new HashSet<string>(StringComparer.Ordinal);
        var exportedTypes = new HashSet<string>(StringComparer.Ordinal);
        var exportedImportAliasValues = new HashSet<string>(StringComparer.Ordinal);
        var exportedFunctions = new HashSet<string>(StringComparer.Ordinal);
        var exportedModules = new HashSet<string>(StringComparer.Ordinal);

        foreach (var child in root.Children)
        {
            if (!TryGetExportableDeclaration(child, out var name, out var declaresValue, out var declaresType))
            {
                continue;
            }

            if (declaresValue)
            {
                declaredValues.Add(name);
            }

            if (NodeDeclaresFunction(child))
            {
                declaredFunctions.Add(name);
            }

            if (NodeDeclaresModule(child))
            {
                declaredModules.Add(name);
            }

            if (NodeDeclaresLiteral(child))
            {
                declaredLiterals.Add(name);
            }

            if (NodeDeclaresImportAliasValue(child))
            {
                declaredImportAliasValues.Add(name);
            }

            if (declaresType)
            {
                declaredTypes.Add(name);
            }

            if (!HasModifier(child, SyntaxKind.ExportModifier))
            {
                continue;
            }

            if (declaresValue)
            {
                exportedValues.Add(name);
            }

            if (NodeDeclaresFunction(child))
            {
                exportedFunctions.Add(name);
            }

            if (NodeDeclaresModule(child))
            {
                exportedModules.Add(name);
            }

            if (NodeDeclaresImportAliasValue(child))
            {
                exportedImportAliasValues.Add(name);
            }

            if (declaresType)
            {
                exportedTypes.Add(name);
            }
        }

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
                            exportedValues.Add(exportSpecifier.ExportedName);
                            exportedFunctions.Add(exportSpecifier.ExportedName);
                        }
                        else if (exportSpecifier.IsAlias && declaredLiterals.Contains(exportSpecifier.TargetName))
                        {
                            exportedValues.Add(exportSpecifier.ExportedName);
                            exportedImportAliasValues.Add(exportSpecifier.ExportedName);
                        }
                        else if (exportSpecifier.IsAlias && declaredValues.Contains(exportSpecifier.TargetName))
                        {
                            exportedValues.Add(exportSpecifier.ExportedName);
                            if (declaredImportAliasValues.Contains(exportSpecifier.TargetName))
                            {
                                exportedImportAliasValues.Add(exportSpecifier.ExportedName);
                            }
                        }
                    }
                }
                else if (exportDeclaration.Kind == SyntaxKind.ExportTypeDeclaration)
                {
                    foreach (var exportSpecifier in GetNamedExportSpecifiers(exportDeclaration))
                    {
                        if (exportSpecifier.IsAlias && declaredTypes.Contains(exportSpecifier.TargetName))
                        {
                            exportedTypes.Add(exportSpecifier.ExportedName);
                        }
                    }
                }

                continue;
            }

            foreach (var exportName in GetExportedIdentifiers(exportDeclaration))
            {
                if (exportDeclaration.Kind == SyntaxKind.ExportTypeDeclaration)
                {
                    if (declaredTypes.Contains(exportName))
                    {
                        exportedTypes.Add(exportName);
                    }

                    continue;
                }

                if (declaredValues.Contains(exportName))
                {
                    exportedValues.Add(exportName);
                    if (declaredImportAliasValues.Contains(exportName))
                    {
                        exportedImportAliasValues.Add(exportName);
                    }
                }

                if (declaredFunctions.Contains(exportName))
                {
                    exportedFunctions.Add(exportName);
                }

                if (declaredModules.Contains(exportName))
                {
                    exportedModules.Add(exportName);
                }

                if (declaredTypes.Contains(exportName))
                {
                    exportedTypes.Add(exportName);
                }
            }
        }

        return new SourceModuleExports(exportedValues, exportedTypes, exportedImportAliasValues, exportedFunctions, exportedModules);
    }

    private static bool NodeDeclaresFunction(SyntaxNode node) =>
        node.Kind == SyntaxKind.FunctionDeclaration;

    private static bool NodeDeclaresModule(SyntaxNode node) =>
        node.Kind == SyntaxKind.ModuleDeclaration;

    private static bool NodeDeclaresLiteral(SyntaxNode node) =>
        node.Kind == SyntaxKind.LiteralDeclaration;

    private static bool NodeDeclaresImportAliasValue(SyntaxNode node) =>
        node.Kind == SyntaxKind.LiteralDeclaration || NodeDeclaresLowerableValue(node);

    private static bool NodeDeclaresLowerableValue(SyntaxNode node) =>
        node.Kind == SyntaxKind.ValueDeclaration && (!IsFunctionValueDeclaration(node) || HasFunctionTypeAnnotation(node));

    private static bool TryGetExportableDeclaration(
        SyntaxNode node,
        out string name,
        out bool declaresValue,
        out bool declaresType)
    {
        declaresValue =
            (node.Kind is SyntaxKind.FunctionDeclaration or SyntaxKind.LiteralDeclaration) ||
            NodeDeclaresLowerableValue(node);
        declaresType = node.Kind is
            SyntaxKind.TypeAliasDeclaration or
            SyntaxKind.RecordDeclaration or
            SyntaxKind.UnionDeclaration or
            SyntaxKind.ClassDeclaration or
            SyntaxKind.InterfaceDeclaration or
            SyntaxKind.DelegateDeclaration;

        if (node.Kind == SyntaxKind.ModuleDeclaration)
        {
            declaresValue = true;
            declaresType = true;
        }

        if (!declaresValue && !declaresType)
        {
            name = string.Empty;
            return false;
        }

        var identifier = node.Children.FirstOrDefault(child => child.IsToken && child.Kind == SyntaxKind.IdentifierToken);
        name = identifier?.Text ?? string.Empty;
        return name.Length > 0;
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

    private static bool HasFunctionTypeAnnotation(SyntaxNode node) =>
        node.Children
            .FirstOrDefault(child => child.Kind == SyntaxKind.TypeAnnotation)?
            .Children
            .Any(child => child.Kind == SyntaxKind.FunctionType) == true;

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

            if (insideBraces && child.IsToken && child.Kind == SyntaxKind.IdentifierToken && child.Text is { Length: > 0 } text)
            {
                yield return text;
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
                    yield return new NamedExportSpecifier(name, alias, child.Span);
                    index += 2;
                }
                else
                {
                    yield return new NamedExportSpecifier(name, name, child.Span);
                }
            }
        }
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
                    yield return new NamedImportSpecifier(text, alias, child.Span);
                    index += 2;
                }
                else
                {
                    yield return new NamedImportSpecifier(text, text, child.Span);
                }
            }
        }
    }

    private sealed record SourceModuleExports(
        IReadOnlySet<string> ValueNames,
        IReadOnlySet<string> TypeNames,
        IReadOnlySet<string> ImportAliasValueNames,
        IReadOnlySet<string> FunctionNames,
        IReadOnlySet<string> ModuleNames);

    private sealed record SourceModuleNamespaceImport(
        string Alias,
        string Specifier,
        string ResolvedModulePath,
        SourceModuleExports Exports);

    private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName, SourceSpan Span)
    {
        public bool IsAlias => !string.Equals(ImportedName, LocalName, StringComparison.Ordinal);
    }

    private readonly record struct NamedExportSpecifier(string TargetName, string ExportedName, SourceSpan Span)
    {
        public bool IsAlias => !string.Equals(TargetName, ExportedName, StringComparison.Ordinal);
    }
}
