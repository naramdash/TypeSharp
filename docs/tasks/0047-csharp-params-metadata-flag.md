# Task: CSharp Params Metadata Flag

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:07:18 +09:00
End Time: 2026-05-18 23:11:11 +09:00

## Objective

Local C# metadata index에서 `params` parameter를 식별해 optional/named/params overload ranking 후속 작업의 입력으로 사용할 수 있게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0043-csharp-local-metadata-symbol-index.md](0043-csharp-local-metadata-symbol-index.md)
- [0046-csharp-exact-overload-ranking.md](0046-csharp-exact-overload-ranking.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `MetadataParameterSymbol` records whether a parameter has `System.ParamArrayAttribute`
- metadata reader detects `params` from local `net481` DLL custom attributes
- smoke test verifies `LegacyParams.Join(string, params string[])` marks the second parameter as `params`
- checklist/traceability updates

Out:
- overload ranking that expands `params`
- optional parameter default value decoding
- named argument ranking
- generated C# lowering changes

## Acceptance Criteria

- [x] metadata parameter symbols expose an `IsParams` flag.
- [x] local metadata reader sets `IsParams` for `params` parameters.
- [x] existing non-`params` parameters remain unmarked.
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
- existing tests and new `params` metadata flag assertion pass.
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
- Added `MetadataParameterSymbol.IsParams`.
- Detected `System.ParamArrayAttribute` on local DLL metadata parameters.
- Verified `LegacyParams.Join(string, params string[])` marks only the second parameter as `params`.
- Updated checklist and traceability.

Remaining:
- Continue with optional/named/params overload ranking and nullable metadata validation.

Blocked:
- None.
