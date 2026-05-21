# Task: roadmap-refresh-after-test-suite-runtime-reduction

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 00:21:49 +09:00
End Time: TBD

## Objective

Recheck official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and .NET test-platform source signals after the test-suite runtime reduction, confirm the TypeSharp baseline, and select the next bounded implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0368-test-suite-runtime-reduction-plan-and-refactor)
- Official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and .NET testing references per Project Policy

## Scope

In:
- Recheck current official source signals relevant to TypeSharp's .NET Framework 4.8, C# 7.3 generated-source, TypeScript/F#/C# feature-mapping, NuGet/package, VS Code tooling, and .NET test-platform baseline.
- Compare completed test-suite sharding/cache behavior against Project Policy and Work Ledger.
- Select the next bounded implementation task packet and queue entry.
- Update docs and agent ledgers with the selected slice.

Out:
- Implementing the selected follow-up slice.
- Migrating the custom test catalog to MSTest/xUnit discovery.
- Broad roadmap rewrites unrelated to current official signal changes.
- Preview feature adoption without a stable baseline and explicit policy.

## Acceptance Criteria

- [ ] Official source signal check is recorded with concrete dates and source links.
- [ ] Feature Status, Project Policy, and Work Ledger remain consistent with completed task 0368.
- [ ] `agent/tasks.md` points to the next selected active implementation task.
- [ ] `agent/tasks-rollup.md` records this roadmap refresh.
- [ ] Traceability points at the new active task and evidence.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0368 completed process-safe legacy reference assembly caching plus four parallel shard projects for the custom test runner.

Remaining:
- Recheck official references and select the next bounded implementation slice.

Blocked:
- None.
