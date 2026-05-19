# Task 0151: Dynamic Capability Boundary Diagnostic

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 16:17:19 +09:00
End Time: 2026-05-19 16:21:08 +09:00

## Objective

Move the first capability-boundary enforcement slice out of documentation-only status by reporting a type-checker diagnostic when `dynamic` type annotations appear outside an explicit `dynamic fun` boundary.

## Scope

In:
- Add a stable type-checking diagnostic for markerless `dynamic` type annotations.
- Allow `dynamic` parameter, return, and local value annotations inside functions marked with the `dynamic` function modifier.
- Report the diagnostic for function signature and local value annotations in non-`dynamic` functions.
- Add type-checker fixture coverage and a CLI JSON smoke.
- Update diagnostics, interop, feature map, checklist, traceability, and task index docs.
- Commit and push when this task is completed.

Out:
- Full capability effect propagation across function calls.
- Reflection, COM, unsafe, native pointer, and P/Invoke enforcement.
- Compatibility options for relaxing dynamic diagnostics.
- Generated C# dynamic binder lowering beyond the current parsed marker surface.

## Acceptance Criteria

- [x] `DiagnosticDescriptors` includes a stable `TS2206` descriptor for dynamic capability enforcement.
- [x] The type checker reports `TS2206` for `dynamic` parameter, return, and local value annotations outside `dynamic fun`.
- [x] The type checker allows the same annotations inside `dynamic fun`.
- [x] Golden diagnostics fixture coverage verifies the markerless and allowed cases.
- [x] CLI JSON smoke verifies `TS2206` is emitted through `typesharp check`.
- [x] Docs explain that `dynamic` type annotations require a `dynamic fun` boundary.
- [x] `docs/tasks/README.md`, `docs/checklist.md`, and `docs/traceability.md` are updated.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic capability"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:17:19 +09:00: Started task after confirming no Planned/In Progress task packets remained and selecting the capability-boundary goal slice from `docs/goal.md`.
- 2026-05-19 16:21:08 +09:00: Added `TS2206`, enforced `dynamic` type annotations through `dynamic fun` boundaries, added fixture and CLI JSON smoke coverage, and updated diagnostics/interoperability docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "dynamic capability"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` from `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
