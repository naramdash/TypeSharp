# TypeSharp Standard Library

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical standard library 원장은 [docs-site API And CLI Reference](../docs-site/src/content/docs/api.md)의 `Standard Library Surface` 섹션이다. Core type 의미론은 [docs-site Type System](../docs-site/src/content/docs/type-system.md), public ABI/runtime ABI policy는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)을 따른다.

## Bridge Scope

- `TypeSharp.Core`, `TypeSharp.Collections`, `TypeSharp.Runtime`, and `TypeSharp.Interop` namespace policy는 docs-site API page로 접혔다.
- `Option<T>`, `Result<T,E>`, and `Unit` type policy는 docs-site API page와 Type System page로 접혔다.
- Runtime helper and interop helper policy는 docs-site API page, Lowering page, and .NET Interop page로 접혔다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
