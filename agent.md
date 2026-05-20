# TypeSharp 에이전트 지침

문서 기준일: 2026-05-20

이 문서는 Codex가 `/goal`, Ralph mode, 또는 장기 작업 세션을 통해 TypeSharp 과제를 계속 수행할 때 읽어야 하는 운영 지침이다. 목표의 canonical 원문은 [Docs Core Goal](docs/src/content/docs/goal.md)에 있고, 장기 실행 계약은 [agent/agentic-execution.md](agent/agentic-execution.md)에 있다. 이 문서는 그 목표를 에이전트가 반복 실행 가능한 작업 규칙으로 바꾼다.

Task 0257 이후 `docs/`는 표준 언어/프로젝트 원장의 canonical 표면이고, `agent/`는 agentic goal work를 위한 임시 운영 표면이다. 현재 task 운영 구조는 [agent/tasks.md](agent/tasks.md)가 active task pointer와 다음 우선순위를 들고, [agent/tasks-rollup.md](agent/tasks-rollup.md)가 완료된 task history를 압축 보관하며, 진행 중인 긴 작업만 `agent/NNNN-short-name.md` active packet으로 남기는 방식이다. [Document Ownership](docs/src/content/docs/document-ownership.md)이 어떤 표준 문서를 `docs/` canonical로 둘지와 `agent/`에 어떤 temporary work 문서만 남길지를 정한다. [Project Ledger](docs/src/content/docs/project-ledger.md), [Work Ledger](docs/src/content/docs/work-ledger.md), [Agentic Workflow](docs/src/content/docs/agentic-workflow.md)는 원장성 문서를 웹페이지로 보여준다. Codex CLI goal, Ralph mode, 장기 실행 에이전트가 다음 작업을 고를 때의 운영 입력은 이 파일, [agent/agentic-execution.md](agent/agentic-execution.md), [agent/checklist.md](agent/checklist.md), [agent/tasks.md](agent/tasks.md), [agent/tasks-rollup.md](agent/tasks-rollup.md), [agent/traceability.md](agent/traceability.md), 그리고 docs의 Document Ownership/Project Ledger/Work Ledger/Agentic Workflow다.

## 에이전트의 최상위 목표

TypeSharp는 .NET Framework 4.8용 산출물을 만들고 그 산출물이 .NET Framework 4.8에서 실행될 수 있으면서, 최신 C#, F#, TypeScript에서 검증된 표현력, 안전성, 도구 친화성을 하나의 일관된 정적 타입 언어로 통합하는 새 언어다.

에이전트는 모든 작업에서 이 문장을 먼저 기억해야 한다.

## `/goal`에 넣을 목표 문장

```text
TypeSharp를 .NET Framework 4.8용 산출물을 만들고 실행할 수 있으면서 TypeScript처럼 암묵적이고 유연한 타입 기능, TypeScript식 모듈 기반 파일 구조, F#의 함수형 기능과 일관성, C#의 다양하고 유연한 편의 기능 및 기존 C#/.NET Framework 라이브러리 상호 운용성을 통합한 새 정적 타입 언어로 설계하고 구현한다. TypeScript, F#, C#의 실용 기능을 TypeSharp 문법으로 직접 포괄하거나 TypeSharp식 기능으로 대체할 수 있을 때까지 grammar coverage를 계속 확장한다. 목표 문서, 요구사항, 문법 사양, C# library interop 계약, 기능 매핑, 아키텍처, 실현 가능성 검토, 체크리스트를 기준으로 문서화, 설계 결정, 컴파일러/런타임 구현, 테스트, 검증을 반복하며 과제가 실제로 실행 가능한 상태가 될 때까지 계속 진행한다.
```

## Ralph/Goal mode 실행 문장

