# Task: LSP Hover

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:15:10 +09:00
End Time: 2026-05-19 01:21:08 +09:00

## Objective

`TypeSharp.LanguageServer`가 open `.tysh` document의 identifier 위치에서 compiler binder symbol 정보를 재사용해 LSP `textDocument/hover` response를 반환하게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.LanguageServer`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `initialize` capability에 `hoverProvider` 노출
- `textDocument/hover` request handling
- open document text를 parser/binder로 분석해 identifier hover content 생성
- markdown hover content와 target range 반환
- smoke coverage for bound symbol hover
- checklist, traceability, architecture, and task queue updates

Out:
- VS Code client activation
- cross-file/project manifest-aware symbol lookup
- metadata reference hover
- inferred expression type hover
- go-to-definition
- completion

## Acceptance Criteria

- [x] `src/TypeSharp.LanguageServer` advertises `hoverProvider`.
- [x] `textDocument/hover` returns a markdown hover result for a bound TypeSharp symbol in an open document.
- [x] hover range uses zero-based LSP positions for the identifier under the cursor.
- [x] unknown or unopened document hover requests return `null`.
- [x] `LSP hover` checklist item is marked complete while go-to-definition/completion remain open.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Expected:
- language server project builds.
- test project builds.
- `language server returns hover for bound symbols` passes.
- tests, support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- PASS `dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj`
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- PASS `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- PASS `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- PASS `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json`
- PASS `git diff --check`

## Handoff

Done:
- Added `textDocument/hover` request handling to the language server.
- Advertised `hoverProvider` in initialize capabilities.
- Added `TypeSharpDocumentHover` to parse open document text and map binder symbols to markdown hover content.
- Reused shared LSP range conversion for diagnostics and hover ranges.
- Added smoke coverage for bound symbol hover and unopened document null hover response.
- Updated checklist, traceability, architecture, and task queue docs.

Remaining:
- VS Code client activation.
- Cross-file/project manifest-aware symbol lookup.
- Metadata reference hover.
- Inferred expression type hover.
- Go-to-definition.
- Completion.

Blocked:
- None.
