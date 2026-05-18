# Task: Nominal Union Runtime Helper

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:35:16 +09:00
End Time: 2026-05-19 01:39:44 +09:00

## Objective

`TypeSharp.Runtime`에 generated nominal closed union case class가 사용할 tag/value helper를 추가해 F#식 nominal union lowering의 런타임 기반을 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Runtime`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TypeSharp.Runtime.ITypeSharpUnionCase`
- `TypeSharp.Runtime.TypeSharpUnion` tag, case name, payload, equality, and hash helpers
- `net48` and C# 7.3-compatible implementation
- smoke coverage for generated-case-like runtime behavior
- checklist, traceability, architecture, standard library, and task queue updates

Out:
- parser/type checker support for `union` declarations
- generated union lowering in the C# backend
- pattern matching helper
- exhaustiveness diagnostics
- public ABI snapshot tests

## Acceptance Criteria

- [x] `TypeSharp.Runtime` exposes a generated-case-friendly union case metadata interface.
- [x] `TypeSharp.Runtime` exposes helper methods for tag checks, case name, payload access, payload equality, and hash composition.
- [x] helper implementation remains `net48`/C# 7.3-compatible and package-free.
- [x] smoke tests cover payload and payload-free union cases.
- [x] `nominal union helper` checklist item is marked complete while parser/lowering/pattern matching items remain open.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
git diff --check
```

Expected:
- runtime project builds.
- test project builds.
- `runtime union helper exposes case metadata` passes.
- support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- PASS `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- PASS `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- PASS `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json`
- PASS `git diff --check`

## Handoff

Done:
- Added `TypeSharp.Runtime.ITypeSharpUnionCase`.
- Added `TypeSharp.Runtime.TypeSharpUnion` tag, case name, payload, equality, and hash helpers.
- Added generated-case-like smoke coverage for payload and payload-free union cases.
- Linked runtime helper sources into the smoke test project.
- Updated checklist, traceability, architecture, standard library, runtime README, and task queue docs.

Remaining:
- Parser/type checker support for `union` declarations.
- Generated union lowering in the C# backend.
- Pattern matching helper.
- Exhaustiveness diagnostics.
- Public ABI snapshot tests.

Blocked:
- None.
