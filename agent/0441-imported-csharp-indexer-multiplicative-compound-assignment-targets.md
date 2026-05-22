# Task: imported-csharp-indexer-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 01:17:08 +09:00
Source: Task 0440 roadmap refresh after imported C# member multiplicative compound assignment targets

## Objective

Implement imported C# indexer `*=`, `/=`, and `%=` compound assignment targets over the bounded primitive integral multiplicative policy, while preserving generated `net48`/C# 7.3 compatibility, single receiver/index-argument evaluation, and deterministic diagnostics.

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
- [Task 0440 rollup](tasks-rollup.md#task-0440-roadmap-refresh-after-imported-csharp-member-multiplicative-compound-assignment-targets)

## Scope

In:

- Support `receiver[index] *= value`, `receiver[index] /= value`, and `receiver[index] %= value` for metadata-backed imported C# instance indexer targets when overload resolution selects a public getter/setter pair.
- Reuse the multiplicative compound assignment policy from local/imported member targets: known non-null primitive integral numeric target/value operands and promoted result assignable back to the indexer value type.
- Reuse existing imported indexer argument validation and ranking for supported index arguments, mismatches, and ambiguity.
- Preserve single evaluation for non-trivial indexer receivers and index arguments in generated C#.
- Emit C# 7.3-compatible generated source and package-free `net48` artifacts.
- Add focused generated `net48` C# consumer coverage and deterministic negative checker coverage.
- Update docs canonical pages and agent ledgers for the accepted indexer slice and remaining null-conditional/user-defined backlog.

Out:

- Null-conditional multiplicative compound assignment.
- Floating-point, decimal, checked-overflow, or user-defined multiplicative operator expansion.
- TypeSharp-owned indexer/member assignment policy, event targets, increment/decrement, and broad class-member body analysis.
- Task 0401's GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] Imported C# instance indexer `*=`, `/=`, and `%=` targets type-check through the bounded primitive integral multiplicative policy.
- [ ] Unsupported imported multiplicative indexer targets report deterministic diagnostics before backend emission.
- [ ] Non-trivial imported indexer receivers and index arguments are evaluated once in generated C#.
- [ ] Generated source remains C# 7.3-compatible and generated assemblies remain package-free `net48`.
- [ ] Positive and negative tests cover the new imported indexer behavior.
- [ ] Docs and agent ledgers record the implemented indexer slice and leave null-conditional multiplicative targets as future work.

## Verification

Command: TBD
Expected: focused multiplicative imported-indexer tests, relevant compiler build, docs build, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0440 rechecked official signals and selected this bounded imported C# indexer multiplicative slice.

Remaining:

- Implement checker/backend/tests/docs for imported C# indexer `*=`, `/=`, and `%=` targets.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
