# Task Rollup: CSharp Metadata Backed Interop Validation

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:51:23 +09:00
End Time: 2026-05-18 23:16:27 +09:00
Rollup Time: 2026-05-18 23:17:39 +09:00

## Objective

Referenced local `net481` C# DLL metadata를 읽고, 그 metadata를 사용해 TypeSharp source의 C# interop call에서 byref modifier, overload ambiguity, exact overload narrowing, expanded `params` overload 후보를 emission 전에 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [0038-0042-csharp-member-byref-interop-smokes.md](0038-0042-csharp-member-byref-interop-smokes.md)
- `src/TypeSharp.Compiler/Interop`
- `src/TypeSharp.Compiler/Checking`
- `src/TypeSharp.Compiler/Building`
- `tests/TypeSharp.Compiler.Tests`

## Completed Work

- `0043-csharp-local-metadata-symbol-index`: local `net481` DLL의 public top-level type, method, property, parameter, and byref metadata를 `MetadataAssemblySymbol.Types`로 index했다.
- `0044-csharp-invalid-byref-diagnostic`: parsed `ref`/`out`/`in` call-site modifier가 metadata parameter modifier와 맞지 않을 때 `TS2403`을 보고하고 build emission을 중단한다.
- `0045-csharp-ambiguous-overload-diagnostic`: direct imported static method call이 여러 same-arity C# metadata 후보와 매칭될 때 `TS2402`를 보고한다.
- `0046-csharp-exact-overload-ranking`: string, bool, int, double, decimal literal/primitive exact match 후보 하나를 선택해 불필요한 `TS2402`를 피한다.
- `0047-csharp-params-metadata-flag`: local metadata reader가 `System.ParamArrayAttribute`를 읽어 `MetadataParameterSymbol.IsParams`로 보존한다.
- `0048-csharp-params-overload-validation`: trailing `params` parameter를 expanded arity 후보로 취급하고 expanded argument exact match와 ambiguity를 검증한다.

## Acceptance Criteria

- [x] local C# metadata reader indexes public type/member/parameter symbols from local `net481` DLLs.
- [x] `ref`/`out`/`in` modifier mismatch is reported as `TS2403`.
- [x] ambiguous C# overload candidates are reported as `TS2402`.
- [x] exact literal/primitive overload match can narrow a candidate set to one method.
- [x] `params` metadata is preserved and used for expanded overload applicability.
- [x] check/build diagnostics pipeline receives metadata-backed interop diagnostics before generated C# emission.
- [x] smoke tests cover metadata indexing, byref diagnostics, overload ambiguity, exact narrowing, and expanded `params` validation.
- [x] checklist and traceability are updated.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Result:
- Passed on 2026-05-18 23:16:27 +09:00 after `0048`.

## Handoff

Done:
- Local C# metadata now drives early interop validation for byref modifiers and overload candidate selection.
- Expanded `params` calls are included in metadata-backed applicability and exact match narrowing.
- Related task packets `0043` through `0048` are compressed into this rollup.

Remaining:
- Continue with optional parameter default metadata, named argument overload ranking, nullable metadata/unknown nullability diagnostics, delegate/event interop, and public ABI fixtures.

Blocked:
- None.
