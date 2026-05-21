using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public static class TypeSharpManifestLoader
{
    public static ManifestLoadResult Load(string manifestPath)
    {
        var diagnostics = new List<Diagnostic>();
        var fullManifestPath = Path.GetFullPath(manifestPath);

        if (!File.Exists(fullManifestPath))
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.ManifestNotFound,
                $"Could not find TypeSharp manifest '{fullManifestPath}'.",
                fullManifestPath));
            return new ManifestLoadResult(null, diagnostics);
        }

        string[] lines;
        try
        {
            lines = File.ReadAllLines(fullManifestPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.ManifestUnreadable,
                $"Could not read TypeSharp manifest: {ex.Message}",
                fullManifestPath));
            return new ManifestLoadResult(null, diagnostics);
        }

        var document = MinimalTomlDocument.Parse(fullManifestPath, lines, diagnostics);
        if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return new ManifestLoadResult(null, diagnostics);
        }

        var projectDirectory = Path.GetDirectoryName(fullManifestPath) ?? Directory.GetCurrentDirectory();
        var defaultProject = ProjectOptions.Default(projectDirectory);

        var project = new ProjectOptions(
            document.GetString("project", "name", defaultProject.Name, diagnostics),
            document.GetString("project", "targetFramework", defaultProject.TargetFramework, diagnostics),
            document.GetString("project", "outputType", defaultProject.OutputType, diagnostics),
            document.GetOptionalString("project", "rootNamespace", diagnostics),
            document.GetStringArray("project", "sourceRoots", defaultProject.SourceRoots, diagnostics),
            document.GetOptionalString("project", "main", diagnostics),
            document.GetString("project", "generatedOutputRoot", defaultProject.GeneratedOutputRoot, diagnostics));

        var defaultLanguage = LanguageOptions.Default;
        var language = new LanguageOptions(
            document.GetString("language", "version", defaultLanguage.Version, diagnostics),
            document.GetBool("language", "strict", defaultLanguage.Strict, diagnostics),
            document.GetString("language", "nullable", defaultLanguage.Nullable, diagnostics),
            document.GetStringArray("language", "previewFeatures", defaultLanguage.PreviewFeatures, diagnostics));

        var modules = new ModuleOptions(
            document
                .GetStringMap("modules.aliases", diagnostics)
                .Select(entry => new SourceAliasOption(entry.Key, entry.Value, fullManifestPath, entry.Line, entry.Column))
                .ToArray());

        document.TryGetLocation("projectReferences", "paths", out var projectReferenceLine, out var projectReferenceColumn);
        var projectReferences = new ProjectReferenceOptions(
            document
                .GetStringArray("projectReferences", "paths", ProjectReferenceOptions.Empty.Paths.Select(path => path.Path).ToArray(), diagnostics)
                .Select(path => new ProjectReferencePathOption(path, fullManifestPath, projectReferenceLine, projectReferenceColumn))
                .ToArray());

        var references = new ReferenceOptions(
            document.GetStringArray("references", "assemblies", ReferenceOptions.Empty.Assemblies, diagnostics),
            document.GetStringArray("references", "paths", ReferenceOptions.Empty.Paths, diagnostics),
            document.GetStringArray("references", "packages", ReferenceOptions.Empty.Packages, diagnostics));

        var defaultTooling = ToolingOptions.Default;
        var tooling = new ToolingOptions(
            document.GetString("tooling", "diagnosticFormat", defaultTooling.DiagnosticFormat, diagnostics),
            document.GetBool("tooling", "treatWarningsAsErrors", defaultTooling.TreatWarningsAsErrors, diagnostics));

        ValidateManifestOptions(fullManifestPath, document, project, language, tooling, diagnostics);

        if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return new ManifestLoadResult(null, diagnostics);
        }

        return new ManifestLoadResult(
            new TypeSharpManifest(fullManifestPath, projectDirectory, project, language, modules, projectReferences, references, tooling),
            diagnostics);
    }

    private static void ValidateManifestOptions(
        string manifestPath,
        MinimalTomlDocument document,
        ProjectOptions project,
        LanguageOptions language,
        ToolingOptions tooling,
        List<Diagnostic> diagnostics)
    {
        ValidateOneOf(
            manifestPath,
            document,
            "project",
            "outputType",
            project.OutputType,
            ["library", "exe"],
            diagnostics);
        ValidateOneOf(
            manifestPath,
            document,
            "language",
            "version",
            language.Version,
            [TypeSharpCompilerInfo.LanguageVersion],
            diagnostics);
        ValidateOneOf(
            manifestPath,
            document,
            "language",
            "nullable",
            language.Nullable,
            ["strict", "loose"],
            diagnostics);
        ValidateOneOf(
            manifestPath,
            document,
            "tooling",
            "diagnosticFormat",
            tooling.DiagnosticFormat,
            ["text", "json"],
            diagnostics);
    }

    private static void ValidateOneOf(
        string manifestPath,
        MinimalTomlDocument document,
        string section,
        string key,
        string value,
        IReadOnlyList<string> supportedValues,
        List<Diagnostic> diagnostics)
    {
        if (supportedValues.Contains(value, StringComparer.Ordinal))
        {
            return;
        }

        if (!document.TryGetLocation(section, key, out var line, out var column))
        {
            return;
        }

        diagnostics.Add(DiagnosticFactory.Manifest(
            DiagnosticDescriptors.InvalidManifestValue,
            $"Manifest value '{section}.{key}' must be {FormatSupportedValues(supportedValues)}.",
            manifestPath,
            line,
            column));
    }

    private static string FormatSupportedValues(IReadOnlyList<string> supportedValues)
    {
        if (supportedValues.Count == 1)
        {
            return $"'{supportedValues[0]}'";
        }

        return "one of " + string.Join(", ", supportedValues.Select(value => $"'{value}'"));
    }
}
