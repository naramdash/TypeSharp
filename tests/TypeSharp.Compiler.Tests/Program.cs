using System.Diagnostics;
using System.Xml.Linq;
using TypeSharp.Compiler;
using TypeSharp.Cli;
using TypeSharp.Compiler.Backend;
using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Checking;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Interop;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.Projects;
using TypeSharp.Compiler.Testing;
using TypeSharp.Compiler.TypeChecking;
using TypeSharp.Core;

var tests = new (string Name, Action Body)[]
{
    ("version defaults match the documented CLI contract", VersionDefaultsMatchCliContract),
    ("diagnostic descriptor registry is stable", DiagnosticDescriptorRegistryIsStable),
    ("diagnostic text follows CLI text shape", DiagnosticTextUsesCliShape),
    ("parse result exposes error state", ParseResultExposesErrorState),
    ("parser fixture convention paths are stable", ParserFixtureConventionPathsAreStable),
    ("binder fixture convention paths are stable", BinderFixtureConventionPathsAreStable),
    ("type checker fixture convention paths are stable", TypeCheckerFixtureConventionPathsAreStable),
    ("C# backend fixture convention paths are stable", CSharpBackendFixtureConventionPathsAreStable),
    ("manifest loader reads explicit manifest path", ManifestLoaderReadsExplicitManifestPath),
    ("manifest locator searches parent directories", ManifestLocatorSearchesParentDirectories),
    ("source discovery defaults to src root", SourceDiscoveryDefaultsToSrcRoot),
    ("source discovery excludes build and generated folders", SourceDiscoveryExcludesBuildAndGeneratedFolders),
    ("runtime project targets net48", RuntimeProjectTargetsNet48),
    ("core project targets net48", CoreProjectTargetsNet48),
    ("net48 runtime artifacts avoid external package dependencies", Net48RuntimeArtifactsAvoidExternalPackageDependencies),
    ("core option and result expose basic states", CoreOptionAndResultExposeBasicStates),
    ("reference resolver normalizes framework assemblies", ReferenceResolverNormalizesFrameworkAssemblies),
    ("reference resolver normalizes local DLL paths", ReferenceResolverNormalizesLocalDllPaths),
    ("reference resolver reports missing local DLL diagnostics", ReferenceResolverReportsMissingLocalDllDiagnostics),
    ("metadata reader creates framework assembly placeholders", MetadataReaderCreatesFrameworkAssemblyPlaceholders),
    ("metadata reader creates local assembly placeholders", MetadataReaderCreatesLocalAssemblyPlaceholders),
    ("metadata reader indexes local public symbols", MetadataReaderIndexesLocalPublicSymbols),
    ("metadata reader preserves reference resolution diagnostics", MetadataReaderPreservesReferenceResolutionDiagnostics),
    ("metadata reader reports missing local metadata inputs", MetadataReaderReportsMissingLocalMetadataInputs),
    ("checker reports missing reference diagnostics", CheckerReportsMissingReferenceDiagnostics),
    ("checker reports invalid byref interop diagnostics", CheckerReportsInvalidByRefInteropDiagnostics),
    ("checker reports ambiguous C# overload diagnostics", CheckerReportsAmbiguousCSharpOverloadDiagnostics),
    ("checker reports ambiguous expanded params overload diagnostics", CheckerReportsAmbiguousExpandedParamsOverloadDiagnostics),
    ("checker reports ambiguous optional overload diagnostics", CheckerReportsAmbiguousOptionalOverloadDiagnostics),
    ("checker reports unknown C# nullability diagnostics", CheckerReportsUnknownCSharpNullabilityDiagnostics),
    ("CLI check emits JSON reference diagnostics", CliCheckEmitsJsonReferenceDiagnostics),
    ("CLI build stops before emission on reference diagnostics", CliBuildStopsBeforeEmissionOnReferenceDiagnostics),
    ("CLI build stops before emission on invalid byref interop", CliBuildStopsBeforeEmissionOnInvalidByRefInterop),
    ("CLI build stops before emission on ambiguous C# overload", CliBuildStopsBeforeEmissionOnAmbiguousCSharpOverload),
    ("CLI build stops before emission on type checker diagnostics", CliBuildStopsBeforeEmissionOnTypeCheckerDiagnostics),
    ("manifest loader reports invalid manifest shape", ManifestLoaderReportsInvalidManifestShape),
    ("CLI run builds and runs generated net48 executable", CliRunBuildsAndRunsGeneratedNet48Executable),
    ("CLI run passes arguments to generated main", CliRunPassesArgumentsToGeneratedMain),
    ("CLI run reports unsupported main signature", CliRunReportsUnsupportedMainSignature),
    ("CLI run rejects library projects", CliRunRejectsLibraryProjects),
    ("lexer handles tokens used by hello fixture", LexerHandlesHelloFixtureTokens),
    ("parser parses hello fixture without diagnostics", ParserParsesHelloFixtureWithoutDiagnostics),
    ("parser fixture snapshots match", ParserFixtureSnapshotsMatch),
    ("binder fixture diagnostics match", BinderFixtureDiagnosticsMatch),
    ("type checker fixture diagnostics match", TypeCheckerFixtureDiagnosticsMatch),
    ("C# backend fixture snapshots match", CSharpBackendFixtureSnapshotsMatch),
    ("generated C# compiles in net48 project", GeneratedCSharpCompilesInNet48Project),
    ("binder binds local declarations without diagnostics", BinderBindsLocalDeclarationsWithoutDiagnostics),
    ("checker reports unresolved name diagnostics", CheckerReportsUnresolvedNameDiagnostics),
    ("type checker accepts basic annotations", TypeCheckerAcceptsBasicAnnotations),
    ("checker reports type mismatch diagnostics", CheckerReportsTypeMismatchDiagnostics),
    ("checker reports parser diagnostics", CheckerReportsParserDiagnostics),
    ("CLI check succeeds on parse-clean project", CliCheckSucceedsOnParseCleanProject),
    ("CLI check emits JSON parser diagnostics", CliCheckEmitsJsonParserDiagnostics),
    ("CLI check emits JSON type checker diagnostics", CliCheckEmitsJsonTypeCheckerDiagnostics),
    ("CLI build emits generated C# source", CliBuildEmitsGeneratedCSharpSource),
    ("CLI build emits generated C# project scaffold", CliBuildEmitsGeneratedCSharpProjectScaffold),
    ("CLI build propagates manifest references to generated C# project", CliBuildPropagatesManifestReferencesToGeneratedCSharpProject),
    ("CLI build compiles framework static member call", CliBuildCompilesFrameworkStaticMemberCall),
    ("CLI build compiles local DLL static member call", CliBuildCompilesLocalDllStaticMemberCall),
    ("CLI build compiles imported constructor and instance member call", CliBuildCompilesImportedConstructorAndInstanceMemberCall),
    ("CLI build compiles imported property access", CliBuildCompilesImportedPropertyAccess),
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
    ("CLI build emits generated net48 assembly", CliBuildEmitsGeneratedNet48Assembly),
    ("C# net48 project consumes generated TypeSharp assembly", CSharpNet48ProjectConsumesGeneratedTypeSharpAssembly),
    ("net48 application model hosts reference generated assembly and runtime", Net48ApplicationModelHostsReferenceGeneratedAssemblyAndRuntime),
    ("CLI build stops before emission on diagnostics", CliBuildStopsBeforeEmissionOnDiagnostics)
};

