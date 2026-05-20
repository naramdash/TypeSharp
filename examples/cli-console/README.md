# CLI Console Example

문서 기준일: 2026-05-18

이 폴더는 초기 CLI console workflow sketch다. 현재 smoke-tested runnable console project는 [../runnable/console-hello](../runnable/console-hello/README.md)에 있으며, 새 예제 검증은 [../runnable](../runnable/README.md) catalog를 기준으로 한다.

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
TS2204: Compile-time-only type cannot appear in public API.

Use a nominal union, nominal interface, or explicit wrapper at the public .NET boundary.
```
