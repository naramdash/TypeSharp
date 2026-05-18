# Task Rollup: CLI Run Net48 Executable Flow

Status: Done
Queue: Q0-Q4
Start Time: 2026-05-19 00:06:24 +09:00
End Time: 2026-05-19 00:34:58 +09:00

## Objective

`typesharp run`이 `net48` executable project를 build하고 실행하며, `main()` 및 `main(args: string[])` entry point를 generated C# entry point로 안전하게 연결하도록 만든다.

## Compressed Packets

- `0057-cli-run-net481-executable-smoke`
- `0058-net48-default-target-migration`
- `0059-cli-run-main-args-forwarding`
- `0060-cli-run-main-signature-diagnostic`

## Source Of Truth

- [../goal.md](../goal.md)
- [../framework-targeting.md](../framework-targeting.md)
- [../cli.md](../cli.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `src/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- `src/TypeSharp.Cli`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generated C# entry point for executable projects
- `.exe` generated assembly path for `outputType = "exe"`
- default generated artifact/runtime/core target migration from `net481` to `net48`
- `typesharp run` build-and-execute flow
- TypeSharp function parameter emission in C# backend
- `string[]` TypeSharp type mapping to C# `string[]`
- forwarding `--` program arguments into `main(args: string[])`
- `TS3500` unsupported executable entry point diagnostic before generated emission
- smoke coverage for generated executable run, args forwarding, unsupported main signature, `net48` builds, and example `check`

Out:
- async main support
- richer executable return type policy
- source-span precise diagnostics for entry point validation
- full `docs/examples/cli-console` executable lowering for records, arrays, `if`, `for`, and pipeline
- `typesharp new` templates
- optional parallel `net481` latest Framework verification profile

## Acceptance Criteria

- [x] executable projects emit a generated C# entry point.
- [x] generated executable path uses `.exe`.
- [x] generated artifacts, `TypeSharp.Core`, and `TypeSharp.Runtime` use `net48` as the default target.
- [x] `typesharp run` rejects non-`exe` projects before execution.
- [x] `typesharp run` executes a generated `net48` executable after successful build.
- [x] backend emits `main(string[] args)` for TypeSharp `main(args: string[])`.
- [x] generated entry point emits `Module.main(args)` when manifest main resolves to a single `string[]` parameter.
- [x] unsupported executable main parameter signatures report `TS3500` before generated source/project/executable emission.
- [x] docs record current `run` support and remaining async/richer return limitations.

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
- Core and Runtime build into `bin/Debug/net48`.
- CLI builds.
- descriptor registry includes `TS3500`.
- CLI run generated executable smoke passes.
- CLI run argument forwarding smoke passes and verifies generated source contains `main(string[] args)` and `Module.main(args)`.
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
- Implemented initial `typesharp run` for generated `net48` executables.
- Switched generated artifact/runtime/core default target to `net48`.
- Forwarded `--` program arguments into `main(args: string[])`.
- Added `TS3500` for unsupported executable entry point signatures.
- Marked initial CLI `run` checklist coverage complete with explicit limitations.

Remaining:
- Add async main support.
- Define and implement richer executable return type policy.
- Bring `docs/examples/cli-console` into executable coverage after backend support for its richer syntax matures.
- Add optional `net481` latest Framework profile only if dual-profile verification becomes a project requirement.

Blocked:
- None.
