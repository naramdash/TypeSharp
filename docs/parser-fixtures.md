# Parser Fixture Format

문서 기준일: 2026-05-18

이 문서는 TypeSharp parser 구현 전에 고정해야 하는 fixture layout, golden diagnostic 형식, syntax tree snapshot 정책을 정의한다. 목표는 compiler skeleton이 생겼을 때 문서 예제를 바로 parser test로 승격할 수 있게 만드는 것이다.

## 위치 결정

Parser fixture는 구현 테스트 자산이므로 `tests/fixtures/parser/` 아래에 둔다.

```text
tests/
  fixtures/
    parser/
      positive/
        0001-hello-cli/
          input.tysh
          expected.diagnostics.json
          expected.tree
          README.md
      negative/
        0001-missing-function-body/
          input.tysh
          expected.diagnostics.json
          expected.tree
          README.md
```

규칙:
- `docs/examples/*.tysh`는 사람이 읽는 설계 예제의 원본이다.
- parser fixture는 실행 가능한 test input이므로 `tests/fixtures/parser/`에 복사한다.
- fixture가 문서 예제에서 왔다면 fixture `README.md`에 원본 문서 경로를 기록한다.
- 한 fixture directory는 하나의 parser input과 그 input의 expected output만 가진다.
- 파일 이름은 `NNNN-short-kebab-name`을 사용한다. `NNNN`은 같은 category 안에서 증가한다.

## Positive Fixture

Positive fixture는 parse diagnostic이 없어야 하는 입력이다.

첫 positive 후보:

```text
tests/fixtures/parser/positive/0001-hello-cli/input.tysh
```

원본:

```text
docs/examples/01-hello-cli.tysh
```

기대 diagnostic:

```json
{
  "diagnostics": []
}
```

추가 stable grammar 후보:
- `docs/examples/02-modules-records.tysh`
- `docs/examples/03-unions-patterns.tysh`
- `docs/examples/04-structural-narrowing.tysh`
- `docs/examples/08-csharp-library-interop.tysh`

## Negative Fixture

Negative fixture는 parser recovery와 parse diagnostic을 검증하는 입력이다. Semantic error는 parser negative fixture에 넣지 않는다.

첫 negative 후보:

```text
tests/fixtures/parser/negative/0001-missing-function-body/input.tysh
```

입력:

```typesharp
namespace Samples.ParserErrors

export fun broken(name: string): string
```

기대 diagnostic:

```json
{
  "diagnostics": [
    {
      "code": "TS1001",
      "severity": "error",
      "message": "Expected function body after function signature.",
      "file": "input.tysh",
      "start": { "line": 3, "column": 40 },
      "end": { "line": 3, "column": 40 }
    }
  ]
}
```

규칙:
- Parser diagnostic code는 `TS1000`부터 `TS1099` 범위로 시작한다.
- `expected.diagnostics.json`은 [cli.md](cli.md)의 JSON diagnostics schema와 같은 shape를 사용한다.
- `file`은 fixture directory 기준 상대 경로다.
- `start`와 `end`는 1-based line/column이다.
- `end`는 half-open 위치로 해석한다. 길이 0 diagnostic은 `start`와 `end`가 같다.
- Recovery 결과도 `expected.tree`에 남겨 parser가 다음 선언으로 진행했는지 검증한다.

## Syntax Tree Snapshot

`expected.tree`는 parser가 만든 concrete syntax tree의 안정 snapshot이다.

필수 정보:
- node kind
- token kind와 raw text
- source span
- missing token 여부
- skipped token 여부
- leading/trailing trivia 요약

Span 표기:

```text
TextSpan=3:1-3:40
FullSpan=1:1-3:40
```

규칙:
- `TextSpan`은 leading/trailing trivia를 제외한 half-open span이다.
- `FullSpan`은 trivia를 포함한 half-open span이다.
- line/column은 diagnostics와 동일하게 1-based로 표기한다.
- whitespace trivia는 길이와 newline 수로 요약한다.
- comment와 doc comment trivia는 kind와 raw text를 보존한다.
- raw string, escaped identifier, interpolated string은 raw token text를 보존한다.
- parser implementation이 node 이름을 바꿀 때는 snapshot migration을 별도 변경으로 남긴다.

예:

```text
SourceFile TextSpan=3:1-3:40 FullSpan=1:1-3:40
  NamespaceDeclaration TextSpan=1:1-1:31 FullSpan=1:1-2:1
    Token NamespaceKeyword "namespace" TextSpan=1:1-1:10
    QualifiedName "Samples.ParserErrors" TextSpan=1:11-1:31
  FunctionDeclaration TextSpan=3:1-3:40 FullSpan=3:1-3:40
    Token ExportKeyword "export" TextSpan=3:1-3:7
    Token FunKeyword "fun" TextSpan=3:8-3:11
    Identifier "broken" TextSpan=3:12-3:18
    ParameterList TextSpan=3:18-3:32
    ReturnType TextSpan=3:32-3:40
    Missing FunctionBody TextSpan=3:40-3:40
```

## Test Runner Contract

초기 parser golden test runner는 다음 순서로 동작한다.

1. `tests/fixtures/parser/positive/**/input.tysh`와 `tests/fixtures/parser/negative/**/input.tysh`를 찾는다.
2. 각 input을 parser recovery enabled mode로 parse한다.
3. 실제 diagnostics를 CLI JSON diagnostics shape로 직렬화한다.
4. `expected.diagnostics.json`과 byte-for-byte 비교한다.
5. 실제 syntax tree snapshot을 `expected.tree`와 byte-for-byte 비교한다.
6. 차이가 있으면 fixture directory, relative input path, first diff를 출력한다.

