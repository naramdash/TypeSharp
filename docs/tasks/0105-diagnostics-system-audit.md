# Task: Diagnostics System Audit

Status: Done
Queue: Q1-Q2
Start Time: 2026-05-19 05:15:35 +09:00
End Time: 2026-05-19 05:17:24 +09:00

## Objective

Audit the implemented diagnostics system and align the top-level checklist item with descriptor registry, source span, CLI text, JSON formatter, golden fixture, CLI, build-stop, and LSP evidence.

## Source Of Truth

- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../regression-testing.md](../regression-testing.md)
- `src/TypeSharp.Compiler/Diagnostics`
- `tests/fixtures/parser`
- `tests/fixtures/diagnostics`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- diagnostic code taxonomy and descriptor metadata
- descriptor registry uniqueness and explanation metadata
- 1-based source positions/spans
- CLI text shape through `Diagnostic.ToCliText`
- JSON diagnostics formatter used by CLI and golden fixtures
- parser/binder/type checker/interop/backend descriptor allocation evidence
- LSP diagnostic mapping evidence

Out:
- `typesharp explain` command implementation
- related-information diagnostics
- warning suppression configuration
- localized messages
- machine-readable fix suggestions

## Acceptance Criteria

- [x] Existing docs and implementation define diagnostic code ranges and descriptor metadata.
- [x] Existing tests prove descriptor registry stability and CLI text shape.
- [x] Existing parser/binder/type-checker fixtures prove JSON diagnostic output shape.
- [x] Existing CLI/build/LSP smokes prove diagnostics propagate through user-facing surfaces.
- [x] Checklist and traceability distinguish completed diagnostics system scope from future explain/suppression/fix features.

## Verification

Command:

```text
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- diagnostic registry, fixture, CLI, build-stop, and LSP smoke paths pass.
- documentation changes have no whitespace errors.

Result:
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Audited `src/TypeSharp.Compiler/Diagnostics`, diagnostics policy, descriptor registry tests, golden diagnostics fixtures, CLI/build no-emission diagnostics, and LSP diagnostics usage.
- Marked the top-level `diagnostics system` checklist item complete.
- Updated traceability with exact current diagnostics system scope and explicit out-of-scope boundaries.

Remaining:
- None.

Blocked:
- None.
