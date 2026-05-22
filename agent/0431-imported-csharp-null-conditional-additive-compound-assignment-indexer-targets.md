# Task: imported-csharp-null-conditional-additive-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 22:02:14 +09:00
Source: Task 0430 roadmap refresh after imported C# null-conditional additive compound assignment member targets

## Objective

Implement `receiver?[index] += value` and `receiver?[index] -= value` for readable/writable metadata-backed imported C# instance indexer targets while preserving generated package-free `net48` artifacts.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Task 0430 rollup](tasks-rollup.md#task-0430-roadmap-refresh-after-imported-csharp-null-conditional-additive-compound-assignment-member-targets)

## Scope

In:

- Add checker support for `receiver?[index] += value` and `receiver?[index] -= value` when metadata-backed overload resolution selects a readable/writable imported C# instance indexer with supported arguments.
- Reuse the bounded additive primitive integral target/value policy from null-conditional member additive compound assignment.
- Preserve single receiver and index-argument evaluation; skip index arguments and the right side when the receiver is null.
- Lower accepted targets through C# 7.3-compatible `System.Func` null/index guards and ordinary C# `+=`/`-=` in the non-null branch.
- Add generated `net48` C# consumer coverage for accepted target/value shapes, skipped evaluation, and non-trivial receiver/index single-evaluation behavior.
- Add deterministic negative coverage for unsupported operands, missing setters, mismatched/ambiguous indexer arguments, static/event/member-only targets, TypeSharp-owned targets, and unsupported null-conditional additive forms.
- Update catalog/shard expectations, docs, Work Ledger, tasks, and traceability.

Out:

- Implementing other null-conditional compound operators beyond the additive indexer slice.
- Supporting events, static targets, local binding targets, TypeSharp-owned member/indexer targets, invocation, chains, increment/decrement, user-defined operators, or broad class-member body analysis.
- Changing generated artifact target frameworks, adding NuGet dependencies to generated `net48` artifacts, or replacing the existing `net10.0` MSTest.Sdk/MTP package bridge.
- Implementing Task 0401 without explicit user approval.

## Acceptance Criteria

- [ ] `receiver?[index] += value` and `receiver?[index] -= value` type-check for supported readable/writable imported C# instance indexers.
- [ ] Accepted generated C# stays C# 7.3-compatible and emits no `?[]` on the assignment target.
- [ ] Receiver and index arguments are evaluated once, and index/right-side expressions are skipped when the receiver is null.
- [ ] Unsupported additive indexer targets and values fail with deterministic diagnostics before backend emission.
- [ ] Shared catalog counts, package-free shard counts, and package-shard MTP minimums are updated when tests are added.
- [ ] Type System, Lowering, Diagnostics, .NET Interop, Feature Status, Work Ledger, tasks, and traceability are updated.
- [ ] Relevant compiler, package-shard, docs, and diff verification commands pass.

## Verification

Command: TBD
Expected: compiler build, focused positive/negative additive indexer tests, preserved nearby null-conditional member/indexer assignment tests, shard/package counts, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0430 selected this bounded implementation slice after official-source review.

Remaining:

- Implement checker/backend support, add tests, update docs/ledgers, verify, then roll this packet into [tasks-rollup.md](tasks-rollup.md).

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
