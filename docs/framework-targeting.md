# .NET Framework 타깃 선택 기준

문서 기준일: 2026-05-19

이 문서는 TypeSharp가 의료기기, 분석 장비, 장기 운영 Windows 환경에서 어떤 .NET Framework 타깃을 기본 호환성 기준으로 삼아야 하는지 정리한다. 특히 SCIEX, Thermo Fisher 같은 장비 벤더가 .NET Framework 기반 SDK, 플러그인, 호스트 프로세스를 요구하는 경우를 고려한다.

## 결정 요약

- 장비 벤더 호환성을 우선하는 배포 profile에서는 `net48`을 기본 후보로 본다.
- `net481`은 최신 .NET Framework profile이지만, Windows 10과 초기 Windows 11 장비에서 기본 설치되어 있다고 가정하기 어렵다.
- TypeSharp compiler, CLI, language server host는 현대 .NET LTS 기반을 사용할 수 있다.
- TypeSharp generated assembly, `TypeSharp.Core`, `TypeSharp.Runtime`처럼 기존 .NET Framework host에 로드되는 산출물은 host PC의 설치 상태를 우선해야 한다.
- 현재 구현과 문서 세트의 주 기준선은 `net48`이다. `net481`은 최신 .NET Framework profile 검증 대상으로 추적한다.

## 왜 `net48`이 더 보수적인가

.NET Framework 4.x 계열은 CLR 4 기반의 in-place update다. 높은 버전의 .NET Framework가 설치된 PC는 낮은 target framework로 빌드된 앱을 실행할 수 있지만, 그 반대 방향은 권장되지 않는다. 따라서 `net48` 산출물은 .NET Framework 4.8.1이 설치된 PC에서도 실행 가능하지만, `net481` 산출물은 .NET Framework 4.8만 설치된 PC를 최소 환경으로 보기 어렵다.

장비 벤더 환경에서는 다음 제약이 더 중요하다.

- 장비 제어 PC는 벤더 검증 상태 때문에 OS나 .NET Framework 업데이트가 제한될 수 있다.
- 플러그인 또는 SDK 확장은 벤더 host process가 이미 로드한 .NET Framework 버전에 맞춰야 한다.
- 사용자가 제어하지 않는 실험실 PC, 병원 PC, 제조 현장 PC에서는 "설치 가능"보다 "이미 설치되어 있음"이 더 중요하다.
- `net481`의 최신성보다 `net48`의 설치 기반이 배포 실패를 줄일 가능성이 높다.

## Windows 기본 포함 버전

아래 표는 Microsoft Learn의 .NET Framework versions and dependencies 문서를 기준으로, Windows에 기본 포함되는 .NET Framework 버전과 TypeSharp 관점의 의미를 요약한다.

| Windows 버전 | 기본 포함 .NET Framework | TypeSharp 배포 의미 |
| --- | --- | --- |
| Windows 10 1507 | 4.6 | 최초 Windows 10까지 고려하려면 `net48`도 기본 설치 기준을 넘는다. 특별 벤더 profile 없이는 기본 목표로 삼지 않는다. |
| Windows 10 1511 | 4.6.1 | 4.6.1은 지원 종료 상태다. 특별 벤더 profile 없이는 기본 목표로 삼지 않는다. |
| Windows 10 1607 | 4.6.2 | 오래된 LTSC, Windows Server 2016 계열 장비에서 만날 수 있다. 필요 시 별도 `net462` compatibility profile로만 검토한다. |
| Windows 10 1703 | 4.7 | 일반 기본 목표로 삼지 않는다. |
| Windows 10 1709 | 4.7.1 | 일반 기본 목표로 삼지 않는다. |
| Windows 10 1803, 1809 | 4.7.2 | 일반 기본 목표로 삼지 않는다. |
| Windows 10 1903부터 22H2 | 4.8 | 장비 벤더 호환성 기준에서 `net48`을 기본 후보로 삼는 핵심 근거다. |
| Windows 11 21H2 | 4.8 | 초기 Windows 11 장비까지 고려하면 `net48`이 `net481`보다 안전하다. |
| Windows 11 22H2 이상 | 4.8.1 | 최소 OS가 이 범위로 고정된 경우 `net481`을 기본값으로 둘 수 있다. |

