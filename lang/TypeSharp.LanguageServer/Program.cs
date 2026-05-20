namespace TypeSharp.LanguageServer;

public static class Program
{
    public static int Main()
    {
        TypeSharpLanguageServer.Run(
            Console.OpenStandardInput(),
            Console.OpenStandardOutput(),
            Directory.GetCurrentDirectory());
        return 0;
    }
}
