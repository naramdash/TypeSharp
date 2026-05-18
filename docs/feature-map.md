# 최신 언어 기능 매핑

문서 기준일: 2026-05-18

이 문서는 C#, F#, TypeScript의 최신 기능을 TypeSharp에서 어떻게 해석할지 정리한다. 목표는 기능 복제가 아니라 .NET Framework 4.8.1에서 실행 가능한 일관된 언어 설계다.

## 기능 분류

| 상태 | 의미 |
| --- | --- |
| MVP | 첫 구현에서 목표로 삼는 기능 |
| Stable Backlog | 안정 언어에서 검증됐지만 첫 구현 이후로 미루는 기능 |
| Preview Watch | 외부 언어에서 아직 preview라 안정 계약으로 삼지 않는 기능 |
| Experimental | TypeSharp에서 feature gate와 별도 검증이 필요한 실험 |
| Rejected | TypeSharp 목표와 맞지 않거나 .NET Framework 호환성이 낮은 기능 |

## 기준 언어 버전

| 언어 | 기준 | 주요 관찰 |
| --- | --- | --- |
| C# | C# 14 안정, C# 15 preview 추적 | C# 14는 확장 멤버와 null 조건부 대입 등 생산성 기능을 제공한다. C# 15는 union type과 컬렉션 표현식 인자를 preview로 제공한다. |
| F# | F# 10 | F# 10은 명확성, 일관성, 성능에 초점을 둔 개선판이다. option, computation expression, task 동시 바인딩, 경고 제어가 참고점이다. |
| TypeScript | TypeScript 6.0 안정, TypeScript 7.0 beta 추적 | TypeScript 6.0은 엄격한 기본값과 7.0 전환 준비를 강조한다. 7.0은 native compiler와 병렬 검사 방향을 보여준다. |

## 우선순위 렌즈

기능을 채택할 때는 다음 네 가지 목표에 어떻게 기여하는지 먼저 본다.

- TypeScript식 암묵적이고 유연한 타입: inference, contextual typing, structural typing, narrowing은 적극 채택하되 무제한 `any`는 격리한다.
- TypeScript식 모듈 기반 파일 구조: 모든 파일을 module graph의 일부로 해석하고 ambient/global 선언은 명시한다.
- F#식 함수형 일관성: immutable data, expression-oriented syntax, option/result, nominal closed union, pattern matching, pipeline을 언어 중심부에 둔다.
- C#식 편의 기능: .NET interop, property/object/collection initializer, async/await, attribute, extension-like member, compile-time literal constant를 실용성 기준으로 검토한다.

## 1. Null Safety

상태: MVP

출처:
- C# nullable reference type 계열
- F# `option`/`ValueOption`
- TypeScript `strictNullChecks` 철학

TypeSharp 결정:
- 모든 reference-like type은 기본적으로 non-null로 취급한다.
- nullable 값은 `T?` 또는 `Option<T>` 계열로 명시한다.
- interop에서 C# nullable annotation이 없으면 "unknown nullability"로 다루고 경고를 낸다.
- null-forgiving escape hatch는 허용하되 lint 대상이 된다.

Lowering:
- non-null은 metadata만으로 완전히 보장할 수 없으므로 compiler check와 runtime guard를 조합한다.
- `Option<T>`는 표준 라이브러리 타입으로 제공한다.

위험:
- 기존 .NET Framework assembly는 nullable metadata가 없을 수 있다.
- reflection, dynamic, COM interop는 null 안정성을 약화시킨다.

## 2. Union Type과 Discriminated Union

상태: MVP for F#-style nominal closed union, MVP for TS-style local type-level union, Preview Watch for C# 15 style open union

출처:
- F# discriminated union
- TypeScript union type
- C# 15 preview union type

TypeSharp 결정:
- TypeSharp의 공식 런타임 union은 F#식 nominal closed union이다.
- `union` 선언은 tag, case payload, runtime representation, public .NET API 노출 규칙을 가진다.
- nominal closed union은 pattern matching에서 exhaustiveness를 진단한다.
- TypeScript식 `A | B` union은 local inference, literal union, `unknown` narrowing, structural shape union을 위한 compile-time type-level union으로 사용한다.
- type-level union은 public .NET metadata에 직접 노출하지 않는다.
- public API에 type-level union이 나타나면 nominal union, nominal interface, wrapper 중 하나로 명시적으로 닫으라는 diagnostic을 낸다.
- C# 15 preview union은 방향성만 추적하며 TypeSharp의 안정 ABI 계약으로 삼지 않는다.

