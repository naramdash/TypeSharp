# Task: roadmap-refresh-after-imported-csharp-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0467 imported C# user-defined multiplicative operator policy

## Objective

Recheck current official language, platform, package, testing, editor, and CI signals after the imported C# static binary user-defined multiplicative operator precursor. Preserve generated package-free `net48`, generated C# 7.3 compatibility, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard baseline while selecting the next bounded TypeSharp implementation slice.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata separately from ordinary method metadata.
- Mutable local `let mut` `*=`, `/=`, and `%=` assignment can now use one selected imported static binary multiplicative operator when the result assigns back to the target.
- Generated source remains C# 7.3-compatible because accepted cases lower to ordinary compound assignment that the generated C# compiler binds to the imported operator.
- C# 14 instance compound-assignment operators, imported member/indexer/null-conditional user-defined operator targets, TypeSharp-authored operators, checked user-defined operators, broader overload ranking, and Task 0401 remain out of scope unless the user explicitly redirects.

## Scope

- Recheck official sources already tracked by Feature Status and Project Policy for C#, F#, TypeScript, .NET Framework/.NET, NuGet, .NET test platforms, MSTest SDK/MTP, xUnit.net/NUnit comparison signals, VS Code, and GitHub Actions.
- Confirm Task 0467 did not change generated-artifact requirements or package policy beyond raising the shared catalog to 570 cases and the package-shard MTP minimum to 574 tests.
- Update Feature Status, Project Policy, Work Ledger, tasks, and traceability if the current official signals or queue state need changes.
- Select the next bounded implementation task and create its task packet.

## Out Of Scope

- Implementing Task 0401 without explicit user approval.
- Implementing C# 14 instance compound-assignment operators or TypeSharp-authored operator declarations during the refresh.
- Changing generated target frameworks, generated C# language version, package restore policy, or test framework selection without evidence that the project baseline should change.

## Acceptance

- [ ] Official signal review is recorded with links and concrete version/package/platform facts.
- [ ] Generated package-free `net48` and C# 7.3 compatibility remain explicitly preserved or any change is justified with evidence.
- [ ] MSTest.Sdk/MTP package-shard baseline is updated or reaffirmed at 574 tests.
- [ ] Task 0401 remains blocked unless the user explicitly approves the CI process-launch fix.
- [ ] The next bounded implementation packet is created, and active ledgers point to it.

## References

- [Task 0467 rollup](tasks-rollup.md#task-0467-imported-csharp-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Traceability](traceability.md)

## Handoff Notes

- Start from the official-source list used by Task 0466 and include the C# 14 user-defined compound assignment speclet because Task 0467 deliberately implemented only the static binary C# 7.3-compatible precursor.
- Do not use Python.
