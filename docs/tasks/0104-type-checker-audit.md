# Task: Type Checker Audit

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 05:12:53 +09:00
End Time: 2026-05-19 05:14:41 +09:00

## Objective

Audit the implemented type checker scope and align the top-level checklist item with existing mismatch, nullability, union, public boundary, structural shape, async `Task`, fixture, CLI, build-stop, and LSP evidence.

## Source Of Truth

- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../regression-testing.md](../regression-testing.md)
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeCheckResult.cs`
- `tests/fixtures/diagnostics/type-checker`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- explicit annotation mismatch diagnostics
- literal/reference/call/block/if/match/member/await expression type checks for current implemented syntax
- nullability contract diagnostics
- nominal union exhaustiveness diagnostics
- type-level union public boundary diagnostics and local narrowing
- basic structural shape assignment/member checks
- async `Task<T>` result checking
- type checker fixture, CLI JSON, build no-emission, backend precheck, and LSP diagnostic evidence

Out:
- full inference engine
- C# overload resolution
- generalized generic constraint checking
- data-flow definite assignment analysis
- full structural object type operators and public metadata adapters
- optimizer or performance tuning beyond the existing smoke

## Acceptance Criteria

- [x] Existing implementation and tests prove current type mismatch and nullability diagnostics.
- [x] Existing implementation and tests prove union exhaustiveness, type-level boundary, and narrowing checks.
- [x] Existing implementation and tests prove current structural shape and async `Task` checks.
- [x] Existing implementation and tests prove type checker fixtures, CLI JSON, build-stop, backend precheck, and LSP diagnostic paths.
- [x] Checklist and traceability distinguish completed current type checker scope from inference engine and overload resolution.

## Verification

Command:

```text
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- type checker fixtures and existing suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Audited `TypeSharpTypeChecker`, type checker fixtures, CLI JSON diagnostics, build no-emission gates, backend fixture prechecks, and LSP diagnostic usage.
- Marked the top-level `type checker` checklist item complete.
- Updated traceability with exact current type checker scope and explicit out-of-scope boundaries.

Remaining:
- None.

Blocked:
- None.
