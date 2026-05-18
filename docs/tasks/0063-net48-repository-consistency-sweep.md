# Task: Net48 Repository Consistency Sweep

Status: Done
Queue: Q0-Q3
Start Time: 2026-05-19 00:47:58 +09:00
End Time: 2026-05-19 00:51:59 +09:00

## Objective

`net481`에서 `net48`로 내려온 기본 build target 결정이 코드, 문서 색인, task rollup 이름과 본문에 일관되게 반영되어 있는지 repo-wide로 정리한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../framework-targeting.md](../framework-targeting.md)
- [../requirements.md](../requirements.md)
- [../README.md](../README.md)
- [../traceability.md](../traceability.md)
- [README.md](README.md)
- `src/TypeSharp.Compiler`
- `src/TypeSharp.Core`
- `src/TypeSharp.Runtime`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- stale `net481` task rollup file names and links
- generated artifact/runtime/core task packet wording that now represents the `net48` baseline
- documentation index wording that accidentally collapsed `net48`/`net481` comparison text
- repo-wide search verification for intentional versus stale `net481` references

Out:
- code target migration already covered by [0057-0060-cli-run-net48-executable-flow.md](0057-0060-cli-run-net48-executable-flow.md)
- removing intentional `net481` latest Framework profile references from [../framework-targeting.md](../framework-targeting.md)
- adding dual-profile `net481` build support

## Acceptance Criteria

- [x] Core/Runtime/compiler default target code still reports `net48`.
- [x] task rollup filenames and links use `net48` when they describe the current generated artifact baseline.
- [x] remaining `net481` references are limited to intentional latest Framework profile comparison or migration history.
- [x] repository build/test smoke still passes after the documentation consistency sweep.

## Verification

Command:

```text
rg -n "net481|Net481|4\.8\.1" docs src tests
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- stale `net481` build target references are gone from code and task rollup baseline wording.
- intentional `net481` references remain only for latest Framework profile comparison or migration history.
- tests, support library builds, CLI build, and example CLI check pass.

Result:
- Pass. `rg -n "net481|Net481|4\.8\.1" docs src tests` now returns only intentional latest Framework profile comparison, migration history, official .NET Framework 4.8.1 reference text, and this task packet.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Renamed stale task rollup files from `net481` to `net48` where they describe the current generated artifact baseline.
- Updated task README and traceability links to the renamed rollups.
- Normalized older task rollup wording from `net481` to `net48` for runtime/core/generated artifact baseline references.
- Restored `net48`/`net481` comparison wording in README and requirements where both profiles are intentionally contrasted.

Remaining:
- Commit the consistency sweep.

Blocked:
- None.
