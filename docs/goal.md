# TypeSharp 목표

문서 기준일: 2026-05-19

## 한 문장 과제

TypeSharp는 .NET Framework 4.8용 산출물을 만들고 그 산출물이 .NET Framework 4.8에서 실행될 수 있으면서, 최신 C#, F#, TypeScript에서 검증된 표현력, 안전성, 도구 친화성을 하나의 일관된 정적 타입 언어로 통합하는 새 언어다.

## 왜 만드는가

.NET Framework 생태계에는 장기간 운영되는 Windows 데스크톱, 서버, 플러그인, 사내 업무 시스템이 많다. 이 환경은 안정적이지만, 기본 C# 언어 버전이 .NET Framework 타깃에서 C# 7.3으로 제한되는 등 최신 언어 경험과 거리가 생겼다. TypeSharp의 과제는 런타임 호환성을 버리지 않고 최신 언어 설계의 장점을 가져오는 것이다.

TypeSharp는 다음 문제를 해결한다.

- .NET Framework 4.8 프로젝트에서도 최신 정적 타입 언어의 개발 경험을 제공한다.
- VS Code와 CLI만으로 프로젝트 생성, 편집, 진단 확인, 빌드, 실행이 가능한 기본 개발 루프를 제공한다.
- C#의 CLR 친화성, F#의 함수형 모델, TypeScript의 구조적 타입 표현을 충돌 없이 조합한다.
- 최신 문법을 단순히 흉내 내지 않고, .NET Framework에서 예측 가능한 IL, 라이브러리 호출, 또는 컴파일 타임 검사로 낮춘다.
- TypeScript, F#, C#의 실용 기능을 TypeSharp 문법으로 직접 포괄하거나, TypeSharp의 더 일관된 기능으로 대체할 수 있을 때까지 문법 커버리지를 계속 확장한다.
- 레거시 .NET 자산과 상호 운용하면서도 새 코드에는 명시적 null 안전성, 합집합 타입, 패턴 매칭, 타입 추론, 모듈성을 제공한다.

## 언어 정체성 목표

TypeSharp는 다음 네 가지 성향을 동시에 만족하는 언어를 목표로 한다.

1. TypeScript와 같이 암묵적이면서 유연한 타입 기능
   - local inference, contextual typing, structural typing, union/intersection-like type을 통해 명시 타입 없이도 자연스럽게 작성할 수 있어야 한다.
   - 단, `any`에 해당하는 완전 escape hatch는 격리하고, 기본값은 안전한 추론과 `unknown`-style narrowing에 가깝게 둔다.

2. TypeScript와 같이 모듈 기반 파일 구조
   - 파일은 module graph의 일부로 해석되고, import/export가 의존성과 public surface를 결정해야 한다.
   - ambient/global scope는 명시적으로 선언해야 하며, 프로젝트 루트와 source root는 재현 가능해야 한다.

3. F#의 함수형 기능들과 일관성
   - immutable binding, expression-oriented syntax, option/result, discriminated union, pattern matching, pipeline/composition을 언어의 중심 기능으로 둔다.
   - 함수형 기능은 객체 지향 interop 위에 얹힌 부가 문법이 아니라 TypeSharp 코드 스타일의 기본 축이어야 한다.

4. C#의 다양하고 유연한 편의 기능들
   - property, object initializer, collection expression, extension-like member, async/await, partial declaration, attribute, pattern matching 같은 실용적 편의성을 적극 검토한다.
   - 기존 C#/.NET Framework 라이브러리를 참조하고 호출할 수 있어야 하며, C#과 잘 상호 운용되는 public API를 만들 수 있어야 한다.
   - .NET 개발자가 언어 전환 비용을 낮게 느끼도록 constructor, member call, delegate, event, attribute, async `Task`, generic API, compile-time `literal` constant를 자연스럽게 다룬다.

## 기능 목표

이 기능들은 TypeSharp의 목표에 직접 포함된다. 각 기능은 .NET Framework 4.8 호환성, C#/.NET 상호 운용성, 타입 안전성, 설명 가능한 lowering을 만족해야 한다.

