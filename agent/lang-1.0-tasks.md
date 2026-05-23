# TypeSharp Language And Adoption 1.0 Tasks

Last audited: 2026-05-23

This file records the current language implementation level, the end-to-end user adoption level, and the remaining gaps before TypeSharp can honestly call the language and toolchain surface `1.0`.

## Evidence Checked

- Canonical goal and requirements: `docs/src/content/docs/goal.md`, `docs/src/content/docs/requirements.md`
- Current language map: `docs/src/content/docs/feature-status.md`, `docs/src/content/docs/type-system.md`, `docs/src/content/docs/reference.md`, `docs/src/content/docs/grammar.md`, `docs/src/content/docs/modules.md`, `docs/src/content/docs/lowering.md`, `docs/src/content/docs/dotnet-interop.md`, `docs/src/content/docs/diagnostics.md`
- Current adoption map: `README.md`, `docs/src/content/docs/index.md`, `docs/src/content/docs/install.md`, `docs/src/content/docs/start-here.md`, `docs/src/content/docs/tutorials.md`, `docs/src/content/docs/cli.md`, `docs/src/content/docs/project-configuration.md`, `docs/src/content/docs/runtime-artifacts.md`, `docs/src/content/docs/migration.md`, `docs/src/content/docs/troubleshooting.md`
- Current test evidence: `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`, `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`, `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`, `.github/workflows/regression.yml`
- Tracker mode: manual 1.0 readiness tracker; no separate active work packet

## Current Implementation Level

TypeSharp is past a toy compiler and has a broad MVP language slice:

- Generated output targets package-free `net48` artifacts through C# 7.3-compatible source lowering.
- The package-free compiler test catalog is currently asserted at 586 cases, and the MSTest/MTP package shard gate expects 590 tests.
- Implemented MVP language foundations include modules, source imports/re-exports in the supported subset, local inference, null safety, records, classes, interfaces, delegate-compatible function values, imported C# delegates/events, enums, nominal unions, local type-level unions, structural shape checks, `satisfies`, pattern matching over the supported domains, async `Task`, collection expressions, pipeline/composition, `yield`, `lock`, `nameof`, `checked`/`unchecked`, TypeSharp generic calls, params/default/named arguments, extension methods, getter-only extension properties, and a large metadata-backed C# interop surface.
- `docs/src/content/docs/csharp-type-model.md` now contains a public declaration ABI matrix for `fun`, `record`, `class`, `interface`, `delegate`, `event`, `enum`, `union`, `type` aliases, extension methods, and getter-only extension properties.
- The CLI path supports `new`, `check`, `build`, `run`, `format`, `version`, and `explain`; the docs now define a GitHub Release download/checksum/install route and the release artifact includes a `typesharp.cmd` wrapper, but a real hosted release download has not yet been smoke-tested from the public page.
- Current dependency support is conservative: framework assemblies, local DLL paths, and direct TypeSharp project references are supported; staged release-wrapper tests verify missing local DLLs report `TS2401`, `references.packages` remains reserved and reports `TS2405`, and build stops before generated output on those dependency diagnostics.
- The 1.0 dependency acquisition scope is now explicit: framework assemblies, explicit local `net48` DLLs, direct TypeSharp project references, and matching TypeSharp Core/Runtime DLLs from the release runtime archive. NuGet package restore is post-1.0.
- Core/Runtime deployment is now proven for the 1.0 explicit installed runtime archive path: clean staged tests reference extracted `lib/net48/TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` from generated TypeSharp output and from a separate C# `net48` consumer.
- Current Core ABI documentation is aligned with implementation: `Option<T>` and `Result<T,E>` are importable types, C# consumers construct through static factories such as `Result<T,E>.Ok(...)`, and direct TypeSharp source imports named `Some`/`None`/`Ok`/`Error` are future ergonomics rather than current release guarantees.
- `typesharp version` reports CLI, compiler, language, release channel, runtime ABI/status, generated target default, CLI host target, runtime target, artifact kind, build metadata, and source revision in text and JSON form.
- The main 1.0 risk is not "missing everything"; it is that many areas are still explicitly `MVP limited`, have broad `TS2201`/unsupported-shape boundaries, or are represented as backlog in canonical docs.

Working assessment: **usable MVP / pre-1.0 language and toolchain**, not yet a stable 1.0 contract.

## Required 1.0 User Journey

A real 1.0 user must be able to complete this path without cloning the TypeSharp repository or knowing repo-internal build commands:

1. Visit the official TypeSharp web page.
2. Download or install a versioned CLI artifact from that page.
3. Verify the CLI with `typesharp version`.
4. Create a console or library project with `typesharp new`.
5. Add supported dependencies through `TypeSharp.toml`.
6. Run `typesharp check`.
7. Run `typesharp build` and get a generated `net48` DLL or EXE.
8. Run executable projects with `typesharp run` when local security policy allows.
9. Reference generated library output plus required TypeSharp Core/Runtime DLLs from a C# `.NET Framework 4.8` consumer project.
10. Diagnose missing toolchain, missing dependency, unsupported package, build, and runtime-deployment failures from docs and CLI messages.

## Official Webpage-To-Build Acceptance Scenario

This is the concrete adoption scenario that the 1.0 tracker must preserve. It starts from the public website and ends with user-owned build output, not repo-local developer commands.

### Happy Path

- A user opens the official TypeSharp web page and can find the install path in the first viewport or primary navigation.
- The page links to a versioned release artifact, checksum manifest, release notes, and matching runtime archive.
- The user downloads `typesharp-cli-dotnet-<tag>.zip`, verifies it against `SHA256SUMS.txt`, extracts it to a normal user-writable tools directory, and runs `typesharp.cmd` from a shell.
- The user downloads the matching `typesharp-runtime-net48-<tag>.zip` archive when runtime/core libraries are needed and verifies that archive against the same `SHA256SUMS.txt` manifest before referencing its DLLs.
- `typesharp version` prints enough metadata for support: CLI version, compiler version, language version/channel, runtime ABI/status, CLI host target, generated target default, artifact kind, build metadata, and source revision.
- From an empty directory, the user runs `typesharp new console` or `typesharp new library` and receives a complete starter project with `TypeSharp.toml`, source file, `.gitignore`, and local next-step guidance.
- The user can add supported dependencies in `TypeSharp.toml` using `references.assemblies`, `references.paths`, direct `[projectReferences]`, and explicit TypeSharp Core/Runtime DLL paths from the matching runtime archive.
- The user can run `typesharp check` and receive deterministic diagnostics before generated C# emission for missing local DLLs, unsupported package references, invalid metadata, or unsupported language shapes.
- The user can run `typesharp build` and receive generated C# source, an offline generated project, and a `net48` DLL or EXE without NuGet restore or repository-relative paths.
- For console projects, `typesharp run` executes the generated artifact when local security policy allows and reports an actionable failure otherwise.
- For library projects, an ordinary C# `.NET Framework 4.8` consumer can reference the generated DLL plus the matching `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` from the release runtime archive.

### Failure And Recovery Path

- Missing .NET host prerequisites explain which CLI host runtime or SDK is required without implying generated projects require that same target.
- Missing or mismatched TypeSharp Core/Runtime DLLs explain how to download and reference the matching runtime archive.
- `references.packages` reports the documented unsupported-package diagnostic and points users toward local `net48` DLL references for 1.0.
- Missing local DLLs, invalid DLL metadata, unsupported project references, generated C# build failures, and local executable policy failures are documented with the same command names and paths used by the official tutorial.
- The docs provide an uninstall or rollback path: remove the extracted CLI folder, remove any PATH entry, and reinstall a previous release artifact with its matching runtime archive after checksum verification.

### Evidence Required

