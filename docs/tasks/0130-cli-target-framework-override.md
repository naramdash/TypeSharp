# Task 0130: CLI Target Framework Override

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:36:00 +09:00
End Time: 2026-05-19 12:43:00 +09:00

## Objective

Connect the documented `--target net48` project command option to generated `net48` build and run outputs instead of silently consuming it.

## Scope

In:
- `typesharp build --target net48`.
- `typesharp run --target net48`.
- Validation for unsupported target framework values.
- Generated SDK-style C# project `<TargetFramework>` override.
- Reported generated assembly/executable paths.
- Smoke tests and traceability updates.

Out:
- Arbitrary target frameworks.
- Multi-targeting.
- Runtime compatibility guarantees beyond the repository's current `net48` baseline.
- Manifest migration or target inference.

## Acceptance Criteria

- [x] `typesharp build --target net48` validates the target value.
- [x] Build output uses CLI target override when manifest `targetFramework` differs.
- [x] `typesharp run --target net48` builds and runs or launch-checks the generated `bin/Debug/net48/*.exe`.
- [x] Invalid target values return usage exit code `2`.
- [x] CLI docs, checklist, and traceability include target override evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "target framework"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused target framework smoke tests.
- PASS full compiler test suite.

## Handoff

Done:
- CLI project command parsing validates `--target net48`.
- `TypeSharpBuilder.Build` accepts an optional target framework override.
- Generated project target framework and generated assembly path use the override when present.
- Focused build/run/invalid target smokes were added.

Remaining:
- None.

Blocked:
- None.
