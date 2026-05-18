using TypeSharp.Compiler;
using TypeSharp.Compiler.Building;
using TypeSharp.Compiler.Checking;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Projects;
using System.Diagnostics;
using System.Text;

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
            "run" => RunRun(args, output, error),
            "explain" => RunExplain(args, output, error),
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
        output.WriteLine("  explain <diagnostic-code> [--json]");
        return 0;
    }

    private static int RunExplain(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseExplainCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        if (!DiagnosticDescriptors.TryGetByCode(parseResult.Code ?? string.Empty, out var descriptor))
        {
            error.WriteLine($"Unknown diagnostic code '{parseResult.Code}'.");
            return 1;
        }

        if (parseResult.Json)
        {
            output.Write(FormatDescriptorJson(descriptor));
            return 0;
        }

        output.WriteLine($"{descriptor.Code}: {descriptor.Title}");
        output.WriteLine($"Severity: {descriptor.DefaultSeverity.ToString().ToLowerInvariant()}");
        output.WriteLine($"Category: {descriptor.Category}");
        output.WriteLine($"Message: {descriptor.MessageTemplate}");
        output.WriteLine($"Explanation: {descriptor.Explanation}");
        output.WriteLine($"Suggested action: {descriptor.SuggestedAction}");
        return 0;
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

    private static int RunRun(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseProjectCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        if (parseResult.Emit != "csharp")
        {
            error.WriteLine("Only '--emit csharp' is supported by the current run skeleton.");
            return 2;
        }

        var manifestPath = TypeSharpManifestLocator.Locate(parseResult.ProjectArgument, Directory.GetCurrentDirectory());
        if (manifestPath.HasErrors || manifestPath.ManifestPath is null)
        {
            WriteDiagnostics(error, manifestPath.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        var manifestResult = TypeSharpManifestLoader.Load(manifestPath.ManifestPath);
        if (manifestResult.HasErrors || manifestResult.Manifest is null)
        {
            WriteDiagnostics(error, manifestResult.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        if (!string.Equals(manifestResult.Manifest.Project.OutputType, "exe", StringComparison.OrdinalIgnoreCase))
        {
            error.WriteLine("typesharp run requires project outputType = \"exe\".");
            return 5;
        }

        var buildResult = TypeSharpBuilder.Build(manifestPath.ManifestPath);
        if (buildResult.HasErrors)
        {
            WriteDiagnostics(error, buildResult.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        if (buildResult.GeneratedAssembly is null)
        {
            error.WriteLine("typesharp run could not find a generated executable.");
            return 1;
        }

        var runResult = RunGeneratedExecutable(buildResult.GeneratedAssembly.Path, parseResult.ProgramArguments);
        output.Write(runResult.StandardOutput);
        error.Write(runResult.StandardError);
        return runResult.ExitCode;
    }

    private static ProjectCommandParseResult ParseProjectCommand(string[] args)
    {
        string? projectArgument = null;
        var diagnosticFormat = "text";
        var emit = "csharp";
        var programArguments = new List<string>();

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];

            if (arg == "--")
            {
                for (var programArgIndex = index + 1; programArgIndex < args.Length; programArgIndex++)
                {
                    programArguments.Add(args[programArgIndex]);
                }

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

        return new ProjectCommandParseResult(projectArgument, diagnosticFormat, emit, programArguments, null, null);
    }

    private static ExplainCommandParseResult ParseExplainCommand(string[] args)
    {
        string? code = null;
        var json = false;

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg == "--json")
            {
                json = true;
                continue;
            }

            if (arg == "--diagnostic-format")
            {
                if (++index >= args.Length)
                {
                    return ExplainCommandParseResult.Usage("Missing value for --diagnostic-format.");
                }

                if (args[index] == "json")
                {
                    json = true;
                    continue;
                }

                if (args[index] == "text")
                {
                    continue;
                }

                return ExplainCommandParseResult.Usage("Diagnostic format must be 'text' or 'json'.");
            }

            if (arg.StartsWith("--diagnostic-format=", StringComparison.Ordinal))
            {
                var value = arg["--diagnostic-format=".Length..];
                if (value == "json")
                {
                    json = true;
                    continue;
                }

                if (value == "text")
                {
                    continue;
                }

                return ExplainCommandParseResult.Usage("Diagnostic format must be 'text' or 'json'.");
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                return ExplainCommandParseResult.Usage($"Unknown option '{arg}'.");
            }

            if (code is not null)
            {
                return ExplainCommandParseResult.Usage("Only one diagnostic code can be specified.");
            }

            code = arg;
        }

        return string.IsNullOrWhiteSpace(code)
            ? ExplainCommandParseResult.Usage("Usage: typesharp explain <diagnostic-code> [--json]")
            : new ExplainCommandParseResult(code, json, null, null);
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

    private static string FormatDescriptorJson(DiagnosticDescriptor descriptor)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"diagnostic\": {");
        builder.AppendLine($"    \"code\": \"{EscapeJson(descriptor.Code)}\",");
        builder.AppendLine($"    \"title\": \"{EscapeJson(descriptor.Title)}\",");
        builder.AppendLine($"    \"severity\": \"{descriptor.DefaultSeverity.ToString().ToLowerInvariant()}\",");
        builder.AppendLine($"    \"category\": \"{EscapeJson(descriptor.Category.ToString())}\",");
        builder.AppendLine($"    \"messageTemplate\": \"{EscapeJson(descriptor.MessageTemplate)}\",");
        builder.AppendLine($"    \"explanation\": \"{EscapeJson(descriptor.Explanation)}\",");
        builder.AppendLine($"    \"suggestedAction\": \"{EscapeJson(descriptor.SuggestedAction)}\"");
        builder.AppendLine("  }");
        builder.AppendLine("}");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string EscapeJson(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private static GeneratedProgramRunResult RunGeneratedExecutable(
        string executablePath,
        IReadOnlyList<string> programArguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in programArguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        try
        {
            process.Start();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            return new GeneratedProgramRunResult(4, string.Empty, $"Could not run generated executable: {ex.Message}{Environment.NewLine}");
        }

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(milliseconds: 120_000))
        {
            process.Kill(entireProcessTree: true);
            return new GeneratedProgramRunResult(4, standardOutput, standardError + "Generated executable timed out." + Environment.NewLine);
        }

        return new GeneratedProgramRunResult(process.ExitCode, standardOutput, standardError);
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
        IReadOnlyList<string> ProgramArguments,
        int? ExitCode,
        string? Message)
    {
        public static ProjectCommandParseResult Usage(string message) => new(null, "text", "csharp", [], 2, message);
    }

    private sealed record ExplainCommandParseResult(
        string? Code,
        bool Json,
        int? ExitCode,
        string? Message)
    {
        public static ExplainCommandParseResult Usage(string message) => new(null, false, 2, message);
    }

    private sealed record GeneratedProgramRunResult(int ExitCode, string StandardOutput, string StandardError);
}
