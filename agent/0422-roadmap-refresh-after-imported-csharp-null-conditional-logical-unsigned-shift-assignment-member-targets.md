# Task: roadmap-refresh-after-imported-csharp-null-conditional-logical-unsigned-shift-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:10:00 +09:00
End Time: TBD

## Objective

Refresh the roadmap after imported C# `receiver?.Member >>>= count` support and select the next bounded implementation slice.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework/.NET, NuGet, .NET testing, MSTest SDK, xUnit.net/NUnit, VS Code, and GitHub Actions signals after Task 0421.
- Confirm whether generated artifacts still stay package-free `net48` with C# 7.3-compatible source.
- Confirm whether the existing `net10.0` `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge plus package shard projects remain the selected NuGet test-host path.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI implementation fix.
- Update docs, task ledger, and traceability with the roadmap result.
- Select the next bounded implementation slice and write its packet.

Out:
- Implementing Task 0401 without explicit approval.
- Replacing the package-free main/shard runner or the current MSTest bridge unless the refresh finds a concrete reason.
- Changing generated artifact target frameworks or adding generated-project NuGet restore.

## Acceptance Criteria

- [ ] Official source refresh is summarized with concrete source links.
- [ ] Generated `net48`/C# 7.3 and `net10.0` MSTest.Sdk/MTP test-host baselines are reaffirmed or updated.
- [ ] Task 0401 status remains accurate.
- [ ] Next implementation task is selected and packeted.
- [ ] Docs, task ledger, and traceability are updated.
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
