# Task: roadmap-refresh-after-imported-csharp-regular-member-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0469 imported C# regular member user-defined multiplicative operator policy

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular field/property member user-defined multiplicative operator assignment landed, preserve TypeSharp's generated package-free `net48` and C# 7.3 baseline, and select the next bounded implementation slice.

## Context

- Task 0469 extended the Task 0467 imported C# static binary `operator *`, `operator /`, and `operator %` precursor from mutable local `*=`, `/=`, and `%=` targets to regular readable/writable imported C# instance/static field/property member targets.
- Accepted Task 0469 cases lower to ordinary C# 7.3-compatible compound assignment syntax that the C# compiler binds to the imported static binary operator.
- Primitive regular member multiplicative assignment behavior remains unchanged.
- The shared catalog is now 572 cases with package-free shard expectations `143`, `143`, `143`, and `143`; the MTP package-shard minimum is now 576 tests.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Scope

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet/package, .NET testing/MTP/MSTest/xUnit.net/NUnit, VS Code, and GitHub Actions signals.
- Confirm whether any official signal changes TypeSharp's generated package-free `net48`, C# 7.3, deterministic diagnostics, or package policy baseline.
- Update Feature Status, Project Policy, Work Ledger, task queue, and traceability if the official-signal refresh changes current status or next-slice selection.
- Select one bounded next implementation slice that fits the existing roadmap and avoids broad refactors.
- Keep Task 0401 blocked unless the user explicitly approves the CI fix.

## Out Of Scope

- Implementing new compiler behavior during this roadmap refresh.
- Implementing Task 0401 without explicit user approval.
- Adopting package dependencies or changing generated artifact targets unless the refreshed official evidence requires a deliberate policy update.

## Acceptance

- [ ] Official language/platform/package/testing/editor/CI signals are rechecked and summarized.
- [ ] Generated package-free `net48`, C# 7.3, deterministic diagnostics, and test-host package baselines are either preserved or explicitly updated with evidence.
- [ ] The next bounded implementation slice is selected and recorded in `agent/tasks.md`, `agent/traceability.md`, `docs/src/content/docs/feature-status.md`, and `docs/src/content/docs/work-ledger.md`.
- [ ] Docs build, stale scan, and whitespace checks pass.

## References

- [Task 0469 rollup](tasks-rollup.md#task-0469-imported-csharp-regular-member-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)
