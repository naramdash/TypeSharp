---
name: "typesharp-language-engineering"
description: "Use for TypeSharp programming-language implementation work involving grammar, parser, AST, binder, type checker, diagnostics, modules/imports/exports, narrowing, lowering, runtime semantics, fixtures, compiler architecture, or language-design decisions."
---

# TypeSharp Language Engineering

Use this skill when the task is about designing or implementing TypeSharp as a programming language rather than editing ordinary app code.

## Workflow

1. Classify the language surface:
   - syntax or grammar
   - AST and parser behavior
   - binder, symbols, modules, imports, exports, or visibility
   - type checker, inference, narrowing, unions, intersections, structural shapes, or diagnostics
   - lowering, runtime semantics, generated C# shape, or public ABI
2. Read only the matching canonical docs:
   - `docs/src/content/docs/goal.md`
   - `docs/src/content/docs/requirements.md`
   - `docs/src/content/docs/grammar.md`
   - `docs/src/content/docs/reference.md`
   - `docs/src/content/docs/modules.md`
   - `docs/src/content/docs/type-system.md`
   - `docs/src/content/docs/lowering.md`
   - `docs/src/content/docs/diagnostics.md`
   - `docs/src/content/docs/feature-status.md`
3. Check the design boundary before coding:
   - Is this feature Direct, Equivalent, Replacement, Planned, Experimental, or Rejected?
   - Can it lower to C# 7.3-compatible source for `net48` artifacts?
   - Does it preserve deterministic diagnostics?
   - Does it keep TypeScript-style compile-time flexibility separate from public .NET ABI?
4. Implement through the existing compiler shape. Prefer local patterns in `src/TypeSharp.Compiler` and focused fixtures under `tests/fixtures`.
5. Verify with repository-native commands:
   - `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
   - `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
   - targeted `dotnet run --project ... --no-build "<filter>"` when the existing harness supports it

## Constraints

- Do not invent a separate compiler pipeline when the existing parser, binder, checker, diagnostics, or lowering layer can be extended.
- Do not broaden grammar without fixture coverage and docs updates.
- Do not hide language decisions only in code; update the canonical docs or task ledger when behavior changes.
- Do not require runtimes or global tools beyond Windows 10 defaults plus installed `dotnet` and `node`.
