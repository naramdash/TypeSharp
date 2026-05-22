# Task: roadmap-refresh-after-imported-csharp-null-conditional-indexer-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 06:20:26 +09:00
Source: Task 0455 imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment policy

## Objective

Recheck current official language, platform, package, testing, editor, and CI signals after imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment landed, preserve TypeSharp's generated package-free `net48`/C# 7.3 and package-shard baselines, and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0455 rollup](tasks-rollup.md#task-0455-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK/MTP, xUnit.net, NUnit, VS Code, and GitHub Actions signals that can affect TypeSharp's current boundaries.
- Preserve generated package-free `net48` output, generated C# 7.3 compatibility, and the current `net10.0` MSTest.Sdk/MTP package-shard bridge unless official evidence requires a documented change.
- Confirm Task 0455's completion state and keep checked-overflow policy plus user-defined multiplicative operators as separate candidates unless the refresh identifies a higher-priority bounded slice.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Update docs, ledgers, task queue, traceability, and the next active packet.

Out:

- Implementing checked-overflow policy changes.
- Implementing user-defined multiplicative operator resolution.
- Changing generated artifact baselines, framework targets, NuGet package strategy, or CI behavior without evidence and task scope.
- Fixing Task 0401 without explicit user approval.

## Acceptance Criteria

- [ ] Official source refresh is recorded with concrete current versions/signals and no stale Task0455 active wording.
- [ ] Generated package-free `net48`/C# 7.3 and MSTest.Sdk/MTP package-shard baselines are preserved or any change is explicitly justified.
- [ ] Task 0401 remains blocked without explicit approval.
- [ ] The next active task packet is selected with bounded scope, source of truth, acceptance criteria, and verification expectations.
- [ ] Docs build, stale-reference scan, and `git diff --check` pass.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0455 completed imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment over the bounded known non-null integral/floating-point/decimal assign-back policy while preserving generated package-free `net48` C# 7.3 output and the current package-shard baseline.

Remaining:

- Recheck official signals and select the next bounded implementation slice, likely checked-overflow policy or user-defined multiplicative operators unless the refresh identifies a clearer priority.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
