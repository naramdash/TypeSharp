# Task: Local Expression Inference Engine

Status: Done
Queue: Q2
Start Time: 2026-05-19 07:57:35 +09:00
End Time: 2026-05-19 08:02:42 +09:00

## Objective

Extract the current local expression inference logic from the type checker into a dedicated inference engine so the compiler has a concrete seam for extending inference beyond explicit annotations.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../feature-specs.md](../feature-specs.md)

## Scope

In:
- local expression inference engine for literal, identifier, direct call, and basic binary expressions
- type checker delegation to the inference engine
- preservation of existing mismatch/nullability/union/structural diagnostics
- smoke test proving unannotated local expression flow produces concrete inferred mismatch diagnostics
- docs/checklist/traceability/task index updates

Out:
- generic method inference
- contextual lambda inference
- overload conversion ranking
- project-wide flow inference
- Hindley-Milner-style whole-program inference

## Acceptance Criteria

- [x] Inference logic is separated from `TypeSharpTypeChecker`.
- [x] Type checker uses the inference engine for local expression checks.
- [x] Unannotated literal, identifier, call, and comparison flows infer concrete types.
- [x] Existing type checker, backend, CLI, LSP, and generated `net48` smokes still pass.
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
- full smoke suite passes, including `inference engine infers local expression graph`.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`.
- Pass. `git diff --check`.

## Handoff

Done:
- Added `TypeSharpInferenceEngine`, `ITypeSharpInferenceScope`, and top-level `SimpleType`.
- Updated `TypeSharpTypeChecker` to delegate local expression inference for literals, identifiers, direct calls, and basic binary expressions.
- Added `inference engine infers local expression graph` smoke coverage for unannotated literal, call, and comparison flows.
- Marked `inference engine` complete in the checklist and linked the scope from architecture and traceability docs.

Remaining:
- None.

Blocked:
- None.
