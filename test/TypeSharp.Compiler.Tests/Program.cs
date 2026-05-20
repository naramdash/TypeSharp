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
    ("fixture scenario README coverage is stable", FixtureScenarioReadmeCoverageIsStable),
    ("diagnostic fixture polarity is stable", DiagnosticFixturePolarityIsStable),
    ("backend abstraction exposes C# source backend", BackendAbstractionExposesCSharpSourceBackend),
    ("backend artifact contract supports direct assembly output", BackendArtifactContractSupportsDirectAssemblyOutput),
    ("lowering pipeline injects runtime helper imports", LoweringPipelineInjectsRuntimeHelperImports),
    ("manifest loader reads explicit manifest path", ManifestLoaderReadsExplicitManifestPath),
    ("manifest locator searches parent directories", ManifestLocatorSearchesParentDirectories),
    ("source discovery defaults to src root", SourceDiscoveryDefaultsToSrcRoot),
    ("source discovery excludes build and generated folders", SourceDiscoveryExcludesBuildAndGeneratedFolders),
    ("source discovery reports duplicate module paths", SourceDiscoveryReportsDuplicateModulePaths),
    ("source module graph collects relative import dependencies", SourceModuleGraphCollectsRelativeImportDependencies),
    ("source module graph collects relative import alias dependencies", SourceModuleGraphCollectsRelativeImportAliasDependencies),
    ("source module graph collects relative value import alias dependencies", SourceModuleGraphCollectsRelativeValueImportAliasDependencies),
    ("source module graph collects relative type import alias dependencies", SourceModuleGraphCollectsRelativeTypeImportAliasDependencies),
    ("source module graph collects relative named type import alias dependencies", SourceModuleGraphCollectsRelativeNamedTypeImportAliasDependencies),
    ("source module graph collects relative module import alias dependencies", SourceModuleGraphCollectsRelativeModuleImportAliasDependencies),
    ("source module graph collects local export alias surface", SourceModuleGraphCollectsLocalExportAliasSurface),
    ("source module graph collects local literal export alias surface", SourceModuleGraphCollectsLocalLiteralExportAliasSurface),
    ("source module graph collects local value export alias surface", SourceModuleGraphCollectsLocalValueExportAliasSurface),
    ("source module graph collects function value export surface", SourceModuleGraphCollectsFunctionValueExportSurface),
    ("source module graph collects local type export alias surface", SourceModuleGraphCollectsLocalTypeExportAliasSurface),
    ("source module graph collects relative re-export surface", SourceModuleGraphCollectsRelativeReExportSurface),
    ("source module graph collects relative re-export alias surface", SourceModuleGraphCollectsRelativeReExportAliasSurface),
    ("source module graph collects relative module re-export alias surface", SourceModuleGraphCollectsRelativeModuleReExportAliasSurface),
    ("source module graph collects relative value re-export surface", SourceModuleGraphCollectsRelativeValueReExportSurface),
    ("source module graph collects relative type re-export surface", SourceModuleGraphCollectsRelativeTypeReExportSurface),
    ("source module graph collects relative star re-export surface", SourceModuleGraphCollectsRelativeStarReExportSurface),
    ("source module graph reports missing named exports", SourceModuleGraphReportsMissingNamedExports),
    ("source module graph reports missing re-exported names", SourceModuleGraphReportsMissingReExportedNames),
    ("source module graph reports missing type re-exported names", SourceModuleGraphReportsMissingTypeReExportedNames),
    ("source module graph reports missing namespace import members", SourceModuleGraphReportsMissingNamespaceImportMembers),
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
    ("metadata reader indexes framework assembly public symbols", MetadataReaderIndexesFrameworkAssemblyPublicSymbols),
    ("metadata reader creates local assembly placeholders", MetadataReaderCreatesLocalAssemblyPlaceholders),
    ("metadata reader indexes local public symbols", MetadataReaderIndexesLocalPublicSymbols),
    ("metadata reader preserves reference resolution diagnostics", MetadataReaderPreservesReferenceResolutionDiagnostics),
    ("metadata reader reports missing local metadata inputs", MetadataReaderReportsMissingLocalMetadataInputs),
    ("checker reports missing reference diagnostics", CheckerReportsMissingReferenceDiagnostics),
    ("checker reports invalid byref interop diagnostics", CheckerReportsInvalidByRefInteropDiagnostics),
    ("checker reports explicit generic byref interop diagnostics", CheckerReportsExplicitGenericByRefInteropDiagnostics),
    ("checker reports unsatisfied C# generic constraint diagnostics", CheckerReportsUnsatisfiedCSharpGenericConstraintDiagnostics),
    ("checker reports unsatisfied framework C# generic constraint diagnostics", CheckerReportsUnsatisfiedFrameworkCSharpGenericConstraintDiagnostics),
    ("checker accepts transitive C# generic type constraints", CheckerAcceptsTransitiveCSharpGenericTypeConstraints),
    ("checker accepts inferred C# generic type constraints", CheckerAcceptsInferredCSharpGenericTypeConstraints),
    ("checker reports unsatisfied inferred C# generic constraint diagnostics", CheckerReportsUnsatisfiedInferredCSharpGenericConstraintDiagnostics),
    ("checker reports unsatisfied inferred constructed C# generic constraint diagnostics", CheckerReportsUnsatisfiedInferredConstructedCSharpGenericConstraintDiagnostics),
    ("checker accepts imported C# interface implementation relation", CheckerAcceptsImportedCSharpInterfaceImplementationRelation),
    ("checker reports no matching C# overload diagnostics", CheckerReportsNoMatchingCSharpOverloadDiagnostics),
    ("checker reports no matching C# overload for known argument type diagnostics", CheckerReportsNoMatchingCSharpOverloadForKnownArgumentTypeDiagnostics),
    ("checker reports no matching C# overload for numeric literal conversion diagnostics", CheckerReportsNoMatchingCSharpOverloadForNumericLiteralConversionDiagnostics),
    ("checker reports no matching C# overload for imported metadata argument diagnostics", CheckerReportsNoMatchingCSharpOverloadForImportedMetadataArgumentDiagnostics),
    ("checker reports no matching C# overload for null literal diagnostics", CheckerReportsNoMatchingCSharpOverloadForNullLiteralDiagnostics),
    ("checker reports no matching C# constructor diagnostics", CheckerReportsNoMatchingCSharpConstructorDiagnostics),
    ("checker reports no matching C# generic constructor diagnostics", CheckerReportsNoMatchingCSharpGenericConstructorDiagnostics),
    ("checker reports ambiguous C# constructor diagnostics", CheckerReportsAmbiguousCSharpConstructorDiagnostics),
    ("checker reports missing C# method diagnostics", CheckerReportsMissingCSharpMethodDiagnostics),
    ("checker reports missing C# type diagnostics", CheckerReportsMissingCSharpTypeDiagnostics),
    ("checker reports missing framework C# type diagnostics", CheckerReportsMissingFrameworkCSharpTypeDiagnostics),
    ("checker reports missing framework C# method diagnostics", CheckerReportsMissingFrameworkCSharpMethodDiagnostics),
    ("checker reports missing C# static member diagnostics", CheckerReportsMissingCSharpStaticMemberDiagnostics),
    ("checker reports missing framework C# static member diagnostics", CheckerReportsMissingFrameworkCSharpStaticMemberDiagnostics),
    ("checker reports missing C# instance member diagnostics", CheckerReportsMissingCSharpInstanceMemberDiagnostics),
    ("checker reports missing C# parameter instance member diagnostics", CheckerReportsMissingCSharpParameterInstanceMemberDiagnostics),
    ("checker reports missing C# alias instance member diagnostics", CheckerReportsMissingCSharpAliasInstanceMemberDiagnostics),
    ("checker reports missing C# assigned instance member diagnostics", CheckerReportsMissingCSharpAssignedInstanceMemberDiagnostics),
    ("checker accepts imported C# extension method instance syntax", CheckerAcceptsImportedCSharpExtensionMethodInstanceSyntax),
    ("checker accepts imported C# extension receiver relationship ranking", CheckerAcceptsImportedCSharpExtensionReceiverRelationshipRanking),
    ("checker accepts imported C# extension receiver object fallback", CheckerAcceptsImportedCSharpExtensionReceiverObjectFallback),
    ("checker reports no matching C# extension overload diagnostics", CheckerReportsNoMatchingCSharpExtensionOverloadDiagnostics),
    ("checker reports missing C# instance indexer diagnostics", CheckerReportsMissingCSharpInstanceIndexerDiagnostics),
    ("checker reports mismatched C# instance indexer diagnostics", CheckerReportsMismatchedCSharpInstanceIndexerDiagnostics),
    ("checker reports mismatched C# instance indexer numeric literal diagnostics", CheckerReportsMismatchedCSharpInstanceIndexerNumericLiteralDiagnostics),
    ("checker reports ambiguous C# instance indexer diagnostics", CheckerReportsAmbiguousCSharpInstanceIndexerDiagnostics),
    ("checker accepts imported C# indexer metadata relationship ranking", CheckerAcceptsImportedCSharpIndexerMetadataRelationshipRanking),
    ("checker reports missing C# instance property setter diagnostics", CheckerReportsMissingCSharpInstancePropertySetterDiagnostics),
    ("checker reports missing C# parameter instance property setter diagnostics", CheckerReportsMissingCSharpParameterInstancePropertySetterDiagnostics),
    ("checker reports missing C# annotated local instance property setter diagnostics", CheckerReportsMissingCSharpAnnotatedLocalInstancePropertySetterDiagnostics),
    ("checker reports readonly C# instance field assignment diagnostics", CheckerReportsReadOnlyCSharpInstanceFieldAssignmentDiagnostics),
    ("checker reports missing C# static property setter diagnostics", CheckerReportsMissingCSharpStaticPropertySetterDiagnostics),
    ("checker reports readonly C# static field assignment diagnostics", CheckerReportsReadOnlyCSharpStaticFieldAssignmentDiagnostics),
    ("checker reports missing C# instance event diagnostics", CheckerReportsMissingCSharpInstanceEventDiagnostics),
    ("checker reports ambiguous C# overload diagnostics", CheckerReportsAmbiguousCSharpOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda parameter return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaParameterReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda member return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaMemberReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda chained member return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaChainedMemberReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda method return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaMethodReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda extension method return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaExtensionMethodReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda static method return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaStaticMethodReturnOverloadDiagnostics),
    ("checker reports no matching C# delegate lambda binary return overload diagnostics", CheckerReportsNoMatchingCSharpDelegateLambdaBinaryReturnOverloadDiagnostics),
    ("C# overload resolver selects exact literal match", CSharpOverloadResolverSelectsExactLiteralMatch),
    ("C# overload resolver filters known argument type mismatch", CSharpOverloadResolverFiltersKnownArgumentTypeMismatch),
    ("C# overload resolver filters numeric literal conversion mismatch", CSharpOverloadResolverFiltersNumericLiteralConversionMismatch),
    ("C# overload resolver ranks null literal reference match", CSharpOverloadResolverRanksNullLiteralReferenceMatch),
    ("C# overload resolver ranks null literal nearest metadata reference", CSharpOverloadResolverRanksNullLiteralNearestMetadataReference),
    ("C# overload resolver ranks nearest metadata relationship", CSharpOverloadResolverRanksNearestMetadataRelationship),
    ("C# overload resolver filters explicit generic arity", CSharpOverloadResolverFiltersExplicitGenericArity),
    ("C# overload resolver filters lambda delegate arity", CSharpOverloadResolverFiltersLambdaDelegateArity),
    ("C# overload resolver filters lambda delegate return type", CSharpOverloadResolverFiltersLambdaDelegateReturnType),
    ("C# overload resolver filters lambda delegate parameter return type", CSharpOverloadResolverFiltersLambdaDelegateParameterReturnType),
    ("C# overload resolver filters lambda delegate member return type", CSharpOverloadResolverFiltersLambdaDelegateMemberReturnType),
    ("C# overload resolver filters lambda delegate chained member return type", CSharpOverloadResolverFiltersLambdaDelegateChainedMemberReturnType),
    ("C# overload resolver filters lambda delegate method return type", CSharpOverloadResolverFiltersLambdaDelegateMethodReturnType),
    ("C# overload resolver filters lambda delegate extension method return type", CSharpOverloadResolverFiltersLambdaDelegateExtensionMethodReturnType),
    ("C# overload resolver filters lambda delegate static method return type", CSharpOverloadResolverFiltersLambdaDelegateStaticMethodReturnType),
    ("C# overload resolver filters lambda delegate binary return type", CSharpOverloadResolverFiltersLambdaDelegateBinaryReturnType),
    ("C# overload resolver ranks lambda delegate return type", CSharpOverloadResolverRanksLambdaDelegateReturnType),
    ("checker reports ambiguous expanded params overload diagnostics", CheckerReportsAmbiguousExpandedParamsOverloadDiagnostics),
    ("checker reports ambiguous optional overload diagnostics", CheckerReportsAmbiguousOptionalOverloadDiagnostics),
    ("checker reports unknown C# nullability diagnostics", CheckerReportsUnknownCSharpNullabilityDiagnostics),
    ("CLI check emits JSON reference diagnostics", CliCheckEmitsJsonReferenceDiagnostics),
    ("CLI check emits JSON duplicate source module diagnostics", CliCheckEmitsJsonDuplicateSourceModuleDiagnostics),
    ("CLI check accepts relative source module import aliases", CliCheckAcceptsRelativeSourceModuleImportAliases),
    ("CLI check emits JSON unresolved source module diagnostics", CliCheckEmitsJsonUnresolvedSourceModuleDiagnostics),
    ("CLI check emits JSON missing source module export diagnostics", CliCheckEmitsJsonMissingSourceModuleExportDiagnostics),
    ("CLI check emits JSON missing source module re-export diagnostics", CliCheckEmitsJsonMissingSourceModuleReExportDiagnostics),
    ("CLI check emits JSON missing source module namespace member diagnostics", CliCheckEmitsJsonMissingSourceModuleNamespaceMemberDiagnostics),
    ("CLI check emits JSON unsupported package diagnostics", CliCheckEmitsJsonUnsupportedPackageDiagnostics),
    ("CLI build stops before emission on reference diagnostics", CliBuildStopsBeforeEmissionOnReferenceDiagnostics),
    ("CLI build stops before emission on duplicate source modules", CliBuildStopsBeforeEmissionOnDuplicateSourceModules),
    ("CLI build lowers relative source module import aliases", CliBuildLowersRelativeSourceModuleImportAliases),
    ("CLI build stops before emission on missing source module exports", CliBuildStopsBeforeEmissionOnMissingSourceModuleExports),
    ("CLI build stops before emission on missing source module re-exports", CliBuildStopsBeforeEmissionOnMissingSourceModuleReExports),
    ("CLI build stops before emission on missing source module namespace members", CliBuildStopsBeforeEmissionOnMissingSourceModuleNamespaceMembers),
    ("CLI build stops before emission on package diagnostics", CliBuildStopsBeforeEmissionOnPackageDiagnostics),
    ("CLI build stops before emission on invalid byref interop", CliBuildStopsBeforeEmissionOnInvalidByRefInterop),
    ("CLI build stops before emission on unsatisfied C# generic constraint", CliBuildStopsBeforeEmissionOnUnsatisfiedCSharpGenericConstraint),
    ("CLI build stops before emission on unsatisfied framework C# generic constraint", CliBuildStopsBeforeEmissionOnUnsatisfiedFrameworkCSharpGenericConstraint),
    ("CLI build stops before emission on unsatisfied inferred C# generic constraint", CliBuildStopsBeforeEmissionOnUnsatisfiedInferredCSharpGenericConstraint),
    ("CLI build stops before emission on unsatisfied inferred constructed C# generic constraint", CliBuildStopsBeforeEmissionOnUnsatisfiedInferredConstructedCSharpGenericConstraint),
    ("CLI build stops before emission on no matching C# overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpOverload),
    ("CLI build stops before emission on no matching C# delegate lambda overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaOverload),
    ("CLI build stops before emission on no matching C# delegate lambda return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda parameter return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaParameterReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda member return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaMemberReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda chained member return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaChainedMemberReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda method return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaMethodReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda extension method return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaExtensionMethodReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda static method return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaStaticMethodReturnOverload),
    ("CLI build stops before emission on no matching C# delegate lambda binary return overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaBinaryReturnOverload),
    ("CLI build stops before emission on known argument type C# overload mismatch", CliBuildStopsBeforeEmissionOnKnownArgumentTypeCSharpOverloadMismatch),
    ("CLI build stops before emission on numeric literal C# overload mismatch", CliBuildStopsBeforeEmissionOnNumericLiteralCSharpOverloadMismatch),
    ("CLI build stops before emission on imported metadata argument C# overload mismatch", CliBuildStopsBeforeEmissionOnImportedMetadataArgumentCSharpOverloadMismatch),
    ("CLI build stops before emission on null literal C# overload mismatch", CliBuildStopsBeforeEmissionOnNullLiteralCSharpOverloadMismatch),
    ("CLI build stops before emission on no matching C# constructor", CliBuildStopsBeforeEmissionOnNoMatchingCSharpConstructor),
    ("CLI build stops before emission on no matching C# generic constructor", CliBuildStopsBeforeEmissionOnNoMatchingCSharpGenericConstructor),
    ("CLI build stops before emission on ambiguous C# constructor", CliBuildStopsBeforeEmissionOnAmbiguousCSharpConstructor),
    ("CLI build stops before emission on missing C# method", CliBuildStopsBeforeEmissionOnMissingCSharpMethod),
    ("CLI build stops before emission on missing C# type", CliBuildStopsBeforeEmissionOnMissingCSharpType),
    ("CLI build stops before emission on missing framework C# type", CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpType),
    ("CLI build stops before emission on missing framework C# method", CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpMethod),
    ("CLI build stops before emission on missing C# static member", CliBuildStopsBeforeEmissionOnMissingCSharpStaticMember),
    ("CLI build stops before emission on missing framework C# static member", CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpStaticMember),
    ("CLI build stops before emission on missing C# instance member", CliBuildStopsBeforeEmissionOnMissingCSharpInstanceMember),
    ("CLI build stops before emission on missing C# parameter instance member", CliBuildStopsBeforeEmissionOnMissingCSharpParameterInstanceMember),
    ("CLI build stops before emission on missing C# alias instance member", CliBuildStopsBeforeEmissionOnMissingCSharpAliasInstanceMember),
    ("CLI build stops before emission on missing C# assigned instance member", CliBuildStopsBeforeEmissionOnMissingCSharpAssignedInstanceMember),
    ("CLI build stops before emission on no matching C# extension overload", CliBuildStopsBeforeEmissionOnNoMatchingCSharpExtensionOverload),
    ("CLI build stops before emission on missing C# instance indexer", CliBuildStopsBeforeEmissionOnMissingCSharpInstanceIndexer),
    ("CLI build stops before emission on mismatched C# instance indexer", CliBuildStopsBeforeEmissionOnMismatchedCSharpInstanceIndexer),
    ("CLI build stops before emission on mismatched C# instance indexer numeric literal", CliBuildStopsBeforeEmissionOnMismatchedCSharpInstanceIndexerNumericLiteral),
    ("CLI build stops before emission on ambiguous C# instance indexer", CliBuildStopsBeforeEmissionOnAmbiguousCSharpInstanceIndexer),
    ("CLI build stops before emission on missing C# instance property setter", CliBuildStopsBeforeEmissionOnMissingCSharpInstancePropertySetter),
    ("CLI build stops before emission on readonly C# instance field assignment", CliBuildStopsBeforeEmissionOnReadOnlyCSharpInstanceFieldAssignment),
    ("CLI build stops before emission on missing C# static property setter", CliBuildStopsBeforeEmissionOnMissingCSharpStaticPropertySetter),
    ("CLI build stops before emission on readonly C# static field assignment", CliBuildStopsBeforeEmissionOnReadOnlyCSharpStaticFieldAssignment),
    ("CLI build stops before emission on missing C# instance event", CliBuildStopsBeforeEmissionOnMissingCSharpInstanceEvent),
    ("CLI build stops before emission on ambiguous C# overload", CliBuildStopsBeforeEmissionOnAmbiguousCSharpOverload),
    ("CLI build stops before emission on type checker diagnostics", CliBuildStopsBeforeEmissionOnTypeCheckerDiagnostics),
    ("CLI build stops before emission on unsatisfied imported C# interface implementation", CliBuildStopsBeforeEmissionOnUnsatisfiedImportedCSharpInterfaceImplementation),
    ("CLI build stops before emission on nullability diagnostics", CliBuildStopsBeforeEmissionOnNullabilityDiagnostics),
    ("CLI build stops before emission on public boundary diagnostics", CliBuildStopsBeforeEmissionOnPublicBoundaryDiagnostics),
    ("CLI build stops before emission on non-exhaustive match", CliBuildStopsBeforeEmissionOnNonExhaustiveMatch),
    ("CLI build stops before emission on unsupported export forwarding", CliBuildStopsBeforeEmissionOnUnsupportedExportForwarding),
    ("CLI build stops before emission on duplicate local export", CliBuildStopsBeforeEmissionOnDuplicateLocalExport),
    ("CLI build uses local export lists for public surface", CliBuildUsesLocalExportListsForPublicSurface),
    ("CLI build lowers local function export aliases", CliBuildLowersLocalFunctionExportAliases),
    ("CLI build lowers local literal export aliases", CliBuildLowersLocalLiteralExportAliases),
    ("CLI build lowers local value export aliases", CliBuildLowersLocalValueExportAliases),
    ("CLI build lowers function value exports", CliBuildLowersFunctionValueExports),
    ("CLI build lowers local type export aliases", CliBuildLowersLocalTypeExportAliases),
    ("manifest loader reports invalid manifest shape", ManifestLoaderReportsInvalidManifestShape),
    ("manifest loader rejects invalid option domains", ManifestLoaderRejectsInvalidOptionDomains),
    ("CLI check emits JSON invalid manifest value diagnostics", CliCheckEmitsJsonInvalidManifestValueDiagnostics),
    ("CLI run builds and runs generated net48 executable", CliRunBuildsAndRunsGeneratedNet48Executable),
    ("CLI run passes arguments to generated main", CliRunPassesArgumentsToGeneratedMain),
    ("CLI run uses module path container for multi-source executable", CliRunUsesModulePathContainerForMultiSourceExecutable),
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
    ("checker reports duplicate local export diagnostics", CheckerReportsDuplicateLocalExportDiagnostics),
    ("checker reports unresolved local export list diagnostics", CheckerReportsUnresolvedLocalExportListDiagnostics),
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
    ("CLI check emits JSON duplicate local export diagnostics", CliCheckEmitsJsonDuplicateLocalExportDiagnostics),
    ("CLI check emits JSON unsupported generic constraint diagnostics", CliCheckEmitsJsonUnsupportedGenericConstraintDiagnostics),
    ("CLI check emits JSON dynamic capability diagnostics", CliCheckEmitsJsonDynamicCapabilityDiagnostics),
    ("CLI check emits JSON dynamic call capability diagnostics", CliCheckEmitsJsonDynamicCallCapabilityDiagnostics),
    ("CLI check emits JSON capability call marker diagnostics", CliCheckEmitsJsonCapabilityCallMarkerDiagnostics),
    ("CLI check emits JSON unknown access diagnostics", CliCheckEmitsJsonUnknownAccessDiagnostics),
    ("CLI check keeps warnings nonblocking by default", CliCheckKeepsWarningsNonblockingByDefault),
    ("CLI check treats warnings as errors", CliCheckTreatsWarningsAsErrors),
    ("CLI build stops before emission on warnings as errors", CliBuildStopsBeforeEmissionOnWarningsAsErrors),
    ("CLI lsp runs language server protocol", CliLspRunsLanguageServerProtocol),
    ("CLI lsp rejects unknown options", CliLspRejectsUnknownOptions),
    ("LSP diagnostic mapper uses zero-based ranges", LspDiagnosticMapperUsesZeroBasedRanges),
    ("language server publishes diagnostics on didOpen", LanguageServerPublishesDiagnosticsOnDidOpen),
    ("language server clears diagnostics on didClose", LanguageServerClearsDiagnosticsOnDidClose),
    ("language server returns hover for bound symbols", LanguageServerReturnsHoverForBoundSymbols),
    ("language server returns definition for bound symbols", LanguageServerReturnsDefinitionForBoundSymbols),
    ("language server returns completion items", LanguageServerReturnsCompletionItems),
    ("VS Code extension activates LSP client", VsCodeExtensionActivatesLspClient),
    ("VS Code extension activation smoke runs in mocked extension host", VsCodeExtensionActivationSmokeRunsInMockedExtensionHost),
    ("VS Code extension live smoke runs against bundled language server", VsCodeExtensionLiveSmokeRunsAgainstBundledLanguageServer),
    ("VS Code extension package shape is stable", VsCodeExtensionPackageShapeIsStable),
    ("VS Code syntax grammar covers stable TypeSharp tokens", VsCodeSyntaxGrammarCoversStableTypeSharpTokens),
    ("runnable example catalog smoke matrix is stable", RunnableExampleCatalogSmokeMatrixIsStable),
    ("runnable example project commands are smoke-tested", RunnableExampleProjectCommandsAreSmokeTested),
    ("docs site contract is stable", DocsSiteContractIsStable),
    ("GitHub Pages workflow contract is stable", GitHubPagesWorkflowContractIsStable),
    ("release artifacts workflow contract is stable", ReleaseArtifactsWorkflowContractIsStable),
    ("repository monorepo layout is stable", RepositoryMonorepoLayoutIsStable),
    ("CLI build emits generated C# source", CliBuildEmitsGeneratedCSharpSource),
    ("CLI build uses root namespace for namespace-less source", CliBuildUsesRootNamespaceForNamespaceLessSource),
    ("CLI build uses module path containers for multiple sources", CliBuildUsesModulePathContainersForMultipleSources),
    ("CLI build lowers relative source named imports", CliBuildLowersRelativeSourceNamedImports),
    ("CLI build lowers relative source named import aliases", CliBuildLowersRelativeSourceNamedImportAliases),
    ("CLI build lowers relative source value import aliases", CliBuildLowersRelativeSourceValueImportAliases),
    ("CLI build lowers relative source type import aliases", CliBuildLowersRelativeSourceTypeImportAliases),
    ("CLI build lowers relative source named type import aliases", CliBuildLowersRelativeSourceNamedTypeImportAliases),
    ("CLI build lowers relative source namespace imports", CliBuildLowersRelativeSourceNamespaceImports),
    ("CLI build lowers relative source re-exports", CliBuildLowersRelativeSourceReExports),
    ("CLI build lowers relative source re-export aliases", CliBuildLowersRelativeSourceReExportAliases),
    ("CLI build lowers relative source module re-export aliases", CliBuildLowersRelativeSourceModuleReExportAliases),
    ("CLI build lowers relative source value re-exports", CliBuildLowersRelativeSourceValueReExports),
    ("CLI build lowers relative source type re-exports", CliBuildLowersRelativeSourceTypeReExports),
    ("CLI build lowers relative source star re-exports", CliBuildLowersRelativeSourceStarReExports),
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
    ("CLI build compiles imported constructor named optional params calls", CliBuildCompilesImportedConstructorNamedOptionalParamsCalls),
    ("CLI build compiles imported parameter instance member call", CliBuildCompilesImportedParameterInstanceMemberCall),
    ("CLI build compiles imported alias instance member call", CliBuildCompilesImportedAliasInstanceMemberCall),
    ("CLI build compiles imported assigned instance member call", CliBuildCompilesImportedAssignedInstanceMemberCall),
    ("CLI build compiles imported property access", CliBuildCompilesImportedPropertyAccess),
    ("CLI build compiles imported property assignment", CliBuildCompilesImportedPropertyAssignment),
    ("CLI build compiles imported field access", CliBuildCompilesImportedFieldAccess),
    ("CLI build compiles imported field assignment", CliBuildCompilesImportedFieldAssignment),
    ("CLI build compiles imported static property assignment", CliBuildCompilesImportedStaticPropertyAssignment),
    ("CLI build compiles imported static field assignment", CliBuildCompilesImportedStaticFieldAssignment),
    ("CLI build compiles imported indexer access", CliBuildCompilesImportedIndexerAccess),
    ("CLI build compiles imported indexer numeric literal conversion", CliBuildCompilesImportedIndexerNumericLiteralConversion),
    ("CLI build compiles imported overloaded indexer exact match", CliBuildCompilesImportedOverloadedIndexerExactMatch),
    ("CLI build compiles imported indexer metadata relationship match", CliBuildCompilesImportedIndexerMetadataRelationshipMatch),
    ("CLI build compiles imported params call", CliBuildCompilesImportedParamsCall),
    ("CLI build compiles imported out call", CliBuildCompilesImportedOutCall),
    ("CLI build compiles imported in call", CliBuildCompilesImportedInCall),
    ("CLI build compiles imported ref call", CliBuildCompilesImportedRefCall),
    ("CLI build compiles exact overload match", CliBuildCompilesExactOverloadMatch),
    ("CLI build compiles object overload fallback for known argument type", CliBuildCompilesObjectOverloadFallbackForKnownArgumentType),
    ("CLI build compiles null literal reference overload match", CliBuildCompilesNullLiteralReferenceOverloadMatch),
    ("CLI build compiles null literal metadata relationship overload match", CliBuildCompilesNullLiteralMetadataRelationshipOverloadMatch),
    ("CLI build compiles imported metadata overload match", CliBuildCompilesImportedMetadataOverloadMatch),
    ("CLI build compiles imported metadata relationship overload match", CliBuildCompilesImportedMetadataRelationshipOverloadMatch),
    ("CLI build compiles numeric literal constant conversion", CliBuildCompilesNumericLiteralConstantConversion),
    ("CLI build compiles exact expanded params overload match", CliBuildCompilesExactExpandedParamsOverloadMatch),
    ("CLI build compiles imported optional call", CliBuildCompilesImportedOptionalCall),
    ("CLI build compiles imported named argument call", CliBuildCompilesImportedNamedArgumentCall),
    ("CLI build compiles imported delegate lambda call", CliBuildCompilesImportedDelegateLambdaCall),
    ("CLI build compiles imported delegate lambda overload arity match", CliBuildCompilesImportedDelegateLambdaOverloadArityMatch),
    ("CLI build compiles imported delegate lambda overload return match", CliBuildCompilesImportedDelegateLambdaOverloadReturnMatch),
    ("CLI build compiles imported delegate lambda overload parameter return match", CliBuildCompilesImportedDelegateLambdaOverloadParameterReturnMatch),
    ("CLI build compiles imported delegate lambda overload return ranking", CliBuildCompilesImportedDelegateLambdaOverloadReturnRanking),
    ("CLI build compiles imported delegate lambda overload member return match", CliBuildCompilesImportedDelegateLambdaOverloadMemberReturnMatch),
    ("CLI build compiles imported delegate lambda overload chained member return match", CliBuildCompilesImportedDelegateLambdaOverloadChainedMemberReturnMatch),
    ("CLI build compiles imported delegate lambda overload method return match", CliBuildCompilesImportedDelegateLambdaOverloadMethodReturnMatch),
    ("CLI build compiles imported delegate lambda overload extension method return match", CliBuildCompilesImportedDelegateLambdaOverloadExtensionMethodReturnMatch),
    ("CLI build compiles imported delegate lambda overload static method return match", CliBuildCompilesImportedDelegateLambdaOverloadStaticMethodReturnMatch),
    ("CLI build compiles imported delegate lambda overload binary return match", CliBuildCompilesImportedDelegateLambdaOverloadBinaryReturnMatch),
    ("CLI build compiles imported event add and remove call", CliBuildCompilesImportedEventAddRemoveCall),
    ("CLI build compiles imported extension method instance call", CliBuildCompilesImportedExtensionMethodInstanceCall),
    ("CLI build compiles imported extension receiver relationship match", CliBuildCompilesImportedExtensionReceiverRelationshipMatch),
    ("CLI build compiles imported extension receiver object fallback", CliBuildCompilesImportedExtensionReceiverObjectFallback),
    ("CLI build compiles imported generic method call", CliBuildCompilesImportedGenericMethodCall),
    ("CLI build compiles imported explicit generic method call", CliBuildCompilesImportedExplicitGenericMethodCall),
    ("CLI build compiles imported generic constraint call", CliBuildCompilesImportedGenericConstraintCall),
    ("CLI build compiles imported framework generic constraint call", CliBuildCompilesImportedFrameworkGenericConstraintCall),
    ("CLI build compiles imported transitive generic constraint call", CliBuildCompilesImportedTransitiveGenericConstraintCall),
    ("CLI build compiles imported inferred generic constraint call", CliBuildCompilesImportedInferredGenericConstraintCall),
    ("CLI build compiles imported inferred constructed generic constraint call", CliBuildCompilesImportedInferredConstructedGenericConstraintCall),
    ("CLI build compiles imported interface reference", CliBuildCompilesImportedInterfaceReference),
    ("CLI build compiles imported interface implementation return", CliBuildCompilesImportedInterfaceImplementationReturn),
    ("CLI build compiles imported attribute and generic type references", CliBuildCompilesImportedAttributeAndGenericTypeReferences),
    ("CLI build compiles imported generic type constructor call", CliBuildCompilesImportedGenericTypeConstructorCall),
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
    ("CLI build compiles keyof type operator", CliBuildCompilesKeyofTypeOperator),
    ("CLI build compiles indexed access type operator", CliBuildCompilesIndexedAccessTypeOperator),
    ("CLI build compiles nominal union API", CliBuildCompilesNominalUnionApi),
    ("CLI build compiles nominal union match lowering", CliBuildCompilesNominalUnionMatchLowering),
    ("CLI build compiles type-level union narrowing", CliBuildCompilesTypeLevelUnionNarrowing),
    ("CLI build compiles async Task interop", CliBuildCompilesAsyncTaskInterop),
    ("CLI build compiles collection expression lowering", CliBuildCompilesCollectionExpressionLowering),
    ("CLI build compiles pipeline lowering", CliBuildCompilesPipelineLowering),
    ("CLI build compiles composition lowering", CliBuildCompilesCompositionLowering),
    ("CLI build compiles satisfies expression", CliBuildCompilesSatisfiesExpression),
    ("CLI build compiles yield iterator lowering", CliBuildCompilesYieldIteratorLowering),
    ("CLI build compiles lock statement lowering", CliBuildCompilesLockStatementLowering),
    ("CLI build compiles extension method lowering", CliBuildCompilesExtensionMethodLowering),
    ("CLI build compiles nameof intrinsic", CliBuildCompilesNameofIntrinsic),
    ("CLI build compiles checked unchecked expressions", CliBuildCompilesCheckedUncheckedExpressions),
    ("CLI build compiles literal constants", CliBuildCompilesLiteralConstants),
    ("CLI build emits generated net48 assembly", CliBuildEmitsGeneratedNet48Assembly),
    ("generated net48 assembly public ABI snapshot is stable", GeneratedNet48AssemblyPublicAbiSnapshotIsStable),
    ("C# net48 project consumes generated TypeSharp assembly", CSharpNet48ProjectConsumesGeneratedTypeSharpAssembly),
    ("net48 application model hosts reference generated assembly and runtime", Net48ApplicationModelHostsReferenceGeneratedAssemblyAndRuntime),
    ("compiler check keeps parallel diagnostics in source order", CompilerCheckKeepsParallelDiagnosticsInSourceOrder),
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
            "TS0112",
            "TS0113",
            "TS0114",
            "TS1000",
            "TS1001",
            "TS1002",
            "TS1003",
            "TS1004",
            "TS2001",
            "TS2002",
            "TS2003",
            "TS2004",
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
            "TS2406",
            "TS2407",
            "TS2408",
            "TS2409",
            "TS2410",
            "TS2411",
            "TS2412",
            "TS2413",
            "TS2414",
            "TS2415",
            "TS2416",
            "TS2417",
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
    AssertContains("Message: Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.", output.ToString());
    AssertContains("Explanation: Type-level unions, intersections, and structural shapes are compile-time TypeSharp types", output.ToString());
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
    AssertEqual("test/fixtures/parser", ParserFixtureConventions.Root);
    AssertEqual("test/fixtures/parser/positive", ParserFixtureConventions.PositiveRoot);
    AssertEqual("test/fixtures/parser/negative", ParserFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", ParserFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", ParserFixtureConventions.ExpectedDiagnosticsFileName);
    AssertEqual("expected.tree", ParserFixtureConventions.ExpectedTreeFileName);
}

static void BinderFixtureConventionPathsAreStable()
{
    AssertEqual("test/fixtures/diagnostics/binder", BinderFixtureConventions.Root);
    AssertEqual("test/fixtures/diagnostics/binder/positive", BinderFixtureConventions.PositiveRoot);
    AssertEqual("test/fixtures/diagnostics/binder/negative", BinderFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", BinderFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", BinderFixtureConventions.ExpectedDiagnosticsFileName);
}

static void TypeCheckerFixtureConventionPathsAreStable()
{
    AssertEqual("test/fixtures/diagnostics/type-checker", TypeCheckerFixtureConventions.Root);
    AssertEqual("test/fixtures/diagnostics/type-checker/positive", TypeCheckerFixtureConventions.PositiveRoot);
    AssertEqual("test/fixtures/diagnostics/type-checker/negative", TypeCheckerFixtureConventions.NegativeRoot);
    AssertEqual("input.tysh", TypeCheckerFixtureConventions.InputFileName);
    AssertEqual("expected.diagnostics.json", TypeCheckerFixtureConventions.ExpectedDiagnosticsFileName);
}

static void CSharpBackendFixtureConventionPathsAreStable()
{
    AssertEqual("test/fixtures/backend/csharp", CSharpBackendFixtureConventions.Root);
    AssertEqual("test/fixtures/backend/csharp/positive", CSharpBackendFixtureConventions.PositiveRoot);
    AssertEqual("input.tysh", CSharpBackendFixtureConventions.InputFileName);
    AssertEqual("expected.cs", CSharpBackendFixtureConventions.ExpectedCSharpFileName);
}

static void FixtureScenarioReadmeCoverageIsStable()
{
    var fixtureSets = new[]
    {
        (Root: ParserFixtureConventions.PositiveRoot, InputFileName: ParserFixtureConventions.InputFileName),
        (Root: ParserFixtureConventions.NegativeRoot, InputFileName: ParserFixtureConventions.InputFileName),
        (Root: BinderFixtureConventions.PositiveRoot, InputFileName: BinderFixtureConventions.InputFileName),
        (Root: BinderFixtureConventions.NegativeRoot, InputFileName: BinderFixtureConventions.InputFileName),
        (Root: TypeCheckerFixtureConventions.PositiveRoot, InputFileName: TypeCheckerFixtureConventions.InputFileName),
        (Root: TypeCheckerFixtureConventions.NegativeRoot, InputFileName: TypeCheckerFixtureConventions.InputFileName),
        (Root: CSharpBackendFixtureConventions.PositiveRoot, InputFileName: CSharpBackendFixtureConventions.InputFileName)
    };

    foreach (var fixtureSet in fixtureSets)
    {
        foreach (var fixtureDirectory in Directory.EnumerateDirectories(fixtureSet.Root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(Path.Combine(fixtureDirectory, fixtureSet.InputFileName)))
            {
                continue;
            }

            var readmePath = Path.Combine(fixtureDirectory, "README.md");
            var relativeFixture = Path.GetRelativePath(Directory.GetCurrentDirectory(), fixtureDirectory).Replace('\\', '/');
            AssertTrue(File.Exists(readmePath), $"Fixture '{relativeFixture}' should document the scenario in README.md.");
            AssertFalse(string.IsNullOrWhiteSpace(File.ReadAllText(readmePath)), $"Fixture '{relativeFixture}' README.md should not be empty.");
        }
    }
}

static void DiagnosticFixturePolarityIsStable()
{
    var fixtureSets = new[]
    {
        (Root: ParserFixtureConventions.PositiveRoot, ExpectedDiagnosticsFileName: ParserFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: false),
        (Root: ParserFixtureConventions.NegativeRoot, ExpectedDiagnosticsFileName: ParserFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: true),
        (Root: BinderFixtureConventions.PositiveRoot, ExpectedDiagnosticsFileName: BinderFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: false),
        (Root: BinderFixtureConventions.NegativeRoot, ExpectedDiagnosticsFileName: BinderFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: true),
        (Root: TypeCheckerFixtureConventions.PositiveRoot, ExpectedDiagnosticsFileName: TypeCheckerFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: false),
        (Root: TypeCheckerFixtureConventions.NegativeRoot, ExpectedDiagnosticsFileName: TypeCheckerFixtureConventions.ExpectedDiagnosticsFileName, ShouldHaveDiagnostics: true)
    };

    foreach (var fixtureSet in fixtureSets)
    {
        foreach (var fixtureDirectory in Directory.EnumerateDirectories(fixtureSet.Root).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var diagnosticsPath = Path.Combine(fixtureDirectory, fixtureSet.ExpectedDiagnosticsFileName);
            var relativeFixture = Path.GetRelativePath(Directory.GetCurrentDirectory(), fixtureDirectory).Replace('\\', '/');
            using var document = JsonDocument.Parse(File.ReadAllText(diagnosticsPath));
            var diagnostics = document.RootElement.GetProperty("diagnostics");

            if (fixtureSet.ShouldHaveDiagnostics)
            {
                AssertTrue(diagnostics.GetArrayLength() > 0, $"Negative fixture '{relativeFixture}' should expect at least one diagnostic.");
            }
            else
            {
                AssertEqual(0, diagnostics.GetArrayLength());
            }

            foreach (var diagnostic in diagnostics.EnumerateArray())
            {
                AssertFalse(string.IsNullOrWhiteSpace(diagnostic.GetProperty("code").GetString()), $"Fixture '{relativeFixture}' diagnostic should include a code.");
                AssertFalse(string.IsNullOrWhiteSpace(diagnostic.GetProperty("message").GetString()), $"Fixture '{relativeFixture}' diagnostic should include a message.");
                AssertFalse(string.IsNullOrWhiteSpace(diagnostic.GetProperty("file").GetString()), $"Fixture '{relativeFixture}' diagnostic should include a file.");
                AssertTrue(diagnostic.GetProperty("start").TryGetProperty("line", out _), $"Fixture '{relativeFixture}' diagnostic should include a start line.");
                AssertTrue(diagnostic.GetProperty("start").TryGetProperty("column", out _), $"Fixture '{relativeFixture}' diagnostic should include a start column.");
                AssertTrue(diagnostic.GetProperty("end").TryGetProperty("line", out _), $"Fixture '{relativeFixture}' diagnostic should include an end line.");
                AssertTrue(diagnostic.GetProperty("end").TryGetProperty("column", out _), $"Fixture '{relativeFixture}' diagnostic should include an end column.");
            }
        }
    }
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

static void SourceModuleGraphCollectsRelativeImportDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraph"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraph

            import { keep } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraph

            export fun keep(): string = "ok"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Supported relative source imports should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeImportAliasDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphImportAlias"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraphImportAlias

            import { keep as keepHelper } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraphImportAlias

            export fun keep(): string = "ok"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source function import aliases should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeValueImportAliasDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphValueImportAlias"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraphValueImportAlias

            import { PublicName as ImportedName } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraphValueImportAlias

            let InternalName: string = "Ada"
            export { InternalName as PublicName }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source value import aliases should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeTypeImportAliasDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphTypeImportAlias"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraphTypeImportAlias

            import type { VisibleModel as Model } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraphTypeImportAlias

            export record VisibleModel(Name: string)
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source type import aliases should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeNamedTypeImportAliasDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphNamedTypeImportAlias"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraphNamedTypeImportAlias

            import { VisibleModel as Model } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraphNamedTypeImportAlias

            export record VisibleModel(Name: string)
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative named source type import aliases should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeModuleImportAliasDependencies()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphModuleImportAlias"));
        WriteFile(root, "src/Feature/Main.tysh", """
            namespace Samples.SourceGraphModuleImportAlias

            import { Tools as HelperTools } from "./Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.SourceGraphModuleImportAlias

            export module Tools {
              export fun keep(): string = "ok"
            }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source module import aliases should be recorded without blocking diagnostics.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Feature/Main", dependency.FromModulePath);
        AssertEqual("Feature/Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsLocalExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphLocalExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphLocalExportAlias

            import { publicKeep } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphLocalExportAlias

            fun keep(): string = "ok"
            export { keep as publicKeep }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Local function export aliases should contribute their alias to the source module export surface.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Main", dependency.FromModulePath);
        AssertEqual("Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsLocalLiteralExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphLocalLiteralExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphLocalLiteralExportAlias

            import { PublicVersion } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphLocalLiteralExportAlias

            literal InternalVersion: string = "1.0"
            export { InternalVersion as PublicVersion }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Local literal export aliases should contribute their alias to the source module export surface.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Main", dependency.FromModulePath);
        AssertEqual("Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsLocalValueExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphLocalValueExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphLocalValueExportAlias

            import { PublicName } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphLocalValueExportAlias

            let InternalName: string = "Ada"
            export { InternalName as PublicName }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Local top-level value export aliases should contribute their alias to the source module export surface.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Main", dependency.FromModulePath);
        AssertEqual("Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsFunctionValueExportSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphFunctionValueExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphFunctionValueExport

            import { Transform, PublicTransform } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphFunctionValueExport

            export let Transform: string -> string = text => text

            let internalTransform: string -> string = text => text
            export { internalTransform as PublicTransform }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Annotated function-valued top-level let exports should contribute to the value export surface.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Main", dependency.FromModulePath);
        AssertEqual("Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsLocalTypeExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphLocalTypeExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphLocalTypeExportAlias

            import type { Model } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphLocalTypeExportAlias

            record VisibleModel(Name: string)
            export type { VisibleModel as Model }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Local type export aliases should contribute their alias to the source module export surface.");
        var dependency = graph.Dependencies.Single();
        AssertEqual(SourceModuleDependencyKind.Import, dependency.Kind);
        AssertEqual("Main", dependency.FromModulePath);
        AssertEqual("Helper", dependency.ToModulePath);
        AssertEqual("./Helper", dependency.Specifier);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeReExportSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphReExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphReExport

            import { keep } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphReExport

            export { keep } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphReExport

            export fun keep(): string = "ok"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Unaliased relative source re-exports should contribute to the importing module export surface.");
        AssertEqual(2, graph.Dependencies.Count);
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Import &&
            dependency.FromModulePath == "Main" &&
            dependency.ToModulePath == "Barrel" &&
            dependency.Specifier == "./Barrel"), "Main should depend on the barrel module import.");
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Export &&
            dependency.FromModulePath == "Barrel" &&
            dependency.ToModulePath == "Helper" &&
            dependency.Specifier == "./Helper"), "Barrel should depend on the re-export target module.");
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeReExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphReExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphReExportAlias

            import { publicKeep } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphReExportAlias

            export { keep as publicKeep } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphReExportAlias

            export fun keep(): string = "ok"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source re-export aliases should contribute their exported alias to the importing module export surface.");
        AssertEqual(2, graph.Dependencies.Count);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeModuleReExportAliasSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphModuleReExportAlias"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphModuleReExportAlias

            import { PublicTools as HelperTools } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphModuleReExportAlias

            export { Tools as PublicTools } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphModuleReExportAlias

            export module Tools {
              export fun keep(): string = "ok"
            }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source module re-export aliases should contribute their exported alias to the importing module export surface.");
        AssertEqual(2, graph.Dependencies.Count);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeValueReExportSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphValueReExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphValueReExport

            import { PublicName } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphValueReExport

            export { PublicName } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphValueReExport

            let InternalName: string = "Ada"
            export { InternalName as PublicName }
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source top-level value re-exports should contribute to the importing module value export surface.");
        AssertEqual(2, graph.Dependencies.Count);
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeTypeReExportSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphTypeReExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphTypeReExport

            import type { PublicModel as Model } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphTypeReExport

            export type { VisibleModel as PublicModel } from "./Models"
            """);
        WriteFile(root, "src/Models.tysh", """
            namespace Samples.SourceGraphTypeReExport

            export record VisibleModel(Name: string)
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source type re-exports should contribute their exported alias to the importing module type export surface.");
        AssertEqual(2, graph.Dependencies.Count);
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Import &&
            dependency.FromModulePath == "Main" &&
            dependency.ToModulePath == "Barrel" &&
            dependency.Specifier == "./Barrel"), "Main should depend on the barrel module type import.");
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Export &&
            dependency.FromModulePath == "Barrel" &&
            dependency.ToModulePath == "Models" &&
            dependency.Specifier == "./Models"), "Barrel should depend on the type re-export target module.");
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphCollectsRelativeStarReExportSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphStarReExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphStarReExport

            import { helper, PublicName } from "./Barrel"
            import type { VisibleModel } from "./Barrel"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphStarReExport

            export * from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphStarReExport

            export fun helper(): string = "helper"
            export let PublicName: string = "Ada"
            export record VisibleModel(Name: string)
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertFalse(graph.HasErrors, "Relative source star re-exports should forward the lowerable function/value/type export surface.");
        AssertEqual(3, graph.Dependencies.Count);
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Import &&
            dependency.FromModulePath == "Main" &&
            dependency.ToModulePath == "Barrel" &&
            dependency.Specifier == "./Barrel"), "Main should depend on the barrel module imports.");
        AssertTrue(graph.Dependencies.Any(dependency =>
            dependency.Kind == SourceModuleDependencyKind.Export &&
            dependency.FromModulePath == "Barrel" &&
            dependency.ToModulePath == "Helper" &&
            dependency.Specifier == "./Helper"), "Barrel should depend on the star re-export target module.");
        AssertEqual(0, graph.Diagnostics.Count);
    });
}

static void SourceModuleGraphReportsMissingNamedExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphExports"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphExports

            import { hidden, VisibleModel } from "./Helper"
            import type { HiddenModel } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphExports

            fun hidden(): string = "hidden"

            record HiddenModel(Name: string)
            record VisibleModel(Name: string)

            export type { VisibleModel }
            export fun shown(): string = "shown"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertTrue(graph.HasErrors, "Relative source named imports should be limited to the target module export surface.");
        AssertEqual(2, graph.Diagnostics.Count);
        AssertEqual("TS0114", graph.Diagnostics[0].Code);
        AssertContains("exported name 'hidden' was not found", graph.Diagnostics[0].Message);
        AssertEqual("TS0114", graph.Diagnostics[1].Code);
        AssertContains("exported type 'HiddenModel' was not found", graph.Diagnostics[1].Message);
    });
}

static void SourceModuleGraphReportsMissingReExportedNames()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphMissingReExport"));
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphMissingReExport

            export { hidden } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphMissingReExport

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertTrue(graph.HasErrors, "Relative source re-exports should be limited to the target module export surface.");
        var diagnostic = graph.Diagnostics.Single();
        AssertEqual("TS0114", diagnostic.Code);
        AssertContains("Source module re-export './Helper' resolves to 'Helper', but exported function, top-level value, or module 'hidden' was not found.", diagnostic.Message);
        AssertEqual("src/Barrel.tysh", diagnostic.File);
    });
}

static void SourceModuleGraphReportsMissingTypeReExportedNames()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphMissingTypeReExport"));
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.SourceGraphMissingTypeReExport

            export type { HiddenModel } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphMissingTypeReExport

            record HiddenModel(Name: string)
            export record VisibleModel(Name: string)
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertTrue(graph.HasErrors, "Relative source type re-exports should be limited to the target module type export surface.");
        var diagnostic = graph.Diagnostics.Single();
        AssertEqual("TS0114", diagnostic.Code);
        AssertContains("Source module re-export './Helper' resolves to 'Helper', but exported type 'HiddenModel' was not found.", diagnostic.Message);
        AssertEqual("src/Barrel.tysh", diagnostic.File);
    });
}

static void SourceModuleGraphReportsMissingNamespaceImportMembers()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceGraphNamespaceExports"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceGraphNamespaceExports

            import * as Helper from "./Helper"

            export fun mainValue(): string = Helper.hidden()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceGraphNamespaceExports

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var discovery = SourceDiscovery.Discover(manifest);
        var modules = discovery.SourceFiles
            .Select(sourceFile =>
            {
                var parseResult = TypeSharpParser.ParseText(File.ReadAllText(sourceFile.Path), sourceFile.RelativePath);
                return new SourceModule(sourceFile, Require(parseResult.Root, "Source should parse."));
            })
            .ToArray();

        var graph = SourceModuleGraph.Build(modules);

        AssertTrue(graph.HasErrors, "Relative source namespace imports should expose only the target module export surface.");
        var diagnostic = graph.Diagnostics.Single();
        AssertEqual("TS0114", diagnostic.Code);
        AssertContains("exported member 'hidden' was not found", diagnostic.Message);
    });
}

static void RuntimeProjectTargetsNet48()
{
    var project = File.ReadAllText(Path.Combine("lang", "TypeSharp.Runtime", "TypeSharp.Runtime.csproj"));
    var runtimeInfo = File.ReadAllText(Path.Combine("lang", "TypeSharp.Runtime", "TypeSharpRuntimeInfo.cs"));

    AssertContains("<TargetFramework>net48</TargetFramework>", project);
    AssertContains("<AssemblyName>TypeSharp.Runtime</AssemblyName>", project);
    AssertContains("<LangVersion>7.3</LangVersion>", project);
    AssertContains("namespace TypeSharp.Runtime", runtimeInfo);
    AssertContains("RuntimeAbiVersion = 0", runtimeInfo);
}

static void CoreProjectTargetsNet48()
{
    var project = File.ReadAllText(Path.Combine("lang", "TypeSharp.Core", "TypeSharp.Core.csproj"));
    var option = File.ReadAllText(Path.Combine("lang", "TypeSharp.Core", "Option.cs"));
    var result = File.ReadAllText(Path.Combine("lang", "TypeSharp.Core", "Result.cs"));
    var unit = File.ReadAllText(Path.Combine("lang", "TypeSharp.Core", "Unit.cs"));

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
    AssertNet48PackageFreeArtifact("lang/TypeSharp.Core/TypeSharp.Core.csproj");
    AssertNet48PackageFreeArtifact("lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj");

    AssertNoDisallowedNet5RuntimeApiReferences("lang/TypeSharp.Core");
    AssertNoDisallowedNet5RuntimeApiReferences("lang/TypeSharp.Runtime");
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

static void MetadataReaderIndexesFrameworkAssemblyPublicSymbols()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "FrameworkMetadata"

            [references]
            assemblies = [
              "mscorlib",
              "System",
              "System.Core"
            ]
            """);

        var manifest = Require(TypeSharpManifestLoader.Load(manifestPath).Manifest, "Manifest should be available.");
        var references = TypeSharpReferenceResolver.Resolve(manifest);
        var metadata = TypeSharpMetadataReader.Read(references);

        AssertFalse(metadata.HasErrors, "Framework metadata indexing should not produce diagnostics.");
        AssertSequence(["mscorlib", "System", "System.Core"], metadata.Assemblies.Select(assembly => assembly.Identity).ToArray());
        AssertSequence(
            [ResolvedReferenceKind.FrameworkAssembly, ResolvedReferenceKind.FrameworkAssembly, ResolvedReferenceKind.FrameworkAssembly],
            metadata.Assemblies.Select(assembly => assembly.ReferenceKind).ToArray());
        AssertTrue(metadata.Assemblies.All(assembly => assembly.IsFrameworkAssembly), "Framework metadata entries should report framework kind.");
        AssertTrue(metadata.Assemblies.All(assembly => assembly.Path is not null && File.Exists(assembly.Path)), "Framework metadata entries should point at readable net48 reference assemblies when available.");

        var stringType = metadata.Assemblies
            .Single(assembly => assembly.Identity == "mscorlib")
            .Types
            .Single(type => type.FullName == "System.String");
        AssertTrue(stringType.Methods.Any(method => method.IsStatic && method.Name == "Concat"), "Framework metadata should index public static methods from mscorlib.");
        AssertTrue(stringType.Fields.Any(field => field.IsStatic && field.Name == "Empty"), "Framework metadata should index public static fields from mscorlib.");
        var nullableType = metadata.Assemblies
            .Single(assembly => assembly.Identity == "mscorlib")
            .Types
            .Single(type => type.FullName == "System.Nullable");
        var compare = Require(nullableType.Methods.SingleOrDefault(method => method.Name == "Compare"), "Nullable.Compare<T> metadata should be present.");
        AssertTrue(compare.GenericParameters.Single().HasNotNullableValueTypeConstraint, "Nullable.Compare<T> should preserve the struct constraint.");
        AssertTrue(compare.GenericParameters.Single().HasDefaultConstructorConstraint, "Nullable.Compare<T> should preserve the value-type default constructor constraint.");
        AssertSequence(["System.ValueType"], compare.GenericParameters.Single().TypeConstraints.ToArray());

        var uriType = metadata.Assemblies
            .Single(assembly => assembly.Identity == "System")
            .Types
            .Single(type => type.FullName == "System.Uri");
        AssertTrue(uriType.Properties.Any(property => property.Name == "Host" && property.HasPublicGetter), "Framework metadata should index public properties from System.dll.");

        var enumerableType = metadata.Assemblies
            .Single(assembly => assembly.Identity == "System.Core")
            .Types
            .Single(type => type.FullName == "System.Linq.Enumerable");
        AssertTrue(enumerableType.Methods.Any(method => method.Name == "Where" && method.IsExtension), "Framework metadata should mark public extension methods from System.Core.");
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
            ["Legacy.Tools.LegacyApi", "Legacy.Tools.LegacyParams", "Legacy.Tools.LegacyByRef", "Legacy.Tools.LegacyOverloads", "Legacy.Tools.LegacyNullOverloads", "Legacy.Tools.LegacyNumeric", "Legacy.Tools.LegacyParamsOverloads", "Legacy.Tools.LegacyParamsAmbiguousOverloads", "Legacy.Tools.LegacyOptional", "Legacy.Tools.LegacyOptionalOverloads", "Legacy.Tools.LegacyNamedOverloads", "Legacy.Tools.LegacyDelegates", "Legacy.Tools.LegacyDelegateOverloads", "Legacy.Tools.LegacyEvents", "Legacy.Tools.LegacyMarkerAttribute", "Legacy.Tools.LegacyBox`1", "Legacy.Tools.LegacyDefaultConstructible", "Legacy.Tools.LegacyFormatter", "Legacy.Tools.LegacyFlexibleConstructor", "Legacy.Tools.LegacyParamsConstructor", "Legacy.Tools.LegacyAmbiguousConstructor", "Legacy.Tools.LegacyByteIndexer", "Legacy.Tools.LegacyOverloadedIndexer", "Legacy.Tools.LegacyAmbiguousIndexer", "Legacy.Tools.LegacyRelationshipIndexer", "Legacy.Tools.LegacyFields", "Legacy.Tools.LegacyExtensions", "Legacy.Tools.LegacyGenericMethods", "Legacy.Tools.LegacyGenericByRefMethods", "Legacy.Tools.ILegacyNamed", "Legacy.Tools.ILegacyTagged", "Legacy.Tools.LegacyNamed", "Legacy.Tools.LegacyNamedOwner", "Legacy.Tools.LegacyDualNamed", "Legacy.Tools.LegacyBaseNamed", "Legacy.Tools.LegacyIntermediateNamed", "Legacy.Tools.LegacyDerivedNamed"],
            assembly.Types.Select(type => type.FullName).ToArray());

        var legacyApi = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyApi"), "LegacyApi metadata should be present.");
        AssertSequence(["Echo"], legacyApi.Methods.Select(method => method.Name).ToArray());
        AssertSequence(["value"], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string"], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.Type).ToArray());
        AssertEqual(MetadataNullabilityKind.Unknown, legacyApi.Methods.Single().ReturnNullability);
        AssertSequence([MetadataByRefKind.None], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.ByRefKind).ToArray());
        AssertSequence([false], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.IsParams).ToArray());
        AssertSequence([false], legacyApi.Methods.Single().Parameters.Select(parameter => parameter.IsOptional).ToArray());
        AssertFalse(legacyApi.Methods.Single().IsExtension, "Normal static methods should not be marked as extension methods.");

        var legacyExtensions = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyExtensions"), "LegacyExtensions metadata should be present.");
        var shout = Require(legacyExtensions.Methods.SingleOrDefault(method => method.Name == "Shout"), "LegacyExtensions.Shout metadata should be present.");
        AssertTrue(shout.IsStatic, "Extension methods should remain static metadata methods.");
        AssertTrue(shout.IsExtension, "Extension methods should preserve the ExtensionAttribute marker.");
        var describe = Require(legacyExtensions.Methods.SingleOrDefault(method => method.Name == "Describe"), "LegacyExtensions.Describe metadata should be present.");
        AssertTrue(describe.IsExtension, "Extension methods over imported metadata receivers should preserve the ExtensionAttribute marker.");
        AssertEqual("string", describe.ReturnType);
        AssertSequence(["Legacy.Tools.ILegacyNamed"], describe.Parameters.Select(parameter => parameter.Type).ToArray());
        var describeObjectOnly = Require(legacyExtensions.Methods.SingleOrDefault(method => method.Name == "DescribeObjectOnly"), "LegacyExtensions.DescribeObjectOnly metadata should be present.");
        AssertTrue(describeObjectOnly.IsExtension, "Object fallback extension methods should preserve the ExtensionAttribute marker.");
        AssertSequence(["object"], describeObjectOnly.Parameters.Select(parameter => parameter.Type).ToArray());
        var describeSpecific = legacyExtensions.Methods.Where(method => method.Name == "DescribeSpecific").ToArray();
        AssertEqual(3, describeSpecific.Length);
        AssertTrue(describeSpecific.All(method => method.IsExtension), "Overloaded extension methods should preserve the ExtensionAttribute marker.");
        AssertSequence(
            ["Legacy.Tools.ILegacyNamed", "Legacy.Tools.LegacyBaseNamed", "object"],
            describeSpecific.Select(method => method.Parameters.Single().Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        AssertSequence(["value"], shout.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string"], shout.Parameters.Select(parameter => parameter.Type).ToArray());

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
        AssertSequence(["Item", "MutablePrefix", "Prefix"], legacyFormatter.Properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        AssertSequence([false, false, false], legacyFormatter.Properties.OrderBy(property => property.Name, StringComparer.Ordinal).Select(property => property.IsStatic).ToArray());
        var prefix = Require(legacyFormatter.Properties.SingleOrDefault(property => property.Name == "Prefix"), "LegacyFormatter.Prefix metadata should be present.");
        AssertTrue(prefix.HasPublicGetter, "LegacyFormatter.Prefix should expose a public getter.");
        AssertFalse(prefix.HasPublicSetter, "LegacyFormatter.Prefix should not expose a public setter.");
        var mutablePrefix = Require(legacyFormatter.Properties.SingleOrDefault(property => property.Name == "MutablePrefix"), "LegacyFormatter.MutablePrefix metadata should be present.");
        AssertTrue(mutablePrefix.HasPublicGetter, "LegacyFormatter.MutablePrefix should expose a public getter.");
        AssertTrue(mutablePrefix.HasPublicSetter, "LegacyFormatter.MutablePrefix should expose a public setter.");
        var item = Require(legacyFormatter.Properties.SingleOrDefault(property => property.Name == "Item"), "LegacyFormatter indexer metadata should be present.");
        AssertTrue(item.IsIndexer, "LegacyFormatter.Item should be marked as an indexer.");
        AssertEqual(1, item.ParameterCount);
        AssertSequence(["int"], item.ParameterTypes.ToArray());
        AssertSequence(["Format"], legacyFormatter.Methods.Select(method => method.Name).ToArray());
        AssertFalse(legacyFormatter.Methods.Single(method => method.Name == "Format").IsStatic, "LegacyFormatter.Format should be marked instance.");
        var legacyFormatterConstructor = Require(legacyFormatter.Constructors.SingleOrDefault(), "LegacyFormatter public constructor metadata should be present.");
        AssertSequence(["prefix"], legacyFormatterConstructor.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string"], legacyFormatterConstructor.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyFlexibleConstructor = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyFlexibleConstructor"), "LegacyFlexibleConstructor metadata should be present.");
        var flexibleConstructor = Require(legacyFlexibleConstructor.Constructors.SingleOrDefault(), "LegacyFlexibleConstructor public constructor metadata should be present.");
        AssertSequence(["prefix", "value"], flexibleConstructor.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "string"], flexibleConstructor.Parameters.Select(parameter => parameter.Type).ToArray());
        AssertSequence([false, true], flexibleConstructor.Parameters.Select(parameter => parameter.IsOptional).ToArray());
        AssertSequence([false, false], flexibleConstructor.Parameters.Select(parameter => parameter.IsParams).ToArray());

        var legacyParamsConstructor = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyParamsConstructor"), "LegacyParamsConstructor metadata should be present.");
        var paramsConstructor = Require(legacyParamsConstructor.Constructors.SingleOrDefault(), "LegacyParamsConstructor public constructor metadata should be present.");
        AssertSequence(["separator", "values"], paramsConstructor.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "string[]"], paramsConstructor.Parameters.Select(parameter => parameter.Type).ToArray());
        AssertSequence([false, true], paramsConstructor.Parameters.Select(parameter => parameter.IsParams).ToArray());

        var legacyAmbiguousConstructor = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyAmbiguousConstructor"), "LegacyAmbiguousConstructor metadata should be present.");
        var ambiguousConstructors = legacyAmbiguousConstructor.Constructors.ToArray();
        AssertEqual(2, ambiguousConstructors.Length);
        AssertSequence(
            ["Legacy.Tools.ILegacyNamed", "Legacy.Tools.ILegacyTagged"],
            ambiguousConstructors.Select(constructor => constructor.Parameters.Single().Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());

        var legacyByteIndexer = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyByteIndexer"), "LegacyByteIndexer metadata should be present.");
        var byteItem = Require(legacyByteIndexer.Properties.SingleOrDefault(property => property.Name == "Item"), "LegacyByteIndexer indexer metadata should be present.");
        AssertTrue(byteItem.IsIndexer, "LegacyByteIndexer.Item should be marked as an indexer.");
        AssertEqual(1, byteItem.ParameterCount);
        AssertSequence(["byte"], byteItem.ParameterTypes.ToArray());

        var legacyOverloadedIndexer = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyOverloadedIndexer"), "LegacyOverloadedIndexer metadata should be present.");
        var overloadedItems = legacyOverloadedIndexer.Properties.Where(property => property.Name == "Item").ToArray();
        AssertEqual(2, overloadedItems.Length);
        AssertSequence(["int", "object"], overloadedItems.Select(property => property.ParameterTypes.Single()).OrderBy(type => type, StringComparer.Ordinal).ToArray());

        var legacyAmbiguousIndexer = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyAmbiguousIndexer"), "LegacyAmbiguousIndexer metadata should be present.");
        var ambiguousItems = legacyAmbiguousIndexer.Properties.Where(property => property.Name == "Item").ToArray();
        AssertEqual(2, ambiguousItems.Length);
        AssertSequence(["Legacy.Tools.ILegacyNamed", "Legacy.Tools.ILegacyTagged"], ambiguousItems.Select(property => property.ParameterTypes.Single()).OrderBy(type => type, StringComparer.Ordinal).ToArray());

        var legacyRelationshipIndexer = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyRelationshipIndexer"), "LegacyRelationshipIndexer metadata should be present.");
        var relationshipItems = legacyRelationshipIndexer.Properties.Where(property => property.Name == "Item").ToArray();
        AssertEqual(3, relationshipItems.Length);
        AssertSequence(["Legacy.Tools.ILegacyNamed", "Legacy.Tools.LegacyBaseNamed", "object"], relationshipItems.Select(property => property.ParameterTypes.Single()).OrderBy(type => type, StringComparer.Ordinal).ToArray());

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

        var legacyDelegateOverloads = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyDelegateOverloads"), "LegacyDelegateOverloads metadata should be present.");
        var delegatePicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "Pick").ToArray();
        AssertEqual(2, delegatePicks.Length);
        AssertSequence(
            ["System.Func`2<string, string>", "System.Func`3<string, string, string>"],
            delegatePicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickReturn").ToArray();
        AssertEqual(2, delegateReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<string, int>", "System.Func`2<string, string>"],
            delegateReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateReturnWideningPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickReturnWidening").ToArray();
        AssertEqual(2, delegateReturnWideningPicks.Length);
        AssertSequence(
            ["System.Func`2<string, int>", "System.Func`2<string, long>"],
            delegateReturnWideningPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateMemberReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickMemberReturn").ToArray();
        AssertEqual(2, delegateMemberReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamed, int>", "System.Func`2<Legacy.Tools.LegacyNamed, string>"],
            delegateMemberReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateChainedMemberReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickChainedMemberReturn").ToArray();
        AssertEqual(2, delegateChainedMemberReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamedOwner, int>", "System.Func`2<Legacy.Tools.LegacyNamedOwner, string>"],
            delegateChainedMemberReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateMethodReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickMethodReturn").ToArray();
        AssertEqual(2, delegateMethodReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamedOwner, int>", "System.Func`2<Legacy.Tools.LegacyNamedOwner, string>"],
            delegateMethodReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateExtensionMethodReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickExtensionMethodReturn").ToArray();
        AssertEqual(2, delegateExtensionMethodReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamed, int>", "System.Func`2<Legacy.Tools.LegacyNamed, string>"],
            delegateExtensionMethodReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateStaticMethodReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickStaticMethodReturn").ToArray();
        AssertEqual(2, delegateStaticMethodReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamed, int>", "System.Func`2<Legacy.Tools.LegacyNamed, string>"],
            delegateStaticMethodReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateBinaryReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickBinaryReturn").ToArray();
        AssertEqual(2, delegateBinaryReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<Legacy.Tools.LegacyNamed, bool>", "System.Func`2<Legacy.Tools.LegacyNamed, string>"],
            delegateBinaryReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var delegateIdentityReturnPicks = legacyDelegateOverloads.Methods.Where(method => method.Name == "PickIdentityReturn").ToArray();
        AssertEqual(2, delegateIdentityReturnPicks.Length);
        AssertSequence(
            ["System.Func`2<string, int>", "System.Func`2<string, string>"],
            delegateIdentityReturnPicks.Select(method => method.Parameters[1].Type).OrderBy(type => type, StringComparer.Ordinal).ToArray());
        var requiresBinary = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresBinary"), "RequiresBinary metadata should be present.");
        AssertSequence(["value", "combine"], requiresBinary.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "System.Func`3<string, string, string>"], requiresBinary.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresString = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresString"), "RequiresString metadata should be present.");
        AssertSequence(["value", "transform"], requiresString.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "System.Func`2<string, string>"], requiresString.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresIntReturn = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresIntReturn"), "RequiresIntReturn metadata should be present.");
        AssertSequence(["value", "transform"], requiresIntReturn.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["string", "System.Func`2<string, int>"], requiresIntReturn.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresMemberInt = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresMemberInt"), "RequiresMemberInt metadata should be present.");
        AssertSequence(["value", "transform"], requiresMemberInt.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamed", "System.Func`2<Legacy.Tools.LegacyNamed, int>"], requiresMemberInt.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresNestedMemberInt = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresNestedMemberInt"), "RequiresNestedMemberInt metadata should be present.");
        AssertSequence(["value", "transform"], requiresNestedMemberInt.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamedOwner", "System.Func`2<Legacy.Tools.LegacyNamedOwner, int>"], requiresNestedMemberInt.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresMethodInt = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresMethodInt"), "RequiresMethodInt metadata should be present.");
        AssertSequence(["value", "transform"], requiresMethodInt.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamedOwner", "System.Func`2<Legacy.Tools.LegacyNamedOwner, int>"], requiresMethodInt.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresExtensionMethodInt = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresExtensionMethodInt"), "RequiresExtensionMethodInt metadata should be present.");
        AssertSequence(["value", "transform"], requiresExtensionMethodInt.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamed", "System.Func`2<Legacy.Tools.LegacyNamed, int>"], requiresExtensionMethodInt.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresStaticMethodInt = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresStaticMethodInt"), "RequiresStaticMethodInt metadata should be present.");
        AssertSequence(["value", "transform"], requiresStaticMethodInt.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamed", "System.Func`2<Legacy.Tools.LegacyNamed, int>"], requiresStaticMethodInt.Parameters.Select(parameter => parameter.Type).ToArray());
        var requiresBinaryReturnString = Require(legacyDelegateOverloads.Methods.SingleOrDefault(method => method.Name == "RequiresBinaryReturnString"), "RequiresBinaryReturnString metadata should be present.");
        AssertSequence(["value", "transform"], requiresBinaryReturnString.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyNamed", "System.Func`2<Legacy.Tools.LegacyNamed, string>"], requiresBinaryReturnString.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyEvents = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyEvents"), "LegacyEvents metadata should be present.");
        AssertTrue(legacyEvents.Methods.Any(method => method.Name == "Raise"), "LegacyEvents public methods should include Raise while event accessors remain special-name metadata.");
        var transform = Require(legacyEvents.Events.SingleOrDefault(eventSymbol => eventSymbol.Name == "Transform"), "LegacyEvents.Transform event metadata should be present.");
        AssertEqual("System.Func`2<string, string>", transform.Type);
        AssertFalse(transform.IsStatic, "LegacyEvents.Transform should be marked instance.");
        AssertTrue(transform.HasPublicAdder, "LegacyEvents.Transform should expose a public add accessor.");
        AssertTrue(transform.HasPublicRemover, "LegacyEvents.Transform should expose a public remove accessor.");

        var legacyMarkerAttribute = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyMarkerAttribute"), "LegacyMarkerAttribute metadata should be present.");
        AssertSequence(["Name"], legacyMarkerAttribute.Properties.Select(property => property.Name).ToArray());

        var legacyBox = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyBox`1"), "LegacyBox<T> metadata should be present.");
        AssertSequence(["Value"], legacyBox.Properties.Select(property => property.Name).ToArray());
        var legacyBoxConstructor = Require(legacyBox.Constructors.SingleOrDefault(), "LegacyBox<T> public constructor metadata should be present.");
        AssertSequence(["value"], legacyBoxConstructor.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["!0"], legacyBoxConstructor.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyNamedOwner = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyNamedOwner"), "LegacyNamedOwner metadata should be present.");
        var owner = Require(legacyNamedOwner.Properties.SingleOrDefault(property => property.Name == "Owner"), "LegacyNamedOwner.Owner metadata should be present.");
        AssertFalse(owner.IsStatic, "LegacyNamedOwner.Owner should be an instance property.");
        AssertTrue(owner.HasPublicGetter, "LegacyNamedOwner.Owner should expose a public getter.");
        AssertEqual("Legacy.Tools.LegacyNamed", owner.Type);
        var legacyNamedForDisplay = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyNamed"), "LegacyNamed metadata should be present.");
        var display = Require(legacyNamedForDisplay.Methods.SingleOrDefault(method => method.Name == "Display"), "LegacyNamed.Display metadata should be present.");
        AssertFalse(display.IsStatic, "LegacyNamed.Display should be an instance method.");
        AssertEqual("string", display.ReturnType);
        AssertSequence(Array.Empty<string>(), display.Parameters.Select(parameter => parameter.Type).ToArray());

        var legacyFields = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyFields"), "LegacyFields metadata should be present.");
        AssertSequence(["MutableStaticName", "StaticName"], legacyFields.Properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        var staticName = Require(legacyFields.Properties.SingleOrDefault(property => property.Name == "StaticName"), "StaticName property metadata should be present.");
        AssertTrue(staticName.IsStatic, "StaticName should be marked static.");
        AssertTrue(staticName.HasPublicGetter, "StaticName should expose a public getter.");
        AssertFalse(staticName.HasPublicSetter, "StaticName should not expose a public setter.");
        var mutableStaticName = Require(legacyFields.Properties.SingleOrDefault(property => property.Name == "MutableStaticName"), "MutableStaticName property metadata should be present.");
        AssertTrue(mutableStaticName.IsStatic, "MutableStaticName should be marked static.");
        AssertTrue(mutableStaticName.HasPublicGetter, "MutableStaticName should expose a public getter.");
        AssertTrue(mutableStaticName.HasPublicSetter, "MutableStaticName should expose a public setter.");
        AssertSequence(["InstanceCode", "MutableCode", "MutableStaticCode", "StaticCode"], legacyFields.Fields.Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        var staticCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "StaticCode"), "StaticCode field metadata should be present.");
        AssertEqual("string", staticCode.Type);
        AssertTrue(staticCode.IsStatic, "StaticCode should be marked static.");
        AssertTrue(staticCode.IsLiteral, "StaticCode should be marked literal.");
        AssertFalse(staticCode.IsReadOnly, "StaticCode should not be marked readonly.");
        var mutableStaticCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "MutableStaticCode"), "MutableStaticCode field metadata should be present.");
        AssertEqual("string", mutableStaticCode.Type);
        AssertTrue(mutableStaticCode.IsStatic, "MutableStaticCode should be marked static.");
        AssertFalse(mutableStaticCode.IsLiteral, "MutableStaticCode should not be marked literal.");
        AssertFalse(mutableStaticCode.IsReadOnly, "MutableStaticCode should not be marked readonly.");
        var instanceCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "InstanceCode"), "InstanceCode field metadata should be present.");
        AssertEqual("string", instanceCode.Type);
        AssertFalse(instanceCode.IsStatic, "InstanceCode should not be marked static.");
        AssertFalse(instanceCode.IsLiteral, "InstanceCode should not be marked literal.");
        AssertTrue(instanceCode.IsReadOnly, "InstanceCode should be marked readonly.");
        var mutableCode = Require(legacyFields.Fields.SingleOrDefault(field => field.Name == "MutableCode"), "MutableCode field metadata should be present.");
        AssertEqual("string", mutableCode.Type);
        AssertFalse(mutableCode.IsStatic, "MutableCode should not be marked static.");
        AssertFalse(mutableCode.IsLiteral, "MutableCode should not be marked literal.");
        AssertFalse(mutableCode.IsReadOnly, "MutableCode should not be marked readonly.");

        var legacyGenericMethods = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyGenericMethods"), "LegacyGenericMethods metadata should be present.");
        var identity = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "Identity"), "Identity<T> metadata should be present.");
        AssertEqual("!!0", identity.ReturnType);
        AssertTrue(identity.IsStatic, "Identity<T> should be marked static.");
        AssertEqual(1, identity.GenericParameterCount);
        AssertSequence(["value"], identity.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["!!0"], identity.Parameters.Select(parameter => parameter.Type).ToArray());
        var requireClass = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "RequireClass"), "RequireClass<T> metadata should be present.");
        AssertEqual(1, requireClass.GenericParameters.Count);
        AssertTrue(requireClass.GenericParameters.Single().HasReferenceTypeConstraint, "RequireClass<T> should preserve the class constraint.");
        var requireStruct = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "RequireStruct"), "RequireStruct<T> metadata should be present.");
        AssertEqual(1, requireStruct.GenericParameters.Count);
        AssertTrue(requireStruct.GenericParameters.Single().HasNotNullableValueTypeConstraint, "RequireStruct<T> should preserve the struct constraint.");
        var create = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "Create"), "Create<T> metadata should be present.");
        AssertTrue(create.GenericParameters.Single().HasDefaultConstructorConstraint, "Create<T> should preserve the new() constraint.");
        var requireNamed = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "RequireNamed"), "RequireNamed<T> metadata should be present.");
        AssertSequence(["Legacy.Tools.ILegacyNamed"], requireNamed.GenericParameters.Single().TypeConstraints.ToArray());
        var requireBaseNamed = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "RequireBaseNamed"), "RequireBaseNamed<T> metadata should be present.");
        AssertSequence(["Legacy.Tools.LegacyBaseNamed"], requireBaseNamed.GenericParameters.Single().TypeConstraints.ToArray());
        var requireBoxedNamed = Require(legacyGenericMethods.Methods.SingleOrDefault(method => method.Name == "RequireBoxedNamed"), "RequireBoxedNamed<T> metadata should be present.");
        AssertSequence(["box"], requireBoxedNamed.Parameters.Select(parameter => parameter.Name).ToArray());
        AssertSequence(["Legacy.Tools.LegacyBox`1<!!0>"], requireBoxedNamed.Parameters.Select(parameter => parameter.Type).ToArray());
        AssertSequence(["Legacy.Tools.ILegacyNamed"], requireBoxedNamed.GenericParameters.Single().TypeConstraints.ToArray());

        var legacyDefaultConstructible = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyDefaultConstructible"), "LegacyDefaultConstructible metadata should be present.");
        AssertTrue(legacyDefaultConstructible.HasPublicParameterlessConstructor, "LegacyDefaultConstructible should expose a public parameterless constructor.");
        AssertTrue(legacyDefaultConstructible.Constructors.Any(constructor => constructor.Parameters.Count == 0), "LegacyDefaultConstructible parameterless constructor metadata should be present.");

        var legacyNamedInterface = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.ILegacyNamed"), "ILegacyNamed metadata should be present.");
        AssertSequence(["Name"], legacyNamedInterface.Properties.Select(property => property.Name).ToArray());
        AssertTrue(legacyNamedInterface.IsInterface, "ILegacyNamed should be marked as an interface.");
        var legacyNamed = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyNamed"), "LegacyNamed metadata should be present.");
        AssertSequence(["Name"], legacyNamed.Properties.Select(property => property.Name).ToArray());
        AssertSequence(["Legacy.Tools.ILegacyNamed"], legacyNamed.InterfaceNames.ToArray());
        var legacyBaseNamed = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyBaseNamed"), "LegacyBaseNamed metadata should be present.");
        AssertSequence(["Name"], legacyBaseNamed.Properties.Select(property => property.Name).ToArray());
        AssertSequence(["Legacy.Tools.ILegacyNamed"], legacyBaseNamed.InterfaceNames.ToArray());
        var legacyIntermediateNamed = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyIntermediateNamed"), "LegacyIntermediateNamed metadata should be present.");
        AssertEqual("Legacy.Tools.LegacyBaseNamed", legacyIntermediateNamed.BaseTypeName);
        var legacyDerivedNamed = Require(assembly.Types.SingleOrDefault(type => type.FullName == "Legacy.Tools.LegacyDerivedNamed"), "LegacyDerivedNamed metadata should be present.");
        AssertEqual("Legacy.Tools.LegacyIntermediateNamed", legacyDerivedNamed.BaseTypeName);
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

static void CheckerReportsExplicitGenericByRefInteropDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ExplicitGenericByRef"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ExplicitGenericByRef"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ExplicitGenericByRef

            import { LegacyGenericByRefMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericByRefMethods.Echo<string>("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Explicit generic C# call should use generic overload metadata for byref diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2403");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Legacy.Tools.LegacyGenericByRefMethods.Echo", diagnostic.Message);
        AssertContains("expects parameter 'value' to be passed with 'ref'", diagnostic.Message);
        AssertContains("but the argument uses no byref modifier", diagnostic.Message);
    });
}

static void CheckerReportsUnsatisfiedCSharpGenericConstraintDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedCSharpGenericConstraint

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): int =
              LegacyGenericMethods.RequireClass<int>(42)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Unsatisfied C# generic constraint should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2417");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Legacy.Tools.LegacyGenericMethods.RequireClass", diagnostic.Message);
        AssertContains("Type argument 'int'", diagnostic.Message);
        AssertContains("must satisfy the C# 'class' constraint", diagnostic.Message);
    });
}

