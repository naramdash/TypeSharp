# TypeSharp CLI

문서 기준일: 2026-05-19

이 문서는 TypeSharp CLI의 초기 command surface와 동작 계약을 정의한다. CLI는 부가 도구가 아니라 TypeSharp 개발 루프의 1급 산출물이며, VS Code extension과 같은 compiler core, project manifest, diagnostics model을 공유해야 한다.

## 목표

- `.tysh` 파일을 CLI만으로 검사, 빌드, 실행할 수 있게 한다.
- CI 환경에서 재현 가능한 diagnostics와 exit code를 제공한다.
- VS Code language server와 같은 parser, binder, type checker, diagnostics code를 사용한다.
- .NET Framework 4.8 산출물을 만들 수 있는 프로젝트 단위를 표준화한다.
- 사용자가 오류를 만났을 때 `typesharp explain`으로 다음 행동을 알 수 있게 한다.

## Command 등급

| 등급 | 의미 |
| --- | --- |
| MVP | 첫 구현에서 제공해야 하는 명령 |
| Stable Backlog | 안정 기능이지만 MVP 이후 구현 가능 |
| Experimental | feature gate 또는 내부 도구로 시작 |

## MVP Command Surface

```text
typesharp version
typesharp new <template> <name> [options]
typesharp check [project] [options]
typesharp build [project] [options]
typesharp run [project] [-- args...]
typesharp explain <diagnostic-code> [--json]
typesharp format [project-or-path] [--check]
```

### `typesharp version`

CLI, compiler, language, runtime ABI 버전을 출력한다.

```text
typesharp version
```

권장 출력:

```text
TypeSharp CLI 0.1.0-preview
Compiler 0.1.0-preview
Language preview
Runtime ABI 0
Target default net48
```

규칙:
- script나 CI가 읽을 수 있도록 `--json`을 지원한다.
- `version`은 project load 없이 실행되어야 한다.

### `typesharp new`

새 프로젝트 골격을 만든다.

```text
typesharp new console HelloApp
typesharp new library Billing.Core --target net48
```

MVP template:
- `console`
- `library`

생성 파일:
- `TypeSharp.toml`
- `src/Main.tysh` 또는 `src/Library.tysh`
- `.gitignore`

규칙:
- 기본 target framework는 `net48`이다.
- template은 preview feature를 기본으로 켜지 않는다.
- 생성되는 `.tysh` 코드는 [grammar/consistency.md](grammar/consistency.md)의 문법 일관성 규칙을 따라야 한다.

### `typesharp check`

소스를 파싱, 바인딩, 타입 검사하고 diagnostics를 출력한다. 산출물은 만들지 않는다.

```text
typesharp check
typesharp check ./TypeSharp.toml
typesharp check --diagnostic-format json
```

필수 동작:
- source discovery
- manifest validation
- parse diagnostics
- name resolution diagnostics
- type diagnostics
- public ABI diagnostics
- feature gate diagnostics

규칙:
- `check`는 VS Code diagnostics와 같은 diagnostic code를 사용한다.
- diagnostic code taxonomy와 explanation metadata는 [diagnostics.md](diagnostics.md)를 따른다.
- generated output을 만들지 않아야 한다.
- CI에서는 `--warnings-as-errors`를 사용할 수 있어야 한다.

### `typesharp build`

소스를 검사한 뒤 `net48` 실행 파일 또는 라이브러리 산출물을 만든다.

```text
typesharp build
typesharp build --configuration Release
typesharp build --emit csharp
```

MVP backend:
- `--emit csharp`: C# 7.3 compatible source backend

Stable Backlog backend:
- `--emit il`: 직접 IL backend

규칙:
- backend 선택과 상관없이 public API diagnostics는 동일해야 한다.
- output path는 manifest와 CLI option이 충돌하지 않게 결정한다.
- `build`는 `check`와 같은 diagnostics를 먼저 통과해야 한다.
- MVP에서 `--emit`을 생략하면 `csharp`를 기본값으로 사용한다.
- 현재 `--configuration`은 `Debug` 또는 `Release`만 허용하며 generated SDK-style C# project의 build configuration과 reported assembly path를 결정한다.
- 현재 `--target`은 `net48`만 허용하며 manifest `targetFramework`보다 우선한다.
- 현재 `--verbosity quiet`은 성공 artifact log를 숨기고, `minimal`은 final assembly path만 출력하며, `normal`은 generated source/project/assembly path를 출력한다. `diagnostic`은 `normal` 출력에 build option 요약을 추가한다.

### `typesharp run`

실행 가능한 프로젝트를 빌드하고 실행한다.

```text
typesharp run
typesharp run --configuration Debug -- Alice
```

