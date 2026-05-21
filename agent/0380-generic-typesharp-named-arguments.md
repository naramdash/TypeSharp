# Task: generic-typesharp-named-arguments

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 12:10:00 +09:00
End Time: TBD

## Objective

Extend TypeSharp-owned named argument binding to known generic TypeSharp direct function calls and first-argument pipeline targets while preserving generated `net48` and C# 7.3-compatible output.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md#task-0379-roadmap-refresh-after-direct-named-arguments)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

## Scope

In:
- Bind named arguments for known TypeSharp-declared generic direct `fun` calls when the call fits the existing generic inference/substitution slice.
- Include first-argument pipeline calls where the target is a known TypeSharp-declared generic function and non-piped arguments use named syntax.
- Reuse the 0378 parameter-name binding and positional C# lowering path.
- Reuse existing direct generic inference for simple type-parameter positions and bounded constructed shapes such as arrays and matching single-argument generic wrappers.
- Diagnose unknown names, duplicates, positional-after-named ordering, missing required parameters, argument type mismatches, inconsistent generic inference, unsupported `params` combinations, and unsupported generic shapes before C# emission.
- Add focused checker/backend or CLI smoke evidence and update canonical docs.

Out:
- TypeSharp function overload ranking.
- Named binding for TypeSharp `params` generic functions.
- Optional/default parameters on generic functions.
- Function-typed values, higher-order calls, lambdas, delegates, constructors, union cases, or ambient/`extern` signatures.
- Replacing existing imported C# metadata-backed named argument validation.
- Broad generic constraint or higher-kinded/type-constructor unification beyond existing bounded inference shapes.

## Acceptance Criteria

- [ ] Known TypeSharp-owned generic direct calls can use valid named arguments and lower successfully.
- [ ] Generic first-argument pipeline calls validate and lower supported named non-piped arguments.
- [ ] Explicit generic type arguments plus named arguments are validated for the supported shapes.
- [ ] Inferred generic type arguments plus named arguments are validated for the supported shapes.
- [ ] Invalid generic named calls report deterministic diagnostics.
- [ ] Imported C# named arguments still use the existing metadata-backed interop path.
- [ ] Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, traceability, and task rollup are updated where behavior changes.
- [ ] Verification includes focused runner filters, docs build, and `git diff --check`.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test runner builds after generic named-argument binding changes.
Result: TBD

Command: focused `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "<generic named argument filters>"`
Expected: focused generic named-argument checker/backend/build cases pass.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after ledger and reference updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0379 completed roadmap refresh after direct TypeSharp named argument binding and selected this bounded follow-up.

Remaining:
- Implement generic TypeSharp-owned named argument binding, fixtures/smokes, docs, verification, and commit/push.

Blocked:
- None.
