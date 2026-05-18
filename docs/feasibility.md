# TypeSharp Feasibility Review

문서 기준일: 2026-05-18

이 문서는 현재 TypeSharp 문서 세트가 실제 구현 가능한 계획인지 검토하고, 불가능하거나 지나치게 어렵거나 모호한 약속을 실현 가능한 형태로 낮춘 결과를 기록한다.

## 결론

TypeSharp의 방향은 실현 가능하다. 다만 실현 가능하려면 다음 경계를 반드시 지켜야 한다.

1. `net481`은 생성 산출물과 표준 런타임의 필수 타깃으로 고정한다.
2. compiler, CLI, language server host는 현대 .NET LTS 기반으로 구현할 수 있게 허용한다.
3. MVP backend는 C# 7.3 compatible source generation으로 시작한다.
4. 직접 IL emit은 장기 backend로 남긴다.
5. type-level union과 structural shape는 MVP에서 public ABI로 자동 변환하지 않고 diagnostic으로 막는다.
6. nominal closed union의 MVP lowering은 reference-type class hierarchy로 시작한다.
7. TypeScript의 고급 type-level programming은 MVP에서 제외하고 complexity budget 아래 Planned/Experimental로 둔다.
8. C# library interop는 managed `net481` assembly 참조와 metadata 기반 호출을 MVP로 두고, NuGet restore와 extension-method instance sugar는 Stable Backlog로 둔다.

## 조정한 결정

| 영역 | 이전 위험 | 실현 가능한 결정 |
| --- | --- | --- |
| Compiler host | compiler/CLI/LSP까지 `net481` 실행을 요구하면 tooling 구현과 dependency 선택이 지나치게 어려워진다. | 산출물과 런타임은 `net481` 필수, compiler host는 현대 .NET LTS 허용. |
| Backend | 직접 IL emit을 초기 목표로 두면 async/debug info/generic metadata 비용이 크다. | MVP는 C# 7.3 source generation, IR은 IL backend 확장을 막지 않게 설계. |
| Union ABI | class hierarchy/tagged struct/generated closed type을 계속 열어두면 표준 라이브러리와 예제가 흔들린다. | MVP는 abstract base class + sealed case class 계열 reference representation. |
| Structural public ABI | interface/wrapper 자동 생성은 overload, naming, versioning 문제가 크다. | MVP는 public boundary diagnostic만 제공하고 자동 adapter는 Stable Backlog. |
| Pipeline call | `value |> map(arg)`의 인자 삽입 위치가 모호했다. | `value |> f`는 `f(value)`, `value |> f(args...)`는 `f(value, args...)`로 고정. |
| Type operators | `typeof`, `keyof`, indexed access까지 MVP로 보이면 범위가 너무 넓다. | MVP는 literal union과 local shape/narrowing 중심, type operators는 Planned. |
| Type provider | 빌드 중 외부 코드 실행 모델은 보안/캐시/재현성 비용이 크다. | Type provider는 Experimental이며, schema import/generator도 sandbox 정책 이후. |
| C# library interop | NuGet restore, source generator, extension method sugar까지 한 번에 넣으면 compiler와 build system 범위가 커진다. | MVP는 framework assembly/local DLL reference, metadata reader, nominal-first overload resolution, C# call lowering으로 제한한다. |

## MVP 가능 범위

MVP에서 실제로 구현 가능한 언어 범위:

- lexer/parser/syntax tree
- file-scoped namespace, import, export
- `let`, `let mut`, `literal`, `fun`
- class/interface/record/enum/delegate/event/property의 최소 모델
- local type inference와 function type
- nullable `T?`
- `Option<T>`, `Result<T, E>`
- nominal closed union with exhaustive `match`
- local type-level union and literal union narrowing
- local structural shape checking
- framework assembly and local C# DLL reference
- C# constructor/static/instance member/property/delegate/event call
- C# `ref`/`out`/`in`/`params` interop
- array literal and basic collection helper
- `Task`/`Task<T>` async interop through C# source backend
- `typesharp version/check/build/run`
- VS Code syntax highlighting and diagnostics

## MVP에서 제외하거나 낮춘 범위

| 기능 | 상태 | 이유 |
| --- | --- | --- |
| Direct IL backend | Stable Backlog | async state machine, PDB, metadata emit 비용이 높다. |
| structural type public ABI adapter | Stable Backlog | generated interface/wrapper naming과 binary compatibility 정책이 필요하다. |
| tagged struct union representation | Stable Backlog | boxing, generic, layout, C# consumption 비용이 높다. |
| mapped/conditional/template literal type | Planned/Experimental | compiler complexity가 빠르게 증가한다. |
| type provider | Experimental | 빌드 중 외부 코드 실행과 cache/security 정책이 필요하다. |
| macro system | Rejected for MVP | parser, formatter, LSP, security를 불안정하게 만든다. |
| `Span<T>` 중심 언어 기능 | Stable Backlog | `System.Memory` dependency와 ref-like 제약 검증이 필요하다. |
| F# computation expression builder | Planned | 기본 `async fun`/`Task` interop 이후가 적절하다. |
| NuGet restore in compiler | Stable Backlog | lock file, transitive dependency, license, checksum 정책이 필요하다. |
| C# extension method instance sugar | Stable Backlog | overload ranking과 name conflict 규칙이 안정화되어야 한다. |

## 구현 우선순위

1. 문서와 예제를 parser fixture로 사용할 수 있게 정리한다.
2. `TypeSharp.toml` manifest parser와 source discovery를 만든다.
3. lexer/parser/syntax tree를 만든다.
4. `typesharp check`가 parse diagnostics를 출력하게 한다.
5. reference resolver와 metadata reader를 추가한다.
6. binder가 source symbol과 metadata symbol을 같은 semantic model에서 다루게 한다.
7. local binding, function, record, class의 type checker를 만든다.
8. C# 7.3 source backend로 `net481` console app을 생성한다.
9. union/option/result/pattern matching을 추가한다.
10. C# library interop smoke test를 만든다.
11. VS Code diagnostics를 `check`와 같은 compiler core에 연결한다.

## 검증 기준

- [examples/cli-console](examples/cli-console/README.md)이 `typesharp check/build/run` smoke sample이 된다.
- generated C# source는 C# 7.3 compiler로 컴파일 가능해야 한다.
- generated assembly는 `net481` project에서 참조 가능해야 한다.
- TypeSharp가 framework assembly와 local `net481` C# DLL을 참조하고 constructor, member, delegate, event, generic API를 호출해야 한다.
- C# `net481` project가 TypeSharp library assembly를 참조하고 public API를 호출해야 한다.
- TypeSharp public API에 type-level union 또는 structural shape가 직접 나타나면 diagnostic을 낸다.
- CLI JSON diagnostics는 VS Code/LSP diagnostics로 손실 없이 변환 가능해야 한다.