### 핵심 채택 목표

0. VS Code와 CLI 중심 개발 경험
   - TypeSharp는 초기부터 VS Code와 CLI로 개발 가능해야 한다.
   - CLI는 project creation, check, build, run, format, version을 제공하는 방향으로 설계한다.
   - VS Code extension은 syntax highlighting, diagnostics, hover, go-to-definition, basic completion을 제공해야 한다.
   - VS Code 기능은 Language Server Protocol 기반으로 만들고 compiler semantic model을 공유해야 한다.
   - VS Code 지원은 저장소 안의 scaffold에 머물지 않고, extension host에서 Language Server Protocol client를 활성화해 `.tysh` 편집, 진단, hover, definition, completion을 실제 사용 가능한 형태로 제공해야 한다.
   - 에디터 없이도 CLI만으로 CI와 로컬 개발이 가능해야 하며, CLI 없이도 VS Code가 diagnostics와 navigation을 제공해야 한다.
   - 이 개발 경험은 부가 도구가 아니라 TypeSharp 채택의 1순위에 가까운 핵심 산출물이다.

1. .NET Framework ASP.NET, WCF, worker 호환성
   - TypeSharp 산출물은 ASP.NET Web Forms, ASP.NET MVC/Web API, WCF service/client, Windows Service, scheduled job, queue/background worker처럼 .NET Framework에 남아 장기 운영되는 애플리케이션 모델에서 기존 C# assembly와 같은 방식으로 참조, 배포, 로드될 수 있어야 한다.
   - ASP.NET 계열 프로젝트의 `web.config`, `bin` deployment, IIS/AppDomain lifecycle, MSBuild packaging, 기존 NuGet/package reference 관례를 깨지 않는 generated assembly와 runtime library shape를 목표로 한다.
   - WCF의 service contract, data contract, message contract, configuration 기반 endpoint/binding/behavior, generated proxy/client interop를 TypeSharp public API와 C# interop 규칙 안에서 다룰 수 있어야 한다.
   - worker/service 계열에서는 Windows Service, IIS-hosted background task, console/daemon-style worker, scheduler 기반 batch job의 .NET Framework runtime, configuration, logging, diagnostics 제약을 명시적으로 고려한다.
   - 이 목표는 ASP.NET Core나 최신 .NET worker로의 전환을 전제로 하지 않는다. .NET Framework ASP.NET/WCF/worker 환경에 남아 있는 전용 hosting, configuration, deployment, diagnostics 연동 지점을 TypeSharp 호환성 범위로 둔다.

채택 경로 공통 요구사항:
- 공식 문서는 GitHub Pages로 배포 가능한 Astro Starlight 사이트로 작성한다. 저장소 문서와 중복된 설명을 흩뜨리지 않고, 언어 목표, CLI, VS Code/LSP, 예제, 마이그레이션, 진단 설명을 사용자가 탐색 가능한 정보 구조로 제공해야 한다.
- 예제 프로젝트는 단순 문법 샘플에 그치지 않고 실제로 실행 가능한 프로젝트여야 한다. console/library, C# interop, ASP.NET/WCF/worker-style .NET Framework 4.8 host, diagnostics/tooling workflow처럼 실제 채택 사례에 가까운 여러 시나리오를 포함해야 한다.
- 공식 문서와 예제는 `typesharp check`, `typesharp build`, `typesharp run`, VS Code LSP 동작, generated `net48` 산출물 검증과 연결되어야 하며, 문서에 있는 명령은 CI나 smoke test에서 재현 가능해야 한다.

2. `unknown` 중심의 안전한 gradual typing
   - TypeScript식 유연성을 가져오되 `any`를 기본 탈출구로 두지 않는다.
   - 타입 계층은 `inferred`, `unknown`, `dynamic`을 분리한다.
   - `unknown`은 shape 검사나 union narrowing 이후에만 사용할 수 있게 한다.
   - `dynamic`은 .NET `dynamic`, reflection, COM interop 같은 명시적 escape hatch로 격리한다.
   - 무제한 `any` 타입은 만들지 않거나 compatibility mode로만 허용한다.