```text
TypeSharp 장기 작업을 시작할 때는 agent/agentic-execution.md의 부트스트랩 순서와 작업 선택 규칙을 따른다. 각 반복은 `agent/tasks.md`의 active task 또는 checklist/traceability의 다음 후보 하나를 선택하고, `docs/` canonical 표준 문서와 `agent/` temporary work 문서가 충돌하지 않게 문서, 사양, 코드, 테스트 중 필요한 산출물을 만든다. 완료 시 검증 결과와 남은 작업을 checklist.md, traceability.md, `agent/tasks-rollup.md`, 관련 docs canonical 문서에 반영하고, Done이 된 active task packet은 rollup에 압축한 뒤 제거한다. task가 Done이면 관련 변경을 git commit한 뒤 원격 브랜치로 push한다. active/user/queue/checklist가 모두 비었다고 판단되면 전체 goal이 끝났다고 보지 말고 공식 C#/F#/TypeScript/.NET reference를 다시 확인해 새 Q1 roadmap-refresh task를 만들고 계속 계획/실행한다.
```

## 작업 시작 루틴

새 세션이나 새 목표 실행이 시작되면 다음 순서로 읽는다.

1. [Docs Core Goal](docs/src/content/docs/goal.md)
2. [agent/agentic-execution.md](agent/agentic-execution.md)
3. [agent/tasks.md](agent/tasks.md)
4. `agent/tasks.md`가 가리키는 active task packet이 있으면 그 파일
5. [agent/tasks-rollup.md](agent/tasks-rollup.md)
6. [agent/checklist.md](agent/checklist.md)
7. [agent/traceability.md](agent/traceability.md)
8. [Docs Document Ownership](docs/src/content/docs/document-ownership.md)
9. [Docs Project Ledger](docs/src/content/docs/project-ledger.md)
10. [Docs Work Ledger](docs/src/content/docs/work-ledger.md)
11. [Docs Agentic Workflow](docs/src/content/docs/agentic-workflow.md)
12. 현재 작업과 직접 관련된 `docs/` canonical 문서 또는 `agent/` temporary work 문서

프로젝트-local Codex skill이 필요한 작업에서만 `.codex/skills/<skill>/SKILL.md`를 읽는다. 언어/compiler 작업은 `typesharp-language-engineering`, .NET/runtime/interop/build 작업은 `typesharp-dotnet`에 세부 체크를 위임한다.

표준 언어/프로젝트 문서를 바꾸는 작업이라면 [Document Ownership](docs/src/content/docs/document-ownership.md)의 target owner를 먼저 확인하고 docs canonical page를 갱신한다. task 상태, checklist/traceability, goal/agent 규칙처럼 agentic 운영 정보가 바뀌면 `agent/` temporary work 문서와 [Project Ledger](docs/src/content/docs/project-ledger.md), [Work Ledger](docs/src/content/docs/work-ledger.md), [Agentic Workflow](docs/src/content/docs/agentic-workflow.md)를 함께 갱신할 필요가 있는지 확인한다.

읽은 뒤에는 다음 질문에 답한 다음 움직인다.

- 지금 작업은 [Docs Core Goal](docs/src/content/docs/goal.md)의 한 문장 과제에 직접 기여하는가?
- 지금 작업은 Windows 10 기본 설정에 `dotnet`과 `node`가 설치된 상태만으로 수행 가능한가?
- 지금 작업은 VS Code와 CLI만으로 개발 가능한 TypeSharp 개발 루프를 진전시키는가?
- 지금 작업은 TypeScript식 유연성, TypeScript식 module graph, F#식 함수형 일관성, C#식 편의성 중 어느 축을 진전시키는가?
- 언어/compiler 변경이면 `typesharp-language-engineering`을 적용했는가?
- .NET/runtime/interop/build 변경이면 `typesharp-dotnet`을 적용했는가?
- 지금 작업은 체크리스트의 어떤 미완료 항목을 줄이는가?
- 장기 실행 작업이라면 [agent/agentic-execution.md](agent/agentic-execution.md)의 queue, task packet, Done 기준 중 무엇에 해당하는가?
- 최신 C#, F#, TypeScript 기능을 언급한다면 안정 기능과 preview 기능을 분리했는가?

