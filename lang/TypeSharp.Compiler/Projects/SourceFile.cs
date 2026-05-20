namespace TypeSharp.Compiler.Projects;

public sealed record SourceFile(
    string Path,
    string RelativePath,
    string SourceRoot,
    string SourceRootRelativePath,
    string ModulePath)
{
    public SourceFile(string path, string relativePath)
        : this(path, relativePath, string.Empty, relativePath, DeriveModulePath(relativePath))
    {
    }

    public static string DeriveModulePath(string sourceRootRelativePath)
    {
        var normalizedPath = sourceRootRelativePath
            .Replace(System.IO.Path.DirectorySeparatorChar, '/')
            .Replace(System.IO.Path.AltDirectorySeparatorChar, '/')
            .Trim('/');

        return normalizedPath.EndsWith(".tysh", StringComparison.OrdinalIgnoreCase)
            ? normalizedPath[..^".tysh".Length]
            : normalizedPath;
    }
}
