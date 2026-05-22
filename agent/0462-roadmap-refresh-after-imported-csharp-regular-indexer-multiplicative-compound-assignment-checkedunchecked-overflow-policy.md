# Task: roadmap-refresh-after-imported-csharp-regular-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 08:11:52 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, testing, editor, and CI signals after imported C# regular indexer checked/unchecked multiplicative compound assignment landed, preserve generated package-free `net48`, C# 7.3-compatible lowering, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard baseline, keep Task 0401 blocked without explicit approval, and select the next bounded implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0461 rollup](tasks-rollup.md#task-0461-imported-csharp-regular-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Recheck current official C#, F#, TypeScript, .NET Framework, .NET, NuGet package, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals that can affect TypeSharp's generated artifact, host, or package/test baselines.
- Confirm Task 0461 preserved generated package-free `net48`, C# 7.3 checked/unchecked block lowering for statement-form imported regular indexer multiplicative assignment, deterministic diagnostics, and the 572-test package-shard minimum.
- Keep Task 0401's GitHub Actions npm process-launch fix blocked unless the user explicitly approves implementation.
- Update docs and agent ledgers with any changed roadmap signal.
- Select the next bounded implementation slice.

Out:

- Implementing the Task 0401 CI fix without explicit user approval.
- Implementing null-conditional checked-overflow targets or user-defined compound assignment operators in this roadmap-refresh task.
- Changing generated package, target framework, language-version, or test-host baselines without direct evidence.

## Acceptance Criteria

- [ ] Official signal recheck is recorded with exact versions/dates where they matter.
- [ ] Generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and the 572-test MTP package-shard baseline are reaffirmed or intentionally updated with evidence.
- [ ] Task 0401 remains blocked unless explicit approval is received.
- [ ] Feature Status, Work Ledger, task ledger, and traceability are updated.
- [ ] Docs build, package-free tests, package-shard smoke, stale-reference scan, and whitespace check pass.
- [ ] The next bounded task is selected with a packet or queue entry.

## Verification

Command: TBD
Expected: official signal review, docs build, package-free compiler catalog, MTP package-shard smoke, stale-reference scan, and whitespace check pass.
Result: TBD

## Handoff

Done:

- Task 0461 implemented imported C# regular indexer `checked(...)` and `unchecked(...)` multiplicative assignment wrappers for selected readable/writable metadata-backed instance indexers with supported arguments, reused the bounded integral/floating-point/decimal assign-back policy, lowered statement forms to C# 7.3 checked/unchecked blocks, preserved deterministic diagnostics, generated package-free `net48`, and the 568 shared-case/572 package-shard baselines.

Remaining:

- Recheck official roadmap signals and select the next bounded slice, likely before null-conditional checked-overflow or user-defined multiplicative operator work.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
