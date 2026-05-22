# Task: roadmap-refresh-after-imported-csharp-null-conditional-member-reads

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 13:23:19 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, test, editor, and CI signals after imported C# null-conditional member read expressions, confirm no baseline drift, and select the next bounded TypeSharp slice.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET, NuGet, .NET testing, MSTest, xUnit.net, VS Code, and GitHub Actions signals that could affect the current baseline.
- Confirm generated artifacts remain package-free `net48` C# 7.3-compatible outputs.
- Confirm the existing `net10.0` `MSTest.Sdk/4.2.3`/MTP test-host path and package shards remain the selected NuGet test bridge unless official evidence changes.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions `npm` process-launch implementation fix.
- Update docs/ledgers and select the next bounded implementation slice.

Out:
- Task 0401 CI implementation.
- Generated target changes, `net481`, or NuGet package restore inside generated artifacts.
- New package bridge work without distinct evidence beyond the existing MSTest SDK/MTP bridge.
- Implementing null-conditional indexer reads, invocation, chains, compound assignment, events, static targets, local binding targets, TypeSharp-owned targets, nullable receiver lifting, or broad assignment/member target analysis.

## Acceptance Criteria

- [ ] Official ecosystem signals are rechecked and recorded.
- [ ] Generated artifact and test-host package baselines are confirmed or deliberately updated with evidence.
- [ ] Task 0401 status remains accurate under the `gh-fix-ci` approval boundary.
- [ ] Docs, task queue, traceability, and work ledger identify the selected next slice.
- [ ] Verification commands and diff checks pass.

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
