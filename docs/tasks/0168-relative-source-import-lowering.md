# Task 0168: Relative Source Import Lowering

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 22:18:17 +09:00
End Time: 2026-05-19 22:26:56 +09:00

## Objective

Move relative source imports from graph-only diagnostics into generated C# lowering for the first supported source module import slice.

## Scope

In:
- Keep `TS0112` for missing relative source module targets.
- Allow supported resolved relative source imports through `SourceModuleGraph` without `TS0113`.
- Lower unaliased relative named imports to generated C# `using static <target namespace>.<target module container>;`.
- Lower relative namespace imports to generated C# aliases such as `using Helper = <target namespace>.<target module container>;`.
- Keep unsupported relative source import forms, such as named source import aliases, blocked by `TS0113`.
- Preserve existing external C# namespace/type import alias behavior.
- Add build/check smoke coverage and update docs/checklist/traceability.
- Commit and push when this task is completed.

Out:
- Full project-wide export/public-surface filtering.
- Re-export lowering.
- Named source method alias lowering.
- Precise cross-file type inference for imported source declarations.
- User-configurable generated container naming.

## Acceptance Criteria

- [x] `SourceModuleGraph` records supported relative source import dependencies without reporting `TS0113`.
- [x] `import { helper } from "./Feature/Helper"` can build when `helper` is emitted from the target source module.
- [x] `import * as Helper from "./Feature/Helper"` can build and call `Helper.helper()`.
- [x] Unsupported named source import aliases still report `TS0113` and stop build before emission.
- [x] Missing relative source targets still report `TS0112`.
- [x] Documentation describes the supported relative source import lowering and remaining unsupported forms.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "relative source"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported source module"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unresolved source module"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 22:18:17 +09:00: Started after the module graph and multi-source generated container work left resolved relative imports blocked by `TS0113`.
- 2026-05-19 22:26:56 +09:00: Added source import targets for the C# backend, shared generated module container naming, relative named/namespace import lowering, unsupported alias diagnostics, smoke tests, and docs updates.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "relative source"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported source module"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unresolved source module"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry"`: passed.
- `npm run build` from `docs-site`: passed and generated 24 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
