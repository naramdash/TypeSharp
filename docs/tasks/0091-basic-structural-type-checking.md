# Task: Basic Structural Type Checking

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:22:52 +09:00
End Time: 2026-05-19 04:28:48 +09:00

## Objective

Implement the MVP structural shape check promised by the TypeSharp goal: local compile-time structural aliases should accept nominal record values that provide the required shape and report diagnostics when required members are missing or incompatible.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/types.md](../grammar/types.md)
- [../grammar/resolution.md](../grammar/resolution.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- record shape type aliases such as `type Named = { Name: string }`
- width-subtyping checks from nominal records or other shape aliases into local structural aliases
- structural member access type inference for known shape/record members
- positive and negative type checker diagnostics fixtures
- CLI `check` JSON smoke for structural shape mismatch

Out:
- anonymous structural type annotations without aliases
- index signatures
- `satisfies`
- structural overload resolution
- generated public metadata adapters
- reflection/dynamic runtime shape checks

## Acceptance Criteria

- [x] A nominal record with all required shape members is assignable to a local structural alias.
- [x] Extra record members do not prevent assignment to the structural alias.
- [x] Missing required shape members produce a type-checker diagnostic.
- [x] Incompatible shape member types produce a type-checker diagnostic.
- [x] Member access on a structural alias returns the declared member type for type checking.
- [x] `docs/checklist.md` and `docs/traceability.md` reflect the implemented MVP structural type checking evidence.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- type checker fixture diagnostics include structural positive and negative coverage.
- CLI JSON smoke reports the structural mismatch diagnostic.
- existing parser, binder, backend, CLI, LSP, runtime, and net48 smokes continue to pass.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- `TypeSharpTypeChecker` now records structural shape aliases and nominal record shapes.
- Assignment and return checks accept width-compatible record/shape values for local structural aliases.
- Missing required members, incompatible member types, and invalid structural member access report `TS2201`.
- Added positive/negative type checker fixtures and CLI JSON structural diagnostic smoke coverage.
- Marked `basic structural type checking` complete in the checklist and traceability docs.

Remaining:
- Anonymous structural type annotations, index signatures, `satisfies`, structural overload resolution, and public metadata adapters remain out of scope.

Blocked:
- None.
