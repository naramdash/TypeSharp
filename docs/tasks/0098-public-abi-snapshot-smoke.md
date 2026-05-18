# Task: Public ABI Snapshot Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:52:14 +09:00
End Time: 2026-05-19 04:56:57 +09:00

## Objective

Add a public ABI snapshot smoke test for generated `net48` assemblies so C# consumer compatibility is backed by metadata shape assertions, not only compile success.

## Source Of Truth

- [../goal.md](../goal.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../lowering.md](../lowering.md)
- [../checklist.md](../checklist.md)
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- generated assembly metadata read using the existing metadata reader
- public type, method, property, parameter, and return type snapshot assertions
- checklist and traceability updates

Out:
- binary compatibility diff tooling
- full API baseline file format
- nested union case ABI snapshot
- release-time breaking change enforcement

## Acceptance Criteria

- [x] Test builds a generated `net48` TypeSharp library.
- [x] Test reads the generated DLL metadata without errors.
- [x] Test asserts public Module method signature.
- [x] Test asserts public record properties and methods.
- [x] Checklist and traceability mark public ABI snapshot tests with the new evidence.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- public ABI snapshot smoke passes.
- existing tests remain green.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj`
- Pass. `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj`
- Pass. `dotnet run --project src\TypeSharp.Cli\TypeSharp.Cli.csproj -- check docs\examples\cli-console\TypeSharp.toml --diagnostic-format json` returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Added `GeneratedNet48AssemblyPublicAbiSnapshotIsStable`.
- Verified generated `net48` DLL metadata shape for public module method and immutable record API.
- Marked public ABI snapshot evidence in checklist, traceability, and task index.

Remaining:
- None.

Blocked:
- None.
