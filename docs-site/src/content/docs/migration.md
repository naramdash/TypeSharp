---
title: Migration
description: Adopting TypeSharp from existing .NET Framework and C# projects.
---

This is the canonical docs-site migration guide for adopting TypeSharp from existing `.NET Framework 4.8` and C# projects.

The migration stance is incremental. TypeSharp does not automatically convert an existing C# project. Instead, write new TypeSharp `net48` libraries, reference existing C# assemblies from TypeSharp, and reference generated TypeSharp assemblies from existing C#/.NET Framework applications.

## Fit

TypeSharp is a good fit when:

- keep existing `.NET Framework 4.8` projects and C# assemblies,
- existing ASP.NET Web Forms/MVC/Web API, WCF, Windows Service, scheduled job, or worker-style hosts must keep their deployment model,
- new code benefits from null safety, nominal unions, pattern matching, immutable records, `Option<T>`, `Result<T,E>`, type-level unions, and structural shape checks,
- C# `net48` libraries, framework assemblies, and local DLLs must remain callable,
- C# consumers need to reference generated TypeSharp assemblies like ordinary class libraries.

TypeSharp is not yet a good fit when you need:

- automatic whole-project C# conversion,
- TypeSharp-managed NuGet restore, package locks, and license inventory,
- generated ASP.NET/WCF host templates and config,
- direct IL backend output,
- final release packaging/signing/checksum automation.

## Strategy

1. Put a TypeSharp library project in a separate folder inside the existing C# solution.
2. Start with `targetFramework = "net48"`.
3. Add existing C# DLLs or framework assemblies to `TypeSharp.toml`.
4. Keep public APIs C#-friendly and nominal.
5. Use structural shapes, type-level unions, null safety, and pattern matching inside local TypeSharp code.
6. Run `typesharp check` and fix diagnostics first.
7. Run `typesharp build` to produce a generated `net48` DLL.
8. Reference the generated DLL plus required `TypeSharp.Core`/`TypeSharp.Runtime` DLLs from the C# project.
9. Add the C# consumer build to CI before moving the boundary deeper into the application.

## Minimal Project

`TypeSharp.toml`:

```toml
[project]
name = "Billing.Rules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing.Rules"
sourceRoots = ["src"]
generatedOutputRoot = "generated"

[references]
assemblies = [
  "System",
  "System.Core"
]

paths = [
  "lib/Legacy.Billing.dll"
]
```

`src/Main.tysh`:

```text
namespace Company.Billing.Rules

import { LegacyCalculator } from "Legacy.Billing"
import { Result, Ok } from "TypeSharp.Core"

public union PriceError {
  MissingSku(sku: string)
  InvalidAmount(message: string)
}

public record PriceQuote(Sku: string, Amount: decimal)

export fun quote(sku: string): Result<PriceQuote, PriceError> {
  let amount = LegacyCalculator.Calculate(sku)
  Ok(PriceQuote(sku, amount))
}
```

CLI loop:

```text
typesharp check TypeSharp.toml --diagnostic-format json
typesharp build TypeSharp.toml
```

Use [CLI](../cli/) for command behavior, [Lowering](../lowering/) for generated C# shape, and [.NET Interop](../dotnet-interop/) for public ABI and runtime dependency rules.

## Public API Rules

Recommended public boundary shapes:

- `record` for immutable data crossing into C#,
- nominal `union` for closed domain alternatives,
- `class`, `interface`, and `delegate` for C#-friendly object models,
- `Option<T>` and `Result<T,E>` from `TypeSharp.Core`,
- `Task<T>` for async public APIs,
- explicit parameter and return type annotations on exported functions.

Avoid direct public exposure of:

- type-level union aliases such as `string | int`,
- structural shape aliases such as `{ Name: string }`,
- anonymous object/record expressions,
- marker-free `dynamic`,
- inferred anonymous function types.

Those are TypeSharp compile-time tools and do not have stable CLR metadata representation in the MVP.

## C# Pattern Mapping

