# TypeSharp 권장 아키텍처

문서 기준일: 2026-05-19

이 문서는 TypeSharp 구현을 시작할 때 권장하는 구조를 제안한다. 목표는 .NET Framework 4.8 호환 산출물과 런타임을 만들면서 최신 언어 기능을 안정적으로 낮출 수 있는 컴파일러와 도구를 만드는 것이다.

## 전체 구조

```text
source files
  -> lexer/parser
  -> syntax tree
  -> binder/name resolver
  -> semantic model
  -> type checker/inference
  -> lowering
  -> TypeSharp IR
  -> backend
       -> C# 7.3-compatible source for MVP
       -> IL assembly for net48 later
  -> runtime library
  -> .NET Framework 4.8 execution
```

## Host Target Policy

실현 가능성 결정은 [feasibility.md](feasibility.md)를 따른다.

- 생성 assembly와 TypeSharp 표준 런타임은 `net48`에서 실행되어야 한다.
- compiler, CLI, language server host는 현대 .NET LTS 기반으로 구현할 수 있다.
- host가 `net48`에서 실행되지 않아도 실패가 아니다. 실패 기준은 생성 산출물이 `net48`에서 실행되지 않는 경우다.
- host/runtime/source backend는 프로젝트를 분리해 dependency가 섞이지 않게 한다.

## 권장 프로젝트 분리

```text
src/
  TypeSharp.Compiler/
  TypeSharp.Runtime/
  TypeSharp.Cli/
  TypeSharp.LanguageServer/
tests/
  TypeSharp.Compiler.Tests/
  TypeSharp.Runtime.Tests/
  TypeSharp.Interop.Tests/
  TypeSharp.GoldenTests/
samples/
docs/
```

현재 저장소에는 초기 `TypeSharp.Compiler`, `TypeSharp.Cli`, `TypeSharp.Core`, `TypeSharp.Runtime`, `TypeSharp.LanguageServer`, `TypeSharp.Compiler.Tests`, `vscode/typesharp` skeleton이 있다. interop tests, golden tests는 후속 작업으로 추가한다.

## Compiler Core

### Lexer와 Parser

권장사항:
- 문법의 기준 문서는 [grammar/README.md](grammar/README.md)와 하위 문서로 둔다.
- 손으로 작성한 recursive descent parser 또는 parser generator 중 하나를 초기에 결정한다.
- syntax tree는 trivia, source span, 오류 복구 정보를 보존한다.
- formatter와 language server가 syntax tree를 재사용할 수 있어야 한다.
- parser 구현 전에 [grammar/coverage.md](grammar/coverage.md)의 Direct, Equivalent, Replacement 항목이 lexical/declaration/type/expression/pattern 문서에 실제 syntax 또는 대체 문법으로 연결되어 있는지 확인한다.

필수 계약:
- 잘못된 코드에서도 가능한 많은 diagnostics를 낸다.
- parser 단계에서 semantic 판단을 하지 않는다.
- 문법 ambiguity는 최소화한다.
- TextMate grammar와 LSP semantic token이 compiler grammar와 다른 언어처럼 갈라지지 않게 한다.

### Binder와 Symbol Model

권장사항:
- source symbol과 metadata symbol을 같은 interface 계층으로 다룬다.
- namespace, module, type, member, local scope를 분리한다.
- overload set은 symbol resolution 결과로 보존하고 type checker가 최종 선택한다.

필수 계약:
- .NET assembly reference를 읽을 수 있어야 한다.
- TypeSharp public symbol이 .NET metadata로 어떻게 나갈지 추적해야 한다.
- import/wildcard/open 규칙은 deterministic해야 한다.

### Type Checker

권장사항:
- nominal .NET type system 위에 structural constraint layer와 type-level union layer를 얹는다.
- nullability는 type annotation의 일부로 다룬다.
- type inference는 local expression graph부터 구현한다.

필수 계약:
- generic constraint, overload resolution, variance, nullable, nominal union exhaustiveness, type-level union narrowing을 단계적으로 지원한다.
- diagnostics는 source span과 관련 symbol 정보를 포함한다.
- 추론 실패는 명시 annotation 제안을 제공한다.

### Lowering과 IR

권장사항:
- TypeSharp 고수준 AST와 backend-friendly IR을 분리한다.
- feature lowering은 독립 pass로 구성하고 테스트한다.
- IR은 .NET Framework에서 표현 가능한 개념만 포함한다.

