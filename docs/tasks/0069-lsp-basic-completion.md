# Task: LSP Basic Completion

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:27:24 +09:00
End Time: 2026-05-19 01:31:04 +09:00

## Objective

`TypeSharp.LanguageServer`가 open `.tysh` document에서 compiler binder symbol, built-in type, core keyword 기반의 LSP `textDocument/completion` response를 반환하게 한다.

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
- `initialize` capability에 `completionProvider` 노출
- `textDocument/completion` request handling
- open document binder symbols, built-in types, and core keywords as completion items
- prefix filtering for the current identifier fragment
- smoke coverage for completion response and unopened document null response
- checklist, traceability, architecture, and task queue updates

Out:
- VS Code client activation
- project-wide/cross-file completion
- metadata reference member completion
- snippet completion
- completion resolve
- ranking beyond deterministic label sort

## Acceptance Criteria

- [x] `src/TypeSharp.LanguageServer` advertises `completionProvider`.
- [x] `textDocument/completion` returns binder symbol items for an open document.
- [x] completion returns built-in type items.
- [x] completion uses deterministic prefix filtering and label sorting.
- [x] unopened document completion requests return `null`.
- [x] `LSP basic completion` checklist item is marked complete.
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
- `language server returns completion items` passes.
- diagnostics, hover, and go-to-definition tests still pass.
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
- Added `textDocument/completion` request handling to the language server.
- Advertised `completionProvider` in initialize capabilities.
- Added completion items for open document binder symbols, built-in types, and core keywords.
- Added deterministic prefix filtering and label sorting.
- Added smoke coverage for completion response and unopened document null response.
- Updated checklist, traceability, architecture, and task queue docs.

Remaining:
- VS Code client activation.
- Project-wide/cross-file completion.
- Metadata reference member completion.
- Snippet completion.
- Completion resolve.
- Ranking beyond deterministic label sort.

Blocked:
- None.
