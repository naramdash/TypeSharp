# TypeSharp 필수사항

문서 기준일: 2026-05-18

이 문서는 TypeSharp가 반드시 만족해야 하는 요구사항을 정의한다. RFC 2119의 엄격한 용어 대신 한국어 표기를 사용한다.

- 필수: 구현하지 않으면 TypeSharp의 목표가 깨지는 항목
- 권장: 초기 버전에 없어도 되지만 설계를 막아서는 안 되는 항목
- 선택: 실험 또는 장기 후보

## 1. 플랫폼 요구사항

### 필수

- 생성 산출물의 기본 실행 타깃은 .NET Framework 4.8.1이어야 한다.
- 생성 assembly는 `net481` 프로젝트에서 참조 가능해야 한다.
- 기본 런타임 라이브러리는 .NET Framework 4.8.1에서 로드되어야 한다.
- .NET 5 이상에만 존재하는 BCL API를 기본 lowering 대상으로 삼지 않아야 한다.
- .NET Framework가 Windows 전용 기술이라는 사실을 전제로 호환성 문서를 작성해야 한다.
- 생성 assembly와 표준 런타임은 ASP.NET Web Forms, ASP.NET MVC/Web API, WCF service/client, Windows Service, scheduled job, queue/background worker 같은 .NET Framework application model에서 기존 C# library와 같은 방식으로 참조, 배포, 로드될 수 있어야 한다.
- ASP.NET/WCF/worker 호환성은 ASP.NET Core 또는 최신 .NET worker로의 migration을 요구하지 않아야 하며, `web.config`, `bin` deployment, IIS/AppDomain lifecycle, configuration-based service model, MSBuild packaging 관례를 깨지 않아야 한다.
- 외부 NuGet dependency는 `net481` 호환 여부, 라이선스, 배포 방식이 문서화되어야 한다.
- compiler, CLI, language server host는 현대 .NET LTS 기반으로 실행될 수 있다. 단, 생성 산출물과 표준 런타임의 `net481` 호환성 테스트를 반드시 통과해야 한다.

### 권장

- compiler core, runtime library, CLI entrypoint를 분리한다.
- deterministic build를 지원한다.
- portable PDB 또는 Windows PDB 지원 전략을 초기에 결정한다.
- CI는 최소한 Windows에서 `net481` smoke test를 실행한다.

## 2. 언어 핵심 요구사항

### 필수

- 파일, namespace, module, type, member, local binding의 이름 해석 규칙을 명시해야 한다.
- lexical, module, declaration, type, expression, pattern, interop 문법 초안이 있어야 한다.
- TypeScript, F#, C#의 주요 실용 기능은 문법 커버리지 문서에서 직접 지원, 동등 기능, 대체 기능, 계획, 실험, 거절 중 하나로 분류되어야 한다.
- 기본 타입은 .NET Framework의 primitive type과 명확히 대응되어야 한다.
- class, interface, enum, delegate, record-like type, nominal closed union의 최소 모델을 정의해야 한다.
- TypeScript식 type-level union은 local inference, literal union, `unknown` narrowing, structural shape 검사에 사용할 수 있어야 한다.
- type-level union은 public .NET ABI에 직접 노출되지 않아야 한다. MVP에서는 public boundary diagnostic을 내고, 사용자가 nominal union, nominal interface, wrapper 중 하나로 명시적으로 닫게 한다.
- generic type과 generic function을 지원해야 한다.
- 함수, 메서드, 프로퍼티, 생성자, 이벤트의 선언과 호출 규칙을 정의해야 한다.
- immutable binding을 기본으로 하고 mutable state는 명시해야 한다.
- 컴파일 타임 상수는 `const`가 아니라 `literal` declaration으로 표현해야 한다.
- null 가능성은 타입 시스템에서 표현되어야 한다.
- pattern matching은 union, option, tuple, primitive literal, type test 중 최소 일부를 지원해야 한다.
- 패턴 매칭 exhaustiveness는 가능한 범위에서 진단해야 한다.
- 모든 public API는 .NET metadata로 어떻게 노출되는지 문서화해야 한다.

### 권장

- expression-oriented 문법을 기본으로 하되 .NET 객체 지향 모델과 충돌하지 않게 한다.
- type inference는 local-first로 시작하고 public boundary에는 명시 타입을 권장한다.
- C# 사용자에게 익숙한 member access와 F# 사용자에게 익숙한 pipeline/composition 스타일을 모두 허용할 수 있게 설계한다.
- TypeScript식 structural type은 public ABI가 아니라 컴파일 타임 검사로 시작한다.