3. 명시적 module graph와 ambient 격리
   - 모든 TypeSharp source file은 기본적으로 module graph의 일부다.
   - import/export가 없는 파일도 암묵적 global script가 되지 않는다.
   - ambient declaration은 `ambient` 문법, 별도 파일 확장자, 또는 manifest 설정으로만 가능하게 한다.
   - source root와 generated output root는 project manifest에 명시한다.

4. F# 중심의 nominal closed union과 TS식 type-level union
   - TypeSharp의 공식 런타임/도메인 union 모델은 F#의 discriminated union에 가까운 nominal closed union이다.
   - `union` 선언은 tag, case payload, exhaustiveness, runtime representation, public .NET API 노출 규칙을 가진다.
   - TypeScript식 `A | B` union은 compile-time 타입 표현, inference, narrowing, structural shape 검사에 사용한다.
   - TypeScript식 structural union은 public .NET ABI로 직접 노출하지 않는다. MVP public boundary에서는 diagnostic을 내고 nominal closed union, wrapper, nominal interface 중 하나를 명시적으로 작성하게 한다.
   - `match`는 statement가 아니라 expression이어야 한다.
   - nominal closed union의 case 누락은 exhaustiveness diagnostic으로 보고한다.
   - C# public API 노출 방식은 M1 전에 확정한다.

5. record-first immutable data
   - record는 immutable이 기본이다.
   - copy/update 문법을 제공한다.
   - value equality와 hash semantics를 명확히 한다.
   - C# 소비자가 예측 가능한 generated class shape를 제공한다.

6. pipeline, composition, partial application
   - pipeline operator는 MVP 또는 M2에서 다룬다.
   - function composition을 표준 스타일로 문서화한다.
   - placeholder 기반 partial application은 syntax 충돌을 검토한 뒤 Stable Backlog로 둔다.
   - method chaining과 pipeline이 함께 읽히는 formatting rule을 만든다.

### 설계 채택 목표

7. row-polymorphic record 방향의 structural typing
   - MVP는 width subtyping 기반 shape check로 시작한다.
   - 장기적으로 row variable이 있는 record constraint를 검토한다.
   - MVP public .NET boundary에서는 diagnostic으로 막고, nominal interface 또는 wrapper를 명시적으로 작성하게 한다. generated adapter는 Stable Backlog로 둔다.

8. 타입 레벨 계산 complexity budget
   - literal type과 union narrowing은 적극 지원한다.
   - mapped type, conditional type, template-literal-like type 계산은 제한된 형태로만 검토한다.
   - compiler는 type calculation depth, instantiation count, diagnostic simplification budget을 가져야 한다.

9. `Result<T, E>` 중심 오류 모델과 .NET exception interop
   - `Result<T, E>`는 runtime library 핵심 타입이다.
   - 새 TypeSharp API는 실패 가능성을 타입으로 표현할 수 있어야 한다.
   - .NET exception model은 interop를 위해 유지한다.
   - 함수 시그니처의 `throws` 또는 `raises` annotation은 실험 기능으로 둔다.
   - C# 호출자를 위한 exception 변환 helper를 제공한다.

10. capability-based unsafe/interop boundary
   - reflection, dynamic, COM, unsafe pointer, P/Invoke는 명시적 capability 경계 안에 둔다.
   - `unsafe`, `dynamic`, `reflect`, `interop` 같은 marker를 검토한다.
   - 해당 marker가 있는 함수는 호출자에게 효과가 전파되거나 warning을 만든다.
   - strict mode에서는 암묵적 interop escape를 금지한다.

