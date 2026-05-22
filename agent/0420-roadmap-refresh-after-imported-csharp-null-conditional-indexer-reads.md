# Task: roadmap-refresh-after-imported-csharp-null-conditional-indexer-reads

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 14:07:50 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, test-platform, editor, and CI signals after imported C# null-conditional indexer reads, refresh roadmap state, and select the next bounded implementation slice.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- Official C#, F#, TypeScript, .NET Framework/.NET, NuGet, .NET testing, MSTest/xUnit/NUnit, VS Code, and GitHub Actions sources.

## Scope

In:
- Recheck current official sources for language/platform/package/test/editor/CI signals.
- Confirm whether generated `net48`/C# 7.3 and package-free artifact baselines still hold.
- Reassess the existing `net10.0` MSTest.Sdk/MTP package bridge and shards.
- Keep Task 0401 blocked unless the user explicitly approves the CI implementation fix.
- Update canonical roadmap/work-ledger notes and select the next bounded implementation task.

Out:
- Implementing the Task 0401 GitHub Actions `npm` process-launch fix without explicit approval.
- Adopting preview-only .NET/C# features as stable TypeSharp behavior.
- Adding duplicate test frameworks unless they provide distinct value over the existing package bridge.

## Acceptance Criteria

- [ ] Official-source signals are rechecked and summarized.
- [ ] Feature Status, Project Policy if needed, Work Ledger, tasks, rollup, and traceability are updated.
- [ ] Task 0401 status remains accurate.
- [ ] A next bounded implementation slice is selected with an active packet if work should continue.
- [ ] Verification commands pass.

## Verification

Command: TBD
Expected: docs build and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