static void CheckerReportsUnsatisfiedFrameworkCSharpGenericConstraintDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedFrameworkCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedFrameworkCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["mscorlib"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedFrameworkCSharpGenericConstraint

            import { Nullable } from "System"

            export fun broken(): int =
              Nullable.Compare<string>(null, null)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Unsatisfied framework C# generic constraint should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2417");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("System.Nullable.Compare", diagnostic.Message);
        AssertContains("Type argument 'string'", diagnostic.Message);
        AssertContains("must satisfy the C# 'struct' constraint", diagnostic.Message);
    });
}

static void CheckerAcceptsTransitiveCSharpGenericTypeConstraints()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "TransitiveCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.TransitiveCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.TransitiveCSharpGenericConstraint

            import { LegacyGenericMethods, LegacyDerivedNamed } from "Legacy.Tools"

            export fun keep(): string {
              let named = LegacyGenericMethods.RequireNamed<LegacyDerivedNamed>(LegacyDerivedNamed("Ada"))
              let baseNamed = LegacyGenericMethods.RequireBaseNamed<LegacyDerivedNamed>(named)
              baseNamed.Name
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2417"), "Transitive C# base/interface constraints should satisfy imported generic constraints.");
    });
}

static void CheckerAcceptsInferredCSharpGenericTypeConstraints()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InferredCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.InferredCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.InferredCSharpGenericConstraint

            import { LegacyGenericMethods, LegacyDerivedNamed } from "Legacy.Tools"

            export fun keepClass(): string =
              LegacyGenericMethods.RequireClass("value")

            export fun keepStruct(): int =
              LegacyGenericMethods.RequireStruct(42)

            export fun keepNamed(): LegacyDerivedNamed =
              LegacyGenericMethods.RequireNamed(LegacyDerivedNamed("Ada"))

            export fun keepTracked(value: LegacyDerivedNamed): LegacyDerivedNamed =
              LegacyGenericMethods.RequireBaseNamed(value)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.HasErrors, "Inferred C# generic constraints should pass for compatible literal and imported constructor arguments.");
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2417"), "Compatible inferred C# generic constraints should not produce TS2417.");
    });
}

