using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using TypeSharp.Compiler;
using TypeSharp.Cli;
using TypeSharp.Compiler.Abi;
using TypeSharp.Compiler.Backend;
using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Checking;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Interop;
using TypeSharp.Compiler.Lowering;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;
using TypeSharp.Compiler.Semantics;
using TypeSharp.Compiler.Testing;
using TypeSharp.Compiler.TypeChecking;
using TypeSharp.Core;
using TypeSharp.LanguageServer;
using TypeSharp.Runtime;

var tests = new (string Name, Action Body)[]
{
    ("version defaults match the documented CLI contract", VersionDefaultsMatchCliContract),
    ("diagnostic descriptor registry is stable", DiagnosticDescriptorRegistryIsStable),
    ("diagnostic text follows CLI text shape", DiagnosticTextUsesCliShape),
    ("CLI new creates console project", CliNewCreatesConsoleProject),
    ("CLI new creates library project", CliNewCreatesLibraryProject),
    ("CLI new rejects non-empty output directory", CliNewRejectsNonEmptyOutputDirectory),
    ("CLI accepts common no-color option", CliAcceptsCommonNoColorOption),
    ("CLI accepts preview project option", CliAcceptsPreviewProjectOption),
    ("CLI rejects unknown project option", CliRejectsUnknownProjectOption),
    ("CLI rejects invalid diagnostic format", CliRejectsInvalidDiagnosticFormat),
    ("CLI rejects invalid configuration", CliRejectsInvalidConfiguration),
    ("CLI rejects invalid target framework", CliRejectsInvalidTargetFramework),
    ("CLI rejects invalid verbosity", CliRejectsInvalidVerbosity),
    ("CLI explain prints diagnostic descriptor metadata", CliExplainPrintsDiagnosticDescriptorMetadata),
    ("CLI explain emits JSON descriptor metadata", CliExplainEmitsJsonDescriptorMetadata),
    ("CLI explain rejects unknown diagnostic code", CliExplainRejectsUnknownDiagnosticCode),
    ("CLI format check succeeds on formatted project", CliFormatCheckSucceedsOnFormattedProject),
    ("CLI format checks and writes normalized source", CliFormatChecksAndWritesNormalizedSource),
    ("CLI format reports parse diagnostics without rewriting", CliFormatReportsParseDiagnosticsWithoutRewriting),
    ("parse result exposes error state", ParseResultExposesErrorState),
    ("parser fixture convention paths are stable", ParserFixtureConventionPathsAreStable),
    ("binder fixture convention paths are stable", BinderFixtureConventionPathsAreStable),
    ("type checker fixture convention paths are stable", TypeCheckerFixtureConventionPathsAreStable),
    ("C# backend fixture convention paths are stable", CSharpBackendFixtureConventionPathsAreStable),
    ("backend abstraction exposes C# source backend", BackendAbstractionExposesCSharpSourceBackend),
    ("backend artifact contract supports direct assembly output", BackendArtifactContractSupportsDirectAssemblyOutput),
    ("lowering pipeline injects runtime helper imports", LoweringPipelineInjectsRuntimeHelperImports),
    ("manifest loader reads explicit manifest path", ManifestLoaderReadsExplicitManifestPath),
    ("manifest locator searches parent directories", ManifestLocatorSearchesParentDirectories),
    ("source discovery defaults to src root", SourceDiscoveryDefaultsToSrcRoot),
    ("source discovery excludes build and generated folders", SourceDiscoveryExcludesBuildAndGeneratedFolders),
    ("source discovery reports duplicate module paths", SourceDiscoveryReportsDuplicateModulePaths),
    ("runtime project targets net48", RuntimeProjectTargetsNet48),
    ("core project targets net48", CoreProjectTargetsNet48),
    ("runtime ABI constants are aligned", RuntimeAbiConstantsAreAligned),
    ("net48 runtime artifacts avoid external package dependencies", Net48RuntimeArtifactsAvoidExternalPackageDependencies),
    ("core option and result expose basic states", CoreOptionAndResultExposeBasicStates),
    ("runtime union helper exposes case metadata", RuntimeUnionHelperExposesCaseMetadata),
    ("runtime pattern helper matches union cases", RuntimePatternHelperMatchesUnionCases),
    ("runtime equality helper combines values", RuntimeEqualityHelperCombinesValues),
    ("runtime async helper creates tasks", RuntimeAsyncHelperCreatesTasks),
    ("reference resolver normalizes framework assemblies", ReferenceResolverNormalizesFrameworkAssemblies),
    ("reference resolver normalizes local DLL paths", ReferenceResolverNormalizesLocalDllPaths),
    ("reference resolver reports missing local DLL diagnostics", ReferenceResolverReportsMissingLocalDllDiagnostics),
    ("reference resolver reports unsupported package diagnostics", ReferenceResolverReportsUnsupportedPackageDiagnostics),
    ("metadata reader creates framework assembly placeholders", MetadataReaderCreatesFrameworkAssemblyPlaceholders),
    ("metadata reader creates local assembly placeholders", MetadataReaderCreatesLocalAssemblyPlaceholders),
    ("metadata reader indexes local public symbols", MetadataReaderIndexesLocalPublicSymbols),
    ("metadata reader preserves reference resolution diagnostics", MetadataReaderPreservesReferenceResolutionDiagnostics),
    ("metadata reader reports missing local metadata inputs", MetadataReaderReportsMissingLocalMetadataInputs),
    ("checker reports missing reference diagnostics", CheckerReportsMissingReferenceDiagnostics),
    ("checker reports invalid byref interop diagnostics", CheckerReportsInvalidByRefInteropDiagnostics),
    ("checker reports ambiguous C# overload diagnostics", CheckerReportsAmbiguousCSharpOverloadDiagnostics),
    ("C# overload resolver selects exact literal match", CSharpOverloadResolverSelectsExactLiteralMatch),
    ("checker reports ambiguous expanded params overload diagnostics", CheckerReportsAmbiguousExpandedParamsOverloadDiagnostics),
    ("checker reports ambiguous optional overload diagnostics", CheckerReportsAmbiguousOptionalOverloadDiagnostics),
    ("checker reports unknown C# nullability diagnostics", CheckerReportsUnknownCSharpNullabilityDiagnostics),
    ("CLI check emits JSON reference diagnostics", CliCheckEmitsJsonReferenceDiagnostics),
    ("CLI check emits JSON duplicate source module diagnostics", CliCheckEmitsJsonDuplicateSourceModuleDiagnostics),
    ("CLI check emits JSON unsupported package diagnostics", CliCheckEmitsJsonUnsupportedPackageDiagnostics),
    ("CLI build stops before emission on reference diagnostics", CliBuildStopsBeforeEmissionOnReferenceDiagnostics),
    ("CLI build stops before emission on duplicate source modules", CliBuildStopsBeforeEmissionOnDuplicateSourceModules),
    ("CLI build stops before emission on package diagnostics", CliBuildStopsBeforeEmissionOnPackageDiagnostics),
    ("CLI build stops before emission on invalid byref interop", CliBuildStopsBeforeEmissionOnInvalidByRefInterop),
    ("CLI build stops before emission on ambiguous C# overload", CliBuildStopsBeforeEmissionOnAmbiguousCSharpOverload),
    ("CLI build stops before emission on type checker diagnostics", CliBuildStopsBeforeEmissionOnTypeCheckerDiagnostics),
    ("CLI build stops before emission on nullability diagnostics", CliBuildStopsBeforeEmissionOnNullabilityDiagnostics),
    ("CLI build stops before emission on public boundary diagnostics", CliBuildStopsBeforeEmissionOnPublicBoundaryDiagnostics),
    ("CLI build stops before emission on non-exhaustive match", CliBuildStopsBeforeEmissionOnNonExhaustiveMatch),
    ("CLI build stops before emission on unsupported export forwarding", CliBuildStopsBeforeEmissionOnUnsupportedExportForwarding),
    ("manifest loader reports invalid manifest shape", ManifestLoaderReportsInvalidManifestShape),
    ("CLI run builds and runs generated net48 executable", CliRunBuildsAndRunsGeneratedNet48Executable),
    ("CLI run passes arguments to generated main", CliRunPassesArgumentsToGeneratedMain),
    ("CLI run reports unsupported main signature", CliRunReportsUnsupportedMainSignature),
    ("CLI run rejects library projects", CliRunRejectsLibraryProjects),
    ("CLI build honors Release configuration", CliBuildHonorsReleaseConfiguration),
    ("CLI run honors Release configuration", CliRunHonorsReleaseConfiguration),
    ("CLI build honors target framework override", CliBuildHonorsTargetFrameworkOverride),
    ("CLI run honors target framework override", CliRunHonorsTargetFrameworkOverride),
    ("CLI build honors quiet verbosity", CliBuildHonorsQuietVerbosity),
    ("CLI build honors minimal verbosity", CliBuildHonorsMinimalVerbosity),
    ("lexer handles tokens used by hello fixture", LexerHandlesHelloFixtureTokens),
    ("parser parses hello fixture without diagnostics", ParserParsesHelloFixtureWithoutDiagnostics),
    ("parser parses module declaration without diagnostics", ParserParsesModuleDeclarationWithoutDiagnostics),
    ("parser fixture snapshots match", ParserFixtureSnapshotsMatch),
    ("binder fixture diagnostics match", BinderFixtureDiagnosticsMatch),
    ("type checker fixture diagnostics match", TypeCheckerFixtureDiagnosticsMatch),
    ("C# backend fixture snapshots match", CSharpBackendFixtureSnapshotsMatch),
    ("generated C# compiles in net48 project", GeneratedCSharpCompilesInNet48Project),
    ("binder binds local declarations without diagnostics", BinderBindsLocalDeclarationsWithoutDiagnostics),
    ("semantic model resolves symbols at source positions", SemanticModelResolvesSymbolsAtSourcePositions),
    ("checker reports unresolved name diagnostics", CheckerReportsUnresolvedNameDiagnostics),
    ("checker reports duplicate symbol diagnostics", CheckerReportsDuplicateSymbolDiagnostics),
    ("checker reports import alias conflict diagnostics", CheckerReportsImportAliasConflictDiagnostics),
    ("checker reports unsupported export forwarding diagnostics", CheckerReportsUnsupportedExportForwardingDiagnostics),
    ("type checker accepts basic annotations", TypeCheckerAcceptsBasicAnnotations),
    ("inference engine infers local expression graph", InferenceEngineInfersLocalExpressionGraph),
    ("checker reports type mismatch diagnostics", CheckerReportsTypeMismatchDiagnostics),
    ("checker reports parser diagnostics", CheckerReportsParserDiagnostics),
    ("CLI check succeeds on parse-clean project", CliCheckSucceedsOnParseCleanProject),
    ("CLI check emits JSON parser diagnostics", CliCheckEmitsJsonParserDiagnostics),
    ("CLI check emits JSON type checker diagnostics", CliCheckEmitsJsonTypeCheckerDiagnostics),
    ("CLI check emits JSON nullability diagnostics", CliCheckEmitsJsonNullabilityDiagnostics),
    ("CLI check emits JSON structural diagnostics", CliCheckEmitsJsonStructuralDiagnostics),
    ("CLI check emits JSON public boundary diagnostics", CliCheckEmitsJsonPublicBoundaryDiagnostics),
    ("CLI check emits JSON duplicate symbol diagnostics", CliCheckEmitsJsonDuplicateSymbolDiagnostics),
    ("CLI check emits JSON import alias conflict diagnostics", CliCheckEmitsJsonImportAliasConflictDiagnostics),
    ("CLI check emits JSON unsupported export forwarding diagnostics", CliCheckEmitsJsonUnsupportedExportForwardingDiagnostics),
    ("CLI check emits JSON unsupported generic constraint diagnostics", CliCheckEmitsJsonUnsupportedGenericConstraintDiagnostics),
    ("CLI check emits JSON dynamic capability diagnostics", CliCheckEmitsJsonDynamicCapabilityDiagnostics),
    ("CLI check emits JSON dynamic call capability diagnostics", CliCheckEmitsJsonDynamicCallCapabilityDiagnostics),
    ("CLI check emits JSON capability call marker diagnostics", CliCheckEmitsJsonCapabilityCallMarkerDiagnostics),
    ("CLI check emits JSON unknown access diagnostics", CliCheckEmitsJsonUnknownAccessDiagnostics),
    ("CLI check keeps warnings nonblocking by default", CliCheckKeepsWarningsNonblockingByDefault),
    ("CLI check treats warnings as errors", CliCheckTreatsWarningsAsErrors),
    ("CLI build stops before emission on warnings as errors", CliBuildStopsBeforeEmissionOnWarningsAsErrors),
    ("LSP diagnostic mapper uses zero-based ranges", LspDiagnosticMapperUsesZeroBasedRanges),
    ("language server publishes diagnostics on didOpen", LanguageServerPublishesDiagnosticsOnDidOpen),
    ("language server returns hover for bound symbols", LanguageServerReturnsHoverForBoundSymbols),
    ("language server returns definition for bound symbols", LanguageServerReturnsDefinitionForBoundSymbols),
    ("language server returns completion items", LanguageServerReturnsCompletionItems),
    ("VS Code extension activates LSP client", VsCodeExtensionActivatesLspClient),
    ("VS Code extension activation smoke runs in mocked extension host", VsCodeExtensionActivationSmokeRunsInMockedExtensionHost),
    ("VS Code extension live smoke runs against bundled language server", VsCodeExtensionLiveSmokeRunsAgainstBundledLanguageServer),
    ("VS Code extension package shape is stable", VsCodeExtensionPackageShapeIsStable),
    ("runnable example catalog smoke matrix is stable", RunnableExampleCatalogSmokeMatrixIsStable),
    ("runnable example project commands are smoke-tested", RunnableExampleProjectCommandsAreSmokeTested),
    ("docs site contract is stable", DocsSiteContractIsStable),
    ("GitHub Pages workflow contract is stable", GitHubPagesWorkflowContractIsStable),
    ("CLI build emits generated C# source", CliBuildEmitsGeneratedCSharpSource),
    ("CLI build uses root namespace for namespace-less source", CliBuildUsesRootNamespaceForNamespaceLessSource),
    ("CLI build omits ambient function declarations", CliBuildOmitsAmbientFunctionDeclarations),
    ("CLI build ignores ambient main entry point", CliBuildIgnoresAmbientMainEntryPoint),
    ("CLI build lowers open declarations to using directives", CliBuildLowersOpenDeclarationsToUsingDirectives),
    ("CLI build lowers import alias to using alias directive", CliBuildLowersImportAliasToUsingAliasDirective),
    ("CLI build lowers namespace import to using alias directive", CliBuildLowersNamespaceImportToUsingAliasDirective),
    ("CLI build emits generated C# project scaffold", CliBuildEmitsGeneratedCSharpProjectScaffold),
    ("CLI build propagates manifest references to generated C# project", CliBuildPropagatesManifestReferencesToGeneratedCSharpProject),
    ("CLI build compiles framework static member call", CliBuildCompilesFrameworkStaticMemberCall),
    ("CLI build compiles local DLL static member call", CliBuildCompilesLocalDllStaticMemberCall),
    ("CLI build compiles imported constructor and instance member call", CliBuildCompilesImportedConstructorAndInstanceMemberCall),
    ("CLI build compiles imported property access", CliBuildCompilesImportedPropertyAccess),
    ("CLI build compiles imported field access", CliBuildCompilesImportedFieldAccess),
    ("CLI build compiles imported indexer access", CliBuildCompilesImportedIndexerAccess),
    ("CLI build compiles imported params call", CliBuildCompilesImportedParamsCall),
    ("CLI build compiles imported out call", CliBuildCompilesImportedOutCall),
    ("CLI build compiles imported in call", CliBuildCompilesImportedInCall),
    ("CLI build compiles imported ref call", CliBuildCompilesImportedRefCall),
    ("CLI build compiles exact overload match", CliBuildCompilesExactOverloadMatch),
    ("CLI build compiles exact expanded params overload match", CliBuildCompilesExactExpandedParamsOverloadMatch),
    ("CLI build compiles imported optional call", CliBuildCompilesImportedOptionalCall),
    ("CLI build compiles imported named argument call", CliBuildCompilesImportedNamedArgumentCall),
    ("CLI build compiles imported delegate lambda call", CliBuildCompilesImportedDelegateLambdaCall),
    ("CLI build compiles imported event add and remove call", CliBuildCompilesImportedEventAddRemoveCall),
    ("CLI build compiles imported generic method call", CliBuildCompilesImportedGenericMethodCall),
    ("CLI build compiles imported interface reference", CliBuildCompilesImportedInterfaceReference),
    ("CLI build compiles imported attribute and generic type references", CliBuildCompilesImportedAttributeAndGenericTypeReferences),
    ("CLI build compiles basic semantics", CliBuildCompilesBasicSemantics),
    ("CLI build compiles module namespace", CliBuildCompilesModuleNamespace),
    ("CLI build compiles core option result APIs", CliBuildCompilesCoreOptionResultApis),
    ("CLI build compiles generic function API", CliBuildCompilesGenericFunctionApi),
    ("CLI build compiles class declaration API", CliBuildCompilesClassDeclarationApi),
    ("CLI build compiles interface declaration API", CliBuildCompilesInterfaceDeclarationApi),
    ("CLI build compiles partial declaration API", CliBuildCompilesPartialDeclarationApi),
    ("CLI build compiles generic type declaration API", CliBuildCompilesGenericTypeDeclarationApi),
    ("CLI build compiles generic constraint API", CliBuildCompilesGenericConstraintApi),
    ("CLI build compiles immutable record API", CliBuildCompilesImmutableRecordApi),
    ("CLI build compiles record update lowering", CliBuildCompilesRecordUpdateLowering),
    ("CLI build compiles record expression construction", CliBuildCompilesRecordExpressionConstruction),
    ("CLI build compiles nominal union API", CliBuildCompilesNominalUnionApi),
    ("CLI build compiles nominal union match lowering", CliBuildCompilesNominalUnionMatchLowering),
    ("CLI build compiles type-level union narrowing", CliBuildCompilesTypeLevelUnionNarrowing),
    ("CLI build compiles async Task interop", CliBuildCompilesAsyncTaskInterop),
    ("CLI build compiles collection expression lowering", CliBuildCompilesCollectionExpressionLowering),
    ("CLI build compiles pipeline lowering", CliBuildCompilesPipelineLowering),
    ("CLI build compiles literal constants", CliBuildCompilesLiteralConstants),
    ("CLI build emits generated net48 assembly", CliBuildEmitsGeneratedNet48Assembly),
    ("generated net48 assembly public ABI snapshot is stable", GeneratedNet48AssemblyPublicAbiSnapshotIsStable),
    ("C# net48 project consumes generated TypeSharp assembly", CSharpNet48ProjectConsumesGeneratedTypeSharpAssembly),
    ("net48 application model hosts reference generated assembly and runtime", Net48ApplicationModelHostsReferenceGeneratedAssemblyAndRuntime),
    ("compiler check performance smoke stays bounded", CompilerCheckPerformanceSmokeStaysBounded),
    ("CLI build stops before emission on diagnostics", CliBuildStopsBeforeEmissionOnDiagnostics)
};

var failures = 0;
var filter = args.Length == 0 ? null : args[0];
var executed = 0;

foreach (var (name, body) in tests)
{
    if (!string.IsNullOrWhiteSpace(filter) &&
        !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
    {
        continue;
    }

    executed++;
    try
    {
        body();
        Console.WriteLine($"PASS {name}");
        Console.Out.Flush();
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
        Console.Error.Flush();
    }
}

if (executed == 0)
{
    failures++;
    Console.Error.WriteLine($"No tests matched filter '{filter}'.");
    Console.Error.Flush();
}

return failures == 0 ? 0 : 1;

static void VersionDefaultsMatchCliContract()
{
    AssertEqual("0.1.0-preview", TypeSharpCompilerInfo.CliVersion);
    AssertEqual("0.1.0-preview", TypeSharpCompilerInfo.CompilerVersion);
    AssertEqual("preview", TypeSharpCompilerInfo.LanguageVersion);
    AssertEqual(0, TypeSharpCompilerInfo.RuntimeAbiVersion);
    AssertEqual("net48", TypeSharpCompilerInfo.DefaultTargetFramework);
}

static void DiagnosticDescriptorRegistryIsStable()
{
    var descriptors = DiagnosticDescriptors.All;
    AssertSequence(
        [
            "TS0100",
            "TS0101",
            "TS0102",
            "TS0103",
            "TS0110",
            "TS0111",
            "TS1000",
            "TS1001",
            "TS1002",
            "TS1003",
            "TS1004",
            "TS2001",
            "TS2002",
            "TS2003",
            "TS2201",
            "TS2202",
            "TS2203",
            "TS2204",
            "TS2205",
            "TS2206",
            "TS2207",
            "TS2208",
            "TS2209",
            "TS2401",
            "TS2402",
            "TS2403",
            "TS2404",
            "TS2405",
            "TS3500",
            "TS3501"
        ],
        descriptors.Select(descriptor => descriptor.Code).ToArray());

    AssertEqual(
        descriptors.Count,
        descriptors.Select(descriptor => descriptor.Code).Distinct(StringComparer.Ordinal).Count());

    foreach (var descriptor in descriptors)
    {
        AssertFalse(string.IsNullOrWhiteSpace(descriptor.Title), $"Diagnostic {descriptor.Code} should have a title.");
        AssertFalse(string.IsNullOrWhiteSpace(descriptor.MessageTemplate), $"Diagnostic {descriptor.Code} should have a message template.");
        AssertFalse(string.IsNullOrWhiteSpace(descriptor.Explanation), $"Diagnostic {descriptor.Code} should have an explanation.");
        AssertFalse(string.IsNullOrWhiteSpace(descriptor.SuggestedAction), $"Diagnostic {descriptor.Code} should have a suggested action.");
    }
}

static void DiagnosticTextUsesCliShape()
{
    var diagnostic = new Diagnostic(
        "TS1001",
        DiagnosticSeverity.Error,
        "Expected function body after function signature.",
        "input.tysh",
        new SourceSpan(new SourcePosition(3, 40), new SourcePosition(3, 40)));

    AssertEqual(
        "input.tysh(3,40): error TS1001: Expected function body after function signature.",
        diagnostic.ToCliText());
}

static void CliNewCreatesConsoleProject()
{
    WithWorkspace(root =>
    {
        var projectRoot = Path.Combine(root, "HelloApp");
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["new", "console", "HelloApp", "--output", projectRoot], new StringWriter(output), new StringWriter(error));

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());
        AssertContains("Created TypeSharp console project", output.ToString());
        AssertTrue(File.Exists(Path.Combine(projectRoot, "TypeSharp.toml")), "Console template should create a manifest.");
        AssertTrue(File.Exists(Path.Combine(projectRoot, "src", "Main.tysh")), "Console template should create src/Main.tysh.");
        AssertTrue(File.Exists(Path.Combine(projectRoot, ".gitignore")), "Console template should create .gitignore.");
        AssertContains("outputType = \"exe\"", File.ReadAllText(Path.Combine(projectRoot, "TypeSharp.toml")));
        AssertContains("main = \"HelloApp.main\"", File.ReadAllText(Path.Combine(projectRoot, "TypeSharp.toml")));
        AssertContains("export fun main(): string", File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh")));

        var checkExitCode = TypeSharpCli.Run(["check", Path.Combine(projectRoot, "TypeSharp.toml")], new StringWriter(), new StringWriter());
        AssertEqual(0, checkExitCode);
    });
}

