# Task 0121: Imported Attribute And Generic Type Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-19 10:35:00 +09:00
End Time: 2026-05-19 10:37:16 +09:00

## Objective

Close a completion-audit gap in `docs/goal.md` by adding direct evidence that TypeSharp can reference C# `.NET Framework` assembly attribute types and generic types from a local `net48` DLL.

## Scope

In:
- Extend the local legacy C# DLL smoke source with `LegacyMarkerAttribute : System.Attribute` and `LegacyBox<T>`.
- Add a TypeSharp CLI build smoke that imports both symbols, uses the attribute in an attribute list, and uses `LegacyBox<string>` in public function signatures.
- Fix parser declaration prefix handling so `[Attribute] export fun ...` preserves the export modifier.
- Update checklist, traceability, and task index evidence.

Out:
- Emitting TypeSharp attributes into generated C# metadata. Attribute emission remains governed by grammar/interop docs and is separate from this imported-reference smoke.
- Full attribute payload decoding in metadata reader.

## Acceptance Criteria

- [x] Local C# `net48` DLL contains a public attribute type and generic type.
- [x] TypeSharp source can import the attribute and generic type names from that DLL.
- [x] TypeSharp source can place the imported attribute before an exported function.
- [x] Generated C# build preserves `LegacyBox<string>` in the signature and compiles against the local DLL.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "imported attribute"
```

Result:
- PASS compiler test project build.
- PASS `CLI build compiles imported attribute and generic type references`.

## Handoff

Done:
- Added the imported attribute/generic type smoke.
- Fixed attribute-before-export parser modifier preservation.

Remaining:
- Attribute metadata emission and full attribute payload decoding remain future work, not required for this imported-reference smoke.

Blocked:
- None.
