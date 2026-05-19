# Task 0143: C# Generic Method Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-19 14:22:49 +09:00
End Time: 2026-05-19 14:30:01 +09:00

## Objective

Add direct evidence that TypeSharp can reference a local `net48` C# DLL containing a generic method and preserve an imported generic method call through generated C# so the C# compiler can close the method type argument.

## Scope

In:
- `LegacyGenericMethods.Identity<T>(T value)` local DLL fixture.
- Metadata reader smoke coverage for generic method return/parameter placeholder signatures.
- CLI build smoke for an imported generic method call.
- Checklist, feature-spec, C# interop, traceability, and task queue updates.

Out:
- Explicit generic method type argument syntax at call sites.
- TypeSharp-side generic method type inference.
- Generic method overload ranking.
- Generic constraints on imported C# generic methods.
- Generic method ABI formatting beyond current placeholder signature evidence.

## Acceptance Criteria

- [x] The local metadata reader indexes a generic method on a referenced `net48` C# DLL.
- [x] The metadata smoke records the current `!!0` placeholder signature shape for generic method return and parameter types.
- [x] A generated `net48` project compiles an imported generic method call through C# compiler method type inference.
- [x] Docs and traceability distinguish this compile smoke from full TypeSharp-side generic method inference.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "generic method"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "metadata reader indexes local public symbols"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS compiler test project build.
- PASS focused imported generic method call build smoke.
- PASS metadata reader local public symbol index smoke.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.

## Handoff

Done:
- Added a local C# generic method fixture.
- Added metadata reader assertions for generic method placeholder signatures.
- Added generated `net48` compile smoke for imported generic method calls.
- Updated checklist, feature-spec, C# interop, traceability, and task queue docs.

Remaining:
- Explicit call-site generic type arguments, TypeSharp-side generic method inference, imported generic method overload ranking, imported generic constraints, and richer metadata naming remain future work.

Blocked:
- None.
