# TypeSharp 에이전트 지침

문서 기준일: 2026-05-18

이 문서는 Codex가 `/goal`, Ralph mode, 또는 장기 작업 세션을 통해 TypeSharp 과제를 계속 수행할 때 읽어야 하는 운영 지침이다. 목표의 원문은 [docs/goal.md](docs/goal.md)에 있고, 장기 실행 계약은 [docs/agentic-execution.md](docs/agentic-execution.md)에 있다. 이 문서는 그 목표를 에이전트가 반복 실행 가능한 작업 규칙으로 바꾼다.

## 에이전트의 최상위 목표

TypeSharp는 .NET Framework 4.8.1용 산출물을 만들고 그 산출물이 .NET Framework 4.8.1에서 실행될 수 있으면서, 최신 C#, F#, TypeScript에서 검증된 표현력, 안전성, 도구 친화성을 하나의 일관된 정적 타입 언어로 통합하는 새 언어다.

에이전트는 모든 작업에서 이 문장을 먼저 기억해야 한다.

## `/goal`에 넣을 목표 문장

```text
TypeSharp를 .NET Framework 4.8.1용 산출물을 만들고 실행할 수 있으면서 TypeScript처럼 암묵적이고 유연한 타입 기능, TypeScript식 모듈 기반 파일 구조, F#의 함수형 기능과 일관성, C#의 다양하고 유연한 편의 기능 및 기존 C#/.NET Framework 라이브러리 상호 운용성을 통합한 새 정적 타입 언어로 설계하고 구현한다. TypeScript, F#, C#의 실용 기능을 TypeSharp 문법으로 직접 포괄하거나 TypeSharp식 기능으로 대체할 수 있을 때까지 grammar coverage를 계속 확장한다. 목표 문서, 요구사항, 문법 사양, C# library interop 계약, 기능 매핑, 아키텍처, 실현 가능성 검토, 체크리스트를 기준으로 문서화, 설계 결정, 컴파일러/런타임 구현, 테스트, 검증을 반복하며 과제가 실제로 실행 가능한 상태가 될 때까지 계속 진행한다.
```

## Ralph/Goal mode 실행 문장

```text
TypeSharp 장기 작업을 시작할 때는 docs/agentic-execution.md의 부트스트랩 순서와 작업 선택 규칙을 따른다. 각 반복은 checklist.md의 미완료 항목 하나를 선택하고, goal.md/feasibility.md/traceability.md와 충돌하지 않게 문서, 사양, 코드, 테스트 중 필요한 산출물을 만든다. 완료 시 검증 결과와 남은 작업을 checklist.md, traceability.md, 관련 사양 문서에 반영한다.
```

## 작업 시작 루틴

새 세션이나 새 목표 실행이 시작되면 다음 순서로 읽는다.

1. [docs/goal.md](docs/goal.md)
2. [docs/README.md](docs/README.md)
3. [docs/agentic-execution.md](docs/agentic-execution.md)
4. [docs/grammar/README.md](docs/grammar/README.md)
5. [docs/grammar/consistency.md](docs/grammar/consistency.md)
6. [docs/grammar/coverage.md](docs/grammar/coverage.md)
7. [docs/standard-library.md](docs/standard-library.md)
8. [docs/csharp-interop.md](docs/csharp-interop.md)
9. [docs/cli.md](docs/cli.md)
10. [docs/feasibility.md](docs/feasibility.md)
11. [docs/checklist.md](docs/checklist.md)
12. [docs/tasks/README.md](docs/tasks/README.md)
13. [docs/traceability.md](docs/traceability.md)
14. 현재 작업과 직접 관련된 문서

읽은 뒤에는 다음 질문에 답한 다음 움직인다.

- 지금 작업은 [docs/goal.md](docs/goal.md)의 한 문장 과제에 직접 기여하는가?
- 지금 작업은 VS Code와 CLI만으로 개발 가능한 TypeSharp 개발 루프를 진전시키는가?
- 지금 작업은 TypeScript식 유연성, TypeScript식 module graph, F#식 함수형 일관성, C#식 편의성 중 어느 축을 진전시키는가?
- 문법 관련 작업이라면 TypeScript/F#/C#의 어떤 기능을 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 분류했는가?
- 새 문법을 추가했다면 [docs/grammar/consistency.md](docs/grammar/consistency.md)의 공통 기호 규칙을 먼저 만족하는가?
- 표준 라이브러리 타입이나 helper를 사용한다면 [docs/standard-library.md](docs/standard-library.md)의 namespace 정책을 따르는가?
- C# library interop를 건드린다면 [docs/csharp-interop.md](docs/csharp-interop.md)의 reference, overload, nullable, public ABI 계약을 따르는가?
- CLI나 개발 루프를 건드린다면 [docs/cli.md](docs/cli.md)의 command, manifest, diagnostics 계약을 따르는가?
- 구현 범위를 늘린다면 [docs/feasibility.md](docs/feasibility.md)의 MVP, Stable Backlog, Preview Watch, Experimental, Rejected 경계와 충돌하지 않는가?
- union 관련 작업이라면 F#식 nominal closed union을 런타임/도메인 모델로, TS식 type-level union을 compile-time 표현과 narrowing으로 분리했는가?
- 지금 작업은 체크리스트의 어떤 미완료 항목을 줄이는가?
- 장기 실행 작업이라면 [docs/agentic-execution.md](docs/agentic-execution.md)의 queue, task packet, Done 기준 중 무엇에 해당하는가?
- 지금 작업이 .NET Framework 4.8.1 호환성을 깨뜨릴 가능성이 있는가?
- 최신 C#, F#, TypeScript 기능을 언급한다면 안정 기능과 preview 기능을 분리했는가?

