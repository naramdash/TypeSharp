using TypeSharp.Compiler;
using TypeSharp.Compiler.Building;
using TypeSharp.Compiler.Checking;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;
using TypeSharp.LanguageServer;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace TypeSharp.Cli;

public static class TypeSharpCli
{
    public static int Run(string[] args, TextWriter output, TextWriter error) =>
        Run(args, output, error, input: null, protocolOutput: null, Directory.GetCurrentDirectory());

    public static int Run(
        string[] args,
        TextWriter output,
        TextWriter error,
        Stream? input,
        Stream? protocolOutput,
        string workspaceRoot)
    {
        if (args.Length == 0)
        {
            WriteUsage(error);
            return 2;
        }

        return args[0] switch
        {
            "version" => RunVersion(args, output, error),
            "new" => RunNew(args, output, error),
            "check" => RunCheck(args, output, error),
            "build" => RunBuild(args, output, error),
            "run" => RunRun(args, output, error),
            "format" => RunFormat(args, output, error),
            "explain" => RunExplain(args, output, error),
            "runtime-path" => RunRuntimePath(args, output, error),
            "lsp" => RunLsp(args, error, input, protocolOutput, workspaceRoot),
            "--help" or "-h" or "help" => RunHelp(output),
            _ => RunUnknownCommand(args[0], error)
        };
    }

    private static int RunVersion(string[] args, TextWriter output, TextWriter error)
    {
        var json = false;
        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg == "--json")
            {
                json = true;
                continue;
            }

            if (arg == "--no-color")
            {
                continue;
            }

