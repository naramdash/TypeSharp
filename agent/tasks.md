# Agentic Tasks

문서 기준일: 2026-05-21

이 파일은 goal-mode/Ralph/장기 실행 agent와 사용자가 공유하는 task control plane이다. 역할은 네 가지로만 나눈다.

| Section          | Owner | Purpose                                                 |
| ---------------- | ----- | ------------------------------------------------------- |
| State            | Agent | 현재 active task와 완료 범위만 기록한다.                |
| User Task Inbox  | User  | 사용자가 실행 중에도 `- [ ]` 문법으로 새 요청을 쌓는다. |
| Agent Task Queue | Agent | agent가 실제 처리 순서, 상태, packet을 관리한다.        |
| Rules            | Agent | 이 파일을 읽고 갱신하는 규칙만 둔다.                    |

## State

| Field              | Value                              |
| ------------------ | ---------------------------------- |
| Active task packet | None                               |
| Active summary     | None                               |
| Completed range    | 0001-0275                          |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md) |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [x] `docs-site`를 `docs`로 변경하기, `docs` 폴더는 `agent`로 변경하기
- [x] 이 프로젝트를 위해 유용한 codex skills 목록 확인하고 구성하기
- [x] Task 수행 및 프로젝트 코드들 병렬로 수행할 수 있는 구간을 찾아 최적화하기
- [x] docs package.json deps 최신화하기
- [x] docs 에 쓰이는 javascript는 최대한 typescript로 바꾸기, typescript로 바꿀 수 없는 부분은 명확하게 주석으로 설명하기
- [x] VSCode Syntax Highlighting Extension 만들고 설치 방법 설명하기, 사용자의 개입이 필요할 경우 사용자를 위해 marketplace에 extension을 올리는 방법 설명하는 임시 가이드 만들기
- [x] `docs` 폴더의 tysh 코드 예제 표시가 syntax highlight 될 수 있도록 하기
- [x] docs에 tysh 프로젝트가 실제로 어떻게 net48 런타임 아티펙트를 구성하는지 아키텍처 & 원리를 설명하는 문서 남기기, 적절하게 mermaid 다이어그램을 사용해서 tysh 프로젝트의 아키텍처 설명하기
- [x] [Vue Docs Writing Guide](https://github.com/vuejs/docs/blob/main/.github/contributing/writing-guide.md)를 확인해서 이 프로젝트에 맞게 문서 작성 가이드라인 만들기, 특히 tysh 예제 프로젝트 코드에 대한 문서 작성 가이드라인 만들기, 이모지를 적절하게 활용해서 가독성 높이기
- [x] GitHub Action 으로 중요한 변경이 있을때마다 실제로 이 언어와 관련된 중요 산출물들을 release를 통해 제공될 수 있게 변경하기
- [x] C# 공식문서 확인해서 현재 docs 폴더 내용 구체화하기. 현재 처럼 2단 구성이 아니라 C# 공식문서 만큼 상세하게 설명하기
- [x] tysh 예제 프로젝트 코드 현실성 있게 작성하기, 실제로 실행 가능한 코드로 작성하기
- [x] tysh 예제 프로젝트 코드에 대한 설명 추가하기, 각 코드 블록마다 설명을 추가해서 사용자가 이해하기 쉽게 만들기
- [x] 이 레포가 모노레포임을 감안하여 폴더 구조 과감하게 최적화하기. cli, test, docs, lang, agent 등으로 폴더 구조를 명확하게 나누고, 각 폴더에 README.md 파일을 추가해서 해당 폴더의 목적과 내용을 설명하기
- [x] vscode extension이 LSP 서버로서의 역할을 제대로 할 수 있도록 개선하기. 현재는 단순한 syntax highlighting 수준이지만, 실제로 코드 분석과 피드백을 줄 수 있는 수준으로 개선하기

## Agent Task Queue

| Priority | Status | Source         | Task                                                | Packet                                                                                                                                                         | Notes                                                                                                                                                   |
| -------- | ------ | -------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Q3       | Done   | Work ledger future area | Lambda binary value overload inference             | [tasks-rollup.md#task-0275-lambda-binary-value-overload-inference](tasks-rollup.md#task-0275-lambda-binary-value-overload-inference)                         | Added contextual lambda binary value return inference for C# delegate overload filtering/ranking, including string concatenation, numeric arithmetic, diagnostics, and `net48` CLI build coverage. |
| Q3       | Done   | Work ledger future area | Lambda indexer overload inference                  | [tasks-rollup.md#task-0274-lambda-indexer-overload-inference](tasks-rollup.md#task-0274-lambda-indexer-overload-inference)                                   | Added metadata-backed lambda indexer-expression return inference for C# delegate overload filtering/ranking, including imported C# indexer return metadata, diagnostics, and `net48` CLI build coverage. |
| Q3       | Done   | Work ledger future area | Lambda null-coalescing overload inference          | [tasks-rollup.md#task-0273-lambda-null-coalescing-overload-inference](tasks-rollup.md#task-0273-lambda-null-coalescing-overload-inference)                   | Added metadata-backed `??` lambda body return inference for C# delegate overload filtering/ranking, with checker diagnostics, resolver coverage, and `net48` CLI build smokes. |
| Q2       | Done   | Work ledger future area | Inferred lambda-valued export let                  | [tasks-rollup.md#task-0272-inferred-lambda-valued-export-let](tasks-rollup.md#task-0272-inferred-lambda-valued-export-let)                                     | Lowered unannotated lambda-valued top-level `let` exports and aliases as conservative `System.Func<object, TResult>` delegate values with source graph, import alias, docs, and net48 build coverage. |
| Q4       | Done   | User directive | VS Code LSP feedback                                | [tasks-rollup.md#task-0271-vs-code-lsp-feedback](tasks-rollup.md#task-0271-vs-code-lsp-feedback)                                                               | Advertised LSP open/change/close sync, cleared stale diagnostics on `didClose`, kept semantic diagnostics/hover/definition/completion/formatting covered by server and extension smokes. |
| Q5       | Done   | User directive | Monorepo folder structure                           | [tasks-rollup.md#task-0270-monorepo-folder-structure](tasks-rollup.md#task-0270-monorepo-folder-structure)                                                     | Split product, language, test, docs, agent, examples, and VS Code ownership into explicit top-level folders with README contracts and updated path references. |
| Q5       | Done   | User directive | Runnable example code explanations                 | [tasks-rollup.md#task-0269-runnable-example-code-explanations](tasks-rollup.md#task-0269-runnable-example-code-explanations)                                   | Added guided README explanations before every command, output, TypeSharp, C#, and XML code block in runnable example projects and strengthened docs/test contracts. |
| Q5       | Done   | User directive | Realistic runnable tysh examples                   | [tasks-rollup.md#task-0268-realistic-runnable-tysh-examples](tasks-rollup.md#task-0268-realistic-runnable-tysh-examples)                                       | Reworked runnable TypeSharp projects into invoice, public API, local C# billing interop, ASP.NET/WCF, worker, and null-safety scenarios with smoke-tested commands. |
| Q5       | Done   | User directive | C# docs parity detail                              | [tasks-rollup.md#task-0267-csharp-docs-parity-detail](tasks-rollup.md#task-0267-csharp-docs-parity-detail)                                                     | Added Microsoft Learn C#-style detailed reference pages for TypeSharp CLR type mapping, members, overloads, byref/delegates/events/extension methods, and diagnostics. |
| Q5       | Done   | User directive | Release artifacts workflow                         | [tasks-rollup.md#task-0266-release-artifacts-workflow](tasks-rollup.md#task-0266-release-artifacts-workflow)                                                   | Added a tag/manual GitHub Release workflow for CLI, `net48` runtime libraries, VSIX, release notes, and SHA-256 manifest assets.                         |
| Q5       | Done   | User directive | Docs writing guide                                  | [tasks-rollup.md#task-0265-docs-writing-guide](tasks-rollup.md#task-0265-docs-writing-guide)                                                                   | Adapted the Vue Docs Writing Guide into TypeSharp docs authoring rules, `tysh` example project guidance, emoji policy, and review checks.               |
| Q5       | Done   | User directive | Runtime artifact architecture docs                  | [tasks-rollup.md#task-0264-runtime-artifact-architecture-docs](tasks-rollup.md#task-0264-runtime-artifact-architecture-docs)                                   | Added Runtime Artifacts docs with Mermaid diagrams for tool/runtime boundaries, build sequence, references, and deployable `net48` artifacts.           |
| Q5       | Done   | User directive | Docs tysh syntax highlighting                       | [tasks-rollup.md#task-0263-docs-tysh-syntax-highlighting](tasks-rollup.md#task-0263-docs-tysh-syntax-highlighting)                                             | Reused the VS Code TextMate grammar in Starlight/Shiki and converted TypeSharp source examples from `text` to `tysh` fences.                            |
| Q4       | Done   | User directive | VS Code syntax highlighting extension install guide | [tasks-rollup.md#task-0262-vs-code-syntax-highlighting-extension-install-guide](tasks-rollup.md#task-0262-vs-code-syntax-highlighting-extension-install-guide) | Completed VS Code TextMate grammar/package coverage, local VSIX install instructions, and temporary Marketplace publishing guide.                       |
| Q5       | Done   | User directive | Docs TypeScript config conversion                   | [tasks-rollup.md#task-0261-docs-typescript-config-conversion](tasks-rollup.md#task-0261-docs-typescript-config-conversion)                                     | Converted the docs Astro config from JavaScript to TypeScript and added a contract check that docs-owned source has no JavaScript config/source files.  |
| Q5       | Done   | User directive | Docs dependency update                              | [tasks-rollup.md#task-0260-docs-dependency-update](tasks-rollup.md#task-0260-docs-dependency-update)                                                           | Updated `docs` package and lockfile to current npm registry latest tags for Astro, Starlight, and TypeScript; refreshed docs package contract coverage. |
| Q2       | Done   | User directive | Parallel execution optimization                     | [tasks-rollup.md#task-0259-parallel-execution-optimization](tasks-rollup.md#task-0259-parallel-execution-optimization)                                         | Parallelized source-file parse and semantic validation while preserving deterministic diagnostics; documented agent and compiler parallelism rules.     |
| Q0       | Done   | User directive | Codex skills configuration                          | [tasks-rollup.md#task-0258-codex-skills-configuration](tasks-rollup.md#task-0258-codex-skills-configuration)                                                   | Vendored project-useful Codex skills under `.codex/skills`, including TypeSharp .NET and language-engineering skills.                                   |
| Q0       | Done   | User directive | Docs/agent directory rename                         | [tasks-rollup.md#task-0257-docs-agent-directory-rename](tasks-rollup.md#task-0257-docs-agent-directory-rename)                                                 | Renamed the public docs site source to `docs/`, moved temporary agent work records to `agent/`, and updated ownership/workflow contracts.               |
| Q0       | Done   | User directive | Test suite quality audit                            | [tasks-rollup.md#task-0256-test-suite-quality-audit](tasks-rollup.md#task-0256-test-suite-quality-audit)                                                       | Audited repository tests, hardened weak fixture/smoke checks, verified compiler, VS Code, and docs commands.                                            |

Status values: `Requested`, `Ready`, `In Progress`, `Blocked`, `Done`, `Dropped`.

## Rules

- 매 반복 시작 시 이 파일을 다시 읽는다.
- active task가 있으면 최신 사용자 요청이 명시적으로 중단시키지 않는 한 계속 진행한다.
- active task가 없으면 `User Task Inbox`의 unchecked item을 먼저 `Agent Task Queue`로 승격한다.
- `User Task Inbox`가 비어 있으면 `Agent Task Queue`의 `Requested` 또는 `Ready` 항목을 고른다.
- 두 task 섹션이 모두 비어 있으면 [checklist.md](checklist.md)에서 다음 미완료 항목을 고른다.
- 한 세션을 넘길 작업은 `agent/NNNN-short-name.md` active packet을 만들고 `State`와 `Agent Task Queue`에 연결한다.
- 완료된 active packet은 [tasks-rollup.md](tasks-rollup.md)에 요약하고 packet 파일은 제거한다.
- 완료 시 `State`, `User Task Inbox`, `Agent Task Queue`, [traceability.md](traceability.md), docs Work Ledger 중 필요한 항목만 갱신한다.
