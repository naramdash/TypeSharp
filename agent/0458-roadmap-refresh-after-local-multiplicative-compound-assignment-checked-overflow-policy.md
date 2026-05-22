# Task: roadmap-refresh-after-local-multiplicative-compound-assignment-checked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 07:05:58 +09:00
End Time: TBD

## Objective

Recheck current roadmap signals after local multiplicative compound assignment checked/unchecked overflow policy landed, preserve generated package-free `net48` and C# 7.3 baselines, keep the current MSTest.Sdk/MTP package-shard boundary, and select the next bounded TypeSharp implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0457 rollup](tasks-rollup.md#task-0457-local-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing/MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals that can affect TypeSharp roadmap order.
- Preserve generated package-free `net48`, C# 7.3-compatible source output, and the current `MSTest.Sdk/4.2.3` MTP package-shard baseline unless official evidence requires a change.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Update Feature Status, Project Policy, Work Ledger, task ledger, and traceability as needed.
- Select the next bounded implementation slice after local checked/unchecked multiplicative assignment.

Out:

- Implementing Task 0401's CI fix.
- Adding NuGet dependencies to generated `net48` artifacts.
- Implementing user-defined operator resolution, imported checked-overflow targets, or other language/runtime slices during the refresh.

## Acceptance Criteria

- [ ] Official signal refresh is documented with source links and concrete version/status notes.
- [ ] Generated artifact and test-host package baselines are explicitly preserved or updated with evidence.
- [ ] Task 0401 remains blocked absent explicit user approval.
- [ ] The next bounded implementation task is selected and recorded.
- [ ] Docs, task ledger, traceability, and Work Ledger are updated.
- [ ] Docs build, stale-reference scan, and `git diff --check` pass.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and whitespace check pass after roadmap updates.
Result: TBD

## Handoff

Done:

- Task 0457 implemented local `checked(...)` and `unchecked(...)` wrappers for bounded mutable-local `*=`, `/=`, and `%=` assignment, lowered statement-form wrappers to C# 7.3 checked/unchecked blocks, preserved deterministic diagnostics, and kept shared catalog/package-shard baselines unchanged.

Remaining:

- Recheck official roadmap signals and select the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
