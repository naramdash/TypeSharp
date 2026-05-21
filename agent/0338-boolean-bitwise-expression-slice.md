# Task 0338 Boolean Bitwise Expression Slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 17:29:25 +09:00
End Time: TBD

## Objective

Add expression-level boolean `|`, `&`, and `^` over known non-null `bool` operands with C# 7.3-compatible lowering.

## Scope

In:

- Reuse existing parser tokens and expression precedence for `|`, `&`, and `^`.
- Type-check binary operands that are both known non-null `bool`.
- Infer `bool` result type.
- Preserve same-enum and integral numeric bitwise behavior.
- Add focused type-checker/backend/CLI/docs evidence.

Out:

- Unary boolean complement.
- Shifts `<<`/`>>`.
- Compound assignment `|=`, `&=`, `^=`.
- User-defined operators.
- Flag-aware enum algebra.
- Imported enum flag reasoning.

## Verification Plan

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

## Notes

- Keep generated artifacts on `net48`.
- Keep emitted source C# 7.3-compatible.
- Keep shifts separate because `>>`/`<<` are already function-composition syntax.
