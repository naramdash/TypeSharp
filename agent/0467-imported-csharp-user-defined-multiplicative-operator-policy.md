# Task: imported-csharp-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0466 roadmap refresh after imported C# null-conditional indexer multiplicative compound assignment checked/unchecked overflow policy

## Objective

Implement the first bounded user-defined multiplicative operator policy that can still lower through generated C# 7.3 source: imported C# public static binary `operator *`, `operator /`, and `operator %` metadata for mutable local `*=`, `/=`, and `%=` assignment. Preserve generated package-free `net48`, deterministic diagnostics, and the current MSTest.Sdk/MTP package-shard baseline.

## Context

- Task 0466 rechecked official C# 14/C# 15, F# 10, TypeScript 6.0/7.0 Beta, .NET Framework/.NET/NuGet/test/editor/CI signals and selected this bounded precursor before true C# 14 instance compound-assignment operators.
- C# 14 documents instance compound-assignment operators as void-returning special-name instance methods such as `op_MultiplicationAssignment`, but generated TypeSharp source must remain C# 7.3-compatible and cannot depend on emitting or directly calling C# 14 operator declarations.
- C# 7.3-compatible generated source can still use imported static binary operator overloads through ordinary `target *= value`, `/=`, or `%=` lowering when the checker has proven a single applicable operator and assign-back result.
- Existing primitive local/imported/null-conditional multiplicative assignment policies and checked/unchecked wrappers must keep their current behavior.

## Scope

- Extend imported C# metadata capture for public static special-name binary multiplicative operators `op_Multiply`, `op_Division`, and `op_Modulus` without exposing them as ordinary callable methods.
- Add a bounded checker path for mutable local assignment targets whose target type and right operand select a single imported C# static binary multiplicative operator and whose result is assignable back to the local target.
- Keep the initial accepted surface to local `let mut` targets. Imported member/indexer/null-conditional targets, TypeSharp-authored operator declarations, C# 14 instance compound-assignment operators, checked user-defined operators, conversions beyond the existing safe metadata rules, and broader overload ranking remain backlog.
- Preserve generated C# 7.3 source. Do not emit C# 14 operator declarations or require new runtime packages.
- Add focused positive generated `net48` consumer evidence and negative diagnostics for unsupported, ambiguous, nullable, missing, instance-compound-only, and non-assignable user-defined operator shapes.

## Out Of Scope

- Implementing C# 14 instance compound-assignment operators such as `op_MultiplicationAssignment`, `op_DivisionAssignment`, or `op_ModulusAssignment`.
- Implementing TypeSharp-authored operator declaration syntax.
- Extending user-defined operator support to imported field/property/indexer, null-conditional, checked/unchecked, additive, bitwise, shift, or increment/decrement targets.
- Implementing Task 0401 without explicit user approval.
- Changing generated target frameworks or adopting .NET 10/11-only generated-artifact APIs.

## Acceptance

- [ ] Imported C# static binary multiplicative operator metadata is captured and tested without becoming an ordinary method-call surface.
- [ ] Mutable local `*=`, `/=`, and `%=` targets can use one selected imported C# static binary operator when the result assigns back to the target.
- [ ] C# 14 instance compound-assignment operators remain rejected or ignored with deterministic diagnostics until a C# 7.3-compatible callable lowering or direct IL backend policy exists.
- [ ] Generated output remains package-free `net48` and C# 7.3-compatible.
- [ ] Regression coverage includes focused generated consumer, negative checker diagnostics, docs updates, shard/package baselines, stale-reference scan, and whitespace check.

## References

- [Task 0466 rollup](tasks-rollup.md#task-0466-roadmap-refresh-after-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-checkedunchecked-overflow-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Traceability](traceability.md)

## Handoff Notes

- Start by inspecting `TypeSharpMetadataReader`, metadata symbol types, `TypeSharpTypeChecker.CheckAssignmentExpression`, and existing multiplicative assignment tests.
- Keep the first implementation deliberately local-target-only unless the code already has a reusable operator-selection path with deterministic single-evaluation lowering for imported targets.
- Do not use Python.
