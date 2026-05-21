# Task: roadmap-refresh-after-params-parameter-declaration

Status: In Progress
Queue: Q1
Start Time: 2026-05-21 23:19:58 +09:00
End Time: TBD

## Objective

Recheck official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals after TypeSharp-owned final-array `params` parameter declarations, confirm the TypeSharp baseline, and select the next bounded implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0366-params-parameter-declaration-slice)
- Official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code references per Project Policy

## Scope

In:
- Recheck current official source signals relevant to TypeSharp's .NET Framework 4.8, C# 7.3 generated-source, TypeScript/F#/C# feature-mapping, NuGet/package, and VS Code tooling baseline.
- Compare completed final-array `params` parameter behavior against remaining Feature Status and Work Ledger backlog.
- Create the next bounded implementation task packet and queue entry.
- Update docs and agent ledgers with the selected slice.

Out:
- Implementing the selected follow-up slice.
- Broad roadmap rewrites unrelated to current official signal changes.
- Preview feature adoption without a stable baseline and explicit policy.

## Acceptance Criteria

- [ ] Official source signal check is recorded with concrete dates and source links.
- [ ] Feature Status and Work Ledger remain consistent with completed task 0366.
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
- Task 0366 completed TypeSharp-owned final-array `params` parameter declarations for direct calls, pipeline validation, generic tail inference, and C# `params T[]` lowering.

Remaining:
- Recheck official references and select the next bounded implementation slice.

Blocked:
- None.
