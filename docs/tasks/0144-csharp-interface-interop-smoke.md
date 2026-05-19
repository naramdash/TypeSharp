# Task 0144: C# Interface Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-19 14:31:05 +09:00
End Time: 2026-05-19 14:38:45 +09:00

## Objective

Add direct evidence that TypeSharp can reference a public interface from a local `net48` C# DLL, preserve it in generated C# public signatures, and compile member access against that interface.

## Scope

In:
- `ILegacyNamed` local C# interface fixture and `LegacyNamed` implementation fixture.
- Metadata reader smoke coverage for imported public interface and class properties.
- CLI build smoke for an imported C# interface type annotation and interface property access.
- Checklist, feature-spec, C# interop, traceability, and task queue updates.

Out:
- Metadata relationship indexing for interface implementation.
- TypeSharp-side validation that an imported class implements an imported interface.
- Interface inheritance and explicit interface implementation metadata.
- C# interface variance analysis.

## Acceptance Criteria

- [x] The local metadata reader indexes a public interface from a referenced `net48` C# DLL.
- [x] The metadata smoke records interface and implementation class public properties.
- [x] A generated `net48` project compiles a TypeSharp function with imported interface parameter type and interface property access.
- [x] Docs and traceability distinguish imported interface reference support from full implementation relationship validation.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "imported interface"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "metadata reader indexes local public symbols"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS compiler test project build.
- PASS focused imported interface reference build smoke.
- PASS metadata reader local public symbol index smoke.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.
- During development, a broader class-to-interface return smoke failed with `TS2201` because imported interface implementation relationships are not indexed yet; the accepted scope now tests interface reference and member access only.

## Handoff

Done:
- Added a local C# interface fixture and implementation class fixture.
- Added metadata reader assertions for imported interface/class property metadata.
- Added generated `net48` compile smoke for imported interface parameter type and property access.
- Updated checklist, feature-spec, C# interop, traceability, and task queue docs.

Remaining:
- Imported class-to-interface implementation relationship validation, interface inheritance, explicit interface implementation metadata, and variance remain future work.

Blocked:
- None.
