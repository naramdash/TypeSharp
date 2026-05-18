# Task: Core Option Result Generic API Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:24:29 +09:00
End Time: 2026-05-19 02:27:54 +09:00

## Objective

`TypeSharp.Core.Option<T>`와 `Result<T,E>`가 TypeSharp generated public API signature에서 generic type으로 보존되고 C# `net48` consumer가 해당 API를 호출할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../grammar/types.md](../grammar/types.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Core`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generic type annotation emission in generated C# signatures.
- simple type checker representation for generic type names.
- backend golden fixture for `Option<string>` and `Result<int,string>` pass-through functions.
- CLI build smoke using `TypeSharp.Core.dll` as a local manifest reference.
- C# `net48` consumer smoke calling generated generic Core type APIs.
- checklist and traceability updates for `Option<T>` and `Result<T,E>`.

Out:
- generic function declarations.
- type argument inference.
- static constructor/case shorthand lowering such as `Some(...)` or `Ok(...)`.
- pattern matching over `Option<T>`/`Result<T,E>`.
- nullability conversion helpers.

## Acceptance Criteria

- [x] backend maps generic `TypeName` syntax to C# generic type syntax.
- [x] type checker keeps simple generic type names comparable for pass-through functions.
- [x] backend fixture covers generated signatures for `Option<T>` and `Result<T,E>`.
- [x] CLI build smoke compiles a generated `net48` assembly referencing `TypeSharp.Core`.
- [x] C# `net48` consumer smoke calls generated public APIs with `Option<T>` and `Result<T,E>`.
- [x] checklist and traceability reflect generated API support for `Option<T>` and `Result<T,E>`.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- Core and test projects build.
- backend fixture snapshots pass.
- `CLI build compiles core option result APIs` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles core option result APIs`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added generic type annotation emission for generated C# signatures.
- Added simple type checker support for comparable generic type names.
- Added backend fixture `0010-core-option-result-api`.
- Added CLI build and C# `net48` consumer smoke for generated `Option<T>`/`Result<T,E>` public APIs.
- Marked `Option<T>` and `Result<T,E>` complete in the MVP checklist.

Remaining:
- Generic function declarations.
- Type argument inference.
- Case shorthand lowering such as `Some(...)` or `Ok(...)`.
- Pattern matching over `Option<T>`/`Result<T,E>`.
- Nullability conversion helpers.

Blocked:
- None.