## 반복 실행 규칙

에이전트는 과제를 한 번의 문서 작성으로 끝났다고 간주하지 않는다. 다음 루프를 계속 반복한다.

1. 목표 확인
   - [docs/goal.md](docs/goal.md)의 목표, 성공 조건, 비목표를 다시 확인한다.

2. 다음 미완료 항목 선택
   - [docs/agentic-execution.md](docs/agentic-execution.md)의 우선순위 큐를 적용한다.
   - [docs/checklist.md](docs/checklist.md)에서 아직 체크되지 않은 항목 중 가장 앞선 의존성을 고른다.

3. 근거 확인
   - 플랫폼 또는 최신 언어 버전 정보가 필요하면 [docs/references.md](docs/references.md)의 공식 문서를 기준으로 갱신한다.

4. 설계 또는 구현
   - 문서가 부족하면 문서를 먼저 구체화한다.
   - 문법이 부족하면 [docs/grammar/](docs/grammar/README.md)에 syntax, 예제, parser 주의사항, coverage 상태를 남긴다.
   - 표준 라이브러리 이름이나 helper가 필요하면 [docs/standard-library.md](docs/standard-library.md)를 먼저 갱신한다.
   - 구현이 가능하면 코드, 테스트, 샘플을 만든다.
   - 설계 결정이 막히면 선택지, 장단점, 권장안을 문서화한다.

5. 검증
   - 가능한 테스트를 실행한다.
   - 테스트가 아직 없으면 최소한 문서 링크, 체크리스트, 추적성을 검증한다.

6. 상태 갱신
   - 완료된 항목은 체크리스트에 반영한다.
   - 새로 드러난 미해결 문제는 관련 문서의 열린 결정 또는 체크리스트에 추가한다.

## 의사결정 우선순위

1. .NET Framework 4.8.1 호환성
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
- C#, F#, TypeScript 기능을 이름만 보고 1:1로 복제하지 않는다.
- `net481` 호환성 검증 없이 dependency를 추가하지 않는다.
- 문서와 체크리스트를 갱신하지 않고 큰 설계 결정을 숨기지 않는다.
- preview 기능을 안정 목표처럼 작성하지 않는다.
- 빌드 중 임의 사용자 코드를 실행하는 확장 모델을 기본 설계로 두지 않는다.

## 장기 작업 모드

이 프로젝트는 에이전틱하게 오래 수행될 수 있도록 설계한다. 에이전트는 매번 "작업을 끝낸다"가 아니라 "목표에 더 가까워진 상태를 남긴다"는 관점으로 움직인다.

좋은 종료 상태:
- 무엇을 바꿨는지 문서 또는 코드에 남아 있다.
- 다음에 할 일이 체크리스트나 열린 결정에 남아 있다.
- 검증한 내용과 검증하지 못한 내용이 분리되어 있다.
- 장기 작업이면 task packet 또는 인계 기준에 맞게 남은 일을 설명할 수 있다.
- [docs/goal.md](docs/goal.md)의 성공 조건 중 어느 항목을 진전시켰는지 설명할 수 있다.

## 현재 우선순위

현재는 compiler skeleton 위에서 semantic fixtures와 첫 C# source backend golden output을 runtime/build 경로로 확장하는 단계다. 다음 큰 결정은 아래 순서로 처리한다.

1. Grammar coverage 확장: TypeScript/F#/C# 기능을 직접 포괄, 대체, 실험, 거절 중 하나로 계속 분류
2. Parser-friendly grammar 유지: parser precedence table과 새 ambiguity가 생기면 [docs/grammar/ambiguity.md](docs/grammar/ambiguity.md)에 반영
3. C# byref interop smoke: TypeSharp source compiles a narrow imported C# `ref` or `in` call shape in the generated `net481` project.
4. C# library interop 구현 범위 확정: framework assembly, local DLL, overload, nullable, public ABI fixture
5. Standard library core surface 확정: `TypeSharp.Core`, `TypeSharp.Collections`, `TypeSharp.Runtime`
6. C# reference resolver and metadata reader expansion
7. C# 7.3 source backend 구현 확대
8. CLI command 구현 순서와 project manifest validation 확정
9. VS Code extension/LSP 최소 구조 확정
10. nominal closed union 런타임 표현 선택
11. TypeScript식 type-level union의 public boundary 정책 확정
12. structural type public boundary 정책
13. MSBuild 통합 전략
14. IL backend 도입 조건 정의

이 결정들이 내려지면 컴파일러 골격, 런타임 라이브러리, CLI, 테스트 프로젝트를 만들기 시작한다.

