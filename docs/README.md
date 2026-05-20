# TypeSharp 문서

문서 기준일: 2026-05-20

이 디렉터리는 task `0251-docs-site-canonical-language-ledger` 이후 표준 언어/프로젝트 원장이 아니라 agentic goal work를 위한 임시 운영 표면이다. rollup, checklist, traceability, agentic execution contract, and short bridge file은 이곳에 남지만, 장기적으로 유지할 사용자/프로젝트 원장은 [../docs-site](../docs-site)의 canonical pages로 접는다.

[../docs-site](../docs-site)는 GitHub Pages 배포를 전제로 한 Astro Starlight 공식 문서 사이트이며, task 0251의 목표 canonical surface다. 어떤 legacy `docs/` 파일이 docs-site canonical, temporary agentic, archive/remove 대상인지는 [Document Ownership](../docs-site/src/content/docs/document-ownership.md)이 정한다.

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
   - task 0251 이후 남은 bridge file이다. target canonical page는 [Project Requirements](../docs-site/src/content/docs/requirements.md)다.

7. [grammar/README.md](grammar/README.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site Grammar](../docs-site/src/content/docs/grammar.md)와 [Grammar And Language Reference](../docs-site/src/content/docs/reference.md)다.

8. [grammar/coverage.md](grammar/coverage.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site Feature Status](../docs-site/src/content/docs/feature-status.md), [Grammar](../docs-site/src/content/docs/grammar.md), and [Grammar And Language Reference](../docs-site/src/content/docs/reference.md)다.

9. [../examples/README.md](../examples/README.md)
   - `docs/` 밖으로 이동한 example artifact index다. target canonical pages는 [docs-site Examples](../docs-site/src/content/docs/examples.md), [Tutorials](../docs-site/src/content/docs/tutorials.md), and [Cookbook](../docs-site/src/content/docs/cookbook.md)다.

10. [standard-library.md](standard-library.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site API And CLI Reference](../docs-site/src/content/docs/api.md), [Type System](../docs-site/src/content/docs/type-system.md), [Lowering](../docs-site/src/content/docs/lowering.md), and [.NET Interop](../docs-site/src/content/docs/dotnet-interop.md)다.

11. [csharp-interop.md](csharp-interop.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)다.

12. [cli.md](cli.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site CLI](../docs-site/src/content/docs/cli.md)와 [API And CLI Reference](../docs-site/src/content/docs/api.md)다.

13. [formatting.md](formatting.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site CLI](../docs-site/src/content/docs/cli.md)와 [VS Code And LSP](../docs-site/src/content/docs/vscode-lsp.md)다.

14. [diagnostics.md](diagnostics.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Diagnostics](../docs-site/src/content/docs/diagnostics.md)다.

15. [lowering.md](lowering.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Lowering](../docs-site/src/content/docs/lowering.md)이다.

16. [migration-guide.md](migration-guide.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Migration](../docs-site/src/content/docs/migration.md)이다.

