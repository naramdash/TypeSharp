# Task 0147: List Target Collection Expression Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 15:00:55 +09:00
End Time: 2026-05-19 15:06:38 +09:00

## Objective

Move the explicit target `List<T>` collection expression case out of Stable Backlog by lowering it to C# 7.3-compatible collection initializer source while keeping target-type-free collection literals as array inference.

## Scope

In:
- Teach the type checker to use an expected array or `List<T>` collection target for collection expression element compatibility.
- Keep mixed element type diagnostics stable.
- Emit `new List<T> { ... }` when a collection expression is used in a return or local initializer with explicit target `List<T>`.
- Keep `[]` valid for explicit array and explicit `List<T>` targets.
- Update backend fixture and CLI build smoke to cover generated `net48` assembly and C# consumer compatibility.
- Update feature map, lowering guide, checklist, traceability, and task index.

Out:
- Target-type-free `List<T>` inference.
- Dictionary literals.
- Spread elements.
- Capacity/comparer/builder arguments.
- C# 15 collection expression argument syntax.

## Acceptance Criteria

- [x] `export fun f(): List<string> = ["a"]` emits `new List<string> { "a" }`.
- [x] `let xs: List<int> = [1, 2, 3]` emits `new List<int> { 1, 2, 3 }`.
- [x] `export fun empty(): List<string> = []` emits `new List<string> { }`.
- [x] Existing homogeneous array collection lowering remains unchanged.
- [x] Existing mixed element type diagnostic message remains unchanged.
- [x] Generated `net48` assembly compiles and a C# `net48` consumer can call both array and `List<T>` APIs.
- [x] Stable Backlog docs now distinguish implemented explicit target `List<T>` lowering from future `List<T>` inference and advanced collection forms.

## Verification

Planned commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build compiles collection expression lowering"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build compiles collection expression lowering"`
- PASS `npm run build` in `docs-site`
- PASS `git diff --check`
- PASS `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"` returned no tracked binary artifacts.

## Handoff

Done:
- `TypeSharpTypeChecker` now preserves collection element consistency diagnostics and uses expected array/`List<T>` targets for element compatibility.
- `CSharpSourceBackend` now lowers explicit target `List<T>` collection expressions to C# collection initializers.
- Backend fixture `0022-collection-expression-lowering` now covers array and `List<T>` lowering.
- CLI collection expression smoke now builds generated `net48` source and a C# consumer against array and `List<T>` APIs.
- Feature map, lowering guide, checklist, traceability, and task index were updated.

Remaining:
- Target-type-free `List<T>` inference, dictionary/spread/builder support, and collection arguments remain Stable Backlog/Preview Watch.

Blocked:
- None.