static void CliNewCreatesLibraryProject()
{
    WithWorkspace(root =>
    {
        var projectRoot = Path.Combine(root, "Billing.Core");
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["new", "library", "Billing.Core", "--target", "net48", "--output", projectRoot], new StringWriter(output), new StringWriter(error));

        AssertTrue(
            exitCode == 0,
            $"Imported interface reference should compile.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(projectRoot, "TypeSharp.toml")), "Library template should create a manifest.");
        AssertTrue(File.Exists(Path.Combine(projectRoot, "src", "Library.tysh")), "Library template should create src/Library.tysh.");
        AssertContains("outputType = \"library\"", File.ReadAllText(Path.Combine(projectRoot, "TypeSharp.toml")));
        AssertContains("rootNamespace = \"Billing.Core\"", File.ReadAllText(Path.Combine(projectRoot, "TypeSharp.toml")));
        AssertContains("export fun greeting", File.ReadAllText(Path.Combine(projectRoot, "src", "Library.tysh")));

        var buildExitCode = TypeSharpCli.Run(["build", Path.Combine(projectRoot, "TypeSharp.toml")], new StringWriter(), new StringWriter());
        AssertEqual(0, buildExitCode);
        AssertTrue(File.Exists(Path.Combine(projectRoot, "generated", "bin", "Debug", "net48", "Billing.Core.dll")), "Library template should build a generated net48 DLL.");
    });
}

static void CliNewRejectsNonEmptyOutputDirectory()
{
    WithWorkspace(root =>
    {
        var projectRoot = Path.Combine(root, "Existing");
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(projectRoot, "keep.txt"), "do not overwrite");
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["new", "console", "Existing", "--output", projectRoot], new StringWriter(output), new StringWriter(error));

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("is not empty", error.ToString());
        AssertFalse(File.Exists(Path.Combine(projectRoot, "TypeSharp.toml")), "New command should not write into non-empty directories.");
    });
}

static void CliAcceptsCommonNoColorOption()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("NoColor"));
        WriteFile(root, "src/Main.tysh", "namespace Samples.NoColor\n\nexport fun greeting(): string = \"ok\"\n");

        var versionOutput = new StringBuilder();
        var versionError = new StringBuilder();
        AssertEqual(0, TypeSharpCli.Run(["version", "--json", "--no-color"], new StringWriter(versionOutput), new StringWriter(versionError)));
        AssertContains("\"cli\":", versionOutput.ToString());
        AssertEqual(string.Empty, versionError.ToString());

        var newRoot = Path.Combine(root, "NoColorNew");
        var newOutput = new StringBuilder();
        var newError = new StringBuilder();
        AssertEqual(0, TypeSharpCli.Run(["new", "library", "NoColorNew", "--no-color", "--output", newRoot], new StringWriter(newOutput), new StringWriter(newError)));
        AssertTrue(File.Exists(Path.Combine(newRoot, "TypeSharp.toml")), "New command should accept --no-color and create a manifest.");
        AssertEqual(string.Empty, newError.ToString());

        var checkOutput = new StringBuilder();
        var checkError = new StringBuilder();
        AssertEqual(0, TypeSharpCli.Run(["check", manifestPath, "--no-color"], new StringWriter(checkOutput), new StringWriter(checkError)));
        AssertEqual(string.Empty, checkError.ToString());

        var explainOutput = new StringBuilder();
        var explainError = new StringBuilder();
        AssertEqual(0, TypeSharpCli.Run(["explain", "TS1001", "--no-color"], new StringWriter(explainOutput), new StringWriter(explainError)));
        AssertContains("TS1001", explainOutput.ToString());
        AssertEqual(string.Empty, explainError.ToString());

        var formatOutput = new StringBuilder();
        var formatError = new StringBuilder();
        AssertEqual(0, TypeSharpCli.Run(["format", manifestPath, "--check", "--no-color"], new StringWriter(formatOutput), new StringWriter(formatError)));
        AssertContains("All TypeSharp files are formatted.", formatOutput.ToString());
        AssertEqual(string.Empty, formatError.ToString());
    });
}

static void CliAcceptsPreviewProjectOption()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("PreviewOption"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.PreviewOption

            export fun greeting(): string = "preview"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--preview"], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());
    });
}

static void CliRejectsUnknownProjectOption()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("UnknownProjectOption"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnknownProjectOption

            export fun greeting(): string = "unknown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--unknown-option"], output, error);

        AssertEqual(2, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("Unknown option '--unknown-option'.", error.ToString());
    });
}

static void CliRejectsInvalidDiagnosticFormat()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("InvalidDiagnosticFormat"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidDiagnosticFormat

            export fun greeting(): string = "ok"
            """);
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "xml"], new StringWriter(output), new StringWriter(error));

        AssertEqual(2, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("Diagnostic format must be 'text' or 'json'.", error.ToString());
    });
}

static void CliRejectsInvalidConfiguration()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("InvalidConfiguration"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidConfiguration

            export fun greeting(): string = "ok"
            """);
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--configuration", "Fast"], new StringWriter(output), new StringWriter(error));

        AssertEqual(2, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("Configuration must be 'Debug' or 'Release'.", error.ToString());
    });
}

static void CliRejectsInvalidTargetFramework()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("InvalidTarget"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidTarget

            export fun greeting(): string = "ok"
            """);
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--target", "net481"], new StringWriter(output), new StringWriter(error));

        AssertEqual(2, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("Target framework must be 'net48'.", error.ToString());
    });
}

static void CliRejectsInvalidVerbosity()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("InvalidVerbosity"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidVerbosity

            export fun greeting(): string = "ok"
            """);
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--verbosity", "loud"], new StringWriter(output), new StringWriter(error));

        AssertEqual(2, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("Verbosity must be 'quiet', 'minimal', 'normal', or 'diagnostic'.", error.ToString());
    });
}

static void CliExplainPrintsDiagnosticDescriptorMetadata()
{
    var output = new StringBuilder();
    var error = new StringBuilder();

    var exitCode = TypeSharpCli.Run(["explain", "TS2204"], new StringWriter(output), new StringWriter(error));

    AssertEqual(0, exitCode);
    AssertEqual(string.Empty, error.ToString());
    AssertContains("TS2204: Compile-time type leaked through public boundary", output.ToString());
    AssertContains("Severity: error", output.ToString());
    AssertContains("Category: TypeChecking", output.ToString());
    AssertContains("Message: Type-level union cannot appear in public API. Use a nominal union or interface.", output.ToString());
    AssertContains("Explanation: Type-level unions and structural shapes are compile-time TypeSharp types", output.ToString());
    AssertContains("Suggested action: Replace the public type with a nominal union", output.ToString());
}

static void CliExplainEmitsJsonDescriptorMetadata()
{
    var output = new StringBuilder();
    var error = new StringBuilder();

    var exitCode = TypeSharpCli.Run(["explain", "ts1001", "--json"], new StringWriter(output), new StringWriter(error));

    AssertEqual(0, exitCode);
    AssertEqual(string.Empty, error.ToString());
    AssertContains("\"code\": \"TS1001\"", output.ToString());
    AssertContains("\"title\": \"Missing function body\"", output.ToString());
    AssertContains("\"severity\": \"error\"", output.ToString());
    AssertContains("\"category\": \"Parser\"", output.ToString());
    AssertContains("\"messageTemplate\": \"Expected function body after function signature.\"", output.ToString());
    AssertContains("\"suggestedAction\": \"Add '= expression', add a block body, or mark the declaration as extern if it is imported.\"", output.ToString());
}

static void CliExplainRejectsUnknownDiagnosticCode()
{
    var output = new StringBuilder();
    var error = new StringBuilder();

    var exitCode = TypeSharpCli.Run(["explain", "TS9999"], new StringWriter(output), new StringWriter(error));

    AssertEqual(1, exitCode);
    AssertEqual(string.Empty, output.ToString());
    AssertContains("Unknown diagnostic code 'TS9999'.", error.ToString());
}

static void CliFormatCheckSucceedsOnFormattedProject()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("FormatProject"));
        WriteFile(root, "src/Main.tysh", "namespace Samples.FormatProject\n\nexport fun greeting(): string = \"ok\"\n");
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["format", manifestPath, "--check"], new StringWriter(output), new StringWriter(error));

        AssertEqual(0, exitCode);
        AssertEqual($"All TypeSharp files are formatted.{Environment.NewLine}", output.ToString());
        AssertEqual(string.Empty, error.ToString());
    });
}

static void CliFormatChecksAndWritesNormalizedSource()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "Main.tysh");
        File.WriteAllText(
            sourcePath,
            "namespace Samples.Format  \r\n\r\n\r\nexport fun greeting(): string = \"ok\"  ");
        var checkOutput = new StringBuilder();
        var checkError = new StringBuilder();

        var checkExitCode = TypeSharpCli.Run(["format", sourcePath, "--check"], new StringWriter(checkOutput), new StringWriter(checkError));

        AssertEqual(1, checkExitCode);
        AssertContains("Needs formatting: Main.tysh", checkOutput.ToString());
        AssertEqual(string.Empty, checkError.ToString());

        var formatOutput = new StringBuilder();
        var formatError = new StringBuilder();
        var formatExitCode = TypeSharpCli.Run(["format", sourcePath], new StringWriter(formatOutput), new StringWriter(formatError));

        AssertEqual(0, formatExitCode);
        AssertContains("Formatted: Main.tysh", formatOutput.ToString());
        AssertEqual(string.Empty, formatError.ToString());
        AssertTextEquals(
            "namespace Samples.Format\n\nexport fun greeting(): string = \"ok\"\n",
            File.ReadAllText(sourcePath));
    });
}

static void CliFormatReportsParseDiagnosticsWithoutRewriting()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "Broken.tysh");
        var original = "namespace Samples.FormatBroken\n\nexport fun broken(): string\n";
        File.WriteAllText(sourcePath, original);
        var output = new StringBuilder();
        var error = new StringBuilder();

        var exitCode = TypeSharpCli.Run(["format", sourcePath], new StringWriter(output), new StringWriter(error));

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("TS1001", error.ToString());
        AssertEqual(original, File.ReadAllText(sourcePath));
    });
}

static void ParseResultExposesErrorState()
{
    var result = new ParseResult(
    [
        new Diagnostic(
            "TS1001",
            DiagnosticSeverity.Error,
            "Expected function body after function signature.",
            "input.tysh",
            new SourceSpan(new SourcePosition(3, 40), new SourcePosition(3, 40)))
    ]);

    AssertTrue(result.HasErrors, "Parse result should report errors.");
}

static void ParserFixtureConventionPathsAreStable()
{
    AssertEqual("tests/fixtures/parser", ParserFixtureConventions.Root);
    AssertEqual("tests/fixtures/parser/positive", ParserFixtureConventions.PositiveRoot);
    AssertEqual("tests/fixtures/parser/negative", ParserFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", ParserFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", ParserFixtureConventions.ExpectedDiagnosticsFileName);
    AssertEqual("expected.tree", ParserFixtureConventions.ExpectedTreeFileName);
}

static void BinderFixtureConventionPathsAreStable()
{
    AssertEqual("tests/fixtures/diagnostics/binder", BinderFixtureConventions.Root);
    AssertEqual("tests/fixtures/diagnostics/binder/positive", BinderFixtureConventions.PositiveRoot);
    AssertEqual("tests/fixtures/diagnostics/binder/negative", BinderFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", BinderFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", BinderFixtureConventions.ExpectedDiagnosticsFileName);
}

static void TypeCheckerFixtureConventionPathsAreStable()
{
    AssertEqual("tests/fixtures/diagnostics/type-checker", TypeCheckerFixtureConventions.Root);
    AssertEqual("tests/fixtures/diagnostics/type-checker/positive", TypeCheckerFixtureConventions.PositiveRoot);
    AssertEqual("tests/fixtures/diagnostics/type-checker/negative", TypeCheckerFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", TypeCheckerFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", TypeCheckerFixtureConventions.ExpectedDiagnosticsFileName);
}

static void CSharpBackendFixtureConventionPathsAreStable()
{
    AssertEqual("tests/fixtures/backend/csharp", CSharpBackendFixtureConventions.Root);
    AssertEqual("tests/fixtures/backend/csharp/positive", CSharpBackendFixtureConventions.PositiveRoot);
    AssertEqual("input.tysh", CSharpBackendFixtureConventions.InputFileName);
    AssertEqual("expected.cs", CSharpBackendFixtureConventions.ExpectedCSharpFileName);
}

static void BackendAbstractionExposesCSharpSourceBackend()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.BackendAbstraction

        export fun hello(): string = "hello"
        """);

    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var backend = CSharpSourceBackendAdapter.Instance;

    AssertEqual("csharp", backend.Name);
    AssertEqual(TypeSharpBackendArtifactKind.SourceText, backend.ArtifactKind);
    AssertEqual(".g.cs", backend.GeneratedArtifactExtension);

    var artifact = backend.Emit(root);
    AssertEqual(TypeSharpBackendArtifactKind.SourceText, artifact.Kind);
    AssertEqual(".g.cs", artifact.Extension);
    AssertEqual(CSharpSourceBackend.Emit(root), artifact.RequireText());
}

static void BackendArtifactContractSupportsDirectAssemblyOutput()
{
    var sourceArtifact = TypeSharpBackendArtifact.SourceText(".g.cs", "namespace Generated {}");
    AssertEqual(TypeSharpBackendArtifactKind.SourceText, sourceArtifact.Kind);
    AssertEqual(".g.cs", sourceArtifact.Extension);
    AssertEqual("namespace Generated {}", sourceArtifact.RequireText());
    AssertThrows<InvalidOperationException>(() => sourceArtifact.RequireBytes());

    var assemblyArtifact = TypeSharpBackendArtifact.Assembly(".dll", [0x4D, 0x5A]);
    AssertEqual(TypeSharpBackendArtifactKind.Assembly, assemblyArtifact.Kind);
    AssertEqual(".dll", assemblyArtifact.Extension);
    AssertSequence<byte>([0x4D, 0x5A], assemblyArtifact.RequireBytes());
    AssertThrows<InvalidOperationException>(() => assemblyArtifact.RequireText());
}

static void LoweringPipelineInjectsRuntimeHelperImports()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.Lowering

        union Message {
            Empty
            Text(value: string)
        }

        export fun describe(message: Message): string =
            match message {
                Empty => "empty"
                Text(value) => value
            }
        """);

    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var pipeline = TypeSharpLoweringPipeline.Default;

    AssertSequence(["csharp-runtime-import"], pipeline.Passes.Select(pass => pass.Name).ToArray());

    var lowered = pipeline.Lower(root);
    AssertEqual(1, CountRuntimeImports(lowered));

    var loweredAgain = pipeline.Lower(lowered);
    AssertEqual(1, CountRuntimeImports(loweredAgain));
    AssertTextEquals(CSharpSourceBackend.Emit(root), CSharpSourceBackend.Emit(lowered));
}

static void ManifestLoaderReadsExplicitManifestPath()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "Billing"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.Billing"
            sourceRoots = ["source"]
            main = "Samples.Billing.main"

            [language]
            version = "preview"
            strict = true
            nullable = "strict"
            previewFeatures = ["pipeline"]

            [references]
            assemblies = [
              "System",
              "System.Core"
            ]
            paths = ["lib/Legacy.dll"]
            packages = []

            [tooling]
            diagnosticFormat = "json"
            treatWarningsAsErrors = false
            """);

        var result = TypeSharpManifestLoader.Load(manifestPath);

        AssertFalse(result.HasErrors, "Manifest should load without errors.");
        var manifest = Require(result.Manifest, "Manifest should be available.");
        AssertEqual("Billing", manifest.Project.Name);
        AssertEqual("net48", manifest.Project.TargetFramework);
        AssertEqual("exe", manifest.Project.OutputType);
        AssertEqual("Samples.Billing", manifest.Project.RootNamespace);
        AssertSequence(["source"], manifest.Project.SourceRoots);
        AssertEqual("Samples.Billing.main", manifest.Project.Main);
        AssertSequence(["pipeline"], manifest.Language.PreviewFeatures);
        AssertSequence(["System", "System.Core"], manifest.References.Assemblies);
        AssertSequence(["lib/Legacy.dll"], manifest.References.Paths);
        AssertEqual("json", manifest.Tooling.DiagnosticFormat);
    });
}

static void ManifestLocatorSearchesParentDirectories()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ParentSearch"));
        var nested = Path.Combine(root, "src", "Feature");
        Directory.CreateDirectory(nested);

        var result = TypeSharpManifestLocator.Locate(null, nested);

        AssertFalse(result.HasErrors, "Manifest locator should find a parent manifest.");
        AssertEqual(Path.GetFullPath(manifestPath), result.ManifestPath);
    });
}

static void SourceDiscoveryDefaultsToSrcRoot()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "DefaultRoot"
            """);
        WriteFile(root, "src/B.tysh", "namespace Samples\n");
        WriteFile(root, "src/A.tysh", "namespace Samples\n");

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = SourceDiscovery.Discover(manifest);

        AssertFalse(result.HasErrors, "Source discovery should not fail.");
        AssertSequence(["src/A.tysh", "src/B.tysh"], result.SourceFiles.Select(file => file.RelativePath).ToArray());
        AssertSequence(["A.tysh", "B.tysh"], result.SourceFiles.Select(file => file.SourceRootRelativePath).ToArray());
        AssertSequence(["A", "B"], result.SourceFiles.Select(file => file.ModulePath).ToArray());
    });
}

static void SourceDiscoveryExcludesBuildAndGeneratedFolders()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "Discovery"
            sourceRoots = ["."]
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", "namespace Samples\n");
        WriteFile(root, "bin/Ignored.tysh", "namespace Samples\n");
        WriteFile(root, "obj/Ignored.tysh", "namespace Samples\n");
        WriteFile(root, ".git/Ignored.tysh", "namespace Samples\n");
        WriteFile(root, "generated/Ignored.tysh", "namespace Samples\n");

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = SourceDiscovery.Discover(manifest);

        AssertSequence(["src/Main.tysh"], result.SourceFiles.Select(file => file.RelativePath).ToArray());
    });
}

