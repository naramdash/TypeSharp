# Task: Pattern Runtime Helper

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:40:56 +09:00
End Time: 2026-05-19 01:43:09 +09:00

## Objective

`TypeSharp.Runtime`에 generated pattern matching lowering이 사용할 union case predicate와 payload extraction helper를 추가한다.

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
- `TypeSharp.Runtime.TypeSharpPattern`
- case tag predicate helpers
- payload and payload-free case predicates
- payload extraction with no-match error helper
- `net48` and C# 7.3-compatible implementation
- smoke coverage for generated-case-like pattern matching behavior
- checklist, traceability, architecture, standard library, and task queue updates

Out:
- parser/type checker implementation for `match`
- generated pattern matching lowering in the C# backend
- exhaustiveness diagnostics
- decision tree optimization
- nested record/shape patterns

## Acceptance Criteria

- [x] `TypeSharp.Runtime` exposes pattern helpers for union case matching.
- [x] helper methods cover payload and payload-free case predicates.
- [x] helper methods provide payload extraction and no-match error construction.
- [x] helper implementation remains `net48`/C# 7.3-compatible and package-free.
- [x] smoke tests cover payload and payload-free union case matching.
- [x] `pattern helper` checklist item is marked complete while parser/lowering/exhaustiveness items remain open.
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
- `runtime pattern helper matches union cases` passes.
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
- Added `TypeSharp.Runtime.TypeSharpPattern`.
- Added case tag, payload case, and payload-free case predicates.
- Added payload extraction and no-match error helper.
- Added generated-case-like smoke coverage for pattern helper behavior.
- Updated checklist, traceability, architecture, standard library, runtime README, and task queue docs.

Remaining:
- Parser/type checker implementation for `match`.
- Generated pattern matching lowering in the C# backend.
- Exhaustiveness diagnostics.
- Decision tree optimization.
- Nested record/shape patterns.

Blocked:
- None.
