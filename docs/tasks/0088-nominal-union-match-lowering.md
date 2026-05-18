# Task: Nominal Union Match Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:37:22 +09:00
End Time: 2026-05-19 03:48:41 +09:00

## Objective

Nominal union `match` expressions should lower to C# 7.3-compatible runtime case checks, and missing nominal union cases should be reported before emission.

## Source Of Truth

- [../goal.md](../goal.md)
- [../diagnostics.md](../diagnostics.md)
- [../grammar/patterns.md](../grammar/patterns.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `src/TypeSharp.Runtime/TypeSharpPattern.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- payload-free nominal union case match lowering
- single-payload nominal union case match lowering with payload variable binding
- generated `TypeSharpPattern` checks and payload extraction
- non-exhaustive nominal union match diagnostic `TS2203`
- backend fixture and CLI build/C# `net48` consumer smoke

Out:
- nested patterns
- record/shape/type pattern narrowing
- multi-payload case deconstruction beyond object-array payload
- generic union match lowering
- type-level union narrowing

## Acceptance Criteria

- [x] `DiagnosticDescriptors.All` includes `TS2203`.
- [x] C# backend snapshot lowers exhaustive nominal union match to runtime case checks.
- [x] Payload case match arms bind payload variables in generated C#.
- [x] Non-exhaustive nominal union matches report `TS2203`.
- [x] `typesharp build` emits a `net48` assembly for exhaustive match code and C# `net48` consumer builds against it.
- [x] `typesharp build` stops before emission on non-exhaustive match diagnostics.
- [x] `docs/checklist.md` and `docs/traceability.md` record pattern matching progress.

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
- Exhaustive nominal union match emits valid C# and builds into a generated `net48` assembly.
- Missing nominal union cases are reported as `TS2203` and stop generated emission.

Result:
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `TS2203` descriptor and descriptor registry coverage for non-exhaustive nominal union matches.
- Extended the type checker to track nominal union case sets and report missing cases before emission.
- Lowered exhaustive nominal union matches to C# 7.3-compatible `TypeSharpPattern` case checks and payload extraction.
- Added backend and diagnostic fixtures plus CLI build smokes for generated `net48` assembly, C# `net48` consumer compatibility, and diagnostic emission stop.
- Refreshed diagnostics, checklist, traceability, and task index documentation.

Remaining:
- Type-level union narrowing remains separate.
- Nested patterns, record patterns, multi-payload deconstruction, and generic union match lowering remain separate follow-up work.

Blocked:
- None.
