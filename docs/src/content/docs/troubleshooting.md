---
title: Troubleshooting
description: Common TypeSharp setup, build, diagnostics, and generated executable issues.
---

Start from [Install](../install/) when setup problems involve the release CLI, checksum manifest, runtime archive, or wrapper/PATH configuration.

## The CLI Cannot Find A Manifest

Use an explicit path:

```powershell
typesharp check .\path\to\TypeSharp.toml
```

Related diagnostic: `TS0100`.

## The Downloaded CLI Does Not Start

The release CLI archive is a framework-dependent modern .NET host. Generated projects still target `.NET Framework 4.8`; the CLI host target and generated artifact target are separate.

Check that `dotnet` is available, then run the wrapper directly from the extracted release folder:

```powershell
dotnet --info
& "$installRoot/typesharp.cmd" version
```

If the host runtime is missing, install the .NET runtime or SDK line named by the release notes and `typesharp version` metadata. Do not change generated project targets away from `net48` to fix a CLI host prerequisite.

The expected target split is:

- CLI host target: `net10.0`
- generated project target default: `net48`
- TypeSharp runtime target: `net48`

After the host is available, `typesharp version` and `typesharp version --json` should report the same split.

## A Local C# DLL Reference Is Missing

Check `references.paths` in `TypeSharp.toml` and confirm the file exists relative to the manifest.

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

Related diagnostic: `TS2401`.

## A Local C# DLL Has Invalid Metadata

`references.paths` must point at readable `.NET Framework 4.8`-compatible assemblies. If the file exists but is not a managed assembly, is corrupted, or targets an unsupported shape, `typesharp check` reports `TS2401` before generated C# is emitted.

Rebuild or copy a compatible local DLL, keep the path relative to `TypeSharp.toml`, then run:

```powershell
typesharp check TypeSharp.toml
```

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

Do not replace this with `references.packages`; package restore is still post-1.0.

## A TypeSharp Project Reference Fails

Direct TypeSharp project references use manifest paths, not DLL paths or package names. Keep `[projectReferences]` entries relative to the dependent `TypeSharp.toml`:

```toml
[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

Run the dependent project through the installed command:

```powershell
typesharp check TypeSharp.toml
typesharp build TypeSharp.toml
```

Project-reference diagnostics stop before dependent generated C# is emitted. Common codes are:

- `TS0100`: referenced manifest cannot be found
- `TS0103`: invalid referenced manifest, duplicate direct project name, cycle, or incompatible target framework
- `TS0112`: unresolved direct project source module specifier
- `TS0114`: imported or re-exported project member is not exported by the referenced project

TypeSharp only grants source-level visibility through direct project references; hidden transitive project references and arbitrary sibling folders are outside the 1.0 source graph.

## TypeSharp Core Or Runtime DLLs Are Missing

Generated projects that expose `Option<T>`, `Result<T,E>`, `Unit`, nominal unions, pattern helpers, or async runtime helpers need the matching release runtime archive when they are built, deployed, or referenced from C#.

Open the tag-specific GitHub Release notes, confirm the exact asset names, download the same-tag `typesharp-runtime-net48-<tag>.zip`, verify it with `SHA256SUMS.txt`, and reference the extracted DLLs through `references.paths`:

```toml
[references]
paths = [
  "../typesharp-runtime/lib/net48/TypeSharp.Core.dll",
  "../typesharp-runtime/lib/net48/TypeSharp.Runtime.dll"
]
```

If a C# `net48` consumer references a generated TypeSharp library, reference the generated DLL plus the same `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` from the runtime archive. Keep the runtime archive tag and Runtime ABI aligned with the CLI release notes.

## A NuGet Package Reference Fails

`references.packages` is reserved for future NuGet restore support. The current compiler reports `TS2405` and stops before generated C# emission.

Resolve the package outside TypeSharp, copy or build a `net48`-compatible DLL, then reference it with `references.paths`.

## Build Stops Before Emitting Generated C#

This is expected when parser, binder, type checker, public boundary, or interop diagnostics contain errors. Fix the diagnostics first, then run `typesharp build` again.

Common diagnostics:

- `TS0114`: source import or re-export references a non-exported target name
- `TS2004`: duplicate local export
- `TS2201`: type mismatch
- `TS2202`: nullability contract violation
- `TS2204`: compile-time type leaked through public boundary
- `TS2228`: invalid function return expression
- `TS2229`: invalid value initializer
- `TS2230`: invalid assignment value
- `TS2401`: missing local DLL or unreadable assembly metadata
- `TS2402`: ambiguous C# overload or constructor
- `TS2403`: invalid byref interop use
- `TS2405`: unsupported package reference
- `TS2406`: no matching C# overload or constructor
- `TS2407`: missing C# method
- `TS2408`: missing C# type
- `TS2409`: missing C# static member
- `TS2410`: missing C# instance member
- `TS2411`: missing or mismatched C# instance indexer
- `TS2412`: missing C# instance property setter
- `TS2413`: read-only C# instance field assignment
- `TS2414`: missing C# static property setter
- `TS2415`: read-only C# static field assignment
- `TS2416`: missing C# instance event
- `TS2417`: unsatisfied C# generic constraint

Use:

```powershell
typesharp explain TS2202
```

## Generated C# Build Fails

If TypeSharp diagnostics pass but the generated SDK-style C# project fails to build, `typesharp build` reports `TS3501`. The generated source and project are left under `generatedOutputRoot`, but the final `generated/bin/<Configuration>/net48` assembly is not produced.

Re-run build with normal or diagnostic verbosity, inspect the generated C# compiler output, and fix the TypeSharp source, manifest references, or local DLL compatibility before treating the generated assembly as deployable. If automation needs a stable payload, request JSON diagnostics for the same build:

```powershell
typesharp build TypeSharp.toml --verbosity diagnostic
typesharp build TypeSharp.toml --diagnostic-format json
typesharp explain TS3501
```

For the default output root, inspect `generated/<ProjectName>.Generated.csproj` and generated source files, then verify `generated/bin/<Configuration>/net48/<ProjectName>.dll` or `.exe` was not produced before copying artifacts to another project.

## Generated `.exe` Is Blocked

Some antivirus tools block short-lived generated `.NET Framework` executables in temporary workspaces. If `typesharp build` succeeds but `typesharp run` cannot launch the executable, inspect the generated output path and your local security policy.

Separate compiler/build failures from local executable launch policy first:

```powershell
typesharp check TypeSharp.toml
typesharp build TypeSharp.toml
typesharp run TypeSharp.toml
```

For executable projects, the generated file is under `generated/bin/<Configuration>/net48/<ProjectName>.exe` unless `generatedOutputRoot` or `--configuration` changes it. If the executable exists and `typesharp run` reports `Could not run generated executable`, treat it as a local policy or permission failure rather than a TypeSharp type-checking failure.

Only add antivirus exclusions for folders you trust.

## Docs Site Does Not Build

Run from `docs`:

```powershell
npm ci
npm run build
```

The repository smoke test `docs site contract is stable` checks the expected docs package and content contract.

## I Need The Canonical Design Decision

Human-facing standard docs live in docs. Start with:

- [Core Goal](../goal/)
- [Project Requirements](../requirements/)
- [Feature Status](../feature-status/)
