# Task: Compile-Time Literal Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:03:41 +09:00
End Time: 2026-05-19 02:07:15 +09:00

## Objective

`literal` compile-time constant declaration을 generated C# `net48` public/internal constant field로 낮춰 C# 소비자가 TypeSharp 상수를 참조할 수 있게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../feature-map.md](../feature-map.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- top-level `literal` declaration emission in the C# source backend
- string, bool, integer, floating, and decimal literal constants
- public/exported literal visibility in generated C# metadata
- backend golden fixture and generated `net48` build smoke
- C# `net48` consumer smoke for public literal fields
- checklist, traceability, and task queue updates

Out:
- enum literal constants
- constant folding across arbitrary expressions
- diagnostic for invalid `literal` initializer
- attribute argument lowering
- record/class/member lowering beyond current backend support

## Acceptance Criteria

- [x] `CSharpSourceBackend` emits top-level `literal` declarations before functions.
- [x] public/exported literals lower to C#-visible constant fields when the initializer is a C# 7.3-compatible constant literal.
- [x] backend golden fixture covers literal constant source emission.
- [x] CLI build smoke compiles generated `net48` assembly with literal constants.
- [x] C# `net48` consumer smoke references generated public literal fields.
- [x] checklist and traceability reflect implemented literal lowering.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- compiler and test projects build.
- backend fixture snapshots pass.
- `CLI build compiles literal constants` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including backend fixture snapshots and `CLI build compiles literal constants`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Implemented top-level `literal` emission in `CSharpSourceBackend`.
- Added inference for unannotated top-level literal declarations in the simple type checker.
- Added backend fixture `0007-literal-constants`.
- Added CLI build and C# `net48` consumer smoke for generated public literal fields.
- Marked compile-time constant `literal` as implemented in the MVP checklist.

Remaining:
- Enum literal constants.
- Constant folding across arbitrary expressions.
- Diagnostic for invalid `literal` initializer.
- Attribute argument lowering.

Blocked:
- None.
