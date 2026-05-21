# Task: roadmap-refresh-after-logical-unsigned-shift-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 05:32:12 +09:00
End Time: TBD

## Objective

Recheck official language, platform, tooling, and package signals after logical unsigned shift expressions landed; confirm the generated `net48`/C# 7.3 baseline and select the next bounded TypeSharp implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0390-logical-unsigned-shift-expressions](tasks-rollup.md#task-0390-logical-unsigned-shift-expressions)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions signals relevant to TypeSharp's roadmap.
- Confirm whether any new official signal changes the generated-artifact baseline, dependency policy, test-host policy, or next feature priority.
- Update Feature Status, Work Ledger, task queue, and traceability with the decision.
- Create the next active task packet for the selected bounded implementation slice.

Out:
- Implementing the next language feature in this refresh slice.
- Changing generated `net48` or C# 7.3 baselines without direct evidence.
- Adding new packages or runtime dependencies.

## Acceptance Criteria

- [ ] Official source signals are checked and summarized with dates.
- [ ] Generated `net48`/C# 7.3 baseline is explicitly preserved or changed with evidence.
- [ ] Next bounded implementation task is selected and packeted.
- [ ] Feature Status, Work Ledger, task ledger, and traceability are updated.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0390 added expression-only `>>>` logical unsigned shifts over known non-null primitive integral operands with C# 7.3-compatible cast lowering and generated `net48` consumer evidence.

Remaining:
- Recheck official signals and select the next implementation slice.

Blocked:
- None.
