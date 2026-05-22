# Task: roadmap-refresh-after-imported-csharp-null-conditional-shift-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 23:59:00 +09:00
Source: Task 0435 imported C# null-conditional shift compound assignment indexer targets

## Objective

Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, editor, and CI signals after imported C# null-conditional shift compound assignment indexer targets landed, then select the next bounded TypeSharp implementation or planning slice while preserving generated package-free `net48` artifacts and C# 7.3-compatible generated source.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0435 rollup](tasks-rollup.md#task-0435-imported-csharp-null-conditional-shift-compound-assignment-indexer-targets)

## Scope

In:

- Recheck official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/editor/CI sources for any baseline drift after Task 0435.
- Separate stable language/runtime signals from preview/watch signals.
- Confirm generated artifacts still target package-free `net48` and generated C# still stays C# 7.3-compatible.
- Confirm the `net10.0` `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge plus package shards remain isolated test-host tooling.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Select the next bounded implementation or planning task and update tasks, traceability, docs, and rollup state.

Out:

- Implementing the Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.
- Replacing the generated artifact baseline with .NET 10/11-only runtime features.
- Adding a second test framework unless official-source review shows distinct evidence beyond the existing extracted catalog.

## Acceptance Criteria

- [ ] Official sources are rechecked and summarized with stable vs preview/watch boundaries.
- [ ] Generated `net48`/C# 7.3 and package-free artifact baselines are reaffirmed or any drift is explicitly documented.
- [ ] Test-host package boundaries are reaffirmed or updated with evidence.
- [ ] Task 0401 remains blocked unless explicit approval appears.
- [ ] The next bounded task is selected and reflected in `agent/tasks.md`, `agent/tasks-rollup.md`, `agent/traceability.md`, Feature Status, Project Policy if needed, and Work Ledger.

## Verification

Command: TBD
Expected: docs build and `git diff --check` pass after roadmap updates.
Result: TBD

## Handoff

Done:

- Task 0435 implemented imported C# null-conditional shift compound assignment indexer targets and updated catalog/package-shard counts.

Remaining:

- Recheck official sources and select the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