- Public docs route: docs home, Install, Start Here, CLI, Project Configuration, Runtime Artifacts, Tutorials, Migration, and Troubleshooting all point to the same install/create/dependency/build flow.
- Release artifact route: staged or hosted artifact smoke runs through the release zip layout and `typesharp.cmd`, not `dotnet run` or repo-local build output.
- Clean workspace route: tests create new projects outside the repository, run `version`, `new`, `check`, `build`, and `run`, then validate generated `net48` output.
- Dependency route: tests cover framework assemblies, local `net48` DLLs, direct TypeSharp project references, matching runtime DLL references, unsupported package references, and missing/invalid dependency diagnostics.
- Consumer route: tests compile a separate C# `.NET Framework 4.8` project against a generated TypeSharp library and the matching runtime DLLs from the release layout.

## 1.0 Blocking Tasks

These items should be closed, intentionally rejected from 1.0, or converted into explicit non-goals before a language 1.0 claim.

- [ ] Publish a complete official download and installation path for the CLI.
  - The official docs entry page must have a first-viewport path to install or download TypeSharp.
  - Ship a versioned Windows-friendly CLI artifact with documented layout, checksum/signature policy, install/uninstall steps, and `typesharp version` output.
  - The checksum policy must cover both the CLI archive and the matching runtime archive.
  - The flow must not require cloning this repository, building the CLI from source, installing Python, or installing hidden global tools.
  - Document required host prerequisites separately from generated artifact requirements: the CLI may require a modern .NET SDK/runtime, while generated projects stay `net48`.
  - Add a clean-machine smoke that runs the downloaded artifact from outside the repository.
  - Acceptance evidence must start from the public page or a staged equivalent of the public page, follow the linked release artifact, verify checksums, and run `typesharp version` from the extracted layout.

- [ ] Make the first project path release-grade.
  - `typesharp new console` and `typesharp new library` should create a complete `TypeSharp.toml`, source file, `.gitignore`, and minimal local README or docs link.
  - Generated starter projects must default to `net48`, avoid preview features by default, and build without manual edits.
  - The official tutorial must show exact commands for `typesharp new`, `typesharp check`, `typesharp build`, and `typesharp run`.
  - Add release-artifact smoke coverage from an empty directory, not only from repo-local examples.
  - The first project docs must not switch between source-built `dotnet ...TypeSharp.Cli...` commands and installed `typesharp` commands without clearly labeling source-built fallback as contributor-only or preview fallback.

- [x] Define the 1.0 dependency acquisition story.
  - 1.0 supports framework assemblies, explicit local `net48` DLLs, direct TypeSharp project references, and matching TypeSharp Core/Runtime DLLs from the release runtime archive.
  - NuGet package restore is explicitly post-1.0; official docs avoid implying that `references.packages` works and show the local-DLL workaround plus deterministic `TS2405`.
  - `typesharp check` and `typesharp build` do not silently execute package restore, arbitrary package build targets, or transitive package asset acquisition.
  - Future `typesharp restore` or equivalent work remains gated on lock files, package source mapping, vulnerability audit policy, license inventory, checksum/signature policy, transitive dependency handling, offline-cache behavior, and package-target execution rules.

- [x] Ship and resolve TypeSharp Core/Runtime dependencies for generated projects.
  - A downloaded CLI user must not have to discover `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` from repo build folders.
  - The 1.0 path is a stable installed runtime archive: users extract `typesharp-runtime-net48-<tag>.zip` and reference `../typesharp-runtime/lib/net48/TypeSharp.Core.dll` plus `../typesharp-runtime/lib/net48/TypeSharp.Runtime.dll` through `references.paths`.
  - CLI runtime auto-copy and hidden template references are post-1.0 ergonomics unless promoted with separate release evidence.
  - Generated projects that use `Option<T>`, `Result<T,E>`, nominal unions, pattern helpers, and `TypeSharp.Runtime` async helpers build and deploy with the expected DLL set in the release-staged runtime archive smoke.
  - A C# `net48` consumer smoke references a generated library plus required TypeSharp runtime assemblies from the installed/downloaded layout.

