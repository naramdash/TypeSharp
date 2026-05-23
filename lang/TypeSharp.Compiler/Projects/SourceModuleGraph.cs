using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;

namespace TypeSharp.Compiler.Projects;

public sealed record SourceModuleGraph(
    IReadOnlyList<SourceModule> Modules,
    IReadOnlyList<SourceModuleDependency> Dependencies,
    IReadOnlyDictionary<string, SourceModuleExports> ExportsByModulePath,
    IReadOnlyList<Diagnostic> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public static SourceModuleGraph Build(
        IReadOnlyList<SourceModule> modules,
        IReadOnlyList<SourceAliasOption>? sourceAliases = null,
        IReadOnlyList<ExternalSourceModule>? externalModules = null)
    {
        var diagnostics = new List<Diagnostic>();
        var dependencies = new List<SourceModuleDependency>();
        var moduleByPath = new Dictionary<string, SourceModule>(StringComparer.OrdinalIgnoreCase);
        var localExportsByPath = new Dictionary<string, SourceModuleExports>(StringComparer.OrdinalIgnoreCase);
        var exportsByPath = new Dictionary<string, SourceModuleExports>(StringComparer.OrdinalIgnoreCase);
        var externalModuleBySpecifier = (externalModules ?? [])
            .GroupBy(module => module.Specifier, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var aliases = CreateSourceAliasContext(sourceAliases ?? [], modules, diagnostics);
        foreach (var module in modules)
        {
            moduleByPath.TryAdd(module.SourceFile.ModulePath, module);
            localExportsByPath.TryAdd(module.SourceFile.ModulePath, CollectLocalExports(module.Root));
        }

        foreach (var module in modules)
        {
            CollectExports(module, aliases, moduleByPath, localExportsByPath, exportsByPath, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        foreach (var module in modules)
        {
            CollectDependencies(module, module.Root, aliases, externalModuleBySpecifier, moduleByPath, exportsByPath, dependencies, diagnostics);
            ValidateNamespaceImportMemberAccesses(module, aliases, externalModuleBySpecifier, moduleByPath, exportsByPath, diagnostics);
        }

        return new SourceModuleGraph(modules, dependencies, new Dictionary<string, SourceModuleExports>(exportsByPath, StringComparer.OrdinalIgnoreCase), diagnostics);
    }

    private static void CollectDependencies(
        SourceModule module,
        SyntaxNode node,
        SourceAliasContext aliases,
        IReadOnlyDictionary<string, ExternalSourceModule> externalModuleBySpecifier,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath,
        List<SourceModuleDependency> dependencies,
        List<Diagnostic> diagnostics)
    {
        if (TryGetDependencyKind(node, out var kind) &&
            TryGetModuleSpecifier(node, out var specifierToken, out var specifier))
        {
            if (node.Kind == SyntaxKind.ImportSideEffectDeclaration)
            {
                diagnostics.Add(new Diagnostic(
                    DiagnosticDescriptors.UnsupportedSourceModuleImport.Code,
                    DiagnosticDescriptors.UnsupportedSourceModuleImport.DefaultSeverity,
                    $"Side-effect-only import '{specifier}' is parsed but not supported by the TypeSharp source module graph.",
                    module.SourceFile.RelativePath,
                    specifierToken.Span));
                goto VisitChildren;
            }

            if (!TryResolveSourceModulePath(module, specifier, specifierToken, aliases, externalModuleBySpecifier, diagnostics, out var resolvedModulePath, out var externalModule))
            {
                if (LooksLikeProjectSourceModuleSpecifier(specifier))
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.UnresolvedSourceModule.Code,
                        DiagnosticDescriptors.UnresolvedSourceModule.DefaultSeverity,
                        $"Project source module specifier '{specifier}' does not resolve to a current source alias or direct project reference.",
                        module.SourceFile.RelativePath,
                        specifierToken.Span));
                }

                goto VisitChildren;
            }

            if (externalModule is null && !moduleByPath.ContainsKey(resolvedModulePath))
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
                var targetExports = externalModule?.Exports;
                dependencies.Add(new SourceModuleDependency(
                    kind,
                    module.SourceFile.ModulePath,
                    resolvedModulePath,
                    specifier,
                    module.SourceFile.RelativePath,
                    specifierToken.Span));

                if (kind == SourceModuleDependencyKind.Import &&
                    (targetExports is not null || exportsByPath.TryGetValue(resolvedModulePath, out targetExports)))
                {
                    ValidateSourceImportExports(
                        node,
                        targetExports,
                        specifier,
                        resolvedModulePath,
                        module.SourceFile.RelativePath,
                        diagnostics);
                }
                else if (kind == SourceModuleDependencyKind.Export &&
                    IsSupportedSourceReExport(node, aliases) &&
                    (targetExports is not null || exportsByPath.TryGetValue(resolvedModulePath, out targetExports)))
                {
                    ValidateSourceReExportExports(
                        node,
                        targetExports,
                        specifier,
                        resolvedModulePath,
                        module.SourceFile.RelativePath,
                        diagnostics);
                }
            }
        }

    VisitChildren:
        foreach (var child in node.Children)
        {
            CollectDependencies(module, child, aliases, externalModuleBySpecifier, moduleByPath, exportsByPath, dependencies, diagnostics);
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
            case SyntaxKind.ImportSideEffectDeclaration:
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

    private static bool IsSupportedSourceReExport(SyntaxNode node, SourceAliasContext aliases) =>
        node.Kind is SyntaxKind.ExportNamedDeclaration or SyntaxKind.ExportTypeDeclaration or SyntaxKind.ExportStarDeclaration &&
        TryGetModuleSpecifier(node, out _, out var specifier) &&
        (IsRelativeSpecifier(specifier) || MatchesSourceAlias(specifier, aliases));

    private static void ValidateNamespaceImportMemberAccesses(
        SourceModule module,
        SourceAliasContext aliases,
        IReadOnlyDictionary<string, ExternalSourceModule> externalModuleBySpecifier,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath,
        List<Diagnostic> diagnostics)
    {
        var namespaceImports = CollectRelativeNamespaceImports(module, aliases, externalModuleBySpecifier, moduleByPath, exportsByPath, diagnostics);
        if (namespaceImports.Count == 0)
        {
            return;
        }

        ValidateNamespaceImportMemberAccesses(module.Root, namespaceImports, module.SourceFile.RelativePath, diagnostics);
    }

    private static IReadOnlyDictionary<string, SourceModuleNamespaceImport> CollectRelativeNamespaceImports(
        SourceModule module,
        SourceAliasContext aliases,
        IReadOnlyDictionary<string, ExternalSourceModule> externalModuleBySpecifier,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
        IReadOnlyDictionary<string, SourceModuleExports> exportsByPath,
        List<Diagnostic> diagnostics)
    {
        var imports = new Dictionary<string, SourceModuleNamespaceImport>(StringComparer.Ordinal);
        foreach (var child in module.Root.Children.Where(child => child.Kind == SyntaxKind.ImportNamespaceDeclaration))
        {
            if (!TryGetModuleSpecifier(child, out _, out var specifier) ||
                !TryGetNamespaceImportAlias(child, out var alias))
            {
                continue;
            }

            var specifierToken = child.Children.First(grandchild => grandchild.Kind == SyntaxKind.StringLiteralToken);
            if (!TryResolveSourceModulePath(module, specifier, specifierToken, aliases, externalModuleBySpecifier, diagnostics, out var resolvedModulePath, out var externalModule))
            {
                continue;
            }

            var exports = externalModule?.Exports;
            if ((externalModule is null && !moduleByPath.ContainsKey(resolvedModulePath)) ||
                (exports is null && !exportsByPath.TryGetValue(resolvedModulePath, out exports)))
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
        if (node.Kind == SyntaxKind.ImportSideEffectDeclaration &&
            node.Children.FirstOrDefault(child => child.Kind == SyntaxKind.StringLiteralToken) is { } sideEffectSpecifier)
        {
            specifierToken = sideEffectSpecifier;
            specifier = Unquote(specifierToken.Text ?? string.Empty);
            return true;
        }

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

    private static SourceAliasContext CreateSourceAliasContext(
        IReadOnlyList<SourceAliasOption> aliases,
        IReadOnlyList<SourceModule> modules,
        List<Diagnostic> diagnostics)
    {
        var modulePathByRelativeTarget = modules
            .GroupBy(module => StripTyshExtension(NormalizeAliasPath(module.SourceFile.RelativePath, allowWildcard: false, out _)), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().SourceFile.ModulePath, StringComparer.OrdinalIgnoreCase);
        var sourceRoots = modules
            .Select(module => NormalizeAliasPath(module.SourceFile.SourceRoot, allowWildcard: false, out _))
            .Where(root => root.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (aliases.Count == 0)
        {
            return new SourceAliasContext([], modulePathByRelativeTarget, sourceRoots);
        }

        var rules = new List<SourceAliasRule>();
        var seen = new Dictionary<string, SourceAliasOption>(StringComparer.OrdinalIgnoreCase);

        foreach (var alias in aliases)
        {
            var pattern = alias.Pattern.Trim();
            var target = alias.Target.Trim();
            var hasError = false;

            if (pattern.Length == 0)
            {
                ReportInvalidAlias(alias, "Source alias key cannot be empty.", diagnostics);
                hasError = true;
            }
            else if (IsRelativeSpecifier(pattern))
            {
                ReportInvalidAlias(alias, $"Source alias '{pattern}' must be a bare module specifier, not a relative path.", diagnostics);
                hasError = true;
            }

            if (target.Length == 0)
            {
                ReportInvalidAlias(alias, $"Source alias '{pattern}' target cannot be empty.", diagnostics);
                hasError = true;
            }

            var patternWildcards = CountWildcards(pattern);
            var targetWildcards = CountWildcards(target);
            if (patternWildcards > 1 || targetWildcards > 1)
            {
                ReportInvalidAlias(alias, $"Source alias '{pattern}' and target '{target}' may contain at most one '*' wildcard each.", diagnostics);
                hasError = true;
            }
            else if (patternWildcards != targetWildcards)
            {
                ReportInvalidAlias(alias, $"Source alias '{pattern}' and target '{target}' must either both use one '*' wildcard or neither use one.", diagnostics);
                hasError = true;
            }

            if (seen.TryGetValue(pattern, out var existing))
            {
                ReportInvalidAlias(alias, $"Source alias '{pattern}' collides with alias '{existing.Pattern}' by Windows-compatible case-insensitive comparison.", diagnostics);
                hasError = true;
            }
            else if (pattern.Length > 0)
            {
                seen.Add(pattern, alias);
            }

            if (!TryCreateAliasRule(pattern, target, sourceRoots, out var rule, out var message))
            {
                ReportInvalidAlias(alias, message, diagnostics);
                hasError = true;
            }

            if (!hasError)
            {
                rules.Add(rule);
            }
        }

        var orderedRules = rules
            .OrderByDescending(rule => rule.MatchPrefix.Length)
            .ThenBy(rule => rule.Pattern, StringComparer.Ordinal)
            .ToArray();
        return new SourceAliasContext(orderedRules, modulePathByRelativeTarget, sourceRoots);
    }

    private static bool TryCreateAliasRule(
        string pattern,
        string target,
        IReadOnlyList<string> sourceRoots,
        out SourceAliasRule rule,
        out string message)
    {
        rule = default!;
        message = string.Empty;

        if (pattern.Length == 0 || target.Length == 0)
        {
            message = "Source alias key and target must be non-empty.";
            return false;
        }

        if (Path.IsPathRooted(target))
        {
            message = $"Source alias '{pattern}' target '{target}' must be project-relative.";
            return false;
        }

        var patternWildcard = pattern.IndexOf('*');
        var targetWildcard = target.IndexOf('*');
        var normalizedTargetPrefix = NormalizeAliasPath(
            targetWildcard >= 0 ? target[..targetWildcard] : target,
            allowWildcard: false,
            out var targetEscapesProject);
        if (targetEscapesProject)
        {
            message = $"Source alias '{pattern}' target '{target}' escapes the project directory.";
            return false;
        }

        if (sourceRoots.Count > 0 &&
            !IsUnderSourceRoot(normalizedTargetPrefix, sourceRoots))
        {
            message = $"Source alias '{pattern}' target '{target}' must normalize under a configured source root.";
            return false;
        }

        var matchPrefix = patternWildcard >= 0 ? pattern[..patternWildcard] : pattern;
        var matchSuffix = patternWildcard >= 0 ? pattern[(patternWildcard + 1)..] : string.Empty;
        var targetPrefix = targetWildcard >= 0 ? target[..targetWildcard] : target;
        var targetSuffix = targetWildcard >= 0 ? target[(targetWildcard + 1)..] : string.Empty;
        rule = new SourceAliasRule(pattern, target, matchPrefix, matchSuffix, targetPrefix, targetSuffix, patternWildcard >= 0);
        return true;
    }

    private static bool TryResolveSourceModulePath(
        SourceModule module,
        string specifier,
        SyntaxNode specifierToken,
        SourceAliasContext aliases,
        IReadOnlyDictionary<string, ExternalSourceModule> externalModuleBySpecifier,
        List<Diagnostic>? diagnostics,
        out string resolvedModulePath,
        out ExternalSourceModule? externalModule)
    {
        externalModule = null;
        if (IsRelativeSpecifier(specifier))
        {
            resolvedModulePath = ResolveRelativeModulePath(module.SourceFile.ModulePath, specifier);
            return true;
        }

        if (externalModuleBySpecifier.TryGetValue(specifier, out externalModule))
        {
            resolvedModulePath = externalModule.ModulePath;
            return true;
        }

        var matches = aliases.Rules.Where(alias => alias.Matches(specifier)).ToArray();
        if (matches.Length == 0)
        {
            resolvedModulePath = string.Empty;
            return false;
        }

        var bestPrefixLength = matches.Max(match => match.MatchPrefix.Length);
        var bestMatches = matches
            .Where(match => match.MatchPrefix.Length == bestPrefixLength)
            .OrderBy(match => match.Pattern, StringComparer.Ordinal)
            .ToArray();
        if (bestMatches.Length > 1)
        {
            diagnostics?.Add(new Diagnostic(
                DiagnosticDescriptors.UnsupportedSourceModuleImport.Code,
                DiagnosticDescriptors.UnsupportedSourceModuleImport.DefaultSeverity,
                $"Source module specifier '{specifier}' matches ambiguous source aliases '{bestMatches[0].Pattern}' and '{bestMatches[1].Pattern}'.",
                module.SourceFile.RelativePath,
                specifierToken.Span));
            resolvedModulePath = string.Empty;
            return false;
        }

        var alias = bestMatches[0];
        var capture = alias.HasWildcard
            ? specifier.Substring(alias.MatchPrefix.Length, specifier.Length - alias.MatchPrefix.Length - alias.MatchSuffix.Length)
            : string.Empty;
        var expandedTarget = alias.HasWildcard
            ? alias.TargetPrefix + capture + alias.TargetSuffix
            : alias.Target;
        var normalizedTarget = NormalizeAliasPath(expandedTarget, allowWildcard: false, out var escapesProject);
        if (escapesProject || (aliases.SourceRoots.Count > 0 && !IsUnderSourceRoot(normalizedTarget, aliases.SourceRoots)))
        {
            diagnostics?.Add(new Diagnostic(
                DiagnosticDescriptors.UnresolvedSourceModule.Code,
                DiagnosticDescriptors.UnresolvedSourceModule.DefaultSeverity,
                $"Source alias specifier '{specifier}' expands to target '{normalizedTarget}', which is outside configured source roots.",
                module.SourceFile.RelativePath,
                specifierToken.Span));
            resolvedModulePath = string.Empty;
            return false;
        }

        var normalizedTargetWithoutExtension = StripTyshExtension(normalizedTarget);
        if (aliases.ModulePathByRelativeTarget.TryGetValue(normalizedTargetWithoutExtension, out var aliasResolvedModulePath))
        {
            resolvedModulePath = aliasResolvedModulePath;
            return true;
        }

        diagnostics?.Add(new Diagnostic(
            DiagnosticDescriptors.UnresolvedSourceModule.Code,
            DiagnosticDescriptors.UnresolvedSourceModule.DefaultSeverity,
            $"Source alias specifier '{specifier}' expands to target '{normalizedTargetWithoutExtension}', but no discovered source module was found.",
            module.SourceFile.RelativePath,
            specifierToken.Span));
        resolvedModulePath = string.Empty;
        return false;
    }

    private static bool MatchesSourceAlias(string specifier, SourceAliasContext aliases) =>
        aliases.Rules.Any(alias => alias.Matches(specifier));

    private static bool LooksLikeProjectSourceModuleSpecifier(string specifier) =>
        specifier.Contains('/', StringComparison.Ordinal) &&
        !specifier.StartsWith("./", StringComparison.Ordinal) &&
        !specifier.StartsWith("../", StringComparison.Ordinal);

    private static int CountWildcards(string value) =>
        value.Count(character => character == '*');

    private static void ReportInvalidAlias(SourceAliasOption alias, string message, List<Diagnostic> diagnostics)
    {
        diagnostics.Add(DiagnosticFactory.Manifest(
            DiagnosticDescriptors.InvalidManifestValue,
            message,
            alias.File,
            alias.Line,
            alias.Column));
    }

    private static bool IsUnderSourceRoot(string normalizedTarget, IReadOnlyList<string> sourceRoots)
    {
        foreach (var sourceRoot in sourceRoots)
        {
            if (sourceRoot == ".")
            {
                return true;
            }

            if (string.Equals(normalizedTarget, sourceRoot, StringComparison.OrdinalIgnoreCase) ||
                normalizedTarget.StartsWith(sourceRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeAliasPath(string path, bool allowWildcard, out bool escapesProject)
    {
        escapesProject = false;
        var normalized = path.Replace('\\', '/').Trim();
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(normalized))
        {
            escapesProject = true;
            return normalized.Trim('/');
        }

        var parts = new List<string>();
        foreach (var part in normalized.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count == 0)
                {
                    escapesProject = true;
                    continue;
                }

                parts.RemoveAt(parts.Count - 1);
                continue;
            }

            if (!allowWildcard && part.Contains('*', StringComparison.Ordinal))
            {
                parts.Add(part.Replace("*", string.Empty, StringComparison.Ordinal));
                continue;
            }

            parts.Add(part);
        }

        return parts.Count == 0 ? "." : string.Join("/", parts);
    }

    private static string StripTyshExtension(string path) =>
        path.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? path[..^".tysh".Length]
            : path;

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
        SourceAliasContext aliases,
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

        foreach (var exportDeclaration in module.Root.Children.Where(child => IsSupportedSourceReExport(child, aliases)))
        {
            if (!TryGetModuleSpecifier(exportDeclaration, out var specifierToken, out var specifier) ||
                !TryResolveSourceModulePath(module, specifier, specifierToken, aliases, new Dictionary<string, ExternalSourceModule>(StringComparer.Ordinal), diagnostics: null, out var resolvedModulePath, out _))
            {
                continue;
            }

            if (!moduleByPath.TryGetValue(resolvedModulePath, out var targetModule))
            {
                continue;
            }

            var targetExports = CollectExports(targetModule, aliases, moduleByPath, localExportsByPath, exportsByPath, visiting);
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
        node.Kind == SyntaxKind.ValueDeclaration && (!IsFunctionValueDeclaration(node) || HasFunctionTypeAnnotation(node) || HasLambdaInitializer(node));

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
            SyntaxKind.EnumDeclaration or
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

    private sealed record SourceModuleNamespaceImport(
        string Alias,
        string Specifier,
        string ResolvedModulePath,
        SourceModuleExports Exports);

    private sealed record SourceAliasContext(
        IReadOnlyList<SourceAliasRule> Rules,
        IReadOnlyDictionary<string, string> ModulePathByRelativeTarget,
        IReadOnlyList<string> SourceRoots);

    private sealed record SourceAliasRule(
        string Pattern,
        string Target,
        string MatchPrefix,
        string MatchSuffix,
        string TargetPrefix,
        string TargetSuffix,
        bool HasWildcard)
    {
        public bool Matches(string specifier)
        {
            if (!HasWildcard)
            {
                return string.Equals(specifier, Pattern, StringComparison.Ordinal);
            }

            return specifier.Length >= MatchPrefix.Length + MatchSuffix.Length &&
                specifier.StartsWith(MatchPrefix, StringComparison.Ordinal) &&
                specifier.EndsWith(MatchSuffix, StringComparison.Ordinal);
        }
    }

    private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName, SourceSpan Span)
    {
        public bool IsAlias => !string.Equals(ImportedName, LocalName, StringComparison.Ordinal);
    }

    private readonly record struct NamedExportSpecifier(string TargetName, string ExportedName, SourceSpan Span)
    {
        public bool IsAlias => !string.Equals(TargetName, ExportedName, StringComparison.Ordinal);
    }
}
