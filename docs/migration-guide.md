# Migration Guide

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical migration guide는 [docs-site Migration](../docs-site/src/content/docs/migration.md)이다.

## Bridge Scope

- Incremental .NET Framework/C# adoption strategy, minimal project shape, public API rules, C# pattern mapping, C# library calls, generated DLL consumption, nullability/error migration, host compatibility, unsupported automation, and adoption checklist는 docs-site Migration으로 접혔다.
- CLI behavior는 [docs-site CLI](../docs-site/src/content/docs/cli.md), lowering shape는 [docs-site Lowering](../docs-site/src/content/docs/lowering.md), interop/runtime dependency policy는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)를 따른다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical page를 찾을 수 있게 하기 위해서만 남아 있다.
