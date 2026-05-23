# TypeSharp Language And Adoption 1.0 Tasks

Last audited: 2026-05-23

This file records the current language implementation level, the end-to-end user adoption level, and the remaining gaps before TypeSharp can honestly call the language and toolchain surface `1.0`.

## Evidence Checked

- Canonical goal and requirements: `docs/src/content/docs/goal.md`, `docs/src/content/docs/requirements.md`
- Current language map: `docs/src/content/docs/feature-status.md`, `docs/src/content/docs/type-system.md`, `docs/src/content/docs/reference.md`, `docs/src/content/docs/grammar.md`, `docs/src/content/docs/modules.md`, `docs/src/content/docs/lowering.md`, `docs/src/content/docs/dotnet-interop.md`, `docs/src/content/docs/diagnostics.md`
- Current adoption map: `README.md`, `docs/src/content/docs/index.md`, `docs/src/content/docs/start-here.md`, `docs/src/content/docs/tutorials.md`, `docs/src/content/docs/cli.md`, `docs/src/content/docs/project-configuration.md`, `docs/src/content/docs/runtime-artifacts.md`, `docs/src/content/docs/migration.md`, `docs/src/content/docs/troubleshooting.md`
- Current test evidence: `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`, `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`, `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`, `.github/workflows/regression.yml`
- Tracker mode: manual 1.0 readiness tracker; no separate active work packet

## Current Implementation Level

TypeSharp is past a toy compiler and has a broad MVP language slice:

- Generated output targets package-free `net48` artifacts through C# 7.3-compatible source lowering.
- The package-free compiler test catalog is currently asserted at 574 cases, and the MSTest/MTP package shard gate expects 578 tests.
- Implemented MVP language foundations include modules, source imports/re-exports in the supported subset, local inference, null safety, records, classes, interfaces, delegates, enums, nominal unions, local type-level unions, structural shape checks, `satisfies`, pattern matching over the supported domains, async `Task`, collection expressions, pipeline/composition, `yield`, `lock`, `nameof`, `checked`/`unchecked`, TypeSharp generic calls, params/default/named arguments, extension methods, getter-only extension properties, and a large metadata-backed C# interop surface.
- The repo-local CLI path supports `new`, `check`, `build`, `run`, `format`, `version`, and `explain`, but a release-grade user journey from official web page to downloaded CLI to first build is not yet proven.
- Current dependency support is conservative: framework assemblies, local DLL paths, and direct TypeSharp project references are supported; `references.packages` remains reserved and reports `TS2405`.
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

## 1.0 Blocking Tasks

These items should be closed, intentionally rejected from 1.0, or converted into explicit non-goals before a language 1.0 claim.

- [ ] Publish a complete official download and installation path for the CLI.
  - The official docs entry page must have a first-viewport path to install or download TypeSharp.
  - Ship a versioned Windows-friendly CLI artifact with documented layout, checksum/signature policy, install/uninstall steps, and `typesharp version` output.
  - The flow must not require cloning this repository, building the CLI from source, installing Python, or installing hidden global tools.
  - Document required host prerequisites separately from generated artifact requirements: the CLI may require a modern .NET SDK/runtime, while generated projects stay `net48`.
  - Add a clean-machine smoke that runs the downloaded artifact from outside the repository.

- [ ] Make the first project path release-grade.
  - `typesharp new console` and `typesharp new library` should create a complete `TypeSharp.toml`, source file, `.gitignore`, and minimal local README or docs link.
  - Generated starter projects must default to `net48`, avoid preview features by default, and build without manual edits.
  - The official tutorial must show exact commands for `typesharp new`, `typesharp check`, `typesharp build`, and `typesharp run`.
  - Add release-artifact smoke coverage from an empty directory, not only from repo-local examples.

- [ ] Define the 1.0 dependency acquisition story.
  - Decide whether 1.0 supports only framework assemblies, local DLLs, direct TypeSharp project references, and bundled TypeSharp Core/Runtime DLLs, or whether an explicit package restore path belongs in 1.0.
  - If NuGet package restore is not in 1.0, the official docs must avoid implying that `references.packages` works and must show the local-DLL workaround plus the deterministic `TS2405` diagnostic.
  - If package restore is promoted to 1.0, implement an explicit `typesharp restore` or equivalent flow with lock files, package source mapping, vulnerability audit policy, license inventory, checksum/signature policy, transitive dependency handling, and offline-cache behavior.
  - `typesharp check` and `typesharp build` must never silently execute package restore or arbitrary package build targets unless the security model explicitly allows it.

