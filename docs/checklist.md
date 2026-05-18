# TypeSharp 체크리스트

문서 기준일: 2026-05-19

이 체크리스트는 설계, 구현, 검증을 반복할 때 사용하는 기준이다.

## 문서 완성도

- [x] 과제의 한 문장 목표가 있다.
- [x] 실행 타깃과 언어 기준선이 날짜와 함께 명시되어 있다.
- [x] 필수사항, 권장사항, 선택사항이 분리되어 있다.
- [x] MVP와 프리뷰 추적 기능이 분리되어 있다.
- [x] 비목표가 명시되어 있다.
- [x] 공식 근거 링크가 있다.
- [x] `/goal` 기반 에이전트 지침이 있다.
- [x] Ralph/Goal mode 장기 실행 계약이 있다.
- [x] 언어 정체성 목표가 명시되어 있다.
- [x] 기능 목표가 goal에 통합되어 있다.
- [x] Union 설계 결정이 goal에 명시되어 있다.
- [x] VS Code와 CLI 개발 경험이 1급 목표로 명시되어 있다.
- [x] .NET Framework ASP.NET/WCF/worker 호환성 목표가 명시되어 있다.
- [x] .NET Framework application model compatibility 계약이 요구사항과 기능 매핑에 반영되어 있다.
- [x] grammar 폴더가 있고 TypeSharp 문법 목표를 설명한다.
- [x] syntax consistency 규칙이 문서화되어 있다.
- [x] TypeScript/F#/C# 기능을 문법으로 포괄하거나 대체하는 coverage matrix가 있다.
- [x] TypeSharp source file 확장자가 `.tysh`로 정해져 있다.
- [x] 대표 기능별 `.tysh` 예제 파일이 있다.
- [x] 표준 라이브러리 namespace 정책 초안이 있다.
- [x] C# library interop 계약 문서가 있다.
- [x] CLI command surface 문서가 있다.
- [x] CLI 예제 프로젝트가 있다.
- [x] 실현 가능성 검토 문서가 있다.
- [ ] 각 기능별 세부 사양 문서가 있다.
- [x] grammar 수준의 syntax 예제가 있다.
- [ ] lowering 예제가 기능별로 있다.
- [ ] migration guide 초안이 있다.

## 에이전틱 실행 준비

- [x] 장기 실행 부트스트랩 순서가 있다.
- [x] 변하지 않는 기준선이 한 곳에 정리되어 있다.
- [x] 작업 선택 우선순위 큐가 있다.
- [x] task packet template이 있다.
- [x] 문서/구현/설계 결정별 Done 기준이 있다.
- [x] 세션 인계 기준이 있다.
- [x] 실제 `docs/tasks/` 작업 패킷이 있다.
- [x] 다음 구현 작업의 첫 task packet이 작성되어 있다.
- [x] parser fixture format 정책이 있다.
- [ ] 반복 실행 결과를 기록할 progress log 정책이 있다.
- [ ] 장기 실행 중 decision record를 남기는 ADR 형식이 있다.

## 플랫폼

- [x] .NET Framework 4.8 설치 환경에서 smoke test가 실행된다.
- [x] 생성 assembly가 `net48` 프로젝트에서 참조된다.
- [x] compiler host 요구사항이 확정되어 있다.
- [x] runtime library가 `net48`로 빌드된다.
- [x] generated assembly/runtime library가 ASP.NET Web Forms/MVC/Web API 프로젝트에서 참조되는 smoke test가 있다.
- [x] generated assembly/runtime library가 WCF service/client 또는 contract 프로젝트에서 참조되는 smoke test가 있다.
- [x] generated assembly/runtime library가 Windows Service 또는 worker-style `net48` 프로젝트에서 참조되는 smoke test가 있다.
- [x] .NET 5+ 전용 API 사용 여부를 검사한다.
- [x] NuGet dependency의 `net48` 호환성과 라이선스를 기록한다.

## 언어 사양

- [x] lexical grammar 초안이 있다.
- [x] expression grammar 초안이 있다.
- [x] declaration grammar 초안이 있다.
- [x] module/namespace/import 규칙 초안이 있다.
- [x] 이름 해석 규칙 초안이 있다.
- [x] overload candidate 규칙 초안이 있다.
- [x] generic constraint 문법 초안이 있다.
- [x] nullability 문법 초안이 있다.
- [x] compile-time constant `literal` 문법 초안이 있다.
- [x] nominal closed union/option/result 문법 초안이 있다.
- [x] TypeScript식 type-level union과 narrowing 문법 초안이 있다.
- [x] type-level union의 public ABI 금지/변환 규칙 초안이 있다.
- [x] pattern matching exhaustiveness 규칙 초안이 있다.
- [x] structural type 문법 초안이 있다.
- [x] async/task 문법 초안이 있다.
- [x] .NET interop 문법 초안이 있다.
- [x] managed C# library import/reference 규칙 초안이 있다.
- [ ] stable grammar별 parser fixture가 있다.
- [x] parser fixture directory와 diagnostic snapshot 형식이 정해져 있다.
- [x] 문법 ambiguity 검토표가 있다.
- [x] parser precedence table이 있다.

## MVP 기능

- [ ] 기본 타입과 literal
- [ ] local binding
- [ ] compile-time constant `literal`
- [ ] function declaration/call
- [ ] class/interface declaration
- [x] property/method/constructor
- [ ] generic type/function
- [ ] module/namespace
- [ ] immutable record
- [ ] `Option<T>`
- [ ] `Result<T, E>`
- [ ] nominal closed union type
- [ ] type-level union alias
- [ ] union narrowing
- [ ] pattern matching
- [ ] null safety
- [ ] basic structural type checking
- [ ] `Task`/`Task<T>` async interop
- [x] C# assembly reference
- [x] C# local DLL reference
- [x] C# constructor/static/instance member call
- [ ] C# `ref`/`out`/`in`/`params` interop
- [x] C# `params` interop compile smoke
- [x] C# `out` interop compile smoke
- [x] C# `in` interop compile smoke
- [x] C# `ref` interop compile smoke
- [x] C# delegate parameter interop compile smoke
- [x] C# event interop
- [x] nullable metadata/unknown nullability diagnostics
- [x] TypeSharp assembly consumed from C#

## 컴파일러

- [x] lexer
- [x] parser
- [x] syntax tree with source spans
- [x] parser error recovery
- [x] compiler core project skeleton
- [x] manifest parser and project loader
- [ ] binder
- [x] binder/name resolution skeleton
- [ ] metadata reader
- [x] C# metadata reader skeleton
- [x] C# metadata reader local public symbol index
- [x] C# metadata reader params parameter flag
- [x] C# reference resolver skeleton
- [x] C# metadata diagnostics pipeline integration
- [x] C# invalid byref interop diagnostic smoke
- [x] C# ambiguous overload diagnostic smoke
- [x] C# exact overload match smoke
- [x] C# expanded params overload validation smoke
- [x] C# optional parameter overload validation smoke
- [x] C# named argument overload validation smoke
- [x] C# unknown nullability diagnostic smoke
- [ ] reference resolver for framework assembly and local DLL
- [ ] semantic model
- [ ] type checker
- [x] type checker basic mismatch skeleton
- [ ] inference engine
- [ ] C# overload resolution
- [ ] public ABI checker
- [ ] lowering passes
- [ ] backend abstraction
- [ ] C# 7.3 source backend
- [x] C# source backend first golden fixture
- [x] C# source backend import directive skeleton
- [x] C# source backend call expression skeleton
- [x] C# source backend block/local skeleton
- [x] generated C# `net48` compile smoke
- [x] generated C# source emission path
- [x] generated C# `net48` project scaffold emission path
- [x] generated C# `net48` assembly build path
- [x] generated C# project manifest reference propagation path
- [x] generated C# framework static member call build smoke
- [x] generated C# local DLL static member call build smoke
- [x] generated C# imported constructor and instance member call build smoke
- [x] generated C# imported property access build smoke
- [x] generated C# imported `params` call build smoke
- [x] generated C# imported `out` call build smoke
- [x] generated C# imported `in` call build smoke
- [x] generated C# imported `ref` call build smoke
- [x] generated C# imported delegate lambda call build smoke
- [x] generated C# imported event add/remove build smoke
- [ ] IL backend abstraction seam
- [ ] diagnostics system
- [x] initial diagnostic/span model
- [x] diagnostics code taxonomy and descriptor metadata
- [x] CLI integration with compiler core
- [x] `.tysh` source file discovery

## 런타임 라이브러리

- [x] runtime namespace 정책 초안
- [x] MVP union representation 정책 초안
- [x] `TypeSharp.Runtime` `net48` project skeleton
- [x] `TypeSharp.Core` `net48` project skeleton
- [x] `Option<T>`
- [x] `Result<T, E>`
- [x] `Unit`
- [ ] nominal union helper
- [ ] pattern helper
- [ ] equality/hash helper
- [ ] async helper
- [ ] public ABI versioning 정책

## 도구

- [x] project manifest 초안
- [x] project manifest loader
- [x] CLI command surface 문서
- [x] CLI `version`
- [x] CLI `check`
- [x] CLI `check` parse diagnostics path
- [x] CLI `check` reference diagnostics path
- [x] CLI `check` type checker diagnostics path
- [x] CLI `build`
- [x] CLI `build` generated C# emission path
- [x] CLI `build` reference diagnostics stop path
- [x] CLI `build` type checker diagnostics stop path
- [x] CLI `build` generated C# project scaffold path
- [x] CLI `build` generated `net48` assembly path
- [x] CLI `build` generated project reference propagation path
- [x] CLI `run`
- [x] CLI `run` generated `net48` executable smoke path
- [x] CLI `run` forwards `--` arguments to `main(args: string[])`
- [x] CLI `run` unsupported main signature diagnostics
- [x] formatter convention 문서
- [x] VS Code extension scaffold
- [x] VS Code syntax highlighting
- [ ] LSP diagnostics
- [ ] LSP hover
- [ ] LSP go-to-definition
- [ ] LSP basic completion
- [x] diagnostic explanation 문서
- [x] sample project

## 테스트

- [x] parser positive fixtures
- [x] parser negative fixtures
- [x] parser modules/records positive fixture
- [x] parser unions/patterns positive fixture
- [x] parser structural/narrowing positive fixture
- [x] parser async/result interop positive fixture
- [x] parser public API positive fixture
- [x] parser pipeline/collections positive fixture
- [x] parser C# library interop positive fixture
- [x] parser literals/attributes positive fixture
- [x] parser public boundary contract positive fixture
- [x] parser capability boundaries positive fixture
- [x] parser fixture format policy
- [x] compiler skeleton smoke test harness
- [x] manifest/source discovery smoke tests
- [x] CLI check parse diagnostics smoke tests
- [x] CLI check type checker diagnostics smoke tests
- [x] diagnostic descriptor registry smoke tests
- [x] binder/name resolution smoke tests
- [x] type checker basic mismatch smoke tests
- [x] binder fixtures
- [x] type checker positive fixtures
- [x] type checker negative fixtures
- [x] diagnostic golden tests
- [x] C# source backend golden fixture
- [x] C# source backend import directive golden fixture
- [x] C# source backend call expression golden fixture
- [x] C# source backend block/local golden fixture
- [x] generated C# `net48` compile smoke
- [x] runtime `net48` build smoke
- [x] CLI build generated C# emission smoke tests
- [x] CLI build type checker diagnostics stop smoke tests
- [x] CLI build generated C# project scaffold smoke tests
- [x] TypeSharp.Core Option/Result behavior smoke tests
- [x] C# reference resolver smoke tests
- [x] C# metadata reader smoke tests
- [x] C# metadata reader local public symbol index smoke tests
- [x] C# metadata reader params parameter flag smoke tests
- [x] C# metadata pipeline diagnostics smoke tests
- [x] C# invalid byref interop diagnostic smoke tests
- [x] C# ambiguous overload diagnostic smoke tests
- [x] C# exact overload match smoke tests
- [x] C# expanded params overload validation smoke tests
- [x] C# optional parameter overload validation smoke tests
- [x] C# named argument overload validation smoke tests
- [x] C# unknown nullability diagnostic smoke tests
- [x] emitted assembly smoke tests
- [x] generated project reference propagation smoke tests
- [x] C# framework static member call smoke tests
- [x] C# local DLL static member call smoke tests
- [x] C# constructor and instance member call smoke tests
- [x] C# property access smoke tests
- [x] C# `params` call smoke tests
- [x] C# `out` call smoke tests
- [x] C# `in` call smoke tests
- [x] C# `ref` call smoke tests
- [x] C# event add/remove call smoke tests
- [x] ASP.NET/WCF/worker-style `net48` host compatibility smoke tests
- [x] `net48` runtime dependency compatibility audit smoke tests
- [x] CLI run generated `net48` executable smoke tests
- [x] CLI run `main(args: string[])` argument forwarding smoke tests
- [x] CLI run unsupported main signature diagnostic smoke tests
- [ ] lowering golden tests
- [ ] runtime unit tests
- [ ] C# interop tests
- [x] C# library consumption smoke tests
- [ ] public ABI snapshot tests
- [ ] performance smoke benchmark
- [ ] regression test policy

## 릴리스 준비

- [ ] versioning 정책
- [ ] breaking change 정책
- [ ] preview feature gate
- [ ] package signing 또는 checksum 정책
- [x] dependency inventory
- [ ] security policy
- [ ] release notes template
- [ ] compatibility matrix

## 반복 검토 질문

- [ ] 이 기능은 .NET Framework 4.8에서 실행 가능한가?
- [ ] 이 기능은 public .NET metadata로 표현 가능한가, 아니면 compile-time only인가?
- [ ] lowering이 문서화되어 있는가?
- [ ] C# 소비자가 이 API를 이해할 수 있는가?
- [ ] diagnostics가 사용자의 다음 행동을 알려주는가?
- [ ] 프리뷰 기능이 안정 기능처럼 섞여 있지 않은가?
- [ ] 기능 추가가 formatter/LSP/analyzer를 불필요하게 어렵게 만들지 않는가?
- [ ] 테스트가 positive/negative 양쪽을 포함하는가?

