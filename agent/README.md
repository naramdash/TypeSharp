# TypeSharp Agent Work Docs

문서 기준일: 2026-05-20

`docs/`가 표준 언어/프로젝트 원장이다. 이 `agent/` 디렉터리는 Codex goal, Ralph mode, 장기 실행 인계, checklist, traceability 같은 agent work 운영 문서만 둔다.

사용자용 또는 장기 유지되는 표준 문서는 [../docs/src/content/docs](../docs/src/content/docs)에 있다. 어떤 표면이 원장인지 확인해야 하면 [Document Ownership](../docs/src/content/docs/document-ownership.md), [Project Ledger](../docs/src/content/docs/project-ledger.md), [Work Ledger](../docs/src/content/docs/work-ledger.md), [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md)를 본다.

## 남아 있는 파일

1. [../agent.md](../agent.md)
   - Codex goal, Ralph mode, 장기 작업 세션의 루트 운영 지침이다.

2. [agentic-execution.md](agentic-execution.md)
   - 장기 실행 모드에서 작업을 고르고 검증하고 인계하는 실행 계약이다.

3. [tasks.md](tasks.md)
   - State, User Task Inbox, Agent Task Queue, 완료 범위, 압축 원장 위치를 알려주는 작업 인덱스다.

4. [tasks-rollup.md](tasks-rollup.md)
   - 완료된 agent work 이력을 압축한 고정 rollup 파일이다.

5. [checklist.md](checklist.md)
   - 남은 구현/검증 작업을 고르는 운영 체크리스트다.

6. [traceability.md](traceability.md)
   - 목표, 요구사항, 기능, 체크리스트, 증거를 연결하는 agent-facing 추적성 문서다.

7. [codex-skills.md](codex-skills.md)
   - TypeSharp goal 작업에 설치/권장된 Codex skill과 사용 규칙을 기록한다.

8. [progress.md](progress.md)
   - active task packet, `tasks-rollup.md`, handoff 기록 규칙이다.

9. [adr.md](adr.md)
   - 큰 설계 결정을 ADR로 남길 때 쓰는 임시 운영 템플릿이다.

## 표준 문서 원장

- 목표: [Core Goal](../docs/src/content/docs/goal.md)
- 요구사항: [Project Requirements](../docs/src/content/docs/requirements.md)
- 기능 상태: [Feature Status](../docs/src/content/docs/feature-status.md)
- 문법: [Grammar](../docs/src/content/docs/grammar.md), [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- 타입 시스템: [Type System](../docs/src/content/docs/type-system.md)
- lowering: [Lowering](../docs/src/content/docs/lowering.md)
- C#/.NET 상호 운용: [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- CLI/API: [CLI](../docs/src/content/docs/cli.md), [API And CLI Reference](../docs/src/content/docs/api.md)
- diagnostics: [Diagnostics](../docs/src/content/docs/diagnostics.md)
- 프로젝트 정책: [Project Policy](../docs/src/content/docs/project-policy.md)

## 유지 규칙

- 표준 언어, 프로젝트 정책, 사용자 가이드는 `docs/src/content/docs/`에 먼저 반영한다.
- `agent/`에는 active task packet, `tasks.md`, `tasks-rollup.md`, handoff, checklist, traceability, and agent execution control만 남긴다.
- 새 bridge stub를 만들지 않는다. 예전 경로 호환이 필요하면 `docs/` 원장 문서에서 former source로 설명한다.
- agent work 상태가 바뀌면 이 디렉터리의 운영 문서와 docs의 Work Ledger/Agentic Workflow를 함께 맞춘다.
