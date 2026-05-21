# Task: roadmap-refresh-after-shift-assignment-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 05:00:07 +09:00
End Time: TBD

## Objective

Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals after shift assignment expressions landed, confirm whether the generated `net48`/C# 7.3 baseline changes, and select the next bounded TypeSharp implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0388-shift-assignment-expressions](tasks-rollup.md#task-0388-shift-assignment-expressions)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Recheck current official language, platform, package, testing, editor, and CI references relevant to TypeSharp's roadmap.
- Preserve generated `net48` and C# 7.3 compatibility unless official evidence changes the baseline.
- Update canonical docs, work ledger, tasks, and traceability with the next bounded task.

Out:
- Implementing the next language feature.
- Adopting preview-only runtime or generated-artifact dependencies.
- Adding a new test runner package unless it provides distinct evidence beyond the current MSTest/MTP bridge.

## Acceptance Criteria

- [ ] Official-source signals are rechecked and summarized with stable/preview distinction.
- [ ] Generated artifact baseline is confirmed or an explicit follow-up is created.
- [ ] Next bounded implementation slice is selected and packeted.
- [ ] Docs Work Ledger, Feature Status, task ledger, and traceability are updated.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0388 implemented bounded `<<=`/`>>=` shift assignment expressions with parser, checker, backend, generated `net48`, docs, and catalog evidence.

Remaining:
- Recheck official roadmap signals and choose the next bounded implementation task.

Blocked:
- None.
