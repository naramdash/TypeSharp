# TypeSharp CLI

문서 기준일: 2026-05-18

이 문서는 TypeSharp CLI의 초기 command surface와 동작 계약을 정의한다. CLI는 부가 도구가 아니라 TypeSharp 개발 루프의 1급 산출물이며, VS Code extension과 같은 compiler core, project manifest, diagnostics model을 공유해야 한다.

## 목표

- `.tysh` 파일을 CLI만으로 검사, 빌드, 실행할 수 있게 한다.
- CI 환경에서 재현 가능한 diagnostics와 exit code를 제공한다.
- VS Code language server와 같은 parser, binder, type checker, diagnostics code를 사용한다.
- .NET Framework 4.8.1 산출물을 만들 수 있는 프로젝트 단위를 표준화한다.
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
Target default net481
```

규칙:
- script나 CI가 읽을 수 있도록 `--json`을 지원한다.
- `version`은 project load 없이 실행되어야 한다.

### `typesharp new`

새 프로젝트 골격을 만든다.

```text
typesharp new console HelloApp
typesharp new library Billing.Core --target net481
```

MVP template:
- `console`
- `library`

생성 파일:
- `TypeSharp.toml`
- `src/Main.tysh` 또는 `src/Library.tysh`
- `.gitignore`

규칙:
- 기본 target framework는 `net481`이다.
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

소스를 검사한 뒤 `net481` 실행 파일 또는 라이브러리 산출물을 만든다.

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

### `typesharp run`

실행 가능한 프로젝트를 빌드하고 실행한다.

```text
typesharp run
typesharp run --configuration Debug -- Alice
```

규칙:
- `outputType = "exe"` 프로젝트에서만 동작한다.
- `--` 뒤의 값은 TypeSharp 프로그램의 `main(args: string[])`로 전달한다.
- 실행 전 build가 실패하면 프로그램을 실행하지 않는다.

## Stable Backlog Command

```text
typesharp format [project-or-path] [--check]
typesharp explain <diagnostic-code>
typesharp lsp
typesharp test [project]
```

### `typesharp format`

공식 formatting convention에 맞게 `.tysh` 파일을 정렬한다.

```text
typesharp format
typesharp format src/Main.tysh
typesharp format --check
```

규칙:
- formatter는 semicolon을 출력하지 않는다.
- `namespace`, `import`/`open`, declaration 순서를 유지한다.
- pipeline과 match expression은 multiline 가독성을 우선한다.

### `typesharp explain`

diagnostic code의 의미와 해결 방향을 설명한다. 출력 내용은 [diagnostics.md](diagnostics.md)의 descriptor metadata를 사용한다.

```text
typesharp explain TS1001
typesharp explain TS2204 --json
```

권장 출력:
- diagnostic title
- 원인
- 예시
- 수정 방향
- 관련 문서 링크

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
| `--target net481` | target framework override |
| `--emit csharp|il` | backend 선택. MVP 기본값은 `csharp`, `il`은 Stable Backlog |
| `--diagnostic-format text|json` | diagnostics 출력 형식 |
| `--warnings-as-errors` | warning을 error로 승격 |
| `--no-color` | ANSI color 비활성화 |
| `--verbosity quiet|minimal|normal|diagnostic` | 로그 상세도 |
| `--preview` | preview feature 허용 |

규칙:
- CLI option은 manifest option을 override할 수 있지만, override 사실을 diagnostics/log에 남겨야 한다.
- CI 친화성을 위해 `--diagnostic-format json`과 `--no-color`는 모든 MVP command에서 동작해야 한다.
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
targetFramework = "net481"
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
- `bin/`, `obj/`, generated output root는 기본 discovery에서 제외한다.
- manifest의 target framework 기본값은 `net481`이다.
- `generatedOutputRoot`를 생략하면 `obj/generated`를 기본값으로 사용한다.
- `main`은 executable project에서만 필수다.
- `assemblies`는 framework/GAC/reference assembly 이름이다.
- `paths`는 명시 local DLL reference이며 `net481` 호환성을 검사해야 한다.
- `packages`는 NuGet reference를 위한 manifest 표면이지만, MVP에서는 restore 구현 없이 diagnostic으로 제한할 수 있다.
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
8. `typesharp format`
9. `typesharp explain`
10. `typesharp lsp`

## 열린 결정

- 직접 IL emit backend를 언제 `--emit il`로 공개할지 결정해야 한다.
- `TypeSharp.toml`을 장기 manifest로 유지할지, MSBuild project와 1급 통합할지 결정해야 한다.
- `typesharp test`가 별도 test framework를 실행할지, 외부 .NET test runner로 위임할지 결정해야 한다.
- `typesharp lsp`를 공개 command로 둘지, VS Code extension 내부 command로 숨길지 결정해야 한다.
