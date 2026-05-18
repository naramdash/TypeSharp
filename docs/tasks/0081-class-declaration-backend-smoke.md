# Task: Class Declaration Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:36:55 +09:00
End Time: 2026-05-19 02:40:37 +09:00

## Objective

TypeSharp `class` declaration의 최소 public API가 generated C# `net48` assembly에 class와 instance method로 노출되고 C# consumer가 호출할 수 있음을 검증한다.

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
- generated C# emission for top-level TypeSharp class declarations.
- class method emission as instance C# methods.
- backend golden fixture for a simple public class method.
- CLI build and C# `net48` consumer smoke constructing the generated class and calling its method.

Out:
- interface declaration implementation.
- primary constructor lowering.
- fields, properties, events, and accessor lowering.
- inheritance/base clauses.
- class member type checking beyond existing parser/binder coverage.

## Acceptance Criteria

- [x] backend emits a top-level TypeSharp `class` as a C# class in the generated namespace.
- [x] backend emits class `fun` members as instance methods.
- [x] backend fixture covers a simple public class method.
- [x] CLI build smoke compiles generated `net48` assembly with a public class.
- [x] C# `net48` consumer smoke constructs the generated class and calls its method.
- [x] traceability records class backend coverage.
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
- `CLI build compiles class declaration API` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles class declaration API`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added top-level class emission in the C# backend.
- Added class `fun` member emission as instance methods.
- Added backend fixture `0012-class-declaration-api`.
- Added CLI build and C# `net48` consumer smoke for generated class public API.
- Recorded class backend coverage in traceability.

Remaining:
- Interface declaration implementation.
- Primary constructor lowering.
- Class properties/events/accessors.
- Class member semantic model and type checking.

Blocked:
- None.
