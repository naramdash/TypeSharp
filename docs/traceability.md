# 목표-요구사항 추적성

문서 기준일: 2026-05-19

이 문서는 TypeSharp의 목표가 요구사항, 기능 매핑, 아키텍처, 체크리스트로 이어지는지 확인한다.

## 핵심 목표 추적표

| 목표 | 요구사항 | 기능 문서 | 검증 체크리스트 |
| --- | --- | --- | --- |
| .NET Framework 4.8에서 실행 | [requirements.md](requirements.md) 1, 4, 5, 8 | [feature-map.md](feature-map.md) 전체 lowering 기준, [dependencies.md](dependencies.md) | [checklist.md](checklist.md) 플랫폼, 컴파일러, 테스트 |
| .NET Framework ASP.NET/WCF/worker 호환성 | [requirements.md](requirements.md) 1, 5 | [feature-map.md](feature-map.md) 16, [csharp-interop.md](csharp-interop.md) Application Model Compatibility | [checklist.md](checklist.md) 플랫폼 |
| TypeScript식 암묵적이고 유연한 타입 | [requirements.md](requirements.md) 2, 3, 4 | [feature-map.md](feature-map.md) 1, 2, 4, 5 | [goal.md](goal.md) 기능 목표 2, 7, 8 |
| TypeScript식 모듈 기반 파일 구조 | [requirements.md](requirements.md) 2, 7 | [feature-map.md](feature-map.md) 10, 13 | [goal.md](goal.md) 기능 목표 3, 11 |
| F#식 함수형 기능과 일관성 | [requirements.md](requirements.md) 2, 6 | [feature-map.md](feature-map.md) 1, 2, 3, 7, 9, 11 | [goal.md](goal.md) 기능 목표 4, 5, 6, 9 |
| C#식 다양하고 유연한 편의 기능 | [requirements.md](requirements.md) 3, 5 | [feature-map.md](feature-map.md) 6, 7, 8, 12, 14, 15 | [goal.md](goal.md) 기능 목표 6, 10, 15 |
| TypeScript/F#/C# 문법 포괄 또는 대체 | [requirements.md](requirements.md) 2, 3, 4 | [grammar/README.md](grammar/README.md), [grammar/coverage.md](grammar/coverage.md) | [checklist.md](checklist.md) 문서 완성도, 언어 사양 |
| 문법 표면 최소화와 일관성 | [requirements.md](requirements.md) 2, 4, 7 | [grammar/consistency.md](grammar/consistency.md) | [checklist.md](checklist.md) 문서 완성도 |
| 표준 라이브러리 핵심 타입 | [requirements.md](requirements.md) 6 | [standard-library.md](standard-library.md) | [checklist.md](checklist.md) 런타임 라이브러리 |
| 실현 가능한 MVP 범위 | [requirements.md](requirements.md) 1, 4, 7, 8 | [feasibility.md](feasibility.md), [architecture.md](architecture.md) | [checklist.md](checklist.md) 플랫폼, 컴파일러, 도구 |
| Union 설계 | [requirements.md](requirements.md) 2, 5, 6 | [feature-map.md](feature-map.md) 2, 3, 4 | [goal.md](goal.md) Union 설계 결정 |
| VS Code와 CLI 개발 경험 | [requirements.md](requirements.md) 7 | [cli.md](cli.md), [architecture.md](architecture.md) Tooling | [goal.md](goal.md) 기능 목표 0, 필수 성공 조건 |
| 최신 C# 기능 반영 | [requirements.md](requirements.md) 3 | [feature-map.md](feature-map.md) 6, 8, 12, 14, 15 | [checklist.md](checklist.md) 언어 사양, MVP 기능 |
| 최신 F# 기능 반영 | [requirements.md](requirements.md) 3, 6 | [feature-map.md](feature-map.md) 1, 2, 3, 7, 11 | [checklist.md](checklist.md) MVP 기능, 런타임 라이브러리 |
| 최신 TypeScript 기능 반영 | [requirements.md](requirements.md) 3, 4 | [feature-map.md](feature-map.md) 4, 5, 13 | [checklist.md](checklist.md) 언어 사양, 도구 |
| C#/.NET 상호 운용 | [requirements.md](requirements.md) 5 | [feature-map.md](feature-map.md) 2, 4, 7, 10, 14, 15, 16, [csharp-interop.md](csharp-interop.md) | [checklist.md](checklist.md) MVP 기능, 테스트 |
| 컴파일 타임 상수 | [requirements.md](requirements.md) 2, 5 | [feature-map.md](feature-map.md) 15, [grammar/consistency.md](grammar/consistency.md), [grammar/declarations.md](grammar/declarations.md) | [checklist.md](checklist.md) 언어 사양, MVP 기능 |
| 타입 안전성 기본값 | [requirements.md](requirements.md) 2, 3, 8 | [feature-map.md](feature-map.md) 1, 2, 3, 5, 11 | [checklist.md](checklist.md) 언어 사양, 테스트 |
| 설명 가능한 lowering | [requirements.md](requirements.md) 3, 4 | [architecture.md](architecture.md) Lowering과 IR, Backend 전략 | [checklist.md](checklist.md) 반복 검토 질문 |
| 도구 친화성 | [requirements.md](requirements.md) 4, 7 | [architecture.md](architecture.md) Tooling, [formatting.md](formatting.md) | [checklist.md](checklist.md) 도구 |
| 에이전틱 장기 수행 | [agent.md](../agent.md) 반복 실행 규칙 | [agentic-execution.md](agentic-execution.md), [README.md](README.md) 반복 검토 프로토콜 | [checklist.md](checklist.md) 문서 완성도, 에이전틱 실행 준비, 반복 검토 질문 |
| 장기 작업 큐 | [agentic-execution.md](agentic-execution.md) 작업 선택 규칙 | [tasks/README.md](tasks/README.md) task packet 목록 | [checklist.md](checklist.md) 에이전틱 실행 준비 |

