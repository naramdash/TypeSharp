# Migration Guide

문서 기준일: 2026-05-19

이 문서는 기존 .NET Framework 4.8 C# 코드베이스가 TypeSharp를 점진적으로 도입하는 방법을 설명한다. 현재 범위는 "기존 프로젝트를 자동 변환한다"가 아니라, TypeSharp로 작성한 새 `net48` library를 기존 C#/.NET Framework 애플리케이션에서 참조하고, 기존 C# assembly를 TypeSharp에서 호출하는 점진적 채택이다.

## 대상

TypeSharp가 적합한 경우:
- 기존 시스템이 .NET Framework 4.8에 남아 있어야 한다.
- ASP.NET Web Forms/MVC/Web API, WCF, Windows Service, scheduled job, worker-style host가 기존 배포 모델을 유지해야 한다.
- 새 코드에서는 null safety, nominal union, pattern matching, immutable record, `Option<T>`/`Result<T,E>`, type-level union, structural shape check 같은 현대적 타입 기능을 쓰고 싶다.
- C# `net48` library, framework assembly, local DLL을 계속 호출해야 한다.
- C# 소비자가 TypeSharp generated assembly를 일반 class library처럼 참조해야 한다.

TypeSharp가 아직 적합하지 않은 경우:
- 기존 C# 프로젝트 전체를 자동 변환해야 한다.
- NuGet restore, package lock, package license inventory까지 TypeSharp manifest가 직접 관리해야 한다.
- TypeSharp source에서 ASP.NET/WCF host template과 config를 자동 생성해야 한다.
- 직접 IL backend나 release packaging/signing policy가 필요하다.

## Migration Strategy

권장 순서:

1. 기존 C# solution 안에 TypeSharp library project를 별도 폴더로 둔다.
2. TypeSharp library는 `targetFramework = "net48"`로 시작한다.
3. 기존 C# DLL이나 framework assembly는 `TypeSharp.toml`의 `[references]`에 명시한다.
4. public API는 C# 소비자가 이해하는 nominal type으로 닫는다.
5. local TypeSharp code 안에서는 structural shape, type-level union, null safety, pattern matching을 사용한다.
6. `typesharp check`로 diagnostics를 먼저 고정한다.
7. `typesharp build`로 generated `net48` DLL을 만든다.
8. 기존 C# project에서 generated DLL과 필요한 `TypeSharp.Core`/`TypeSharp.Runtime` DLL을 `<Reference>`로 참조한다.
9. C# consumer build가 통과하면 해당 API를 기존 application model에 연결한다.

## Minimal Project Shape

`TypeSharp.toml`:

```toml
[project]
name = "Billing.Rules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing.Rules"
sourceRoot = "src"
generatedOutputRoot = "generated"

[references]
assemblies = [
  "System",
  "System.Core"
]

paths = [
  "lib/Legacy.Billing.dll"
]
```

`src/Main.tysh`:

```tysh
namespace Company.Billing.Rules

import { LegacyCalculator } from "Legacy.Billing"
import { Result, Ok, Error } from "TypeSharp.Core"

public union PriceError {
  MissingSku(sku: string)
  InvalidAmount(message: string)
}

public record PriceQuote(Sku: string, Amount: decimal)

export fun quote(sku: string): Result<PriceQuote, PriceError> {
  let amount = LegacyCalculator.Calculate(sku)
  Ok(PriceQuote(sku, amount))
}
```

CLI loop:

```text
typesharp check TypeSharp.toml --diagnostic-format json
typesharp build TypeSharp.toml
```

Generated output is documented in [lowering.md](lowering.md), and CLI behavior is documented in [cli.md](cli.md).

## Public API Rules

Use public API shapes that C# consumers can call predictably.

Recommended:
- `record` for immutable data crossing into C#.
- nominal `union` for closed domain alternatives.
- `class`, `interface`, and `delegate` for C#-friendly object models.
- `Option<T>` and `Result<T,E>` from `TypeSharp.Core` for explicit optional/error values.
- `Task<T>` for async public APIs.
- explicit parameter and return type annotations on exported functions.

Avoid in public API:
- type-level union aliases such as `string | int`
- structural shape aliases such as `{ Name: string }`
- anonymous object/record expressions
- marker-free `dynamic`
- inferred anonymous function types

Reason:
- these features are TypeSharp compile-time tools and do not have stable CLR metadata representation in the MVP.
- the type checker reports public boundary diagnostics when compile-time-only types leak into exported surfaces.

See:
- [csharp-interop.md](csharp-interop.md)
- [runtime-abi.md](runtime-abi.md)
- [lowering.md](lowering.md)

## C# Pattern Mapping

| Existing C# pattern | TypeSharp replacement | Notes |
| --- | --- | --- |
| DTO class with readonly properties | `record` | Lowers to immutable C# class with constructor and get-only properties. |
| Nullable return for absence | `Option<T>` or `T?` | Prefer `Option<T>` for domain absence, `T?` for interop-compatible nullability. |
| Exception for expected domain failure | `Result<T,E>` | Keep C# exception interop for external APIs; model new TypeSharp domain errors as values. |
| enum plus payload class hierarchy | nominal `union` | Lowers to abstract base class and sealed case classes. |
| `switch` over cases | `match` | Type checker reports non-exhaustive union matches. |
| ad hoc anonymous shape checks | structural shape alias | Local compile-time check only; do not expose publicly. |
| overload accepting unrelated primitive alternatives | type-level union alias | Local code only; publish nominal wrapper APIs for C# callers. |
| async method returning `Task<T>` | `async fun ...: Task<T>` | Lowers to C# `async Task<T>` and C# `await`. |
| static helper class | `module` or top-level exported functions | Lowers to generated static `Module` class or named module static class. |

