# Task: Direct Generic Pipeline Inference Slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 21:02:17 +09:00
End Time: TBD

## Objective

Extend direct pipeline checks to TypeSharp-declared generic function targets using bounded direct-call inference semantics.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Task 0357 roadmap refresh](tasks-rollup.md#task-0357-roadmap-refresh-after-constructed-generic-function-call-inference)

## Scope

In:
- Direct `value |> f` and `value |> f(args...)` where `f` is a known TypeSharp-declared generic function.
- Inference from the piped input and non-piped arguments for simple and bounded constructed parameter shapes.
- Substituted generic pipeline return types.
- Deterministic `TS2201` diagnostics for generic pipeline arity, input, non-piped argument, and repeated inference conflicts.
- Type checker fixtures, pipeline backend smoke, docs, and operational ledger updates.

Out:
- Imported C# pipeline targets.
- Generic composition inference.
- Higher-order pipeline targets, function values, currying, and partial application.
- Optional/default/params TypeSharp parameter policy.
- Pipeline overload ranking and broader type-constructor unification.

## Acceptance Criteria

- [ ] Generic pipeline targets infer type arguments from piped input.
- [ ] Generic pipeline targets infer/check non-piped arguments after substitution.
- [ ] Supported constructed generic parameter and return shapes work in pipelines.
- [ ] Repeated generic inference conflicts and explicit mismatches report deterministic `TS2201`.
- [ ] Existing non-generic pipeline behavior remains stable.
- [ ] Fixtures, backend snapshot, and CLI smoke cover the slice.
- [ ] Canonical docs and operational ledgers reflect the completed boundary.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: Build succeeds.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
Expected: Regression suite succeeds.
Result: TBD

Command: `npm run build`
Expected: Docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: No whitespace errors.
Result: TBD

## Handoff

Done:
Remaining:
Blocked:
