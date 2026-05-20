---
title: .NET Interop
description: How TypeSharp fits into .NET Framework 4.8, C# libraries, generated assemblies, and host projects.
---

This is the canonical docs-site ledger for TypeSharp's C#/.NET Framework interop, public ABI, host compatibility, and runtime ABI policy.

TypeSharp's interop model is conservative: generated output should behave like ordinary `.NET Framework 4.8` assemblies that C# projects can reference. TypeScript-style structural and type-level features may help local TypeSharp checking, but they must not leak through public CLR metadata boundaries.

Transition bridges:

- [`docs/csharp-interop.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/csharp-interop.md)
- [`docs/runtime-abi.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/runtime-abi.md)

## Goals

TypeSharp interop must support these durable goals:

- reference existing `net48` C# framework assemblies and local DLLs from a TypeSharp project,
- expose TypeSharp library assemblies to C# .NET Framework consumers,
- load generated TypeSharp assemblies and `TypeSharp.Core`/`TypeSharp.Runtime` in ASP.NET Web Forms/MVC/Web API, WCF, Windows Service, scheduled job, and worker-style hosts,
- model C# classes, interfaces, structs, enums, delegates, events, attributes, generic types, and async `Task` APIs inside the TypeSharp type system,
- require explicit capability markers for unstable boundaries such as `dynamic`, reflection, COM, P/Invoke, and unsafe interop.

## Generated Target

The current backend emits C# 7.3-compatible source and builds generated projects for `net48`.

Generated output includes:

- generated `.g.cs` source files,
- a generated C# project,
- generated `bin` and `obj` outputs,
- a generated `.dll` or `.exe` depending on `outputType`.

Generated artifacts should stay under the configured generated output root and should not be committed.

## Reference Model

`TypeSharp.toml` describes the external .NET metadata graph through `[references]`.

```toml
[references]
assemblies = [
  "System",
  "System.Core",
  "System.Xml"
]

paths = [
  "lib/Legacy.Billing.dll"
]

packages = [
  "Newtonsoft.Json:13.0.3"
]
```

Rules:

- `assemblies` names .NET Framework reference assemblies or framework/GAC assemblies.
- `paths` names explicit repository or build-environment DLL paths compatible with `net48`.
- The metadata reader indexes available `net48` framework reference assemblies and local DLL public top-level types plus selected public members.
- Missing framework/local metadata is reported before generated C# emission when the compiler has enough metadata context. Examples include `TS2408` for missing framework types, `TS2407` for missing framework static methods, and `TS2409` for missing framework static members.
- `packages` is a reserved manifest surface. The current compiler does not restore NuGet packages and reports `TS2405`.
- Future package support must include `net48` asset selection, transitive dependencies, license inventory, checksums or lock files, and deterministic restore policy.
- If multiple references expose the same simple assembly or type name, the manifest must define precedence or the compiler must report an ambiguity diagnostic.

Add local DLL references in `TypeSharp.toml`:

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

Then import public C# types by namespace:

```text
import { LegacyFormatter } from "Legacy.Tools"
```

The current metadata reader indexes public top-level types and selected member metadata from local `net48` assemblies and available `net48` framework reference assemblies.

## Import Model

TypeSharp `import` handles source modules and .NET namespace/type imports through one syntax surface, then separates them during resolution.

```text
namespace Samples.CSharpInterop

import { Console, DateTime, StringComparer } from "System"
import { File } from "System.IO"
import { Regex } from "System.Text.RegularExpressions"
import { StringBuilder } from "System.Text"
import type { Task } from "System.Threading.Tasks"
import static System.Math
```

Rules:

- A string-literal module specifier that matches a .NET namespace performs metadata namespace lookup.
- `import type` binds only type-space names and creates no runtime import.
- `import static T` adds static member candidates, equivalent to C# `using static`.
- `open System` is allowed for root-level namespace imports, but explicit imports are preferred because ambiguity diagnostics are clearer.
- Missing external assemblies, namespaces, or public types are reported as reference or import diagnostics.

## Supported C# Interop Shape

Smoke-tested interop currently covers:

- public class and interface references, including metadata-proven class-to-interface/base assignments,
- constructors,
- static and instance methods,
- instance member calls on imported C#-typed parameters, local aliases, and assignment-updated locals,
- properties and fields,
- indexers,
- delegates and delegate lambdas,
- event add/remove calls,
- attributes,
- extension method metadata markers, imported receiver instance-call syntax, and TypeSharp-authored explicit-receiver extension method lowering,
- generic type references and imported generic type constructor calls,
- generic method call sites that C# can infer or that use explicit type arguments,
- `ref`, `out`, `in`, `params`, optional, and named argument validation for selected local metadata cases.

