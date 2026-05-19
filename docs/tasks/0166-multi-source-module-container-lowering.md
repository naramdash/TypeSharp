# Task 0166: Multi-Source Module Container Lowering

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 18:03:53 +09:00
End Time: 2026-05-19 18:05:25 +09:00

## Objective

Make multiple TypeSharp source modules in the same generated C# namespace compile cleanly by giving multi-source projects deterministic module-path-based C# containers while preserving the existing single-source `Module` ABI.

## Scope

In:
- Keep single-source C# output using `public static class Module`.
- Use source module path based generated C# container names when a project has multiple source files.
- Update executable entry point generation to call the correct generated container for `main`.
- Add build/run smoke tests for multi-source projects in the same namespace.
- Update lowering/module/docs-site/checklist/traceability documentation.
- Commit and push when this task is completed.

Out:
- Source import binding or lowering.
- Export public surface filtering.
- User-configurable module container naming.
- Direct IL backend naming rules.

## Acceptance Criteria

- [x] Existing single-source backend output remains `Module`.
- [x] Multi-source build emits distinct module-path-based generated containers.
- [x] Multi-source same-namespace project builds a `net48` assembly.
- [x] Multi-source executable entry point invokes the generated container that contains `main`.
- [x] Documentation explains the single-source and multi-source container rules.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "module path container"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build emits generated C# source"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI run passes arguments"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 18:03:53 +09:00: Started after source module graph work exposed that multiple source files in one generated C# namespace need distinct containers before source import lowering can become practical.
- 2026-05-19 18:05:25 +09:00: Added parameterized C# module container emission, module-path-based containers for multi-source build/run, preserved single-source `Module`, and updated docs.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "module path container"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI build emits generated C# source"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI run passes arguments"`: passed.
- `npm run build` in `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
