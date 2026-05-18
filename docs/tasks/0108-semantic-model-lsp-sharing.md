# Task: Semantic Model LSP Sharing

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 05:30:24 +09:00
End Time: 2026-05-19 05:37:15 +09:00

## Objective

Introduce a minimal compiler-owned semantic model so LSP diagnostics, hover, go-to-definition, and completion share compiler parse/bind/type-check data instead of reassembling binder state inside the language server.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../architecture.md](../architecture.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Semantics/TypeSharpSemanticModel.cs`
- `src/TypeSharp.LanguageServer`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- single-document `TypeSharpSemanticModel.AnalyzeText`
- parser diagnostics, binder symbols, and type-checker diagnostics aggregation
- `FindSymbolAt(SourcePosition)` for source symbols and built-in type fallback
- partial symbol collection for incomplete source completion
- LSP diagnostics, hover, go-to-definition, and completion sharing the compiler semantic model
- smoke test for semantic model source-position lookup

Out:
- cross-file project semantic model
- imported metadata/source unified symbol graph
- rich expression type information
- inference engine
- incremental semantic cache
- analyzer/plugin API surface

## Acceptance Criteria

- [x] Compiler exposes a semantic model API independent of the language server.
- [x] LSP diagnostics use the compiler semantic model for the same parse/bind/type-check diagnostics path.
- [x] LSP hover and go-to-definition resolve symbols through the compiler semantic model.
- [x] LSP completion uses compiler semantic symbols while preserving partial source completion.
- [x] Tests prove semantic model symbol lookup and existing LSP smoke paths.
- [x] Checklist and traceability distinguish current single-document semantic model from future project/inference/metadata work.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- compiler, CLI, language server, and tests build without warnings.
- semantic model smoke and existing LSP smoke tests pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added compiler-owned `TypeSharpSemanticModel` and `TypeSharpSemanticSymbol`.
- Routed LSP diagnostics, hover, definition, and completion through the semantic model.
- Added semantic model source-position symbol lookup smoke coverage.
- Marked the `semantic model` checklist item complete for the current single-document scope.

Remaining:
- None.

Blocked:
- None.
