# Task 0340 Bitwise Compound Assignment Slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 17:55:52 +09:00
End Time: TBD

## Objective

Add parser and C# 7.3-compatible lowering support for bitwise compound assignment `|=`, `&=`, and `^=` over the already supported assignment surface.

## Source Of Truth

- [agent.md](../agent.md)
- [tasks.md](tasks.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)

## Scope

In:

- Lex and parse `|=`, `&=`, and `^=` as assignment operators.
- Preserve existing `=`, `+=`, and `-=` assignment behavior.
- Lower accepted bitwise compound assignments to ordinary C# 7.3-compatible assignment operators.
- Add focused parser, backend, CLI, and docs evidence.

Out:

- Shift assignment `<<=` and `>>=`.
- Shift expressions.
- User-defined operators.
- New assignment target analysis beyond the current assignment surface.
- Flag-aware enum algebra or imported enum flag reasoning.

## Acceptance Criteria

- [ ] Parser fixture covers `|=`, `&=`, and `^=` as assignment expressions.
- [ ] Backend fixture lowers all three operators to generated C#.
- [ ] CLI build smoke proves a generated `net48` assembly can use the lowered operators.
- [ ] Docs and ledgers state the implemented boundary and remaining exclusions.

## Verification

Command:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles bitwise compound assignment API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Expected:

- All commands pass.

Result:

- TBD

## Handoff

Done:

- TBD

Remaining:

- Implement and verify the slice.

Blocked:

- None.
