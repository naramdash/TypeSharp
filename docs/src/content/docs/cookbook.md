---
title: Cookbook
description: Short TypeSharp recipes for common project workflows.
---

Start from [Install](../install/) before running these recipes: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. When a recipe needs TypeSharp Core/Runtime DLLs, use `typesharp runtime-path` to locate the matching DLLs bundled with the installed tool.

## Call A Local C# DLL

1. Build the C# library to `lib/Legacy.Tools.dll`.
2. Reference it in `TypeSharp.toml`.
3. Import the type and call it.

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Tools.dll"]
packages = []
```

```tysh
namespace Samples.Interop

import { LegacyFormatter } from "Legacy.Tools"

export fun format(value: string): string {
  let formatter = LegacyFormatter("legacy:")
  formatter.Format(value)
}
```

## Expose A TypeSharp API To C#

Keep public signatures nominal:

```tysh
namespace Samples.PublicApi

export record Customer(Name: string, Age: int)

export fun describe(customer: Customer): string =
  customer.Name
```

Avoid exposing structural shapes, intersection aliases, or type-level unions directly in public API; use records, classes, interfaces, or nominal unions.

## Model Nullable Input Safely

Prefer nullable annotations or `Option<T>` where absence is expected:

```tysh
export fun displayName(name: string?): string =
  if name == null {
    "Anonymous"
  } else {
    name
  }
```

## Create And Update A Record

```tysh
export record Customer(Name: string, Age: int)

export fun birthday(customer: Customer): Customer =
  customer with { Age: customer.Age + 1 }
```

Status: record update lowering is implemented and smoke-tested.

## Use A Nominal Union With Match

```tysh
export union OrderState {
  Draft
  Submitted(string)
}

export fun label(state: OrderState): string =
  match state {
    Draft => "Draft"
    Submitted(id) => id
  }
```

Status: nominal union and match lowering are implemented for the smoke-tested subset.

## Build And Run With Arguments

```tysh
namespace Samples.Args

export fun main(args: string[]): string =
  args.Length.ToString()
```

```powershell
typesharp check TypeSharp.toml
typesharp build TypeSharp.toml
typesharp run TypeSharp.toml -- alpha beta
```

Status: `main(args: string[])` argument forwarding is smoke-tested.

## Consume Generated DLLs From A Host Project

Build the TypeSharp library, then reference the generated assembly plus TypeSharp Core/Runtime dependencies from the installed `TypeSharp.Tool` package in your `net48` host project. Use `typesharp runtime-path` before copying or referencing `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll`. See [Examples](../examples/) for ASP.NET/WCF-style and worker-style host smoke projects.
