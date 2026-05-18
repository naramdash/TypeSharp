# Agentic Task Packets

문서 기준일: 2026-05-19

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
| [0043-0048-csharp-metadata-backed-interop-validation.md](0043-0048-csharp-metadata-backed-interop-validation.md) | Q3 | Done | local C# metadata index, byref diagnostics, overload ambiguity, exact narrowing, and expanded `params` validation을 묶는다. |
| [0049-netfx-application-model-compatibility-contract.md](0049-netfx-application-model-compatibility-contract.md) | Q1 | Done | .NET Framework ASP.NET, WCF, and worker compatibility goal을 requirements/checklist/traceability로 연결한다. |
| [0050-0051-csharp-optional-named-overload-validation.md](0050-0051-csharp-optional-named-overload-validation.md) | Q3 | Done | optional parameter omission과 named argument overload validation을 묶는다. |
| [0052-csharp-unknown-nullability-diagnostic.md](0052-csharp-unknown-nullability-diagnostic.md) | Q3 | Done | nullable annotation 없는 imported C# reference return을 strict mode warning으로 보고한다. |
| [0053-0055-csharp-delegate-event-host-smokes.md](0053-0055-csharp-delegate-event-host-smokes.md) | Q3 | Done | C# delegate/event call-site와 ASP.NET/WCF/worker-style host reference smokes를 묶는다. |
| [0056-net481-dependency-compatibility-audit.md](0056-net481-dependency-compatibility-audit.md) | Q3 | Done | Core/Runtime/generated artifact dependency inventory와 `net481` API drift audit를 고정한다. |
| [0057-cli-run-net481-executable-smoke.md](0057-cli-run-net481-executable-smoke.md) | Q4 | Done | `typesharp run`이 generated `net481` executable을 빌드하고 실행하는 smoke path를 만든다. |
| [0058-net48-default-target-migration.md](0058-net48-default-target-migration.md) | Q0 | Done | 기본 산출물 타깃을 `net48`로 낮추고 compiler/runtime/tests/current docs 기준을 정렬한다. |
| [0059-cli-run-main-args-forwarding.md](0059-cli-run-main-args-forwarding.md) | Q4 | Done | `typesharp run -- ...` program arguments를 generated `main(args: string[])`로 전달한다. |