## 반복 실행 규칙

에이전트는 과제를 한 번의 문서 작성으로 끝났다고 간주하지 않는다. 다음 루프를 계속 반복한다.

1. 목표 확인
   - [Docs Core Goal](docs/src/content/docs/goal.md)의 목표, 성공 조건, 비목표를 다시 확인한다.

2. 다음 미완료 항목 선택
   - [agent/agentic-execution.md](agent/agentic-execution.md)의 우선순위 큐를 적용한다.
   - [agent/checklist.md](agent/checklist.md)에서 아직 체크되지 않은 항목 중 가장 앞선 의존성을 고른다.
   - active task, user inbox, agent queue, checklist가 모두 비어 있으면 [Docs Project Policy](docs/src/content/docs/project-policy.md)의 official reference tracking 기준으로 C#/F#/TypeScript/.NET 최신 공식 문서를 다시 확인하고 새 Q1 roadmap-refresh task를 만들어 `agent/tasks.md`에 다음 latest-five 실행 후보를 채운다.

3. 근거 확인
   - 플랫폼 또는 최신 언어 버전 정보가 필요하면 [Docs Project Policy](docs/src/content/docs/project-policy.md)의 official reference tracking 기준과 공식 문서를 기준으로 갱신한다.

4. 설계 또는 구현
   - 문서가 부족하면 문서를 먼저 구체화한다.
   - 언어/compiler 작업은 `typesharp-language-engineering`이 지시하는 canonical docs와 fixture 기준을 따른다.
   - .NET/runtime/interop/build 작업은 `typesharp-dotnet`이 지시하는 target framework, ABI, lowering, verification 기준을 따른다.
   - 구현이 가능하면 코드, 테스트, 샘플을 만든다.
   - 설계 결정이 막히면 선택지, 장단점, 권장안을 문서화한다.

5. 검증
   - 가능한 테스트를 실행한다.
   - 테스트가 아직 없으면 최소한 문서 링크, 체크리스트, 추적성을 검증한다.

6. 상태 갱신
   - 완료된 항목은 체크리스트에 반영한다.
   - 새로 드러난 미해결 문제는 관련 문서의 열린 결정 또는 체크리스트에 추가한다.
   - task 상태, checklist/traceability, goal/agent 규칙처럼 원장성 정보가 바뀌면 docs의 Project Ledger, Work Ledger, Agentic Workflow 중 해당 웹 원장도 함께 맞춘다.
   - task가 `Done`이면 이번 task와 직접 관련된 변경만 git commit하고 현재 원격 브랜치로 push한다. unrelated worktree 변경은 함께 staging하지 않는다.

## 의사결정 우선순위

1. .NET Framework 4.8 및 기존 ASP.NET/WCF/worker host 호환성
2. VS Code와 CLI 개발 가능성
3. C#/.NET 상호 운용성
4. TypeScript식 암묵성과 유연성
5. TypeScript식 명시적 module graph
6. F#식 함수형 일관성
7. C#식 편의성과 .NET 친화성
8. 타입 안전성
9. 설명 가능한 lowering
10. 도구 친화성
11. 구현 단순성

표현력이 좋아도 .NET Framework에서 안정적으로 낮출 수 없으면 기본 기능으로 채택하지 않는다. preview 기능은 반드시 `Preview Watch` 또는 feature gate로 분리한다.

## 에이전트가 지켜야 할 금지선