## 3. 최신 언어 기능 반영 요구사항

### 필수

- 최신 C#, F#, TypeScript의 기능은 직접 복제가 아니라 TypeSharp 의미론으로 재해석해야 한다.
- 각 기능은 다음 다섯 가지 중 하나로 분류되어야 한다.
  - MVP: 첫 안정 버전에 포함
  - Stable Backlog: 안정 기능이지만 첫 버전 이후
  - Preview Watch: 외부 언어에서 아직 프리뷰라 안정 계약으로 삼지 않음
  - Experimental: TypeSharp에서 feature gate와 별도 검증이 필요한 실험
  - Rejected: 목표와 맞지 않거나 .NET Framework 호환성이 낮음
- 기능별로 lowering, 런타임 비용, .NET interop 노출 방식을 기록해야 한다.
- C# 15, TypeScript 7.0 beta 등 프리뷰 기반 기능은 feature gate 없이 기본 기능이 될 수 없다.

### 권장

- C#에서 extension member, null safety, partial declaration, collection expression 계열 아이디어를 검토한다.
- F#에서 nominal closed union, option/value option, computation expression, task 동시 바인딩, 경고 제어, 명확한 module 구조를 검토한다.
- TypeScript에서 structural typing, type-level union/intersection type, type alias, literal type, strict default, explicit ambient types 철학을 검토한다.

## 4. 컴파일러 요구사항

### 필수

- 컴파일러 파이프라인은 최소한 `parse -> bind -> type check -> lower -> emit` 단계로 나뉘어야 한다.
- parser는 오류 복구를 지원해 한 파일에서 여러 diagnostics를 낼 수 있어야 한다.
- symbol table은 .NET metadata symbol과 TypeSharp source symbol을 함께 다룰 수 있어야 한다.
- type checker는 nullability, generic constraint, overload resolution, member resolution을 다뤄야 한다.
- lowering 단계는 고수준 기능을 .NET Framework 호환 표현으로 변환해야 한다.
- emit 단계의 MVP backend는 C# 7.3 호환 source generation으로 시작한다.
- 직접 IL emit은 backend abstraction을 통해 Stable Backlog로 확장 가능해야 한다.
- diagnostics는 안정적인 error code, source span, 설명, 가능하면 fix hint를 포함해야 한다.
- compiler crash는 사용자 코드 오류로 보이면 안 되며 internal error로 분리되어야 한다.

### 권장

- syntax tree와 semantic model을 공개 API로 설계해 language server와 analyzer가 재사용하게 한다.
- grammar 문서는 parser, formatter, VS Code TextMate grammar, LSP semantic token 구현의 입력으로 사용할 수 있게 유지한다.
- parser golden test, diagnostics golden test, lowering golden test를 별도 fixture로 둔다.
- multi-target backend를 염두에 둔 IR을 만든다.

## 5. .NET 상호 운용 요구사항

### 필수

- 기존 .NET Framework assembly를 참조할 수 있어야 한다.
- manifest에서 framework assembly와 local DLL reference를 구분해 선언할 수 있어야 한다.
- C# class/interface/delegate/enum/attribute/generic type을 TypeSharp에서 사용할 수 있어야 한다.
- C# constructor, static member, instance member, property, field, indexer, event를 호출할 수 있어야 한다.
- TypeSharp public API를 C# .NET Framework 프로젝트에서 호출할 수 있어야 한다.
- overload resolution은 C# interop에서 예측 가능해야 한다.
- C# named argument, optional parameter, `params`, `ref`, `out`, `in` parameter를 metadata 기반으로 처리해야 한다.
- attribute 사용과 생성은 .NET metadata 규칙을 따라야 한다.
- WCF service contract, data contract, message contract, generated proxy/client interop는 TypeSharp public API와 C# metadata interop 규칙 안에서 표현 가능해야 한다.
- exception 모델은 .NET exception과 호환되어야 한다.
- async 모델은 `Task`와 `Task<T>`를 기본 interop 타입으로 사용해야 한다.
- nullable annotation이 없는 C# assembly는 unknown nullability로 다루고 strict mode에서 진단 또는 guard를 요구해야 한다.
- public ABI에서 structural shape, type-level union, anonymous object, marker 없는 `dynamic`이 노출되지 않게 해야 한다.

