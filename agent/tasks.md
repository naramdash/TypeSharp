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
| Active task packet | [0312-enum-match-exhaustiveness-slice.md](0312-enum-match-exhaustiveness-slice.md) |
| Active summary     | Implement TypeSharp enum match exhaustiveness over owned enum declarations. |
| Completed range    | 0001-0311                         |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md) |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. agent 실행 중에도 언제든 이 섹션을 수정할 수 있다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [x] agent/tasks.md 파일의 Agent Task Queue 섹션은 최근 5개만 남기도록 변경, User Task Inbox는 유저가 언제나 변경할 수 있음을 명시
- [x] 모태가 된 언어들의 모든 기능, 환경, 이코시스템을 분석해서 이 언어에 적절한 계획을 세우고 tasks.md에 반영하기. goal 모드가 끝났다고 생각되었을때에는 이러한 작업을 반복하여 멈추지 않고 계속 계획을 세우고 실행하도록 agent.md 문서에 반영할것

## Agent Task Queue

최근 5개 행만 유지한다. 이전 완료 이력은 [tasks-rollup.md](tasks-rollup.md)를 본다.

| Priority | Status      | Source                    | Task                                     | Packet                                                                   | Notes                                                                                                                                         |
| -------- | ----------- | ------------------------- | ---------------------------------------- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- |
| Q2       | In Progress | Task 0311 roadmap refresh | 0312 Enum match exhaustiveness slice | [0312-enum-match-exhaustiveness-slice.md](0312-enum-match-exhaustiveness-slice.md) | Implement TypeSharp-owned enum match exhaustiveness now that enum declaration groundwork exists. |
| Q1       | Done        | Empty queue roadmap-refresh rule | 0311 Roadmap refresh after enum declarations | [tasks-rollup.md#task-0311-roadmap-refresh-after-enum-declarations](tasks-rollup.md#task-0311-roadmap-refresh-after-enum-declarations) | Rechecked official source signals after enum declarations and selected enum match exhaustiveness as the next bounded implementation slice. |
| Q2       | Done        | Task 0309 roadmap refresh | 0310 Enum declaration implementation slice | [tasks-rollup.md#task-0310-enum-declaration-implementation-slice](tasks-rollup.md#task-0310-enum-declaration-implementation-slice) | Implemented TypeSharp-owned simple enum declarations and C# 7.3 lowering before enum match exhaustiveness. |
| Q1       | Done        | Empty queue roadmap-refresh rule | 0309 Roadmap refresh after literal match exhaustiveness | [tasks-rollup.md#task-0309-roadmap-refresh-after-literal-match-exhaustiveness](tasks-rollup.md#task-0309-roadmap-refresh-after-literal-match-exhaustiveness) | Rechecked official source signals after literal match exhaustiveness, confirmed the baseline, and selected enum declaration groundwork as the next bounded implementation slice. |
| Q2       | Done        | Task 0307 roadmap refresh | 0308 Literal match exhaustiveness slice | [tasks-rollup.md#task-0308-literal-match-exhaustiveness-slice](tasks-rollup.md#task-0308-literal-match-exhaustiveness-slice) | Implemented literal pattern parsing plus bool and local literal-union match exhaustiveness/lowering; enum exhaustiveness and richer pattern algebra remain follow-ups. |

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
