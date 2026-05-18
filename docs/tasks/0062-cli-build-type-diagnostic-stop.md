# Task: CLI Build Type Diagnostic Stop

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:42:38 +09:00
End Time: 2026-05-19 00:46:07 +09:00

## Objective

`typesharp build`가 type checker diagnostics를 발견하면 generated C# source, generated project, generated assembly를 쓰기 전에 멈춘다는 CLI smoke coverage를 추가한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../cli.md](../cli.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `src/TypeSharp.Cli/TypeSharpCli.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- CLI `build` smoke for `TS2201` type mismatch diagnostics
- no-emission assertions for generated `.g.cs`, generated project, and generated assembly
- checklist update for high-level CLI `build`
- traceability update for type-checker diagnostic stop behavior

Out:
- new type checker rules
- source mapping changes
- generated C# backend lowering changes

## Acceptance Criteria

- [x] `typesharp build --diagnostic-format json` returns exit code 1 for a type mismatch.
- [x] CLI error output includes `TS2201` and the type mismatch message.
- [x] generated source is not emitted when type checker diagnostics contain errors.
- [x] generated project is not emitted when type checker diagnostics contain errors.
- [x] generated assembly is not emitted when type checker diagnostics contain errors.
- [x] high-level `CLI build` checklist item is marked complete with generated output and diagnostic stop coverage.

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
- new `CLI build stops before emission on type checker diagnostics` smoke passes.
- existing CLI build emission/reference/interop and full test suite still pass.
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
- Added CLI `build` JSON `TS2201` no-emission smoke coverage.
- Marked CLI `build` complete in the current checklist with explicit type-checker diagnostic stop path.

Remaining:
- Formatter convention is tracked by [0064-formatter-convention.md](0064-formatter-convention.md); continue toward VS Code/LSP scaffolding after CLI command basics.
- Add binder-specific CLI build diagnostic stop smoke if the project wants every semantic phase pinned separately.

Blocked:
- None.
