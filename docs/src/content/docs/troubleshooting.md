---
title: Troubleshooting
description: Common TypeSharp setup, build, diagnostics, and generated executable issues.
---

## The CLI Cannot Find A Manifest

Use an explicit path:

```powershell
typesharp check .\path\to\TypeSharp.toml
```

Related diagnostic: `TS0100`.

## A Local C# DLL Reference Is Missing

Check `references.paths` in `TypeSharp.toml` and confirm the file exists relative to the manifest.

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

Related diagnostic: `TS2401`.

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

## Generated `.exe` Is Blocked

Some antivirus tools block short-lived generated `.NET Framework` executables in temporary workspaces. If `typesharp build` succeeds but `typesharp run` cannot launch the executable, inspect the generated output path and your local security policy.

Only add antivirus exclusions for folders you trust.

## Docs Site Does Not Build

Run from `docs`:

```powershell
npm ci
npm run build
```

The repository smoke test `docs site contract is stable` checks the expected docs package and content contract.

## I Need The Canonical Design Decision

Human-facing standard docs live in docs. Agent-facing traceability and task records live in the repository:

- [Core Goal](../goal/)
- [`agent/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/traceability.md)
- [`agent/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/tasks.md)
