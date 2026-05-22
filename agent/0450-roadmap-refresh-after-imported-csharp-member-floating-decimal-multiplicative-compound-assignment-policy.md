# Task: roadmap-refresh-after-imported-csharp-member-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 04:16:37 +09:00
Source: Task 0449 imported C# regular member floating-point and decimal multiplicative compound assignment policy

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular member floating-point and decimal multiplicative compound assignment landed. Preserve or update TypeSharp's generated package-free `net48`/C# 7.3 baseline and the current `net10.0` MSTest.Sdk/MTP package bridge, keep Task 0401 blocked without explicit user approval, and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0449 rollup](tasks-rollup.md#task-0449-imported-csharp-regular-member-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- Official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions sources.

## Scope

In:

- Recheck official current docs/releases for C# stable/preview, F#, TypeScript, .NET Framework, .NET SDK/runtime, NuGet package signals, .NET testing/MTP/MSTest SDK, xUnit.net, NUnit, VS Code extension/LSP guidance, and GitHub Actions runner/tooling signals.
- Confirm whether the generated package-free `net48`/C# 7.3 artifact baseline or current `net10.0` MSTest.Sdk/MTP package bridge should change.
- Keep Task 0401's GitHub Actions `npm` process-launch fix blocked unless the user explicitly approves implementation.
- Update Feature Status, Project Policy or Work Ledger if the roadmap/baseline changes, then select the next bounded implementation task packet.

Out:

- Implementing Task 0401 without explicit user approval.
- Changing generated artifact package/framework baselines unless official-source evidence justifies it.
- Implementing the next language/compiler slice during this roadmap refresh.

## Acceptance Criteria

- [ ] Official current sources are rechecked and summarized with links in the rollup.
- [ ] Generated package-free `net48`/C# 7.3 and test-host package strategy are reaffirmed or updated with evidence.
- [ ] Task 0401 remains blocked absent explicit user approval.
- [ ] The next bounded implementation slice is selected and represented by an active packet, tasks.md state, traceability, and docs ledger updates.
- [ ] Docs build and stale-reference scans pass.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0449 extended imported C# regular field/property member `*=`, `/=`, and `%=` targets to bounded known non-null integral/floating-point/decimal operands while preserving generated package-free `net48`/C# 7.3 output and existing test-host package counts.

Remaining:

- Recheck official signals and choose the next bounded implementation slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