- [ ] Ship and resolve TypeSharp Core/Runtime dependencies for generated projects.
  - A downloaded CLI user must not have to discover `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` from repo build folders.
  - Decide whether the CLI copies Core/Runtime into generated output, templates add explicit local references, or docs provide a stable installed runtime path.
  - Verify generated projects that use `Option<T>`, `Result<T,E>`, nominal unions, pattern helpers, and async helpers build and deploy with the expected DLL set.
  - Add a C# `net48` consumer smoke that references a generated library plus required TypeSharp runtime assemblies from the installed/downloaded layout.

- [ ] Prove supported dependency references from user-created projects.
  - Cover framework assembly references through `references.assemblies`.
  - Cover local `net48` DLL references through `references.paths`, including missing file and incompatible assembly diagnostics.
  - Cover direct TypeSharp project references through `[projectReferences]` from a user-created multi-project workspace.
  - Confirm generated C# projects use deterministic hint paths and offline `NuGet.config` behavior.
  - Document the difference between source project references, local binary references, framework references, and unsupported package references.

- [ ] Make the official webpage-to-build docs path coherent.
  - The docs home, Start Here, CLI, Project Configuration, Runtime Artifacts, Tutorials, Migration, and Troubleshooting pages should form one consistent route.
  - Each page should use the same install command, project command names, generated output paths, and dependency terminology.
  - The docs should clearly separate repo contributor commands from end-user commands.
  - The first tutorial must start from the downloaded CLI, not from `dotnet cli/TypeSharp.Cli/bin/.../typesharp.dll`.

- [ ] Add release-style end-to-end adoption tests.
  - Build or stage the CLI artifact exactly as a user would receive it.
  - From a clean temp directory, run `typesharp version`, `typesharp new console`, `typesharp check`, `typesharp build`, and `typesharp run`.
  - From a second clean temp directory, run `typesharp new library`, add a local C# `net48` DLL dependency, build, then consume the generated DLL from a C# `net48` project.
  - Add a direct TypeSharp project-reference smoke that builds referenced projects first.
  - Include negative smokes for missing CLI prerequisites, missing local DLLs, unsupported package references, and generated C# build failures.

- [ ] Stabilize download, release, and versioning metadata.
  - `typesharp version` should report CLI, compiler, language, runtime ABI, default target, and artifact build metadata useful for support.
  - Release artifacts need repeatable names, checksums, version provenance, and a rollback story.
  - Docs should state which CLI versions support which language/runtime ABI and which generated Core/Runtime assemblies must be deployed.

- [ ] Resolve imported C# mutable-local checked/unchecked user-defined multiplicative operator policy.
  - Verify actual metadata names for checked static operators such as `op_CheckedMultiply`, `op_CheckedDivision`, and `op_CheckedModulus`.
  - Decide whether generated C# 7.3 can consume the accepted shape or whether TypeSharp must report deterministic diagnostics.
  - Reconcile docs after the decision, because checked/unchecked assignment wording currently overlaps with imported static-operator wording.
  - Add positive generated `net48` consumer tests only if the accepted shape stays package-free and C# 7.3-compatible; otherwise add negative diagnostics.

- [ ] Produce a public ABI language matrix for all TypeSharp-authored declaration forms.
  - Cover `fun`, `record`, `class`, `interface`, `delegate`, `event`, `enum`, `union`, `type`, extension methods, and extension properties.
  - For each form, record whether it is stable for public `net48` ABI, local-only, compile-time-only, or rejected.
  - Add C# consumer smoke or ABI snapshot coverage for any form promoted to 1.0.

- [ ] Harden TypeSharp-authored class/interface/member semantics.
  - The parser/backend handle class and interface declarations, but canonical docs still mention broader class-member body analysis as follow-up work.
  - Decide the 1.0 member body subset: fields, properties, methods, constructors, events, visibility, partial declarations, attributes, generics, inheritance, and interface implementation.
  - Add deterministic diagnostics for unsupported member forms before backend emission.

- [ ] Close public ABI leakage checks for compile-time-only types.
  - Re-audit every exported/public position for structural shapes, intersection aliases, type-level unions, anonymous object shapes, `unknown`, nullable unknown C# metadata, and marker-free `dynamic`.
  - Include project-reference exports, re-exports, aliases, extension members, delegate values, and inferred public values.
  - Keep structural and TypeScript-style flexibility local unless a nominal CLR-visible adapter policy exists.

- [ ] Stabilize the module graph boundary.
  - Tighten unsupported non-relative imports, non-lowerable re-exports, and side-effect-only imports.
  - Decide what direct project references expose for source-level exports versus generated assembly metadata.
  - Add navigation/source-span metadata requirements if 1.0 promises editor navigation across generated/source boundaries.

