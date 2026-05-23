# TypeSharp Agent Notes

문서 기준일: 2026-05-23

이 파일은 TypeSharp 작업을 시작할 때 참고할 최소 기준만 남긴다.

## 에이전트의 최상위 목표

TypeSharp는 .NET Framework 4.8용 산출물을 만들고 그 산출물이 .NET Framework 4.8에서 실행될 수 있으면서, 최신 C#, F#, TypeScript에서 검증된 표현력, 안전성, 도구 친화성을 하나의 일관된 정적 타입 언어로 통합하는 새 언어다.

TypeSharp를 .NET Framework 4.8용 산출물을 만들고 실행할 수 있으면서 TypeScript처럼 암묵적이고 유연한 타입 기능, TypeScript식 모듈 기반 파일 구조, F#의 함수형 기능과 일관성, C#의 다양하고 유연한 편의 기능 및 기존 C#/.NET Framework 라이브러리 상호 운용성을 통합한 새 정적 타입 언어로 설계하고 구현한다. TypeScript, F#, C#의 실용 기능을 TypeSharp 문법으로 직접 포괄하거나 TypeSharp식 기능으로 대체할 수 있을 때까지 grammar coverage를 계속 확장한다. 목표 문서, 요구사항, 문법 사양, C# library interop 계약, 기능 매핑, 아키텍처, 실현 가능성 검토, 체크리스트를 기준으로 문서화, 설계 결정, 컴파일러/런타임 구현, 테스트, 검증을 반복하며 과제가 실제로 실행 가능한 상태가 될 때까지 계속 진행한다.

에이전트는 모든 작업에서 이 문장을 먼저 기억해야 한다.

## 기본 기준

- 표준 목표와 요구사항은 `docs/src/content/docs/goal.md`와 `docs/src/content/docs/requirements.md`를 기준으로 본다.
- 언어 구현 상태와 1.0 전 남은 항목은 `agent/lang-1.0-tasks.md`를 본다.
- 필요한 경우에만 관련 canonical docs와 테스트를 확인하고 갱신한다.

## 작업 원칙

- 기존 사용자 변경을 되돌리지 않는다.
- 코드/문서 변경은 요청 범위에 맞게 작게 유지한다.
- 검증 가능한 변경이면 관련 build/test/docs 명령을 실행하거나, 실행하지 못한 이유를 남긴다.

## 의사결정 우선순위

1. .NET Framework 4.8 및 기존 ASP.NET/WCF/worker host 호환성
2. VS Code와 CLI 개발 가능성
3. C#/.NET 상호 운용성
4. TypeScript식 암묵성과 유연성
5. TypeScript식 명시적 module graph
6. F#식 함수형 일관성
7. C#식 편의성과 .NET 친화성
8. 타입 안전성
9. 설명 가능한 lowering
10. 도구 친화성
11. 구현 단순성

표현력이 좋아도 .NET Framework에서 안정적으로 낮출 수 없으면 기본 기능으로 채택하지 않는다. preview 기능은 반드시 `Preview Watch` 또는 feature gate로 분리한다.

## 에이전트가 지켜야 할 금지선

- .NET 10/11 전용 런타임 기능을 TypeSharp 기본 요구사항으로 만들지 않는다.
- Python 관련 행위나 작업을 하지 않는다. Python 스크립트 작성/실행, `python`/`py`/`pip`/`venv`/Python 기반 도구 사용, Python 의존성 추가, Python 워크플로 문서화를 하지 않는다.
- Windows 10 기본 설정과 이미 설치된 `dotnet`, `node`만으로 가능한 작업 경로를 선택한다. 새 언어 런타임이나 별도 전역 도구 설치를 전제로 설계하거나 검증하지 않는다.
- C#, F#, TypeScript 기능을 이름만 보고 1:1로 복제하지 않는다.
- `net48` 호환성 검증 없이 dependency를 추가하지 않는다.
- 문서와 체크리스트를 갱신하지 않고 큰 설계 결정을 숨기지 않는다.
- preview 기능을 안정 목표처럼 작성하지 않는다.
- 빌드 중 임의 사용자 코드를 실행하는 확장 모델을 기본 설계로 두지 않는다.
