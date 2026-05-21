# Task: roadmap-refresh-after-logical-unsigned-shift-assignment-imported-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 10:45:00 +09:00
End Time: TBD

## Objective

Recheck official language, platform, package, test, editor, and CI signals after imported C# indexer `>>>=` landed; confirm the generated artifact baseline and the package-test-host boundary; answer the `net10.0` NuGet test package recommendation explicitly; and select the next bounded implementation slice.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0396-logical-unsigned-shift-assignment-imported-indexer-targets](tasks-rollup.md#task-0396-logical-unsigned-shift-assignment-imported-indexer-targets)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Recheck official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions signals relevant to TypeSharp's current roadmap.
- Confirm whether TypeSharp-generated assemblies should continue targeting package-free `net48` with C# 7.3-compatible generated source.
- Confirm whether `MSTest.Sdk`/Microsoft Testing Platform remains the selected `net10.0` package-based test bridge or whether a broader package such as xUnit.net v3 now provides distinct project value.
- Select the next bounded implementation slice and document traceability.

Out:
- Implementing the selected language/compiler slice.
- Switching generated artifacts away from `net48`.
- Adding a second test framework package unless source review shows distinct value beyond the existing shared catalog/MSTest SDK shard bridge.
- Broad NuGet restore support for generated TypeSharp projects.

## Acceptance Criteria

- [ ] Current official signals are summarized with source links and dated evidence.
- [ ] Generated artifact target, C# language version, and package-free runtime/core constraints are either reaffirmed or explicitly changed with rationale.
- [ ] The `net10.0` test-host NuGet package decision is documented, including why the existing MSTest SDK/MTP shard bridge is sufficient or what a future xUnit.net v3 bridge would add.
- [ ] Feature Status, Project Policy if needed, Work Ledger, tasks queue, and traceability identify the next bounded slice.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap and ledger updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0396 implemented bounded metadata-backed imported C# instance indexer `>>>=` checker/backend lowering, single-evaluation receiver/index-argument behavior, generated `net48` C# consumer evidence, shared catalog count 530, and docs/ledger updates.
- The existing test-host package path is `net10.0` `MSTest.Sdk/4.2.3` over the shared catalog, with lock/source-mapping/audit controls and package-based shard projects.

Remaining:
- Recheck official sources, answer the NuGet test package recommendation in canonical docs, select the next bounded slice, verify, commit, and push.

Blocked:
- None.
