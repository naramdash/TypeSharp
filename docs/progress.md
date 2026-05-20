# Progress Log Policy

문서 기준일: 2026-05-19

이 문서는 Ralph mode, Goal mode, Codex `/goal`처럼 여러 세션에 걸쳐 진행되는 TypeSharp 작업의 진행 기록 정책을 정의한다. 목표는 채팅 기록 없이도 현재 상태, 검증 근거, 다음 작업을 복원할 수 있게 하는 것이다.

## 원칙

- 진행 기록은 사용 가능한 산출물이어야 한다. 대화 요약이나 임시 메모만으로 완료 상태를 증명하지 않는다.
- 실제 checklist 항목을 줄이지 않는 기록용 문서는 만들지 않는다.
- 작업은 가능한 한 [tasks.md](tasks.md)의 task packet으로 기록한다.
- 큰 주제가 완료되면 관련 task packet을 rollup 문서로 압축해 `docs/`가 읽을 수 있는 규모를 유지한다.
- 각 task packet과 rollup은 검증 명령, 결과, 남은 범위, out-of-scope를 보존해야 한다.
- 완료 후에는 관련 코드/문서/테스트 변경과 task 기록을 같은 커밋 또는 명확히 연결된 커밋에 남긴다.
- task가 `Done`이 되면 같은 작업 흐름 안에서 해당 변경을 커밋하고 원격 브랜치로 push한다.

## 기록 단위

| 단위 | 사용 시점 | 위치 |
| --- | --- | --- |
| Task packet | 한 반복에서 checklist 항목 하나를 줄일 때 | `docs/NNNN-topic.md` |
| Task rollup | 같은 큰 주제의 여러 task packet이 완료되어 압축할 때 | `docs/NNNN-NNNN-topic.md` |
| ADR | 목표, architecture, public ABI, backend, runtime, tool 계약에 영향을 주는 큰 결정이 있을 때 | `docs/NNNN-adr-title.md` |
| Traceability row | 완료 근거가 목표/요구사항/체크리스트와 연결될 때 | `docs/traceability.md` |
| Checklist update | 완료 근거가 실제 파일, fixture, test, command로 확인될 때 | `docs/checklist.md` |

## Task Packet 규칙

새 task packet은 다음을 반드시 포함한다.

- `Status`: `Planned`, `In Progress`, `Blocked`, `Done`
- `Queue`: `Q0` through `Q5`
- `Start Time`: 현재 컴퓨터 기준 `yyyy-MM-dd HH:mm:ss zzz`
- `End Time`: 완료 시 현재 컴퓨터 기준 `yyyy-MM-dd HH:mm:ss zzz`
- `Objective`: 어떤 goal/checklist 항목을 줄이는지
- `Scope`: in/out 범위
- `Acceptance Criteria`: 완료 판단 기준
- `Verification`: 실제 실행한 명령과 결과
- `Handoff`: 완료, 남은 일, blocker

규칙:
- 작업 시작 전에는 `End Time: TBD`로 둔다.
- 완료 직전에 `End Time`을 초 단위까지 채운다.
- verification은 "테스트 통과"만 쓰지 않고 어떤 명령이 어떤 범위를 검증했는지 적는다.
- 실패한 검증이 있었다면 최종 통과 결과와 함께 실패 원인을 간단히 남긴다.
- code/test/doc 변경이 같은 주제라면 같은 task packet에 묶는다.
- 같은 주제의 새 packet을 중복 생성하지 말고 기존 packet을 재개하거나 rollup에 refresh section을 추가한다.

## Rollup 규칙

Rollup은 문서 수를 줄이기 위한 삭제가 아니라 정보 압축이다.

Rollup 대상:
- 같은 language feature, backend feature, interop feature, tooling feature에 속한 task packet 묶음
- 모두 `Done`인 task packet 묶음
- 각 packet의 검증 명령과 남은 작업이 보존 가능한 묶음

Rollup에는 다음을 보존한다.
- 하위 task 번호와 목적
- 구현된 파일/기능 범위
- 검증 명령과 결과
- public ABI 또는 `net48` 호환성 영향
- 남은 out-of-scope와 다음 후보

Rollup 후에는 `docs/tasks.md`가 rollup 문서를 가리키도록 갱신하고, rollup 커밋을 별도로 남긴다.

## Commit 규칙

- 구현/테스트/문서/task packet 변경은 의미 단위로 커밋한다.
- 하나의 task가 완료될 때마다 해당 task의 변경을 커밋하고 `git push`까지 수행한다.
- task rollup 또는 문서 압축은 별도 커밋으로 남긴다.
- 커밋 전에는 가능한 검증 명령과 `git diff --check`를 실행한다.
- 검증하지 못한 명령은 task packet의 `Remaining` 또는 final summary에 남긴다.
- push가 네트워크, 인증, 권한 문제로 실패하면 task packet이나 final summary에 실패 원인과 마지막 로컬 커밋 hash를 남긴다.

## 인계 규칙

세션 인계 시 다음 정보를 task packet 또는 final summary에 남긴다.

- 마지막 커밋
- 변경한 주요 파일
- 실행한 검증 명령
- 실패했거나 실행하지 못한 검증
- 다음 checklist 후보
- 목표/기준선과 충돌할 수 있는 위험

대화 final summary는 짧게 유지하되, 세부 근거는 문서에 남긴다.