static void CheckerReportsUnsatisfiedInferredCSharpGenericConstraintDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedInferredCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedInferredCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedInferredCSharpGenericConstraint

            import { LegacyFormatter, LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): LegacyFormatter =
              LegacyGenericMethods.RequireNamed(LegacyFormatter("legacy:"))
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Unsatisfied inferred C# generic constraint should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2417");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Legacy.Tools.LegacyGenericMethods.RequireNamed", diagnostic.Message);
        AssertContains("Type argument 'Legacy.Tools.LegacyFormatter'", diagnostic.Message);
        AssertContains("must satisfy the C# type constraint 'Legacy.Tools.ILegacyNamed'", diagnostic.Message);
    });
}

static void CheckerReportsUnsatisfiedInferredConstructedCSharpGenericConstraintDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedInferredConstructedCSharpGenericConstraint"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedInferredConstructedCSharpGenericConstraint"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedInferredConstructedCSharpGenericConstraint

            import { LegacyBox, LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.RequireBoxedNamed(LegacyBox<string>("Ada"))
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Unsatisfied inferred constructed C# generic constraint should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2417");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Legacy.Tools.LegacyGenericMethods.RequireBoxedNamed", diagnostic.Message);
        AssertContains("Type argument 'string'", diagnostic.Message);
        AssertContains("must satisfy the C# type constraint 'Legacy.Tools.ILegacyNamed'", diagnostic.Message);
    });
}

static void CheckerAcceptsImportedCSharpExtensionMethodInstanceSyntax()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionMethodInstanceSyntax"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionMethodInstanceSyntax"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionMethodInstanceSyntax

            import { LegacyNamed } from "Legacy.Tools"

            export fun describe(): string {
              let named = LegacyNamed("Ada")
              named.Describe()
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2410"), "Applicable C# extension methods should satisfy imported instance call syntax before emission.");
    });
}

static void CheckerAcceptsImportedCSharpExtensionReceiverRelationshipRanking()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionReceiverRelationshipRanking"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionReceiverRelationshipRanking"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionReceiverRelationshipRanking

            import { LegacyDerivedNamed } from "Legacy.Tools"

            export fun describe(): string {
              let derived = LegacyDerivedNamed("Ada")
              derived.DescribeSpecific()
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2402"), "Extension receiver metadata relationship ranking should avoid ambiguous overload diagnostics.");
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2406"), "Extension receiver metadata relationship ranking should keep the nearest receiver overload applicable.");
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2410"), "Applicable ranked C# extension methods should satisfy imported instance call syntax before emission.");
    });
}

static void CheckerAcceptsImportedCSharpExtensionReceiverObjectFallback()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionReceiverObjectFallback"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionReceiverObjectFallback"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionReceiverObjectFallback

            import { LegacyDerivedNamed } from "Legacy.Tools"

            export fun describe(): string {
              let derived = LegacyDerivedNamed("Ada")
              derived.DescribeObjectOnly()
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2406"), "Object receiver fallback should keep the extension overload applicable.");
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2410"), "Object fallback C# extension methods should satisfy imported instance call syntax before emission.");
    });
}

static void CheckerReportsNoMatchingCSharpExtensionOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingExtensionOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingExtensionOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingExtensionOverload

            import { LegacyNamed } from "Legacy.Tools"

            export fun broken(): string {
              let named = LegacyNamed("Ada")
              named.Describe(true)
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "No matching C# extension overload should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("named.Describe", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerAcceptsImportedCSharpInterfaceImplementationRelation()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInterfaceImplementationRelation"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInterfaceImplementationRelation"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInterfaceImplementationRelation

            import { ILegacyNamed, LegacyDerivedNamed } from "Legacy.Tools"

            export fun create(): ILegacyNamed =
              LegacyDerivedNamed("Ada")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2201"), "Imported C# class-to-interface metadata relations should satisfy TypeSharp assignment checks.");
    });
}

static void CheckerReportsNoMatchingCSharpOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingOverload

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.Identity<string, int>("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "No matching C# overload should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyGenericMethods.Identity", diagnostic.Message);
        AssertContains("2 explicit generic type argument", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpOverloadForKnownArgumentTypeDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingKnownArgumentTypeOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingKnownArgumentTypeOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingKnownArgumentTypeOverload

            import { LegacyApi } from "Legacy.Tools"

            export fun broken(): string =
              LegacyApi.Echo(true)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Known argument type C# overload mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyApi.Echo", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpOverloadForNumericLiteralConversionDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingNumericLiteralOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingNumericLiteralOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingNumericLiteralOverload

            import { LegacyNumeric } from "Legacy.Tools"

            export fun broken(): string =
              LegacyNumeric.FormatInt(1.5)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Impossible numeric literal C# overload conversion should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyNumeric.FormatInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpOverloadForImportedMetadataArgumentDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingImportedMetadataArgumentOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingImportedMetadataArgumentOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingImportedMetadataArgumentOverload

            import { LegacyFormatter, LegacyOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyOverloads.NeedNamed(LegacyFormatter("legacy:"))
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Imported metadata argument C# overload mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyOverloads.NeedNamed", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpOverloadForNullLiteralDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingNullLiteralOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingNullLiteralOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingNullLiteralOverload

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyNullOverloads.OnlyInt(null)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Null literal C# overload mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyNullOverloads.OnlyInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpConstructorDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingConstructor"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingConstructor"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingConstructor

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): LegacyFormatter =
              LegacyFormatter(42)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "No matching C# constructor should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("C# constructor 'LegacyFormatter'", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpGenericConstructorDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingGenericConstructor"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingGenericConstructor"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingGenericConstructor

            import { LegacyBox, LegacyNamed } from "Legacy.Tools"

            export fun broken(): LegacyBox<LegacyNamed> =
              LegacyBox<LegacyNamed>("Ada")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "No matching C# generic type constructor should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("C# constructor 'LegacyBox'", diagnostic.Message);
        AssertContains("1 explicit generic type argument", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsAmbiguousCSharpConstructorDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousConstructor"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousConstructor"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousConstructor

            import { LegacyAmbiguousConstructor, LegacyDualNamed } from "Legacy.Tools"

            export fun broken(): LegacyAmbiguousConstructor =
              LegacyAmbiguousConstructor(LegacyDualNamed("Ada"))
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous C# constructor should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("C# constructor 'LegacyAmbiguousConstructor'", diagnostic.Message);
        AssertContains("matches 2 overload candidates", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpMethodDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpMethod"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpMethod"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpMethod

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.DoesNotExist("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# method should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2407");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyGenericMethods", diagnostic.Message);
        AssertContains("does not contain a public static method named 'DoesNotExist'", diagnostic.Message);
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2409"), "Missing method calls should not also report missing static member diagnostics.");
    });
}

static void CheckerReportsMissingCSharpTypeDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpType"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpType"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpType

            import { LegacyMissing } from "Legacy.Tools"

            export fun broken(): string =
              LegacyMissing.Echo("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# type import should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2408");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Legacy.Tools", diagnostic.Message);
        AssertContains("does not contain a public type named 'LegacyMissing'", diagnostic.Message);
    });
}

static void CheckerReportsMissingFrameworkCSharpTypeDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpType"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpType"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpType

            import { DefinitelyMissing } from "System"

            export fun broken(): string =
              DefinitelyMissing.Echo("value")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing framework C# type import should produce diagnostics when framework metadata is available.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2408");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("System", diagnostic.Message);
        AssertContains("does not contain a public type named 'DefinitelyMissing'", diagnostic.Message);
    });
}

static void CheckerReportsMissingFrameworkCSharpMethodDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpMethod"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpMethod"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpMethod

            import { Uri } from "System"

            export fun broken(): string =
              Uri.DoesNotExist("https://example.com")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing framework C# method should produce diagnostics when framework metadata is available.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2407");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Uri", diagnostic.Message);
        AssertContains("does not contain a public static method named 'DoesNotExist'", diagnostic.Message);
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2409"), "Missing framework method calls should not also report missing static member diagnostics.");
    });
}

static void CheckerReportsMissingCSharpStaticMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpStaticMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpStaticMember"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpStaticMember

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string =
              LegacyFields.MissingCode
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# static member should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2409");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyFields", diagnostic.Message);
        AssertContains("does not contain a public static member named 'MissingCode'", diagnostic.Message);
    });
}

