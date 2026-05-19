# Task 0131: CLI Verbosity Build Logging

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:42:00 +09:00
End Time: 2026-05-19 12:49:00 +09:00

## Objective

Make the documented `--verbosity quiet|minimal|normal|diagnostic` project command option validated and useful for generated build success logs.

## Scope

In:
- Project command `--verbosity` parsing.
- Validation for `quiet`, `minimal`, `normal`, and `diagnostic`.
- `typesharp build` success output behavior:
  - `quiet`: no artifact log output.
  - `minimal`: final generated assembly path only.
  - `normal`: existing generated source/project/assembly log shape.
  - `diagnostic`: normal output plus build option summary.
- Smoke tests and traceability updates.

Out:
- MSBuild verbosity forwarding.
- Check/run/format verbosity-specific output shaping.
- Structured logging.
- Colorized output.

## Acceptance Criteria

- [x] Invalid verbosity values return usage exit code `2`.
- [x] `typesharp build --verbosity quiet` emits generated artifacts but no success log output.
- [x] `typesharp build --verbosity minimal` reports only the final generated assembly path.
- [x] Existing normal build output remains unchanged.
- [x] CLI docs, checklist, and traceability include verbosity evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "verbosity"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused verbosity smoke tests.
- PASS full compiler test suite.

## Handoff

Done:
- CLI project command parsing validates `--verbosity`.
- Build success logging respects `quiet`, `minimal`, `normal`, and `diagnostic`.
- Focused invalid/quiet/minimal smokes were added.

Remaining:
- None.

Blocked:
- None.
