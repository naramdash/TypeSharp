# 공식 근거와 추적 링크

문서 기준일: 2026-05-19

TypeSharp는 "최신 언어 기능"을 목표로 하므로 외부 언어 버전과 플랫폼 상태가 변할 수 있다. 이 문서는 현재 문서 작성 시점의 기준선과 추적해야 할 공식 링크를 모은다.

## .NET Framework

- .NET Framework 설치 및 지원 버전
  - https://learn.microsoft.com/en-us/dotnet/framework/install/on-windows-and-server
  - 현재 기준: 최신 .NET Framework는 4.8.1이며 Windows 전용 기술이다.

- .NET Framework 버전과 종속성
  - https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies
  - 현재 기준: .NET Framework 4.8.1은 CLR 4 계열이다.
  - Windows 기본 포함 버전과 `net48`/`net481` 배포 판단은 [framework-targeting.md](framework-targeting.md)에 따로 기록한다.

- Windows 10 Home and Pro lifecycle
  - https://learn.microsoft.com/en-us/lifecycle/products/windows-10-home-and-pro
  - 현재 기준: Windows 10 Home/Pro 22H2의 일반 지원은 2025-10-14에 종료되었지만, 장비 벤더 환경에서는 vendor qualification, LTSC, ESU, offline 운용으로 설치 기반이 계속 남을 수 있다.

## C#

- C# language versioning
  - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning
  - 현재 기준: .NET Framework 타깃의 기본 C# 언어 버전은 C# 7.3이다. C# 14는 .NET 10, C# 15는 .NET 11 이상 계열이다.

- What's new in C# 14
  - https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14
  - 현재 기준: C# 14는 최신 안정 C# 릴리스다.
  - 추적 기능: extension members, null-conditional assignment, unbound generic `nameof`, Span conversion, lambda parameter modifiers, `field` backed properties, partial events/constructors, compound assignment operators, file-based app directives.

- What's new in C# 15
  - https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15
  - 현재 기준: C# 15는 최신 preview C# 릴리스다.
  - 추적 기능: collection expression arguments, union types.

## F#

- What's new in F# 10
  - https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10
  - 현재 기준: F# 10은 .NET 10 및 Visual Studio 2026과 함께 제공된다.
  - 추적 기능: scoped warning suppression, accessor modifiers on auto properties, ValueOption optional parameters, computation expression tail-call support, task expression `and!`, trimming improvement, parallel compilation preview, type subsumption cache.

## TypeScript

- Announcing TypeScript 6.0
  - https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/
  - 현재 기준: TypeScript 6.0은 안정 릴리스이며 TypeScript 7.0 전환을 준비하는 릴리스다.
  - 추적 기능: `es2025` target/lib, Temporal types, improved module resolution, `stableTypeOrdering`, stricter defaults and deprecations.

- Announcing TypeScript 7.0 Beta
  - https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/
  - 현재 기준: TypeScript 7.0 beta는 native compiler와 병렬화 방향을 보여주는 preview 추적 대상이다.

## 추적 규칙

- 안정 기준선은 release 문서 또는 Microsoft Learn 공식 문서에 올라온 안정 릴리스만 사용한다.
- preview 기능은 `Preview Watch`로 분류하고 기본 안정 기능으로 문서화하지 않는다.
- 기준선은 release마다 날짜와 함께 갱신한다.
- 외부 기능을 TypeSharp에 채택할 때는 반드시 다음을 기록한다.
  - 원 출처
  - TypeSharp 의미론
  - .NET Framework lowering
  - interop 노출
  - 테스트 전략
  - 안정성 상태

