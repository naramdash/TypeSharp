using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public static class SourceDiscovery
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git"
    };

    public static SourceDiscoveryResult Discover(TypeSharpManifest manifest)
    {
        var diagnostics = new List<Diagnostic>();
        var files = new List<SourceFile>();
        var generatedOutputRoot = NormalizeFullPath(
            Path.Combine(manifest.ProjectDirectory, manifest.Project.GeneratedOutputRoot));

        foreach (var sourceRoot in manifest.Project.SourceRoots)
        {
            var sourceRootPath = NormalizeFullPath(Path.Combine(manifest.ProjectDirectory, sourceRoot));
            if (!Directory.Exists(sourceRootPath))
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.SourceRootNotFound,
                    $"Source root '{sourceRoot}' does not exist.",
                    manifest.ManifestPath));
                continue;
            }

            CollectSourceFiles(
                manifest.ProjectDirectory,
                sourceRootPath,
                generatedOutputRoot,
                files);
        }

        var orderedFiles = files
            .DistinctBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new SourceDiscoveryResult(orderedFiles, diagnostics);
    }

    private static void CollectSourceFiles(
        string projectDirectory,
        string directory,
        string generatedOutputRoot,
        List<SourceFile> files)
    {
        if (IsExcludedDirectory(directory, generatedOutputRoot))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.tysh").OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            var fullPath = NormalizeFullPath(file);
            var relativePath = NormalizeRelativePath(Path.GetRelativePath(projectDirectory, fullPath));
            files.Add(new SourceFile(fullPath, relativePath));
        }

        foreach (var childDirectory in Directory.EnumerateDirectories(directory).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            CollectSourceFiles(projectDirectory, NormalizeFullPath(childDirectory), generatedOutputRoot, files);
        }
    }

    private static bool IsExcludedDirectory(string directory, string generatedOutputRoot)
    {
        var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (ExcludedDirectoryNames.Contains(name))
        {
            return true;
        }

        return IsSameOrChildPath(directory, generatedOutputRoot);
    }

    private static bool IsSameOrChildPath(string path, string parent)
    {
        var normalizedPath = NormalizeFullPath(path);
        var normalizedParent = NormalizeFullPath(parent);
        return normalizedPath.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeFullPath(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string NormalizeRelativePath(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
}