Unsupported package references, missing named type imports from known local/framework C# metadata namespaces, missing static method calls or static members on known local/framework C# metadata types, no-matching imported constructor arguments including generic constructor type-argument substitution mismatches, ambiguous imported constructor candidates, missing instance members on metadata-constructed local values, imported C#-typed parameters, local aliases, or assignment-updated locals, known literal, `null`, imported metadata argument type, delegate lambda arity, known delegate lambda return, identity lambda parameter return, metadata-backed lambda member-chain return, metadata-backed lambda method-call return, metadata-backed lambda extension method-call return, metadata-backed lambda static method-call return, or metadata-backed lambda binary predicate return overload mismatches, no-matching extension method overloads on metadata-tracked receivers, missing instance indexers, known indexer argument mismatches, impossible numeric literal indexer conversions, or ambiguous known imported indexer candidates on metadata-constructed local values, missing property setters for metadata-constructed local values, imported C#-typed parameters, annotated locals, or assignment-updated locals, readonly field assignment on metadata-constructed local values, missing static property setters, readonly static field assignment, missing instance event add/remove targets, and unsatisfied explicit or inferred C# generic method constraints, including constructed generic argument cases like `LegacyBox<!!0>`, are reported as diagnostics instead of falling through to generated C# build failures. Known imported indexer candidates rank exact argument matches and metadata relationship distance ahead of `object` fallback. Applicable C# extension methods are accepted for metadata-tracked imported receivers, including `object` receiver fallback, and extension receiver overloads rank closer metadata relationships ahead of farther interface or `object` fallback candidates while generated C# preserves normal extension-call syntax.

## Type Mapping

| TypeSharp | C#/.NET Metadata | Rule |
| --- | --- | --- |
| `bool` | `System.Boolean` | Primitive alias. |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | CLR integral types | Used for numeric overload ranking; expression-local overflow can be explicit with `checked(...)` or `unchecked(...)`. |
| `float`, `double`, `decimal` | `System.Single`, `System.Double`, `System.Decimal` | Used for numeric overload ranking. |
| `char`, `string`, `object` | `System.Char`, `System.String`, `System.Object` | `string` is reference-like and non-null by default in TypeSharp. |
| `unit` return | `void` | Lowered to C# `void` in return position. |
| `unit` value/generic position | `TypeSharp.Core.Unit` | Used when unit must be represented as a value. |
| `T?` value type | `System.Nullable<T>` | Applies when `T: struct`. |
| `T?` reference type | nullable reference contract plus runtime guard | Unknown imported nullability produces diagnostics or warnings in strict positions. |
| `T[]` | CLR array | Direct C# array interop. |
| `List<T>`, `Dictionary<K,V>` | BCL generic collections | Require import or fully qualified resolution. |
| `A -> B` public type | `System.Func<A,B>` or `System.Action<A>` | If the shape cannot be represented naturally, require an explicit delegate. |
| `delegate` | CLR delegate type | Preferred for events, callbacks, and public C# boundaries. |
| `Task<T>` | `System.Threading.Tasks.Task<T>` | Recommended for public `async fun` returns. |
| `Option<T>`, `Result<T,E>` | `TypeSharp.Core` nominal unions | Helper APIs must be stable for C# consumers. |
| type-level union, structural shape | compile-time only | Rejected at public CLR ABI boundaries. |

## Calling C# APIs

Imported C# calls preserve C#-predictable semantics:

- `TypeName(args)` and imported generic `TypeName<T>(args)` are constructor calls.
- `TypeName.Member` is static lookup; `value.Member` is instance lookup.
- Metadata-backed locals, parameters, aliases, annotated locals, and simple assignments preserve enough receiver metadata for member validation.
- Public property get/set, field read/write, indexers, events, extension methods, delegates, optional/named/`params` parameters, `ref`/`out`/`in`, generic methods, and generic constraints are validated for the implemented metadata subset before emission.
- Missing setters, readonly field assignment, missing events, missing indexers, no-matching overloads, ambiguous overloads, impossible numeric literal conversions, invalid generic constraints, and invalid byref arguments are diagnostics before generated C# build.

Overload ranking is nominal-first and intentionally narrower than the full C# compiler:

1. arity and named argument match,
2. required/optional/`params` parameter match,
3. exact nominal type match,
4. nullable compatibility,
5. numeric conversion,
6. generic inference and constraint satisfaction,
7. delegate/lambda contextual typing for the implemented subset,
8. explicit conversion,
9. structural proof or type-level union narrowing,
10. explicit `dynamic` boundary.

