# Task Rollup: Basic C# Backend Semantics

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:03:41 +09:00
End Time: 2026-05-19 02:10:33 +09:00

## Compressed Tasks

- `0075-compile-time-literal-lowering.md`
- `0076-basic-semantics-smoke.md`

## Objective

Generated C# `net48` backend의 기본 의미론을 넓혀 `literal` compile-time constant, primitive literals, local `let`, and function declaration/call 경로를 실제 build smoke로 고정한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../feature-map.md](../feature-map.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

Completed:
- top-level `literal` declaration emission before functions in generated C#.
- string, bool, integer, floating, and decimal literal constant type inference for backend emission.
- public/exported `literal` declarations lowered to C#-visible `const` fields for C# 7.3-compatible initializer literals.
- simple type checker inference for unannotated top-level literal declarations.
- backend golden fixture `0007-literal-constants`.
- CLI build and C# `net48` consumer smoke for generated public literal fields.
- backend golden fixture `0008-basic-semantics`.
- CLI build smoke for primitive string/int/bool literals, local `let`, generated function declarations, and generated function calls.
- checklist completion for `기본 타입과 literal`, `local binding`, `compile-time constant literal`, and `function declaration/call`.
- traceability entries for compile-time literal lowering and basic semantics generated build.

Out:
- enum literal constants.
- constant folding across arbitrary expressions.
- invalid `literal` initializer diagnostics.
- attribute argument lowering.
- numeric operator and binary expression lowering.
- generic function implementation.
- class/interface/record lowering.

## Verification

Commands run across the compressed packet set:

```text
dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Result:
- PASS `dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj`.
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`, including backend fixture snapshots, `CLI build compiles literal constants`, and `CLI build compiles basic semantics`.
- PASS `git diff --check`.

## Handoff

Done:
- Basic generated C# backend semantics now cover public/internal constants, primitive literal returns, local `let`, and calls between generated functions.
- C# `net48` consumer compatibility for public TypeSharp literal constants is covered.

Remaining:
- Implement operator/binary expression lowering.
- Implement generic functions.
- Implement class/interface/record lowering.
- Add diagnostics for invalid literal initializers and unsupported constant expressions.

Blocked:
- None.
