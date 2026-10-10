# Bop It Access

Bop It Access는 Windows x64용 Steam 버전 **Bop It! The Video Game**을 시각장애인이 즐길 수 있도록 돕는 접근성 모드입니다. [MelonLoader](https://github.com/LavaGang/MelonLoader)와 [Prism](https://github.com/ethindp/prism)을 사용해 게임의 메뉴와 화면에 음성 읽기와 점자 출력을 추가하며, 더 편안하게 즐길 수 있도록 추가 조작과 설정도 제공합니다.

**Windows x64용 버전 1.0이 공개되었습니다.** 모드와 설치 프로그램의 소스 버전은 **1.0.0**입니다. 안내에 따라 설치하려면 [설치 프로그램](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe)을, 수동으로 설치하려면 [컴파일된 ZIP](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip)을 다운로드하세요.

## 주요 기능

- 메뉴, 설정, 튜토리얼, 순위표, 도전 과제, 크레딧, 일시 정지 화면, 결과 화면의 음성 읽기.
- 필요할 때 불러올 수 있는 점수, 스테이지 장면 설명, 현재 조작 할당을 반영하는 힌트.
- 게임이 제공하는 모든 언어로 지원되는 음성 읽기와 게임 내 사용자 가이드.
- 화면 낭독기와 점자 출력. Windows 시스템 음성으로 OneCore와 SAPI도 사용할 수 있습니다.
- 음성 안내의 상세도, 힌트 제공 시점과 반복 횟수 조절, 음성 단축키 재할당.
- 게임 기본 조작에 대한 추가 할당, 프레임 속도 제한, 백그라운드 오디오 설정, 읽기 쉬운 설정 파일.
- 키보드와 컨트롤러를 지원하며 설치, 업데이트, 제거를 할 수 있는 접근성 설치 프로그램.

## 프로젝트 상태

핵심 기능은 대부분 완성되었습니다. 플레이어의 의견을 바탕으로 수정과 개선을 진행하며, 필요에 따라 프로젝트를 유지 관리할 예정입니다. 현재 지원하는 플랫폼은 Windows x64입니다.

[첫 공개 릴리스 v1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0)에는 다음 네 가지 다운로드가 있습니다.

| 다운로드 | 용도 |
| --- | --- |
| [BopItAccess-Installer.exe](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) | 게임을 찾고 필요한 구성 요소와 최신 안정판 모드를 설치하며 업데이트와 제거를 관리하는 접근성 설치 프로그램. |
| [BopItAccess-v1.0.zip](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) | 수동 설치용으로 컴파일된 모드. |
| [Source code (zip)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.zip) | 이 릴리스의 코드를 읽거나 빌드하기 위한 소스 파일. |
| [Source code (tar.gz)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.tar.gz) | 같은 소스 파일을 다른 형식으로 묶은 압축 파일. |

최신 안정판을 설치하려면 설치 프로그램에서 **Install**을 선택하세요. **Show advanced > Install alpha**는 선택 사항인 실험용 기능으로, 확인을 받은 뒤 `main`의 최신 소스를 빌드합니다. GitHub 소스 압축 파일에는 코드와 문서가 들어 있습니다. 빌드된 모드를 설치하려면 컴파일된 ZIP이나 설치 프로그램을 사용하세요.

Git 커밋이 프로젝트의 변경 기록입니다. 처음 37개 빌드는 각각 별도의 소스 스냅샷으로 가져왔으며, 해당 커밋 날짜는 원래 빌드 날짜가 아니라 가져온 날짜를 나타냅니다. 컴파일된 바이너리, 게임 파일, MelonLoader가 생성한 게임 어셈블리는 이 저장소에 포함되지 않습니다.

## 문서

설치, 업데이트, 제거, 조작, 설정, 메뉴, 모든 게임 모드에 대해서는 [영어 사용자 가이드](https://chris-e-shaw.github.io/BopItAccess/BopItAccess-user-guide.html)를 읽어 주세요. 게임 내 가이드는 현재 게임 언어를 자동으로 사용합니다.

## 필요한 항목

- Windows x64와 정식으로 구매한 본인의 Steam 버전 Bop It! The Video Game.
- x64용 **MelonLoader 0.7.3 Open-Beta**. 개발에는 Unity 2022.3.50f1 게임 빌드를 사용합니다.
- 모드 실행을 위한 Windows x64용 **.NET 6 런타임**.
- 공식 Windows x64용 **Prism v0.18.3**의 `prism.dll`. 게임 실행 파일과 같은 폴더에 설치합니다.
- 소스에서 모드를 빌드할 경우: **.NET 6 타기팅 팩**을 갖춘 호환되는 .NET SDK와 MelonLoader가 본인의 게임에서 생성한 참조 파일.
- 소스에서 설치 프로그램을 빌드할 경우: Windows의 **.NET 10 SDK**.

설치 프로그램은 필요한 구성 요소를 공식 배포처에서 받습니다. 컴파일된 릴리스 설치에는 개발용 SDK가 필요하지 않지만, 알파 설치에는 필요합니다.

<a id="build-from-source"></a>
## 소스에서 빌드하기

1. 게임 폴더에 MelonLoader 0.7.3 Open-Beta를 설치합니다. 게임을 한 번 실행하고 MelonLoader가 파일 준비를 마칠 때까지 기다린 다음 종료합니다. 생성된 참조 파일은 게임 폴더의 `MelonLoader\Il2CppAssemblies`에 있어야 합니다.
2. 이 저장소를 다운로드하거나 복제한 다음, 저장소의 최상위 폴더에서 PowerShell을 엽니다.
3. 아래 예제 경로를 본인의 게임 위치로 바꾸고 실행합니다.

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

컴파일된 DLL은 `src\bin\Release\net6.0\BopItAccess.dll`입니다. 게임이나 로더 참조 파일이 누락되면 프로젝트가 컴파일 전에 알려 줍니다. SDK가 .NET 6 타기팅 팩이 없다고 알리면 해당 팩이 포함된 SDK를 설치하세요. 프로젝트의 `NuGet.Config`에는 온라인 패키지 공급원이 설정되어 있지 않습니다.

설치 프로그램의 알파 설치 방식은 게임을 실행하지 않고 로컬 빌드 참조 파일을 준비합니다. 이 참조 파일은 임시 빌드 입력이며, 커밋하거나 컴파일된 모드 릴리스에 포함하지 않습니다.

<a id="install-your-build"></a>
## 빌드한 모드 설치하기

게임을 종료한 상태에서 다음과 같이 진행합니다.

1. 빌드한 `BopItAccess.dll`을 게임의 `Mods` 폴더에 복사합니다. 폴더가 없으면 만드세요.
2. [Prism 릴리스](https://github.com/ethindp/prism/releases)에서 공식 Windows x64용 Prism v0.18.3을 다운로드합니다. `prism.dll`을 게임 실행 파일과 같은 폴더에 넣습니다.
3. 빌드 결과의 `src\bin\Release\net6.0\documentation` 폴더를 게임 폴더에 복사하고, 모든 언어 하위 폴더를 그대로 유지합니다. Prism 바이너리를 배포할 때는 적용되는 Prism 라이선스 파일도 함께 유지하세요.
4. 화면 낭독기를 사용한다면 먼저 실행한 뒤 Steam에서 게임을 시작합니다. 시작 안내를 듣고, 이어서 제목 화면, 환영 화면 또는 메인 메뉴 안내가 나올 때까지 기다린 후 게임을 조작하세요.

모드 빌드 과정은 Prism을 다운로드하거나 컴파일하지 않습니다. 음성 읽기가 시작되지 않으면 게임 폴더의 `Mods\BopItAccess.log`를 확인하세요. 로그의 전송 성공 기록은 모드가 텍스트를 보냈다는 뜻이며, 실제로 소리가 들렸다는 사실까지 확인해 주지는 않습니다.

컴파일된 릴리스 ZIP을 사용할 때는 **내용 전체**를 게임 폴더에 복사하고, 안내가 나오면 폴더를 합치거나 파일을 바꾸세요. ZIP에는 모드, Prism, 가이드, 라이선스 고지가 들어 있습니다. MelonLoader와 .NET은 별도로 설치합니다. 자세한 수동 설치 방법은 사용자 가이드를 참고하세요.

## 게임 밖에서 설정 편집하기

게임을 실행한 후에는 게임 폴더의 `UserData\BopItAccess.ini`에 게임과 모드 설정, 음성 프로필, 플레이어가 사용하는 조작 할당이 읽기 쉬운 형식으로 저장됩니다. 게임을 종료하고 메모장으로 파일을 열어 기존 항목을 수정한 다음 저장하세요. 다음 실행 때 변경 사항을 읽습니다. 주석에는 사용할 수 있는 선택 사항과 범위가 설명되어 있습니다.

예를 들어 `[Game]`에서 `Language=en`을 설정하면 영어로 돌아갑니다. `MusicVolume`, `SfxVolume`, `VoiceOverVolume`을 줄이거나, `[OneCore]` 또는 `[SAPI]`의 `Voice=System default`로 적합하지 않은 음성 선택을 해제할 수 있습니다. `[Mod]`의 `SpeechOutput=On`과 `OutputMode=Auto`는 자동 음성 출력을 복구합니다. 다른 항목은 그대로 두고, 같은 섹션을 중복해서 추가하지 마세요.

## 설치 프로그램 빌드하기

Windows에서 .NET 10 SDK를 사용해 다음을 실행합니다.

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

결과 파일은 `build\installer\BopItAccess.Installer.exe`입니다. 필요한 실행 환경을 포함하는 Windows x64 실행 파일이므로, 사용자는 실행하기 위해 .NET 10을 설치할 필요가 없습니다. 제공되는 설치 프로그램은 서명되지 않았습니다. 사용자 가이드에서 조작 방법과 Windows 보안 확인 창을 설명합니다.

`scripts/package-mod.ps1`은 이미 컴파일된 일치하는 모드로 릴리스 ZIP을 준비합니다. 플레이어에게 필요한 가이드, 고지, 라이선스가 포함됩니다. 개발자용 README, Git 작업 절차, 생성된 게임 참조 파일, 컴파일된 설치 프로그램 파일은 ZIP에 포함되지 않습니다. 패키지 작업은 모드를 컴파일하거나 GitHub 릴리스를 게시하지 않습니다.

## AI 활용에 관한 투명성 안내

이 모드는 AI에 코드 생성을 맡기는 이른바 바이브 코딩으로 만들어졌습니다. 모든 코드의 생성과 관련 조사는 인공지능이 수행했으며, 내부 구조에 대한 인간의 기술적 이해는 제한적입니다. 이 모드는 사용자의 책임 아래 이용해 주세요.

다만 모드의 모든 기능과 설계 결정은 사람이 구상하고 승인했습니다. 테스트는 자동화하지 않았으며, 실제 플레이어와 테스터가 주의 깊고 충분하게 수행했습니다.

참고해 주세요. 다국어 텍스트와 문서는 AI가 생성했으며, 원어민의 검토를 받지 않았습니다. 번역에 상당한 부정확성이 있을 수 있습니다. 스스로 작업을 진행하는 AI 코딩이 없었다면 이 프로젝트는 존재하지 않았을 것입니다. 기회를 주셔서 감사합니다!

## 라이선스와 법적 고지

Bop It Access 자체의 소스 코드와 문서는 **[MIT 라이선스](../../LICENSE)**로 제공됩니다. Copyright © 2026 Christopher Shaw. 의존하는 소프트웨어에는 각자의 라이선스가 적용됩니다. MIT 라이선스는 해당 소프트웨어의 라이선스를 바꾸거나 게임 자산에 대한 권리를 부여하지 않습니다. 의존 소프트웨어에 관한 고지는 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)를 참고하세요.

Bop It Access는 팬이 만든 비공식 프로젝트입니다. Hasbro, 게임 개발사·배급사 Alliance, Valve, Microsoft, Unity, MelonLoader, Prism 또는 화면 낭독기 제작사가 만들거나 승인하거나 보증하지 않습니다. **Bop It!**과 관련 캐릭터, 그림, 소리, 상표는 Hasbro와 각 권리자의 소유입니다. Steam은 Valve의 소유입니다. 다른 제품명, 상표, 소프트웨어도 각 소유자의 재산입니다.

게임은 반드시 본인이 정식으로 구매해야 합니다. 이 저장소는 게임이나 게임 자산을 포함하지 않으며, 해당 자산에 대한 권리를 부여하지 않습니다. 원작 게임의 권리 정보는 [Bop It! 공식 게임 사이트](https://bopitthevideogame.com/)와 [Steam 페이지](https://store.steampowered.com/app/3214360/)를 참고하세요.

## 감사합니다

출시 전에 이 모드를 플레이 테스트하고 현재 모습까지 발전하도록 도와주신 분들께 감사드립니다. 누구를 말하는지 본인들은 아실 거예요. 의견을 주시는 플레이어, 모드를 처음 사용해 보시는 분, 저와 이 프로젝트를 믿어 주시는 분들께도 감사합니다. 여러분의 응원 덕분에 이 정신없는 세상에서도 계속 무언가를 만들어 나갈 수 있습니다. 이 프로젝트가 게임을 즐기고 다른 사람들과 함께 플레이하는 데 도움이 되기를 바랍니다. 모두 정말 감사합니다. Bop It!을 즐겨 주세요!

— Christopher Shaw