11. compiler API와 language server를 1급 산출물로 설계
    - syntax tree, semantic model, diagnostics는 공개 가능한 internal API로 설계한다.
    - language server는 compiler semantic model을 공유한다.
    - incremental build와 deterministic output을 초기부터 설계 제약으로 둔다.
    - editor experience는 compiler CLI만큼 중요한 산출물로 취급한다.

### 실험 및 제한 목표

12. effect annotation 또는 typed effect
    - 완전한 algebraic effects와 effect handler는 MVP에서 제외한다.
    - `async`, `throws`, `io`, `unsafe`, `dynamic` 같은 작은 effect annotation을 장기 실험으로 둔다.
    - effect는 runtime 기능보다 compiler diagnostics와 API 문서화에 먼저 활용한다.

13. type provider-like 기능
    - F# type provider 수준의 빌드 중 외부 코드 실행은 초기 목표가 아니다.
    - 장기적으로 source-generated schema import 정도로 제한해 검토한다.
    - 네트워크, DB, 파일 시스템 접근은 명시 permission과 cache policy를 요구한다.

14. macro system 제한
    - macro system은 MVP에서 제외한다.
    - macro가 parser, formatter, LSP, diagnostics, security를 복잡하게 만들기 때문이다.
    - 반복되는 코드 생성은 attribute-based generator나 external build step으로 대체한다.

15. decorator 중심 메타프로그래밍 제한
    - TypeScript decorator식 모델은 MVP에서 제외한다.
    - .NET attribute interop를 먼저 안정화한다.
    - decorator syntax는 .NET attribute, analyzer, generator 모델과의 관계가 정리된 뒤 장기 실험으로만 둔다.

## 기준선

| 축 | 기준 | TypeSharp에서의 의미 |
| --- | --- | --- |
| 실행 타깃 | .NET Framework 4.8 | 생성 산출물과 표준 런타임 라이브러리는 `net48` 호환이어야 한다. compiler/CLI/LSP host는 [feasibility.md](feasibility.md)에 따라 현대 .NET LTS 기반을 허용한다. |
| C# 안정 기능 | C# 14 | 확장 멤버, null 조건부 대입, `field` 지원 프로퍼티, 람다 매개변수 modifier, 부분 생성자/이벤트 등에서 언어 설계 아이디어를 가져온다. |
| C# 프리뷰 추적 | C# 15 | 컬렉션 표현식 인자와 union type은 프리뷰로 추적하되 안정화 전까지 핵심 호환성 계약으로 삼지 않는다. |
| F# 안정 기능 | F# 10 | 옵션/ValueOption, computation expression/task 동시 바인딩, 명확한 모듈 구조, 정밀한 경고 제어, 컴파일 성능 방향성을 반영한다. |
| TypeScript 안정 기능 | TypeScript 6.0 | 구조적 타입, 엄격한 기본값, 명시적 ambient type 관리, ECMAScript 최신 타깃 추적, 7.0 전환 준비 철학을 반영한다. |
| TypeScript 프리뷰 추적 | TypeScript 7.0 beta | native compiler, 병렬 타입 검사, 더 엄격한 기본값은 구현 전략과 장기 도구 전략에서 추적한다. |

장비 벤더 호환성 배포 profile에서는 `net48`을 기본 호환성 기준으로 둔다. `net481`은 최신 .NET Framework profile로 추적하되, Windows 10/초기 Windows 11 장비, vendor qualification, offline 운용을 고려한 타깃 선택 근거는 [framework-targeting.md](framework-targeting.md)에 기록한다.

## Union 설계 결정

TypeSharp의 union 설계는 F#과 TypeScript 중 하나를 고르는 방식이 아니라, 두 모델의 책임을 분리한다.

| 역할 | 채택 모델 | TypeSharp 의미 |
| --- | --- | --- |
| 도메인 모델링 | F# 중심 nominal closed union | 런타임 표현, public API, exhaustive match, metadata 노출을 가진 공식 union 선언 |
| 타입 표현 | TypeScript 중심 type-level union | local inference, overload 후보 표현, `unknown` narrowing, literal union, structural shape union을 위한 compile-time 타입 |
| 상호 운용 | .NET/C# 친화 표현 | public boundary에서 MVP는 diagnostic을 내고 명시 nominal union/interface/wrapper를 요구한다. 자동 wrapper 생성은 Stable Backlog다. |

