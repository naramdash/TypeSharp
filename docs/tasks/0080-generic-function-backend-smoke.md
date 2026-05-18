# Task: Generic Function Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:32:11 +09:00
End Time: 2026-05-19 02:34:55 +09:00

## Objective

TypeSharp generic function declaration이 generated C# public API signature로 보존되고 C# `net48` consumer가 해당 generic API를 호출할 수 있음을 검증한다.

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
- function type parameter binding for function signatures and bodies.
- generated C# generic method signature emission.
- backend golden fixture for a generic pass-through function.
- CLI build and C# `net48` consumer smoke calling a generated generic method.

Out:
- generic type declaration lowering.
- generic constraints and `where` clauses.
- generic method type argument inference beyond what C# compiler already provides.
- generic call expression type argument syntax.

## Acceptance Criteria

- [x] binder resolves function type parameters in parameter and return type annotations.
- [x] backend emits function type parameters in generated C# method declarations.
- [x] backend fixture covers `fun identity<T>(value: T): T`.
- [x] CLI build smoke compiles generated `net48` assembly with a generic public method.
- [x] C# `net48` consumer smoke calls generated generic method with explicit type argument.
- [x] traceability records the generic function API coverage.
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
- `CLI build compiles generic function API` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles generic function API`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added function type parameter binding for signatures and bodies.
- Added generated C# generic method signature emission.
- Added backend fixture `0011-generic-function-api`.
- Added CLI build and C# `net48` consumer smoke for a generated generic public method.
- Recorded generic function API coverage in traceability.

Remaining:
- Generic type declaration lowering.
- Generic constraints and `where` clauses.
- Generic call expression type argument syntax.
- Type argument inference in the TypeSharp semantic model.

Blocked:
- None.
