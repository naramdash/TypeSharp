# Task: roadmap-refresh-after-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0465 imported C# null-conditional indexer multiplicative compound assignment checked/unchecked overflow policy

## Objective

Recheck official language/platform/package/testing/editor/CI signals after imported C# null-conditional indexer checked/unchecked multiplicative assignment, preserve generated package-free `net48`, C# 7.3-compatible lowering, deterministic diagnostics, and the current 572-test MSTest.Sdk/MTP package-shard baseline, keep Task 0401 blocked absent explicit approval, and select the next bounded implementation slice.

## Context

- Task 0465 completed the paired null-conditional indexer checked/unchecked multiplicative assignment slice.
- Generated artifacts remain package-free `net48` with C# 7.3-compatible outer receiver and inner index guard lowering.
- The shared package-free catalog remains 568 cases with four 142-case shards, and the package-shard MTP minimum remains 572 tests.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions `npm` process-launch implementation fix.

## Scope

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing/MSTest SDK/xUnit.net/NUnit, VS Code, and GitHub Actions signals using official sources.
- Compare current signals against Feature Status, Project Policy, Work Ledger, tasks, and traceability.
- Preserve the generated artifact baseline: package-free `net48`, C# 7.3-compatible source, and deterministic diagnostics.
- Preserve the current MSTest.Sdk/MTP package bridge and 572-test package-shard minimum unless official/package evidence clearly justifies a change.
- Keep Task 0401 blocked without explicit user approval.
- Select the next bounded implementation slice and create/update the active task packet.

## Out Of Scope

- Implementing Task 0401 without explicit user approval.
- Adding xUnit.net or NUnit unless the refresh finds distinct value beyond the existing MSTest.Sdk/MTP bridge.
- Changing generated target frameworks or adopting .NET 10/11-only generated-artifact APIs.
- Treating preview C#/F#/TypeScript/.NET signals as stable TypeSharp behavior.

## Acceptance

- [ ] Official source checks are recorded with current concrete version/status facts.
- [ ] Feature Status, Project Policy, Work Ledger, tasks, traceability, and rollup state reflect the refresh.
- [ ] The next bounded slice is selected with a concrete active packet.
- [ ] Task 0401 remains blocked unless the user explicitly approves implementation.
- [ ] Verification covers compiler build/catalog, MSTest package shards, docs build, stale-reference scan, and whitespace check.

## References

- [Task 0465 rollup](tasks-rollup.md#task-0465-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)

## Handoff Notes

- Start by re-reading `agent.md`, `agent/tasks.md`, `docs/src/content/docs/goal.md`, and `agent/agentic-execution.md`.
- Use official sources for version/package/editor/CI refresh facts.
- Do not use Python.
