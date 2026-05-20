# .NET Framework 타깃 선택 기준

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical target framework policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Dependency And Target Policy` 섹션과 [Project Configuration](../docs-site/src/content/docs/project-configuration.md)이다.

## Bridge Scope

- `net48` broad compatibility profile, `net481` latest-framework profile boundary, compiler/CLI/LSP host target separation, and generated/runtime/core target rules는 docs-site Project Policy로 접혔다.
- Project manifest target behavior는 docs-site Project Configuration을 따른다.
- Official Microsoft source links must be refreshed before changing target baselines.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
