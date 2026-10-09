# Bop It Access 的 Git 工作流程

Git 保存本项目源代码和文档的历史记录。**提交（commit）**是一个带有说明的快照，您可以查看它，也可以回到它记录的状态。GitHub 发布这些提交，让其他人阅读更改并自行构建项目。创建提交不会自动生成公开发行版。

## 关于现有历史

最初的 37 个源代码构建，从 `0.1.0` 到 `0.6.12`，已根据留存的源代码存档和原始修改说明分别导入为独立提交。它们的 Git 时间戳记录导入时间，而不是最初的构建日期。之后的工作直接记录在源代码提交中。现在 Git 和 GitHub 就是项目的修改历史，不再维护独立的构建历史文档。

提交作者使用 Christopher Shaw 的 GitHub no-reply 地址。AI 参与的提交带有 `Co-authored-by` 尾注，注明实际贡献的模型。会话记录表明，第一个历史构建由 GPT-6 Luna 参与，之后的 36 个由 GPT-6 Sol 参与。今后的提交应使用参与该工作模型的当前名称。

已编译的 DLL、安装程序、发行 ZIP、生成的游戏程序集、个人日志和临时构建输出不进入 Git 的源代码历史。本地版本标签不会自动发布。创建 GitHub 发行版是需要另行明确执行的操作。

## 常用命令

在仓库文件夹中打开 PowerShell，然后运行：

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

这些命令依次用于查看已更改、已添加和未跟踪的文件，检查尚未暂存的更改，浏览提交，以及查看上一次提交中更改的文件。它们只检查仓库，不会更改已安装的模组或游戏。

## 今后每次修改的步骤

1. 完成所需的源代码与文档修改。新构建还需要更新版本号。
2. 更新所有受到影响的翻译指南。编译后的输出应包含玩家文档和许可证声明；开发者 README 和本工作流程不是面向玩家的发行文件。
3. 如果修改需要新的二进制文件，就进行编译并准备本地文件。游戏测试由人类玩家在提出测试要求时进行；不要仅凭编译成功就宣称已验证运行时行为。
4. 运行 `git status` 和 `git diff`。暂存需要的文件，然后检查 `git diff --cached`。不要将生成的二进制文件、私人日志或游戏引用文件纳入暂存的更改。
5. 创建说明清楚的提交，标题不包含版本号。正文解释修改了什么、为什么修改，以及相关检查和限制。如果 AI 作出了贡献，在共同作者尾注中注明实际模型：

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   将 `MODEL NAME` 替换为完成该工作的模型名称。作者仍为 Christopher Shaw，地址使用 `336230252+Chris-E-Shaw@users.noreply.github.com`。
6. 更改准备好发布后，运行 `git push origin main`。此操作发布分支提交，不会推送本地标签，也不会创建发行版。

一个完整、统一的修改可以在同一提交中包含源代码和文档。如果记录彼此独立的修改，分成不同提交也很有用。Git 不会自动上传工作：准备好让其他人阅读后，再明确执行推送。