## 성공 조건별 검증 경로

### `net48` Hello World

- 요구사항: 플랫폼 요구사항, 컴파일러 요구사항
- 아키텍처: Backend 전략, CLI
- 체크리스트: 플랫폼, 컴파일러, 테스트
- 완료 기준: `typesharp build`가 `net48` 실행 파일 또는 라이브러리를 만들고 Windows .NET Framework 4.8 환경에서 실행된다.
- 현재 검증: `tests/TypeSharp.Compiler.Tests`의 `CLI run builds and runs generated net48 executable` smoke가 `typesharp run`으로 generated `RunSmoke.exe`를 빌드하고 현재 Windows .NET Framework 환경에서 실행해 `Hello from TypeSharp run` 출력을 확인한다. `CLI run passes arguments to generated main` smoke는 `typesharp run ... -- alpha beta`가 generated `main(string[] args)`에 인자를 전달하고 `args.Length` 기반 출력 `2`를 생성하는지 검증한다. `CLI run reports unsupported main signature` smoke는 지원하지 않는 executable main signature를 `TS3500`으로 보고하고 generated emission 전에 중단하는지 검증한다.

### .NET Framework Application Model Compatibility

- 요구사항: 플랫폼 요구사항, .NET 상호 운용 요구사항
- 기능 매핑: .NET Framework Application Model Compatibility, C# Library Interop
- 문서: [csharp-interop.md](csharp-interop.md)
- 체크리스트: 플랫폼
- 완료 기준: TypeSharp generated library와 runtime dependency가 ASP.NET Web Forms/MVC/Web API 프로젝트에서 일반 `net48` class library처럼 참조된다.
- 완료 기준: WCF service/client 또는 contract project가 TypeSharp public API를 CLR-visible metadata로 소비한다.
- 완료 기준: Windows Service 또는 worker-style `net48` project가 TypeSharp generated assembly를 참조하고 long-running host lifecycle에서 loader/runtime dependency 문제가 없다.

### VS Code와 CLI 개발 루프

- 요구사항: 도구 요구사항
- 문서: [cli.md](cli.md)
- 아키텍처: CLI, Language Server
- 체크리스트: 도구
- 완료 기준: CLI에서 `check`, `build`, `run`, `version`을 사용할 수 있다.
- 완료 기준: [examples/cli-console](examples/cli-console/README.md) 프로젝트가 `typesharp check`, `typesharp build`, `typesharp run` smoke fixture로 사용된다.
- 완료 기준: formatter 구현 전에 [formatting.md](formatting.md)가 `.tysh` canonical layout과 `typesharp format --check` 기준을 정의한다.
- 완료 기준: [../vscode/typesharp](../vscode/typesharp)이 `.tysh` 파일을 `typesharp` language id로 등록하고 lexical TextMate grammar를 제공한다.
- 완료 기준: `TypeSharp.LanguageServer`가 open document를 compiler diagnostics로 검사하고 LSP `publishDiagnostics` notification으로 보고한다.
- 완료 기준: VS Code에서 syntax highlighting, diagnostics, hover, go-to-definition의 최소 기능을 사용할 수 있다.
- 완료 기준: VS Code extension과 CLI가 같은 compiler semantic model과 diagnostics code를 공유한다.

### Grammar Coverage

- 요구사항: 언어 핵심 요구사항, 최신 언어 기능 반영 요구사항, 컴파일러 요구사항
- 문법: [grammar/README.md](grammar/README.md), [grammar/coverage.md](grammar/coverage.md)
- 체크리스트: 문서 완성도, 언어 사양, 도구
- 완료 기준: TypeScript, F#, C#의 주요 실용 기능이 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 분류된다.
- 완료 기준: Stable Grammar로 분류된 기능은 syntax 예제와 parser가 구현할 수 있는 grammar skeleton을 가진다.
- 완료 기준: Replacement 또는 Rejected 기능은 goal의 원칙과 비목표에 연결된다.

### C# 상호 운용

- 요구사항: .NET 상호 운용 요구사항
- 기능 매핑: nominal union public ABI, type-level union boundary, structural type boundary, async `Task` interop, C# library interop
- 문서: [csharp-interop.md](csharp-interop.md), [grammar/interop.md](grammar/interop.md)
- 체크리스트: C# assembly/local DLL reference, C# member call, TypeSharp assembly consumed from C#, C# interop tests
- 완료 기준: C# .NET Framework 프로젝트가 TypeSharp assembly를 참조하고 public API를 호출한다.
- 완료 기준: TypeSharp 프로젝트가 framework assembly와 local `net48` C# library DLL을 참조하고 constructor, method, property, delegate, event, generic API를 호출한다.
- 완료 기준: nullable unknown, ambiguous overload, invalid byref, public ABI leak diagnostic이 fixture로 검증된다.

### Null Safety

- 요구사항: 언어 핵심 요구사항, 표준 라이브러리 요구사항
- 기능 매핑: Null Safety, Option/Result
- 체크리스트: nullability 규칙, `Option<T>`, type checker negative fixtures
- 완료 기준: nullable contract 위반이 compile-time diagnostic으로 보고된다.

### Union과 Pattern Matching