- [x] Prove supported dependency references from user-created projects.
  - Framework assembly references through `references.assemblies` are covered by clean source-built and release-staged dependency smokes.
  - Local `net48` DLL references through `references.paths` are covered by positive smokes plus missing-file `TS2401` and incompatible/invalid DLL metadata diagnostics.
  - Direct TypeSharp project references through `[projectReferences]` are covered by clean multi-project source-built and release-staged workspace smokes.
  - Generated C# projects use deterministic generated-project-relative `<HintPath>` values and write an offline `NuGet.config` with package sources cleared.
  - Docs distinguish source project references, local binary references, framework references, matching runtime DLL references, and unsupported package references.

- [ ] Make the official webpage-to-build docs path coherent.
  - The docs home, Start Here, CLI, Project Configuration, Runtime Artifacts, Tutorials, Migration, and Troubleshooting pages should form one consistent route.
  - Each page should use the same install command, project command names, generated output paths, and dependency terminology.
  - The docs should clearly separate repo contributor commands from end-user commands.
  - The first tutorial must start from the downloaded CLI, not from `dotnet cli/TypeSharp.Cli/bin/.../typesharp.dll`.
  - The route must include the dependency step explicitly: framework assembly, local DLL, project reference, matching runtime DLL, and unsupported package examples use the same `TypeSharp.toml` terminology.
  - The route must include generated-output expectations: where generated source/project/assembly files appear and which files should be copied or referenced by a C# `net48` consumer.

- [ ] Add release-style end-to-end adoption tests.
  - Build or stage the CLI artifact exactly as a user would receive it.
  - From a clean temp directory, run `typesharp version`, `typesharp new console`, `typesharp check`, `typesharp build`, and `typesharp run`.
  - From a second clean temp directory, run `typesharp new library`, add a local C# `net48` DLL dependency, build, then consume the generated DLL from a C# `net48` project.
  - Add a direct TypeSharp project-reference smoke that builds referenced projects first.
  - Include negative smokes for missing CLI prerequisites, missing local DLLs, unsupported package references, and generated C# build failures.
  - Include a docs-link contract or staged public-page contract so the install command and artifact names used by docs cannot drift from the release workflow.
  - The release workflow now includes a post-publication hosted asset smoke that downloads the published CLI/runtime assets from GitHub Releases, verifies `SHA256SUMS.txt`, checks release-page provenance against `typesharp version --json`, and builds a clean console project from outside the repository; remaining evidence requires the first tagged release run to pass this smoke.

- [ ] Stabilize download, release, and versioning metadata.
  - `typesharp version` reports CLI, compiler, language, release channel, runtime ABI/status, default target, artifact kind, build metadata, and source revision useful for support.
  - Release artifacts have repeatable names, checksums, version provenance, and a documented rollback story in the release workflow and policy docs.
  - Docs state the current CLI line, language channel, runtime ABI, generated target, and matching runtime artifact.
  - Remaining evidence: the post-publication hosted GitHub Release smoke must pass on a real tagged release and verify the published `typesharp version` metadata matches the release page and checksums.

- [x] Resolve imported C# mutable-local checked/unchecked user-defined multiplicative operator policy.
  - Verified the checked static operator metadata names `op_CheckedMultiply`, `op_CheckedDivision`, and `op_CheckedModulus`.
  - 1.0 accepts only ordinary C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata for imported user-defined `*=`, `/=`, and `%=` assignment.
  - `checked(...)` and `unchecked(...)` wrappers over accepted imported operator assignments lower as generated C# 7.3-compatible checked/unchecked blocks around ordinary compound assignment.
  - Checked-only imported operator metadata reports deterministic `TS2217`; true checked user-defined operators stay post-1.0.

- [x] Produce a public ABI language matrix for all TypeSharp-authored declaration forms.
  - `docs/src/content/docs/csharp-type-model.md` covers `fun`, `record`, `class`, `interface`, `delegate`, `event`, `enum`, `union`, `type`, extension methods, and getter-only extension properties.
  - The matrix records whether each form is a public ABI slice, deferred from 1.0, source-only, compile-time-only, or not promoted.
  - Existing generated `net48` C# consumer evidence is linked for promoted forms such as functions, records, classes, interfaces, enums, nominal unions, extension methods, and getter-only extension properties.
  - TypeSharp-authored `delegate` and `event` public ABI forms are explicitly deferred from 1.0 until lowering, class-member diagnostics, and C# consumer evidence exist.

