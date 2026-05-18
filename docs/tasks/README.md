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
| [0033-0037-generated-net48-build-pipeline.md](0033-0037-generated-net48-build-pipeline.md) | Q3 | Done | generated C# source를 CLI-visible `net48` assembly build, C# consumer smoke, manifest reference propagation까지 연결한다. |
| [0038-0042-csharp-member-byref-interop-smokes.md](0038-0042-csharp-member-byref-interop-smokes.md) | Q3 | Done | referenced `net48` C# local DLL의 property, `params`, `out`, `in`, `ref` call-site compile smokes를 묶는다. |
| [0043-0048-csharp-metadata-backed-interop-validation.md](0043-0048-csharp-metadata-backed-interop-validation.md) | Q3 | Done | local C# metadata index, byref diagnostics, overload ambiguity, exact narrowing, and expanded `params` validation을 묶는다. |
| [0049-netfx-application-model-compatibility-contract.md](0049-netfx-application-model-compatibility-contract.md) | Q1 | Done | .NET Framework ASP.NET, WCF, and worker compatibility goal을 requirements/checklist/traceability로 연결한다. |
| [0050-0051-csharp-optional-named-overload-validation.md](0050-0051-csharp-optional-named-overload-validation.md) | Q3 | Done | optional parameter omission과 named argument overload validation을 묶는다. |
| [0052-csharp-unknown-nullability-diagnostic.md](0052-csharp-unknown-nullability-diagnostic.md) | Q3 | Done | nullable annotation 없는 imported C# reference return을 strict mode warning으로 보고한다. |
| [0053-0055-csharp-delegate-event-host-smokes.md](0053-0055-csharp-delegate-event-host-smokes.md) | Q3 | Done | C# delegate/event call-site와 ASP.NET/WCF/worker-style host reference smokes를 묶는다. |
| [0056-net48-dependency-compatibility-audit.md](0056-net48-dependency-compatibility-audit.md) | Q3 | Done | Core/Runtime/generated artifact dependency inventory와 `net48` API drift audit를 고정한다. |
| [0057-0060-cli-run-net48-executable-flow.md](0057-0060-cli-run-net48-executable-flow.md) | Q0-Q4 | Done | `net48` executable `typesharp run`, args forwarding, and unsupported main signature diagnostics를 묶는다. |
| [0061-cli-check-type-diagnostic-coverage.md](0061-cli-check-type-diagnostic-coverage.md) | Q4 | Done | `typesharp check`가 `TS2201` type checker diagnostics를 CLI JSON output으로 보고하는지 고정한다. |
| [0062-cli-build-type-diagnostic-stop.md](0062-cli-build-type-diagnostic-stop.md) | Q4 | Done | `typesharp build`가 `TS2201` type checker diagnostics에서 generated emission 전에 멈추는지 고정한다. |
| [0063-net48-repository-consistency-sweep.md](0063-net48-repository-consistency-sweep.md) | Q0-Q3 | Done | `net481`에서 `net48`로 내려온 기본 build target 결정을 repo-wide 문서 색인과 task rollup에 일관되게 반영한다. |
| [0064-formatter-convention.md](0064-formatter-convention.md) | Q4 | Done | `typesharp format`과 VS Code formatter가 공유할 `.tysh` canonical layout을 문서화한다. |
| [0065-0069-vscode-lsp-tooling.md](0065-0069-vscode-lsp-tooling.md) | Q4 | Done | VS Code `.tysh` language scaffold와 LSP diagnostics, hover, go-to-definition, basic completion을 묶는다. |
| [0070-0073-runtime-helper-surface.md](0070-0073-runtime-helper-surface.md) | Q3 | Done | `TypeSharp.Runtime`의 union, pattern, equality/hash, async generated-code helper surface를 묶는다. |
| [0074-runtime-abi-versioning-policy.md](0074-runtime-abi-versioning-policy.md) | Q3 | Done | runtime/core/generated assembly public ABI versioning 정책과 ABI constant alignment smoke를 고정한다. |
| [0075-compile-time-literal-lowering.md](0075-compile-time-literal-lowering.md) | Q2-Q3 | Done | `literal` compile-time constant declaration을 generated C# `net48` public/internal constant field로 낮춘다. |
| [0076-basic-semantics-smoke.md](0076-basic-semantics-smoke.md) | Q2 | Done | 기본 타입/literal, local binding, function declaration/call의 generated C# `net48` smoke를 고정한다. |
