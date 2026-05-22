# Task: roadmap-refresh-after-imported-csharp-null-conditional-additive-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 22:14:41 +09:00
Source: Task 0431 imported C# null-conditional additive compound assignment indexer targets

## Objective

Recheck current official language/platform/package/test/editor/CI signals after imported C# null-conditional additive compound assignment indexer targets, confirm TypeSharp's baseline, and select the next bounded implementation slice while preserving generated package-free `net48` artifacts.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0431 rollup](tasks-rollup.md#task-0431-imported-csharp-null-conditional-additive-compound-assignment-indexer-targets)

## Scope

In:

- Review current official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals that could affect TypeSharp's language, generated artifact, package, editor, or CI baseline.
- Reconfirm generated user assemblies remain package-free `net48` with C# 7.3-compatible source output.
- Reconfirm the isolated `net10.0` `MSTest.Sdk/4.2.3` Microsoft Testing Platform package bridge, lock/source-mapping/audit posture, package-shard expectations, and package-free shard runner policy remain appropriate.
- Keep Task 0401 blocked unless the user explicitly approves implementing the GitHub Actions `npm` process-launch fix.
- Select the next bounded TypeSharp implementation slice, create its active packet, and update tasks, traceability, Feature Status, Work Ledger, and rollup records.

Out:

- Implementing the selected follow-up slice during this refresh task.
- Implementing Task 0401 without explicit user approval.
- Adding new generated-artifact package dependencies, changing generated target frameworks, adopting preview APIs as stable behavior, or replacing the existing MSTest SDK/MTP bridge without a concrete baseline reason.

## Acceptance Criteria

- [ ] Current official sources are reviewed and cited in the rollup.
- [ ] Generated `net48`/C# 7.3 and test-host package boundaries are reaffirmed or explicitly updated with rationale.
- [ ] Task 0401 remains blocked unless explicit approval is given.
- [ ] A next bounded implementation packet is selected and linked from `agent/tasks.md`.
- [ ] Feature Status, Work Ledger, tasks, traceability, and rollup are updated.
- [ ] Docs build and diff checks pass.

## Verification

Command: TBD
Expected: official-source review, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0431 implemented imported C# null-conditional additive compound assignment indexer targets and updated the shared catalog to 552 cases with package-shard MTP minimum 556.

Remaining:

- Recheck official signals, select the next bounded slice, update docs/ledgers, verify, then roll this packet into [tasks-rollup.md](tasks-rollup.md).

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
