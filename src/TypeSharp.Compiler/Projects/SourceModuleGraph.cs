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
        foreach (var module in modules)
        {
            moduleByPath.TryAdd(module.SourceFile.ModulePath, module);
        }

        foreach (var module in modules)
        {
            CollectDependencies(module, module.Root, moduleByPath, dependencies, diagnostics);
        }

        return new SourceModuleGraph(modules, dependencies, diagnostics);
    }

    private static void CollectDependencies(
        SourceModule module,
        SyntaxNode node,
        IReadOnlyDictionary<string, SourceModule> moduleByPath,
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
                    !IsSupportedSourceImport(node))
                {
                    diagnostics.Add(new Diagnostic(
                        DiagnosticDescriptors.UnsupportedSourceModuleImport.Code,
                        DiagnosticDescriptors.UnsupportedSourceModuleImport.DefaultSeverity,
                        $"Source module import '{specifier}' resolves to '{resolvedModulePath}', but this import form is not implemented yet.",
                        module.SourceFile.RelativePath,
                        specifierToken.Span));
                }
            }
        }

        foreach (var child in node.Children)
        {
            CollectDependencies(module, child, moduleByPath, dependencies, diagnostics);
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

    private static bool IsSupportedSourceImport(SyntaxNode node)
    {
        if (node.Kind == SyntaxKind.ImportNamespaceDeclaration)
        {
            return true;
        }

        if (node.Kind is SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration)
        {
            return !GetNamedImportSpecifiers(node).Any(specifier => specifier.IsAlias);
        }

        return false;
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

    private readonly record struct NamedImportSpecifier(string ImportedName, string LocalName)
    {
        public bool IsAlias => !string.Equals(ImportedName, LocalName, StringComparison.Ordinal);
    }
}
