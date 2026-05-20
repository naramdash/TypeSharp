# Task Rollup: 0001-0253 Project Work Ledger

Status: Done
Queue: Q0-Q5
Start Time: 2026-05-20 02:17:44 +09:00
End Time: 2026-05-20 21:50:21 +09:00

## Objective

Compress the historical `docs/tasks` packet set into one project work ledger so the folder stays readable while preserving completed scope, verification themes, and remaining follow-up areas.

## Compression Rule

This rollup replaces the individual task packet files for completed work 0001 through 0253. Future long-running work may create a temporary task packet while it is active, but completed packets should be folded back into this ledger or a successor rollup instead of accumulating indefinitely.

## Current State

| Area | State |
| --- | --- |
| Active task packet | None |
| Next planned task packet | None selected |
| Completed work covered | 0001-0253 |
| Generated artifact target | `net48` generated assemblies and runtime/core libraries |
| Host/tool target | Modern .NET host for compiler, CLI, LSP, and tests |
| Web docs target | Astro Starlight docs-site with GitHub Pages-compatible static output |

## Foundation Parser And Semantic Skeleton

Covered work:
- 0001-0022 foundation bootstrap, parser implementation, diagnostics, binder, and type-checker skeleton.
- 0097 stable grammar parser fixture audit.
- 0103-0105 binder, type checker, and diagnostics system audits.
- 0126 same-scope duplicate symbol diagnostic.

Summary:
- Established fixture policy, grammar ambiguity/precedence notes, compiler/CLI/test project skeletons, manifest loading, source discovery, lexer/parser coverage, diagnostics taxonomy, binder skeleton, and type checker skeleton.
- Added parser and semantic fixtures for basic declarations, expressions, name resolution, and diagnostics.
- Audited binder/type-checker/diagnostics evidence and added `TS2002` duplicate-symbol reporting.

Representative verification:
- Compiler test harness runs for parser snapshots, binder diagnostics, type checker diagnostics, diagnostic descriptor registry, and CLI diagnostic smoke paths.

## Runtime Build Backend And Language Lowering

Covered work:
- 0023-0037 runtime/CLI/interop/backend skeleton and generated `net48` build pipeline.
- 0070-0077 runtime helper surface, runtime ABI policy, basic C# backend semantics, and module namespace lowering.
- 0079-0093 public API declarations, immutable records, unions, null safety, structural checking, async `Task` lowering, and lowering examples.
- 0098-0099 public ABI snapshot and performance smoke benchmark.
- 0106-0116 C# source backend audit, release/compiler readiness, backend seams, lowering pipeline, ABI checker, overload resolver, inference engine, and future IL seam.
- 0123, 0138-0141, 0147, 0149 generic constraints, collection expressions, pipeline, indexer, record expression construction, list-target collection expression, and partial declaration lowering.
- 0247 collection spread elements parsed, type checked, and lowered to C# 7.3-compatible `System.Linq.Enumerable.Concat`/`ToArray` sequences for array and explicit `List<T>` targets.
- 0248 nominal record spread fields parsed, type checked, and lowered to C# 7.3-compatible constructor calls for expected record expressions.
- 0249 limited TypeScript-style `keyof` over known record/shape types parsed, checked as a string literal union, and lowered to C# string implementation details when kept out of public API.
- 0250 limited TypeScript-style indexed access types such as `Customer["Name"]` parsed, checked over known record/shape members, and lowered to the selected C# member type when kept out of public API.
- 0170 explicit imported C# generic method invocation lowering.

Summary:
- Connected generated TypeSharp source to C# 7.3-compatible `net48` project build output.
- Added runtime/core helper policies for union, pattern, equality/hash, and async helper surfaces.
- Implemented or smoke-tested modules, functions, literals, public declarations, generic APIs, classes/interfaces, records, unions, structural shape checks including limited `keyof` and limited indexed access types, null safety diagnostics, async `Task`, collection expression array/List lowering with spread elements, pipeline/indexer/record expressions including nominal record spread, partial declarations, and explicit or inferred generic C# method calls.
- Added public ABI snapshot and performance guardrails.

