# Task: roadmap-refresh-after-logical-unsigned-shift-assignment-imported-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 08:35:00 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# member `>>>=` support, confirm the generated-artifact baseline, and select the next bounded TypeSharp slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0394-logical-unsigned-shift-assignment-imported-member-targets](tasks-rollup.md#task-0394-logical-unsigned-shift-assignment-imported-member-targets)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Recheck current official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing/MSTest, xUnit.net, VS Code, and GitHub Actions runner/setup-action signals.
- Compare those signals against the current Feature Status, Project Policy, Work Ledger, and package-based test-shard state.
- Confirm whether generated TypeSharp artifacts should still target `net48` and C# 7.3-compatible source, and whether test-host `net10.0` NuGet packages remain isolated to tests.
- Select one next bounded implementation or documentation slice and update the latest-five queue accordingly.

Out:
- Implementing the selected next slice.
- Switching generated artifacts to .NET 10/11.
- Adding a new test framework package unless the official-source review shows distinct value beyond the existing MSTest SDK/MTP bridge.
- Broad roadmap rewrites unrelated to changed official signals.

## Acceptance Criteria

- [ ] Official-source recheck is summarized with stable vs preview/tooling signals separated.
- [ ] Generated-artifact baseline and package-test-host boundary are explicitly confirmed or updated.
- [ ] Feature Status, Work Ledger, tasks queue, and traceability reflect the selected next bounded slice.
- [ ] Any NuGet/test-package recommendation explains why the existing `net10.0` MSTest SDK/MTP shard bridge is sufficient or why a change is now warranted.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap/ledger updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0394 added imported C# field/property `>>>=` checker/backend lowering, single-evaluation receiver evidence, generated `net48` C# consumer coverage, and shared catalog count 528.

Remaining:
- Run the official-source refresh, update roadmap/ledger state, select the next bounded slice, verify docs, commit, and push.

Blocked:
- None.