| Existing C# Pattern | TypeSharp Replacement | Notes |
| --- | --- | --- |
| DTO class with readonly properties | `record` | Lowers to immutable C# class with constructor and get-only properties. |
| Nullable return for absence | `Option<T>` or `T?` | Prefer `Option<T>` for domain absence and `T?` for interop-compatible nullability. |
| Exception for expected domain failure | `Result<T,E>` | Keep C# exception interop for external APIs; model new domain errors as values. |
| enum plus payload class hierarchy | nominal `union` | Lowers to abstract base class and sealed case classes. |
| `switch` over cases | `match` | Type checker reports non-exhaustive union matches. |
| ad hoc anonymous shape checks | structural shape alias | Local compile-time check only; do not expose publicly. |
| overload accepting unrelated primitive alternatives | type-level union alias | Local code only; publish nominal wrapper APIs for C# callers. |
| async method returning `Task<T>` | `async fun ...: Task<T>` | Lowers to C# `async Task<T>`. |
| static helper class | `module` or top-level exported functions | Lowers to generated static module containers. |

## Calling Existing C# Libraries

Use manifest references:

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Tools.dll"]
```

TypeSharp source:

```text
namespace Company.Tools

import { Regex } from "System.Text.RegularExpressions"
import { LegacyFormatter } from "Legacy.Tools"

export fun normalize(value: string): string {
  let formatter = LegacyFormatter("legacy:")
  formatter.Format(value)
}

export fun isSlug(value: string): bool =
  Regex.IsMatch(value, "^[a-z0-9-]+$")
```

Implemented interop coverage includes framework assembly references, local DLL references, constructors, static/instance member calls, properties, fields, indexers, `params`, `out`, `in`, `ref`, optional/named arguments, delegate lambdas, events, nullable metadata warnings, generic calls, attributes, and ambiguous overload diagnostics.

## Consuming TypeSharp From C#

Reference generated output like a normal `net48` class library:

```xml
<ItemGroup>
  <Reference Include="Billing.Rules">
    <HintPath>..\Billing.Rules\generated\bin\Debug\net48\Billing.Rules.dll</HintPath>
  </Reference>
  <Reference Include="TypeSharp.Core">
    <HintPath>..\Billing.Rules\lib\TypeSharp.Core.dll</HintPath>
  </Reference>
  <Reference Include="TypeSharp.Runtime">
    <HintPath>..\Billing.Rules\lib\TypeSharp.Runtime.dll</HintPath>
  </Reference>
</ItemGroup>
```

Include `TypeSharp.Core.dll` when public APIs expose `Option<T>`, `Result<T,E>`, or `Unit`. Include `TypeSharp.Runtime.dll` when generated code uses runtime helpers such as nominal union metadata or pattern matching. Do not depend on generated internal helper names unless documented as public ABI.

## Nullability And Error Migration

TypeSharp reference-like types are non-null by default. Treat unannotated C# reference returns as unknown nullability, add guards or nullable annotations at interop boundaries, and prefer `Option<T>` when absence is part of the domain.

Use `Result<T,E>` for expected domain failure:

```text
public union ParseError {
  Empty
  Invalid(message: string)
}

export fun parse(text: string): Result<int, ParseError> =
  if text == "" {
    Error(Empty)
  }
  else {
    Ok(42)
  }
```

Keep exceptions for existing C# APIs, infrastructure failures, and boundaries where C# callers expect exceptions.

## Host Compatibility

Current stance:

- TypeSharp generates libraries that existing .NET Framework hosts can reference.
- TypeSharp does not replace ASP.NET/WCF/Windows Service hosting or configuration systems.
- ASP.NET Web Forms-style, WCF service/client, and worker-style host reference shapes are smoke-tested.
- Host-specific templates, `web.config`, WCF endpoint generation, Windows Service installer scaffolding, and IIS packaging remain Stable Backlog.

## Unsupported Automation

Not currently provided:

- automatic C# syntax conversion to TypeSharp,
- automatic `.csproj` to `TypeSharp.toml` conversion,
- NuGet dependency restore through TypeSharp CLI,
- generated ASP.NET/WCF/worker templates,
- direct IL backend,
- binary-compatible public ABI snapshot tooling,
- release signing/checksum pipeline.

## Adoption Checklist

- Identify one low-risk C# library boundary.
- Create a TypeSharp library project with `targetFramework = "net48"`.
- Add required framework/local DLL references to `TypeSharp.toml`.
- Write TypeSharp source with nominal public API types.
- Run `typesharp check`.
- Fix public boundary, nullability, overload, and byref diagnostics.
- Run `typesharp build`.
- Reference generated DLL plus required TypeSharp Core/Runtime DLLs from a C# `net48` consumer.
- Add C# consumer build to CI.
- Keep structural/type-level flexibility local until an explicit C#-friendly wrapper is designed.

