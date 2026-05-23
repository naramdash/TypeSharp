# Task: imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0464 roadmap refresh after imported C# null-conditional member multiplicative compound assignment checked/unchecked overflow policy

## Objective

Implement `checked(receiver?[index] *= value)`, `checked(receiver?[index] /= value)`, `checked(receiver?[index] %= value)`, and unchecked counterparts for metadata-backed imported C# instance indexer targets, preserving generated package-free `net48`, C# 7.3-compatible nested null/index guard lowering, deterministic diagnostics, and the current 572-test MSTest.Sdk/MTP package-shard baseline.

## Context

- Task 0463 implemented checked/unchecked wrappers for imported C# null-conditional member multiplicative compound assignment.
- Existing null-conditional indexer multiplicative assignment supports `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` for readable/writable metadata-backed imported C# instance indexers with supported arguments.
- Existing bounded multiplicative assign-back policy covers known non-null integral, floating-point, and decimal operands and rejects nullable operands, mixed decimal-floating operands, narrowing assign-back, bool/string/enum targets, missing setters, unsupported operands, and unsupported shapes before backend emission.
- Checked/unchecked overflow context must be inside generated guard bodies. An outer C# checked block does not flow into nested lambda bodies.
- The shared catalog remains 568 cases with four 142-case package-free shards and the package-shard MTP minimum remains 572 tests.

## Scope

- Accept statement-form `checked(...)` and `unchecked(...)` wrappers around imported C# null-conditional indexer `*=`, `/=`, and `%=` assignments when overload resolution selects a public getter/setter pair with supported index arguments.
- Reuse the current bounded known non-null integral/floating-point/decimal multiplicative assign-back policy.
- Preserve single receiver evaluation and evaluate index arguments plus the right side only when the receiver is non-null.
- Lower accepted forms to C# 7.3-compatible outer receiver and inner index guard bodies whose non-null/non-skipped branch contains the checked or unchecked assignment body.
- Keep deterministic diagnostics for unsupported operands, nullable operands, mixed decimal-floating operands, narrowing assign-back, missing setters, unsupported or ambiguous indexers, TypeSharp-owned/local/static-like/unresolved targets, and unsupported checked-overflow shapes.
- Update Type System, Lowering, Diagnostics, .NET Interop, Feature Status, Work Ledger, tasks, traceability, and rollup state as needed.

## Out Of Scope

- User-defined multiplicative compound assignment operators.
- TypeSharp-owned indexer assignment policy.
- Null-conditional invocation, chains, events, static targets, increment/decrement, and broader assignment target analysis.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance

- [ ] Positive coverage accepts checked/unchecked null-conditional indexer `*=`, `/=`, and `%=` over readable/writable metadata-backed imported C# instance indexers with supported arguments.
- [ ] Backend/generated C# evidence shows C# 7.3-compatible nested guard lowering, single receiver/index evaluation, skipped index/right-side evaluation on null receivers, and checked/unchecked assignment bodies inside the guard body.
- [ ] Negative coverage preserves deterministic diagnostics for unsupported operands, missing setters, bad/ambiguous indexers, unsupported targets, and unsupported checked-overflow shapes.
- [ ] Generated artifacts remain package-free `net48`; shared catalog and package-shard baselines remain documented or updated with evidence.
- [ ] `agent/tasks.md`, `agent/traceability.md`, docs, and rollup state are updated.
- [ ] Verification covers focused filters, full compiler catalog, MSTest package shards, docs build, stale-reference scan, and whitespace check.

## References

- [Task 0464 rollup](tasks-rollup.md#task-0464-roadmap-refresh-after-imported-csharp-null-conditional-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)

## Handoff Notes

- Start by re-reading `agent.md`, `agent/tasks.md`, `docs/src/content/docs/goal.md`, and `agent/agentic-execution.md`.
- Reuse the Task 0463 null-conditional member checked/unchecked guard-body pattern, but keep indexer argument validation and nested index guard behavior from the existing null-conditional indexer multiplicative slice.
- Do not use Python.
