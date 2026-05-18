# Task: Generic Type Declaration Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:48:57 +09:00
End Time: 2026-05-19 02:51:06 +09:00

## Objective

TypeSharp generic type declaration의 최소 public API가 generated C# generic class로 보존되고 C# `net48` consumer가 해당 type argument로 사용할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/types.md](../grammar/types.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Binding/TypeSharpBinder.cs`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- type declaration type parameter binding for nested member signatures.
- generated C# generic class signature emission.
- backend golden fixture for a generic class instance method.
- CLI build and C# `net48` consumer smoke using a generated generic class.
- checklist and traceability update for `generic type/function`.

Out:
- generic constraints and `where` clauses.
- generic record/union lowering.
- generic type inference in the TypeSharp semantic model.
- variance and type parameter constraints.

## Acceptance Criteria

- [x] binder resolves type declaration type parameters in member signatures.
- [x] backend fixture covers a generated generic class method.
- [x] CLI build smoke compiles generated `net48` assembly with a generic public class.
- [x] C# `net48` consumer smoke constructs the generated generic class and calls its method.
- [x] checklist and traceability reflect minimal generic type/function coverage.
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
- `CLI build compiles generic type declaration API` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles generic type declaration API`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added type declaration type parameter binding for nested member signatures.
- Added backend fixture `0014-generic-type-declaration-api`.
- Added CLI build and C# `net48` consumer smoke for generated generic class public API.
- Marked minimal `generic type/function` coverage complete in checklist and traceability.

Remaining:
- Generic constraints and `where` clauses.
- Generic record/union lowering.
- Generic type inference in the TypeSharp semantic model.
- Variance and type parameter constraints.

Blocked:
- None.
