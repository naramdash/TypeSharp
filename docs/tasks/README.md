# Agentic Task Packets

문서 기준일: 2026-05-18

이 폴더는 Ralph mode, Goal mode, Codex `/goal` 같은 장기 실행 에이전트가 바로 집어 들 수 있는 작업 패킷을 보관한다. 작업 패킷 형식과 선택 규칙은 [../agentic-execution.md](../agentic-execution.md)를 따른다.

## 사용 규칙

- 작업이 한 세션 안에서 끝나지 않을 크기라면 task packet을 만든다.
- `Status`는 `Planned`, `In Progress`, `Blocked`, `Done` 중 하나다.
- 새 task packet은 현재 컴퓨터 기준 `Start Time`과 `End Time`을 기록한다. 시작 전이면 `TBD`로 두고, 실제 작업을 시작/완료할 때 시분초까지 갱신한다.
- 완료된 task packet은 지우지 않고 검증 결과와 남은 후속 작업을 남긴다.
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
| [0038-csharp-property-access-smoke.md](0038-csharp-property-access-smoke.md) | Q3 | Done | referenced `net481` C# local DLL의 public property access compile smoke를 추가한다. |
| [0039-csharp-params-interop-smoke.md](0039-csharp-params-interop-smoke.md) | Q3 | Done | referenced `net481` C# local DLL의 `params` API positional call compile smoke를 추가한다. |
| [0040-csharp-out-interop-smoke.md](0040-csharp-out-interop-smoke.md) | Q3 | Done | referenced `net481` C# local DLL의 `out` API call-site compile smoke를 추가한다. |
| [0041-csharp-in-interop-smoke.md](0041-csharp-in-interop-smoke.md) | Q3 | Done | referenced `net481` C# local DLL의 `in` API call-site compile smoke를 추가한다. |
