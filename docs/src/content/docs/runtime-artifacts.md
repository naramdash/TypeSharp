---
title: Runtime Artifacts
description: How a TypeSharp project becomes generated C# source, a net48 project, runtime/core references, and deployable .NET Framework artifacts.
---

TypeSharp projects produce ordinary `.NET Framework 4.8` artifacts through a conservative source backend. The compiler and CLI can run on a modern .NET host, but the generated project, generated assembly, `TypeSharp.Core`, and `TypeSharp.Runtime` stay on the `net48` side of the boundary.

Use [Install](../install/) first to install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`. The installed NuGet tool package is both the CLI distribution and the matching TypeSharp Core/Runtime DLL distribution. Use `typesharp runtime-path` when examples need explicit runtime DLL paths.

## Artifact Boundary

The key split is between the tool host and the runtime artifact.

```mermaid
flowchart LR
  subgraph Host["Modern .NET host"]
    Cli["TypeSharp.Cli"]
    Compiler["TypeSharp.Compiler"]
    Lsp["TypeSharp.LanguageServer"]
  end

  subgraph Generated["Generated net48 artifact set"]
    Source["*.tysh source"]
    CSharp["generated/src/*.g.cs"]
    Project["<Project>.Generated.csproj"]
    Assembly["bin/<Configuration>/net48/<Project>.dll or .exe"]
  end

  subgraph Runtime["net48 runtime dependencies"]
    Core["TypeSharp.Core.dll"]
    RuntimeDll["TypeSharp.Runtime.dll"]
    Framework[".NET Framework assemblies"]
    LocalDll["Local net48 DLLs"]
  end

  Cli --> Compiler
  Compiler --> Source
  Source --> CSharp
  CSharp --> Project
  Project --> Assembly
  Project -. manifest references .-> Core
  Project -. manifest references .-> RuntimeDll
  Project -. manifest references .-> Framework
  Project -. manifest references .-> LocalDll
