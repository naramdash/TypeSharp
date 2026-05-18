# CLI Console Example

문서 기준일: 2026-05-18

이 폴더는 TypeSharp CLI가 목표로 하는 최소 console project 흐름을 보여준다. 아직 실제 CLI가 구현된 것은 아니며, 이 예제는 `typesharp new`, `typesharp check`, `typesharp build`, `typesharp run`의 기준 fixture로 승격될 수 있어야 한다.

## Project Structure

```text
cli-console/
  TypeSharp.toml
  src/
    Main.tysh
```

## New Project

```powershell
typesharp new console CliConsole --target net48
```

기대 결과:

```text
created TypeSharp.toml
created src/Main.tysh
```

## Check

```powershell
typesharp check
```

기대 결과:

```text
TypeSharp check succeeded.
0 errors, 0 warnings
```

JSON diagnostics 예:

```powershell
typesharp check --diagnostic-format json
```

```json
{
  "diagnostics": []
}
```

## Build

```powershell
typesharp build --configuration Debug --emit csharp
```

기대 결과:

```text
TypeSharp build succeeded.
Target framework: net48
Output: bin/Debug/net48/CliConsole.exe
```

## Run

```powershell
typesharp run -- Ada
```

기대 출력:

```text
Hello, Ada
HELLO, ADA
```

## Format Check

```powershell
typesharp format --check
```

기대 결과:

```text
All files are formatted.
```

## Diagnostic Explain

```powershell
typesharp explain TS2204
```

기대 출력:

```text
TS2204: Type-level union cannot appear in public API.

Use a nominal union, nominal interface, or explicit wrapper at the public .NET boundary.
```
