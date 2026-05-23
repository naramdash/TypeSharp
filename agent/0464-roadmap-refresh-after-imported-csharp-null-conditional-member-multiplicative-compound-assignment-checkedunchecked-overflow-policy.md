# Task: roadmap-refresh-after-imported-csharp-null-conditional-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0463 imported C# null-conditional member multiplicative compound assignment checked/unchecked overflow policy

## Objective

Recheck the official language, platform, package, testing, editor, and CI signals after imported C# null-conditional member checked/unchecked multiplicative assignment landed, preserve the generated package-free `net48`/C# 7.3 baseline and current MSTest.Sdk/MTP package-shard baseline, keep Task 0401 blocked absent explicit approval, and select the next bounded implementation slice.

## Context

- Task 0463 implemented `checked(receiver?.Member *= value)`, `checked(receiver?.Member /= value)`, `checked(receiver?.Member %= value)`, and unchecked counterparts for readable/writable metadata-backed imported C# instance field/property targets.
- Accepted null-conditional member wrappers lower to C# 7.3-compatible block lambdas whose null branch returns before right-side evaluation and whose non-null branch contains the checked/unchecked assignment body.
- The shared catalog remains 568 cases with four 142-case package-free shards and the package-shard MTP minimum remains 572 tests.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix for the Windows `npm` process-launch failure.

## Scope

- Recheck official C#, F#, TypeScript, .NET Framework/.NET, NuGet/package, .NET testing/MSTest/xUnit.net/NUnit, VS Code, and GitHub Actions signals.
- Update Feature Status, Project Policy if needed, Work Ledger, tasks, and traceability with any current signal changes.
- Preserve generated `net48`, package-free generated artifacts, C# 7.3-compatible lowering, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard path unless official signals force a documented change.
- Select the next bounded implementation slice after this refresh.

## Out Of Scope

- Implementing the Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.
- Implementing null-conditional indexer checked/unchecked multiplicative assignment inside this roadmap refresh unless the refresh explicitly selects it as the next task.
- Implementing user-defined multiplicative compound assignment operators.

## Acceptance

- [ ] Official signals are checked and summarized with concrete dates/version facts.
- [ ] Current generated artifact and package/test-host baselines are either reaffirmed or updated with evidence.
- [ ] Task 0401 remains blocked unless the user explicitly approves implementation.
- [ ] `agent/tasks.md`, `agent/traceability.md`, docs, and rollup state are updated.
- [ ] Verification covers docs build, compiler/test baselines relevant to a roadmap refresh, stale-reference scan, and whitespace check.

## References

- [Task 0463 rollup](tasks-rollup.md#task-0463-imported-csharp-null-conditional-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)

## Handoff Notes

- Start by re-reading `agent.md`, `agent/tasks.md`, `docs/src/content/docs/goal.md`, and `agent/agentic-execution.md`.
- Use only official primary sources for the refresh.
- Do not use Python.