static void CheckerReportsMissingFrameworkCSharpStaticMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpStaticMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpStaticMember"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpStaticMember

            import { Uri } from "System"

            export fun broken(): string =
              Uri.MissingValue
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing framework C# static member should produce diagnostics when framework metadata is available.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2409");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("Uri", diagnostic.Message);
        AssertContains("does not contain a public static member named 'MissingValue'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpInstanceMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceMember"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceMember

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.MissingValue
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# instance member should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2410");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("formatter", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance member named 'MissingValue'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpParameterInstanceMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpParameterInstanceMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpParameterInstanceMember"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpParameterInstanceMember

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string =
              formatter.MissingValue
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# parameter instance member should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2410");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("formatter", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance member named 'MissingValue'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpAliasInstanceMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpAliasInstanceMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpAliasInstanceMember"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpAliasInstanceMember

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              let alias = formatter
              alias.MissingValue
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# alias instance member should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2410");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("alias", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance member named 'MissingValue'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpAssignedInstanceMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpAssignedInstanceMember"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpAssignedInstanceMember"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpAssignedInstanceMember

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              let mut alias = "placeholder"
              alias = formatter
              alias.MissingValue
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# assigned instance member should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2410");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("alias", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance member named 'MissingValue'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpInstanceIndexerDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceIndexer"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceIndexer"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceIndexer

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              let fields = LegacyFields()
              fields[0]
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# instance indexer should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2411");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("fields", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFields", diagnostic.Message);
        AssertContains("does not contain a public instance indexer with 1 argument", diagnostic.Message);
    });
}

static void CheckerReportsMismatchedCSharpInstanceIndexerDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MismatchedCSharpInstanceIndexer"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MismatchedCSharpInstanceIndexer"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MismatchedCSharpInstanceIndexer

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter[true]
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Mismatched C# instance indexer argument should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2411");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("formatter", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance indexer compatible with argument type(s) 'bool'", diagnostic.Message);
    });
}

static void CheckerReportsMismatchedCSharpInstanceIndexerNumericLiteralDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MismatchedCSharpInstanceIndexerNumericLiteral"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MismatchedCSharpInstanceIndexerNumericLiteral"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MismatchedCSharpInstanceIndexerNumericLiteral

            import { LegacyByteIndexer } from "Legacy.Tools"

            export fun broken(): string {
              let indexer = LegacyByteIndexer()
              indexer[1.5]
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Mismatched C# instance indexer numeric literal argument should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2411");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("indexer", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyByteIndexer", diagnostic.Message);
        AssertContains("does not contain a public instance indexer compatible with argument type(s) 'double'", diagnostic.Message);
    });
}

static void CheckerReportsAmbiguousCSharpInstanceIndexerDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousCSharpInstanceIndexer"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousCSharpInstanceIndexer"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousCSharpInstanceIndexer

            import { LegacyAmbiguousIndexer, LegacyDualNamed } from "Legacy.Tools"

            export fun broken(): string {
              let indexer = LegacyAmbiguousIndexer()
              indexer[LegacyDualNamed("value")]
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous C# instance indexer should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("indexer", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyAmbiguousIndexer", diagnostic.Message);
        AssertContains("matches 2 indexer candidates", diagnostic.Message);
        AssertContains("make the indexer access unambiguous", diagnostic.Message);
    });
}

static void CheckerAcceptsImportedCSharpIndexerMetadataRelationshipRanking()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedIndexerMetadataRelationshipRanking"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedIndexerMetadataRelationshipRanking"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedIndexerMetadataRelationshipRanking

            import { LegacyDerivedNamed, LegacyRelationshipIndexer } from "Legacy.Tools"

            export fun pick(): string {
              let indexer = LegacyRelationshipIndexer()
              indexer[LegacyDerivedNamed("Ada")]
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2402"), "Imported C# indexer metadata relationship ranking should avoid ambiguous indexer diagnostics.");
        AssertFalse(result.Diagnostics.Any(diagnostic => diagnostic.Code == "TS2411"), "Imported C# indexer metadata relationship ranking should keep the indexer applicable.");
    });
}

static void CheckerReportsMissingCSharpInstancePropertySetterDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstancePropertySetter"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstancePropertySetter"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstancePropertySetter

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.Prefix = "updated"
              formatter.Prefix
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# instance property setter should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2412");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("formatter", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance setter for property 'Prefix'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpParameterInstancePropertySetterDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpParameterInstancePropertySetter"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpParameterInstancePropertySetter"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpParameterInstancePropertySetter

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              formatter.Prefix = "updated"
              formatter.Prefix
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# parameter instance property setter should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2412");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("formatter", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance setter for property 'Prefix'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpAnnotatedLocalInstancePropertySetterDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpAnnotatedLocalInstancePropertySetter"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpAnnotatedLocalInstancePropertySetter"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpAnnotatedLocalInstancePropertySetter

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              let alias: LegacyFormatter = formatter
              alias.Prefix = "updated"
              alias.Prefix
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# annotated local instance property setter should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2412");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("alias", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFormatter", diagnostic.Message);
        AssertContains("does not contain a public instance setter for property 'Prefix'", diagnostic.Message);
    });
}

static void CheckerReportsReadOnlyCSharpInstanceFieldAssignmentDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReadOnlyCSharpInstanceFieldAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ReadOnlyCSharpInstanceFieldAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReadOnlyCSharpInstanceFieldAssignment

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              let fields = LegacyFields()
              fields.InstanceCode = "updated"
              fields.InstanceCode
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Readonly C# instance field assignment should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2413");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("fields", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyFields", diagnostic.Message);
        AssertContains("cannot assign to readonly instance field 'InstanceCode'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpStaticPropertySetterDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpStaticPropertySetter"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpStaticPropertySetter"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpStaticPropertySetter

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              LegacyFields.StaticName = "updated"
              LegacyFields.StaticName
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# static property setter should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2414");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyFields", diagnostic.Message);
        AssertContains("does not contain a public static setter for property 'StaticName'", diagnostic.Message);
    });
}

static void CheckerReportsReadOnlyCSharpStaticFieldAssignmentDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReadOnlyCSharpStaticFieldAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ReadOnlyCSharpStaticFieldAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReadOnlyCSharpStaticFieldAssignment

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              LegacyFields.StaticCode = "updated"
              LegacyFields.StaticCode
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Readonly C# static field assignment should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2415");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyFields", diagnostic.Message);
        AssertContains("cannot assign to read-only static field 'StaticCode'", diagnostic.Message);
    });
}

static void CheckerReportsMissingCSharpInstanceEventDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceEvent"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceEvent"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceEvent

            import { LegacyEvents } from "Legacy.Tools"

            export fun broken(): string {
              let source = LegacyEvents()
              source.Missing += text => text
              source.Raise("value")
            }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Missing C# instance event should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2416");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("source", diagnostic.Message);
        AssertContains("Legacy.Tools.LegacyEvents", diagnostic.Message);
        AssertContains("does not contain a public instance event named 'Missing' with public add accessor", diagnostic.Message);
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

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyNullOverloads.Ambiguous(null)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Ambiguous C# overload interop should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2402");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("matches 2 overload candidates", diagnostic.Message);
        AssertContains("make the call unambiguous", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaOverload

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresBinary("Ada", text => text)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda overload arity mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresBinary", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaReturnOverload

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresString("Ada", text => 42)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresString", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaParameterReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaParameterReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaParameterReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaParameterReturnOverload

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresIntReturn("Ada", text => text)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda parameter return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresIntReturn", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaMemberReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaMemberReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaMemberReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaMemberReturnOverload

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresMemberInt(LegacyNamed("Ada"), item => item.Name)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda member return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresMemberInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaChainedMemberReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaChainedMemberReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaChainedMemberReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaChainedMemberReturnOverload

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresNestedMemberInt(LegacyNamedOwner("Ada"), item => item.Owner.Name)
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda chained member return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresNestedMemberInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaMethodReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaMethodReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaMethodReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaMethodReturnOverload

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresMethodInt(LegacyNamedOwner("Ada"), item => item.Owner.Display())
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda method return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresMethodInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaExtensionMethodReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaExtensionMethodReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaExtensionMethodReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaExtensionMethodReturnOverload

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresExtensionMethodInt(LegacyNamed("Ada"), item => item.Describe())
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda extension method return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresExtensionMethodInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaStaticMethodReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaStaticMethodReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaStaticMethodReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaStaticMethodReturnOverload

            import { LegacyDelegateOverloads, LegacyNamed, LegacyOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresStaticMethodInt(LegacyNamed("Ada"), item => LegacyOverloads.Describe(item))
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda static method return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresStaticMethodInt", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
    });
}

static void CheckerReportsNoMatchingCSharpDelegateLambdaBinaryReturnOverloadDiagnostics()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaBinaryReturnOverload"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaBinaryReturnOverload"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaBinaryReturnOverload

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresBinaryReturnString(LegacyNamed("Ada"), item => item.Name == "Ada")
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "C# delegate lambda binary return mismatch should produce diagnostics.");
        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Code == "TS2406");
        AssertEqual("src/Main.tysh", diagnostic.File);
        AssertContains("LegacyDelegateOverloads.RequiresBinaryReturnString", diagnostic.Message);
        AssertContains("matches no overload candidate", diagnostic.Message);
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
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
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

