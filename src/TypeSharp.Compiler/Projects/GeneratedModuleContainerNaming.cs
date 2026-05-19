using System.Text;

namespace TypeSharp.Compiler.Projects;

public static class GeneratedModuleContainerNaming
{
    public static string GetContainerName(SourceFile sourceFile, int sourceFileCount)
    {
        if (sourceFileCount <= 1)
        {
            return "Module";
        }

        var builder = new StringBuilder("Module");
        foreach (var character in sourceFile.ModulePath)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }
}