Lowering 후보:
- MVP class hierarchy: abstract base class + sealed case class 계열로 낮춘다. interop가 쉽고 C# 7.3 source backend로 구현 가능하지만 allocation이 늘 수 있다.
- tagged struct: 성능이 좋을 수 있으나 boxing, generic, metadata 표현이 복잡하므로 Stable Backlog로 둔다.
- generated closed type: 제어가 쉽지만 public ABI가 장황해질 수 있으므로 Stable Backlog로 둔다.
- type-level union: 기본적으로 erase되거나 narrowing proof로만 남는다. public boundary에서는 MVP에서 diagnostic을 내고, 사용자가 wrapper/interface/nominal union을 명시적으로 작성한다.

위험:
- C#에서 TypeSharp union을 자연스럽게 소비하는 API 디자인이 필요하다.
- generic nominal union의 equality/hash semantics가 복잡하다.
- TypeScript식 union을 너무 넓게 허용하면 .NET metadata와 diagnostics가 복잡해진다.

## 3. Pattern Matching

상태: MVP

출처:
- C# pattern matching
- F# match expression
- TypeScript discriminated union narrowing

TypeSharp 결정:
- `match`는 expression이어야 한다.
- literal, type, nominal union case, tuple, record shape 패턴을 단계적으로 지원한다.
- match 결과 타입은 모든 branch의 공통 타입 또는 union으로 추론한다.
- nominal closed union의 누락 case와 도달 불가능 branch를 진단한다.
- structural/type-level union narrowing은 exhaustiveness를 보장할 수 있는 범위부터 단계적으로 지원한다.

Lowering:
- union tag switch, type test, equality comparison, nested guard로 낮춘다.
- 복잡한 패턴은 decision tree로 변환한다.

## 4. Structural Type

상태: MVP for local shape checking, Stable Backlog for public ABI

출처:
- TypeScript object type, union/intersection type, literal type
- C# duck-typing 없음에 대한 보완

TypeSharp 결정:
- 구조적 타입은 기본적으로 컴파일 타임 shape constraint다.
- public .NET metadata에는 MVP에서 직접 노출하지 않고 diagnostic을 낸다. nominal interface 또는 generated adapter 노출은 Stable Backlog로 둔다.
- structural type은 reflection 기반 late binding이 아니라 정적 member resolution으로 처리한다.
- intersection type은 MVP 이후로 미룬다.

Lowering:
- compile-time proof가 있으면 원래 객체 타입을 유지한다.
- 외부 boundary에서는 MVP에서 명시 nominal interface/wrapper 작성을 요구한다. interface generation 또는 wrapper generation은 Stable Backlog다.

위험:
- overload resolution과 structural matching이 충돌할 수 있다.
- public API에서 구조적 타입을 그대로 표현할 방법이 없다.

## 5. Type Inference

상태: MVP

출처:
- F# local inference
- TypeScript contextual typing
- C# local type inference와 target-typed 표현식

TypeSharp 결정:
- local binding과 lambda는 타입 추론을 지원한다.
- public API boundary는 명시 타입을 권장하고, 안정 release 전에는 필수로 둘 수 있다.
- overload resolution은 추론 결과가 불안정하면 명시 annotation을 요구한다.
- 추론 실패 메시지는 "어디에 타입을 적으면 되는지"를 알려야 한다.

## 6. Extension-Like Members

상태: Stable Backlog

출처:
- C# 14 extension members

TypeSharp 결정:
- 기존 타입에 인스턴스/정적 확장 멤버를 추가하는 문법을 검토한다.
- .NET metadata에는 static helper method로 낮춘다.
- 충돌 resolution 규칙을 명확히 정의하기 전까지 MVP에는 넣지 않는다.

Lowering:
- C# extension method와 유사한 static method emit.
- extension property는 getter/setter method로 emit.

