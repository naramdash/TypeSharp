# Task 0138: Collection Expression Array Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 13:27:55 +09:00
End Time: 2026-05-19 13:32:01 +09:00

## Objective

Implement the MVP collection expression path for simple homogeneous array literals so the parser-level collection syntax has generated `net48` lowering, diagnostics, and C# consumer evidence.

## Scope

In:
- C# source backend lowering for simple `CollectionExpression` nodes.
- Expected array type support for return/local annotations, including empty array literals.
- Type checker inference for homogeneous collection expressions and `TS2201` mismatch diagnostics for mixed known element types.
- Backend fixture and CLI build/C# consumer smoke.
- Checklist, feature map, lowering, feature-spec, traceability, and task queue updates.

Out:
- `List<T>` target inference.
- Dictionary literals.
- Spread/rest collection elements.
- Target-specific builders.
- C# 15 collection argument syntax.

## Acceptance Criteria

- [x] `["Ada", "Grace"]` lowers to C# 7.3-compatible array creation.
- [x] `[]` lowers when an expected array type is available from return/local annotation.
- [x] Mixed known element types produce a type checker diagnostic.
- [x] Backend fixture snapshots pin generated C#.
- [x] CLI build smoke produces a generated `net48` assembly and C# consumer compiles against it.
- [x] Docs and traceability distinguish implemented array lowering from Stable Backlog collection forms.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "collection expression"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture"
```

Result:
- PASS compiler test project build.
- PASS focused collection expression build/C# consumer smoke.
- PASS C# backend fixture snapshots.
- PASS type checker diagnostics fixtures.

## Handoff

Done:
- Added simple array literal lowering and expected-type handling.
- Added homogeneous element inference and mixed-element diagnostic coverage.
- Added backend fixture, CLI build smoke, and docs/task updates.

Remaining:
- None for MVP simple array lowering.
- `List<T>`, dictionary, spread, target-specific builder, and C# 15 collection argument support remain Stable Backlog/Preview Watch.

Blocked:
- None.