- [x] Harden TypeSharp-authored class/interface/member semantics.
  - 1.0 TypeSharp-authored classes lower to named CLR classes with optional type parameters, supported C# 7.3-compatible generic constraints, `partial`, an implicit public parameterless constructor, and public instance `fun` methods.
  - 1.0 TypeSharp-authored interfaces lower to named CLR interfaces with optional type parameters, supported generic constraints, `partial`, and method signatures.
  - C# `net48` consumer smokes cover generated classes, generated interfaces, generic classes, generic constraints, and partial declarations.
  - TypeSharp-authored class constructors, class fields, class properties, TypeSharp-authored class/interface events, explicit inheritance or interface implementation clauses, static/abstract/virtual/override members, interface default implementations, property setters, indexers, operators, class/interface member attributes beyond the emitted declaration subset, partial methods, nested type declarations, and broader member-body analysis are post-1.0.
  - Unsupported class/interface member forms now report deterministic `TS2210` diagnostics before backend emission, and build-stop coverage verifies generated C# source/project/assembly output is not produced for those forms.

- [x] Close public ABI leakage checks for compile-time-only types.
  - Exported/public function parameters and returns, exported/public values, local export-list functions, local export-list type aliases, direct type aliases, aliases to structural shapes, intersections, type-level unions, `keyof`, indexed access, inline anonymous structural shapes, and `unknown` public signatures are covered by public-boundary diagnostics before emission.
  - Public-boundary build-stop coverage verifies `TS2204` prevents generated C# source, generated project, and generated `net48` assembly emission.
  - Marker-free `dynamic` remains covered by explicit capability diagnostics (`TS2206`/`TS2207`), and unknown member/indexer access remains covered by `TS2209`.
  - Unknown nullable C# metadata remains a strict-mode interop warning (`TS2404`) so users can wrap, annotate, or isolate that boundary before exposing a nominal public API.
  - Structural and TypeScript-style flexibility stays local unless a future nominal CLR-visible adapter policy is designed and versioned.

- [x] Stabilize the module graph boundary.
  - The 1.0 source graph resolves imports only through relative source specifiers, current-project manifest aliases, or direct TypeSharp project references.
  - Non-relative specifiers without a direct project-reference match are C# namespace/type imports, not package or JavaScript resolver inputs.
  - Side-effect-only imports now report `TS0113`; non-relative forwarding, cross-project forwarding, and non-lowerable forwarding forms report `TS2003`.
  - Source imports and namespace source member access can use only exported target names; missing target exports report `TS0114` before emission.
  - Direct project references expose source-level exports only to direct dependents, build referenced `net48` assemblies before dependents, and do not grant hidden transitive project-reference visibility.
  - Editor navigation/source-span metadata across generated/source boundaries remains post-1.0 unless separately promoted.

- [x] Define the 1.0 pattern matching boundary.
  - 1.0-supported executable patterns are nominal union case names with an optional single identifier payload capture, TypeSharp-owned enum member names, named imported C# enum member names, `true`/`false`, literal members of literal-only local type-level unions, typed member patterns for non-literal local type-level unions, `_` discard arms, and `when` guard type checks.
  - Exhaustiveness diagnostics cover nominal unions, TypeSharp-owned enums, named imported C# enums, `bool`, non-literal local type-level unions, and literal-only local type-level unions; guarded arms do not prove coverage without an unguarded arm or `_`.
  - Numeric patterns over ordinary numeric inputs, numeric enum patterns, flag-style enum reasoning, flag algebra in patterns, record/structural patterns, nested payload destructuring, standalone identifier binding patterns, extractor-style patterns, and broader guard analysis are post-1.0.
  - Unsupported executable pattern forms now report `TS2211` before backend emission, and docs/contract tests preserve the boundary.

