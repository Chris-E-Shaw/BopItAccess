# Bop It Access

Bop It Access 是为 Windows x64 平台 Steam 版 **Bop It! The Video Game** 制作的盲人无障碍模组。它使用 [MelonLoader](https://github.com/LavaGang/MelonLoader) 和 [Prism](https://github.com/ethindp/prism)，为游戏的菜单和界面提供语音朗读与盲文输出，并通过额外的操作和设置让游戏体验更舒适。

**Windows x64 版 1.0 已公开发布。** 模组和安装程序的源代码版本均为 **1.0.0**。下载[安装程序](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe)并按说明安装，或使用[编译好的 ZIP](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip)手动安装。

## 主要功能

- 朗读菜单、设置、教程、排行榜、成就、制作人员名单、暂停界面和成绩界面。
- 随时按需朗读分数、场景描述，以及根据当前按键设置生成的操作提示。
- 提供游戏支持的所有语言的语音文本和游戏内用户指南。
- 支持屏幕阅读器与盲文输出，也可使用 OneCore 和 SAPI 系统语音。
- 可调整朗读的详细程度、提示延迟和重复次数，并重新分配语音快捷键。
- 扩展原有游戏操作的按键设置，增加帧率限制、后台音频控制和易读的设置文件。
- 无障碍安装程序支持键盘和手柄，可安装、更新和卸载模组。

## 项目状态

主要功能已基本完成。项目将根据需要持续维护，依据玩家反馈修复问题并改进体验。目前支持 Windows x64 平台。

[首个公开发行版 v1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0)提供以下四种下载：

| 下载 | 用途 |
| --- | --- |
| [BopItAccess-Installer.exe](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) | 无障碍安装程序，可查找游戏、安装所需组件和最新稳定版模组，并管理更新与卸载。 |
| [BopItAccess-v1.0.zip](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) | 用于手动安装的编译版模组。 |
| [Source code (zip)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.zip) | 供阅读或自行编译此发行版的源代码文件。 |
| [Source code (tar.gz)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.tar.gz) | 相同源代码的另一种压缩格式。 |

在安装程序中选择 **Install**，即可安装最新稳定版。**Show advanced > Install alpha** 是可选的实验功能，会在确认后编译 `main` 分支的最新源代码。GitHub 的源代码压缩包包含代码和文档；要安装编译好的模组，请使用编译版 ZIP 或安装程序。

Git 提交就是本项目的历史记录。最初的 37 个构建已分别导入为独立的源代码快照；这些提交的日期记录的是导入时间，而非原始构建时间。此仓库不包含已编译的二进制文件、游戏文件或 MelonLoader 为游戏生成的程序集。

## 文档

有关安装、更新、卸载、操作、设置、菜单以及所有游戏模式，请阅读[英文用户指南](../../BopItAccess-user-guide.html)。游戏内指南会自动使用当前游戏语言。

## 所需环境

- Windows x64，以及您合法拥有的 Steam 版 Bop It! The Video Game。
- x64 版 **MelonLoader 0.7.3 Open-Beta**。开发使用 Unity 2022.3.50f1 游戏构建。
- 运行模组需要 Windows x64 版 **.NET 6 运行时**。
- 官方 Windows x64 版 **Prism v0.18.3** 的 `prism.dll`，放在游戏可执行文件旁。
- 从源代码构建模组需要：兼容的 .NET SDK、**.NET 6 目标包**，以及 MelonLoader 从您自己的游戏生成的引用文件。
- 从源代码构建安装程序需要：Windows 上的 **.NET 10 SDK**。

安装程序会从官方来源获取依赖组件。安装已编译的发行版不需要开发用 SDK；安装 Alpha 版则需要。

<a id="build-from-source"></a>
## 从源代码构建

1. 将 MelonLoader 0.7.3 Open-Beta 安装到游戏文件夹。启动一次游戏，等待 MelonLoader 准备好文件后关闭游戏。生成的引用文件应位于游戏文件夹内的 `MelonLoader\Il2CppAssemblies`。
2. 下载或克隆此仓库，在仓库根文件夹中打开 PowerShell。
3. 将以下示例路径替换为您的游戏路径，然后运行：

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

编译后的 DLL 位于 `src\bin\Release\net6.0\BopItAccess.dll`。如果缺少游戏或加载器的引用文件，项目会在编译前报告。如果 SDK 提示缺少 .NET 6 目标包，请安装包含该目标包的 SDK。项目的 `NuGet.Config` 未配置在线软件包源。

安装程序的 Alpha 安装流程会在不启动游戏的情况下准备本地构建引用文件。这些文件只是临时构建输入，不会被提交，也不会包含在已编译的模组发行版中。

<a id="install-your-build"></a>
## 安装您构建的模组

先关闭游戏，然后执行以下操作：

1. 将构建好的 `BopItAccess.dll` 复制到游戏的 `Mods` 文件夹；如果该文件夹不存在，请先创建。
2. 从 [Prism 发行页面](https://github.com/ethindp/prism/releases)下载官方 Windows x64 版 Prism v0.18.3。将 `prism.dll` 放到游戏可执行文件旁。
3. 将构建输出的 `src\bin\Release\net6.0\documentation` 文件夹复制到游戏文件夹，保留所有语言子文件夹。分发 Prism 二进制文件时，也请保留适用的 Prism 许可证文件。
4. 如果使用屏幕阅读器，请先启动它，再通过 Steam 启动游戏。听到启动语音后，继续等待标题界面、欢迎界面或主菜单的语音，再开始操作游戏。

构建模组不会下载或编译 Prism。如果没有开始朗读，请查看游戏文件夹中的 `Mods\BopItAccess.log`。日志中的发送成功只表示模组已发送文本，不能证明用户实际听到了语音。

如果使用已编译的发行 ZIP，请将**全部内容**复制到游戏文件夹，并在出现询问时合并文件夹或替换文件。ZIP 包含模组、Prism、指南和许可证声明；MelonLoader 与 .NET 需要单独安装。完整的手动安装步骤请参阅用户指南。

## 在游戏外编辑设置

启动后，游戏文件夹中的 `UserData\BopItAccess.ini` 会保存易读的游戏与模组设置、语音配置和面向玩家的按键分配。关闭游戏，用记事本打开此文件，修改现有条目并保存。模组会在下次启动时读取更改。注释说明了有效选项和取值范围。

例如，将 `[Game]` 中的 `Language=en` 设为英语；降低 `MusicVolume`、`SfxVolume` 和 `VoiceOverVolume`；或者将 `[OneCore]` 或 `[SAPI]` 中的 `Voice=System default` 设为系统默认，以恢复不合适的语音选择。将 `[Mod]` 中的 `SpeechOutput=On` 和 `OutputMode=Auto` 设为这些值，可以恢复自动语音输出。保留其他条目，不要添加重复的节。

## 构建安装程序

在装有 .NET 10 SDK 的 Windows 系统上运行：

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

输出为 `build\installer\BopItAccess.Installer.exe`。这是自包含的 Windows x64 可执行文件，用户不需要安装 .NET 10 就能运行。提供的安装程序尚未签名。用户指南介绍了其操作方式和 Windows 安全提示。

`scripts/package-mod.ps1` 会使用已编译且版本匹配的模组准备发行 ZIP，其中包含玩家需要的指南、声明和许可证。开发者 README、Git 工作流程说明、生成的游戏引用文件和编译后的安装程序不包含在 ZIP 中。打包过程不会编译模组，也不会发布 GitHub 发行版。

## AI 透明度说明

此模组通过让 AI 生成代码的方式，即“氛围编程”，开发而成。全部代码的生成和研究工作均由人工智能完成，人类对其底层结构的技术理解有限。请自行承担使用此模组的风险。

不过，模组的每一项功能和设计决定都由人类提出并批准。测试从未自动化，而是由真实玩家和测试者认真、充分地完成。

请注意：多语言文本和文档由 AI 生成，尚未经母语使用者审核。翻译可能存在大量不准确之处。没有能够自主推进工作的 AI 编程，这个项目就不会存在。感谢您愿意给它一次机会！

## 许可证与法律声明

Bop It Access 自身的源代码和文档采用 **[MIT 许可证](../../LICENSE)**。Copyright © 2026 Christopher Shaw。依赖组件仍遵守各自的许可证；MIT 许可证不会重新许可这些组件，也不会授予游戏资产的权利。依赖组件声明请参阅 [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)。

Bop It Access 是粉丝制作的非官方项目。它并非由 Hasbro、游戏开发商和发行商 Alliance、Valve、Microsoft、Unity、MelonLoader、Prism 或任何屏幕阅读器开发商制作、批准或背书。**Bop It!** 及相关角色、美术、声音和商标归 Hasbro 及各自权利人所有。Steam 归 Valve 所有。其他产品名称、商标和软件仍属于其各自所有者。

您必须合法获取自己的游戏副本。本仓库不包含游戏或其资产，也不授予这些内容的任何权利。有关原版游戏的权利信息，请参阅 [Bop It! 官方游戏网站](https://bopitthevideogame.com/)和 [Steam 商店页面](https://store.steampowered.com/app/3214360/)。

## 感谢

感谢所有在发行前试玩测试此模组，并帮助它走到今天的人。你们知道我说的是谁。也感谢提供反馈、第一次尝试模组，或相信我和这个项目的玩家。你们的支持让我有动力在这个疯狂的世界里继续创作。希望这个项目能让您更轻松地享受游戏，并与其他人一起玩。真心感谢大家。祝您玩得开心，享受 Bop It!

— Christopher Shaw
