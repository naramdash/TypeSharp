# Task: Net48 Default Target Migration

Status: Done
Queue: Q0
Start Time: 2026-05-19 00:13:56 +09:00
End Time: 2026-05-19 00:18:35 +09:00

## Objective

TypeSharp generated artifact, `TypeSharp.Core`, and `TypeSharp.Runtime`의 기본 .NET Framework target을 `net481`에서 `net48`로 낮추고, 코드/테스트/현재 기준 문서/예제가 같은 기준을 사용하게 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../framework-targeting.md](../framework-targeting.md)
- [../requirements.md](../requirements.md)
- [../dependencies.md](../dependencies.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/TypeSharpCompilerInfo.cs`
- `src/TypeSharp.Core/TypeSharp.Core.csproj`
- `src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- compiler default target framework switch to `net48`
- `TypeSharp.Core` and `TypeSharp.Runtime` project target framework switch to `net48`
- generated project/test expectations and output paths updated to `net48`
- docs, examples, and current traceability/checklist language aligned to `net48`
- `framework-targeting.md` updated so `net48` is the current baseline and `net481` remains an optional latest Framework profile

Out:
- renaming historical task packet files that recorded earlier `net481` work
- adding a parallel `net481` verification profile
- lowering support changes unrelated to the target framework migration

## Acceptance Criteria

- [x] `TypeSharpCompilerInfo.DefaultTargetFramework` is `net48`.
- [x] `TypeSharp.Core` and `TypeSharp.Runtime` build as `net48`.
- [x] generated project scaffolds, output path checks, and C# interop smokes expect `net48`.
- [x] CLI example manifest defaults to `net48`.
- [x] current project docs describe `net48` as the baseline target.
- [x] official/latest Framework references still preserve `net481` only as comparison or optional profile context.

## Verification

Command:

```text
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- Core/Runtime build into `bin/Debug/net48`.
- test suite reports generated `net48` compile/build/run and host compatibility smokes as passing.
- CLI check reports no diagnostics for the example project.

Result:
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Switched compiler default target, runtime/core TFM, runtime info, generated path tests, and examples to `net48`.
- Updated current docs to treat `.NET Framework 4.8`/`net48` as the baseline.
- Left `net481` references only where they describe official latest Framework status, comparison, optional profile, or historical task packet filenames.

Completed later:
- CLI run `main(args: string[])` forwarding is completed by [0059-cli-run-main-args-forwarding.md](0059-cli-run-main-args-forwarding.md).

Remaining:
- Add an explicit optional `net481` latest Framework profile only if the project decides to keep dual-profile smoke coverage.

Blocked:
- None.
