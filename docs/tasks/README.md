# Agentic Task Packets

문서 기준일: 2026-05-20

이 폴더는 Codex `/goal`, Ralph mode, 장기 실행 에이전트가 이어받을 수 있는 작업 원장을 둔다. 완료된 개별 task packet은 누적 보관하지 않고 rollup으로 압축한다.

## 현재 상태

| 항목 | 상태 |
| --- | --- |
| 활성 task packet | None |
| 다음 최우선 task | Not selected |
| 완료 범위 | 0001-0252 |
| 압축 원장 | [0001-0252-task-ledger-rollup.md](0001-0252-task-ledger-rollup.md) |

## 사용 규칙

- 한 세션 안에서 끝나지 않을 새 작업은 임시 task packet으로 만들 수 있다.
- task packet은 `Status`, `Queue`, `Start Time`, `End Time`, objective, scope, acceptance criteria, verification, handoff를 포함한다.
- task가 `Done`이 되면 개별 파일을 계속 쌓지 말고 이 rollup 또는 후속 rollup에 요약한 뒤 제거한다.
- 다음 작업 선택은 [../agentic-execution.md](../agentic-execution.md), [../checklist.md](../checklist.md), [../traceability.md](../traceability.md)를 기준으로 한다.
- 웹에서 볼 때는 docs-site의 [Project Ledger](../../docs-site/src/content/docs/project-ledger.md), [Work Ledger](../../docs-site/src/content/docs/work-ledger.md), [Agentic Workflow](../../docs-site/src/content/docs/agentic-workflow.md)가 이 원장을 설명한다.

## 읽는 순서

1. 이 README에서 활성 task가 있는지 확인한다.
2. 완료된 작업 이력은 [0001-0252-task-ledger-rollup.md](0001-0252-task-ledger-rollup.md)를 읽는다.
3. 새 작업을 고를 때는 checklist와 traceability를 보고, 필요한 사양 문서로 바로 이동한다.
4. 새 작업이 완료되면 rollup에 요약하고 개별 packet은 남기지 않는다.
