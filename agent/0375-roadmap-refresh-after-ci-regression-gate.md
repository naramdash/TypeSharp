# Task: roadmap-refresh-after-ci-regression-gate

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 01:33:44 +09:00
End Time: TBD

## Objective

Refresh the TypeSharp roadmap after the Windows regression CI gate landed, confirm whether current C#, .NET, TypeScript, NuGet, GitHub Actions, MSTest/MTP, xUnit.net, and VS Code ecosystem signals change the generated-artifact baseline, and select the next bounded implementation or adoption task.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0374-ci-regression-gate-for-shard-runner-and-mstest-smoke)
- [test README](../test/README.md)
- [regression workflow](../.github/workflows/regression.yml)
- Official source docs for the ecosystem areas being rechecked.

## Scope

In:
- Recheck official source signals that could affect TypeSharp's generated `net48` baseline, C# 7.3 output contract, package-free runtime/core policy, CI posture, or next adoption slice.
- Record whether `.github/workflows/regression.yml` changes release confidence, package-runner policy, or remaining CI gaps.
- Update Feature Status, Work Ledger, Project Policy, tasks queue, and traceability as needed.
- Select one concrete next task and create its packet.

Out:
- Implementing the next selected feature or adoption slice.
- Replacing the package-free shard runner with package-based `dotnet test`.
- Adding xUnit.net v3 unless the roadmap refresh explicitly selects it as the next task.
- Changing generated artifact target frameworks, generated C# language version, or runtime/core package policy.

## Acceptance Criteria

- [ ] Official source signals are rechecked or explicitly scoped out with rationale.
- [ ] Baseline implications are recorded for generated artifacts, CI, NuGet/test-host use, and VS Code/adoption work.
- [ ] Docs and agent ledgers reflect task 0374 completion and the newly selected next task.
- [ ] Verification includes docs build and diff checks.
- [ ] A next active task packet exists and `agent/tasks.md` points at it.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0374 added the Windows regression CI gate for package-free shard runners plus focused MSTest/MTP smoke evidence.

Remaining:
- Recheck official signals and pick the next bounded task.

Blocked:
- None.