필수 계약:
- 모든 MVP 기능은 lowering 문서와 golden test를 가져야 한다.
- preview 기능은 feature gate를 거쳐 lowering된다.
- lowering 결과는 deterministic해야 한다.

## Backend 전략

### 선택지 A: C# 7.3 호환 Source Generation

MVP 결정:
- 첫 backend는 C# 7.3 compatible source generation으로 구현한다.
- generated C# source는 `net48` 프로젝트에서 컴파일 가능해야 한다.
- generated source와 TypeSharp source span mapping을 초기부터 설계한다.

장점:
- .NET Framework 기본 C# 타깃과 잘 맞는다.
- async, iterator, debug info를 C# compiler에 위임할 수 있다.
- 초기 MVP를 빠르게 검증할 수 있다.

단점:
- 일부 최신 기능 lowering이 장황해질 수 있다.
- 생성 코드 품질과 diagnostics mapping을 별도로 관리해야 한다.
- C# compiler behavior에 의존한다.

### 선택지 B: 직접 IL Emit

장점:
- C# 언어 버전에 묶이지 않는다.
- public metadata를 세밀하게 제어할 수 있다.
- 장기적으로 빠르고 명확한 compiler architecture가 된다.

단점:
- async state machine, debug info, generic metadata, sequence point 구현 비용이 크다.
- 초기 개발 속도가 느릴 수 있다.

### 선택지 C: Backend Abstraction

권장:
- MVP는 C# 7.3 source backend로 시작하고, IR과 테스트를 직접 IL backend로 확장 가능하게 둔다.
- public API와 metadata fidelity가 중요한 기능부터 IL backend를 도입한다.

## Runtime Library

필수 구성:
- `TypeSharp.Core.Option<T>`
- `TypeSharp.Core.Result<T, E>`
- reference-type nominal union tag/value helper
- structural equality/hash helper
- pattern matching helper
- async/task helper

권장사항:
- runtime library는 작고 안정적이어야 한다.
- 사용자 코드가 직접 참조하는 핵심 namespace는 `TypeSharp.Core`와 `TypeSharp.Collections`로 시작한다.
- compiler-generated helper는 `TypeSharp.Runtime`에 격리한다.
- compiler-generated code가 의존하는 API는 versioning 정책을 가져야 한다.
- public API와 compiler internal API를 분리한다.
- MVP nominal union representation은 abstract base class + sealed case class 계열로 시작한다. tagged struct representation은 Stable Backlog다.

## .NET Interop Layer

필수 구성:
- reference resolver for framework assembly and local DLL
- assembly reference loader
- metadata symbol reader
- metadata-to-TypeSharp type mapper
- attribute reader/writer
- C# overload resolver input model
- C#-friendly public API generation
- `Task`/`Task<T>` async interop

권장사항:
- nullable annotation이 없는 assembly는 "unknown"으로 표시한다.
- imported C# symbol과 TypeSharp source symbol은 binder 이후 같은 semantic model에서 조회되어야 한다.
- `ref`/`out`/`in`, optional parameter, `params`, generic constraint, delegate conversion은 interop test fixture를 가진다.
- local DLL reference는 deterministic path resolution과 duplicate assembly diagnostic을 가져야 한다.
- NuGet package resolution은 restore, lock file, license inventory 정책이 준비될 때까지 Stable Backlog로 둔다.
- COM/dynamic/reflection interop는 strict mode에서 경고를 낸다.
- CLS compliance analyzer를 장기 목표로 둔다.

권장 pipeline:

```text
TypeSharp.toml references
  -> reference resolver
  -> metadata reader
  -> metadata symbol table
  -> binder/name resolver
  -> overload/type checker
  -> public ABI checker
  -> C# source backend or IL backend
```

## Tooling

VS Code와 CLI는 TypeSharp의 1급 산출물이다. compiler core는 CLI와 language server가 같은 parser, binder, type checker, diagnostics model을 공유하도록 분리해야 한다.
Diagnostic code range, descriptor metadata, and explanation surface는 [diagnostics.md](diagnostics.md)를 따른다.

### CLI

세부 command contract는 [cli.md](cli.md)를 기준으로 한다.

MVP command:
- `typesharp version`
- `typesharp check <project>`
- `typesharp build <project>`
- `typesharp run <project>`

추가 command:
- `typesharp format`
- `typesharp new`
- `typesharp test`
- `typesharp explain <diagnostic-code>`

