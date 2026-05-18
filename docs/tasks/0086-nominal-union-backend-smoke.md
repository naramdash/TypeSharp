# Task: Nominal Union Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:14:27 +09:00
End Time: 2026-05-19 03:24:38 +09:00

## Objective

`union` declarations should lower to a closed nominal C# class hierarchy that a `net48` C# consumer can reference as ordinary CLR metadata.

## Source Of Truth

- [../goal.md](../goal.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../standard-library.md](../standard-library.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Runtime/TypeSharpUnion.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- top-level nominal `union` C# source emission
- payload-free and single-payload case factories
- generated case metadata through `ITypeSharpUnionCase`
- generated value equality and hash behavior for cases
- backend golden fixture and CLI build/C# `net48` consumer smoke

Out:
- `match` lowering and exhaustiveness diagnostics
- type-level union alias public boundary diagnostics
- generic union case constructor inference in expression lowering
- multi-payload pattern payload extraction

## Acceptance Criteria

- [x] C# backend snapshot shows an abstract union base type with sealed nested case classes.
- [x] Payload-free cases expose a stable singleton/static property.
- [x] Payload cases expose a C# 7.3-compatible factory and immutable payload property.
- [x] Generated case classes implement `TypeSharp.Runtime.ITypeSharpUnionCase`.
- [x] `typesharp build` emits a `net48` assembly that a C# `net48` project can consume with `TypeSharp.Runtime`.
- [x] `docs/checklist.md` and `docs/traceability.md` record the nominal union progress.

## Verification

Command:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- Runtime still builds for `net48`.
- Backend snapshot includes the nominal union hierarchy.
- CLI build/C# consumer smoke compiles generated union APIs and calls runtime case metadata helpers.

Result:
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added union case symbols to binder scope so payload-free cases and case factories can be referenced by TypeSharp functions.
- Added C# source backend emission for nominal union base types, static case property/factory members, nested sealed case classes, `ITypeSharpUnionCase` metadata, equality, and hash behavior.
- Added `tests/fixtures/backend/csharp/positive/0017-nominal-union-api`.
- Added `CLI build compiles nominal union API` smoke with generated `net48` assembly and C# `net48` consumer references to `TypeSharp.Runtime`.
- Marked `nominal closed union type` complete in [../checklist.md](../checklist.md) and added traceability evidence.

Remaining:
- Generic union case constructor inference, multi-payload pattern extraction, `match` lowering, and exhaustiveness diagnostics remain separate follow-up tasks.

Blocked:
- None.