위험:
- 기존 instance member와 extension member 우선순위가 혼란을 만들 수 있다.
- C# 소비자에게 extension property가 자연스럽게 보이지 않는다.

## 7. Async와 Task Workflow

상태: MVP for basic `Task`, Stable Backlog for workflow syntax

출처:
- C# async/await
- F# task expression과 `and!`
- TypeScript Promise 기반 비동기 모델

TypeSharp 결정:
- 기본 async interop 타입은 `Task`와 `Task<T>`다.
- async function은 .NET Framework에서 동작해야 한다.
- F# `and!`에서 영감을 받은 concurrent binding은 Stable Backlog로 둔다.
- cancellation은 `CancellationToken`을 명시적으로 전달하는 방식을 권장한다.

Lowering:
- C# 스타일 state machine emit 또는 C# source backend 위임.
- 초기 구현은 C# 7.3 source generation backend가 더 단순할 수 있다.

## 8. Collection Expression

상태: MVP for simple array/list literal, Stable Backlog for advanced collection expression, C# 15 collection argument는 Preview Watch

출처:
- C# collection expression과 C# 15 collection expression arguments
- F# list/array/seq expression
- TypeScript array/object literal

TypeSharp 결정:
- 기본 배열/리스트 literal은 MVP 문법으로 지원한다.
- 사전 literal, spread, target-specific builder는 Stable Backlog로 둔다.
- capacity/comparer 같은 생성 인자는 preview feature로 분리한다.
- target type이 없는 homogeneous collection literal은 MVP에서 `T[]`로 추론한다.
- 빈 collection literal `[]`은 contextual type 또는 명시 타입 주석을 요구한다.

Lowering:
- array literal, `List<T>` constructor, collection initializer, helper factory 중 target type에 따라 선택한다.

## 9. Record와 Immutable Data

상태: MVP for record-like product type

출처:
- C# record
- F# record
- TypeScript object type

TypeSharp 결정:
- record는 immutable property, value equality, copy/update 문법을 제공한다.
- public .NET API는 class 또는 struct 중 명시 선택하게 한다.
- with-copy는 generated method로 낮춘다.

위험:
- equality semantics가 reference type interop와 충돌할 수 있다.

## 10. Module과 Namespace

상태: MVP

출처:
- F# module
- C# namespace
- TypeScript module/import model

TypeSharp 결정:
- namespace는 .NET metadata namespace와 대응한다.
- module은 static class 또는 generated container type으로 낮춘다.
- 파일 시스템 경로와 namespace가 항상 같아야 하는지는 프로젝트 정책으로 둔다.
- import는 명시적이어야 하며 wildcard import는 제한한다.

## 11. Diagnostics와 경고 제어

상태: MVP

출처:
- F# 10 `#warnon`/`#nowarn` 개선
- TypeScript strict defaults
- C# compiler diagnostics

TypeSharp 결정:
- 모든 diagnostic은 안정적인 code를 가진다.
- warning suppression은 좁은 scope를 기본으로 한다.
- project-wide suppression은 감사 가능해야 한다.
- strict mode를 기본값으로 삼고, compatibility relaxation은 opt-in으로 둔다.

## 12. Span/Memory 계열

상태: Stable Backlog

출처:
- C# 14 Span 관련 언어 지원

TypeSharp 결정:
- .NET Framework에서는 `System.Memory` 패키지와 런타임 특성을 검증해야 한다.
- 안전한 stack-only/ref-like type 모델은 CLR/컴파일러 제약이 크므로 초기 MVP에서 제외한다.
- 라이브러리 interop를 먼저 지원하고 언어 문법은 나중에 검토한다.

## 13. TypeScript 6.0/7.0 도구 철학

상태: Stable Backlog

출처:
- TypeScript 6.0 strict defaults, explicit `types`, 7.0 native compiler/beta defaults

TypeSharp 결정:
- 새 프로젝트는 strict mode가 기본이다.
- ambient/global import는 명시해야 한다.
- language server와 compiler core는 semantic model을 공유한다.
- 병렬 type checking은 장기 목표로 두되, deterministic output을 먼저 보장한다.

