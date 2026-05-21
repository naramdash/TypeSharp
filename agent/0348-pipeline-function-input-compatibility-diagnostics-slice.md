# Task: pipeline-function-input-compatibility-diagnostics-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 19:21:41 +09:00
End Time: TBD

## Objective

Add bounded diagnostics for direct pipeline targets whose first known TypeSharp-declared parameter cannot accept the pipeline input.

## Source Of Truth

- [tasks.md](tasks.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [F# functions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/functions/)

## Scope

In:

- Check `value |> f` when `f` is a direct TypeSharp-declared function with a known first parameter type.
- Check `value |> f(args...)` when `f` is a direct TypeSharp-declared call target with a known first parameter type.
- Reuse existing assignment compatibility and `TS2201` style.
- Keep existing pipeline parsing and C# 7.3-compatible lowering.
- Add focused negative fixture coverage and docs updates.

Out:

- Higher-order pipeline targets.
- Imported C# functions and extension methods.
- Generic function inference improvements.
- Currying, partial application, and delegate shape redesign.
- Pipeline overload ranking.
- Numeric shifts, shift assignment, and user-defined operators.

## Acceptance Criteria

- [ ] Incompatible direct pipeline input reports a deterministic `TS2201` diagnostic.
- [ ] Compatible direct pipeline targets stay accepted.
- [ ] Direct call targets validate the piped input against the first declared parameter.
- [ ] Existing parser/backend pipeline fixtures remain stable.
- [ ] Docs state the bounded pipeline compatibility boundary.
- [ ] Task ledgers close cleanly after verification.

## Verification

Command:
Expected:
Result:

## Handoff

Done:
Remaining:
Blocked:
