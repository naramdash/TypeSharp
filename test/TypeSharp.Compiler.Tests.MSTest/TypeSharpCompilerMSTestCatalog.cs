using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]

[TestClass]
public sealed class TypeSharpCompilerMSTestCatalog
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    public static IEnumerable<object[]> CatalogCases()
    {
        var settings = TestRunnerSettings.Create(Array.Empty<string>());
        var ordinal = 0;

        foreach (var test in TypeSharpCompilerTestCases.All)
        {
            var currentOrdinal = ordinal++;
            if (!settings.Includes(test.Name, currentOrdinal))
            {
                continue;
            }

            yield return new object[] { test };
        }
    }

    public static string GetCatalogCaseDisplayName(MethodInfo methodInfo, object[] data)
    {
        return data.Length == 1 && data[0] is TypeSharpCompilerTestCase test
            ? test.Name
            : methodInfo.Name;
    }

    [TestMethod]
    public void CatalogIsExposedForPackageRunners()
    {
        Assert.AreEqual(525, TypeSharpCompilerTestCases.All.Count);
        Assert.AreEqual(
            TypeSharpCompilerTestCases.All.Count,
            TypeSharpCompilerTestCases.All.Select(test => test.Name).Distinct(StringComparer.Ordinal).Count());
    }

    [TestMethod]
    [DynamicData(
        nameof(CatalogCases),
        DynamicDataDisplayName = nameof(GetCatalogCaseDisplayName))]
    public void CatalogCase(object catalogCase)
    {
        var test = (TypeSharpCompilerTestCase)catalogCase;
        RunFromRepositoryRoot(test.Body);
    }

    private static void RunFromRepositoryRoot(Action action)
    {
        var previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = RepositoryRoot;
            action();
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            if (File.Exists(Path.Combine(directory, "agent.md")) &&
                File.Exists(Path.Combine(directory, "Directory.Build.props")) &&
                Directory.Exists(Path.Combine(directory, "test")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not find the TypeSharp repository root.");
    }
}
