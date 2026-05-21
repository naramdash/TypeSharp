# Task 0336 Integral Numeric Bitwise Expression Slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 17:03:40 +09:00
End Time: TBD

## Objective

Add expression-level integral numeric `|`, `&`, `^`, and unary `~` over known non-null primitive integral operands with C# 7.3-compatible lowering.

## Scope

In:

- Reuse existing parser tokens and expression precedence for `|`, `&`, `^`, and unary `~`.
- Type-check known non-null primitive integral operands for supported bitwise expressions.
- Infer deterministic result types and diagnostics.
- Preserve same-enum value bitwise behavior.
- Add focused parser/type-checker/backend/CLI/docs evidence.

Out:

- Shifts `<<`/`>>`.
- Compound assignment `|=`, `&=`, `^=`.
- Boolean bitwise expressions.
- Flag-aware match algebra.
- Imported enum flag reasoning.
- Arbitrary/general computed enum member declarations beyond the implemented initializer-local composite member slice.

## Verification Plan

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

## Notes

- Keep generated artifacts on `net48`.
- Keep emitted source C# 7.3-compatible.
- Keep docs concise: numeric bitwise active; shifts/mixed broad operators remain future work.
