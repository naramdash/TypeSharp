# Task: roadmap-refresh-after-mstest-package-shards

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:25:00 +09:00
End Time: TBD

## Objective

Recheck official language, runtime, package, test, editor, and CI signals after package-based MSTest shard execution, confirm whether TypeSharp's generated-artifact baseline changes, and select the next bounded implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0382-mstest-package-shard-bridge](tasks-rollup.md#task-0382-mstest-package-shard-bridge)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals relevant to TypeSharp's current baseline.
- Confirm whether generated TypeSharp artifacts should remain `net48` with C# 7.3-compatible generated source.
- Review the now-package-sharded MSTest bridge, package-free shard runner, and remaining language/tooling backlog.
- Select one bounded next task and create/update the corresponding active packet, queue row, docs ledger, and traceability entry.

Out:
- Implementing a new compiler feature during the refresh.
- Replacing the package-free shard runner or changing the pinned MSTest SDK package without a separate task.
- Changing generated target framework or generated C# language version without explicit evidence and follow-up tasking.

## Acceptance Criteria

- [ ] Official signals are rechecked with dates and links in the rollup or docs.
- [ ] Feature Status and Work Ledger reflect the post-0382 baseline.
- [ ] `agent/tasks.md`, `agent/tasks-rollup.md`, and `agent/traceability.md` point to the selected next bounded task.
- [ ] Any new task packet has scope, acceptance criteria, verification plan, and handoff notes.
- [ ] Docs build and `git diff --check` are run.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap refresh docs updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0382 completed package-based MSTest shard execution over the shared catalog.

Remaining:
- Recheck official signals and select the next bounded implementation slice.

Blocked:
- None.
