---
title: Project Ledger
description: Canonical TypeSharp source-of-truth documents grouped for web navigation.
---

Docs-site is the canonical standard language and project ledger surface. `docs/` remains a temporary operating area for agentic goal work and handoff state only.

Use this page for integrated web navigation. The detailed ownership matrix lives in [Document Ownership](../document-ownership/).

## Operating Rule

| Surface | Role | Update Rule |
| --- | --- | --- |
| `docs-site/` | Canonical standard language, project ledger, reference, guide, and user-facing documentation. | Update first for standard language and project reference decisions. |
| `docs/` | Temporary agentic goal work, active task packets, `tasks-rollup.md`, checklist, traceability, handoff state, and execution-control files. | Keep operational and short; do not add long-lived language reference here. |
| `agent.md` | Codex CLI goal bootstrap and long-running agent instructions. | Keep concise and operational. |

## Project Record Pages

- [Core Goal](../goal/): project mission, required proof, non-goals, and adoption boundaries.
- [Project Requirements](../requirements/): required platform, language, compiler, interop, tooling, runtime, quality, security, and release constraints.
- [Feature Status](../feature-status/): MVP, stable backlog, preview watch, experimental, and rejected feature classification.
- [Project Policy](../project-policy/): architecture, dependency, target framework, regression, feature review, parser fixture, release, and official-reference tracking policy.
- [Document Ownership](../document-ownership/): target owner for each former `docs/` source and the remaining temporary agentic files.
- [Work Ledger](../work-ledger/): active task state and compressed completed-work rollup.
- [Agentic Workflow](../agentic-workflow/): how Codex CLI goal and long-running agents use the canonical records.

## Language And Compiler Contracts

- [Grammar](../grammar/) and [Grammar And Language Reference](../reference/): stable syntax, grammar summaries, feature spec index, and parser evidence.
- [Modules And Imports](../modules/): source module paths, imports, exports, and generated containers.
- [Type System](../type-system/): inference, null safety, `unknown`, `dynamic`, structural shapes, intersections, unions, and public ABI boundaries.
- [Lowering](../lowering/): TypeSharp-to-C# 7.3 lowering contract and fixture evidence.
- [.NET Interop](../dotnet-interop/): C#/.NET Framework interop, runtime ABI, public ABI, and host compatibility.
- [API And CLI Reference](../api/): manifest, runtime/core libraries, generated assembly layout, and API reference entry points.

## Tooling And Adoption

- [CLI](../cli/): command surface, project workflow, formatting convention, diagnostics output, exit codes, and source discovery.
- [VS Code And LSP](../vscode-lsp/): extension, language server, formatting, diagnostics, hover, definition, and completion workflow.
- [Diagnostics](../diagnostics/): diagnostic code ranges, descriptor metadata, JSON/text shape, and explanation surface.
- [Migration](../migration/): incremental .NET Framework/C# adoption guide.
- [Examples](../examples/), [Tutorials](../tutorials/), and [Cookbook](../cookbook/): runnable examples, learning paths, and recipes.

## Agentic Work Records

These are the only `docs/` records that should remain:

- [`docs/README.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/README.md): short index for the agentic work surface.
- [`docs/agentic-execution.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/agentic-execution.md): long-running agent execution contract.
- [`docs/progress.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/progress.md): task packet, rollup, commit, and handoff policy.
- [`docs/checklist.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/checklist.md): executable project checklist and remaining work source.
- [`docs/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks.md): current active task index and completed range pointer.
- [`docs/tasks-rollup.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks-rollup.md): compressed completed task history.
- [`docs/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/traceability.md): goal, requirements, features, checklist, and evidence connections.
- [`docs/adr.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/adr.md): architecture decision record format.

## Change Discipline

When a decision changes:

1. For standard language or project ledger decisions, update the docs-site canonical page listed in [Document Ownership](../document-ownership/).
2. Do not recreate standard-document bridge stubs under `docs/`.
3. Update implementation, tests, examples, and task packets if the rule is already implemented or being implemented.
4. Run the docs-site build when the change affects web pages or navigation.
