# Task: CSharp Source Backend Audit

Status: Done
Queue: Q3
Start Time: 2026-05-19 05:18:24 +09:00
End Time: 2026-05-19 05:20:18 +09:00

## Objective

Audit the implemented C# 7.3 source backend scope and align the top-level checklist item with backend snapshots, generated `net48` compile/build, C# consumer, public ABI, and host compatibility evidence.

## Source Of Truth

- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../lowering.md](../lowering.md)
- [../regression-testing.md](../regression-testing.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp/positive`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- current `CSharpSourceBackend.Emit` implementation
- backend snapshots `0001` through `0020`
- C# 7.3-compatible generated source shape
- generated SDK-style `net48` project compile/build path
- generated public API C# consumer smokes
- runtime helper reference shape, public ABI metadata smoke, and host compatibility smokes

Out:
- backend abstraction interface
- IL backend
- source maps from generated C# diagnostics back to TypeSharp source
- optimizer/lowering phase separation beyond the current direct source emitter
- full coverage for planned grammar not yet implemented

## Acceptance Criteria

- [x] Existing implementation and tests prove C# backend snapshot coverage for implemented features.
- [x] Existing implementation and tests prove generated source compiles with C# 7.3 in `net48` projects.
- [x] Existing implementation and tests prove generated assemblies and C# consumers work for current public API surface.
- [x] Existing implementation and tests prove host compatibility and public ABI metadata smoke paths.
- [x] Checklist and traceability distinguish completed C# source backend scope from backend abstraction and IL backend work.

## Verification

Command:

```text
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- backend snapshots, generated build/consumer smokes, host smokes, and existing suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Audited `CSharpSourceBackend`, backend snapshots `0001` through `0020`, generated `net48` compile/build smokes, public API consumer smokes, public ABI metadata smoke, and ASP.NET/WCF/worker host smokes.
- Marked the top-level `C# 7.3 source backend` checklist item complete.
- Updated traceability with exact current source backend scope and explicit out-of-scope boundaries.

Remaining:
- None.

Blocked:
- None.
