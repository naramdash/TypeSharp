# Agentic Execution Contract

문서 기준일: 2026-05-20

이 문서는 TypeSharp 장기 작업 세션이 어떤 파일을 읽고, 어떻게 작업을 고르고, 어떤 상태를 남겨야 하는지만 정의한다. 목표와 표준 사양의 owner는 docs canonical pages다.

## Source Order

새 goal/Ralph/장기 작업 세션은 아래 순서로 읽는다.

1. [../agent.md](../agent.md)
2. [Core Goal](../docs/src/content/docs/goal.md)
3. [tasks.md](tasks.md), including `State`, `User Task Inbox`, and `Agent Task Queue`
4. `tasks.md`가 가리키는 active task packet
5. [tasks-rollup.md](tasks-rollup.md)
6. [checklist.md](checklist.md)
7. [traceability.md](traceability.md)
8. [Document Ownership](../docs/src/content/docs/document-ownership.md)
9. [Project Ledger](../docs/src/content/docs/project-ledger.md)
10. [Work Ledger](../docs/src/content/docs/work-ledger.md)
11. [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md)
12. 작업과 직접 관련된 docs canonical page 또는 repo source

Codex skill이 필요한 작업에서만 [codex-skills.md](codex-skills.md)를 읽고, 설치된 skill 본문은 사용자 Codex home의 해당 `SKILL.md`에서 확인한다.

## Stable Baseline

| Area | Rule |
| --- | --- |
| Generated target | Generated assemblies and runtime/core libraries must support `net48`. |
| Compiler/CLI/LSP host | Modern .NET host is allowed. |
| Backend | MVP backend is C# 7.3-compatible source generation. |
| Source extension | `.tysh` |
| Manifest | `TypeSharp.toml` |
| Public ABI | Structural shapes, type-level unions, and anonymous object shapes must not leak directly as public .NET ABI. |
| Interop | C#/.NET Framework library interop is a first-class goal. |
| Non-goal | No JS runtime compatibility, macro system, or .NET 10/11 runtime requirement as a default target. |

## Work Selection

1. Re-read [tasks.md](tasks.md) at the start of every loop.
2. If [tasks.md](tasks.md) has an active task, continue it unless a newer user request explicitly interrupts or supersedes it.
3. If there is no active task, promote the first unchecked item from `User Task Inbox` into `Agent Task Queue`.
4. If the user inbox is empty, choose the highest-priority `Requested` or `Ready` item from `Agent Task Queue`.
5. If both task sections are empty, choose the first unchecked item in [checklist.md](checklist.md).
6. If the work is too large for one turn, create `agent/NNNN-short-name.md`, set it as the active task in [tasks.md](tasks.md), and mark its agent queue row `In Progress`.
7. Use Q0-Q5 to order queue items:

| Queue | Meaning |
| --- | --- |
| Q0 | Goal/baseline conflict, broken ledger, invalid task state |
| Q1 | Spec gap blocking implementation |
| Q2 | Compiler skeleton or core language implementation |
| Q3 | Runtime or C# interop core |
| Q4 | CLI, VS Code, LSP, formatting, diagnostics tooling |
| Q5 | Samples, docs polish, tutorials, compatibility guides |

## Active Task Packet

Use this format for `agent/NNNN-short-name.md`:

```md
# Task: <short-name>

Status: Planned | In Progress | Blocked | Done
Queue: Q0 | Q1 | Q2 | Q3 | Q4 | Q5
Start Time: <yyyy-MM-dd HH:mm:ss zzz>
End Time: TBD

## Objective

<one sentence>

## Source Of Truth

- <links>

## Scope

In:
- <items>

Out:
- <items>

## Acceptance Criteria

- [ ] <criteria>

## Verification

Command:
Expected:
Result:

## Handoff

Done:
Remaining:
Blocked:
```

## Done Rules

- Code changes need meaningful tests or smoke coverage.
- Failing behavior needs diagnostic or negative fixture coverage.
- Fixture snapshot changes must be justified by the task.
- docs canonical pages must be updated for standard language/project reference changes.
- `agent/` operational files must be updated for task state, `User Task Inbox`, `Agent Task Queue`, checklist, traceability, or agent workflow changes.
- Completed active packets are summarized in [tasks-rollup.md](tasks-rollup.md), then removed.
- [tasks.md](tasks.md) must point to the next active task or `None`, and the queue row must be `Done`, `Blocked`, `Dropped`, or still explicitly pending.
- Run relevant verification and `git diff --check`. For docs changes, run `npm run build` in `docs`.

## Parallel Work Rules

- Parallelize read-only repository inspection and independent verification commands when their output paths do not overlap.
- Do not run two commands in parallel when they both write the same generated output, package folder, `tests/tmp` child, VS Code server folder, docs build output, or git index.
- Keep file edits serialized through one patch at a time.
- For compiler code, preserve deterministic source-order diagnostics when using source-file parallelism.

## Handoff

When stopping, leave only high-signal state:

- changed files
- commands run and result
- unverified commands
- next task candidate
- blockers or baseline risks
