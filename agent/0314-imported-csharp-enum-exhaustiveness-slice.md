# Task 0314 Imported C# Enum Exhaustiveness Slice

Status: In Progress
Priority: Q3
Created: 2026-05-21

## Objective

Extend the existing enum match exhaustiveness path from TypeSharp-owned enum declarations to finite imported C# enum metadata.

## Scope

- Read imported C# enum member names from metadata-backed public literal static enum fields.
- Register named imported C# enums in the TypeSharp type-checking scope, including import aliases.
- Report `TS2203` when a `match` over an imported C# enum omits known members.
- Keep guarded enum arms non-covering unless a later unguarded member or discard covers the same member space.
- Lower imported C# enum matches to C# 7.3-compatible member comparisons using the local imported type name or alias.
- Add metadata, checker, and CLI build coverage with a local `net48` C# reference assembly.

## Out Of Scope

- `[Flags]` set algebra, combined bitmask exhaustiveness, and zero-value special handling.
- Explicit TypeSharp enum underlying types, numeric values, aliases, and member attributes.
- Namespace import enum shapes such as `Tools.Color` unless the existing named-import path naturally supports them.
- General richer pattern algebra beyond the existing enum member/discard/guard model.

## Evidence Targets

- `lang/TypeSharp.Compiler/Interop/MetadataAssemblySymbol.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpMetadataReader.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- `docs/src/content/docs/feature-status.md`
- `docs/src/content/docs/dotnet-interop.md`
- `docs/src/content/docs/lowering.md`

## Done Criteria

- Imported C# enum metadata exposes a deterministic member list.
- `typesharp check` reports missing imported enum match members before emission.
- `typesharp build` compiles a generated `net48` library that matches over an imported C# enum.
- Docs and operational ledgers describe the implemented boundary and remaining flags/numeric-value work.
- Full compiler tests, docs build, and `git diff --check` pass.
