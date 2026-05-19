# Task 0136: Docs Site VS Code/LSP Smoke Contract

Status: Done
Queue: Q5
Start Time: 2026-05-19 13:11:00 +09:00
End Time: 2026-05-19 13:17:00 +09:00

## Objective

Make the official docs-site VS Code/LSP page list the same reproducible smoke commands that verify editor diagnostics, hover, go-to-definition, completion, and formatting.

## Scope

In:
- `docs-site/src/content/docs/vscode-lsp.md` smoke-tested command list.
- VS Code extension README cross-reference to the docs-site command contract.
- Docs site contract smoke assertions for the command list.
- Checklist and traceability updates.

Out:
- Changing VS Code extension behavior.
- Running the real Extension Host smoke in every docs build.
- VSIX publishing automation.

## Acceptance Criteria

- [x] Docs-site VS Code/LSP page lists `npm run check`.
- [x] Docs-site VS Code/LSP page lists `npm run check:live`.
- [x] Docs-site VS Code/LSP page lists `npm run prepare:server`.
- [x] Docs-site VS Code/LSP page lists `npm run test:live`.
- [x] Docs-site VS Code/LSP page lists `npm run test:host`.
- [x] Test suite pins the docs-site command list.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site contract"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
```

Result:
- PASS compiler test project build.
- PASS focused docs site contract smoke.
- PASS full compiler test suite.

## Handoff

Done:
- Added smoke-tested VS Code/LSP commands to the docs-site page.
- Added tests that pin the docs-site command list.
- Updated checklist and traceability evidence.

Remaining:
- None.

Blocked:
- None.
