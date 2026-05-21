# Task: roadmap-refresh-after-direct-composition-value-inference

Status: In Progress
Queue: Q1
Start Time: 2026-05-21 22:43:26 +09:00
End Time: TBD

## Objective

Recheck official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals after private direct composition value inference, confirm the TypeSharp baseline, and select the next bounded implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0364-direct-composition-value-inference-slice)
- Official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code references per Project Policy

## Scope

In:
- Recheck current official source signals relevant to TypeSharp's .NET Framework 4.8, C# 7.3 generated-source, TypeScript/F#/C# feature-mapping, NuGet/package, and VS Code tooling baseline.
- Compare the latest implemented composition value inference behavior against remaining Feature Status and Work Ledger backlog.
- Create the next bounded implementation task packet and queue entry.
- Update docs and agent ledgers with the selected slice.

Out:
- Implementing the selected follow-up slice.
- Broad roadmap rewrites unrelated to current official signal changes.
- Preview feature adoption without a stable baseline and explicit policy.

## Acceptance Criteria

- [ ] Official source signal check is recorded with concrete dates and source links.
- [ ] Feature Status and Work Ledger remain consistent with completed task 0364.
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
- Task 0364 completed private direct composition value inference and closed public unannotated direct composition with explicit annotation diagnostics.

Remaining:
- Recheck official references and select the next bounded implementation slice.

Blocked:
- None.