규칙:
- `outputType = "exe"` 프로젝트에서만 동작한다.
- `--` 뒤의 값은 TypeSharp 프로그램의 `main(args: string[])`로 전달한다.
- executable `main`은 현재 `main()` 또는 `main(args: string[])` 형태여야 하며, 이 외 signature는 `TS3500`으로 보고한다.
- 실행 전 build가 실패하면 프로그램을 실행하지 않는다.
- 현재 `--configuration`은 build와 동일하게 generated executable의 `bin/<Configuration>/net48` output path를 선택한다.
- 현재 `--target`은 build와 동일하게 generated executable project의 target framework와 output path를 선택한다.

현재 구현 메모:
- 초기 smoke path는 `main(): string`, `main(): int`, `main(args: string[]): string`, `main(args: string[]): int` 형태를 generated C# entry point로 감싸 실행한다.
- `int` return은 process exit code가 되고, non-null non-`int` return은 stdout에 출력된다.
- async main과 richer return handling은 후속 구현 범위다.

### `typesharp explain`

diagnostic code의 의미와 해결 방향을 설명한다. 출력 내용은 [diagnostics.md](diagnostics.md)의 descriptor metadata를 사용한다.

```text
typesharp explain TS1001
typesharp explain TS2204 --json
typesharp explain TS2204 --diagnostic-format json
```

현재 출력:
- code
- title
- severity
- category
- message template
- explanation
- suggested action

규칙:
- code lookup은 대소문자를 구분하지 않는다.
- descriptor registry에 없는 code는 exit code `1`을 반환한다.
- 잘못된 option 또는 누락된 code는 exit code `2`를 반환한다.
- `--json`과 `--diagnostic-format json`은 같은 JSON descriptor payload를 출력한다.

## Formatting Command

```text
typesharp format [project-or-path] [--check]
```

### `typesharp format`

공식 formatting convention에 맞게 `.tysh` 파일을 정렬한다.

```text
typesharp format
typesharp format src/Main.tysh
typesharp format --check
```

규칙:
- formatter convention은 [formatting.md](formatting.md)를 따른다.
- 현재 formatter MVP는 parser-clean `.tysh` 파일만 rewrite한다.
- 현재 rewrite 범위는 LF line ending, trailing whitespace 제거, 연속 blank line 하나로 축소, 최종 newline 보장이다.
- `namespace`, `import`/`open`, declaration 순서를 유지한다.
- pipeline과 match expression은 현재 재배치하지 않으며, canonical layout은 [formatting.md](formatting.md)의 후속 formatter 확장 기준이다.
- parse diagnostics가 있는 파일은 rewrite하지 않는다.
- `--check`는 파일을 쓰지 않고 format diff가 있으면 non-zero exit code를 반환한다.

## Stable Backlog Command

```text
typesharp lsp
typesharp test [project]
```

### `typesharp lsp`

VS Code extension이 시작하는 Language Server Protocol entrypoint다.

규칙:
- 사용자가 직접 실행할 수도 있지만 기본 대상은 editor integration이다.
- CLI와 language server는 compiler core를 공유해야 한다.
- diagnostics code, source span, semantic model이 `check`와 일치해야 한다.

## 공통 Option

| Option | 의미 |
| --- | --- |
| `--project <path>` | project manifest 경로 지정 |
| `--configuration Debug|Release` | 빌드 설정 |
| `--target net48` | target framework override |
| `--emit csharp|il` | backend 선택. MVP 기본값은 `csharp`, `il`은 Stable Backlog |
| `--diagnostic-format text|json` | diagnostics 출력 형식 |
| `--warnings-as-errors` | warning을 error로 승격 |
| `--no-color` | ANSI color 비활성화 |
| `--verbosity quiet|minimal|normal|diagnostic` | 로그 상세도 |
| `--preview` | preview feature 허용 |

규칙:
- CLI option은 manifest option을 override할 수 있지만, override 사실을 diagnostics/log에 남겨야 한다.
- CI 친화성을 위해 `--diagnostic-format json`과 `--no-color`는 모든 MVP command에서 동작해야 한다.
- `--verbosity`는 현재 project command에서 검증되며 build success log 상세도를 제어한다.
- 알 수 없는 project command option은 usage 오류로 중단한다.
- `--preview`는 현재 project command에서 인식되며 후속 preview feature gate와 연결할 reserved option이다.
- `--preview` 없이 preview 문법을 사용하면 diagnostic을 낸다.

## Exit Code

| Code | 의미 |
| --- | --- |
| `0` | 성공 |
| `1` | 사용자 코드 또는 project diagnostics 때문에 실패 |
| `2` | CLI 사용법 오류 |
| `3` | compiler internal error |
| `4` | 환경 오류, target framework 또는 toolchain 누락 |
| `5` | 지원하지 않는 target/backend/feature 조합 |

