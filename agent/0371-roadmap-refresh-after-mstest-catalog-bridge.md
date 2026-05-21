# Task: roadmap-refresh-after-mstest-catalog-bridge

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 00:57:00 +09:00
End Time: TBD

## Objective

Recheck the durable roadmap after extracting the custom test catalog and adding the pinned MSTest SDK/Microsoft Testing Platform bridge. Confirm that generated artifact baselines remain unchanged, record any updated external platform signals, and select the next bounded task.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0370-test-catalog-extraction-and-mstest-bridge)
- [test README](../test/README.md)

## Scope

In:
- Recheck official source signals relevant to the TypeSharp roadmap after the MSTest bridge.
- Confirm generated `net48` artifacts and C# 7.3 backend policy are unchanged.
- Decide whether the next bounded slice should be CI adoption, xUnit.net v3 comparison, NuGet/package policy work, or a language/compiler backlog item.
- Update the docs/agent ledgers with the selected next task.

Out:
- Changing test framework packages.
- Removing the custom runner or shard path.
- Changing generated artifact targets.
- Implementing a language feature during the refresh.

## Acceptance Criteria

- [ ] Roadmap sources are refreshed where needed.
- [ ] Feature Status, Work Ledger, Project Policy, and task queue remain consistent.
- [ ] Next active bounded task is recorded with a packet if it spans sessions.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0370 extracted the reusable test catalog and added the MSTest SDK/MTP bridge while preserving the package-free main/shard runners.

Remaining:
- Refresh the roadmap and select the next task.

Blocked:
- None.
