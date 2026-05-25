---
title: Learning Paths
description: TypeSharp study paths for beginners, C# maintainers, TypeScript users, F# users, and advanced implementers.
---

Use this page when you are trying to decide what to read next. Each path moves from concrete commands to language features, then to reference material.

Before choosing a path, install the release CLI from [Install](../install/): open the tag-specific GitHub Release notes, confirm the exact asset names, download `typesharp-cli-dotnet-<tag>.zip`, verify it with `SHA256SUMS.txt`, and run `typesharp version`. Use the matching `typesharp-runtime-net48-<tag>.zip` and verify it with the same manifest when a path needs TypeSharp Core/Runtime DLLs, and install `typesharp-vscode-<tag>.vsix` from [VS Code And LSP](../vscode-lsp/) if you want editor integration.

## Programming Beginner

Goal: build and understand a small TypeSharp project without learning compiler internals first.

1. Install the release CLI and verify `typesharp version` on [Install](../install/).
2. Read [Start Here](../start-here/) to understand what TypeSharp is for.
3. Read [Language Tour](../language-tour/) through values, functions, records, and diagnostics.
4. Run the [Hello Project](../tutorials/#1-hello-project) tutorial.
5. Use [Fundamentals](../fundamentals/) when a tutorial uses a new language term.
6. Use [Troubleshooting](../troubleshooting/) when `check`, `build`, or `run` fails.

Focus on these concepts first:

- A `.tysh` file contains declarations.
- `let` creates a value.
- `fun` creates a function.
- Public functions should usually have explicit parameter and return types.
- `typesharp check` reports problems before generated C# is emitted.

## C# And .NET Framework Maintainer

Goal: add TypeSharp code to an existing `net48` environment without changing the host application model.

1. Read [.NET Interop](../dotnet-interop/) for generated assembly shape, runtime dependencies, and host compatibility.
2. Run [Library Public API](../tutorials/#2-library-public-api).
3. Run [C# Interop](../tutorials/#3-c-interop).
4. Use [Guides](../guides/) for manifest and CLI workflow details.
5. Use [Project Configuration](../project-configuration/) and [API And CLI Reference](../api/) for command, manifest, and generated output details.

Important boundaries:

- TypeSharp currently emits C# 7.3-compatible generated source for `net48`.
- Public TypeSharp APIs must become CLR-visible metadata that C# can understand.
- Structural shapes, intersection aliases, and type-level unions are compile-time-only and cannot be exposed directly as public .NET signatures.
- `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` must be deployed with generated assemblies when their public helpers are used.

## TypeScript User

Goal: understand where TypeSharp is intentionally similar to TypeScript and where .NET metadata changes the rules.

1. Read [Language Tour](../language-tour/) for type inference, structural shapes, type-level unions, narrowing, and public boundary rules.
2. Read [Type System](../type-system/) for `unknown`, `dynamic`, structural shapes, type-level unions, and public CLR boundaries.
3. Read [Modules And Imports](../modules/) for TypeScript-style source files and relative module paths.
4. Read [Diagnostics](../diagnostics/) for `TS2201`, `TS2202`, `TS2204`, and the narrower assignability diagnostics `TS2228`, `TS2229`, and `TS2230`.
5. Read [Grammar And Language Reference](../reference/) when you need formal syntax boundaries.
6. Use [Cookbook](../cookbook/) for common interop workflows.

Translate expectations carefully:

- TypeSharp can use structural ideas inside compilation, but public .NET API shape is nominal.
- Type-level unions and intersection aliases are useful for local checking, but public boundaries need records, classes, interfaces, or nominal unions.
- Module and namespace choices affect generated C# names.

## F# Or Functional Programming User

Goal: use immutable data, options/results, unions, pattern matching, pipelines, and async while staying compatible with .NET Framework hosts.

1. Read [Language Tour](../language-tour/) from `Option`, `Result`, and unions onward.
2. Read [Fundamentals](../fundamentals/) for expression-oriented features and current inference scope.
3. Read [Type System](../type-system/) for nominal unions, pattern matching boundaries, generics, and async.
4. Read [Grammar And Language Reference](../reference/) for patterns, type syntax, and declarations.
5. Use [Advanced Topics](../advanced/) to understand lowering and stability boundaries.

Keep in mind:

- TypeSharp borrows expression-oriented and functional ideas, but it is not F#.
- Public API design is constrained by C# and CLR metadata.
- Async lowers to .NET `Task` or `Task<T>`.

## Advanced Implementer

Goal: evaluate TypeSharp as a language, compiler, tooling, or hosting technology.

1. Read [Core Goal](../goal/) to understand the project constraints.
2. Read [Advanced Topics](../advanced/) for compiler pipeline, lowering, metadata, diagnostics, LSP, and regression strategy.
3. Read [Grammar And Language Reference](../reference/) and [Grammar](../grammar/) for syntax coverage.
4. Read [API And CLI Reference](../api/) and [CLI](../cli/) for tooling contracts.
5. Follow docs canonical pages and test evidence when you need implementation detail.

Good first questions:

- Does the feature lower to C# 7.3-compatible `net48` source?
- Does it expose public CLR metadata safely?
- Does it have diagnostics that tell users what to fix?
- Does it have positive and negative smoke coverage?

## Lookup

| Need | Start with |
| --- | --- |
| Create a project | [Install](../install/) and [Tutorials](../tutorials/) |
| Understand the language quickly | [Language Tour](../language-tour/) |
| Understand current feature support | [Fundamentals](../fundamentals/) and [Reference](../reference/) |
| Configure a project | [Project Configuration](../project-configuration/) |
| Understand modules and imports | [Modules And Imports](../modules/) |
| Understand types and public boundaries | [Type System](../type-system/) |
| Call a C# DLL | [.NET Interop](../dotnet-interop/) and [Cookbook](../cookbook/) |
| Consume TypeSharp from C# | [.NET Interop](../dotnet-interop/) |
| Fix diagnostics | [Diagnostics](../diagnostics/) and [Troubleshooting](../troubleshooting/) |
| Use VS Code | [VS Code And LSP](../vscode-lsp/) |
| Inspect compiler behavior | [Advanced Topics](../advanced/) |
