# Runtime ABI Policy

문서 기준일: 2026-05-19

이 문서는 `TypeSharp.Core`, `TypeSharp.Runtime`, generated `net48` assembly가 공유하는 public ABI versioning 정책을 정의한다. 목표는 기존 .NET Framework host와 C# 소비자가 TypeSharp 산출물을 일반 `net48` library처럼 참조하는 계약을 깨뜨리지 않는 것이다.

## Version Fields

현재 ABI 기준:

- `TypeSharp.Compiler.TypeSharpCompilerInfo.RuntimeAbiVersion = 0`
- `TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion = 0`

규칙:
- compiler와 runtime의 `RuntimeAbiVersion`은 항상 같은 값을 가져야 한다.
- CLI `version --json`과 text output은 compiler가 대상으로 삼는 runtime ABI를 보여준다.
- generated assembly는 같은 major runtime ABI를 가진 `TypeSharp.Core`와 `TypeSharp.Runtime`를 참조해야 한다.
- ABI 0은 pre-1.0 preview ABI다. breaking change가 허용되지만, task packet과 traceability에 기록해야 한다.

## Stable Surface

ABI 정책이 적용되는 surface:

- `TypeSharp.Core.Option<T>`, `Result<T, E>`, `Unit`
- `TypeSharp.Runtime.TypeSharpRuntimeInfo`
- generated-code helper surface in `TypeSharp.Runtime`
- generated public member signatures and metadata shape
- generated assembly target framework and runtime/core references

ABI 정책이 적용되지 않는 surface:

- compiler internal syntax tree and binder/type checker implementation details
- CLI internal command implementation
- test fixture helper code
- generated `.g.cs` formatting when public metadata is unchanged

## ABI Change Rules

Runtime ABI version must change when any of the following changes:

- a public type, method, property, field, constructor, or nested type is removed or renamed from `TypeSharp.Core` or `TypeSharp.Runtime`
- a public member signature changes in a binary-incompatible way
- generated public metadata shape changes for the same TypeSharp source
- generated code requires a new runtime helper that older runtime assemblies do not provide
- `net48` generated assembly reference shape changes in a way that existing hosts must deploy differently

Runtime ABI version does not need to change for:

- adding a new public helper that old generated code does not require
- fixing helper behavior without changing signatures or generated metadata shape
- improving compiler diagnostics, parsing, or CLI output unrelated to generated public metadata
- changing internal implementation details of generated code when public metadata remains compatible

## Compatibility Gates

Before closing a change that affects public ABI:

- `TypeSharpCompilerInfo.RuntimeAbiVersion` and `TypeSharpRuntimeInfo.RuntimeAbiVersion` must remain aligned.
- `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj` must pass.
- `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj` must pass.
- generated `net48` assembly smoke tests must pass.
- C# `net48` consumer smoke tests must pass when public surface can be observed from C#.
- ASP.NET/WCF/worker host compatibility smokes must pass when runtime/core reference shape changes.

## Release Policy

General release readiness, release notes, checksums, security, and compatibility matrix policy lives in [release.md](release.md). This section only defines runtime/public ABI release rules.

Before TypeSharp 1.0:

- ABI version remains `0` unless generated assemblies need an incompatible runtime helper contract that must be called out explicitly.
- breaking ABI changes are allowed only with task packet documentation and traceability updates.
- migration guidance can be short, but the affected public surface must be named.

At TypeSharp 1.0 and later:

- ABI version increments on breaking runtime/generated metadata changes.
- compatible helper additions do not increment ABI version.
- breaking changes require release notes and migration guidance.
