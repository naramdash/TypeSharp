---
title: Feature Status
description: Canonical status map for TypeSharp language, compiler, interop, tooling, and documentation features.
---

This page is the docs canonical feature-status ledger established by task `0251-docs-canonical-language-ledger`. It folds the durable status model from the former `docs/feature-map.md` and MVP boundaries from the former `docs/feasibility.md`; those bridge files were removed after migration.

## Status Buckets

| Status | Meaning |
| --- | --- |
| MVP | Targeted by the first stable implementation path and should have docs plus fixtures or smoke tests when implemented. |
| Stable Backlog | Accepted direction, but not required for the first stable slice. |
| Preview Watch | External language or .NET feature is still preview or too unsettled for TypeSharp's stable contract. |
| Experimental | Needs a TypeSharp feature gate, security review, or separate validation before becoming stable. |
| Rejected | Not part of the current language direction. |

## Adoption Lens

TypeSharp evaluates every C#, F#, and TypeScript feature through four practical goals:

- TypeScript-style flexible static typing: inference, contextual typing, structural checks, literal types, `unknown` narrowing, and compile-time type operators where they stay explainable.
- TypeScript-style modules: explicit source module graph, imports, exports, source roots, and ambient declarations.
- F#-style functional consistency: immutable data, option/result modeling, nominal closed unions, pattern matching, pipeline, and composition.
- C#/.NET practicality: generated public metadata, attributes, records/classes/interfaces/delegates/events, async `Task`, generics, and local C# DLL/framework assembly interop.

## TypeScript Structural And Module Review

Official TypeScript sources refreshed on 2026-05-21:

