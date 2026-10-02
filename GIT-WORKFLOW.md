# Git workflow for Bop It Access

Git keeps a history of changes to the project's source code and documents. A **commit** is a named snapshot you can inspect or return to. GitHub publishes this source history so others can read the code and build the mod themselves. The repository does not contain compiled mod files or GitHub Releases.

## About the existing history

The source archives for builds `v0.1.0` through `v0.6.12` were imported as 37 successive Git commits. Each commit describes the source changes using the corresponding entry in [BopItAccess-build-history.html](BopItAccess-build-history.html). These commits were created during the Git import, so their Git timestamps are **not** the original build dates. Their subjects describe the changes without version numbers; the build-history document records which source snapshot belongs to each version.

Release ZIP files, compiled DLLs, and temporary build output stay outside Git's source history. The build-history page links to the corresponding GitHub source commits. Existing version tags remain local and are not part of the initial GitHub publication. No GitHub version tags or Releases are published yet.

Git commits use Christopher Shaw's GitHub no-reply address as the author. Commits written with Codex also include a `Co-authored-by` trailer naming the model that contributed to the work. Session records identify GPT-6 Luna for the first historical build and GPT-6 Sol for the following 36. If the model changes for a later commit, use its new name in that commit's trailer.

## Useful commands

Open PowerShell in this project directory, then run:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

These commands only inspect the repository; they do not change the mod or the installed game.

## For each future build

1. Make the source changes and choose the next build version number.
2. Build the mod and prepare local archives as usual. Include `documentation/BopItAccess-user-guide.html` and its companion documents in every install archive, and copy that folder into the game installation when installing a build. The in-game guide reads the installed HTML on every opening. Inspect the result before recording the build as complete. Compiled archives remain outside GitHub.
3. Run `git status` and `git diff`. Check which files changed. Stage the intended source and documentation changes, then review them with `git diff --cached`.
4. Create a descriptive source commit without a version number in its subject. Include a `Co-authored-by` trailer with the actual model name when Codex wrote the commit. For example, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; replace `MODEL NAME` with the model used for that commit.
5. Add an entry for the build to `BopItAccess-build-history.html`, using the existing commit-style format. Describe the actual change, its reason, and any relevant limitations, and link the source commit from step 4. Commit the updated history with the same author and accurate co-author trailer. Refresh the local documentation folder and archive if the history file was already copied into them.
6. Publish both source and history commits with `git push origin main` when ready. This pushes the branch only; it does not push local version tags or create GitHub Releases.

Small work that does not produce a build can have its own commit. The next build commit can then follow it. Keep personal logs, game installations, generated binaries, and other machine-specific files out of commits. If GitHub Releases become useful later, decide on tags and compiled downloads at that time.

Git does not automatically upload new work. After each local commit, push it deliberately when it is ready for other people to see.