Experimental 또는 Stable Backlog:
- `typesharp test`
- `typesharp build --emit il`

### Language Server

권장 순서:
1. diagnostics
2. hover
3. go-to-definition
4. completion
5. rename
6. code action

Language server는 compiler semantic model을 별도 구현하지 않고 공유해야 한다.

현재 language server skeleton:
- `src/TypeSharp.LanguageServer`는 modern .NET host로 실행되고 `TypeSharp.Compiler`를 참조한다.
- 최소 stdio JSON-RPC framing으로 `initialize`, `shutdown`, `exit`, `textDocument/didOpen`, `textDocument/didChange`를 처리한다.
- open document diagnostics는 compiler parser, binder, type checker를 재사용한다.
- compiler diagnostics는 LSP zero-based range, severity, source, code, message로 변환되어 `textDocument/publishDiagnostics` notification으로 전송된다.
- workspace manifest discovery, project reference-aware diagnostics, hover, go-to-definition, completion은 후속 LSP 작업으로 남긴다.

### VS Code Extension

MVP 기능:
- syntax highlighting
- diagnostics
- hover
- go-to-definition
- basic completion

VS Code extension은 Language Server Protocol client로 시작하고, 문법 색상화는 TextMate grammar 또는 semantic token의 최소 구현으로 제공한다.
TextMate grammar는 [grammar/lexical.md](grammar/lexical.md)의 토큰과 keyword 분류를 첫 입력으로 삼고, semantic token은 compiler semantic model에서 보강한다.

현재 scaffold:
- `vscode/typesharp/package.json` registers language id `typesharp` and extension `.tysh`.
- `vscode/typesharp/language-configuration.json` defines comments, brackets, pairs, and word pattern.
- `vscode/typesharp/syntaxes/typesharp.tmLanguage.json` provides lexical TextMate highlighting for comments, strings, numeric literals, attributes, keywords, primitive types, and operators.
- LSP diagnostics, hover, go-to-definition, and completion remain open and must share compiler diagnostics and semantic model with the CLI.

## 프로젝트 파일

초기 manifest 예시:

```toml
[project]
name = "Sample"
targetFramework = "net48"
outputType = "exe"

[language]
version = "preview"
strict = true
nullable = "strict"

[references]
assemblies = [
  "System",
  "System.Core"
]
```

결정 필요:
- MSBuild와 직접 통합할지, 독립 manifest를 먼저 둘지 결정해야 한다.
- NuGet restore와 reference resolution을 어느 레이어에서 처리할지 결정해야 한다.

## 테스트 전략

필수 테스트:
- parser fixture
- binder/name resolution fixture
- type checker positive/negative fixture
- diagnostic golden test following [diagnostics.md](diagnostics.md)
- lowering golden test
- emitted assembly smoke test
- C# interop test
- runtime library unit test

권장 테스트:
- random syntax fuzzing
- type inference stress test
- public API compatibility snapshot
- performance benchmark
- debugger/source mapping smoke test

## 버전 관리 정책

권장:
- `0.x`: 사양과 구현이 함께 변하는 실험 단계
- `1.0`: MVP 기능, diagnostics code, runtime ABI 안정화
- feature gate: `stable`, `preview`, `experimental`
- compatibility note: 모든 breaking change는 migration guide를 가져야 한다.

## 리스크와 대응

| 리스크 | 영향 | 대응 |
| --- | --- | --- |
| .NET Framework metadata 제약 | 최신 타입 기능 표현이 어려움 | compile-time only 기능과 public ABI 기능을 분리한다. |
| structural type과 overload 충돌 | 예측하기 어려운 호출 해석 | 명시 annotation 요구와 resolution 우선순위를 사양화한다. |
| nominal union representation 최적화 지연 | 성능 최적화 지연 | MVP는 reference class hierarchy로 고정하고 tagged struct는 성능 검증 뒤 도입한다. |
| type-level union이 public ABI로 새는 문제 | C# interop와 metadata 표현이 불안정해짐 | MVP는 public boundary diagnostic을 내고 명시 wrapper/interface 작성 가이드를 제공한다. |
| C# source backend diagnostics mapping | 사용자 경험 저하 | source map과 generated code span mapping을 초기부터 설계한다. |
| 프리뷰 기능 과다 채택 | 언어 안정성 저하 | feature gate와 Preview Watch 분류를 강제한다. |

