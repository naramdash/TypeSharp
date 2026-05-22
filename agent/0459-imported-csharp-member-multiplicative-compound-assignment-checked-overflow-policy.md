# Task: imported-csharp-member-multiplicative-compound-assignment-checked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 08:44:00 +09:00
End Time: TBD

## Objective

Extend checked/unchecked multiplicative compound assignment from mutable local binding targets to readable/writable metadata-backed imported C# field/property member targets while preserving generated package-free `net48`, C# 7.3-compatible lowering, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard boundary.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0458 rollup](tasks-rollup.md#task-0458-roadmap-refresh-after-local-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Support `checked(receiver.Member *= value)`, `checked(receiver.Member /= value)`, `checked(receiver.Member %= value)`, and matching `unchecked(...)` forms for metadata-backed imported C# instance/static field/property targets that are readable and writable.
- Reuse the existing bounded known non-null integral/floating-point/decimal multiplicative assign-back policy.
- Preserve deterministic diagnostics for readonly fields, missing setters, events, unsupported operands, nullable operands, mixed decimal/floating operands, narrowing assign-back, TypeSharp-owned targets, and unresolved targets.
- Lower statement-form checked/unchecked imported member assignments to C# 7.3-compatible checked/unchecked blocks without emitting newer C# syntax.
- Add focused checker/backend/generated `net48` C# consumer coverage and update canonical docs/ledgers.

Out:

- Imported C# indexer checked/unchecked multiplicative targets.
- Null-conditional checked/unchecked multiplicative targets.
- User-defined compound assignment operators or user-defined checked operators.
- TypeSharp-owned member assignment policy.
- Task 0401's GitHub Actions CI fix.

## Acceptance Criteria

- [ ] Imported C# regular member checked/unchecked multiplicative assignment type-checks for supported field/property targets and operands.
- [ ] Unsupported imported member cases report deterministic diagnostics before backend emission.
- [ ] Generated C# remains C# 7.3-compatible and package-free `net48` consumer evidence passes.
- [ ] Existing local checked/unchecked and unchecked ordinary imported member behavior is preserved.
- [ ] Docs, task ledger, traceability, and Work Ledger are updated.
- [ ] Focused tests plus required regression gates pass.

## Verification

Command: TBD
Expected: focused multiplicative/imported-member tests, generated `net48` consumer evidence, docs build, stale-reference scan, and whitespace check pass.
Result: TBD

## Handoff

Done:

- Task 0458 rechecked official roadmap signals, preserved generated package-free `net48`/C# 7.3 plus the pinned MSTest.Sdk/MTP package-shard baseline, kept Task 0401 blocked without explicit approval, and selected this imported regular member checked-overflow policy slice.

Remaining:

- Implement the imported C# regular member checked/unchecked multiplicative assignment slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
