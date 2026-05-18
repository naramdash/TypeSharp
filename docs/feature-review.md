# Feature Review Gate

문서 기준일: 2026-05-19

이 문서는 TypeSharp 기능을 추가하거나 안정 기능으로 승격할 때 반복해서 답해야 하는 review gate를 정의한다. [checklist.md](checklist.md)의 반복 검토 질문은 이 문서로 정책화하며, 각 feature task는 관련 질문에 대한 근거를 task packet, traceability, spec, fixture, smoke test 중 하나에 남겨야 한다.

## 원칙

- 질문에 "예"라고 답하려면 구체적 artifact가 있어야 한다.
- 테스트 통과만으로 충분하지 않다. 어떤 요구사항을 어떤 테스트가 덮는지 연결해야 한다.
- Preview/Experimental 기능은 stable 기능과 같은 compatibility claim을 할 수 없다.
- Public .NET boundary, lowering, diagnostics, tooling impact 중 하나라도 불명확하면 feature는 Done이 아니라 Planned 또는 Preview 상태로 남긴다.

## Required Questions

| Question | Required evidence |
| --- | --- |
| 이 기능은 .NET Framework 4.8에서 실행 가능한가? | generated `net48` build smoke, runtime/core `net48` audit, or explicit compile-time-only classification |
| 이 기능은 public .NET metadata로 표현 가능한가, 아니면 compile-time only인가? | [csharp-interop.md](csharp-interop.md), [runtime-abi.md](runtime-abi.md), public ABI snapshot, or public-boundary diagnostic |
| lowering이 문서화되어 있는가? | [lowering.md](lowering.md), backend snapshot, generated C# shape note, or explicit no-lowering/compile-time-only note |
| C# 소비자가 이 API를 이해할 수 있는가? | C# `net48` consumer smoke, public ABI metadata smoke, or documented reason the feature is not public ABI |
| diagnostics가 사용자의 다음 행동을 알려주는가? | diagnostic descriptor with cause/action, golden diagnostic fixture, CLI JSON/text smoke, or LSP diagnostic smoke |
| 프리뷰 기능이 안정 기능처럼 섞여 있지 않은가? | [feature-map.md](feature-map.md) status, [release.md](release.md) preview gate, manifest/CLI feature gate, and release note classification |
| 기능 추가가 formatter/LSP/analyzer를 불필요하게 어렵게 만들지 않는가? | grammar consistency/ambiguity review, formatting convention impact, LSP semantic model impact, or explicit tooling backlog note |
| 테스트가 positive/negative 양쪽을 포함하는가? | [regression-testing.md](regression-testing.md) coverage rule, positive fixture/smoke, and negative diagnostic fixture/smoke when the feature has failure behavior |

## Task Packet Requirements

Feature task packets should include a short review gate section when the change touches syntax, semantics, interop, public API, backend, CLI, LSP, runtime, or release behavior.

```text
## Feature Review Gate

- .NET Framework 4.8: <evidence or compile-time-only reason>
- Public metadata: <evidence or not-public reason>
- Lowering: <document/snapshot/evidence>
- C# consumer: <smoke/evidence or not-public reason>
- Diagnostics: <descriptor/fixture/smoke>
- Preview/stable separation: <feature-map/release gate>
- Tooling impact: <formatter/LSP/analyzer note>
- Positive/negative tests: <fixture/smoke list>
```

Docs-only policy changes can answer with linked documentation and `git diff --check` when no runtime behavior changes.

## Closure Rule

The checklist review questions are considered complete as project process gates because this document defines the required evidence for every future feature task. A specific feature is complete only when its own task packet or traceability row answers the applicable questions with concrete evidence.
