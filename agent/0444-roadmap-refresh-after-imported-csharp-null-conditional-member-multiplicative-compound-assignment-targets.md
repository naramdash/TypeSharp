# Task: roadmap-refresh-after-imported-csharp-null-conditional-member-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 02:21:43 +09:00
Source: Task 0443 imported C# null-conditional member multiplicative compound assignment targets

## Objective

Recheck official language, platform, package, test-host, editor, and CI signals after imported C# null-conditional member multiplicative compound assignment landed, confirm TypeSharp's `net48`/C# 7.3 generated-artifact baseline and `net10.0` MSTest.Sdk/MTP package bridge still hold, and select the next bounded roadmap slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0443 rollup](tasks-rollup.md#task-0443-imported-csharp-null-conditional-member-multiplicative-compound-assignment-targets)

## Scope

In:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions sources listed by Project Policy.
- Separate stable signals from preview/watch signals and keep generated artifacts package-free `net48`/C# 7.3 unless official evidence and project policy justify a change.
- Compare the refreshed signals with Feature Status, Project Policy, Work Ledger, task rollup, and current queue state.
- Keep Task 0401's GitHub Actions `npm` process-launch fix blocked unless the user explicitly approves implementation.
- Select the next bounded implementation or planning slice, likely the paired imported C# null-conditional indexer multiplicative compound assignment target slice if no higher-priority baseline drift appears.

Out:

- Implementing the next compiler feature in this refresh task.
- Fixing Task 0401 without explicit user approval.
- Adding a duplicate xUnit.net or NUnit bridge while the existing MSTest.Sdk/MTP bridge remains sufficient.
- Changing TypeSharp generated artifacts away from package-free `net48`/C# 7.3 output.

## Acceptance Criteria

- [ ] Official sources are rechecked and cited in the rollup.
- [ ] Stable versus preview/watch signals are separated.
- [ ] Generated artifact baseline, MSTest.Sdk/MTP package bridge, package-shard minimums, and CI/editor watch items are either reaffirmed or explicitly updated.
- [ ] Feature Status, Project Policy, Work Ledger, `agent/tasks.md`, `agent/traceability.md`, and `agent/tasks-rollup.md` remain consistent.
- [ ] The next bounded latest-five roadmap slice is selected without unblocking Task 0401 absent explicit approval.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0443 implemented imported C# null-conditional member `*=`, `/=`, and `%=` targets with explicit C# 7.3 null guards, skipped right-side evaluation on null receivers, generated package-free `net48` C# consumer coverage, deterministic negative checker coverage, and catalog/test-host count updates.

Remaining:

- Perform the official-source refresh and select the next bounded roadmap slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
