# TypeSharp Agent Checklist

문서 기준일: 2026-05-21

이 파일은 다음 작업을 고르기 위한 짧은 운영 체크리스트다. 완료된 대량 항목의 세부 이력은 [tasks-rollup.md](tasks-rollup.md)와 docs canonical 문서가 가진다.

## 현재 미완료 작업

- [x] 전체 테스트 구성을 감사하고, placeholder/약한 smoke/stale fixture/의미 없는 assertion을 원래 목적에 맞는 실제 사례 기반 테스트로 교체한다.

## 작업 선택 규칙

- [tasks.md](tasks.md)에 active task가 있으면 그 작업을 먼저 이어간다.
- active task가 없으면 위 미완료 항목 중 가장 앞선 의존성을 고른다.
- 새 작업이 한 세션을 넘기면 `agent/NNNN-short-name.md` active task packet을 만들고 [tasks.md](tasks.md)를 갱신한다.
- 완료된 active task packet은 [tasks-rollup.md](tasks-rollup.md)에 압축하고 개별 packet 파일은 제거한다.

## 완료 기준

- 문서/사양 변경은 docs canonical owner와 `agent/` 운영 문서가 충돌하지 않아야 한다.
- 구현 변경은 positive coverage와 실패해야 하는 경우의 diagnostic 또는 smoke coverage를 가진다.
- 테스트 변경은 실제 input, expected output, diagnostic, generated code, runtime output, or assertion으로 목적을 증명해야 한다.
- `net48` generated assembly/runtime compatibility를 깨뜨릴 수 있는 변경은 관련 smoke 또는 명시적 검증 근거를 남긴다.
- 작업 종료 시 [tasks.md](tasks.md), [traceability.md](traceability.md), [tasks-rollup.md](tasks-rollup.md), docs Work Ledger 중 필요한 원장을 갱신한다.

## 완료된 기준선 요약

- [x] Core goal, requirements, feature status, grammar/reference, project policy, CLI/API, interop, lowering, diagnostics, examples, migration, and release policy live in docs canonical pages.
- [x] `agent/`는 agent work 운영 표면으로 축소되었고 하위 폴더 없이 flat file 구조를 가진다.
- [x] [agent.md](../agent.md), [agentic-execution.md](agentic-execution.md), [progress.md](progress.md), [tasks.md](tasks.md), and [tasks-rollup.md](tasks-rollup.md) define the long-running agent workflow.
- [x] Parser, binder, type checker, backend, CLI, VS Code/LSP, runtime/core, .NET interop, host compatibility, docs build, runnable examples, and regression smoke coverage exist.
- [x] Completed task evidence through task 0277 is compressed in [tasks-rollup.md](tasks-rollup.md).

## 반복 검토 질문

- 이 작업은 [Core Goal](../docs/src/content/docs/goal.md)에 직접 기여하는가?
- docs canonical 문서와 `agent/` 운영 문서 중 어느 표면이 owner인가?
- 언어/compiler 변경이면 `typesharp-language-engineering`을 적용했는가?
- .NET/runtime/interop/build 변경이면 `typesharp-dotnet`을 적용했는가?
- 테스트가 positive/negative 양쪽을 충분히 포함하는가?
