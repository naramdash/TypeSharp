# Task: direct-generic-function-call-inference-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 20:13:52 +09:00
End Time: TBD

## Objective

Add bounded direct TypeSharp generic function call inference for simple TypeSharp-declared generic functions without changing generated `net48` C# lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:
- Track TypeSharp-declared function type parameter names in the type checker function table.
- Infer direct generic function type arguments from known direct call arguments when generic parameters appear directly in parameter types.
- Support explicit direct generic function calls such as `identity<string>("a")` when the generic callee is a TypeSharp-declared function.
- Substitute inferred or explicit simple generic parameter names into direct call return types.
- Report deterministic `TS2201` diagnostics for explicit generic arity mismatches and incompatible repeated type-parameter inference.
- Add focused positive/negative type-checker fixtures and docs notes.

Out:
- Imported C# generic call validation changes.
- Generic pipeline/composition inference.
- Constructed generic parameter inference such as `List<T>` or `T[]`.
- Generic constraints beyond the existing backend-compatible constraint checks.
- Optional/default/params TypeSharp parameter policy.
- Function-typed values, higher-order calls, currying, partial application, and TypeSharp function overload ranking.

## Acceptance Criteria

- [ ] Direct generic TypeSharp calls infer simple type parameters from arguments and return the substituted type.
- [ ] Explicit direct generic TypeSharp calls validate generic arity and use explicit arguments for direct argument/return checking.
- [ ] Repeated simple type parameters reject inconsistent argument types with `TS2201`.
- [ ] Non-generic direct calls, imported C# calls, type constructor calls, and generated C# call lowering remain unchanged.
- [ ] Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, tasks, rollup, and traceability stay aligned.

## Verification

Command:
Expected:
Result:

## Handoff

Done:
Remaining:
Blocked:
