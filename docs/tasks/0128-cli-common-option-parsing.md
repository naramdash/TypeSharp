# Task 0128: CLI Common Option Parsing

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:21:00 +09:00
End Time: 2026-05-19 12:24:00 +09:00

## Objective

Tighten the CLI contract for CI-friendly common options by accepting `--no-color` consistently on MVP command paths and rejecting invalid diagnostic format values on project commands.

## Scope

In:
- `typesharp version --no-color`.
- `typesharp new ... --no-color`.
- `typesharp check/build/run ... --no-color`.
- `typesharp explain ... --no-color`.
- `typesharp format ... --no-color`.
- Validation for project command `--diagnostic-format text|json`.
- Smoke tests for no-color acceptance and invalid diagnostic format failure.

Out:
- Colored output implementation.
- Verbosity-controlled logging behavior.
- Full option override logging.

## Acceptance Criteria

- [x] `--no-color` is accepted on representative MVP command paths.
- [x] Invalid project command diagnostic format returns usage exit code `2`.
- [x] Existing command behavior is unchanged when no common option is passed.
- [x] Checklist and traceability include common option evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "common no-color"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "invalid diagnostic format"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS common no-color focused smoke.
- PASS invalid diagnostic format focused smoke.
- PASS full compiler test suite.

## Handoff

Done:
- Updated CLI option parsing for `version`, `new`, project commands, `explain`, and `format`.
- Added focused smoke tests.
- Updated checklist, traceability, and task index.

Remaining:
- Actual colorized output and verbosity-controlled logging remain future tooling work.

Blocked:
- None.
