# Task: Constructed Generic Function Call Inference Slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 20:39:46 +09:00
End Time: TBD

## Objective

Extend direct TypeSharp-declared generic calls to infer and substitute bounded constructed parameter/return types.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Task 0355 roadmap refresh](tasks-rollup.md#task-0355-roadmap-refresh-after-direct-generic-function-call-inference)

## Scope

In:
- Direct `f(args...)` calls to TypeSharp-declared generic functions.
- Bounded inference for constructed parameter positions such as `T[]` and matching single-argument generic wrappers.
- Return-type substitution for matching constructed returns after explicit or inferred type arguments.
- Deterministic `TS2201` diagnostics for mismatched explicit/inferred constructed generic arguments.
- Type checker fixtures, backend lowering smoke, CLI build smoke, and docs updates.

Out:
- Imported C# generic call validation.
- Generic pipeline/composition inference.
- Broader generic constraints or overload ranking.
- Optional/default/params TypeSharp parameter policy.
- Higher-order calls, currying, partial application, and type constructor policy.
- General type-constructor unification beyond bounded exact-shape constructed types.

## Acceptance Criteria

- [ ] Direct generic calls infer `T` through supported constructed parameter shapes.
- [ ] Explicit and inferred calls substitute supported constructed return shapes.
- [ ] Repeated constructed inference conflicts report deterministic `TS2201`.
- [ ] Existing simple generic inference behavior remains stable.
- [ ] Positive and negative type-checker fixtures cover the new slice.
- [ ] Generated C# lowering remains C# 7.3-compatible and smoke-tested.
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
