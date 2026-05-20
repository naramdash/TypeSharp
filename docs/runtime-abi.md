# Runtime ABI Policy

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical runtime ABI/public ABI 원장은 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)의 `Runtime ABI Policy` 섹션이다. General release readiness, release notes, checksums, security, and compatibility matrix policy는 아직 [release.md](release.md) bridge에 남아 있으며, target canonical owner는 [Document Ownership](../docs-site/src/content/docs/document-ownership.md)이 정한다.

## Bridge Scope

- `TypeSharp.Compiler.TypeSharpCompilerInfo.RuntimeAbiVersion`과 `TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion` alignment rule은 docs-site `.NET Interop`로 접혔다.
- ABI-covered public surface, ABI change rules, compatibility gates, and pre-1.0 ABI `0` policy는 docs-site `.NET Interop`로 접혔다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical page를 찾을 수 있게 하기 위해서만 남아 있다.
