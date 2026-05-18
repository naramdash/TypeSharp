# Task: CSharp Optional Parameter Overload Validation

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:26:04 +09:00
End Time: 2026-05-18 23:28:41 +09:00

## Objective

Local C# metadata validator가 default value metadata가 있는 optional parameter를 omitted argument 후보로 다뤄 optional overload ambiguity를 generated C# emission 전에 진단하고, 단일 optional call은 `net481` build까지 통과하게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0043-0048-csharp-metadata-backed-interop-validation.md](0043-0048-csharp-metadata-backed-interop-validation.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `MetadataParameterSymbol` records whether a parameter is optional with default metadata
- metadata reader detects optional parameter default metadata from local `net481` DLLs
- overload validator treats omitted optional parameters as applicable arity
- ambiguous optional overloads report `TS2402` before generated C# emission
- smoke test proves a single imported optional call compiles through generated `net481` project
- checklist/traceability updates

Out:
- named argument overload ranking
- optional default value decoding into TypeSharp constants
- generic overload inference
- generated source rewriting for omitted optional arguments

## Acceptance Criteria

- [x] metadata parameter symbols expose an optional-with-default flag.
- [x] local metadata reader sets the optional flag only when default metadata exists.
- [x] optional omitted arguments participate in overload applicability.
- [x] ambiguous optional overloads report `TS2402`.
- [x] single optional call compiles through generated `net481` project.
- [x] existing tests still pass.
- [x] checklist and traceability are updated.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- optional metadata flag assertions pass.
- ambiguous optional overload call reports `TS2402`.
- single optional call compiles through generated `net481` project.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Added `MetadataParameterSymbol.IsOptional`.
- Detected optional-with-default metadata in local DLL parameters.
- Included omitted optional parameters in overload applicability.
- Added `TS2402` smoke coverage for ambiguous optional overloads.
- Added generated `net481` build smoke for an imported optional call.
- Updated checklist and traceability.

Remaining:
- Continue with named argument overload ranking and nullable metadata validation.

Blocked:
- None.
