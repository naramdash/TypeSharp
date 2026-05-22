# Task: roadmap-refresh-after-imported-csharp-member-multiplicative-compound-assignment-checked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 07:45:46 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, testing, editor, and CI roadmap signals after imported C# regular member multiplicative compound assignment checked/unchecked overflow policy landed, preserve the generated package-free `net48`, C# 7.3-compatible lowering, deterministic diagnostics, and current MSTest.Sdk/MTP package-shard baseline, keep Task 0401 blocked absent explicit approval, and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0459 rollup](tasks-rollup.md#task-0459-imported-csharp-regular-member-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck current official C#, F#, TypeScript, .NET Framework, .NET, NuGet package, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals against TypeSharp's generated `net48`/C# 7.3 baseline.
- Confirm whether the imported C# regular member checked/unchecked multiplicative assignment slice changes any generated-artifact, package-shard, docs, or CI assumptions.
- Keep Task 0401 blocked unless the user explicitly approves implementing the GitHub Actions `npm` process-launch fix.
- Choose the next bounded implementation task after imported regular member checked/unchecked multiplicative assignment, with imported indexer checked-overflow policy as the likely next candidate unless the official refresh changes priorities.
- Update canonical docs, task queue, traceability, and work ledger state.

Out:

- Implementing imported indexer checked-overflow targets in this roadmap task.
- Implementing null-conditional checked-overflow targets or user-defined multiplicative operators.
- Implementing Task 0401's GitHub Actions CI fix without explicit user approval.

## Acceptance Criteria

- [ ] Official language/platform/package/testing/editor/CI signals are rechecked from official sources and summarized.
- [ ] Generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and the 572-test package-shard baseline are either preserved or explicitly updated with evidence.
- [ ] Task 0401 remains blocked unless explicit approval arrives.
- [ ] The next bounded active task is selected and packeted.
- [ ] Docs, task ledger, traceability, and Work Ledger are updated.
- [ ] Docs build, stale-reference scan, and whitespace check pass.

## Verification

Command: TBD
Expected: docs build, stale-reference scan, and whitespace check pass after roadmap updates.
Result: TBD

## Handoff

Done:

- Task 0459 implemented imported C# regular member `checked(...)` and `unchecked(...)` multiplicative assignment wrappers for readable/writable metadata-backed instance/static field/property targets, reused the bounded integral/floating-point/decimal assign-back policy, preserved generated package-free `net48` C# 7.3 block lowering, kept imported indexer/null-conditional checked-overflow targets out of scope, and passed focused/full package-free plus MTP package-shard verification.

Remaining:

- Recheck official roadmap signals and pick the next bounded slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
