# TypeSharp 문서

문서 기준일: 2026-05-19

이 디렉터리는 TypeSharp 언어를 만들기 위한 목표, 요구사항, 문법 사양, 권장 아키텍처, 기능 매핑, 체크리스트를 모은다. 문서는 다음 순서로 읽는다.

## 읽는 순서

1. [goal.md](goal.md)
   - TypeSharp가 무엇을 만들려는 프로젝트인지 한 문장 과제, 기준선, 성공 조건, 비목표로 정의한다.

2. [agent.md](../agent.md)
   - Codex `/goal` 기반 장기 작업에서 에이전트가 목표를 상기하고 다음 일을 고르는 운영 지침이다.

3. [agentic-execution.md](agentic-execution.md)
   - Ralph mode, Goal mode, `/goal` 같은 장기 실행 모드에서 작업을 고르고 검증하고 인계하는 실행 계약이다.

4. [progress.md](progress.md)
   - 장기 실행 작업의 task packet, rollup, commit, 인계 기록 정책을 정의한다.

5. [adr/README.md](adr/README.md)
   - 장기 설계 결정을 남기는 Architecture Decision Record 형식과 사용 기준을 정의한다.

6. [requirements.md](requirements.md)
   - 플랫폼, 언어 핵심, 컴파일러, 상호 운용, 표준 라이브러리, 도구, 품질, 보안 필수사항을 정의한다.

7. [grammar/README.md](grammar/README.md)
   - TypeSharp가 가져야 할 문법을 lexical, module, declaration, type, expression, pattern, interop, name resolution 단위로 정의한다.

8. [grammar/coverage.md](grammar/coverage.md)
   - TypeScript, F#, C# 기능을 TypeSharp 문법이 직접 포괄하거나 TypeSharp식 기능으로 대체하는지 추적한다.

9. [examples/README.md](examples/README.md)
   - 현재 문법을 기준으로 TypeSharp `.tysh` 코드가 실제로 어떻게 보일지 대표 예제를 보여준다.

10. [standard-library.md](standard-library.md)
   - `TypeSharp.Core`, `TypeSharp.Collections`, `TypeSharp.Runtime`의 초기 namespace 정책과 핵심 타입을 정의한다.

11. [csharp-interop.md](csharp-interop.md)
   - 기존 C#/.NET Framework 라이브러리 참조, 호출, public ABI 노출 계약을 정의한다.

12. [cli.md](cli.md)
   - `typesharp` command surface, project manifest, diagnostics format, exit code, source discovery 규칙을 정의한다.

13. [formatting.md](formatting.md)
   - `.tysh` source의 공식 formatter convention과 `typesharp format --check`의 기준 layout을 정의한다.

14. [diagnostics.md](diagnostics.md)
   - diagnostic code range, descriptor metadata, explanation surface, golden diagnostic fixture 정책을 정의한다.

15. [lowering.md](lowering.md)
   - 현재 구현된 TypeSharp 기능이 C# 7.3-compatible `net48` source로 어떻게 낮아지는지 기능별 예제와 fixture 근거를 정리한다.

16. [migration-guide.md](migration-guide.md)
   - 기존 .NET Framework 4.8/C# 코드베이스가 TypeSharp library를 점진적으로 도입하는 방법과 현재 미지원 자동화를 정리한다.

17. [feasibility.md](feasibility.md)
   - 현재 설계의 실현 가능성, MVP로 낮춘 범위, backend/host/ABI 결정을 기록한다.

18. [feature-map.md](feature-map.md)
   - 최신 C#, F#, TypeScript 기능을 TypeSharp에서 MVP, Stable Backlog, Preview Watch, Experimental, Rejected로 분류한다.

19. [architecture.md](architecture.md)
   - compiler pipeline, backend 전략, runtime library, tooling, test strategy의 권장 구조를 제안한다.

20. [checklist.md](checklist.md)
   - 설계와 구현을 반복할 때 완료 여부를 확인하는 실행 체크리스트다.

21. [parser-fixtures.md](parser-fixtures.md)
   - compiler skeleton 전에 parser positive/negative fixture, diagnostics snapshot, syntax tree snapshot 형식을 고정한다.

22. [tasks/README.md](tasks/README.md)
   - 장기 실행 에이전트가 바로 집어 들 수 있는 작업 패킷 목록이다.

23. [traceability.md](traceability.md)
   - 목표, 요구사항, 기능, 체크리스트가 서로 어떻게 연결되는지 검증한다.

24. [references.md](references.md)
   - .NET Framework, C#, F#, TypeScript 최신 기준선의 공식 근거 링크를 기록한다.

25. [dependencies.md](dependencies.md)
   - generated assembly, runtime/core, compiler/CLI/test host dependency inventory와 `net48` compatibility audit 기준을 기록한다.

26. [framework-targeting.md](framework-targeting.md)
   - Windows 10/11 장비 벤더 환경에서 `net48`과 `net481` 중 어떤 .NET Framework 타깃을 선택할지 판단 기준을 기록한다.

27. [runtime-abi.md](runtime-abi.md)
   - `TypeSharp.Core`, `TypeSharp.Runtime`, generated `net48` assembly의 public ABI versioning 정책을 정의한다.

## 문서 원칙

