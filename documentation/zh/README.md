# Bop It Access

Bop It Access 是 **Bop It!** 的 Windows Steam 版本的非官方辅助功能模组。它使用 MelonLoader 和 [Prism](https://github.com/ethindp/prism) 将语音和盲文反馈添加到菜单和游戏屏幕。目前的功能包括首次运行的欢迎屏幕、游戏内用户指南、语音标题和暂停屏幕、设置和控制、歌曲选择、最终分数和排行榜、成就、积分、按钮提示、带有回合前当前控制分配的点播教程文本以及四个阶段的描述。版本 0.9.0 使用 Prism 进行语音和盲文输出。该模组遵循游戏选择的语言，并包含游戏提供的每种语言的指南。

## 项目状况

该项目正处于早期开发阶段。该存储库包含源代码和技术文档。 **这里还没有编译的版本或 GitHub 版本。** 要使用此存储库中的 mod，请从源代码构建它并提供下面描述的 Prism 运行时。

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

**优化朗读格式**: 将全大写英文调整为自然的大小写，仅用于语音和盲文。在游戏内指南中，如果一行结尾没有标点，就在行号前加上省略号，留出停顿。屏幕上的文字不变。关闭后按原文输出。

## 文档

- [游戏和模组用户指南（英文）](BopItAccess-user-guide.html) — 适合初学者的控制、设置、菜单和播放模式的演练。
- [日语用户指南（日本语）](../ja/BopItAccess-user-guide.html)。其他翻译的指南可在以下语言文件夹中找到 [`documentation/`](../).
- [详细的功能和控制指南](README.txt)。它的安装部分描述了本地准备的安装 ZIP；此 GitHub 存储库仅提供源代码。
- [技术构建历史](BopItAccess-build-history.html).
- [此项目的 Git 工作流程](GIT-WORKFLOW.md).
- [第三方通知](THIRD-PARTY-NOTICES.txt).

上述所有六份文件的翻译副本位于 [`documentation/`](../) 在每种支持的语言代码下。它们的来源是英语； `scripts/translate_documents.py` 可以在源更改后重新生成机器翻译的草稿。

## 人工智能透明度

Christopher Shaw 指导该项目并评估其在游戏中的可访问性。 OpenAI Codex 模型协助研究、代码和文档。发布的提交消息包括 `Co-authored-by` 预告片识别促成每次变更的模型；历史学分已根据该项目的会话记录进行检查。早期的构建历史是根据保存的源档案重建的，而不是记录为当时的提交。人工智能辅助的贡献可能包含错误，应在使用前进行审查。

## 许可

尚未选择 Bop It Access 源的许可证。 Prism 有自己的许可证；看到 [第三方通知](THIRD-PARTY-NOTICES.txt)。 Bop It! 及其资产属于其各自所有者，不包含在此。
