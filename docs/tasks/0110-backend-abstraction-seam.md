# Task: Backend Abstraction Seam

Status: Done
Queue: Q3
Start Time: 2026-05-19 05:39:54 +09:00
End Time: 2026-05-19 05:42:32 +09:00

## Objective

Add a concrete compiler backend abstraction so the builder emits through a backend seam instead of calling the C# source backend directly, while leaving direct IL backend work separate.

## Source Of Truth

- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend`
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- `ITypeSharpBackend` seam
- C# source backend adapter
- builder source emission through the backend interface
- smoke test proving the adapter preserves current C# backend output
- checklist and traceability update for backend abstraction

Out:
- direct IL backend
- IL backend abstraction seam
- source map diagnostics
- multi-file/multi-artifact backend contract beyond generated source emission
- changing backend snapshots

## Acceptance Criteria

- [x] A compiler-owned backend interface exists.
- [x] The current C# source backend is reachable through an adapter implementing that interface.
- [x] `TypeSharpBuilder` uses the backend interface for generated source emission.
- [x] Tests prove the adapter emits the same C# as the existing backend.
- [x] Existing backend snapshots and generated `net48` build smokes still pass.
- [x] Checklist and traceability distinguish backend abstraction from the direct IL backend seam.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- compiler, CLI, language server, and tests build without warnings.
- backend abstraction smoke, backend snapshots, generated build smokes, and existing suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `ITypeSharpBackend`.
- Added `CSharpSourceBackendAdapter`.
- Routed `TypeSharpBuilder` generated source emission through the backend interface.
- Added a backend abstraction smoke test.
- Marked `backend abstraction` complete for the current source-emission seam.

Remaining:
- None.

Blocked:
- None.
