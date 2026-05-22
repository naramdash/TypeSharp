# Agentic Tasks

문서 기준일: 2026-05-23

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
| Active task packet | [0457-local-multiplicative-compound-assignment-checked-overflow-policy.md](0457-local-multiplicative-compound-assignment-checked-overflow-policy.md) |
| Active summary     | Implement local multiplicative compound assignment checked/unchecked overflow policy. |
| Completed range    | 0001-0400, 0402-0456                                                                  |
| Completed rollup   | [tasks-rollup.md](tasks-rollup.md)                                                                                       |

## User Task Inbox

사용자는 여기에만 새 task를 추가한다. agent 실행 중에도 언제든 이 섹션을 수정할 수 있다. 형식은 아래처럼 checkbox bullet만 쓴다.

```md
- [ ] 새로 시킬 일
```

Agent는 사용자가 추가한 항목을 삭제하지 않는다. 처리 완료 시에만 `- [x]`로 바꾸고, 필요한 설명은 Agent Task Queue 또는 task packet에 남긴다.

<!-- user tasks below -->

- [x] test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj 테스트 시간을 축소하기 위한 계획을 세우고 이 프로젝트의 목적에 부합하는지 따져서 구체화하기. 구체화한 계획에 따라 test 구성을 리팩토링할 것.
- [ ] 현재 GitHub Actions 에 의해 regression.yml 을 수행할 때, Run shard runners in parallel 단계에서 `FAIL VS Code extension live smoke runs against bundled language server: An error occurred trying to start process 'npm' with working directory 'D:\a\TypeSharp\TypeSharp\vscode\typesharp'. The system cannot find the file specified.` 에러가 발생하고 있음.

## Agent Task Queue

최근 5개 행만 유지한다. 이전 완료 이력은 [tasks-rollup.md](tasks-rollup.md)를 본다.

| Priority | Status      | Source                           | Task                                                            | Packet                                                                                                                                                                       | Notes                                                                                                                                                                                   |
| -------- | ----------- | -------------------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Q1       | In Progress | Task 0456 roadmap refresh after imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment policy | 0457 Local multiplicative compound assignment checked/unchecked overflow policy | [0457-local-multiplicative-compound-assignment-checked-overflow-policy.md](0457-local-multiplicative-compound-assignment-checked-overflow-policy.md) | Implement the local `let mut` `*=`, `/=`, and `%=` checked/unchecked overflow policy over the existing `checked(...)`/`unchecked(...)` surface, preserving generated package-free `net48`, C# 7.3 lowering, the 572-test package-shard baseline, and Task 0401's blocked status. |
| Q1       | Done        | Task 0455 imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment policy | 0456 Roadmap refresh after imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment policy | [tasks-rollup.md#task-0456-roadmap-refresh-after-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy](tasks-rollup.md#task-0456-roadmap-refresh-after-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy) | Rechecked official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals; preserved generated package-free `net48`/C# 7.3 and the pinned `net10.0` MSTest.Sdk/MTP package-shard bridge with a 572-test minimum; corrected user-defined compound assignment to C# 14 stable backlog; kept Task 0401 blocked; and selected Task 0457. |
| Q1       | Done        | Task 0454 roadmap refresh after imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy | 0455 Imported C# null-conditional indexer floating-point and decimal multiplicative compound assignment policy | [tasks-rollup.md#task-0455-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy](tasks-rollup.md#task-0455-imported-csharp-null-conditional-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy) | Extended imported C# instance indexer null-conditional `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` targets to bounded known non-null integral/floating-point/decimal operands, preserving skipped index/RHS evaluation, C# 7.3 guard lowering, generated package-free `net48`, and existing catalog/package-shard baselines. |
| Q1       | Done        | Task 0453 imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy | 0454 Roadmap refresh after imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy | [tasks-rollup.md#task-0454-roadmap-refresh-after-imported-csharp-null-conditional-member-floating-point-and-decimal-multiplicative-compound-assignment-policy](tasks-rollup.md#task-0454-roadmap-refresh-after-imported-csharp-null-conditional-member-floating-point-and-decimal-multiplicative-compound-assignment-policy) | Rechecked official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals; preserved generated package-free `net48`/C# 7.3 and the pinned `net10.0` MSTest.Sdk/MTP package-shard bridge with a 572-test minimum; kept Task 0401 blocked; and selected Task 0455. |
| Q1       | Blocked     | User Task Inbox | 0401 GitHub Actions regression npm missing in VS Code live smoke       | TBD                                                                                                                                                                        | Investigation is complete for run 26260793703 and earlier failures: `Setup Node` succeeds, but the package-free shard runner fails when the C# VS Code live smoke starts `npm` with `UseShellExecute=false`; likely fix is a Windows `cmd.exe /d /s /c npm ...` helper, but `gh-fix-ci` requires explicit user approval before implementation.                                           |

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
