# Task 0118: VS Code LSP Client Activation

Status: Done
Queue: Q4
Start Time: 2026-05-19 10:06:48 +09:00
End Time: 2026-05-19 10:12:47 +09:00

## Objective

Complete the next tooling adoption slice by moving the VS Code extension from syntax-only scaffold to an extension-host LSP client that starts `TypeSharp.LanguageServer` and exposes diagnostics, hover, go-to-definition, and completion for `.tysh` editing.

## Scope

In:
- VS Code extension activation on `typesharp` documents.
- Dependency-free stdio JSON-RPC client for the existing language server.
- `didOpen`/`didChange` document sync.
- `textDocument/publishDiagnostics` to VS Code diagnostic collection mapping.
- Hover, definition, and completion provider forwarding to LSP requests.
- Package metadata for extension entrypoint, grammar/config assets, and bundled `server/**` output.
- `prepare:server` package script that publishes `TypeSharp.LanguageServer` into the bundled server folder.
- Static smoke tests for the activation and package contract.
- Mocked extension-host smoke that calls `activate()` and verifies bundled server discovery plus LSP request/notification forwarding.
- Live extension smoke that uses the published bundled language server process over stdio.
- Real VS Code Extension Host smoke that launches the installed VS Code executable with `--extensionDevelopmentPath` and `--extensionTestsPath`.

Out:
- Publishing VSIX artifacts.
- Project-wide LSP semantic model, metadata member completion, rename, code actions, and incremental range edits.
- Astro Starlight docs site, GitHub Pages workflow, and runnable example catalog.

## Acceptance Criteria

- [x] `vscode/typesharp/package.json` has `main`, `activationEvents`, language server settings, package files, and a syntax check script.
- [x] `vscode/typesharp/package.json` has a `prepare:server` script for bundled language server output.
- [x] `vscode/typesharp/extension.js` starts the language server and forwards diagnostics, hover, definition, and completion through LSP.
- [x] VS Code README explains the development launch and packaging contract.
- [x] Checklist and traceability mark VS Code LSP client activation/package evidence.
- [x] Smoke tests cover the package/activation contract, bundled server discovery, and mocked extension activation flow.
- [x] Live smoke covers bundled server process startup and editor feature forwarding without mocking the language server.
- [x] Extension Host smoke opens a `.tysh` file in VS Code and verifies diagnostics, hover, definition, and completion through VS Code commands.

## Verification

Representative commands:

```text
node --check extension.js
node --check test/extension-smoke.js
node --check test/extension-live-smoke.js
node --check test/extension-host-smoke.js
node --check test/run-extension-host-smoke.js
node test/extension-smoke.js
npm run prepare:server
npm run test:live
npm run test:host
Get-Content vscode/typesharp/package.json | ConvertFrom-Json | Out-Null
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "VS Code extension"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI run passes arguments"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
git diff --check
```

Result:
- PASS `node --check extension.js`.
- PASS `node --check test/extension-smoke.js`.
- PASS `node --check test/extension-live-smoke.js`.
- PASS `node --check test/extension-host-smoke.js`.
- PASS `node --check test/run-extension-host-smoke.js`.
- PASS `node test/extension-smoke.js`.
- PASS `npm run prepare:server`.
- PASS `npm run test:live`.
- PASS `npm run test:host`.
- PASS package JSON parse.
- PASS compiler test project build.
- PASS focused `VS Code extension` smoke tests.
- PASS focused `CLI run passes arguments` rerun.
- PASS `git diff --check`.
- PASS full compiler test suite.

## Handoff

Done:
- Added a dependency-free VS Code LSP client for TypeSharp.
- Connected extension-host diagnostics, hover, definition, and completion to `TypeSharp.LanguageServer`.
- Added package metadata, bundled server publish script, static/mocked activation smoke tests, a live bundled-language-server extension smoke, and a real VS Code Extension Host smoke for the contract.

Remaining:
- VSIX publishing remains a release packaging step.

Blocked:
- None.
