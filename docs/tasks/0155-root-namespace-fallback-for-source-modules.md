# Task 0155: Root Namespace Fallback For Source Modules

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 16:44:01 +09:00
End Time: 2026-05-19 16:48:38 +09:00

## Objective

Keep namespace-less TypeSharp source files inside the explicit project module graph by lowering them under the manifest `rootNamespace` instead of the synthetic `TypeSharp.Generated` namespace.

## Scope

In:
- Use manifest `rootNamespace` as the generated C# namespace fallback when a source file has no file-scoped namespace.
- Use the same fallback for executable entry point lookup and generated `Program.g.cs` argument forwarding.
- Preserve explicit source `namespace` declarations as the strongest namespace source.
- Add CLI build smoke coverage for namespace-less source files.
- Update module, feature spec, checklist, traceability, and task index docs.
- Commit and push when this task is completed.

Out:
- Full source-module path based namespace derivation.
- Cross-file project symbol graph and import/export resolution.
- Ambient declaration manifest partitioning.
- Namespace/file-path analyzer warnings.

## Acceptance Criteria

- [x] `typesharp build` emits namespace-less source files under manifest `rootNamespace`.
- [x] Explicit source `namespace` declarations still control generated C# namespaces.
- [x] Executable projects can find a namespace-less `main` through manifest `rootNamespace`.
- [x] Generated entry point argument forwarding uses the same namespace fallback.
- [x] CLI build smoke coverage verifies generated source and executable assembly output.
- [x] Docs explain that missing file namespaces fall back to manifest `rootNamespace`, not global namespace.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "root namespace"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:44:01 +09:00: Started after auditing `docs/goal.md`; selected the explicit module graph/ambient isolation slice where namespace-less source files were still lowered to `TypeSharp.Generated`.
- 2026-05-19 16:48:38 +09:00: Added manifest `rootNamespace` fallback to C# source emission, executable main lookup, and entry point argument forwarding; added CLI build smoke and docs/checklist/traceability updates.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "root namespace"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build emits generated C# source"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`: passed.
- `npm run build`: passed.
- `git diff --check`: passed with expected line-ending normalization warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed; no tracked generated binaries were listed.

## Follow-Up

- Cross-file project symbol graph, source path based module names, import/export resolution, and ambient declaration partitioning remain future work.
