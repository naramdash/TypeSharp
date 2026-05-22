# Task: roadmap-refresh-after-imported-csharp-null-conditional-shift-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 23:18:00 +09:00
Source: Task 0433 imported C# null-conditional shift compound assignment member targets

## Objective

Recheck official language/platform/package/test/editor/CI signals after imported C# null-conditional shift compound assignment member targets, preserve TypeSharp's generated package-free `net48`/C# 7.3 baseline, keep Task 0401 blocked without explicit approval, and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0433 rollup](tasks-rollup.md#task-0433-imported-csharp-null-conditional-shift-compound-assignment-member-targets)

## Scope

In:

- Recheck current official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals listed in Project Policy.
- Confirm whether generated artifacts still stay package-free `net48` and generated C# still stays C# 7.3-compatible.
- Compare the latest official signals against Feature Status, Work Ledger, and the current compiler surface.
- Keep Task 0401 blocked unless the user explicitly approves implementing the GitHub Actions `npm` process-launch fix.
- Select the next bounded implementation slice and update tasks, rollup, traceability, and docs ledgers.

Out:

- Implementing Task 0401 without explicit approval.
- Adding package dependencies to generated artifacts.
- Changing generated target frameworks, adopting .NET 10/11-only runtime APIs, or making preview features stable.
- Implementing the next compiler slice in this roadmap refresh task.

## Acceptance Criteria

- [ ] Official source signals are rechecked and summarized with stable vs preview boundaries.
- [ ] Generated `net48`/C# 7.3 and package/test-host boundaries are reaffirmed or updated with evidence.
- [ ] The next bounded implementation slice is selected from current TypeSharp gaps.
- [ ] Task 0401 remains blocked unless explicit approval is present.
- [ ] `agent/tasks.md`, `agent/tasks-rollup.md`, `agent/traceability.md`, and docs ledgers agree on the new active task.

## Verification

Command: TBD
Expected: official source links checked, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0433 implemented imported C# null-conditional shift compound assignment member targets with generated `net48` consumer and negative checker coverage.

Remaining:

- Recheck official signals and select the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
