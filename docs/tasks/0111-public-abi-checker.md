# Task: Public ABI Checker

Status: Done
Queue: Q3-Q5
Start Time: 2026-05-19 05:44:15 +09:00
End Time: 2026-05-19 05:46:14 +09:00

## Objective

Promote the generated public ABI metadata smoke from ad hoc test assertions into a compiler-owned deterministic public ABI checker API.

## Source Of Truth

- [../runtime-abi.md](../runtime-abi.md)
- [../regression-testing.md](../regression-testing.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Abi`
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- deterministic public ABI snapshot lines for local assembly metadata
- assembly identity, public top-level type names, public property names/types, and public method signatures
- metadata reader property type extraction
- generated `net48` assembly public ABI checker smoke
- checklist and traceability update

Out:
- release-time baseline file management
- binary compatibility diffing
- nested type ABI snapshots
- attribute payloads
- generic constraints
- full CLR metadata coverage

## Acceptance Criteria

- [x] Compiler exposes a public ABI checker API.
- [x] Public ABI checker output is deterministic.
- [x] Metadata reader includes public property type names.
- [x] Generated public ABI smoke uses the checker instead of ad hoc metadata assertions.
- [x] Existing compiler, backend, interop, host, and LSP tests still pass.
- [x] Checklist and traceability describe current checker scope and remaining boundaries.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- compiler, CLI, language server, and tests build without warnings.
- generated public ABI snapshot smoke and existing suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `PublicAbiSnapshot` and `TypeSharpPublicAbiChecker`.
- Added public property type extraction to `TypeSharpMetadataReader`.
- Updated generated public ABI snapshot smoke to assert deterministic checker output.
- Marked `public ABI checker` complete for the current generated assembly metadata snapshot scope.

Remaining:
- None.

Blocked:
- None.
