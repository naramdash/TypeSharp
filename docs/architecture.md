# TypeSharp 권장 아키텍처

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical architecture and implementation policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)와 [Advanced Topics](../docs-site/src/content/docs/advanced.md)다.

## Bridge Scope

- Compiler pipeline, host target policy, project separation, backend strategy, runtime helper boundary, interop layer, tooling architecture, project manifest stance, test strategy, versioning direction, and risk controls는 docs-site Project Policy로 접혔다.
- User-facing generated shape and lowering policy는 [docs-site Lowering](../docs-site/src/content/docs/lowering.md)를 따른다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
