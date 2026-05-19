# Task 0124: CLI New MVP

Status: Done
Queue: Q4
Start Time: 2026-05-19 11:43:30 +09:00
End Time: 2026-05-19 11:50:00 +09:00

## Objective

Close the CLI project creation gap in the core VS Code/CLI development loop by implementing `typesharp new` for starter console and library projects targeting .NET Framework 4.8.

## Scope

In:
- `typesharp new console <name>`.
- `typesharp new library <name>`.
- `--target net48` validation.
- `--output <path>` for deterministic smoke tests and scripted project creation.
- Generated `TypeSharp.toml`, `.gitignore`, and starter source under `src/`.
- Overwrite protection for non-empty output directories.
- Smoke tests and docs traceability.

Out:
- ASP.NET/WCF/worker host-specific templates.
- NuGet/package restore templates.
- Interactive prompts and force overwrite mode.

## Acceptance Criteria

- [x] Console template creates an executable `net48` TypeSharp project with `main`.
- [x] Library template creates a library `net48` TypeSharp project.
- [x] Generated starter projects pass `typesharp check`.
- [x] Generated starter projects pass `typesharp build`.
- [x] Console starter project can run with `typesharp run` in the current local environment.
- [x] Existing non-empty directories are not overwritten.
- [x] Checklist and traceability mention the `new` implementation evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll new console HelloApp --output <tmp>\HelloApp
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll check <tmp>\HelloApp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll build <tmp>\HelloApp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll run <tmp>\HelloApp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll new library Billing.Core --target net48 --output <tmp>\Billing.Core
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll check <tmp>\Billing.Core\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll build <tmp>\Billing.Core\TypeSharp.toml
```

Result:
- PASS compiler/test project build.
- PASS console project creation.
- PASS console `typesharp check`.
- PASS console `typesharp build`.
- PASS console `typesharp run` printing `Hello, TypeSharp`.
- PASS library project creation.
- PASS library `typesharp check`.
- PASS library `typesharp build`.

## Handoff

Done:
- Added `typesharp new` command handling to the CLI.
- Added console/library template generation.
- Added smoke tests for template shape and overwrite safety.
- Updated docs-site CLI summary, checklist, traceability, and task index.

Remaining:
- Host-specific ASP.NET/WCF/worker templates remain Stable Backlog.

Blocked:
- Full in-process test harness execution is still blocked locally by Windows Application Control on `TypeSharp.Compiler.Tests.dll` until the workspace is added to the local exclusion policy.
