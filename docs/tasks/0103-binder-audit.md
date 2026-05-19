# Task: Binder Audit

Status: Done
Queue: Q2
Start Time: 2026-05-19 05:10:20 +09:00
End Time: 2026-05-19 05:12:07 +09:00

## Objective

Audit the implemented binder scope and align the top-level checklist item with existing symbol collection, scoped name resolution, unresolved-name diagnostics, fixture, CLI, and LSP evidence.

## Source Of Truth

- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../regression-testing.md](../regression-testing.md)
- `src/TypeSharp.Compiler/Binding/TypeSharpBinder.cs`
- `src/TypeSharp.Compiler/Binding/BoundSymbol.cs`
- `src/TypeSharp.Compiler/Binding/BindingResult.cs`
- `tests/fixtures/diagnostics/binder`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- built-in type declarations
- namespace/import/type/function/value/literal/module symbol collection
- function parameter, local `let`, lambda, for-pattern, and match-pattern scoped bindings
- simple type annotation and value expression name resolution
- unresolved-name `TS2001` diagnostics
- duplicate symbol `TS2002` diagnostics, now implemented by [0126-binder-duplicate-symbol-diagnostics.md](0126-binder-duplicate-symbol-diagnostics.md)
- binder fixture, CLI, backend, and LSP smoke evidence

Out:
- full semantic model
- overload binding
- imported metadata member binding beyond current static-call allowance
- control/data flow analysis

## Acceptance Criteria

- [x] Existing implementation and tests prove binder symbol collection for current syntax scope.
- [x] Existing implementation and tests prove unresolved-name diagnostics.
- [x] Existing implementation and tests prove binder fixtures run as golden diagnostics.
- [x] Existing implementation and tests prove downstream compiler/CLI/LSP paths consume binder results.
- [x] Checklist and traceability distinguish completed current binder scope from semantic model and overload binding.

## Verification

Command:

```text
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- binder fixtures and existing suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Audited `TypeSharpBinder`, binding result, binder fixtures, compiler checker, backend fixture prechecks, and LSP binder symbol usage.
- Marked the top-level `binder` checklist item complete.
- Updated traceability with exact current binder scope and explicit out-of-scope boundaries.

Remaining:
- None.

Blocked:
- None.