- `goal.md`는 프로젝트의 북극성이다. 다른 문서가 goal과 충돌하면 goal을 먼저 수정하거나 충돌을 명시한다.
- `agent.md`는 `/goal` 기반 장기 실행 지침이다. 새 세션의 Codex는 루트 [agent.md](../agent.md)를 통해 목표, 반복 루프, 금지선을 복원한다.
- `agentic-execution.md`는 Ralph/Goal mode가 작업을 선택하고 검증하고 인계하는 구체적 실행 계약이다.
- `progress.md`는 task packet, rollup, commit, 인계 기록 정책을 정한다.
- `adr/`는 큰 설계 결정을 반복 논쟁 없이 이어받을 수 있게 하는 Architecture Decision Record 형식을 제공한다.
- `requirements.md`는 "반드시 지켜야 할 것"을 다룬다. 구현 편의나 선호는 `architecture.md`에 둔다.
- `grammar/`는 TypeSharp 문법의 실제 설계 표면이다. 새 문법은 [grammar/coverage.md](grammar/coverage.md)에서 TypeScript, F#, C# 기능과 연결되어야 한다.
- [grammar/consistency.md](grammar/consistency.md)는 새 문법을 추가하기 전에 먼저 확인해야 하는 공통 표기 규칙이다.
- `examples/`는 문법을 사용자가 보는 `.tysh` 코드로 검증하는 곳이다. 예제가 문법 문서와 충돌하면 문법 또는 예제 중 하나를 수정한다.
- `standard-library.md`는 예제와 compiler-generated code가 공유해야 할 runtime namespace와 core type 이름을 정한다.
- `csharp-interop.md`는 C# library reference, metadata symbol, nullable, overload, public ABI 경계를 정한다.
- `cli.md`는 VS Code/CI/사용자가 공유하는 CLI command, manifest, diagnostics 계약을 정한다.
- `formatting.md`는 parser, formatter, LSP, 예제 파일이 공유하는 canonical layout을 정한다.
- `diagnostics.md`는 CLI, VS Code, test fixtures가 공유하는 diagnostic code와 explanation metadata를 정한다.
- `lowering.md`는 구현된 기능의 TypeSharp source, generated C# shape, runtime/helper 의존성, fixture 근거를 연결한다.
- `migration-guide.md`는 기존 .NET Framework/C# 환경에서 TypeSharp를 점진적으로 도입하는 절차와 현재 미지원 자동화를 설명한다.
- `feasibility.md`는 과도한 약속을 MVP, Stable Backlog, Preview Watch, Experimental, Rejected 중 적절한 범위로 낮추고 실제 구현 순서를 정리한다.
- `feature-map.md`는 외부 언어의 최신 기능을 TypeSharp 의미론으로 번역하는 곳이다.
- `checklist.md`는 실제 반복 작업의 입구다. 체크되지 않은 항목은 다음 설계 또는 구현 작업 후보가 된다.
- `parser-fixtures.md`는 parser 구현 전에 fixture layout, expected diagnostics, syntax tree snapshot 형식을 고정한다.
- `tasks/`는 한 세션보다 긴 작업을 task packet으로 쪼개고 상태를 남기는 곳이다.
- `references.md`는 시간이 지나면 갱신해야 한다. 최신 언어 버전은 반드시 공식 문서로 다시 확인한다.
- `dependencies.md`는 package-free runtime/core surface와 future dependency gate를 추적한다.
- `framework-targeting.md`는 의료기기/분석기기 벤더 호환성처럼 설치 기반이 중요한 환경에서 `net48`/`net481` 선택 근거를 추적한다.
- `runtime-abi.md`는 compiler/runtime ABI version alignment, breaking ABI change 기준, compatibility gate를 추적한다.

## 반복 검토 프로토콜

문서를 바꿀 때마다 다음 순서로 확인한다.

1. 목표 확인: 변경이 [goal.md](goal.md)의 한 문장 과제와 맞는가?
2. 에이전트 확인: [agent.md](../agent.md)의 반복 실행 규칙과 금지선을 지키는가?
3. 장기 실행 확인: [agentic-execution.md](agentic-execution.md)의 queue, task packet, Done 기준과 맞는가?
4. 요구사항 확인: 필수사항을 새로 만들거나 깨뜨리지 않는가?
5. 문법 확인: [grammar/README.md](grammar/README.md)의 문법 원칙과 충돌하지 않는가?
6. 커버리지 확인: [grammar/coverage.md](grammar/coverage.md)에 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 분류했는가?
7. 표준 라이브러리 확인: [standard-library.md](standard-library.md)의 namespace와 core type 이름을 지키는가?
8. CLI 확인: [cli.md](cli.md)의 command, manifest, diagnostics 계약을 깨뜨리지 않는가?
9. 실현 가능성 확인: [feasibility.md](feasibility.md)의 MVP host/backend/ABI 결정과 충돌하지 않는가?
10. 기능 상태 확인: MVP, Stable Backlog, Preview Watch, Experimental, Rejected 중 하나로 분류했는가?
11. lowering 확인: .NET Framework 4.8에서 실행 가능한 표현으로 낮출 수 있는가?
12. interop 확인: C#/.NET Framework 소비자가 이해할 public API인가?
13. 테스트 확인: 체크리스트에 positive, negative, interop 검증이 반영되어 있는가?
14. 근거 확인: 최신 외부 언어 기능을 언급했다면 공식 링크와 기준일이 있는가?

## 현재 결론

현재 문서 세트는 TypeSharp의 초기 설계 방향과 문법 표면을 잡기에 충분한 구조를 갖춘다. 남은 미해결 항목은 문서 누락이 아니라 구현 전 결정해야 할 설계 선택과 세부 문법 확장이다.

주요 열린 결정:
- [grammar/coverage.md](grammar/coverage.md)에 새로 발견되는 TypeScript/F#/C# 기능을 계속 분류하는 작업
- 직접 IL backend를 도입할 시점과 범위
- nominal closed union의 런타임 표현
- TypeScript식 type-level union의 public boundary diagnostic과 수동 대체 가이드
- structural type의 public API adapter를 언제 자동 생성할지 여부
- CLI manifest를 장기적으로 `TypeSharp.toml`로 유지할지 MSBuild와 1급 통합할지 여부

