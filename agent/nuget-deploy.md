# TypeSharp NuGet Deploy Guide

문서 기준일: 2026-05-26

TypeSharp CLI/runtime 배포 기조는 `TypeSharp.Tool` NuGet global tool입니다. 현재 repository workflow는 `.nupkg` 생성과 local install smoke까지만 수행하고, NuGet.org 계정 등록과 실제 publish는 자동화하지 않습니다.

## 배포자가 먼저 해야 할 일

1. NuGet.org 계정 준비
   - NuGet.org 계정을 만들고 2FA를 켭니다.
   - TypeSharp를 소유한 개인 계정 또는 organization 계정을 정합니다.
   - 패키지 소유권을 개인에게만 묶지 않도록 가능한 경우 organization/team ownership을 사용합니다.

2. 패키지 ID 확인
   - 공식 패키지 ID는 `TypeSharp.Tool`입니다.
   - 패키지 설명은 modern .NET에서 실행되는 CLI이며, 생성 산출물과 bundled Core/Runtime DLL은 `net48`이라는 점을 유지합니다.
   - 첫 publish 전에 같은 ID가 이미 점유되어 있지 않은지 확인합니다.

3. API key 정책 결정
   - 첫 배포는 수동으로 수행합니다.
   - 자동화를 켤 때는 NuGet.org에서 `TypeSharp.Tool` publish 권한만 가진 scoped API key를 만듭니다.
   - key는 GitHub Actions secret `NUGET_API_KEY`에 넣는 방향이지만, 이 repository에는 아직 자동 push 단계를 두지 않습니다.

4. 첫 패키지 검증
   - release workflow 또는 로컬에서 생성된 `TypeSharp.Tool.<version>.nupkg`를 사용합니다.
   - 패키지 내부에 다음 파일이 있는지 확인합니다.

```text
tools/net10.0/any/typesharp.dll
tools/net10.0/any/runtime/net48/TypeSharp.Core.dll
tools/net10.0/any/runtime/net48/TypeSharp.Runtime.dll
```

5. 로컬 tool install smoke

```powershell
$version = "0.1.0-preview.5"
$source = Resolve-Path ".\artifacts\release"
$toolPath = ".\artifacts\tool-smoke"

dotnet tool install TypeSharp.Tool `
  --tool-path $toolPath `
  --add-source $source `
  --version $version

& "$toolPath\typesharp.exe" version --json
& "$toolPath\typesharp.exe" runtime-path --json
```

Repository root `NuGet.config` uses package source mapping. If `--add-source` is rejected, create a temporary `NuGet.config` that maps `TypeSharp.Tool` to the local package folder, matching the release workflow smoke.

## 수동 publish 절차

첫 배포는 release owner가 로컬에서 명시적으로 실행합니다.

```powershell
$version = "0.1.0-preview.5"
$package = ".\artifacts\release\TypeSharp.Tool.$version.nupkg"

dotnet nuget push $package `
  --api-key $env:NUGET_API_KEY `
  --source https://api.nuget.org/v3/index.json `
  --skip-duplicate
```

Publish 후 확인:

```powershell
dotnet tool install --global TypeSharp.Tool --version $version
typesharp version --json
typesharp runtime-path --json
```

확인할 값:

- `artifactKind` is `dotnet-tool`
- `targetDefault` is `net48`
- `runtimeTargetFramework` is `net48`
- `runtime-path` points to `runtime/net48/TypeSharp.Core.dll`
- `runtime-path` points to `runtime/net48/TypeSharp.Runtime.dll`

## 자동 publish를 켜기 전 조건

- NuGet.org package ownership이 개인 단일 계정에 묶이지 않았습니다.
- `NUGET_API_KEY`는 `TypeSharp.Tool` push로 scope가 제한되어 있습니다.
- 첫 수동 publish와 rollback install이 성공했습니다.
- release workflow에서 local install smoke가 안정적으로 통과합니다.
- 문서의 설치 명령이 실제 NuGet.org 패키지 버전과 일치합니다.

이 조건이 충족되면 `.github/workflows/release-artifacts.yml` 끝에 `dotnet nuget push` 단계를 추가할 수 있습니다.