- .NET 10/11 전용 런타임 기능을 TypeSharp 기본 요구사항으로 만들지 않는다.
- Python 관련 행위나 작업을 하지 않는다. Python 스크립트 작성/실행, `python`/`py`/`pip`/`venv`/Python 기반 도구 사용, Python 의존성 추가, Python 워크플로 문서화를 하지 않는다.
- Windows 10 기본 설정과 이미 설치된 `dotnet`, `node`만으로 가능한 작업 경로를 선택한다. 새 언어 런타임이나 별도 전역 도구 설치를 전제로 설계하거나 검증하지 않는다.
- C#, F#, TypeScript 기능을 이름만 보고 1:1로 복제하지 않는다.
- `net48` 호환성 검증 없이 dependency를 추가하지 않는다.
- 문서와 체크리스트를 갱신하지 않고 큰 설계 결정을 숨기지 않는다.
- preview 기능을 안정 목표처럼 작성하지 않는다.
- 빌드 중 임의 사용자 코드를 실행하는 확장 모델을 기본 설계로 두지 않는다.

## 장기 작업 모드

이 프로젝트는 에이전틱하게 오래 수행될 수 있도록 설계한다. 에이전트는 매번 "작업을 끝낸다"가 아니라 "목표에 더 가까워진 상태를 남긴다"는 관점으로 움직인다.

좋은 종료 상태:
- 무엇을 바꿨는지 문서 또는 코드에 남아 있다.
- 다음에 할 일이 체크리스트나 열린 결정에 남아 있다.
- 검증한 내용과 검증하지 못한 내용이 분리되어 있다.
- 장기 작업이면 task packet 또는 인계 기준에 맞게 남은 일을 설명할 수 있다.
- [Docs Core Goal](docs/src/content/docs/goal.md)의 성공 조건 중 어느 항목을 진전시켰는지 설명할 수 있다.
- task가 끝났다면 관련 변경이 git commit되고 원격 브랜치로 push되어 있다.

## 현재 우선순위

현재 우선순위는 이 파일에 고정 목록으로 복사하지 않는다. 오래 도는 goal 세션은 다음 순서로 최신 상태를 판단한다.

1. [agent/tasks.md](agent/tasks.md)를 매 반복 다시 읽고 `State`, `User Task Inbox`, `Agent Task Queue`를 확인한다.
2. `In Progress` active task packet이 있으면, 사용자 요청과 충돌하지 않는 한 그 작업을 먼저 이어간다.
3. 활성 task가 없으면 [agent/tasks.md](agent/tasks.md)의 `User Task Inbox` unchecked item을 먼저 `Agent Task Queue`로 승격한다.
4. user inbox가 비어 있으면 `Agent Task Queue`의 가장 높은 우선순위 `Requested` 또는 `Ready` 항목을 고른다.
5. 두 task 섹션이 모두 비어 있으면 [agent/checklist.md](agent/checklist.md)의 미완료 항목을 [agent/agentic-execution.md](agent/agentic-execution.md)의 Q0-Q5 규칙으로 고른다.
6. checklist도 비어 있으면 전체 goal 완료가 아니라 roadmap refresh 상태로 간주한다. 공식 C#/F#/TypeScript/.NET reference를 갱신하고, [docs/src/content/docs/feature-status.md](docs/src/content/docs/feature-status.md), [docs/src/content/docs/work-ledger.md](docs/src/content/docs/work-ledger.md), [agent/tasks-rollup.md](agent/tasks-rollup.md)를 비교해 새 Q1 planning task와 그 뒤 실행 후보를 `agent/tasks.md`에 만든다.
7. 완료된 큰 주제는 [agent/tasks-rollup.md](agent/tasks-rollup.md)를 먼저 읽고, 필요한 경우에만 현재 active packet을 연다. 완료된 개별 task packet은 `agent/`에 계속 쌓지 않는다.
8. task 0257 이후 `docs/`는 표준 언어/프로젝트 원장의 canonical 표면이 되며, `agent/`는 agentic temporary work와 handoff 문서만 남긴다. 구현 우선순위와 활성 작업 상태는 `agent/tasks.md`, active task packet, `agent/tasks-rollup.md`가 결정하고, Docs Work Ledger는 그 상태를 웹에서 보여준다.

이렇게 해야 docs navigation이 바뀌어도 Codex CLI goal의 실제 작업 선택 기준은 흔들리지 않는다.

