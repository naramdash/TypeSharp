# Agentic Task Packets

문서 기준일: 2026-05-18

이 폴더는 Ralph mode, Goal mode, Codex `/goal` 같은 장기 실행 에이전트가 바로 집어 들 수 있는 작업 패킷을 보관한다. 작업 패킷 형식과 선택 규칙은 [../agentic-execution.md](../agentic-execution.md)를 따른다.

## 사용 규칙

- 작업이 한 세션 안에서 끝나지 않을 크기라면 task packet을 만든다.
- `Status`는 `Planned`, `In Progress`, `Blocked`, `Done` 중 하나다.
- 새 task packet은 현재 컴퓨터 기준 `Start Time`과 `End Time`을 기록한다. 시작 전이면 `TBD`로 두고, 실제 작업을 시작/완료할 때 시분초까지 갱신한다.
- 완료된 task packet은 rollup 전까지 검증 결과와 남은 후속 작업을 남긴다.
- 큰 주제의 작업 묶음이 완료되면 관련 task packet을 하나의 rollup 문서로 압축할 수 있다.
- task packet이 실제 checklist 항목을 줄이지 않으면 만들지 않는다.

## 현재 작업 큐

| Task | Queue | Status | 목적 |
| --- | --- | --- | --- |
| [0001-0005-foundation-bootstrap.md](0001-0005-foundation-bootstrap.md) | Q1-Q2 | Done | fixture 정책, parser 결정, compiler/CLI skeleton, manifest/source discovery 기반을 묶는다. |
| [0006-0017-parser-implementation-and-coverage.md](0006-0017-parser-implementation-and-coverage.md) | Q2 | Done | lexer/parser 구현과 parser feature coverage fixtures를 묶는다. |
| [0018-0022-diagnostics-and-semantics-skeleton.md](0018-0022-diagnostics-and-semantics-skeleton.md) | Q1-Q2 | Done | diagnostics taxonomy, binder, type checker skeleton과 semantic fixtures를 묶는다. |
| [0023-0032-runtime-cli-interop-backend-skeleton.md](0023-0032-runtime-cli-interop-backend-skeleton.md) | Q2-Q3 | Done | runtime/core, CLI build emission, C# interop metadata, C# backend skeleton을 묶는다. |
| [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md) | Q3 | Done | generated C# source를 CLI-visible `net481` assembly build, C# consumer smoke, manifest reference propagation까지 연결한다. |
| [0038-0042-csharp-member-byref-interop-smokes.md](0038-0042-csharp-member-byref-interop-smokes.md) | Q3 | Done | referenced `net481` C# local DLL의 property, `params`, `out`, `in`, `ref` call-site compile smokes를 묶는다. |
| [0043-csharp-local-metadata-symbol-index.md](0043-csharp-local-metadata-symbol-index.md) | Q3 | Done | local `net481` C# DLL의 public type/member metadata index를 추가한다. |
| [0044-csharp-invalid-byref-diagnostic.md](0044-csharp-invalid-byref-diagnostic.md) | Q3 | Done | local C# metadata와 call-site `ref`/`out`/`in` modifier mismatch를 `TS2403`으로 진단한다. |
| [0045-csharp-ambiguous-overload-diagnostic.md](0045-csharp-ambiguous-overload-diagnostic.md) | Q3 | Done | local C# metadata의 같은 arity overload ambiguity를 `TS2402`로 진단한다. |
| [0046-csharp-exact-overload-ranking.md](0046-csharp-exact-overload-ranking.md) | Q3 | Done | local C# metadata overload set에서 literal/primitive exact match 후보를 선택한다. |
| [0047-csharp-params-metadata-flag.md](0047-csharp-params-metadata-flag.md) | Q3 | Done | local C# metadata index에서 `params` parameter flag를 보존한다. |
