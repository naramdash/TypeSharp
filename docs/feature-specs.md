# Feature Specification Index

문서 기준일: 2026-05-19

이 문서는 현재 구현 또는 안정 문법으로 분류된 TypeSharp 기능의 세부 사양 위치를 묶는다. 목표는 기능별 사양이 어느 문서에 있는지 빠르게 찾고, 새 기능을 추가할 때 [feature-review.md](feature-review.md)와 [regression-testing.md](regression-testing.md)의 Done gate를 어디에 연결해야 하는지 명확히 하는 것이다.

## Scope Rule

- 이 인덱스는 현재 구현된 기능, Stable Grammar, MVP로 명시된 기능, 그리고 이미 smoke/fixture evidence가 있는 기능을 대상으로 한다.
- Planned, Preview Watch, Experimental, Stable Backlog 기능은 [feature-map.md](feature-map.md), [feasibility.md](feasibility.md), 또는 각 grammar 문서에 남기되 완료된 구현 사양으로 간주하지 않는다.
- 기능이 public ABI, lowering, diagnostics, or tooling behavior를 바꾸면 해당 기능의 task packet 또는 traceability row가 이 인덱스의 관련 문서에 연결되어야 한다.

## Core Language Features

| Feature | Primary spec | Implementation/evidence |
| --- | --- | --- |
| Lexical grammar and tokens | [grammar/lexical.md](grammar/lexical.md), [grammar/README.md](grammar/README.md) | parser fixtures, TextMate syntax scaffold |
| Parser precedence and ambiguity rules | [grammar/precedence.md](grammar/precedence.md), [grammar/ambiguity.md](grammar/ambiguity.md) | parser snapshot fixtures |
| Module graph, namespace, import/export, ambient signatures | [grammar/modules.md](grammar/modules.md), [grammar/resolution.md](grammar/resolution.md) | parser fixtures, binder symbols, backend import snapshots, root namespace fallback build smoke, ambient parser fixture and build smoke |
| Function, value, literal declarations | [grammar/declarations.md](grammar/declarations.md), [lowering.md](lowering.md) | parser fixtures, binder/type checker smokes, generated C# snapshots |
| Record, record expression, class, interface, delegate declarations | [grammar/declarations.md](grammar/declarations.md), [grammar/types.md](grammar/types.md), [grammar/expressions.md](grammar/expressions.md), [lowering.md](lowering.md) | backend snapshots, record expression diagnostics, generic constraint lowering snapshot, generated `net48` build, C# consumer smokes |
| Nominal closed union declarations | [grammar/declarations.md](grammar/declarations.md), [grammar/types.md](grammar/types.md), [standard-library.md](standard-library.md), [lowering.md](lowering.md) | runtime helper smokes, backend snapshots, generated build and consumer smokes |
| Type-level union and public boundary rule | [grammar/types.md](grammar/types.md), [grammar/resolution.md](grammar/resolution.md), [csharp-interop.md](csharp-interop.md) | type checker diagnostics, CLI no-emission smokes |
| Structural shape checking | [grammar/types.md](grammar/types.md), [feature-map.md](feature-map.md), [lowering.md](lowering.md) | type checker positive/negative fixtures, CLI diagnostics smoke |
| Unknown access narrowing | [grammar/types.md](grammar/types.md), [feature-map.md](feature-map.md), [diagnostics.md](diagnostics.md) | `TS2209`, diagnostics fixture, CLI smoke |
| Nullability and unknown C# nullability | [grammar/types.md](grammar/types.md), [csharp-interop.md](csharp-interop.md), [diagnostics.md](diagnostics.md) | `TS2202`, `TS2404`, diagnostics fixtures, CLI smokes |
| Capability boundaries | [grammar/types.md](grammar/types.md), [grammar/interop.md](grammar/interop.md), [csharp-interop.md](csharp-interop.md), [diagnostics.md](diagnostics.md) | `TS2206`, `TS2207`, `TS2208`, diagnostics fixtures, CLI smokes |
| Pattern matching and exhaustiveness | [grammar/patterns.md](grammar/patterns.md), [grammar/expressions.md](grammar/expressions.md), [lowering.md](lowering.md) | nominal/type-level union diagnostics and backend snapshots |
| Pipeline expression lowering | [grammar/expressions.md](grammar/expressions.md), [feature-map.md](feature-map.md), [feasibility.md](feasibility.md), [lowering.md](lowering.md) | pipeline backend snapshot, generated build and C# consumer smoke |
| Async `Task<T>` interop | [grammar/expressions.md](grammar/expressions.md), [csharp-interop.md](csharp-interop.md), [lowering.md](lowering.md) | async backend snapshot, generated build and C# consumer smoke |
| Collection expression array lowering | [grammar/expressions.md](grammar/expressions.md), [feature-map.md](feature-map.md), [lowering.md](lowering.md) | collection backend snapshot, mismatch diagnostic fixture, generated build and C# consumer smoke |
| Blocks, local `let`, calls, member access, indexer access, lambdas, for expressions | [grammar/expressions.md](grammar/expressions.md), [lowering.md](lowering.md) | parser fixtures, binder smoke, backend snapshots |

