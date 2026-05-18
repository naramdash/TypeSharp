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

## 2026-05-19 Refresh

Refresh Start Time: 2026-05-19 03:08:05 +09:00
Refresh End Time: 2026-05-19 03:10:50 +09:00

Trigger:
- User confirmed the build target has moved from `net481` to `net48` and requested a repo-wide refresh with that decision in mind.

Refresh Scope:
- Re-check code, project files, tests, docs, and task packets for stale `net481` build target references.
- Keep intentional `net481` mentions only where they describe the latest .NET Framework profile comparison, official .NET Framework 4.8.1 facts, or migration history.
- Verify the repository still builds and the compiler smoke suite still passes with `net48` as the generated artifact/runtime/core baseline.

Refresh Result:
- Pass. `rg -n "net481|Net481|NET481|v4\.8\.1|4\.8\.1" docs src tests agent.md -S` returns no stale code, project, or test target references. Remaining matches are intentional latest Framework profile comparison, official .NET Framework 4.8.1 facts, migration history, or this task packet.
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` after serial rerun. The first parallel run hit a shared `src/TypeSharp.Cli/obj` file lock, not a source or target-framework failure.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json` returned `{ "diagnostics": [] }`.
- Pass. `git diff --check`

## Handoff

Done:
- Renamed stale task rollup files from `net481` to `net48` where they describe the current generated artifact baseline.
- Updated task README and traceability links to the renamed rollups.
- Normalized older task rollup wording from `net481` to `net48` for runtime/core/generated artifact baseline references.
- Restored `net48`/`net481` comparison wording in README and requirements where both profiles are intentionally contrasted.
- Reopened this packet for a 2026-05-19 repo-wide refresh instead of creating a duplicate task packet for the same topic.

Remaining:
- None.

Blocked:
- None.

## 2026-05-19 Net48 Recheck

Recheck Start Time: 2026-05-19 03:43:39 +09:00
Recheck End Time: 2026-05-19 03:46:26 +09:00

Trigger:
- User reiterated that the build target moved from `net481` to `net48` and asked to refresh the full repository with that in mind.

Result:
- Pass. `rg -n "net481" .` returns no stale code, project, or test target references. Remaining matches are intentional latest Framework profile comparison, migration history, or this task packet.
- Pass. `src/TypeSharp.Runtime/TypeSharp.Runtime.csproj` targets `net48`; compiler/CLI hosts remain `net10.0`.
- Pass. Current in-flight nominal union match work keeps generated assembly and C# consumer smoke paths on `net48`.

## 2026-05-19 Post-Null-Safety Net48 Recheck

Recheck Start Time: 2026-05-19 04:17:03 +09:00
Recheck End Time: 2026-05-19 04:19:41 +09:00

Trigger:
- User reiterated that the repository should be refreshed with the `net481` to `net48` build target change in mind.

Result:
- Pass. `rg -n "net481|Net481|NET481" src tests Directory.Build.props agent.md -S` returns no code, project, test, or agent contract matches.
- Pass. `rg -n "net481|Net481|NET481" docs -S` returns only intentional latest Framework profile comparison, migration history, and this task packet.
- Pass. `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj` emits `bin\Debug\net48\TypeSharp.Core.dll`.
- Pass. `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj` emits `bin\Debug\net48\TypeSharp.Runtime.dll`.
- Pass. `dotnet build src\TypeSharp.Cli\TypeSharp.Cli.csproj`.
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project src\TypeSharp.Cli\TypeSharp.Cli.csproj -- check docs\examples\cli-console\TypeSharp.toml --diagnostic-format json` returned `{ "diagnostics": [] }`.

## 2026-05-19 Public ABI Net48 Recheck

Recheck Start Time: 2026-05-19 04:54:52 +09:00
Recheck End Time: 2026-05-19 04:56:57 +09:00

Trigger:
- User reiterated that the repository should be refreshed with the `net481` to `net48` build target change in mind.

Result:
- Pass. `rg -n "net481|Net481|NET481|v4\.8\.1|4\.8\.1" docs src tests agent.md -S` returns no stale code, project, or test target references. Remaining matches are intentional latest Framework profile comparison, official .NET Framework 4.8.1 facts, migration history, or this task packet.
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj` emits `bin\Debug\net48\TypeSharp.Core.dll`.
- Pass. `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj` emits `bin\Debug\net48\TypeSharp.Runtime.dll`.
- Pass. `dotnet run --project src\TypeSharp.Cli\TypeSharp.Cli.csproj -- check docs\examples\cli-console\TypeSharp.toml --diagnostic-format json` returned `{ "diagnostics": [] }`.

## 2026-05-19 Source Backend Net48 Recheck

Recheck Start Time: 2026-05-19 05:20:30 +09:00
Recheck End Time: 2026-05-19 05:25:12 +09:00

Trigger:
- User reiterated that the repository should be refreshed with the `net481` to `net48` build target change in mind after the C# source backend audit.

Result:
- Pass. `rg -n "net481|Net481|NET481" src tests Directory.Build.props agent.md -S` returns no code, project, test, or agent contract matches.
- Pass. Remaining `net481`/4.8.1 matches in docs are intentional latest Framework profile comparison, official .NET Framework 4.8.1 facts, migration history, or this task packet.
- Pass. `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj` emits `bin\Debug\net48\TypeSharp.Core.dll`.
- Pass. `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj` emits `bin\Debug\net48\TypeSharp.Runtime.dll`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `git diff --check`.

## 2026-05-19 Feature Spec Net48 Recheck

Recheck Start Time: 2026-05-19 05:56:45 +09:00
Recheck End Time: 2026-05-19 05:58:21 +09:00

Trigger:
- User reiterated that the build target moved from `net481` to `net48`, noted that the docs were updated, and asked to refresh the whole repository with that in mind.

Result:
- Pass. `rg -n "net481|Net481|NET481|v4\.8\.1|4\.8\.1" src tests vscode Directory.Build.props agent.md -S` returns no code, project, test, VS Code, or agent contract matches.
- Pass. `rg -n "<TargetFramework>|TargetFramework|DefaultTargetFramework|net48|net481" src tests Directory.Build.props -S` confirms `TypeSharp.Core` and `TypeSharp.Runtime` target `net48`, generated project defaults remain `net48`, and compiler/CLI/LSP/test hosts remain `net10.0`.
- Pass. Remaining `net481`/4.8.1 matches in docs are intentional latest Framework profile comparison, official .NET Framework 4.8.1 facts, migration history, or this task packet.
- Pass. `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj` emits `bin\Debug\net48\TypeSharp.Core.dll`.
- Pass. `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj` emits `bin\Debug\net48\TypeSharp.Runtime.dll`.
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`.