- [x] Stabilize advanced type operator limits.
  - 1.0 supports the current local-only subset: type-level union aliases, structural intersection aliases over named shapes, limited `keyof` over known records/shapes, and limited indexed access over known records/shapes and literal key unions.
  - Type alias chains deeper than 16, recursive type alias cycles, local union aliases wider than 64 distinct members, `keyof`/indexed access aliases with more than 64 selected keys, and structural intersection aliases with more than 64 resulting members now report `TS2212` before emission.
  - General mapped, conditional, template-literal, and utility type computation remains post-1.0 until the full evaluator has parser/checker coverage, budget diagnostics, public-boundary diagnostics, and generated `net48` evidence where emitted signatures can change.
  - Public ABI leakage for type-level unions, structural shapes, intersections, `keyof`, indexed access, and unresolved computed forms remains covered by `TS2204`.

- [x] Review C# overload and conversion ranking for the 1.0 interop contract.
  - The 1.0 overload and conversion ranking matrix is frozen in `docs/src/content/docs/csharp-members-overloads.md` and summarized in Feature Status.
  - Supported metadata-backed ranking covers arity, named arguments, optional parameters, `params`, byref modifiers, exact nominal matches, `null`, numeric literals, metadata relationship distance, homogeneous collection expressions, delegates/lambdas, generic methods/constructors and constraints, imported extension receivers, indexers, nullable metadata checks, and `object` fallback.
  - Unsupported or unstable conversion/ranking paths are deterministic diagnostics: `TS2402` for ambiguous stable candidates, `TS2406` for no applicable method/constructor candidates, `TS2417` for unsatisfied generic constraints, the `TS2407`-`TS2416` missing imported member family, and explicit capability diagnostics for dynamic boundaries.
  - Full C# overload conversion parity, user-defined conversion operators, TypeSharp-authored operator overload ranking, richer contextual typing, imported pipeline/composition overload ranking, general collection-builder conversions, function-typed value overload inference, and dynamic dispatch without capability boundaries remain post-1.0.
  - Existing generated `net48` and build-stop smokes cover exact overloads, object fallback, `null`, metadata relationships, collection expression overloads, numeric literal conversions, named/optional/`params`/byref calls, delegate/lambda return ranking, imported extension receiver ranking, generic constraints, constructor/indexer overloads, no-match diagnostics, and ambiguity diagnostics.

- [x] Finalize nullability and `unknown` rules across interop and public APIs.
  - The 1.0 warning-versus-error policy is documented in Type System, C# And CLR Type Model, and Feature Status.
  - TypeSharp-owned nullable-to-non-null flow reports error `TS2202`; public `unknown` and other compile-time-only public boundaries report error `TS2204`; member/indexer access on unproved `unknown` reports error `TS2209`.
  - Strict-mode imported C# reference returns without nullable metadata report warning `TS2404`; this warns users to wrap or isolate legacy metadata but does not block builds by itself.
  - Implemented imported C# nullable receiver/member/indexer/null-conditional slices lower through C# 7.3-compatible guards or report deterministic diagnostics before backend emission.
  - Broader TypeSharp-owned nullable receiver lifting, null-conditional chains, invocation, events, local-binding assignment, and fuller legacy-nullability propagation remain post-1.0 unless separately promoted.
  - Existing tests cover unknown C# nullability warnings, nullability build-stop diagnostics, public-boundary `unknown` rejection, unknown member/indexer access diagnostics, nullable receiver extension-property diagnostics, and nullable imported member/indexer/null-conditional target failures.

