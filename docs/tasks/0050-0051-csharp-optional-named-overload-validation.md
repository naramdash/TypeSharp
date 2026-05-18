# Task Rollup: CSharp Optional And Named Overload Validation

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:26:04 +09:00
End Time: 2026-05-18 23:33:23 +09:00
Rollup Time: 2026-05-18 23:34:23 +09:00

## Objective

Local C# metadata validator가 optional parameter omission과 named argument parameter matching을 overload 후보 선택에 반영하고, generated C# backend가 TypeSharp named arguments를 C# `name: value` syntax로 낮추게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../checklist.md](../checklist.md)
- [0043-0048-csharp-metadata-backed-interop-validation.md](0043-0048-csharp-metadata-backed-interop-validation.md)
- `src/TypeSharp.Compiler/Backend`
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Completed Work

- `0050-csharp-optional-parameter-overload-validation`: local metadata reader preserves optional-with-default parameter metadata as `MetadataParameterSymbol.IsOptional`; validator treats omitted optional parameters as applicable and reports `TS2402` for ambiguous optional overloads.
- `0051-csharp-named-argument-overload-validation`: backend emits `NamedArgument` as C# `name: value`; validator matches named arguments against metadata parameter names and uses inner expressions for exact type inference and byref validation.

## Acceptance Criteria

- [x] metadata parameter symbols expose optional-with-default metadata.
- [x] optional omitted arguments participate in overload applicability.
- [x] ambiguous optional overloads report `TS2402`.
- [x] generated C# preserves TypeSharp named arguments as C# named arguments.
- [x] named arguments filter overload candidates by metadata parameter name.
- [x] single optional and named optional calls compile through generated `net48` projects.
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

Result:
- Passed on 2026-05-18 23:33:23 +09:00 after `0051`.

## Handoff

Done:
- Optional and named C# interop arguments now participate in metadata-backed overload validation.
- Generated C# source preserves named argument syntax for C# compiler consumption.
- Related task packets `0050` through `0051` are compressed into this rollup.

Remaining:
- Continue with duplicate/unknown named argument diagnostics, nullable metadata validation, delegate/event interop, and public ABI fixtures.

Blocked:
- None.