static void SourceDiscoveryReportsDuplicateModulePaths()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "DuplicateModules"
            sourceRoots = ["src", "shared"]
            """);
        WriteFile(root, "src/Feature/Config.tysh", "namespace Samples\n");
        WriteFile(root, "shared/Feature/Config.tysh", "namespace Samples\n");

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = SourceDiscovery.Discover(manifest);

        AssertTrue(result.HasErrors, "Duplicate source module paths should fail source discovery.");
        AssertSequence(["shared/Feature/Config.tysh", "src/Feature/Config.tysh"], result.SourceFiles.Select(file => file.RelativePath).ToArray());
        AssertSequence(["Feature/Config", "Feature/Config"], result.SourceFiles.Select(file => file.ModulePath).ToArray());
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS0111");
        AssertEqual("src/Feature/Config.tysh", diagnostic.File);
        AssertContains("Duplicate source module path 'Feature/Config'", diagnostic.Message);
        AssertContains("shared/Feature/Config.tysh", diagnostic.Message);
    });
}

static void RuntimeProjectTargetsNet48()
{
    var project = File.ReadAllText(Path.Combine("src", "TypeSharp.Runtime", "TypeSharp.Runtime.csproj"));
    var runtimeInfo = File.ReadAllText(Path.Combine("src", "TypeSharp.Runtime", "TypeSharpRuntimeInfo.cs"));

    AssertContains("<TargetFramework>net48</TargetFramework>", project);
    AssertContains("<AssemblyName>TypeSharp.Runtime</AssemblyName>", project);
    AssertContains("<LangVersion>7.3</LangVersion>", project);
    AssertContains("namespace TypeSharp.Runtime", runtimeInfo);
    AssertContains("RuntimeAbiVersion = 0", runtimeInfo);
}

static void CoreProjectTargetsNet48()
{
    var project = File.ReadAllText(Path.Combine("src", "TypeSharp.Core", "TypeSharp.Core.csproj"));
    var option = File.ReadAllText(Path.Combine("src", "TypeSharp.Core", "Option.cs"));
    var result = File.ReadAllText(Path.Combine("src", "TypeSharp.Core", "Result.cs"));
    var unit = File.ReadAllText(Path.Combine("src", "TypeSharp.Core", "Unit.cs"));

    AssertContains("<TargetFramework>net48</TargetFramework>", project);
    AssertContains("<AssemblyName>TypeSharp.Core</AssemblyName>", project);
    AssertContains("<LangVersion>7.3</LangVersion>", project);
    AssertContains("namespace TypeSharp.Core", option);
    AssertContains("abstract class Option<T>", option);
    AssertContains("namespace TypeSharp.Core", result);
    AssertContains("abstract class Result<T, E>", result);
    AssertContains("namespace TypeSharp.Core", unit);
    AssertContains("struct Unit", unit);
}

static void RuntimeAbiConstantsAreAligned()
{
    AssertEqual(TypeSharpCompilerInfo.RuntimeAbiVersion, TypeSharpRuntimeInfo.RuntimeAbiVersion);
    AssertEqual(0, TypeSharpRuntimeInfo.RuntimeAbiVersion);
}

static void Net48RuntimeArtifactsAvoidExternalPackageDependencies()
{
    AssertNet48PackageFreeArtifact("src/TypeSharp.Core/TypeSharp.Core.csproj");
    AssertNet48PackageFreeArtifact("src/TypeSharp.Runtime/TypeSharp.Runtime.csproj");

    AssertNoDisallowedNet5RuntimeApiReferences("src/TypeSharp.Core");
    AssertNoDisallowedNet5RuntimeApiReferences("src/TypeSharp.Runtime");
}

static void CoreOptionAndResultExposeBasicStates()
{
    var some = Option<string>.Some("value");
    AssertTrue(some.IsSome, "Some should report IsSome.");
    AssertFalse(some.IsNone, "Some should not report IsNone.");
    AssertEqual("value", some.Value);

    var none = Option<string>.None;
    AssertFalse(none.IsSome, "None should not report IsSome.");
    AssertTrue(none.IsNone, "None should report IsNone.");
    AssertThrows<InvalidOperationException>(() => _ = none.Value);

    var ok = Result<int, string>.Ok(42);
    AssertTrue(ok.IsOk, "Ok should report IsOk.");
    AssertFalse(ok.IsError, "Ok should not report IsError.");
    AssertEqual(42, ok.Value);
    AssertThrows<InvalidOperationException>(() => _ = ok.ErrorValue);

    var error = Result<int, string>.Error("failed");
    AssertFalse(error.IsOk, "Error should not report IsOk.");
    AssertTrue(error.IsError, "Error should report IsError.");
    AssertEqual("failed", error.ErrorValue);
    AssertThrows<InvalidOperationException>(() => _ = error.Value);

    AssertEqual(Unit.Value, new Unit());
    AssertEqual("()", Unit.Value.ToString());
}

static void RuntimeUnionHelperExposesCaseMetadata()
{
    var message = new RuntimeUnionSmoke.MessageCase("hello");
    var samePayload = new RuntimeUnionSmoke.MessageCase("hello");
    var differentPayload = new RuntimeUnionSmoke.MessageCase("bye");
    var empty = RuntimeUnionSmoke.EmptyCase.Instance;

    AssertTrue(TypeSharpUnion.IsCase(message, 1), "Union helper should match a case tag.");
    AssertFalse(TypeSharpUnion.IsCase(message, 2), "Union helper should reject a different case tag.");
    AssertEqual(1, TypeSharpUnion.GetTag(message));
    AssertEqual("Message", TypeSharpUnion.GetCaseName(message));
    AssertTrue(TypeSharpUnion.HasPayload(message), "Payload case should report payload availability.");
    AssertEqual("hello", TypeSharpUnion.GetPayload<string>(message));
    AssertTrue(TypeSharpUnion.SameCase(message, samePayload), "Same case tag and name should match.");
    AssertTrue(TypeSharpUnion.PayloadEquals(message, samePayload), "Same payload should compare equal.");
    AssertFalse(TypeSharpUnion.PayloadEquals(message, differentPayload), "Different payload should not compare equal.");
    AssertEqual(TypeSharpUnion.CombineHash(1, "hello"), message.GetHashCode());

    AssertTrue(TypeSharpUnion.IsCase(empty, 0), "Payload-free case should expose its tag.");
    AssertFalse(TypeSharpUnion.HasPayload(empty), "Payload-free case should report no payload.");
    AssertTrue(TypeSharpUnion.PayloadEquals(empty, RuntimeUnionSmoke.EmptyCase.Instance), "Payload-free cases should compare payload equality.");
    AssertThrows<InvalidOperationException>(() => TypeSharpUnion.GetPayload(empty));
    AssertThrows<ArgumentException>(() => TypeSharpUnion.GetTag("not-a-union"));
}

static void RuntimePatternHelperMatchesUnionCases()
{
    var message = new RuntimeUnionSmoke.MessageCase("hello");
    var empty = RuntimeUnionSmoke.EmptyCase.Instance;

    AssertTrue(TypeSharpPattern.IsCase(message, 1), "Pattern helper should match a case tag.");
    AssertTrue(TypeSharpPattern.IsPayloadCase(message, 1), "Payload case should match payload case predicate.");
    AssertFalse(TypeSharpPattern.IsPayloadlessCase(message, 1), "Payload case should not match payload-free predicate.");
    AssertEqual("hello", TypeSharpPattern.RequirePayload<string>(message, 1));

    AssertTrue(TypeSharpPattern.IsCase(empty, 0), "Payload-free case should match its tag.");
    AssertTrue(TypeSharpPattern.IsPayloadlessCase(empty, 0), "Payload-free case should match payload-free predicate.");
    AssertFalse(TypeSharpPattern.IsPayloadCase(empty, 0), "Payload-free case should not match payload predicate.");

    var noMatch = TypeSharpPattern.NoMatch(message);
    AssertContains("No pattern matched union case 'Message' with tag 1.", noMatch.Message);
    AssertThrows<InvalidOperationException>(() => TypeSharpPattern.RequirePayload(message, 2));
    AssertThrows<InvalidOperationException>(() => throw TypeSharpPattern.NoMatch("not-a-union"));
}

static void RuntimeEqualityHelperCombinesValues()
{
    AssertTrue(TypeSharpEquality.AreEqual("value", "value"), "Equal values should compare equal.");
    AssertFalse(TypeSharpEquality.AreEqual("value", "other"), "Different values should not compare equal.");
    AssertTrue(TypeSharpEquality.SequenceEqual(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }), "Equal sequences should compare equal.");
    AssertFalse(TypeSharpEquality.SequenceEqual(new[] { 1, 2, 3 }, new[] { 1, 3, 2 }), "Different sequence order should not compare equal.");

    var hash = TypeSharpEquality.CombineHash("alpha", 42, true);
    AssertEqual(hash, TypeSharpEquality.CombineHash("alpha", 42, true));
    AssertFalse(hash == TypeSharpEquality.CombineHash("alpha", 43, true), "Different values should produce a different combined smoke hash.");
    AssertEqual(TypeSharpEquality.CombineHash(17, TypeSharpEquality.GetHash("alpha")), TypeSharpEquality.CombineHash(17, "alpha".GetHashCode()));
}

static void RuntimeAsyncHelperCreatesTasks()
{
    var completed = TypeSharpAsync.Completed();
    completed.Wait();
    AssertTrue(completed.IsCompletedSuccessfully, "Completed helper should return a completed task.");

    var value = TypeSharpAsync.FromResult("value").GetAwaiter().GetResult();
    AssertEqual("value", value);

    var failed = TypeSharpAsync.FromException<string>(new InvalidOperationException("boom"));
    AssertTrue(failed.IsFaulted, "Failed helper should return a faulted task.");
    AssertThrows<InvalidOperationException>(() => failed.GetAwaiter().GetResult());

    var failedUnit = TypeSharpAsync.FromException(new InvalidOperationException("unit boom"));
    AssertTrue(failedUnit.IsFaulted, "Non-generic failed helper should return a faulted task.");
    AssertThrows<InvalidOperationException>(() => failedUnit.GetAwaiter().GetResult());
}

static void ReferenceResolverNormalizesFrameworkAssemblies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "References"

            [references]
            assemblies = [
              "System.Core",
              "System",
              "System.Core"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = TypeSharpReferenceResolver.Resolve(manifest);

        AssertFalse(result.HasErrors, "Framework assembly names should not require file-system resolution.");
        AssertSequence(
            [ResolvedReferenceKind.FrameworkAssembly, ResolvedReferenceKind.FrameworkAssembly],
            result.References.Select(reference => reference.Kind).ToArray());
        AssertSequence(["System.Core", "System"], result.References.Select(reference => reference.Identity).ToArray());
        AssertTrue(result.References.All(reference => reference.Path is null), "Framework references should not have local paths.");
    });
}

static void ReferenceResolverNormalizesLocalDllPaths()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalReferences"

            [references]
            paths = [
              "lib/Legacy.Billing.dll",
              "lib/Legacy.Reporting.dll"
            ]
            """);
        WriteFile(root, "lib/Legacy.Billing.dll", string.Empty);
        WriteFile(root, "lib/Legacy.Reporting.dll", string.Empty);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = TypeSharpReferenceResolver.Resolve(manifest);

        AssertFalse(result.HasErrors, "Existing local DLL references should resolve.");
        AssertSequence(
            [ResolvedReferenceKind.LocalAssembly, ResolvedReferenceKind.LocalAssembly],
            result.References.Select(reference => reference.Kind).ToArray());
        AssertSequence(["Legacy.Billing", "Legacy.Reporting"], result.References.Select(reference => reference.Identity).ToArray());
        AssertSequence(["lib/Legacy.Billing.dll", "lib/Legacy.Reporting.dll"], result.References.Select(reference => reference.RelativePath ?? string.Empty).ToArray());
        AssertTrue(result.References.All(reference => reference.Path is not null && Path.IsPathFullyQualified(reference.Path)), "Local references should have full paths.");
    });
}

static void ReferenceResolverReportsMissingLocalDllDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingReferences"

            [references]
            paths = [
              "lib/Missing.dll"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = TypeSharpReferenceResolver.Resolve(manifest);

        AssertTrue(result.HasErrors, "Missing local DLL references should produce diagnostics.");
        AssertEqual(0, result.References.Count);
        var diagnostic = result.Diagnostics.Single();
        AssertEqual("TS2401", diagnostic.Code);
        AssertEqual("Referenced assembly path 'lib/Missing.dll' does not exist.", diagnostic.Message);
        AssertEqual(Path.GetFullPath(manifestPath), diagnostic.File);
    });
}

static void ReferenceResolverReportsUnsupportedPackageDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "PackageReferences"

            [references]
            packages = [
              "Newtonsoft.Json:13.0.3",
              "Newtonsoft.Json:13.0.3"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var result = TypeSharpReferenceResolver.Resolve(manifest);

        AssertTrue(result.HasErrors, "NuGet package references should produce an unsupported diagnostic until restore is implemented.");
        AssertEqual(0, result.References.Count);
        var diagnostic = result.Diagnostics.Single();
        AssertEqual("TS2405", diagnostic.Code);
        AssertEqual("NuGet package reference 'Newtonsoft.Json:13.0.3' is not supported by the current compiler.", diagnostic.Message);
        AssertEqual(Path.GetFullPath(manifestPath), diagnostic.File);
    });
}

static void MetadataReaderCreatesFrameworkAssemblyPlaceholders()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "FrameworkMetadata"

            [references]
            assemblies = [
              "System.Core",
              "System"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var references = TypeSharpReferenceResolver.Resolve(manifest);
        var metadata = TypeSharpMetadataReader.Read(references);

        AssertFalse(metadata.HasErrors, "Framework metadata placeholders should not produce diagnostics.");
        AssertSequence(["System.Core", "System"], metadata.Assemblies.Select(assembly => assembly.Identity).ToArray());
        AssertSequence(
            [ResolvedReferenceKind.FrameworkAssembly, ResolvedReferenceKind.FrameworkAssembly],
            metadata.Assemblies.Select(assembly => assembly.ReferenceKind).ToArray());
        AssertTrue(metadata.Assemblies.All(assembly => assembly.IsFrameworkAssembly), "Framework placeholders should report framework kind.");
        AssertTrue(metadata.Assemblies.All(assembly => assembly.Path is null), "Framework placeholders should not have local paths.");
    });
}

static void MetadataReaderCreatesLocalAssemblyPlaceholders()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Billing");
        BuildLegacyReferenceDll(root, "Legacy.Reporting");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalMetadata"

            [references]
            paths = [
              "lib/Legacy.Billing.dll",
              "lib/Legacy.Reporting.dll"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var references = TypeSharpReferenceResolver.Resolve(manifest);
        var metadata = TypeSharpMetadataReader.Read(references);

        AssertFalse(metadata.HasErrors, "Readable local metadata placeholders should not produce diagnostics.");
        AssertSequence(["Legacy.Billing", "Legacy.Reporting"], metadata.Assemblies.Select(assembly => assembly.Identity).ToArray());
        AssertSequence(
            [ResolvedReferenceKind.LocalAssembly, ResolvedReferenceKind.LocalAssembly],
            metadata.Assemblies.Select(assembly => assembly.ReferenceKind).ToArray());
        AssertSequence(["lib/Legacy.Billing.dll", "lib/Legacy.Reporting.dll"], metadata.Assemblies.Select(assembly => assembly.RelativePath ?? string.Empty).ToArray());
        AssertTrue(metadata.Assemblies.All(assembly => assembly.IsLocalAssembly), "Local placeholders should report local kind.");
        AssertTrue(metadata.Assemblies.All(assembly => assembly.Path is not null && Path.IsPathFullyQualified(assembly.Path)), "Local placeholders should keep full paths.");
        AssertTrue(metadata.Assemblies.All(assembly => assembly.Types.Any()), "Local metadata should include public type symbols.");
    });
}