## Calling Existing C# Libraries

Use manifest references for external metadata:

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Tools.dll"]
```

TypeSharp source:

```tysh
namespace Company.Tools

import { Regex } from "System.Text.RegularExpressions"
import { LegacyFormatter } from "Legacy.Tools"

export fun normalize(value: string): string {
  let formatter = LegacyFormatter("legacy:")
  formatter.Format(value)
}

export fun isSlug(value: string): bool =
  Regex.IsMatch(value, "^[a-z0-9-]+$")
```

Implemented interop coverage includes:
- framework assembly reference
- local DLL reference
- constructor/static/instance member call
- property access
- `params`, `out`, `in`, `ref`
- optional and named arguments
- delegate lambda calls
- event add/remove calls
- nullable metadata warning for unknown C# nullability
- ambiguous overload diagnostics

See [csharp-interop.md](csharp-interop.md) for exact rules.

## Consuming TypeSharp From C#

C# `net48` project reference shape:

```xml
<ItemGroup>
  <Reference Include="Billing.Rules">
    <HintPath>..\Billing.Rules\generated\bin\Debug\net48\Billing.Rules.dll</HintPath>
  </Reference>
  <Reference Include="TypeSharp.Core">
    <HintPath>..\Billing.Rules\lib\TypeSharp.Core.dll</HintPath>
  </Reference>
  <Reference Include="TypeSharp.Runtime">
    <HintPath>..\Billing.Rules\lib\TypeSharp.Runtime.dll</HintPath>
  </Reference>
</ItemGroup>
```

Rules:
- reference the generated TypeSharp assembly like any other `net48` class library.
- include `TypeSharp.Core.dll` when public APIs expose `Option<T>`, `Result<T,E>`, or `Unit`.
- include `TypeSharp.Runtime.dll` when generated code uses runtime helpers such as nominal union metadata or pattern matching.
- avoid depending on generated internal helper names unless they are documented public ABI.

The current test suite verifies C# `net48` consumers and ASP.NET/WCF/worker-style host references.

## Nullability Migration

TypeSharp source is non-null by default for reference-like types.

Recommended steps:
- annotate existing C# libraries with nullable metadata when possible.
- treat unannotated C# reference returns as unknown nullability.
- add null guards or nullable TypeSharp annotations at interop boundaries.
- prefer `Option<T>` for new TypeSharp APIs where absence is part of the domain.

Current diagnostics:
- `TS2202` reports null or nullable values flowing into non-null TypeSharp positions.
- `TS2404` warns when imported C# nullability is unknown.

## Error Model Migration

Use `Result<T,E>` for expected domain failure:

```tysh
public union ParseError {
  Empty
  Invalid(message: string)
}

export fun parse(text: string): Result<int, ParseError> =
  if text == "" {
    Error(Empty)
  }
  else {
    Ok(42)
  }
```

Keep exceptions for:
- existing C# APIs that already throw
- infrastructure failures
- boundaries where C# callers expect exceptions

Exception-to-`Result` helper policy remains future work.

## ASP.NET, WCF, Worker Hosts

Current migration stance:
- TypeSharp generates libraries that existing .NET Framework hosts can reference.
- It does not replace ASP.NET/WCF/Windows Service hosting or configuration systems.
- Host-specific project templates, `web.config`, WCF endpoint generation, service installer scaffolding, and IIS packaging remain Stable Backlog.

Validated compatibility shape:
- ASP.NET Web Forms-style `System.Web.UI.Page` project references generated TypeSharp/Core/Runtime assemblies.
- WCF service/client contract project references generated TypeSharp/Core/Runtime assemblies.
- Windows Service-style `ServiceBase` project references generated TypeSharp/Core/Runtime assemblies.

## Unsupported Automation

Not currently provided:
- automatic C# syntax conversion to TypeSharp
- automatic `.csproj` to `TypeSharp.toml` conversion
- NuGet dependency restore through TypeSharp CLI
- generated ASP.NET/WCF/worker templates
- direct IL backend
- binary-compatible public ABI snapshot tooling
- release signing/checksum pipeline

These belong to later migration tooling or release readiness work.

## Adoption Checklist

- [ ] Identify one low-risk C# library boundary.
- [ ] Create a TypeSharp library project with `targetFramework = "net48"`.
- [ ] Add required framework/local DLL references to `TypeSharp.toml`.
- [ ] Write TypeSharp source with nominal public API types.
- [ ] Run `typesharp check`.
- [ ] Fix public boundary, nullability, overload, and byref diagnostics.
- [ ] Run `typesharp build`.
- [ ] Reference generated DLL plus required TypeSharp Core/Runtime DLLs from a C# `net48` consumer.
- [ ] Add C# consumer build to CI.
- [ ] Keep structural/type-level flexibility local until an explicit C#-friendly wrapper is designed.
