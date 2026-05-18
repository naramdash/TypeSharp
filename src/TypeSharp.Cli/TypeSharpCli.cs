using TypeSharp.Compiler;
using TypeSharp.Compiler.Building;
using TypeSharp.Compiler.Checking;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Projects;

namespace TypeSharp.Cli;

public static class TypeSharpCli
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length == 0)
        {
            WriteUsage(error);
            return 2;
        }

        return args[0] switch
        {
            "version" => RunVersion(args, output, error),
            "check" => RunCheck(args, output, error),
            "build" => RunBuild(args, output, error),
            "run" => RunProjectCommandPlaceholder(args[0], args, output, error),
            "--help" or "-h" or "help" => RunHelp(output),
            _ => RunUnknownCommand(args[0], error)
        };
    }

    private static int RunVersion(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length > 2 || args is [_, var option] && option != "--json")
        {
            error.WriteLine("Usage: typesharp version [--json]");
            return 2;
        }

        if (args.Length == 2)
        {
            output.WriteLine("{");
            output.WriteLine($"  \"cli\": \"{TypeSharpCompilerInfo.CliVersion}\",");
            output.WriteLine($"  \"compiler\": \"{TypeSharpCompilerInfo.CompilerVersion}\",");
            output.WriteLine($"  \"language\": \"{TypeSharpCompilerInfo.LanguageVersion}\",");
            output.WriteLine($"  \"runtimeAbi\": {TypeSharpCompilerInfo.RuntimeAbiVersion},");
            output.WriteLine($"  \"targetDefault\": \"{TypeSharpCompilerInfo.DefaultTargetFramework}\"");
            output.WriteLine("}");
            return 0;
        }

        output.WriteLine($"TypeSharp CLI {TypeSharpCompilerInfo.CliVersion}");
        output.WriteLine($"Compiler {TypeSharpCompilerInfo.CompilerVersion}");
        output.WriteLine($"Language {TypeSharpCompilerInfo.LanguageVersion}");
        output.WriteLine($"Runtime ABI {TypeSharpCompilerInfo.RuntimeAbiVersion}");
        output.WriteLine($"Target default {TypeSharpCompilerInfo.DefaultTargetFramework}");
        return 0;
    }

    private static int RunHelp(TextWriter output)
    {
        output.WriteLine("Usage: typesharp <command> [options]");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine("  version [--json]");
        output.WriteLine("  check [project] [options]");
        output.WriteLine("  build [project] [options]");
        output.WriteLine("  run [project] [-- args...]");
        return 0;
    }

    private static int RunProjectCommandPlaceholder(string command, string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseProjectCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        var manifestPath = TypeSharpManifestLocator.Locate(parseResult.ProjectArgument, Directory.GetCurrentDirectory());
        if (manifestPath.HasErrors)
        {
            WriteDiagnostics(error, manifestPath.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        output.WriteLine($"TypeSharp {command} manifest: {manifestPath.ManifestPath}");
        output.WriteLine($"TypeSharp {command} is not implemented yet.");
        return 5;
    }

    private static int RunCheck(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseProjectCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        var manifestPath = TypeSharpManifestLocator.Locate(parseResult.ProjectArgument, Directory.GetCurrentDirectory());
        if (manifestPath.HasErrors || manifestPath.ManifestPath is null)
        {
            WriteDiagnostics(error, manifestPath.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        var checkResult = TypeSharpChecker.Check(manifestPath.ManifestPath);
        var writer = checkResult.HasErrors ? error : output;
        WriteDiagnostics(writer, checkResult.Diagnostics, parseResult.DiagnosticFormat);
        return checkResult.HasErrors ? 1 : 0;
    }

    private static int RunBuild(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseProjectCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        if (parseResult.Emit != "csharp")
        {
            error.WriteLine("Only '--emit csharp' is supported by the current build skeleton.");
            return 2;
        }

        var manifestPath = TypeSharpManifestLocator.Locate(parseResult.ProjectArgument, Directory.GetCurrentDirectory());
        if (manifestPath.HasErrors || manifestPath.ManifestPath is null)
        {
            WriteDiagnostics(error, manifestPath.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        var buildResult = TypeSharpBuilder.Build(manifestPath.ManifestPath);
        if (buildResult.HasErrors)
        {
            WriteDiagnostics(error, buildResult.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        foreach (var generatedFile in buildResult.GeneratedFiles)
        {
            output.WriteLine($"Generated C# source: {generatedFile.RelativePath}");
        }

        if (buildResult.GeneratedProject is not null)
        {
            output.WriteLine($"Generated C# project: {buildResult.GeneratedProject.RelativePath}");
        }

        if (buildResult.GeneratedAssembly is not null)
        {
            output.WriteLine($"Generated assembly: {buildResult.GeneratedAssembly.RelativePath}");
        }

        return 0;
    }

    private static ProjectCommandParseResult ParseProjectCommand(string[] args)
    {
        string? projectArgument = null;
        var diagnosticFormat = "text";
        var emit = "csharp";

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];

            if (arg == "--")
            {
                break;
            }

            if (arg == "--project")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --project.");
                }

                projectArgument = args[index];
                continue;
            }

            if (arg == "--diagnostic-format")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --diagnostic-format.");
                }

                diagnosticFormat = args[index];
                continue;
            }

            if (arg == "--emit")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --emit.");
                }

                emit = args[index];
                continue;
            }

            if (arg.StartsWith("--diagnostic-format=", StringComparison.Ordinal))
            {
                diagnosticFormat = arg["--diagnostic-format=".Length..];
                continue;
            }

            if (arg.StartsWith("--emit=", StringComparison.Ordinal))
            {
                emit = arg["--emit=".Length..];
                continue;
            }

            if (OptionConsumesValue(arg))
            {
                index++;
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (projectArgument is not null)
            {
                return ProjectCommandParseResult.Usage("Only one project path can be specified.");
            }

            projectArgument = arg;
        }

        return new ProjectCommandParseResult(projectArgument, diagnosticFormat, emit, null, null);
    }

    private static bool OptionConsumesValue(string option) =>
        option is "--configuration" or "--target" or "--verbosity";

    private static void WriteDiagnostics(TextWriter writer, IReadOnlyList<Diagnostic> diagnostics, string format)
    {
        if (format == "json")
        {
            writer.Write(DiagnosticJsonFormatter.ToJson(diagnostics));
            return;
        }

        foreach (var diagnostic in diagnostics)
        {
            writer.WriteLine(diagnostic.ToCliText());
        }
    }

    private static int RunUnknownCommand(string command, TextWriter error)
    {
        error.WriteLine($"Unknown command '{command}'.");
        WriteUsage(error);
        return 2;
    }

    private static void WriteUsage(TextWriter writer)
    {
        writer.WriteLine("Usage: typesharp <command> [options]");
    }

    private sealed record ProjectCommandParseResult(
        string? ProjectArgument,
        string DiagnosticFormat,
        string Emit,
        int? ExitCode,
        string? Message)
    {
        public static ProjectCommandParseResult Usage(string message) => new(null, "text", "csharp", 2, message);
    }
}
