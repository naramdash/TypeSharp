# Task: roadmap-refresh-after-local-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 03:50:00 +09:00
Source: Task 0447 local floating-point and decimal multiplicative compound assignment policy

## Objective

Refresh the roadmap after local floating-point and decimal `*=`, `/=`, and `%=` policy landed. Recheck official language, platform, package, test, editor, and CI signals; compare canonical docs and operational ledgers; preserve the generated package-free `net48`/C# 7.3 baseline plus the current `net10.0` MSTest.Sdk/MTP package bridge; and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0447 rollup](tasks-rollup.md#task-0447-local-floating-point-and-decimal-multiplicative-compound-assignment-policy)

## Scope

In:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals after Task 0447.
- Reconcile Feature Status, Project Policy, Work Ledger, tasks, and traceability against the current implementation and test-host package baseline.
- Keep generated TypeSharp output package-free `net48` with C# 7.3-compatible generated source unless official-source review exposes a blocker.
- Keep the existing `net10.0` MSTest.Sdk/MTP package bridge and package shards unless the source review shows a distinct improvement that benefits the TypeSharp goal.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Select the next bounded implementation slice and create its active task packet.

Out:

- Implementing Task 0401 without explicit user approval.
- Adding a duplicate xUnit.net or NUnit bridge without distinct evidence value.
- Changing generated-artifact target frameworks, generated C# language version, or package-free output policy as part of the refresh itself.
- Implementing the selected next slice in this roadmap-refresh task.

## Acceptance Criteria

- [ ] Official source signals are refreshed and summarized with stable versus preview distinctions.
- [ ] Feature Status, Project Policy, Work Ledger, tasks, and traceability are consistent with Task 0447 completion and the selected next slice.
- [ ] Generated package-free `net48`/C# 7.3 and test-host package bridge baselines are either reaffirmed or explicitly updated with evidence.
- [ ] Task 0401 remains blocked unless explicit user approval is present.
- [ ] A next bounded implementation packet is selected and linked from `agent/tasks.md`.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check` after ledger refresh.
Result: TBD

## Handoff

Done:

- Task 0447 completed local floating-point and decimal multiplicative compound assignment policy for local `let mut` targets while preserving generated package-free `net48`/C# 7.3 output and keeping imported/member/indexer/null-conditional expansion for later slices.

Remaining:

- Recheck official sources and select the next bounded implementation slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
