# Task: roadmap-refresh-after-extension-property-null-conditional-access-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:25:00 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, test-platform, editor, and CI signals after extension-property null-conditional access diagnostics, confirm no TypeSharp baseline drift, and select the next bounded slice.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net, VS Code, and GitHub Actions signals.
- Confirm generated artifacts remain package-free `net48` with C# 7.3-compatible output.
- Confirm the `net10.0` `MSTest.Sdk/4.2.3`/Microsoft Testing Platform bridge and package shards remain the selected NuGet test-host path unless official signals change.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Select one next bounded implementation or planning slice and update canonical docs/ledgers.

Out:
- Implementing Task 0401 without explicit approval.
- Changing generated artifact target frameworks or C# source baseline.
- Replacing the existing MSTest SDK/MTP test-host bridge with another package without distinct evidence.
- Implementing extension property setters, static extension members, operators, nullable receiver lifting, imported C# extension property metadata, or richer extension ranking during this refresh.

## Acceptance Criteria

- [ ] Official source status is rechecked and summarized.
- [ ] Docs and agent ledgers agree on generated artifact, test-host package, and CI state.
- [ ] The next bounded task is selected with a packet when needed.
- [ ] Docs build and diff checks pass.

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
