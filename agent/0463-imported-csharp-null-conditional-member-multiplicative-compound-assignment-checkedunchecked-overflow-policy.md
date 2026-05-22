# Task: imported-csharp-null-conditional-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 08:41:20 +09:00
End Time: TBD

## Objective

Implement imported C# null-conditional member `checked(receiver?.Member *= value)`, `checked(receiver?.Member /= value)`, `checked(receiver?.Member %= value)`, and unchecked counterparts for metadata-backed instance field/property targets, preserving generated package-free `net48`, C# 7.3-compatible null-guard lowering, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard baseline.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0462 rollup](tasks-rollup.md#task-0462-roadmap-refresh-after-imported-csharp-regular-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Support statement-form checked/unchecked wrappers for imported C# null-conditional instance field/property member multiplicative assignment.
- Require a readable/writable metadata-backed instance field/property target and a nullable/reference-like receiver shape already accepted by the existing null-conditional member multiplicative slice.
- Reuse the bounded known non-null integral/floating-point/decimal multiplicative assign-back policy.
- Lower accepted statement forms to C# 7.3-compatible checked/unchecked null guards, ensuring the overflow context applies inside the generated guard body, the receiver is evaluated once, and the right side is skipped on null receivers.
- Preserve deterministic diagnostics for unsupported operands, nullable operands, mixed decimal-floating operands, narrowing assign-back, readonly/missing-setter/event/static/indexer/local/TypeSharp-owned/unresolved targets, unsupported checked-overflow shapes, and null-conditional indexer checked-overflow targets not in this slice.

Out:

- Null-conditional indexer checked-overflow policy.
- TypeSharp-owned null-conditional assignment policy.
- User-defined compound assignment operators.
- Changing generated target framework, generated C# language version, or test-host package baseline.
- Implementing Task 0401's GitHub Actions CI fix without explicit user approval.

## Acceptance Criteria

- [ ] `checked(...)` and `unchecked(...)` wrappers are accepted only for supported imported C# null-conditional field/property member `*=`, `/=`, and `%=` statement targets.
- [ ] Generated C# remains package-free `net48` and C# 7.3-compatible, with checked/unchecked overflow context applied inside the null guard body rather than relying on preview C# null-conditional assignment syntax.
- [ ] Existing receiver single-evaluation and skipped right-side evaluation on null receivers are preserved.
- [ ] Unsupported operands, unsupported targets, null-conditional indexers, and broader checked-overflow shapes report deterministic diagnostics before backend emission.
- [ ] Focused package-free coverage, generated `net48` C# consumer evidence, package-shard smoke, docs build, stale-reference scan, and whitespace check pass.

## Verification

Command: TBD
Expected: focused compiler coverage, generated `net48` consumer evidence, package-free catalog smoke, MTP package-shard smoke, docs build, stale-reference scan, and whitespace check pass.
Result: TBD

## Handoff

Done:

- Task 0462 rechecked official language/platform/package/testing/editor/CI signals after imported C# regular indexer checked/unchecked multiplicative assignment landed, reaffirmed the generated package-free `net48`/C# 7.3 and 572-test package-shard baselines, kept Task 0401 blocked, and selected this null-conditional member checked/unchecked slice.

Remaining:

- Implement and verify the bounded null-conditional member checked/unchecked policy.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
