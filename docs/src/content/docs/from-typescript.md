---
title: From TypeScript
description: How TypeScript developers should translate familiar concepts into TypeSharp.
---

Start from [Install](../install/) before running examples: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. Use `typesharp runtime-path` when a C# host needs the matching TypeSharp Core/Runtime DLLs bundled with the installed tool.

TypeSharp borrows TypeScript's local type flexibility, but it does not emit JavaScript and it does not run on Node. The output is generated C# 7.3-compatible source and a `.NET Framework 4.8` assembly. That means local TypeSharp checking can feel structural, while public APIs must become CLR-visible nominal metadata.

## Mental Model

| TypeScript Expectation | TypeSharp Rule |
| --- | --- |
| Source files form modules. | `.tysh` files form an explicit source module graph. `namespace` also controls generated C# namespace identity. |
| `import` and `export` describe module boundaries. | TypeSharp imports source modules or .NET namespaces/types through one syntax surface, then resolves them before lowering. |
| Inference keeps local code lightweight. | Local inference is supported, but exported functions and interop boundaries should use explicit types. |
| Structural typing is normal for object shapes. | Structural shapes are useful locally, but public CLR signatures need records, classes, interfaces, delegates, or nominal unions. |
| Union and intersection types can appear in APIs. | Type-level unions and intersection aliases are compile-time-only unless wrapped in nominal public types. |
| `unknown` is safer than `any`. | `unknown` is the safe boundary. `dynamic` needs an explicit capability boundary. |
| `tsconfig.json` owns compiler and runtime assumptions. | `TypeSharp.toml` owns source roots, generated output, references, and `net48` artifact shape. |
| npm packages and declaration files define library shape. | Current TypeSharp interop uses framework assemblies and explicit local DLL paths. NuGet package restore inside the compiler is backlog. |

## Local Structural Checks

Use structural shapes when the proof stays inside TypeSharp source:

```tysh
namespace Samples.FromTypeScript

public record Customer(Name: string, Age: int)

type Named = { Name: string }

fun keepCustomer(customer: Customer): Customer =
  customer satisfies Named
```

`satisfies` checks that `Customer` has the required members, preserves the original type, and erases after checking. This is close to a TypeScript local shape check, but it is not a public .NET contract.

## Public API Boundary

Do not expose structural or type-level-only forms directly:

```tysh
type ApiResult = Customer | string
type Named = { Name: string }
```

Use a named CLR-visible type instead:

```tysh
public union LookupResult {
  Found(Customer)
  Missing(message: string)
}

export fun findCustomer(id: int): LookupResult =
  Missing("not found")
```

C# callers can reference the generated `LookupResult` type because it lowers to nominal CLR metadata. A TypeScript-style `Customer | string` alias cannot cross that boundary directly.

## Modules And Names

TypeScript developers should treat `namespace` as generated .NET identity, not as a replacement for ES modules:

```tysh
namespace Company.Billing

import { LegacyFormatter } from "Legacy.Tools"
```

The import may bind a TypeSharp source export or a .NET metadata type. The namespace becomes visible to C# consumers, so choose stable public names before publishing a generated assembly.

## Common Traps

| Trap | Fix |
| --- | --- |
| Exposing `{ Name: string }` from an exported function. | Define `public record NamedCustomer(Name: string)` or a public interface. |
| Exposing `A | B` as a public return type. | Define a `public union` with named cases. |
| Expecting Node or browser module resolution. | Use TypeSharp source imports, manifest source roots, and explicit .NET references. |
| Expecting npm package restore during `typesharp check`. | Reference local `net48` DLLs explicitly. Package restore support is not stable. |
| Using `dynamic` like TypeScript `any`. | Prefer `unknown` and narrowing. Mark dynamic boundaries explicitly when needed. |
| Assuming generated code is JavaScript-like output. | Generated output is C# source plus a `net48` assembly. Do not edit generated files. |

## First Reading Path

1. [Language Tour](../language-tour/) for inference, records, unions, structural shapes, and diagnostics.
2. [Type System](../type-system/) for `unknown`, `dynamic`, narrowing, compile-time-only types, and public ABI rules.
3. [Modules And Imports](../modules/) for source modules, imports, exports, and generated C# containers.
4. [Cookbook](../cookbook/) for short local interop and public API recipes.
5. [Feature Status](../feature-status/) when you need to know whether a TypeScript-like feature is MVP, backlog, experimental, or rejected.
