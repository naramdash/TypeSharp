# Task: IL Backend Artifact Seam

Status: Done
Queue: Q3
Start Time: 2026-05-19 07:53:56 +09:00
End Time: 2026-05-19 07:56:52 +09:00

## Objective

Extend the backend abstraction so a future direct IL backend can return a binary assembly artifact without overloading the current C# source text backend contract.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../requirements.md](../requirements.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [0110-backend-abstraction-seam.md](0110-backend-abstraction-seam.md)

## Scope

In:
- backend artifact kind contract for source text versus direct assembly output
- source backend adapter update
- current project builder guard that accepts source-text artifacts only
- smoke tests for source backend artifact shape and future assembly artifact shape
- docs/checklist/traceability/task index updates

Out:
- direct IL emit implementation
- PDB/debug symbol generation
- replacing the generated C# project build path
- changing generated public ABI

## Acceptance Criteria

- [x] Backend abstraction distinguishes source-text artifacts from assembly artifacts.
- [x] C# source backend reports and returns source-text artifacts.
- [x] Future `.dll` assembly artifact shape is testable without implementing direct IL emit.
- [x] Current builder explicitly rejects non-source artifacts until direct assembly build flow exists.
- [x] Checklist, architecture, traceability, and task index are updated.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
git diff --check
```

Expected:
- compiler and tests build without warnings or errors.
- full smoke suite passes, including `backend artifact contract supports direct assembly output`.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`.
- Pass. `git diff --check`.

## Handoff

Done:
- Added `TypeSharpBackendArtifactKind` and `TypeSharpBackendArtifact` to distinguish source text from future assembly output.
- Updated `ITypeSharpBackend`, `CSharpSourceBackendAdapter`, and `TypeSharpBuilder` to use backend artifacts while preserving the current C# source build path.
- Added smoke coverage for source backend artifacts and `.dll` assembly artifact shape.
- Marked `IL backend abstraction seam` complete in the checklist and linked the contract from architecture and traceability docs.

Remaining:
- None.

Blocked:
- None.
