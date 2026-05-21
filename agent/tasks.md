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

| Field              | Value                                                                                                                    |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| Active task packet | [0367-roadmap-refresh-after-params-parameter-declaration.md](0367-roadmap-refresh-after-params-parameter-declaration.md) |
| Active summary     | 0367 Roadmap refresh after params parameter declaration                                                                  |
| Completed range    | 0001-0366                                                                                                                |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md)                                                                                       |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. agent 실행 중에도 언제든 이 섹션을 수정할 수 있다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [ ] test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj 테스트 시간을 축소하기 위한 계획을 세우고 이 프로젝트의 목적에 부합하는지 따져서 구체화하기. 구체화한 계획에 따라 test 구성을 리팩토링할 것.

## Agent Task Queue

최근 5개 행만 유지한다. 이전 완료 이력은 [tasks-rollup.md](tasks-rollup.md)를 본다.

| Priority | Status      | Source                           | Task                                                            | Packet                                                                                                                                                                       | Notes                                                                                                                                                                                   |
| -------- | ----------- | -------------------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Q1       | In Progress | Empty queue roadmap-refresh rule | 0367 Roadmap refresh after params parameter declaration         | [0367-roadmap-refresh-after-params-parameter-declaration.md](0367-roadmap-refresh-after-params-parameter-declaration.md)                                                     | Recheck official source signals after TypeSharp-owned final-array `params` parameter declarations and select the next bounded implementation slice.                                     |
| Q2       | Done        | Task 0365 roadmap refresh        | 0366 Params parameter declaration slice                         | [tasks-rollup.md#task-0366-params-parameter-declaration-slice](tasks-rollup.md#task-0366-params-parameter-declaration-slice)                                                 | Implemented final-array `params` declarations for TypeSharp-declared functions, direct exact/expanded calls, pipeline validation, generic tail inference, and C# `params T[]` lowering. |
| Q1       | Done        | Empty queue roadmap-refresh rule | 0365 Roadmap refresh after direct composition value inference   | [tasks-rollup.md#task-0365-roadmap-refresh-after-direct-composition-value-inference](tasks-rollup.md#task-0365-roadmap-refresh-after-direct-composition-value-inference)     | Rechecked official source signals after private direct composition value inference, confirmed the baseline, and selected bounded TypeSharp-owned `params` parameter declarations.       |
| Q2       | Done        | Task 0363 roadmap refresh        | 0364 Direct composition value inference slice                   | [tasks-rollup.md#task-0364-direct-composition-value-inference-slice](tasks-rollup.md#task-0364-direct-composition-value-inference-slice)                                     | Inferred concrete delegates for unannotated non-exported direct named-function composition values with fully known signatures, while keeping public ABI inference closed.               |
| Q1       | Done        | Empty queue roadmap-refresh rule | 0363 Roadmap refresh after composition annotation compatibility | [tasks-rollup.md#task-0363-roadmap-refresh-after-composition-annotation-compatibility](tasks-rollup.md#task-0363-roadmap-refresh-after-composition-annotation-compatibility) | Rechecked official source signals after explicit composition annotation compatibility, confirmed the baseline, and selected bounded private direct composition value inference.         |

Status values: `Requested`, `Ready`, `In Progress`, `Blocked`, `Done`, `Dropped`.

## Rules

- 매 반복 시작 시 이 파일을 다시 읽는다.
- 사용자는 `User Task Inbox`를 agent 실행 중에도 언제든 수정할 수 있다.
- active task가 있으면 최신 사용자 요청이 명시적으로 중단시키지 않는 한 계속 진행한다.
- active task가 없으면 `User Task Inbox`의 unchecked item을 먼저 `Agent Task Queue`로 승격한다.
- `User Task Inbox`가 비어 있으면 `Agent Task Queue`의 `Requested` 또는 `Ready` 항목을 고른다.
- 두 task 섹션이 모두 비어 있으면 [checklist.md](checklist.md)에서 다음 미완료 항목을 고른다.
- `Agent Task Queue`는 최신 5개 행만 유지하고, 오래된 완료 이력은 [tasks-rollup.md](tasks-rollup.md)에만 둔다.
- 한 세션을 넘길 작업은 `agent/NNNN-short-name.md` active packet을 만들고 `State`와 `Agent Task Queue`에 연결한다.
- 완료된 active packet은 [tasks-rollup.md](tasks-rollup.md)에 요약하고 packet 파일은 제거한다.
- 완료 시 `State`, `User Task Inbox`, `Agent Task Queue`, [traceability.md](traceability.md), docs Work Ledger 중 필요한 항목만 갱신한다.
