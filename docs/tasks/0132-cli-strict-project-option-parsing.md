# Task 0132: CLI Strict Project Option Parsing

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:47:00 +09:00
End Time: 2026-05-19 12:53:00 +09:00

## Objective

Stop project commands from silently ignoring unknown options while preserving the documented `--preview` option as a recognized future feature gate switch.

## Scope

In:
- `typesharp check/build/run` project command option parsing.
- Explicit acceptance of `--preview`.
- Usage failure for unknown `--*` project options.
- Smoke tests and traceability updates.

Out:
- Actual preview feature enablement.
- Preview syntax diagnostics.
- Manifest-level preview configuration.

## Acceptance Criteria

- [x] `typesharp check <project> --preview` is accepted.
- [x] Unknown project command options return usage exit code `2`.
- [x] Unknown project command options are not silently ignored.
- [x] CLI docs, checklist, and traceability include strict option parsing evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "project option"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused project option smoke tests.
- PASS full compiler test suite.

## Handoff

Done:
- Project command parser accepts `--preview`.
- Project command parser rejects unknown options with usage exit code `2`.
- Focused preview and unknown-option smokes were added.

Remaining:
- None.

Blocked:
- None.
