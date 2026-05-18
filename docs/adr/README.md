# Architecture Decision Records

문서 기준일: 2026-05-19

이 폴더는 TypeSharp의 장기 설계 결정을 기록한다. ADR은 task packet과 다르다. task packet은 "무엇을 했는가"를 기록하고, ADR은 "왜 이 방향을 선택했는가"를 기록한다.

## 언제 ADR을 쓰는가

ADR이 필요한 경우:
- `net48` compatibility, public ABI, backend, runtime, compiler API, CLI/LSP 계약이 바뀐다.
- MVP, Stable Backlog, Preview Watch, Experimental, Rejected 분류가 크게 바뀐다.
- 두 개 이상의 합리적인 선택지 중 하나를 고르고 나머지를 보류하거나 포기한다.
- 미래 구현자가 같은 논쟁을 반복할 가능성이 높다.

ADR이 필요 없는 경우:
- 단순 버그 수정
- fixture 추가
- 이미 결정된 정책의 반복 적용
- task packet 안에 충분히 설명되는 작은 구현 선택

## 파일 이름

```text
docs/adr/NNNN-short-title.md
```

규칙:
- `NNNN`은 0001부터 증가한다.
- title은 소문자 kebab-case를 사용한다.
- superseded ADR은 삭제하지 않는다.

## ADR 상태

| Status | 의미 |
| --- | --- |
| Proposed | 아직 검토 중인 결정 |
| Accepted | 현재 적용 중인 결정 |
| Superseded | 다른 ADR로 대체된 결정 |
| Rejected | 검토했지만 채택하지 않은 결정 |

## Template

```md
# ADR NNNN: <Title>

Status: Proposed | Accepted | Superseded | Rejected
Date: yyyy-MM-dd
Supersedes: ADR NNNN | None
Superseded By: ADR NNNN | None

## Context

<결정이 필요한 배경, goal/checklist/requirement 연결, 현재 제약을 쓴다.>

## Decision Drivers

- .NET Framework 4.8 compatibility
- C#/.NET interop
- TypeSharp language consistency
- Tooling and diagnostics impact
- Implementation cost

## Options

1. <Option A>
   - Pros:
   - Cons:

2. <Option B>
   - Pros:
   - Cons:

## Decision

<선택한 옵션과 이유를 쓴다.>

## Consequences

Positive:
- <좋아지는 점>

Negative:
- <비용 또는 제약>

Neutral:
- <관찰할 점>

## Validation

- <검증할 docs/tests/commands>

## Follow-Up

- <다음 task packet, checklist item, or traceability update>
```

## 연결 규칙

- ADR이 accepted 되면 관련 [../traceability.md](../traceability.md) row 또는 관련 사양 문서에서 링크한다.
- 구현이 필요한 ADR은 `docs/tasks` packet을 만든다.
- ADR이 superseded 되면 기존 ADR의 `Superseded By`를 갱신하고 새 ADR의 `Supersedes`를 채운다.
