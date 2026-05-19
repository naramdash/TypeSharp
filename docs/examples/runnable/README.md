# Runnable Example Project Catalog

문서 기준일: 2026-05-19

이 폴더는 현재 `typesharp` CLI와 smoke test로 검증하는 실행 가능한 예제 프로젝트 catalog다. 상위 폴더의 단일 `.tysh` 파일 예제는 문법과 설계 의도를 보여주고, 이 폴더의 프로젝트들은 `TypeSharp.toml`, source root, reference, generated output root, 검증 명령을 함께 가진다.

## Smoke Matrix

| Project | Scenario | Primary commands | Smoke evidence |
| --- | --- | --- | --- |
| [console-hello](console-hello/README.md) | `net48` console app and `typesharp run` workflow | `typesharp check`, `typesharp build`, `typesharp run` | copied to a temporary workspace and checked/built; run command is attempted with antivirus-aware executable launch handling |
| [library-public-api](library-public-api/README.md) | C#-friendly public record and class API | `typesharp check`, `typesharp build` | copied to a temporary workspace and built as a generated `net48` library |
| [csharp-interop](csharp-interop/README.md) | TypeSharp consuming an explicit local `net48` C# DLL | `dotnet build legacy-src`, `typesharp check`, `typesharp build` | local DLL is built into `lib/` and consumed by TypeSharp build |
| [host-aspnet-wcf](host-aspnet-wcf/README.md) | generated TypeSharp library referenced by ASP.NET Web Forms-style, WCF service, and WCF client/proxy-shaped `net48` host code | `typesharp build`, `dotnet build TypeSharp.Core/Runtime`, `dotnet build host` | generated library plus Core/Runtime dependencies are built, then ASP.NET/WCF host code references them with `System.Web`, `System.ServiceModel`, `ServiceContract`, and `ClientBase<T>` |
| [host-worker](host-worker/README.md) | generated TypeSharp library referenced by a worker-style `net48` host | `typesharp build`, `dotnet build TypeSharp.Core/Runtime`, `dotnet build host` | generated library plus Core/Runtime dependencies are built, then the host project references them |
| [diagnostics-null-safety](diagnostics-null-safety/README.md) | diagnostics/tooling workflow for null safety | `typesharp check --diagnostic-format json` | check is expected to fail with `TS2202` JSON diagnostics |

## Verification Contract

The smoke test `runnable example project commands are smoke-tested` copies this folder to `tests/tmp`, then executes the commands above against the copied projects so generated `bin/`, `obj/`, and `generated/` artifacts do not dirty the repository.

The console run smoke accepts a generated executable launch failure only when the CLI returns environment exit code `4` after successfully producing the executable. This keeps the example matrix useful on machines where antivirus blocks short-lived generated `net48` executables.