정책:
- Snapshot update는 명시적인 test command option으로만 허용한다.
- Parser fixture는 bind/type/lowering diagnostic을 기대하지 않는다.
- Parser가 recovery를 했더라도 syntax tree snapshot은 성공적으로 만들어져야 한다.
- Positive fixture에 warning이 생기면 positive가 아니라 별도 diagnostic fixture로 옮긴다.

## 문서 예제 승격 순서

첫 compiler skeleton이 생기면 아래 순서로 fixture를 추가한다.

1. `docs/examples/01-hello-cli.tysh`
2. `docs/examples/02-modules-records.tysh`
3. `docs/examples/03-unions-patterns.tysh`
4. `docs/examples/04-structural-narrowing.tysh`
5. `docs/examples/08-csharp-library-interop.tysh`

이 순서는 [feasibility.md](feasibility.md)의 구현 우선순위와 [grammar/README.md](grammar/README.md)의 문법 구현 순서를 따른다.

## Current Stable Grammar Coverage

현재 compiler test harness는 positive/negative parser fixture를 byte-for-byte snapshot으로 검증한다. `ParserFixtureSnapshotsMatch`는 각 `input.tysh`를 parse하고 `expected.diagnostics.json` 및 `expected.tree`와 비교한다.

Positive fixture coverage:

| Fixture | Source | Stable grammar coverage |
| --- | --- | --- |
| `tests/fixtures/parser/positive/0001-hello-cli` | `docs/examples/01-hello-cli.tysh` | namespace, static import, exported function, nullable type, block expression, `if`, member/indexer access, null coalescing, call expression, interpolated string |
| `tests/fixtures/parser/positive/0002-modules-records` | `docs/examples/02-modules-records.tysh` | type alias, record declaration, named call arguments, nullable/array types, record update |
| `tests/fixtures/parser/positive/0003-unions-patterns` | `docs/examples/03-unions-patterns.tysh` | named imports, nominal union declarations, generic type parameter/type argument lists, `match`, union case patterns |
| `tests/fixtures/parser/positive/0004-structural-narrowing` | `docs/examples/04-structural-narrowing.tysh` | structural shape types, type-level union aliases, `unknown`, type patterns, record patterns, local narrowing-oriented match |
| `tests/fixtures/parser/positive/0005-async-result-interop` | `docs/examples/05-async-result-interop.tysh` | type-only imports, exported async functions, `Task` generic return types, `try`/typed `catch`, `using`, `await`, block-bodied match arms |
| `tests/fixtures/parser/positive/0006-public-api` | `docs/examples/06-public-api.tysh` | attribute lists, public/private modifiers, record/union/delegate/class declarations, class members, accessors, events, `elif`, assignment, decimal suffix literals |
| `tests/fixtures/parser/positive/0007-pipeline-collections` | `docs/examples/07-pipeline-collections.tysh` | pipeline expressions, array types, collection literals, exported value declarations, `for`, function type annotations, lambdas, named arguments, fractional decimal literals |
| `tests/fixtures/parser/positive/0008-csharp-library-interop` | `docs/examples/08-csharp-library-interop.tysh` | literal declarations, framework member call chains, named arguments, call-site `out`, indexers, interop-oriented `Result`/`Option` wrapper syntax |
| `tests/fixtures/parser/positive/0009-literals-attributes` | `docs/examples/09-literals-attributes.tysh` | public literal declarations, attribute lists, attribute constructor arguments, expression-bodied calls with named arguments |
| `tests/fixtures/parser/positive/0010-public-boundary-contract` | `docs/examples/10-public-boundary-contract.tysh` | public-boundary shape aliases, type-level union aliases, nominal public records/unions, record shorthand expressions, `Result` calls, public factory functions |
| `tests/fixtures/parser/positive/0011-capability-boundaries` | `docs/examples/11-capability-boundaries.tysh` | explicit `dynamic`, `reflect`, `interop`, `extern` markers, attribute-prefixed native interop declarations, exported capability-marked functions |
| `tests/fixtures/parser/positive/0012-interface-declaration` | parser-only fixture | interface declarations with function signatures |
| `tests/fixtures/parser/positive/0013-partial-declarations` | parser-only fixture | `partial` modules, records, classes, and interfaces |
| `tests/fixtures/parser/positive/0014-ambient-declarations` | parser-only fixture | `ambient` function signatures without TypeSharp bodies |
| `tests/fixtures/parser/positive/0015-open-declarations` | parser-only fixture | root-level `open` declarations before exported declarations |
| `tests/fixtures/parser/positive/0016-import-alias-declarations` | parser-only fixture | named import specifier aliases with `as` |
| `tests/fixtures/parser/positive/0017-namespace-import-declarations` | parser-only fixture | namespace import aliases with `import * as Alias` |

Negative fixture coverage:

| Fixture | Coverage |
| --- | --- |
| `tests/fixtures/parser/negative/0001-missing-function-body` | parser recovery and `TS1001` diagnostic for function declarations without a body |

Coverage boundary:
- These fixtures prove syntax recognition and parser recovery, not binder/type checker semantics.
- Semantic coverage lives under `tests/fixtures/diagnostics`.
- Backend lowering coverage lives under `tests/fixtures/backend/csharp`.
- New stable grammar must add or extend a parser fixture before the checklist can remain complete.

## 완료 기준

Parser fixture 정책은 다음 조건을 만족할 때 구현 준비가 된 것으로 본다.

- fixture 위치가 `tests/fixtures/parser/`로 고정되어 있다.
- positive/negative fixture 예제가 있다.
- diagnostic snapshot이 CLI JSON diagnostics format과 같은 shape를 사용한다.
- syntax tree snapshot이 source span과 trivia expectation을 보존한다.
- 첫 fixture 후보가 `docs/examples/*.tysh`와 연결되어 있다.
