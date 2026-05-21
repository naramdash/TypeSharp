# Task: roadmap-refresh-after-direct-named-arguments

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 03:58:00 +09:00
End Time: TBD

## Objective

Refresh roadmap signals after direct TypeSharp named argument binding and select the next bounded implementation slice while preserving generated `net48` and C# 7.3-compatible output.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md#task-0378-direct-typesharp-named-argument-binding)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Recheck relevant official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals.
- Confirm whether direct named argument binding changes any generated artifact, runtime, package, or CI baseline.
- Select the next bounded TypeSharp implementation slice.
- Update Feature Status, Work Ledger, task queue, traceability, and rollup.

Out:
- Implementing the next language feature.
- Replacing the existing package-free shard runner or pinned MSTest SDK bridge unless official signals require it.

## Acceptance Criteria

- [ ] Official signal check is summarized with dates and links where needed.
- [ ] Generated `net48`, C# 7.3, runtime/core package-free, and test-host package boundaries are confirmed or updated.
- [ ] Next active task packet is created with clear bounded scope.
- [ ] Feature Status, Work Ledger, traceability, and task rollup are updated.
- [ ] Verification includes docs build and `git diff --check`.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0378 completed direct TypeSharp named argument binding for known TypeSharp-owned direct and pipeline calls.

Remaining:
- Perform roadmap refresh and select the next bounded implementation slice.

Blocked:
- None.