### 권장

- NuGet package reference는 `net481` compatible asset, transitive dependency, license, checksum/lock file 정책과 함께 설계한다.
- C# extension method instance-call sugar는 overload ranking이 안정된 뒤 제공한다.
- F# option, tuple, record와의 interop는 별도 compatibility layer로 검토한다.
- CLS compliance를 public API 체크 옵션으로 제공한다.
- dynamic, reflection, COM interop는 가능하지만 strict mode에서는 명시 opt-in으로 둔다.

## 6. 표준 라이브러리 요구사항

### 필수

- `Option<T>` 또는 동등한 nullable 대체 타입을 제공해야 한다.
- `Result<T, E>` 또는 동등한 오류 모델링 타입을 제공해야 한다.
- `Option<T>`와 `Result<T, E>`의 기본 namespace는 `TypeSharp.Core`로 문서화해야 한다.
- nominal closed union lowering이 필요로 하는 tag/value representation을 제공해야 한다.
- collection helper는 .NET Framework collection과 상호 운용되어야 한다.
- async helper는 `Task` 기반이어야 한다.
- 표준 라이브러리는 compiler-generated code와 사용자 API의 경계를 안정적으로 유지해야 한다.

### 권장

- immutable collection은 BCL에 없는 경우 dependency 또는 자체 구현을 비교한 뒤 결정한다.
- structural equality, ordering, hashing 정책을 타입별로 명확히 한다.
- Span 계열 기능은 `System.Memory` 패키지 의존성과 .NET Framework 성능 특성을 검증한 뒤 채택한다.

## 7. 도구 요구사항

### 필수

- CLI는 최소한 build/check/version을 지원해야 한다.
- CLI는 `typesharp check`, `typesharp build`, `typesharp run`, `typesharp version`을 목표 command surface로 가져야 한다.
- CLI command, option, exit code, diagnostics format은 [cli.md](cli.md)에 문서화되어야 한다.
- VS Code extension은 최소한 syntax highlighting, diagnostics, hover, go-to-definition을 목표 기능으로 가져야 한다.
- VS Code extension은 Language Server Protocol 기반이어야 하며 compiler semantic model을 재사용해야 한다.
- 프로젝트 파일 또는 manifest 형식을 정의해야 한다.
- compiler option은 재현 가능해야 하며 CLI와 project file에서 같은 의미여야 한다.
- formatter가 없더라도 공식 formatting convention은 문서화해야 한다.
- VS Code syntax highlighting은 [grammar/lexical.md](grammar/lexical.md)와 [grammar/README.md](grammar/README.md)의 stable grammar를 기준으로 구현해야 한다.

### 권장

- `typesharp new`와 `typesharp format`을 제공한다.
- Language Server Protocol 기반 hover, go-to-definition, diagnostics를 제공한다.
- formatter, linter, analyzer를 compiler semantic model 위에 올린다.
- TypeSharp Playground 또는 샘플 runner는 장기 목표로 둔다.

## 8. 품질 요구사항

### 필수

- MVP의 모든 기능은 최소 하나 이상의 positive test와 negative diagnostic test를 가져야 한다.
- public 기능은 문서, 예제, 테스트가 함께 있어야 한다.
- .NET Framework 4.8.1이 설치된 Windows 환경에서 smoke test를 통과해야 한다.
- breaking change 정책을 문서화해야 한다.
- 프리뷰 기능은 기본 안정성 약속에서 분리해야 한다.

### 권장

- 대형 프로젝트 type checking 성능 기준을 만든다.
- error message snapshot을 리뷰 대상으로 삼는다.
- fuzzing 또는 property-based test로 parser/type checker 견고성을 검증한다.

## 9. 보안 및 배포 요구사항

### 필수

- compiler는 임의의 사용자 코드를 빌드 중 실행하지 않아야 한다.
- source generator, plugin, macro 같은 확장 지점은 초기 버전에서 비활성화하거나 sandbox 정책을 가져야 한다.
- NuGet 패키지와 배포 파일은 checksum 또는 서명 전략을 가져야 한다.
- 빌드 산출물의 dependency 목록을 추적해야 한다.

### 권장

- release artifact 재현성을 검증한다.
- 취약 dependency 대응 정책을 문서화한다.
- signed assembly와 strong naming 필요성을 검토한다.

