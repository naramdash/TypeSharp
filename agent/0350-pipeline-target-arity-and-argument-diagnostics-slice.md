# Task: pipeline-target-arity-and-argument-diagnostics-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 19:39:04 +09:00
End Time: TBD

## Objective

Add bounded diagnostics for direct pipeline targets whose known TypeSharp-declared arity or non-piped call arguments cannot lower to a valid first-argument call.

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

- Check direct `value |> f` targets with known TypeSharp-declared parameter lists for zero-parameter or multi-parameter arity that cannot be satisfied by the pipeline input alone.
- Check direct `value |> f(args...)` targets with known TypeSharp-declared parameter lists for too few or too many supplied arguments after the pipeline input is prepended.
- Validate known non-piped call arguments against parameters after the first pipeline input parameter.
- Reuse existing `TS2201` type-checking diagnostic style.
- Keep existing pipeline parsing and C# 7.3-compatible lowering.

Out:

- Imported C# function, method, or extension pipeline target validation.
- Generic function inference improvements.
- Optional/default/params TypeSharp function parameter policy.
- Higher-order pipeline targets.
- Currying, partial application, and pipeline overload ranking.
- General direct TypeSharp function call argument checking outside pipeline targets.

## Acceptance Criteria

- [ ] `value |> zeroParameterFunction` reports deterministic `TS2201`.
- [ ] `value |> twoParameterFunction` without the remaining argument reports deterministic `TS2201`.
- [ ] `value |> f(args...)` reports deterministic `TS2201` for too few or too many supplied arguments.
- [ ] `value |> f(args...)` reports deterministic `TS2201` for incompatible known non-piped call arguments.
- [ ] Existing valid pipeline parser/backend fixtures remain stable.
- [ ] Docs state the bounded arity and argument compatibility boundary.
- [ ] Task ledgers close cleanly after verification.

## Verification

Command:
Expected:
Result:

## Handoff

Done:
Remaining:
Blocked:
