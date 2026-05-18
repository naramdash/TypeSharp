# Regression Test Policy

문서 기준일: 2026-05-19

이 문서는 TypeSharp 변경이 어떤 회귀 테스트 근거를 가져야 하는지 정의한다. 목표는 새 기능이나 버그 수정이 parser snapshot, diagnostic fixture, generated `net48` build, C# interop, public ABI, performance smoke 중 어디에 들어가야 하는지 매번 다시 판단하지 않게 하는 것이다.

## 원칙

- 회귀 테스트는 실패 모드를 재현해야 한다. 단순히 전체 test suite가 통과한다는 사실만으로 새 요구사항을 닫지 않는다.
- public surface, generated code, diagnostic behavior, CLI output, runtime helper behavior가 바뀌면 해당 계층의 가장 가까운 fixture 또는 smoke를 추가한다.
- `net48` 산출물 호환성을 주장하는 변경은 generated C# compile, generated assembly build, C# consumer, host compatibility, 또는 metadata reader 기반 public ABI check 중 하나로 검증한다.
- negative behavior가 있는 기능은 positive fixture만으로 닫지 않는다. 진단해야 하는 입력은 diagnostics fixture나 CLI no-emission smoke가 필요하다.
- snapshot은 계약이다. parser tree, diagnostic JSON, generated C# snapshot을 바꾸면 같은 변경에서 의도와 근거를 문서나 task packet에 남긴다.

## Test Types

| Test type | Location | Use when |
| --- | --- | --- |
| Parser golden fixture | `tests/fixtures/parser` | syntax recognition, recovery, trivia/span preserving tree shape changes |
| Binder diagnostic fixture | `tests/fixtures/diagnostics/binder` | name resolution, symbol scope, duplicate/unresolved symbol behavior |
| Type checker diagnostic fixture | `tests/fixtures/diagnostics/type-checker` | type compatibility, nullability, exhaustiveness, public boundary diagnostics |
| C# backend snapshot | `tests/fixtures/backend/csharp` | generated C# source shape, lowering, helper calls, stable formatting |
| Compiler smoke test | `tests/TypeSharp.Compiler.Tests/Program.cs` | CLI/check/build/run integration, generated project emission, source discovery, metadata reader behavior |
| Generated `net48` build smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | generated C# must compile as `net48` or produce a generated assembly/executable |
| C# consumer smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | public TypeSharp API must be callable from C# `net48` projects |
| Host compatibility smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | ASP.NET/WCF/worker-style host references or runtime dependency shape changes |
| Runtime unit smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | `TypeSharp.Core` or `TypeSharp.Runtime` helper behavior changes |
| Public ABI metadata smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | generated public metadata shape is the contract, not only compile success |
| Performance smoke | `tests/TypeSharp.Compiler.Tests/Program.cs` | check/build path needs a guardrail against hangs or extreme regressions |
| Documentation link check | `rg`/`git diff --check` plus targeted reads | docs-only policy, guide, or traceability changes |

## Coverage Rules

| Change kind | Minimum regression evidence |
| --- | --- |
| New stable syntax | parser positive fixture, grammar coverage entry, and parser fixture coverage note |
| Parser recovery or diagnostic change | parser negative fixture with expected diagnostic JSON and tree snapshot |
| Binder/name-resolution behavior | binder positive or negative diagnostics fixture |
| Type checking behavior | type checker positive or negative diagnostics fixture; CLI JSON smoke if users rely on CLI output |
| Public boundary restriction | type checker negative fixture and CLI build no-emission smoke |
| Lowering change | C# backend snapshot and generated `net48` compile/build smoke |
| Public API lowering | backend snapshot, generated `net48` assembly build, and C# consumer smoke |
| Runtime/helper behavior | runtime unit smoke and generated build/consumer smoke if generated code calls the helper |
| C# interop metadata or overload behavior | local/framework metadata smoke and generated `net48` build or diagnostic no-emission smoke |
| Public ABI shape change | public ABI metadata smoke and runtime ABI policy review |
| CLI command behavior | direct CLI smoke with stdout/stderr/exit-code assertion |
| VS Code/LSP behavior | LSP request/notification smoke using zero-based ranges |
| Performance-sensitive compiler path | performance smoke with a deliberately generous budget |
| Documentation-only decision | linked docs, checklist, traceability, and task packet verification |

## Checklist Closure

A checklist item can be marked done only when at least one concrete artifact proves the requirement:

- For syntax, a parser fixture or grammar document section must exist.
- For semantics, binder/type checker diagnostics fixtures or smoke tests must exist.
- For generated behavior, a backend snapshot plus generated `net48` compile/build path must exist.
- For C#/.NET interop, local/framework metadata and generated build or C# consumer evidence must exist.
- For public ABI, metadata reader or C# consumer evidence must prove the shape.
- For policy/doc items, the new document must be linked from `docs/README.md`, `docs/checklist.md`, `docs/traceability.md`, or the relevant task packet.

Do not mark a broad compiler subsystem done merely because a skeleton exists. The item is done only when the implemented scope is documented and its current boundaries are explicit.

## Snapshot Updates

Snapshot changes should be reviewed as source changes:

- Parser `expected.tree` changes must preserve spans, missing tokens, skipped tokens, and trivia summary intent.
- Diagnostic JSON changes must preserve CLI schema and source positions.
- Backend `expected.cs` changes must remain C# 7.3-compatible and compatible with generated `net48` project builds.
- Large mechanical snapshot refreshes should be separate from semantic implementation when practical.

## Failure Triage

When a regression test fails:

1. Identify the layer: parser, binder, type checker, backend, CLI, runtime, metadata, host, or docs.
2. Decide whether the expected contract changed intentionally.
3. If intentional, update the nearest spec, fixture, traceability row, and task packet.
4. If unintentional, fix the implementation and keep the old fixture.
5. If the gap reveals a missing regression category, update this policy before closing the task.
