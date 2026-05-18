# Task: CLI Run Main Signature Diagnostic

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:29:06 +09:00
End Time: 2026-05-19 00:34:58 +09:00

## Objective

Executable project의 configured `main`이 현재 지원하는 `main()` 또는 `main(args: string[])` 형태가 아닐 때 generated C# build failure로 흘러가지 않고, emission 전에 명확한 diagnostic을 보고한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../cli.md](../cli.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `src/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TS3500` unsupported executable entry point descriptor
- executable entry point validation before generated C# emission
- missing configured main diagnostic
- unsupported main parameter signature diagnostic
- CLI run smoke for unsupported `main(count: int)` signature
- checklist, diagnostics, CLI docs, and traceability updates

Out:
- async main support
- richer return type policy
- source-span precise diagnostics for the function declaration
- full sample project executable lowering

## Acceptance Criteria

- [x] descriptor registry includes `TS3500`.
- [x] executable build validates the configured main before writing generated files.
- [x] supported `main()` and `main(args: string[])` paths still pass.
- [x] unsupported executable main parameter signature returns CLI exit code 1 with `TS3500`.
- [x] unsupported executable main parameter signature does not emit generated source, project, or executable.
- [x] docs record `TS3500` and current executable `main` shape constraints.

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
- descriptor registry smoke includes `TS3500`.
- CLI run generated executable and argument forwarding smokes still pass.
- CLI run unsupported main signature smoke reports `TS3500` before emission.
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
- Added `TS3500` for unsupported executable entry points.
- Stopped executable builds before generated emission when configured main is missing or has unsupported parameters.
- Marked the initial CLI `run` checklist item complete with explicit current limitations.

Remaining:
- Add async main support.
- Define and implement richer executable return type policy.
- Add source-specific spans for entry point diagnostics after project-level diagnostics grow source-oriented factory helpers.

Blocked:
- None.