Representative verification:
- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`
- focused generated C# golden, CLI build, ABI snapshot, lowering, runtime helper, and performance smokes.

## CSharp Interop And Metadata Diagnostics

Covered work:
- 0038-0056 C# member/byref/metadata-backed interop validation, overload validation, unknown nullability, delegate/event/host smokes, and `net48` dependency audit.
- 0078 byref/params interop status.
- 0101-0102 reference resolver and metadata reader audits.
- 0121 imported attribute/generic type smoke.
- 0142-0144 imported field, generic method, and interface smokes.
- 0148 unsupported package reference diagnostic.
- 0171-0185 explicit generic arity and constraint validation, metadata-backed `TS2406`/`TS2407`/`TS2408`/`TS2409`/`TS2410`/`TS2411`/`TS2412`/`TS2413`/`TS2414`/`TS2415`/`TS2416`/`TS2417` diagnostics, and imported C# parameter/local alias/assignment receiver tracking.
- 0189 framework assembly public metadata indexing and missing framework type validation.
- 0190 framework metadata-backed missing method and static member diagnostics.
- 0191 transitive metadata base/interface graph validation for imported C# generic type constraints.
- 0192 local/framework extension method metadata marker indexing.
- 0193 framework generic method constraint validation and no-emission diagnostics.
- 0194 imported C# extension method instance-call validation and `net48` build smoke.
- 0195 imported C# class-to-interface/base metadata assignment validation.
- 0196 inferred imported C# generic method constraint validation.
- 0197 imported C# indexer parameter type metadata and mismatch diagnostics.
- 0198 known-argument C# overload applicability filtering and `TS2406` no-emission diagnostics.
- 0199 known numeric literal C# overload conversion filtering and constant-conversion build smoke.
- 0200 imported C# extension method instance-call argument validation and `TS2406` no-emission diagnostics.
- 0201 imported C# indexer numeric literal conversion validation and constant-conversion build smoke.
- 0202 imported C# overloaded indexer exact ranking and ambiguous indexer no-emission diagnostics.
- 0203 imported metadata argument C# overload filtering/ranking and `TS2406` no-emission diagnostics.
- 0204 `null` literal C# overload applicability/ranking and `TS2406` no-emission diagnostics.
- 0205 imported metadata base/interface relationship distance overload ranking.
- 0206 imported C# overloaded indexer metadata relationship distance ranking.
- 0207 imported C# extension receiver metadata relationship distance ranking.
- 0208 imported C# extension receiver `object` fallback validation.
- 0209 `null` literal C# overload metadata relationship specificity ranking.
- 0210 imported C# generic type constructor call lowering and metadata receiver tracking.
- 0211 imported C# constructor parameter metadata and no-matching constructor diagnostics.
- 0212 imported C# constructor optional/named/params validation smokes and ambiguous constructor diagnostics.
- 0213 inferred imported C# generic method constraints from constructed generic arguments.
- 0214 imported C# delegate lambda overload arity filtering and no-emission diagnostics.
- 0215 imported C# delegate lambda return filtering for known literal body types.
- 0216 imported C# delegate lambda parameter-return filtering for identity lambda bodies.
- 0217 imported C# delegate lambda return ranking for exact body return matches over numeric conversions.
- 0218 imported C# delegate lambda member-return filtering for metadata-backed lambda parameter member access.
- 0219 imported C# delegate lambda chained member-return filtering for metadata-backed lambda parameter member access chains.
- 0220 imported C# delegate lambda method-call return filtering for metadata-backed lambda parameter receiver/member chains.
- 0221 imported C# delegate lambda extension-method-call return filtering for metadata-backed lambda parameter receivers.
- 0222 imported C# delegate lambda static method-call return filtering for metadata-backed lambda parameter arguments.
- 0223 imported C# delegate lambda binary predicate return filtering for comparison/logical lambda bodies.

Summary:
- Added framework/local DLL reference resolution, selected public metadata indexing for local `net48` C# assemblies, and public type/member/property/field metadata indexing for available `net48` framework reference assemblies.
- Smoke-tested imported properties, fields, indexers, constructors including generic type constructors and optional/named/params constructor forms, static/instance members, delegates, events, attributes, generic types, generic methods, interfaces, byref/params/optional/named overloads, and generated TypeSharp assembly consumption from C#.
- Added unsupported package reference `TS2405`, ambiguous overload or constructor `TS2402`, no matching overload or constructor `TS2406`, missing C# method `TS2407`, missing C# type `TS2408`, missing static member `TS2409`, metadata-constructed local instance member `TS2410`, missing instance indexer `TS2411`, missing instance property setter `TS2412`, readonly instance field assignment `TS2413`, missing static property setter `TS2414`, readonly static field assignment `TS2415`, missing instance event `TS2416`, and unsatisfied explicit or inferred C# generic constraint `TS2417` no-emission diagnostics.
- Indexed public property getter/setter metadata and smoke-tested imported property assignment on settable C# properties.
- Indexed readonly field metadata and smoke-tested imported field assignment on mutable C# instance/static fields.
- Extended metadata-backed lambda member return inference from a single parameter member to chained public instance property/field paths such as `item => item.Owner.Name`, preserving `TS2406` no-emission diagnostics for incompatible delegate return targets.
- Extended metadata-backed lambda return inference to public instance method-call bodies reached from a lambda parameter or member chain, such as `item => item.Owner.Display()`, preserving `TS2406` no-emission diagnostics when the selected method return type cannot satisfy the delegate return target.
- Extended metadata-backed lambda return inference to imported extension method-call bodies reached from a lambda parameter receiver, such as `item => item.Describe()`, using source import/open extension namespaces and preserving `TS2406` no-emission diagnostics when the extension return type cannot satisfy the delegate return target.
- Extended metadata-backed lambda return inference to imported static method-call bodies that receive lambda parameters or their metadata-backed member chains, such as `item => LegacyOverloads.Describe(item)`, preserving `TS2406` no-emission diagnostics when the static method return type cannot satisfy the delegate return target.
- Extended lambda body return inference to comparison and logical binary expressions, such as `item => item.Name == "Ada"`, so imported delegate predicate overloads can select `Func<LegacyNamed, bool>` or report `TS2406` before emission when only an incompatible delegate return target remains.
- Smoke-tested static property assignment validation on get-only and settable C# static properties.
- Indexed public event metadata and smoke-tested missing imported instance event add/remove diagnostics before generated C# emission.
- Indexed public indexer parameter type metadata and smoke-tested missing, mismatched, numeric-literal, relationship-ranked, and ambiguous imported C# indexer argument diagnostics before generated C# emission. Numeric literal indexer validation rejects impossible conversions before emission and permits fitting integral constants for smaller numeric indexer parameters; overloaded indexer validation now ranks exact known argument matches and metadata relationship distance ahead of `object` fallback and reports ambiguous equally-ranked indexer candidates before emission.
- Imported C# overload resolution now filters and ranks candidates when TypeSharp knows a literal primitive, `null` literal, or imported metadata argument is incompatible with a metadata parameter, while preserving `object`, generic-parameter, inherited/interface, and conservative numeric applicability; incompatible known arguments report `TS2406` before generated C# emission and compatible `object` fallback, reference/nullable `null` targets, or metadata-compatible overloads still build. Imported metadata arguments now score base/interface relationship distance, so a closer base-class overload can outrank a farther interface or `object` fallback candidate.
- `null` literal overload validation rejects non-nullable value-type parameters before generated C# emission, ranks concrete reference/nullable parameters ahead of `object` fallback, ranks nearer metadata reference targets ahead of farther base/interface or `object` fallback candidates when relationships are indexed, and preserves ambiguity for unrelated equally ranked reference candidates.
- Known numeric literal overload validation now rejects impossible numeric conversions such as `double` literal to `int` before generated C# emission, while preserving integral constant conversions that C# 7.3 accepts for smaller numeric parameters.
- Tracked function parameters annotated with imported C# metadata types as instance receivers, so existing instance member/property/indexer/event diagnostics apply beyond constructor-created locals.
- Propagated imported instance receiver metadata through local aliases and local type annotations, keeping alias member diagnostics and successful alias member calls inside the pre-emission interop pipeline.
- Propagated imported instance receiver metadata through simple assignments, so assignment-updated locals participate in `TS2410`/`TS2412`/`TS2416` diagnostics before generated C# emission.
- Explicit generic method calls now preserve type arguments and validate metadata arity before generated C# emission.
- Indexed imported C# generic parameter constraints and validates explicit generic method type arguments against `class`, `struct`, `new()`, and nominal/interface constraints before generated C# emission.
- Imported generic method calls without explicit type arguments now infer metadata generic arguments from literal arguments, imported constructor calls, metadata-tracked imported locals/parameters, and explicit constructed generic arguments such as `LegacyBox<LegacyNamed>` flowing into metadata parameters such as `LegacyBox<!!0>`, then validate the same C# generic constraints before generated C# emission.
- Imported generic type constructor calls such as `LegacyBox<T>(...)` now lower to C# 7.3 `new LegacyBox<T>(...)` syntax and can seed imported receiver metadata for following member checks.
- Public constructor parameter metadata is indexed for imported C# types, and mismatched constructor arguments, including generic type constructor type-argument substitution such as `LegacyBox<LegacyNamed>("Ada")`, now report `TS2406` before generated C# emission.
- Imported C# constructor overload validation now has explicit smoke coverage for omitted optional parameters, named constructor arguments, expanded `params` constructor arguments, and ambiguous equally applicable constructor candidates reporting `TS2402` before generated C# emission.
- Constructed generic imported arguments now contribute to inferred generic method constraint validation, so `RequireBoxedNamed<T>(LegacyBox<T>)` accepts `LegacyBox<LegacyNamed>(...)` and rejects `LegacyBox<string>(...)` with `TS2417` before generated C# emission.
- Imported C# delegate lambda overload validation now uses known `System.Func`/`System.Action` delegate parameter counts to reject incompatible lambda arity before generated C# emission, so single-parameter lambdas select one-parameter delegate overloads and report `TS2406` for binary delegate targets.
- Imported C# delegate lambda overload validation now also uses known lambda body return types for `System.Func` targets, so `text => 42` can select `Func<string, int>` over `Func<string, string>` and report `TS2406` when a `Func<string, string>` target is the only candidate.
- Imported C# delegate lambda overload validation now treats an identity lambda body such as `text => text` as the corresponding delegate parameter type, so it can select `Func<string, string>` over `Func<string, int>` and report `TS2406` when the lambda parameter type cannot satisfy the delegate return type.
- Imported C# delegate lambda overload ranking now scores known lambda return conversions, so exact body return targets such as `Func<string, int>` outrank widening numeric targets such as `Func<string, long>` when both candidates are otherwise applicable.
- Imported C# delegate lambda overload validation now infers simple lambda parameter member access returns such as `item => item.Name` when the delegate parameter is a metadata-backed C# type with a public instance property or field, allowing `Func<LegacyNamed, string>` to be selected or incompatible `Func<LegacyNamed, int>` targets to report `TS2406` before emission.
- Imported C# generic type constraints now walk indexed metadata base/interface chains, so a type that satisfies a constraint through an inherited base or interface relation is accepted before generated C# emission.
- Framework generic method constraints are indexed and validated with the same `TS2417` no-emission path as local DLL metadata, including `System.Nullable.Compare<T>` value-type constraints from `mscorlib`.
- Indexed `ExtensionAttribute` on public metadata methods, so local DLL and framework extension methods are represented distinctly from normal static methods.
- Imported C# metadata receivers may use applicable extension methods through instance-call syntax; validation accepts extension methods whose first parameter matches the receiver type, an indexed base/interface relation, or `object` fallback, validates the receiver plus instance-call arguments with the C# overload resolver, ranks receiver metadata relationship distance ahead of farther interface or `object` fallback candidates, and generated C# preserves the normal extension-call syntax for C# 7.3 overload binding.
- TypeSharp check/build now pass imported metadata relationships into type checking, so C# class-to-interface or derived-to-base assignments and returns are accepted when indexed metadata proves the relation, while invalid imported metadata assignments still stop before generated C# emission with `TS2201`.
- Framework assembly imports now use available `net48` reference assembly metadata, so missing framework type imports, missing framework static method calls, and missing framework static member access can report `TS2408`, `TS2407`, and `TS2409` before generated C# emission instead of relying only on generated C# compilation.

Representative verification:
- focused metadata reader, reference resolver, overload resolver, interop validator, checker, and CLI build no-emission smokes.
- full compiler smoke suite recorded on the latest C# interop diagnostic packets.

## CLI VSCode And Tooling

Covered work:
- 0057-0069 `typesharp run`, CLI diagnostic behavior, formatter convention, VS Code/LSP scaffold, diagnostics, hover, definition, and completion.
- 0117-0118 tooling/docs adoption goal and VS Code LSP client activation.
- 0122, 0124-0132 CLI format/new/options work and VS Code format provider.
- 0136 docs-site VS Code/LSP smoke contract.
- 0253 CLI manifest semantic validation for supported `TypeSharp.toml` option values.

Summary:
- Implemented CLI flows for `check`, `build`, `run`, `format`, `new`, common options, warning gates, configuration/target framework selection, verbosity, strict project option parsing, and build no-emission on diagnostics.
- Added semantic manifest value checks for `project.outputType`, `language.version`, `language.nullable`, and `tooling.diagnosticFormat`, reporting `TS0103` before source discovery or build emission.
- Connected VS Code extension activation to stdio LSP diagnostics, hover, definition, completion, and document formatting.
- Documented reproducible VS Code/LSP smoke commands in the docs site.

Representative verification:
- CLI command smoke tests, manifest validation smokes, extension host smoke notes, formatter checks, and docs-site smoke contract build.

## Documentation Process Release And Adoption

Covered work:
- 0049 .NET Framework application model compatibility contract.
- 0094-0100 test coverage checklist audit, progress/ADR policy, migration guide draft, regression test policy.
- 0119-0120 runnable example catalog and Astro Starlight docs site.
- 0133-0137 ASP.NET/WCF/worker host runnable examples and host compatibility consistency.
- 0145-0146 GitHub README and GitHub Pages human docs expansion.
- 0150 official docs benchmark and docs expansion.
- 0160 docs-site custom 404.
- 0167 official docs deep benchmark and docs overhaul.
- 0251 docs-site canonical language/project ledger migration and `docs/` temporary agentic surface reduction.
- 0252 agent bootstrap docs-site canonical follow-up and task-end commit/push rule.

Summary:
- Established progress, ADR, regression, checklist, traceability, feature review, release, and docs-site governance.
- Added runnable examples for console, library public API, C# interop, diagnostics, worker host, and ASP.NET/WCF-style host scenarios.
- Built Astro Starlight docs-site and expanded user-facing information architecture across learning paths, guides, reference, tools, migration, examples, diagnostics, and advanced topics.
- Added docs benchmark artifacts against TypeScript/F#/C#/Vue/Nuxt-style documentation patterns.
- Made docs-site the canonical home for standard language, requirements, feature status, grammar/reference, lowering, interop/runtime ABI, standard library, CLI/formatting/diagnostics, migration/examples, project policy, official reference tracking, project ledger, and work ledger documentation.
- Reduced `docs/` standard documents to short bridge stubs, moved example source/projects to root `examples/`, moved official docs benchmark reports/inventory to `docs-site/research`, and kept `docs/` focused on task packets, rollups, handoff, checklist, traceability, and agentic execution control.
- Aligned `agent.md`, `docs/agentic-execution.md`, and `docs/goal.md` with the post-0251 docs-site canonical ownership model, and made task-end git commit/push handoff explicit in `agent.md`.

Representative verification:
- `npm run build` from `docs-site`.
- docs route/content contract checks recorded in source history.
- Full compiler smoke harness after updating example/benchmark artifact paths.
- Preview HTTP checks for docs-site grammar/reference/modules/document-ownership/examples/tutorials/project-ledger/work-ledger pages.

## Language Safety Modules And Import Export

Covered work:
- 0151-0154 `dynamic`, capability marker propagation, and `unknown` narrowing diagnostics.
- 0155-0159 root namespace fallback, ambient signatures, `open`, named import aliases, and namespace import aliases.
- 0161-0166 import alias conflicts, export specifier parsing, unsupported export forwarding diagnostic, source module path identity, relative source module graph, and multi-source module container lowering.
- 0168-0169 relative source import lowering and local export-list public surface.
- 0186 duplicate local export diagnostics.
- 0187 relative source named import public-surface diagnostics.
- 0188 relative source namespace import member public-surface diagnostics.
- 0224 relative named source re-export surface and generated C# function forwarding.
- 0225 relative named source re-export aliases for generated C# function forwarding.
- 0226 relative named source function import aliases for generated C# function forwarding.
- 0227 local named function export aliases for generated C# public forwarding.
- 0228 relative source type import aliases for generated C# using alias lowering.
- 0229 local type export aliases with relative source type import target remapping.
- 0230 local literal export aliases for generated C# public constant forwarding.
- 0231 local top-level value export aliases and field/property lowering.
- 0232 relative named source top-level value import aliases for generated C# property forwarding.
- 0233 relative type-only source re-exports for source graph type surface and generated C# import alias remapping.
- 0234 relative named top-level value re-exports and relative star re-exports over the currently lowerable function/value/type source module surface.
- 0235 explicitly annotated function-valued top-level `let` exports and local export aliases lowered as generated C# `System.Func`/`System.Action` values.
- 0236 relative named source type import aliases outside `import type` lowered as generated C# using aliases.
- 0237 relative named source module import aliases lowered as generated C# type aliases.
- 0238 relative named source module re-export aliases remapped to original generated C# module aliases.
- 0239 `nameof` intrinsic expressions parsed, root-validated, inferred as `string`, and lowered to C# `nameof(...)`.
- 0240 `checked(...)`/`unchecked(...)` intrinsic expressions parsed, inferred as the inner expression type, and lowered to C# overflow-context expressions.
- 0241 F#-style `>>`/`<<` unary function composition expressions parsed without breaking nested generic type closers and lowered to C# delegate lambdas.
- 0242 TypeScript-style `satisfies` proof expressions parsed with a type RHS, checked against named types or named structural shapes, and erased to the original left-side expression in generated C#.
- 0243 Block-level `yield` statements parsed inside function bodies, checked against explicit `IEnumerable<T>`/`IEnumerator<T>` iterator element types, and lowered to C# `yield return` statements.
- 0244 Block-level `lock` statements parsed inside function bodies, checked for non-null reference synchronization expressions when known, and lowered to C# `lock` blocks.
- 0245 TypeSharp-authored explicit-receiver `extension` methods parsed as declarations, checked for receiver parameter compatibility, and lowered to C# extension methods.
- 0246 TypeScript-style `A & B` intersection type aliases parsed as `IntersectionType`, combined named structural shape aliases for local checking, and rejected compile-time-only boundary leaks with `TS2204`.
- 0247 TypeScript-style collection spread elements `...expr` parsed as `SpreadElement`, checked to require known array or `List<T>` sources, and lowered to C# 7.3-compatible concatenation for array and explicit `List<T>` targets.
- 0248 TypeScript-style nominal record spread fields `...expr` parsed as `RecordSpreadField`, checked to require known nominal record sources when constructing an expected record type, and lowered to C# 7.3-compatible constructor arguments that copy matching target fields before later explicit field overrides.
- 0249 TypeScript-style `keyof T` parsed as `KeyofType`, string literal type members parsed as `LiteralType`, checked as compile-time-only string literal unions over known nominal record or named structural shape members, rejected at public .NET API boundaries with `TS2204`, and lowered to string-typed C# implementation details for private/internal generated code.
- 0250 TypeScript-style `T["Member"]` indexed access types parsed as `IndexedAccessType`, checked against known nominal record or named structural shape member keys, rejected at public .NET API boundaries with `TS2204`, and lowered to the selected C# runtime member type for private/internal generated code when the member type is singular.

Summary:
- Enforced `dynamic`, `reflect`, `interop`, and `unsafe` capability boundaries before generated C# emission.
- Required narrowing proof for `unknown` member/indexer access.
- Added source-root-relative module path identity, duplicate module path diagnostics, relative source import/export graph edges, deterministic multi-source generated containers, relative named/namespace import lowering, relative named function import alias forwarding, relative named top-level value import alias forwarding, relative source type import alias lowering including regular named exported type aliases, relative source module import alias lowering, relative named source module re-export alias remapping, relative named re-export function forwarding including `as` aliases, relative named top-level value re-export property forwarding, relative type-only re-export surface forwarding including `as` aliases, relative star re-exports over the lowerable function/value/type surface, local export-list public markers, local named function export alias public forwarding, local literal export alias public constant forwarding, local top-level value export alias public property lowering backed by generated fields, explicitly annotated function-valued top-level `let` lowering and local export alias forwarding as generated C# delegate values, local type export alias public-surface mapping, `nameof` intrinsic parsing/root validation/type inference/lowering, `checked(...)`/`unchecked(...)` intrinsic parsing/type-preserving inference/lowering, `>>`/`<<` composition parsing and delegate lowering, `satisfies` structural proof checking and C# erasure lowering, iterator `yield` element checking and C# `yield return` lowering, block-level `lock` gate checking and C# `lock` lowering, explicit-receiver extension method checking and C# extension method lowering, intersection type alias parsing and structural shape composition checking, limited `keyof` parsing/type checking/string lowering, limited indexed access type parsing/type checking/member-type lowering, collection spread element parsing/type checking/C# concatenation lowering, nominal record spread parsing/type checking/C# constructor lowering, unsupported export forwarding diagnostics for non-relative/non-lowerable forwarding forms, duplicate local export diagnostics, and missing source module export diagnostics for named/type imports, re-exports, and namespace import member access.

Remaining out-of-scope:
- Non-relative re-export, non-lowerable re-export forms beyond the current function/literal/value/type/module alias forwarding and remapping slice, and unannotated lambda-valued top-level `let` exports whose delegate shape cannot be emitted safely.
- Broader generic method inference beyond direct parameters and explicit constructed generic argument positions, fuller C# overload conversion/contextual ranking beyond current literal/null metadata-specificity/imported metadata relationship/delegate arity, known return checks, identity lambda parameter return checks, known lambda return conversion ranking, metadata member-chain return inference, metadata instance method-call return inference, imported extension method-call return inference, imported static method-call return inference, and comparison/logical binary predicate return inference, fuller indexer conversion ranking beyond exact/object/known numeric/metadata relationship checks, richer lambda body contextual typing beyond known return expressions, identity parameter returns, metadata-backed member chains, metadata-backed instance/extension/static method-call bodies, and comparison/logical binary predicate bodies, and richer extension conversion/conflict diagnostics.

Representative verification:
- module path, source graph, import/export, capability, `unknown`, and generated C# lowering smokes.
- docs-site build and tracked binary checks recorded in the later module packets.

## Verification Summary

The compressed packet set records passing results for:
- compiler/test project build and full smoke harness runs,
- parser fixture snapshots,
- binder/type checker positive and negative diagnostics,
- generated C# golden snapshots and `net48` CLI build smokes,
- runtime/core helper unit smokes,
- C# metadata/reference/interop validation smokes,
- CLI check/build/run/format/new behavior,
- VS Code/LSP activation and feature smokes,
- docs-site `npm run build`,
- `git diff --check`,
- tracked binary guard checks.

This rollup started as a documentation compression step and now also records follow-up feature slices as they complete. Re-run feature-specific tests from the relevant source area when changing implementation behavior; do not rely on this rollup as a substitute for fresh verification.

## Compression Verification

Commands:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet --ignore-failed-sources
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "binary return"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "delegate lambda"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "metadata reader indexes local public symbols"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "overload resolver"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "re-export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "module re-export alias"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "source module"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "source module import alias"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "type import alias"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "relative source named type"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "module import alias"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "relative source value"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "relative type re-export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "relative source type re-export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "value re-export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "star re-export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "function value"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "relative source named"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "import"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "local export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "local function export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "local literal export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "local value export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "local type export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "export"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "nameof"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "checked unchecked"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "composition lowering"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "satisfies"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "yield iterator"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "lock statement"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "extension method lowering"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "collection expression lowering"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI build compiles record expression construction"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI build compiles keyof type operator"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI build compiles indexed access type operator"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI explain prints diagnostic descriptor metadata"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI check emits JSON public boundary diagnostics"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "CLI build stops before emission on public boundary diagnostics"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "binder fixture diagnostics match"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "parser fixture snapshots match"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "type checker fixture diagnostics match"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "C# backend fixture snapshots match"
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
npm run build
rg --files docs/tasks
stale previous rollup filename/range search
rg -n "[ \t]$" <touched files>
git diff --check
```