            error.WriteLine("Usage: typesharp version [--json] [--no-color]");
            return 2;
        }

        if (json)
        {
            output.WriteLine("{");
            output.WriteLine($"  \"cli\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.CliVersion)},");
            output.WriteLine($"  \"compiler\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.CompilerVersion)},");
            output.WriteLine($"  \"language\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.LanguageVersion)},");
            output.WriteLine($"  \"releaseChannel\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.ReleaseChannel)},");
            output.WriteLine($"  \"runtimeAbi\": {TypeSharpCompilerInfo.RuntimeAbiVersion},");
            output.WriteLine($"  \"runtimeAbiStatus\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.RuntimeAbiStatus)},");
            output.WriteLine($"  \"targetDefault\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.DefaultTargetFramework)},");
            output.WriteLine($"  \"cliTargetFramework\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.CliTargetFramework)},");
            output.WriteLine($"  \"runtimeTargetFramework\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.RuntimeTargetFramework)},");
            output.WriteLine($"  \"artifactKind\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.ArtifactKind)},");
            output.WriteLine($"  \"buildMetadata\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.BuildMetadata)},");
            output.WriteLine($"  \"sourceRevision\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.SourceRevision)}");
            output.WriteLine("}");
            return 0;
        }

        output.WriteLine($"TypeSharp CLI {TypeSharpCompilerInfo.CliVersion}");
        output.WriteLine($"Compiler {TypeSharpCompilerInfo.CompilerVersion}");
        output.WriteLine($"Language {TypeSharpCompilerInfo.LanguageVersion}");
        output.WriteLine($"Release channel {TypeSharpCompilerInfo.ReleaseChannel}");
        output.WriteLine($"Runtime ABI {TypeSharpCompilerInfo.RuntimeAbiVersion}");
        output.WriteLine($"Runtime ABI status {TypeSharpCompilerInfo.RuntimeAbiStatus}");
        output.WriteLine($"Target default {TypeSharpCompilerInfo.DefaultTargetFramework}");
        output.WriteLine($"CLI target {TypeSharpCompilerInfo.CliTargetFramework}");
        output.WriteLine($"Runtime target {TypeSharpCompilerInfo.RuntimeTargetFramework}");
        output.WriteLine($"Artifact kind {TypeSharpCompilerInfo.ArtifactKind}");
        output.WriteLine($"Build metadata {TypeSharpCompilerInfo.BuildMetadata}");
        output.WriteLine($"Source revision {TypeSharpCompilerInfo.SourceRevision}");
        return 0;
    }

    private static int RunHelp(TextWriter output)
    {
        output.WriteLine("Usage: typesharp <command> [options]");
        output.WriteLine();
        output.WriteLine("Commands:");
        output.WriteLine("  version [--json]");
        output.WriteLine("  new <console|library> <name> [--target net48] [--output <path>]");
        output.WriteLine("  check [project] [options]");
        output.WriteLine("  build [project] [options]");
        output.WriteLine("  run [project] [-- args...]");
        output.WriteLine("  format [project-or-file] [--check]");
        output.WriteLine("  explain <diagnostic-code> [--json]");
        output.WriteLine("  runtime-path [--json]");
        output.WriteLine("  lsp");
        return 0;
    }

    private static int RunRuntimePath(string[] args, TextWriter output, TextWriter error)
    {
        var json = false;
        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg == "--json")
            {
                json = true;
                continue;
            }

            if (arg == "--no-color")
            {
                continue;
            }

            error.WriteLine("Usage: typesharp runtime-path [--json] [--no-color]");
            return 2;
        }

        var runtimeRoot = Path.Combine(AppContext.BaseDirectory, "runtime", "net48");
        var corePath = Path.Combine(runtimeRoot, "TypeSharp.Core.dll");
        var runtimePath = Path.Combine(runtimeRoot, "TypeSharp.Runtime.dll");

        if (json)
        {
            output.WriteLine("{");
            output.WriteLine($"  \"targetFramework\": {JsonSerializer.Serialize(TypeSharpCompilerInfo.RuntimeTargetFramework)},");
            output.WriteLine($"  \"root\": {JsonSerializer.Serialize(runtimeRoot)},");
            output.WriteLine($"  \"core\": {JsonSerializer.Serialize(corePath)},");
            output.WriteLine($"  \"runtime\": {JsonSerializer.Serialize(runtimePath)}");
            output.WriteLine("}");
            return 0;
        }

        output.WriteLine($"Runtime target {TypeSharpCompilerInfo.RuntimeTargetFramework}");
        output.WriteLine($"Runtime root {runtimeRoot}");
        output.WriteLine($"TypeSharp.Core {corePath}");
        output.WriteLine($"TypeSharp.Runtime {runtimePath}");
        return 0;
    }

    private static int RunLsp(string[] args, TextWriter error, Stream? input, Stream? protocolOutput, string workspaceRoot)
    {
        for (var index = 1; index < args.Length; index++)
        {
            if (args[index] == "--no-color")
            {
                continue;
            }

            error.WriteLine("Usage: typesharp lsp [--no-color]");
            return 2;
        }

        if (input is null || protocolOutput is null)
        {
            error.WriteLine("typesharp lsp requires standard input and output streams.");
            return 2;
        }

        TypeSharpLanguageServer.Run(input, protocolOutput, workspaceRoot);
        return 0;
    }

    private static int RunNew(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseNewCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        var projectDirectory = Path.GetFullPath(parseResult.OutputDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), parseResult.Name ?? string.Empty));
        if (Directory.Exists(projectDirectory) && Directory.EnumerateFileSystemEntries(projectDirectory).Any())
        {
            error.WriteLine($"Project directory '{projectDirectory}' is not empty.");
            return 1;
        }

        Directory.CreateDirectory(Path.Combine(projectDirectory, "src"));

        var projectName = parseResult.Name ?? "TypeSharpApp";
        var rootNamespace = ToRootNamespace(projectName);
        var isConsole = string.Equals(parseResult.Template, "console", StringComparison.OrdinalIgnoreCase);
        var sourceFileName = isConsole ? "Main.tysh" : "Library.tysh";

        File.WriteAllText(
            Path.Combine(projectDirectory, "TypeSharp.toml"),
            NewManifest(projectName, rootNamespace, isConsole),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.WriteAllText(
            Path.Combine(projectDirectory, "src", sourceFileName),
            NewSource(rootNamespace, isConsole),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.WriteAllText(
            Path.Combine(projectDirectory, ".gitignore"),
            "bin/\nobj/\ngenerated/\n.vs/\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.WriteAllText(
            Path.Combine(projectDirectory, "README.md"),
            NewReadme(projectName, isConsole),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        output.WriteLine($"Created TypeSharp {parseResult.Template} project at {projectDirectory}");
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

        var warningsAsErrors = ResolveWarningsAsErrors(manifestPath.ManifestPath, parseResult.WarningsAsErrors);
        var checkResult = TypeSharpChecker.Check(manifestPath.ManifestPath);
        var hasBlockingDiagnostics = HasBlockingDiagnostics(checkResult.Diagnostics, warningsAsErrors);
        var writer = hasBlockingDiagnostics ? error : output;
        WriteDiagnostics(writer, checkResult.Diagnostics, parseResult.DiagnosticFormat);
        return hasBlockingDiagnostics ? 1 : 0;
    }

    private static int RunFormat(string[] args, TextWriter output, TextWriter error)
    {
        var parseResult = ParseFormatCommand(args);
        if (parseResult.ExitCode is not null)
        {
            error.WriteLine(parseResult.Message);
            return parseResult.ExitCode.Value;
        }

        var sourceFilesResult = ResolveFormatSourceFiles(parseResult.TargetArgument);
        if (sourceFilesResult.ExitCode is not null)
        {
            error.WriteLine(sourceFilesResult.Message);
            return sourceFilesResult.ExitCode.Value;
        }

        if (sourceFilesResult.Diagnostics.Count > 0)
        {
            WriteDiagnostics(error, sourceFilesResult.Diagnostics, parseResult.DiagnosticFormat);
            return sourceFilesResult.HasErrors ? 1 : 0;
        }

        var changedFiles = new List<string>();
        foreach (var sourceFile in sourceFilesResult.SourceFiles)
        {
            var text = File.ReadAllText(sourceFile.Path);
            var parse = TypeSharpParser.ParseText(text, sourceFile.RelativePath);
            if (parse.HasErrors)
            {
                WriteDiagnostics(error, parse.Diagnostics, parseResult.DiagnosticFormat);
                return 1;
            }

            var formatted = FormatSourceText(text);
            if (string.Equals(text.Replace("\r\n", "\n", StringComparison.Ordinal), formatted, StringComparison.Ordinal))
            {
                continue;
            }

            changedFiles.Add(sourceFile.RelativePath);
            if (!parseResult.Check)
            {
                File.WriteAllText(sourceFile.Path, formatted, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
        }

        if (parseResult.Check && changedFiles.Count > 0)
        {
            foreach (var relativePath in changedFiles)
            {
                output.WriteLine($"Needs formatting: {relativePath}");
            }

            return 1;
        }

        if (parseResult.Check)
        {
            output.WriteLine("All TypeSharp files are formatted.");
            return 0;
        }

        foreach (var relativePath in changedFiles)
        {
            output.WriteLine($"Formatted: {relativePath}");
        }

        if (changedFiles.Count == 0)
        {
            output.WriteLine("All TypeSharp files are formatted.");
        }

        return 0;
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

        var warningsAsErrors = ResolveWarningsAsErrors(manifestPath.ManifestPath, parseResult.WarningsAsErrors);
        if (warningsAsErrors)
        {
            var checkResult = TypeSharpChecker.Check(manifestPath.ManifestPath);
            if (HasBlockingDiagnostics(checkResult.Diagnostics, warningsAsErrors))
            {
                WriteDiagnostics(error, checkResult.Diagnostics, parseResult.DiagnosticFormat);
                return 1;
            }
        }

        var buildResult = TypeSharpBuilder.Build(manifestPath.ManifestPath, parseResult.Configuration, parseResult.TargetFramework);
        if (buildResult.HasErrors)
        {
            WriteDiagnostics(error, buildResult.Diagnostics, parseResult.DiagnosticFormat);
            return 1;
        }

        if (parseResult.Verbosity == "quiet")
        {
            return 0;
        }

        if (parseResult.Verbosity == "diagnostic")
        {
            output.WriteLine($"Build configuration: {parseResult.Configuration}");
            output.WriteLine($"Target framework override: {parseResult.TargetFramework ?? "<manifest>"}");
        }

        if (parseResult.Verbosity is "normal" or "diagnostic")
        {
            foreach (var generatedFile in buildResult.GeneratedFiles)
            {
                output.WriteLine($"Generated C# source: {generatedFile.RelativePath}");
            }

            if (buildResult.GeneratedProject is not null)
            {
                output.WriteLine($"Generated C# project: {buildResult.GeneratedProject.RelativePath}");
            }
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

        var warningsAsErrors = parseResult.WarningsAsErrors || manifestResult.Manifest.Tooling.TreatWarningsAsErrors;
        if (warningsAsErrors)
        {
            var checkResult = TypeSharpChecker.Check(manifestPath.ManifestPath);
            if (HasBlockingDiagnostics(checkResult.Diagnostics, warningsAsErrors))
            {
                WriteDiagnostics(error, checkResult.Diagnostics, parseResult.DiagnosticFormat);
                return 1;
            }
        }

        var buildResult = TypeSharpBuilder.Build(manifestPath.ManifestPath, parseResult.Configuration, parseResult.TargetFramework);
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
        var configuration = "Debug";
        string? targetFramework = null;
        var verbosity = "normal";
        var warningsAsErrors = false;
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

            if (arg == "--configuration")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --configuration.");
                }

                configuration = args[index];
                continue;
            }

            if (arg == "--target")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --target.");
                }

                targetFramework = args[index];
                continue;
            }

            if (arg == "--verbosity")
            {
                if (++index >= args.Length)
                {
                    return ProjectCommandParseResult.Usage("Missing value for --verbosity.");
                }

                verbosity = args[index];
                continue;
            }

            if (arg == "--warnings-as-errors")
            {
                warningsAsErrors = true;
                continue;
            }

            if (arg == "--no-color")
            {
                continue;
            }

            if (arg == "--preview")
            {
                continue;
            }

            if (arg.StartsWith("--diagnostic-format=", StringComparison.Ordinal))
            {
                diagnosticFormat = arg["--diagnostic-format=".Length..];
                continue;
            }

            if (arg.StartsWith("--configuration=", StringComparison.Ordinal))
            {
                configuration = arg["--configuration=".Length..];
                continue;
            }

            if (arg.StartsWith("--target=", StringComparison.Ordinal))
            {
                targetFramework = arg["--target=".Length..];
                continue;
            }

            if (arg.StartsWith("--verbosity=", StringComparison.Ordinal))
            {
                verbosity = arg["--verbosity=".Length..];
                continue;
            }

            if (arg.StartsWith("--emit=", StringComparison.Ordinal))
            {
                emit = arg["--emit=".Length..];
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                return ProjectCommandParseResult.Usage($"Unknown option '{arg}'.");
            }

            if (projectArgument is not null)
            {
                return ProjectCommandParseResult.Usage("Only one project path can be specified.");
            }

            projectArgument = arg;
        }

        if (diagnosticFormat is not "text" and not "json")
        {
            return ProjectCommandParseResult.Usage("Diagnostic format must be 'text' or 'json'.");
        }

        configuration = NormalizeBuildConfiguration(configuration);
        if (configuration.Length == 0)
        {
            return ProjectCommandParseResult.Usage("Configuration must be 'Debug' or 'Release'.");
        }

        targetFramework = NormalizeTargetFrameworkOverride(targetFramework);
        if (targetFramework is not null && targetFramework.Length == 0)
        {
            return ProjectCommandParseResult.Usage("Target framework must be 'net48'.");
        }

        verbosity = NormalizeVerbosity(verbosity);
        if (verbosity.Length == 0)
        {
            return ProjectCommandParseResult.Usage("Verbosity must be 'quiet', 'minimal', 'normal', or 'diagnostic'.");
        }

        return new ProjectCommandParseResult(projectArgument, diagnosticFormat, emit, configuration, targetFramework, verbosity, warningsAsErrors, programArguments, null, null);
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

            if (arg == "--no-color")
            {
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

    private static NewCommandParseResult ParseNewCommand(string[] args)
    {
        string? template = null;
        string? name = null;
        string? outputDirectory = null;
        var target = "net48";

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg == "--target")
            {
                if (++index >= args.Length)
                {
                    return NewCommandParseResult.Usage("Missing value for --target.");
                }

                target = args[index];
                continue;
            }

            if (arg.StartsWith("--target=", StringComparison.Ordinal))
            {
                target = arg["--target=".Length..];
                continue;
            }

            if (arg == "--no-color")
            {
                continue;
            }

            if (arg == "--output")
            {
                if (++index >= args.Length)
                {
                    return NewCommandParseResult.Usage("Missing value for --output.");
                }

                outputDirectory = args[index];
                continue;
            }

            if (arg.StartsWith("--output=", StringComparison.Ordinal))
            {
                outputDirectory = arg["--output=".Length..];
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                return NewCommandParseResult.Usage($"Unknown option '{arg}'.");
            }

            if (template is null)
            {
                template = arg;
                continue;
            }

            if (name is null)
            {
                name = arg;
                continue;
            }

            return NewCommandParseResult.Usage("Only one project name can be specified.");
        }

        if (!string.Equals(template, "console", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(template, "library", StringComparison.OrdinalIgnoreCase))
        {
            return NewCommandParseResult.Usage("Template must be 'console' or 'library'.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return NewCommandParseResult.Usage("Usage: typesharp new <console|library> <name> [--target net48] [--output <path>]");
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return NewCommandParseResult.Usage($"Project name '{name}' contains invalid file name characters.");
        }

        if (!string.Equals(target, "net48", StringComparison.OrdinalIgnoreCase))
        {
            return new NewCommandParseResult(template, name, outputDirectory, 5, "Only '--target net48' is supported by the current project templates.");
        }

        return new NewCommandParseResult(template, name, outputDirectory, null, null);
    }

    private static FormatCommandParseResult ParseFormatCommand(string[] args)
    {
        string? targetArgument = null;
        var check = false;
        var diagnosticFormat = "text";

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg == "--check")
            {
                check = true;
                continue;
            }

            if (arg == "--no-color")
            {
                continue;
            }

            if (arg == "--project")
            {
                if (++index >= args.Length)
                {
                    return FormatCommandParseResult.Usage("Missing value for --project.");
                }

                targetArgument = args[index];
                continue;
            }

            if (arg == "--diagnostic-format")
            {
                if (++index >= args.Length)
                {
                    return FormatCommandParseResult.Usage("Missing value for --diagnostic-format.");
                }

                diagnosticFormat = args[index];
                continue;
            }

            if (arg.StartsWith("--diagnostic-format=", StringComparison.Ordinal))
            {
                diagnosticFormat = arg["--diagnostic-format=".Length..];
                continue;
            }

            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                return FormatCommandParseResult.Usage($"Unknown option '{arg}'.");
            }

            if (targetArgument is not null)
            {
                return FormatCommandParseResult.Usage("Only one project or file path can be specified.");
            }

            targetArgument = arg;
        }

        if (diagnosticFormat is not "text" and not "json")
        {
            return FormatCommandParseResult.Usage("Diagnostic format must be 'text' or 'json'.");
        }

        return new FormatCommandParseResult(targetArgument, check, diagnosticFormat, null, null);
    }

    private static FormatSourceFilesResult ResolveFormatSourceFiles(string? targetArgument)
    {
        if (!string.IsNullOrWhiteSpace(targetArgument)
            && string.Equals(Path.GetExtension(targetArgument), ".tysh", StringComparison.OrdinalIgnoreCase))
        {
            var fullPath = Path.GetFullPath(targetArgument);
            if (!File.Exists(fullPath))
            {
                return FormatSourceFilesResult.Usage($"Source file '{targetArgument}' does not exist.");
            }

            return new FormatSourceFilesResult(
                [new SourceFile(fullPath, Path.GetFileName(fullPath))],
                [],
                null,
                null);
        }

        var manifestPath = TypeSharpManifestLocator.Locate(targetArgument, Directory.GetCurrentDirectory());
        if (manifestPath.HasErrors || manifestPath.ManifestPath is null)
        {
            return new FormatSourceFilesResult([], manifestPath.Diagnostics, null, null);
        }

        var manifestResult = TypeSharpManifestLoader.Load(manifestPath.ManifestPath);
        if (manifestResult.HasErrors || manifestResult.Manifest is null)
        {
            return new FormatSourceFilesResult([], manifestResult.Diagnostics, null, null);
        }

        var discovery = SourceDiscovery.Discover(manifestResult.Manifest);
        return new FormatSourceFilesResult(discovery.SourceFiles, discovery.Diagnostics, null, null);
    }

    private static string FormatSourceText(string text)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var formattedLines = new List<string>();
        var blankLineCount = 0;

        for (var index = 0; index < lines.Length; index++)
        {
            if (index == lines.Length - 1 && lines[index].Length == 0)
            {
                continue;
            }

            var line = lines[index].TrimEnd(' ', '\t');
            if (line.Length == 0)
            {
                blankLineCount++;
                if (blankLineCount <= 1 && formattedLines.Count > 0)
                {
                    formattedLines.Add(string.Empty);
                }

                continue;
            }

            blankLineCount = 0;
            formattedLines.Add(line);
        }

        while (formattedLines.Count > 0 && formattedLines[^1].Length == 0)
        {
            formattedLines.RemoveAt(formattedLines.Count - 1);
        }

        return string.Join("\n", formattedLines) + "\n";
    }

    private static string NewManifest(string projectName, string rootNamespace, bool isConsole) =>
        $"""
        [project]
        name = "{EscapeToml(projectName)}"
        targetFramework = "net48"
        outputType = "{(isConsole ? "exe" : "library")}"
        rootNamespace = "{EscapeToml(rootNamespace)}"
        sourceRoots = ["src"]
        generatedOutputRoot = "generated"
        {(isConsole ? $"main = \"{EscapeToml(rootNamespace)}.main\"" : string.Empty)}

        [language]
        version = "preview"
        strict = true
        nullable = "strict"
        previewFeatures = []

        [references]
        assemblies = [
          "System",
          "System.Core"
        ]
        paths = []
        packages = []

        [tooling]
        diagnosticFormat = "text"
        treatWarningsAsErrors = false
        """.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string NewSource(string rootNamespace, bool isConsole) =>
        (isConsole
            ? $"""
              namespace {rootNamespace}

              export fun main(): string = "Hello, TypeSharp"
              """.Replace("\r\n", "\n", StringComparison.Ordinal)
            : $"""
              namespace {rootNamespace}

              export fun greeting(name: string): string = name
              """.Replace("\r\n", "\n", StringComparison.Ordinal)) + "\n";

    private static string NewReadme(string projectName, bool isConsole) =>
        isConsole
            ? $"""
              # {projectName}

              This TypeSharp console project targets `net48` and generates a runnable .NET Framework executable.
              These commands assume the release-installed `typesharp` command from https://naramdash.github.io/TypeSharp/install/ is on `PATH`.

              ```powershell
              typesharp check TypeSharp.toml
              typesharp build TypeSharp.toml
              typesharp run TypeSharp.toml
              ```

              Generated C# source and build outputs are written under `generated/`.
              Use https://naramdash.github.io/TypeSharp/troubleshooting/ for host, dependency, build, or executable launch failures.
              """.Replace("\r\n", "\n", StringComparison.Ordinal)
            : $"""
              # {projectName}

              This TypeSharp library project targets `net48` and generates a .NET Framework class library.
              These commands assume the release-installed `typesharp` command from https://naramdash.github.io/TypeSharp/install/ is on `PATH`.

              ```powershell
              typesharp check TypeSharp.toml
              typesharp build TypeSharp.toml
              ```

              Generated C# source and build outputs are written under `generated/`.
              Use https://naramdash.github.io/TypeSharp/runtime-artifacts/ before referencing TypeSharp Core/Runtime DLLs from C# consumers.
              """.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string ToRootNamespace(string projectName)
    {
        var segments = projectName
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ToIdentifierSegment)
            .Where(segment => segment.Length > 0)
            .ToArray();

        return segments.Length == 0 ? "TypeSharpApp" : string.Join(".", segments);
    }

    private static string ToIdentifierSegment(string segment)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < segment.Length; index++)
        {
            var ch = segment[index];
            var valid = index == 0
                ? IsIdentifierStart(ch)
                : IsIdentifierPart(ch);
            builder.Append(valid ? ch : '_');
        }

        if (builder.Length == 0)
        {
            return string.Empty;
        }

        if (!IsIdentifierStart(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }

    private static bool IsIdentifierStart(char ch) =>
        ch is '_' || ch is >= 'A' and <= 'Z' || ch is >= 'a' and <= 'z';

    private static bool IsIdentifierPart(char ch) =>
        IsIdentifierStart(ch) || ch is >= '0' and <= '9';

    private static string EscapeToml(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static bool ResolveWarningsAsErrors(string manifestPath, bool commandLineWarningsAsErrors)
    {
        if (commandLineWarningsAsErrors)
        {
            return true;
        }

        var manifestResult = TypeSharpManifestLoader.Load(manifestPath);
        return manifestResult.Manifest?.Tooling.TreatWarningsAsErrors == true;
    }

    private static bool HasBlockingDiagnostics(IReadOnlyList<Diagnostic> diagnostics, bool warningsAsErrors) =>
        diagnostics.Any(diagnostic =>
            diagnostic.Severity == DiagnosticSeverity.Error
            || warningsAsErrors && diagnostic.Severity == DiagnosticSeverity.Warning);

    private static string NormalizeBuildConfiguration(string configuration)
    {
        if (string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase))
        {
            return "Debug";
        }

        if (string.Equals(configuration, "Release", StringComparison.OrdinalIgnoreCase))
        {
            return "Release";
        }

        return string.Empty;
    }

    private static string? NormalizeTargetFrameworkOverride(string? targetFramework)
    {
        if (targetFramework is null)
        {
            return null;
        }

        if (string.Equals(targetFramework, TypeSharpCompilerInfo.DefaultTargetFramework, StringComparison.OrdinalIgnoreCase))
        {
            return TypeSharpCompilerInfo.DefaultTargetFramework;
        }

        return string.Empty;
    }

    private static string NormalizeVerbosity(string verbosity)
    {
        if (string.Equals(verbosity, "quiet", StringComparison.OrdinalIgnoreCase))
        {
            return "quiet";
        }

        if (string.Equals(verbosity, "minimal", StringComparison.OrdinalIgnoreCase))
        {
            return "minimal";
        }

        if (string.Equals(verbosity, "normal", StringComparison.OrdinalIgnoreCase))
        {
            return "normal";
        }

        if (string.Equals(verbosity, "diagnostic", StringComparison.OrdinalIgnoreCase))
        {
            return "diagnostic";
        }

        return string.Empty;
    }

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
        string Configuration,
        string? TargetFramework,
        string Verbosity,
        bool WarningsAsErrors,
        IReadOnlyList<string> ProgramArguments,
        int? ExitCode,
        string? Message)
    {
        public static ProjectCommandParseResult Usage(string message) => new(null, "text", "csharp", "Debug", null, "normal", false, [], 2, message);
    }

    private sealed record NewCommandParseResult(
        string? Template,
        string? Name,
        string? OutputDirectory,
        int? ExitCode,
        string? Message)
    {
        public static NewCommandParseResult Usage(string message) => new(null, null, null, 2, message);
    }

    private sealed record ExplainCommandParseResult(
        string? Code,
        bool Json,
        int? ExitCode,
        string? Message)
    {
        public static ExplainCommandParseResult Usage(string message) => new(null, false, 2, message);
    }

    private sealed record FormatCommandParseResult(
        string? TargetArgument,
        bool Check,
        string DiagnosticFormat,
        int? ExitCode,
        string? Message)
    {
        public static FormatCommandParseResult Usage(string message) => new(null, false, "text", 2, message);
    }

    private sealed record FormatSourceFilesResult(
        IReadOnlyList<SourceFile> SourceFiles,
        IReadOnlyList<Diagnostic> Diagnostics,
        int? ExitCode,
        string? Message)
    {
        public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        public static FormatSourceFilesResult Usage(string message) => new([], [], 2, message);
    }

    private sealed record GeneratedProgramRunResult(int ExitCode, string StandardOutput, string StandardError);
}
