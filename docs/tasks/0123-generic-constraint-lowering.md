# Task 0123: Generic Constraint Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 11:10:00 +09:00
End Time: 2026-05-19 11:19:46 +09:00

## Objective

Promote TypeSharp generic `where` clauses from grammar draft toward generated `net48` API behavior by preserving C# 7.3-compatible constraints in the C# source backend.

## Scope

In:
- Parse `where T: ...` clauses on functions, records, classes, interfaces, and delegates.
- Lower `class`, `struct`, `new()`, and nominal/interface type constraints to C# `where` clauses.
- Preserve constraints on generated generic functions, classes, and interface methods.
- Bind nested generic method type parameters inside type declarations.
- Report `TS2205` for `notnull`, which is a design-level constraint that cannot be emitted by the current C# 7.3 backend.
- Add backend snapshot and generated `net48` C# consumer smoke coverage.

Out:
- Full generic constraint satisfiability checks.
- Constraint ordering diagnostics beyond generated C# build validation.
- C# 8+ `notnull` lowering.
- Generic method inference.

## Acceptance Criteria

- [x] Parser produces `WhereClause`/`GenericConstraint`/`ConstraintItem` nodes.
- [x] C# backend emits generic `where` clauses for functions, classes, and interface methods.
- [x] Unsupported `notnull` constraints produce a TypeSharp diagnostic before generated C# emission.
- [x] Snapshot fixture covers generated C# constraint shape.
- [x] CLI build smoke compiles generated `net48` assembly and a C# `net48` consumer.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "generic constraint"
```

Result:
- PASS compiler test project build.
- PASS C# backend fixture snapshots.
- PASS `CLI build compiles generic constraint API`.
- PASS `CLI check emits JSON unsupported generic constraint diagnostics`.

## Handoff

Done:
- Generic constraints now have parser, backend, diagnostic, snapshot, and build smoke evidence for the C# 7.3-compatible subset.

Remaining:
- Full constraint validation, ordering diagnostics, and generic method inference remain future work.
