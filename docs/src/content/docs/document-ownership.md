---
title: Document Ownership
description: Target ownership model for TypeSharp documentation after the docs canonical ledger migration.
---

The target model is simple: `docs/` is the canonical standard language and project ledger surface. `agent/` is only the temporary operating area for agentic goal work, handoff packets, checklist state, traceability, and execution control.

Task `0251-docs-canonical-language-ledger` moved the durable language and project records into the docs site. Task `0255-docs-canonical-cleanup` removed the remaining standard-document bridge stubs from the former operational docs surface. Task `0257-docs-agent-directory-rename` made that boundary literal by renaming the public site source to `docs/` and the temporary agent work surface to `agent/`.

## Ownership Rule

| Surface | Target Role |
| --- | --- |
| `docs/src/content/docs/` | Canonical human-readable language reference, feature status, lowering guide, interop contract, CLI/LSP guide, examples guide, migration guide, and project ledger pages. |
| `agent/` | Temporary agentic workspace for active goal runs, active task packets, `tasks-rollup.md`, checklist/traceability files, handoff state, and execution-control documents. |
| `.codex/skills/` | Project-local Codex skill packages selected for TypeSharp goal work. |
| `agent.md` | Minimal bootstrap file for Codex goal runs. It points to docs canonical pages and the remaining `agent/` temporary work surface. |

## Canonical Standard Pages

Former operational bridge files below are no longer present. The target owner is the docs page to update first.

| Former Source | Canonical Owner |
| --- | --- |
| `docs/goal.md` | [Core Goal](../goal/) |
| `docs/requirements.md` | [Project Requirements](../requirements/) |
| `docs/feature-map.md`, `docs/feasibility.md` | [Feature Status](../feature-status/), [Advanced Topics](../advanced/) |
| `docs/grammar/**` | [Grammar](../grammar/), [Grammar And Language Reference](../reference/), [Modules And Imports](../modules/), [Type System](../type-system/) |
| `docs/standard-library.md`, `docs/runtime-abi.md` | [API And CLI Reference](../api/), [.NET Interop](../dotnet-interop/) |
| `docs/csharp-interop.md` | [.NET Interop](../dotnet-interop/) |
| `docs/lowering.md` | [Lowering](../lowering/) |
| `docs/cli.md`, `docs/formatting.md` | [CLI](../cli/), [API And CLI Reference](../api/), [VS Code And LSP](../vscode-lsp/) |
| `docs/diagnostics.md` | [Diagnostics](../diagnostics/), [Troubleshooting](../troubleshooting/) |
| `docs/migration-guide.md` | [Migration](../migration/) |
| `docs/architecture.md`, `docs/dependencies.md`, `docs/framework-targeting.md`, `docs/release.md`, `docs/regression-testing.md`, `docs/feature-review.md`, `docs/parser-fixtures.md`, `docs/references.md` | [Project Policy](../project-policy/), [Project Configuration](../project-configuration/), [Advanced Topics](../advanced/) |

Raw benchmark artifacts live under `docs/research/`. Source and runnable samples live under root `examples/`.

## Temporary Agentic Work Surface

| File | Target Role |
| --- | --- |
| `agent/README.md` | Short index for the remaining agentic work surface. |
| `agent/agentic-execution.md` | Execution contract for goal/Ralph/long-running agent runs. |
| `agent/progress.md` | Task packet, rollup, commit, and handoff mechanics. |
| `agent/checklist.md` | Remaining implementation and verification work source. |
| `agent/traceability.md` | Evidence bridge connecting goal, requirements, features, checklist, and completed behavior. |
| `agent/tasks.md` | Active task packet index while agentic goal work is running. |
| `agent/NNNN-*.md` | Temporary active task packets while long-running work is in progress. |
| `agent/tasks-rollup.md` | Temporary compressed work history kept under `agent/` until folded further into docs Work Ledger. |
| `agent/adr.md` | ADR authoring template for large decisions. |

## Maintenance Rule

1. Put new standard language/project reference material in docs canonical pages.
2. Do not add new `docs/` bridge stubs for standard docs.
3. Keep active task, handoff, checklist, traceability, and execution-control documents in `agent/`.
4. Update `agent.md`, `agent/agentic-execution.md`, Work Ledger, Project Ledger, and Agentic Workflow when agent work selection or ownership rules change.
5. Run docs build and stale-path scans before marking docs ownership work done.
