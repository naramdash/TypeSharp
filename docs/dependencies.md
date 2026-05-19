# Dependency Inventory

문서 기준일: 2026-05-19

이 문서는 TypeSharp 산출물과 도구가 의존하는 외부 dependency를 추적한다. 기준은 [goal.md](goal.md)의 .NET Framework 4.8 호환성과 [requirements.md](requirements.md)의 dependency 추적 요구사항이다.

## 원칙

- Generated TypeSharp assembly와 `TypeSharp.Core`, `TypeSharp.Runtime`는 `net48` project에서 일반 class library처럼 참조될 수 있어야 한다.
- `TypeSharp.Core`와 `TypeSharp.Runtime`에는 현재 외부 NuGet dependency를 두지 않는다.
- compiler, CLI, test host는 현대 .NET 기반 실행을 허용하지만, generated artifact와 runtime/core surface의 `net48` 호환성을 깨면 안 된다.
- 새 NuGet dependency를 추가하려면 `net48` compatible asset, transitive dependency, license, checksum 또는 lock file 정책을 이 문서에 먼저 기록한다.

## 현재 Inventory

| Artifact | Target | External NuGet | License | Notes |
| --- | --- | --- | --- | --- |
| generated TypeSharp project | `net48` | None | N/A | `typesharp build`가 offline `NuGet.config` with cleared package sources를 생성한다. Manifest references는 framework assembly 또는 explicit local DLL reference다. |
| `TypeSharp.Core` | `net48` | None | N/A | User-facing `Option<T>`, `Result<T,E>`, `Unit` surface. |
| `TypeSharp.Runtime` | `net48` | None | N/A | Compiler-generated helper surface. 현재 host-specific framework dependency 없음. |
| `TypeSharp.Compiler` | `net10.0` host | None | N/A | Host-side compiler implementation. Generated/runtime artifact dependency가 아니다. |
| `TypeSharp.Cli` | `net10.0` host | None | N/A | Depends on `TypeSharp.Compiler` project reference only. |
| `TypeSharp.Compiler.Tests` | `net10.0` host | None | N/A | Project references and linked core sources only. |
| `docs-site` | Astro static site | `astro` 6.3.5, `@astrojs/starlight` 0.39.2, `typescript` 5.9.3 | MIT, MIT, Apache-2.0 | GitHub Pages documentation site only. Locked by `docs-site/package-lock.json`; not part of generated `net48` runtime deployment. |

## Compatibility Audit

현재 자동 감사는 `tests/TypeSharp.Compiler.Tests`에서 수행한다.

- `src/TypeSharp.Core/TypeSharp.Core.csproj` and `src/TypeSharp.Runtime/TypeSharp.Runtime.csproj` target `net48`.
- Core/Runtime artifacts do not use `PackageReference` or `packages.config`.
- Core/Runtime source is scanned for a small denylist of .NET 5+ or package-backed runtime APIs such as `DateOnly`, `TimeOnly`, `System.Text.Json`, `IAsyncEnumerable`, `ValueTask`, `Span<T>`, and `Task.WaitAsync`.
- Existing `dotnet build` on the `net48` projects remains the primary compatibility check; the static scan is a guardrail for obvious drift.
- `docs-site` dependencies are installed with `npm ci` from `package-lock.json`; `npm run build` is the docs site build smoke.

## Future Dependency Gate

When a dependency is proposed, record:

- package id and version
- direct or transitive role
- `net48` asset availability
- license
- runtime deployment shape for ASP.NET/WCF/worker hosts
- checksum, lock file, or signing strategy
- replacement or no-dependency alternative considered

Release artifact checksum/signing requirements are defined in [release.md](release.md).
