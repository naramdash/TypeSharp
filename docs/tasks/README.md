# Agentic Task Packets

문서 기준일: 2026-05-19

이 폴더는 Ralph mode, Goal mode, Codex `/goal` 같은 장기 실행 에이전트가 바로 집어 들 수 있는 작업 패킷을 보관한다. 작업 패킷 형식과 선택 규칙은 [../agentic-execution.md](../agentic-execution.md)를 따른다.

## 사용 규칙

- 작업이 한 세션 안에서 끝나지 않을 크기라면 task packet을 만든다.
- `Status`는 `Planned`, `In Progress`, `Blocked`, `Done` 중 하나다.
- 새 task packet은 현재 컴퓨터 기준 `Start Time`과 `End Time`을 기록한다. 시작 전이면 `TBD`로 두고, 실제 작업을 시작/완료할 때 시분초까지 갱신한다.
- 완료된 task packet은 rollup 전까지 검증 결과와 남은 후속 작업을 남긴다.
- task가 `Done`이 되면 관련 변경을 의미 단위로 커밋하고 원격 브랜치에 push한다.
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
| [0107-0116-release-and-compiler-readiness.md](0107-0116-release-and-compiler-readiness.md) | Q1-Q5 | Done | release readiness, feature review/spec index, semantic model/LSP sharing, backend/IL seams, ABI/overload checks, lowering pipeline, and local inference engine을 묶는다. |
| [0117-tooling-docs-adoption-goal.md](0117-tooling-docs-adoption-goal.md) | Q4-Q5 | Done | VS Code LSP extension, Astro Starlight 공식 문서, 실행 예제 프로젝트 목표를 추가하고 `typesharp explain` 구현을 완료한다. |
| [0118-vscode-lsp-client-activation.md](0118-vscode-lsp-client-activation.md) | Q4 | Done | VS Code extension host에서 stdio LSP client를 활성화하고 diagnostics, hover, definition, completion을 language server로 연결한다. |
| [0119-runnable-example-project-catalog.md](0119-runnable-example-project-catalog.md) | Q5 | Done | 실행 가능한 console/library/C# interop/worker host/diagnostics 예제 catalog와 smoke matrix를 추가한다. |
| [0120-astro-starlight-docs-site.md](0120-astro-starlight-docs-site.md) | Q5 | Done | Astro Starlight 공식 문서 사이트와 GitHub Pages 배포 workflow를 추가하고 build smoke를 검증한다. |
| [0121-imported-attribute-generic-interop-smoke.md](0121-imported-attribute-generic-interop-smoke.md) | Q3 | Done | C# local DLL의 attribute type과 generic type을 TypeSharp source에서 참조하는 smoke를 추가한다. |
| [0122-cli-format-mvp.md](0122-cli-format-mvp.md) | Q4 | Done | `typesharp format` MVP와 `--check`/parse-diagnostic no-rewrite smoke를 추가한다. |
| [0123-generic-constraint-lowering.md](0123-generic-constraint-lowering.md) | Q2-Q3 | Done | C# 7.3-compatible generic `where` constraint parsing/lowering과 generated `net48` smoke를 추가한다. |
| [0124-cli-new-mvp.md](0124-cli-new-mvp.md) | Q4 | Done | `typesharp new` console/library project templates와 smoke 검증을 추가한다. |
| [0125-vscode-format-provider.md](0125-vscode-format-provider.md) | Q4 | Done | VS Code `.tysh` document formatter provider와 Extension Host smoke 검증을 추가한다. |
| [0126-binder-duplicate-symbol-diagnostics.md](0126-binder-duplicate-symbol-diagnostics.md) | Q2 | Done | Binder same-scope duplicate symbol `TS2002` 진단과 CLI/fixture smoke를 추가한다. |
| [0127-cli-warnings-as-errors.md](0127-cli-warnings-as-errors.md) | Q4 | Done | CLI `--warnings-as-errors`와 manifest warning gate smoke를 추가한다. |
| [0128-cli-common-option-parsing.md](0128-cli-common-option-parsing.md) | Q4 | Done | CLI `--no-color` 공통 옵션과 diagnostic format 검증 smoke를 추가한다. |
| [0129-cli-configuration-selection.md](0129-cli-configuration-selection.md) | Q4 | Done | CLI `--configuration Debug|Release`를 generated build/run output 경로에 연결한다. |
| [0130-cli-target-framework-override.md](0130-cli-target-framework-override.md) | Q4 | Done | CLI `--target net48`를 generated build/run target framework와 output 경로에 연결한다. |
| [0131-cli-verbosity-build-logging.md](0131-cli-verbosity-build-logging.md) | Q4 | Done | CLI `--verbosity quiet|minimal|normal|diagnostic`를 검증하고 build success log 상세도를 제어한다. |
| [0132-cli-strict-project-option-parsing.md](0132-cli-strict-project-option-parsing.md) | Q4 | Done | CLI project command가 `--preview`를 인식하고 unknown option을 usage 오류로 거부한다. |
| [0133-runnable-aspnet-wcf-host-example.md](0133-runnable-aspnet-wcf-host-example.md) | Q5 | Done | Runnable example catalog에 ASP.NET Web Forms-style/WCF `net48` host 예제를 추가한다. |
| [0134-runnable-wcf-client-proxy-shape.md](0134-runnable-wcf-client-proxy-shape.md) | Q5 | Done | ASP.NET/WCF runnable host 예제에 WCF `ClientBase<T>` client/proxy-shaped 소비 경로를 추가한다. |
| [0135-runnable-host-runtime-dependencies.md](0135-runnable-host-runtime-dependencies.md) | Q5 | Done | Runnable host 예제가 generated/Core/Runtime DLL 참조 배포 형태를 함께 검증한다. |
| [0136-docs-site-vscode-lsp-smoke-contract.md](0136-docs-site-vscode-lsp-smoke-contract.md) | Q5 | Done | Docs-site VS Code/LSP 페이지에 재현 가능한 smoke 명령 계약을 고정한다. |
| [0137-host-compatibility-doc-consistency.md](0137-host-compatibility-doc-consistency.md) | Q5 | Done | Host compatibility 문서에서 구현된 smoke coverage와 Stable Backlog template/packaging 범위를 분리한다. |
| [0138-collection-expression-array-lowering.md](0138-collection-expression-array-lowering.md) | Q2-Q3 | Done | 단순 homogeneous collection expression을 C# 7.3-compatible array creation으로 낮추고 smoke/diagnostic 근거를 추가한다. |
| [0139-pipeline-expression-lowering.md](0139-pipeline-expression-lowering.md) | Q2-Q3 | Done | `value |> f`와 `value |> f(args...)`를 C# 7.3-compatible nested calls로 낮추고 smoke 근거를 추가한다. |
| [0140-indexer-expression-lowering.md](0140-indexer-expression-lowering.md) | Q2-Q3 | Done | `receiver[index]`를 C# 7.3-compatible indexer/array access로 낮추고 imported C# indexer smoke 근거를 추가한다. |
| [0141-record-expression-construction.md](0141-record-expression-construction.md) | Q2-Q3 | Done | expected nominal record type이 있는 `{ Field: value }` expression을 C# 7.3-compatible constructor call로 낮추고 mismatch diagnostics를 추가한다. |
| [0142-csharp-field-interop-smoke.md](0142-csharp-field-interop-smoke.md) | Q3 | Done | local `net48` C# DLL의 public field metadata index와 static/instance field access compile smoke를 추가한다. |
| [0143-csharp-generic-method-interop-smoke.md](0143-csharp-generic-method-interop-smoke.md) | Q3 | Done | local `net48` C# DLL의 generic method metadata placeholder와 imported generic method compile smoke를 추가한다. |
| [0144-csharp-interface-interop-smoke.md](0144-csharp-interface-interop-smoke.md) | Q3 | Done | local `net48` C# DLL의 interface metadata index와 imported interface reference compile smoke를 추가한다. |
| [0145-github-readme-human-accessibility.md](0145-github-readme-human-accessibility.md) | Q5 | Done | GitHub repository 첫 화면용 root `README.md`를 일반 사용자에게 읽히는 소개/quickstart/문서 길잡이로 만든다. |
| [0146-github-pages-human-docs-expansion.md](0146-github-pages-human-docs-expansion.md) | Q5 | Done | TypeScript/F#/C# 공식 문서 구조를 참고해 GitHub Pages 문서 사이트를 tutorials, guides, cookbook, fundamentals, grammar, API 중심으로 확장한다. |
| [0147-list-target-collection-expression-lowering.md](0147-list-target-collection-expression-lowering.md) | Q2-Q3 | Done | 명시적 target `List<T>` collection expression을 C# 7.3-compatible collection initializer로 낮추고 smoke/fixture 근거를 추가한다. |
| [0148-unsupported-package-reference-diagnostic.md](0148-unsupported-package-reference-diagnostic.md) | Q3-Q4 | Done | manifest `references.packages`를 `TS2405` unsupported package reference diagnostic으로 고정하고 check/build no-emission smoke를 추가한다. |
| [0149-partial-declaration-lowering.md](0149-partial-declaration-lowering.md) | Q2-Q3 | Done | `partial` declaration modifier를 parser/backend에 연결하고 class/interface/record/module C# 7.3 lowering smoke를 추가한다. |
| [0150-official-docs-benchmark-and-docs-expansion.md](0150-official-docs-benchmark-and-docs-expansion.md) | Q5 | Done | Vue/Nuxt/TypeScript/C#/F# 공식 문서 사이트 전체 구조를 벤치마크하고 그 결과로 GitHub Pages 문서를 초심자부터 고급 사용자까지 포괄하도록 확장한다. |
| [0151-dynamic-capability-boundary-diagnostic.md](0151-dynamic-capability-boundary-diagnostic.md) | Q2-Q3 | Done | `dynamic` type annotation이 명시 `dynamic fun` boundary 밖에서 쓰이면 `TS2206`으로 막는다. |
| [0152-dynamic-call-capability-propagation.md](0152-dynamic-call-capability-propagation.md) | Q2-Q3 | Done | non-`dynamic` 함수가 `dynamic fun`을 직접 호출하거나 pipeline target으로 사용하면 `TS2207`로 막는다. |
| [0153-capability-call-marker-propagation.md](0153-capability-call-marker-propagation.md) | Q2-Q3 | Done | non-capability 함수가 `reflect`, `interop`, `unsafe` 함수를 직접 호출하거나 pipeline target으로 사용하면 `TS2208`로 막는다. |
| [0154-unknown-access-narrowing-diagnostic.md](0154-unknown-access-narrowing-diagnostic.md) | Q2-Q3 | Done | `unknown` 값의 member/indexer access를 narrowing proof 없이 사용하면 `TS2209`로 막는다. |
| [0155-root-namespace-fallback-for-source-modules.md](0155-root-namespace-fallback-for-source-modules.md) | Q2-Q4 | Done | namespace 없는 source file을 global namespace가 아니라 manifest `rootNamespace` 아래 generated module로 낮춘다. |
| [0156-ambient-function-signature-parsing.md](0156-ambient-function-signature-parsing.md) | Q2-Q4 | Done | `ambient` function signature를 파싱하되 generated C# member로 emit하지 않는다. |
| [0157-open-declaration-lowering.md](0157-open-declaration-lowering.md) | Q2-Q4 | Done | root-level `open Namespace`를 파싱하고 generated C# `using Namespace;`로 낮춘다. |
| [0158-named-import-alias-lowering.md](0158-named-import-alias-lowering.md) | Q2-Q4 | Done | `import { Name as Alias }`를 파싱하고 generated C# alias using으로 낮춘다. |
| [0159-namespace-import-alias-lowering.md](0159-namespace-import-alias-lowering.md) | Q2-Q4 | Done | `import * as Alias`를 파싱하고 generated C# namespace alias using으로 낮춘다. |
| [0160-docs-site-custom-404.md](0160-docs-site-custom-404.md) | Q5 | Done | Starlight docs-site custom 404 page를 추가해 GitHub Pages build warning을 제거한다. |
| [0161-import-alias-conflict-diagnostics.md](0161-import-alias-conflict-diagnostics.md) | Q2-Q4 | Done | import alias가 같은 file scope의 선언/alias와 충돌하면 `TS2002`로 보고하는 계약을 고정한다. |
| [0162-export-specifier-parsing.md](0162-export-specifier-parsing.md) | Q2-Q4 | Done | `export { ... }`, `export type { ... }`, `export * from ...` 문법을 parser surface에 추가한다. |
| [0163-unsupported-export-forwarding-diagnostic.md](0163-unsupported-export-forwarding-diagnostic.md) | Q2-Q4 | Done | source module graph 지원 전 `export { ... }`/`export type { ... }`/`export * from ...` 사용을 `TS2003`으로 막는다. |
| [0164-source-module-path-identity.md](0164-source-module-path-identity.md) | Q2-Q4 | Done | source root 상대 module path를 source discovery에 고정하고 중복 module path를 `TS0111`로 막는다. |
| [0165-relative-source-module-graph.md](0165-relative-source-module-graph.md) | Q2-Q4 | Done | relative source import/export specifier를 source module graph edge로 수집하고 미지원 source import를 `TS0113`으로 막는다. |
| [0166-multi-source-module-container-lowering.md](0166-multi-source-module-container-lowering.md) | Q2-Q4 | Done | 다중 source project에서 module path 기반 generated C# container를 써서 같은 namespace 안의 source files를 함께 빌드한다. |
