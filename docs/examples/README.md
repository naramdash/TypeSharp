# TypeSharp Examples

문서 기준일: 2026-05-19

이 폴더는 TypeSharp 코드 예제와 실행 가능한 예제 프로젝트를 함께 둔다. 상위의 단일 `.tysh` 파일들은 언어 표면과 설계 방향을 보여주는 feature examples이고, [runnable/](runnable/README.md) 아래 프로젝트들은 현재 CLI와 smoke test로 검증하는 adoption examples다.

## 파일 확장자

TypeSharp source file의 기본 확장자는 `.tysh`로 정한다.

선택 이유:
- `.tsp`는 TypeSpec 생태계에서 이미 사용하므로 피한다.
- `.tysh`는 TypeSharp를 짧게 줄이면서 기존 TypeScript/F#/C# 확장자와 충돌이 적다.
- VS Code language id는 `typesharp`, source extension은 `.tysh`로 분리한다.

## 예제 목록

- [01-hello-cli.tysh](01-hello-cli.tysh): CLI entry point, module source file, .NET static import.
- [02-modules-records.tysh](02-modules-records.tysh): import/export, type alias, immutable record, nullable field, record update.
- [03-unions-patterns.tysh](03-unions-patterns.tysh): nominal closed union, generic union, exhaustive `match`.
- [04-structural-narrowing.tysh](04-structural-narrowing.tysh): structural shape type, type-level union, `unknown` narrowing.
- [05-async-result-interop.tysh](05-async-result-interop.tysh): `async fun`, `Task<T>`, .NET exception interop, typed `Result<T, E>`.
- [06-public-api.tysh](06-public-api.tysh): C#-friendly public API, attribute, delegate, event, property.
- [07-pipeline-collections.tysh](07-pipeline-collections.tysh): pipeline, lambda, collection/object expression, local inference.
- [08-csharp-library-interop.tysh](08-csharp-library-interop.tysh): C#/.NET Framework library import, member call, named args, `out`, `Result` wrapper.
- [09-literals-attributes.tysh](09-literals-attributes.tysh): compile-time `literal`, public metadata constants, .NET attributes.
- [10-public-boundary-contract.tysh](10-public-boundary-contract.tysh): local structural/type-level flexibility normalized into C#-friendly public types.
- [11-capability-boundaries.tysh](11-capability-boundaries.tysh): explicit `dynamic`, `reflect`, `interop` capability markers.
- [cli-console/](cli-console/README.md): `TypeSharp.toml`, `src/Main.tysh`, `typesharp check/build/run` workflow.
- [runnable/](runnable/README.md): smoke-tested console, library, C# interop, ASP.NET/WCF host, worker host, and diagnostics example projects.

## 예제 커버리지

| 설계 계약 | 예제 |
| --- | --- |
| `.tysh` source file과 CLI entry point | [01-hello-cli.tysh](01-hello-cli.tysh), [cli-console/](cli-console/README.md) |
| module graph, import/export, immutable record | [02-modules-records.tysh](02-modules-records.tysh) |
| F#식 nominal closed union, exhaustive `match`, higher-order function | [03-unions-patterns.tysh](03-unions-patterns.tysh) |
| TypeScript식 local type-level union, structural shape, `unknown` narrowing | [04-structural-narrowing.tysh](04-structural-narrowing.tysh), [10-public-boundary-contract.tysh](10-public-boundary-contract.tysh) |
| `Result<T, E>`, exception interop, `Task<T>` async lowering | [05-async-result-interop.tysh](05-async-result-interop.tysh) |
| C# 소비자 친화 public ABI, property, delegate, event | [06-public-api.tysh](06-public-api.tysh), [10-public-boundary-contract.tysh](10-public-boundary-contract.tysh) |
| Pipeline, `let` lambda, local inference, collection expression | [07-pipeline-collections.tysh](07-pipeline-collections.tysh) |
| Framework assembly import, constructor/member call, named args, `out` | [08-csharp-library-interop.tysh](08-csharp-library-interop.tysh) |
| `literal`, public metadata constant, .NET attribute | [09-literals-attributes.tysh](09-literals-attributes.tysh) |
| `dynamic`, `reflect`, `interop` capability boundary | [11-capability-boundaries.tysh](11-capability-boundaries.tysh) |

## 작성 원칙

- public .NET boundary에는 nominal type을 사용한다.
- export/public value는 public metadata 또는 module surface가 흔들리지 않도록 명시 타입을 우선 사용한다.
- 파일은 `namespace`, `import`/`open`, declaration 순서로 작성한다.
- 표준 라이브러리 타입과 case constructor는 `TypeSharp.Core`에서 명시적으로 import한다.
- 함수 선언은 `fun name(params): ReturnType = expr` 또는 `fun name(params): ReturnType { ... }` 형태를 사용한다.
- lambda는 `let name = params => expr` 또는 `let name: A -> B = params => expr` 형태로 함수값에 바인딩할 수 있다.
- binding은 `let`, `let mut`, compile-time constant용 `literal`만 사용한다.
- `literal`은 primitive/string/enum/null 또는 compiler가 증명 가능한 constant expression에만 사용한다.
- type-level union과 structural shape는 local inference와 compile-time check에 사용한다.
- public API에는 type-level union, structural shape, anonymous object, inferred anonymous function type을 직접 노출하지 않는다.
- public .NET boundary로 나가는 값은 record/class/interface/union/delegate/framework type 같은 nominal surface로 닫는다.
- capability가 필요한 interop는 `interop`, `dynamic`, `reflect`, `unsafe` marker를 선언부에 드러낸다.
- marker가 붙은 함수는 호출자에게 warning/effect를 전파할 수 있으므로 public 예제에서도 marker를 숨기지 않는다.
- 예제에서 사용한 문법은 [../grammar/coverage.md](../grammar/coverage.md)의 Direct, Replacement, Planned 항목과 연결되어야 한다.
- CLI 예제는 [../cli.md](../cli.md)의 command surface와 exit code, diagnostics format을 따른다.
