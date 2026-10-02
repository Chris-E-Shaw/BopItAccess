# Bop It Access 的 Git 工作流程

Git 保留项目源代码和文档的更改历史记录。 **提交**是您可以检查或返回的命名快照。 GitHub 发布了此源历史记录，以便其他人可以阅读代码并自己构建 mod。该存储库不包含已编译的 mod 文件或 GitHub 版本。

## 关于现有的历史

构建的源档案 `v0.1.0` 通过 `v0.6.12` 被导入为 37 个连续的 Git 提交。每个提交都使用相应的条目描述源更改 [BopItAccess-build-history.html](BopItAccess-build-history.html)。这些提交是在 Git 导入期间创建的，因此它们的 Git 时间戳**不是**原始构建日期。他们的主题描述了没有版本号的更改；构建历史文档记录了哪个源快照属于每个版本。

发布 ZIP 文件、编译的 DLL 和临时构建输出都保留在 Git 的源历史记录之外。构建历史页面链接到相应的 GitHub 源提交。现有版本标签保留在本地，并且不属于初始 GitHub 发布的一部分。尚未发布 GitHub 版本标签或版本。

Git 提交使用 Christopher Shaw 的 GitHub 无回复地址作为作者。使用 Codex 编写的提交还包括 `Co-authored-by` 预告片命名了对这项工作做出贡献的模型。会话记录标识第一个历史构建的 GPT-6 Luna 和接下来的 36 个构建的 GPT-6 Sol。如果模型在以后的提交中发生更改，请在该提交的预告片中使用其新名称。

## 有用的命令

在该项目目录下打开PowerShell，然后运行：

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

这些命令仅检查存储库；他们不会更改模组或已安装的游戏。

## 对于每个未来的构建

1. 进行源更改并选择下一个构建版本号。
2. 像往常一样构建 mod 并准备本地档案。包括整个 `documentation` 文件夹，其中包含英语指南和每个翻译语言的子文件夹，位于每个安装存档中。安装版本时将该文件夹复制到游戏安装中。游戏内指南会在每次打开时读取当前游戏语言的 HTML。在将构建记录为完成之前检查结果。编译后的档案保留在GitHub之外。
3. 运行 `git status` 和 `git diff`。检查哪些文件发生了变化。暂存预期的源代码和文档更改，然后使用 `git diff --cached`.
4. 创建主题中不包含版本号的描述性源提交。包括一个 `Co-authored-by` 预告片与 Codex 写入提交时的实际模型名称。例如， `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`;替换 `MODEL NAME` 以及用于该提交的模型。
5. 添加一个构建条目 `BopItAccess-build-history.html`，使用现有的提交样式格式。描述实际更改、其原因以及任何相关限制，并链接步骤 4 中的源提交。在打包之前更新相应的翻译副本。使用同一作者和准确的合著者预告片提交更新的历史记录。如果历史文件已复制到本地文档文件夹和存档中，请刷新它们。
6. 发布源代码和历史提交 `git push origin main` 当准备好时。这只会推动分支；它不会推送本地版本标签或创建 GitHub 版本。

不生成构建的小型工作可以有自己的提交。下一个构建提交可以遵循它。不要提交个人日志、游戏安装、生成的二进制文件和其他特定于计算机的文件。如果 GitHub 版本稍后有用，请当时决定标签和编译的下载。

Git 不会自动上传新作品。每次本地提交后，当它准备好供其他人查看时，故意推送它。