- [Type compatibility](https://www.typescriptlang.org/docs/handbook/type-compatibility)
- [Narrowing](https://www.typescriptlang.org/docs/handbook/2/narrowing.html)
- [Creating Types from Types](https://www.typescriptlang.org/docs/handbook/2/types-from-types.html)
- [Modules reference](https://www.typescriptlang.org/docs/handbook/modules/reference)
- [Project references](https://www.typescriptlang.org/docs/handbook/project-references)
- [TSConfig `paths`](https://www.typescriptlang.org/tsconfig/paths.html)
- [Type declarations](https://www.typescriptlang.org/docs/handbook/2/type-declarations)
- [TSConfig reference](https://www.typescriptlang.org/tsconfig/)
- [TypeScript 6.0 release notes](https://www.typescriptlang.org/docs/handbook/release-notes/typescript-6-0.html)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [TypeScript 5.9 release notes](https://www.typescriptlang.org/docs/handbook/release-notes/typescript-5-9.html)

Related .NET artifact source refreshed on 2026-05-21:

- [MSBuild `ProjectReference` items](https://learn.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-items?view=vs-2022#projectreference)

Current boundary:

- TypeSharp adopts TypeScript's structural-checking ergonomics only where they remain local, deterministic, and explainable before C# lowering.
- Compile-time-only structural shapes, type-level unions, intersections, and derived type operators must not appear directly in public CLR metadata.
- TypeSharp keeps files as modules by default and uses an explicit source module graph; it does not model Node, bundler, CommonJS, package `exports`, `paths`, or JavaScript emit semantics as stable language behavior.
- `TypeSharp.toml` is the project contract. It can learn from TSConfig options such as source includes, project references, declaration output, and stricter checking, but it must map to generated `net48` artifacts instead of JavaScript runtime settings.
- Manifest-owned current-project source aliases are implemented for source graph imports/re-exports. TypeSharp direct project references are implemented for manifest-owned source imports, referenced-project build ordering, generated `net48` assembly consumption, and deterministic graph/export diagnostics before dependent emission.
- TypeScript 7.0 Beta and the native compiler port are tooling strategy signals only. TypeSharp should learn from stable type ordering, faster project graph analysis, and side-by-side tooling migration, but it must not depend on TypeScript's Go toolchain or JavaScript compiler APIs.

| TypeScript Signal | TypeSharp Status | TypeSharp Direction |
| --- | --- | --- |
| Structural compatibility and object shapes | MVP local only | Shape aliases and `satisfies` provide local proof. Public APIs use records, classes, interfaces, delegates, or nominal unions unless a future adapter policy generates stable names. |
| `unknown`, type guards, and discriminated narrowing | MVP limited | `unknown` requires proof before access. Type-level unions support type-pattern narrowing, and local structural/type-level unions support bounded equality/inequality narrowing on required literal discriminant members. Broader boolean algebra remains backlog. |
| Type aliases and interfaces | MVP split | Type aliases remain flexible local type expressions; `public interface` is the CLR-visible contract shape. TypeScript declaration merging is not stable because it obscures generated metadata ownership. |
| `keyof` and indexed access | MVP limited | Known records and named structural shapes can derive local key/member types. Optional members, index signatures, and deeper generic operators need more checker coverage. |
| Mapped, conditional, template-literal, and utility types | Stable Backlog | Accepted direction only through the documented evaluator budget: bounded alias depth, evaluator steps, union width, mapped keys, conditional distribution, template literal products, deterministic diagnostics, and no public compile-time-only leakage. |
| ES module imports/exports and type-only imports | MVP limited | Relative source imports/re-exports, type-only imports, namespace imports, explicit export surfaces, current-project manifest-owned source aliases, and direct TypeSharp project source imports are implemented for source modules. Alias and project-reference targets must resolve into deterministic TypeSharp source graphs and generated C# identity, or diagnose before emission. |
| TSConfig, project references, and declaration files | Stable Backlog | Useful as configuration signals for source roots, broader project graph partitioning, generated declaration metadata, and editor navigation. TypeSharp keeps `TypeSharp.toml`, generated `net48` assemblies, and compiler-derived export metadata as the stable artifact model; the implemented project-reference slice requires direct manifest ownership and builds referenced projects first. |
| TypeScript 6.0/5.9 module defaults, subpath imports, `node20`, `import defer`, and stable type ordering | Preview/Directional Watch | TypeSharp already uses modules by default. Node/bundler modes, package subpath imports, and deferred JavaScript evaluation are runtime-specific signals; stable type ordering is useful compiler-engineering input for deterministic diagnostics and generated artifacts. |
| JSX, decorators, JavaScript runtime compatibility, and npm package semantics | Rejected or Experimental | JSX and JavaScript execution are outside the CLR target. Decorator-like metaprogramming must route through .NET attributes/analyzers/generators with explicit safety policy. |

## F# Functional Consistency Review

Official F# sources refreshed on 2026-05-21:

- [F# documentation](https://learn.microsoft.com/en-us/dotnet/fsharp/)
- [F# strategy](https://learn.microsoft.com/en-us/dotnet/fsharp/strategy)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [F# functions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/functions/)
- [F# pattern matching](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/pattern-matching)
- [F# discriminated unions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions)
- [F# options](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/options)
- [F# computation expressions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)
- [F# task expressions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/task-expressions)

Current boundary:

- TypeSharp uses F# as the functional-consistency benchmark, not as a syntax compatibility target.
- Generated artifacts remain `net48` assemblies with C# 7.3-compatible generated source and no `FSharp.Core` runtime dependency by default.
- F# 10 features are design signals only when they improve clarity, diagnostics, or .NET task interop without introducing F# compiler/runtime requirements. Scoped warning control, attribute target enforcement, `task` `and!`, and parallel compilation reinforce TypeSharp's need for deterministic diagnostics, explicit async lowering, and ordered parallel compiler work.

| F# Signal | TypeSharp Status | TypeSharp Direction |
| --- | --- | --- |
| Immutable values, expression-result functions, local inference, first-class functions | MVP | TypeSharp keeps `let`, expression/block-bodied `fun`, function type values, and local inference, while public CLR boundaries continue to prefer explicit annotations. |
| Pipelines and composition | MVP limited | `|>`, `>>`, and `<<` are implemented with C# 7.3-compatible lowering. Broader partial-application/currying remains backlog because generated delegate shapes must stay predictable for C# consumers. |
| Records and discriminated unions | MVP | TypeSharp records and nominal unions are the stable public data/domain model. Recursive and mutually recursive union ergonomics remain backlog after exhaustiveness and ABI shape are stronger. |
| Pattern matching and exhaustiveness | MVP expanding | Nominal-union, TypeSharp-owned enum, imported C# enum, `bool`, and local type-level union match diagnostics cover known missing cases/members, including local literal-union members. `_` discard arms can cover the remaining known space, and `when` guards are checked in narrowed arm scope without proving coverage by themselves. Richer pattern algebra remains backlog. |
| Option, ValueOption, and result ergonomics | MVP plus Stable Backlog | `Option<T>` and `Result<T,E>` are core nominal unions. Struct-backed value options and richer bind/map/default helpers are backlog until ABI and allocation tradeoffs are documented for `net48`. |
| Computation expressions, task workflows, and `and!`-style concurrency | Stable Backlog | TypeSharp keeps direct `async fun`/`Task<T>` interop as MVP. General builder-based computation expressions need a design that avoids macros, user-code execution during build, and non-obvious lowering. |
| Active patterns | Stable Backlog | Useful as named match extractors, but they need deterministic binder/type-checker rules and diagnostics before syntax is accepted. |
| Type providers | Experimental | Build-time external schema/code execution remains behind a security, cache, reproducibility, and sandbox policy. |
| Units of measure | Experimental | Attractive compile-time numeric safety, but TypeSharp needs a public ABI erasure policy, diagnostics, and operator rules before adopting it. |
| F# 10 warning/attribute tightening | Stable Backlog | Scoped warning control and stricter attribute-target diagnostics are useful tooling signals, but they belong in TypeSharp diagnostic/attribute policy rather than F# directive syntax. |

## C# Stable And Preview Parity Review

Official C# sources refreshed on 2026-05-21:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)

Current boundary:

- C# 14 is the current stable C# release and is supported on .NET 10.
- C# 15 is the current preview C# release and is supported on .NET 11 preview.
- Microsoft documents .NET Framework projects as defaulting to C# 7.3, so TypeSharp generated source remains C# 7.3-compatible for `net48`.
- C# 15 preview currently includes collection expression arguments and union types. Both remain directional only because TypeSharp already owns collection lowering and union semantics under its `net48` public ABI policy.

| C# Signal | TypeSharp Status | TypeSharp Direction |
| --- | --- | --- |
| C# 14 extension members, including extension properties and static extension members | Stable Backlog | TypeSharp-authored explicit-receiver extension methods are already MVP limited. Richer extension properties/static extension members need a metadata/lowering design that emits ordinary C# 7.3-compatible extension/static members. |
| C# 14 null-conditional assignment | Stable Backlog | Useful TypeSharp ergonomics if lowered to explicit null guards in generated C# 7.3. Design must preserve left/right evaluation order and diagnostics before implementation. |
| C# 14 `nameof` on unbound generic types | MVP | Implemented as TypeSharp `nameof(Generic<>)` and higher-arity `nameof(Generic<,>)` targets with type-root binding and string constant lowering instead of C# 14 syntax. |
| C# 14 simple lambda parameter modifiers | Stable Backlog | Useful for imported delegate/byref interop. TypeSharp should accept only explicitly modeled `ref`/`out`/`in` lambda boundaries and lower to C# 7.3-compatible typed lambdas. |
| C# 14 partial constructors and partial events | Stable Backlog | Relevant only after TypeSharp has a broader partial member/public ABI merge policy. Generated code can keep complete members until that policy exists. |
| C# 14 field-backed properties | Replacement | TypeSharp should keep explicit record/class/property semantics and generate backing fields when needed. It does not need a `field` keyword surface. |
| C# 14 user-defined compound assignment operators | Stable Backlog | Depends on a broader operator overload policy. Not stable while full C# operator overloading remains outside the TypeSharp surface. |
| C# 14 file-based app preprocessor directives | Rejected for MVP | TypeSharp uses `TypeSharp.toml`, explicit source discovery, and module graph rules instead of C# file-based app directives. |
| C# 14 first-class `Span<T>` conversions | Experimental | Span-like APIs require `System.Memory`, `net48` deployment, and performance review before TypeSharp adopts a stable surface. |
| C# 15 collection expression arguments | Preview Watch | Directional input for TypeSharp collection expressions only. Do not make `with(...)` collection arguments stable until C# 15 leaves preview and TypeSharp has C# 7.3 lowering semantics. |
| C# 15 union types | Preview Watch | Directional input only. TypeSharp's stable union model remains nominal closed unions plus local type-level unions until C# union runtime/metadata contracts settle. |

## Roadmap Refresh Result

Official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources were rechecked on 2026-05-21 after the match guard implementation. The refresh did not change TypeSharp's baseline: generated artifacts remain `net48`, generated C# remains C# 7.3-compatible, external preview features stay behind Preview Watch, and package/Marketplace/template publication remains gated by Project Policy.

Refresh notes:

- C# 14 remains the latest stable C# release, and C# 15 remains preview on .NET 11 preview with collection expression arguments and union types as directional signals only.
- Microsoft C# language versioning still maps all .NET Framework targets to C# 7.3 by default, preserving TypeSharp's generated-source baseline.
- TypeScript 6.0 remains the transition release toward the TypeScript 7.0 native compiler, while TypeScript 7.0 Beta remains a side-by-side native preview/tooling signal rather than a TypeSharp runtime or syntax dependency.
- F# 10 remains a refinement release for clarity, tooling, and performance; its scoped warnings, task improvements, and trimming signals continue to inform diagnostics/tooling policy rather than adding an F# runtime dependency.
- .NET Framework lifecycle, NuGet lock/source-mapping/audit requirements, and VS Code LSP/Marketplace publication requirements do not change the current TypeSharp baseline.

The bounded implementation slices after the refresh added match guard support and literal match exhaustiveness:

- `match` arm `when` guards parse, bind, type-check, and lower for nominal unions and existing local type-level union match paths.
- Guarded arms do not satisfy exhaustiveness by themselves unless a later unguarded arm or discard arm covers the same closed set.
- Literal match patterns parse for bool, string, and numeric literal arms; `bool` and local literal-union matches report missing cases/members and lower to C# 7.3-compatible conditional comparisons.
- Enum exhaustiveness and richer pattern algebra remain separate follow-ups.

After literal match exhaustiveness, the empty-queue refresh rechecked the same official sources on 2026-05-21 and found no baseline change. The next bounded implementation slice completed simple TypeSharp enum declaration parsing/checking/lowering, giving enum match exhaustiveness a stable TypeSharp-owned enum symbol and generated C# shape.

After enum declarations, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed enum match exhaustiveness for TypeSharp-owned enums; imported C# enum exhaustiveness, flag semantics, explicit underlying types, explicit numeric member values, and enum member attributes remain backlog.

After TypeSharp-owned enum match exhaustiveness, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice extended enum exhaustiveness to named imported C# enum metadata using finite public enum-member fields from referenced assemblies. Flag algebra, explicit TypeSharp enum numeric values, enum aliases, and enum member attributes remain separate backlog items.

After imported C# enum exhaustiveness, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice added explicit numeric member values for TypeSharp-owned enums. This keeps enum type checking and exhaustiveness name/member based, and leaves flags, aliases, explicit underlying types, computed enum expressions, and imported enum numeric metadata as separate backlog items.

After explicit TypeSharp enum numeric values, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice added explicit underlying types for TypeSharp-owned enums. This keeps enum reasoning name/member based and leaves flags, aliases, computed enum expressions, imported enum underlying/numeric metadata, and numeric range validation as separate backlog items.

After explicit TypeSharp enum underlying types, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice is explicit enum numeric range validation for TypeSharp-owned enums. This keeps enum reasoning name/member based and leaves flags, aliases, computed enum expressions, enum member attributes, and imported enum underlying/numeric metadata as separate backlog items.

After explicit TypeSharp enum numeric range validation, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice is explicit enum member aliases for TypeSharp-owned enums. This keeps aliasing separate from arbitrary computed enum expressions, flag algebra, enum member attributes, and imported enum underlying/numeric metadata.

After explicit TypeSharp enum member aliases, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice captured imported C# enum underlying type names and literal numeric member values as metadata. This keeps metadata capture separate from flag algebra, numeric pattern matching, enum member attributes, and arbitrary TypeSharp-owned computed enum expressions.

After imported C# enum numeric metadata capture, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed TypeSharp-owned enum declaration/member attribute lowering. This gives enum metadata attributes such as `[FlagsAttribute]` and member attributes a C# 7.3-compatible source shape while keeping flag algebra, broad attribute target validation, numeric pattern matching, and arbitrary computed enum expressions separate.

After TypeSharp-owned enum declaration/member attribute lowering, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed TypeSharp-owned enum composite member expressions, limited to enum initializer-local `|` over previously declared same-enum members and integer literals. This enables flag-shaped declarations such as `ReadWrite = Read | Write` without enabling general expression bitwise operators, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, broad attribute target validation, or arbitrary computed enum expressions.

After TypeSharp-owned enum composite member expressions, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed expression-level same-enum value `|` expressions such as `Permission.Read | Permission.Write`, without enabling numeric/general bitwise operators, `&`, `^`, `~`, shifts, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, broad attribute target validation, or arbitrary/general computed enum member declarations.

After expression-level same-enum value `|`, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed expression-level same-enum value `&` expressions such as `permission & Permission.Read`, without enabling numeric/general bitwise operators, `^`, `~`, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, broad attribute target validation, or arbitrary/general computed enum member declarations.

After expression-level same-enum value `&`, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed expression-level same-enum value `^` and unary `~` expressions such as `permission ^ Permission.Write` and `~permission`, without enabling numeric/general bitwise operators, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, broad attribute target validation, or arbitrary/general computed enum member declarations.

After expression-level same-enum value `^` and unary `~`, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed expression-level integral numeric `|`, `&`, `^`, and unary `~` over known non-null primitive integral operands, without enabling shifts, compound assignment, boolean bitwise expressions, flag-aware match algebra, imported enum flag reasoning, broad attribute target validation, arbitrary/general computed enum member declarations, numeric pattern algebra, or richer pattern algebra.

After expression-level integral numeric bitwise expressions, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed expression-level boolean `|`, `&`, and `^` over known non-null `bool` operands, without enabling unary boolean complement, shifts, compound assignment, user-defined operators, flag-aware enum algebra, imported enum flag reasoning, broad attribute target validation, arbitrary/general computed enum member declarations, numeric pattern algebra, or richer pattern algebra. Shifts stay separate because `>>` and `<<` already serve TypeSharp function composition and need a dedicated grammar/design pass.

After expression-level boolean bitwise expressions, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice completed bitwise compound assignment `|=`, `&=`, and `^=` over the already supported assignment surface, without enabling shifts, shift assignment, user-defined operators, broader assignment target analysis, flag-aware enum algebra, imported enum flag reasoning, broad attribute target validation, arbitrary/general computed enum member declarations, numeric pattern algebra, or richer pattern algebra.

After bitwise compound assignments, the empty-queue refresh rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 and found no baseline change. The selected bounded implementation slice is local assignment target analysis for TypeSharp identifier assignments, tracking `let mut` and focused assignment compatibility diagnostics while leaving imported C# member, indexer, static member, and event assignment on the existing metadata-backed interop validator path. Shifts and shift assignment remain separate because `>>` and `<<` already serve TypeSharp function composition and need a dedicated grammar/design pass.

The local assignment target slice completed `let mut` tracking for TypeSharp identifier assignments, known simple-assignment compatibility diagnostics, local bitwise compound assignment operand diagnostics, and invalid local assignment target diagnostics. Imported C# member, indexer, static member, and event assignment remains metadata validated by existing interop diagnostics.

## MVP Language Features

| Area | Status | Current TypeSharp Direction |
| --- | --- | --- |
| Null safety | MVP | Reference-like types are non-null by default; nullable values use `T?` or `Option<T>`; unknown C# nullability is reported in strict contexts. |
| Nominal closed unions | MVP | `union` declarations are the runtime/domain union model and lower to a C#-compatible class hierarchy for the implemented slice. |
| Type-level unions | MVP local only | `A \| B` is compile-time-only for local inference, literal unions, structural narrowing, and overload reasoning; public ABI reports diagnostics. |
| Pattern matching | MVP | `match` is expression-oriented with nominal-union, TypeSharp-owned enum, named imported C# enum, `bool`, and local type-level union exhaustiveness, including local literal-union cases. Discard fallback coverage and narrowed `when` guard checks are implemented for the supported paths. |
| Structural shapes | MVP local only | Shape checks and `satisfies` are compile-time proof tools; public APIs must expose nominal alternatives. |
| Structural intersection aliases | MVP limited | Named structural shape aliases can compose with `A & B`; general intersection normalization remains backlog. |
| `keyof` | MVP limited | Known records and named structural shapes can produce local string literal key unions. |
| Indexed access types | MVP limited | Known record/shape member types can be selected with `T["Member"]`; public ABI leakage is rejected. |
| Local type inference | MVP | Locals, literals, calls, binary expressions, lambdas, and supported pipeline flows infer common types. |
| Records | MVP | Immutable public data shapes lower to C#-friendly nominal types. |
| Enums | MVP limited | Simple TypeSharp-owned enum declarations parse, bind duplicate members, support optional explicit integral underlying types, integer numeric member values, aliases to previously declared members, enum initializer-local composite member expressions over previously declared same-enum members and integer literals, declaration/member attribute lists, and expression-level same-enum value `|`/`&`/`^` plus unary `~`; validate explicit member values, numeric operands, alias targets, composite identifier operands, same-enum value `|`/`&`/`^` operands, and unary `~` enum operands; type-check same-enum member values; lower to ordinary C# enums and C# `|`/`&`/`^`/`~` expressions with supported enum attributes; and participate in match exhaustiveness. Named imported C# enums also participate in match exhaustiveness when metadata exposes finite public enum members, and imported enum metadata captures underlying type names plus literal numeric member values for future interop decisions. Shifts, shift assignment, flag algebra beyond these same-enum value operators, arbitrary/general computed enum values, broad attribute target validation, numeric pattern algebra, and flag-style reasoning over imported numeric metadata remain backlog. |
| Integral numeric bitwise expressions | MVP limited | Expression-level primitive integral `|`, `&`, `^`, and unary `~` type-check over known non-null operands, use supported C# integral promotion, and lower to ordinary C# 7.3-compatible operators. Shifts, shift assignment, checked overflow policy changes, and operator overloads remain backlog. |
| Boolean bitwise expressions | MVP limited | Expression-level `bool` `|`, `&`, and `^` type-check over known non-null operands, infer `bool`, and lower to ordinary C# 7.3-compatible non-short-circuit boolean operators. Unary boolean complement, shifts, shift assignment, and user-defined operators remain backlog. |
| Local assignment target analysis | MVP limited | Local identifier assignment requires `let mut`; known simple assignment checks nullability, structural compatibility, and ordinary assignment compatibility; and local bitwise compound assignment checks known enum/integral/bool operands. Imported C# member, indexer, static member, and event assignment remains on the metadata-backed interop validator path. Shift assignment, user-defined operators, TypeSharp member assignment policy, and broader class-member body analysis remain backlog. |
| Bitwise compound assignment | MVP limited | `|=`, `&=`, and `^=` parse and lower for mutable local identifier targets and existing imported C# assignment targets to ordinary C# 7.3-compatible assignment operators. Shift assignment and user-defined operators remain backlog. |
| Async `Task` interop | MVP | `async fun` lowers to `Task` or `Task<T>` for .NET Framework compatibility. |
| Pipeline and composition | MVP limited | First-argument pipeline and unary composition lower to C# 7.3-compatible calls and delegate lambdas. |
| Collection expressions | MVP limited | Array and `List<T>` targets are supported, including known array/List spread lowering. |
| Record expressions and spread | MVP limited | Expected nominal records lower to constructor calls; nominal record spread copies fields then applies overrides. |
| Iterator `yield` | MVP limited | Explicit CLR enumerable return types are checked against yielded element values. |
| `lock` statement | MVP limited | Block-level lock lowers to C# `lock` and requires a non-null reference gate when known. |
| `nameof` | MVP | Ordinary name references preserve C# `nameof(...)`; unbound generic type targets lower to string constants for C# 7.3-compatible generated source. |
| `checked` and `unchecked` | MVP | Overflow context expressions lower directly to C# checked/unchecked expressions. |
| Explicit-receiver extension methods | MVP limited | TypeSharp-authored extension methods lower to C# extension methods; richer conversion/conflict rules remain backlog. |

## Module And Interop Features

| Area | Status | Current TypeSharp Direction |
| --- | --- | --- |
| Explicit module graph | MVP | Files are source-root-relative modules; imports/exports determine public surface and generated containers. |
| Relative imports and re-exports | MVP limited | Function/value/type/module aliases and currently lowerable star re-exports are implemented for relative source modules. |
| Direct TypeSharp project references | MVP limited | `[projectReferences] paths` loads direct TypeSharp manifests, detects cycles and target mismatches, validates direct referenced source exports, builds referenced projects before dependents, and consumes referenced generated assemblies through explicit local references. Hidden transitive source imports and richer cross-project re-export metadata remain backlog. |
| Ambient function signatures | MVP limited | Parsed and checked as external signatures, omitted from generated C# output. |
| C# framework/local DLL references | MVP | Metadata-backed references support constructors, members, properties, fields, indexers, delegates, events, attributes, and generics in the implemented subset. |
| C# overload validation | MVP limited | Supported overload, optional/named/params/byref/generic/nullable and receiver-ranking checks are metadata-backed. |
| Generated `net48` library consumption | MVP | C# .NET Framework projects can consume supported TypeSharp public APIs in smoke tests. |
| ASP.NET/WCF/worker host compatibility | MVP smoke coverage | Generated `net48` library shape is validated through host-style build smokes; packaging automation is backlog. |

## Stable Backlog

| Area | Reason It Is Backlog |
| --- | --- |
| Direct IL backend | Async state machines, PDB/debug info, metadata fidelity, and ABI control need more maturity before source backend replacement. |
| Structural public ABI adapters | Generated interface/wrapper naming, overload rules, versioning, and binary compatibility need a dedicated policy. |
| Tagged struct union representation | Boxing, generics, layout, and C# consumption are more complex than the MVP class hierarchy. |
| General mapped/conditional/template-literal type computation | Type-level complexity can grow quickly. The accepted design requires the evaluator budget in Type System before implementation: finite TypeSharp-owned inputs, bounded recursion/expansion/cardinality, no user-code execution, no TypeScript declaration-file compatibility layer, and public ABI rejection unless the result fully normalizes to CLR-visible metadata. |
| NuGet restore inside the compiler | Requires `PackageReference` metadata, checked-in lock files, locked-mode CI, package source mapping, transitive dependency policy, vulnerability audit severity, license handling, checksums/signatures, and no package-target execution during `typesharp check`. |
| .NET Framework 4.8.1 qualified profile | `net481` can be useful for vendor-qualified deployments, but it needs explicit manifest/CLI admission, target-pack assumptions, Core/Runtime builds, generated project smokes, C# consumer smokes, and host compatibility evidence before support is claimed. |
| ASP.NET/WCF/Windows Service templates and packaging automation | Current support proves host reference/build shape; generation and deployment automation are separate adoption work. |
| VS Code Marketplace and `dotnet new` template publication | Local VSIX packaging and built-in `typesharp new` are current adoption paths. Marketplace and template-pack publication need release-owner credentials, versioned artifacts, checksums, documentation, and rollback paths. |
| Richer extension method conversion diagnostics | Current receiver metadata matching is useful, but full C#-style conversion/conflict behavior is broader. |
| F# option/tuple/record interop layer | Valuable for .NET ecosystem fit, but not required for the initial C#-first interop path. TypeSharp's own functional model must stay independent of `FSharp.Core` by default. |
| General partial application and currying | Valuable for functional ergonomics, but generated delegate and overload shapes need a predictable C# consumption policy first. |
| Computation-expression-style workflows | Useful for options/results/tasks, but requires explicit builder contracts, no build-time user-code execution, and deterministic lowering before it can be stable. |
| Active-pattern-style extractors | Useful for pattern readability, but requires binder/type-checker design for extractor names, payload types, and exhaustiveness impact. |

## Preview Watch And Experimental

| Area | Status | Boundary |
| --- | --- | --- |
| C# preview union types | Preview Watch | Directional input only; TypeSharp's stable union model remains nominal closed unions plus local type-level unions. |
| C# preview collection expression arguments | Preview Watch | Directional input only; TypeSharp collection expression constructor/factory arguments need stable C# semantics and independent `net48` lowering before adoption. |
| TypeScript native compiler and 7.0 behavior | Preview Watch | Tooling strategy input only; not a TypeSharp runtime or syntax requirement. |
| Type providers | Experimental | Build-time external schema/code execution needs permission, cache, reproducibility, and sandbox policy first. |
| Units of measure | Experimental | Compile-time numeric dimensions need erasure, ABI, operator, and diagnostic design before TypeSharp adopts them. |
| Effect annotations | Experimental | Small `async`, `throws`, `io`, `unsafe`, or `dynamic` annotations may inform diagnostics before runtime effect systems. |
| Decorator-like metaprogramming | Experimental or rejected for MVP | .NET attributes, analyzers, and generators must be settled first. |

## Rejected For MVP

| Area | Reason |
| --- | --- |
| Macro system | Adds parser, formatter, LSP, diagnostics, and security risk before the core language is stable. |
| Full dependent types or theorem proving | Outside the project's practical .NET Framework interop goal. |
| JavaScript runtime compatibility | TypeSharp targets CLR/.NET Framework artifacts, not JavaScript execution. |
| Unbounded `any` as the default escape hatch | Conflicts with the `unknown`-first safety model. |

## Related Pages

- [Project Requirements](../requirements/)
- [Core Goal](../goal/)
- [Language Tour](../language-tour/)
- [Type System](../type-system/)
- [Grammar And Language Reference](../reference/)
- [Advanced Topics](../advanced/)