## Interop And ABI Features

| Feature | Primary spec | Implementation/evidence |
| --- | --- | --- |
| Framework assembly and local DLL references | [cli.md](cli.md), [csharp-interop.md](csharp-interop.md), [grammar/interop.md](grammar/interop.md) | reference resolver smokes, metadata reader smokes |
| C# metadata reader | [csharp-interop.md](csharp-interop.md), [regression-testing.md](regression-testing.md) | local public type/interface/method/generic method placeholder/property/field/parameter metadata smokes |
| C# constructor/static/instance/property/field/indexer/generic method calls and interface type references | [csharp-interop.md](csharp-interop.md), [grammar/interop.md](grammar/interop.md), [lowering.md](lowering.md) | generated `net48` build smokes |
| C# `params`, `out`, `in`, `ref` calls | [csharp-interop.md](csharp-interop.md), [grammar/interop.md](grammar/interop.md), [diagnostics.md](diagnostics.md) | metadata smokes, byref diagnostics, generated build smokes |
| C# optional and named argument overload validation | [csharp-interop.md](csharp-interop.md), [grammar/expressions.md](grammar/expressions.md) | optional/named overload diagnostics and generated build smokes |
| C# overload resolution | [csharp-interop.md](csharp-interop.md), [grammar/resolution.md](grammar/resolution.md), [traceability.md](traceability.md) | `TypeSharpCSharpOverloadResolver`, ambiguity diagnostics, exact/params/optional/named smokes |
| Public ABI checker and versioning | [runtime-abi.md](runtime-abi.md), [release.md](release.md), [regression-testing.md](regression-testing.md) | `TypeSharpPublicAbiChecker`, public ABI snapshot smoke |
| ASP.NET/WCF/worker-style host compatibility | [goal.md](goal.md), [requirements.md](requirements.md), [framework-targeting.md](framework-targeting.md), [csharp-interop.md](csharp-interop.md) | host compatibility smoke tests |

## Tooling And Build Features

| Feature | Primary spec | Implementation/evidence |
| --- | --- | --- |
| Project manifest and source discovery | [cli.md](cli.md), [architecture.md](architecture.md) | manifest loader, locator, source discovery smokes |
| CLI `version`, `check`, `build`, `run` | [cli.md](cli.md), [diagnostics.md](diagnostics.md) | CLI smoke tests, JSON/text diagnostics smokes |
| Generated C# source/project/assembly emission | [architecture.md](architecture.md), [lowering.md](lowering.md), [regression-testing.md](regression-testing.md) | backend snapshots, generated `net48` build smokes |
| Backend abstraction | [architecture.md](architecture.md), [requirements.md](requirements.md) | `ITypeSharpBackend`, C# backend adapter smoke |
| Semantic model for LSP | [architecture.md](architecture.md), [requirements.md](requirements.md) | `TypeSharpSemanticModel`, LSP diagnostics/hover/definition/completion smokes |
| VS Code syntax highlighting and LSP client | [grammar/lexical.md](grammar/lexical.md), [README.md](README.md), [architecture.md](architecture.md) | VS Code extension scaffold, TextMate grammar, and stdio LSP client activation |
| Formatter convention, CLI format MVP, and VS Code format provider | [formatting.md](formatting.md), [cli.md](cli.md), [architecture.md](architecture.md) | docs policy, `typesharp format`, VS Code document formatter provider, and CLI/extension format smoke tests |
| Regression, feature review, release policy | [regression-testing.md](regression-testing.md), [feature-review.md](feature-review.md), [release.md](release.md) | checklist and traceability policy rows |

## Adding A New Feature

When a feature is added or promoted:

1. Add or update the primary spec in the relevant grammar, interop, lowering, CLI, runtime, or release document.
2. Add the feature to [feature-map.md](feature-map.md) when it maps to C#/F#/TypeScript source features.
3. Answer the applicable [feature-review.md](feature-review.md) questions in the task packet.
4. Add regression evidence according to [regression-testing.md](regression-testing.md).
5. Add or update the traceability row linking spec, implementation, and verification.
