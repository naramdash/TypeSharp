# Task 0153: Capability Call Marker Propagation

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 16:29:18 +09:00
End Time: 2026-05-19 16:30:40 +09:00

## Objective

Extend capability effect propagation beyond `dynamic` by reporting a type-checker diagnostic when non-capability functions call `reflect`, `interop`, or `unsafe` functions.

## Scope

In:
- Track `reflect`, `interop`, and `unsafe` function capability metadata in the type-checker function scope.
- Report a stable diagnostic for direct calls to capability-marked functions from functions without the matching marker.
- Report the same diagnostic for pipeline targets.
- Preserve `TS2207` for `dynamic fun` calls while using the new diagnostic for `reflect`, `interop`, and `unsafe` calls.
- Add type-checker fixture coverage and a CLI JSON smoke.
- Update diagnostics, interop, feature map, checklist, traceability, feature-spec, and task index docs.
- Commit and push when this task is completed.

Out:
- Cross-file/cross-assembly capability summaries beyond current source function scope.
- Capability subtyping or effect polymorphism.
- Warning-only compatibility modes or per-code suppression.
- Native pointer, reflection, COM, and P/Invoke runtime semantics beyond call-boundary diagnostics.

## Acceptance Criteria

- [x] `DiagnosticDescriptors` includes a stable `TS2208` descriptor for capability call marker enforcement.
- [x] The type checker records `reflect`, `interop`, and `unsafe` capability metadata for declared functions.
- [x] The type checker reports `TS2208` for direct calls to `reflect`, `interop`, or `unsafe` functions when the caller lacks the same marker.
- [x] The type checker reports `TS2208` when a capability-marked function is used as a pipeline target without the matching caller marker.
- [x] Calls from inside functions with the matching marker are allowed.
- [x] Existing `dynamic fun` call diagnostics still use `TS2207`.
- [x] Golden diagnostics fixture coverage verifies direct, pipeline, and allowed cases.
- [x] CLI JSON smoke verifies `TS2208` is emitted through `typesharp check`.
- [x] Docs explain that capability-marked function calls require a matching containing marker.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "capability call marker"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic call capability"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:29:18 +09:00: Started task after completing `TS2207` dynamic call propagation and selecting the remaining capability-marker call propagation slice from `docs/goal.md`.
- 2026-05-19 16:30:40 +09:00: Added `TS2208`, extended function capability metadata for `reflect`, `interop`, and `unsafe`, enforced direct and pipeline call boundaries, and updated diagnostics/interop docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "capability call marker"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic call capability"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` from `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
