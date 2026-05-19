# Task 0148: Unsupported Package Reference Diagnostic

Status: Done
Queue: Q3-Q4
Start Time: 2026-05-19 15:09:10 +09:00
End Time: 2026-05-19 15:14:14 +09:00

## Objective

Make the reserved NuGet package reference manifest surface explicit and safe by reporting `TS2405` for `[references].packages` until package restore, lock files, compatibility selection, and license/checksum policy are implemented.

## Scope

In:
- Add an interop diagnostic descriptor for unsupported package references.
- Report a deduplicated diagnostic for non-empty `references.packages`.
- Ensure `typesharp check` emits JSON `TS2405`.
- Ensure `typesharp build` stops before generated C# source/project/assembly emission on `TS2405`.
- Update user-facing docs, diagnostic taxonomy, checklist, traceability, and task index.

Out:
- NuGet restore.
- Transitive dependency resolution.
- Lock file, checksum, signing, or license inventory generation.
- Package asset selection for `net48`.

## Acceptance Criteria

- [x] `DiagnosticDescriptors.All` includes stable `TS2405`.
- [x] `TypeSharpReferenceResolver.Resolve` reports `TS2405` for non-empty `references.packages`.
- [x] Duplicate package strings produce one resolver diagnostic.
- [x] `typesharp check --diagnostic-format json` surfaces `TS2405`.
- [x] `typesharp build --diagnostic-format json` surfaces `TS2405` and does not emit generated artifacts.
- [x] Docs tell users to use local `net48` DLL paths until package restore is implemented.

## Verification

Planned commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "reference resolver reports unsupported package diagnostics"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI check emits JSON unsupported package diagnostics"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build stops before emission on package diagnostics"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "reference resolver reports unsupported package diagnostics"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI check emits JSON unsupported package diagnostics"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build stops before emission on package diagnostics"`
- PASS `npm run build` in `docs-site`
- PASS `git diff --check`
- PASS `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"` returned no tracked binary artifacts.

## Handoff

Done:
- Added `TS2405` Unsupported package reference descriptor.
- Added resolver, CLI check, and CLI build no-emission smokes.
- Updated diagnostics, CLI, C# interop, docs-site API/troubleshooting, feature map, checklist, traceability, and task index.

Remaining:
- Full NuGet restore, lock file, transitive dependency, asset selection, checksum/signing, and license inventory support remains Stable Backlog.

Blocked:
- None.