If inference or conversion leaves more than one stable candidate, TypeSharp reports an ambiguity diagnostic instead of relying on generated C# compilation.

## Nullability And Overloads

Legacy C# assemblies often do not contain precise nullable metadata. In strict mode, unknown nullability from imported C# APIs is reported with `TS2404` so the user can decide whether to wrap, annotate, or isolate that boundary.

The overload validator handles the implemented subset of exact primitive/literal matching, known literal, `null`, imported metadata argument type applicability filtering/ranking, delegate lambda arity filtering for known `System.Func`/`System.Action` targets, known lambda body return, identity lambda parameter return, metadata-backed lambda member-chain return filtering/ranking, metadata-backed lambda method-call return filtering/ranking, metadata-backed lambda extension method-call return filtering/ranking, metadata-backed lambda static method-call return filtering/ranking, and comparison/logical binary predicate return filtering/ranking for `System.Func` targets, impossible numeric literal conversion rejection, integral constant conversion for fitting smaller numeric parameters, explicit generic method arity filtering, local/framework explicit and inferred generic method constraint validation, transitive base/interface metadata checks for generic constraints, extension method receiver relationship ranking, `object` receiver fallback, and no-matching argument diagnostics, named arguments, optional parameters, `params`, and byref modifiers. Inferred generic method constraints use direct generic parameter positions and explicit constructed generic argument positions such as `LegacyBox<!!0>` receiving `LegacyBox<LegacyNamed>(...)`. Constructor calls use the same named, optional, `params`, and ambiguity checks for imported metadata candidates. Known lambda return conversions rank exact delegate return targets ahead of numeric widening targets when both are applicable; member access bodies such as `item => item.Owner.Name` use public instance property/field metadata from the delegate parameter type, method-call bodies such as `item => item.Owner.Display()` use public instance method metadata from the resolved receiver type, extension method-call bodies such as `item => item.Describe()` use public extension method metadata from imported/opened extension namespaces, static method-call bodies such as `item => LegacyOverloads.Describe(item)` use imported static method metadata when lambda parameters or their member chains are metadata-backed, and binary predicate bodies such as `item => item.Name == "Ada"` are treated as `bool`. `null` rejects non-nullable value-type parameters, ranks concrete reference/nullable overloads ahead of `object` fallback, and ranks nearer metadata-related reference targets ahead of farther base/interface candidates while unrelated reference targets remain ambiguous. Imported metadata arguments rank inherited/interface overloads by metadata relationship distance, so closer base-class candidates can outrank farther interface or `object` fallback candidates. Known local/framework metadata types with missing public static method names report `TS2407`; known method or constructor calls with no applicable candidate report `TS2406`; ambiguous method or constructor calls report `TS2402`; explicit or inferred generic method type arguments that violate imported metadata constraints report `TS2417`. Full C# overload conversion and richer contextual ranking are intentionally out of the current stable scope.

## Capability Boundaries

Unstable C#/.NET interop surfaces require explicit function capability markers:

- `dynamic` type annotations without a `dynamic fun` boundary report `TS2206`.
- Calling a `dynamic fun` from a non-`dynamic` function reports `TS2207`.
- Calling `reflect`, `interop`, or `unsafe` functions without the matching marker reports `TS2208`.
- Reflection, COM, P/Invoke, unsafe code, and dynamic dispatch are not treated as ordinary pure TypeSharp calls in strict mode.

## Exposing TypeSharp APIs To C#

Public TypeSharp APIs must lower to CLR-visible metadata. Prefer these shapes:

- records for public data,
- classes and interfaces for object contracts,
- delegates for callbacks,
- nominal unions for closed alternatives,
- `Option<T>` and `Result<T, E>` for explicit absence and failure,
- `Task` and `Task<T>` for async APIs.

Avoid exposing these compile-time-only shapes directly:

- structural shape and intersection aliases,
- type-level union aliases,
- anonymous or inferred public boundaries.

If C# callers need the concept, wrap it in a named public type.

## Runtime Dependencies

Generated assemblies can reference:

- `TypeSharp.Core.dll` for core public helper types such as `Option<T>` and `Result<T, E>`,
- `TypeSharp.Runtime.dll` for lowering/runtime helper behavior.

Host projects that consume generated assemblies must deploy the generated assembly and required TypeSharp runtime/core assemblies together.

## Runtime ABI Policy

The runtime ABI governs `TypeSharp.Core`, `TypeSharp.Runtime`, generated `net48` assemblies, and C# consumers that reference those assemblies.

Current ABI fields:

- `TypeSharp.Compiler.TypeSharpCompilerInfo.RuntimeAbiVersion = 0`
- `TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion = 0`