static void CSharpOverloadResolverFiltersKnownArgumentTypeMismatch()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyOverloads.Pick(true)
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
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Known argument type mismatches should be removed from applicable overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible overload candidate.");
    AssertEqual("object", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverFiltersNumericLiteralConversionMismatch()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun broken(): string = LegacyNumeric.FormatInt(1.5)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNumeric",
        [
            new MetadataMethodSymbol(
                "FormatInt",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "int", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(0, resolution.ApplicableCandidates.Count);
    AssertTrue(resolution.SelectedCandidate is null, "Resolver should not select an impossible numeric conversion candidate.");

    parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun ok(): string = LegacyNumeric.FormatByte(42)
        """);
    root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNumeric",
        [
            new MetadataMethodSymbol(
                "FormatByte",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "byte", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);

    resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Integral constants that fit a smaller numeric parameter should remain applicable.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible numeric conversion candidate.");
    AssertEqual("byte", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverRanksNullLiteralReferenceMatch()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyNullOverloads.Pick(null)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNullOverloads",
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
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "int", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(2, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Null literal reference overload should outrank object fallback and reject value-type parameters.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible reference overload candidate.");
    AssertEqual("string", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverRanksNullLiteralNearestMetadataReference()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyNullOverloads.DescribeNamed(null)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNullOverloads",
        [
            new MetadataMethodSymbol(
                "DescribeNamed",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.LegacyBaseNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "DescribeNamed",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.LegacyDerivedNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "DescribeNamed",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);
    var metadata = new MetadataAssemblySymbol(
        "Legacy.Tools",
        ResolvedReferenceKind.LocalAssembly,
        "Legacy.Tools",
        Path: null,
        RelativePath: null)
    {
        Types =
        [
            metadataType,
            new MetadataTypeSymbol("Legacy.Tools", "LegacyBaseNamed", [], [], [], []),
            new MetadataTypeSymbol("Legacy.Tools", "LegacyDerivedNamed", [], [], [], [])
            {
                BaseTypeName = "Legacy.Tools.LegacyBaseNamed"
            }
        ]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [metadata]);

    AssertEqual(3, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Null literal metadata reference overload should choose the most specific metadata target.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the nearest null literal metadata reference overload candidate.");
    AssertEqual("Legacy.Tools.LegacyDerivedNamed", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverRanksNearestMetadataRelationship()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyOverloads.DescribeSpecific(LegacyDerivedNamed("Ada"))
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyOverloads",
        [
            new MetadataMethodSymbol(
                "DescribeSpecific",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.ILegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "DescribeSpecific",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.LegacyBaseNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "DescribeSpecific",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)])
        ],
        [],
        [],
        []);
    var metadata = new MetadataAssemblySymbol(
        "Legacy.Tools",
        ResolvedReferenceKind.LocalAssembly,
        "Legacy.Tools",
        Path: null,
        RelativePath: null)
    {
        Types =
        [
            metadataType,
            new MetadataTypeSymbol("Legacy.Tools", "ILegacyNamed", [], [], [], [])
            {
                IsInterface = true
            },
            new MetadataTypeSymbol("Legacy.Tools", "LegacyBaseNamed", [], [], [], [])
            {
                InterfaceNames = ["Legacy.Tools.ILegacyNamed"]
            },
            new MetadataTypeSymbol("Legacy.Tools", "LegacyIntermediateNamed", [], [], [], [])
            {
                BaseTypeName = "Legacy.Tools.LegacyBaseNamed"
            },
            new MetadataTypeSymbol("Legacy.Tools", "LegacyDerivedNamed", [], [], [], [])
            {
                BaseTypeName = "Legacy.Tools.LegacyIntermediateNamed"
            }
        ]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [metadata]);

    AssertEqual(3, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Nearest metadata relationship should outrank farther interface and object candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the nearest metadata relationship overload candidate.");
    AssertEqual("Legacy.Tools.LegacyBaseNamed", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverFiltersExplicitGenericArity()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyGenericOverloads.Pick<string>("value")
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyGenericOverloads",
        [
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false)]),
            new MetadataMethodSymbol(
                "Pick",
                "!!0",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "!!0", MetadataByRefKind.None, IsParams: false, IsOptional: false)],
                GenericParameterCount: 1)
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        explicitGenericTypeArgumentCount: 1);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Explicit generic type arguments should filter non-generic overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select one overload candidate.");
    AssertEqual(1, selected.Method.GenericParameterCount);
    AssertEqual("!!0", selected.Method.Parameters[0].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateArity()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.Pick("Ada", text => text)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "Pick",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("combine", "System.Func`3<string, string, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda parameter count should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate overload candidate.");
    AssertEqual("System.Func`2<string, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickReturn("Ada", text => 42)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda body return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate return overload candidate.");
    AssertEqual("System.Func`2<string, int>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateParameterReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickIdentityReturn("Ada", text => text)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickIdentityReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickIdentityReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda parameter return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate parameter return overload candidate.");
    AssertEqual("System.Func`2<string, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateMemberReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickMemberReturn(LegacyNamed("Ada"), item => item.Name)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [],
        [new MetadataPropertySymbol("Name", "string", IsStatic: false, HasPublicGetter: true, HasPublicSetter: false)],
        [],
        []);
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickMemberReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickMemberReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda member return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate member return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamed, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateChainedMemberReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickChainedMemberReturn(LegacyNamedOwner("Ada"), item => item.Owner.Name)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [],
        [new MetadataPropertySymbol("Name", "string", IsStatic: false, HasPublicGetter: true, HasPublicSetter: false)],
        [],
        []);
    var legacyNamedOwner = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamedOwner",
        [],
        [new MetadataPropertySymbol("Owner", "Legacy.Tools.LegacyNamed", IsStatic: false, HasPublicGetter: true, HasPublicSetter: false)],
        [],
        []);
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickChainedMemberReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamedOwner", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamedOwner, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickChainedMemberReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamedOwner", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamedOwner, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, legacyNamedOwner, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda chained member return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate chained member return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamedOwner, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateMethodReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickMethodReturn(LegacyNamedOwner("Ada"), item => item.Owner.Display())
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [
            new MetadataMethodSymbol(
                "Display",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [],
                IsStatic: false)
        ],
        [],
        [],
        []);
    var legacyNamedOwner = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamedOwner",
        [],
        [new MetadataPropertySymbol("Owner", "Legacy.Tools.LegacyNamed", IsStatic: false, HasPublicGetter: true, HasPublicSetter: false)],
        [],
        []);
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamedOwner", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamedOwner, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamedOwner", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamedOwner, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, legacyNamedOwner, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda method return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate method return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamedOwner, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateExtensionMethodReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickExtensionMethodReturn(LegacyNamed("Ada"), item => item.Describe())
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [],
        [],
        [],
        []);
    var legacyExtensions = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyExtensions",
        [
            new MetadataMethodSymbol(
                "Describe",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.ILegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)],
                IsStatic: true,
                IsExtension: true)
        ],
        [],
        [],
        []);
    var legacyNamedInterface = new MetadataTypeSymbol(
        "Legacy.Tools",
        "ILegacyNamed",
        [],
        [],
        [],
        [])
    {
        IsInterface = true
    };
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickExtensionMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickExtensionMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    legacyNamed = legacyNamed with
    {
        InterfaceNames = ["Legacy.Tools.ILegacyNamed"]
    };
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, legacyNamedInterface, legacyExtensions, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly],
        extensionNamespaces: ["Legacy.Tools"]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda extension method return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate extension method return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamed, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateStaticMethodReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickStaticMethodReturn(LegacyNamed("Ada"), item => LegacyOverloads.Describe(item))
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [],
        [],
        [],
        []);
    var legacyNamedInterface = new MetadataTypeSymbol(
        "Legacy.Tools",
        "ILegacyNamed",
        [],
        [],
        [],
        [])
    {
        IsInterface = true
    };
    var legacyOverloads = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyOverloads",
        [
            new MetadataMethodSymbol(
                "Describe",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "Legacy.Tools.ILegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false)],
                IsStatic: true),
            new MetadataMethodSymbol(
                "Describe",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [new MetadataParameterSymbol("value", "object", MetadataByRefKind.None, IsParams: false, IsOptional: false)],
                IsStatic: true)
        ],
        [],
        [],
        []);
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickStaticMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickStaticMethodReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    legacyNamed = legacyNamed with
    {
        InterfaceNames = ["Legacy.Tools.ILegacyNamed"]
    };
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, legacyNamedInterface, legacyOverloads, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda static method return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate static method return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamed, string>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverFiltersLambdaDelegateBinaryReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickBinaryReturn(LegacyNamed("Ada"), item => item.Name == "Ada")
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var legacyNamed = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyNamed",
        [],
        [new MetadataPropertySymbol("Name", "string", IsStatic: false, HasPublicGetter: true, HasPublicSetter: false)],
        [],
        []);
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickBinaryReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, bool>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickBinaryReturn",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "Legacy.Tools.LegacyNamed", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<Legacy.Tools.LegacyNamed, string>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);
    var assembly = new MetadataAssemblySymbol("Legacy.Tools", ResolvedReferenceKind.LocalAssembly, "Legacy.Tools", null, null)
    {
        Types = [legacyNamed, metadataType]
    };

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments,
        assemblies: [assembly]);

    AssertEqual(1, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Lambda binary expression return type should remove incompatible delegate overload candidates.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the compatible delegate binary return overload candidate.");
    AssertEqual("System.Func`2<Legacy.Tools.LegacyNamed, bool>", selected.Method.Parameters[1].Type);
}

static void CSharpOverloadResolverRanksLambdaDelegateReturnType()
{
    var parseResult = TypeSharpParser.ParseText("""
        namespace Samples.OverloadResolver

        fun choose(): string = LegacyDelegateOverloads.PickReturnWidening("Ada", text => 42)
        """);
    var root = Require(parseResult.Root, "Parser should produce a root syntax node.");
    var call = Require(FindFirstNode(root, SyntaxKind.CallExpression), "Test input should contain a call expression.");
    var arguments = call.Children.Skip(1).Where(child => !child.IsToken).ToArray();
    var metadataType = new MetadataTypeSymbol(
        "Legacy.Tools",
        "LegacyDelegateOverloads",
        [
            new MetadataMethodSymbol(
                "PickReturnWidening",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, long>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ]),
            new MetadataMethodSymbol(
                "PickReturnWidening",
                "string",
                MetadataNullabilityKind.NotApplicable,
                [
                    new MetadataParameterSymbol("value", "string", MetadataByRefKind.None, IsParams: false, IsOptional: false),
                    new MetadataParameterSymbol("transform", "System.Func`2<string, int>", MetadataByRefKind.None, IsParams: false, IsOptional: false)
                ])
        ],
        [],
        [],
        []);

    var resolution = TypeSharpCSharpOverloadResolver.Resolve(
        metadataType.Methods.Select(method => new CSharpOverloadCandidate(metadataType, method)),
        arguments);

    AssertEqual(2, resolution.ApplicableCandidates.Count);
    AssertFalse(resolution.IsAmbiguous, "Exact lambda body return type should outrank numeric delegate return conversions.");
    var selected = Require(resolution.SelectedCandidate, "Resolver should select the exact delegate return overload candidate.");
    AssertEqual("System.Func`2<string, int>", selected.Method.Parameters[1].Type);
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

            import { LegacyParamsAmbiguousOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyParamsAmbiguousOverloads.Pick(",", null, null)
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

static void CliCheckAcceptsRelativeSourceModuleImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("SourceModuleImportAliasJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.SourceModuleImportAliasJson

            import { Tools as HelperTools } from "./Helper"

            export fun read(): string = HelperTools.keep()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.SourceModuleImportAliasJson

            export module Tools {
              export fun keep(): string = "ok"
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("\"diagnostics\": []", output.ToString());
        AssertEqual(string.Empty, error.ToString());
    });
}

static void CliCheckEmitsJsonUnresolvedSourceModuleDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("MissingSourceImportJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingSourceImportJson

            import { Helper } from "./Missing"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0112\"", error.ToString());
        AssertContains("Source module specifier './Missing' could not be resolved from module 'Main'.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonMissingSourceModuleExportDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("MissingSourceExportJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingSourceExportJson

            import { hidden } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.MissingSourceExportJson

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module import './Helper' resolves to 'Helper', but exported name 'hidden' was not found.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonMissingSourceModuleReExportDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("MissingSourceReExportJson"));
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.MissingSourceReExportJson

            export { hidden } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.MissingSourceReExportJson

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module re-export './Helper' resolves to 'Helper', but exported function, top-level value, or module 'hidden' was not found.", error.ToString());
        AssertContains("\"file\": \"src/Barrel.tysh\"", error.ToString());
    });
}

static void CliCheckEmitsJsonMissingSourceModuleNamespaceMemberDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("MissingSourceNamespaceMemberJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingSourceNamespaceMemberJson

            import * as Helper from "./Helper"

            export fun mainValue(): string = Helper.hidden()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.MissingSourceNamespaceMemberJson

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module namespace import './Helper' resolves to 'Helper', but exported member 'hidden' was not found.", error.ToString());
        AssertContains("\"file\": \"src/Main.tysh\"", error.ToString());
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

static void CliBuildLowersRelativeSourceModuleImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceModuleAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceModuleAlias

            import { Tools as HelperTools } from "./Helper"

            export fun read(): string = HelperTools.keep()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.RelativeSourceModuleAlias

            export module Tools {
              export fun keep(): string = "ok"
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceModuleAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceModuleAlias;", generatedMain);
        AssertContains("using static Samples.RelativeSourceModuleAlias.ModuleHelper;", generatedMain);
        AssertContains("using HelperTools = Samples.RelativeSourceModuleAlias.Tools;", generatedMain);
        AssertContains("return HelperTools.keep();", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceModuleAlias.dll")), "Generated relative source module import alias project should compile to a net48 DLL.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingSourceModuleExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildMissingSourceExports"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildMissingSourceExports

            import { hidden } from "./Helper"

            export fun mainValue(): string = hidden()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.BuildMissingSourceExports

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module import './Helper' resolves to 'Helper', but exported name 'hidden' was not found.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when source module imports non-exported names.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildMissingSourceExports.Generated.csproj")), "Build should not emit generated project when source module imports non-exported names.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildMissingSourceExports.dll")), "Build should not emit generated assembly when source module imports non-exported names.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingSourceModuleReExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildMissingSourceReExports"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.BuildMissingSourceReExports

            export { hidden } from "./Helper"
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.BuildMissingSourceReExports

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module re-export './Helper' resolves to 'Helper', but exported function, top-level value, or module 'hidden' was not found.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Barrel.g.cs")), "Build should not emit generated C# when source module re-exports non-exported names.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildMissingSourceReExports.Generated.csproj")), "Build should not emit generated project when source module re-exports non-exported names.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildMissingSourceReExports.dll")), "Build should not emit generated assembly when source module re-exports non-exported names.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingSourceModuleNamespaceMembers()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "BuildMissingSourceNamespaceMembers"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.BuildMissingSourceNamespaceMembers

            import * as Helper from "./Helper"

            export fun mainValue(): string = Helper.hidden()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.BuildMissingSourceNamespaceMembers

            fun hidden(): string = "hidden"
            export fun shown(): string = "shown"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0114\"", error.ToString());
        AssertContains("Source module namespace import './Helper' resolves to 'Helper', but exported member 'hidden' was not found.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when source namespace import uses non-exported members.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "BuildMissingSourceNamespaceMembers.Generated.csproj")), "Build should not emit generated project when source namespace import uses non-exported members.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "BuildMissingSourceNamespaceMembers.dll")), "Build should not emit generated assembly when source namespace import uses non-exported members.");
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

static void CliBuildStopsBeforeEmissionOnUnsatisfiedCSharpGenericConstraint()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedCSharpGenericConstraintBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedCSharpGenericConstraintBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedCSharpGenericConstraintBuild

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.RequireNamed<string>("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2417\"", error.ToString());
        AssertContains("must satisfy the C# type constraint", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when unsatisfied C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "UnsatisfiedCSharpGenericConstraintBuild.Generated.csproj")), "Build should not emit generated project when unsatisfied C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "UnsatisfiedCSharpGenericConstraintBuild.dll")), "Build should not emit generated assembly when unsatisfied C# generic constraint diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnUnsatisfiedFrameworkCSharpGenericConstraint()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedFrameworkCSharpGenericConstraintBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedFrameworkCSharpGenericConstraintBuild"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["mscorlib"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedFrameworkCSharpGenericConstraintBuild

            import { Nullable } from "System"

            export fun broken(): int =
              Nullable.Compare<string>(null, null)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2417\"", error.ToString());
        AssertContains("System.Nullable.Compare", error.ToString());
        AssertContains("must satisfy the C# 'struct' constraint", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when unsatisfied framework C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "UnsatisfiedFrameworkCSharpGenericConstraintBuild.Generated.csproj")), "Build should not emit generated project when unsatisfied framework C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "UnsatisfiedFrameworkCSharpGenericConstraintBuild.dll")), "Build should not emit generated assembly when unsatisfied framework C# generic constraint diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnUnsatisfiedInferredCSharpGenericConstraint()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedInferredCSharpGenericConstraintBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedInferredCSharpGenericConstraintBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedInferredCSharpGenericConstraintBuild

            import { LegacyFormatter, LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): LegacyFormatter =
              LegacyGenericMethods.RequireNamed(LegacyFormatter("legacy:"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2417\"", error.ToString());
        AssertContains("Legacy.Tools.LegacyGenericMethods.RequireNamed", error.ToString());
        AssertContains("Legacy.Tools.LegacyFormatter", error.ToString());
        AssertContains("must satisfy the C# type constraint", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when unsatisfied inferred C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "UnsatisfiedInferredCSharpGenericConstraintBuild.Generated.csproj")), "Build should not emit generated project when unsatisfied inferred C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "UnsatisfiedInferredCSharpGenericConstraintBuild.dll")), "Build should not emit generated assembly when unsatisfied inferred C# generic constraint diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnUnsatisfiedInferredConstructedCSharpGenericConstraint()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedInferredConstructedCSharpGenericConstraintBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedInferredConstructedCSharpGenericConstraintBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedInferredConstructedCSharpGenericConstraintBuild

            import { LegacyBox, LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.RequireBoxedNamed(LegacyBox<string>("Ada"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2417\"", error.ToString());
        AssertContains("Legacy.Tools.LegacyGenericMethods.RequireBoxedNamed", error.ToString());
        AssertContains("Type argument 'string'", error.ToString());
        AssertContains("must satisfy the C# type constraint", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when unsatisfied inferred constructed C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "UnsatisfiedInferredConstructedCSharpGenericConstraintBuild.Generated.csproj")), "Build should not emit generated project when unsatisfied inferred constructed C# generic constraint diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "UnsatisfiedInferredConstructedCSharpGenericConstraintBuild.dll")), "Build should not emit generated assembly when unsatisfied inferred constructed C# generic constraint diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingOverloadBuild

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.Identity<string, int>("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingOverloadBuild.dll")), "Build should not emit generated assembly when no matching C# overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaOverloadBuild

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresBinary("Ada", text => text)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresBinary", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaReturnOverloadBuild

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresString("Ada", text => 42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresString", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaParameterReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaParameterReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaParameterReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaParameterReturnOverloadBuild

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresIntReturn("Ada", text => text)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresIntReturn", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda parameter return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaParameterReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda parameter return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaParameterReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda parameter return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaMemberReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaMemberReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaMemberReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaMemberReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresMemberInt(LegacyNamed("Ada"), item => item.Name)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresMemberInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda member return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaMemberReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda member return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaMemberReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda member return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaChainedMemberReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaChainedMemberReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaChainedMemberReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaChainedMemberReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresNestedMemberInt(LegacyNamedOwner("Ada"), item => item.Owner.Name)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresNestedMemberInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda chained member return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaChainedMemberReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda chained member return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaChainedMemberReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda chained member return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaMethodReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaMethodReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaMethodReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaMethodReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresMethodInt(LegacyNamedOwner("Ada"), item => item.Owner.Display())
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresMethodInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaMethodReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaMethodReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda method return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaExtensionMethodReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaExtensionMethodReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaExtensionMethodReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaExtensionMethodReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresExtensionMethodInt(LegacyNamed("Ada"), item => item.Describe())
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresExtensionMethodInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda extension method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaExtensionMethodReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda extension method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaExtensionMethodReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda extension method return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaStaticMethodReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaStaticMethodReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaStaticMethodReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaStaticMethodReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamed, LegacyOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresStaticMethodInt(LegacyNamed("Ada"), item => LegacyOverloads.Describe(item))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresStaticMethodInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda static method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaStaticMethodReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda static method return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaStaticMethodReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda static method return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpDelegateLambdaBinaryReturnOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingDelegateLambdaBinaryReturnOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingDelegateLambdaBinaryReturnOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingDelegateLambdaBinaryReturnOverloadBuild

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun broken(): string =
              LegacyDelegateOverloads.RequiresBinaryReturnString(LegacyNamed("Ada"), item => item.Name == "Ada")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyDelegateOverloads.RequiresBinaryReturnString", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching delegate lambda binary return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingDelegateLambdaBinaryReturnOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching delegate lambda binary return overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingDelegateLambdaBinaryReturnOverloadBuild.dll")), "Build should not emit generated assembly when no matching delegate lambda binary return overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnKnownArgumentTypeCSharpOverloadMismatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "KnownArgumentTypeOverloadMismatchBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.KnownArgumentTypeOverloadMismatchBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.KnownArgumentTypeOverloadMismatchBuild

            import { LegacyApi } from "Legacy.Tools"

            export fun broken(): string =
              LegacyApi.Echo(true)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyApi.Echo", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when known argument type C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "KnownArgumentTypeOverloadMismatchBuild.Generated.csproj")), "Build should not emit generated project when known argument type C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "KnownArgumentTypeOverloadMismatchBuild.dll")), "Build should not emit generated assembly when known argument type C# overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNumericLiteralCSharpOverloadMismatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NumericLiteralOverloadMismatchBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NumericLiteralOverloadMismatchBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NumericLiteralOverloadMismatchBuild

            import { LegacyNumeric } from "Legacy.Tools"

            export fun broken(): string =
              LegacyNumeric.FormatInt(1.5)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyNumeric.FormatInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when numeric literal C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NumericLiteralOverloadMismatchBuild.Generated.csproj")), "Build should not emit generated project when numeric literal C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NumericLiteralOverloadMismatchBuild.dll")), "Build should not emit generated assembly when numeric literal C# overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnImportedMetadataArgumentCSharpOverloadMismatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedMetadataArgumentOverloadMismatchBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedMetadataArgumentOverloadMismatchBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedMetadataArgumentOverloadMismatchBuild

            import { LegacyFormatter, LegacyOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyOverloads.NeedNamed(LegacyFormatter("legacy:"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyOverloads.NeedNamed", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when imported metadata argument C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "ImportedMetadataArgumentOverloadMismatchBuild.Generated.csproj")), "Build should not emit generated project when imported metadata argument C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedMetadataArgumentOverloadMismatchBuild.dll")), "Build should not emit generated assembly when imported metadata argument C# overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNullLiteralCSharpOverloadMismatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NullLiteralOverloadMismatchBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NullLiteralOverloadMismatchBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NullLiteralOverloadMismatchBuild

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun broken(): string =
              LegacyNullOverloads.OnlyInt(null)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("LegacyNullOverloads.OnlyInt", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when null literal C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NullLiteralOverloadMismatchBuild.Generated.csproj")), "Build should not emit generated project when null literal C# overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NullLiteralOverloadMismatchBuild.dll")), "Build should not emit generated assembly when null literal C# overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpConstructor()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingConstructorBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingConstructorBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingConstructorBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): LegacyFormatter =
              LegacyFormatter(42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("C# constructor", error.ToString());
        AssertContains("LegacyFormatter", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching C# constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingConstructorBuild.Generated.csproj")), "Build should not emit generated project when no matching C# constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingConstructorBuild.dll")), "Build should not emit generated assembly when no matching C# constructor diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpGenericConstructor()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingGenericConstructorBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingGenericConstructorBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingGenericConstructorBuild

            import { LegacyBox, LegacyNamed } from "Legacy.Tools"

            export fun broken(): LegacyBox<LegacyNamed> =
              LegacyBox<LegacyNamed>("Ada")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("C# constructor", error.ToString());
        AssertContains("LegacyBox", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching C# generic constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingGenericConstructorBuild.Generated.csproj")), "Build should not emit generated project when no matching C# generic constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingGenericConstructorBuild.dll")), "Build should not emit generated assembly when no matching C# generic constructor diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnAmbiguousCSharpConstructor()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousConstructorBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousConstructorBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousConstructorBuild

            import { LegacyAmbiguousConstructor, LegacyDualNamed } from "Legacy.Tools"

            export fun broken(): LegacyAmbiguousConstructor =
              LegacyAmbiguousConstructor(LegacyDualNamed("Ada"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2402\"", error.ToString());
        AssertContains("C# constructor", error.ToString());
        AssertContains("LegacyAmbiguousConstructor", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when ambiguous C# constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "AmbiguousConstructorBuild.Generated.csproj")), "Build should not emit generated project when ambiguous C# constructor diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "AmbiguousConstructorBuild.dll")), "Build should not emit generated assembly when ambiguous C# constructor diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpMethod()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpMethodBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpMethodBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpMethodBuild

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun broken(): string =
              LegacyGenericMethods.DoesNotExist("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2407\"", error.ToString());
        AssertContains("does not contain a public static method named", error.ToString());
        AssertFalse(error.ToString().Contains("\"code\": \"TS2409\"", StringComparison.Ordinal), "Missing method calls should not also emit TS2409.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# method diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpMethodBuild.Generated.csproj")), "Build should not emit generated project when missing C# method diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpMethodBuild.dll")), "Build should not emit generated assembly when missing C# method diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpType()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpTypeBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpTypeBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpTypeBuild

            import { LegacyMissing } from "Legacy.Tools"

            export fun broken(): string =
              LegacyMissing.Echo("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2408\"", error.ToString());
        AssertContains("does not contain a public type named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# type diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpTypeBuild.Generated.csproj")), "Build should not emit generated project when missing C# type diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpTypeBuild.dll")), "Build should not emit generated assembly when missing C# type diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpType()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpTypeBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpTypeBuild"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpTypeBuild

            import { DefinitelyMissing } from "System"

            export fun broken(): string =
              DefinitelyMissing.Echo("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2408\"", error.ToString());
        AssertContains("System", error.ToString());
        AssertContains("does not contain a public type named 'DefinitelyMissing'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing framework C# type diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingFrameworkCSharpTypeBuild.Generated.csproj")), "Build should not emit generated project when missing framework C# type diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingFrameworkCSharpTypeBuild.dll")), "Build should not emit generated assembly when missing framework C# type diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpMethod()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpMethodBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpMethodBuild"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpMethodBuild

            import { Uri } from "System"

            export fun broken(): string =
              Uri.DoesNotExist("https://example.com")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2407\"", error.ToString());
        AssertContains("Uri", error.ToString());
        AssertContains("does not contain a public static method named 'DoesNotExist'", error.ToString());
        AssertFalse(error.ToString().Contains("\"code\": \"TS2409\"", StringComparison.Ordinal), "Missing framework method calls should not also emit TS2409.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing framework C# method diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingFrameworkCSharpMethodBuild.Generated.csproj")), "Build should not emit generated project when missing framework C# method diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingFrameworkCSharpMethodBuild.dll")), "Build should not emit generated assembly when missing framework C# method diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpStaticMember()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpStaticMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpStaticMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpStaticMemberBuild

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string =
              LegacyFields.MissingCode
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2409\"", error.ToString());
        AssertContains("does not contain a public static member named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# static member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpStaticMemberBuild.Generated.csproj")), "Build should not emit generated project when missing C# static member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpStaticMemberBuild.dll")), "Build should not emit generated assembly when missing C# static member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingFrameworkCSharpStaticMember()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingFrameworkCSharpStaticMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingFrameworkCSharpStaticMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["System"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingFrameworkCSharpStaticMemberBuild

            import { Uri } from "System"

            export fun broken(): string =
              Uri.MissingValue
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2409\"", error.ToString());
        AssertContains("Uri", error.ToString());
        AssertContains("does not contain a public static member named 'MissingValue'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing framework C# static member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingFrameworkCSharpStaticMemberBuild.Generated.csproj")), "Build should not emit generated project when missing framework C# static member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingFrameworkCSharpStaticMemberBuild.dll")), "Build should not emit generated assembly when missing framework C# static member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpInstanceMember()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceMemberBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.MissingValue
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2410\"", error.ToString());
        AssertContains("does not contain a public instance member named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpInstanceMemberBuild.Generated.csproj")), "Build should not emit generated project when missing C# instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpInstanceMemberBuild.dll")), "Build should not emit generated assembly when missing C# instance member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpParameterInstanceMember()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpParameterInstanceMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpParameterInstanceMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpParameterInstanceMemberBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string =
              formatter.MissingValue
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2410\"", error.ToString());
        AssertContains("does not contain a public instance member named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# parameter instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpParameterInstanceMemberBuild.Generated.csproj")), "Build should not emit generated project when missing C# parameter instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpParameterInstanceMemberBuild.dll")), "Build should not emit generated assembly when missing C# parameter instance member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpAliasInstanceMember()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpAliasInstanceMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpAliasInstanceMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpAliasInstanceMemberBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              let alias = formatter
              alias.MissingValue
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2410\"", error.ToString());
        AssertContains("does not contain a public instance member named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# alias instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpAliasInstanceMemberBuild.Generated.csproj")), "Build should not emit generated project when missing C# alias instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpAliasInstanceMemberBuild.dll")), "Build should not emit generated assembly when missing C# alias instance member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpAssignedInstanceMember()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpAssignedInstanceMemberBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpAssignedInstanceMemberBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpAssignedInstanceMemberBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(formatter: LegacyFormatter): string {
              let mut alias = "placeholder"
              alias = formatter
              alias.MissingValue
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2410\"", error.ToString());
        AssertContains("does not contain a public instance member named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# assigned instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpAssignedInstanceMemberBuild.Generated.csproj")), "Build should not emit generated project when missing C# assigned instance member diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpAssignedInstanceMemberBuild.dll")), "Build should not emit generated assembly when missing C# assigned instance member diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnNoMatchingCSharpExtensionOverload()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NoMatchingExtensionOverloadBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NoMatchingExtensionOverloadBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NoMatchingExtensionOverloadBuild

            import { LegacyNamed } from "Legacy.Tools"

            export fun broken(): string {
              let named = LegacyNamed("Ada")
              named.Describe(true)
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2406\"", error.ToString());
        AssertContains("named.Describe", error.ToString());
        AssertContains("matches no overload candidate", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when no matching C# extension overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "NoMatchingExtensionOverloadBuild.Generated.csproj")), "Build should not emit generated project when no matching C# extension overload diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NoMatchingExtensionOverloadBuild.dll")), "Build should not emit generated assembly when no matching C# extension overload diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpInstanceIndexer()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceIndexerBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceIndexerBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceIndexerBuild

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              let fields = LegacyFields()
              fields[0]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2411\"", error.ToString());
        AssertContains("does not contain a public instance indexer", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpInstanceIndexerBuild.Generated.csproj")), "Build should not emit generated project when missing C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpInstanceIndexerBuild.dll")), "Build should not emit generated assembly when missing C# instance indexer diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMismatchedCSharpInstanceIndexer()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MismatchedCSharpInstanceIndexerBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MismatchedCSharpInstanceIndexerBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MismatchedCSharpInstanceIndexerBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter[true]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2411\"", error.ToString());
        AssertContains("does not contain a public instance indexer compatible with argument type(s) 'bool'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when mismatched C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MismatchedCSharpInstanceIndexerBuild.Generated.csproj")), "Build should not emit generated project when mismatched C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MismatchedCSharpInstanceIndexerBuild.dll")), "Build should not emit generated assembly when mismatched C# instance indexer diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMismatchedCSharpInstanceIndexerNumericLiteral()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MismatchedCSharpInstanceIndexerNumericLiteralBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MismatchedCSharpInstanceIndexerNumericLiteralBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MismatchedCSharpInstanceIndexerNumericLiteralBuild

            import { LegacyByteIndexer } from "Legacy.Tools"

            export fun broken(): string {
              let indexer = LegacyByteIndexer()
              indexer[1.5]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2411\"", error.ToString());
        AssertContains("Legacy.Tools.LegacyByteIndexer", error.ToString());
        AssertContains("does not contain a public instance indexer compatible with argument type(s) 'double'", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when mismatched C# instance indexer numeric literal diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MismatchedCSharpInstanceIndexerNumericLiteralBuild.Generated.csproj")), "Build should not emit generated project when mismatched C# instance indexer numeric literal diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MismatchedCSharpInstanceIndexerNumericLiteralBuild.dll")), "Build should not emit generated assembly when mismatched C# instance indexer numeric literal diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnAmbiguousCSharpInstanceIndexer()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "AmbiguousCSharpInstanceIndexerBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.AmbiguousCSharpInstanceIndexerBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.AmbiguousCSharpInstanceIndexerBuild

            import { LegacyAmbiguousIndexer, LegacyDualNamed } from "Legacy.Tools"

            export fun broken(): string {
              let indexer = LegacyAmbiguousIndexer()
              indexer[LegacyDualNamed("value")]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2402\"", error.ToString());
        AssertContains("Legacy.Tools.LegacyAmbiguousIndexer", error.ToString());
        AssertContains("matches 2 indexer candidates", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when ambiguous C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "AmbiguousCSharpInstanceIndexerBuild.Generated.csproj")), "Build should not emit generated project when ambiguous C# instance indexer diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "AmbiguousCSharpInstanceIndexerBuild.dll")), "Build should not emit generated assembly when ambiguous C# instance indexer diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpInstancePropertySetter()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstancePropertySetterBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstancePropertySetterBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstancePropertySetterBuild

            import { LegacyFormatter } from "Legacy.Tools"

            export fun broken(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.Prefix = "updated"
              formatter.Prefix
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2412\"", error.ToString());
        AssertContains("does not contain a public instance setter for property", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# instance property setter diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpInstancePropertySetterBuild.Generated.csproj")), "Build should not emit generated project when missing C# instance property setter diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpInstancePropertySetterBuild.dll")), "Build should not emit generated assembly when missing C# instance property setter diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnReadOnlyCSharpInstanceFieldAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReadOnlyCSharpInstanceFieldAssignmentBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ReadOnlyCSharpInstanceFieldAssignmentBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReadOnlyCSharpInstanceFieldAssignmentBuild

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              let fields = LegacyFields()
              fields.InstanceCode = "updated"
              fields.InstanceCode
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2413\"", error.ToString());
        AssertContains("cannot assign to readonly instance field", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when readonly C# instance field assignment diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "ReadOnlyCSharpInstanceFieldAssignmentBuild.Generated.csproj")), "Build should not emit generated project when readonly C# instance field assignment diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ReadOnlyCSharpInstanceFieldAssignmentBuild.dll")), "Build should not emit generated assembly when readonly C# instance field assignment diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpStaticPropertySetter()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpStaticPropertySetterBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpStaticPropertySetterBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpStaticPropertySetterBuild

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              LegacyFields.StaticName = "updated"
              LegacyFields.StaticName
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2414\"", error.ToString());
        AssertContains("does not contain a public static setter for property", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# static property setter diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpStaticPropertySetterBuild.Generated.csproj")), "Build should not emit generated project when missing C# static property setter diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpStaticPropertySetterBuild.dll")), "Build should not emit generated assembly when missing C# static property setter diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnReadOnlyCSharpStaticFieldAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ReadOnlyCSharpStaticFieldAssignmentBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ReadOnlyCSharpStaticFieldAssignmentBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ReadOnlyCSharpStaticFieldAssignmentBuild

            import { LegacyFields } from "Legacy.Tools"

            export fun broken(): string {
              LegacyFields.StaticCode = "updated"
              LegacyFields.StaticCode
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2415\"", error.ToString());
        AssertContains("cannot assign to read-only static field", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when readonly C# static field assignment diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "ReadOnlyCSharpStaticFieldAssignmentBuild.Generated.csproj")), "Build should not emit generated project when readonly C# static field assignment diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ReadOnlyCSharpStaticFieldAssignmentBuild.dll")), "Build should not emit generated assembly when readonly C# static field assignment diagnostics contain errors.");
    });
}

static void CliBuildStopsBeforeEmissionOnMissingCSharpInstanceEvent()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MissingCSharpInstanceEventBuild"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.MissingCSharpInstanceEventBuild"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingCSharpInstanceEventBuild

            import { LegacyEvents } from "Legacy.Tools"

            export fun broken(): string {
              let source = LegacyEvents()
              source.Missing -= text => text
              source.Raise("value")
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2416\"", error.ToString());
        AssertContains("does not contain a public instance event named", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when missing C# instance event diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "MissingCSharpInstanceEventBuild.Generated.csproj")), "Build should not emit generated project when missing C# instance event diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MissingCSharpInstanceEventBuild.dll")), "Build should not emit generated assembly when missing C# instance event diagnostics contain errors.");
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

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun choose(): string = LegacyNullOverloads.Ambiguous(null)
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

static void CliBuildStopsBeforeEmissionOnUnsatisfiedImportedCSharpInterfaceImplementation()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "UnsatisfiedImportedInterfaceImplementation"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.UnsatisfiedImportedInterfaceImplementation"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.UnsatisfiedImportedInterfaceImplementation

            import { ILegacyNamed, LegacyFormatter } from "Legacy.Tools"

            export fun broken(): ILegacyNamed =
              LegacyFormatter("legacy:")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2201\"", error.ToString());
        AssertContains("Cannot return expression of type 'LegacyFormatter' from function returning 'ILegacyNamed'.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when imported C# interface implementation diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "UnsatisfiedImportedInterfaceImplementation.Generated.csproj")), "Build should not emit generated project when imported C# interface implementation diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "UnsatisfiedImportedInterfaceImplementation.dll")), "Build should not emit generated assembly when imported C# interface implementation diagnostics contain errors.");
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
        AssertContains("Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.", error.ToString());
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

static void CliBuildStopsBeforeEmissionOnDuplicateLocalExport()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "DuplicateLocalExportBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DuplicateLocalExportBuild

            fun keep(): string = "ok"

            export { keep }
            export { keep }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2004\"", error.ToString());
        AssertContains("Duplicate export 'keep'.", error.ToString());
        AssertFalse(File.Exists(Path.Combine(root, "generated", "src", "Main.g.cs")), "Build should not emit generated C# when duplicate local export diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "DuplicateLocalExportBuild.Generated.csproj")), "Build should not emit generated project when duplicate local export diagnostics contain errors.");
        AssertFalse(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "DuplicateLocalExportBuild.dll")), "Build should not emit generated assembly when duplicate local export diagnostics contain errors.");
    });
}

static void CliBuildUsesLocalExportListsForPublicSurface()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalExportListBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalExportListBuild

            record Customer(Name: string)

            fun visible(): string = "visible"

            fun hidden(): string = "hidden"

            export type { Customer }
            export { visible }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalExportListBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static string visible()", generatedSource);
        AssertContains("internal static string hidden()", generatedSource);
        AssertContains("public sealed class Customer", generatedSource);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalExportListBuild.dll")), "Generated project with local export lists should compile to a net48 DLL.");
    });
}

static void CliBuildLowersLocalFunctionExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalExportAliasBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalExportAliasBuild

            import { publicKeep } from "./Helper"

            export fun mainValue(): string = publicKeep()
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.LocalExportAliasBuild

            fun keep(): string = "ok"

            export { keep as publicKeep }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalExportAliasBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.LocalExportAliasBuild.ModuleHelper;", generatedMain);
        AssertContains("return publicKeep();", generatedMain);
        AssertContains("internal static string keep()", generatedHelper);
        AssertContains("public static string publicKeep()", generatedHelper);
        AssertContains("return Samples.LocalExportAliasBuild.ModuleHelper.keep();", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalExportAliasBuild.dll")), "Generated project with local function export aliases should compile to a net48 DLL.");
    });
}

static void CliBuildLowersLocalLiteralExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalLiteralExportAliasBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalLiteralExportAliasBuild

            import { PublicVersion } from "./Helper"

            export fun version(): string = PublicVersion
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.LocalLiteralExportAliasBuild

            literal InternalVersion: string = "1.0"

            export { InternalVersion as PublicVersion }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalLiteralExportAliasBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.LocalLiteralExportAliasBuild.ModuleHelper;", generatedMain);
        AssertContains("return PublicVersion;", generatedMain);
        AssertContains("internal const string InternalVersion = \"1.0\";", generatedHelper);
        AssertContains("public const string PublicVersion = InternalVersion;", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalLiteralExportAliasBuild.dll")), "Generated project with local literal export aliases should compile to a net48 DLL.");
    });
}

static void CliBuildLowersLocalValueExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalValueExportAliasBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalValueExportAliasBuild

            import { PublicName } from "./Helper"

            export fun name(): string = PublicName
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.LocalValueExportAliasBuild

            let InternalName: string = "Ada"

            export { InternalName as PublicName }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalValueExportAliasBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.LocalValueExportAliasBuild.ModuleHelper;", generatedMain);
        AssertContains("return PublicName;", generatedMain);
        AssertContains("internal static readonly string InternalName = \"Ada\";", generatedHelper);
        AssertContains("public static string PublicName", generatedHelper);
        AssertContains("get { return InternalName; }", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalValueExportAliasBuild.dll")), "Generated project with local value export aliases should compile to a net48 DLL.");
    });
}

static void CliBuildLowersFunctionValueExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "FunctionValueExportBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.FunctionValueExportBuild

            import { Transform, PublicTransform } from "./Helper"

            export fun name(): string =
              Transform("Ada") + PublicTransform(" Lovelace")
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.FunctionValueExportBuild

            export let Transform: string -> string = text => text

            let internalTransform: string -> string = text => text
            export { internalTransform as PublicTransform }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/FunctionValueExportBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.FunctionValueExportBuild.ModuleHelper;", generatedMain);
        AssertContains("return Transform(\"Ada\") + PublicTransform(\" Lovelace\");", generatedMain);
        AssertContains("public static readonly System.Func<string, string> Transform = text => text;", generatedHelper);
        AssertContains("internal static readonly System.Func<string, string> internalTransform = text => text;", generatedHelper);
        AssertContains("public static System.Func<string, string> PublicTransform", generatedHelper);
        AssertContains("get { return internalTransform; }", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "FunctionValueExportBuild.dll")), "Generated project with function-valued let exports should compile to a net48 DLL.");
    });
}

static void CliBuildLowersLocalTypeExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LocalTypeExportAliasBuild"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.LocalTypeExportAliasBuild

            import type { Model } from "./Helper"

            export fun pass(model: Model): Model = model
            """);
        WriteFile(root, "src/Helper.tysh", """
            namespace Samples.LocalTypeExportAliasBuild

            record VisibleModel(Name: string)

            export type { VisibleModel as Model }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LocalTypeExportAliasBuild.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.LocalTypeExportAliasBuild;", generatedMain);
        AssertContains("using Model = Samples.LocalTypeExportAliasBuild.VisibleModel;", generatedMain);
        AssertContains("public static Model pass(Model model)", generatedMain);

        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public sealed class VisibleModel", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "LocalTypeExportAliasBuild.dll")), "Generated project with local type export aliases should compile to a net48 DLL.");
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

static void ManifestLoaderRejectsInvalidOptionDomains()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InvalidDomains"
            outputType = "console"

            [language]
            version = "stable"
            nullable = "permissive"

            [tooling]
            diagnosticFormat = "xml"
            """);

        var result = TypeSharpManifestLoader.Load(manifestPath);

        AssertTrue(result.HasErrors, "Unsupported manifest option values should produce errors.");
        AssertTrue(result.Manifest is null, "Invalid manifest option values should stop manifest loading.");
        AssertEqual(4, result.Diagnostics.Count(diagnostic => diagnostic.Code == "TS0103"));
        var messages = string.Join("\n", result.Diagnostics.Select(diagnostic => diagnostic.Message));
        AssertContains("Manifest value 'project.outputType' must be one of 'library', 'exe'.", messages);
        AssertContains("Manifest value 'language.version' must be 'preview'.", messages);
        AssertContains("Manifest value 'language.nullable' must be one of 'strict', 'loose'.", messages);
        AssertContains("Manifest value 'tooling.diagnosticFormat' must be one of 'text', 'json'.", messages);
    });
}

static void CliCheckEmitsJsonInvalidManifestValueDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "InvalidManifestJson"
            outputType = "console"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS0103\"", error.ToString());
        AssertContains("Manifest value 'project.outputType' must be one of 'library', 'exe'.", error.ToString());
        AssertContains("TypeSharp.toml", error.ToString());
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

