# Task: roadmap-refresh-after-imported-csharp-null-conditional-member-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 05:43:00 +09:00
Source: Task 0453 imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy

## Objective

Recheck official language/platform/package/testing/editor/CI signals after imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy landed, then choose the next bounded implementation slice without changing generated artifact or package baselines unless the evidence requires it.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0453 rollup](tasks-rollup.md#task-0453-imported-csharp-null-conditional-member-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals relevant to current TypeSharp language, generated `net48`, package-free generated artifacts, and test-host decisions.
- Preserve the C# 7.3 generated-source baseline unless official platform evidence requires a documented change.
- Preserve the pinned `net10.0` MSTest.Sdk/MTP package-shard bridge unless official package/testing evidence shows a concrete improvement that fits the project goal.
- Keep Task 0401 blocked unless the user explicitly approves the CI implementation fix.
- Select the next bounded roadmap slice, with paired null-conditional indexer floating-point/decimal multiplicative expansion as the expected leading candidate unless fresh evidence points elsewhere.

Out:

- Implementing paired null-conditional indexer floating-point/decimal multiplicative assignment in this refresh task.
- Checked-overflow policy changes.
- User-defined multiplicative operator resolution.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.
- Package/framework/generated artifact baseline changes without evidence and docs updates.

## Acceptance Criteria

- [ ] Official sources are rechecked and summarized with concrete version/status signals.
- [ ] Docs and ledgers preserve or update generated artifact, package, test-host, editor, and CI policy accurately.
- [ ] Task 0401 remains blocked unless the user explicitly approves implementation.
- [ ] The next active packet is selected with a narrow objective and acceptance criteria.
- [ ] Stale Task 0454 active references are absent after the refresh completes.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0453 extended imported C# null-conditional instance field/property member `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` targets to bounded known non-null integral/floating-point/decimal operands while preserving skipped right-side evaluation, explicit C# 7.3 null guards, generated package-free `net48`, and existing catalog/package-shard baselines.

Remaining:

- Recheck official sources and select the next bounded implementation slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
