# Task: roadmap-refresh-after-integral-shift-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 04:33:42 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after integral numeric shift expressions, confirm TypeSharp's baseline, and select the next bounded slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0386-integral-numeric-shift-expressions](tasks-rollup.md#task-0386-integral-numeric-shift-expressions)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)

## Scope

In:
- Recheck current official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals that can affect TypeSharp's roadmap.
- Separate stable, preview, and watch-only signals before changing TypeSharp's baseline.
- Confirm generated artifacts remain `net48` and generated C# remains C# 7.3-compatible unless a canonical policy change is explicitly justified.
- Compare Feature Status, Work Ledger, and recent task rollup evidence after task 0386.
- Select the next bounded TypeSharp slice and update the task queue.

Out:
- Implementing the next language/compiler feature.
- Changing generated artifact target frameworks or runtime dependencies.
- Adding test framework packages, NuGet restore behavior, CI runners, or editor tooling unless the refresh explicitly queues a follow-up.
- Treating preview-only platform or language signals as stable TypeSharp behavior.

## Acceptance Criteria

- [ ] Official source signals are rechecked and summarized with stable/preview/watch boundaries.
- [ ] TypeSharp generated-artifact and test-host baselines are either confirmed unchanged or a follow-up task is queued with explicit evidence.
- [ ] Feature Status and Work Ledger reflect the post-0386 roadmap state.
- [ ] `agent/tasks.md`, this packet, and traceability identify the next bounded slice.
- [ ] Docs build and diff checks pass after any documentation updates.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0386 accepted primitive integral `<<`/`>>` expressions, preserved function composition, and updated the shared catalog to 523 cases.

Remaining:
- Recheck official sources and select the next bounded slice.

Blocked:
- None.