static void CliRunUsesModulePathContainerForMultiSourceExecutable()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RunMultiSource"
            targetFramework = "net48"
            outputType = "exe"
            rootNamespace = "Samples.RunMultiSource"
            generatedOutputRoot = "generated"
            main = "Samples.RunMultiSource.main"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RunMultiSource

            export fun main(args: string[]): string = args.Length.ToString()
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RunMultiSource

            export fun helper(): string = "unused"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["run", manifestPath, "--", "alpha", "beta", "gamma"], output, error);

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs"));
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Feature", "Helper.g.cs"));
        var generatedProgram = File.ReadAllText(Path.Combine(root, "generated", "Program.g.cs"));
        AssertContains("public static class ModuleMain", generatedMain);
        AssertContains("public static class ModuleFeature_Helper", generatedHelper);
        AssertContains("ModuleMain.main(args)", generatedProgram);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RunMultiSource.exe")), "Run should build a generated multi-source net48 executable.");

        if (exitCode == 0)
        {
            AssertEqual($"3{Environment.NewLine}", output.ToString());
            AssertEqual(string.Empty, error.ToString());
            return;
        }

        AssertGeneratedExecutableLaunchBlocked(exitCode, output.ToString(), error.ToString(), "RunMultiSource");
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
    var input = File.ReadAllText(Path.Combine("test", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh"));
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
    var input = File.ReadAllText(Path.Combine("test", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh"));
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
        Path.Combine("test", "fixtures", "parser", "positive"),
        Path.Combine("test", "fixtures", "parser", "negative")
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
        var inputPath = Path.Combine("test", "fixtures", "backend", "csharp", "positive", "0004-block-local", CSharpBackendFixtureConventions.InputFileName);
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
            let functionValueAliasTarget: string -> string = text => text
            union Choice {
                Pick(value: string)
            }

            export { functionValueAliasTarget as alias }
            export { Pick as PublicPick }
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

static void CheckerReportsDuplicateLocalExportDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DuplicateLocalExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DuplicateLocalExport

            fun keep(): string = "ok"

            record Customer(Name: string)

            export { keep, keep }
            export type { Customer }
            export type { Customer }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report duplicate local export list entries.");
        var diagnostics = result.Diagnostics
            .Where(diagnostic => diagnostic.Code == "TS2004")
            .OrderBy(diagnostic => diagnostic.Span.Start.Line)
            .ThenBy(diagnostic => diagnostic.Span.Start.Column)
            .ToArray();
        AssertEqual(2, diagnostics.Length);
        AssertEqual("Duplicate export 'keep'.", diagnostics[0].Message);
        AssertEqual("Duplicate export 'Customer'.", diagnostics[1].Message);
        AssertEqual("src/Main.tysh", diagnostics[0].File);
    });
}

static void CheckerReportsUnresolvedLocalExportListDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("MissingLocalExport"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MissingLocalExport

            export { missingValue }
            export type { MissingType }
            """);

        var result = TypeSharpChecker.Check(manifestPath);

        AssertTrue(result.HasErrors, "Checker should report unresolved local export list entries.");
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2001").OrderBy(diagnostic => diagnostic.Span.Start.Line).ToArray();
        AssertEqual(2, diagnostics.Length);
        AssertEqual("Unresolved name 'missingValue'.", diagnostics[0].Message);
        AssertEqual("Unresolved name 'MissingType'.", diagnostics[1].Message);
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
        WriteFile(root, "src/Main.tysh", File.ReadAllText(Path.Combine("test", "fixtures", "parser", "positive", "0001-hello-cli", "input.tysh")));
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
        AssertContains("Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.", error.ToString());
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

static void CliCheckEmitsJsonDuplicateLocalExportDiagnostics()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, MinimalManifest("DuplicateLocalExportJson"));
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.DuplicateLocalExportJson

            fun keep(): string = "ok"

            export { keep }
            export { keep }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["check", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(1, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertContains("\"code\": \"TS2004\"", error.ToString());
        AssertContains("Duplicate export 'keep'.", error.ToString());
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

static void CliLspRunsLanguageServerProtocol()
{
    WithWorkspace(root =>
    {
        using var input = new MemoryStream();
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var protocolOutput = new MemoryStream();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["lsp"], output, error, input, protocolOutput, root);

        AssertEqual(0, exitCode);
        AssertEqual(string.Empty, output.ToString());
        AssertEqual(string.Empty, error.ToString());
        var response = Encoding.UTF8.GetString(protocolOutput.ToArray());
        AssertContains("\"textDocumentSync\":{\"openClose\":true,\"change\":1}", response);
        AssertContains("\"hoverProvider\":true", response);
        AssertContains("\"definitionProvider\":true", response);
        AssertContains("\"completionProvider\"", response);
    });
}

static void CliLspRejectsUnknownOptions()
{
    using var output = new StringWriter();
    using var error = new StringWriter();

    var exitCode = TypeSharpCli.Run(["lsp", "--stdio"], output, error);

    AssertEqual(2, exitCode);
    AssertEqual(string.Empty, output.ToString());
    AssertContains("Usage: typesharp lsp [--no-color]", error.ToString());
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
        AssertContains("\"textDocumentSync\":{\"openClose\":true,\"change\":1}", response);
        AssertContains("\"method\":\"textDocument/publishDiagnostics\"", response);
        AssertContains("\"uri\":\"" + uri + "\"", response);
        AssertContains("\"severity\":1", response);
        AssertContains("\"source\":\"typesharp\"", response);
        AssertContains("\"code\":\"TS2201\"", response);
        AssertContains("Cannot return expression of type", response);
    });
}

static void LanguageServerClearsDiagnosticsOnDidClose()
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
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"method\":\"textDocument/didClose\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + "}}}");
        WriteLspFrame(
            input,
            "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"textDocument/hover\",\"params\":{\"textDocument\":{\"uri\":"
                + JsonSerializer.Serialize(uri)
                + "},\"position\":{\"line\":2,\"character\":11}}}");
        WriteLspFrame(input, "{\"jsonrpc\":\"2.0\",\"method\":\"exit\"}");
        input.Position = 0;

        using var output = new MemoryStream();
        TypeSharpLanguageServer.Run(input, output, root);

        var response = Encoding.UTF8.GetString(output.ToArray());
        AssertContains("\"openClose\":true", response);
        AssertContains("\"code\":\"TS2201\"", response);
        AssertContains("\"diagnostics\":[]", response);
        AssertContains("\"id\":2,\"result\":null", response);
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
    AssertContains("textDocument/didClose", extensionSource);
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
    var repository = root.GetProperty("repository");

    var scripts = root.GetProperty("scripts");
    AssertEqual("git", repository.GetProperty("type").GetString());
    AssertEqual("https://github.com/naramdash/TypeSharp.git", repository.GetProperty("url").GetString());
    AssertEqual("vscode/typesharp", repository.GetProperty("directory").GetString());
    AssertEqual("node --check extension.js", scripts.GetProperty("check").GetString());
    AssertEqual("node --check test/extension-smoke.js", scripts.GetProperty("check:smoke").GetString());
    AssertEqual("node --check test/extension-live-smoke.js", scripts.GetProperty("check:live").GetString());
    AssertEqual("node --check test/extension-host-smoke.js && node --check test/run-extension-host-smoke.js", scripts.GetProperty("check:host").GetString());
    AssertEqual("node test/extension-smoke.js", scripts.GetProperty("test:smoke").GetString());
    AssertEqual("node test/extension-live-smoke.js", scripts.GetProperty("test:live").GetString());
    AssertEqual("node test/run-extension-host-smoke.js", scripts.GetProperty("test:host").GetString());
    AssertEqual("npx --yes @vscode/vsce package", scripts.GetProperty("package:vsix").GetString());
    AssertEqual(
        "dotnet publish ../../lang/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj -c Release -o server --nologo",
        scripts.GetProperty("prepare:server").GetString());
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "extension.js")), "VS Code extension entrypoint should exist.");
    AssertTrue(File.Exists(Path.Combine(extensionRoot, "MARKETPLACE.md")), "VS Code Marketplace publishing guide should exist.");
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
    AssertTrue(files.Contains("MARKETPLACE.md"), "VS Code package should include the temporary Marketplace publishing guide.");
    AssertFalse(root.TryGetProperty("dependencies", out var dependencies) && dependencies.EnumerateObject().Any(), "VS Code extension should remain dependency-free for the current package smoke.");

    var readme = File.ReadAllText(Path.Combine(extensionRoot, "README.md"));
    AssertContains("npm run package:vsix", readme);
    AssertContains("code --install-extension", readme);
    AssertContains("MARKETPLACE.md", readme);

    var marketplace = File.ReadAllText(Path.Combine(extensionRoot, "MARKETPLACE.md"));
    AssertContains("Marketplace Manage", marketplace);
    AssertContains("npx --yes @vscode/vsce login", marketplace);
    AssertContains("npx --yes @vscode/vsce publish", marketplace);

    var gitignore = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ".gitignore"));
    AssertContains("vscode/typesharp/server/", gitignore);
}

static void VsCodeSyntaxGrammarCoversStableTypeSharpTokens()
{
    var grammarPath = Path.Combine(Directory.GetCurrentDirectory(), "vscode", "typesharp", "syntaxes", "typesharp.tmLanguage.json");
    using var grammarJson = JsonDocument.Parse(File.ReadAllText(grammarPath));
    var root = grammarJson.RootElement;
    var grammarText = File.ReadAllText(grammarPath);

    AssertEqual("source.typesharp", root.GetProperty("scopeName").GetString());
    AssertTrue(
        root.GetProperty("fileTypes").EnumerateArray().Any(value => value.GetString() == "tysh"),
        "TypeSharp TextMate grammar should map .tysh source files.");
    AssertContains("keyword.operator.expression.typesharp", grammarText);
    AssertContains("keyword.operator.type.typesharp", grammarText);
    AssertContains("support.function.intrinsic.typesharp", grammarText);

    foreach (var token in new[]
    {
        "ambient",
        "async",
        "checked",
        "extension",
        "keyof",
        "literal",
        "lock",
        "match",
        "nameof",
        "open",
        "satisfies",
        "unchecked",
        "union",
        "unknown",
        "yield"
    })
    {
        AssertContains(token, grammarText);
    }
}

static void RunnableExampleCatalogSmokeMatrixIsStable()
{
    var catalogRoot = Path.Combine(Directory.GetCurrentDirectory(), "examples", "runnable");
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
        var projectReadmePath = Path.Combine(projectRoot, "README.md");
        var projectReadme = File.ReadAllText(projectReadmePath);
        AssertContains("## Code Walkthrough", projectReadme);
        AssertMarkdownCodeFencesHaveExplanations(projectReadmePath);
        AssertContains($"[{project}]({project}/README.md)", catalogReadme);
    }

    AssertContains("Each project README explains every command, output, TypeSharp, C#, or XML code block before the block appears", catalogReadme);
    AssertContains("typesharp check", catalogReadme);
    AssertContains("typesharp build", catalogReadme);
    AssertContains("typesharp run", catalogReadme);
    AssertContains("dotnet build legacy-src", catalogReadme);
    AssertContains("dotnet build host", catalogReadme);
    AssertContains("invoice-style", catalogReadme);
    AssertContains("billing account", catalogReadme);
    AssertContains("named/optional/params/out interop calls", catalogReadme);
    AssertContains("billing work-item", catalogReadme);
    AssertContains("ASP.NET", catalogReadme);
    AssertContains("WCF", catalogReadme);
    AssertContains("TS2202", catalogReadme);

    var consoleReadme = File.ReadAllText(Path.Combine(catalogRoot, "console-hello", "README.md"));
    AssertContains("This TypeSharp block defines the public data shapes", consoleReadme);
    AssertContains("This output block is the stable smoke result", consoleReadme);
    var libraryReadme = File.ReadAllText(Path.Combine(catalogRoot, "library-public-api", "README.md"));
    AssertContains("This C# block is the host-side proof", libraryReadme);
    var interopReadme = File.ReadAllText(Path.Combine(catalogRoot, "csharp-interop", "README.md"));
    AssertContains("This TypeSharp block shows the `out` parameter case", interopReadme);
    var aspNetWcfReadme = File.ReadAllText(Path.Combine(catalogRoot, "host-aspnet-wcf", "README.md"));
    AssertContains("This XML block is not executed by TypeSharp", aspNetWcfReadme);
    var workerReadme = File.ReadAllText(Path.Combine(catalogRoot, "host-worker", "README.md"));
    AssertContains("This C# block is the worker host proof", workerReadme);
    var diagnosticsReadme = File.ReadAllText(Path.Combine(catalogRoot, "diagnostics-null-safety", "README.md"));
    AssertContains("This TypeSharp block is intentionally invalid", diagnosticsReadme);
}

static void AssertMarkdownCodeFencesHaveExplanations(string markdownPath)
{
    var lines = File.ReadAllLines(markdownPath);
    var inFence = false;
    for (var i = 0; i < lines.Length; i++)
    {
        if (!lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
        {
            continue;
        }

        if (!inFence)
        {
            var previous = i - 1;
            while (previous >= 0 && string.IsNullOrWhiteSpace(lines[previous]))
            {
                previous--;
            }

            AssertTrue(previous >= 0, $"Code block in '{markdownPath}' should have an explanatory sentence immediately before it.");
            var explanation = lines[previous].Trim();
            AssertTrue(
                explanation.Contains("block", StringComparison.OrdinalIgnoreCase),
                $"Code block in '{markdownPath}' should be introduced by a sentence that explains the block. Found: '{explanation}'.");
            AssertTrue(
                explanation.EndsWith(":", StringComparison.Ordinal),
                $"Code block explanation in '{markdownPath}' should end with a colon. Found: '{explanation}'.");
        }

        inFence = !inFence;
    }

    AssertFalse(inFence, $"Markdown code fences should be balanced in '{markdownPath}'.");
}

static void RunnableExampleProjectCommandsAreSmokeTested()
{
    WithWorkspace(root =>
    {
        var sourceRoot = Path.Combine(Directory.GetCurrentDirectory(), "examples", "runnable");
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
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("public record InvoiceLine", source);
    AssertContains("public record InvoiceDraft", source);
    AssertContains("StringBuilder", source);
    AssertContains("invoiceTotal", source);

    var output = new StringWriter();
    var error = new StringWriter();
    var runExitCode = TypeSharpCli.Run(["run", manifestPath], output, error);
    if (runExitCode == 0)
    {
        AssertEqual($"Invoice Contoso Billing: MIG-48=900, SUP-12=300, total=1200{Environment.NewLine}", output.ToString());
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
    CopyRuntimeDependenciesToExample(projectRoot);
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("public record BillingAccount", source);
    AssertContains("public record InvoiceQuote", source);
    AssertContains("public record InvoiceDecision", source);
    AssertContains("public class InvoiceCalculator", source);
    var hostSource = File.ReadAllText(Path.Combine(projectRoot, "host", "LibraryConsumerSmoke.cs"));
    AssertContains("new BillingAccount", hostSource);
    AssertContains("new InvoiceQuote", hostSource);
    AssertContains("new InvoiceCalculator", hostSource);
    AssertContains("TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework", hostSource);

    var hostBuild = RunProcess(
        "dotnet",
        "build host/LibraryConsumerSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources",
        projectRoot);
    AssertTrue(
        hostBuild.ExitCode == 0,
        $"Library public API host example should compile after TypeSharp build.\nSTDOUT:\n{hostBuild.StandardOutput}\nSTDERR:\n{hostBuild.StandardError}");
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
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("LegacyCustomerRepository.Load", source);
    AssertContains("LegacyInvoiceRules.CalculateRenewalTotal", source);
    AssertContains("LegacyInvoiceRules.TryGetCreditLimit", source);
    AssertContains("LegacyBatchFormatter.Join", source);
    var legacySource = File.ReadAllText(Path.Combine(projectRoot, "legacy-src", "LegacyApi.cs"));
    AssertContains("LegacyCustomerSnapshot", legacySource);
    AssertContains("supportFee = 250", legacySource);
    AssertContains("out int creditLimit", legacySource);
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
    AssertContains("nextRunLabel()", hostSource);
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("public record WorkItem", source);
    AssertContains("public record WorkDecision", source);
    AssertContains("classify", source);

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
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("public record GreetingRequest", source);
    AssertContains("renderGreeting", source);
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
        "lang/TypeSharp.Core/TypeSharp.Core.csproj",
        "lang/TypeSharp.Core/bin/Debug/net48/TypeSharp.Core.dll");
    var runtimeAssemblyPath = BuildRepositoryAssembly(
        "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
        "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
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
    var source = File.ReadAllText(Path.Combine(projectRoot, "src", "Main.tysh"));
    AssertContains("public record CustomerProfile", source);
    AssertContains("loadProfile(): CustomerProfile?", source);
    AssertContains("renderWelcomeEmail(): CustomerProfile", source);
}

static void DocsSiteContractIsStable()
{
    var siteRoot = Path.Combine(Directory.GetCurrentDirectory(), "docs");
    using var packageJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(siteRoot, "package.json")));
    var root = packageJson.RootElement;

    AssertEqual("typesharp-docs", root.GetProperty("name").GetString());
    AssertEqual("astro build", root.GetProperty("scripts").GetProperty("build").GetString());
    AssertEqual("6.3.6", root.GetProperty("dependencies").GetProperty("astro").GetString());
    AssertEqual("0.39.2", root.GetProperty("dependencies").GetProperty("@astrojs/starlight").GetString());
    AssertEqual("2.0.1", root.GetProperty("dependencies").GetProperty("astro-mermaid").GetString());
    AssertEqual("11.15.0", root.GetProperty("dependencies").GetProperty("mermaid").GetString());
    AssertEqual("6.0.3", root.GetProperty("devDependencies").GetProperty("typescript").GetString());
    AssertTrue(File.Exists(Path.Combine(siteRoot, "package-lock.json")), "Docs site should have a committed npm lockfile.");

    AssertTrue(File.Exists(Path.Combine(siteRoot, "astro.config.ts")), "Docs site should keep the Astro configuration in TypeScript.");
    AssertFalse(File.Exists(Path.Combine(siteRoot, "astro.config.mjs")), "Docs site should not keep a JavaScript Astro configuration when TypeScript is supported.");

    var docsOwnedJavaScript = Directory
        .EnumerateFiles(siteRoot, "*.*", SearchOption.AllDirectories)
        .Where(path => IsDocsOwnedSourcePath(siteRoot, path))
        .Where(path => IsJavaScriptSourcePath(path))
        .Select(path => Path.GetRelativePath(siteRoot, path).Replace('\\', '/'))
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();
    AssertEqual(string.Empty, string.Join(", ", docsOwnedJavaScript));

    var astroConfig = File.ReadAllText(Path.Combine(siteRoot, "astro.config.ts"));
    AssertContains("starlight({", astroConfig);
    AssertContains("import mermaid from 'astro-mermaid'", astroConfig);
    AssertContains("mermaid({", astroConfig);
    AssertContains("title: 'TypeSharp'", astroConfig);
    AssertContains("../vscode/typesharp/syntaxes/typesharp.tmLanguage.json", astroConfig);
    AssertContains("expressiveCode:", astroConfig);
    AssertContains("langs: [typesharpShikiLanguage]", astroConfig);
    AssertContains("langAlias:", astroConfig);
    AssertContains("typesharp: 'typesharp'", astroConfig);
    AssertContains("tysh: 'typesharp'", astroConfig);
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
    AssertContains("slug: 'project-configuration'", astroConfig);
    AssertContains("slug: 'runtime-artifacts'", astroConfig);
    AssertContains("slug: 'dotnet-interop'", astroConfig);
    AssertContains("slug: 'cookbook'", astroConfig);
    AssertContains("slug: 'goal'", astroConfig);
    AssertContains("slug: 'modules'", astroConfig);
    AssertContains("slug: 'type-system'", astroConfig);
    AssertContains("slug: 'csharp-type-model'", astroConfig);
    AssertContains("slug: 'csharp-members-overloads'", astroConfig);
    AssertContains("slug: 'grammar'", astroConfig);
    AssertContains("slug: 'reference'", astroConfig);
    AssertContains("slug: 'api'", astroConfig);
    AssertContains("slug: 'cli'", astroConfig);
    AssertContains("slug: 'diagnostics'", astroConfig);
    AssertContains("slug: 'advanced'", astroConfig);
    AssertContains("slug: 'vscode-lsp'", astroConfig);
    AssertContains("slug: 'migration'", astroConfig);
    AssertContains("slug: 'examples'", astroConfig);
    AssertContains("slug: 'writing-guide'", astroConfig);
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
        "project-configuration",
        "runtime-artifacts",
        "dotnet-interop",
        "cookbook",
        "examples",
        "migration",
        "modules",
        "type-system",
        "csharp-type-model",
        "csharp-members-overloads",
        "grammar",
        "reference",
        "api",
        "cli",
        "diagnostics",
        "advanced",
        "vscode-lsp",
        "troubleshooting",
        "goal",
        "document-ownership",
        "project-ledger",
        "writing-guide",
        "work-ledger",
        "agentic-workflow"
    })
    {
        AssertTrue(
            File.Exists(Path.Combine(siteRoot, "src", "content", "docs", $"{page}.md")),
            $"Docs site page '{page}' should exist.");
    }

    var docsContentRoot = Path.Combine(siteRoot, "src", "content", "docs");
    var docsMarkdownPages = Directory
        .EnumerateFiles(docsContentRoot, "*.md", SearchOption.TopDirectoryOnly)
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();
    var tyshFenceCount = docsMarkdownPages.Sum(path => CountOccurrences(File.ReadAllText(path), "```tysh"));
    AssertTrue(tyshFenceCount >= 60, $"Docs TypeSharp examples should use tysh fences. Found {tyshFenceCount}.");
    AssertDocsTextFencesDoNotContainTypeSharpSource(docsContentRoot, docsMarkdownPages);

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
    AssertContains("nominal invoice records", tutorialsPage);
    AssertContains("Library Public API", tutorialsPage);
    AssertContains("consumed from a C# host smoke project", tutorialsPage);
    AssertContains("C# Interop", tutorialsPage);
    AssertContains("realistic billing APIs", tutorialsPage);
    AssertContains("Diagnostics Workflow", tutorialsPage);
    AssertContains("customer profile boundary", tutorialsPage);
    AssertContains("VS Code And LSP Workflow", tutorialsPage);
    AssertContains("Host Compatibility Overview", tutorialsPage);

    var guidesPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "guides.md"));
    AssertContains("Project Structure", guidesPage);
    AssertContains("CLI Workflow", guidesPage);
    AssertContains("C# References And Imports", guidesPage);
    AssertContains("Option, Result, Records, And Unions", guidesPage);
    AssertContains("Project Configuration", guidesPage);
    AssertContains("Modules And Imports", guidesPage);

    var projectConfigurationPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "project-configuration.md"));
    AssertContains("Minimal Manifest", projectConfigurationPage);
    AssertContains("Source Roots", projectConfigurationPage);
    AssertContains("Generated Output", projectConfigurationPage);
    AssertContains("References", projectConfigurationPage);
    AssertContains("Configuration And Target Overrides", projectConfigurationPage);

    var runtimeArtifactsPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "runtime-artifacts.md"));
    AssertContains("Artifact Boundary", runtimeArtifactsPage);
    AssertContains("Build Pipeline", runtimeArtifactsPage);
    AssertContains("Generated Project Shape", runtimeArtifactsPage);
    AssertContains("Deployment Set", runtimeArtifactsPage);
    AssertEqual(3, CountOccurrences(runtimeArtifactsPage, "```mermaid"));
    AssertContains("TypeSharp.Core.dll", runtimeArtifactsPage);
    AssertContains("TypeSharp.Runtime.dll", runtimeArtifactsPage);
    AssertContains("bin/<Configuration>/net48", runtimeArtifactsPage);
    AssertContains("Current preview builds require these assemblies to be available as local `net48` DLL references", runtimeArtifactsPage);

    var writingGuidePage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "writing-guide.md"));
    AssertContains("Vue Docs Writing Guide", writingGuidePage);
    AssertContains("Reviewed on 2026-05-21", writingGuidePage);
    AssertContains("Core Principles", writingGuidePage);
    AssertContains("tysh Example Project Guidelines", writingGuidePage);
    AssertContains("Emoji Policy", writingGuidePage);
    AssertContains("Review Checklist", writingGuidePage);
    AssertContains("explain every command, expected output, `tysh`, C#, XML, or manifest block before the block appears", writingGuidePage);
    AssertContains("Confirm runnable example project READMEs explain every code block before the block appears", writingGuidePage);
    AssertContains("```tysh", writingGuidePage);
    AssertContains("TypeSharp.toml", writingGuidePage);
    AssertContains("targetFramework = \"net48\"", writingGuidePage);
    AssertContains("TypeSharp.Core.dll", writingGuidePage);
    AssertContains("TypeSharp.Runtime.dll", writingGuidePage);
    AssertContains("typesharp check", writingGuidePage);
    AssertContains("typesharp build", writingGuidePage);

    var dotnetInteropPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "dotnet-interop.md"));
    AssertContains("Generated Target", dotnetInteropPage);
    AssertContains("Supported C# Interop Shape", dotnetInteropPage);
    AssertContains("Host Project Compatibility", dotnetInteropPage);
    AssertContains("Executables And Antivirus", dotnetInteropPage);
    AssertContains("C# And CLR Type Model", dotnetInteropPage);
    AssertContains("C# Members And Overloads", dotnetInteropPage);

    var cookbookPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "cookbook.md"));
    AssertContains("Call A Local C# DLL", cookbookPage);
    AssertContains("Expose A TypeSharp API To C#", cookbookPage);
    AssertContains("Model Nullable Input Safely", cookbookPage);
    AssertContains("Consume Generated DLLs From A Host Project", cookbookPage);

    var fundamentalsPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "fundamentals.md"));
    AssertContains("Values And Functions", fundamentalsPage);
    AssertContains("Structural Shapes Versus Nominal Public API", fundamentalsPage);
    AssertContains("Collections, Pipelines, And Async", fundamentalsPage);
    AssertContains("Type System", fundamentalsPage);

    var modulesPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "modules.md"));
    AssertContains("Files Are Modules", modulesPage);
    AssertContains("Generated Containers", modulesPage);
    AssertContains("Relative Source Imports", modulesPage);
    AssertContains("Export Surface", modulesPage);
    AssertContains("function re-exports", modulesPage);
    AssertContains("runHelper", modulesPage);
    AssertContains("publicHelper", modulesPage);

    var examplesPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "examples.md"));
    AssertContains("invoice-style", examplesPage);
    AssertContains("billing API", examplesPage);
    AssertContains("C# host smoke project", examplesPage);
    AssertContains("named/optional/params/out calls", examplesPage);
    AssertContains("billing work-item", examplesPage);
    AssertContains("nullable customer profile flow", examplesPage);
    AssertContains("introduce every command, output, TypeSharp, C#, or XML code block with a short explanation before the block", examplesPage);

    var typeSystemPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "type-system.md"));
    AssertContains("Local Inference", typeSystemPage);
    AssertContains("Null Safety", typeSystemPage);
    AssertContains("`unknown`", typeSystemPage);
    AssertContains("Nominal Public API", typeSystemPage);
    AssertContains("Type-Level And Nominal Unions", typeSystemPage);
    AssertContains("C# And CLR Type Model", typeSystemPage);

    var csharpTypeModelPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "csharp-type-model.md"));
    AssertContains("Official sources reviewed on 2026-05-21", csharpTypeModelPage);
    AssertContains("The C# type system", csharpTypeModelPage);
    AssertContains("Strong Typing And Metadata", csharpTypeModelPage);
    AssertContains("Value Types And Reference Types", csharpTypeModelPage);
    AssertContains("Built-In Type Mapping", csharpTypeModelPage);
    AssertContains("Compile-Time-Only Types", csharpTypeModelPage);
    AssertContains("Generics And Constraints", csharpTypeModelPage);
    AssertContains("Function Types And Delegates", csharpTypeModelPage);
    AssertContains("Public Type Decision Matrix", csharpTypeModelPage);
    AssertContains("TypeSharp.Core.Unit", csharpTypeModelPage);
    AssertContains("System.Nullable<T>", csharpTypeModelPage);
    AssertContains("```tysh", csharpTypeModelPage);

    var csharpMembersPage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "csharp-members-overloads.md"));
    AssertContains("Official sources reviewed on 2026-05-21", csharpMembersPage);
    AssertContains("Object-oriented techniques in C#", csharpMembersPage);
    AssertContains("Member Surface", csharpMembersPage);
    AssertContains("Methods And Overload Ranking", csharpMembersPage);
    AssertContains("Named, Optional, And Params Arguments", csharpMembersPage);
    AssertContains("Ref, Out, And In", csharpMembersPage);
    AssertContains("Delegates And Lambdas", csharpMembersPage);
    AssertContains("Extension Methods", csharpMembersPage);
    AssertContains("Exceptions And Domain Failures", csharpMembersPage);
    AssertContains("TS2406", csharpMembersPage);
    AssertContains("```tysh", csharpMembersPage);

    var referencePage = File.ReadAllText(Path.Combine(siteRoot, "src", "content", "docs", "reference.md"));
    AssertContains("Declarations", referencePage);
    AssertContains("Expressions", referencePage);
    AssertContains("Types", referencePage);
    AssertContains("Public ABI Rules", referencePage);
    AssertContains("Modules And Imports", referencePage);
    AssertContains("Type System", referencePage);
    AssertContains("C# And CLR Type Model", referencePage);
    AssertContains("C# Members And Overloads", referencePage);

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
    AssertContains("npm run check:smoke", vscodeLspPage);
    AssertContains("npm run check:live", vscodeLspPage);
    AssertContains("npm run prepare:server", vscodeLspPage);
    AssertContains("npm run package:vsix", vscodeLspPage);
    AssertContains("code --install-extension", vscodeLspPage);
    AssertContains("npm run test:smoke", vscodeLspPage);
    AssertContains("npm run test:live", vscodeLspPage);
    AssertContains("npm run test:host", vscodeLspPage);
    AssertContains("diagnostics, hover, go-to-definition, completion, and formatting", vscodeLspPage);
    AssertContains("stale diagnostic clearing", vscodeLspPage);
    AssertContains("MARKETPLACE.md", vscodeLspPage);
    AssertContains("@vscode/vsce", vscodeLspPage);

    var benchmarkRoot = Path.Combine(Directory.GetCurrentDirectory(), "docs", "research");
    var benchmarkPage = File.ReadAllText(Path.Combine(benchmarkRoot, "official-docs-deep-benchmark.md"));
    AssertContains("Official Documentation Deep Benchmark", benchmarkPage);
    AssertContains("Vue.js", benchmarkPage);
    AssertContains("Nuxt", benchmarkPage);
    AssertContains("TypeScript", benchmarkPage);
    AssertContains("C#", benchmarkPage);
    AssertContains("F#", benchmarkPage);
    AssertContains("Project Configuration", benchmarkPage);
    AssertContains("Modules And Imports", benchmarkPage);
    AssertContains("Type System", benchmarkPage);

    using var benchmarkInventory = JsonDocument.Parse(File.ReadAllText(Path.Combine(benchmarkRoot, "official-docs-deep-benchmark-inventory.json")));
    var benchmarkSources = benchmarkInventory.RootElement.GetProperty("sources");
    AssertEqual(5, benchmarkSources.GetArrayLength());
    AssertTrue(benchmarkSources[0].GetProperty("totalEntries").GetInt32() >= 90, "Vue official docs inventory should include guide/API entries.");
    AssertTrue(benchmarkSources[1].GetProperty("totalEntries").GetInt32() >= 300, "Nuxt official docs inventory should include v4 docs entries.");
    AssertTrue(benchmarkSources[2].GetProperty("totalEntries").GetInt32() >= 70, "TypeScript official docs inventory should include docs navigation entries.");
    AssertTrue(benchmarkSources[3].GetProperty("totalEntries").GetInt32() >= 250, "C# official docs inventory should include Learn TOC entries.");
    AssertTrue(benchmarkSources[4].GetProperty("totalEntries").GetInt32() >= 140, "F# official docs inventory should include Learn TOC entries.");
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
    AssertContains("uses: actions/checkout@v6", workflow);
    AssertContains("uses: actions/setup-node@v6", workflow);
    AssertContains("node-version: 24", workflow);
    AssertContains("cache-dependency-path: docs/package-lock.json", workflow);
    AssertContains("- 'docs/**'", workflow);
    AssertContains("- 'agent/**'", workflow);
    AssertContains("run: npm ci", workflow);
    AssertContains("run: npm run build", workflow);
    AssertContains("working-directory: docs", workflow);
    AssertContains("uses: actions/configure-pages@v6", workflow);
    AssertContains("uses: actions/upload-pages-artifact@v5", workflow);
    AssertContains("path: docs/dist", workflow);
    AssertContains("uses: actions/deploy-pages@v5", workflow);
    AssertContains("if: github.event_name != 'pull_request'", workflow);
    AssertFalse(workflow.Contains("docs-site", StringComparison.Ordinal), "Docs workflow should no longer reference the former docs-site path.");
}

static void ReleaseArtifactsWorkflowContractIsStable()
{
    var workflowPath = Path.Combine(Directory.GetCurrentDirectory(), ".github", "workflows", "release-artifacts.yml");
    var workflow = File.ReadAllText(workflowPath);

    AssertContains("name: Release Artifacts", workflow);
    AssertContains("tags:", workflow);
    AssertContains("- 'v*.*.*'", workflow);
    AssertContains("workflow_dispatch:", workflow);
    AssertContains("tag:", workflow);
    AssertContains("contents: write", workflow);
    AssertContains("concurrency:", workflow);
    AssertContains("$PSNativeCommandUseErrorActionPreference = $true", workflow);
    AssertContains("runs-on: windows-latest", workflow);
    AssertContains("uses: actions/checkout@v6", workflow);
    AssertContains("fetch-depth: 0", workflow);
    AssertContains("uses: actions/setup-dotnet@v5", workflow);
    AssertContains("dotnet-version: ${{ env.DOTNET_VERSION }}", workflow);
    AssertContains("DOTNET_VERSION: '10.0.x'", workflow);
    AssertContains("uses: actions/setup-node@v6", workflow);
    AssertContains("NODE_VERSION: '24'", workflow);
    AssertContains("dotnet restore test\\TypeSharp.Compiler.Tests\\TypeSharp.Compiler.Tests.csproj", workflow);
    AssertContains("dotnet build test\\TypeSharp.Compiler.Tests\\TypeSharp.Compiler.Tests.csproj -c $env:CONFIGURATION --no-restore", workflow);
    AssertContains("generated C# compiles in net48 project", workflow);
    AssertContains("net48 runtime artifacts avoid external package dependencies", workflow);
    AssertContains("runnable example project commands are smoke-tested", workflow);
    AssertContains("VS Code extension package shape is stable", workflow);
    AssertContains("dotnet publish cli\\TypeSharp.Cli\\TypeSharp.Cli.csproj", workflow);
    AssertContains("-p:UseAppHost=false", workflow);
    AssertContains("lang\\TypeSharp.Core\\bin\\$env:CONFIGURATION\\net48\\TypeSharp.Core.dll", workflow);
    AssertContains("lang\\TypeSharp.Runtime\\bin\\$env:CONFIGURATION\\net48\\TypeSharp.Runtime.dll", workflow);
    AssertContains("npm run prepare:server", workflow);
    AssertContains("npm run package:vsix", workflow);
    AssertContains("Compress-Archive", workflow);
    AssertContains("Get-FileHash -Algorithm SHA256", workflow);
    AssertContains("SHA256SUMS.txt", workflow);
    AssertContains("typesharp-cli-dotnet-$tag.zip", workflow);
    AssertContains("typesharp-runtime-net48-$tag.zip", workflow);
    AssertContains("typesharp-vscode-$tag.vsix", workflow);
    AssertContains("uses: actions/upload-artifact@v5", workflow);
    AssertContains("if-no-files-found: error", workflow);
    AssertContains("GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}", workflow);
    AssertContains("'release',", workflow);
    AssertContains("'create',", workflow);
    AssertContains("gh release upload", workflow);
    AssertFalse(workflow.Contains("python", StringComparison.OrdinalIgnoreCase), "Release workflow should not introduce Python.");

    var projectPolicyPage = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "docs", "src", "content", "docs", "project-policy.md"));
    AssertContains("Release automation", projectPolicyPage);
    AssertContains(".github/workflows/release-artifacts.yml", projectPolicyPage);
    AssertContains("typesharp-cli-dotnet-<tag>.zip", projectPolicyPage);
    AssertContains("typesharp-runtime-net48-<tag>.zip", projectPolicyPage);
    AssertContains("typesharp-vscode-<tag>.vsix", projectPolicyPage);
    AssertContains("SHA256SUMS.txt", projectPolicyPage);
    AssertContains("contents: write", projectPolicyPage);
}

static void RepositoryMonorepoLayoutIsStable()
{
    var repositoryRoot = Directory.GetCurrentDirectory();
    var topLevelDirectories = new[]
    {
        "agent",
        "cli",
        "docs",
        "examples",
        "lang",
        "test",
        "vscode"
    };

    foreach (var directory in topLevelDirectories)
    {
        AssertTrue(Directory.Exists(Path.Combine(repositoryRoot, directory)), $"Expected top-level '{directory}' directory to exist.");
        AssertTrue(File.Exists(Path.Combine(repositoryRoot, directory, "README.md")), $"Expected top-level '{directory}' directory to explain its purpose in README.md.");
    }

    AssertFalse(Directory.Exists(Path.Combine(repositoryRoot, "src")), "Repository implementation projects should be split into cli/ and lang/, not root src/.");
    AssertFalse(Directory.Exists(Path.Combine(repositoryRoot, "tests")), "Repository tests should live under test/, not root tests/.");

    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "cli", "TypeSharp.Cli", "TypeSharp.Cli.csproj")), "CLI project should live under cli/.");
    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "lang", "TypeSharp.Compiler", "TypeSharp.Compiler.csproj")), "Compiler project should live under lang/.");
    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "lang", "TypeSharp.LanguageServer", "TypeSharp.LanguageServer.csproj")), "Language server project should live under lang/.");
    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "lang", "TypeSharp.Core", "TypeSharp.Core.csproj")), "Core library project should live under lang/.");
    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "lang", "TypeSharp.Runtime", "TypeSharp.Runtime.csproj")), "Runtime library project should live under lang/.");
    AssertTrue(File.Exists(Path.Combine(repositoryRoot, "test", "TypeSharp.Compiler.Tests", "TypeSharp.Compiler.Tests.csproj")), "Test project should live under test/.");

    var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
    AssertContains("| [cli](cli) | TypeSharp command-line host", rootReadme);
    AssertContains("| [lang](lang) | compiler, language server, runtime, and core library projects |", rootReadme);
    AssertContains("| [test](test) | smoke tests, parser/type-checker/backend fixtures, runnable example verification |", rootReadme);
    AssertContains("| [docs](docs) | canonical Astro Starlight GitHub Pages documentation site |", rootReadme);
    AssertContains("| [agent](agent) | temporary agentic work surface", rootReadme);
    AssertContains("| [examples](examples) | single-file examples and runnable adoption projects |", rootReadme);
    AssertContains("| [vscode](vscode) | VS Code extension workspace", rootReadme);

    var projectLedger = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "src", "content", "docs", "project-ledger.md"));
    AssertContains("## Repository Layout", projectLedger);
    AssertContains("| `cli/` | TypeSharp command-line host", projectLedger);
    AssertContains("| `lang/` | Compiler, language server", projectLedger);
    AssertContains("| `test/` | Regression runner", projectLedger);
    AssertContains("| `vscode/` | VS Code extension workspace", projectLedger);

    var cliProject = File.ReadAllText(Path.Combine(repositoryRoot, "cli", "TypeSharp.Cli", "TypeSharp.Cli.csproj"));
    AssertContains(@"..\..\lang\TypeSharp.Compiler\TypeSharp.Compiler.csproj", cliProject);
    AssertContains(@"..\..\lang\TypeSharp.LanguageServer\TypeSharp.LanguageServer.csproj", cliProject);

    var testProject = File.ReadAllText(Path.Combine(repositoryRoot, "test", "TypeSharp.Compiler.Tests", "TypeSharp.Compiler.Tests.csproj"));
    AssertContains(@"..\..\lang\TypeSharp.Compiler\TypeSharp.Compiler.csproj", testProject);
    AssertContains(@"..\..\cli\TypeSharp.Cli\TypeSharp.Cli.csproj", testProject);
    AssertContains(@"..\..\lang\TypeSharp.LanguageServer\TypeSharp.LanguageServer.csproj", testProject);
    AssertContains(@"..\..\lang\TypeSharp.Core\Option.cs", testProject);
    AssertContains(@"..\..\lang\TypeSharp.Runtime\TypeSharpRuntimeInfo.cs", testProject);

    var gitignore = File.ReadAllText(Path.Combine(repositoryRoot, ".gitignore"));
    AssertContains("test/tmp/", gitignore);
    AssertFalse(gitignore.Contains("tests/tmp/", StringComparison.Ordinal), ".gitignore should not point at the former tests/tmp path.");

    var releaseWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "release-artifacts.yml"));
    AssertContains(@"dotnet restore test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj", releaseWorkflow);
    AssertContains(@"dotnet publish cli\TypeSharp.Cli\TypeSharp.Cli.csproj", releaseWorkflow);
    AssertContains(@"dotnet build lang\TypeSharp.Core\TypeSharp.Core.csproj", releaseWorkflow);
    AssertContains(@"dotnet build lang\TypeSharp.Runtime\TypeSharp.Runtime.csproj", releaseWorkflow);

    var extension = File.ReadAllText(Path.Combine(repositoryRoot, "vscode", "typesharp", "extension.js"));
    AssertContains(@"path.join(repositoryRoot, ""lang"", ""TypeSharp.LanguageServer""", extension);
    AssertFalse(
        extension.Contains(@"path.join(repositoryRoot, ""src"", ""TypeSharp.LanguageServer""", StringComparison.Ordinal),
        "VS Code development fallback should use lang/TypeSharp.LanguageServer.");
}

