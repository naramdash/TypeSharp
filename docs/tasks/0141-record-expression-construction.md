# Task 0141: Record Expression Construction

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 13:54:00 +09:00
End Time: 2026-05-19 14:04:26 +09:00

## Objective

Implement the MVP nominal record construction path for `{ Field: value }` record expressions when an expected record type is known, so the stable grammar has generated `net48` lowering, field diagnostics, and C# consumer evidence.

## Scope

In:
- Parser disambiguation for record expressions in expression context.
- C# source backend lowering from expected nominal record expressions to constructor calls.
- Type checker diagnostics for missing, extra, and incompatible record expression fields when an expected record or structural shape is known.
- Backend fixture and CLI build/C# consumer smoke.
- Checklist, feature map, lowering, feature-spec, traceability, and task queue updates.

Out:
- Structural object literal lowering without a nominal target.
- Spread fields.
- C# object initializer lowering for mutable imported types.
- Full contextual typing through arbitrary call arguments.

## Acceptance Criteria

- [x] `{ Name: "Ada", Age: 36 }` parses as a record expression in expression contexts.
- [x] A record expression with expected nominal record type lowers to a C# 7.3-compatible constructor call.
- [x] Missing, extra, and incompatible fields produce `TS2201` diagnostics.
- [x] Backend fixture snapshots pin generated C#.
- [x] CLI build smoke produces a generated `net48` assembly and C# consumer compiles against it.
- [x] Docs and traceability distinguish implemented nominal record construction from broader structural object literal lowering backlog.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "record expression"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS compiler test project build.
- PASS focused record expression build/C# consumer smoke.
- PASS C# backend fixture snapshots.
- PASS type checker diagnostics fixtures.
- PASS parser fixture snapshots.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.

## Handoff

Done:
- Added expression-context record expression parsing.
- Added expected nominal record expression lowering.
- Added record expression field diagnostics.
- Added backend fixture, diagnostic fixture, CLI build smoke, and docs/task updates.

Remaining:
- None for MVP expected nominal record construction.
- Structural object literal lowering without a nominal target, spread fields, object initializer lowering, and richer contextual typing remain future work.

Blocked:
- None.
