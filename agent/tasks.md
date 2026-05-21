# Agentic Tasks

문서 기준일: 2026-05-22

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
| Active task packet | None                                                                                                                     |
| Active summary     | None                                                                                                                     |
| Completed range    | 0001-0399                                                                         |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md)                                                                                       |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. agent 실행 중에도 언제든 이 섹션을 수정할 수 있다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [x] test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj 테스트 시간을 축소하기 위한 계획을 세우고 이 프로젝트의 목적에 부합하는지 따져서 구체화하기. 구체화한 계획에 따라 test 구성을 리팩토링할 것.

## Agent Task Queue

최근 5개 행만 유지한다. 이전 완료 이력은 [tasks-rollup.md](tasks-rollup.md)를 본다.

| Priority | Status      | Source                           | Task                                                            | Packet                                                                                                                                                                       | Notes                                                                                                                                                                                   |
| -------- | ----------- | -------------------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Q2       | Ready       | Task 0399 roadmap refresh after null-conditional assignment imported member targets | 0400 Null-conditional assignment imported indexer targets       | TBD                                                                                                                                                                        | Implement the C# 14-inspired `?[]` counterpart to the completed `?.` member slice: parse/check/lower metadata-backed imported C# instance indexer `receiver?[index] = value` with skipped right-side evaluation on null receivers, single receiver/index evaluation, and C# 7.3-compatible generated `net48` source; keep compound assignment, reads, events, static targets, TypeSharp-owned targets, and package/runtime baseline changes out.                                           |
| Q1       | Done        | Task 0398 null-conditional assignment imported member targets | 0399 Roadmap refresh after null-conditional assignment imported member targets       | [tasks-rollup.md#task-0399-roadmap-refresh-after-null-conditional-assignment-imported-member-targets](tasks-rollup.md#task-0399-roadmap-refresh-after-null-conditional-assignment-imported-member-targets)                                                                                                                                                                        | Rechecked official C#/F#/TypeScript/.NET Framework/NuGet/.NET testing/MSTest SDK/xUnit.net/VS Code/GitHub Actions signals after `receiver?.Member = value` landed, confirmed no `net48`/C# 7.3/package/test-host baseline change, and selected imported C# instance indexer null-conditional assignment next.                                           |
| Q2       | Done        | Task 0397 roadmap refresh after logical unsigned shift assignment imported indexer targets | 0398 Null-conditional assignment imported member targets       | [tasks-rollup.md#task-0398-null-conditional-assignment-imported-member-targets](tasks-rollup.md#task-0398-null-conditional-assignment-imported-member-targets)                                                     | Added `?.` parser coverage plus metadata-backed imported C# instance field/property `receiver?.Member = value` checker/backend lowering with C# 7.3 `System.Func` null guards, skipped RHS evaluation on null receivers, generated `net48` C# consumer evidence, and shared catalog count 532.                                           |
| Q1       | Done        | Task 0396 logical unsigned shift assignment imported indexer targets | 0397 Roadmap refresh after logical unsigned shift assignment imported indexer targets       | [tasks-rollup.md#task-0397-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-indexer-targets](tasks-rollup.md#task-0397-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-indexer-targets)                                                     | Rechecked official language/platform/package/test/editor/CI signals, reaffirmed the package-free generated `net48` baseline and existing `net10.0` MSTest SDK/MTP package shards, and selected null-conditional assignment next.                                           |
| Q2       | Done        | Task 0395 roadmap refresh after logical unsigned shift assignment imported member targets | 0396 Logical unsigned shift assignment imported indexer targets       | [tasks-rollup.md#task-0396-logical-unsigned-shift-assignment-imported-indexer-targets](tasks-rollup.md#task-0396-logical-unsigned-shift-assignment-imported-indexer-targets)                                                     | Added bounded metadata-backed imported C# instance indexer `>>>=` checker/backend lowering with single-evaluation receiver/index-argument evidence and shared catalog count 530.                                           |

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