Result:
- Pass. Compiler test project build passed.
- Pass. Focused binary return, delegate lambda, metadata reader, overload resolver, re-export, module re-export alias, source module, source module import alias, type import alias, relative source named type, module import alias, relative source value, relative type re-export, relative source type re-export, value re-export, star re-export, function value, relative source named, import, local export, local function export, local literal export, local value export, local type export, export, `nameof`, `checked unchecked`, `composition lowering`, `satisfies`, `yield iterator`, `lock statement`, `extension method lowering`, `collection expression lowering`, record expression construction/spread smoke, `keyof` type operator smoke, indexed access type operator smoke, public-boundary diagnostic descriptor/CLI smokes, binder fixture, parser fixture, type checker fixture, and C# backend fixture smokes passed, including local/import/re-export/star/function-value alias, `nameof`, checked/unchecked, composition, satisfies, iterator yield, lock statement, extension method, intersection type alias coverage, collection spread lowering, nominal record spread lowering, limited `keyof` lowering, and limited indexed access type lowering.
- Pass. Full compiler smoke suite passed.
- Pass. docs-site build generated 32 pages, including `document-ownership`, `project-ledger`, `project-policy`, `work-ledger`, and `agentic-workflow`.
- Pass. Post-0252 docs-site build generated 32 pages after agent bootstrap ownership and task-end commit/push rule updates.
- Pass. Post-0252 compiler test project build passed. The full compiler smoke harness was started but did not finish before the 304-second command timeout, and the remaining `dotnet` processes were stopped; no code files were changed during task 0252 after the earlier full smoke result recorded above.
- Pass. Post-0253 compiler test project build passed.
- Pass. Post-0253 focused `manifest`, `invalid manifest value`, diagnostic descriptor registry, and target framework override smokes passed.
- Pass. Post-0253 docs-site build generated 32 pages, and focused docs-site/GitHub Pages contract smokes passed.
- Pass. No stale `0001-0252` rollup references or temporary 0253 task packet references remain in live docs.
- Pass. No trailing whitespace was found in touched files.
- Pass. `git diff --check` reported no whitespace errors; Git printed line-ending normalization warnings only.
- Pass. `docs/tasks` contains `README.md` and this completed-work rollup.
- Pass. No live docs/tests links use old `docs/examples` or `docs/official-docs-*` artifact paths.
- Pass. No live task ledger references point to the removed `0001-0251` rollup or the temporary 0252 task packet.
- Pass. No stale migration-in-progress wording remains in `agent.md`, `docs/`, or docs-site source docs.
- Pass. No trailing whitespace was found in touched files.
- Pass. `git diff --check` reported no whitespace errors; Git printed line-ending normalization warnings only.