static void RunCliCommand(string[] args, int expectedExitCode)
{
    var output = new StringWriter();
    var error = new StringWriter();
    var exitCode = TypeSharpCli.Run(args, output, error);

    AssertTrue(
        exitCode == expectedExitCode,
        $"Expected CLI exit code {expectedExitCode}, got {exitCode}.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
    if (expectedExitCode == 0)
    {
        AssertEqual(string.Empty, error.ToString());
    }
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

static void CliBuildUsesModulePathContainersForMultipleSources()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "MultiSourceModules"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.MultiSourceModules

            export fun mainValue(): string = "main"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.MultiSourceModules

            export fun helperValue(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/MultiSourceModules.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedHelper = File.ReadAllText(Path.Combine(root, "generated", "src", "Feature", "Helper.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static class ModuleMain", generatedMain);
        AssertContains("public static string mainValue()", generatedMain);
        AssertContains("public static class ModuleFeature_Helper", generatedHelper);
        AssertContains("public static string helperValue()", generatedHelper);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "MultiSourceModules.dll")), "Generated multi-source project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceNamedImports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceNamed"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceNamed

            import { helper } from "./Feature/Helper"

            export fun mainValue(): string = helper()
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceNamed

            export fun helper(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceNamed.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceNamed;", generatedMain);
        AssertContains("using static Samples.RelativeSourceNamed.ModuleFeature_Helper;", generatedMain);
        AssertContains("public static class ModuleMain", generatedMain);
        AssertContains("return helper();", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceNamed.dll")), "Generated relative source named import project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceNamedImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceNamedAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceNamedAlias

            import { helper as runHelper } from "./Feature/Helper"

            export fun mainValue(): string = runHelper()
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceNamedAlias

            export fun helper(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceNamedAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceNamedAlias;", generatedMain);
        AssertContains("using static Samples.RelativeSourceNamedAlias.ModuleFeature_Helper;", generatedMain);
        AssertContains("private static string runHelper()", generatedMain);
        AssertContains("return Samples.RelativeSourceNamedAlias.ModuleFeature_Helper.helper();", generatedMain);
        AssertContains("return runHelper();", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceNamedAlias.dll")), "Generated relative source named import alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceValueImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceValueAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceValueAlias

            import { PublicName as ImportedName } from "./Feature/Helper"

            export fun mainValue(): string = ImportedName
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceValueAlias

            let InternalName: string = "Ada"

            export { InternalName as PublicName }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceValueAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceValueAlias;", generatedMain);
        AssertContains("using static Samples.RelativeSourceValueAlias.ModuleFeature_Helper;", generatedMain);
        AssertContains("private static string ImportedName", generatedMain);
        AssertContains("get { return Samples.RelativeSourceValueAlias.ModuleFeature_Helper.PublicName; }", generatedMain);
        AssertContains("return ImportedName;", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceValueAlias.dll")), "Generated relative source value import alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceTypeImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceTypeAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceTypeAlias

            import type { VisibleModel as Model } from "./Feature/Models"

            export fun pass(model: Model): Model = model
            """);
        WriteFile(root, "src/Feature/Models.tysh", """
            namespace Samples.RelativeSourceTypeAlias

            export record VisibleModel(Name: string)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceTypeAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceTypeAlias;", generatedMain);
        AssertContains("using Model = Samples.RelativeSourceTypeAlias.VisibleModel;", generatedMain);
        AssertFalse(generatedMain.Contains("using static Samples.RelativeSourceTypeAlias.ModuleFeature_Models;", StringComparison.Ordinal), "Type-only source import aliases should not add source module static usings.");
        AssertContains("public static Model pass(Model model)", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceTypeAlias.dll")), "Generated relative source type import alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceNamedTypeImportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceNamedTypeAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceNamedTypeAlias

            import { VisibleModel as Model } from "./Feature/Models"

            export fun pass(model: Model): Model = model
            """);
        WriteFile(root, "src/Feature/Models.tysh", """
            namespace Samples.RelativeSourceNamedTypeAlias

            export record VisibleModel(Name: string)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceNamedTypeAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceNamedTypeAlias;", generatedMain);
        AssertContains("using Model = Samples.RelativeSourceNamedTypeAlias.VisibleModel;", generatedMain);
        AssertContains("using static Samples.RelativeSourceNamedTypeAlias.ModuleFeature_Models;", generatedMain);
        AssertContains("public static Model pass(Model model)", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceNamedTypeAlias.dll")), "Generated relative source named type import alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceNamespaceImports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceNamespace"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceNamespace

            import * as Helper from "./Feature/Helper"

            export fun mainValue(): string = Helper.helper()
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceNamespace

            export fun helper(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceNamespace.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Helper = Samples.RelativeSourceNamespace.ModuleFeature_Helper;", generatedMain);
        AssertContains("public static class ModuleMain", generatedMain);
        AssertContains("return Helper.helper();", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceNamespace.dll")), "Generated relative source namespace import project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceReExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceReExport"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceReExport

            import { helper } from "./Barrel"

            export fun mainValue(): string = helper()
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceReExport

            export { helper } from "./Feature/Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceReExport

            export fun helper(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceReExport.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedBarrel = File.ReadAllText(Path.Combine(root, "generated", "src", "Barrel.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.RelativeSourceReExport.ModuleBarrel;", generatedMain);
        AssertContains("return helper();", generatedMain);
        AssertContains("public static class ModuleBarrel", generatedBarrel);
        AssertContains("public static string helper()", generatedBarrel);
        AssertContains("return Samples.RelativeSourceReExport.ModuleFeature_Helper.helper();", generatedBarrel);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceReExport.dll")), "Generated relative source re-export project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceReExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceReExportAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceReExportAlias

            import { publicHelper } from "./Barrel"

            export fun mainValue(): string = publicHelper()
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceReExportAlias

            export { helper as publicHelper } from "./Feature/Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceReExportAlias

            export fun helper(): string = "helper"
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceReExportAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedBarrel = File.ReadAllText(Path.Combine(root, "generated", "src", "Barrel.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.RelativeSourceReExportAlias.ModuleBarrel;", generatedMain);
        AssertContains("return publicHelper();", generatedMain);
        AssertContains("public static class ModuleBarrel", generatedBarrel);
        AssertContains("public static string publicHelper()", generatedBarrel);
        AssertContains("return Samples.RelativeSourceReExportAlias.ModuleFeature_Helper.helper();", generatedBarrel);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceReExportAlias.dll")), "Generated relative source re-export alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceModuleReExportAliases()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceModuleReExportAlias"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceModuleReExportAlias

            import { PublicTools as HelperTools } from "./Barrel"

            export fun mainValue(): string = HelperTools.keep()
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceModuleReExportAlias

            export { Tools as PublicTools } from "./Feature/Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceModuleReExportAlias

            export module Tools {
              export fun keep(): string = "helper"
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceModuleReExportAlias.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.RelativeSourceModuleReExportAlias.ModuleBarrel;", generatedMain);
        AssertContains("using HelperTools = Samples.RelativeSourceModuleReExportAlias.Tools;", generatedMain);
        AssertContains("return HelperTools.keep();", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceModuleReExportAlias.dll")), "Generated relative source module re-export alias project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceValueReExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceValueReExport"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceValueReExport

            import { PublicName } from "./Barrel"

            export fun mainValue(): string = PublicName
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceValueReExport

            export { PublicName } from "./Feature/Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceValueReExport

            let InternalName: string = "Ada"
            export { InternalName as PublicName }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceValueReExport.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedBarrel = File.ReadAllText(Path.Combine(root, "generated", "src", "Barrel.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.RelativeSourceValueReExport.ModuleBarrel;", generatedMain);
        AssertContains("return PublicName;", generatedMain);
        AssertContains("public static string PublicName", generatedBarrel);
        AssertContains("get { return Samples.RelativeSourceValueReExport.ModuleFeature_Helper.PublicName; }", generatedBarrel);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceValueReExport.dll")), "Generated relative source value re-export project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceTypeReExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceTypeReExport"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceTypeReExport.App

            import type { PublicModel as Model } from "./Barrel"

            export fun pass(model: Model): Model = model
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceTypeReExport.Barrel

            export type { VisibleModel as PublicModel } from "./Models"
            """);
        WriteFile(root, "src/Models.tysh", """
            namespace Samples.RelativeSourceTypeReExport.Models

            export record VisibleModel(Name: string)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceTypeReExport.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Samples.RelativeSourceTypeReExport.Barrel;", generatedMain);
        AssertContains("using Model = Samples.RelativeSourceTypeReExport.Models.VisibleModel;", generatedMain);
        AssertFalse(generatedMain.Contains("using static Samples.RelativeSourceTypeReExport.Barrel.ModuleBarrel;", StringComparison.Ordinal), "Type-only source re-export imports should not add source module static usings.");
        AssertContains("public static Model pass(Model model)", generatedMain);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceTypeReExport.dll")), "Generated relative source type re-export project should compile to a net48 DLL.");
    });
}

static void CliBuildLowersRelativeSourceStarReExports()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "RelativeSourceStarReExport"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.RelativeSourceStarReExport.App

            import { helper, PublicName } from "./Barrel"
            import type { VisibleModel as Model } from "./Barrel"

            export fun mainValue(model: Model): string = helper() + PublicName
            """);
        WriteFile(root, "src/Barrel.tysh", """
            namespace Samples.RelativeSourceStarReExport.Barrel

            export * from "./Feature/Helper"
            """);
        WriteFile(root, "src/Feature/Helper.tysh", """
            namespace Samples.RelativeSourceStarReExport.Feature

            export fun helper(): string = "helper"
            export let PublicName: string = "Ada"
            export record VisibleModel(Name: string)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath, "--diagnostic-format", "json"], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RelativeSourceStarReExport.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedMain = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        var generatedBarrel = File.ReadAllText(Path.Combine(root, "generated", "src", "Barrel.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using static Samples.RelativeSourceStarReExport.Barrel.ModuleBarrel;", generatedMain);
        AssertContains("using Model = Samples.RelativeSourceStarReExport.Feature.VisibleModel;", generatedMain);
        AssertContains("return helper() + PublicName;", generatedMain);
        AssertContains("public static string PublicName", generatedBarrel);
        AssertContains("get { return Samples.RelativeSourceStarReExport.Feature.ModuleFeature_Helper.PublicName; }", generatedBarrel);
        AssertContains("public static string helper()", generatedBarrel);
        AssertContains("return Samples.RelativeSourceStarReExport.Feature.ModuleFeature_Helper.helper();", generatedBarrel);
        AssertTrue(File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "RelativeSourceStarReExport.dll")), "Generated relative source star re-export project should compile to a net48 DLL.");
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

static void CliBuildCompilesImportedConstructorNamedOptionalParamsCalls()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedConstructorNamedOptionalParams"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedConstructorNamedOptionalParams"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedConstructorNamedOptionalParams

            import { LegacyFlexibleConstructor, LegacyParamsConstructor } from "Legacy.Tools"

            export fun optionalText(): string {
              let value = LegacyFlexibleConstructor("pre:")
              value.Text
            }

            export fun namedText(): string {
              let value = LegacyFlexibleConstructor(prefix: "pre:", value: "named")
              value.Text
            }

            export fun paramsText(): string {
              let value = LegacyParamsConstructor(",", "a", "b")
              value.Joined
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedConstructorNamedOptionalParams.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var value = new LegacyFlexibleConstructor(\"pre:\");", generatedSource);
        AssertContains("var value = new LegacyFlexibleConstructor(prefix: \"pre:\", value: \"named\");", generatedSource);
        AssertContains("var value = new LegacyParamsConstructor(\",\", \"a\", \"b\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedConstructorNamedOptionalParams.dll")),
            "Generated project build should compile imported constructor named, optional, and params calls.");
    });
}

static void CliBuildCompilesImportedParameterInstanceMemberCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedParameterInstanceCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedParameterInstanceCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedParameterInstanceCall

            import { LegacyFormatter } from "Legacy.Tools"

            export fun render(formatter: LegacyFormatter): string =
              formatter.Format("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedParameterInstanceCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("public static string render(LegacyFormatter formatter)", generatedSource);
        AssertContains("return formatter.Format(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedParameterInstanceCall.dll")),
            "Generated project build should compile imported parameter instance member call.");
    });
}

static void CliBuildCompilesImportedAliasInstanceMemberCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedAliasInstanceCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedAliasInstanceCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedAliasInstanceCall

            import { LegacyFormatter } from "Legacy.Tools"

            export fun render(formatter: LegacyFormatter): string {
              let alias = formatter
              alias.Format("value")
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedAliasInstanceCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var alias = formatter;", generatedSource);
        AssertContains("return alias.Format(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedAliasInstanceCall.dll")),
            "Generated project build should compile imported alias instance member call.");
    });
}

static void CliBuildCompilesImportedAssignedInstanceMemberCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedAssignedInstanceCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedAssignedInstanceCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedAssignedInstanceCall

            import { LegacyFormatter } from "Legacy.Tools"

            export fun render(primary: LegacyFormatter, secondary: LegacyFormatter): string {
              let mut alias = primary
              alias = secondary
              alias.Format("value")
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedAssignedInstanceCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var alias = primary;", generatedSource);
        AssertContains("alias = secondary;", generatedSource);
        AssertContains("return alias.Format(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedAssignedInstanceCall.dll")),
            "Generated project build should compile imported assigned instance member call.");
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

static void CliBuildCompilesImportedPropertyAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedPropertyAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedPropertyAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedPropertyAssignment

            import { LegacyFormatter } from "Legacy.Tools"

            export fun prefix(): string {
              let formatter = LegacyFormatter("legacy:")
              formatter.MutablePrefix = "updated"
              formatter.MutablePrefix
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedPropertyAssignment.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var formatter = new LegacyFormatter(\"legacy:\");", generatedSource);
        AssertContains("formatter.MutablePrefix = \"updated\";", generatedSource);
        AssertContains("return formatter.MutablePrefix;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedPropertyAssignment.dll")),
            "Generated project build should compile imported property assignment.");
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

static void CliBuildCompilesImportedFieldAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedFieldAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedFieldAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedFieldAssignment

            import { LegacyFields } from "Legacy.Tools"

            export fun code(): string {
              let fields = LegacyFields()
              fields.MutableCode = "updated"
              fields.MutableCode
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedFieldAssignment.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var fields = new LegacyFields();", generatedSource);
        AssertContains("fields.MutableCode = \"updated\";", generatedSource);
        AssertContains("return fields.MutableCode;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedFieldAssignment.dll")),
            "Generated project build should compile imported field assignment.");
    });
}

static void CliBuildCompilesImportedStaticPropertyAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedStaticPropertyAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedStaticPropertyAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedStaticPropertyAssignment

            import { LegacyFields } from "Legacy.Tools"

            export fun name(): string {
              LegacyFields.MutableStaticName = "updated"
              LegacyFields.MutableStaticName
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedStaticPropertyAssignment.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("LegacyFields.MutableStaticName = \"updated\";", generatedSource);
        AssertContains("return LegacyFields.MutableStaticName;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedStaticPropertyAssignment.dll")),
            "Generated project build should compile imported static property assignment.");
    });
}

static void CliBuildCompilesImportedStaticFieldAssignment()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedStaticFieldAssignment"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedStaticFieldAssignment"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedStaticFieldAssignment

            import { LegacyFields } from "Legacy.Tools"

            export fun code(): string {
              LegacyFields.MutableStaticCode = "updated"
              LegacyFields.MutableStaticCode
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedStaticFieldAssignment.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("LegacyFields.MutableStaticCode = \"updated\";", generatedSource);
        AssertContains("return LegacyFields.MutableStaticCode;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedStaticFieldAssignment.dll")),
            "Generated project build should compile imported static field assignment.");
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

static void CliBuildCompilesImportedIndexerNumericLiteralConversion()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedIndexerNumericLiteralConversion"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedIndexerNumericLiteralConversion"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedIndexerNumericLiteralConversion

            import { LegacyByteIndexer } from "Legacy.Tools"

            export fun pick(): string {
              let indexer = LegacyByteIndexer()
              indexer[42]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedIndexerNumericLiteralConversion.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var indexer = new LegacyByteIndexer();", generatedSource);
        AssertContains("return indexer[42];", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedIndexerNumericLiteralConversion.dll")),
            "Generated project build should compile imported indexer integral constant conversion.");
    });
}

static void CliBuildCompilesImportedOverloadedIndexerExactMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedOverloadedIndexerExactMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedOverloadedIndexerExactMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedOverloadedIndexerExactMatch

            import { LegacyOverloadedIndexer } from "Legacy.Tools"

            export fun pick(): string {
              let indexer = LegacyOverloadedIndexer()
              indexer[42]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedOverloadedIndexerExactMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var indexer = new LegacyOverloadedIndexer();", generatedSource);
        AssertContains("return indexer[42];", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedOverloadedIndexerExactMatch.dll")),
            "Generated project build should compile exact imported overloaded indexer access.");
    });
}

static void CliBuildCompilesImportedIndexerMetadataRelationshipMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedIndexerMetadataRelationshipMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedIndexerMetadataRelationshipMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedIndexerMetadataRelationshipMatch

            import { LegacyDerivedNamed, LegacyRelationshipIndexer } from "Legacy.Tools"

            export fun pick(): string {
              let indexer = LegacyRelationshipIndexer()
              indexer[LegacyDerivedNamed("Ada")]
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedIndexerMetadataRelationshipMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var indexer = new LegacyRelationshipIndexer();", generatedSource);
        AssertContains("return indexer[new LegacyDerivedNamed(\"Ada\")];", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedIndexerMetadataRelationshipMatch.dll")),
            "Generated project build should compile imported indexer metadata relationship access.");
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

static void CliBuildCompilesObjectOverloadFallbackForKnownArgumentType()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ObjectOverloadFallback"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ObjectOverloadFallback"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ObjectOverloadFallback

            import { LegacyOverloads } from "Legacy.Tools"

            export fun pick(): string =
              LegacyOverloads.Pick(true)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ObjectOverloadFallback.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyOverloads.Pick(true);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ObjectOverloadFallback.dll")),
            "Generated project build should compile a known argument type object overload fallback.");
    });
}

static void CliBuildCompilesNullLiteralReferenceOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NullLiteralReferenceOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NullLiteralReferenceOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NullLiteralReferenceOverloadMatch

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun pick(): string =
              LegacyNullOverloads.Pick(null)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/NullLiteralReferenceOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyNullOverloads.Pick(null);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NullLiteralReferenceOverloadMatch.dll")),
            "Generated project build should compile a null literal reference overload match.");
    });
}

static void CliBuildCompilesNullLiteralMetadataRelationshipOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NullLiteralMetadataRelationshipOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NullLiteralMetadataRelationshipOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NullLiteralMetadataRelationshipOverloadMatch

            import { LegacyNullOverloads } from "Legacy.Tools"

            export fun choose(): string {
              LegacyNullOverloads.DescribeNamed(null)
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Null literal metadata relationship overload match should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/NullLiteralMetadataRelationshipOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyNullOverloads.DescribeNamed(null);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NullLiteralMetadataRelationshipOverloadMatch.dll")),
            "Generated project build should compile a null literal metadata relationship overload match.");
    });
}

static void CliBuildCompilesImportedMetadataOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedMetadataOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedMetadataOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedMetadataOverloadMatch

            import { LegacyNamed, LegacyOverloads } from "Legacy.Tools"

            export fun describe(): string =
              LegacyOverloads.Describe(LegacyNamed("Ada"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(exitCode == 0, $"Imported metadata overload match should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedMetadataOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyOverloads.Describe(new LegacyNamed(\"Ada\"));", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedMetadataOverloadMatch.dll")),
            "Generated project build should compile an imported metadata overload match.");
    });
}

static void CliBuildCompilesImportedMetadataRelationshipOverloadMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedMetadataRelationshipOverloadMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedMetadataRelationshipOverloadMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedMetadataRelationshipOverloadMatch

            import { LegacyDerivedNamed, LegacyOverloads } from "Legacy.Tools"

            export fun describe(): string =
              LegacyOverloads.DescribeSpecific(LegacyDerivedNamed("Ada"))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Imported metadata relationship overload match should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedMetadataRelationshipOverloadMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyOverloads.DescribeSpecific(new LegacyDerivedNamed(\"Ada\"));", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedMetadataRelationshipOverloadMatch.dll")),
            "Generated project build should compile an imported metadata relationship overload match.");
    });
}

static void CliBuildCompilesNumericLiteralConstantConversion()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NumericLiteralConstantConversion"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NumericLiteralConstantConversion"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NumericLiteralConstantConversion

            import { LegacyNumeric } from "Legacy.Tools"

            export fun pick(): string =
              LegacyNumeric.FormatByte(42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/NumericLiteralConstantConversion.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyNumeric.FormatByte(42);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NumericLiteralConstantConversion.dll")),
            "Generated project build should compile an integral constant conversion to a smaller C# numeric parameter.");
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

static void CliBuildCompilesImportedDelegateLambdaOverloadArityMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadArityMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadArityMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadArityMatch

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.Pick("Ada", text => text)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadArityMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.Pick(\"Ada\", text => text);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadArityMatch.dll")),
            "Generated project build should compile imported delegate lambda overload arity matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadReturnMatch

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickReturn("Ada", text => 42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickReturn(\"Ada\", text => 42);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadParameterReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadParameterReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadParameterReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadParameterReturnMatch

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickIdentityReturn("Ada", text => text)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadParameterReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickIdentityReturn(\"Ada\", text => text);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadParameterReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload parameter return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadReturnRanking()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadReturnRanking"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadReturnRanking"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadReturnRanking

            import { LegacyDelegateOverloads } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickReturnWidening("Ada", text => 42)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadReturnRanking.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickReturnWidening(\"Ada\", text => 42);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadReturnRanking.dll")),
            "Generated project build should compile imported delegate lambda overload return ranking.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadMemberReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadMemberReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadMemberReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadMemberReturnMatch

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickMemberReturn(LegacyNamed("Ada"), item => item.Name)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadMemberReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickMemberReturn(new LegacyNamed(\"Ada\"), item => item.Name);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadMemberReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload member return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadChainedMemberReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadChainedMemberReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadChainedMemberReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadChainedMemberReturnMatch

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickChainedMemberReturn(LegacyNamedOwner("Ada"), item => item.Owner.Name)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadChainedMemberReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickChainedMemberReturn(new LegacyNamedOwner(\"Ada\"), item => item.Owner.Name);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadChainedMemberReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload chained member return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadMethodReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadMethodReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadMethodReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadMethodReturnMatch

            import { LegacyDelegateOverloads, LegacyNamedOwner } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickMethodReturn(LegacyNamedOwner("Ada"), item => item.Owner.Display())
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadMethodReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickMethodReturn(new LegacyNamedOwner(\"Ada\"), item => item.Owner.Display());", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadMethodReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload method return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadExtensionMethodReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadExtensionMethodReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadExtensionMethodReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadExtensionMethodReturnMatch

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickExtensionMethodReturn(LegacyNamed("Ada"), item => item.Describe())
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadExtensionMethodReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickExtensionMethodReturn(new LegacyNamed(\"Ada\"), item => item.Describe());", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadExtensionMethodReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload extension method return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadStaticMethodReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadStaticMethodReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadStaticMethodReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadStaticMethodReturnMatch

            import { LegacyDelegateOverloads, LegacyNamed, LegacyOverloads } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickStaticMethodReturn(LegacyNamed("Ada"), item => LegacyOverloads.Describe(item))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(exitCode == 0, $"Imported delegate lambda overload static method return match should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadStaticMethodReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickStaticMethodReturn(new LegacyNamed(\"Ada\"), item => LegacyOverloads.Describe(item));", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadStaticMethodReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload static method return matches.");
    });
}

static void CliBuildCompilesImportedDelegateLambdaOverloadBinaryReturnMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedDelegateLambdaOverloadBinaryReturnMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedDelegateLambdaOverloadBinaryReturnMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedDelegateLambdaOverloadBinaryReturnMatch

            import { LegacyDelegateOverloads, LegacyNamed } from "Legacy.Tools"

            export fun pick(): string = LegacyDelegateOverloads.PickBinaryReturn(LegacyNamed("Ada"), item => item.Name == "Ada")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(exitCode == 0, $"Imported delegate lambda overload binary return match should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedDelegateLambdaOverloadBinaryReturnMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyDelegateOverloads.PickBinaryReturn(new LegacyNamed(\"Ada\"), item => item.Name == \"Ada\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedDelegateLambdaOverloadBinaryReturnMatch.dll")),
            "Generated project build should compile imported delegate lambda overload binary return matches.");
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

static void CliBuildCompilesImportedExtensionMethodInstanceCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionMethodInstanceCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionMethodInstanceCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionMethodInstanceCall

            import { LegacyNamed } from "Legacy.Tools"

            export fun describe(): string {
              let named = LegacyNamed("Ada")
              named.Describe()
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported extension method instance call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedExtensionMethodInstanceCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var named = new LegacyNamed(\"Ada\");", generatedSource);
        AssertContains("return named.Describe();", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedExtensionMethodInstanceCall.dll")),
            "Generated project build should compile imported extension method instance syntax.");
    });
}

static void CliBuildCompilesImportedExtensionReceiverRelationshipMatch()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionReceiverRelationshipMatch"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionReceiverRelationshipMatch"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionReceiverRelationshipMatch

            import { LegacyDerivedNamed } from "Legacy.Tools"

            export fun describe(): string {
              let derived = LegacyDerivedNamed("Ada")
              derived.DescribeSpecific()
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported extension receiver metadata relationship match.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedExtensionReceiverRelationshipMatch.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var derived = new LegacyDerivedNamed(\"Ada\");", generatedSource);
        AssertContains("return derived.DescribeSpecific();", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedExtensionReceiverRelationshipMatch.dll")),
            "Generated project build should compile imported extension receiver metadata relationship access.");
    });
}

static void CliBuildCompilesImportedExtensionReceiverObjectFallback()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExtensionReceiverObjectFallback"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExtensionReceiverObjectFallback"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExtensionReceiverObjectFallback

            import { LegacyDerivedNamed } from "Legacy.Tools"

            export fun describe(): string {
              let derived = LegacyDerivedNamed("Ada")
              derived.DescribeObjectOnly()
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported extension receiver object fallback.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedExtensionReceiverObjectFallback.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("var derived = new LegacyDerivedNamed(\"Ada\");", generatedSource);
        AssertContains("return derived.DescribeObjectOnly();", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedExtensionReceiverObjectFallback.dll")),
            "Generated project build should compile imported extension receiver object fallback.");
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

static void CliBuildCompilesImportedExplicitGenericMethodCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedExplicitGenericMethodCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedExplicitGenericMethodCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedExplicitGenericMethodCall

            import { LegacyGenericMethods } from "Legacy.Tools"

            export fun echo(): string =
              LegacyGenericMethods.Identity<string>("value")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ImportedExplicitGenericMethodCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return LegacyGenericMethods.Identity<string>(\"value\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedExplicitGenericMethodCall.dll")),
            "Generated project build should compile imported explicit generic method call.");
    });
}

static void CliBuildCompilesImportedGenericConstraintCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedGenericConstraintCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedGenericConstraintCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedGenericConstraintCall

            import { LegacyGenericMethods, LegacyNamed, LegacyDefaultConstructible } from "Legacy.Tools"

            export fun render(): string {
              let text = LegacyGenericMethods.RequireClass<string>("value")
              let number = LegacyGenericMethods.RequireStruct<int>(42)
              let named = LegacyGenericMethods.RequireNamed<LegacyNamed>(LegacyNamed("Ada"))
              let created = LegacyGenericMethods.Create<LegacyDefaultConstructible>()
              named.Name
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported generic constraint call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedGenericConstraintCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var text = LegacyGenericMethods.RequireClass<string>(\"value\");", generatedSource);
        AssertContains("var number = LegacyGenericMethods.RequireStruct<int>(42);", generatedSource);
        AssertContains("var named = LegacyGenericMethods.RequireNamed<LegacyNamed>(new LegacyNamed(\"Ada\"));", generatedSource);
        AssertContains("var created = LegacyGenericMethods.Create<LegacyDefaultConstructible>();", generatedSource);
        AssertContains("return named.Name;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedGenericConstraintCall.dll")),
            "Generated project build should compile imported generic constraint calls.");
    });
}

static void CliBuildCompilesImportedFrameworkGenericConstraintCall()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedFrameworkGenericConstraintCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedFrameworkGenericConstraintCall"
            generatedOutputRoot = "generated"

            [references]
            assemblies = ["mscorlib"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedFrameworkGenericConstraintCall

            import { Nullable } from "System"

            export fun compare(): int =
              Nullable.Compare<int>(null, null)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported framework generic constraint call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedFrameworkGenericConstraintCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return Nullable.Compare<int>(null, null);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedFrameworkGenericConstraintCall.dll")),
            "Generated project build should compile imported framework generic constraint calls.");
    });
}

