# Task: CSharp Exact Overload Ranking

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:03:39 +09:00
End Time: 2026-05-18 23:06:16 +09:00

## Objective

Local C# metadata index를 사용해 imported static method overload set에서 TypeSharp literal/primitive argument type과 정확히 맞는 단일 후보를 선택하고, 여전히 좁혀지지 않는 overload만 `TS2402`로 진단하는 최소 nominal overload ranking을 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0045-csharp-ambiguous-overload-diagnostic.md](0045-csharp-ambiguous-overload-diagnostic.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- exact argument type inference for string, bool, numeric, and byref-wrapped arguments in the interop validator
- single exact-match overload candidate selection before reporting `TS2402`
- smoke test proving `Pick(string)` is selected over `Pick(object)` for a string literal
- ambiguity smoke remains for unranked overload shapes
- checklist/traceability updates

Out:
- numeric conversion ranking
- null-literal specificity ranking
- generic overload inference
- optional/named/params overload ranking
- selected symbol propagation into backend lowering

## Acceptance Criteria

- [x] exact string literal overload candidate is not reported as ambiguous.
- [x] generated `net481` project compiles for the exact overload smoke.
- [x] ambiguous overload diagnostic remains for an unranked same-arity overload call.
- [x] existing interop diagnostics still pass.
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
- existing tests and new exact overload ranking smoke tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 23:06:16 +09:00.

## Handoff

Done:
- Added literal/primitive argument type inference to the interop validator.
- Added exact-match overload candidate narrowing before `TS2402`.
- Added a generated `net481` build smoke proving `LegacyOverloads.Pick("value")` is accepted through exact string overload matching.
- Kept ambiguous overload diagnostics for currently unranked same-arity shapes.
- Updated checklist and traceability.

Remaining:
- Continue with richer nominal ranking, nullable metadata validation, and optional/named/params ranking.

Blocked:
- None.