- [x] Reduce broad `TS2201` usage where a 1.0 user needs actionable guidance.
  - Keep stable diagnostic codes/spans, but split high-frequency language boundaries into more specific messages where the current descriptor is too general.
  - Prioritize public ABI leakage, unsupported class/member forms, unsupported operators, unsupported patterns, unsupported import/export forms, and unsupported overload conversion paths.
  - Completed splits: TypeSharp-authored class/interface member subset violations now use `TS2210`, unsupported executable match patterns use `TS2211`, type-operator budget failures use `TS2212`, unsupported extension-property policy failures use `TS2213`, unsupported enum/bitwise/shift algebra failures use `TS2214`, TypeSharp-owned function application/pipeline/composition failures use `TS2215`, unsupported assignment target policy failures use `TS2216`, unsupported imported C# operator policy failures use `TS2217`, invalid match guard predicates use `TS2218`, unsupported collection/record construction expressions use `TS2219`, structural proof failures use `TS2220`, unsupported extension-method receiver shapes use `TS2221`, primitive arithmetic compound-assignment failures use `TS2222`, unsupported null-conditional read shapes use `TS2223`, invalid yield expressions use `TS2224`, invalid lock expressions use `TS2225`, invalid nominal-union match case names use `TS2226`, invalid `satisfies` expressions use `TS2227`, invalid function return expressions use `TS2228`, invalid value initializers use `TS2229`, and invalid assignment values use `TS2230` instead of broad `TS2201`.
  - Current checker dispatch has no remaining `TS2201` emission path; keep the descriptor for registry compatibility and reintroduce new narrow descriptors if a future broad mismatch path is added.

- [x] Finalize TypeSharp-authored operator policy.
  - Imported static binary multiplicative operators are partially supported for compound assignment.
  - TypeSharp-authored operator declarations are explicitly post-1.0: records, classes, interfaces, unions, and extension declarations cannot introduce overload or conversion operators.
  - 1.0 TypeSharp operator-like expressions use only the documented built-in numeric, enum, boolean, assignment, pipeline, composition, and selected imported C# static binary metadata rules.
  - True C# 14 instance compound-assignment operators, checked user-defined operators, TypeSharp-authored operator syntax, operator overload ranking, and public CLR metadata emission remain backlog.
  - Unsupported TypeSharp-authored nominal operands continue to report deterministic operand diagnostics before backend emission.

- [x] Finalize extension member policy for 1.0.
  - Current TypeSharp-authored extension methods and getter-only extension properties are implemented in a bounded form.
  - 1.0 supports explicit-receiver extension methods that lower to ordinary C# extension methods.
  - 1.0 supports getter-only extension properties that require a declaration receiver name, exact known non-null receiver matching, explicit property type, initializer, and C# 7.3-compatible `GetName(this T receiver)` helper lowering.
  - Duplicate exact receiver/name declarations, ordinary/structural member precedence conflicts, helper-name collisions, nullable receiver declarations/accesses, assignment targets, and null-conditional read/simple assignment targets report deterministic `TS2213` diagnostics before backend emission.
  - Setters, static extension members, extension operators, nullable receiver lifting, imported C# extension property metadata, richer conversion/ranking, and C# 14 `extension(...)` block emission are post-1.0.

- [x] Validate functional language scope.
  - Pipeline and unary composition have bounded checking and lowering.
  - 1.0 functional ergonomics are immutable values, expression-oriented direct `fun` declarations, explicit function types lowering to CLR delegates, nominal unions, `Option<T>`/`Result<T,E>` Core types, pattern matching inside the documented 1.0 match boundary, direct first-argument pipeline calls, unary named-function composition, lambdas in supported delegate contexts, iterator `yield`, and `Task`-based async.
  - Higher-order function values beyond the documented delegate-compatible subset, automatic currying, general partial application, imported pipeline/composition target inference, pipeline overload ranking, computation-expression-style workflows, active-pattern-style extractors, units of measure, type providers, and F#-complete semantics are post-1.0.
  - Public/exported function-shaped values and direct composition values require explicit function type annotations where stable CLR metadata is needed; unsupported shapes report deterministic `TS2215` diagnostics before generated C# emission.

- [x] Validate enum and bitwise algebra scope.
  - 1.0 enum support includes TypeSharp-owned CLR enum declarations with optional integral underlying types, explicit integer values, aliases to previously declared same-enum members, initializer-local composite-or expressions over previous same-enum members and integer literals, declaration/member attributes as metadata, same-enum value `|`/`&`/`^`/`~`, and match exhaustiveness for TypeSharp-owned and named imported C# enum members.
  - 1.0 bitwise algebra includes primitive integral `|`/`&`/`^`/`~`/`<<`/`>>`/`>>>`, boolean `|`/`&`/`^`, local enum/integral/bool `|=`/`&=`/`^=`, primitive integral `<<=`/`>>=`, and bounded `>>>=` for mutable locals plus supported imported field/property/indexer targets.
  - Imported enum metadata records underlying type names and literal numeric member values, but 1.0 enum reasoning stays enum/member-name based.
  - Enum-valued shifts and shift assignments, flag algebra beyond same-enum value operators, flag-aware match/pattern reasoning, imported numeric enum flag reasoning, arbitrary/general computed enum values outside initializer-local composite-or, numeric pattern algebra, numeric enum patterns, and broad attribute target validation are post-1.0.
  - Unsupported enum and bitwise algebra shapes report deterministic `TS2214` diagnostics before backend emission.

