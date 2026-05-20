---
title: Agentic Workflow
description: How Codex CLI goal, Ralph mode, and long-running agents should use the TypeSharp docs.
---

TypeSharp is designed to be worked on by long-running agents without losing the project goal, task state, or verification requirements. Since task `0257-docs-agent-directory-rename`, `docs/` is the canonical standard language and project ledger surface, and `agent/` is the temporary agentic work surface.

## Source Of Truth

| Need | Canonical File |
| --- | --- |
| Goal text to give Codex CLI `/goal` | [`agent.md`](https://github.com/naramdash/TypeSharp/blob/main/agent.md) |
| Project mission and success criteria | [Core Goal](../goal/) |
| Standard language/project reference ownership | [Document Ownership](../document-ownership/) |
| Task selection, queue policy, Done criteria | [`agent/agentic-execution.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/agentic-execution.md) |
| Active task pointer | [`agent/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/tasks.md) |
| Completed task rollup | [`agent/tasks-rollup.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/tasks-rollup.md) |
| Remaining implementation work | [`agent/checklist.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/checklist.md) |
| Evidence connections | [`agent/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/agent/traceability.md) |

docs project record pages are the canonical human-visible management surface for standard language and project ledger records. The `agent/` files stay operational for active agent work, handoff, traceability, `tasks-rollup.md`, and execution control.

## Bootstrapping A Codex Goal Run

Use the goal text in `agent.md`. Then the agent should read:

1. `agent.md`
2. [Core Goal](../goal/)
3. `agent/agentic-execution.md`
4. `agent/tasks.md`
5. the active task packet linked by `agent/tasks.md`, when one exists
6. `agent/tasks-rollup.md`
7. `agent/checklist.md`
8. `agent/traceability.md`
9. [Document Ownership](../document-ownership/)
10. [Project Ledger](../project-ledger/)
11. [Work Ledger](../work-ledger/)
12. this Agentic Workflow page
13. the docs canonical page or `agent/` temporary work file directly related to the chosen task

If a task explicitly needs a Codex skill, read `agent/codex-skills.md` and the matching installed skill `SKILL.md` in the user Codex home.

This order keeps the running goal stable even when docs navigation changes.

After a task changes a standard language or project ledger decision, update the docs canonical page listed in [Document Ownership](../document-ownership/). Do not recreate standard-document bridge stubs under `docs/` or store durable standard docs under `agent/`.

## Choosing Work

The agent should:

- re-read `agent/tasks.md` at each loop and check `State`, `User Task Inbox`, and `Agent Task Queue`;
- continue an `In Progress` task packet when one exists and the current request does not override it;
- otherwise promote unchecked user inbox items before choosing agent-owned queue work;
- if the task sections are empty, choose from unchecked `agent/checklist.md` items using the queue rules in `agent/agentic-execution.md`;
- create or update a task packet for work that will outlive one session;
- record verification results before marking a task `Done`;
- update [Work Ledger](../work-ledger/) when task state or `agent/tasks-rollup.md` changes;
- update docs user guides when the canonical change affects public behavior.

## Avoiding Split-Brain Documentation

Do not let the website and agent records become competing authorities.

| If You Change | Also Check |
| --- | --- |
| Goal, non-goal, success criteria | [Core Goal](../goal/), [Project Ledger](../project-ledger/), and `agent.md` |
| Task state or rollup | `agent/tasks.md`, active task packet, `agent/tasks-rollup.md`, [Work Ledger](../work-ledger/), [Project Ledger](../project-ledger/) if categories change |
| CLI, VS Code, diagnostics, or examples | matching docs page, ownership listed in [Document Ownership](../document-ownership/), smoke commands |
| Grammar or lowering | [Grammar](../grammar/), [Grammar And Language Reference](../reference/), [Advanced Topics](../advanced/), and ownership listed in [Document Ownership](../document-ownership/) |

## Verification

For documentation-only work, the minimum useful checks are:

```powershell
cd docs
npm run build
```

For implementation work, use the commands recorded in the task packet and the relevant compiler/CLI smoke tests before marking the task complete.
