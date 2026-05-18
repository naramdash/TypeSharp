# Task: CSharp Overload Resolver

Status: Done
Queue: Q3
Start Time: 2026-05-19 05:48:01 +09:00
End Time: 2026-05-19 05:51:49 +09:00

## Objective

Extract metadata-backed C# overload candidate selection into a compiler-owned resolver API and align the checklist with the currently implemented overload resolution scope.

## Source Of Truth

- [../csharp-interop.md](../csharp-interop.md)
- [../regression-testing.md](../regression-testing.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- metadata-backed imported static method overload candidate resolver
- arity filtering
- named argument parameter matching
- optional parameter omission
- trailing `params` expanded argument handling
- byref modifier matching
- exact literal primitive ranking
- ambiguity reporting through existing validator diagnostic path

Out:
- full C# conversion ranking
- generic method inference
- extension method instance sugar
- delegate/lambda contextual overload ranking
- user-defined conversions
- overload binding for TypeSharp-declared functions

## Acceptance Criteria

- [x] C# overload candidate selection is exposed through a compiler-owned resolver API.
- [x] Interop validator uses the resolver instead of private candidate-selection helpers.
- [x] Direct resolver smoke proves exact literal match narrowing.
- [x] Existing ambiguous overload, params, optional, named argument, byref, and generated build smokes still pass.
- [x] Checklist and traceability describe current C# overload resolution scope and remaining boundaries.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- compiler, CLI, language server, and tests build without warnings.
- resolver smoke and existing interop overload suite pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `CSharpOverloadCandidate`, `CSharpOverloadResolution`, and `TypeSharpCSharpOverloadResolver`.
- Routed `TypeSharpInteropValidator` through the resolver.
- Added direct exact literal overload resolver smoke coverage.
- Marked `C# overload resolution` complete for the current metadata-backed interop resolver scope.

Remaining:
- None.

Blocked:
- None.