규칙:

- `union` 선언은 nominal closed union을 만든다.
- MVP nominal union lowering은 abstract base class + sealed case class 계열 reference representation으로 시작한다.
- `type A = B | C` 형태의 union type alias는 기본적으로 compile-time type-level union이다.
- type-level union은 local code와 generic/type inference에는 허용하지만, public .NET metadata에 직접 노출하지 않는다.
- public API에 type-level union이 나타나면 compiler는 명시적인 nominal union, interface, wrapper로 닫으라고 진단한다.
- pattern matching은 nominal closed union에 대해 exhaustiveness를 보장해야 한다.
- structural union narrowing은 TypeScript식 개발 경험을 위해 제공하되, lowering 결과가 reflection/dynamic dispatch에 의존하지 않게 한다.

## 핵심 산출물

- 에이전트 지침: `/goal` 기반 장기 작업에서 Codex가 목표를 잊지 않고 반복 실행할 수 있도록 [agent.md](../agent.md)를 유지한다.
- 장기 실행 계약: Ralph mode, Goal mode, `/goal` 세션이 같은 방식으로 작업을 고르고 검증하고 인계하도록 [agentic-execution.md](agentic-execution.md)를 유지한다.
- 작업 패킷: 한 세션보다 긴 작업은 [tasks/README.md](tasks/README.md)의 규칙에 따라 task packet으로 남긴다.
- 문법 사양: [grammar/README.md](grammar/README.md)를 중심으로 lexical, module, declaration, type, expression, pattern, interop, name resolution 문법을 설명한다.
- 문법 커버리지: [grammar/coverage.md](grammar/coverage.md)에서 TypeScript, F#, C# 기능을 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 계속 분류한다.
- 실현 가능성 검토: [feasibility.md](feasibility.md)에 MVP 범위, 낮춘 범위, backend/host 결정을 유지한다.
- 언어 사양: 타입 시스템, 이름 해석, 모듈 시스템, 표준 라이브러리, .NET 상호 운용 규칙을 설명한다.
- C# library interop: [csharp-interop.md](csharp-interop.md)에 기존 `net48` C# assembly 참조, metadata symbol, overload, nullable, public ABI 경계 규칙을 유지한다.
- 컴파일러: TypeSharp 소스를 파싱, 바인딩, 타입 검사, lowering, emit까지 처리한다.
- 런타임 라이브러리: `TypeSharp.Core.Option<T>`, `TypeSharp.Core.Result<T, E>`, nominal union representation, structural helper, async/workflow helper처럼 생성 코드가 의존하는 최소 라이브러리를 제공한다.
- 표준 라이브러리 namespace: 사용자 코드가 직접 참조하는 핵심 타입은 [standard-library.md](standard-library.md)의 `TypeSharp.Core`, `TypeSharp.Collections` 정책을 따른다.
- CLI: [cli.md](cli.md)의 계약에 따라 `typesharp build`, `typesharp check`, `typesharp run`, `typesharp format`의 초기 형태를 제공한다.
- VS Code extension: syntax highlighting, diagnostics, hover, go-to-definition, basic completion을 제공하고 Language Server Protocol을 통해 compiler semantic model을 재사용한다. Extension host에서 LSP client를 실제 활성화하고 배포 가능한 extension package 기준을 둔다.
- 공식 문서 사이트: GitHub Pages에 배포 가능한 Astro Starlight 기반 문서 사이트를 제공하고, 목표/문법/CLI/진단/VS Code/예제/마이그레이션 문서를 탐색 가능한 구조로 묶는다.
- 실행 예제 프로젝트: 실제 `typesharp` 명령으로 검사, 빌드, 실행할 수 있는 console/library/interop/host-style 예제 프로젝트들을 제공한다.
- 테스트 스위트: golden diagnostics, emitted IL/metadata, .NET interop, 표준 라이브러리, 회귀 테스트를 포함한다.
- 도구 문서: 언어 가이드, 기능별 lowering 설명, migration guide, compatibility guide를 제공한다.

## 설계 원칙

1. .NET Framework 호환성이 먼저다.
   최신 문법을 추가하더라도 생성된 프로그램과 TypeSharp 표준 런타임은 .NET Framework 4.8에서 실행 가능해야 한다. .NET 5+ 전용 API, 런타임 타입, JIT 동작에 의존하는 기능은 `net48` 산출물의 기본 lowering 대상이 될 수 없다.

2. 기능은 출처가 아니라 의미로 채택한다.
   C#, F#, TypeScript의 기능 이름을 그대로 복제하는 것이 목표가 아니다. TypeSharp 내부 의미론에 맞고 .NET Framework로 낮출 수 있는 기능만 채택한다.

3. lowering은 설명 가능해야 한다.
   모든 고수준 기능은 "어떤 IL, 어떤 helper type, 어떤 metadata, 어떤 런타임 비용으로 낮아지는가"를 문서화해야 한다.

4. 타입 안전성은 기본값이어야 한다.
   null 가능성, unchecked dynamic, reflection 기반 escape hatch는 명시해야 하며, 기본 코드는 엄격하게 검사되어야 한다.

5. 상호 운용성은 기능이다.
   TypeSharp 코드는 C#과 F#에서 호출 가능해야 하고, 기존 .NET Framework assembly를 자연스럽게 참조할 수 있어야 한다.

6. VS Code와 CLI 개발 루프가 1급 목표여야 한다.
   TypeSharp는 언어 기능만으로 완성되지 않는다. 사용자는 VS Code와 CLI만으로 코드를 작성하고, 진단을 보고, 탐색하고, 빌드하고, 실행할 수 있어야 한다.

7. 도구가 읽기 쉬운 언어여야 한다.
   parser, formatter, language server, analyzer, refactoring 도구가 안정적으로 동작할 수 있도록 모호한 문법과 문맥 의존 규칙을 줄인다.

8. 에이전트가 이어받을 수 있어야 한다.
   TypeSharp는 Ralph mode, Goal mode, Codex `/goal` 기반 장기 작업으로 계속 진행될 수 있어야 한다. 모든 중요한 결정, 검증 결과, 다음 작업은 [agent.md](../agent.md), [agentic-execution.md](agentic-execution.md), [checklist.md](checklist.md), [traceability.md](traceability.md) 중 적절한 곳에 남긴다.

## 필수 성공 조건

- `net48` 환경에서 `Hello, TypeSharp` 프로그램을 빌드하고 실행한다.
- TypeSharp에서 만든 public API를 C# .NET Framework 프로젝트가 참조해 호출한다.
- C# .NET Framework assembly의 class, interface, delegate, event, attribute, generic type을 TypeSharp에서 참조한다.
- TypeSharp가 framework assembly와 local `net48` C# library DLL의 constructor, static/instance member, property, `ref`/`out`/`params` API를 호출한다.
- VS Code extension이 Language Server Protocol client를 활성화해 `.tysh` 문서의 diagnostics, hover, go-to-definition, basic completion을 실제 편집 루프에서 제공한다.
- GitHub Pages 배포를 전제로 한 Astro Starlight 공식 문서 사이트가 빌드 가능하고, core goal, grammar, CLI, diagnostics, VS Code/LSP, migration, examples 문서를 연결한다.
- 여러 실행 예제 프로젝트가 `typesharp check/build/run` 또는 host-specific smoke 명령으로 재현 가능해야 하며, 실제 채택 시나리오별 README와 검증 명령을 가진다.
- null-safe API를 작성하고, null 관련 오류를 컴파일 단계에서 진단한다.
- nominal closed union/option/result 기반 모델링을 지원하고, 패턴 매칭에서 누락 case를 진단한다.
- TypeScript식 type-level union을 local inference, literal union, structural narrowing에 활용하되 public .NET ABI로 직접 새지 않게 진단한다.
- VS Code에서 syntax highlighting, diagnostics, hover, go-to-definition의 최소 기능을 제공한다.
- CLI에서 `typesharp check`, `typesharp build`, `typesharp run`, `typesharp version`을 제공한다.
- [grammar/coverage.md](grammar/coverage.md)가 TypeScript, F#, C#의 주요 실용 기능을 직접 문법, 동등 기능, 대체 기능, 계획, 실험, 거절 중 하나로 분류한다.
- stable grammar로 분류한 기능은 syntax 예제와 parser가 읽을 수 있는 문법 초안을 가진다.
- 구조적 object type 또는 interface-like shape 검사를 최소 MVP 범위에서 지원한다.
- async/task 기반 코드를 .NET Framework의 `Task`/`Task<T>`와 호환되게 낮춘다.
- 모든 기능은 사양 문서, 진단 테스트, lowering 테스트 중 최소 하나 이상의 근거를 가져야 한다.

