# Agentic Execution Contract

문서 기준일: 2026-05-18

이 문서는 Ralph mode, Goal mode, Codex `/goal`처럼 장기 작업을 이어가는 에이전트가 TypeSharp 과제를 안정적으로 수행하기 위해 필요한 실행 계약을 정의한다. 특정 도구의 내부 구현에 의존하지 않고, 어떤 장기 실행 모드에서도 같은 목표, 같은 우선순위, 같은 완료 기준으로 움직이게 만드는 것이 목적이다.

## 목적

장기 실행 에이전트는 다음 문제를 해결해야 한다.

- 목표를 잊지 않는다.
- 다음 작업을 임의로 고르지 않는다.
- 문서, 설계, 구현, 테스트가 서로 어긋나지 않게 한다.
- 세션이 끊겨도 이어받을 수 있는 상태를 남긴다.
- 실현 불가능하거나 과도한 기능을 MVP에 섞지 않는다.
- TypeSharp가 실제로 `net48` 산출물을 만들 수 있는 방향으로 계속 전진한다.

## 모드별 기대 입력

| 모드 | 필요한 입력 | TypeSharp에서 제공하는 문서 |
| --- | --- | --- |
| Goal mode | 장기 목표 문장, 금지선, 반복 루프, 완료 기준 | [../agent.md](../agent.md), [goal.md](goal.md), 이 문서 |
| Ralph mode | 작업 큐, 상태 요약, 다음 행동, 검증 방식, 인계 포맷 | 이 문서, [checklist.md](checklist.md), [traceability.md](traceability.md) |
| 일반 Codex 세션 | 현재 요청, 관련 문서, 변경 범위, 검증 명령 | [README.md](README.md), [agent.md](../agent.md), 관련 사양 문서 |

## 부트스트랩 순서

장기 실행 모드는 새 반복을 시작할 때 아래 순서로 읽는다.

1. [../agent.md](../agent.md)
2. [goal.md](goal.md)
3. [agentic-execution.md](agentic-execution.md)
4. [progress.md](progress.md)
5. [checklist.md](checklist.md)
6. [traceability.md](traceability.md)
7. [feasibility.md](feasibility.md)
8. [architecture.md](architecture.md)
9. 현재 작업과 직접 관련된 문서

이 순서는 세 가지 질문에 답하기 위한 것이다.

- 목표는 무엇인가?
- 지금 가장 앞선 의존성은 무엇인가?
- 완료를 어떻게 검증할 것인가?

## 변하지 않는 기준선

에이전트는 모든 작업에서 아래 기준을 유지한다.

| 기준 | 결정 |
| --- | --- |
| 실행 타깃 | 생성 산출물과 TypeSharp runtime library는 `net48` 필수 |
| Host | compiler, CLI, LSP host는 현대 .NET LTS 허용 |
| MVP backend | C# 7.3 compatible source generation |
| Source extension | `.tysh` |
| Project manifest | `TypeSharp.toml` |
| 개발 루프 | VS Code와 CLI를 1급 산출물로 취급 |
| Binding | `let`, `let mut`, compile-time `literal` |
| Function syntax | `fun name(params): ReturnType = expr` 또는 block body |
| Lambda syntax | `let f: A -> B = x => ...` |
| Union | F#식 nominal closed `union` + TS식 local type-level union |
| Public ABI | structural shape, type-level union, anonymous object는 직접 노출 금지 |
| C# interop | framework assembly/local DLL reference, metadata symbol, nominal-first overload |
| Escape hatch | `dynamic`, `reflect`, `interop`, `unsafe` marker 필요 |
| 비목표 | JS runtime compatibility, macro system, .NET 10/11 runtime requirement |

## 작업 선택 규칙

작업은 [checklist.md](checklist.md)의 미완료 항목에서 고른다. 같은 중요도로 보이면 더 앞선 의존성을 선택한다.

우선순위 큐:

| Queue | 의미 | 예시 |
| --- | --- | --- |
| Q0 | 목표와 기준선을 깨는 충돌 수정 | `net48` 불가능한 설계, 문법 충돌, public ABI 누수 |
| Q1 | 다음 구현을 막는 사양 구멍 | parser fixture 형식, lowering 사양, diagnostics code 정책 |
| Q2 | MVP compiler skeleton | lexer, parser, syntax tree, source discovery, diagnostics |
| Q3 | Runtime/interop 핵심 | `Option<T>`, `Result<T,E>`, metadata reader, C# interop tests |
| Q4 | Tooling | CLI command, VS Code LSP, formatter, explain command |
| Q5 | Samples/docs polish | 예제 보강, migration guide, tutorial, compatibility guide |

선택 규칙:
- Q0가 있으면 Q0를 먼저 처리한다.
- 구현이 없는 영역에서는 문서만 더 늘리지 말고 skeleton 또는 fixture를 만든다.
- 사양 없이 구현하면 안 되는 영역은 먼저 좁은 사양과 acceptance test를 만든다.
- 새 기능은 [grammar/coverage.md](grammar/coverage.md)에 분류가 있어야 한다.
- 큰 결정을 내리면 [goal.md](goal.md), [feasibility.md](feasibility.md), [traceability.md](traceability.md), [checklist.md](checklist.md) 중 필요한 곳에 연결한다.

