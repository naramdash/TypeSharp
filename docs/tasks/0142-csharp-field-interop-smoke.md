# Task 0142: C# Field Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-19 14:09:56 +09:00
End Time: 2026-05-19 14:20:14 +09:00

## Objective

Add metadata-backed public field indexing and generated C# compile evidence for imported static and instance field access from a local `net48` C# DLL.

## Scope

In:
- `MetadataFieldSymbol` and `MetadataTypeSymbol.Fields`.
- Public field reading from local C# metadata, including static and literal flags.
- Public ABI snapshot emission for indexed fields.
- `LegacyFields` local DLL fixture and CLI build smoke for static/instance field access.
- Checklist, feature spec, C# interop, traceability, and task queue updates.

Out:
- Field assignment validation.
- Const value decoding.
- Member binding diagnostics for missing fields.
- Inherited/private/internal field metadata.
- Full generic field type decoding beyond the current simple signature provider.

## Acceptance Criteria

- [x] Local public fields are indexed on `MetadataTypeSymbol.Fields`.
- [x] Field metadata preserves type name, static flag, and literal flag.
- [x] Public ABI snapshot output includes indexed fields.
- [x] A generated `net48` project compiles imported static and instance field access.
- [x] Docs and traceability distinguish implemented field read smoke from broader field assignment and member validation backlog.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "field"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "metadata reader indexes local public symbols"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "public ABI snapshot"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS compiler test project build.
- PASS focused imported field access build smoke.
- PASS metadata reader local public symbol index smoke.
- PASS public ABI snapshot smoke.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.

## Handoff

Done:
- Added public field metadata symbols to the local metadata reader graph.
- Added public ABI field output for indexed fields.
- Added local C# fixture coverage for `const` static and instance fields.
- Added generated `net48` compile smoke for imported field access.
- Updated checklist, feature-spec, C# interop, traceability, and task queue docs.

Remaining:
- Field assignment validation, const value decoding, and member binding diagnostics remain future work.

Blocked:
- None.