static void MetadataReaderIndexesLocalPublicSymbols()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalMetadataSymbols"

            [references]
            paths = [
              "lib/Legacy.Tools.dll"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var references = TypeSharpReferenceResolver.Resolve(manifest);
        var metadata = TypeSharpMetadataReader.Read(references);

        AssertFalse(metadata.HasErrors, "Valid local DLL metadata should be indexed without diagnostics.");
        var assembly = metadata.Assemblies.Single();
        AssertSequence(
            ["Legacy.Tools.LegacyApi", "Legacy.Tools.LegacyParams", "Legacy.Tools.LegacyByRef", "Legacy.Tools.LegacyOverloads", "Legacy.Tools.LegacyParamsOverloads", "Legacy.Tools.LegacyOptional", "Legacy.Tools.LegacyOptionalOverloads", "Legacy.Tools.LegacyNamedOverloads", "Legacy.Tools.LegacyDelegates", "Legacy.Tools.LegacyEvents", "Legacy.Tools.LegacyMarkerAttribute", "Legacy.Tools.LegacyBox`1", "Legacy.Tools.LegacyFormatter", "Legacy.Tools.LegacyFields", "Legacy.Tools.LegacyGenericMethods", "Legacy.Tools.ILegacyNamed", "Legacy.Tools.LegacyNamed"],
            assembly.Types.Select(type => type.FullName).ToArray());

        var legacyApi = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyApi"), "LegacyApi metadata should be present.");
        AssertSequence(["Echo"], legacyApi.Methods.Select(method => method.Name).ToArray());
        AssertSequence(["value"], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string"], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.Type).ToArray());
        AssertEqual(MetadataNullabilityKind.Unknown, legacyApi.Methods.Single().ReturnNullability);
        AssertSequence([MetadataByRefKind.None], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.ByRefKind).ToArray());
        AssertSequence([false], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.IsParams).ToArray());
        AssertSequence([false], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.IsOptional).ToArray());

        var legacyParams = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyParams"), "LegacyParams metadata should be present.");
        var join = Require(legacyParams.Methods.SingleOrDefault(method => method.Name == "Join"), "Join metadata should be present.");
        AssertSequence(["separator", "values"], join.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "string[]"], join.Parameters.Select(parameter => parameter.Type).ToArray());
        AssertSequence([false, true], join.Parameters.Select(parameter => parameter.IsParams).ToArray());
        AssertSequence([false, false], join.Parameters.Select(parameter => parameter.IsOptional).ToArray());

        var legacyOptional = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyOptional"), "LegacyOptional metadata should be present.");
        var format = Require(legacyOptional.Methods.SingleOrDefault(method => method.Name == "Format"), "Format metadata should be present.");
        AssertSequence(["value", "suffix"], format.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "string"], format.Parameters.Select(parameter => parameter.Type).ToArray());
        AssertSequence([false, true], format.Parameters.Select(parameter => parameter.IsOptional).ToArray());

        var legacyFormatter = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyFormatter"), "LegacyFormatter metadata should be present.");
        AssertSequence(["Item", "Prefix"], legacyFormatter.Properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        AssertSequence(["Format"], legacyFormatter.Methods.Select(method => method.Name).ToArray());

        var legacyByRef = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyByRef"), "LegacyByRef metadata should be present.");
        var tryParse = Require(legacyByRef.Methods.SingleOrDefault(method => method.Name == "TryParseCount"), "TryParseCount metadata should be present.");
        AssertSequence(["text", "value"], tryParse.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertEqual(MetadataNullabilityKind.NotApplicable, tryParse.ReturnNullability);
        AssertSequence([MetadataByRefKind.None, MetadataByRefKind.Out], tryParse.Parameters.Select(parameter => parameter.ByRefKind).ToArray());

        var addOne = Require(legacyByRef.Methods.SingleOrDefault(method => method.Name == "AddOne"), "AddOne metadata should be present.");
        AssertSequence([MetadataByRefKind.In], addOne.Parameters.Select(parameter => parameter.ByRefKind).ToArray());

        var increment = Require(legacyByRef.Methods.SingleOrDefault(method => method.Name == "Increment"), "Increment metadata should be present.");
        AssertSequence([MetadataByRefKind.Ref], increment.Parameters.Select(parameter => parameter.ByRefKind).ToArray());

        var legacyDelegates = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyDelegates"), "LegacyDelegates metadata should be present.");
        var apply = Require(legacyDelegates.Methods.SingleOrDefault(method => method.Name == "Apply"), "Apply metadata should be present.");
        AssertSequence(["value", "transform"], apply.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "System.Func`2<string, string>"], apply.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyEvents = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyEvents"), "LegacyEvents metadata should be present.");
        AssertTrue(legacyEvents.Methods.Any(method => method.Name == "Raise"), "LegacyEvents public methods should include Raise while event accessors remain special-name metadata.");

        var legacyMarkerAttribute = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyMarkerAttribute"), "LegacyMarkerAttribute metadata should be present.");
        AssertSequence(["Name"], legacyMarkerAttribute.Properties.Select(property => property.Name).ToArray());

        var legacyBox = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyBox`1"), "LegacyBox<T> metadata should be present.");
        AssertSequence(["Value"], legacyBox.Properties.Select(property => property.Name).ToArray());

        var legacyFields = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyFields"), "LegacyFields metadata should be present.");
        AssertSequence(["InstanceCode", "StaticCode"], legacyFields.Fields.Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        var staticCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "StaticCode"), "StaticCode field metadata should be present.");
        AssertEqual("string", staticCode.Type);
        AssertTrue(staticCode.IsStatic, "StaticCode should be marked static.");
        AssertTrue(staticCode.IsLiteral, "StaticCode should be marked literal.");
        var instanceCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "InstanceCode"), "InstanceCode field metadata should be present.");
        AssertEqual("string", instanceCode.Type);
        AssertFalse(instanceCode.IsStatic, "InstanceCode should not be marked static.");
        AssertFalse(instanceCode.IsLiteral, "InstanceCode should not be marked literal.");

        var legacyGenericMethods = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyGenericMethods"), "LegacyGenericMethods metadata should be present.");
        var identity = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "Identity"), "Identity<T> metadata should be present.");
        AssertEqual("!!0", identity.ReturnType);
        AssertSequence(["value"], identity.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["!!0"], identity.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyNamedInterface = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.ILegacyNamed"), "ILegacyNamed metadata should be present.");
        AssertSequence(["Name"], legacyNamedInterface.Properties.Select(property => property.Name).ToArray());
        var legacyNamed = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyNamed"), "LegacyNamed metadata should be present.");
        AssertSequence(["Name"], legacyNamed.Properties.Select(property => property.Name).ToArray());
    });
}

static void MetadataReaderPreservesReferenceResolutionDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MetadataDiagnostics"

            [references]
            paths = [
              "lib/Missing.dll"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var references = TypeSharpReferenceResolver.Resolve(manifest);
        var metadata = TypeSharpMetadataReader.Read(references);

        AssertTrue(metadata.HasErrors, "Metadata reader should preserve reference resolution diagnostics.");
        AssertEqual(0, metadata.Assemblies.Count);
        AssertEqual("TS2401", metadata.Diagnostics.Single().Code);
        AssertEqual("Referenced assembly path 'lib/Missing.dll' does not exist.", metadata.Diagnostics.Single().Message);
    });
}

static void MetadataReaderReportsMissingLocalMetadataInputs()
{
    WithWorkspace(root =>
    {
        var missingPath = Path.Combine(root, "lib", "Deleted.dll");
        var reference = new ResolvedReference(
            ResolvedReferenceKind.LocalAssembly,
            "Deleted",
            "lib/Deleted.dll",
            missingPath,
            "lib/Deleted.dll");

        var metadata = TypeSharpMetadataReader.Read([reference]);

        AssertTrue(metadata.HasErrors, "Missing local metadata input should produce diagnostics.");
        AssertEqual(0, metadata.Assemblies.Count);
        var diagnostic = metadata.Diagnostics.Single();
        AssertEqual("TS2401", diagnostic.Code);
        AssertEqual("Referenced assembly path 'lib/Deleted.dll' does not exist.", diagnostic.Message);
        AssertEqual(missingPath, diagnostic.File);
    });
}

static void CheckerReportsMissingReferenceDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CheckReferences"

            [references]
            paths = [
              "lib/Missing.dll"
            ]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CheckReferences

            export fun ok(): string = "ok"
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report reference resolver diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2401");
        AssertEqual("Referenced assembly path 'lib/Missing.dll' does not exist.", diagnostic.Message);
        AssertEqual(Path.GetFullPath(manifestPath), diagnostic.File);
    });
}

static void CheckerReportsInvalidByRefInteropDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InvalidByRef"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.InvalidByRef"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidByRef

            import { LegacyByRef } from "Legacy.Tools"

            export fun broken(): int {
              let mut value: int = 0
              LegacyByRef.Increment(out value)
              value
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Invalid C# byref interop should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2403");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("expects parameter 'value' to be passed with 'ref'", diagnostic.Message);
        AssertContains("but the argument uses 'out'", diagnostic.Message);
    });
}

static void CheckerReportsAmbiguousCSharpOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousOverload

            import { LegacyOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyOverloads.Pick(null)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous C# overload interop should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("matches 2 overload candidates", diagnostic.Message);
        AssertContains("make the call unambiguous", diagnostic.Message);
    });
}

static void CSharpOverloadResolverSelectsExactLiteralMatch()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyOverloads.Pick("value")
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyOverloads",
        [
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "int", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(2, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Exact literal type match should narrow overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select one overload candidate.");
    AssertEqual("string", selected.Method.Parameters[0].Type);
}

static void CheckerReportsAmbiguousExpandedParamsOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousExpandedParamsOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousExpandedParamsOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousExpandedParamsOverload

            import { LegacyParamsOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyParamsOverloads.Pick(",", null, null)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous expanded params overload interop should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("matches 2 overload candidates", diagnostic.Message);
        AssertContains("make the call unambiguous", diagnostic.Message);
    });
}

static void CheckerReportsAmbiguousOptionalOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousOptionalOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousOptionalOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousOptionalOverload

            import { LegacyOptionalOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyOptionalOverloads.Pick("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous optional overload interop should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("matches 2 overload candidates", diagnostic.Message);
        AssertContains("make the call unambiguous", diagnostic.Message);
    });
}

static void CheckerReportsUnknownCSharpNullabilityDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnknownNullability"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnknownNullability"
            generatedOutputRoot = "generated"

            [language]
            nullable = "strict"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnknownNullability

            import { LegacyApi } from "Legacy.Tools"

            export fun echo(): string = LegacyApi.Echo("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.HasErrors, "Unknown C# nullability should be a warning, not a build-blocking error.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2404");
        AssertEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("returns reference type 'string' from metadata without nullable annotations", diagnostic.Message);
    });
}

static void CliCheckEmitsJsonReferenceDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CliCheckReferences"

            [references]
            paths = [
              "lib/Missing.dll"
            ]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CliCheckReferences

            export fun ok(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2401\"", error.ToString());
        AssertContains("Referenced assembly path 'lib/Missing.dll' does not exist.", error.ToString());
    });
}

static void CliCheckEmitsJsonDuplicateSourceModuleDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CliDuplicateModules"
            sourceRoots = ["src", "shared"]
            """);
        WriteFile(root, "src/Feature/Config.tysh", "namespace Samples.CliDuplicateModules\n");
        WriteFile(root, "shared/Feature/Config.tysh", "namespace Samples.CliDuplicateModules\n");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0111\"", error.ToString());
        AssertContains("Duplicate source module path 'Feature/Config'", error.ToString());
        AssertContains("\"file\": \"src/Feature/Config.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonUnsupportedPackageDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CliCheckPackages"

            [references]
            packages = [
              "Newtonsoft.Json:13.0.3"
            ]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CliCheckPackages

            export fun ok(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2405\"", error.ToString());
        AssertContains("NuGet package reference 'Newtonsoft.Json:13.0.3' is not supported by the current compiler.", error.ToString());
    });
}

static void CliBuildStopsBeforeEmissionOnReferenceDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildReferences"
            generatedOutputRoot = "generated"

            [references]
            paths = [
              "lib/Missing.dll"
            ]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildReferences

            export fun ok(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2401\"", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when reference diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildReferences.Generated.csproj")), "Build should not emit generated project when reference diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildReferences.dll")), "Build should not emit generated assembly when reference diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnDuplicateSourceModules()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildDuplicateModules"
            sourceRoots = ["src", "shared"]
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Feature/Config.tysh", "namespace Samples.BuildDuplicateModules\n");
        WriteFile(root, "shared/Feature/Config.tysh", "namespace Samples.BuildDuplicateModules\n");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0111\"", error.ToString());
        AssertContains("Duplicate source module path 'Feature/Config'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Feature", "Config.g.cs")), "Build should not emit generated C# when duplicate source modules contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildDuplicateModules.Generated.csproj")), "Build should not emit generated project when duplicate source modules contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildDuplicateModules.dll")), "Build should not emit generated assembly when duplicate source modules contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnPackageDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildPackages"
            generatedOutputRoot = "generated"

            [references]
            packages = [
              "Newtonsoft.Json:13.0.3"
            ]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildPackages

            export fun ok(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2405\"", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when package diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildPackages.Generated.csproj")), "Build should not emit generated project when package diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildPackages.dll")), "Build should not emit generated assembly when package diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnInvalidByRefInterop()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InvalidByRefBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.InvalidByRefBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InvalidByRefBuild

            import { LegacyByRef } from "Legacy.Tools"

            export fun broken(): int {
              let mut value: int = 0
              LegacyByRef.Increment(out value)
              value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2403\"", error.ToString());
        AssertContains("expects parameter 'value' to be passed with 'ref'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when invalid byref diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "InvalidByRefBuild.Generated.csproj")), "Build should not emit generated project when invalid byref diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "InvalidByRefBuild.dll")), "Build should not emit generated assembly when invalid byref diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnAmbiguousCSharpOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousOverloadBuild

            import { LegacyOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyOverloads.Pick(null)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2402\"", error.ToString());
        AssertContains("matches 2 overload candidates", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when ambiguous overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "AmbiguousOverloadBuild.Generated.csproj")), "Build should not emit generated project when ambiguous overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "AmbiguousOverloadBuild.dll")), "Build should not emit generated assembly when ambiguous overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnTypeCheckerDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildTypeDiagnostics"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildTypeDiagnostics

            export fun broken(): string = 42
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2201\"", error.ToString());
        AssertContains("Cannot return expression of type 'int' from function returning 'string'.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when type checker diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildTypeDiagnostics.Generated.csproj")), "Build should not emit generated project when type checker diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildTypeDiagnostics.dll")), "Build should not emit generated assembly when type checker diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNullabilityDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NullabilityDiagnostics"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NullabilityDiagnostics

            export fun broken(): string = null
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2202\"", error.ToString());
        AssertContains("Cannot return null from function returning non-null type 'string'.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when nullability diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NullabilityDiagnostics.Generated.csproj")), "Build should not emit generated project when nullability diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NullabilityDiagnostics.dll")), "Build should not emit generated assembly when nullability diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnPublicBoundaryDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "PublicBoundaryBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.PublicBoundaryBuild

            type LocalAmount = decimal | string

            export fun leak(input: LocalAmount): string = "bad"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2204\"", error.ToString());
        AssertContains("Type-level union cannot appear in public API. Use a nominal union or interface.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when public boundary diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "PublicBoundaryBuild.Generated.csproj")), "Build should not emit generated project when public boundary diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "PublicBoundaryBuild.dll")), "Build should not emit generated assembly when public boundary diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNonExhaustiveMatch()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NonExhaustiveMatchBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NonExhaustiveMatchBuild

            union PaymentStatus {
              Pending
              Paid(at: string)
              Failed(reason: string)
            }

            export fun describe(status: PaymentStatus): string =
              match status {
                Pending => "Waiting"
                Paid(at) => at
              }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2203\"", error.ToString());
        AssertContains("Missing cases: Failed", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when non-exhaustive match diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NonExhaustiveMatchBuild.Generated.csproj")), "Build should not emit generated project when non-exhaustive match diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NonExhaustiveMatchBuild.dll")), "Build should not emit generated assembly when non-exhaustive match diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnUnsupportedExportForwarding()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ExportForwardingBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExportForwardingBuild

            export fun keep(): string = "ok"
            export * from "Prelude"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2003\"", error.ToString());
        AssertContains(DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate, error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when unsupported export forwarding diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "ExportForwardingBuild.Generated.csproj")), "Build should not emit generated project when unsupported export forwarding diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ExportForwardingBuild.dll")), "Build should not emit generated assembly when unsupported export forwarding diagnostics contain errors.");
    });
}

static void ManifestLoaderReportsInvalidManifestShape()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "Invalid"

            [language]
            strict = "yes"
            """);

        var result = TypeSharpManifestLoader.Load(manifestPath);

        AssertTrue(result.HasErrors, "Invalid manifest shape should produce an error.");
        AssertEqual("TS0103", result.Diagnostics.Single().Code);
    });
}

static void CliRunBuildsAndRunsGeneratedNet48Executable()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RunSmoke"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.RunSmoke"
            generatedOutputRoot = "generated"
            main = "Samples.RunSmoke.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RunSmoke

            export fun main(): string = "Hello from TypeSharp run"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath], output, error);

        AssertTrue(File.Exists(Path.Combine(root, "generated", "Program.g.cs")), "Run should emit a generated entry point.");
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RunSmoke.exe")), "Run should build a generated net48 executable.");

        if (exitCode == 0)
        {
            AssertEqual($"Hello from TypeSharp run{Environment.NewLine}", output.ToString());
            AssertEqual(string.Empty, error.ToString());
            return;
        }

        AssertGeneratedExecutableLaunchBlocked(exitCode, output.ToString(), error.ToString(), "RunSmoke");
    });
}

static void CliRunPassesArgumentsToGeneratedMain()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RunArgs"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.RunArgs"
            generatedOutputRoot = "generated"
            main = "Samples.RunArgs.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RunArgs

            export fun main(args: string[]): string = args.Length.ToString()
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath, "--", "alpha", "beta"], output, error);

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        var generatedProgram = File.ReadAllText(Path.Combine(root, "generated", "Program.g.cs"));
        AssertContains("public static string main(string[] args)", generatedSource);
        AssertContains("Module.main(args)", generatedProgram);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RunArgs.exe")), "Run should build a generated net48 executable with argument forwarding.");

        if (exitCode == 0)
        {
            AssertEqual($"2{Environment.NewLine}", output.ToString());
            AssertEqual(string.Empty, error.ToString());
            return;
        }

        AssertGeneratedExecutableLaunchBlocked(exitCode, output.ToString(), error.ToString(), "RunArgs");
    });
}

static void CliRunReportsUnsupportedMainSignature()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RunBadMain"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.RunBadMain"
            generatedOutputRoot = "generated"
            main = "Samples.RunBadMain.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RunBadMain

            export fun main(count: int): string = "bad"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS3500\"", error.ToString());
        AssertContains("must have no parameters or exactly one 'string[]' parameter", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Run should not emit generated C# when main signature is unsupported.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "RunBadMain.Generated.csproj")), "Run should not emit generated project when main signature is unsupported.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RunBadMain.exe")), "Run should not emit generated executable when main signature is unsupported.");
    });
}

static void CliRunRejectsLibraryProjects()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("RunLibrary"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RunLibrary

            export fun greeting(): string = "library"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath], output, error);

        AssertEqual(5, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("typesharp run requires project outputType = \"exe\".", error.ToString());
    });
}

static void CliBuildHonorsReleaseConfiguration()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReleaseBuild"
            targetFramework = "net48"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReleaseBuild

            export fun greeting(): string = "release"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--configuration", "Release"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Release/net48/ReleaseBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Release", "net48", "ReleaseBuild.dll")), "Build should write the generated assembly under the selected Release configuration.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ReleaseBuild.dll")), "Release build should not report or require the Debug assembly path.");
    });
}

static void CliRunHonorsReleaseConfiguration()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReleaseRun"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.ReleaseRun"
            generatedOutputRoot = "generated"
            main = "Samples.ReleaseRun.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReleaseRun

            export fun main(): string = "Release run"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath, "--configuration=Release"], output, error);

        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Release", "net48", "ReleaseRun.exe")), "Run should build the generated executable under the selected Release configuration.");

        if (exitCode == 0)
        {
            AssertEqual($"Release run{Environment.NewLine}", output.ToString());
            AssertEqual(string.Empty, error.ToString());
            return;
        }

        AssertGeneratedExecutableLaunchBlocked(exitCode, output.ToString(), error.ToString(), "ReleaseRun");
    });
}

static void CliBuildHonorsTargetFrameworkOverride()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "TargetOverrideBuild"
            targetFramework = "net481"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TargetOverrideBuild

            export fun greeting(): string = "target"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--target", "net48"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/TargetOverrideBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "TargetOverrideBuild.dll")), "Build should use the selected target framework override for generated assembly output.");
        var projectText = File.ReadAllText(Path.Combine(root, "generated", "TargetOverrideBuild.Generated.csproj"));
        AssertContains("<TargetFramework>net48</TargetFramework>", projectText);
        AssertFalse(projectText.Contains("<TargetFramework>net481</TargetFramework>", StringComparison.Ordinal), "Generated project should not keep the manifest target when CLI --target overrides it.");
    });
}

static void CliRunHonorsTargetFrameworkOverride()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "TargetOverrideRun"
            targetFramework = "net481"
            outputType = "exe"
            rootNamespace = "Samples.TargetOverrideRun"
            generatedOutputRoot = "generated"
            main = "Samples.TargetOverrideRun.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TargetOverrideRun

            export fun main(): string = "Target run"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath, "--target=net48"], output, error);

        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "TargetOverrideRun.exe")), "Run should use the selected target framework override for generated executable output.");
        var projectText = File.ReadAllText(Path.Combine(root, "generated", "TargetOverrideRun.Generated.csproj"));
        AssertContains("<TargetFramework>net48</TargetFramework>", projectText);

        if (exitCode == 0)
        {
            AssertEqual($"Target run{Environment.NewLine}", output.ToString());
            AssertEqual(string.Empty, error.ToString());
            return;
        }

        AssertGeneratedExecutableLaunchBlocked(exitCode, output.ToString(), error.ToString(), "TargetOverrideRun");
    });
}

static void CliBuildHonorsQuietVerbosity()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "QuietBuild"
            targetFramework = "net48"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.QuietBuild

            export fun greeting(): string = "quiet"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--verbosity", "quiet"], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "QuietBuild.dll")), "Quiet build should still emit the generated assembly.");
    });
}

static void CliBuildHonorsMinimalVerbosity()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MinimalBuild"
            targetFramework = "net48"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MinimalBuild

            export fun greeting(): string = "minimal"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--verbosity=minimal"], output, error);

        AssertEqual(0, exitCode);
        AssertEqual($"Generated assembly: bin/Debug/net48/MinimalBuild.dll{Environment.NewLine}", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MinimalBuild.dll")), "Minimal build should emit the generated assembly.");
    });
}

static void LexerHandlesHelloFixtureTokens()
{
    var input = File.ReadAllText(Path.Combine("tests", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh"));
    var lexer = new TypeSharpLexer(input, "input.tysh");
    var tokens = lexer.Lex();

    AssertFalse(lexer.Diagnostics.Any(), "Hello fixture should lex without diagnostics.");
    AssertTrue(tokens.Any(token => token.Kind == SyntaxKind.NamespaceKeyword), "Expected namespace keyword.");
    AssertEqual(
        "spaces=0;newlines=3;lineComments=2;blockComments=0",
        tokens.First(token => token.Kind == SyntaxKind.NamespaceKeyword).LeadingTriviaSummary);
    AssertTrue(tokens.Any(token => token.Kind == SyntaxKind.ImportKeyword), "Expected import keyword.");
    AssertTrue(tokens.Any(token => token.Kind == SyntaxKind.FunKeyword), "Expected fun keyword.");
    AssertTrue(tokens.Any(token => token.Kind == SyntaxKind.InterpolatedStringLiteralToken), "Expected interpolated string token.");
    AssertEqual(SyntaxKind.EndOfFileToken, tokens[^1].Kind);
}

static void ParserParsesHelloFixtureWithoutDiagnostics()
{
    var input = File.ReadAllText(Path.Combine("tests", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh"));
    var result = TypeSharpParser.ParseText(input);

    AssertFalse(result.HasErrors, "Hello fixture should parse without diagnostics.");
    AssertEqual(SyntaxKind.SourceFile, Require(result.Root, "Parser should produce a root syntax node.").Kind);
}

static void ParserParsesModuleDeclarationWithoutDiagnostics()
{
    var result = TypeSharpParser.ParseText("""
        namespace Samples.ModuleSmoke

        export module MathEx {
          public literal Seed = 7

          export fun identity(value: string): string = value
        }
        """);

    AssertFalse(result.HasErrors, "Module declaration should parse without diagnostics.");
    var root = Require(result.Root, "Parser should produce a root syntax node.");
    AssertTrue(root.Children.Any(child => child.Kind == SyntaxKind.ModuleDeclaration), "Parser should produce a module declaration node.");
}

static void ParserFixtureSnapshotsMatch()
{
    var fixtureRoots = new[]
    {
        Path.Combine("tests", "fixtures", "parser", "positive"),
        Path.Combine("tests", "fixtures", "parser", "negative")
    };

    foreach (var fixtureRoot in fixtureRoots)
    {
        foreach (var fixtureDirectory in Directory.EnumerateDirectories(fixtureRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var inputPath = Path.Combine(fixtureDirectory, ParserFixtureConventions.InputFileName);
            var diagnosticsPath = Path.Combine(fixtureDirectory, ParserFixtureConventions.ExpectedDiagnosticsFileName);
            var treePath = Path.Combine(fixtureDirectory, ParserFixtureConventions.ExpectedTreeFileName);

            var result = TypeSharpParser.ParseText(File.ReadAllText(inputPath), ParserFixtureConventions.InputFileName);
            var actualDiagnostics = DiagnosticJsonFormatter.ToJson(result.Diagnostics);
            var actualTree = SyntaxSnapshotWriter.Write(Require(result.Root, "Parser should produce a root syntax node."));

            AssertTextFileEquals(diagnosticsPath, actualDiagnostics);
            AssertTextFileEquals(treePath, actualTree);
        }
    }
}

static void BinderFixtureDiagnosticsMatch()
{
    var fixtureRoots = new[]
    {
        BinderFixtureConventions.PositiveRoot,
        BinderFixtureConventions.NegativeRoot
    };

    foreach (var fixtureRoot in fixtureRoots)
    {
        foreach (var fixtureDirectory in Directory.EnumerateDirectories(fixtureRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var inputPath = Path.Combine(fixtureDirectory, BinderFixtureConventions.InputFileName);
            var diagnosticsPath = Path.Combine(fixtureDirectory, BinderFixtureConventions.ExpectedDiagnosticsFileName);

            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(inputPath), BinderFixtureConventions.InputFileName);
            AssertFalse(parseResult.HasErrors, $"Binder fixture {fixtureDirectory} should parse without diagnostics.");
            var root = Require(parseResult.Root, "Parser should produce a root syntax node.");

            var bindingResult = TypeSharpBinder.Bind(root, BinderFixtureConventions.InputFileName);
            var actualDiagnostics = DiagnosticJsonFormatter.ToJson(bindingResult.Diagnostics);

            AssertTextFileEquals(diagnosticsPath, actualDiagnostics);
        }
    }
}

static void TypeCheckerFixtureDiagnosticsMatch()
{
    var fixtureRoots = new[]
    {
        TypeCheckerFixtureConventions.PositiveRoot,
        TypeCheckerFixtureConventions.NegativeRoot
    };

    foreach (var fixtureRoot in fixtureRoots)
    {
        foreach (var fixtureDirectory in Directory.EnumerateDirectories(fixtureRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var inputPath = Path.Combine(fixtureDirectory, TypeCheckerFixtureConventions.InputFileName);
            var diagnosticsPath = Path.Combine(fixtureDirectory, TypeCheckerFixtureConventions.ExpectedDiagnosticsFileName);

            var parseResult = TypeSharpParser.ParseText(File.ReadAllText(inputPath), TypeCheckerFixtureConventions.InputFileName);
            AssertFalse(parseResult.HasErrors, $"Type checker fixture {fixtureDirectory} should parse without diagnostics.");
            var root = Require(parseResult.Root, "Parser should produce a root syntax node.");

            var bindingResult = TypeSharpBinder.Bind(root, TypeCheckerFixtureConventions.InputFileName);
            AssertFalse(bindingResult.HasErrors, $"Type checker fixture {fixtureDirectory} should bind without diagnostics.");

            var typeCheckResult = TypeSharpTypeChecker.Check(root, TypeCheckerFixtureConventions.InputFileName);
            var actualDiagnostics = DiagnosticJsonFormatter.ToJson(typeCheckResult.Diagnostics);

            AssertTextFileEquals(diagnosticsPath, actualDiagnostics);
        }
    }
}

static void CSharpBackendFixtureSnapshotsMatch()
{
    foreach (var fixtureDirectory in Directory.EnumerateDirectories(CSharpBackendFixtureConventions.PositiveRoot).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
    {
        var inputPath = Path.Combine(fixtureDirectory, CSharpBackendFixtureConventions.InputFileName);
        var csharpPath = Path.Combine(fixtureDirectory, CSharpBackendFixtureConventions.ExpectedCSharpFileName);

        var parseResult = TypeSharpParser.ParseText(File.ReadAllText(inputPath), CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(parseResult.HasErrors, $"C# backend fixture {fixtureDirectory} should parse without diagnostics.");
        var root = Require(parseResult.Root, "Parser should produce a root syntax node.");

        var bindingResult = TypeSharpBinder.Bind(root, CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(bindingResult.HasErrors, $"C# backend fixture {fixtureDirectory} should bind without diagnostics.");

        var typeCheckResult = TypeSharpTypeChecker.Check(root, CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(typeCheckResult.HasErrors, $"C# backend fixture {fixtureDirectory} should type-check without diagnostics.");

        var actualCSharp = CSharpSourceBackend.Emit(root);
        AssertTextFileEquals(csharpPath, actualCSharp);
    }
}

static void GeneratedCSharpCompilesInNet48Project()
{
    WithWorkspace(root =>
    {
        var inputPath = Path.Combine("tests", "fixtures", "backend", "csharp", "positive", "0004-block-local", CSharpBackendFixtureConventions.InputFileName);
        var parseResult = TypeSharpParser.ParseText(File.ReadAllText(inputPath), CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(parseResult.HasErrors, "Generated C# smoke input should parse without diagnostics.");
        var syntaxRoot = Require(parseResult.Root, "Parser should produce a root syntax node.");

        var bindingResult = TypeSharpBinder.Bind(syntaxRoot, CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(bindingResult.HasErrors, "Generated C# smoke input should bind without diagnostics.");

        var typeCheckResult = TypeSharpTypeChecker.Check(syntaxRoot, CSharpBackendFixtureConventions.InputFileName);
        AssertFalse(typeCheckResult.HasErrors, "Generated C# smoke input should type-check without diagnostics.");

        var projectRoot = Path.Combine(root, "GeneratedSmoke");
        Directory.CreateDirectory(projectRoot);
        WriteFile(projectRoot, "GeneratedSmoke.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>GeneratedSmoke</AssemblyName>
              </PropertyGroup>
            </Project>
            """);
        WriteFile(projectRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(projectRoot, "Module.g.cs", CSharpSourceBackend.Emit(syntaxRoot));

        var build = RunProcess("dotnet", "build GeneratedSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources", projectRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"Generated C# net48 project should compile.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void BinderBindsLocalDeclarationsWithoutDiagnostics()
{
    var result = TypeSharpParser.ParseText("""
        namespace Samples.Binding

        fun identity(value: string): string = value

        fun main(): string {
          let name = "TypeSharp"
          identity(name)
        }
        """);

    AssertFalse(result.HasErrors, "Positive binder input should parse without errors.");
    var binding = TypeSharpBinder.Bind(Require(result.Root, "Parser should produce a root syntax node."), "input.tysh");

    AssertFalse(binding.HasErrors, "Binder should resolve local function, parameter, and let references.");
    AssertTrue(binding.Symbols.Any(symbol => symbol.Kind == BoundSymbolKind.Function && symbol.Name == "identity"), "Binder should expose function symbols.");
    AssertTrue(binding.Symbols.Any(symbol => symbol.Kind == BoundSymbolKind.Local && symbol.Name == "name"), "Binder should expose local let symbols.");
}

static void SemanticModelResolvesSymbolsAtSourcePositions()
{
    var model = TypeSharpSemanticModel.AnalyzeText("""
        namespace Samples.Semantics

        fun identity(value: string): string = value

        fun main(): string {
          let name = "TypeSharp"
          identity(name)
        }
        """, "input.tysh");

    AssertFalse(model.HasErrors, "Semantic model should include binder and type-checker diagnostics without errors for clean input.");
    AssertTrue(model.Symbols.Any(symbol => symbol.Kind == BoundSymbolKind.Function && symbol.Name == "identity"), "Semantic model should expose bound function symbols.");

    var reference = Require(model.FindSymbolAt(new SourcePosition(7, 13)), "Semantic model should resolve the local name reference.");
    AssertEqual("name", reference.Name);
    AssertEqual(BoundSymbolKind.Local, reference.Kind);
    AssertEqual(new SourcePosition(6, 7), reference.SymbolSpan.Start);
    AssertEqual(new SourcePosition(7, 12), reference.TargetSpan.Start);

    var builtIn = Require(model.FindSymbolAt(new SourcePosition(3, 21)), "Semantic model should resolve built-in type references.");
    AssertEqual("string", builtIn.Name);
    AssertTrue(builtIn.IsBuiltIn, "Built-in types should be marked separately from source symbols.");
}

static void CheckerReportsUnresolvedNameDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("BindError"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BindError

            fun echo(value: MissingType): string = value

            fun main(): string = missingName
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report binder diagnostics.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2001").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(2, diagnostics.Length);
        AssertEqual("src/Main.tysh", diagnostics[0].File);
        AssertEqual("Unresolved name 'MissingType'.", diagnostics[0].Message);
        AssertEqual("Unresolved name 'missingName'.", diagnostics[1].Message);
    });
}

static void CheckerReportsDuplicateSymbolDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DuplicateSymbols"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DuplicateSymbols

            fun echo(): string = "one"
            fun echo(): string = "two"

            record Customer {
              Name: string
            }

            record Customer {
              Name: string
            }

            fun local(value: string, value: string): string = value
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report duplicate binder diagnostics.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2002").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(3, diagnostics.Length);
        AssertEqual("Duplicate symbol 'echo' in the same scope.", diagnostics[0].Message);
        AssertEqual("Duplicate symbol 'Customer' in the same scope.", diagnostics[1].Message);
        AssertEqual("Duplicate symbol 'value' in the same scope.", diagnostics[2].Message);
    });
}

static void CheckerReportsImportAliasConflictDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ImportAliasConflict"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportAliasConflict

            import { StringBuilder as TextBuilder } from "System.Text"

            record TextBuilder {
              Value: string
            }

            import * as Text from "System.Text"
            import * as Text from "System.IO"

            fun describe(value: TextBuilder): string = value.ToString()
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report import alias binder conflicts.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2002").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(2, diagnostics.Length);
        AssertEqual("Duplicate symbol 'TextBuilder' in the same scope.", diagnostics[0].Message);
        AssertEqual("Duplicate symbol 'Text' in the same scope.", diagnostics[1].Message);
    });
}

static void CheckerReportsUnsupportedExportForwardingDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ExportForwarding"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExportForwarding

            export fun keep(): string = "ok"
            export { keep as alias }
            export type { Customer } from "Models"
            export * from "Prelude"
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report parser-visible but unsupported export specifier declarations.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2003").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(3, diagnostics.Length);
        AssertEqual(DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate, diagnostics[0].Message);
        AssertEqual(DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate, diagnostics[1].Message);
        AssertEqual(DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate, diagnostics[2].Message);
        AssertEqual("src/Main.tysh", diagnostics[0].File);
    });
}

static void TypeCheckerAcceptsBasicAnnotations()
{
    var result = TypeSharpParser.ParseText("""
        namespace Samples.TypeCheck

        fun identity(value: string): string = value

        fun main(): string {
          let greeting: string = "hello"
          identity(greeting)
        }
        """);

    AssertFalse(result.HasErrors, "Positive type checker input should parse without errors.");
    var root = Require(result.Root, "Parser should produce a root syntax node.");
    var binding = TypeSharpBinder.Bind(root, "input.tysh");
    AssertFalse(binding.HasErrors, "Positive type checker input should bind without errors.");

    var typeCheck = TypeSharpTypeChecker.Check(root, "input.tysh");

    AssertFalse(typeCheck.HasErrors, "Type checker should accept simple literal/reference annotations.");
}

static void InferenceEngineInfersLocalExpressionGraph()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("InferenceGraph"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InferenceGraph

            fun seed(): int = 1

            fun badLiteralFlow(): string {
              let first = 1
              let second = first
              second
            }

            fun badCallFlow(): string {
              let value = seed()
              value
            }

            fun badComparisonFlow(): int {
              let same = seed() == 1
              same
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2201").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();

        AssertEqual(3, diagnostics.Length);
        AssertEqual("Cannot return expression of type 'int' from function returning 'string'.", diagnostics[0].Message);
        AssertEqual("Cannot return expression of type 'int' from function returning 'string'.", diagnostics[1].Message);
        AssertEqual("Cannot return expression of type 'bool' from function returning 'int'.", diagnostics[2].Message);
    });
}

static void CheckerReportsTypeMismatchDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("TypeMismatch"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TypeMismatch

            fun stringFromNumber(): string = 42

            fun local(): int {
              let answer: int = "forty two"
              answer
            }

            fun missingNullGuard(): string = null
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report type checker diagnostics.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2201").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(2, diagnostics.Length);
        AssertEqual("Cannot return expression of type 'int' from function returning 'string'.", diagnostics[0].Message);
        AssertEqual("Cannot assign expression of type 'string' to 'int'.", diagnostics[1].Message);

        var nullabilityDiagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2202");
        AssertEqual("Cannot return null from function returning non-null type 'string'.", nullabilityDiagnostic.Message);
    });
}

static void CheckerReportsParserDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ParseError"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ParseError

            export fun broken(name: string): string
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report parser diagnostics.");
        AssertEqual("TS1001", result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS1001").Code);
    });
}

static void CliCheckSucceedsOnParseCleanProject()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ParseClean"));
        WriteFile(root, "src/Main.tysh", File.ReadAllText(Path.Combine("tests", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh")));
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertEqual(string.Empty, error.ToString());
    });
}

static void CliCheckEmitsJsonParserDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ParseJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ParseJson

            export fun broken(name: string): string
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS1001\"", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonTypeCheckerDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("TypeJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TypeJson

            export fun broken(): string = 42
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2201\"", error.ToString());
        AssertContains("Cannot return expression of type 'int' from function returning 'string'.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonNullabilityDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("NullabilityJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NullabilityJson

            export fun broken(): string = null
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2202\"", error.ToString());
        AssertContains("Cannot return null from function returning non-null type 'string'.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonStructuralDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("StructuralJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.StructuralJson

            record HasAge(Age: int)

            type Named = { Name: string }

            fun broken(): Named = HasAge(42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2201\"", error.ToString());
        AssertContains("Type 'HasAge' is missing required member 'Name' for structural type 'Named'.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonPublicBoundaryDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("PublicBoundaryJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.PublicBoundaryJson

            type LocalAmount = decimal | string

            export fun leak(input: LocalAmount): string = "bad"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2204\"", error.ToString());
        AssertContains("Type-level union cannot appear in public API. Use a nominal union or interface.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonDuplicateSymbolDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DuplicateJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DuplicateJson

            export fun keep(): string = "one"
            export fun keep(): string = "two"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2002\"", error.ToString());
        AssertContains("Duplicate symbol 'keep' in the same scope.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonImportAliasConflictDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ImportAliasConflictJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportAliasConflictJson

            import { StringBuilder as Builder } from "System.Text"
            import * as Builder from "System.IO"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2002\"", error.ToString());
        AssertContains("Duplicate symbol 'Builder' in the same scope.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonUnsupportedExportForwardingDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("ExportForwardingJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExportForwardingJson

            export fun keep(): string = "ok"
            export { keep } from "Prelude"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2003\"", error.ToString());
        AssertContains(DiagnosticDescriptors.UnsupportedExportForwarding.MessageTemplate, error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonUnsupportedGenericConstraintDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsupportedConstraint"
            diagnosticFormat = "json"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsupportedConstraint

            export fun keep<T>(value: T): T where T: notnull = value
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2205\"", error.ToString());
        AssertContains("Generic constraint 'notnull' cannot be lowered by the C# 7.3 backend.", error.ToString());
    });
}

static void CliCheckEmitsJsonDynamicCapabilityDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DynamicCapabilityJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DynamicCapabilityJson

            export fun leak(value: dynamic): string =
              value.ToString()
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2206\"", error.ToString());
        AssertContains("Dynamic type annotation requires a 'dynamic' function modifier.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonDynamicCallCapabilityDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DynamicCallCapabilityJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DynamicCallCapabilityJson

            dynamic fun readLegacy(value: dynamic): string =
              value.ToString()

            export fun leak(value: string): string =
              readLegacy(value)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2207\"", error.ToString());
        AssertContains("Call to dynamic function 'readLegacy' requires a 'dynamic' function modifier on the containing function.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonCapabilityCallMarkerDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("CapabilityCallMarkerJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CapabilityCallMarkerJson

            reflect fun readProperty(value: string): string =
              value

            export fun leak(value: string): string =
              readProperty(value)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2208\"", error.ToString());
        AssertContains("Call to reflect function 'readProperty' requires a 'reflect' function modifier on the containing function.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonUnknownAccessDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("UnknownAccessJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnknownAccessJson

            export fun leak(value: unknown): string =
              value.Name
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2209\"", error.ToString());
        AssertContains("Unknown value must be narrowed before member or indexer access.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckKeepsWarningsNonblockingByDefault()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "WarningDefaultCheck"
            targetFramework = "net48"
            outputType = "library"
            sourceRoots = ["missing"]
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("warning TS0110", output.ToString());
        AssertEqual(string.Empty, error.ToString());
    });
}

static void CliCheckTreatsWarningsAsErrors()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "WarningAsErrorCheck"
            targetFramework = "net48"
            outputType = "library"
            sourceRoots = ["missing"]
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--warnings-as-errors", "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0110\"", error.ToString());
        AssertContains("\"severity\": \"warning\"", error.ToString());
    });
}

static void CliBuildStopsBeforeEmissionOnWarningsAsErrors()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "WarningAsErrorBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.WarningAsErrorBuild"
            sourceRoots = ["src", "missing"]
            generatedOutputRoot = "generated"

            [tooling]
            diagnosticFormat = "text"
            treatWarningsAsErrors = true
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.WarningAsErrorBuild

            export fun greeting(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("warning TS0110", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when warnings are treated as errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "WarningAsErrorBuild.Generated.csproj")), "Build should not emit generated project when warnings are treated as errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "WarningAsErrorBuild.dll")), "Build should not emit generated assembly when warnings are treated as errors.");
    });
}

static void LspDiagnosticMapperUsesZeroBasedRanges()
{
    var diagnostic = new Diagnostic(
        "TS2201",
        DiagnosticSeverity.Error,
        "Cannot return expression of type 'int' from function returning 'string'.",
        "src/Main.tysh",
        new SourceSpan(new SourcePosition(3, 31), new SourcePosition(3, 33)));

    var lspDiagnostic = LspDiagnosticMapper.ToLspDiagnostic(diagnostic);

    AssertEqual(2, lspDiagnostic.Range.Start.Line);
    AssertEqual(30, lspDiagnostic.Range.Start.Character);
    AssertEqual(2, lspDiagnostic.Range.End.Line);
    AssertEqual(32, lspDiagnostic.Range.End.Character);
    AssertEqual(1, lspDiagnostic.Severity);
    AssertEqual("typesharp", lspDiagnostic.Source);
    AssertEqual("TS2201", lspDiagnostic.Code);
}

static void LanguageServerPublishesDiagnosticsOnDidOpen()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "src", "Main.tysh");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath) ?? root);
        var uri = new Uri(sourcePath).AbsoluteUri;
        var source = """
            namespace Samples.Lsp

            export fun broken(): string = 42
            """;

        using var input = new MemoryStream();
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"method\":\"textDocument/didOpen\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + ",\"languageId\":\"typesharp\",\"version\":1,\"text\":"
                + JsonSerializer.Serialize(source)
                + "}}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var output = new MemoryStream();
        TypeSharpLanguageServer.Run(input, output, root);

        var response = Encoding.UTF8.GetString(output.ToArray());
        AssertContains("\"textDocumentSync\":1", response);
        AssertContains("\"method\":\"textDocument/publishDiagnostics\"", response);
        AssertContains("\"uri\":\"" + uri + "\"", response);
        AssertContains("\"severity\":1", response);
        AssertContains("\"source\":\"typesharp\"", response);
        AssertContains("\"code\":\"TS2201\"", response);
        AssertContains("Cannot return expression of type", response);
    });
}

static void LanguageServerReturnsHoverForBoundSymbols()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "src", "Main.tysh");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath) ?? root);
        var uri = new Uri(sourcePath).AbsoluteUri;
        var source = """
            namespace Samples.Lsp

            export fun greeting(name: string): string = name

            export fun main(): string = greeting("TypeSharp")
            """;

        using var input = new MemoryStream();
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"method\":\"textDocument/didOpen\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + ",\"languageId\":\"typesharp\",\"version\":1,\"text\":"
                + JsonSerializer.Serialize(source)
                + "}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"textDocument/hover\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + "},\"position\":{\"line\":4,\"character\":30}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"textDocument/hover\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(new Uri(Path.Combine(root, "src", "Missing.tysh")).AbsoluteUri)
                + "},\"position\":{\"line\":0,\"character\":0}}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var output = new MemoryStream();
        TypeSharpLanguageServer.Run(input, output, root);

        var response = Encoding.UTF8.GetString(output.ToArray());
        AssertContains("\"hoverProvider\":true", response);
        AssertContains("\"id\":2", response);
        AssertContains("\"kind\":\"markdown\"", response);
        AssertContains("function", response);
        AssertContains("greeting", response);
        AssertContains("src/Main.tysh:3:12", response);
        AssertContains("\"line\":4", response);
        AssertContains("\"character\":28", response);
        AssertContains("\"id\":3,\"result\":null", response);
    });
}

static void LanguageServerReturnsDefinitionForBoundSymbols()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "src", "Main.tysh");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath) ?? root);
        var uri = new Uri(sourcePath).AbsoluteUri;
        var source = """
            namespace Samples.Lsp

            export fun greeting(name: string): string = name

            export fun main(): string = greeting("TypeSharp")
            """;

        using var input = new MemoryStream();
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"method\":\"textDocument/didOpen\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + ",\"languageId\":\"typesharp\",\"version\":1,\"text\":"
                + JsonSerializer.Serialize(source)
                + "}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"textDocument/definition\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + "},\"position\":{\"line\":4,\"character\":30}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"textDocument/definition\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(new Uri(Path.Combine(root, "src", "Missing.tysh")).AbsoluteUri)
                + "},\"position\":{\"line\":0,\"character\":0}}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var output = new MemoryStream();
        TypeSharpLanguageServer.Run(input, output, root);

        var response = Encoding.UTF8.GetString(output.ToArray());
        AssertContains("\"definitionProvider\":true", response);
        AssertContains("\"id\":2", response);
        AssertContains("\"uri\":\"" + uri + "\"", response);
        AssertContains("\"line\":2", response);
        AssertContains("\"character\":11", response);
        AssertContains("\"character\":19", response);
        AssertContains("\"id\":3,\"result\":null", response);
    });
}

static void LanguageServerReturnsCompletionItems()
{
    WithWorkspace(root =>
    {
        var sourcePath = Path.Combine(root, "src", "Main.tysh");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath) ?? root);
        var uri = new Uri(sourcePath).AbsoluteUri;
        var source = "namespace Samples.Lsp\n\nexport fun greeting(): string = \"hello\"\n\nexport fun main(): string = ";

        using var input = new MemoryStream();
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"method\":\"textDocument/didOpen\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + ",\"languageId\":\"typesharp\",\"version\":1,\"text\":"
                + JsonSerializer.Serialize(source)
                + "}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"textDocument/completion\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + "},\"position\":{\"line\":4,\"character\":28}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"textDocument/completion\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(new Uri(Path.Combine(root, "src", "Missing.tysh")).AbsoluteUri)
                + "},\"position\":{\"line\":0,\"character\":0}}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var output = new MemoryStream();
        TypeSharpLanguageServer.Run(input, output, root);

        var response = Encoding.UTF8.GetString(output.ToArray());
        AssertContains("\"completionProvider\"", response);
        AssertContains("\"resolveProvider\":false", response);
        AssertContains("\"id\":2", response);
        AssertContains("\"label\":\"greeting\"", response);
        AssertContains("\"kind\":3", response);
        AssertContains("\"detail\":\"function\"", response);
        AssertContains("\"label\":\"string\"", response);
        AssertContains("\"detail\":\"built-in type\"", response);
        AssertContains("\"id\":3,\"result\":null", response);
    });
}

static void VsCodeExtensionActivatesLspClient()
{
    var extensionRoot = Path.Combine(Directory.GetCurrentDirectory(), "vscode", "typesharp");
    using var packageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(extensionRoot, "package.json")));
    var root = packageJson.RootElement;

    AssertEqual("./extension.js", root.GetProperty("main").GetString());
    AssertTrue(
        root.GetProperty("activationEvents").EnumerateArray().Any(value => value.GetString() == "onLanguage:typesharp"),
        "VS Code extension should activate when a TypeSharp file opens.");

    var configuration = root.GetProperty("contributes").GetProperty("configuration").GetProperty("properties");
    AssertTrue(
        configuration.TryGetProperty("typesharp.languageServer.command", out _),
        "VS Code extension should expose a language server command override.");
    AssertTrue(
        configuration.TryGetProperty("typesharp.languageServer.args", out _),
        "VS Code extension should expose language server argument overrides.");
    AssertTrue(
        configuration.TryGetProperty("typesharp.languageServer.cwd", out _),
        "VS Code extension should expose language server working directory overrides.");

    var extensionSource = File.ReadAllText(Path.Combine(extensionRoot, "extension.js"));
    AssertContains("childProcess.spawn", extensionSource);
    AssertContains("textDocument/didOpen", extensionSource);
    AssertContains("textDocument/didChange", extensionSource);
    AssertContains("textDocument/publishDiagnostics", extensionSource);
    AssertContains("textDocument/hover", extensionSource);
    AssertContains("textDocument/definition", extensionSource);
    AssertContains("textDocument/completion", extensionSource);
    AssertContains("TypeSharp.LanguageServer.dll", extensionSource);
}

static void VsCodeExtensionActivationSmokeRunsInMockedExtensionHost()
{
    var extensionRoot = Path.Combine(Directory.GetCurrentDirectory(), "vscode", "typesharp");
    var result = RunProcess("node", "test/extension-smoke.js", extensionRoot);

    AssertTrue(
        result.ExitCode == 0,
        $"VS Code extension smoke should exercise activation, LSP forwarding, diagnostics, and shutdown.\nSTDOUT:\n{result.StandardOutput}\nSTDERR:\n{result.StandardError}");
}

static void VsCodeExtensionLiveSmokeRunsAgainstBundledLanguageServer()
{
    var extensionRoot = Path.Combine(Directory.GetCurrentDirectory(), "vscode", "typesharp");
    var prepare = RunProcess("npm", "run prepare:server", extensionRoot);
    AssertTrue(
        prepare.ExitCode == 0,
        $"VS Code extension server publish should succeed before live smoke.\nSTDOUT:\n{prepare.StandardOutput}\nSTDERR:\n{prepare.StandardError}");

    var result = RunProcess("npm", "run test:live", extensionRoot);
    AssertTrue(
        result.ExitCode == 0,
        $"VS Code extension live smoke should use the bundled language server for diagnostics, hover, definition, completion, and shutdown.\nSTDOUT:\n{result.StandardOutput}\nSTDERR:\n{result.StandardError}");
}

static void VsCodeExtensionPackageShapeIsStable()
{
    var extensionRoot = Path.Combine(Directory.GetCurrentDirectory(), "vscode", "typesharp");
    using var packageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(extensionRoot, "package.json")));
    var root = packageJson.RootElement;

    var scripts = root.GetProperty("scripts");
    AssertEqual("node --check extension.js", scripts.GetProperty("check").GetString());
    AssertEqual("node --check test/extension-smoke.js", scripts.GetProperty("check:smoke").GetString());
    AssertEqual("node --check test/extension-live-smoke.js", scripts.GetProperty("check:live").GetString());
    AssertEqual("node --check test/extension-host-smoke.js && node --check test/run-extension-host-smoke.js", scripts.GetProperty("check:host").GetString());
    AssertEqual("node test/extension-smoke.js", scripts.GetProperty("test:smoke").GetString());
    AssertEqual("node test/extension-live-smoke.js", scripts.GetProperty("test:live").GetString());
    AssertEqual("node test/run-extension-host-smoke.js", scripts.GetProperty("test:host").GetString());
    AssertEqual(
        "dotnet publish ../../src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj -c Release -o server --nologo",
        scripts.GetProperty("prepare:server").GetString());
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "extension.js")), "VS Code extension entrypoint should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "test", "extension-smoke.js")), "VS Code mocked extension host smoke should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "test", "extension-live-smoke.js")), "VS Code live extension smoke should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "test", "extension-host-smoke.js")), "VS Code Extension Host smoke should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "test", "run-extension-host-smoke.js")), "VS Code Extension Host smoke runner should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "language-configuration.json")), "VS Code language configuration should exist.");
    AssertTrue(
        File.Exists(Path.Combine(extensionRoot, "syntaxes", "typesharp.tmLanguage.json")),
        "VS Code TextMate grammar should exist.");

    var files = root.GetProperty("files")
        .EnumerateArray()
        .Select(value => value.GetString())
        .ToArray();
    AssertTrue(files.Contains("extension.js"), "VS Code package should include the extension entrypoint.");
    AssertTrue(files.Contains("language-configuration.json"), "VS Code package should include language configuration.");
    AssertTrue(files.Contains("syntaxes/**"), "VS Code package should include TextMate grammar assets.");
    AssertTrue(files.Contains("server/**"), "VS Code package should reserve bundled language server assets.");
    AssertFalse(root.TryGetProperty("dependencies", out var dependencies) && dependencies.EnumerateObject().Any(), "VS Code extension should remain dependency-free for the current package smoke.");

    var gitignore = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ".gitignore"));
    AssertContains("vscode/typesharp/server/", gitignore);
}

static void RunnableExampleCatalogSmokeMatrixIsStable()
{
    var catalogRoot = Path.Combine(Directory.GetCurrentDirectory(), "docs", "examples", "runnable");
    var catalogReadme = File.ReadAllText(Path.Combine(catalogRoot, "README.md"));
    var projects = new[]
    {
        "console-hello",
        "library-public-api",
        "csharp-interop",
        "host-aspnet-wcf",
        "host-worker",
        "diagnostics-null-safety"
    };

    foreach (var project in projects)
    {
        var projectRoot = Path.Combine(catalogRoot, project);
        AssertTrue(Directory.Exists(projectRoot), $"Runnable example '{project}' should exist.");
        AssertTrue(File.Exists(Path.Combine(projectRoot, "README.md")), $"Runnable example '{project}' should have a README.");
        AssertTrue(File.Exists(Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName)), $"Runnable example '{project}' should have a TypeSharp manifest.");
        AssertTrue(
            Directory.EnumerateFiles(Path.Combine(projectRoot, "src"), "*.tysh", SearchOption.AllDirectories).Any(),
            $"Runnable example '{project}' should have TypeSharp source files.");
        AssertContains($"[{project}]({project}/README.md)", catalogReadme);
    }

    AssertContains("typesharp check", catalogReadme);
    AssertContains("typesharp build", catalogReadme);
    AssertContains("typesharp run", catalogReadme);
    AssertContains("dotnet build legacy-src", catalogReadme);
    AssertContains("dotnet build host", catalogReadme);
    AssertContains("ASP.NET", catalogReadme);
    AssertContains("WCF", catalogReadme);
    AssertContains("TS2202", catalogReadme);
}

static void RunnableExampleProjectCommandsAreSmokeTested()
{
    WithWorkspace(root =>
    {
        var sourceRoot = Path.Combine(Directory.GetCurrentDirectory(), "docs", "examples", "runnable");
        var examplesRoot = Path.Combine(root, "runnable");
        CopyDirectory(sourceRoot, examplesRoot);

        SmokeConsoleHelloExample(Path.Combine(examplesRoot, "console-hello"));
        SmokeLibraryPublicApiExample(Path.Combine(examplesRoot, "library-public-api"));
        SmokeCSharpInteropExample(Path.Combine(examplesRoot, "csharp-interop"));
        SmokeHostAspNetWcfExample(Path.Combine(examplesRoot, "host-aspnet-wcf"));
        SmokeHostWorkerExample(Path.Combine(examplesRoot, "host-worker"));
        SmokeDiagnosticsNullSafetyExample(Path.Combine(examplesRoot, "diagnostics-null-safety"));
    });
}

static void SmokeConsoleHelloExample(string projectRoot)
{
    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    RunCliCommand(["check", manifestPath], expectedExitCode: 0);
    RunCliCommand(["build", manifestPath], expectedExitCode: 0);

    var output = new StringWriter();
    var error = new StringWriter();
    var runExitCode = TypeSharpCli.Run(["run", manifestPath], output, error);
    if (runExitCode == 0)
    {
        AssertEqual($"Hello, TypeSharp{Environment.NewLine}", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        return;
    }

    AssertEqual(4, runExitCode);
    AssertContains("Could not run generated executable", error.ToString());
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "generated", "bin", "Debug", "net48", "ConsoleHello.exe")),
        "Console example should still produce the generated executable before environment launch failures.");
}

static void SmokeLibraryPublicApiExample(string projectRoot)
{
    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    RunCliCommand(["check", manifestPath], expectedExitCode: 0);
    RunCliCommand(["build", manifestPath], expectedExitCode: 0);
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "generated", "bin", "Debug", "net48", "LibraryPublicApi.dll")),
        "Library public API example should build a generated net48 assembly.");
}

static void SmokeCSharpInteropExample(string projectRoot)
{
    var legacyBuild = RunProcess(
        "dotnet",
        "build legacy-src/Legacy.Tools.csproj --nologo --verbosity quiet --ignore-failed-sources",
        projectRoot);
    AssertTrue(
        legacyBuild.ExitCode == 0,
        $"C# interop legacy DLL should compile.\nSTDOUT:\n{legacyBuild.StandardOutput}\nSTDERR:\n{legacyBuild.StandardError}");
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "lib", "Legacy.Tools.dll")),
        "C# interop example should build lib/Legacy.Tools.dll.");

    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    RunCliCommand(["check", manifestPath], expectedExitCode: 0);
    RunCliCommand(["build", manifestPath], expectedExitCode: 0);
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "generated", "bin", "Debug", "net48", "CSharpInterop.dll")),
        "C# interop example should build a generated net48 assembly.");
}

static void SmokeHostWorkerExample(string projectRoot)
{
    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    RunCliCommand(["build", manifestPath], expectedExitCode: 0);
    CopyRuntimeDependenciesToExample(projectRoot);
    var hostProject = File.ReadAllText(Path.Combine(projectRoot, "host", "WorkerHostSmoke.csproj"));
    AssertContains("<Reference Include=\"TypeSharp.Core\">", hostProject);
    AssertContains("<Reference Include=\"TypeSharp.Runtime\">", hostProject);
    var hostSource = File.ReadAllText(Path.Combine(projectRoot, "host", "WorkerSmoke.cs"));
    AssertContains("TypeSharp.Core.Result<string, string>", hostSource);
    AssertContains("TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion", hostSource);

    var hostBuild = RunProcess(
        "dotnet",
        "build host/WorkerHostSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources",
        projectRoot);
    AssertTrue(
        hostBuild.ExitCode == 0,
        $"Host worker example should compile after TypeSharp build.\nSTDOUT:\n{hostBuild.StandardOutput}\nSTDERR:\n{hostBuild.StandardError}");
}

static void SmokeHostAspNetWcfExample(string projectRoot)
{
    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    RunCliCommand(["build", manifestPath], expectedExitCode: 0);
    CopyRuntimeDependenciesToExample(projectRoot);
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "generated", "bin", "Debug", "net48", "HostAspNetWcf.dll")),
        "ASP.NET/WCF host example should build a generated net48 assembly.");
    var hostProject = File.ReadAllText(Path.Combine(projectRoot, "host", "AspNetWcfHostSmoke.csproj"));
    AssertContains("<Reference Include=\"TypeSharp.Core\">", hostProject);
    AssertContains("<Reference Include=\"TypeSharp.Runtime\">", hostProject);
    AssertTrue(
        File.Exists(Path.Combine(projectRoot, "host", "web.config")),
        "ASP.NET/WCF host example should include a web.config deployment-shape placeholder.");
    var hostSource = File.ReadAllText(Path.Combine(projectRoot, "host", "AspNetWcfSmoke.cs"));
    AssertContains("ClientBase<IGreetingService>", hostSource);
    AssertContains("Channel.GetGreeting()", hostSource);
    AssertContains("TypeSharp.Core.Option<string>", hostSource);
    AssertContains("TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework", hostSource);
    var webConfig = File.ReadAllText(Path.Combine(projectRoot, "host", "web.config"));
    AssertContains("<basicHttpBinding>", webConfig);
    AssertContains("<client>", webConfig);
    AssertContains("contract=\"Samples.Runnable.HostAspNetWcf.Host.IGreetingService\"", webConfig);

    var hostBuild = RunProcess(
        "dotnet",
        "build host/AspNetWcfHostSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources",
        projectRoot);
    AssertTrue(
        hostBuild.ExitCode == 0,
        $"ASP.NET/WCF host example should compile after TypeSharp build.\nSTDOUT:\n{hostBuild.StandardOutput}\nSTDERR:\n{hostBuild.StandardError}");
}

static void CopyRuntimeDependenciesToExample(string projectRoot)
{
    var coreAssemblyPath = BuildRepositoryAssembly(
        "src/TypeSharp.Core/TypeSharp.Core.csproj",
        "src/TypeSharp.Core/bin/Debug/net48/TypeSharp.Core.dll");
    var runtimeAssemblyPath = BuildRepositoryAssembly(
        "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
        "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
    var libRoot = Path.Combine(projectRoot, "lib");
    Directory.CreateDirectory(libRoot);
    File.Copy(coreAssemblyPath, Path.Combine(libRoot, "TypeSharp.Core.dll"), overwrite: true);
    File.Copy(runtimeAssemblyPath, Path.Combine(libRoot, "TypeSharp.Runtime.dll"), overwrite: true);
}

static void SmokeDiagnosticsNullSafetyExample(string projectRoot)
{
    var manifestPath = Path.Combine(projectRoot, TypeSharpManifestLocator.ManifestFileName);
    var output = new StringWriter();
    var error = new StringWriter();
    var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

    AssertEqual(1, exitCode);
    AssertEqual(string.Empty, output.ToString());
    AssertContains("\"code\": \"TS2202\"", error.ToString());
}

static void DocsSiteContractIsStable()
{
    var siteRoot = Path.Combine(Directory.GetCurrentDirectory(), "docs-site");
    using var packageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(siteRoot, "package.json")));
    var root = packageJson.RootElement;

    AssertEqual("typesharp-docs-site", root.GetProperty("name").GetString());
    AssertEqual("astro build", root.GetProperty("scripts").GetProperty("build").GetString());
    AssertEqual("6.3.5", root.GetProperty("dependencies").GetProperty("astro").GetString());
    AssertEqual("0.39.2", root.GetProperty("dependencies").GetProperty("@astrojs/starlight").GetString());
    AssertTrue(File.Exists(Path.Combine(siteRoot, "package-lock.json")), "Docs site should have a committed npm lockfile.");

    var astroConfig = File.ReadAllText(Path.Combine(siteRoot, "astro.config.mjs"));
    AssertContains("starlight({", astroConfig);
    AssertContains("title: 'TypeSharp'", astroConfig);
    AssertContains("label: 'Learn'", astroConfig);
    AssertContains("label: 'Use TypeSharp'", astroConfig);
    AssertContains("label: 'Reference'", astroConfig);
    AssertContains("label: 'Tools And Project'", astroConfig);
    AssertContains("slug: 'start-here'", astroConfig);
    AssertContains("slug: 'learning-paths'", astroConfig);
    AssertContains("slug: 'language-tour'", astroConfig);
    AssertContains("slug: 'tutorials'", astroConfig);
    AssertContains("slug: 'fundamentals'", astroConfig);
    AssertContains("slug: 'guides'", astroConfig);
    AssertContains("slug: 'dotnet-interop'", astroConfig);
    AssertContains("slug: 'cookbook'", astroConfig);
    AssertContains("slug: 'goal'", astroConfig);
    AssertContains("slug: 'grammar'", astroConfig);
    AssertContains("slug: 'reference'", astroConfig);
    AssertContains("slug: 'api'", astroConfig);
    AssertContains("slug: 'cli'", astroConfig);
    AssertContains("slug: 'diagnostics'", astroConfig);
    AssertContains("slug: 'advanced'", astroConfig);
    AssertContains("slug: 'vscode-lsp'", astroConfig);
    AssertContains("slug: 'migration'", astroConfig);
    AssertContains("slug: 'examples'", astroConfig);
    AssertContains("slug: 'troubleshooting'", astroConfig);

    var contentConfig = File.ReadAllText(Path.Combine(siteRoot, "src", "content.config.ts"));
    AssertContains("docsLoader", contentConfig);
    AssertContains("docsSchema", contentConfig);

    foreach (var page in new[]
    {
        "index",
        "start-here",
        "learning-paths",
        "language-tour",
        "tutorials",
        "fundamentals",
        "guides",
        "dotnet-interop",
        "cookbook",
        "examples",
        "migration",
        "grammar",
        "reference",
        "api",
        "cli",
        "diagnostics",
        "advanced",
        "vscode-lsp",
        "troubleshooting",
        "goal"
    })
    {
        AssertTrue(
            File.Exists(Path.Combine(siteRoot, "src", "content", "docs", $"{page}.md")),
            $"Docs site page '{page}' should exist.");
    }

    var startHerePage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "start-here.md"));
    AssertContains("I Maintain .NET Framework Applications", startHerePage);
    AssertContains("I Know C#", startHerePage);
    AssertContains("I Know F#", startHerePage);
    AssertContains("I Know TypeScript", startHerePage);
    AssertContains("I Am Evaluating The Compiler Or Tooling", startHerePage);

    var learningPathsPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "learning-paths.md"));
    AssertContains("Programming Beginner", learningPathsPage);
    AssertContains("C# And .NET Framework Maintainer", learningPathsPage);
    AssertContains("TypeScript User", learningPathsPage);
    AssertContains("F# Or Functional Programming User", learningPathsPage);
    AssertContains("Advanced Implementer", learningPathsPage);

    var languageTourPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "language-tour.md"));
    AssertContains("Files And Projects", languageTourPage);
    AssertContains("Type-Level Unions", languageTourPage);
    AssertContains("C# Interop", languageTourPage);
    AssertContains("Current Stability Model", languageTourPage);

    var tutorialsPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "tutorials.md"));
    AssertContains("Hello Project", tutorialsPage);
    AssertContains("Library Public API", tutorialsPage);
    AssertContains("C# Interop", tutorialsPage);
    AssertContains("Diagnostics Workflow", tutorialsPage);
    AssertContains("VS Code And LSP Workflow", tutorialsPage);
    AssertContains("Host Compatibility Overview", tutorialsPage);

    var guidesPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "guides.md"));
    AssertContains("Project Structure", guidesPage);
    AssertContains("CLI Workflow", guidesPage);
    AssertContains("C# References And Imports", guidesPage);
    AssertContains("Option, Result, Records, And Unions", guidesPage);

    var dotnetInteropPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "dotnet-interop.md"));
    AssertContains("Generated Target", dotnetInteropPage);
    AssertContains("Supported C# Interop Shape", dotnetInteropPage);
    AssertContains("Host Project Compatibility", dotnetInteropPage);
    AssertContains("Executables And Antivirus", dotnetInteropPage);

    var cookbookPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "cookbook.md"));
    AssertContains("Call A Local C# DLL", cookbookPage);
    AssertContains("Expose A TypeSharp API To C#", cookbookPage);
    AssertContains("Model Nullable Input Safely", cookbookPage);
    AssertContains("Consume Generated DLLs From A Host Project", cookbookPage);

    var fundamentalsPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "fundamentals.md"));
    AssertContains("Values And Functions", fundamentalsPage);
    AssertContains("Structural Shapes Versus Nominal Public API", fundamentalsPage);
    AssertContains("Collections, Pipelines, And Async", fundamentalsPage);

    var referencePage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "reference.md"));
    AssertContains("Declarations", referencePage);
    AssertContains("Expressions", referencePage);
    AssertContains("Types", referencePage);
    AssertContains("Public ABI Rules", referencePage);

    var apiPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "api.md"));
    AssertContains("CLI Commands", apiPage);
    AssertContains("Manifest Reference", apiPage);
    AssertContains("Runtime And Core Libraries", apiPage);
    AssertContains("Generated Assembly Layout", apiPage);

    var advancedPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "advanced.md"));
    AssertContains("Compiler Pipeline", advancedPage);
    AssertContains("Public ABI Contract", advancedPage);
    AssertContains("Metadata Reader And Interop Validation", advancedPage);
    AssertContains("Regression Strategy", advancedPage);

    var troubleshootingPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "troubleshooting.md"));
    AssertContains("The CLI Cannot Find A Manifest", troubleshootingPage);
    AssertContains("Generated `.exe` Is Blocked", troubleshootingPage);
    AssertContains("typesharp explain TS2202", troubleshootingPage);

    var vscodeLspPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "vscode-lsp.md"));
    AssertContains("npm run check", vscodeLspPage);
    AssertContains("npm run check:live", vscodeLspPage);
    AssertContains("npm run prepare:server", vscodeLspPage);
    AssertContains("npm run test:live", vscodeLspPage);
    AssertContains("npm run test:host", vscodeLspPage);
    AssertContains("diagnostics, hover, go-to-definition, completion, and formatting", vscodeLspPage);
}

static void GitHubPagesWorkflowContractIsStable()
{
    var workflowPath = Path.Combine(Directory.GetCurrentDirectory(), ".github", "workflows", "docs.yml");
    var workflow = File.ReadAllText(workflowPath);

    AssertContains("name: Docs", workflow);
    AssertContains("branches:", workflow);
    AssertContains("- main", workflow);
    AssertContains("pages: write", workflow);
    AssertContains("id-token: write", workflow);
    AssertContains("uses: actions/checkout@v5", workflow);
    AssertContains("uses: actions/setup-node@v5", workflow);
    AssertContains("node-version: 24", workflow);
    AssertContains("cache-dependency-path: docs-site/package-lock.json", workflow);
    AssertContains("run: npm ci", workflow);
    AssertContains("run: npm run build", workflow);
    AssertContains("uses: actions/configure-pages@v5", workflow);
    AssertContains("uses: actions/upload-pages-artifact@v5", workflow);
    AssertContains("path: docs-site/dist", workflow);
    AssertContains("uses: actions/deploy-pages@v5", workflow);
    AssertContains("if: github.event_name != 'pull_request'", workflow);
}

static void RunCliCommand(string[] args, int expectedExitCode)
{
    var output = new StringWriter();
    var error = new StringWriter();
    var exitCode = TypeSharpCli.Run(args, output, error);

    AssertTrue(
        exitCode == expectedExitCode,
        $"Expected CLI exit code {expectedExitCode}, got {exitCode}.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
}

static void CliBuildEmitsGeneratedCSharpSource()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildEmit"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildEmit

            export fun greeting(): string = "Hello, TypeSharp"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated C# source: src/Main.g.cs", output.ToString());
        AssertContains("Generated C# project: BuildEmit.Generated.csproj", output.ToString());
        AssertContains("Generated assembly: bin/Debug/net48/BuildEmit.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        var generatedPath = Path.Combine(root, "generated", "src", "Main.g.cs");
        AssertTrue(File.Exists(generatedPath), "Build should write generated C# source.");
        AssertEqual(
            """
            // <auto-generated />

            namespace Samples.BuildEmit
            {
                public static class Module
                {
                    public static string greeting()
                    {
                        return "Hello, TypeSharp";
                    }
                }
            }

            """.Replace("\r\n", "\n", StringComparison.Ordinal),
            File.ReadAllText(generatedPath).Replace("\r\n", "\n", StringComparison.Ordinal));
    });
}

static void CliBuildUsesRootNamespaceForNamespaceLessSource()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RootNamespaceFallback"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.RootNamespaceFallback"
            main = "main"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            export fun main(args: string[]): int =
              args.Length
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RootNamespaceFallback.exe", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        AssertContains("namespace Samples.RootNamespaceFallback", generatedSource);
        AssertContains("public static int main(string[] args)", generatedSource);
        AssertContains("return args.Length;", generatedSource);

        var generatedEntryPoint = File.ReadAllText(Path.Combine(root, "generated", "Program.g.cs"));
        AssertContains("namespace Samples.RootNamespaceFallback", generatedEntryPoint);
        AssertContains("object result = Module.main(args);", generatedEntryPoint);
    });
}

static void CliBuildOmitsAmbientFunctionDeclarations()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbientBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbientBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbientBuild

            ambient public fun HostValue(): int

            export fun local(): int = 42
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        AssertContains("public static int local()", generatedSource);
        AssertFalse(generatedSource.Contains("HostValue", StringComparison.Ordinal), "Ambient functions should not be emitted as generated C# members.");
    });
}

static void CliBuildIgnoresAmbientMainEntryPoint()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbientMain"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.AmbientMain"
            main = "main"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbientMain

            ambient public fun main(args: string[]): int

            export fun fallback(): int = 0
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS3500\"", error.ToString());
        AssertContains("Executable project main 'Samples.AmbientMain.main' was not found.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when only an ambient main signature exists.");
    });
}

static void CliBuildLowersOpenDeclarationsToUsingDirectives()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "OpenUsing"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.OpenUsing"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.OpenUsing

            import { StringBuilder } from "System.Text"
            open System.Text
            open System.Globalization

            export fun greeting(): string = "ok"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        AssertEqual(1, CountOccurrences(generatedSource, "using System.Text;"));
        AssertContains("using System.Globalization;", generatedSource);
        AssertContains("namespace Samples.OpenUsing", generatedSource);
    });
}

static void CliBuildLowersImportAliasToUsingAliasDirective()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportAlias"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportAlias

            import { StringBuilder as Builder } from "System.Text"

            export fun create(): Builder = Builder()
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        AssertContains("using Builder = System.Text.StringBuilder;", generatedSource);
        AssertContains("public static Builder create()", generatedSource);
        AssertContains("return new Builder();", generatedSource);
    });
}

static void CliBuildLowersNamespaceImportToUsingAliasDirective()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NamespaceImport"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NamespaceImport"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NamespaceImport

            import * as Text from "System.Text"

            export fun encodingName(): string = Text.Encoding.UTF8.EncodingName
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        AssertContains("using Text = System.Text;", generatedSource);
        AssertContains("return Text.Encoding.UTF8.EncodingName;", generatedSource);
    });
}

static void CliBuildEmitsGeneratedCSharpProjectScaffold()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ProjectScaffold"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ProjectScaffold"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ProjectScaffold

            export fun greeting(): string = "Hello, scaffold"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated C# project: ProjectScaffold.Generated.csproj", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var projectPath = Path.Combine(root, "generated", "ProjectScaffold.Generated.csproj");
        AssertTrue(File.Exists(projectPath), "Build should write generated C# project scaffold.");
        AssertTextEquals(
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <OutputType>Library</OutputType>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>ProjectScaffold</AssemblyName>
                <RootNamespace>Samples.ProjectScaffold</RootNamespace>
              </PropertyGroup>
            </Project>
            """,
            File.ReadAllText(projectPath));
    });
}

static void CliBuildPropagatesManifestReferencesToGeneratedCSharpProject()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReferencePropagation"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ReferencePropagation"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System.Core"]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReferencePropagation

            export fun greeting(): string = "Hello, references"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated C# project: ReferencePropagation.Generated.csproj", output.ToString());
        AssertContains("Generated assembly: bin/Debug/net48/ReferencePropagation.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var projectPath = Path.Combine(root, "generated", "ReferencePropagation.Generated.csproj");
        var projectText = File.ReadAllText(projectPath).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"System.Core\" />", projectText);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ReferencePropagation.dll")),
            "Generated project build should succeed with valid manifest references.");
    });
}

static void CliBuildCompilesFrameworkStaticMemberCall()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "FrameworkCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.FrameworkCall"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System.Core"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.FrameworkCall

            import { Regex } from "System.Text.RegularExpressions"

            export fun isSlug(): bool = Regex.IsMatch("hello", "^[a-z]+$")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/FrameworkCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using System.Text.RegularExpressions;", generatedSource);
        AssertContains("return Regex.IsMatch(\"hello\", \"^[a-z]+$\");", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "FrameworkCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"System.Core\" />", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "FrameworkCall.dll")),
            "Generated project build should compile a framework static member call.");
    });
}

static void CliBuildCompilesLocalDllStaticMemberCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalDllCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.LocalDllCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalDllCall

            import { LegacyApi } from "Legacy.Tools"

            export fun echo(): string = LegacyApi.Echo("from local dll")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalDllCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyApi.Echo(\"from local dll\");", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "LocalDllCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalDllCall.dll")),
            "Generated project build should compile a local DLL static member call.");
    });
}

static void CliBuildCompilesImportedConstructorAndInstanceMemberCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInstanceCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInstanceCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInstanceCall

            import { LegacyFormatter } from "Legacy.Tools"

            export fun render(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.Format("value")
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInstanceCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var formatter = new LegacyFormatter(\"legacy:\");", generatedSource);
        AssertContains("return formatter.Format(\"value\");", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedInstanceCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInstanceCall.dll")),
            "Generated project build should compile imported constructor and instance member calls.");
    });
}

static void CliBuildCompilesImportedPropertyAccess()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedPropertyAccess"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedPropertyAccess"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedPropertyAccess

            import { LegacyFormatter } from "Legacy.Tools"

            export fun prefix(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.Prefix
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedPropertyAccess.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var formatter = new LegacyFormatter(\"legacy:\");", generatedSource);
        AssertContains("return formatter.Prefix;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedPropertyAccess.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedPropertyAccess.dll")),
            "Generated project build should compile imported property access.");
    });
}

static void CliBuildCompilesImportedFieldAccess()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedFieldAccess"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedFieldAccess"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedFieldAccess

            import { LegacyFields } from "Legacy.Tools"

            export fun codes(): string {
              let fields = LegacyFields()
              LegacyFields.StaticCode + ":" + fields.InstanceCode
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedFieldAccess.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var fields = new LegacyFields();", generatedSource);
        AssertContains("return LegacyFields.StaticCode + \":\" + fields.InstanceCode;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedFieldAccess.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedFieldAccess.dll")),
            "Generated project build should compile imported field access.");
    });
}

static void CliBuildCompilesImportedIndexerAccess()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedIndexerAccess"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedIndexerAccess"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedIndexerAccess

            import { LegacyFormatter } from "Legacy.Tools"

            export fun item(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter[2]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedIndexerAccess.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var formatter = new LegacyFormatter(\"legacy:\");", generatedSource);
        AssertContains("return formatter[2];", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedIndexerAccess.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedIndexerAccess.dll")),
            "Generated project build should compile imported indexer access.");
    });
}

static void CliBuildCompilesImportedParamsCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedParamsCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedParamsCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedParamsCall

            import { LegacyParams } from "Legacy.Tools"

            export fun join(): string = LegacyParams.Join(",", "a", "b", "c")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedParamsCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyParams.Join(\",\", \"a\", \"b\", \"c\");", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedParamsCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedParamsCall.dll")),
            "Generated project build should compile an imported params call.");
    });
}

static void CliBuildCompilesImportedOutCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedOutCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedOutCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedOutCall

            import { LegacyByRef } from "Legacy.Tools"

            export fun parse(): int {
              let mut value: int = 0
              LegacyByRef.TryParseCount("42", out value)
              value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedOutCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var value = 0;", generatedSource);
        AssertContains("LegacyByRef.TryParseCount(\"42\", out value);", generatedSource);
        AssertContains("return value;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedOutCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedOutCall.dll")),
            "Generated project build should compile an imported out call.");
    });
}

static void CliBuildCompilesImportedInCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInCall

            import { LegacyByRef } from "Legacy.Tools"

            export fun next(): int {
              let value: int = 41
              LegacyByRef.AddOne(in value)
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var value = 41;", generatedSource);
        AssertContains("return LegacyByRef.AddOne(in value);", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedInCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInCall.dll")),
            "Generated project build should compile an imported in call.");
    });
}

static void CliBuildCompilesImportedRefCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedRefCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedRefCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedRefCall

            import { LegacyByRef } from "Legacy.Tools"

            export fun increment(): int {
              let mut value: int = 41
              LegacyByRef.Increment(ref value)
              value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedRefCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var value = 41;", generatedSource);
        AssertContains("LegacyByRef.Increment(ref value);", generatedSource);
        AssertContains("return value;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedRefCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedRefCall.dll")),
            "Generated project build should compile an imported ref call.");
    });
}

static void CliBuildCompilesExactOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ExactOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ExactOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExactOverloadMatch

            import { LegacyOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyOverloads.Pick("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ExactOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyOverloads.Pick(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ExactOverloadMatch.dll")),
            "Generated project build should compile an exact overload match.");
    });
}

static void CliBuildCompilesExactExpandedParamsOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ExactExpandedParamsOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ExactExpandedParamsOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExactExpandedParamsOverloadMatch

            import { LegacyParamsOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyParamsOverloads.Pick(",", "a", "b")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ExactExpandedParamsOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyParamsOverloads.Pick(\",\", \"a\", \"b\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ExactExpandedParamsOverloadMatch.dll")),
            "Generated project build should compile an exact expanded params overload match.");
    });
}

static void CliBuildCompilesImportedOptionalCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedOptionalCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedOptionalCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedOptionalCall

            import { LegacyOptional } from "Legacy.Tools"

            export fun format(): string = LegacyOptional.Format("hello")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedOptionalCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyOptional.Format(\"hello\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedOptionalCall.dll")),
            "Generated project build should compile an imported optional call.");
    });
}

static void CliBuildCompilesImportedNamedArgumentCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedNamedArgumentCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedNamedArgumentCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedNamedArgumentCall

            import { LegacyNamedOverloads } from "Legacy.Tools"

            export fun route(): string = LegacyNamedOverloads.Route("/orders", controller: "Orders")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedNamedArgumentCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyNamedOverloads.Route(\"/orders\", controller: \"Orders\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedNamedArgumentCall.dll")),
            "Generated project build should compile an imported named argument call.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaCall

            import { LegacyDelegates } from "Legacy.Tools"

            export fun apply(): string = LegacyDelegates.Apply("value", text => text)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegates.Apply(\"value\", text => text);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaCall.dll")),
            "Generated project build should compile an imported delegate lambda call.");
    });
}

static void CliBuildCompilesImportedEventAddRemoveCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedEventAddRemoveCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedEventAddRemoveCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedEventAddRemoveCall

            import { LegacyEvents } from "Legacy.Tools"

            export fun subscribe(): string {
              let source = LegacyEvents()
              source.Transform += text => text
              source.Transform -= text => text
              source.Transform += text => text
              source.Raise("value")
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedEventAddRemoveCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var source = new LegacyEvents();", generatedSource);
        AssertContains("source.Transform += text => text;", generatedSource);
        AssertContains("source.Transform -= text => text;", generatedSource);
        AssertContains("return source.Raise(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedEventAddRemoveCall.dll")),
            "Generated project build should compile imported event add/remove calls.");
    });
}

static void CliBuildCompilesImportedGenericMethodCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedGenericMethodCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedGenericMethodCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedGenericMethodCall

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun echo(): string =
              LegacyGenericMethods.Identity("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedGenericMethodCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyGenericMethods.Identity(\"value\");", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedGenericMethodCall.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedGenericMethodCall.dll")),
            "Generated project build should compile imported generic method call.");
    });
}

static void CliBuildCompilesImportedInterfaceReference()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInterfaceReference"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInterfaceReference"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInterfaceReference

            import { ILegacyNamed } from "Legacy.Tools"

            export fun describe(value: ILegacyNamed): string =
              value.Name
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Imported interface reference should compile.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInterfaceReference.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("public static string describe(ILegacyNamed value)", generatedSource);
        AssertContains("return value.Name;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedInterfaceReference.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInterfaceReference.dll")),
            "Generated project build should compile imported interface references.");
    });
}

static void CliBuildCompilesImportedAttributeAndGenericTypeReferences()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedAttributeGenericType"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedAttributeGenericType"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedAttributeGenericType

            import { LegacyBox, LegacyMarkerAttribute } from "Legacy.Tools"

            [LegacyMarkerAttribute("generic-type")]
            export fun keep(box: LegacyBox<string>): LegacyBox<string> = box
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedAttributeGenericType.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("public static LegacyBox<string> keep(LegacyBox<string> box)", generatedSource);
        AssertContains("return box;", generatedSource);

        var projectText = File.ReadAllText(Path.Combine(root, "generated", "ImportedAttributeGenericType.Generated.csproj")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("    <Reference Include=\"Legacy.Tools\">", projectText);
        AssertContains("      <HintPath>../lib/Legacy.Tools.dll</HintPath>", projectText);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedAttributeGenericType.dll")),
            "Generated project build should compile imported attribute and generic type references.");
    });
}

static void CliBuildCompilesBasicSemantics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BasicSemantics"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.BasicSemantics"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BasicSemantics

            export fun identity(value: string): string = value

            export fun text(): string {
              let message: string = "hello"
              identity(message)
            }

            export fun count(): int = 42

            export fun enabled(): bool = true
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/BasicSemantics.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static string identity(string value)", generatedSource);
        AssertContains("var message = \"hello\";", generatedSource);
        AssertContains("return identity(message);", generatedSource);
        AssertContains("return 42;", generatedSource);
        AssertContains("return true;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BasicSemantics.dll")),
            "Generated project build should compile basic literals, local binding, and function calls.");
    });
}

static void CliBuildCompilesModuleNamespace()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ModuleNamespace"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ModuleNamespace"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ModuleNamespace

            export module MathEx {
              public literal Seed = 7

              export fun identity(value: string): string = value

              export fun seed(): int = Seed
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ModuleNamespace.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("namespace Samples.ModuleNamespace", generatedSource);
        AssertContains("public static class MathEx", generatedSource);
        AssertContains("public const int Seed = 7;", generatedSource);
        AssertContains("public static string identity(string value)", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "ModuleNamespace.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with module namespace output.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "ModuleConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>ModuleConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="ModuleNamespace">
                  <HintPath>../generated/bin/Debug/net48/ModuleNamespace.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace ModuleConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.ModuleNamespace.MathEx.identity("ok") + ":" + Samples.ModuleNamespace.MathEx.Seed.ToString();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build ModuleConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated public module members.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesCoreOptionResultApis()
{
    WithWorkspace(root =>
    {
        var coreBuild = RunProcess(
            "dotnet",
            "build src/TypeSharp.Core/TypeSharp.Core.csproj --nologo --verbosity quiet --ignore-failed-sources",
            Directory.GetCurrentDirectory());
        AssertTrue(
            coreBuild.ExitCode == 0,
            $"TypeSharp.Core should build before generated API smoke.\nSTDOUT:\n{coreBuild.StandardOutput}\nSTDERR:\n{coreBuild.StandardError}");

        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(
            Path.Combine(Directory.GetCurrentDirectory(), "src", "TypeSharp.Core", "bin", "Debug", "net48", "TypeSharp.Core.dll"),
            Path.Combine(libRoot, "TypeSharp.Core.dll"));

        var manifestPath = WriteManifest(root, """
            [project]
            name = "CoreModels"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.CoreModels"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/TypeSharp.Core.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CoreModels

            import { Option, Result } from "TypeSharp.Core"

            export fun keepOption(value: Option<string>): Option<string> = value

            export fun keepResult(value: Result<int, string>): Result<int, string> = value
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/CoreModels.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using TypeSharp.Core;", generatedSource);
        AssertContains("public static Option<string> keepOption(Option<string> value)", generatedSource);
        AssertContains("public static Result<int, string> keepResult(Result<int, string> value)", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "CoreModels.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with Core model APIs.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "CoreModelsConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>CoreModelsConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="CoreModels">
                  <HintPath>../generated/bin/Debug/net48/CoreModels.dll</HintPath>
                </Reference>
                <Reference Include="TypeSharp.Core">
                  <HintPath>../lib/TypeSharp.Core.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace CoreModelsConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var option = TypeSharp.Core.Option<string>.Some("value");
                        var result = TypeSharp.Core.Result<int, string>.Ok(42);
                        return Samples.CoreModels.Module.keepOption(option).Value
                            + ":"
                            + Samples.CoreModels.Module.keepResult(result).Value.ToString();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build CoreModelsConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated Core model APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesGenericFunctionApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "GenericApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.GenericApi"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.GenericApi

            export fun identity<T>(value: T): T = value
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/GenericApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static T identity<T>(T value)", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "GenericApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with generic function API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "GenericApiConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>GenericApiConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="GenericApi">
                  <HintPath>../generated/bin/Debug/net48/GenericApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace GenericApiConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.GenericApi.Module.identity<string>("value");
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build GenericApiConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated generic function APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesClassDeclarationApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ClassApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Classes"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Classes

            public class Greeter {
              public fun Echo(value: string): string = value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ClassApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public class Greeter", generatedSource);
        AssertContains("public string Echo(string value)", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "ClassApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with class declaration API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "ClassApiConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>ClassApiConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="ClassApi">
                  <HintPath>../generated/bin/Debug/net48/ClassApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace ClassApiConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var greeter = new Samples.Classes.Greeter();
                        return greeter.Echo("value");
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build ClassApiConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated class declaration APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesInterfaceDeclarationApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InterfaceApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Interfaces"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Interfaces

            public interface IGreeter {
              fun Echo(value: string): string
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/InterfaceApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public interface IGreeter", generatedSource);
        AssertContains("string Echo(string value);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "InterfaceApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with interface declaration API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "InterfaceApiConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>InterfaceApiConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="InterfaceApi">
                  <HintPath>../generated/bin/Debug/net48/InterfaceApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace InterfaceApiConsumer
            {
                public sealed class Greeter : Samples.Interfaces.IGreeter
                {
                    public string Echo(string value)
                    {
                        return value;
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build InterfaceApiConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated interface declaration APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesPartialDeclarationApi()
{
    WithWorkspace(root =>
    {
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(runtimeAssemblyPath, Path.Combine(libRoot, "TypeSharp.Runtime.dll"));

        var manifestPath = WriteManifest(root, """
            [project]
            name = "PartialApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Partials"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/TypeSharp.Runtime.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Partials

            export partial module Hooks {
              export fun label(): string = "hook"
            }

            public partial record Customer(Name: string)

            public partial union Status {
              Pending
            }

            public partial class Greeter {
              public fun Echo(value: string): string = value
            }

            public partial interface IGreeter {
              fun Echo(value: string): string
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Partial declaration API build should succeed.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/PartialApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static partial class Hooks", generatedSource);
        AssertContains("public sealed partial class Customer", generatedSource);
        AssertContains("public abstract partial class Status", generatedSource);
        AssertContains("public partial class Greeter", generatedSource);
        AssertContains("public partial interface IGreeter", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "PartialApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with partial declarations.");
    });
}

static void CliBuildCompilesGenericTypeDeclarationApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "GenericTypes"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.GenericTypes"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.GenericTypes

            public class Box<T> {
              public fun Keep(value: T): T = value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/GenericTypes.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public class Box<T>", generatedSource);
        AssertContains("public T Keep(T value)", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "GenericTypes.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with generic type declaration API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "GenericTypesConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>GenericTypesConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="GenericTypes">
                  <HintPath>../generated/bin/Debug/net48/GenericTypes.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace GenericTypesConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var box = new Samples.GenericTypes.Box<string>();
                        return box.Keep("value");
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build GenericTypesConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated generic type declaration APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesGenericConstraintApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "GenericConstraints"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.GenericConstraints"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.GenericConstraints

            export fun keep<T>(value: T): T where T: class + IEntity = value

            public class Repository<T> where T: class + IEntity + new() {
              public fun Keep(value: T): T = value
            }

            public interface IEntity {
              fun Id(): string
            }

            public interface IFactory {
              fun Create<T>(): T where T: class + new()
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/GenericConstraints.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static T keep<T>(T value)", generatedSource);
        AssertContains("where T : class, IEntity", generatedSource);
        AssertContains("public class Repository<T>", generatedSource);
        AssertContains("where T : class, IEntity, new()", generatedSource);
        AssertContains("T Create<T>()", generatedSource);
        AssertContains("where T : class, new();", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "GenericConstraints.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with generic constraint APIs.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "GenericConstraintsConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>GenericConstraintsConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="GenericConstraints">
                  <HintPath>../generated/bin/Debug/net48/GenericConstraints.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace GenericConstraintsConsumer
            {
                public sealed class Entity : Samples.GenericConstraints.IEntity
                {
                    public string Id()
                    {
                        return "id";
                    }
                }

                public sealed class Factory : Samples.GenericConstraints.IFactory
                {
                    public T Create<T>() where T : class, new()
                    {
                        return new T();
                    }
                }

                public static class Consumer
                {
                    public static string Read()
                    {
                        var entity = new Entity();
                        var repository = new Samples.GenericConstraints.Repository<Entity>();
                        var kept = Samples.GenericConstraints.Module.keep<Entity>(repository.Keep(entity));
                        return kept.Id();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build GenericConstraintsConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated generic constraint APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesImmutableRecordApi()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RecordsApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Records"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Records

            public record Customer(Name: string, Age: int)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RecordsApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public sealed class Customer", generatedSource);
        AssertContains("public Customer(string Name, int Age)", generatedSource);
        AssertContains("public string Name { get; }", generatedSource);
        AssertContains("public override bool Equals(object obj)", generatedSource);
        AssertContains("public override int GetHashCode()", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "RecordsApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with immutable record API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "RecordsApiConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>RecordsApiConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="RecordsApi">
                  <HintPath>../generated/bin/Debug/net48/RecordsApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace RecordsApiConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var left = new Samples.Records.Customer("Ada", 42);
                        var right = new Samples.Records.Customer("Ada", 42);
                        return left.Equals(right) && left.GetHashCode() == right.GetHashCode()
                            ? left.Name
                            : string.Empty;
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build RecordsApiConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated immutable record APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesRecordUpdateLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RecordUpdateApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Records"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Records

            public record Customer(Name: string, Age: int)

            export fun Birthday(customer: Customer): Customer = customer with { Age: 43 }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RecordUpdateApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return new Customer(customer.Name, 43);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "RecordUpdateApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with record update lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "RecordUpdateConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>RecordUpdateConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="RecordUpdateApi">
                  <HintPath>../generated/bin/Debug/net48/RecordUpdateApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace RecordUpdateConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var original = new Samples.Records.Customer("Ada", 42);
                        var updated = Samples.Records.Module.Birthday(original);
                        return updated.Name + ":" + updated.Age.ToString();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build RecordUpdateConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated record update lowering.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesRecordExpressionConstruction()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RecordExpressionApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.RecordExpressions"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RecordExpressions

            public record Customer(Name: string, Age: int)

            export fun Create(): Customer = { Name: "Ada", Age: 42 }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RecordExpressionApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return new Customer(\"Ada\", 42);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "RecordExpressionApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with record expression construction.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "RecordExpressionConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>RecordExpressionConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="RecordExpressionApi">
                  <HintPath>../generated/bin/Debug/net48/RecordExpressionApi.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace RecordExpressionConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var customer = Samples.RecordExpressions.Module.Create();
                        return customer.Name + ":" + customer.Age.ToString();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build RecordExpressionConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated record expression construction.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesNominalUnionApi()
{
    WithWorkspace(root =>
    {
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(runtimeAssemblyPath, Path.Combine(libRoot, "TypeSharp.Runtime.dll"));

        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnionApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Unions"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/TypeSharp.Runtime.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Unions

            public union PaymentStatus {
              Pending
              Paid(at: string)
            }

            export fun pending(): PaymentStatus = Pending

            export fun paid(at: string): PaymentStatus = Paid(at)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/UnionApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using TypeSharp.Runtime;", generatedSource);
        AssertContains("return PaymentStatus.Pending;", generatedSource);
        AssertContains("return PaymentStatus.Paid(at);", generatedSource);
        AssertContains("public abstract class PaymentStatus", generatedSource);
        AssertContains("public sealed class PendingCase : PaymentStatus, ITypeSharpUnionCase", generatedSource);
        AssertContains("public sealed class PaidCase : PaymentStatus, ITypeSharpUnionCase", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "UnionApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with nominal union API.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "UnionApiConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>UnionApiConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="UnionApi">
                  <HintPath>../generated/bin/Debug/net48/UnionApi.dll</HintPath>
                </Reference>
                <Reference Include="TypeSharp.Runtime">
                  <HintPath>../lib/TypeSharp.Runtime.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            using TypeSharp.Runtime;

            namespace UnionApiConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        var pending = Samples.Unions.Module.pending();
                        var paid = Samples.Unions.Module.paid("now");
                        var paidCase = (Samples.Unions.PaymentStatus.PaidCase)paid;
                        return TypeSharpUnion.GetCaseName(pending)
                            + ":"
                            + TypeSharpUnion.GetTag(paid).ToString()
                            + ":"
                            + paidCase.at
                            + ":"
                            + TypeSharpUnion.GetPayload<string>(paid);
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build UnionApiConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated nominal union APIs.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesNominalUnionMatchLowering()
{
    WithWorkspace(root =>
    {
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(runtimeAssemblyPath, Path.Combine(libRoot, "TypeSharp.Runtime.dll"));

        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnionMatchApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Unions"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/TypeSharp.Runtime.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Unions

            public union PaymentStatus {
              Pending
              Paid(at: string)
              Failed(reason: string)
            }

            export fun describe(status: PaymentStatus): string =
              match status {
                Pending => "Waiting"
                Paid(at) => $"Paid:{at}"
                Failed(reason) => $"Failed:{reason}"
              }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/UnionMatchApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("TypeSharpPattern.IsPayloadlessCase(__match0, 0)", generatedSource);
        AssertContains("TypeSharpPattern.IsPayloadCase(__match0, 1)", generatedSource);
        AssertContains("var at = TypeSharpPattern.RequirePayload<string>(__match0, 1);", generatedSource);
        AssertContains("throw TypeSharpPattern.NoMatch(__match0);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "UnionMatchApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with nominal union match lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "UnionMatchConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>UnionMatchConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="UnionMatchApi">
                  <HintPath>../generated/bin/Debug/net48/UnionMatchApi.dll</HintPath>
                </Reference>
                <Reference Include="TypeSharp.Runtime">
                  <HintPath>../lib/TypeSharp.Runtime.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace UnionMatchConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.Unions.Module.describe(Samples.Unions.PaymentStatus.Pending)
                            + ":"
                            + Samples.Unions.Module.describe(Samples.Unions.PaymentStatus.Paid("now"))
                            + ":"
                            + Samples.Unions.Module.describe(Samples.Unions.PaymentStatus.Failed("late"));
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build UnionMatchConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated nominal union match lowering.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesTypeLevelUnionNarrowing()
{
    WithWorkspace(root =>
    {
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(runtimeAssemblyPath, Path.Combine(libRoot, "TypeSharp.Runtime.dll"));

        var manifestPath = WriteManifest(root, """
            [project]
            name = "TypeLevelUnionNarrowing"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.TypeLevelUnion"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/TypeSharp.Runtime.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TypeLevelUnion

            type PrimitiveId = string | int

            fun normalizeInternal(id: PrimitiveId): string =
              match id {
                text: string => text
                number: int => number.ToString()
              }

            export fun normalizeText(text: string): string =
              normalizeInternal(text)

            export fun normalizeNumber(number: int): string =
              normalizeInternal(number)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/TypeLevelUnionNarrowing.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("internal static string normalizeInternal(object id)", generatedSource);
        AssertContains("if (__match0 is string text)", generatedSource);
        AssertContains("if (__match0 is int number)", generatedSource);
        AssertContains("throw TypeSharpPattern.NoMatch(__match0);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "TypeLevelUnionNarrowing.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with type-level union narrowing.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "TypeLevelUnionConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>TypeLevelUnionConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="TypeLevelUnionNarrowing">
                  <HintPath>../generated/bin/Debug/net48/TypeLevelUnionNarrowing.dll</HintPath>
                </Reference>
                <Reference Include="TypeSharp.Runtime">
                  <HintPath>../lib/TypeSharp.Runtime.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace TypeLevelUnionConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.TypeLevelUnion.Module.normalizeText("abc")
                            + ":"
                            + Samples.TypeLevelUnion.Module.normalizeNumber(7);
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build TypeLevelUnionConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated type-level union narrowing wrappers.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesAsyncTaskInterop()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AsyncTaskInterop"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AsyncInterop"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AsyncInterop

            import { Task } from "System.Threading.Tasks"

            export async fun greeting(): Task<string> {
              let value = await Task.FromResult("Hello, async")
              value
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/AsyncTaskInterop.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using System.Threading.Tasks;", generatedSource);
        AssertContains("public static async Task<string> greeting()", generatedSource);
        AssertContains("var value = await Task.FromResult(\"Hello, async\");", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "AsyncTaskInterop.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with async Task interop.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "AsyncTaskConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>AsyncTaskConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="AsyncTaskInterop">
                  <HintPath>../generated/bin/Debug/net48/AsyncTaskInterop.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace AsyncTaskConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.AsyncInterop.Module.greeting().GetAwaiter().GetResult();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build AsyncTaskConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated async Task API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesCollectionExpressionLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CollectionExpressions"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Collections"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Collections

            import { List } from "System.Collections.Generic"

            export fun names(): string[] = ["Ada", "Grace"]

            export fun emptyNames(): string[] = []

            export fun numbers(): int[] {
              let values: int[] = [1, 2, 3]
              values
            }

            export fun nameList(): List<string> = ["Ada", "Grace"]

            export fun emptyNameList(): List<string> = []

            export fun numberList(): List<int> {
              let values: List<int> = [1, 2, 3]
              values
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/CollectionExpressions.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return new string[] { \"Ada\", \"Grace\" };", generatedSource);
        AssertContains("return new string[] { };", generatedSource);
        AssertContains("var values = new int[] { 1, 2, 3 };", generatedSource);
        AssertContains("return new List<string> { \"Ada\", \"Grace\" };", generatedSource);
        AssertContains("return new List<string> { };", generatedSource);
        AssertContains("var values = new List<int> { 1, 2, 3 };", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "CollectionExpressions.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with collection expression lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "CollectionConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>CollectionConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="CollectionExpressions">
                  <HintPath>../generated/bin/Debug/net48/CollectionExpressions.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace CollectionConsumer
            {
                public static class Consumer
                {
                    public static bool Read()
                    {
                        var names = Samples.Collections.Module.names();
                        var empty = Samples.Collections.Module.emptyNames();
                        var numbers = Samples.Collections.Module.numbers();
                        var nameList = Samples.Collections.Module.nameList();
                        var emptyNameList = Samples.Collections.Module.emptyNameList();
                        var numberList = Samples.Collections.Module.numberList();
                        return names.Length == 2
                            && names[1] == "Grace"
                            && empty.Length == 0
                            && numbers[2] == 3
                            && nameList.Count == 2
                            && nameList[1] == "Grace"
                            && emptyNameList.Count == 0
                            && numberList[2] == 3;
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build CollectionConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated collection expression API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesPipelineLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "PipelineLowering"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Pipeline"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Pipeline

            export fun increment(value: int): int = value + 1

            export fun add(value: int, amount: int): int = value + amount

            export fun format(value: int): string = value.ToString()

            export fun compute(): string =
              1
              |> increment
              |> add(2)
              |> format
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/PipelineLowering.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return value + 1;", generatedSource);
        AssertContains("return value + amount;", generatedSource);
        AssertContains("return format(add(increment(1), 2));", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "PipelineLowering.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with pipeline lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "PipelineConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>PipelineConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="PipelineLowering">
                  <HintPath>../generated/bin/Debug/net48/PipelineLowering.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace PipelineConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.Pipeline.Module.compute();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build PipelineConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated pipeline API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesLiteralConstants()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LiteralConstants"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.LiteralConstants"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LiteralConstants

            public literal ApiVersion: string = "1.0"
            public literal MaxRetryCount = 3
            literal FeatureEnabled = true

            export fun version(): string = ApiVersion
            export fun retries(): int = MaxRetryCount
            export fun enabled(): bool = FeatureEnabled
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LiteralConstants.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSourcePath = Path.Combine(root, "generated", "src", "Main.g.cs");
        var generatedSource = File.ReadAllText(generatedSourcePath).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public const string ApiVersion = \"1.0\";", generatedSource);
        AssertContains("public const int MaxRetryCount = 3;", generatedSource);
        AssertContains("internal const bool FeatureEnabled = true;", generatedSource);
        AssertContains("return ApiVersion;", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "LiteralConstants.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with literal constants.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "LiteralConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>LiteralConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="LiteralConstants">
                  <HintPath>../generated/bin/Debug/net48/LiteralConstants.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace LiteralConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.LiteralConstants.Module.ApiVersion + ":" + Samples.LiteralConstants.Module.MaxRetryCount.ToString();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build LiteralConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated public literal constants.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildEmitsGeneratedNet48Assembly()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AssemblyEmit"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AssemblyEmit"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AssemblyEmit

            export fun greeting(): string = "Hello, assembly"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/AssemblyEmit.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "AssemblyEmit.dll")),
            "Build should produce generated net48 assembly.");
    });
}

static void GeneratedNet48AssemblyPublicAbiSnapshotIsStable()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "PublicAbiSnapshot"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.PublicAbi"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.PublicAbi

            public record Customer(Name: string, Age: int)

            export fun describe(customer: Customer): string = customer.Name
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());
        AssertContains("Generated assembly: bin/Debug/net48/PublicAbiSnapshot.dll", output.ToString());

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "PublicAbiSnapshot.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated public ABI snapshot assembly.");

        var metadata = TypeSharpMetadataReader.Read(
        [
            new ResolvedReference(
                ResolvedReferenceKind.LocalAssembly,
                "PublicAbiSnapshot",
                generatedAssemblyPath,
                generatedAssemblyPath,
                "generated/bin/Debug/net48/PublicAbiSnapshot.dll")
        ]);

        AssertFalse(metadata.HasErrors, "Generated public ABI snapshot assembly metadata should be readable.");
        var snapshot = TypeSharpPublicAbiChecker.CreateSnapshot(metadata.Assemblies.Single());
        AssertSequence(
        [
            "assembly PublicAbiSnapshot",
            "type Samples.PublicAbi.Customer",
            "  property int Age",
            "  property string Name",
            "  method bool Equals(object obj)",
            "  method int GetHashCode()",
            "type Samples.PublicAbi.Module",
            "  method string describe(Samples.PublicAbi.Customer customer)"
        ], snapshot.Lines);
    });
}

static void CSharpNet48ProjectConsumesGeneratedTypeSharpAssembly()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InteropSource"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.GeneratedInterop"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.GeneratedInterop

            export fun greeting(): string = "Hello from TypeSharp"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());
        AssertContains("Generated assembly: bin/Debug/net48/InteropSource.dll", output.ToString());

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "InteropSource.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "TypeSharp build should produce a generated assembly for the C# consumer.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "ConsumerSmoke.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>ConsumerSmoke</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="InteropSource">
                  <HintPath>../generated/bin/Debug/net48/InteropSource.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """);
        WriteFile(consumerRoot, "NuGet.config", """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
              </packageSources>
            </configuration>
            """);
        WriteFile(consumerRoot, "Consumer.cs", """
            namespace ConsumerSmoke
            {
                public static class Consumer
                {
                    public static string CallTypeSharp()
                    {
                        return Samples.GeneratedInterop.Module.greeting();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build ConsumerSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against the generated TypeSharp assembly.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CompilerCheckPerformanceSmokeStaysBounded()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "PerformanceSmoke"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Performance"
            generatedOutputRoot = "generated"
            """);

        const int sourceFileCount = 80;
        const int functionsPerFile = 15;
        var builder = new StringBuilder();
        for (var fileIndex = 0; fileIndex < sourceFileCount; fileIndex++)
        {
            builder.Clear();
            builder.AppendLine("namespace Samples.Performance");
            builder.AppendLine();

            for (var functionIndex = 0; functionIndex < functionsPerFile; functionIndex++)
            {
                var value = fileIndex * functionsPerFile + functionIndex;
                builder.AppendLine($"export fun value{value}(): int = {value}");
            }

            WriteFile(root, $"src/File{fileIndex:000}.tysh", builder.ToString());
        }

        var stopwatch = Stopwatch.StartNew();
        var result = TypeSharpChecker.Check(manifestPath);
        stopwatch.Stop();

        AssertEqual(sourceFileCount, result.SourceFiles.Count);
        AssertEqual(0, result.Diagnostics.Count);
        AssertTrue(
            stopwatch.Elapsed < TimeSpan.FromSeconds(15),
            $"Compiler check performance smoke exceeded 15 seconds: {stopwatch.Elapsed.TotalMilliseconds:0}ms.");
    });
}

static void Net48ApplicationModelHostsReferenceGeneratedAssemblyAndRuntime()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "HostInteropSource"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.HostInterop"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.HostInterop

            export fun greeting(): string = "Hello from TypeSharp"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, error.ToString());
        AssertContains("Generated assembly: bin/Debug/net48/HostInteropSource.dll", output.ToString());

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "HostInteropSource.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "TypeSharp build should produce a generated assembly for host compatibility smokes.");

        var coreAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Core/TypeSharp.Core.csproj",
            "src/TypeSharp.Core/bin/Debug/net48/TypeSharp.Core.dll");
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "src/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "src/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");

        BuildApplicationModelHostProject(
            root,
            "AspNetWebFormsHostSmoke",
            ["System.Web"],
            generatedAssemblyPath,
            coreAssemblyPath,
            runtimeAssemblyPath,
            """
            using System.Web.UI;

            namespace AspNetWebFormsHostSmoke
            {
                public sealed class HomePage : Page
                {
                    public string RenderGreeting()
                    {
                        var unit = TypeSharp.Core.Unit.Value;
                        return Samples.HostInterop.Module.greeting()
                            + ":"
                            + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework
                            + ":"
                            + unit.ToString();
                    }
                }
            }
            """);

        BuildApplicationModelHostProject(
            root,
            "WcfContractHostSmoke",
            ["System.ServiceModel"],
            generatedAssemblyPath,
            coreAssemblyPath,
            runtimeAssemblyPath,
            """
            using System.ServiceModel;

            namespace WcfContractHostSmoke
            {
                [ServiceContract]
                public interface IGreetingService
                {
                    [OperationContract]
                    string GetGreeting();
                }

                public sealed class GreetingService : IGreetingService
                {
                    public string GetGreeting()
                    {
                        var option = TypeSharp.Core.Option<string>.Some(Samples.HostInterop.Module.greeting());
                        return option.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework;
                    }
                }
            }
            """);

        BuildApplicationModelHostProject(
            root,
            "WorkerServiceHostSmoke",
            ["System.ServiceProcess"],
            generatedAssemblyPath,
            coreAssemblyPath,
            runtimeAssemblyPath,
            """
            using System.ServiceProcess;

            namespace WorkerServiceHostSmoke
            {
                public sealed class GreetingWorker : ServiceBase
                {
                    public string RunOnce()
                    {
                        var result = TypeSharp.Core.Result<string, string>.Ok(Samples.HostInterop.Module.greeting());
                        return result.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion.ToString();
                    }
                }
            }
            """);
    });
}

static void CliBuildStopsBeforeEmissionOnDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildDiagnostics"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildDiagnostics

            export fun broken(): string
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS1001\"", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildDiagnostics.Generated.csproj")), "Build should not emit generated C# project when diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildDiagnostics.dll")), "Build should not emit generated assembly when diagnostics contain errors.");
    });
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertSequence<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
{
    AssertEqual(expected.Count, actual.Count);
    for (var index = 0; index < expected.Count; index++)
    {
        AssertEqual(expected[index], actual[index]);
    }
}

static void AssertTrue(bool value, string message)
{
    if (!value)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertFalse(bool value, string message) => AssertTrue(!value, message);

static void AssertContains(string expectedSubstring, string actual)
{
    if (!actual.Contains(expectedSubstring, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Expected output to contain '{expectedSubstring}', got '{actual}'.");
    }
}

static int CountOccurrences(string text, string value)
{
    var count = 0;
    var index = 0;
    while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
    {
        count++;
        index += value.Length;
    }

    return count;
}

static void AssertThrows<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Expected {typeof(TException).Name}, got {ex.GetType().Name}.");
    }

    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}

static void AssertTextFileEquals(string path, string actual)
{
    var expected = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
    AssertTextEquals(expected, actual);
}

static void AssertTextEquals(string expected, string actual)
{
    expected = expected.Replace("\r\n", "\n", StringComparison.Ordinal);
    actual = actual.Replace("\r\n", "\n", StringComparison.Ordinal);
    if (!string.Equals(expected, actual, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"Text mismatch.\nExpected:\n{expected}\nActual:\n{actual}");
    }
}

static void AssertNet48PackageFreeArtifact(string projectPath)
{
    var project = XDocument.Load(projectPath);
    var targetFrameworks = project.Descendants()
        .Where(element => element.Name.LocalName is "TargetFramework" or "TargetFrameworks")
        .Select(element => element.Value.Trim())
        .ToArray();

    AssertEqual(1, targetFrameworks.Length);
    AssertEqual("net48", targetFrameworks[0]);
    AssertFalse(
        project.Descendants().Any(element => element.Name.LocalName == "PackageReference"),
        $"{projectPath} should not use external NuGet package references.");

    var packagesConfig = Path.Combine(Path.GetDirectoryName(projectPath) ?? string.Empty, "packages.config");
    AssertFalse(File.Exists(packagesConfig), $"{projectPath} should not use packages.config.");
}

static void AssertNoDisallowedNet5RuntimeApiReferences(string sourceRoot)
{
    var disallowed = new[]
    {
        "System.Text.Json",
        "DateOnly",
        "TimeOnly",
        "System.Half",
        "System.Index",
        "System.Range",
        "Span<",
        "ReadOnlySpan<",
        "Memory<",
        "ReadOnlyMemory<",
        "ValueTask",
        "IAsyncEnumerable",
        "IAsyncEnumerator",
        "Parallel.ForEachAsync",
        "Task.WaitAsync"
    };

    foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
    {
        var text = File.ReadAllText(sourceFile);
        foreach (var api in disallowed)
        {
            AssertFalse(
                text.Contains(api, StringComparison.Ordinal),
                $"{sourceFile} should not reference .NET 5+ or package-backed runtime API '{api}'.");
        }
    }
}

static T Require<T>(T? value, string message)
    where T : class
{
    if (value is null)
    {
        throw new InvalidOperationException(message);
    }

    return value;
}

static void WithWorkspace(Action<string> action)
{
    var root = Path.Combine(Directory.GetCurrentDirectory(), "tests", "tmp", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);

    try
    {
        action(root);
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}

static void CopyDirectory(string sourceRoot, string targetRoot)
{
    foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var relativeDirectory = Path.GetRelativePath(sourceRoot, directory);
        Directory.CreateDirectory(Path.Combine(targetRoot, relativeDirectory));
    }

    Directory.CreateDirectory(targetRoot);
    foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
    {
        var relativeFile = Path.GetRelativePath(sourceRoot, file);
        var targetFile = Path.Combine(targetRoot, relativeFile);
        Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? targetRoot);
        File.Copy(file, targetFile, overwrite: true);
    }
}

static string WriteManifest(string root, string content)
{
    var manifestPath = Path.Combine(root, TypeSharpManifestLocator.ManifestFileName);
    File.WriteAllText(manifestPath, content.Replace("\r\n", "\n", StringComparison.Ordinal));
    return manifestPath;
}

static void WriteFile(string root, string relativePath, string content)
{
    var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? root);
    File.WriteAllText(path, content);
}

static void WriteLspFrame(Stream stream, string json)
{
    var payload = Encoding.UTF8.GetBytes(json);
    var header = Encoding.ASCII.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");
    stream.Write(header, 0, header.Length);
    stream.Write(payload, 0, payload.Length);
}

static void BuildLegacyReferenceDll(string root, string assemblyName)
{
    var projectRoot = Path.Combine(root, "legacy-src");
    Directory.CreateDirectory(projectRoot);
    WriteFile(projectRoot, $"{assemblyName}.csproj", $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net48</TargetFramework>
            <LangVersion>7.3</LangVersion>
            <ImplicitUsings>false</ImplicitUsings>
            <Nullable>disable</Nullable>
            <AssemblyName>{{assemblyName}}</AssemblyName>
          </PropertyGroup>
        </Project>
        """);
    WriteFile(projectRoot, "NuGet.config", """
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <packageSources>
            <clear />
          </packageSources>
        </configuration>
        """);
    WriteFile(projectRoot, "LegacyApi.cs", """
        namespace Legacy.Tools
        {
            public static class LegacyApi
            {
                public static string Echo(string value)
                {
                    return value;
                }
            }

            public static class LegacyParams
            {
                public static string Join(string separator, params string[] values)
                {
                    return string.Join(separator, values);
                }
            }

            public static class LegacyByRef
            {
                public static bool TryParseCount(string text, out int value)
                {
                    return int.TryParse(text, out value);
                }

                public static int AddOne(in int value)
                {
                    return value + 1;
                }

                public static void Increment(ref int value)
                {
                    value++;
                }
            }

            public static class LegacyOverloads
            {
                public static string Pick(string value)
                {
                    return value;
                }

                public static string Pick(object value)
                {
                    return value == null ? string.Empty : value.ToString();
                }
            }

            public static class LegacyParamsOverloads
            {
                public static string Pick(string separator, params string[] values)
                {
                    return string.Join(separator, values);
                }

                public static string Pick(string separator, params object[] values)
                {
                    return string.Join(separator, values);
                }
            }

            public static class LegacyOptional
            {
                public static string Format(string value, string suffix = "!")
                {
                    return value + suffix;
                }
            }

            public static class LegacyOptionalOverloads
            {
                public static string Pick(string value, string suffix = "!")
                {
                    return value + suffix;
                }

                public static string Pick(string value, int count = 1)
                {
                    return value + count.ToString();
                }
            }

            public static class LegacyNamedOverloads
            {
                public static string Route(string path, string controller = "Home")
                {
                    return controller + ":" + path;
                }

                public static string Route(string path, int statusCode = 200)
                {
                    return statusCode.ToString() + ":" + path;
                }
            }

            public static class LegacyDelegates
            {
                public static string Apply(string value, System.Func<string, string> transform)
                {
                    return transform(value);
                }
            }

            public sealed class LegacyEvents
            {
                public event System.Func<string, string> Transform;

                public string Raise(string value)
                {
                    var handler = Transform;
                    return handler == null ? value : handler(value);
                }
            }

            public sealed class LegacyMarkerAttribute : System.Attribute
            {
                public LegacyMarkerAttribute(string name)
                {
                    Name = name;
                }

                public string Name { get; }
            }

            public sealed class LegacyBox<T>
            {
                public LegacyBox(T value)
                {
                    Value = value;
                }

                public T Value { get; }
            }

            public sealed class LegacyFormatter
            {
                private readonly string prefix;

                public LegacyFormatter(string prefix)
                {
                    this.prefix = prefix;
                }

                public string Prefix
                {
                    get { return prefix; }
                }

                public string this[int index]
                {
                    get { return prefix + index.ToString(); }
                }

                public string Format(string value)
                {
                    return prefix + value;
                }
            }

            public sealed class LegacyFields
            {
                public const string StaticCode = "static";

                public readonly string InstanceCode = "instance";
            }

            public static class LegacyGenericMethods
            {
                public static T Identity<T>(T value)
                {
                    return value;
                }
            }

            public interface ILegacyNamed
            {
                string Name { get; }
            }

            public sealed class LegacyNamed : ILegacyNamed
            {
                public LegacyNamed(string name)
                {
                    Name = name;
                }

                public string Name { get; }
            }
        }
        """);

    var build = RunProcess("dotnet", $"build {assemblyName}.csproj --nologo --verbosity quiet --ignore-failed-sources", projectRoot);
    AssertTrue(
        build.ExitCode == 0,
        $"Legacy reference assembly should compile.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");

    var builtDll = Path.Combine(projectRoot, "bin", "Debug", "net48", $"{assemblyName}.dll");
    var targetDll = Path.Combine(root, "lib", $"{assemblyName}.dll");
    Directory.CreateDirectory(Path.GetDirectoryName(targetDll) ?? root);
    File.Copy(builtDll, targetDll, overwrite: true);
}

static string BuildRepositoryAssembly(string projectRelativePath, string assemblyRelativePath)
{
    var repositoryRoot = Directory.GetCurrentDirectory();
    var build = RunProcess(
        "dotnet",
        $"build {projectRelativePath} --nologo --verbosity quiet --ignore-failed-sources",
        repositoryRoot);

    AssertTrue(
        build.ExitCode == 0,
        $"Repository project '{projectRelativePath}' should compile for host compatibility smoke.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");

    var assemblyPath = Path.Combine(repositoryRoot, assemblyRelativePath.Replace('/', Path.DirectorySeparatorChar));
    AssertTrue(File.Exists(assemblyPath), $"Expected repository assembly '{assemblyRelativePath}' to exist after build.");
    return assemblyPath;
}

static void BuildApplicationModelHostProject(
    string root,
    string projectName,
    IReadOnlyList<string> frameworkReferences,
    string generatedAssemblyPath,
    string coreAssemblyPath,
    string runtimeAssemblyPath,
    string source)
{
    var projectRoot = Path.Combine(root, projectName);
    Directory.CreateDirectory(projectRoot);
    var referenceLines = string.Join(Environment.NewLine, frameworkReferences.Select(reference => $"    <Reference Include=\"{reference}\" />"));

    WriteFile(projectRoot, $"{projectName}.csproj", $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net48</TargetFramework>
            <LangVersion>7.3</LangVersion>
            <ImplicitUsings>false</ImplicitUsings>
            <Nullable>disable</Nullable>
            <AssemblyName>{{projectName}}</AssemblyName>
          </PropertyGroup>
          <ItemGroup>
        {{referenceLines}}
            <Reference Include="HostInteropSource">
              <HintPath>{{ToProjectHintPath(projectRoot, generatedAssemblyPath)}}</HintPath>
            </Reference>
            <Reference Include="TypeSharp.Core">
              <HintPath>{{ToProjectHintPath(projectRoot, coreAssemblyPath)}}</HintPath>
            </Reference>
            <Reference Include="TypeSharp.Runtime">
              <HintPath>{{ToProjectHintPath(projectRoot, runtimeAssemblyPath)}}</HintPath>
            </Reference>
          </ItemGroup>
        </Project>
        """);
    WriteFile(projectRoot, "NuGet.config", """
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <packageSources>
            <clear />
          </packageSources>
        </configuration>
        """);
    WriteFile(projectRoot, "HostSmoke.cs", source);

    var build = RunProcess("dotnet", $"build {projectName}.csproj --nologo --verbosity quiet --ignore-failed-sources", projectRoot);

    AssertTrue(
        build.ExitCode == 0,
        $"Application model host project '{projectName}' should compile against generated TypeSharp and runtime assemblies.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
}

static string ToProjectHintPath(string projectRoot, string assemblyPath) =>
    Path.GetRelativePath(projectRoot, assemblyPath).Replace('\\', '/');

static string MinimalManifest(string name) => $$"""
    [project]
    name = "{{name}}"
    targetFramework = "net48"
    outputType = "library"
    """;

static SyntaxNode? FindFirstNode(SyntaxNode node, SyntaxKind kind)
{
    if (node.Kind == kind)
    {
        return node;
    }

    foreach (var child in node.Children.Where(child => !child.IsToken))
    {
        var match = FindFirstNode(child, kind);
        if (match is not null)
        {
            return match;
        }
    }

    return null;
}

static int CountRuntimeImports(SyntaxNode root) =>
    root.Children.Count(child =>
        (child.Kind is SyntaxKind.ImportNamedDeclaration or SyntaxKind.ImportTypeDeclaration)
        && child.Children.Any(grandchild =>
            grandchild.IsToken
            && grandchild.Kind == SyntaxKind.StringLiteralToken
            && string.Equals(grandchild.Text, "\"TypeSharp.Runtime\"", StringComparison.Ordinal)));

static void AssertGeneratedExecutableLaunchBlocked(int exitCode, string output, string error, string executableName)
{
    AssertEqual(4, exitCode);
    AssertEqual(string.Empty, output);
    AssertContains("Could not run generated executable", error);
    AssertTrue(
        error.Contains("application control", StringComparison.OrdinalIgnoreCase)
            || error.Contains("애플리케이션 제어 정책", StringComparison.OrdinalIgnoreCase)
            || error.Contains("permission", StringComparison.OrdinalIgnoreCase)
            || error.Contains("access", StringComparison.OrdinalIgnoreCase)
            || error.Contains(executableName, StringComparison.OrdinalIgnoreCase),
        $"Launch failure should look like a local executable policy or permission block.\nSTDERR:\n{error}");
}

static ProcessResult RunProcess(string fileName, string arguments, string workingDirectory)
{
    using var process = new Process();
    process.StartInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    process.Start();
    var standardOutput = process.StandardOutput.ReadToEnd();
    var standardError = process.StandardError.ReadToEnd();
    if (!process.WaitForExit(milliseconds: 120_000))
    {
        process.Kill(entireProcessTree: true);
        throw new InvalidOperationException($"Process '{fileName} {arguments}' timed out.");
    }

    return new ProcessResult(process.ExitCode, standardOutput, standardError);
}

internal abstract class RuntimeUnionSmoke
{
    public sealed class MessageCase : RuntimeUnionSmoke, ITypeSharpUnionCase
    {
        public MessageCase(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public int Tag
        {
            get { return 1; }
        }

        public string CaseName
        {
            get { return "Message"; }
        }

        public bool HasPayload
        {
            get { return true; }
        }

        public object Payload
        {
            get { return Value; }
        }

        public override bool Equals(object? obj)
        {
            return obj is MessageCase other
                && TypeSharpUnion.SameCase(this, other)
                && TypeSharpUnion.PayloadEquals(this, other);
        }

        public override int GetHashCode()
        {
            return TypeSharpUnion.CombineHash(Tag, Value);
        }
    }

    public sealed class EmptyCase : RuntimeUnionSmoke, ITypeSharpUnionCase
    {
        public static readonly EmptyCase Instance = new EmptyCase();

        private EmptyCase()
        {
        }

        public int Tag
        {
            get { return 0; }
        }

        public string CaseName
        {
            get { return "Empty"; }
        }

        public bool HasPayload
        {
            get { return false; }
        }

        public object Payload
        {
            get { return null!; }
        }
    }
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