static void CliBuildCompilesImportedTransitiveGenericConstraintCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedTransitiveGenericConstraintCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedTransitiveGenericConstraintCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedTransitiveGenericConstraintCall

            import { LegacyGenericMethods, LegacyDerivedNamed } from "Legacy.Tools"

            export fun render(): string {
              let named = LegacyGenericMethods.RequireNamed<LegacyDerivedNamed>(LegacyDerivedNamed("Ada"))
              let baseNamed = LegacyGenericMethods.RequireBaseNamed<LegacyDerivedNamed>(named)
              baseNamed.Name
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported transitive generic constraint call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedTransitiveGenericConstraintCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("var named = LegacyGenericMethods.RequireNamed<LegacyDerivedNamed>(new LegacyDerivedNamed(\"Ada\"));", generatedSource);
        AssertContains("var baseNamed = LegacyGenericMethods.RequireBaseNamed<LegacyDerivedNamed>(named);", generatedSource);
        AssertContains("return baseNamed.Name;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedTransitiveGenericConstraintCall.dll")),
            "Generated project build should compile imported transitive generic constraint calls.");
    });
}

static void CliBuildCompilesImportedInferredGenericConstraintCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInferredGenericConstraintCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInferredGenericConstraintCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInferredGenericConstraintCall

            import { LegacyDerivedNamed, LegacyGenericMethods } from "Legacy.Tools"

            export fun keepClass(): string =
              LegacyGenericMethods.RequireClass("value")

            export fun keepStruct(): int =
              LegacyGenericMethods.RequireStruct(42)

            export fun keepNamed(): LegacyDerivedNamed =
              LegacyGenericMethods.RequireNamed(LegacyDerivedNamed("Ada"))

            export fun keepTracked(value: LegacyDerivedNamed): LegacyDerivedNamed =
              LegacyGenericMethods.RequireBaseNamed(value)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported inferred generic constraint call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInferredGenericConstraintCall.dll", output.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return LegacyGenericMethods.RequireClass(\"value\");", generatedSource);
        AssertContains("return LegacyGenericMethods.RequireStruct(42);", generatedSource);
        AssertContains("return LegacyGenericMethods.RequireNamed(new LegacyDerivedNamed(\"Ada\"));", generatedSource);
        AssertContains("return LegacyGenericMethods.RequireBaseNamed(value);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInferredGenericConstraintCall.dll")),
            "Generated project build should compile imported inferred generic constraint calls.");
    });
}

static void CliBuildCompilesImportedInferredConstructedGenericConstraintCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInferredConstructedGenericConstraintCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInferredConstructedGenericConstraintCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInferredConstructedGenericConstraintCall

            import { LegacyBox, LegacyGenericMethods, LegacyNamed } from "Legacy.Tools"

            export fun name(): string =
              LegacyGenericMethods.RequireBoxedNamed(LegacyBox<LegacyNamed>(LegacyNamed("Ada")))
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Build should compile imported inferred constructed generic constraint call.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInferredConstructedGenericConstraintCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return LegacyGenericMethods.RequireBoxedNamed(new LegacyBox<LegacyNamed>(new LegacyNamed(\"Ada\")));", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInferredConstructedGenericConstraintCall.dll")),
            "Generated project build should compile imported inferred constructed generic constraint calls.");
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

static void CliBuildCompilesImportedInterfaceImplementationReturn()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedInterfaceImplementationReturn"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedInterfaceImplementationReturn"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedInterfaceImplementationReturn

            import { ILegacyNamed, LegacyDerivedNamed } from "Legacy.Tools"

            export fun create(): ILegacyNamed =
              LegacyDerivedNamed("Ada")
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Imported interface implementation return should compile.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedInterfaceImplementationReturn.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("public static ILegacyNamed create()", generatedSource);
        AssertContains("return new LegacyDerivedNamed(\"Ada\");", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedInterfaceImplementationReturn.dll")),
            "Generated project build should compile imported class-to-interface implementation returns.");
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

static void CliBuildCompilesImportedGenericTypeConstructorCall()
{
    WithWorkspace(root =>
    {
        BuildLegacyReferenceDll(root, "Legacy.Tools");
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ImportedGenericTypeConstructorCall"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ImportedGenericTypeConstructorCall"
            generatedOutputRoot = "generated"

            [references]
            paths = ["lib/Legacy.Tools.dll"]
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.ImportedGenericTypeConstructorCall

            import { LegacyBox, LegacyNamed } from "Legacy.Tools"

            export fun make(): LegacyBox<LegacyNamed> {
              LegacyBox<LegacyNamed>(LegacyNamed("Ada"))
            }

            export fun name(): string {
              let box = LegacyBox<LegacyNamed>(LegacyNamed("Ada"))
              box.Value.Name
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertTrue(
            exitCode == 0,
            $"Imported generic type constructor call should build.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
        AssertContains("Generated assembly: bin/Debug/net48/ImportedGenericTypeConstructorCall.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("using Legacy.Tools;", generatedSource);
        AssertContains("return new LegacyBox<LegacyNamed>(new LegacyNamed(\"Ada\"));", generatedSource);
        AssertContains("var box = new LegacyBox<LegacyNamed>(new LegacyNamed(\"Ada\"));", generatedSource);
        AssertContains("return box.Value.Name;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "ImportedGenericTypeConstructorCall.dll")),
            "Generated project build should compile imported generic type constructor calls.");
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
            "build lang/TypeSharp.Core/TypeSharp.Core.csproj --nologo --verbosity quiet --ignore-failed-sources",
            Directory.GetCurrentDirectory());
        AssertTrue(
            coreBuild.ExitCode == 0,
            $"TypeSharp.Core should build before generated API smoke.\nSTDOUT:\n{coreBuild.StandardOutput}\nSTDERR:\n{coreBuild.StandardError}");

        var libRoot = Path.Combine(root, "lib");
        Directory.CreateDirectory(libRoot);
        File.Copy(
            Path.Combine(Directory.GetCurrentDirectory(), "lang", "TypeSharp.Core", "bin", "Debug", "net48", "TypeSharp.Core.dll"),
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
            "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
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

            export fun Older(customer: Customer): Customer = { ...customer, Age: customer.Age + 1 }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/RecordExpressionApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return new Customer(\"Ada\", 42);", generatedSource);
        AssertContains("return new Customer(customer.Name, customer.Age + 1);", generatedSource);

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
                        var older = Samples.RecordExpressions.Module.Older(customer);
                        return older.Name + ":" + older.Age.ToString();
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

static void CliBuildCompilesKeyofTypeOperator()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "KeyofApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Keyof"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Keyof

            record Customer(Name: string, Age: int)

            type CustomerKey = keyof Customer

            fun MakeKey(): CustomerKey = "Name"

            fun IsName(key: CustomerKey): bool = key == "Name"

            export fun Check(): bool = IsName(MakeKey())
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/KeyofApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("internal static string MakeKey()", generatedSource);
        AssertContains("internal static bool IsName(string key)", generatedSource);
        AssertContains("return IsName(MakeKey());", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "KeyofApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with keyof type operator lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "KeyofConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>KeyofConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="KeyofApi">
                  <HintPath>../generated/bin/Debug/net48/KeyofApi.dll</HintPath>
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
            namespace KeyofConsumer
            {
                public static class Consumer
                {
                    public static bool Read()
                    {
                        return Samples.Keyof.Module.Check();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build KeyofConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated keyof lowering.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesIndexedAccessTypeOperator()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "IndexedAccessApi"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.IndexedAccess"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.IndexedAccess

            record Customer(Name: string, Age: int)

            type CustomerName = Customer["Name"]

            type CustomerAge = Customer["Age"]

            fun MakeName(): CustomerName = "Ada"

            fun MakeAge(): CustomerAge = 42

            fun IsAda(name: CustomerName): bool = name == "Ada"

            export fun Check(): bool = IsAda(MakeName()) && MakeAge() == 42
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/IndexedAccessApi.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("internal static string MakeName()", generatedSource);
        AssertContains("internal static int MakeAge()", generatedSource);
        AssertContains("internal static bool IsAda(string name)", generatedSource);
        AssertContains("return IsAda(MakeName()) && MakeAge() == 42;", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "IndexedAccessApi.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with indexed access type lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "IndexedAccessConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>IndexedAccessConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="IndexedAccessApi">
                  <HintPath>../generated/bin/Debug/net48/IndexedAccessApi.dll</HintPath>
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
            namespace IndexedAccessConsumer
            {
                public static class Consumer
                {
                    public static bool Read()
                    {
                        return Samples.IndexedAccess.Module.Check();
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build IndexedAccessConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated indexed access lowering.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesNominalUnionApi()
{
    WithWorkspace(root =>
    {
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
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
            "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
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
            "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");
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

            export fun spreadNumbers(extra: int[]): int[] = [0, ...extra, 4]

            export fun spreadNumberList(extra: int[], more: List<int>): List<int> = [0, ...extra, ...more, 4]
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
        AssertContains("return System.Linq.Enumerable.ToArray<int>(System.Linq.Enumerable.Concat<int>(System.Linq.Enumerable.Concat<int>(new int[] { 0 }, extra), new int[] { 4 }));", generatedSource);
        AssertContains("return new List<int>(System.Linq.Enumerable.Concat<int>(System.Linq.Enumerable.Concat<int>(System.Linq.Enumerable.Concat<int>(new int[] { 0 }, extra), more), new int[] { 4 }));", generatedSource);

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
                        var spreadNumbers = Samples.Collections.Module.spreadNumbers(new int[] { 1, 2 });
                        var spreadNumberList = Samples.Collections.Module.spreadNumberList(
                            new int[] { 1, 2 },
                            new System.Collections.Generic.List<int> { 3 });
                        return names.Length == 2
                            && names[1] == "Grace"
                            && empty.Length == 0
                            && numbers[2] == 3
                            && nameList.Count == 2
                            && nameList[1] == "Grace"
                            && emptyNameList.Count == 0
                            && numberList[2] == 3
                            && spreadNumbers.Length == 4
                            && spreadNumbers[3] == 4
                            && spreadNumberList.Count == 5
                            && spreadNumberList[3] == 3;
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

static void CliBuildCompilesCompositionLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CompositionLowering"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Composition"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Composition

            export fun increment(value: int): int = value + 1

            export fun format(value: int): string = value.ToString()

            export let formatAfterIncrement: int -> string = increment >> format

            export let formatBeforeIncrement: int -> string = format << increment
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/CompositionLowering.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static readonly System.Func<int, string> formatAfterIncrement = __compose0 => format(increment(__compose0));", generatedSource);
        AssertContains("public static readonly System.Func<int, string> formatBeforeIncrement = __compose1 => format(increment(__compose1));", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "CompositionLowering.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with composition lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "CompositionConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>CompositionConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="CompositionLowering">
                  <HintPath>../generated/bin/Debug/net48/CompositionLowering.dll</HintPath>
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
            namespace CompositionConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.Composition.Module.formatAfterIncrement(1) + Samples.Composition.Module.formatBeforeIncrement(1);
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build CompositionConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated composition API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesSatisfiesExpression()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "SatisfiesExpression"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Satisfies"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Satisfies

            public record Customer(Name: string, Age: int)

            type Named = { Name: string }

            export fun keepCustomer(customer: Customer): Customer =
              customer satisfies Named

            export fun nextAge(customer: Customer): int =
              customer.Age + 1 satisfies int
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/SatisfiesExpression.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return customer;", generatedSource);
        AssertContains("return customer.Age + 1;", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "SatisfiesExpression.dll")),
            "Generated project build should compile satisfies expression lowering.");
    });
}

static void CliBuildCompilesYieldIteratorLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "YieldIterator"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Yield"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Yield

            import { IEnumerable } from "System.Collections.Generic"

            export fun names(): IEnumerable<string> {
              yield "Ada"
              yield "Grace"
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/YieldIterator.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static IEnumerable<string> names()", generatedSource);
        AssertContains("yield return \"Ada\";", generatedSource);
        AssertContains("yield return \"Grace\";", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "YieldIterator.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with yield iterator lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "YieldConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>YieldConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="YieldIterator">
                  <HintPath>../generated/bin/Debug/net48/YieldIterator.dll</HintPath>
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
            namespace YieldConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return string.Join(",", Samples.Yield.Module.names());
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build YieldConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated yield iterator API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesLockStatementLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "LockStatement"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Lock"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Lock

            export fun pulse(gate: object): string {
              lock gate {
                gate.ToString()
              }
              "done"
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/LockStatement.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("lock (gate)", generatedSource);
        AssertContains("gate.ToString();", generatedSource);
        AssertContains("return \"done\";", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "LockStatement.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with lock statement lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "LockConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>LockConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="LockStatement">
                  <HintPath>../generated/bin/Debug/net48/LockStatement.dll</HintPath>
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
            namespace LockConsumer
            {
                public static class Consumer
                {
                    public static string Read()
                    {
                        return Samples.Lock.Module.pulse(new object());
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build LockConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated lock statement API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesExtensionMethodLowering()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ExtensionMethod"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.Extensions"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.Extensions

            public extension string {
              public fun HasPrefix(text: string, prefix: string): bool =
                text.StartsWith(prefix)
            }
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/ExtensionMethod.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("public static class StringExtensions", generatedSource);
        AssertContains("public static bool HasPrefix(this string text, string prefix)", generatedSource);
        AssertContains("return text.StartsWith(prefix);", generatedSource);

        var generatedAssemblyPath = Path.Combine(root, "generated", "bin", "Debug", "net48", "ExtensionMethod.dll");
        AssertTrue(File.Exists(generatedAssemblyPath), "Build should produce generated net48 assembly with extension method lowering.");

        var consumerRoot = Path.Combine(root, "Consumer");
        Directory.CreateDirectory(consumerRoot);
        WriteFile(consumerRoot, "ExtensionConsumer.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net48</TargetFramework>
                <LangVersion>7.3</LangVersion>
                <ImplicitUsings>false</ImplicitUsings>
                <Nullable>disable</Nullable>
                <AssemblyName>ExtensionConsumer</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <Reference Include="ExtensionMethod">
                  <HintPath>../generated/bin/Debug/net48/ExtensionMethod.dll</HintPath>
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
            using Samples.Extensions;

            namespace ExtensionConsumer
            {
                public static class Consumer
                {
                    public static bool Read()
                    {
                        return "legacy".HasPrefix("leg");
                    }
                }
            }
            """);

        var build = RunProcess("dotnet", "build ExtensionConsumer.csproj --nologo --verbosity quiet --ignore-failed-sources", consumerRoot);

        AssertTrue(
            build.ExitCode == 0,
            $"C# net48 consumer project should compile against generated extension method API.\nSTDOUT:\n{build.StandardOutput}\nSTDERR:\n{build.StandardError}");
    });
}

static void CliBuildCompilesNameofIntrinsic()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "NameofIntrinsic"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.NameofIntrinsic"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.NameofIntrinsic

            public record Customer(Name: string)

            export fun propertyName(): string = nameof(Customer.Name)

            export fun parameterName(value: string): string =
              nameof(value)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/NameofIntrinsic.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return nameof(Customer.Name);", generatedSource);
        AssertContains("return nameof(value);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "NameofIntrinsic.dll")),
            "Generated project build should compile nameof intrinsic lowering.");
    });
}

static void CliBuildCompilesCheckedUncheckedExpressions()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "CheckedUncheckedExpressions"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.CheckedUncheckedExpressions"
            generatedOutputRoot = "generated"
            """);
        WriteFile(root, "src/Main.tysh", """
            namespace Samples.CheckedUncheckedExpressions

            export fun checkedAdd(value: int): int = checked(value + 1)

            export fun uncheckedAdd(value: int): int =
              unchecked(value + 1)
            """);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = TypeSharpCli.Run(["build", manifestPath], output, error);

        AssertEqual(0, exitCode);
        AssertContains("Generated assembly: bin/Debug/net48/CheckedUncheckedExpressions.dll", output.ToString());
        AssertEqual(string.Empty, error.ToString());

        var generatedSource = File.ReadAllText(Path.Combine(root, "generated", "src", "Main.g.cs")).Replace("\r\n", "\n", StringComparison.Ordinal);
        AssertContains("return checked(value + 1);", generatedSource);
        AssertContains("return unchecked(value + 1);", generatedSource);
        AssertTrue(
            File.Exists(Path.Combine(root, "generated", "bin", "Debug", "net48", "CheckedUncheckedExpressions.dll")),
            "Generated project build should compile checked/unchecked expression lowering.");
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

static void CompilerCheckKeepsParallelDiagnosticsInSourceOrder()
{
    WithWorkspace(root =>
    {
        var manifestPath = WriteManifest(root, """
            [project]
            name = "ParallelDiagnostics"
            targetFramework = "net48"
            outputType = "library"
            rootNamespace = "Samples.ParallelDiagnostics"
            generatedOutputRoot = "generated"
            """);

        WriteFile(root, "src/Zeta.tysh", """
            namespace Samples.ParallelDiagnostics

            export fun zeta(): string = 1
            """);
        WriteFile(root, "src/Alpha.tysh", """
            namespace Samples.ParallelDiagnostics

            export fun alpha(): string = 2
            """);

        var result = TypeSharpChecker.Check(manifestPath);
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Code == "TS2201").ToArray();

        AssertEqual(2, diagnostics.Length);
        AssertEqual("src/Alpha.tysh", diagnostics[0].File);
        AssertEqual("src/Zeta.tysh", diagnostics[1].File);
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
            "lang/TypeSharp.Core/TypeSharp.Core.csproj",
            "lang/TypeSharp.Core/bin/Debug/net48/TypeSharp.Core.dll");
        var runtimeAssemblyPath = BuildRepositoryAssembly(
            "lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj",
            "lang/TypeSharp.Runtime/bin/Debug/net48/TypeSharp.Runtime.dll");

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

static bool IsDocsOwnedSourcePath(string siteRoot, string path)
{
    var relativePath = Path.GetRelativePath(siteRoot, path);
    var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    if (segments.Length == 0)
    {
        return false;
    }

    var firstSegment = segments[0];
    return !string.Equals(firstSegment, ".astro", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(firstSegment, "dist", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(firstSegment, "node_modules", StringComparison.OrdinalIgnoreCase);
}

static bool IsJavaScriptSourcePath(string path)
{
    var extension = Path.GetExtension(path);
    return string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".mjs", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".cjs", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".jsx", StringComparison.OrdinalIgnoreCase)
        || string.Equals(extension, ".mjsx", StringComparison.OrdinalIgnoreCase);
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

static void AssertDocsTextFencesDoNotContainTypeSharpSource(string docsContentRoot, IReadOnlyList<string> markdownPaths)
{
    var violations = new List<string>();
    foreach (var markdownPath in markdownPaths)
    {
        var text = File.ReadAllText(markdownPath);
        foreach (var block in EnumerateMarkdownCodeBlocks(text))
        {
            if (string.Equals(block.Language, "text", StringComparison.Ordinal)
                && LooksLikeTypeSharpSourceBlock(block.Body))
            {
                var relativePath = Path.GetRelativePath(docsContentRoot, markdownPath).Replace('\\', '/');
                violations.Add($"{relativePath}:{block.StartLine}");
            }
        }
    }

    AssertEqual(string.Empty, string.Join(Environment.NewLine, violations));
}

static IEnumerable<(string Language, string Body, int StartLine)> EnumerateMarkdownCodeBlocks(string text)
{
    var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    var lines = normalized.Split('\n');
    var body = new StringBuilder();
    var inFence = false;
    var language = string.Empty;
    var startLine = 0;

    for (var index = 0; index < lines.Length; index++)
    {
        var line = lines[index];
        if (!inFence)
        {
            if (!line.StartsWith("```", StringComparison.Ordinal))
            {
                continue;
            }

            language = line.Substring(3).Trim();
            var languageSuffix = language.IndexOf(' ');
            if (languageSuffix >= 0)
            {
                language = language.Substring(0, languageSuffix);
            }

            body.Clear();
            inFence = true;
            startLine = index + 1;
            continue;
        }

        if (line.StartsWith("```", StringComparison.Ordinal))
        {
            yield return (language, body.ToString(), startLine);
            inFence = false;
            language = string.Empty;
            continue;
        }

        body.AppendLine(line);
    }
}

static bool LooksLikeTypeSharpSourceBlock(string body)
{
    var firstLine = body
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace('\r', '\n')
        .Split('\n')
        .Select(line => line.Trim())
        .FirstOrDefault(line => line.Length > 0);
    if (firstLine is null)
    {
        return false;
    }

    return StartsWithTypeSharpKeyword(firstLine, "namespace")
        || StartsWithTypeSharpKeyword(firstLine, "import")
        || firstLine.StartsWith("public union ", StringComparison.Ordinal)
        || StartsWithTypeSharpKeyword(firstLine, "export")
        || StartsWithTypeSharpKeyword(firstLine, "let")
        || StartsWithTypeSharpKeyword(firstLine, "type")
        || StartsWithTypeSharpKeyword(firstLine, "record")
        || StartsWithTypeSharpKeyword(firstLine, "fun")
        || StartsWithTypeSharpKeyword(firstLine, "literal")
        || firstLine.StartsWith("value |>", StringComparison.Ordinal)
        || firstLine.StartsWith("g(f(", StringComparison.Ordinal);
}

static bool StartsWithTypeSharpKeyword(string line, string keyword)
{
    return string.Equals(line, keyword, StringComparison.Ordinal)
        || line.StartsWith(keyword + " ", StringComparison.Ordinal)
        || line.StartsWith(keyword + "\t", StringComparison.Ordinal);
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
    var root = Path.Combine(Directory.GetCurrentDirectory(), "test", "tmp", Guid.NewGuid().ToString("N"));
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
        using System.Runtime.CompilerServices;

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

                public static string Describe(ILegacyNamed value)
                {
                    return value.Name;
                }

                public static string Describe(object value)
                {
                    return value == null ? string.Empty : value.ToString();
                }

                public static string NeedNamed(ILegacyNamed value)
                {
                    return value.Name;
                }

                public static string DescribeSpecific(ILegacyNamed value)
                {
                    return "interface:" + value.Name;
                }

                public static string DescribeSpecific(LegacyBaseNamed value)
                {
                    return "base:" + value.Name;
                }

                public static string DescribeSpecific(object value)
                {
                    return "object:" + (value == null ? string.Empty : value.ToString());
                }
            }

            public static class LegacyNullOverloads
            {
                public static string Pick(string value)
                {
                    return value == null ? "string:null" : value;
                }

                public static string Pick(object value)
                {
                    return value == null ? "object:null" : value.ToString();
                }

                public static string DescribeNamed(LegacyBaseNamed value)
                {
                    return value == null ? "base:null" : value.Name;
                }

                public static string DescribeNamed(LegacyDerivedNamed value)
                {
                    return value == null ? "derived:null" : value.Name;
                }

                public static string DescribeNamed(object value)
                {
                    return value == null ? "object:null" : value.ToString();
                }

                public static string OnlyInt(int value)
                {
                    return value.ToString();
                }

                public static string Ambiguous(string value)
                {
                    return value;
                }

                public static string Ambiguous(System.Uri value)
                {
                    return value == null ? string.Empty : value.ToString();
                }
            }

            public static class LegacyNumeric
            {
                public static string FormatInt(int value)
                {
                    return value.ToString();
                }

                public static string FormatByte(byte value)
                {
                    return value.ToString();
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

            public static class LegacyParamsAmbiguousOverloads
            {
                public static string Pick(string separator, params ILegacyNamed[] values)
                {
                    return separator;
                }

                public static string Pick(string separator, params ILegacyTagged[] values)
                {
                    return separator;
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

            public static class LegacyDelegateOverloads
            {
                public static string Pick(string value, System.Func<string, string> transform)
                {
                    return "one:" + transform(value);
                }

                public static string Pick(string value, System.Func<string, string, string> combine)
                {
                    return "two:" + combine(value, value);
                }

                public static string PickReturn(string value, System.Func<string, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickReturn(string value, System.Func<string, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickReturnWidening(string value, System.Func<string, long> transform)
                {
                    return "long:" + transform(value).ToString();
                }

                public static string PickReturnWidening(string value, System.Func<string, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickMemberReturn(LegacyNamed value, System.Func<LegacyNamed, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickMemberReturn(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickChainedMemberReturn(LegacyNamedOwner value, System.Func<LegacyNamedOwner, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickChainedMemberReturn(LegacyNamedOwner value, System.Func<LegacyNamedOwner, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickMethodReturn(LegacyNamedOwner value, System.Func<LegacyNamedOwner, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickMethodReturn(LegacyNamedOwner value, System.Func<LegacyNamedOwner, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickExtensionMethodReturn(LegacyNamed value, System.Func<LegacyNamed, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickExtensionMethodReturn(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickStaticMethodReturn(LegacyNamed value, System.Func<LegacyNamed, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickStaticMethodReturn(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string PickBinaryReturn(LegacyNamed value, System.Func<LegacyNamed, bool> transform)
                {
                    return transform(value) ? "true" : "false";
                }

                public static string PickBinaryReturn(LegacyNamed value, System.Func<LegacyNamed, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickIdentityReturn(string value, System.Func<string, string> transform)
                {
                    return "text:" + transform(value);
                }

                public static string PickIdentityReturn(string value, System.Func<string, int> transform)
                {
                    return "int:" + transform(value).ToString();
                }

                public static string RequiresBinary(string value, System.Func<string, string, string> combine)
                {
                    return combine(value, value);
                }

                public static string RequiresString(string value, System.Func<string, string> transform)
                {
                    return transform(value);
                }

                public static string RequiresIntReturn(string value, System.Func<string, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresMemberInt(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresNestedMemberInt(LegacyNamedOwner value, System.Func<LegacyNamedOwner, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresMethodInt(LegacyNamedOwner value, System.Func<LegacyNamedOwner, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresExtensionMethodInt(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresStaticMethodInt(LegacyNamed value, System.Func<LegacyNamed, int> transform)
                {
                    return transform(value).ToString();
                }

                public static string RequiresBinaryReturnString(LegacyNamed value, System.Func<LegacyNamed, string> transform)
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

            public sealed class LegacyDefaultConstructible
            {
                public LegacyDefaultConstructible()
                {
                    Name = "default";
                }

                public string Name { get; }
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

                public string MutablePrefix { get; set; }

                public string this[int index]
                {
                    get { return prefix + index.ToString(); }
                }

                public string Format(string value)
                {
                    return prefix + value;
                }
            }

            public sealed class LegacyFlexibleConstructor
            {
                public LegacyFlexibleConstructor(string prefix, string value = "default")
                {
                    Text = prefix + value;
                }

                public string Text { get; }
            }

            public sealed class LegacyParamsConstructor
            {
                public LegacyParamsConstructor(string separator, params string[] values)
                {
                    Joined = string.Join(separator, values);
                }

                public string Joined { get; }
            }

            public sealed class LegacyAmbiguousConstructor
            {
                public LegacyAmbiguousConstructor(ILegacyNamed value)
                {
                    Value = value.Name;
                }

                public LegacyAmbiguousConstructor(ILegacyTagged value)
                {
                    Value = value.Tag;
                }

                public string Value { get; }
            }

            public sealed class LegacyByteIndexer
            {
                public string this[byte index]
                {
                    get { return index.ToString(); }
                }
            }

            public sealed class LegacyOverloadedIndexer
            {
                public string this[int index]
                {
                    get { return "int:" + index.ToString(); }
                }

                public string this[object value]
                {
                    get { return "object:" + value.ToString(); }
                }
            }

            public sealed class LegacyAmbiguousIndexer
            {
                public string this[ILegacyNamed value]
                {
                    get { return value.Name; }
                }

                public string this[ILegacyTagged value]
                {
                    get { return value.Tag; }
                }
            }

            public sealed class LegacyRelationshipIndexer
            {
                public string this[ILegacyNamed value]
                {
                    get { return "interface:" + value.Name; }
                }

                public string this[LegacyBaseNamed value]
                {
                    get { return "base:" + value.Name; }
                }

                public string this[object value]
                {
                    get { return "object:" + (value == null ? string.Empty : value.ToString()); }
                }
            }

            public sealed class LegacyFields
            {
                public const string StaticCode = "static";

                public static string StaticName
                {
                    get { return "property"; }
                }

                public static string MutableStaticName { get; set; }

                public static string MutableStaticCode = "mutable-static";

                public readonly string InstanceCode = "instance";

                public string MutableCode = "mutable";
            }

            public static class LegacyExtensions
            {
                public static string Shout(this string value)
                {
                    return value.ToUpperInvariant();
                }

                public static string Describe(this ILegacyNamed value)
                {
                    return value.Name;
                }

                public static string DescribeObjectOnly(this object value)
                {
                    return "object-only";
                }

                public static string DescribeSpecific(this ILegacyNamed value)
                {
                    return "interface:" + value.Name;
                }

                public static string DescribeSpecific(this LegacyBaseNamed value)
                {
                    return "base:" + value.Name;
                }

                public static string DescribeSpecific(this object value)
                {
                    return "object";
                }
            }

            public static class LegacyGenericMethods
            {
                public static T Identity<T>(T value)
                {
                    return value;
                }

                public static T RequireClass<T>(T value) where T : class
                {
                    return value;
                }

                public static T RequireStruct<T>(T value) where T : struct
                {
                    return value;
                }

                public static T Create<T>() where T : new()
                {
                    return new T();
                }

                public static T RequireNamed<T>(T value) where T : ILegacyNamed
                {
                    return value;
                }

                public static T RequireBaseNamed<T>(T value) where T : LegacyBaseNamed
                {
                    return value;
                }

                public static string RequireBoxedNamed<T>(LegacyBox<T> box) where T : ILegacyNamed
                {
                    return box.Value.Name;
                }
            }

            public static class LegacyGenericByRefMethods
            {
                public static string Echo(string value)
                {
                    return value;
                }

                public static T Echo<T>(ref T value)
                {
                    return value;
                }
            }

            public interface ILegacyNamed
            {
                string Name { get; }
            }

            public interface ILegacyTagged
            {
                string Tag { get; }
            }

            public sealed class LegacyNamed : ILegacyNamed
            {
                public LegacyNamed(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public string Display()
                {
                    return Name;
                }
            }

            public sealed class LegacyNamedOwner
            {
                public LegacyNamedOwner(string name)
                {
                    Owner = new LegacyNamed(name);
                }

                public LegacyNamed Owner { get; }
            }

            public sealed class LegacyDualNamed : ILegacyNamed, ILegacyTagged
            {
                public LegacyDualNamed(string value)
                {
                    Name = value;
                    Tag = value;
                }

                public string Name { get; }

                public string Tag { get; }
            }

            public class LegacyBaseNamed : ILegacyNamed
            {
                public LegacyBaseNamed(string name)
                {
                    Name = name;
                }

                public string Name { get; }
            }

            public class LegacyIntermediateNamed : LegacyBaseNamed
            {
                public LegacyIntermediateNamed(string name)
                    : base(name)
                {
                }
            }

            public sealed class LegacyDerivedNamed : LegacyIntermediateNamed
            {
                public LegacyDerivedNamed(string name)
                    : base(name)
                {
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