Rules:

- The compiler and runtime `RuntimeAbiVersion` values must stay aligned.
- CLI `version --json` and text output must show the runtime ABI targeted by the compiler.
- A generated assembly must reference `TypeSharp.Core` and `TypeSharp.Runtime` with the same major runtime ABI.
- ABI `0` is the pre-1.0 preview ABI. Breaking changes are allowed only when the task packet and traceability records name the affected public surface.

ABI-covered surface:

- `TypeSharp.Core.Option<T>`, `Result<T,E>`, and `Unit`,
- `TypeSharp.Runtime.TypeSharpRuntimeInfo`,
- generated-code helper APIs in `TypeSharp.Runtime`,
- generated public member signatures and metadata shape,
- generated assembly target framework and runtime/core reference shape.

Out of ABI scope:

- compiler internal syntax trees, binder, and type-checker implementation details,
- CLI internal command implementation,
- test fixture helper code,
- generated `.g.cs` formatting when public metadata is unchanged.

The runtime ABI version must change when:

- a public type, member, constructor, field, property, or nested type is removed or renamed from `TypeSharp.Core` or `TypeSharp.Runtime`,
- a public member signature changes in a binary-incompatible way,
- generated public metadata shape changes for the same TypeSharp source,
- generated code requires a runtime helper not provided by older runtime assemblies,
- generated `net48` assembly references change in a way that forces host deployment changes.

The runtime ABI version does not need to change for:

- adding a public helper unused by older generated code,
- fixing helper behavior without signature or metadata-shape changes,
- improving diagnostics, parsing, or CLI output unrelated to generated public metadata,
- changing generated implementation details when public metadata remains compatible.

Before closing a public ABI change:

- verify compiler/runtime ABI version alignment,
- build `src/TypeSharp.Core/TypeSharp.Core.csproj`,
- build `src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`,
- run generated `net48` assembly smoke tests,
- run C# `net48` consumer smokes when public surface is observable from C#,
- run ASP.NET/WCF/worker host compatibility smokes when runtime/core reference shape changes.

## Host Project Compatibility

Runnable smoke examples cover these host shapes:

- ASP.NET Web Forms-style / WCF-style `net48` host reference,
- WCF `ClientBase<T>` proxy-shaped consumption,
- worker-style `net48` host reference,
- C# class library consumption of TypeSharp public APIs.

The current scope verifies reference shape and build compatibility. IIS deployment packaging, Windows Service installer scaffolding, NuGet packaging, and automatic host template generation remain future work.

Host rules:

- Generated TypeSharp libraries should be referenced like ordinary `net48` C# class libraries.
- `TypeSharp.Runtime` must not require a host-specific loader, startup hook, or nonstandard `web.config`/IIS/AppDomain lifecycle behavior by default.
- WCF interop should rely on CLR-visible interfaces, classes, and attributes for service contracts, data contracts, message contracts, proxy generation, and client consumption.
- Worker/service interop must not add ASP.NET, WCF, or Windows Service framework packages as required runtime dependencies.
- Host templates, IIS packaging, WCF config generation beyond hand-authored runnable examples, and Windows Service installer scaffolding remain Stable Backlog.

## Executables And Antivirus

`typesharp run` builds and launches generated `.exe` files for executable projects. Some local antivirus tools can block newly generated executables. When that happens, use `typesharp check` and `typesharp build` to verify the compiler path, then inspect local security policy before trusting or launching the generated executable.

Do not commit generated `.exe` or `.dll` files unless a task explicitly requires a checked-in binary fixture.

## Interop Smoke Tests

Minimum coverage for interop-facing changes:

1. TypeSharp calls framework APIs such as `System`, `System.Core`, `System.IO`, `System.Text`, and `System.Text.RegularExpressions`.
2. TypeSharp references a local `net48` C# class library DLL, preserves interface types in public signatures, and calls generic methods, events, and delegates.
3. A C# `net48` consumer references a generated TypeSharp library and calls its public API.
4. Public boundary diagnostics reject structural shapes, intersection aliases, type-level unions, unknown nullable escapes, and dynamic escapes where required.
5. ASP.NET Web Forms-style, WCF service/client, and worker-style `net48` hosts reference generated TypeSharp/Core/Runtime DLLs without TypeSharp-specific startup hooks.

## Recommended Adoption Path

1. Start with a library project, not the application entry point.
2. Keep public APIs explicit and nominal.
3. Reference the generated library from a small C# smoke project.
4. Add C# local DLL references only when the boundary is understood.
5. Move host integration last, after `check`, `build`, and C# consumption work.
