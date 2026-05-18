# Task: C# Byref Params Interop Status

Status: Done
Queue: Q3
Start Time: 2026-05-19 02:20:27 +09:00
End Time: 2026-05-19 02:22:14 +09:00

## Objective

C# `params`, `out`, `in`, `ref` interop의 개별 smoke와 diagnostic coverage가 이미 구현된 상태를 감사하고, 남아 있던 aggregate MVP checklist 항목을 실제 검증 근거와 맞춘다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `tests/TypeSharp.Compiler.Tests`
- `src/TypeSharp.Compiler/Interop`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`

## Scope

In:
- checklist aggregate item for C# `ref`/`out`/`in`/`params` interop
- traceability entry connecting the aggregate item to concrete tests
- task queue update

Out:
- new byref interop syntax
- new overload ranking behavior
- new metadata reader behavior
- new runtime helper

## Acceptance Criteria

- [x] `params` call smoke exists and passes.
- [x] `out` call smoke exists and passes.
- [x] `in` call smoke exists and passes.
- [x] `ref` call smoke exists and passes.
- [x] invalid byref call-site diagnostic exists and passes.
- [x] checklist aggregate item is marked complete.
- [x] traceability names the concrete tests behind the aggregate item.

## Verification

Command:

```text
rg -n "LegacyByRef|LegacyParams|TS2403|CLI build compiles imported (params|out|in|ref) call" tests/TypeSharp.Compiler.Tests/Program.cs docs/traceability.md
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- search returns concrete byref/params smoke and diagnostic coverage.
- test runner passes.
- whitespace check has no errors.

Result:
- Pass. Search found `CLI build compiles imported params call`, `out call`, `in call`, `ref call`, `LegacyByRef`, `LegacyParams`, and `TS2403` diagnostic coverage.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` passed and includes all byref/params smokes.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Marked aggregate C# `ref`/`out`/`in`/`params` interop complete based on concrete smoke and diagnostic tests.

Remaining:
- Broader C# interop test policy remains open.
- Full C# overload resolution remains open.

Blocked:
- None.
