using TypeSharp.Cli;

return TypeSharpCli.Run(
    args,
    Console.Out,
    Console.Error,
    Console.OpenStandardInput(),
    Console.OpenStandardOutput(),
    Directory.GetCurrentDirectory());
