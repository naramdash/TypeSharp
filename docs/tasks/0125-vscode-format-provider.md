# Task 0125: VS Code Format Provider

Status: Done
Queue: Q4
Start Time: 2026-05-19 11:55:00 +09:00
End Time: 2026-05-19 11:56:10 +09:00

## Objective

Connect the formatting convention to the VS Code editing loop by adding a document formatting provider for `.tysh` files that mirrors the CLI formatter MVP whitespace normalization.

## Scope

In:
- VS Code `DocumentFormattingEditProvider` registration for `typesharp` files.
- Safe whitespace normalization matching the CLI MVP: line-ending normalization, trailing whitespace removal, repeated blank line collapse, and final newline.
- Mocked extension smoke coverage.
- Live language-server extension smoke coverage.
- Real VS Code Extension Host smoke coverage through `vscode.executeFormatDocumentProvider`.

Out:
- Full AST-based formatter.
- Range formatting.
- Format-on-type behavior.
- Import organization or declaration sorting.

## Acceptance Criteria

- [x] VS Code extension registers a document formatter for `.tysh` files.
- [x] Mocked extension-host smoke verifies the provider returns normalized text edits.
- [x] Live bundled-language-server smoke verifies formatting provider registration and edit output.
- [x] Real VS Code Extension Host smoke applies formatting edits and verifies normalized document text.
- [x] README, checklist, and traceability mention VS Code formatter evidence.

## Verification

Representative commands:

```text
npm run check
npm run check:smoke
npm run check:live
npm run check:host
npm run test:smoke
npm run test:live
npm run test:host
```

Result:
- PASS extension syntax checks.
- PASS mocked extension smoke.
- PASS live bundled-language-server extension smoke.
- PASS real VS Code Extension Host smoke.

## Handoff

Done:
- Added a dependency-free document formatter provider in `vscode/typesharp/extension.js`.
- Extended mocked, live, and real Extension Host smokes to cover formatting.
- Updated extension README, checklist, traceability, and task index.

Remaining:
- Full AST-based formatting remains future tooling work.

Blocked:
- None.