## Handoff

Done:
- Replaced the accumulated per-task packet set with one compact project work ledger.
- Added block-level `lock` statement parsing, type checking, C# lowering, fixtures, and CLI smoke coverage.
- Added TypeSharp-authored explicit-receiver `extension` method parsing, receiver checking, C# lowering, fixtures, and CLI smoke coverage.
- Added TypeScript-style `A & B` intersection type alias parsing, structural shape composition checking, public boundary diagnostics, fixtures, and documentation coverage.
- Added collection spread element parsing, type checking, C# 7.3-compatible array/List lowering, fixtures, and CLI smoke coverage.
- Added nominal record spread field parsing, non-record source diagnostics, C# 7.3-compatible constructor lowering, parser/backend/type-checker fixtures, and CLI smoke coverage.
- Added limited `keyof` type operator parsing, string literal type members, record/shape key checking, public-boundary diagnostics, C# 7.3-compatible string lowering, parser/backend/type-checker fixtures, and CLI smoke coverage.
- Added limited indexed access type operator parsing, record/shape member-type checking, public-boundary diagnostics, C# 7.3-compatible selected-member-type lowering, parser/backend/type-checker fixtures, and CLI smoke coverage.
- Completed Q0 task `0251-docs-site-canonical-language-ledger`, migrated standard language/project ledger ownership into docs-site, and left `docs/` focused on temporary agentic work plus short bridge stubs.
- Completed Q0 task `0252-agent-bootstrap-docs-site-canonical-followup`, aligned remaining agent bootstrap docs with docs-site canonical ownership, and added the task-end commit/push rule to `agent.md`.
- Completed Q4 task `0253-cli-manifest-semantic-validation`, adding semantic `TypeSharp.toml` option value validation, `TS0103` compiler/CLI smoke coverage, and docs-site manifest value-domain documentation.
- Preserved completed ranges, major implemented surfaces, verification themes, and remaining follow-up areas.
- Kept `docs/tasks/README.md` as the short entry point.

Remaining:
- Select the next task from `docs/checklist.md` and `docs/traceability.md`.

Blocked:
- None.