## 14. C# Library Interop

상태: MVP for managed `net481` library interop, Stable Backlog for NuGet restore and extension-method instance sugar

출처:
- C#/.NET metadata model
- .NET Framework class library
- TypeScript식 explicit module import

TypeSharp 결정:
- 기존 C#/.NET Framework assembly를 manifest reference로 읽고, metadata symbol을 TypeSharp source symbol과 같은 semantic model에서 다룬다.
- `import { Type } from "Namespace"` 문법으로 C# namespace/type을 가져온다.
- C# constructor, static/instance member, property, field, indexer, delegate, event, generic type을 MVP interop 대상으로 둔다.
- overload resolution은 C# 소비자가 예측하기 쉬운 nominal-first ranking을 사용한다.
- nullable annotation이 없는 C# assembly는 unknown nullability로 다루고 strict mode에서 warning 또는 guard 요구 diagnostic을 낸다.
- `dynamic`, reflection, COM, P/Invoke는 capability marker 없이 암묵적으로 허용하지 않는다.
- NuGet restore, lock file, license inventory까지 포함한 package reference는 Stable Backlog로 둔다.

Lowering:
- MVP backend는 C# 7.3 source generation이므로 managed interop 호출은 generated C#의 일반 member call로 낮춘다.
- `ref`/`out`/`in`, optional parameter, `params`, delegate conversion은 C# metadata 정보를 보존해 generated C# call site에 반영한다.
- TypeSharp public API는 C#에서 이해 가능한 nominal metadata로만 노출한다.

위험:
- nullable metadata가 없는 legacy assembly는 null 안전성 보장이 약하다.
- overload resolution에 structural proof를 너무 빨리 섞으면 C# interop 호출이 예측하기 어려워진다.
- NuGet restore를 compiler가 직접 실행하면 build reproducibility와 보안 정책이 복잡해진다.

## 15. Compile-Time Literal Constant

상태: MVP

출처:
- C# `const` field와 attribute constant requirement
- F# literal value philosophy
- TypeScript `as const` literal preservation 철학

TypeSharp 결정:
- TypeSharp는 standalone `const` binding keyword를 도입하지 않는다.
- 일반 불변 값은 `let`, 가변 값은 `let mut`, 컴파일 타임 상수는 `literal` declaration으로 표현한다.
- `literal` initializer는 primitive, string, enum, null, 또는 compiler가 constant expression으로 증명할 수 있는 값으로 제한한다.
- public `literal`은 C# 소비자가 상수처럼 볼 수 있는 metadata literal 또는 equivalent field로 낮춘다.
- TypeScript식 `as const`는 literal type 보존 annotation 후보로 남기되, binding keyword로 확장하지 않는다.

Lowering:
- public metadata에 안전하게 노출 가능한 경우 C# `const` field와 동등한 literal metadata로 낮춘다.
- metadata literal로 표현할 수 없는 값은 diagnostic을 내거나 private generated readonly field로 낮추는 선택지를 별도 설계한다.
- attribute argument에 쓰이는 `literal`은 C# attribute argument 제한과 같은 constant expression 제한을 따른다.

위험:
- C# `const`는 호출자 assembly에 값이 bake-in될 수 있어 public ABI versioning 위험이 있다.
- 모든 immutable `let`을 constant로 오해하면 binary compatibility가 나빠진다.
- 따라서 `literal`은 명시 선언으로만 허용하고, 일반 `let`과 의미를 섞지 않는다.

## 지연 또는 거절 후보

| 기능 | 상태 | 이유 |
| --- | --- | --- |
| JavaScript emit 호환성 | Rejected for MVP | TypeSharp의 1차 타깃은 .NET Framework다. |
| TypeScript decorator 복제 | Rejected for MVP | .NET attribute와 충돌하며 JS 런타임 모델에 가깝다. |
| C# source generator-style compile-time execution | Rejected for MVP | 빌드 중 임의 코드 실행 위험이 크다. |
| F# type provider | Experimental | 강력하지만 compiler architecture, sandbox, cache, permission 정책이 필요하다. |
| macro system | Rejected for MVP | 도구 안정성과 보안 모델이 먼저 필요하다. |

