# Task: roadmap-refresh-after-imported-csharp-null-conditional-member-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0473 imported C# null-conditional member user-defined multiplicative operator policy

## Objective

Recheck official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals after imported C# null-conditional member user-defined multiplicative operator assignment, preserve generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and the 574/578 test baselines, then select the next bounded implementation slice.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata and enabled mutable local `*=`, `/=`, and `%=` through one selected imported static binary operator.
- Task 0469 extended that precursor to regular readable/writable imported C# field/property member targets.
- Task 0471 extended the same precursor to regular readable/writable imported C# indexer targets with supported arguments.
- Task 0473 extended the precursor to null-conditional readable/writable imported C# field/property member targets while preserving skipped right-side evaluation and C# 7.3 guard lowering without changing the 574/578 baselines.

## Scope

In:

- Recheck the official language/platform/package/testing/editor/CI sources named in Project Policy and the Task 0472 rollup.
- Confirm whether Task 0473 changes any generated-artifact, test-host, package, or CI baseline.
- Compare Feature Status, Type System, Lowering, .NET Interop, Diagnostics, Project Policy, Work Ledger, tasks, and traceability for stale backlog/current-state wording.
- Select the next bounded implementation slice before null-conditional indexer user-defined operators, checked user-defined operators, TypeSharp-authored operators, true C# 14 instance compound-assignment operators, or broader overload-ranking expansion.

Out:

- Implementing the selected next slice.
- Adding dependencies or changing generated target frameworks/language version.
- Repeating broad official-source text when no current signal changed; prefer concise deltas.

## Acceptance

- [ ] Official signals are rechecked from primary sources and current/stable vs preview inputs remain separated.
- [ ] Generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and 574/578 baselines are preserved unless a source-backed change requires a deliberate update.
- [ ] Task 0474 records the next bounded implementation slice in `agent/tasks.md`, `agent/traceability.md`, `agent/tasks-rollup.md`, and docs Work Ledger.
- [ ] Any stale Task 0473 active wording is removed outside the rollup.
- [ ] Relevant build/test/docs/stale-reference/whitespace verification passes.

## References

- [Task 0473 rollup](tasks-rollup.md#task-0473-imported-csharp-null-conditional-member-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Handoff Notes

- Do not use Python.
- Preserve the package-free generated artifact boundary and current test-host package split.
- Prefer the next smallest implementation slice that advances the C# 14-inspired user-defined compound-assignment roadmap while keeping generated C# 7.3-compatible.
