# Task Rollup: Runtime Helper Surface

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:35:16 +09:00
End Time: 2026-05-19 01:50:29 +09:00

## Compressed Tasks

- `0070-nominal-union-runtime-helper.md`
- `0071-pattern-runtime-helper.md`
- `0072-equality-hash-runtime-helper.md`
- `0073-async-runtime-helper.md`

## Objective

`TypeSharp.Runtime`에 generated union, pattern matching, record/union equality, async lowering이 공유할 최소 helper surface를 추가한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Runtime`
- `tests/TypeSharp.Compiler.Tests`

## Scope

Completed:
- `ITypeSharpUnionCase` generated-case metadata interface.
- `TypeSharpUnion` tag, case name, payload, equality, and hash helpers.
- `TypeSharpPattern` union case and payload checks for generated pattern matching lowering.
- `TypeSharpEquality` value equality, sequence equality, and hash composition helpers.
- `TypeSharpAsync` completed/result/faulted `Task` creation helpers.
- `net48`, C# 7.3-compatible, package-free runtime implementation.
- smoke tests for union helper, pattern helper, equality/hash helper, and async helper.
- checklist, traceability, architecture, standard library, runtime README, and task queue updates.

Out:
- parser/type checker support for `union`, `match`, and `async` declarations.
- generated union, pattern, record, and async lowering in the C# backend.
- exhaustiveness diagnostics.
- public ABI snapshot tests.
- performance benchmarking.

## Verification

Commands run across the compressed packet set:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Result:
- PASS `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`.
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`.
- PASS `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`.
- PASS `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json`.
- PASS `git diff --check`.

## Handoff

Done:
- `TypeSharp.Runtime` now has helper classes for union metadata, pattern matching predicates, equality/hash composition, and async task creation.
- `tests/TypeSharp.Compiler.Tests` links runtime helper sources and covers the helper behaviors.
- Runtime helper checklist items are complete except public ABI versioning policy.

Remaining:
- Public ABI versioning policy.
- Generated lowering for union, pattern matching, record equality, and async.
- Exhaustiveness diagnostics.
- Public ABI snapshot tests.

Blocked:
- None.
