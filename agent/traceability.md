# TypeSharp Traceability

문서 기준일: 2026-05-20

이 파일은 agent가 목표, active work, evidence 위치를 빠르게 찾기 위한 운영용 bridge다. 상세 요구사항과 장기 사용자 문서는 docs canonical pages가 owner다.

## Canonical Sources

| Need | Source |
| --- | --- |
| Project mission, success criteria, non-goals | [Core Goal](../docs/src/content/docs/goal.md) |
| Required platform, language, compiler, interop, tooling, runtime, quality constraints | [Project Requirements](../docs/src/content/docs/requirements.md) |
| MVP, stable backlog, preview watch, experimental/rejected boundaries | [Feature Status](../docs/src/content/docs/feature-status.md) |
| Syntax and language contracts | [Grammar](../docs/src/content/docs/grammar.md), [Grammar And Language Reference](../docs/src/content/docs/reference.md) |
| Type system, lowering, interop, CLI/API, diagnostics | [Type System](../docs/src/content/docs/type-system.md), [Lowering](../docs/src/content/docs/lowering.md), [.NET Interop](../docs/src/content/docs/dotnet-interop.md), [API And CLI Reference](../docs/src/content/docs/api.md), [Diagnostics](../docs/src/content/docs/diagnostics.md) |
| Architecture, dependencies, fixture policy, regression policy, release policy | [Project Policy](../docs/src/content/docs/project-policy.md) |
| User-facing tutorials, examples, cookbook, migration | [docs/src/content/docs](../docs/src/content/docs) |

## Operational Sources

| Need | Source |
| --- | --- |
| Goal bootstrap and long-running agent rules | [../agent.md](../agent.md) |
| Queue policy, task packet template, Done criteria | [agentic-execution.md](agentic-execution.md) |
| Current active task and next priority | [tasks.md](tasks.md) |
| Completed task history | [tasks-rollup.md](tasks-rollup.md) |
| Remaining operational work | [checklist.md](checklist.md) |
| Task recording policy | [progress.md](progress.md) |
| ADR template | [adr.md](adr.md) |
| Web-visible ledger state | [Work Ledger](../docs/src/content/docs/work-ledger.md), [Project Ledger](../docs/src/content/docs/project-ledger.md), [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md) |

## Active Trace

| Work | Goal Link | Required Evidence |
| --- | --- | --- |
| None | No active task selected | Use [tasks.md](tasks.md) and [checklist.md](checklist.md) for the next task |

## Completed Evidence Index

| Evidence Area | Where To Look |
| --- | --- |
| Parser, binder, semantic skeleton | [tasks-rollup.md#foundation-parser-and-semantic-skeleton](tasks-rollup.md#foundation-parser-and-semantic-skeleton), `tests/fixtures/parser`, `tests/fixtures/diagnostics` |
| Runtime, backend, generated C# lowering | [tasks-rollup.md#runtime-build-backend-and-language-lowering](tasks-rollup.md#runtime-build-backend-and-language-lowering), `tests/fixtures/backend/csharp`, `tests/TypeSharp.Compiler.Tests/Program.cs` |
| C# interop and metadata diagnostics | [tasks-rollup.md#csharp-interop-and-metadata-diagnostics](tasks-rollup.md#csharp-interop-and-metadata-diagnostics), interop smoke tests in `tests/TypeSharp.Compiler.Tests/Program.cs` |
| CLI, VS Code, language server, docs build | [tasks-rollup.md#cli-vscode-and-tooling](tasks-rollup.md#cli-vscode-and-tooling), `vscode/typesharp`, docs build smoke |
| Documentation process and adoption | [tasks-rollup.md#documentation-process-release-and-adoption](tasks-rollup.md#documentation-process-release-and-adoption), docs pages, `docs/research` |
| Docs/agent directory ownership | [tasks-rollup.md#task-0257-docs-agent-directory-rename](tasks-rollup.md#task-0257-docs-agent-directory-rename), `docs/src/content/docs`, `agent/`, `.github/workflows/docs.yml` |
| Codex skills configuration | [tasks-rollup.md#task-0258-codex-skills-configuration](tasks-rollup.md#task-0258-codex-skills-configuration), `.codex/skills` |
| Parallel execution optimization | [tasks-rollup.md#task-0259-parallel-execution-optimization](tasks-rollup.md#task-0259-parallel-execution-optimization), `src/TypeSharp.Compiler/Checking`, `src/TypeSharp.Compiler/Building`, `tests/TypeSharp.Compiler.Tests/Program.cs` |
| Docs dependency update | [tasks-rollup.md#task-0260-docs-dependency-update](tasks-rollup.md#task-0260-docs-dependency-update), `docs/package.json`, `docs/package-lock.json`, `tests/TypeSharp.Compiler.Tests/Program.cs` |
| Docs TypeScript config conversion | [tasks-rollup.md#task-0261-docs-typescript-config-conversion](tasks-rollup.md#task-0261-docs-typescript-config-conversion), `docs/astro.config.ts`, `tests/TypeSharp.Compiler.Tests/Program.cs` |
| VS Code syntax highlighting extension install guide | [tasks-rollup.md#task-0262-vs-code-syntax-highlighting-extension-install-guide](tasks-rollup.md#task-0262-vs-code-syntax-highlighting-extension-install-guide), `vscode/typesharp`, [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md), `tests/TypeSharp.Compiler.Tests/Program.cs` |
| Docs tysh syntax highlighting | [tasks-rollup.md#task-0263-docs-tysh-syntax-highlighting](tasks-rollup.md#task-0263-docs-tysh-syntax-highlighting), `docs/astro.config.ts`, `docs/src/content/docs`, `vscode/typesharp/syntaxes/typesharp.tmLanguage.json`, `tests/TypeSharp.Compiler.Tests/Program.cs` |
| Modules, imports, exports, safety gates | [tasks-rollup.md#language-safety-modules-and-import-export](tasks-rollup.md#language-safety-modules-and-import-export), source module and diagnostics fixtures |
| Test suite quality audit | [tasks-rollup.md#task-0256-test-suite-quality-audit](tasks-rollup.md#task-0256-test-suite-quality-audit), `tests/TypeSharp.Compiler.Tests/Program.cs`, `tests/fixtures`, `vscode/typesharp/test`, `examples/runnable` |

## Update Rule

- Add rows only when an active task or canonical evidence location changes.
- Do not recreate detailed completed-work checklists here.
- Put user-facing or long-lived specification content in docs canonical pages, not in `agent/`.