17. [feasibility.md](feasibility.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [Advanced Topics](../docs-site/src/content/docs/advanced.md)와 [Feature Status](../docs-site/src/content/docs/feature-status.md)다.

18. [feature-map.md](feature-map.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [Feature Status](../docs-site/src/content/docs/feature-status.md)다.

19. [feature-specs.md](feature-specs.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Grammar And Language Reference](../docs-site/src/content/docs/reference.md)의 `Feature Specification Index` 섹션이다.

20. [architecture.md](architecture.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)와 [Advanced Topics](../docs-site/src/content/docs/advanced.md)다.

21. [checklist.md](checklist.md)
   - 설계와 구현을 반복할 때 완료 여부를 확인하는 실행 체크리스트다.

22. [parser-fixtures.md](parser-fixtures.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Parser Fixture Policy` 섹션이다.

23. [regression-testing.md](regression-testing.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Regression Policy` 섹션이다.

24. [feature-review.md](feature-review.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Feature Review Gate` 섹션이다.

25. [tasks/README.md](tasks/README.md)
   - 장기 실행 에이전트가 바로 집어 들 수 있는 작업 패킷 목록이다.

26. [traceability.md](traceability.md)
   - 목표, 요구사항, 기능, 체크리스트가 서로 어떻게 연결되는지 검증한다.

27. [../docs-site](../docs-site)
   - GitHub Pages 배포를 전제로 한 Astro Starlight 공식 문서 사이트이자 task 0251 이후 표준 언어/프로젝트 원장의 target canonical surface다. 원장 색인은 `docs-site/src/content/docs/project-ledger.md`, 문서 소유권은 `docs-site/src/content/docs/document-ownership.md`, 작업 원장은 `docs-site/src/content/docs/work-ledger.md`, 에이전트 작업 흐름 요약은 `docs-site/src/content/docs/agentic-workflow.md`에서 웹페이지로 제공한다.

28. [references.md](references.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Official Reference Tracking` 섹션이다.

29. [dependencies.md](dependencies.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Dependency And Target Policy` 섹션이다.

30. [framework-targeting.md](framework-targeting.md)
   - task 0251 이후 남은 bridge file이다. target canonical pages는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)와 [Project Configuration](../docs-site/src/content/docs/project-configuration.md)이다.

31. [runtime-abi.md](runtime-abi.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)의 `Runtime ABI Policy` 섹션이다.

32. [release.md](release.md)
   - task 0251 이후 남은 bridge file이다. target canonical page는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Release Policy` 섹션이다.

## 문서 원칙

- `goal.md`는 task 0251 이후 남은 agent bootstrap bridge다. 표준 목표의 target canonical page는 [docs-site Core Goal](../docs-site/src/content/docs/goal.md)이다.
- `agent.md`는 `/goal` 기반 장기 실행 지침이다. 새 세션의 Codex는 루트 [agent.md](../agent.md)를 통해 목표, 반복 루프, 금지선을 복원한다.
- `agentic-execution.md`는 Ralph/Goal mode가 작업을 선택하고 검증하고 인계하는 구체적 실행 계약이다.
- `progress.md`는 task packet, rollup, commit, 인계 기록 정책을 정한다.
- `adr/`는 큰 설계 결정을 반복 논쟁 없이 이어받을 수 있게 하는 Architecture Decision Record 형식을 제공한다.
- `requirements.md`는 task 0251 이후 남은 bridge file이다. "반드시 지켜야 할 것"의 canonical ledger는 docs-site Project Requirements이고, 구현 편의/architecture/policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `grammar/`는 task 0251 이후 남은 short bridge stub surface다. 새 표준 문법 결정은 먼저 [docs-site Grammar](../docs-site/src/content/docs/grammar.md), [Grammar And Language Reference](../docs-site/src/content/docs/reference.md), [Document Ownership](../docs-site/src/content/docs/document-ownership.md)의 target owner 기준을 따른다.
- `grammar/*` detailed files는 task 0251 이후 남은 bridge stubs다. Lexical, module, declaration, expression, type, pattern, interop, resolution, precedence, ambiguity, consistency, and coverage policy는 [docs-site Grammar](../docs-site/src/content/docs/grammar.md), [Grammar And Language Reference](../docs-site/src/content/docs/reference.md), and related canonical docs-site pages를 따른다.
- `../examples/`는 `docs/` 밖의 source/runnable artifact area다. 예제 catalog, coverage map, authoring principles는 [docs-site Examples](../docs-site/src/content/docs/examples.md)를 target canonical surface로 삼는다.
- `standard-library.md`는 task 0251 이후 남은 bridge file이다. Standard library namespace, core type, runtime helper, and interop helper policy는 [docs-site API And CLI Reference](../docs-site/src/content/docs/api.md)를 target canonical surface로 삼고, type semantics는 [docs-site Type System](../docs-site/src/content/docs/type-system.md)을 따른다.
- `csharp-interop.md`는 task 0251 이후 남은 bridge file이다. C# library reference, metadata symbol, nullable, overload, public ABI, host compatibility, and smoke policy는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)을 target canonical surface로 삼는다.
- `cli.md`는 task 0251 이후 남은 bridge file이다. CLI command, manifest, source discovery, diagnostics output, exit code, and formatting contract는 [docs-site CLI](../docs-site/src/content/docs/cli.md)를 target canonical surface로 삼는다.
- `formatting.md`는 task 0251 이후 남은 bridge file이다. Formatter convention과 `typesharp format --check` policy는 [docs-site CLI](../docs-site/src/content/docs/cli.md)의 `Formatting Convention` 섹션을 target canonical surface로 삼는다.
- `diagnostics.md`는 task 0251 이후 남은 bridge file이다. Diagnostic code, descriptor, explanation, output shape, and fixture policy는 [docs-site Diagnostics](../docs-site/src/content/docs/diagnostics.md)를 target canonical surface로 삼는다.
- `lowering.md`는 task 0251 이후 남은 bridge file이다. 구현된 기능의 generated C# shape, runtime/helper 의존성, fixture 근거는 [docs-site Lowering](../docs-site/src/content/docs/lowering.md)을 target canonical surface로 삼는다.
- `migration-guide.md`는 task 0251 이후 남은 bridge file이다. 기존 .NET Framework/C# 환경에서 TypeSharp를 점진적으로 도입하는 절차와 현재 미지원 자동화는 [docs-site Migration](../docs-site/src/content/docs/migration.md)을 target canonical surface로 삼는다.
- `feasibility.md`는 task 0251 이후 남은 bridge file이다. Feasibility boundaries는 [docs-site Advanced Topics](../docs-site/src/content/docs/advanced.md), [Feature Status](../docs-site/src/content/docs/feature-status.md), and [Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `feature-map.md`는 task 0251 이후 남은 bridge file이다. Feature status와 external-language mapping은 [docs-site Feature Status](../docs-site/src/content/docs/feature-status.md)를 target canonical surface로 삼는다.
- `feature-specs.md`는 task 0251 이후 남은 bridge file이다. 현재 구현 또는 안정 문법 기능의 세부 사양 문서와 구현/검증 근거 색인은 [docs-site Grammar And Language Reference](../docs-site/src/content/docs/reference.md)의 `Feature Specification Index`를 target canonical surface로 삼는다.
- `checklist.md`는 실제 반복 작업의 입구다. 체크되지 않은 항목은 다음 설계 또는 구현 작업 후보가 된다.
- `parser-fixtures.md`는 task 0251 이후 남은 bridge file이다. Parser fixture layout, expected diagnostics, syntax tree snapshot, and current parser coverage policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `regression-testing.md`는 task 0251 이후 남은 bridge file이다. Regression evidence, snapshot update, checklist closure, and failure triage policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `feature-review.md`는 task 0251 이후 남은 bridge file이다. Feature review gate and required evidence questions는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `tasks/`는 한 세션보다 긴 작업을 task packet으로 쪼개고 상태를 남기는 임시 운영 표면이다.
- `docs-site/`는 task 0251 이후 표준 언어/프로젝트 원장의 canonical surface다. Codex CLI goal의 작업 선택 원천은 여전히 `agent.md`, `agentic-execution.md`, `checklist.md`, `tasks/README.md`, `traceability.md` 같은 agentic temporary files지만, 표준 언어/프로젝트 reference 결정은 docs-site canonical page로 접는다.
- 원장성 문서를 바꿀 때는 [Document Ownership](../docs-site/src/content/docs/document-ownership.md)의 target owner를 먼저 확인한다. 표준 언어/프로젝트 reference라면 docs-site canonical page를 갱신하고, 아직 남은 `docs/` bridge file은 같은 변경에서 맞추거나 bridge stub으로 줄인다.
- `references.md`는 task 0251 이후 남은 bridge file이다. Official reference tracking and refresh rules는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `dependencies.md`는 task 0251 이후 남은 bridge file이다. Package-free runtime/core surface와 future dependency gate는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `framework-targeting.md`는 task 0251 이후 남은 bridge file이다. 의료기기/분석기기 벤더 호환성처럼 설치 기반이 중요한 환경에서 `net48`/`net481` 선택 근거는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.
- `runtime-abi.md`는 task 0251 이후 남은 bridge file이다. Compiler/runtime ABI version alignment, breaking ABI change 기준, and compatibility gate는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)의 `Runtime ABI Policy` 섹션을 target canonical surface로 삼는다.
- `release.md`는 task 0251 이후 남은 bridge file이다. Release readiness, compatibility, security, integrity, and release-note policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)를 target canonical surface로 삼는다.

## 반복 검토 프로토콜

문서를 바꿀 때마다 다음 순서로 확인한다.

1. 목표 확인: 변경이 [goal.md](goal.md)의 한 문장 과제와 맞는가?
2. 에이전트 확인: [agent.md](../agent.md)의 반복 실행 규칙과 금지선을 지키는가?
3. 장기 실행 확인: [agentic-execution.md](agentic-execution.md)의 queue, task packet, Done 기준과 맞는가?
4. 요구사항 확인: 필수사항을 새로 만들거나 깨뜨리지 않는가?
5. 문법 확인: [docs-site Grammar](../docs-site/src/content/docs/grammar.md)의 문법 원칙과 [grammar/README.md](grammar/README.md) bridge가 가리키는 target owner와 충돌하지 않는가?
6. 커버리지 확인: [docs-site Feature Status](../docs-site/src/content/docs/feature-status.md)와 [Grammar](../docs-site/src/content/docs/grammar.md)에 MVP, Stable Backlog, Preview Watch, Experimental, Rejected 또는 coverage direction이 반영되어 있는가?
7. 표준 라이브러리 확인: [docs-site API And CLI Reference](../docs-site/src/content/docs/api.md)의 namespace와 core type policy를 지키는가?
8. CLI 확인: [docs-site CLI](../docs-site/src/content/docs/cli.md)의 command, manifest, diagnostics 계약을 깨뜨리지 않는가?
9. 실현 가능성 확인: [feasibility.md](feasibility.md)의 MVP host/backend/ABI 결정과 충돌하지 않는가?
10. 기능 상태 확인: MVP, Stable Backlog, Preview Watch, Experimental, Rejected 중 하나로 분류했는가?
11. lowering 확인: .NET Framework 4.8에서 실행 가능한 표현으로 낮출 수 있는가?
12. interop 확인: C#/.NET Framework 소비자가 이해할 public API인가?
13. 테스트 확인: 체크리스트에 positive, negative, interop 검증이 반영되어 있는가?
14. 근거 확인: 최신 외부 언어 기능을 언급했다면 공식 링크와 기준일이 있는가?

## 현재 결론

현재 문서 세트는 TypeSharp의 초기 설계 방향, MVP 구현 근거, 검증 경로를 잡기에 충분한 구조를 갖춘다. 남은 항목은 현재 목표의 누락이 아니라 Stable Backlog 또는 후속 확장으로 분리된 설계 선택과 세부 문법 확장이다.

주요 후속 확장:
- [docs-site Feature Status](../docs-site/src/content/docs/feature-status.md)와 [Grammar](../docs-site/src/content/docs/grammar.md)에 새로 발견되는 TypeScript/F#/C# 기능을 계속 분류하는 작업
- 직접 IL backend 구현
- nominal closed union의 tagged struct/generated closed type 표현
- TypeScript식 type-level union의 수동 대체 가이드 refinement
- structural type의 public API adapter 자동 생성
- CLI manifest를 장기적으로 `TypeSharp.toml`로 유지할지 MSBuild와 1급 통합할지 여부

