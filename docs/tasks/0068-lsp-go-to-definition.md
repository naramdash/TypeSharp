# Task: LSP Go To Definition

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:22:19 +09:00
End Time: 2026-05-19 01:26:11 +09:00

## Objective

`TypeSharp.LanguageServer`가 open `.tysh` document의 identifier 위치에서 compiler binder symbol 정보를 재사용해 LSP `textDocument/definition` Location response를 반환하게 한다.

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
- `initialize` capability에 `definitionProvider` 노출
- `textDocument/definition` request handling
- hover와 공유하는 open document identifier-to-symbol lookup helper
- bound source symbol definition Location 반환
- smoke coverage for definition response and unopened document null response
- checklist, traceability, architecture, and task queue updates

Out:
- VS Code client activation
- cross-file/project manifest-aware symbol lookup
- metadata reference definition navigation
- built-in type definition navigation
- completion

## Acceptance Criteria

- [x] `src/TypeSharp.LanguageServer` advertises `definitionProvider`.
- [x] `textDocument/definition` returns a Location for a bound TypeSharp source symbol in an open document.
- [x] definition range uses zero-based LSP positions for the declaration identifier.
- [x] unopened document definition requests return `null`.
- [x] hover keeps using the same compiler symbol lookup helper.
- [x] `LSP go-to-definition` checklist item is marked complete while completion remains open.
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
- `language server returns definition for bound symbols` passes.
- hover test still passes through the shared symbol helper.
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
- Added shared open document symbol lookup for hover and definition.
- Added `textDocument/definition` request handling to the language server.
- Advertised `definitionProvider` in initialize capabilities.
- Added source symbol definition Location response with zero-based declaration range.
- Added smoke coverage for definition response and unopened document null response.
- Updated checklist, traceability, architecture, and task queue docs.

Remaining:
- VS Code client activation.
- Cross-file/project manifest-aware symbol lookup.
- Metadata reference definition navigation.
- Built-in type definition navigation.
- Completion.

Blocked:
- None.
