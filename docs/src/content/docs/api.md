---
title: API And CLI Reference
description: TypeSharp CLI, manifest, runtime, generated assembly, and tooling reference.
---

## CLI Commands

Implemented commands:

```text
typesharp version
typesharp new <console|library> <name> [--target net48] [--output <path>]
typesharp check [project]
typesharp build [project]
typesharp run [project] [-- args...]
typesharp format [project-or-path] [--check]
typesharp explain <diagnostic-code>
typesharp lsp
```

See [CLI](../cli/) for command details and canonical behavior.

## Manifest Reference

`TypeSharp.toml` defines project identity and build shape.

```toml
[project]
name = "Sample"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Samples"
generatedOutputRoot = "generated"

[references]
paths = ["lib/Legacy.Tools.dll"]
packages = []

[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

The 1.0 dependency acquisition scope is framework assemblies, explicit local `net48` DLLs, direct TypeSharp project references, and matching TypeSharp Core/Runtime DLLs from the release runtime archive. `references.packages` is reserved for post-1.0 NuGet restore support. The current compiler reports `TS2405` instead of restoring packages; reference a local `net48` DLL through `references.paths`.

`projectReferences.paths` names direct TypeSharp manifests. `typesharp check` validates the direct project graph and exported source members; `typesharp build` builds referenced projects first and consumes their generated assemblies through explicit local references.

Canonical pages: [CLI](../cli/) and [Project Configuration](../project-configuration/).

Human guide: [Project Configuration](../project-configuration/)

## Runtime And Core Libraries

The generated code can depend on:

- `TypeSharp.Core`: `Option<T>`, `Result<T, E>`, and user-facing core helpers.
- `TypeSharp.Runtime`: helpers for generated unions, pattern matching, equality, hash composition, and async.

## Standard Library Surface

The standard library is split by public intent. User-facing names stay small and explicit; compiler-generated implementation helpers stay behind the runtime boundary.

| Namespace | Role |
| --- | --- |
| `TypeSharp.Core` | `Option<T>`, `Result<T,E>`, `Unit`, and small user-facing functional helpers. |
| `TypeSharp.Collections` | Collection helpers, sequence helpers, and future immutable/read-only collection adapters. |
| `TypeSharp.Runtime` | Low-level helpers used by compiler-generated code. |
| `TypeSharp.Interop` | Explicit helpers for reflection, dynamic, COM/P/Invoke, nullable interop, exception/result adaptation, and C# consumer helpers. |

Rules:

- User examples and source files should import standard library symbols explicitly.
- There is no large hidden prelude in the MVP contract.
- The project manifest may eventually auto-reference `TypeSharp.Core`, but name import should stay explicit unless a later versioned policy changes it.
- `TypeSharp.Core` must remain small, dependency-free, `net48`-compatible, and stable for C# consumers.
- `TypeSharp.Runtime` helpers may appear in generated implementation code, but should not appear directly in user public APIs.
- Public helper stability and generated helper compatibility follow the runtime ABI policy in [.NET Interop](../dotnet-interop/).

Current Core public ABI:

```csharp
namespace TypeSharp.Core
{
    public abstract class Option<T> { }
    public abstract class Result<T, E> { }
    public struct Unit { }
}
```

`Option<T>` models possible absence. `Result<T,E>` models success or failure. `Unit` is the value representation used when `unit` must exist in value or generic position; return-position `unit` lowers to C# `void`.

C# consumers construct values through the current static factories:

```csharp
var some = TypeSharp.Core.Option<string>.Some("value");
var ok = TypeSharp.Core.Result<int, string>.Ok(42);
```

Direct TypeSharp source imports for ergonomic case constructors such as `Some`, `None`, `Ok`, and `Error` are not part of the current release ABI. Treat them as future source-language ergonomics unless a later versioned policy adds explicit importable symbols.

Standard library imports should be explicit:

```tysh
import { Result } from "TypeSharp.Core"
import { Option } from "TypeSharp.Core"
```

Initial collection helper policy:

- arrays and list-like literals are language syntax,
- reusable collection helpers belong under `TypeSharp.Collections`,
- helper signatures that accept `T -> U` must lower to public `Func<T,U>`-compatible metadata when exposed to C#,
- immutable collection dependencies require a separate `net48` dependency and license review.

Runtime helper policy:

- generated nominal union case classes may implement `TypeSharp.Runtime.ITypeSharpUnionCase`,
- `TypeSharpUnion` can provide case tag, case name, payload, equality, and hash helpers,
- `TypeSharpPattern` can provide generated pattern matching helpers,
- `TypeSharpEquality` can provide generated record and union equality/hash helpers,
- `TypeSharpAsync` can provide package-free `Task` creation helpers for generated async lowering.

Interop helper policy:

- `TypeSharp.Interop` helpers require explicit import,
- helpers that cross `dynamic`, `reflect`, `interop`, or `unsafe` boundaries follow the same capability diagnostics as source calls,
- user-facing interop helpers and compiler-internal runtime helpers stay separate.

Canonical pages:

- [.NET Interop](../dotnet-interop/)
- [Project Requirements](../requirements/)
- [Advanced Topics](../advanced/)
- [Type System](../type-system/)

Implementation notes:

- [`lang/TypeSharp.Core/README.md`](https://github.com/naramdash/TypeSharp/blob/main/lang/TypeSharp.Core/README.md)
- [`lang/TypeSharp.Runtime/README.md`](https://github.com/naramdash/TypeSharp/blob/main/lang/TypeSharp.Runtime/README.md)

## Generated Assembly Layout

`typesharp build` emits generated source and a generated C# project under the configured generated output root. Build output follows:

```text
generated/
  src/*.g.cs
  <ProjectName>.Generated.csproj
  bin/<Configuration>/net48/<ProjectName>.dll
```

Executable projects produce `.exe` instead of `.dll`.

## VS Code Extension

The VS Code extension lives in `vscode/typesharp` and starts the language server over stdio. It currently covers diagnostics, hover, go-to-definition, completion, and formatting smokes.

See [VS Code And LSP](../vscode-lsp/) for commands and package-shape checks.

## Language Reference Entry Points

- [Modules And Imports](../modules/) for source module paths, imports, exports, and generated containers.
- [Type System](../type-system/) for inference, null safety, `unknown`, `dynamic`, structural shapes, intersection aliases, unions, and public ABI boundaries.
- [Grammar And Language Reference](../reference/) for syntax coverage and implemented feature evidence.
