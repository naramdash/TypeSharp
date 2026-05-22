# Task: imported-csharp-null-conditional-member-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 02:03:55 +09:00
Source: Task 0442 roadmap refresh after imported C# indexer multiplicative compound assignment targets

## Objective

Implement imported C# null-conditional member multiplicative compound assignment targets, `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value`, for readable/writable metadata-backed imported C# instance field/property targets while preserving generated package-free `net48`/C# 7.3 output, deterministic diagnostics, and the current `net10.0` MSTest.Sdk/MTP package bridge.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0442 rollup](tasks-rollup.md#task-0442-roadmap-refresh-after-imported-csharp-indexer-multiplicative-compound-assignment-targets)

## Scope

In:

- Type-check `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` for metadata-backed imported C# instance fields/properties when the member is readable and writable.
- Reuse the existing primitive integral multiplicative assign-back policy: known non-null primitive integral operands and promoted result assignable back to the member type.
- Lower accepted member targets through explicit C# 7.3-compatible null guards, evaluate the receiver once, skip the right side when the receiver is null, and emit no C# `?.`.
- Add focused generated package-free `net48` C# consumer coverage and deterministic negative checker coverage.
- Update canonical docs, task ledgers, catalog counts, shard expectations, and package-shard minimums if catalog coverage changes.

Out:

- Null-conditional indexer multiplicative compound assignment `receiver?[index] *=`, `/=`, and `%=`.
- Floating-point or decimal multiplicative compound-assignment policy expansion.
- Checked-overflow policy changes, user-defined multiplicative operators, event targets, static targets, local/TypeSharp-owned null-conditional targets, invocation/chains, increment/decrement, and Task 0401's CI fix.
- Adding a duplicate xUnit.net or NUnit package bridge.

## Acceptance Criteria

- [ ] Imported C# null-conditional member `*=`, `/=`, and `%=` targets accepted only for readable/writable metadata-backed instance field/property targets under the bounded primitive integral assign-back policy.
- [ ] Generated C# is C# 7.3-compatible, evaluates the receiver once, skips right-side evaluation when the receiver is null, emits no C# null-conditional assignment syntax, and builds as package-free `net48`.
- [ ] Unsupported bool/string/enum/nullable/narrowing/readonly/event/static/indexer/local/TypeSharp-owned/unresolved targets report deterministic diagnostics before backend emission.
- [ ] Positive generated consumer coverage and negative checker coverage are added to the shared catalog, with MSTest bridge/shard expectations updated if needed.
- [ ] Feature Status, Type System, Lowering, Diagnostics, .NET Interop, Project Policy, Work Ledger, `agent/tasks.md`, `agent/traceability.md`, and `agent/tasks-rollup.md` are consistent.

## Verification

Command: TBD
Expected: focused Task 0443 filters, relevant build/catalog checks, docs build if docs change, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0442 rechecked official language/platform/package/tooling signals after imported C# indexer multiplicative compound assignment landed and selected this bounded null-conditional member multiplicative slice.

Remaining:

- Implement checker/backend behavior, tests, docs, and ledger updates.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
