# TypeSharp Language 1.0 Tasks

Last audited: 2026-05-23

This file records the current language implementation level and the remaining language gaps before TypeSharp can honestly call the language surface `1.0`.

## Evidence Checked

- Canonical goal and requirements: `docs/src/content/docs/goal.md`, `docs/src/content/docs/requirements.md`
- Current language map: `docs/src/content/docs/feature-status.md`, `docs/src/content/docs/type-system.md`, `docs/src/content/docs/reference.md`, `docs/src/content/docs/grammar.md`, `docs/src/content/docs/modules.md`, `docs/src/content/docs/lowering.md`, `docs/src/content/docs/dotnet-interop.md`, `docs/src/content/docs/diagnostics.md`
- Current test evidence: `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`, `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`, `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`, `.github/workflows/regression.yml`
- Task-driving state: reset on 2026-05-23; no active task packet

## Current Implementation Level

TypeSharp is past a toy compiler and has a broad MVP language slice:

- Generated output targets package-free `net48` artifacts through C# 7.3-compatible source lowering.
- The package-free compiler test catalog is currently asserted at 574 cases, and the MSTest/MTP package shard gate expects 578 tests.
- Implemented MVP language foundations include modules, source imports/re-exports in the supported subset, local inference, null safety, records, classes, interfaces, delegates, enums, nominal unions, local type-level unions, structural shape checks, `satisfies`, pattern matching over the supported domains, async `Task`, collection expressions, pipeline/composition, `yield`, `lock`, `nameof`, `checked`/`unchecked`, TypeSharp generic calls, params/default/named arguments, extension methods, getter-only extension properties, and a large metadata-backed C# interop surface.
- The main 1.0 risk is not "missing everything"; it is that many areas are still explicitly `MVP limited`, have broad `TS2201`/unsupported-shape boundaries, or are represented as backlog in canonical docs.

Working assessment: **usable MVP / pre-1.0 language**, not yet a stable 1.0 contract.

## 1.0 Blocking Tasks

These items should be closed, intentionally rejected from 1.0, or converted into explicit non-goals before a language 1.0 claim.

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
- NuGet restore inside the compiler.
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

When a task above is closed, record the evidence next to the checkbox or in the relevant task packet:

- canonical docs updated;
- positive generated `net48` C# consumer coverage when a feature is accepted;
- negative deterministic diagnostics when a shape is rejected;
- `dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`;
- focused `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build --filter "<feature>"`;
- package-shard or docs verification when the change touches those boundaries.
