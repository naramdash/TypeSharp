using System.Reflection;

namespace TypeSharp.Compiler;

public static class TypeSharpCompilerInfo
{
    public const string CliVersion = "0.1.0-preview";
    public const string CompilerVersion = "0.1.0-preview";
    public const string LanguageVersion = "preview";
    public const int RuntimeAbiVersion = 0;
    public const string DefaultTargetFramework = "net48";
    public const string CliTargetFramework = "net10.0";
    public const string RuntimeTargetFramework = "net48";
    public const string ArtifactKind = "framework-dependent-dotnet";
    public const string ReleaseChannel = "Preview";
    public const string RuntimeAbiStatus = "preview";

    public static string BuildMetadata => GetAssemblyMetadata("TypeSharpBuildMetadata", "local");

    public static string SourceRevision => GetAssemblyMetadata("TypeSharpSourceRevision", "unknown");

    private static string GetAssemblyMetadata(string name, string fallback)
    {
        foreach (var attribute in typeof(TypeSharpCompilerInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (string.Equals(attribute.Key, name, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(attribute.Value))
            {
                return attribute.Value;
            }
        }

        return fallback;
    }
}
