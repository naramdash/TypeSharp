# Task: Record Update Lowering Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:01:29 +09:00
End Time: 2026-05-19 03:04:34 +09:00

## Objective

TypeSharp record copy/update expression을 generated C# `net48` constructor call로 낮춰 immutable record workflow의 최소 작성-복사-소비 경로를 검증한다.

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
- backend record metadata for top-level record primary parameters.
- function parameter type tracking for record update receiver inference.
- generated C# lowering for `recordValue with { Field: value }`.
- backend golden fixture and C# `net48` consumer smoke.
- checklist and traceability update for `immutable record`.

Out:
- type checker inference for arbitrary record update expressions.
- local variable type tracking without annotations.
- nested receiver side-effect preservation.
- generic record update.
- update diagnostics for unknown fields or unknown receiver type.

## Acceptance Criteria

- [x] backend lowers a typed record parameter update to a C# constructor call.
- [x] backend keeps unchanged fields by reading them from the source record value.
- [x] backend fixture covers record update lowering.
- [x] CLI build smoke compiles generated `net48` assembly with record update lowering.
- [x] C# `net48` consumer smoke observes copied and updated values.
- [x] checklist and traceability reflect minimal immutable record coverage.
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
- `CLI build compiles record update lowering` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles record update lowering`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added record metadata tracking in the C# backend.
- Added function parameter type tracking for typed record update receivers.
- Added record update lowering to generated C# constructor calls.
- Added backend fixture `0016-record-update-lowering`.
- Added CLI build and C# `net48` consumer smoke for copied and updated record values.
- Marked minimal `immutable record` coverage complete in checklist and traceability.

Remaining:
- Type checker inference for arbitrary record update expressions.
- Local variable type tracking without annotations.
- Generic record update.
- Diagnostics for unknown fields or unknown receiver type.

Blocked:
- None.
