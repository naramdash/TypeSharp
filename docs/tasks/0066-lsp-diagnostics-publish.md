# Task: LSP Diagnostics Publish

Status: Done
Queue: Q4
Start Time: 2026-05-19 01:10:03 +09:00
End Time: 2026-05-19 01:13:13 +09:00

Timing note:
- Exact implementation start was not captured before this packet was opened.
- The recorded start time is when the task packet was created on the current computer clock.

## Objective

`TypeSharp.LanguageServer`가 open `.tysh` document를 compiler parser/binder/type checker로 검사하고 LSP `textDocument/publishDiagnostics` notification으로 진단을 내보내게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../architecture.md](../architecture.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.LanguageServer`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TypeSharp.LanguageServer` executable project skeleton
- minimal stdio JSON-RPC/LSP message framing
- `initialize`, `shutdown`, `exit`, `textDocument/didOpen`, and `textDocument/didChange` handling
- compiler diagnostics to LSP diagnostic mapping
- smoke coverage for zero-based LSP ranges and `didOpen` diagnostic publishing
- checklist, traceability, architecture, and task queue updates

Out:
- VS Code client activation
- workspace/project manifest discovery from LSP initialization options
- hover
- go-to-definition
- completion
- incremental text edit ranges
- LSP packaging/publishing

## Acceptance Criteria

- [x] `src/TypeSharp.LanguageServer` builds as a modern .NET host and references `TypeSharp.Compiler`.
- [x] open document diagnostics reuse compiler parser, binder, and type checker diagnostics.
- [x] LSP diagnostics use zero-based ranges and LSP severity values.
- [x] `textDocument/didOpen` publishes `textDocument/publishDiagnostics` with compiler diagnostic code/source/message.
- [x] `LSP diagnostics` checklist item is marked complete while hover/go-to-definition/completion remain open.
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
- `LSP diagnostic mapper uses zero-based ranges` passes.
- `language server publishes diagnostics on didOpen` passes.
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
- Added `src/TypeSharp.LanguageServer` as a stdio LSP host project.
- Added minimal JSON-RPC/LSP framing for `initialize`, `shutdown`, `exit`, `textDocument/didOpen`, and `textDocument/didChange`.
- Wired open document diagnostics through the TypeSharp parser, binder, and type checker.
- Added compiler diagnostic to LSP diagnostic mapping with zero-based ranges and LSP severity values.
- Added smoke tests for diagnostic mapping and `didOpen` diagnostic publishing.
- Updated checklist, traceability, architecture, and task queue docs.

Remaining:
- VS Code client activation.
- Project manifest-aware LSP diagnostics.
- Hover.
- Go-to-definition.
- Completion.

Blocked:
- None.
