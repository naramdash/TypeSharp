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

        var references = new ReferenceOptions(
            document.GetStringArray("references", "assemblies", ReferenceOptions.Empty.Assemblies, diagnostics),
            document.GetStringArray("references", "paths", ReferenceOptions.Empty.Paths, diagnostics),
            document.GetStringArray("references", "packages", ReferenceOptions.Empty.Packages, diagnostics));

        var defaultTooling = ToolingOptions.Default;
        var tooling = new ToolingOptions(
            document.GetString("tooling", "diagnosticFormat", defaultTooling.DiagnosticFormat, diagnostics),
            document.GetBool("tooling", "treatWarningsAsErrors", defaultTooling.TreatWarningsAsErrors, diagnostics));

        if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return new ManifestLoadResult(null, diagnostics);
        }

        return new ManifestLoadResult(
            new TypeSharpManifest(fullManifestPath, projectDirectory, project, language, references, tooling),
            diagnostics);
    }
}
