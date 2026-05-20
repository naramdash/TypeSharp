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
| Completed range    | 0001-0291                          |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md) |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. agent 실행 중에도 언제든 이 섹션을 수정할 수 있다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [x] agent/tasks.md 파일의 Agent Task Queue 섹션은 최근 5개만 남기도록 변경, User Task Inbox는 유저가 언제나 변경할 수 있음을 명시

## Agent Task Queue

최근 5개 행만 유지한다. 이전 완료 이력은 [tasks-rollup.md](tasks-rollup.md)를 본다.

| Priority | Status | Source                  | Task                                                | Packet                                                                                                                                                         | Notes                                                                                                                                                                                                    |
| -------- | ------ | ----------------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Q0       | Done   | User directive          | Task queue retention policy                        | [tasks-rollup.md#task-0291-task-queue-retention-policy](tasks-rollup.md#task-0291-task-queue-retention-policy)                                                 | Kept the user inbox user-editable at any time, limited the visible agent queue to the latest five rows, and kept older done work in the compressed rollup.                                               |
| Q3       | Done   | Work ledger future area | Parenthesized overload argument unwrapping          | [tasks-rollup.md#task-0290-parenthesized-overload-argument-unwrapping](tasks-rollup.md#task-0290-parenthesized-overload-argument-unwrapping)                   | Unwrapped parenthesized imported C# overload arguments for null metadata specificity and lambda delegate filtering/ranking while preserving generated grouping.                                          |
| Q3       | Done   | Work ledger future area | Parenthesized indexer argument validation           | [tasks-rollup.md#task-0289-parenthesized-indexer-argument-validation](tasks-rollup.md#task-0289-parenthesized-indexer-argument-validation)                     | Unwrapped parenthesized imported C# indexer arguments for metadata validation so mismatches report `TS2411` before emission, with positive and no-emission CLI coverage.                                 |
| Q3       | Done   | Work ledger future area | Params collection expression array overload         | [tasks-rollup.md#task-0288-params-collection-expression-array-overload](tasks-rollup.md#task-0288-params-collection-expression-array-overload)                 | Ranked a single homogeneous collection expression as the full imported C# `params T[]` array argument before expanded element fallback, with resolver/checker/`net48` CLI build coverage.                |
| Q3       | Done   | Work ledger future area | Collection generic array inference                  | [tasks-rollup.md#task-0287-collection-generic-array-inference](tasks-rollup.md#task-0287-collection-generic-array-inference)                                   | Added homogeneous collection expression inference for imported C# `T[]` generic method parameters, `TS2417` diagnostics before emission, and `net48` CLI build coverage.                                 |

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
