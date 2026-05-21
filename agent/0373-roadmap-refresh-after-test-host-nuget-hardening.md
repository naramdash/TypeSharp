# Task: roadmap-refresh-after-test-host-nuget-hardening

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 01:17:22 +09:00
End Time: TBD

## Objective

Recheck the TypeSharp roadmap after test-host NuGet package selection and restore hardening, confirm whether the hardened MSTest bridge changes any generated-artifact, CI, or package-adoption baseline, and select the next bounded task that best advances the long-running TypeSharp goal.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0372-test-host-nuget-package-selection-and-restore-hardening)
- Official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, and xUnit.net documentation where current signals matter.

## Scope

In:
- Recheck active docs and ledgers after the root `NuGet.config`, MSTest lock file, source mapping, and audit controls landed.
- Confirm whether the next best slice should be CI adoption over the extracted catalog, xUnit.net v3 bridge work, another .NET ecosystem control, or a return to language/compiler backlog.
- Update `Feature Status`, `Project Policy`, `Work Ledger`, `tasks.md`, `traceability.md`, and `tasks-rollup.md` if the selected next task or baseline changes.
- Keep generated `net48` artifacts, generated C# 7.3 compatibility, runtime/core package-free status, and custom shard runner policy intact unless evidence requires a documented policy change.

Out:
- Implementing the next selected feature slice.
- Changing test framework packages without a new task.
- Migrating CI.
- Implementing compiler NuGet restore.
- Changing generated artifact target frameworks or runtime dependencies.

## Acceptance Criteria

- [ ] Current official source signals are rechecked or explicitly judged still covered by the 2026-05-22 refresh.
- [ ] No generated-artifact baseline change is either confirmed or documented with required follow-up.
- [ ] The next bounded task is selected and recorded in `agent/tasks.md`.
- [ ] Docs and agent ledgers are consistent with the selected next task.
- [ ] Verification includes docs build and diff checks.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0372 selected the existing MSTest SDK/MTP bridge over adding xUnit.net v3 now, added test-host-only NuGet lock/source/audit controls, and kept generated `net48` artifacts package-free.

Remaining:
- Recheck roadmap and select the next bounded task.

Blocked:
- None.
