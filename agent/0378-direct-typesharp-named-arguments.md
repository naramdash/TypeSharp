# Task: direct-typesharp-named-arguments

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 02:28:58 +09:00
End Time: TBD

## Objective

Implement bounded named-argument binding for known TypeSharp-owned direct function calls while preserving generated `net48` and C# 7.3-compatible output.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md#task-0377-roadmap-refresh-after-optional-default-parameters)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

## Scope

In:
- Bind named arguments for known TypeSharp-declared direct `fun` calls.
- Include first-argument pipeline calls where the target is a known TypeSharp-declared function and non-piped arguments use named syntax.
- Reuse the existing parser `NamedArgument` syntax and existing function parameter names.
- Diagnose unknown names, duplicates, positional-after-named ordering, missing required parameters, argument type mismatches, and unsupported combinations before C# emission.
- Lower accepted TypeSharp-owned named calls to ordinary positional C# calls so generated source stays C# 7.3-compatible and simple.
- Add focused parser/checker/backend or CLI smoke evidence and update canonical docs.

Out:
- TypeSharp function overload ranking.
- Named binding for function-typed values, higher-order calls, lambdas, delegates, constructors, union cases, or ambient/`extern` signatures.
- Replacing existing imported C# metadata-backed named argument validation.
- Broad optional/default, `params`, or generic-function expansion beyond what can be handled deterministically in this bounded slice.

## Acceptance Criteria

- [ ] Known TypeSharp-owned direct calls can use valid named arguments and lower successfully.
- [ ] First-argument pipeline calls validate and lower supported named non-piped arguments.
- [ ] Invalid TypeSharp-owned named calls report deterministic diagnostics.
- [ ] Imported C# named arguments still use the existing metadata-backed interop path.
- [ ] Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, traceability, and task rollup are updated where behavior changes.
- [ ] Verification includes focused runner filters, docs build, and `git diff --check`.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test runner builds after named-argument binding changes.
Result: TBD

Command: focused `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "<named argument filters>"`
Expected: focused named-argument parser/checker/backend/build cases pass.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after ledger and reference updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0377 completed roadmap refresh after optional/default parameters and selected this implementation slice.

Remaining:
- Implement TypeSharp-owned named argument binding, fixtures/smokes, docs, verification, and commit/push.

Blocked:
- None.