- [ ] Define the 1.0 pattern matching boundary.
  - Current coverage is practical for nominal unions, TypeSharp-owned enums, named imported C# enums, `bool`, local type-level unions, local literal unions, discard arms, and guarded-arm non-coverage.
  - Decide whether numeric patterns, flag-style enum reasoning, richer nested record/shape patterns, active-pattern-style extractors, or broader guard analysis are explicitly post-1.0.
  - Add negative diagnostics for every pattern form left out of 1.0.

- [ ] Stabilize advanced type operator limits.
  - `keyof`, indexed access, and structural intersection aliases are limited; general mapped/conditional/template-literal type computation remains backlog.
  - Turn the existing evaluator-budget design into enforceable tests for recursion depth, normalized union width, expansion/cardinality, cycle detection, and public ABI rejection.
  - Document which limited type operators are 1.0 and which remain future.

- [ ] Review C# overload and conversion ranking for the 1.0 interop contract.
  - Current metadata overload validation is broad but intentionally not full C# overload resolution.
  - Freeze the supported ranking set for literals, `null`, numeric conversions, collections, delegates/lambdas, extension receivers, generic constraints, indexers, named/optional/params/byref, and nullable metadata.
  - Add diagnostics for unsupported conversion/ranking paths instead of relying on generated C# compile failures.

- [ ] Finalize nullability and `unknown` rules across interop and public APIs.
  - Confirm strict-mode diagnostics for missing C# nullable metadata in calls, member access, assignment, generics, delegates, lambdas, and public TypeSharp signatures.
  - Confirm all nullable receiver/member/indexer/null-conditional paths either narrow safely or fail deterministically.
  - Document any warning-versus-error policy before 1.0.

- [ ] Reduce broad `TS2201` usage where a 1.0 user needs actionable guidance.
  - Keep stable diagnostic codes/spans, but split high-frequency language boundaries into more specific messages where the current descriptor is too general.
  - Prioritize public ABI leakage, unsupported class/member forms, unsupported operators, unsupported patterns, unsupported import/export forms, and unsupported overload conversion paths.

- [ ] Finalize TypeSharp-authored operator policy.
  - Imported static binary multiplicative operators are partially supported for compound assignment.
  - TypeSharp-authored operators, true C# 14 instance compound-assignment operators, broader overload ranking, and checked user-defined expansion beyond the selected local slice remain backlog.
  - Decide what, if anything, belongs in 1.0; otherwise record an explicit post-1.0 boundary and diagnostics.

- [ ] Finalize extension member policy for 1.0.
  - Current TypeSharp-authored extension methods and getter-only extension properties are implemented in a bounded form.
  - Decide whether setters, static extension members, operators, nullable receiver lifting, richer conversion/ranking, and imported C# extension property metadata are 1.0 blockers or post-1.0 backlog.
  - Add negative tests for unsupported extension member forms.

- [ ] Validate functional language scope.
  - Pipeline and unary composition have bounded checking and lowering.
  - Higher-order function values, currying, partial application, imported pipeline/composition targets, pipeline overload ranking, computation-expression-style workflows, and active-pattern-style extractors remain future work unless explicitly promoted.
  - Record the 1.0 boundary so users do not infer F#-complete semantics from the surface syntax.

- [ ] Validate enum and bitwise algebra scope.
  - Current enum support includes basic declarations, underlying types, numeric values, aliases, local composite initializers, same-enum value bitwise operators, and match exhaustiveness.
  - Decide whether enum-valued shifts, flag algebra beyond same-enum `|`/`&`/`^`/`~`, imported numeric enum flag reasoning, arbitrary computed enum values, and numeric pattern algebra are 1.0 or post-1.0.

- [ ] Close collection and object construction boundaries.
  - Current collection expressions target arrays and `List<T>` with spread support in known contexts.
  - Decide whether dictionaries, sets, constructor/factory collection arguments, object initializers, broader record/class construction, and contextual collection inference belong in 1.0.

- [ ] Confirm runtime helper ABI for language features.
  - `Option<T>`, `Result<T,E>`, `Unit`, nominal union helpers, async helpers, equality helpers, and pattern helpers need a 1.0 ABI commitment or a clear pre-1.0 versioning rule.
  - Ensure generated assemblies and C# consumers reference the same runtime ABI and that breaking changes are tracked.

## Post-1.0 Or Explicit Non-Goals Unless Reclassified

These are valuable but should not silently block language 1.0 unless the project deliberately promotes them:

- Direct IL backend.
- Structural public ABI adapters.
- Tagged struct union representation.
- General mapped, conditional, and template-literal type computation.
- Hidden NuGet restore inside `check` or `build`.
- Qualified `net481` profile.
- ASP.NET/WCF/Windows Service template generation and packaging automation.
- VS Code Marketplace and `dotnet new` template publication.
- F# option/tuple/record interop layer through `FSharp.Core`.
- Type providers.
- Units of measure.
- Effect annotation system.
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
