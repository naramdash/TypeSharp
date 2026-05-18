# Task: CLI Check Type Diagnostic Coverage

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:38:24 +09:00
End Time: 2026-05-19 00:41:38 +09:00

## Objective

`typesharp check`가 parser/reference diagnostics뿐 아니라 type checker diagnostics도 CLI JSON output과 exit code로 직접 보고한다는 smoke coverage를 추가한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../cli.md](../cli.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Cli/TypeSharpCli.cs`
- `src/TypeSharp.Compiler/Checking/TypeSharpChecker.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- CLI `check` smoke for `TS2201` type mismatch diagnostics
- checklist update for high-level CLI `check`
- traceability update for CLI type-checker diagnostic coverage

Out:
- new type checker rules
- binder-specific CLI diagnostic smoke
- formatter or LSP integration

## Acceptance Criteria

- [x] `typesharp check --diagnostic-format json` returns exit code 1 for a type mismatch.
- [x] CLI error output includes `TS2201`.
- [x] CLI error output includes the type mismatch message and source file path.
- [x] high-level `CLI check` checklist item is marked complete with parse/reference/type checker coverage.

## Verification

Command:

```text
dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- compiler project builds.
- support libraries and CLI build.
- new `CLI check emits JSON type checker diagnostics` smoke passes.
- existing CLI check parse/reference and full test suite still pass.
- CLI check reports no diagnostics for the example project.

Result:
- Pass. `dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Added CLI `check` JSON `TS2201` smoke coverage.
- Marked CLI `check` complete in the current checklist with explicit type-checker diagnostic path.

Remaining:
- Add binder-specific CLI diagnostic smoke if the project wants every semantic phase pinned at the CLI boundary.
- Continue toward formatter convention and VS Code/LSP scaffolding after CLI command basics.

Blocked:
- None.
