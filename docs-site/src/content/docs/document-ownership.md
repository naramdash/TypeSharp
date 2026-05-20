---
title: Document Ownership
description: Target ownership model for TypeSharp documentation after the docs-site canonical ledger migration.
---

The target model is simple: docs-site is the canonical standard language and project ledger surface. `docs/` is only the temporary operating area for agentic goal work, handoff packets, checklist state, traceability, and execution control.

Task `0251-docs-site-canonical-language-ledger` moved the durable language and project records into docs-site. Task `0255-docs-canonical-cleanup` removed the remaining standard-document bridge stubs from `docs/`.

## Ownership Rule

| Surface | Target Role |
| --- | --- |
| `docs-site/src/content/docs/` | Canonical human-readable language reference, feature status, lowering guide, interop contract, CLI/LSP guide, examples guide, migration guide, and project ledger pages. |
| `docs/` | Temporary agentic workspace for active goal runs, task packets, rollups, checklist/traceability files, handoff state, and execution-control documents. |
| `agent.md` | Minimal bootstrap file for Codex goal runs. It points to docs-site canonical pages and the remaining `docs/` temporary work surface. |

## Canonical Standard Pages

Former `docs/` bridge files below are no longer present. The target owner is the docs-site page to update first.

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

Raw benchmark artifacts live under `docs-site/research/`. Source and runnable samples live under root `examples/`.

## Temporary Agentic Work Surface

| File Or Folder | Target Role |
| --- | --- |
| `docs/README.md` | Short index for the remaining agentic work surface. |
| `docs/agentic-execution.md` | Execution contract for goal/Ralph/long-running agent runs. |
| `docs/progress.md` | Task packet, rollup, commit, and handoff mechanics. |
| `docs/checklist.md` | Remaining implementation and verification work source. |
| `docs/traceability.md` | Evidence bridge connecting goal, requirements, features, checklist, and completed behavior. |
| `docs/tasks.md` | Active task packet index while agentic goal work is running. |
| `docs/*-task-ledger-rollup.md` | Temporary compressed work history until folded further into docs-site work ledger. |
| `docs/adr.md` | ADR authoring template for large decisions. |

## Maintenance Rule

1. Put new standard language/project reference material in docs-site canonical pages.
2. Do not add new `docs/` bridge stubs for standard docs.
3. Keep active task, handoff, checklist, traceability, and execution-control documents in `docs/`.
4. Update `agent.md`, `docs/agentic-execution.md`, Work Ledger, Project Ledger, and Agentic Workflow when agent work selection or ownership rules change.
5. Run docs-site build and stale-path scans before marking docs ownership work done.
