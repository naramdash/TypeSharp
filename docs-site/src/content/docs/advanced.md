---
title: Advanced Topics
description: Implementation-aware TypeSharp documentation for lowering, public ABI, metadata, diagnostics, and tooling.
---

This page is for readers who need to reason about TypeSharp's compiler and platform contracts. It summarizes the advanced topics and links them to the user-facing behavior documented elsewhere.

## Compiler Pipeline

The stable workflow is:

1. Load `TypeSharp.toml`.
2. Discover `.tysh` source files.
3. Parse syntax trees with source spans.
4. Bind names and declarations.
5. Read referenced C# metadata.
6. Run type checking and interop validation.
7. Run lowering passes.
8. Emit generated C# source.
9. Build a generated `net48` C# project.

`typesharp check` stops before emission. `typesharp build` runs the same diagnostics path before generated output is written.

## Public ABI Contract

TypeSharp public API must be understandable to C# and CLR metadata consumers. That rule drives several design choices:

- Public declarations should be explicit.
- Compile-time-only structural shapes cannot leak into public signatures.
- Type-level unions must be represented through nominal public alternatives.
- Nullability contracts should be enforced before generated C# is emitted.
- Runtime helper types must remain stable enough for generated assemblies and C# consumers.

Use [Runtime ABI](https://github.com/naramdash/TypeSharp/blob/main/docs/runtime-abi.md) for the source-of-truth policy.

## Lowering

Lowering turns TypeSharp syntax and semantic constructs into C# 7.3-compatible shapes. Implemented examples include:

- functions and modules,
- records and public declarations,
- partial declarations for supported generated types,
- nominal unions and match expressions,
- type-level union narrowing,
- structural shape checks,
- async `Task` interop,
- collection expressions,
- pipeline expressions,
- indexer expressions,
- record expression construction,
- C# imports and calls.

Use [lowering.md](https://github.com/naramdash/TypeSharp/blob/main/docs/lowering.md) for concrete source-to-C# examples and fixture links.

## Metadata Reader And Interop Validation

The metadata reader indexes selected public metadata from framework and local `net48` C# assemblies. Interop validation uses that metadata to catch missing references, invalid byref calls, ambiguous overloads, unsupported package references, and unknown nullability.

Current scope is intentionally narrower than full CLR metadata:

- public top-level types and selected members are indexed,
- local DLL references are supported,
- NuGet package restore is not implemented,
- full C# overload resolution is not implemented,
- nested/private/internal metadata is not the stable target.

## Diagnostics

Diagnostics are part of the language contract, not just compiler errors. They are used by:

- CLI text output,
- CLI JSON output,
- `typesharp explain`,
- VS Code/LSP diagnostics,
- build no-emission gates,
- regression fixtures.

Good diagnostics should identify the code, explain the contract, and point toward the next action.

## VS Code And LSP

The VS Code extension and language server share compiler diagnostics. The current tooling path covers syntax highlighting, diagnostics, hover, go-to-definition, completion, and formatting.

Use [VS Code And LSP](../vscode-lsp/) for commands that verify the extension and language-server contract.

## Regression Strategy

Feature work should usually include one or more of:

- parser fixtures,
- type-checker positive and negative fixtures,
- generated C# golden snapshots,
- CLI smoke tests,
- runtime/core behavior tests,
- C# interop build smokes,
- runnable example updates,
- docs-site contract checks.

The wider the user-facing behavior, the more important it is to include both success and failure coverage.

## Feature Maturity

Do not infer support from a design note alone. A feature becomes user-facing only when implementation, docs, and verification align.

| Status | Meaning |
| --- | --- |
| Implemented | Backed by code and tests. |
| Smoke-tested | Verified by CLI, generated C#, interop, or runnable examples. |
| Documented design | Intent is recorded, but implementation may not exist. |
| Stable backlog | Accepted direction, not a current guarantee. |
| Preview watch | Tracked because related languages or .NET are moving. |
| Rejected | Intentionally not part of the current language direction. |

Use task packets and traceability docs when deciding whether a feature is safe to rely on.
