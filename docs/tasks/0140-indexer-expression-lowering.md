# Task 0140: Indexer Expression Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 13:42:15 +09:00
End Time: 2026-05-19 13:53:06 +09:00

## Objective

Implement the MVP indexer expression path so parser-level `receiver[index]` syntax has C# 7.3 source backend lowering, simple array result inference, and imported C# indexer build evidence.

## Scope

In:
- C# source backend lowering for `IndexerExpression` nodes.
- Simple type checker inference from array receiver `T[]` to indexed result `T`.
- Backend fixture for array indexer lowering.
- CLI build smoke for local `net48` C# DLL indexer access.
- Checklist, C# interop, feature map, lowering, feature-spec, traceability, and task queue updates.

Out:
- Metadata-backed indexer overload validation.
- Multidimensional indexer argument lists.
- Indexer setter-specific validation.
- Rich contextual typing for non-array imported indexers.

## Acceptance Criteria

- [x] `receiver[index]` lowers to C# 7.3-compatible indexer/array access.
- [x] Array receiver types such as `T[]` infer indexed result type `T`.
- [x] Backend fixture snapshots pin generated C#.
- [x] CLI build smoke compiles TypeSharp source that reads an imported C# indexer from a local `net48` DLL.
- [x] Docs and traceability distinguish implemented indexer lowering from metadata-backed indexer overload validation backlog.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "metadata reader indexes local public symbols"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "indexer"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS compiler test project build.
- PASS metadata reader local public symbol smoke with the added indexer metadata.
- PASS focused imported indexer build smoke.
- PASS C# backend fixture snapshots.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.

## Handoff

Done:
- Added indexer expression C# source emission.
- Added array receiver result inference.
- Added backend fixture, imported C# indexer compile smoke, and docs/task updates.

Remaining:
- None for MVP indexer expression lowering.
- Metadata-backed indexer overload validation, multidimensional indexer arguments, and setter-specific validation remain future work.

Blocked:
- None.
