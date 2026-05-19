# Task 0122: CLI Format MVP

Status: Done
Queue: Q4
Start Time: 2026-05-19 10:47:08 +09:00
End Time: 2026-05-19 10:47:52 +09:00

## Objective

Close the `docs/goal.md` CLI artifact gap by implementing the initial `typesharp format` command described in [../cli.md](../cli.md) and [../formatting.md](../formatting.md).

## Scope

In:
- `typesharp format [project-or-file]`.
- `typesharp format --check`.
- Manifest/source discovery for project formatting.
- Single `.tysh` file formatting.
- Parse-diagnostic no-rewrite behavior.
- Safe whitespace normalization: LF line endings, trailing whitespace removal, repeated blank line collapse, final newline.
- CLI smoke tests and docs/checklist/traceability updates.

Out:
- Full AST-based reflow.
- Declaration sorting.
- Indentation recalculation.
- VS Code formatter provider, which is now tracked by [0125-vscode-format-provider.md](0125-vscode-format-provider.md).

## Acceptance Criteria

- [x] `typesharp format` is listed in CLI help and command dispatch.
- [x] `--check` returns non-zero when a file would change and writes no files.
- [x] `format` writes normalized source for parser-clean files.
- [x] Parse diagnostics prevent rewrite and are reported through normal CLI diagnostics.
- [x] Project source discovery and direct file input are both supported.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI format"
```

Result:
- PASS compiler test project build.
- PASS `CLI format check succeeds on formatted project`.
- PASS `CLI format checks and writes normalized source`.
- PASS `CLI format reports parse diagnostics without rewriting`.

## Handoff

Done:
- Added CLI formatter MVP.

Remaining:
- Full AST-based formatter, declaration sorting, and indentation recalculation remain future tooling work.

Blocked:
- None.
