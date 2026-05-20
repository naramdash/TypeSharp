---
title: Examples
description: Runnable TypeSharp project catalog and feature examples.
---

This is the canonical docs example catalog and coverage guide. Example source artifacts live under the repository root `examples/` folder; their durable narrative and coverage index belongs here.

Repository artifact index: [`examples/README.md`](https://github.com/naramdash/TypeSharp/blob/main/examples/README.md)

## Source Extension

TypeSharp source files use `.tysh`.

Rationale:

- `.tsp` is already used by the TypeSpec ecosystem.
- `.tysh` is short for TypeSharp and avoids TypeScript/F#/C# extension collisions.
- VS Code language id is `typesharp`; source extension is `.tysh`.

## Runnable Catalog

The smoke-tested runnable catalog currently lives in [`examples/runnable/`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable):

- `console-hello`: `typesharp check`, `build`, and `run` workflow.
- `library-public-api`: generated `net48` library with public record/class API.
- `csharp-interop`: TypeSharp consuming a local `net48` C# DLL.
- `host-aspnet-wcf`: ASP.NET Web Forms-style, WCF service, and WCF client/proxy-shaped `net48` host code referencing a generated TypeSharp library plus Core/Runtime DLLs.
- `host-worker`: worker-style `net48` host referencing a generated TypeSharp library plus Core/Runtime DLLs.
- `diagnostics-null-safety`: expected `TS2202` JSON diagnostics workflow.

Additional CLI starter example:

- [`examples/cli-console`](https://github.com/naramdash/TypeSharp/tree/main/examples/cli-console): `TypeSharp.toml`, `src/Main.tysh`, and `typesharp check/build/run` workflow.

## Feature Examples

Feature-oriented single-file examples currently live in [`examples/*.tysh`](https://github.com/naramdash/TypeSharp/tree/main/examples):

| Example | Coverage |
| --- | --- |
| `01-hello-cli.tysh` | CLI entry point, module source file, .NET static import. |
| `02-modules-records.tysh` | import/export, type alias, immutable record, nullable field, record update. |
| `03-unions-patterns.tysh` | nominal closed union, generic union, exhaustive `match`. |
| `04-structural-narrowing.tysh` | structural shape type, type-level union, `unknown` narrowing. |
| `05-async-result-interop.tysh` | `async fun`, `Task<T>`, .NET exception interop, typed `Result<T,E>`. |
| `06-public-api.tysh` | C#-friendly public API, attribute, delegate, event, property. |
| `07-pipeline-collections.tysh` | pipeline, lambda, collection/object expression, local inference. |
| `08-csharp-library-interop.tysh` | C#/.NET Framework library import, member call, named arguments, `out`, `Result` wrapper. |
| `09-literals-attributes.tysh` | compile-time `literal`, public metadata constants, .NET attributes. |
| `10-public-boundary-contract.tysh` | local structural/type-level flexibility normalized into C#-friendly public types. |
| `11-capability-boundaries.tysh` | explicit `dynamic`, `reflect`, and `interop` capability markers. |

## Coverage Map

| Design Contract | Examples |
| --- | --- |
| `.tysh` source and CLI entry point | `01-hello-cli.tysh`, `cli-console` |
| module graph, import/export, immutable records | `02-modules-records.tysh` |
| nominal unions, exhaustive `match`, higher-order functions | `03-unions-patterns.tysh` |
| local type-level unions, structural shapes, `unknown` narrowing | `04-structural-narrowing.tysh`, `10-public-boundary-contract.tysh` |
| `Result<T,E>`, exception interop, `Task<T>` async lowering | `05-async-result-interop.tysh` |
| C# consumer-friendly public ABI, property, delegate, event | `06-public-api.tysh`, `10-public-boundary-contract.tysh` |
| pipeline, lambda, local inference, collection expression | `07-pipeline-collections.tysh` |
| framework assembly import, constructor/member call, named args, `out` | `08-csharp-library-interop.tysh`, `csharp-interop` |
| `literal`, public metadata constant, .NET attribute | `09-literals-attributes.tysh` |
| `dynamic`, `reflect`, `interop` capability boundaries | `11-capability-boundaries.tysh` |

## Authoring Principles

- Public .NET boundaries use nominal types.
- Exported/public values prefer explicit types so metadata and module surfaces are stable.
- Files use namespace, imports/open declarations, then declarations.
- Standard library symbols are imported explicitly from `TypeSharp.Core`.
- Function declarations use `fun name(params): ReturnType = expr` or block-bodied `fun name(params): ReturnType { ... }`.
- Function values use `let name = params => expr` or explicit function type annotation.
- Bindings use `let`, `let mut`, or `literal` for compile-time constants.
- `literal` is limited to primitive/string/enum/null values or compiler-proven constant expressions.
- Type-level unions and structural shapes are local compile-time tools.
- Public APIs do not directly expose type-level unions, structural shapes, anonymous objects, or inferred anonymous function types.
- Public .NET boundary values close over record/class/interface/union/delegate/framework nominal surfaces.
- Interop capability boundaries show `interop`, `dynamic`, `reflect`, or `unsafe` markers explicitly.
- Example CLI behavior follows [CLI](../cli/); syntax and feature status follow [Grammar](../grammar/) and [Feature Status](../feature-status/).

## Repository Artifact Location

As of task `0251`, raw example source and runnable projects live outside `docs/` under `examples/`. docs owns the narrative catalog, while the root `examples/` tree remains the smoke-tested source/project artifact area.