- 요구사항: 언어 핵심 요구사항, 최신 언어 기능 반영 요구사항
- 기능 매핑: Union Type과 Discriminated Union, Pattern Matching
- 체크리스트: nominal closed union type, type-level union alias, union narrowing, pattern matching, exhaustiveness 규칙
- 완료 기준: nominal closed union의 누락 case가 diagnostic으로 보고되고, 모든 case를 처리한 match가 실행 코드로 낮아진다.
- 완료 기준: TypeScript식 type-level union이 local inference와 narrowing에는 사용되지만 public .NET ABI로 직접 노출되지 않는다.

### Structural Type

- 요구사항: 언어 핵심 요구사항, 컴파일러 요구사항
- 기능 매핑: Structural Type, Type Inference
- 체크리스트: basic structural type checking, structural type 규칙
- 완료 기준: 필요한 shape를 가진 값은 통과하고 누락 member가 있는 값은 diagnostic을 낸다.

### Async/Task

- 요구사항: .NET 상호 운용 요구사항, 표준 라이브러리 요구사항
- 기능 매핑: Async와 Task Workflow
- 체크리스트: `Task`/`Task<T>` async interop, async helper
- 완료 기준: TypeSharp async function이 C#에서 `Task`/`Task<T>`로 호출된다.

## 자체 검토 결과

