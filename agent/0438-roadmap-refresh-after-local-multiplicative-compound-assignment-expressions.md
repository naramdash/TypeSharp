# Task: roadmap-refresh-after-local-multiplicative-compound-assignment-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 00:22:49 +09:00
Source: Task 0437 local multiplicative compound assignment expressions

## Objective

Recheck the official language/platform/package/tooling signals after local mutable `*=`, `/=`, and `%=` landed, confirm TypeSharp's generated `net48`/C# 7.3/package-free artifact baseline, keep Task 0401 blocked unless the user explicitly approves it, and select the next bounded Q1 implementation slice.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md)
- [Task 0437 rollup](tasks-rollup.md#task-0437-local-multiplicative-compound-assignment-expressions)

## Scope

In:

- Recheck current official C#, F#, TypeScript, .NET Framework/.NET, NuGet, .NET testing/MSTest SDK, VS Code, and GitHub Actions signals relevant to the next TypeSharp slice.
- Confirm whether generated TypeSharp artifacts still target package-free `net48` with C# 7.3-compatible generated source.
- Reaffirm or update the isolated `net10.0` MSTest.Sdk/MTP package-bridge position.
- Keep Task 0401 blocked unless the user explicitly approves implementation of the GitHub Actions `npm` process-launch fix.
- Select the next bounded implementation task and update `agent/tasks.md`, `agent/tasks-rollup.md`, `agent/traceability.md`, [Feature Status](../docs/src/content/docs/feature-status.md), [Project Policy](../docs/src/content/docs/project-policy.md), and [Work Ledger](../docs/src/content/docs/work-ledger.md) as needed.

Out:

- Implementing the selected follow-up slice.
- Implementing Task 0401 without explicit user approval.
- Adding a new test-runner package bridge unless the official-source review shows distinct value beyond the existing MSTest.Sdk/MTP bridge.

## Acceptance Criteria

- [ ] Official source links and dates are refreshed in the relevant docs/rollup.
- [ ] Generated `net48`/C# 7.3/package-free artifact boundaries are reaffirmed or any drift is explicitly documented.
- [ ] Test-host package strategy is reaffirmed or changed with rationale.
- [ ] Task 0401 remains blocked unless explicitly approved.
- [ ] A concrete next Q1 task packet is selected and linked from `agent/tasks.md`.

## Verification

Command: TBD
Expected: docs build and `git diff --check`; implementation verification only if this refresh uncovers an urgent small correction.
Result: TBD

## Handoff

Done:

- Task 0437 implemented local multiplicative compound assignment expressions for mutable primitive integral numeric locals.

Remaining:

- Recheck official signals and pick the next bounded implementation slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
