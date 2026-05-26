---
title: Advanced Topics
description: Implementation-aware TypeSharp documentation for lowering, public ABI, metadata, diagnostics, and tooling.
---

This page is for readers who need to reason about TypeSharp's compiler and platform contracts. It summarizes the advanced topics and links them to the user-facing behavior documented elsewhere.

Command examples and workflow references assume the tool install route from [Install](../install/): install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. When an advanced workflow needs TypeSharp Core/Runtime DLLs, use `typesharp runtime-path` to locate the matching DLLs bundled with the installed tool.

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

The release-installed `typesharp check` command stops before emission. The release-installed `typesharp build` command runs the same diagnostics path before generated output is written.

Use [Project Policy](../project-policy/) for the full architecture, backend, dependency, target framework, regression, parser fixture, feature review, and release policy ledger.

## Feasibility Boundaries

TypeSharp is feasible because the implementation scope is deliberately constrained around `.NET Framework 4.8` generated outputs and a C# 7.3 source backend.

Required feasibility decisions:

- Generated artifacts and TypeSharp runtime/core libraries target `net48`.
- Compiler, CLI, and language-server hosts may use a modern .NET LTS runtime.
- The MVP backend emits C# 7.3-compatible source and then builds a generated `net48` project.
- Direct IL emission is a future backend, not the first stable emitter.
- Type-level unions and structural shapes are compile-time tools and are rejected at public .NET ABI boundaries.
- Nominal closed unions lower through a reference-type class hierarchy for the MVP representation.
- Advanced TypeScript-style mapped, conditional, template-literal, and high-complexity type computation remains outside the MVP and must stay behind the finite evaluator budget in [Type System](../type-system/).
- Managed `net48` framework and local DLL metadata interop is the initial C# interop scope.
- ASP.NET/WCF/worker compatibility is proven through generated `net48` library ABI and host reference/build smokes, not through host template or deployment automation in the MVP.

Implementation order follows those boundaries: manifest and source discovery, parser, CLI check, metadata reader, binder/type checker, C# source backend, unions/patterns, interop smoke tests, then VS Code diagnostics and navigation.

Use [Project Requirements](../requirements/) for the full requirement ledger and [Feature Status](../feature-status/) for the feature bucket map.

## Public ABI Contract

TypeSharp public API must be understandable to C# and CLR metadata consumers. That rule drives several design choices:

- Public declarations should be explicit.
- Compile-time-only structural shapes and intersection aliases cannot leak into public signatures.
- Type-level unions must be represented through nominal public alternatives.
- Nullability contracts should be enforced before generated C# is emitted.
- Runtime helper types must remain stable enough for generated assemblies and C# consumers.

Use [.NET Interop](../dotnet-interop/) for the canonical public ABI and runtime ABI policy, including ABI version fields, covered surfaces, change rules, and compatibility gates. Use [Project Requirements](../requirements/) and [Feature Status](../feature-status/) for the wider platform and maturity policy.

## Compile-Time Evaluator Budget

Future advanced type operators are compile-time-only features. The evaluator must be deterministic, cached, and bounded before any syntax is promoted from backlog.

The accepted first budget is:

- 16 nested alias instantiations,
- 512 evaluator reductions per root alias,
- 64 normalized union members,
- 64 mapped keys,
- 64 distributive conditional branches,
- 128 generated template-literal strings,
- 8 direct evaluator diagnostics per root alias.

The evaluator must not execute user code, import TypeScript declaration files as a compatibility layer, restore packages, inspect runtime values, or infer through C# overload sets. Public use is allowed only when the computed result fully normalizes to CLR-visible metadata; structural, union, template-generated, or unresolved computed results remain local-only and must diagnose before emission.

Use [Type System](../type-system/) for the canonical evaluator budget and [Diagnostics](../diagnostics/) for planned diagnostic classes.

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

Use [Lowering](../lowering/) for the canonical docs lowering contract, generated shape map, runtime-helper boundaries, and fixture evidence.

## Metadata Reader And Interop Validation

The metadata reader indexes selected public metadata from framework and local `net48` C# assemblies, including base/interface relations and extension method markers. Interop validation uses that metadata to catch missing references, missing imported framework/local types, missing framework/local static methods or static members, missing imported instance members while accepting applicable extension methods with receiver relationship ranking and `object` fallback, invalid byref calls, ambiguous overloads, explicit and simple inferred generic constraint violations including inherited base/interface satisfaction, unsupported package references, and unknown nullability. Manifest-based check/build also uses those metadata relations for imported C# assignment and return compatibility.

Use [.NET Interop](../dotnet-interop/) for the canonical reference model, import model, supported metadata shape, type mapping, overload policy, capability boundaries, host compatibility, and interop smoke policy.

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
- docs contract checks.

The wider the user-facing behavior, the more important it is to include both success and failure coverage.

Use [Project Policy](../project-policy/) for the canonical regression evidence matrix, parser fixture policy, snapshot update rules, and feature review gate.

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

Use [Feature Status](../feature-status/), [Project Policy](../project-policy/), linked docs, and test evidence when deciding whether a feature is safe to rely on.
