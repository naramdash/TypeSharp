# Task: imported-csharp-member-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 00:40:03 +09:00
Source: Task 0438 roadmap refresh after local multiplicative compound assignment expressions

## Objective

Implement imported C# field/property member `*=`, `/=`, and `%=` compound assignment targets over the bounded primitive integral multiplicative policy, while preserving generated `net48`/C# 7.3 compatibility and deterministic diagnostics.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0438 rollup](tasks-rollup.md#task-0438-roadmap-refresh-after-local-multiplicative-compound-assignment-expressions)

## Scope

In:

- Support `receiver.Member *= value`, `receiver.Member /= value`, and `receiver.Member %= value` for readable/writable metadata-backed imported C# instance field/property targets.
- Support static imported C# field/property member targets where the existing assignment target policy already admits the member shape.
- Reuse the local multiplicative compound assignment policy: known non-null primitive integral numeric target/value operands and promoted result assignable back to the target type.
- Preserve single evaluation for non-trivial instance receivers before getter/setter use.
- Emit C# 7.3-compatible generated source and package-free `net48` artifacts.
- Add focused generated `net48` C# consumer coverage and deterministic negative checker coverage.
- Update docs canonical pages and agent ledgers for the accepted behavior and remaining indexer/null-conditional backlog.

Out:

- Imported C# indexer `*=`, `/=`, and `%=` targets.
- Null-conditional multiplicative compound assignment.
- Floating-point, decimal, checked-overflow, or user-defined multiplicative operator expansion.
- TypeSharp-owned member assignment policy, event targets, increment/decrement, and broad class-member body analysis.
- Task 0401's GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] Imported C# instance/static field/property member `*=`, `/=`, and `%=` targets type-check through the bounded primitive integral multiplicative policy.
- [ ] Unsupported imported multiplicative member targets report deterministic diagnostics before backend emission.
- [ ] Non-trivial imported instance member receivers are evaluated once in generated C#.
- [ ] Generated source remains C# 7.3-compatible and generated assemblies remain package-free `net48`.
- [ ] Positive and negative tests cover the new imported member behavior.
- [ ] Docs and agent ledgers record the implemented member slice and leave indexer/null-conditional multiplicative targets as future work.

## Verification

Command: TBD
Expected: focused multiplicative imported-member tests, relevant fixture/backend checks, compiler build, docs build, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0438 rechecked official signals and selected this bounded imported C# member multiplicative slice.

Remaining:

- Implement checker/backend/tests/docs for imported C# member `*=`, `/=`, and `%=` targets.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