var failures = 0;

foreach (var (name, body) in tests)
{
    try
    {
        body();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
    }
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
            "TS1000",
            "TS1001",
            "TS1002",
            "TS1003",
            "TS1004",
            "TS2001",
            "TS2201",
            "TS2401",
            "TS2402",
            "TS2403",
            "TS2404",
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
            ["Legacy.Tools.LegacyApi", "Legacy.Tools.LegacyParams", "Legacy.Tools.LegacyByRef", "Legacy.Tools.LegacyOverloads", "Legacy.Tools.LegacyParamsOverloads", "Legacy.Tools.LegacyOptional", "Legacy.Tools.LegacyOptionalOverloads", "Legacy.Tools.LegacyNamedOverloads", "Legacy.Tools.LegacyDelegates", "Legacy.Tools.LegacyEvents", "Legacy.Tools.LegacyFormatter"],
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
        AssertSequence(["Prefix"], legacyFormatter.Properties.Select(property => property.Name).ToArray());
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

        AssertEqual(0, exitCode);
        AssertEqual($"Hello from TypeSharp run{Environment.NewLine}", output.ToString());
        AssertEqual(string.Empty, error.ToString());
        AssertTrue(File.Exists(Path.Combine(root, "generated", "Program.g.cs")), "Run should emit a generated entry point.");
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RunSmoke.exe")), "Run should build a generated net48 executable.");
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

        AssertEqual(0, exitCode);
        AssertEqual($"2{Environment.NewLine}", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        var generatedProgram = File.ReadAllText(Path.Combine(root, "generated", "Program.g.cs"));
        AssertContains("public static string main(string[] args)", generatedSource);
        AssertContains("Module.main(args)", generatedProgram);
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
        AssertEqual(3, diagnostics.Length);
        AssertEqual("Cannot return expression of type 'int' from function returning 'string'.", diagnostics[0].Message);
        AssertEqual("Cannot assign expression of type 'string' to 'int'.", diagnostics[1].Message);
        AssertEqual("Cannot return expression of type 'null' from function returning 'string'.", diagnostics[2].Message);
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

                public string Format(string value)
                {
                    return prefix + value;
                }
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

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
