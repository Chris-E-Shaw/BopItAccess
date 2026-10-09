# Bop It Access

当前源代码构建为模组0.9.12（59次模组构建）、安装程序0.2.6。首次公开发行仍在计划中。

Bop It Access 是 **Bop It!** 的 Windows Steam 版本的非官方辅助功能模组。它使用 MelonLoader 和 [Prism](https://github.com/ethindp/prism) 将语音和盲文反馈添加到菜单和游戏屏幕。目前的功能包括首次运行的欢迎屏幕、游戏内用户指南、语音标题和暂停屏幕、设置和控制、歌曲选择、最终分数和排行榜、成就、积分、按钮提示、带有回合前当前控制分配的点播教程文本以及四个阶段的描述。版本 0.9.0 使用 Prism 进行语音和盲文输出。该模组遵循游戏选择的语言，并包含游戏提供的每种语言的指南。

## 编辑设置文件

如果陌生语言、过大的游戏音量或有问题的语音让菜单难以操作，你可以在游戏外修改设置。启动完成后，模组会根据当前设置，在 Bop It! 游戏文件夹内自动创建 UserData/BopItAccess.ini。这是普通文本文件，可以用记事本等编辑器打开。

文件包含游戏语言、音乐音量、音效音量、语音提示音量、振动、全屏、分辨率和音频延迟；模组的语音、盲文、提示等设置；分别保存的 OneCore 和 SAPI 语音设置；以及面向玩家的游戏和模组操作绑定。可用分辨率和已安装语音会列在注释中。

编辑前请关闭游戏。找到对应分区，修改已有条目的值，保存文件，然后重新启动游戏。修改在启动时读取，不会在游戏运行期间立即生效。通过游戏菜单修改的设置会自动同步到文件。

无论使用哪种语言，分区名称和设置名称都保持英文。开关建议使用 On 和 Off，也支持 True/False、Yes/No 和 1/0。注释会说明选项和范围。缺失或无效的条目会保留对应的已保存设置，其他有效修改仍会生效。重复的操作绑定会被拒绝。

注释和未知条目会保留。如果其他程序在游戏运行期间修改文件，模组会在本次会话剩余时间内停止自动写入，以保护这些编辑。请关闭并重新启动游戏，让修改生效。编辑前也可以保留备份。

如果某个语音有问题，请在 OneCore 或 SAPI 分区中设置 Voice=System default。OneCore 使用名称 | 语言格式；SAPI 支持已安装语音的显示名称或完整注册表 ID。文件中会列出可用选项。OutputMode=Auto 会依次尝试正在运行的受支持屏幕阅读器、OneCore 和 SAPI。

下面的示例会恢复英语、较低的游戏音量，以及使用系统默认语音的自动语音输出。请修改文件中已经存在的对应条目；这只是参考节选，不是要追加到文件末尾的新分区。其他设置请保持不变。

```ini
[Game]
Language=en
MusicVolume=30
SfxVolume=30
VoiceOverVolume=30

[Mod]
SpeechOutput=On
OutputMode=Auto

[OneCore]
Voice=System default

[SAPI]
Voice=System default
```

确认 Uninstall 后，选择 Uninstall for me 或 Uninstall for everyone。两者都会删除这个游戏文件夹中的共享模组文件，因此使用该游戏安装的任何人都将无法再使用模组。这个选择决定删除谁保存在 Windows 中的模组偏好设置：仅发起请求的账户，或者包括已注销账户在内的所有本地 Windows 用户配置。原游戏的偏好设置会保留。.NET SDK 仍保持安装。

当安装程序删除自己安装的 MelonLoader 且其他模组不需要它时，也会删除已知的 Loader.cfg 和 MelonPreferences.cfg，以及空的 Plugins、UserLibs 和 UserData 文件夹。Bop It Access 设置、已知日志、指南和安装程序文件会删除。其他模组、原有共享加载器文件以及无法识别的文件会受到保护。这也意味着未知文件可能使文件夹保留下来；安装程序会在诊断信息中说明，而不会删除无关数据。

Windows“已安装的应用”使用相同的确认、偏好设置范围选择和清理流程。安装程序在游戏文件夹提供 BopItAccess-uninstall.ps1，作为指向已安装卸载程序的快捷脚本；未来源代码构建的输出也会包含它。仅手动复制脚本不会安装卸载程序本身。对于没有文件归属记录的旧手动安装，安装程序会删除可识别的模组文件，并保留无法确认来源的共享文件。

如果无法安全完成清理，安装程序会解释情况并保留重试所需的信息。对于受管理的安装，Windows 卸载条目和清理检查点会一直保留到删除成功。旧手动安装没有持久的文件归属记录；请在打开的安装程序中重试相关警告。Bop It! 运行时不要安装、更新或删除模组。

## 项目状况

该项目已基本完成，未计划主要内容或功能。它将根据需要进行维护，并根据玩家的反馈指导改进。 GitHub 存储库包含源代码和技术文档。 **尚无 GitHub 版本。** 源代码现在还包含 Windows 安装程序项目。在发布版本之前，其**安装**按钮会说明没有可用的版本； **安装 alpha** 从源代码构建最新的主分支提交。

提交历史记录包括 37 个早期版本的重构源快照。当这些档案导入 Git 时，就会创建提交；他们的日期不是最初的构建日期。的 [技术构建历史](BopItAccess-build-history.html) 描述每个快照背后的工作。

## 要求

- Windows x64 和您自己安装的 Bop It! 用于 Steam。
- MelonLoader安装在游戏目录中。开发使用了 MelonLoader **0.7.3 Open-Beta** 和 x64 Unity **2022.3.50f1** 游戏版本。其他组合尚未得到验证。
- 带有 **.NET 6 目标包** 的 .NET SDK，因为 mod 目标 `net6.0`.
- 安装时，官方 Windows x64 Prism v0.18.3 `prism.dll`。此第三方二进制文件不在此存储库中。

该mod引用了游戏目录下MelonLoader生成或安装的DLL。它不包含或重新分发游戏程序集。

<a id="build-from-source"></a>
## 从源代码构建

1. 安装MelonLoader，启动Bop It!一次，然后关闭游戏。 MelonLoader 应该创建 `MelonLoader\Il2CppAssemblies` 游戏目录下。
2. 克隆或下载此存储库。在存储库的根目录中打开 PowerShell。
3. 套装 `$gameDir` 到 **你的** Bop It! 安装目录，然后构建：

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   示例路径是 Steam 的常用位置 Windows。如果您的 Steam 库在其他地方，请更改它。该项目在编译前检查所需的 MelonLoader 和生成的游戏 DLL，并报告缺少路径。

4. 构建的 mod DLL 将位于 `src\bin\Release\net6.0\BopItAccess.dll`.

如果 SDK 报告缺少 .NET 6 定位包，请安装包含该包的 SDK。该项目的 `NuGet.Config` 不配置在线包源。

<a id="install-your-build"></a>
## 安装你的版本

1. 关闭游戏。复制构建的 `BopItAccess.dll` 进入 `<game directory>\Mods\`。创建 `Mods` 目录（如果 MelonLoader 尚未创建）。
2. 从 [Prism 发布页面](https://github.com/ethindp/prism/releases)获取适用于 Windows x64 的官方 Prism v0.18.3 `prism.dll`，或从源代码构建相同版本。将 `prism.dll` 放在游戏主文件夹中，与游戏可执行文件放在一起，不要放进 `Mods` 文件夹。
3. 复制整个构建 `src\bin\Release\net6.0\documentation\` 文件夹放入游戏目录。它包含其根部的英文指南和翻译后的指南 `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, 和 `pt-BR`。保留这些子文件夹和配套文档。游戏内指南每次打开时都会读取当前游戏语言的 HTML，因此替换指南会更新其内容，而无需重建 DLL。
4. 如果使用屏幕阅读器，请先启动它，再启动游戏。没有运行兼容的屏幕阅读器时，Prism 优先使用 OneCore；如果 OneCore 不可用，则使用 SAPI。

Bop It Access build命令仅编译此mod；它不会构建或下载 Prism。如果语音未开始，请检查 `<game directory>\Mods\BopItAccess.log`。日志记录了 Prism 初始化和语音调度，但仅成功调度并不能证明听到了音频。

首次运行时，游戏主菜单准备就绪后会出现欢迎屏幕。它的选择是打开模组设置、阅读游戏中的用户指南或继续游戏。 Mod 设置还提供 **打开用户指南** 和确认的 **重置欢迎屏幕** 操作，该操作会在下次启动时显示欢迎屏幕。在指南中，使用向上/向下选择主题或阅读行，然后使用确认打开主题。在表中，“左”向左移动一列，“右”向右移动一列，“向上/向下”在更改行时保留当前列。列标题标记单元格而不是显示为数据行；该表在进入时宣布，并在退出时结束。后面留下一个主题或指南。

在游戏的 **设置 > 语言** 行中选择一种语言。 Mod 语音遵循该选择。游戏内指南使用匹配的翻译 HTML 文档，如果所选副本丢失或不可读，则使用英语作为后备。捆绑的非英语文本是机器翻译的第一遍；欢迎能说流利的人指正。

游戏操作使用游戏自带的译名。Shapes、Space、City 和 Office 是固定的英文关卡名称。 使用 OneCore 或 SAPI 时，如果系统默认语音无法正确朗读游戏语言，请在模组设置中选择合适的已安装语音。 “语音”“音量”“语速”和“音高”调整当前实际使用的 OneCore 或 SAPI，自动模式也一样。仅显示当前引擎支持的设置；其他输出方式会隐藏这些设置。两个引擎分别保存各自的设置。

朗读菜单项位置. 此设置会保存，默认开启，用于朗读当前菜单项在菜单中的位置。 一对一反馈的设置会保存，默认开启。在开始时和切换时朗读当前颜色。失去生命时，只朗读剩余数量。获得生命时，朗读玩家颜色和新的总数，例如“绿色，3条生命”，让双方知道谁先按下了拍打。关闭一对一反馈后，不会朗读这些提示。游戏的三条生命上限不变。此功能仅在一对一游戏中生效。 最后一次校准输入被记录后，模组会立即说“完成！”。请停止拍打，等待测量结果。如果因为没有输入而校准失败，会说“校准失败。”

**优化朗读格式**: 将全大写英文调整为自然的大小写，仅用于语音和盲文。在游戏内指南中，如果一行结尾没有标点，就在行号前加上省略号，留出停顿。屏幕上的文字不变。关闭后按原文输出。

### MelonLoader 启动窗口

附带的Loader.cfg模板会隐藏MelonLoader单独的启动画面和控制台。安装程序会在您自行启动游戏之前应用这两个默认值。它们不会跳过游戏标题画面或Mod欢迎画面。

关闭游戏后，打开游戏文件夹中的 `UserData/Loader.cfg`。如果文件已经存在，请在现有的 `[loader]` 节中将 `disable_start_screen` 设为 `true`，在现有的 `[console]` 节中将 `hide_console` 设为 `true`。保留其他所有设置。如果文件不存在，请复制构建文件中提供的 `UserData/Loader.cfg` 模板，或源码中的 `configuration/Loader.cfg`。切勿用整个模板覆盖已有的 Loader.cfg。

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

模组不会在每次启动时重置这些选项。如需排查问题，可手动将任一值改回 false。如果卸载时保留共享 MelonLoader，只会恢复安装程序设置后仍未改变的目标标志，并保留其他编辑。如果删除安装程序自己安装且已无人使用的 MelonLoader，也会删除已知的 Loader.cfg 和 MelonPreferences.cfg。

## 文档

- [游戏和模组用户指南（英文）](BopItAccess-user-guide.html) — 适合初学者的控制、设置、菜单和播放模式的演练。
- [日语用户指南（日本语）](../ja/BopItAccess-user-guide.html)。其他翻译的指南可在以下语言文件夹中找到 [`documentation/`](../).
- [详细的功能和控制指南](README.txt)。它的安装部分描述了本地准备的安装 ZIP；此 GitHub 存储库仅提供源代码。
- [技术构建历史](BopItAccess-build-history.html).
- [发布准备代码审查](BopItAccess-release-review.html) — 已实施的修复、已审查文件、编译结果及剩余限制。
- [此项目的 Git 工作流程](GIT-WORKFLOW.md).
- [第三方通知](THIRD-PARTY-NOTICES.txt).

上述文档的翻译副本位于 [`documentation/`](../) 下各受支持语言代码的文件夹中。其源文为英语； `scripts/translate_documents.py` 可在源文改变后重新生成机器翻译草稿。

## 接下来可能会发生什么

该项目已基本完成，未计划主要内容或功能。然而，该模组将根据需要积极维护和更新，并根据玩家反馈推动这些改进。未来潜在的工作包括进一步审查和错误修复、代码细化以及语音响应能力的持续改进。 Prism创建了未来通往其他平台的可能路径，但该mod目前仅支持Windows x64。项目存储库是进行进一步开发的地方。

## AI 透明度说明

本模组采用“vibe coding（氛围编程）”方式开发。所有代码完全由人工智能生成和研究，人类对其底层架构的技术理解有限。请自行承担使用此模组的风险。

话虽这么说，每个模组功能和设计决策都是由人类编写和批准的。测试从来都不是自动化的；它是由真实的人类玩家和测试人员仔细而广泛地执行的。

请注意：多语言文本和文档由人工智能生成，未经母语人士审核。翻译的高度不准确是可以预料的。如果没有代理编码，这个项目就不会存在。感谢您给它一个机会！

## 谢谢

对于那些在发布之前测试过这个模组并帮助它达到现在水平的玩家，谢谢你们。你们都知道自己是谁。对于那些提供反馈、第一次尝试该模组、或者相信我和这个项目的玩家，谢谢你们。你们的支持激励我在一个让人感到疯狂和存在严重缺陷的世界里继续创造东西。我希望这个项目能让您更轻松地享受游戏并与他人一起玩。非常感谢大家。享受Bop It!

— Christopher Shaw

## 许可

尚未选择 Bop It Access 源的许可证。 Prism 有自己的许可证；看到 [第三方通知](THIRD-PARTY-NOTICES.txt)。 Bop It! 及其资产属于其各自所有者，不包含在此。


## 选择正确的下载

首次GitHub公开发行计划提供以下四种下载。它们尚未提供，公开发行版和标签均未发布。在此之前，请使用提供的安装程序或源代码方式。源代码压缩包并非编译好的安装ZIP。

[GitHub Releases](https://github.com/Chris-E-Shaw/BopItAccess/releases)

- BopItAccess-Installer.exe: Windows x64自包含安装程序。查找游戏并管理依赖项、安装、更新、诊断及卸载。此程序没有数字签名。
- BopItAccess-v1.0.zip: 无需运行Bop It Access EXE的手动安装编译包。包含Mods/BopItAccess.dll、prism.dll、全部文档及Prism许可、Loader.cfg模板、README.txt和卸载快捷脚本。不含MelonLoader、.NET、游戏文件或生成的游戏程序集。
- Source code (zip): GitHub自动生成的发行版源代码ZIP。用于阅读或编译代码，不是编译好的模组包。
- Source code (tar.gz): 相同源代码的gzip压缩tar包。只是另一种源代码格式，不是另一种模组安装程序。

### 未签名安装程序与Windows 11安全提示

此安装程序没有数字签名。未签名或不常见的程序可能触发SmartScreen或杀毒警告，也可能出现误报，但并不证明所有检测都错误。仅从Bop It Access官方项目或可信的直接提供者获取，并自行判断是否信任该文件。编译ZIP方式可避免运行此EXE。不要关闭杀毒防护或排除整个磁盘及游戏文件夹。
[Microsoft: unsigned apps and SmartScreen](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app)

### 允许特定Defender检测项

按Win+I，进入隐私和安全性 > Windows安全中心 > 打开Windows安全中心 > 病毒和威胁防护 > 保护历史记录（有时也称威胁历史记录）。展开与此安装程序对应的项，按Tab找到操作或更多操作，按Enter并选择在设备上允许或允许；出现管理员提示时批准。隔离文件可能需要先还原，再次检测后再允许。若文件已删除，请从官方项目重新下载。允许前确认具体项目。
[Microsoft: Protection history](https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app) · [Microsoft Defender FAQ](https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq)

### 可选的有限Defender排除项

在病毒和威胁防护设置标题下选择管理设置，再进入排除项 > 添加或删除排除项。出现管理员提示时选择是。选择添加排除项 > 进程，准确输入BopItAccess-Installer.exe并按Enter。名称必须与实际运行的EXE一致。

Microsoft的进程排除适用于该进程打开的文件，并不排除安装程序EXE本身、还原隔离文件或绕过SmartScreen。若Defender检测的是EXE本身且你信任它，可选的文件排除只选择那个已下载的EXE，才是对应的有限替代方式。不再需要时删除例外。
[Microsoft: exclusions overview](https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview)

SmartScreen是独立提示。若信任该EXE且Windows提供选项，可选择更多信息 > 仍要运行。Defender允许或进程排除不会绕过该提示，策略也可能阻止运行。

步骤于2026年10月9日针对Windows 11 25H2、版本26200.9550核对。界面名称可能不同，未进行UI操作试验。

## Windows 安装程序 0.2.6

### 通过安装程序安装、更新或卸载

1. 关闭游戏，打开BopItAccess-Installer.exe并批准Windows管理员提示。阅读Welcome and controls文本框，再检查检测到的Game folder或使用Browse。Welcome and controls只读、可选择，并首先获得焦点；Alt+W可返回。
2. 公开编译发行版可用后，选择Install。在首次公开前，Show advanced显示Install alpha，确认后在电脑上编译最新源代码。等待成功消息。之后只有选择Play Bop It! The Video Game时才会通过Steam启动游戏。
3. 更新已有安装时，关闭游戏后打开安装程序并检查状态。发现较新的公开发行版后显示Update。选择并等待完成；保存的设置保持不变。Install alpha是获取最新源代码的独立选项，不是公开发行版更新。
4. 卸载时选择Uninstall并确认，再选择Uninstall for me或Uninstall for everyone。两者均删除此游戏文件夹中的共享模组文件。区别是删除当前账户或所有本地Windows用户的模组偏好；保留原游戏偏好。Windows已安装的应用使用相同卸载流程。检查结果后选择Quit。

下表列出主窗口的所有操作及文本框。Installer status和Installation progress是信息，不是按钮。Welcome and controls提供可重复阅读的说明，Status log提供变化的操作消息。Abort会在回退正在进行的安装前确认，Quit也遵循安全取消规则。对话框包含Keep open/Quit、确认/取消及两个卸载偏好范围。Show advanced只改变哪些操作可见。

请使用提供的BopItAccess-Installer-0.2.6.exe或BopItAccess-Installer.exe。两个名称均为同一个Windows x64自包含安装程序。项目包含源代码；目前尚未发布公开编译的二进制文件或GitHub Release。

请先关闭 Bop It!，打开安装程序并批准 Windows 管理员提示。安装程序会显示欢迎消息，在所有可用驱动器的 Steam 库中寻找游戏，并尝试将窗口切到前台。检查显示的游戏文件夹；需要选择其他位置时使用 Browse。Tab 用于在控件之间移动。状态日志是只读文本框：将焦点移到此处，即可用光标键查看消息、选择或复制文本。

安装程序0.2.6会在启动时短暂请求前台激活和键盘焦点。如果有限的启动观察结束时仍有其他窗口处于活动状态，安装程序会闪烁标题和任务栏按钮，并提示用Alt+Tab切换到安装程序。使用键盘或控制器操作前，请先激活安装程序。Alt+G将焦点移到游戏文件夹文本框。

安装程序打开时，Show advanced 默认未勾选。勾选后会显示 Install alpha、Save diagnostics 和 Copy diagnostics。如果已有公开的 GitHub 发行版，Install 会下载最新版本。目前还没有公开发行版，因此测试者需要勾选 Show advanced 并使用 Install alpha。Alpha 会请求确认，下载最新源代码并在您的电脑上构建。当已安装版本有更新的公开发行版可用时，会显示 Update。

状态消息用简明语言说明正在下载、安装或完成什么。一条进度条显示整个安装过程的估计进度，不会在每次下载或处理文件时重置。它以五个百分点为单位前进；某些准备阶段可能花费一段时间而没有可见变化。欢迎消息、新更新可用以及诊断信息已复制的确认，会通过 Windows 无障碍通知传递给屏幕阅读器。是否实际朗读取决于屏幕阅读器及其对 Windows 通知的支持。

安装程序 0.2.6 不会在安装期间启动 Bop It!。Alpha 会复用匹配的本地构建文件，或者在游戏保持关闭的情况下，根据您已安装的游戏准备临时文件。之后安装程序把 MelonLoader 放入游戏文件夹，立即添加 Mods/BopItAccess.dll，再安装 Prism、设置、完整文档和卸载支持。请等待成功消息，然后在准备好时自行从 Steam 启动游戏。

安装成功后会显示 Play Bop It! The Video Game。准备好时可激活它，自行通过 Steam 启动游戏。安装程序不会在安装期间自动启动游戏。

已编译的发行版需要 Windows x64 .NET 6 运行时，不需要开发 SDK。完整的现有运行时会被复用。缺少的运行时会从 Microsoft 下载并放入 MelonLoader/Dependencies/dotnet。Install alpha 还需要兼容的 .NET SDK 和 .NET 6 目标包：复用已有 SDK，或在系统范围内安装 Microsoft 官方 SDK。安装程序不会在游戏根目录创建新的 SDK 文件夹。MelonLoader 0.7.3 Open-Beta 和 Prism 0.18.3 来自官方发行版。共享的 Microsoft .NET 组件和 SDK 在中止或卸载后仍保持安装。

Quit 关闭安装程序。如果安装仍在进行，会询问是否在关闭前中止并撤销安装；Keep open 让操作正常继续。如果您考虑期间安装已经完成，对话框会更新为已完成，Quit 不会撤销完成的安装。卸载一旦开始删除，会安全完成后再退出。Abort 也会请求确认并撤销本次尝试对游戏文件的更改。在 Microsoft .NET 安装期间取消，会等待共享组件的安装安全结束。

确认 Uninstall 后，选择 Uninstall for me 或 Uninstall for everyone。两者都会删除这个游戏文件夹中的共享模组文件，因此使用该游戏安装的任何人都将无法再使用模组。这个选择决定删除谁保存在 Windows 中的模组偏好设置：仅发起请求的账户，或者包括已注销账户在内的所有本地 Windows 用户配置。原游戏的偏好设置会保留。.NET SDK 仍保持安装。

当安装程序删除自己安装的 MelonLoader 且其他模组不需要它时，也会删除已知的 Loader.cfg 和 MelonPreferences.cfg，以及空的 Plugins、UserLibs 和 UserData 文件夹。Bop It Access 设置、已知日志、指南和安装程序文件会删除。其他模组、原有共享加载器文件以及无法识别的文件会受到保护。这也意味着未知文件可能使文件夹保留下来；安装程序会在诊断信息中说明，而不会删除无关数据。

卸载后安装程序保持打开，便于查看结果、保存诊断信息或重新安装。完成后选择 Quit。运行中的卸载辅助文件和自动诊断日志将在窗口关闭后清理。同一窗口中重新安装会建立新的安装记录；之前延迟的清理不会删除新安装。

Windows“已安装的应用”使用相同的确认、偏好设置范围选择和清理流程。安装程序在游戏文件夹提供 BopItAccess-uninstall.ps1，作为指向已安装卸载程序的快捷脚本；未来源代码构建的输出也会包含它。仅手动复制脚本不会安装卸载程序本身。对于没有文件归属记录的旧手动安装，安装程序会删除可识别的模组文件，并保留无法确认来源的共享文件。

如果无法安全完成清理，安装程序会解释情况并保留重试所需的信息。对于受管理的安装，Windows 卸载条目和清理检查点会一直保留到删除成功。旧手动安装没有持久的文件归属记录；请在打开的安装程序中重试相关警告。Bop It! 运行时不要安装、更新或删除模组。

### 安装程序键盘快捷键

| 操作 | 键盘快捷键 | 作用 |
| --- | --- | --- |
| Welcome and controls | Alt+W | 独立Welcome and controls文本框列出控制器文本查看快捷键；Alt+W返回，Alt+L进入变化的Status log。两者均只读，可选择和查看。Show advanced通知选中或未选中，全选通知成功或空文本框。 键盘Ctrl+A选择全部文本，Ctrl+C复制选择内容。 |
| 游戏文件夹 | Alt+G | 聚焦游戏文件夹文本框。 |
| Browse | Alt+B | 选择游戏文件夹。 |
| Install | Alt+I | 有最新公开发行版时安装它。 |
| Install alpha | Alt+A | 确认并构建最新源代码；勾选 Show advanced 后可见。 |
| Update | Alt+U | 安装提供的更新公开发行版。 |
| Play Bop It! The Video Game | Alt+P | 通过 Steam 启动游戏；安装成功后可用。 |
| Uninstall | Alt+N | 确认删除并选择删除谁的 Windows 模组偏好设置。 |
| Abort | Alt+R | 确认取消当前安装。 |
| 状态日志 | Alt+L | 聚焦只读且可选择的状态消息。 |
| Show advanced | Alt+V | 显示或隐藏 Alpha 安装和诊断工具。 |
| Save diagnostics | Alt+D | 保存完整诊断会话并继续记录；勾选 Show advanced 后可见。 |
| Copy diagnostics | Alt+C | 复制完整诊断快照；勾选 Show advanced 后可见。 |
| Quit | Alt+Q | 关闭；如有操作正在运行则安全取消。 |

### 使用控制器操作安装程序

安装程序支持 Xbox 类型控制器以及 Windows 通过 XInput 提供的其他控制器。这些操作独立于游戏内可重新绑定的控制。方向键或左摇杆在控件之间移动；文本框有焦点时，方向输入改为查看文本。肩键始终移动到上一个或下一个可聚焦控件。A 激活有焦点的按钮或复选框。仅当安装程序或其自身对话框位于前台时才处理控制器输入。

B 在对话框中返回或取消；在安装程序主窗口中，安装进行时请求中止，否则执行 Quit 流程。Start 在主窗口执行 Quit，在对话框中返回。Y（上方正面按钮）在安装程序文本框有焦点时选择全部文本。在主窗口的文本框之外，Y切换Show advanced。在状态日志或安装程序的其他文本框中，方向键或左摇杆相当于键盘箭头键：左／右按字符移动，上／下按行移动。按住LT相当于Ctrl：左／右按单词移动，上／下按段落移动。按住RT相当于Shift，可扩展选择范围；同时按住LT和RT可选择单词或段落。X只复制已选中的文本，请先选择需要的部分。键盘Ctrl+C仍可复制选择范围。未选择文本时，安装程序也会发送无障碍通知，告知插入点所在的字符、单词、行或段落。复制成功时安装程序会发送无障碍确认通知，未选择文本或复制失败时也会通知。是否朗读取决于屏幕阅读器对Windows通知的支持。Windows 原生文件夹和保存对话框的控制器操作仍需人工验证。输入文件夹或文件名仍可使用键盘。不支持 XInput 的控制器不在当前实现范围内。

独立Welcome and controls文本框列出控制器文本查看快捷键；Alt+W返回，Alt+L进入变化的Status log。两者均只读，可选择和查看。Show advanced通知选中或未选中，全选通知成功或空文本框。 键盘Ctrl+A选择全部文本，Ctrl+C复制选择内容。

安装程序0.2.6请求每条发出的语音通知替换先前的安装程序语音，包括文本查看、全选、Show advanced的选中／未选中状态、Copy diagnostics及其他确认。LB/RB仍会通知新获得焦点的控件。状态通知频率保持不变，并非每条日志都自动朗读。实际中断取决于屏幕阅读器对Windows通知的支持，仍需人工验证。

### 安装程序诊断信息

Show advanced 会显示 Save diagnostics（Alt+D）和 Copy diagnostics（Alt+C）。自动 UTF-8 日志保存在本机的 %ProgramData%\BopItAccess\diagnostics。Save diagnostics 将当前完整会话写入您选择的 .log 或 .txt 文件，并持续记录到安装程序关闭。Copy diagnostics 复制当前快照，并提供无障碍确认。安装或卸载测试前请先保存，以便自动日志清理后记录仍然存在。虽然状态文本框采用简短消息，文件、下载、编译器和错误的技术细节仍保存在诊断信息中。不会上传任何内容。日志可能包含 Windows 用户名和完整路径：分享前请检查。主动导出到别处的副本在卸载后仍保留。

安装 MelonLoader 后第一次手动启动时，它可能下载支持文件并准备游戏程序集。请等待大约一分钟，某些系统可能更久。MelonLoader 加载模组之前，模组无法朗读。请保持游戏打开，等待 Bop It Access 启动播报，然后等到标题、欢迎画面或主菜单播报后再使用游戏控制。


### 不运行Bop It Access EXE，手动安装编译ZIP

BopItAccess-v1.0.zip发布后，此方式使用已编译DLL，无需.NET SDK。仍需购买的Windows x64游戏、官方x64 MelonLoader 0.7.3 Open-Beta和Windows x64 .NET 6运行时。请遵循MelonLoader及Microsoft官方安装说明；ZIP不包含这些前置软件。

[MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer) · [.NET 6 Windows x64 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

1. 通过Steam安装游戏并找到文件夹。修改文件前关闭Bop It!；需要时使用Steam浏览已安装文件的功能。
2. 在该游戏文件夹安装官方x64 MelonLoader，并确认.NET 6 x64运行时已安装。暂勿启动游戏，先放置模组。
3. 将编译好的BopItAccess-v1.0.zip解压至临时文件夹。把Mods/BopItAccess.dll复制到游戏的Mods，创建或合并文件夹时不要删除其他模组。将prism.dll复制到BopIt!.exe旁。
4. 完整复制documentation和THIRD-PARTY-LICENSES，保留全部语言子文件夹及Prism通知和许可。复制包内README.txt与BopItAccess-uninstall.ps1。此脚本只是指向安装程序管理的卸载工具的快捷方式；复制不会建立可用卸载工具或Windows应用登记。
5. 谨慎处理UserData/Loader.cfg：不存在时复制模板；已存在时只合并对应章节的[loader] disable_start_screen=true和[console] hide_console=true，保留其他设置。不要用模板覆盖已有配置。
6. 先打开使用的屏幕阅读器，再通过Steam启动游戏。模组已放入Mods后，MelonLoader可能在首次启动下载支持文件并生成程序集。等待模组启动及菜单朗读后再操作游戏。

手动更新时关闭游戏，将新包的模组、Prism、文档和许可复制到相同位置。保留BopItAccess.ini、其他模组及无关文件；按上述方式合并Loader.cfg。停用或删除手动模组时，仅删除Mods/BopItAccess.dll。进一步清理时，只删除为此模组复制的文件及UserData/BopItAccess.ini或其.tmp文件；若其他组件使用Prism/MelonLoader，则保留共享文件。Windows保存的偏好可能仍在。手动ZIP没有所有权记录或登记的卸载工具。之后若选择使用安装程序，Uninstall可识别旧手动模组并提供偏好清理，同时保护所有权不明的文件。 模组日志为Mods/BopItAccess.log和Mods/BopItAccess.log.previous；清理时只删除这些已知模组日志。

高级打包：scripts/package-mod.ps1将已编译且版本匹配的模组与已知文档、配置、Prism文件打包。检查源代码/DLL版本，排除游戏、生成及旧格式文件；不进行编译。压缩包及暂存内容留在本地。
