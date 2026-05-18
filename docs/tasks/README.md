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
- 진행 기록 세부 정책은 [../progress.md](../progress.md)를 따른다.

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
| [0075-0076-basic-csharp-backend-semantics.md](0075-0076-basic-csharp-backend-semantics.md) | Q2-Q3 | Done | `literal`, 기본 타입/literal, local binding, function declaration/call의 generated C# `net48` smoke를 묶는다. |
| [0077-module-namespace-backend-smoke.md](0077-module-namespace-backend-smoke.md) | Q2 | Done | `module` 선언을 namespace 안의 generated C# static class로 낮추는 smoke를 고정한다. |
| [0078-csharp-byref-params-interop-status.md](0078-csharp-byref-params-interop-status.md) | Q3 | Done | C# `params`, `out`, `in`, `ref` interop aggregate checklist 상태를 기존 smoke와 diagnostic 근거에 맞춘다. |
| [0079-0083-public-api-declaration-backend-smokes.md](0079-0083-public-api-declaration-backend-smokes.md) | Q2-Q3 | Done | Core generic APIs, generic functions, class/interface declarations, and generic type declarations의 generated C# public API smokes를 묶는다. |
| [0084-0085-immutable-record-backend.md](0084-0085-immutable-record-backend.md) | Q2-Q3 | Done | immutable record class shape와 record copy/update lowering smokes를 묶는다. |
| [0086-0089-union-implementation.md](0086-0089-union-implementation.md) | Q2-Q3 | Done | nominal union API, type-level union public boundary diagnostic, nominal union match lowering, and local type-level union narrowing을 묶는다. |
| [0090-null-safety-diagnostics.md](0090-null-safety-diagnostics.md) | Q2-Q3 | Done | null/nullable 값이 non-null TypeSharp 위치로 흐를 때 `TS2202`로 진단하고 CLI no-emission path를 고정한다. |
| [0091-basic-structural-type-checking.md](0091-basic-structural-type-checking.md) | Q2-Q3 | Done | local structural shape aliases가 required member를 가진 nominal record 값을 compile-time proof로 받아들이고 mismatch를 진단한다. |
| [0092-async-task-interop-lowering.md](0092-async-task-interop-lowering.md) | Q2-Q3 | Done | `async fun`과 `await`을 generated C# `async Task<T>` source로 낮춰 `net48` build 및 C# consumer smoke를 고정한다. |
| [0093-lowering-examples-catalog.md](0093-lowering-examples-catalog.md) | Q1-Q5 | Done | 구현된 C# 7.3 source backend lowering을 기능별 예제와 fixture 근거로 문서화한다. |
| [0094-test-coverage-checklist-audit.md](0094-test-coverage-checklist-audit.md) | Q2-Q3 | Done | existing lowering golden, runtime unit, and C# interop test evidence를 checklist/traceability에 맞춘다. |
| [0095-progress-and-adr-policy.md](0095-progress-and-adr-policy.md) | Q1 | Done | long-running progress log policy와 architecture decision record 형식을 고정한다. |
| [0096-migration-guide-draft.md](0096-migration-guide-draft.md) | Q5 | Done | existing .NET Framework 4.8/C# projects가 TypeSharp library를 점진적으로 도입하는 migration guide 초안을 만든다. |
| [0097-stable-grammar-parser-fixture-audit.md](0097-stable-grammar-parser-fixture-audit.md) | Q1-Q2 | Done | stable grammar parser fixture coverage를 현재 snapshot evidence에 맞춰 문서화한다. |
| [0098-public-abi-snapshot-smoke.md](0098-public-abi-snapshot-smoke.md) | Q2-Q3 | Done | generated `net48` assembly public metadata shape를 smoke test로 고정한다. |
| [0099-performance-smoke-benchmark.md](0099-performance-smoke-benchmark.md) | Q2-Q3 | Done | compiler check pipeline의 극단적 성능 회귀를 잡는 관대한 smoke benchmark를 추가한다. |
| [0100-regression-test-policy.md](0100-regression-test-policy.md) | Q1-Q3 | Done | 새 변경이 어떤 fixture, smoke, metadata check, docs verification을 가져야 하는지 정책화한다. |
| [0101-reference-resolver-audit.md](0101-reference-resolver-audit.md) | Q3 | Done | framework assembly/local DLL reference resolver 구현 범위와 smoke evidence를 체크리스트에 맞춘다. |
| [0102-metadata-reader-audit.md](0102-metadata-reader-audit.md) | Q3 | Done | C# metadata reader 구현 범위와 framework/local public metadata smoke evidence를 체크리스트에 맞춘다. |
| [0103-binder-audit.md](0103-binder-audit.md) | Q2 | Done | binder 구현 범위와 symbol/name-resolution smoke evidence를 체크리스트에 맞춘다. |
| [0104-type-checker-audit.md](0104-type-checker-audit.md) | Q2-Q3 | Done | type checker 구현 범위와 diagnostics/build-stop/LSP smoke evidence를 체크리스트에 맞춘다. |
| [0105-diagnostics-system-audit.md](0105-diagnostics-system-audit.md) | Q1-Q2 | Done | diagnostics system 구현 범위와 descriptor/fixture/CLI/LSP smoke evidence를 체크리스트에 맞춘다. |
| [0106-csharp-source-backend-audit.md](0106-csharp-source-backend-audit.md) | Q3 | Done | C# 7.3 source backend 구현 범위와 snapshot/build/consumer/host smoke evidence를 체크리스트에 맞춘다. |
| [0107-release-readiness-policy.md](0107-release-readiness-policy.md) | Q5 | Done | 릴리스 versioning, breaking change, preview gate, checksum/signing, security, release notes, compatibility matrix 정책을 묶는다. |
| [0108-semantic-model-lsp-sharing.md](0108-semantic-model-lsp-sharing.md) | Q2-Q4 | Done | compiler semantic model API를 만들고 LSP diagnostics/hover/definition/completion이 공유하게 한다. |
| [0109-feature-review-gate.md](0109-feature-review-gate.md) | Q1-Q5 | Done | 반복 검토 질문을 기능별 Done gate 정책으로 고정한다. |
| [0110-backend-abstraction-seam.md](0110-backend-abstraction-seam.md) | Q3 | Done | C# source backend를 compiler backend abstraction 뒤로 연결한다. |
| [0111-public-abi-checker.md](0111-public-abi-checker.md) | Q3-Q5 | Done | generated assembly public metadata를 deterministic ABI snapshot checker로 검증한다. |
| [0112-csharp-overload-resolver.md](0112-csharp-overload-resolver.md) | Q3 | Done | C# interop overload 후보 선택을 독립 resolver API로 분리하고 현재 범위를 문서화한다. |
| [0113-feature-specification-index.md](0113-feature-specification-index.md) | Q1-Q5 | Done | 현재 구현/안정 기능의 세부 사양 문서와 검증 근거를 기능별 색인으로 묶는다. |
| [0114-lowering-pass-pipeline.md](0114-lowering-pass-pipeline.md) | Q2-Q3 | Done | backend emit 전 ordered lowering pass pipeline과 runtime helper import lowering pass를 추가한다. |
