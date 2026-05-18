# Task: Lowering Pass Pipeline

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 05:59:22 +09:00
End Time: 2026-05-19 07:52:04 +09:00

## Objective

Add a concrete lowering pass seam so TypeSharp high-level syntax can be normalized before backend emit without burying every lowering concern inside the C# source backend emitter.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../lowering.md](../lowering.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- ordered lowering pass interface and pipeline
- default pipeline integration before C# source backend emit
- first idempotent runtime helper import lowering pass
- smoke test for pass order, duplicate prevention, and backend output stability
- docs/checklist/traceability/task index updates

Out:
- direct IL lowering
- semantic IR replacement for the full backend
- changing generated public ABI
- changing generated C# snapshots

## Acceptance Criteria

- [x] Compiler exposes an ordered lowering pass pipeline.
- [x] C# source backend runs the default lowering pipeline before emission.
- [x] Runtime helper import lowering is idempotent and tested.
- [x] Existing generated C# output remains stable.
- [x] Checklist, architecture, lowering, traceability, and task index are updated.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
git diff --check
```

Expected:
- compiler and tests build without warnings or errors.
- full smoke suite passes, including `lowering pipeline injects runtime helper imports`.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`.
- Pass. `git diff --check`.

## Handoff

Done:
- Added `ITypeSharpLoweringPass`, `TypeSharpLoweringPipeline`, and `CSharpRuntimeImportLoweringPass`.
- Routed C# source backend emission through the default lowering pipeline.
- Added smoke coverage for pass order, runtime helper import idempotence, and backend output stability.
- Added optional test harness filtering and explicit output flushing to make long smoke runs easier to isolate.
- Marked `lowering passes` complete in the checklist and linked the implementation from architecture, lowering, and traceability docs.

Remaining:
- None.

Blocked:
- None.
