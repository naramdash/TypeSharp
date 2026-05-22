# Task: roadmap-refresh-after-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 03:08:00 +09:00
Source: Task 0445 imported C# null-conditional indexer multiplicative compound assignment targets

## Objective

Recheck official language, platform, package, editor, and CI signals after imported C# null-conditional indexer multiplicative compound assignment targets landed, then update the roadmap and select the next bounded TypeSharp implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0445 rollup](tasks-rollup.md#task-0445-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-targets)

## Scope

In:

- Recheck current official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing/MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions source signals.
- Confirm whether the generated package-free `net48`/C# 7.3 baseline or the existing `net10.0` MSTest.Sdk/MTP package bridge should change.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Update Feature Status, Project Policy, Work Ledger, tasks, and traceability.
- Select the next bounded implementation task and create its active packet.

Out:

- Implementing the next compiler slice during this refresh task.
- Adding a duplicate test-host framework unless the refresh finds distinct evidence that changes the package strategy.
- Fixing Task 0401 without explicit user approval.

## Acceptance Criteria

- [ ] Official source signals are rechecked and summarized with stable/preview boundaries.
- [ ] Generated `net48`/C# 7.3 and package-host decisions are reaffirmed or updated with evidence.
- [ ] The next bounded implementation task is selected.
- [ ] Docs and agent ledgers are updated consistently.
- [ ] Task 0401 remains blocked unless explicit user approval is given.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0445 implemented imported C# instance indexer `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` targets, added generated package-free `net48` consumer and negative checker coverage, and raised the shared catalog to 566 cases with a 570-test MTP package-shard minimum.

Remaining:

- Perform the official-source refresh and choose the next implementation slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
