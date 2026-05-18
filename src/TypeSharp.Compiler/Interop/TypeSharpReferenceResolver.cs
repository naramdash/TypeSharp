using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Projects;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpReferenceResolver
{
    public static ReferenceResolutionResult Resolve(TypeSharpManifest manifest)
    {
        var diagnostics = new List<Diagnostic>();
        var references = new List<ResolvedReference>();
        var seenAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in manifest.References.Assemblies)
        {
            var identity = assembly.Trim();
            if (identity.Length == 0 || !seenAssemblies.Add(identity))
            {
                continue;
            }

            references.Add(new ResolvedReference(
                ResolvedReferenceKind.FrameworkAssembly,
                identity,
                assembly,
                Path: null,
                RelativePath: null));
        }

        foreach (var path in manifest.References.Paths)
        {
            var trimmed = path.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(manifest.ProjectDirectory, trimmed));
            if (!File.Exists(fullPath))
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.MissingReference,
                    $"Referenced assembly path '{trimmed}' does not exist.",
                    manifest.ManifestPath));
                continue;
            }

            if (!seenPaths.Add(fullPath))
            {
                continue;
            }

            references.Add(new ResolvedReference(
                ResolvedReferenceKind.LocalAssembly,
                System.IO.Path.GetFileNameWithoutExtension(fullPath),
                path,
                fullPath,
                NormalizeRelativePath(System.IO.Path.GetRelativePath(manifest.ProjectDirectory, fullPath))));
        }

        return new ReferenceResolutionResult(references, diagnostics);
    }

    private static string NormalizeRelativePath(string path) =>
        path.Replace(System.IO.Path.DirectorySeparatorChar, '/').Replace(System.IO.Path.AltDirectorySeparatorChar, '/');
}
