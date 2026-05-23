# Task: roadmap-refresh-after-imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0471 imported C# regular indexer user-defined multiplicative operator policy

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular indexer user-defined multiplicative operator assignment landed and after Task 0401 fixed the GitHub Actions `npm` process-launch failure, preserve TypeSharp's generated package-free `net48` and C# 7.3 baseline, keep the shared catalog/package-shard baselines at 574/578 unless evidence requires otherwise, and select the next bounded implementation slice.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata and enabled mutable local `*=`, `/=`, and `%=` assignment through one selected imported static binary operator.
- Task 0469 extended that precursor to regular readable/writable imported C# field/property member targets.
- Task 0471 extended the precursor to regular readable/writable imported C# indexer targets with supported arguments while preserving primitive indexer behavior and ordinary C# 7.3 compound-assignment lowering.
- The shared catalog is now 574 cases, package-free shard expectations are `144`, `144`, `143`, and `143`, and the MTP package-shard minimum is 578 tests.
- Task 0401 is now complete: the VS Code live smoke uses a Windows `cmd.exe /d /s /c "npm ..."` wrapper when launched from the C# test runner, avoiding the GitHub Actions `npm.cmd` resolution failure while preserving non-Windows `npm` launches.

## Scope

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals.
- Record any relevant changes in Feature Status, Project Policy, Work Ledger, tasks, traceability, and rollup docs.
- Preserve generated package-free `net48`, C# 7.3-compatible generated source, deterministic diagnostics, and the current test-host package policy unless official evidence changes the decision.
- Select one concrete next implementation slice with a bounded packet.
- Preserve the Task 0401 CI process-launch fix while evaluating current GitHub Actions signals.

## Out Of Scope

- Reworking Task 0401 beyond the approved `npm` process-launch fix.
- Implementing the next language/compiler slice during this roadmap refresh.
- Adding generated package dependencies or raising generated output beyond C# 7.3.
- Switching from the current MSTest.Sdk/MTP package bridge without evidence that it provides distinct value over the package-free runners.

## Acceptance

- [ ] Official signals are rechecked and summarized with dates and source links in the relevant docs/rollup.
- [ ] Generated package-free `net48`, C# 7.3 compatibility, deterministic diagnostics, and 574/578 test baselines are preserved or any change is explicitly justified.
- [ ] Task 0401 remains recorded as complete and the approved CI process-launch fix is preserved.
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
- Task 0401 has explicit user approval and is complete; do not reintroduce the blocked status during this refresh.
