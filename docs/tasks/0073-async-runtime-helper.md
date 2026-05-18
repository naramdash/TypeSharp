# Task: Async Runtime Helper

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:48:20 +09:00
End Time: 2026-05-19 01:50:29 +09:00

## Objective

`TypeSharp.Runtime`에 generated async lowering이 사용할 package-free `Task` creation helper를 추가한다.

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
- `TypeSharp.Runtime.TypeSharpAsync`
- completed `Task` helper
- generic `Task<T>` result helper
- generic and non-generic faulted task helpers
- `net48` and C# 7.3-compatible implementation
- smoke coverage for helper task behavior
- checklist, traceability, architecture, standard library, and task queue updates

Out:
- parser/type checker implementation for `async`
- generated async lowering in the C# backend
- cancellation helper policy
- async workflow/computation expression syntax
- performance benchmarking

## Acceptance Criteria

- [x] `TypeSharp.Runtime` exposes completed task and result task helpers.
- [x] `TypeSharp.Runtime` exposes generic and non-generic faulted task helpers.
- [x] helper implementation remains `net48`/C# 7.3-compatible and package-free.
- [x] smoke tests cover completed, result, and faulted task behavior.
- [x] `async helper` checklist item is marked complete while async syntax/lowering remains open.
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
- `runtime async helper creates tasks` passes.
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
- Added `TypeSharp.Runtime.TypeSharpAsync`.
- Added completed task, generic result task, and generic/non-generic faulted task helpers.
- Added smoke coverage for completed, result, and faulted task behavior.
- Updated checklist, traceability, architecture, standard library, runtime README, and task queue docs.

Remaining:
- Parser/type checker implementation for `async`.
- Generated async lowering in the C# backend.
- Cancellation helper policy.
- Async workflow/computation expression syntax.
- Performance benchmarking.

Blocked:
- None.