## 반복 루프

각 반복은 아래 순서로 진행한다.

1. Orient
   - 목표, 기준선, 현재 체크리스트 미완료 항목을 확인한다.

2. Select
   - 하나의 작업만 고른다.
   - 작업이 너무 크면 task packet으로 쪼갠다.

3. Scope
   - 수정할 문서/코드 범위를 정한다.
   - 건드리지 않을 범위도 명시한다.

4. Execute
   - 문서, 사양, 코드, 테스트 중 필요한 산출물을 만든다.

5. Verify
   - 가능한 테스트를 실행한다.
   - 테스트가 아직 없으면 문서 링크, 예제 참조, checklist/traceability 연결을 검증한다.

6. Record
   - 완료한 항목을 체크한다.
   - 남은 항목, 막힌 결정, 검증하지 못한 내용을 남긴다.
   - 진행 기록은 [progress.md](progress.md)의 task packet, rollup, commit, 인계 정책을 따른다.

## Task Packet Template

장기 실행 모드가 작업을 시작할 때는 아래 형태로 내부 작업 단위를 만든다. 실제 파일로 남겨야 할 정도로 큰 작업은 `docs/tasks/`를 만들고 이 양식을 사용한다.

```md
# Task: <short-name>

Status: Planned | In Progress | Blocked | Done
Queue: Q0 | Q1 | Q2 | Q3 | Q4 | Q5
Start Time: <current computer time, yyyy-MM-dd HH:mm:ss zzz, or TBD>
End Time: <current computer time, yyyy-MM-dd HH:mm:ss zzz, or TBD>

## Objective

<이 작업이 TypeSharp 목표의 어느 부분을 진전시키는지 한 문장으로 쓴다.>

## Source Of Truth

- <관련 문서 링크>
- <관련 checklist 항목>

## Scope

- In:
- Out:

## Acceptance Criteria

- [ ] 문서/코드 변경 기준
- [ ] 테스트 또는 검증 기준
- [ ] checklist/traceability 갱신 기준

## Verification

- Command:
- Expected:
- Result:

## Handoff

- Done:
- Remaining:
- Blocked:
```

## 완료 기준

문서 작업의 Done:
- 목표 또는 요구사항과 연결된다.
- 관련 grammar/feature/checklist/traceability 중 필요한 문서가 함께 갱신된다.
- 예제가 있으면 현재 공식 문법과 충돌하지 않는다.
- 링크 검증이 통과한다.

구현 작업의 Done:
- 최소 positive test 또는 smoke sample이 있다.
- 실패해야 하는 경우는 diagnostic test가 있다.
- generated code 또는 public API가 `net48` 목표와 충돌하지 않는다.
- CLI 또는 test command로 재현 가능하다.
- 문서와 checklist가 구현 상태를 반영한다.

설계 결정의 Done:
- 선택지와 포기한 대안을 기록한다.
- MVP, Stable Backlog, Preview Watch, Experimental, Rejected 중 하나로 분류한다.
- lowering, runtime cost, public ABI 영향이 기록된다.
- 다음 구현 작업이 checklist나 task packet으로 남는다.
- public ABI, backend, runtime, compiler API, CLI/LSP 계약에 영향을 주는 큰 결정은 [adr/README.md](adr/README.md)의 ADR 형식으로 남긴다.

Task packet 압축 기준:
- 큰 주제의 연속 작업이 완료되면 관련 `docs/tasks` 문서를 하나의 rollup task packet으로 압축할 수 있다.
- rollup은 완료된 하위 작업, 검증 명령, 남은 다음 구현 주제를 보존한다.
- 진행 중인 새 작업은 기존 방식대로 개별 task packet에 남기고, 나중에 큰 주제가 닫힐 때 rollup으로 정리할 수 있다.

## 인계 기준

세션이 끝나거나 context가 줄어들 때는 다음 정보를 남긴다.

- 마지막으로 바꾼 파일
- 검증한 명령과 결과
- 아직 검증하지 못한 것
- 다음에 고를 작업 후보
- 목표/기준선과 충돌할 수 있는 위험

인계 문장은 짧아야 한다. 다음 에이전트가 다시 읽을 문서는 이 문서와 [../agent.md](../agent.md)이므로, 모든 세부 내용을 대화에만 남기지 않는다.

## 현재 권장 다음 작업

현 시점의 다음 작업은 문서 확장보다 구현 준비에 가깝다.

1. C# library interop 구현 범위 확정
2. C# metadata-backed interop validation fixture
3. C# 7.3 source backend 구현 확대

이 목록은 [checklist.md](checklist.md)의 미완료 항목을 줄이는 방향으로 갱신한다.
