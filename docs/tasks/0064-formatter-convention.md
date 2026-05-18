# Task: Formatter Convention

Status: Done
Queue: Q4
Start Time: 2026-05-19 00:53:18 +09:00
End Time: 2026-05-19 00:58:47 +09:00

## Objective

`typesharp format`, VS Code formatter, examples, parser fixtures가 공유할 `.tysh` canonical layout을 문서화해 도구 구현의 입력을 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../cli.md](../cli.md)
- [../grammar/consistency.md](../grammar/consistency.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- official formatting convention document
- CLI `typesharp format` section link and behavior alignment
- README/checklist/traceability updates
- task packet recording with start/end time

Out:
- formatter implementation
- `typesharp format` command wiring
- VS Code extension implementation
- linter naming policy

## Acceptance Criteria

- [x] `docs/formatting.md` defines canonical layout for namespace/import/declaration order, indentation, semicolon policy, declarations, pipeline, match, lists, structural shapes, interop, and `--check` behavior.
- [x] `docs/cli.md` references the formatting convention from `typesharp format`.
- [x] `docs/README.md`, `docs/checklist.md`, `docs/traceability.md`, and `docs/tasks/README.md` reflect the new document.
- [x] Verification confirms documentation links and standard build/test smoke remain valid.

## Verification

Command:

```text
rg -n "formatting.md|formatter convention|typesharp format" docs
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Expected:
- formatting convention links are present.
- tests, support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- Pass. `rg -n "formatting.md|formatter convention|typesharp format" docs`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.
- Pass. `git diff --check`

## Handoff

Done:
- Added [../formatting.md](../formatting.md) as the official `.tysh` formatting convention.
- Linked `typesharp format` to the convention in [../cli.md](../cli.md).
- Updated README, requirements, checklist, traceability, and task queue references.
- Updated earlier CLI task handoffs to point to this formatter convention task.

Remaining:
- Commit the formatter convention documentation.

Blocked:
- None.
