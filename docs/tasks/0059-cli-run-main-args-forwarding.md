# Task: CLI Run Main Args Forwarding

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:21:04 +09:00
End Time: 2026-05-19 00:26:32 +09:00

## Objective

`typesharp run`이 `--` 뒤 program arguments를 generated C# entry point에서 TypeSharp `main(args: string[])` 함수로 전달하게 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../cli.md](../cli.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `src/TypeSharp.Cli`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- C# backend function parameter emission for annotated TypeSharp parameters
- C# backend `ArrayType` mapping for `string[]`
- generated entry point detection for `main(args: string[])`
- forwarding CLI program args from generated `Program.Main(string[] args)` to `Module.main(args)`
- smoke test that runs a generated `net48` executable with two arguments
- CLI docs, checklist, and traceability updates

Out:
- arbitrary main signature diagnostics
- async main support
- richer return type handling beyond the current `int` exit code and stdout object path
- full collection/indexer/loop lowering for the richer sample project

## Acceptance Criteria

- [x] backend emits `public static string main(string[] args)` for `main(args: string[]): string`.
- [x] executable entry point emits `Module.main(args)` when manifest main resolves to a single `string[]` parameter.
- [x] existing `main()` executable smoke remains supported.
- [x] `typesharp run <manifest> -- alpha beta` prints `2` for an args length smoke program.
- [x] docs record current `main(args: string[])` support and remaining async/richer return limitations.

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
- existing CLI run generated executable smoke still passes.
- new CLI run argument forwarding smoke passes and verifies generated source contains `main(string[] args)` and `Module.main(args)`.
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
- Function parameters are emitted into generated C# methods.
- `string[]` TypeSharp parameter types lower to C# `string[]`.
- `typesharp run` forwards `--` arguments into `main(args: string[])` for the supported smoke path.

Remaining:
- Add explicit diagnostics for unsupported executable `main` signatures.
- Expand generated entry point support for async main and richer return types.
- Bring `docs/examples/cli-console` into executable coverage after the backend supports its richer syntax.

Blocked:
- None.