- [x] Close collection and object construction boundaries.
  - 1.0 collection expressions require a known array or `System.Collections.Generic.List<T>` target.
  - Accepted collection shapes include empty literals, homogeneous element literals, known array/List spreads, imported C# overload arguments selected as array/List targets, imported `params` array contexts, and lambda return bodies whose delegate return type is a known array or `List<T>`.
  - 1.0 object construction includes expected nominal TypeSharp record expressions and nominal record spread, lowering to constructor calls in record parameter order.
  - Imported C# constructor calls, including selected generic constructors and named/optional/params constructor arguments, remain supported through the metadata-backed constructor path with deterministic overload diagnostics before emission.
  - Dictionary literals, set literals, collection-expression constructor or factory arguments beyond the documented array/List and imported overload slices, object initializer syntax, arbitrary class object construction, inferred anonymous object construction, contextual collection inference without a known array/List target, general collection-builder protocols, and record/class/object initializer mutation are post-1.0.

- [x] Confirm runtime helper ABI for language features.
  - The current release line uses Runtime ABI `0` as the explicit pre-1.0 preview ABI rule, not a 1.0 stability claim.
  - `.NET Interop` names the ABI-covered Core/Runtime surface: `Option<T>`, `Result<T,E>`, `Unit`, `TypeSharpRuntimeInfo`, union helpers, pattern helpers, equality/hash helpers, and async helpers.
  - `runtime ABI constants are aligned` verifies compiler/runtime ABI version and runtime target framework alignment.
  - Core and runtime helper tests cover the named helper behavior, and the release-staged runtime artifact smoke verifies generated assemblies and a C# `net48` consumer reference the same installed Core/Runtime layout.
  - Breaking changes to Core/Runtime public signatures, generated metadata shape, helper requirements, or deployment reference shape are tracked through the runtime ABI policy and release notes.

## Post-1.0 Or Explicit Non-Goals Unless Reclassified

These are valuable but should not silently block language 1.0 unless the project deliberately promotes them:

- Direct IL backend.
- Structural public ABI adapters.
- Tagged struct union representation.
- General mapped, conditional, and template-literal type computation.
- Hidden NuGet restore inside `check` or `build`.
- TypeSharp-managed NuGet package restore in 1.0.
- Qualified `net481` profile.
- ASP.NET/WCF/Windows Service template generation and packaging automation.
- VS Code Marketplace and `dotnet new` template publication.
- F# option/tuple/record interop layer through `FSharp.Core`.
- Type providers.
- Units of measure.
- Effect annotation system.
- TypeSharp-authored `public delegate` and `public event` public ABI lowering.
- Decorator-like metaprogramming.
- Macro system.
- Full dependent types or theorem proving.
- JavaScript runtime compatibility.
- Unbounded `any` as a default escape hatch.

## Verification Needed For This Tracker

When an item above is closed, record the evidence next to the checkbox:

- canonical docs updated;
- official web/download path updated when user onboarding changes;
- positive generated `net48` C# consumer coverage when a feature is accepted;
- negative deterministic diagnostics when a shape is rejected;
- clean-directory CLI artifact smoke for install/new/check/build/run changes;
- dependency smoke for framework assembly, local DLL, direct TypeSharp project reference, and unsupported package paths when dependency behavior changes;
- `dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`;
- focused `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build --filter "<feature>"`;
- package-shard, release-artifact, or docs verification when the change touches those boundaries.
