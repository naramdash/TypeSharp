# Task: Basic Semantics Smoke

Status: Done
Queue: Q2
Start Time: 2026-05-19 02:08:41 +09:00
End Time: 2026-05-19 02:10:33 +09:00

## Objective

현재 구현된 기본 타입/literal, local binding, function declaration/call 경로를 generated C# `net48` build smoke와 backend golden fixture로 고정하고 MVP checklist 상태를 실제 구현에 맞춘다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generated C# golden fixture for primitive string/int/bool literals, local `let`, and function call
- CLI build smoke for the same feature set
- checklist and traceability updates for the three MVP feature items

Out:
- numeric operators and binary expression lowering
- local type inference expansion beyond current simple checker behavior
- generic functions
- class/interface/record lowering

## Acceptance Criteria

- [x] backend golden fixture covers basic literals, local `let`, and function calls.
- [x] CLI build smoke compiles a generated `net48` assembly using those features.
- [x] checklist marks `기본 타입과 literal`, `local binding`, and `function declaration/call` complete.
- [x] traceability points to concrete tests for those MVP feature items.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- test project builds.
- backend fixture snapshots pass.
- `CLI build compiles basic semantics` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles basic semantics`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added backend fixture `0008-basic-semantics`.
- Added CLI build smoke for primitive literals, local `let`, function declarations, and function calls.
- Marked the three corresponding MVP checklist items complete.
- Added traceability for the generated `net48` build path.

Remaining:
- Numeric operator and binary expression lowering.
- Generic function implementation.
- Class/interface/record lowering.

Blocked:
- None.