| 기준 | 상태 | 근거 |
| --- | --- | --- |
| 목표 명료성 | 통과 | [goal.md](goal.md)에 한 문장 과제, 기준선, 성공 조건이 있다. |
| 기능 목표 통합 | 통과 | [goal.md](goal.md)에 기능 목표가 통합되어 있다. |
| 에이전트 지침 | 통과 | [agent.md](../agent.md)에 `/goal` 목표 문장, 시작 루틴, 반복 실행 규칙이 있다. |
| 장기 실행 계약 | 통과 | [agentic-execution.md](agentic-execution.md)가 Ralph/Goal mode의 부트스트랩, 작업 큐, task packet, Done 기준, 인계 기준을 정의한다. |
| 작업 패킷 | 통과 | [tasks/README.md](tasks/README.md), [tasks/0001-0005-foundation-bootstrap.md](tasks/0001-0005-foundation-bootstrap.md), [tasks/0006-0017-parser-implementation-and-coverage.md](tasks/0006-0017-parser-implementation-and-coverage.md), [tasks/0018-0022-diagnostics-and-semantics-skeleton.md](tasks/0018-0022-diagnostics-and-semantics-skeleton.md), [tasks/0023-0032-runtime-cli-interop-backend-skeleton.md](tasks/0023-0032-runtime-cli-interop-backend-skeleton.md), [tasks/0033-0037-generated-net48-build-pipeline.md](tasks/0033-0037-generated-net48-build-pipeline.md), [tasks/0038-0042-csharp-member-byref-interop-smokes.md](tasks/0038-0042-csharp-member-byref-interop-smokes.md), [tasks/0043-0048-csharp-metadata-backed-interop-validation.md](tasks/0043-0048-csharp-metadata-backed-interop-validation.md), [tasks/0049-netfx-application-model-compatibility-contract.md](tasks/0049-netfx-application-model-compatibility-contract.md), [tasks/0050-0051-csharp-optional-named-overload-validation.md](tasks/0050-0051-csharp-optional-named-overload-validation.md), [tasks/0052-csharp-unknown-nullability-diagnostic.md](tasks/0052-csharp-unknown-nullability-diagnostic.md), [tasks/0053-0055-csharp-delegate-event-host-smokes.md](tasks/0053-0055-csharp-delegate-event-host-smokes.md), [tasks/0056-net48-dependency-compatibility-audit.md](tasks/0056-net48-dependency-compatibility-audit.md), [tasks/0057-0060-cli-run-net48-executable-flow.md](tasks/0057-0060-cli-run-net48-executable-flow.md), [tasks/0061-cli-check-type-diagnostic-coverage.md](tasks/0061-cli-check-type-diagnostic-coverage.md), [tasks/0062-cli-build-type-diagnostic-stop.md](tasks/0062-cli-build-type-diagnostic-stop.md), [tasks/0063-net48-repository-consistency-sweep.md](tasks/0063-net48-repository-consistency-sweep.md), [tasks/0064-formatter-convention.md](tasks/0064-formatter-convention.md), [tasks/0065-0069-vscode-lsp-tooling.md](tasks/0065-0069-vscode-lsp-tooling.md), [tasks/0070-0073-runtime-helper-surface.md](tasks/0070-0073-runtime-helper-surface.md), [tasks/0074-runtime-abi-versioning-policy.md](tasks/0074-runtime-abi-versioning-policy.md), [tasks/0075-0076-basic-csharp-backend-semantics.md](tasks/0075-0076-basic-csharp-backend-semantics.md), [tasks/0077-module-namespace-backend-smoke.md](tasks/0077-module-namespace-backend-smoke.md), [tasks/0078-csharp-byref-params-interop-status.md](tasks/0078-csharp-byref-params-interop-status.md), [tasks/0079-0083-public-api-declaration-backend-smokes.md](tasks/0079-0083-public-api-declaration-backend-smokes.md), [tasks/0084-immutable-record-backend-smoke.md](tasks/0084-immutable-record-backend-smoke.md)가 구현 준비, C# interop, 도구, runtime/helper/backend 진행 상태를 압축된 rollup/packet 단위로 정의한다. |
| 필수/권장 분리 | 통과 | [requirements.md](requirements.md)가 영역별 필수/권장을 분리한다. |
| 최신 기능 분류 | 통과 | [feature-map.md](feature-map.md)가 MVP, Stable Backlog, Preview Watch, Experimental, Rejected를 사용한다. |
| .NET Framework 제약 반영 | 통과 | 모든 핵심 문서가 `net48`과 .NET Framework 4.8을 기준으로 둔다. |
| 실행 체크리스트 | 통과 | [checklist.md](checklist.md)가 문서, 플랫폼, 언어, 컴파일러, 런타임, 테스트, 릴리스 항목을 가진다. |
| 공식 근거 | 통과 | [references.md](references.md)에 공식 링크와 기준선이 있다. |
| 문법 사양 구조 | 통과 | [grammar/README.md](grammar/README.md)가 문법 문서 구성, 안정성 등급, 구현 순서를 정의한다. |
| 문법 일관성 규칙 | 통과 | [grammar/consistency.md](grammar/consistency.md)가 공통 기호, namespace/import 순서, mutability, optional/nullable 규칙을 정의한다. |
| 문법 ambiguity 검토 | 통과 | [grammar/ambiguity.md](grammar/ambiguity.md)가 stable grammar의 token/context 충돌, parser decision, recovery 후속 작업을 기록한다. |
| Parser precedence | 통과 | [grammar/precedence.md](grammar/precedence.md)가 expression/type/pattern operator precedence와 associativity를 parser 구현 전에 고정한다. |
| 표준 라이브러리 namespace | 통과 | [standard-library.md](standard-library.md)가 `TypeSharp.Core`, `TypeSharp.Collections`, `TypeSharp.Runtime`의 역할을 정의한다. |
| C# library interop | 통과 | [csharp-interop.md](csharp-interop.md)가 reference, import, type mapping, overload, nullable, public ABI 계약을 정의한다. |
| CLI command surface | 통과 | [cli.md](cli.md)가 `typesharp` command, manifest, diagnostics format, exit code를 정의한다. |
| CLI 예제 프로젝트 | 통과 | [examples/cli-console](examples/cli-console/README.md)이 manifest와 `src/Main.tysh` 기반 workflow를 제공한다. |
| Formatter convention | 통과 | [formatting.md](formatting.md)가 `.tysh` canonical layout, semicolon policy, namespace/import/declaration order, pipeline/match layout, multi-line list layout, interop preservation, `typesharp format --check` behavior를 정의한다. |
| VS Code extension scaffold | 통과 | [../vscode/typesharp/package.json](../vscode/typesharp/package.json)이 `typesharp` language id와 `.tysh` extension을 등록하고, [../vscode/typesharp/syntaxes/typesharp.tmLanguage.json](../vscode/typesharp/syntaxes/typesharp.tmLanguage.json)이 [grammar/lexical.md](grammar/lexical.md) 기반 TextMate syntax highlighting을 제공한다. |
| LSP diagnostics | 통과 | `src/TypeSharp.LanguageServer`가 `textDocument/didOpen`/`didChange` 문서 text를 compiler parser, binder, type checker로 검사하고 LSP zero-based `textDocument/publishDiagnostics` notification을 내보내며 smoke tests가 `TS2201` publish path를 검증한다. |
| LSP hover | 통과 | `src/TypeSharp.LanguageServer`가 `hoverProvider` capability와 `textDocument/hover` request를 처리하고, open document의 identifier 위치를 compiler parser/binder symbol로 해석해 markdown hover content와 zero-based range를 반환하는 smoke test가 검증한다. |
| LSP go-to-definition | 통과 | `src/TypeSharp.LanguageServer`가 `definitionProvider` capability와 `textDocument/definition` request를 처리하고, hover와 공유하는 open document symbol lookup으로 bound source symbol 선언 위치를 LSP Location으로 반환하는 smoke test가 검증한다. |
| LSP basic completion | 통과 | `src/TypeSharp.LanguageServer`가 `completionProvider` capability와 `textDocument/completion` request를 처리하고, open document binder symbols, built-in types, core keywords를 deterministic completion items로 반환하는 smoke test가 검증한다. |
| Parser fixture format | 통과 | [parser-fixtures.md](parser-fixtures.md)가 `tests/fixtures/parser/` layout, positive/negative fixture 예, CLI JSON diagnostics snapshot, syntax tree snapshot 정책을 정의한다. |
| Compiler/CLI skeleton | 통과 | `src/TypeSharp.Compiler`, `src/TypeSharp.Cli`, `tests/TypeSharp.Compiler.Tests`가 host-side compiler/CLI skeleton과 `typesharp version` smoke path를 제공한다. |
| Manifest/source discovery | 통과 | `TypeSharpManifestLoader`, `TypeSharpManifestLocator`, `SourceDiscovery`가 `TypeSharp.toml` load, parent search, default `src`, deterministic `.tysh` discovery, generated/build folder exclusion을 제공하고 smoke tests가 검증한다. |
| Minimal lexer/parser | 통과 | `TypeSharpLexer`, `TypeSharpParser`, parser fixtures, and package-free smoke tests cover the first stable grammar subset and `TS1001` recovery. |
| Parser modules/records coverage | 통과 | `tests/fixtures/parser/positive/0002-modules-records`가 type alias, record declaration, named arguments, record update syntax를 snapshot으로 검증한다. |
| Parser unions/patterns coverage | 통과 | `tests/fixtures/parser/positive/0003-unions-patterns`가 named import, union declaration, generic type parameters/type arguments, match arms, union case patterns를 snapshot으로 검증한다. |
| Parser structural/narrowing coverage | 통과 | `tests/fixtures/parser/positive/0004-structural-narrowing`가 type-level union alias, record shape type, `unknown` type name, match type patterns, record patterns, null-coalescing expression을 snapshot으로 검증한다. |
| Parser async/result interop coverage | 통과 | `tests/fixtures/parser/positive/0005-async-result-interop`가 type-only import, exported async function, nested generic `Task` return type, `try`/typed `catch`, `using`, `await`, block match arms를 snapshot으로 검증한다. |
| Parser public API coverage | 통과 | `tests/fixtures/parser/positive/0006-public-api`가 attribute list, public/private modifier, delegate, class, property-like `let` member, accessor, event, `elif`, assignment, decimal suffix literal을 snapshot으로 검증한다. |
| Parser pipeline/collections coverage | 통과 | `tests/fixtures/parser/positive/0007-pipeline-collections`가 exported `let`, `for`, fractional decimal literal, array type, collection literal, lambda, function type annotation, named argument, `|>` pipeline expression을 snapshot으로 검증한다. |
| Parser C# library interop coverage | 통과 | `tests/fixtures/parser/positive/0008-csharp-library-interop`가 literal declaration, call-site `out`, chained member/indexer/call expression, named arguments, collection literal argument, loop and match syntax를 snapshot으로 검증한다. |
| Parser literals/attributes coverage | 통과 | `tests/fixtures/parser/positive/0009-literals-attributes`가 public literal declaration, attribute list, attribute constructor argument, expression-bodied call with named arguments를 snapshot으로 검증한다. |
| Parser public boundary contract coverage | 통과 | `tests/fixtures/parser/positive/0010-public-boundary-contract`가 public boundary-oriented shape aliases, type-level union aliases, nominal public record/union declarations, public factory functions, and record shorthand call arguments를 snapshot으로 검증한다. |
| Parser capability boundaries coverage | 통과 | `tests/fixtures/parser/positive/0011-capability-boundaries`가 contextual `dynamic`, `reflect`, `interop`, `extern` function modifiers, attribute-prefixed native interop declarations, and exported capability-marked functions를 snapshot으로 검증한다. |
| Diagnostics taxonomy | 통과 | [diagnostics.md](diagnostics.md)가 diagnostic code range, descriptor metadata, explanation surface, golden diagnostic fixture policy를 정의하고 `DiagnosticDescriptors` registry smoke test가 현재 code set을 고정한다. |
| Binder name resolution skeleton | 통과 | `TypeSharpBinder`가 parse-clean source의 namespace/import/type/function/value/local/parameter symbol skeleton을 만들고 `TS2001` unresolved type/value diagnostics를 smoke tests로 검증한다. |
| Binder diagnostic fixtures | 통과 | `BinderFixtureConventions`와 `tests/fixtures/diagnostics/binder` positive/negative fixtures가 `TS2001` golden diagnostics JSON을 package-free smoke harness로 검증한다. |
| Type checker basic mismatch skeleton | 통과 | `TypeSharpTypeChecker`가 parse-clean/bind-clean source에서 primitive literal/reference/call 기반 explicit annotation mismatch를 `TS2201`로 보고하고 smoke tests가 검증한다. |
| Type checker diagnostic fixtures | 통과 | `TypeCheckerFixtureConventions`와 `tests/fixtures/diagnostics/type-checker` positive/negative fixtures가 `TS2201` golden diagnostics JSON을 package-free smoke harness로 검증한다. |
| Compile-time literal lowering | 통과 | `CSharpSourceBackend`가 top-level `literal` declarations를 generated C# constant fields로 emit하고, `tests/fixtures/backend/csharp/positive/0007-literal-constants` snapshot과 `CLI build compiles literal constants` smoke가 generated `net48` assembly build 및 C# `net48` consumer의 public literal field 참조를 검증한다. |
| Basic semantics generated build | 통과 | `tests/fixtures/backend/csharp/positive/0008-basic-semantics` snapshot과 `CLI build compiles basic semantics` smoke가 primitive string/int/bool literals, local `let`, generated function declaration, and generated function call을 C# 7.3 source로 낮추고 `net48` assembly로 빌드하는 경로를 검증한다. |
| Module namespace generated build | 통과 | `module` keyword와 `ModuleDeclaration` parser support를 추가하고, binder/type checker가 module member declaration을 순회하며, `tests/fixtures/backend/csharp/positive/0009-module-namespace` snapshot과 `CLI build compiles module namespace` smoke가 namespace 안의 module을 generated C# static class로 낮춰 `net48` assembly 및 C# `net48` consumer call을 검증한다. |
| C# source backend first golden output | 통과 | `CSharpSourceBackend`가 parse-clean/bind-clean/type-check-clean namespace와 exported string-returning function을 C# 7.3-compatible block namespace/static class/method source로 생성하고 `tests/fixtures/backend/csharp/positive/0001-string-return` snapshot이 검증한다. |
| C# import backend skeleton | 통과 | `CSharpSourceBackend`가 TypeSharp named/type imports를 deterministic C# `using` directives로, static imports를 `using static` directives로 중복 제거해 emit하고 `tests/fixtures/backend/csharp/positive/0002-import-directives` snapshot이 검증한다. |
| C# call expression backend skeleton | 통과 | `CSharpSourceBackend`가 identifier, member access, and simple positional call expressions를 C# expression source로 emit하고 `tests/fixtures/backend/csharp/positive/0003-call-expression` snapshot이 imported `Regex.IsMatch(...)` output을 검증한다. |
| C# block/local backend skeleton | 통과 | `CSharpSourceBackend`가 block-bodied functions, local `let` declarations as `var`, simple expression statements, and final expression returns를 emit하고 `tests/fixtures/backend/csharp/positive/0004-block-local` snapshot이 imported `StringBuilder` constructor and instance calls output을 검증한다. |
| Generated C# net48 compile smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 backend-generated C# source를 temporary SDK-style `net48` C# project에 쓰고 offline `dotnet build`로 컴파일해 generated source가 C# compiler에 수용되는지 검증한다. |
| Runtime net48 skeleton | 통과 | `src/TypeSharp.Runtime`가 `net48` SDK-style class library로 추가되고 `TypeSharp.Runtime.TypeSharpRuntimeInfo`가 ABI 0 placeholder를 제공하며 `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`가 통과한다. |
| Runtime ABI versioning policy | 통과 | [runtime-abi.md](runtime-abi.md)가 compiler/runtime `RuntimeAbiVersion` alignment, ABI change rules, compatibility gates, release policy를 정의하고 smoke test가 compiler/runtime ABI constants alignment를 검증한다. |
| Nominal union runtime helper | 통과 | `src/TypeSharp.Runtime/TypeSharpUnion.cs`가 generated nominal union case class용 `ITypeSharpUnionCase`와 `TypeSharpUnion` helper를 제공하고, smoke test가 tag, case name, payload, equality, hash behavior를 검증한다. |
| Pattern matching runtime helper | 통과 | `src/TypeSharp.Runtime/TypeSharpPattern.cs`가 generated pattern matching lowering용 case/payload predicate와 payload extraction helper를 제공하고, smoke test가 payload and payload-free union case matching을 검증한다. |
| Equality/hash runtime helper | 통과 | `src/TypeSharp.Runtime/TypeSharpEquality.cs`가 generated record/union case lowering용 equality, sequence equality, and hash composition helper를 제공하고, smoke test가 deterministic equality/hash behavior를 검증한다. |
| Async runtime helper | 통과 | `src/TypeSharp.Runtime/TypeSharpAsync.cs`가 generated async lowering용 completed/faulted `Task` helper를 제공하고, smoke test가 generic and non-generic task behavior를 검증한다. |
| Dependency inventory and net48 API audit | 통과 | [dependencies.md](dependencies.md)가 generated project, `TypeSharp.Core`, `TypeSharp.Runtime`, compiler, CLI, test host dependency inventory를 기록하고, `tests/TypeSharp.Compiler.Tests`가 Core/Runtime artifacts의 `net48`, package-free, obvious .NET 5+ or package-backed API drift를 smoke test로 검증한다. |
| CLI run generated net48 executable | 통과 | `TypeSharpBuilder`가 executable project에 generated C# entry point를 추가하고 `.exe` output path를 보고하며, `typesharp run`이 build 후 generated `net48` executable을 실행하는 smoke test가 검증한다. Entry point는 `main(): string`/`main(): int`와 `main(args: string[]): string`/`main(args: string[]): int` smoke path를 지원하고, unsupported main parameter signature는 `TS3500`으로 emission 전에 보고한다. |
| CLI build generated C# emission | 통과 | `TypeSharpBuilder`와 `typesharp build`가 clean 프로젝트를 검사한 뒤 manifest의 generated output root 아래 deterministic `.g.cs` 파일을 쓰고, diagnostics가 있으면 emission 전에 중단하는 smoke tests가 검증한다. `TS2201` type checker diagnostic smoke는 generated source/project/assembly가 생기지 않는지 확인한다. |
| CLI build net48 project scaffold | 통과 | `TypeSharpBuilder`와 `typesharp build`가 generated output root 아래 deterministic SDK-style `net48` C# project scaffold를 `.g.cs` source와 함께 쓰고 CLI output으로 project scaffold path를 보고하는 smoke tests가 검증한다. |
| CLI build generated net48 assembly | 통과 | `TypeSharpBuilder`와 `typesharp build`가 generated SDK-style `net48` C# project를 offline-friendly `dotnet build`로 컴파일하고 CLI output으로 generated assembly path를 보고하며, `TS3501` build-failure descriptor와 generated DLL existence smoke tests가 검증한다. |
| C# consumes generated TypeSharp assembly | 통과 | `tests/TypeSharp.Compiler.Tests`가 `typesharp build`로 만든 generated TypeSharp `net48` DLL을 temporary C# SDK-style `net48` project에서 `<Reference>`로 참조하고 generated `Module.greeting()` public API 호출을 컴파일해 검증한다. |
| ASP.NET/WCF/worker host compatibility smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 `typesharp build`로 만든 generated `net48` DLL과 `TypeSharp.Core`/`TypeSharp.Runtime` DLL을 ASP.NET Web Forms-style `System.Web.UI.Page`, WCF `ServiceContract`/`OperationContract`, Windows Service-style `ServiceBase` consumer projects에서 참조하고 각 project build를 검증한다. |
| CLI build generated project reference propagation | 통과 | `TypeSharpBuilder`가 manifest framework assembly와 local DLL references를 generated C# project `<Reference>` items로 emit하고 local DLL `HintPath`를 generated output root 기준 상대 경로로 기록하며 smoke test가 generated build success와 missing-reference stop behavior를 검증한다. |
| C# framework static member call smoke | 통과 | `typesharp build`가 `System.Core` manifest reference와 `import { Regex } from "System.Text.RegularExpressions"` source를 generated project로 낮추고 `Regex.IsMatch(...)` static call이 `net48` generated DLL로 컴파일되는지 smoke test가 검증한다. |
| C# local DLL static member call smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 temporary `Legacy.Tools` `net48` DLL을 만들고 manifest `references.paths`와 `import { LegacyApi } from "Legacy.Tools"` source를 generated project로 낮춰 `LegacyApi.Echo(...)` static call이 컴파일되는지 검증한다. |
| C# constructor and instance member call smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyFormatter` class를 local `net48` DLL에 만들고 TypeSharp source의 `LegacyFormatter("legacy:")` construction과 `formatter.Format("value")` instance call이 generated `net48` project에서 컴파일되는지 검증한다. |
| C# property access smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyFormatter.Prefix` property를 local `net48` DLL에 만들고 TypeSharp source의 `formatter.Prefix` property read가 generated `net48` project에서 컴파일되는지 검증한다. |
| C# byref and params interop aggregate | 통과 | `tests/TypeSharp.Compiler.Tests`의 `CLI build compiles imported params/out/in/ref call` smokes가 call-site emission과 generated `net48` build를 검증하고, `checker reports invalid byref interop diagnostics` 및 `CLI build stops before emission on invalid byref interop`가 `TS2403` mismatch diagnostic과 emission stop path를 검증한다. |
| C# params call smoke | 통과 | `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyParams.Join(string, params string[])` method를 local `net48` DLL에 만들고 TypeSharp source의 `LegacyParams.Join(",", "a", "b", "c")` call이 generated `net48` project에서 컴파일되는지 검증한다. |
| C# out call smoke | 통과 | `CSharpSourceBackend`가 parsed `out` argument를 `out value` call-site로 emit하고 `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyByRef.TryParseCount(string, out int)` method를 local `net48` DLL에 만들어 generated `net48` project compile smoke로 검증한다. |
| C# in call smoke | 통과 | parser/binder/backend가 parsed `in` argument를 `in value` call-site로 보존하고 `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyByRef.AddOne(in int)` method를 local `net48` DLL에 만들어 generated `net48` project compile smoke로 검증한다. |
| C# ref call smoke | 통과 | lexer/parser/binder/backend가 parsed `ref` argument를 `ref value` call-site로 보존하고 `tests/TypeSharp.Compiler.Tests`가 temporary `LegacyByRef.Increment(ref int)` method를 local `net48` DLL에 만들어 generated `net48` project compile smoke로 검증한다. |
| TypeSharp.Core Option/Result skeleton | 통과 | `src/TypeSharp.Core`가 `net48` SDK-style class library로 추가되고 `Option<T>`, `Result<T,E>`, `Unit` public surface와 Some/None, Ok/Error state smoke tests가 검증한다. |
| TypeSharp.Core Option/Result generated API | 통과 | `CSharpSourceBackend`와 simple type checker가 generic type annotations를 보존하고, `tests/fixtures/backend/csharp/positive/0010-core-option-result-api` snapshot과 `CLI build compiles core option result APIs` smoke가 `Option<string>` 및 `Result<int,string>` pass-through public API를 generated `net48` assembly와 C# `net48` consumer에서 검증한다. |
| Generic function generated API | 통과 | `TypeSharpBinder`가 function type parameter를 signature/body scope에 등록하고 `CSharpSourceBackend`가 generic method signature를 emit하며, `tests/fixtures/backend/csharp/positive/0011-generic-function-api` snapshot과 `CLI build compiles generic function API` smoke가 `identity<T>(T)` public API를 generated `net48` assembly와 C# `net48` consumer에서 검증한다. |
| Class declaration generated API | 통과 | `CSharpSourceBackend`가 top-level `class` declarations를 generated C# class로 emit하고 class `fun` members를 instance methods로 낮추며, `tests/fixtures/backend/csharp/positive/0012-class-declaration-api` snapshot과 `CLI build compiles class declaration API` smoke가 generated `net48` assembly 및 C# `net48` consumer construction/call을 검증한다. |
| Interface declaration generated API | 통과 | `TypeSharpLexer`/`TypeSharpParser`가 `interface` declarations와 body-less `fun` signatures를 읽고, `CSharpSourceBackend`가 C# 7.3-compatible interface signatures를 emit하며, `tests/fixtures/parser/positive/0012-interface-declaration`, `tests/fixtures/backend/csharp/positive/0013-interface-declaration-api`, and `CLI build compiles interface declaration API`가 generated `net48` interface와 C# `net48` implementation을 검증한다. |
| Generic type declaration generated API | 통과 | `TypeSharpBinder`가 type declaration type parameters를 nested member signature scope에 등록하고, `tests/fixtures/backend/csharp/positive/0014-generic-type-declaration-api` snapshot과 `CLI build compiles generic type declaration API` smoke가 generated generic C# class와 C# `net48` consumer construction/call을 검증한다. |
| Immutable record generated API | 부분 통과 | `CSharpSourceBackend`가 top-level `record` declarations를 sealed C# class로 emit하고 constructor, get-only properties, value equality, and hash overrides를 생성한다. `tests/fixtures/backend/csharp/positive/0015-immutable-record-api` snapshot과 `CLI build compiles immutable record API` smoke가 generated `net48` assembly 및 C# `net48` consumer construction/property/equality/hash calls를 검증한다. Copy/update lowering은 아직 남아 있다. |
| C# reference resolver skeleton | 통과 | `TypeSharpReferenceResolver`가 manifest framework assembly names와 local DLL paths를 deterministic `ResolvedReference` records로 정규화하고 missing local DLL을 `TS2401`로 보고하는 smoke tests가 검증한다. |
| C# metadata reader skeleton | 통과 | `TypeSharpMetadataReader`가 `ReferenceResolutionResult`를 소비해 framework/local references를 deterministic `MetadataAssemblySymbol` placeholders로 변환하고 reference diagnostics와 missing local metadata input diagnostics를 보존하거나 보고하는 smoke tests가 검증한다. |
| C# metadata local public symbol index | 통과 | `TypeSharpMetadataReader`가 local `net48` DLL의 public top-level type, method, property, parameter, byref modifier metadata를 `MetadataAssemblySymbol.Types`로 index하고 smoke test가 `Legacy.Tools` fixture의 `params`/`out`/`in`/`ref` metadata를 검증한다. |
| C# metadata params parameter flag | 통과 | `TypeSharpMetadataReader`가 local `net48` DLL parameter의 `System.ParamArrayAttribute`를 읽어 `MetadataParameterSymbol.IsParams`로 보존하고 smoke test가 `LegacyParams.Join(string, params string[])`의 second parameter를 검증한다. |
| C# metadata pipeline integration | 통과 | `TypeSharpChecker`와 `TypeSharpBuilder`가 reference resolver와 metadata reader diagnostics를 source parse 전에 diagnostics pipeline에 포함하고, missing local DLL의 `TS2401`이 CLI check/build JSON diagnostics와 build emission stop smoke tests로 검증된다. |
| C# invalid byref interop diagnostic | 통과 | `TypeSharpInteropValidator`가 local metadata index와 parsed call-site를 비교해 `ref`/`out`/`in` modifier mismatch를 `TS2403`으로 보고하고 `typesharp build`가 generated C# emission 전에 중단하는 smoke tests가 검증한다. |
| C# ambiguous overload diagnostic | 통과 | `TypeSharpInteropValidator`가 local metadata index에서 같은 arity의 imported static method 후보가 여러 개일 때 `TS2402`를 보고하고 `typesharp build`가 generated C# emission 전에 중단하는 smoke tests가 검증한다. |
| C# exact overload match smoke | 통과 | `TypeSharpInteropValidator`가 literal/primitive argument type을 사용해 exact metadata overload 후보 하나를 선택하고, `LegacyOverloads.Pick("value")`가 `Pick(string)` 후보로 좁혀져 generated `net48` project로 컴파일되는 smoke test가 검증한다. |
| C# expanded params overload validation | 통과 | `TypeSharpInteropValidator`가 trailing `params` metadata를 expanded arity 후보로 취급하고, `LegacyParamsOverloads.Pick(",", "a", "b")`는 `params string[]` exact 후보로 좁히며 `LegacyParamsOverloads.Pick(",", null, null)`은 `TS2402` ambiguity로 보고하는 smoke tests가 검증한다. |
| C# optional parameter overload validation | 통과 | `TypeSharpMetadataReader`가 local `net48` DLL parameter default metadata를 `MetadataParameterSymbol.IsOptional`으로 보존하고, `TypeSharpInteropValidator`가 omitted optional arguments를 overload applicability에 포함해 `LegacyOptional.Format("hello")`는 generated `net48` project로 컴파일하며 `LegacyOptionalOverloads.Pick("value")`는 `TS2402` ambiguity로 보고하는 smoke tests가 검증한다. |
| C# named argument overload validation | 통과 | `CSharpSourceBackend`가 `NamedArgument`를 C# `name: value` syntax로 emit하고, `TypeSharpInteropValidator`가 metadata parameter names로 overload 후보를 필터링해 `LegacyNamedOverloads.Route("/orders", controller: "Orders")`가 generated `net48` project로 컴파일되는 smoke test가 검증한다. |
| C# unknown nullability diagnostic | 통과 | `TypeSharpMetadataReader`가 nullable annotation 없는 local `net48` C# reference return을 `MetadataNullabilityKind.Unknown`으로 표시하고, strict nullable mode에서 `LegacyApi.Echo("value")` call을 `TS2404` warning으로 보고하는 smoke test가 검증한다. |
| C# delegate lambda call smoke | 통과 | `CSharpSourceBackend`가 single-parameter lambda expression을 C# `parameter => expression`으로 emit하고, temporary `LegacyDelegates.Apply(string, Func<string,string>)` local DLL call이 generated `net48` project로 컴파일되는 smoke test가 검증한다. |
| C# event add/remove call smoke | 통과 | lexer/parser/backend가 `+=` and `-=` event assignment expressions를 보존하고, temporary `LegacyEvents.Transform` event에 lambda handler를 add/remove한 뒤 generated `net48` project로 컴파일되는 smoke test가 검증한다. |
| CLI check parse diagnostics | 통과 | `TypeSharpChecker`와 `typesharp check`가 manifest/source discovery/parser diagnostics를 실행하고 text/JSON diagnostics와 exit code를 smoke tests로 검증한다. |
| CLI check type checker diagnostics | 통과 | `TypeSharpChecker`와 `typesharp check`가 parse/bind-clean source의 `TS2201` type mismatch를 JSON diagnostics로 보고하고 CLI exit code 1을 반환하는 smoke test가 검증한다. |
| 실현 가능성 검토 | 통과 | [feasibility.md](feasibility.md)가 compiler host, C# source backend, union representation, public ABI boundary를 현실적인 MVP로 낮춘다. |
| 문법 커버리지 | 통과 | [grammar/coverage.md](grammar/coverage.md)가 TypeScript, F#, C# 기능의 직접 지원/대체/계획/거절 상태를 추적한다. |
| 남은 미결정 분리 | 통과 | [goal.md](goal.md)와 [README.md](README.md)가 열린 결정을 명시한다. |

## 다음 반복의 입력

다음 단계에서 문서를 더 구체화하려면 아래 결정 중 하나를 먼저 내려야 한다.

- IL backend 도입 시점과 범위
- grammar coverage: 새로 발견되는 TypeScript/F#/C# 기능의 Direct/Equivalent/Replacement/Planned/Experimental/Rejected 분류
- C# library interop 구현 범위 확정: framework assembly, local DLL, overload, nullable, public ABI fixture.
- nominal union optimization: tagged struct 또는 generated closed type 도입 시점
- type-level union public boundary diagnostic과 수동 nominal union/interface/wrapper 가이드
- manifest 형식: `TypeSharp.toml` 유지 또는 MSBuild 1급 통합
- VS Code extension/LSP packaging strategy
- CLI command implementation order and project manifest validation

