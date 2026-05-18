# Task: VS Code Extension Scaffold

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:00:06 +09:00
End Time: 2026-05-19 01:04:38 +09:00

## Objective

VS Code가 TypeSharp `.tysh` 파일을 `typesharp` language id로 인식하고 lexical grammar 기반 syntax highlighting을 제공할 수 있는 extension scaffold를 추가한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../architecture.md](../architecture.md)
- [../grammar/lexical.md](../grammar/lexical.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `vscode/typesharp`

## Scope

In:
- VS Code extension manifest
- `.tysh` language registration with language id `typesharp`
- language configuration for comments, brackets, auto-closing pairs, and word pattern
- TextMate grammar for comments, strings, numbers, keywords, primitive types, attributes, and operators
- checklist, traceability, architecture, and task queue updates

Out:
- LSP client activation
- diagnostics, hover, go-to-definition, completion
- npm dependency installation
- extension packaging and marketplace publishing

## Acceptance Criteria

- [x] `vscode/typesharp/package.json` contributes the `typesharp` language for `.tysh`.
- [x] `vscode/typesharp/package.json` points to language configuration and TextMate grammar files.
- [x] TextMate grammar is based on [../grammar/lexical.md](../grammar/lexical.md) keyword/comment/string/numeric/operator categories.
- [x] VS Code extension scaffold and syntax highlighting checklist items are marked complete, while LSP items remain open.
- [x] JSON files parse successfully and standard build/test smoke still passes.

## Verification

Command:

```text
Get-Content vscode/typesharp/package.json | ConvertFrom-Json | Out-Null
Get-Content vscode/typesharp/language-configuration.json | ConvertFrom-Json | Out-Null
Get-Content vscode/typesharp/syntaxes/typesharp.tmLanguage.json | ConvertFrom-Json | Out-Null
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Expected:
- VS Code scaffold JSON files parse.
- tests, support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- Pass. `Get-Content vscode/typesharp/package.json | ConvertFrom-Json | Out-Null`
- Pass. `Get-Content vscode/typesharp/language-configuration.json | ConvertFrom-Json | Out-Null`
- Pass. `Get-Content vscode/typesharp/syntaxes/typesharp.tmLanguage.json | ConvertFrom-Json | Out-Null`
- Pass. package contribution check confirmed language id `typesharp`, extension `.tysh`, and grammar scope `source.typesharp`.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.
- Pass. `git diff --check`

## Handoff

Done:
- Added `vscode/typesharp/package.json` with TypeSharp language registration and grammar contribution.
- Added `vscode/typesharp/language-configuration.json`.
- Added `vscode/typesharp/syntaxes/typesharp.tmLanguage.json` for initial lexical syntax highlighting.
- Updated architecture, checklist, traceability, and task queue references.

Remaining:
- Implement LSP diagnostics by sharing compiler diagnostics with the VS Code extension.

Blocked:
- None.
