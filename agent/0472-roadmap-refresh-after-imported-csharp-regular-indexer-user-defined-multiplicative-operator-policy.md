# Task: roadmap-refresh-after-imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0471 imported C# regular indexer user-defined multiplicative operator policy

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular indexer user-defined multiplicative operator assignment landed, preserve TypeSharp's generated package-free `net48` and C# 7.3 baseline, keep the shared catalog/package-shard baselines at 574/578 unless evidence requires otherwise, keep Task 0401 blocked absent explicit user approval, and select the next bounded implementation slice.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata and enabled mutable local `*=`, `/=`, and `%=` assignment through one selected imported static binary operator.
- Task 0469 extended that precursor to regular readable/writable imported C# field/property member targets.
- Task 0471 extended the precursor to regular readable/writable imported C# indexer targets with supported arguments while preserving primitive indexer behavior and ordinary C# 7.3 compound-assignment lowering.
- The shared catalog is now 574 cases, package-free shard expectations are `144`, `144`, `143`, and `143`, and the MTP package-shard minimum is 578 tests.
- Task 0401 remains blocked because the CI implementation fix still requires explicit approval.

## Scope

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals.
- Record any relevant changes in Feature Status, Project Policy, Work Ledger, tasks, traceability, and rollup docs.
- Preserve generated package-free `net48`, C# 7.3-compatible generated source, deterministic diagnostics, and the current test-host package policy unless official evidence changes the decision.
- Select one concrete next implementation slice with a bounded packet.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions CI fix.

## Out Of Scope

- Implementing Task 0401 without explicit user approval.
- Implementing the next language/compiler slice during this roadmap refresh.
- Adding generated package dependencies or raising generated output beyond C# 7.3.
- Switching from the current MSTest.Sdk/MTP package bridge without evidence that it provides distinct value over the package-free runners.

## Acceptance

- [ ] Official signals are rechecked and summarized with dates and source links in the relevant docs/rollup.
- [ ] Generated package-free `net48`, C# 7.3 compatibility, deterministic diagnostics, and 574/578 test baselines are preserved or any change is explicitly justified.
- [ ] Task 0401 remains blocked absent explicit user approval.
- [ ] A new bounded implementation packet is selected and linked from `agent/tasks.md`.
- [ ] Relevant docs build, stale-reference scans, and whitespace checks pass.

## References

- [Task 0471 rollup](tasks-rollup.md#task-0471-imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Agentic execution](agentic-execution.md)

## Handoff Notes

- Do not use Python.
- Reuse official primary sources only for ecosystem signal checks.
- Treat Task 0401 as blocked until the user explicitly approves implementation.