## 비목표

- CLR 또는 .NET Framework 자체를 대체하지 않는다.
- JavaScript 런타임 호환성을 TypeScript처럼 제공하지 않는다.
- 모든 C#, F#, TypeScript 기능을 1:1 문법으로 복제하지 않는다. 대신 사용 사례를 TypeSharp 문법으로 직접 포괄하거나, TypeSharp식 대체 기능으로 설명하거나, 명시적으로 거절한다.
- .NET 10/11 전용 런타임 기능을 .NET Framework 기본 기능으로 요구하지 않는다.
- 완전한 dependent type, theorem proving, 매크로 시스템은 초기 목표가 아니다.
- 빌드 중 외부 코드 실행을 요구하는 type provider나 macro system은 초기 목표가 아니다.
- TypeScript decorator식 런타임 메타프로그래밍을 .NET attribute보다 먼저 도입하지 않는다.
- 프리뷰 기능은 안정 기능과 같은 호환성 보장을 받지 않는다.

## 단계별 목표

### M0: 언어 핵심 결정

- 이름 해석, 파일/모듈 구조, 기본 타입, 함수/메서드/프로퍼티 선언을 정의한다.
- [grammar/](grammar/README.md)에 lexical, module, declaration, type, expression, pattern, interop, name resolution 문법 초안을 둔다.
- [grammar/coverage.md](grammar/coverage.md)에 TypeScript, F#, C# 기능의 포괄/대체/거절 상태를 추적한다.
- `net48` 산출물 타깃, 패키징 방식, compiler host 요구사항을 [feasibility.md](feasibility.md)의 결정에 맞춰 확정한다.
- MVP 기능과 프리뷰 추적 기능을 분리한다.
- CLI command surface와 VS Code extension/LSP 최소 범위를 확정한다.

### M1: 최소 컴파일러

- lexer/parser/AST를 구현한다.
- symbol table과 type checker의 첫 버전을 구현한다.
- C# 7.3 source backend로 간단한 console app 또는 library assembly를 `net48`로 emit한다.
- golden diagnostics 테스트 체계를 만든다.
- `typesharp version`, `typesharp check`, `typesharp build`의 첫 버전을 만든다.
- VS Code syntax highlighting과 diagnostics 연결을 만든다.

### M2: 안전성과 표현력

- nullability, option/result, nominal closed union, type-level union narrowing, pattern matching을 구현한다.
- structural type checking의 MVP를 구현한다.
- C# assembly consume 및 C#에서 TypeSharp assembly consume을 검증한다.
- VS Code hover와 go-to-definition의 최소 기능을 제공한다.

### M3: 생산성

- async/task lowering, extension-like member, collection expression, record/object literal을 구현한다.
- formatter와 language server protocol의 최소 기능을 제공한다.
- 표준 라이브러리 문서와 migration guide를 작성한다.

### M4: 안정화

