# Task Rollup: VS Code And LSP Tooling

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:00:06 +09:00
End Time: 2026-05-19 01:31:04 +09:00

## Compressed Tasks

- `0065-vscode-extension-scaffold.md`
- `0066-lsp-diagnostics-publish.md`
- `0067-lsp-hover.md`
- `0068-lsp-go-to-definition.md`
- `0069-lsp-basic-completion.md`

## Objective

VS Code가 TypeSharp `.tysh` 파일을 인식하고, `TypeSharp.LanguageServer`가 open document 기준 diagnostics, hover, go-to-definition, basic completion을 LSP로 제공하게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../architecture.md](../architecture.md)
- [../diagnostics.md](../diagnostics.md)
- [../grammar/lexical.md](../grammar/lexical.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `vscode/typesharp`
- `src/TypeSharp.LanguageServer`
- `tests/TypeSharp.Compiler.Tests`

## Scope

Completed:
- VS Code extension scaffold with `typesharp` language id and `.tysh` registration.
- VS Code language configuration and lexical TextMate grammar.
- `TypeSharp.LanguageServer` modern .NET host project.
- stdio JSON-RPC framing for core LSP requests/notifications.
- `initialize`, `shutdown`, `exit`, `textDocument/didOpen`, and `textDocument/didChange`.
- compiler diagnostics to LSP `publishDiagnostics`.
- `textDocument/hover` markdown response for open document binder symbols.
- `textDocument/definition` Location response for open document source symbols.
- `textDocument/completion` response for binder symbols, built-in types, and core keywords.
- smoke tests for diagnostics, hover, definition, and completion.
- checklist, architecture, traceability, and task queue updates.

Out:
- VS Code client activation for starting the language server.
- workspace manifest discovery from LSP initialization options.
- cross-file/project-wide symbol lookup.
- metadata reference hover/definition/member completion.
- incremental text edit ranges.
- snippets, completion resolve, ranking beyond deterministic label sort.
- LSP packaging/publishing.

## Verification

Commands run across the compressed packet set:

```text
Get-Content vscode/typesharp/package.json | ConvertFrom-Json | Out-Null
Get-Content vscode/typesharp/language-configuration.json | ConvertFrom-Json | Out-Null
Get-Content vscode/typesharp/syntaxes/typesharp.tmLanguage.json | ConvertFrom-Json | Out-Null
dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Result:
- PASS VS Code scaffold JSON parsing.
- PASS package contribution check for language id `typesharp`, extension `.tysh`, and grammar scope `source.typesharp`.
- PASS `dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj`.
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`.
- PASS `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`.
- PASS `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`.
- PASS `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json`.
- PASS `git diff --check`.

## Handoff

Done:
- `vscode/typesharp` registers TypeSharp files and provides initial syntax highlighting.
- `TypeSharp.LanguageServer` exposes `textDocumentSync`, `hoverProvider`, `definitionProvider`, and `completionProvider`.
- Open document diagnostics share compiler parser, binder, type checker, diagnostic codes, and source spans.
- Hover, definition, and completion share open document compiler symbol data where applicable.
- Tests cover LSP diagnostics, hover, definition, and completion smoke paths.

Remaining:
- VS Code client activation.
- Project manifest-aware language server workspace loading.
- Cross-file semantic model for LSP features.
- Metadata symbol hover, navigation, and member completion.
- Incremental document changes beyond full-text sync.

Blocked:
- None.
