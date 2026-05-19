# Task 0119: Runnable Example Project Catalog

Status: Done
Queue: Q5
Start Time: 2026-05-19 10:16:19 +09:00
End Time: 2026-05-19 10:21:55 +09:00

## Objective

Complete the runnable example adoption slice by adding a catalog of real TypeSharp example projects with concrete verification commands and a smoke matrix that exercises those commands against copied projects.

## Scope

In:
- Runnable examples for console, library public API, local C# DLL interop, worker-style `net48` host consumption, and diagnostics/tooling workflow.
- Per-project `TypeSharp.toml`, `src/*.tysh`, README, and scenario-specific C# host or local DLL source where needed.
- Smoke matrix tests that copy examples to `tests/tmp` and run commands without dirtying the repository.
- Checklist and traceability updates.

Out:
- Shipping generated binaries in the repository.
- Full VS Code extension host automation.
- ASP.NET/WCF sample projects beyond the existing host compatibility smoke coverage.
- Astro Starlight docs site and GitHub Pages workflow.

## Acceptance Criteria

- [x] [../examples/runnable/README.md](../examples/runnable/README.md) lists all runnable scenarios, commands, and smoke evidence.
- [x] Each runnable example has a manifest, source root, README, and scenario-specific support files.
- [x] Console example is checked, built, and run or confirms the generated executable exists when local antivirus blocks launch.
- [x] Library example builds a generated `net48` assembly.
- [x] C# interop example builds a local `net48` DLL and consumes it from TypeSharp.
- [x] Worker host example builds a generated TypeSharp library and a host `net48` project referencing it.
- [x] Diagnostics example produces expected `TS2202` JSON diagnostics.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "runnable example"
```

Result:
- PASS compiler test project build.
- PASS `runnable example catalog smoke matrix is stable`.
- PASS `runnable example project commands are smoke-tested`.

## Handoff

Done:
- Added smoke-tested runnable example catalog and projects.
- Marked runnable example catalog and smoke matrix checklist items complete.

Remaining:
- Astro Starlight official docs site.
- GitHub Pages docs deployment workflow.

Blocked:
- None.