```

Rules:

- `TypeSharp.Compiler`, `TypeSharp.Cli`, and `TypeSharp.LanguageServer` are tooling projects.
- Generated user assemblies target `net48`.
- `TypeSharp.Core` targets `net48` and contains user-facing public types such as `Option<T>`, `Result<T, E>`, and `Unit`.
- `TypeSharp.Runtime` targets `net48` and contains compiler-generated helper APIs for unions, pattern matching, equality, hashing, and async helpers.
- Docs dependencies, Node packages, and Astro build output are documentation-only and are not part of generated runtime deployment.

## Build Pipeline

`typesharp check` stops before emission. `typesharp build` runs the same diagnostics path, emits C# source, writes a generated SDK-style project, then invokes `dotnet build` in the generated output root.

```mermaid
sequenceDiagram
  autonumber
  participant User
  participant CLI as typesharp build
  participant Manifest as TypeSharp.toml
  participant Compiler as Compiler pipeline
  participant Backend as C# backend
  participant MSBuild as dotnet build

  User->>CLI: typesharp build TypeSharp.toml
  CLI->>Manifest: load project, language, references, project references, tooling
  Compiler->>Compiler: validate direct project-reference graph
  CLI->>CLI: build direct referenced projects first
  CLI->>Compiler: discover sources and references
  Compiler->>Compiler: parse, bind, validate interop, type check
  Compiler->>Compiler: validate source module graph and public ABI
  Compiler->>Backend: lower to C# 7.3-compatible source
  Backend-->>CLI: generated source artifacts
  CLI->>CLI: write generated/src/*.g.cs
  CLI->>CLI: write <Project>.Generated.csproj and NuGet.config
  CLI->>MSBuild: dotnet build <Project>.Generated.csproj
  MSBuild-->>CLI: bin/<Configuration>/net48 artifact
  CLI-->>User: generated paths or diagnostics
```

Emission happens only when blocking diagnostics are absent. Reference diagnostics, project-reference diagnostics, parser errors, type errors, public boundary diagnostics, unsupported packages, and interop validation failures stop before generated C# project build.

## Generated Project Shape

The generated project is written under `project.generatedOutputRoot` from `TypeSharp.toml`. The default output root is `obj/generated`; many examples use `generated` to make the artifact tree easier to inspect.

```text
generated/
  src/Main.g.cs
  src/Feature/Rules.g.cs
  <ProjectName>.Generated.csproj
  NuGet.config
  bin/Debug/net48/<ProjectName>.dll
  obj/
```

The generated `.csproj` uses these stable properties:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <OutputType>Library</OutputType>
    <LangVersion>7.3</LangVersion>
    <ImplicitUsings>false</ImplicitUsings>
    <Nullable>disable</Nullable>
    <AssemblyName>ProjectName</AssemblyName>
    <RootNamespace>ProjectName</RootNamespace>
  </PropertyGroup>
</Project>
```

Executable projects use `OutputType` `Exe` and produce `.exe` instead of `.dll`. `typesharp run` builds the executable first, then launches the generated `.exe` and forwards values after `--` to `main(args: string[])`.

## Reference Flow

The manifest is the only current source of generated project references. Framework assemblies, local DLLs, `TypeSharp.Core.dll`, and `TypeSharp.Runtime.dll` enter the generated project through `[references]`. Direct TypeSharp project references enter through `[projectReferences]` and resolve to generated assemblies after the referenced projects build.

```toml
[project]
name = "BillingRules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing"
generatedOutputRoot = "generated"

[references]
assemblies = [
  "System",
  "System.Core"
]
paths = [
  "lib/Legacy.Billing.dll",
  "TypeSharp.Tool-runtime-root/TypeSharp.Core.dll",
  "TypeSharp.Tool-runtime-root/TypeSharp.Runtime.dll"
]
packages = []

[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

Reference rules:

- `references.assemblies` becomes framework `<Reference Include="..."/>` items.
- `references.paths` becomes local `<Reference>` items with generated-project-relative `<HintPath>` values. Runtime/Core paths should come from `typesharp runtime-path`, not repository build folders.
- `projectReferences.paths` names direct TypeSharp manifests. `typesharp build` builds those projects first and then writes local `<Reference>` items with hint paths to the referenced generated assemblies.
- `references.packages` is reserved and currently reports `TS2405`; TypeSharp does not restore NuGet packages during build. Future support must preserve deterministic lock files, package source mapping, vulnerability auditing, license inventory, checksum/signature policy, and offline-friendly no-package builds.
- The generated project writes an offline `NuGet.config` with package sources cleared so normal builds do not accidentally resolve hidden packages.

## TypeSharp Project References

The current preview reads `[projectReferences] paths = [...]` entries that point at other TypeSharp manifests, not DLLs. It does not write generated MSBuild `<ProjectReference>` items yet; the dependent generated project uses explicit local `<Reference>` items to the referenced generated assemblies after build ordering has been enforced by the TypeSharp builder.

The artifact policy is:

- A referenced TypeSharp project is checked and built before the dependent project.
- The dependent project receives explicit generated assembly paths and compiler-derived TypeSharp module/export metadata for direct referenced source imports.
- The generated `net48` project currently uses local `<Reference>` items. Generated `<ProjectReference>` items remain future work until output paths and metadata inputs are deterministic enough for that shape.
- Source-level imports across project boundaries require direct manifest project references; TypeSharp does not infer visibility from arbitrary sibling folders or hidden transitive references.
- Project-reference diagnostics must stop before the dependent generated project is emitted when a referenced manifest, artifact, target framework, or exported module member is invalid.

This keeps TypeSharp's build graph aligned with ordinary .NET artifact consumption while preserving the source module graph as the compiler-owned authority.

## Core And Runtime Roles

`TypeSharp.Core` and `TypeSharp.Runtime` solve different problems.

| Assembly | Public Role | Referenced When |
| --- | --- | --- |
| `TypeSharp.Core.dll` | User-facing `Option<T>`, `Result<T, E>`, `Unit`, and small core public helpers. | TypeSharp source imports `TypeSharp.Core` or exposes core types to C# consumers. |
| `TypeSharp.Runtime.dll` | Generated-code helper surface for nominal union cases, pattern matching, equality/hash composition, and async helpers. | Generated C# needs `TypeSharp.Runtime` helpers, for example nominal unions or union matches. |

Current preview builds require these assemblies to be available as local `net48` DLL references when a project uses them. Run `typesharp runtime-path` or `typesharp runtime-path --json` to locate the matching DLLs bundled with the installed `TypeSharp.Tool` package. A generated assembly that exposes `Option<T>` or `Result<T,E>` to C# consumers also requires the consuming C# project to reference `TypeSharp.Core.dll`. A generated assembly that exposes nominal union cases or calls runtime pattern helpers requires the host or consumer project to deploy `TypeSharp.Runtime.dll` beside the generated assembly.

The 1.0 runtime resolution policy is explicit installed tool-package runtime references: install `TypeSharp.Tool`, resolve the runtime root with `typesharp runtime-path`, and reference `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` through `references.paths` when a project needs them. The CLI does not discover repository build folders or use a second runtime package.

## Source To Artifact Example

This TypeSharp source:

```tysh
namespace Company.Billing

import { Option, Result } from "TypeSharp.Core"

public union InvoiceStatus {
  Draft
  Posted(id: string)
}

export fun keepOption(value: Option<string>): Option<string> = value

export fun keepResult(value: Result<InvoiceStatus, string>): Result<InvoiceStatus, string> = value

export fun posted(id: string): InvoiceStatus = Posted(id)

export fun describe(status: InvoiceStatus): string =
  match status {
    Draft => "Draft"
    Posted(id) => $"Posted:{id}"
  }
```

needs both `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll`:

- `Option<T>` and `Result<T, E>` come from `TypeSharp.Core`.
- `InvoiceStatus` lowers to CLR-visible generated union classes that implement runtime union metadata from `TypeSharp.Runtime`.
- The generated project builds a `net48` assembly that C# can reference together with both TypeSharp helper assemblies.

The current release ABI exposes `Result<T,E>` factories to C# as `Result<T,E>.Ok(...)` and `Result<T,E>.Error(...)`. Direct TypeSharp imports named `Ok` and `Error` are future source ergonomics, not the current installed runtime reference shape.

## Deployment Set

A host consumes the output like an ordinary .NET Framework class library.

```mermaid
flowchart TD
  Build["typesharp build"]
  GeneratedDll["generated/bin/Debug/net48/BillingRules.dll"]
  CoreDll["TypeSharp.Tool runtime/net48/TypeSharp.Core.dll"]
  RuntimeDll["TypeSharp.Tool runtime/net48/TypeSharp.Runtime.dll"]
  LegacyDll["lib/Legacy.Billing.dll"]

  subgraph Host["net48 host application"]
    AspNet["ASP.NET / Web API"]
    Wcf["WCF service or client"]
    Worker["Windows Service / scheduled job / worker"]
  end

  Build --> GeneratedDll
  GeneratedDll --> AspNet
  GeneratedDll --> Wcf
  GeneratedDll --> Worker
  CoreDll --> Host
  RuntimeDll --> Host
  LegacyDll --> Host
```

The deployable set is:

1. the generated TypeSharp assembly,
2. `TypeSharp.Core.dll` from the installed `TypeSharp.Tool` runtime root when public APIs or source imports use core types,
3. `TypeSharp.Runtime.dll` from the installed `TypeSharp.Tool` runtime root when generated code uses runtime helpers,
4. referenced local DLLs from `references.paths`,
5. the normal .NET Framework assemblies available to the target host.

The C# host may reference those paths directly during build or copy the DLLs beside the generated assembly for deployment, but they must come from the same installed `TypeSharp.Tool` version and Runtime ABI as the CLI.

TypeSharp does not require a custom loader, startup hook, ASP.NET Core migration, or nonstandard AppDomain lifecycle hook for the current artifact shape.

## Release Runtime Layout Smokes

The release-style smokes use a locally packed `TypeSharp.Tool` package, install it into a clean tool path, and verify the same installed-command, local-DLL dependency, dependency-diagnostic, generated-build-failure, project-reference build-order, runtime-backed library, and C# consumer output shape before the package is pushed to NuGet.

1. Install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, then verify bare `typesharp` resolves and reports the expected version.
2. Verify `typesharp version --json` reports `artifactKind` as `dotnet-tool` and keeps `targetDefault`/`runtimeTargetFramework` on `net48`.
3. Verify `typesharp runtime-path --json` points at packaged `runtime/net48/TypeSharp.Core.dll` and `runtime/net48/TypeSharp.Runtime.dll`.
4. Create, check, build, and run a clean console project through the installed `typesharp` command.
5. Build a local C# `net48` DLL, reference it with `references.paths = ["../lib/Legacy.Tools.dll"]`, build a generated TypeSharp library against it, and compile a separate C# `net48` consumer that references both assemblies.
6. Verify missing local DLL and unsupported package references report `TS2401`/`TS2405` and do not emit generated source, generated projects, or generated assemblies.
7. Verify generated C# project build failures report `TS3501` after generated source/project emission and before a generated assembly is produced.
8. Create a shared library and a dependent library with `[projectReferences] paths = ["../HostedShared/TypeSharp.toml"]`, then build the dependent project through installed `typesharp` and verify the referenced generated assembly exists before the dependent assembly.
9. Create a library with `typesharp new library`.
10. Reference the `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` paths returned by `typesharp runtime-path` through `references.paths`.
11. Build TypeSharp source that exposes `Option<T>`, `Result<T,E>`, a nominal union, union match lowering, and generated `net48` output.
12. Compile a separate C# `net48` consumer that references the generated assembly plus the packaged Core/Runtime DLLs and calls `TypeSharp.Runtime` union and async helper APIs.

This proves the supported preview shape is explicit local DLL references from the installed `TypeSharp.Tool` package, not discovery from repository build folders. The release workflow also verifies package identity, NuGet installability from a local source before publication, runtime ABI status, default target framework, installed `typesharp version` text plus JSON CLI/compiler/language identity and target/framework/runtime metadata, `typesharp runtime-path`, `typesharp new`, `typesharp format --check`, `typesharp check`, `typesharp build`, `typesharp run`, local C# DLL dependency paths, text and JSON dependency diagnostics, text and JSON generated C# build-failure diagnostics, installed `typesharp explain`, direct TypeSharp project-reference build ordering, library-project `typesharp run` rejection, the runtime-backed TypeSharp library, and a separate C# `net48` consumer output directory containing the generated assembly plus Core/Runtime DLLs from the installed tool package.

## Invariants

- Generated projects target `net48` unless a future documented profile changes that.
- `net481` is a qualified future profile, not an automatic upgrade. It needs Core/Runtime, generated project, C# consumer, and host smoke evidence before TypeSharp can claim support.
- Generated C# stays C# 7.3-compatible.
- Generated output is reproducible from source and should not be committed.
- Source discovery excludes `bin`, `obj`, `.git`, and the generated output root.
- Diagnostics preserve source paths and stop emission before generated project build when the error is known at TypeSharp level.
- Compile-time-only constructs such as structural shapes, type-level unions, and anonymous public boundaries must be wrapped in nominal CLR-visible types before crossing public ABI.
- Runtime ABI `0` is preview. Compiler and runtime ABI constants must remain aligned.

## Evidence

Repository evidence for this artifact model lives in:

- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Checking/TypeSharpChecker.cs`
- `lang/TypeSharp.Core/TypeSharp.Core.csproj`
- `lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- `test/TypeSharp.Compiler.Tests/Program.cs`

Focused smoke areas include generated C# project scaffold, manifest reference propagation, generated `net48` assembly build, C# `net48` consumer builds, Core/Runtime `net48` builds, and ASP.NET/WCF/worker-style host compatibility builds.

## Related Pages

- [Project Configuration](../project-configuration/)
- [.NET Interop](../dotnet-interop/)
- [Lowering](../lowering/)
- [API And CLI Reference](../api/)
- [CLI](../cli/)
