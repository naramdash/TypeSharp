# Task 0149: Partial Declaration Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 15:20:39 +09:00
End Time: 2026-05-19 15:59:03 +09:00

## Objective

Close the documented C# interop `partial` declaration surface by parsing `partial` as a declaration modifier and preserving it in C# 7.3-compatible generated type/module declarations.

## Scope

In:
- Add lexer/parser support for `partial` declaration prefixes.
- Preserve `PartialModifier` in syntax snapshots.
- Emit C# `partial` for TypeSharp `module`, `record`, `union`, `class`, and `interface` declarations where the current backend lowers them to C# types.
- Add parser, backend snapshot, LSP keyword completion, and CLI build smoke coverage.
- Update feature map, lowering docs, checklist, traceability, docs-site reference, and task index.

Out:
- Partial methods, partial constructors, partial events, or generated source augmentation hooks.
- Cross-file partial merge validation.
- C# consumer extension of partial types across assemblies. C# partial parts must compile into the same assembly.

## Acceptance Criteria

- [x] `partial` lexes as a keyword and parses into `PartialModifier` before supported declarations.
- [x] Parser fixture snapshots cover `export partial module`, `public partial record`, `partial class`, and `public partial interface`.
- [x] C# backend emits C# 7.3-compatible `partial` modifiers for module/static class, record/sealed class, union/abstract class, class, and interface declarations.
- [x] CLI build smoke produces a generated `net48` assembly from partial declarations.
- [x] User-facing docs describe the implemented scope and exclude partial methods/events/constructors.

## Verification

Planned commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build compiles partial declaration API"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "language server returns completion items"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build compiles partial declaration API"`
- PASS `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "language server returns completion items"`
- PASS `npm run build` in `docs-site`
- PASS `git diff --check`
- PASS `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"` returned no tracked binary artifacts.

## Handoff

Done:
- `partial` is implemented for the declaration families currently lowered to C# type declarations.
- Parser/backend snapshots and CLI build smoke verify generated C# syntax and `net48` assembly production.

Remaining:
- Partial method/event/constructor syntax and same-assembly generated source augmentation hooks remain future work.

Blocked:
- None.
