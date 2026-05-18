# Task: Equality Hash Runtime Helper

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:44:05 +09:00
End Time: 2026-05-19 01:47:07 +09:00

## Objective

`TypeSharp.Runtime`에 generated record와 union case lowering이 사용할 equality, sequence equality, hash composition helper를 추가한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Runtime`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TypeSharp.Runtime.TypeSharpEquality`
- value equality helper
- sequence equality helper
- deterministic hash composition helper
- `net48` and C# 7.3-compatible implementation
- smoke coverage for equality and hash behavior
- checklist, traceability, architecture, standard library, and task queue updates

Out:
- generated record lowering
- generated union equality lowering
- structural equality for arbitrary object shapes
- public ABI snapshot tests
- performance benchmarking

## Acceptance Criteria

- [x] `TypeSharp.Runtime` exposes equality and sequence equality helpers.
- [x] `TypeSharp.Runtime` exposes deterministic hash composition helpers.
- [x] helper implementation remains `net48`/C# 7.3-compatible and package-free.
- [x] smoke tests cover value equality, sequence equality, and hash determinism.
- [x] `equality/hash helper` checklist item is marked complete while generated record/union lowering remains open.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Expected:
- runtime project builds.
- test project builds.
- `runtime equality helper combines values` passes.
- support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- PASS `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- PASS `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- PASS `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json`
- PASS `git diff --check`

## Handoff

Done:
- Added `TypeSharp.Runtime.TypeSharpEquality`.
- Added value equality, sequence equality, and hash composition helpers.
- Added smoke coverage for equality, sequence equality, and deterministic hash behavior.
- Updated checklist, traceability, architecture, standard library, runtime README, and task queue docs.

Remaining:
- Generated record lowering.
- Generated union equality lowering.
- Structural equality for arbitrary object shapes.
- Public ABI snapshot tests.
- Performance benchmarking.

Blocked:
- None.
