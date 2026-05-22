# Task: local-multiplicative-compound-assignment-checked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 06:42:52 +09:00
End Time: TBD

## Objective

Implement a bounded local `let mut` multiplicative compound assignment checked/unchecked overflow policy over the existing `checked(...)` and `unchecked(...)` expression surface while preserving generated package-free `net48` and C# 7.3-compatible output.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0456 rollup](tasks-rollup.md#task-0456-roadmap-refresh-after-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Define the local mutable `*=`, `/=`, and `%=` checked/unchecked policy for supported known non-null primitive numeric operands.
- Preserve TypeSharp's current bounded integral/floating-point/decimal assign-back diagnostics, especially nullable, mixed decimal-floating, narrowing, immutable local, and non-local target rejection.
- Lower accepted local checked/unchecked multiplicative compound assignment forms to C# 7.3-compatible output using the existing `checked(...)`/`unchecked(...)` expression surface.
- Add focused positive/negative/backend/generated `net48` consumer evidence and update docs/ledgers as needed.
- Preserve generated package-free `net48`, current C# 7.3 source output, the 572-test MSTest.Sdk/MTP package-shard minimum, and Task 0401's blocked state.

Out:

- Imported C# member/indexer checked-overflow policy.
- Null-conditional imported C# checked-overflow policy.
- User-defined compound assignment/operator overload resolution.
- Changing generated artifact baselines, framework targets, NuGet package strategy, or CI behavior.
- Fixing Task 0401 without explicit user approval.

## Acceptance Criteria

- [ ] Local `checked(...)` and `unchecked(...)` multiplicative compound assignment behavior is implemented for bounded mutable-local `*=`, `/=`, and `%=` forms.
- [ ] Unsupported operands and targets still report deterministic diagnostics before backend emission.
- [ ] Generated C# remains C# 7.3-compatible and generated artifacts remain package-free `net48`.
- [ ] Focused positive, negative, backend, generated `net48` consumer, and relevant shard/package verification pass.
- [ ] Docs, task ledger, traceability, and Work Ledger are updated, and Task 0401 remains blocked absent explicit approval.

## Verification

Command: TBD
Expected: focused local checked/unchecked multiplicative filters, fixture coverage, generated `net48` consumer evidence, package-free catalog/shards as needed, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0456 rechecked official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals, preserved the generated package-free `net48`/C# 7.3 and MSTest.Sdk/MTP package-shard baselines, corrected user-defined compound assignment to C# 14 stable backlog, and kept Task 0401 blocked.

Remaining:

- Implement the local checked/unchecked overflow policy for bounded multiplicative compound assignment and add the required evidence.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
