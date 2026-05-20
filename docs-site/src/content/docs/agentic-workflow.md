---
title: Agentic Workflow
description: How Codex CLI goal, Ralph mode, and long-running agents should use the TypeSharp docs.
---

TypeSharp is designed to be worked on by long-running agents without losing the project goal, task state, or verification requirements. Since task `0251-docs-site-canonical-language-ledger`, docs-site is the canonical standard language ledger and `docs/` is the temporary agentic work surface.

## Source Of Truth

| Need | Canonical File |
| --- | --- |
| Goal text to give Codex CLI `/goal` | [`agent.md`](https://github.com/naramdash/TypeSharp/blob/main/agent.md) |
| Project mission and success criteria | [Core Goal](../goal/) |
| Standard language/project reference ownership | [Document Ownership](../document-ownership/) |
| Task selection, queue policy, Done criteria | [`docs/agentic-execution.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/agentic-execution.md) |
| Active task and completed rollups | [`docs/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks.md) |
| Remaining implementation work | [`docs/checklist.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/checklist.md) |
| Evidence connections | [`docs/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/traceability.md) |

Docs-site project record pages are the canonical human-visible management surface for standard language and project ledger records. The remaining `docs/` files stay operational for active agent work, handoff, traceability, rollups, and execution control.

## Bootstrapping A Codex Goal Run

Use the goal text in `agent.md`. Then the agent should read:

1. `agent.md`
2. [Core Goal](../goal/)
3. `docs/agentic-execution.md`
4. `docs/tasks.md`
5. `docs/checklist.md`
6. `docs/traceability.md`
7. [Document Ownership](../document-ownership/)
8. [Project Ledger](../project-ledger/)
9. [Work Ledger](../work-ledger/)
10. this Agentic Workflow page
11. the docs-site canonical page or `docs/` temporary work file directly related to the chosen task

This order keeps the running goal stable even when docs-site navigation changes.

After a task changes a standard language or project ledger decision, update the docs-site canonical page listed in [Document Ownership](../document-ownership/). Do not recreate standard-document bridge stubs under `docs/`.

## Choosing Work

The agent should:

- continue an `In Progress` task packet when one exists and the current request does not override it;
- otherwise choose from unchecked `docs/checklist.md` items using the queue rules in `docs/agentic-execution.md`;
- create or update a task packet for work that will outlive one session;
- record verification results before marking a task `Done`;
- update [Work Ledger](../work-ledger/) when task state or the completed-work rollup changes;
- update docs-site user guides when the canonical change affects public behavior.

## Avoiding Split-Brain Documentation

Do not let the website and agent records become competing authorities.

| If You Change | Also Check |
| --- | --- |
| Goal, non-goal, success criteria | [Core Goal](../goal/), [Project Ledger](../project-ledger/), and `agent.md` |
| Task state or rollup | `docs/tasks.md`, task rollup, [Work Ledger](../work-ledger/), [Project Ledger](../project-ledger/) if categories change |
| CLI, VS Code, diagnostics, or examples | matching docs-site page, ownership listed in [Document Ownership](../document-ownership/), smoke commands |
| Grammar or lowering | [Grammar](../grammar/), [Grammar And Language Reference](../reference/), [Advanced Topics](../advanced/), and ownership listed in [Document Ownership](../document-ownership/) |

## Verification

For documentation-only work, the minimum useful checks are:

```powershell
cd docs-site
npm run build
```

For implementation work, use the commands recorded in the task packet and the relevant compiler/CLI smoke tests before marking the task complete.
