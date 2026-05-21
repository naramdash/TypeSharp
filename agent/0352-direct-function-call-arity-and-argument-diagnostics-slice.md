# Task: direct-function-call-arity-and-argument-diagnostics-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 19:55:41 +09:00
End Time: TBD

## Objective

Add bounded diagnostics for direct TypeSharp-declared function calls whose known arity or argument types cannot lower to valid generated C#.

## Source Of Truth

- [tasks.md](tasks.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:

- Check direct `f(args...)` calls when `f` is a known TypeSharp-declared function with a known parameter list.
- Report deterministic `TS2201` for too few or too many arguments.
- Validate known argument types against corresponding parameters.
- Reuse existing assignment compatibility, nullability, and structural mismatch helpers where appropriate.
- Preserve existing imported C# call validation and generated C# lowering.

Out:

- Imported C# method/constructor/extension overload validation.
- Generic TypeSharp function argument inference improvements.
- Optional/default/params TypeSharp function parameter policy.
- Function-typed values, higher-order calls, currying, and partial application.
- Overload ranking for TypeSharp functions.
- Type constructor calls and record/class constructor policy.

## Acceptance Criteria

- [ ] Direct calls with too few arguments report `TS2201`.
- [ ] Direct calls with too many arguments report `TS2201`.
- [ ] Direct calls with incompatible known argument types report `TS2201`.
- [ ] Compatible direct function calls remain accepted.
- [ ] Imported C# call tests remain stable.
- [ ] Docs state the bounded direct TypeSharp call compatibility boundary.
- [ ] Task ledgers close cleanly after verification.

## Verification

Command:
Expected:
Result:

## Handoff

Done:
Remaining:
Blocked:
