# Task 0127: CLI Warnings As Errors

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:14:00 +09:00
End Time: 2026-05-19 12:18:00 +09:00

## Objective

Close the CI diagnostic gate in the CLI contract by making warnings block `typesharp check` and `typesharp build` when requested by command-line option or project manifest.

## Scope

In:
- `--warnings-as-errors` for project commands.
- Manifest `tooling.treatWarningsAsErrors = true`.
- Warning diagnostics retain their original warning severity in text and JSON output.
- `check` exits non-zero when warnings are blocking.
- `build` stops before generated C# emission when warnings are blocking.
- Smoke tests for default nonblocking warning behavior and warnings-as-errors behavior.

Out:
- Changing warning diagnostics into error diagnostics.
- Warning suppression configuration.
- Per-code warning level configuration.

## Acceptance Criteria

- [x] Warning-only `typesharp check` succeeds by default.
- [x] `typesharp check --warnings-as-errors` exits non-zero on warnings.
- [x] Manifest `treatWarningsAsErrors = true` makes `typesharp build` stop before generated output.
- [x] JSON diagnostics still report warning severity for warning diagnostics.
- [x] Checklist and traceability mention the CI warning gate evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "warnings"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused warnings-as-errors tests.
- PASS full compiler test suite.

## Handoff

Done:
- Added `--warnings-as-errors` parsing to project CLI commands.
- Added manifest warning gate handling for check/build/run paths.
- Added smoke tests for default and blocking warning behavior.
- Updated docs-site CLI summary, checklist, traceability, and task index.

Remaining:
- Per-code suppression and warning level configuration remain future diagnostics work.

Blocked:
- None.
