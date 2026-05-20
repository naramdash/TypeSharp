---
title: Feature Status
description: Canonical status map for TypeSharp language, compiler, interop, tooling, and documentation features.
---

This page is the docs-site canonical feature-status ledger established by task `0251-docs-site-canonical-language-ledger`. It folds the durable status model from the former `docs/feature-map.md` and MVP boundaries from the former `docs/feasibility.md`; those bridge files were removed after migration.

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

## MVP Language Features

| Area | Status | Current TypeSharp Direction |
| --- | --- | --- |
| Null safety | MVP | Reference-like types are non-null by default; nullable values use `T?` or `Option<T>`; unknown C# nullability is reported in strict contexts. |
| Nominal closed unions | MVP | `union` declarations are the runtime/domain union model and lower to a C#-compatible class hierarchy for the implemented slice. |
| Type-level unions | MVP local only | `A \| B` is compile-time-only for local inference, literal unions, structural narrowing, and overload reasoning; public ABI reports diagnostics. |
| Pattern matching | MVP | `match` is expression-oriented and moves toward exhaustive checking for nominal unions and supported narrowing paths. |
| Structural shapes | MVP local only | Shape checks and `satisfies` are compile-time proof tools; public APIs must expose nominal alternatives. |
| Structural intersection aliases | MVP limited | Named structural shape aliases can compose with `A & B`; general intersection normalization remains backlog. |
| `keyof` | MVP limited | Known records and named structural shapes can produce local string literal key unions. |
| Indexed access types | MVP limited | Known record/shape member types can be selected with `T["Member"]`; public ABI leakage is rejected. |
| Local type inference | MVP | Locals, literals, calls, binary expressions, lambdas, and supported pipeline flows infer common types. |
| Records | MVP | Immutable public data shapes lower to C#-friendly nominal types. |
| Async `Task` interop | MVP | `async fun` lowers to `Task` or `Task<T>` for .NET Framework compatibility. |
| Pipeline and composition | MVP limited | First-argument pipeline and unary composition lower to C# 7.3-compatible calls and delegate lambdas. |
| Collection expressions | MVP limited | Array and `List<T>` targets are supported, including known array/List spread lowering. |
| Record expressions and spread | MVP limited | Expected nominal records lower to constructor calls; nominal record spread copies fields then applies overrides. |
| Iterator `yield` | MVP limited | Explicit CLR enumerable return types are checked against yielded element values. |
| `lock` statement | MVP limited | Block-level lock lowers to C# `lock` and requires a non-null reference gate when known. |
| `nameof` | MVP | Compiler intrinsic preserved as C# `nameof(...)`. |
| `checked` and `unchecked` | MVP | Overflow context expressions lower directly to C# checked/unchecked expressions. |
| Explicit-receiver extension methods | MVP limited | TypeSharp-authored extension methods lower to C# extension methods; richer conversion/conflict rules remain backlog. |

## Module And Interop Features

| Area | Status | Current TypeSharp Direction |
| --- | --- | --- |
| Explicit module graph | MVP | Files are source-root-relative modules; imports/exports determine public surface and generated containers. |
| Relative imports and re-exports | MVP limited | Function/value/type/module aliases and currently lowerable star re-exports are implemented for relative source modules. |
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
| General mapped/conditional/template-literal type computation | Type-level complexity can grow quickly and needs a budgeted evaluator. |
| NuGet restore inside the compiler | Requires lock files, transitive dependency policy, license handling, and checksum/security rules. |
| ASP.NET/WCF/Windows Service templates and packaging automation | Current support proves host reference/build shape; generation and deployment automation are separate adoption work. |
| Richer extension method conversion diagnostics | Current receiver metadata matching is useful, but full C#-style conversion/conflict behavior is broader. |
| F# option/tuple/record interop layer | Valuable for .NET ecosystem fit, but not required for the initial C#-first interop path. |

## Preview Watch And Experimental

| Area | Status | Boundary |
| --- | --- | --- |
| C# preview union types | Preview Watch | Directional input only; TypeSharp's stable union model remains nominal closed unions plus local type-level unions. |
| TypeScript native compiler and 7.0 behavior | Preview Watch | Tooling strategy input only; not a TypeSharp runtime or syntax requirement. |
| Type providers | Experimental | Build-time external schema/code execution needs permission, cache, reproducibility, and sandbox policy first. |
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
