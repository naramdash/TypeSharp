# Task: Lowering Examples Catalog

Status: Done
Queue: Q1-Q5
Start Time: 2026-05-19 04:37:28 +09:00
End Time: 2026-05-19 04:39:10 +09:00

## Objective

Document feature-by-feature lowering examples for the implemented C# 7.3 source backend so TypeSharp's high-level syntax remains explainable against the `net48` baseline.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../feasibility.md](../feasibility.md)
- [../checklist.md](../checklist.md)
- backend golden fixtures under `tests/fixtures/backend/csharp/positive`

## Scope

In:
- a `docs/lowering.md` catalog for implemented backend features
- links from `docs/README.md`
- checklist and traceability updates
- mapping each example to concrete backend fixtures or smoke tests

Out:
- future IL backend lowering
- unimplemented feature promises
- changing generated C# output
- migration guide content

## Acceptance Criteria

- [x] `docs/lowering.md` explains the current MVP backend contract.
- [x] Implemented lowering examples cover functions, literals, imports/calls, locals, records, unions, type-level union narrowing, and async `Task<T>`.
- [x] Every lowering example points to a concrete golden fixture or smoke test.
- [x] `docs/README.md`, `docs/checklist.md`, and `docs/traceability.md` link the catalog.

## Verification

Command:

```text
git diff --check
```

Expected:
- documentation diff has no whitespace errors.
- checklist and traceability reflect the lowering catalog evidence.

Result:
- Pass. `git diff --check`

## Handoff

Done:
- Added [../lowering.md](../lowering.md) as the feature-by-feature lowering catalog.
- Linked the catalog from [../README.md](../README.md).
- Marked the lowering examples checklist item complete.
- Added traceability evidence for the lowering catalog.

Remaining:
- Keep adding lowering sections when new backend features become implemented and fixture-backed.

Blocked:
- None.
