# Task 0152: Dynamic Call Capability Propagation

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 16:24:35 +09:00
End Time: 2026-05-19 16:25:59 +09:00

## Objective

Propagate the first implemented capability effect by reporting a type-checker diagnostic when a non-`dynamic` function calls a `dynamic fun` declaration.

## Scope

In:
- Track whether declared functions require the `dynamic` capability in the type-checker function scope.
- Report a stable diagnostic for direct calls from non-`dynamic` functions to `dynamic fun` declarations.
- Report the same diagnostic when a `dynamic fun` is used as a pipeline target.
- Allow calls from inside a `dynamic fun` boundary.
- Add type-checker fixture coverage and a CLI JSON smoke.
- Update diagnostics, interop, feature map, checklist, traceability, and task index docs.
- Commit and push when this task is completed.

Out:
- Reflection, COM, unsafe, native pointer, and P/Invoke effect propagation.
- Cross-file/cross-assembly capability summaries beyond current source function scope.
- Effect polymorphism, warning-only modes, or suppression controls.
- Generated C# `dynamic` runtime binder behavior beyond current diagnostics.

## Acceptance Criteria

- [x] `DiagnosticDescriptors` includes a stable `TS2207` descriptor for dynamic call propagation.
- [x] The type checker records `dynamic fun` capability metadata for declared functions.
- [x] The type checker reports `TS2207` for direct calls to `dynamic fun` from non-`dynamic` functions.
- [x] The type checker reports `TS2207` when a `dynamic fun` is used as a pipeline target from non-`dynamic` functions.
- [x] Calls from inside `dynamic fun` are allowed.
- [x] Golden diagnostics fixture coverage verifies direct, pipeline, and allowed cases.
- [x] CLI JSON smoke verifies `TS2207` is emitted through `typesharp check`.
- [x] Docs explain that calling `dynamic fun` requires a containing `dynamic fun` boundary.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic call capability"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:24:35 +09:00: Started task after implementing `TS2206` dynamic annotation boundary checks and selecting the next capability propagation slice from `docs/goal.md`.
- 2026-05-19 16:25:59 +09:00: Added `TS2207`, tracked `dynamic fun` capability metadata in the type-checker scope, enforced direct and pipeline dynamic call boundaries, and updated diagnostics/interop docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic call capability"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` from `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
