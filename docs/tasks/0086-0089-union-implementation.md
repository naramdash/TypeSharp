# Task: Union Implementation

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:14:27 +09:00
End Time: 2026-05-19 04:01:14 +09:00
Rollup Time: 2026-05-19 04:04:51 +09:00

## Objective

Implement the first practical union feature set: nominal closed union generated APIs, type-level union public boundary diagnostics, nominal union match lowering, and local type-level union narrowing, all while preserving `net48` generated artifact compatibility.

## Source Of Truth

- [../goal.md](../goal.md)
- [../diagnostics.md](../diagnostics.md)
- [../grammar/types.md](../grammar/types.md)
- [../grammar/patterns.md](../grammar/patterns.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../standard-library.md](../standard-library.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `src/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- `src/TypeSharp.Runtime/TypeSharpUnion.cs`
- `src/TypeSharp.Runtime/TypeSharpPattern.cs`
- `tests/TypeSharp.Compiler.Tests`

## Compressed Packets

- `0086-nominal-union-backend-smoke`: nominal `union` declarations lower to C# abstract base types with sealed nested case classes, static payload-free case properties, payload case factories, `ITypeSharpUnionCase` metadata, equality, hash behavior, backend fixture `0017`, and C# `net48` consumer smoke.
- `0087-type-level-union-public-boundary-diagnostic`: `TS2204` blocks type-level union aliases from exported/public .NET metadata boundaries while allowing non-public local use; CLI `check` JSON and CLI `build` no-emission smokes cover the diagnostic.
- `0088-nominal-union-match-lowering`: `TS2203` reports missing nominal union cases; exhaustive nominal union `match` lowers to `TypeSharpPattern` case checks and payload extraction; backend fixture `0018`, generated `net48` assembly, and C# `net48` consumer smoke cover the path.
- `0089-type-level-union-narrowing`: local `type A = B | C` aliases are tracked for type-pattern `match` narrowing; missing union members report `TS2203`; generated C# erases local alias parameters to `object` and lowers arms to C# 7.3 `is Type variable` checks; backend fixture `0019` and CLI/C# consumer smokes cover the path.

## Acceptance Criteria

- [x] Nominal closed union type checklist item is complete.
- [x] Type-level union alias checklist item is complete.
- [x] Pattern matching checklist item is complete for nominal union match expressions.
- [x] Union narrowing checklist item is complete for supported local type-level union aliases.
- [x] `TS2203` and `TS2204` descriptors are allocated and covered by tests.
- [x] Generated union APIs and match/narrowing lowering compile into `net48` assemblies.
- [x] C# `net48` consumers can call public wrapper APIs without seeing compile-time-only type-level unions.
- [x] `docs/checklist.md`, `docs/traceability.md`, and `docs/tasks/README.md` point at this rollup.

## Verification

Commands:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Results:
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Nominal union generated API and runtime metadata smoke coverage.
- Type-level union public boundary diagnostic and no-emission build gate.
- Nominal union match lowering and exhaustiveness diagnostics.
- Local type-level union narrowing through type-pattern `match` arms.
- Backend snapshots and CLI generated `net48` assembly/C# consumer smokes for all completed union paths.

Remaining:
- Generic nominal union lowering.
- Multi-payload union case deconstruction beyond object-array payload storage.
- Nested, record, tuple, list, and guard pattern decision-tree lowering.
- Literal type syntax and literal-union narrowing.
- Structural shape union narrowing and `is` expression narrowing.
- Overload candidate reasoning with type-level unions.

Blocked:
- None.
