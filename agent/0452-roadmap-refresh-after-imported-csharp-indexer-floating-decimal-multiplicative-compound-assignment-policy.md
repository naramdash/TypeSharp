# Task: roadmap-refresh-after-imported-csharp-indexer-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 05:20:00 +09:00
Source: Task 0451 imported C# regular indexer floating-point and decimal multiplicative compound assignment policy

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular indexer floating-point and decimal multiplicative compound assignment policy landed. Preserve or update the generated package-free `net48`/C# 7.3 artifact baseline, the current `MSTest.Sdk/4.2.3` Microsoft Testing Platform package-shard strategy, and the next bounded implementation direction.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0451 rollup](tasks-rollup.md#task-0451-imported-csharp-regular-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck official C# stable/preview language signals relevant to compound assignment, checked overflow, operator policy, and .NET Framework generated source compatibility.
- Recheck official F#, TypeScript, .NET Framework/.NET, NuGet/package, .NET testing/MSTest/xUnit/NUnit, VS Code, and GitHub Actions signals that could affect TypeSharp's generated artifact or test-host baseline.
- Reaffirm or update the package-free generated `net48`/C# 7.3 baseline and current `net10.0` test-host package-shard baseline.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions `npm` process-launch implementation fix.
- Select the next bounded slice after regular imported C# indexer floating/decimal multiplicative policy.

Out:

- Implementing Task 0401 without explicit approval.
- Implementing null-conditional floating-point/decimal multiplicative expansion during this refresh packet.
- Package/framework/generated artifact baseline changes without explicit documented evidence.
- User-defined operator or checked-overflow implementation.

## Acceptance Criteria

- [ ] Official source checks are summarized with links and concrete versions/dates where relevant.
- [ ] Generated package-free `net48`/C# 7.3 and current `MSTest.Sdk/4.2.3`/MTP package-shard baselines are preserved or explicitly updated with evidence.
- [ ] Task 0401 remains blocked absent explicit approval.
- [ ] The next bounded slice is selected and reflected in `agent/tasks.md`, `agent/traceability.md`, `docs/src/content/docs/feature-status.md`, and `docs/src/content/docs/work-ledger.md`.
- [ ] Docs build, stale-reference scan, and `git diff --check` pass.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0451 completed regular imported C# indexer `*=`, `/=`, and `%=` floating-point/decimal policy while preserving generated package-free `net48`/C# 7.3 output and current test-host package strategy.

Remaining:

- Recheck official signals and select the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
