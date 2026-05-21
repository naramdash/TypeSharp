# Task: params-parameter-declaration-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 22:57:53 +09:00
End Time: TBD

## Objective

Implement a bounded TypeSharp-owned `params` parameter declaration slice for direct TypeSharp-declared function calls and C# 7.3-compatible generated signatures, without adopting optional/default parameter policy, named-argument binding, overload ranking, or imported metadata behavior changes.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0365-roadmap-refresh-after-direct-composition-value-inference)
- [C# method parameters](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters)
- [TypeScript rest parameters](https://www.typescriptlang.org/docs/handbook/2/functions.html)

## Scope

In:
- Parse and preserve a final `params name: T[]` parameter declaration shape in the relevant TypeSharp parameter syntax.
- Accept and type-check `params` on direct TypeSharp-declared functions when the `params` parameter is final and array-typed.
- Report deterministic diagnostics when `params` is non-final, repeated, or not array-typed.
- Direct TypeSharp-declared calls accept either an exact array argument for the `params` parameter or expanded trailing element arguments of the array element type.
- Direct pipeline target arity and argument checks account for the `params` tail on known TypeSharp-declared function targets.
- C# backend lowers accepted final `params` parameters to C# 7.3-compatible `params T[]` generated signatures.
- Add parser, type-checker, backend, CLI smoke, docs, and fixture coverage for the bounded behavior.

Out:
- Optional/default parameter declarations and default argument emission.
- Named argument binding for TypeSharp-declared functions.
- TypeSharp function overload ranking.
- Imported C# `params` behavior changes; existing metadata-backed interop stays separate.
- `params` in function type syntax or higher-order delegate/value shapes.
- Constructor, delegate, union-case, class-member, or broad declaration-surface `params` behavior unless the existing implementation path already shares it safely with direct functions.
- Currying, partial application, and broader type-constructor unification.

## Acceptance Criteria

- [ ] Parser fixtures cover valid final `params name: T[]` syntax and invalid/rejected forms.
- [ ] Type-checker fixtures cover direct exact-array calls, expanded trailing element calls, pipeline calls, non-final params, repeated params, and non-array params.
- [ ] C# backend fixture snapshots show generated `params T[]` signatures and compatible call lowering.
- [ ] CLI smoke compiles a generated `net48` project using the accepted `params` function shape.
- [ ] Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs describe the implemented boundary and remaining backlog.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: build succeeds with zero errors.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"`
Expected: parser fixtures pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"`
Expected: type-checker diagnostics pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"`
Expected: backend snapshots pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
Expected: full runner passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0365 rechecked official source signals and selected TypeSharp-owned `params` parameter declarations as the next bounded implementation slice.

Remaining:
- Implement the parser/checker/backend/docs slice above.

Blocked:
- None.
