# Release Policy

문서 기준일: 2026-05-19

이 문서는 TypeSharp release readiness 기준을 정의한다. 범위는 `TypeSharp.Compiler`, `typesharp` CLI, `TypeSharp.Core`, `TypeSharp.Runtime`, generated `net48` assembly 계약, 문서 세트, VS Code/LSP 산출물이다. Public ABI 세부 규칙은 [runtime-abi.md](runtime-abi.md), dependency inventory와 future dependency gate는 [dependencies.md](dependencies.md)를 따른다.

## Versioning Policy

TypeSharp release version은 `MAJOR.MINOR.PATCH[-preview.N]` 형태를 사용한다.

- `0.x`: pre-1.0 development line. Breaking change는 허용되지만 task packet, traceability, release note에 영향을 받은 surface와 migration path를 기록해야 한다.
- `1.0`: MVP stable line. `net48` generated assembly, `TypeSharp.Core`, `TypeSharp.Runtime`, CLI command surface, diagnostics schema, stable grammar subset을 안정 계약으로 둔다.
- Patch release: bug fix, diagnostic text clarification, docs correction, implementation fix. Public ABI, stable syntax, CLI schema, manifest meaning을 깨지 않는다.
- Minor release: backward-compatible feature, new diagnostic, new CLI option, new standard library helper, new lowering support. Existing stable source and generated `net48` public metadata must keep working.
- Major release: stable source compatibility, public ABI, manifest semantics, CLI JSON schema, generated metadata shape, or deployment shape를 깨는 변경.

`RuntimeAbiVersion`은 package version과 별개다. Runtime/helper ABI 판단은 [runtime-abi.md](runtime-abi.md)의 ABI change rules를 따른다.

## Breaking Change Policy

Breaking change로 간주하는 항목:

- stable grammar source가 더 이상 parse/check/build되지 않는다.
- existing diagnostic code, severity, or JSON field meaning changes in a way that tools cannot consume compatibly.
- `typesharp` command, option, exit code, or manifest key meaning changes incompatibly.
- generated `net48` assembly public metadata shape changes for the same TypeSharp source.
- `TypeSharp.Core` or `TypeSharp.Runtime` public type/member signature changes incompatibly.
- generated assembly deployment shape changes for ASP.NET/WCF/worker-style .NET Framework hosts.
- C# interop resolution rules change in a way that selects a different overload or changes nullability/byref behavior for existing valid code.

Required gate for every breaking change:

- task packet names the changed surface, reason, migration path, and verification.
- [traceability.md](traceability.md) records the new evidence or changed boundary.
- [migration-guide.md](migration-guide.md) or the release note has a migration section.
- public ABI changes review [runtime-abi.md](runtime-abi.md) and update `RuntimeAbiVersion` when required.
- release notes include a `Breaking Changes` section even when the section says "None".

## Preview Feature Gate

Stable releases cannot silently include preview behavior.

- Every feature must be classified in [feature-map.md](feature-map.md) as `MVP`, `Stable Backlog`, `Preview Watch`, `Experimental`, or `Rejected`.
- `Preview Watch` and `Experimental` features cannot be documented as stable CLI/compiler behavior.
- Any future parser, checker, lowering, runtime helper, or CLI path for preview syntax must require explicit opt-in through `language.previewFeatures` in `TypeSharp.toml`, `--preview` CLI override, or an equivalent documented gate before the feature is usable in stable mode.
- A preview feature diagnostic must tell the user which feature name to enable and which document explains the current instability.
- Promotion from preview to stable requires grammar docs, feature-map status update, regression evidence, traceability update, and release note entry.

Current implementation has a manifest surface for `language.previewFeatures`; no currently emitted feature is classified as `Preview Watch` and enabled by default.

## Package Signing Or Checksum Policy

Every release must provide an integrity story before distribution.

- Release artifacts must have SHA-256 checksums in a release manifest such as `checksums.txt`.
- NuGet packages, VSIX files, zip archives, and standalone CLI binaries must be covered by the checksum manifest.
- If signing infrastructure is available, NuGet/package signing or Authenticode signing can be added, but checksum coverage is the minimum release gate.
- Generated user assemblies are not signed by TypeSharp by default. Strong naming or signing generated assemblies is a separate user/project setting.
- External package support cannot ship until dependency version, license, transitive dependency, lock/checksum, and deployment shape are recorded in [dependencies.md](dependencies.md).
- The release note must name the checksum manifest and whether packages are signed.

## Security Policy

TypeSharp release security defaults:

- The compiler must not execute arbitrary user code during parse, check, build, or generated project scaffold emission.
- Source generator, plugin, macro, type provider, network schema import, and build-time external execution features are disabled by default until a sandbox/cache/permission policy exists.
- Diagnostics and logs should avoid embedding secrets from environment variables, connection strings, or local credential files.
- New dependencies require license and vulnerability review before release.
- Generated projects must remain offline-friendly by default and must not add package sources implicitly.
- Security-affecting fixes are called out in release notes and should be backported to supported stable lines when practical.

## Compatibility Matrix

Each release must publish the compatibility matrix it claims. The current baseline matrix is:

| Surface | Baseline | Required evidence |
| --- | --- | --- |
| Generated assembly | `net48`, C# 7.3-compatible source backend | generated `net48` build smoke and public ABI metadata smoke |
| `TypeSharp.Core` | `net48`, no external NuGet dependency | `dotnet build` and dependency audit smoke |
| `TypeSharp.Runtime` | `net48`, no external NuGet dependency | `dotnet build`, runtime helper smoke, dependency audit smoke |
| C# consumer | SDK-style `net48` C# project | C# consumer compile smoke |
| ASP.NET/WCF/worker host | .NET Framework host references generated/runtime/core DLLs like normal libraries | host compatibility smoke |
| Compiler/CLI host | modern .NET host allowed by [feasibility.md](feasibility.md) | compiler/CLI smoke suite |
| VS Code/LSP | editor features over compiler semantic model | LSP diagnostics/hover/definition/completion smokes |
| Dependency deployment | package-free generated/runtime/core baseline | [dependencies.md](dependencies.md) inventory and audit |

If a release changes a baseline, the release note must name the new baseline and the migration impact.

## Release Notes Template

Every release note uses this structure:

```text
# TypeSharp <version>

Date:
Channel:
Runtime ABI:
Default target framework:

## Summary

## Compatibility Matrix

## Breaking Changes

## Migration Notes

## Stable Features

## Preview Features

## Diagnostics And Tooling

## Security

## Checksums And Signing

## Verification
```

The `Breaking Changes`, `Preview Features`, `Security`, `Checksums And Signing`, and `Verification` sections are mandatory. Use `None` when there is nothing to report.

## Release Checklist

Before publishing:

- version constants and release notes agree.
- runtime ABI constants are aligned.
- release notes include breaking changes, migration notes, preview features, security notes, compatibility matrix, and checksums/signing status.
- `dotnet build src\TypeSharp.Core\TypeSharp.Core.csproj` passes.
- `dotnet build src\TypeSharp.Runtime\TypeSharp.Runtime.csproj` passes.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj` passes.
- docs-only release policy changes are linked from [README.md](README.md), [checklist.md](checklist.md), [traceability.md](traceability.md), and a task packet.
- release artifact checksums are generated before distribution.