2026년 5월 기준으로 Windows 10 Home/Pro의 일반 지원은 2025-10-14에 종료되었다. 그러나 장비 벤더 환경에서는 지원 종료 Windows 10 PC가 vendor qualification, offline 운용, ESU, LTSC, 사내 검증 정책 때문에 계속 남아 있을 수 있다. TypeSharp는 공식 지원 상태와 현장 설치 기반을 분리해서 판단해야 한다.

## 타깃 선택 규칙

1. 기존 host process에 로드되는 assembly는 host가 보장하는 .NET Framework 버전보다 높은 TFM으로 빌드하지 않는다.
2. Windows 10/11 장비 벤더 호환성을 넓게 잡는 기본 profile은 `net48`로 둔다.
3. 최소 OS가 Windows 11 22H2 이상으로 명시된 폐쇄 환경에서는 `net481`을 사용할 수 있다.
4. Windows 10 1607, Windows Server 2016, 오래된 LTSC 장비까지 요구되면 `net462` 별도 profile을 검토하되 기본값으로 삼지 않는다.
5. `net48`보다 낮은 profile을 추가할 때는 최신 언어 기능 lowering, dependency, API surface가 크게 줄어드는 비용을 별도 문서화한다.
6. TypeSharp compiler/CLI/LSP host의 modern .NET target과 generated assembly/runtime target을 혼동하지 않는다.

## TypeSharp 적용 정책

TypeSharp의 장기 구조는 다음처럼 분리한다.

| 구성요소 | 권장 타깃 | 이유 |
| --- | --- | --- |
| `TypeSharp.Compiler` | 현대 .NET LTS | 최신 C#, Roslyn, CLI, LSP, NuGet 생태계를 활용한다. |
| `TypeSharp.Cli` | 현대 .NET LTS | 개발 도구는 장비 host process에 로드되지 않는다. |
| Language server | 현대 .NET LTS | VS Code와 CLI 개발 경험을 우선한다. |
| `TypeSharp.Core` | `net48` | 사용자 코드와 generated assembly가 참조하는 core ABI다. |
| `TypeSharp.Runtime` | `net48` | generated code helper surface는 장비 host에 로드될 수 있다. |
| Generated assembly | `net48` vendor compatibility profile, `net481` latest Framework profile | 배포 대상 host에 따라 선택한다. |

현재 구현과 문서의 기본 산출물 기준은 `net48`이다. `net481`은 최소 OS가 Windows 11 22H2 이상으로 고정된 환경에서 선택할 수 있는 최신 Framework profile로 남긴다.

## 검증 항목

- `net48` generated library를 Windows .NET Framework host project에서 참조하고 빌드한다.
- `net48` generated executable을 .NET Framework 4.8 이상 설치 환경에서 실행한다.
- `net481` smoke test는 최신 Framework profile 회귀 검증이 필요할 때 별도 profile로 추가한다.
- 외부 NuGet dependency는 `net48` asset 또는 `netstandard2.0` asset을 우선한다.
- vendor SDK DLL을 참조할 때는 SDK 문서의 minimum target과 실제 host process 설치 버전을 기록한다.
- `net48` profile에서는 .NET Framework 4.8에 없는 API를 runtime/core surface에 추가하지 않는다.

## 공식 근거

- Microsoft Learn, ".NET Framework versions and dependencies": https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies
- Microsoft Learn, ".NET Framework on Windows": https://learn.microsoft.com/en-us/dotnet/framework/install/on-windows-and-server
- .NET Framework official support policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework
- Windows 10 Home and Pro lifecycle: https://learn.microsoft.com/en-us/lifecycle/products/windows-10-home-and-pro
