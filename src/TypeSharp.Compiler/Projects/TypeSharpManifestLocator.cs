using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public static class TypeSharpManifestLocator
{
    public const string ManifestFileName = "TypeSharp.toml";

    public static ManifestPathResult Locate(string? projectOrManifestPath, string startDirectory)
    {
        var baseDirectory = Path.GetFullPath(startDirectory);

        if (string.IsNullOrWhiteSpace(projectOrManifestPath))
        {
            return SearchParents(baseDirectory, ManifestFileName);
        }

        var requestedPath = Path.GetFullPath(projectOrManifestPath, baseDirectory);

        if (File.Exists(requestedPath))
        {
            return new ManifestPathResult(requestedPath, []);
        }

        if (Directory.Exists(requestedPath))
        {
            return SearchParents(requestedPath, ManifestFileName);
        }

        return new ManifestPathResult(
            null,
            [
                DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.ManifestNotFound,
                    $"Could not find TypeSharp manifest '{requestedPath}'.",
                    requestedPath)
            ]);
    }

    private static ManifestPathResult SearchParents(string startDirectory, string manifestFileName)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startDirectory));

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, manifestFileName);
            if (File.Exists(candidate))
            {
                return new ManifestPathResult(candidate, []);
            }

            directory = directory.Parent;
        }

        return new ManifestPathResult(
            null,
            [
                DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.ManifestNotFound,
                    $"Could not find {ManifestFileName} in '{startDirectory}' or any parent directory.",
                    Path.Combine(startDirectory, ManifestFileName))
            ]);
    }
}
