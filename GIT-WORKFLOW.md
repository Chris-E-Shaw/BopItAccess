# Git workflow for Bop It Access

Git keeps a history of changes to the project's source code and documents. A **commit** is a named snapshot you can inspect or return to. A **tag** gives a particular commit a stable version name, such as `v0.6.12`. Git works entirely on this computer; GitHub is an optional place to share or back up the repository later.

## About the existing history

The source archives for builds `v0.1.0` through `v0.6.12` were imported as successive Git commits with matching version tags. These commits reconstruct the released source snapshots. They were created during the Git import, so their Git timestamps are **not** the original build dates. [BopItAccess-build-history.html](BopItAccess-build-history.html) contains the detailed technical notes for each build.

Release ZIP files, compiled DLLs, and temporary build output stay outside Git's source history. Git tags preserve the source for each version.

## Useful commands

Open PowerShell in this project directory, then run:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline --decorate       # Browse commits and version tags
git show --stat v0.6.12            # See files changed in a tagged release
```

These commands only inspect the repository; they do not change the mod or the installed game.

## For each future build

1. Make the source changes and choose the next version number.
2. Add an entry for that build to `BopItAccess-build-history.html`, using the existing commit-style format. Describe the actual change, its reason, and any relevant limitations. Keep the document up to date **before** the release commit.
3. Build the mod and prepare its release archives as usual. Inspect the result before calling it a release.
4. Run `git status` and `git diff`. Check which files changed. Stage the intended source and documentation changes with `git add -A`, then review them with `git diff --cached`.
5. Create one descriptive release commit, for example `git commit -m "feat(speech): add example setting"`.
6. Tag that commit with the build's version, for example `git tag v0.6.13`. Use the actual version number for the build. A tag should point to the commit containing both its source and its build-history entry.

Small work that does not produce a build can have its own commit without a version tag. The next release commit can then follow it. Keep personal logs, game installations, generated binaries, and other machine-specific files out of commits.

Git does not automatically upload anything. If we later decide to publish or back up the project on GitHub, we can add a remote repository and push the commits and tags deliberately.
