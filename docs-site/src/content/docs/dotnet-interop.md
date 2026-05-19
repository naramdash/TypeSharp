---
title: .NET Interop
description: How TypeSharp fits into .NET Framework 4.8, C# libraries, generated assemblies, and host projects.
---

TypeSharp's interop model is conservative: generated output should behave like ordinary `.NET Framework 4.8` assemblies that C# projects can reference.

## Generated Target

The current backend emits C# 7.3-compatible source and builds generated projects for `net48`.

Generated output includes:

- generated `.g.cs` source files,
- a generated C# project,
- generated `bin` and `obj` outputs,
- a generated `.dll` or `.exe` depending on `outputType`.

Generated artifacts should stay under the configured generated output root and should not be committed.

## Referencing A Local C# DLL

Add local DLL references in `TypeSharp.toml`:

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

Then import public C# types by namespace:

```text
import { LegacyFormatter } from "Legacy.Tools"
```

The current local metadata reader indexes public top-level types and selected member metadata from local `net48` assemblies.

## Supported C# Interop Shape

Smoke-tested interop currently covers:

- public class and interface references,
- constructors,
- static and instance methods,
- properties and fields,
- indexers,
- delegates and delegate lambdas,
- event add/remove calls,
- attributes,
- generic type references,
- generic method call sites that C# can infer,
- `ref`, `out`, `in`, `params`, optional, and named argument validation for selected local metadata cases.

Unsupported package references are reported as diagnostics instead of silently restoring packages.

## Nullability And Overloads

Legacy C# assemblies often do not contain precise nullable metadata. In strict mode, unknown nullability from imported C# APIs is reported with `TS2404` so the user can decide whether to wrap, annotate, or isolate that boundary.

The overload validator handles the implemented subset of exact primitive/literal matching, named arguments, optional parameters, `params`, and byref modifiers. Full C# overload resolution is intentionally out of the current stable scope.

## Exposing TypeSharp APIs To C#

Public TypeSharp APIs must lower to CLR-visible metadata. Prefer these shapes:

- records for public data,
- classes and interfaces for object contracts,
- delegates for callbacks,
- nominal unions for closed alternatives,
- `Option<T>` and `Result<T, E>` for explicit absence and failure,
- `Task` and `Task<T>` for async APIs.

Avoid exposing these compile-time-only shapes directly:

- structural shape aliases,
- type-level union aliases,
- anonymous or inferred public boundaries.

If C# callers need the concept, wrap it in a named public type.

## Runtime Dependencies

Generated assemblies can reference:

- `TypeSharp.Core.dll` for core public helper types such as `Option<T>` and `Result<T, E>`,
- `TypeSharp.Runtime.dll` for lowering/runtime helper behavior.

Host projects that consume generated assemblies must deploy the generated assembly and required TypeSharp runtime/core assemblies together.

## Host Project Compatibility

Runnable smoke examples cover these host shapes:

- ASP.NET Web Forms-style / WCF-style `net48` host reference,
- WCF `ClientBase<T>` proxy-shaped consumption,
- worker-style `net48` host reference,
- C# class library consumption of TypeSharp public APIs.

The current scope verifies reference shape and build compatibility. IIS deployment packaging, Windows Service installer scaffolding, NuGet packaging, and automatic host template generation remain future work.

## Executables And Antivirus

`typesharp run` builds and launches generated `.exe` files for executable projects. Some local antivirus tools can block newly generated executables. When that happens, use `typesharp check` and `typesharp build` to verify the compiler path, then inspect local security policy before trusting or launching the generated executable.

Do not commit generated `.exe` or `.dll` files unless a task explicitly requires a checked-in binary fixture.

## Recommended Adoption Path

1. Start with a library project, not the application entry point.
2. Keep public APIs explicit and nominal.
3. Reference the generated library from a small C# smoke project.
4. Add C# local DLL references only when the boundary is understood.
5. Move host integration last, after `check`, `build`, and C# consumption work.
