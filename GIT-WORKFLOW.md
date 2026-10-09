# Git workflow for Bop It Access

Git keeps the history of this project's source code and documentation. A **commit** is a named snapshot you can inspect or return to. GitHub publishes these commits so others can read the changes and build the project themselves. A commit does not automatically create a public release.

## About the existing history

The first 37 source builds, from `0.1.0` through `0.6.12`, were imported as separate commits using the available source archives and their original change notes. Their Git timestamps record the import rather than the original build dates. Later work is recorded directly in source commits. Git and GitHub are now the project's change history; no separate build-history document is maintained.

Christopher Shaw's GitHub no-reply address is the commit author. AI-assisted commits include a `Co-authored-by` trailer naming the actual model that contributed. Session records identify GPT-6 Luna for the first historical build and GPT-6 Sol for the following 36. Use the contributing model's current name for future commits.

Compiled DLLs, installers, release ZIPs, generated game assemblies, personal logs and temporary build output stay out of Git's source history. Local version tags are not published automatically. Creating a GitHub release is a separate, deliberate step.

## First public release

[Version 1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0) is the first public release, identified by the `v1.0` tag. Its mod and installer source versions are `1.0.0`. The release attaches `BopItAccess-Installer.exe` and `BopItAccess-v1.0.zip`; GitHub also provides Source code (zip) and Source code (tar.gz), for four downloads in total. Compiled downloads are release assets, not files committed to the source branch.

## Useful commands

Open PowerShell in the repository, then run:

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

These commands inspect the repository without changing the installed mod or game.

## For each future change

1. Make the intended source and documentation changes. For a new build, update its version.
2. Update every affected translated guide. Keep the player documentation and license notices with the compiled output; developer READMEs and this workflow are not player-release files.
3. Compile when the change needs a new binary and prepare local files. Gameplay testing is performed by human players when requested; do not claim runtime verification from compilation alone.
4. Run `git status` and `git diff`. Stage the intended files, then inspect `git diff --cached`. Keep generated binaries, private logs and game references out of the staged changes.
5. Make a descriptive commit whose subject has no version number. In its body, explain what changed and why, plus any relevant checks and limitations. Include the actual AI model in a co-author trailer when it contributed:

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Replace `MODEL NAME` with the model that wrote the work. Keep Christopher Shaw as the author, using `336230252+Chris-E-Shaw@users.noreply.github.com`.
6. When the changes are ready to publish, run `git push origin main`. This publishes the branch commits, without pushing local tags or creating a release.

One coherent change can contain its source and documentation in the same commit. Separate commits remain useful when they describe independent changes. Git does not upload work automatically: push deliberately when it is ready for others to read.
