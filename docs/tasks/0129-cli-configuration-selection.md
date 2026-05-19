# Task 0129: CLI Configuration Selection

Status: Done
Queue: Q4
Start Time: 2026-05-19 12:30:00 +09:00
End Time: 2026-05-19 12:35:00 +09:00

## Objective

Make the documented `--configuration Debug|Release` project command option real for generated `net48` build and run outputs.

## Scope

In:
- `typesharp build --configuration Debug|Release`.
- `typesharp run --configuration Debug|Release`.
- Validation for unsupported configuration values.
- Generated SDK-style C# project build invocation.
- Reported generated assembly/executable paths.
- Smoke tests and traceability updates.

Out:
- Arbitrary MSBuild configuration names.
- Manifest-level configuration defaults.
- Verbosity-controlled MSBuild logging.
- Target framework override behavior.

## Acceptance Criteria

- [x] `typesharp build --configuration Release` invokes generated project build with Release configuration.
- [x] Build output reports `bin/Release/net48/*.dll` for library projects.
- [x] `typesharp run --configuration Release` builds and runs or launch-checks the generated `bin/Release/net48/*.exe`.
- [x] Invalid configuration values return usage exit code `2`.
- [x] CLI docs, checklist, and traceability include configuration selection evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "configuration"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused configuration smoke tests.
- PASS full compiler test suite.

## Handoff

Done:
- CLI project command parsing validates `Debug|Release`.
- `TypeSharpBuilder.Build` accepts the selected configuration and passes it to generated `dotnet build`.
- Generated assembly path reporting uses the selected configuration.
- Focused build/run/invalid configuration smokes were added.

Remaining:
- None.

Blocked:
- None.
