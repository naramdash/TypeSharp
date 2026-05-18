# Task: Immutable Record Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:55:34 +09:00
End Time: 2026-05-19 02:59:13 +09:00

## Objective

TypeSharp `record` declaration을 generated C# `net48` immutable class shape로 낮추고 C# consumer가 constructor, get-only properties, equality/hash surface를 사용할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generated C# emission for top-level record declarations with primary parameters.
- immutable get-only property emission.
- constructor emission for record parameters.
- value equality and hash override emission for record parameters.
- backend golden fixture and C# `net48` consumer smoke.

Out:
- copy/update expression lowering.
- record body members.
- generic record-specific equality beyond generic class signature preservation.
- nested records.
- record semantic model beyond existing parser/binder coverage.

## Acceptance Criteria

- [x] backend emits a TypeSharp `record` as a sealed C# class.
- [x] backend emits constructor and get-only properties for record parameters.
- [x] backend emits value equality and hash overrides based on record parameters.
- [x] backend fixture covers the generated record shape.
- [x] CLI build smoke compiles generated `net48` assembly with a public record class.
- [x] C# `net48` consumer smoke constructs the generated record, reads properties, and calls equality/hash APIs.
- [x] traceability records immutable record backend coverage.
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
- `CLI build compiles immutable record API` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles immutable record API`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added generated C# sealed class emission for record declarations.
- Added constructor and get-only property emission for record parameters.
- Added generated value equality and hash overrides based on record parameters.
- Added backend fixture `0015-immutable-record-api`.
- Added CLI build and C# `net48` consumer smoke for generated immutable record public API.
- Recorded immutable record backend coverage in traceability.

Remaining:
- Copy/update expression lowering.
- Record body members.
- Record semantic model integration.

Blocked:
- None.
