# Task: imported-csharp-indexer-multiplicative-compound-assignment-checked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 08:22:00 +09:00
End Time: TBD

## Objective

Implement imported C# regular indexer multiplicative compound assignment `checked(...)` and `unchecked(...)` wrappers for metadata-backed instance indexer targets, preserving generated package-free `net48`, C# 7.3-compatible checked/unchecked block lowering, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard baseline.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0460 rollup](tasks-rollup.md#task-0460-roadmap-refresh-after-imported-csharp-regular-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Support statement-form `checked(receiver[index] *= value)`, `checked(receiver[index] /= value)`, `checked(receiver[index] %= value)`, `unchecked(receiver[index] *= value)`, `unchecked(receiver[index] /= value)`, and `unchecked(receiver[index] %= value)` for metadata-backed imported C# instance indexers.
- Require overload resolution to select a public getter/setter indexer pair with supported index arguments.
- Reuse the bounded known non-null integral/floating-point/decimal multiplicative assign-back policy already used by local targets, imported regular members, imported regular indexers, and null-conditional multiplicative targets.
- Lower accepted statement forms to C# 7.3 `checked { ... }` and `unchecked { ... }` blocks while preserving ordinary generated `net48` indexer compound-assignment output and single receiver/index-argument emission.
- Preserve deterministic diagnostics for unsupported operands, nullable operands, mixed decimal-floating operands, narrowing assign-back, missing setters, unsupported/ambiguous indexers, TypeSharp-owned targets, null-conditional targets, and unsupported checked-overflow shapes.

Out:

- Null-conditional checked-overflow targets.
- TypeSharp-owned indexer/member assignment policy.
- User-defined compound assignment operators.
- Task 0401's GitHub Actions CI fix without explicit user approval.

## Acceptance Criteria

- [ ] Imported C# regular indexer checked/unchecked multiplicative assignment wrappers type-check only for the supported metadata-backed shape.
- [ ] Unsupported imported indexer checked/unchecked shapes produce deterministic diagnostics before backend emission.
- [ ] Backend output for accepted statement forms uses C# 7.3 checked/unchecked blocks and preserves single receiver/index-argument emission.
- [ ] Generated package-free `net48`, C# 7.3 lowering, and the 572-test package-shard baseline are preserved.
- [ ] Focused compiler/backend/generated `net48` evidence plus package-free and package-shard verification pass.
- [ ] Docs, task ledger, traceability, and Work Ledger are updated.

## Verification

Command: TBD
Expected: focused multiplicative/indexer checks, generated `net48` evidence, package-free catalog, MTP package shards, docs build, stale-reference scan, and whitespace check pass.
Result: TBD

## Handoff

Done:

- Task 0460 rechecked official language/platform/package/testing/editor/CI signals after imported C# regular member checked/unchecked multiplicative assignment landed, preserved generated package-free `net48`, C# 7.3 lowering, the 572-test MTP package-shard baseline, kept Task 0401 blocked, and selected this imported C# regular indexer checked-overflow slice.

Remaining:

- Implement the imported C# regular indexer checked/unchecked multiplicative assignment policy and update focused evidence.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