- 성능 기준, 호환성 매트릭스, breaking change 정책을 수립한다.
- preview feature gate를 도입한다.
- 샘플 프로젝트와 release checklist를 완성한다.
- Astro Starlight 공식 문서 사이트와 GitHub Pages 배포 workflow를 만든다.
- 실행 가능한 예제 프로젝트 catalog를 확장하고 각 예제의 CLI/host 검증 명령을 고정한다.

## 후속 확장 결정

- 직접 IL emit backend 구현은 Stable Backlog다. 현재 결정은 C# 7.3 source backend와 backend artifact seam을 유지하고, public metadata fidelity, PDB/debug info, ABI control, or performance 요구가 source backend 비용을 넘을 때 direct IL backend를 도입하는 것이다.
- VS Code extension과 CLI는 `TypeSharpSemanticModel`을 공유하는 구조를 현재 계약으로 둔다. 후속 확장은 cross-file project model, metadata/source unified symbol graph, and incremental cache다.
- structural type을 public metadata에 자동 adapter로 노출하는 정책은 Stable Backlog다. MVP는 public boundary diagnostic과 명시 nominal interface/wrapper 작성을 요구한다.
- nominal closed union의 MVP 런타임 표현은 reference-type class hierarchy다. tagged struct나 generated closed type은 성능/ABI 검증 뒤 Stable Backlog로 도입한다.
- type-level union public API boundary는 `TS2204` diagnostic으로 막는다. 수동 대체는 nominal union, interface, or wrapper를 사용하며 guide refinement는 Stable Backlog다.

## 근거 자료

- Microsoft Learn, ".NET Framework on Windows": .NET Framework 최신 버전은 4.8.1이며 Windows 전용 기술이다. https://learn.microsoft.com/en-us/dotnet/framework/install/on-windows-and-server
- Microsoft Learn, "C# language versioning": .NET Framework 타깃의 기본 C# 언어 버전은 C# 7.3이고, C# 14는 .NET 10, C# 15는 .NET 11 preview 계열이다. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning
- Microsoft Learn, "What's new in C# 14": C# 14는 최신 안정 C# 릴리스다. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14
- Microsoft Learn, "What's new in C# 15": C# 15는 최신 C# preview 릴리스다. https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15
- Microsoft Learn, "What's new in F# 10": F# 10은 .NET 10과 Visual Studio 2026에 포함된 최신 F# 계열이다. https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10
- TypeScript Team, "Announcing TypeScript 6.0": TypeScript 6.0은 안정 릴리스이며 7.0 전환 릴리스다. https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/
- TypeScript Team, "Announcing TypeScript 7.0 Beta": TypeScript 7.0 beta는 native compiler와 새 기본값을 추적할 대상이다. https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/
- TypeScript 6.0 release notes: strict/module/root/types 기본값과 7.0 전환 방향. https://www.typescriptlang.org/docs/handbook/release-notes/typescript-6-0.html
- TC39 pattern matching proposal: expression-oriented pattern matching과 `is` operator 방향. https://github.com/tc39/proposal-pattern-matching
- Gradual Typing, Siek and Taha: 정적 타입과 동적 타입의 점진적 결합 이론. https://repository.rice.edu/items/a748d07b-fe20-48ef-9e2b-9cf133eb64d3
- Local Type Inference, Pierce and Turner: 객체지향/함수형 스타일에서 지역 타입 추론을 설계하는 기반. https://scholarworks.iu.edu/dspace/items/30aca857-5c94-4d28-9b31-75e0edea8fbb
- Handlers of Algebraic Effects, Plotkin and Pretnar: effect handler 계열 기능의 이론적 기반. https://www.research.ed.ac.uk/files/17909848/Plotkin_Pretnar_2009_Handlers_of_Algebraic_Effects.pdf
- Abstracting Extensible Data Types, Morris and McKinna: row polymorphism과 extensible record/variant 설계 참고. https://www.research.ed.ac.uk/en/publications/abstracting-extensible-data-types-or-rows-by-any-other-name

