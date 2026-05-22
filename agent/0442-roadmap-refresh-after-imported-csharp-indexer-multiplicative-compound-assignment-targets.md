# Task: roadmap-refresh-after-imported-csharp-indexer-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 01:31:16 +09:00
Source: Task 0441 imported C# indexer multiplicative compound assignment targets

## Objective

Recheck official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals after imported C# indexer multiplicative compound assignment targets landed, preserve generated package-free `net48`/C# 7.3 artifacts and the existing `net10.0` MSTest.Sdk/MTP package bridge, keep Task 0401 blocked without explicit approval, and select the next bounded TypeSharp slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0441 rollup](tasks-rollup.md#task-0441-imported-csharp-indexer-multiplicative-compound-assignment-targets)

## Scope

In:

- Recheck current official language/platform/tooling signals for C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing/MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions.
- Compare those signals against TypeSharp's generated package-free `net48`/C# 7.3 baseline, test-host NuGet package bridge, Feature Status, Project Policy, and Work Ledger.
- Keep Task 0401 blocked unless the user explicitly approves its GitHub Actions `npm` process-launch implementation fix.
- Select the next bounded implementation or planning slice after regular imported C# indexer `*=`, `/=`, and `%=` targets.

Out:

- Implementing Task 0401's CI fix without explicit user approval.
- Adding a duplicate xUnit.net or NUnit test bridge unless the refresh finds distinct value beyond the existing MSTest.Sdk/MTP extracted-catalog evidence.
- Changing generated artifact target framework, generated C# language version, or package-free runtime policy without a documented baseline decision.

## Acceptance Criteria

- [ ] Official source signals are rechecked and summarized with stable-vs-preview/tooling boundaries.
- [ ] Generated `net48`/C# 7.3/package-free baseline and `net10.0` MSTest.Sdk/MTP bridge policy are either reaffirmed or explicitly updated with evidence.
- [ ] Task 0401 remains blocked unless explicit implementation approval is present.
- [ ] The next bounded TypeSharp slice is selected and recorded in `agent/tasks.md`, `agent/traceability.md`, docs Work Ledger, and this packet's handoff.
- [ ] Docs and agent ledgers have no stale Task 0441 active references.

## Verification

Command: TBD
Expected: official-source link check, docs build if docs change, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0441 implemented imported C# indexer `*=`, `/=`, and `%=` targets with generated `net48` consumer and deterministic negative checker coverage.

Remaining:

- Recheck official signals and select the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
