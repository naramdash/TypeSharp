# Lowering Bridge

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 긴 표준 lowering 원장이 아니라 short bridge stub이다. TypeSharp-to-C# lowering의 target canonical surface는 [docs-site Lowering](../docs-site/src/content/docs/lowering.md)이다.

## 현재 소유권

| 항목 | 위치 |
| --- | --- |
| Canonical lowering contract | [docs-site Lowering](../docs-site/src/content/docs/lowering.md) |
| Compiler pipeline and backend context | [docs-site Advanced Topics](../docs-site/src/content/docs/advanced.md) |
| Public ABI and interop context | [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md) |
| Migration matrix | [docs-site Document Ownership](../docs-site/src/content/docs/document-ownership.md) |
| Completed migration task | [tasks/0001-0252-task-ledger-rollup.md](tasks/0001-0252-task-ledger-rollup.md) |

## Bridge Scope

The durable content formerly kept here has been folded into docs-site:

- backend contract for deterministic C# 7.3 source generation,
- lowering pipeline requirements,
- generated shape map,
- function/module/import lowering,
- records, public types, partial declarations,
- nominal union and pattern lowering,
- pipeline, composition, `yield`, and `lock`,
- structural proof and type-operator lowering,
- collections, indexers, intrinsics, and extensions,
- async `Task` and C# interop lowering,
- compile-time-only/public ABI rejection rules,
- fixture and CLI smoke evidence pointers.

New lowering decisions should update docs-site [Lowering](../docs-site/src/content/docs/lowering.md) first. Keep this bridge short until it can be removed after references are updated.
