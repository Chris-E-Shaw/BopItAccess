# Bop It Access

Bop It Access 是 **Bop It!** 的 Windows Steam 版本的非官方辅助功能模组。它使用 MelonLoader 和 Tolk 向菜单和游戏屏幕添加语音和盲文反馈。目前的功能包括首次运行的欢迎屏幕、游戏内用户指南、语音标题和暂停屏幕、设置和控制、歌曲选择、最终分数和排行榜、成就、积分、按钮提示、带有回合前当前控制分配的点播教程文本以及四个阶段的描述。版本 0.8.0 遵循游戏选择的语言，并包含游戏提供的每种语言的指南。

## 项目状况

该项目正处于早期开发阶段。该存储库包含源代码和技术文档。 **这里还没有编译的版本或 GitHub 版本。** 要使用此存储库中的 mod，请从源代码构建它并提供下面描述的 Tolk 运行时文件。

提交历史记录包括 37 个早期版本的重构源快照。当这些档案导入 Git 时，就会创建提交；他们的日期不是最初的构建日期。的 [技术构建历史](BopItAccess-build-history.html) 描述每个快照背后的工作。

## 要求

- Windows x64 和您自己安装的 Bop It! 用于 Steam。
- MelonLoader安装在游戏目录中。开发使用了 MelonLoader **0.7.3 Open-Beta** 和 x64 Unity **2022.3.50f1** 游戏版本。其他组合尚未得到验证。
- 带有 **.NET 6 目标包** 的 .NET SDK，因为 mod 目标 `net6.0`.
- 安装时，兼容64位 `Tolk.dll` 和 `nvdaControllerClient64.dll` 运行时文件。这些第三方二进制文件不在此存储库中。

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
2. 获取兼容的 64 位 `Tolk.dll` 来自可信来源或 [从上游 Tolk 源构建它](https://github.com/dkager/tolk#compiling)。获取匹配的 `nvdaControllerClient64.dll` 来自 [Tolk的x64库目录](https://github.com/dkager/tolk/tree/master/libs/x64) 或您的 Tolk 版本。将 **两个 DLL 放入游戏目录**，放在游戏可执行文件旁边，而不是放在里面 `Mods`.
3. 复制整个构建 `src\bin\Release\net6.0\documentation\` 文件夹放入游戏目录。它包含其根部的英文指南和翻译后的指南 `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, 和 `pt-BR`。保留这些子文件夹和配套文档。游戏内指南每次打开时都会读取当前游戏语言的 HTML，因此替换指南会更新其内容，而无需重建 DLL。
4. 如果您使用屏幕阅读器，请启动屏幕阅读器，然后启动 Bop It! 到 Steam。当没有支持的屏幕阅读器运行时，该 mod 可以使用 SAPI 语音。

Bop It Access build命令仅编译此mod；它不会构建或下载 Tolk。如果语音未开始，请检查 `<game directory>\Mods\BopItAccess.log`。该日志记录了 Tolk 是否初始化并接受了语音请求，尽管仅凭这一点并不能证明听到了音频。

首次运行时，游戏主菜单准备就绪后会出现欢迎屏幕。它的选择是打开模组设置、阅读游戏中的用户指南或继续游戏。 Mod 设置还提供 **打开用户指南** 和确认的 **重置欢迎屏幕** 操作，该操作会在下次启动时显示欢迎屏幕。在指南中，使用向上/向下选择主题或阅读行，然后使用确认打开主题。在表中，“左”向左移动一列，“右”向右移动一列，“向上/向下”在更改行时保留当前列。列标题标记单元格而不是显示为数据行；该表在进入时宣布，并在退出时结束。后面留下一个主题或指南。

在游戏的 **设置 > 语言** 行中选择一种语言。 Mod 语音遵循该选择。游戏内指南使用匹配的翻译 HTML 文档，如果所选副本丢失或不可读，则使用英语作为后备。捆绑的非英语文本是机器翻译的第一遍；欢迎能说流利的人指正。

mod使用游戏翻译的名字进行游戏游戏动作. Shapes,Space,City,Office作为固定舞台标题保留英语. 选中的语音输出需要您语言的声音 。 对于SAPI的输出,如果系统默认语音发出错误的声音,则选择适合您语言的安装语音.

## 文档

- [游戏和模组用户指南](BopItAccess-user-guide.html) — 适合初学者的控制、设置、菜单和播放模式的演练。
- [详细的功能和控制指南](README.txt)。它的安装部分描述了本地准备的安装 ZIP；此 GitHub 存储库仅提供源代码。
- [技术构建历史](BopItAccess-build-history.html).
- [此项目的 Git 工作流程](GIT-WORKFLOW.md).
- [第三方通知](THIRD-PARTY-NOTICES.txt).

上述所有六份文件的翻译副本位于 [`documentation/`](../) 在每种支持的语言代码下。它们的来源是英语； `scripts/translate_documents.py` 可以在源更改后重新生成机器翻译的草稿。

## 人工智能透明度

Christopher Shaw 指导该项目并评估其在游戏中的可访问性。 OpenAI Codex 模型协助研究、代码和文档。发布的提交消息包括 `Co-authored-by` 预告片识别促成每次变更的模型；历史学分已根据该项目的会话记录进行检查。早期的构建历史是根据保存的源档案重建的，而不是记录为当时的提交。人工智能辅助的贡献可能包含错误，应在使用前进行审查。

## 许可

尚未选择 Bop It Access 源的许可证。 Tolk和NVDA控制器客户端有自己的许可证；看到 [第三方通知](THIRD-PARTY-NOTICES.txt)。 Bop It! 及其资产属于其各自所有者，不包含在此。
