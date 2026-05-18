# Task Rollup: Release And Compiler Readiness

Status: Done
Queue: Q1-Q5
Start Time: 2026-05-19 08:04:07 +09:00
End Time: 2026-05-19 08:05:06 +09:00

## Objective

Compress the completed release readiness, feature review, compiler API, backend, ABI, overload, lowering, IL seam, and inference task packets into one rollup after the project checklist reached a fully checked state.

## Source Packets

- 0107 Release Readiness Policy
- 0108 Semantic Model LSP Sharing
- 0109 Feature Review Gate
- 0110 Backend Abstraction Seam
- 0111 Public ABI Checker
- 0112 C# Overload Resolver
- 0113 Feature Specification Index
- 0114 Lowering Pass Pipeline
- 0115 IL Backend Artifact Seam
- 0116 Local Expression Inference Engine

## Summary

Release and process:
- Added [../release.md](../release.md) for versioning, breaking changes, preview feature gates, checksum/signing, security policy, release notes, and compatibility matrix baseline.
- Added [../feature-review.md](../feature-review.md) as the repeatable feature Done gate.
- Added [../feature-specs.md](../feature-specs.md) as the implemented/stable feature specification index.

Compiler and tooling seams:
- Added `TypeSharpSemanticModel` and routed LSP diagnostics, hover, definition, and completion through shared compiler semantics.
- Added `ITypeSharpBackend`, `CSharpSourceBackendAdapter`, `TypeSharpBackendArtifact`, and backend artifact kind contracts.
- Added `TypeSharpLoweringPipeline`, `ITypeSharpLoweringPass`, and `csharp-runtime-import` lowering pass.
- Added `TypeSharpInferenceEngine` and delegated local expression inference for literals, identifiers, direct calls, and basic binary expressions.

Interop and ABI:
- Added deterministic generated assembly public ABI snapshots through `TypeSharpPublicAbiChecker`.
- Added `TypeSharpCSharpOverloadResolver` for metadata-backed exact literal, params, optional, named, and byref overload applicability.
- Added a direct assembly artifact seam for future IL backend work while keeping actual IL emit in Stable Backlog.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
rg -n "\[ \]" docs\checklist.md
git diff --check
```

Result:
- Pass. Compiler/test project builds without warnings or errors.
- Pass. Full smoke suite passes, including semantic model, backend artifact, lowering pipeline, public ABI, overload resolver, and inference engine smokes.
- Pass. `rg -n "\[ \]" docs\checklist.md` returns no unchecked checklist items.
- Pass. `git diff --check`.

## Handoff

Done:
- Condensed task packets 0107 through 0116 into this rollup.
- Preserved source packet objectives, core changes, and verification results at summary level.
- Updated the task index to point at this rollup instead of the individual source packets.

Remaining:
- None.

Blocked:
- None.
