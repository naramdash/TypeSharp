# Task: roadmap-refresh-after-optional-default-parameters

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 02:15:18 +09:00
End Time: TBD

## Objective

Recheck the current official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest/xUnit.net, VS Code, and GitHub Actions signals after the optional/default parameter implementation slice, then select the next bounded TypeSharp task without changing the generated `net48` baseline unless official evidence requires it.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md#task-0376-optional-default-parameter-declaration-slice)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- Official C#/F#/TypeScript/.NET Framework/NuGet/.NET testing/MSTest/xUnit.net/VS Code/GitHub Actions documentation and release signals

## Scope

In:
- Recheck official source signals that can affect TypeSharp's language, generated artifact, test-host, NuGet, editor, or CI roadmap.
- Separate stable requirements from preview/watch items.
- Confirm whether generated TypeSharp artifacts still target `net48` and C# 7.3-compatible generated source.
- Choose the next bounded implementation or planning task and update `agent/tasks.md` plus canonical docs ledgers.

Out:
- Implementing the next language feature inside this refresh task.
- Changing generated target frameworks, compiler package dependencies, or CI images without direct official-reference justification.
- Replacing the package-free shard runner or MSTest bridge without a separate task.

## Acceptance Criteria

- [ ] Official C#/F#/TypeScript/.NET Framework/NuGet/.NET testing/MSTest/xUnit.net/VS Code/GitHub Actions signals are rechecked and cited in the rollup.
- [ ] Generated artifact baseline is explicitly confirmed or a justified change task is created.
- [ ] Feature Status, Work Ledger, task rollup, and traceability reflect the selected next task.
- [ ] `agent/tasks.md` points at the selected next active packet or requested queue item.
- [ ] Verification includes docs build and `git diff --check`.

## Verification

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap ledger updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0376 completed optional/default parameter declarations and moved this roadmap refresh into the active slot.

Remaining:
- Recheck official references, decide the next bounded work item, update ledgers, and commit/push the refresh.

Blocked:
- None.
