# Task: roadmap-refresh-after-extension-property-assignment-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 13:08:00 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, test-platform, editor, and CI signals after extension property assignment diagnostics, confirm no TypeSharp baseline drift, and select the next bounded implementation slice.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals relevant to TypeSharp's baseline.
- Confirm generated artifacts remain package-free `net48` and generated source remains C# 7.3-compatible.
- Confirm the existing `net10.0` `MSTest.Sdk/4.2.3`/MTP package bridge and shards still answer the NuGet test-host path.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Select the next bounded implementation slice after extension property assignment diagnostics.

Out:
- Implementing Task 0401's GitHub Actions `npm` process-launch fix without explicit approval.
- Changing generated artifact targets, test-host package selection, or package restore policy unless official source drift requires a documented decision.
- Implementing extension setters, static extension members, operators, nullable receiver lifting, imported C# extension property metadata, or richer extension ranking during the refresh itself.

## Acceptance Criteria

- [ ] Official source recheck is summarized in the task rollup and canonical docs.
- [ ] Generated `net48`/C# 7.3 and test-host package baselines are either reaffirmed or explicitly updated with rationale.
- [ ] The next bounded implementation slice is selected and recorded in `tasks.md`.
- [ ] Docs verification and diff checks pass.

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