규칙:
- warning만 있으면 기본 exit code는 `0`이다.
- `--warnings-as-errors`가 켜져 있으면 warning도 exit code `1`을 만든다.
- internal error는 사용자 코드 diagnostic과 분리한다.

## Project Manifest

초기 manifest 파일 이름은 `TypeSharp.toml`로 둔다.

```toml
[project]
name = "HelloApp"
targetFramework = "net48"
outputType = "exe"
rootNamespace = "HelloApp"
sourceRoots = ["src"]
generatedOutputRoot = "obj/generated"
main = "HelloApp.main"

[language]
version = "preview"
strict = true
nullable = "strict"
previewFeatures = []

[references]
assemblies = [
  "System",
  "System.Core"
]
paths = []
packages = []

[tooling]
diagnosticFormat = "text"
treatWarningsAsErrors = false
```

규칙:
- `sourceRoots` 아래의 `*.tysh` 파일을 source file로 찾는다.
- 각 source file은 source root 상대 경로에서 `.tysh` 확장자를 제거한 module path를 가진다.
- 여러 source root에서 같은 module path가 나오면 source module graph가 모호하므로 `TS0111` error를 보고한다.
- relative source module specifier가 target module path를 찾지 못하면 `TS0112` error를 보고한다.
- unaliased relative named source import는 target module의 generated C# container를 `using static`으로 가져오고, relative namespace import는 generated container alias로 낮춘다.
- 아직 지원하지 않는 relative source import form, 예를 들어 named source import alias는 `TS0113` error로 emission 전에 중단한다.
- `bin/`, `obj/`, generated output root는 기본 discovery에서 제외한다.
- manifest의 target framework 기본값은 `net48`이다.
- `generatedOutputRoot`를 생략하면 `obj/generated`를 기본값으로 사용한다.
- `main`은 executable project에서만 필수다.
- `assemblies`는 framework/GAC/reference assembly 이름이다.
- `paths`는 명시 local DLL reference이며 `net48` 호환성을 검사해야 한다.
- `packages`는 NuGet reference를 위한 manifest 표면이지만, 현재 compiler는 restore를 수행하지 않고 `TS2405` diagnostic으로 제한한다.
- MSBuild 통합은 Stable Backlog로 두되, manifest 의미와 충돌하지 않아야 한다.

## Diagnostics Format

Diagnostic code range와 descriptor metadata는 [diagnostics.md](diagnostics.md)에 고정한다.

Text 형식:

```text
src/Main.tysh(8,15): error TS2204: Type-level union cannot appear in public API. Use a nominal union or interface.
```

JSON 형식:

```json
{
  "diagnostics": [
    {
      "code": "TS2204",
      "severity": "error",
      "message": "Type-level union cannot appear in public API. Use a nominal union or interface.",
      "file": "src/Main.tysh",
      "start": { "line": 8, "column": 15 },
      "end": { "line": 8, "column": 33 }
    }
  ]
}
```

규칙:
- code는 안정적인 문자열이어야 한다.
- source span은 1-based line/column으로 출력한다.
- JSON diagnostics는 VS Code/LSP diagnostics로 손실 없이 변환 가능해야 한다.

## Source Discovery

기본 규칙:
- project 인자가 없으면 현재 디렉터리에서 `TypeSharp.toml`을 찾는다.
- 현재 디렉터리에 없으면 부모 디렉터리로 올라가며 찾는다.
- `sourceRoots`가 없으면 `src`를 기본 source root로 사용한다.
- source file 확장자는 `.tysh`다.
- discovered source file은 project-relative path와 source-root-relative module path를 함께 보존한다.
- source-root-relative module path는 대소문자 차이만으로 구분하지 않는다.
- `./`와 `../` source module specifier는 현재 file의 module path directory를 기준으로 해석한다.

제외 기본값:
- `bin`
- `obj`
- `.git`
- generated output root

## 구현 순서

1. `typesharp version`
2. manifest parser와 source discovery
3. `typesharp check`
4. diagnostics text/json 출력
5. `typesharp build --emit csharp`
6. `typesharp run`
7. `typesharp new`
8. `typesharp explain`
9. `typesharp format`
10. `typesharp lsp`

## 후속 확장 결정

- 직접 IL emit backend 구현과 `--emit il` 공개는 Stable Backlog다.
- `TypeSharp.toml`은 현재 manifest 기준이다. MSBuild 1급 통합은 후속 확장이다.
- `typesharp test`가 별도 test framework를 실행할지, 외부 .NET test runner로 위임할지는 후속 tooling 확장이다.
- `typesharp lsp` 공개 command와 VS Code extension 내부 command packaging은 후속 배포 정책에서 결정한다.
