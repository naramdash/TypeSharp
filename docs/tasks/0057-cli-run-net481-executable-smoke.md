# Task: CLI Run Net481 Executable Smoke

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:06:24 +09:00
End Time: 2026-05-19 00:09:27 +09:00

## Objective

`typesharp run`이 `outputType = "exe"` project를 build한 뒤 generated `net481` executable을 현재 Windows .NET Framework 환경에서 실행하는 최소 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../cli.md](../cli.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Building`
- `src/TypeSharp.Cli`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generated C# entry point for executable projects
- `.exe` generated assembly path for `outputType = "exe"`
- `typesharp run` build-and-execute flow
- smoke test for `main(): string` generated executable output
- checklist and traceability updates

Out:
- `main(args: string[])` lowering and argument passing into TypeSharp main
- async main support
- richer return type policy beyond `string` and `int`
- current `docs/examples/cli-console` feature-complete execution
- `typesharp new` templates

## Acceptance Criteria

- [x] executable projects emit a generated C# entry point.
- [x] generated executable path uses `.exe`.
- [x] `typesharp run` rejects non-`exe` projects before execution.
- [x] `typesharp run` executes the generated `net481` executable after a successful build.
- [x] smoke test observes generated executable output from a TypeSharp `main(): string`.
- [x] docs record the current run limitation.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- CLI run generated executable smoke passes.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Added generated entry point emission for executable projects.
- Added `typesharp run` build-and-execute flow.
- Added smoke test that builds and runs a generated `net481` executable.
- Updated CLI docs, checklist, and traceability.

Completed later:
- `main(args: string[])` lowering and `--` argument forwarding are completed by [0059-cli-run-main-args-forwarding.md](0059-cli-run-main-args-forwarding.md).

Remaining:
- Expand generated entry point return handling beyond the current `string`/`int` smoke path.
- Bring the richer `docs/examples/cli-console` source into backend-supported executable coverage after records/arrays/if/for/pipeline lowering mature.

Blocked:
- None.
