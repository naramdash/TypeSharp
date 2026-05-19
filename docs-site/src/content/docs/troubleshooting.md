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

- `TS2201`: type mismatch
- `TS2202`: nullability contract violation
- `TS2204`: compile-time type leaked through public boundary
- `TS2402`: ambiguous C# overload
- `TS2403`: invalid byref interop use
- `TS2405`: unsupported package reference

Use:

```powershell
typesharp explain TS2202
```

## Generated `.exe` Is Blocked

Some antivirus tools block short-lived generated `.NET Framework` executables in temporary workspaces. If `typesharp build` succeeds but `typesharp run` cannot launch the executable, inspect the generated output path and your local security policy.

Only add antivirus exclusions for folders you trust.

## Docs Site Does Not Build

Run from `docs-site`:

```powershell
npm ci
npm run build
```

The repository smoke test `docs site contract is stable` checks the expected docs-site package and content contract.

## I Need The Canonical Design Decision

Human-facing pages summarize behavior. Canonical design and traceability documents live in the repository:

- [`docs/goal.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/goal.md)
- [`docs/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/traceability.md)
- [`docs/tasks`](https://github.com/naramdash/TypeSharp/tree/main/docs/tasks)
