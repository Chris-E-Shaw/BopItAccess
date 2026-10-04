# Bop It Access

Bop It Access is an unofficial accessibility mod for the Windows Steam version of **Bop It!**. It uses MelonLoader and [Prism](https://github.com/ethindp/prism) to add speech and braille feedback to menus and game screens. Current features include a first-run welcome screen, an in-game user's guide, spoken title and pause screens, settings and controls, song selection, final scores and leaderboards, achievements, credits, button hints, on-demand tutorial text with current control assignments before a round, and descriptions of the four stages. Version 0.9.0 uses Prism for speech and braille output. The mod follows the game's selected language and includes a guide for every language the game offers.

## Project status

This project is in early development. The GitHub repository contains source code and technical documentation. **There are no GitHub releases yet.** The source now also contains a Windows installer project. Until a release is published, its **Install** button explains that no release is available; **Install alpha** builds the latest main-branch commit from source.

The commit history includes reconstructed source snapshots of 37 earlier builds. The commits were created when those archives were imported into Git; their dates are not the original build dates. The [technical build history](BopItAccess-build-history.html) describes the work behind each snapshot.

## Requirements

- Windows x64 and your own installation of Bop It! for Steam.
- MelonLoader installed in the game's directory. Development has used MelonLoader **0.7.3 Open-Beta** with the x64 Unity **2022.3.50f1** game build. Other combinations have not been verified.
- A .NET SDK with the **.NET 6 targeting pack**, because the mod targets `net6.0`.
- For installation, the official Windows x64 Prism v0.18.3 `prism.dll`. This third-party binary is not in this repository.

The mod references DLLs generated or installed by MelonLoader under the game directory. It does not include or redistribute game assemblies.

## Windows installer preview

The installer source is in [`installer/`](installer/). It is a self-contained, x64 Windows Forms application. Its controls have keyboard access keys and accessible names. The status log is a selectable, read-only text field, and a progress bar shows the current download, extraction, or installation step. It requests administrator permission because Bop It! is commonly installed under Program Files and Windows' Installed Apps entry uses the machine registry.

The installer searches Steam libraries across available drives, and its **Browse** button accepts another game folder. **Install** uses the latest GitHub release ZIP when one exists. **Install alpha** warns before downloading the current main-branch source, compiling it on the player's computer, and installing that build. The alpha path may launch Bop It! once to let MelonLoader generate game-specific build references, then waits for the game to close before compiling. The installer downloads checksum-verified MelonLoader 0.7.3, Prism 0.18.3, and a portable .NET 6 SDK when they are missing. The SDK stays in the game's `dotnet` folder after uninstall; it includes the .NET 6 targeting pack. Existing suitable dependencies are reused.

Installed files are recorded in an ownership manifest so updates preserve pre-existing files and an aborted installation can reverse its own changes. **Uninstall** and Windows **Installed Apps** use the same uninstall code. The supplied `installer/uninstall.ps1` launches an accessible uninstall window from Installed Apps, then removes the uninstall launcher and manifest after successful removal. Existing unrelated mods and their shared MelonLoader files are preserved.

For an older hand-installed copy that has no ownership manifest, uninstall removes identifiable Bop It Access files and leaves shared files whose origin cannot be proved. Uninstall also removes only `BopItAccess.*` preference values from each local Windows user profile, including profiles that are signed out. The game's own preferences remain. If Windows denies access to a profile, the installer records a warning in its status log and reports that cleanup was incomplete.

To build the installer executable from source on a Windows development machine with the .NET 10 SDK, run:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer-preview --no-restore
```

The executable is `build\installer-preview\BopItAccess.Installer.exe`. It contains the runtime and does not require .NET 10 on the player's computer. It is currently unsigned, so Windows may show its standard unknown-publisher warning. Do not run installation or removal while Bop It! is open.

When publishing the first mod release, attach a ZIP with `Mods/BopItAccess.dll` and the complete `documentation/` directory. The installer fetches Prism and MelonLoader from their official releases rather than from that ZIP. It reads the latest release through GitHub's Releases API, so the installer executable can continue to find later releases without being rebuilt.

## Build from source

1. Install MelonLoader, start Bop It! once, and then close the game. MelonLoader should create `MelonLoader\Il2CppAssemblies` beneath the game directory.
2. Clone or download this repository. Open PowerShell in the repository's root directory.
3. Set `$gameDir` to **your** Bop It! installation directory, then build:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   The example path is Steam's usual Windows location. Change it if your Steam library is elsewhere. The project checks for the required MelonLoader and generated game DLLs and reports a missing path before compiling.

4. The built mod DLL will be at `src\bin\Release\net6.0\BopItAccess.dll`.

If the SDK reports a missing .NET 6 targeting pack, install an SDK that includes that pack. The project's `NuGet.Config` does not configure online package feeds.

## Install your build

1. Close the game. Copy the built `BopItAccess.dll` into `<game directory>\Mods\`. Create the `Mods` directory if MelonLoader has not created it.
2. Obtain the official Windows x64 Prism v0.18.3 `prism.dll` from the [Prism releases](https://github.com/ethindp/prism/releases), or build the matching Prism version from source. Put `prism.dll` in the game directory, beside the game executable, rather than inside `Mods`.
3. Copy the build's entire `src\bin\Release\net6.0\documentation\` folder into the game directory. It contains the English guide at its root and translated guides under `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, and `pt-BR`. Keep those subfolders and the companion documents. The in-game guide reads the HTML for the current game language each time it opens, so replacing a guide updates its content without rebuilding the DLL.
4. Start your screen reader if you use one, then launch Bop It! through Steam. When no supported screen reader is running, Prism prefers Windows OneCore speech and falls back to SAPI if OneCore is unavailable.

The Bop It Access build command compiles only this mod; it does not build or download Prism. If speech does not start, inspect `<game directory>\Mods\BopItAccess.log`. The log records Prism initialization and speech dispatch, though a successful dispatch alone cannot prove audio was heard.

On a first run, the welcome screen appears after the game's main menu is ready. Its choices open Mod Settings, read the user's guide in-game, or continue to the game. Mod Settings also offers **Open User's Guide** and a confirmed **Reset Welcome Screen** action that shows the welcome screen on the next launch. In the guide, use Up/Down to choose topics or read lines and Confirm to open a topic. Within tables, Left moves one column left, Right moves one column right, and Up/Down keeps the current column while changing rows. Column headings label cells rather than appearing as data rows; the table is announced on entry and its end on exit. Back leaves a topic or the guide.

Choose a language in the game's **Settings > Language** row. Mod speech follows that selection. The in-game guide uses the matching translated HTML document, with English as a fallback if the selected copy is missing or unreadable. The bundled non-English text is a machine-translated first pass; fluent-speaker corrections are welcome.

The mod uses the game's translated names for gameplay actions. Shapes, Space, City, and Office stay in English as fixed stage titles. The selected speech output needs a voice for your language. In Mod Settings, the voice, volume, rate, and pitch rows follow the active OneCore or SAPI engine; each engine keeps its own choices. These rows are hidden when the active output does not offer those controls. Choose an installed voice suited to your language if the system default sounds wrong.

## Documentation

- [Game and mod user's guide (English)](BopItAccess-user-guide.html) — a beginner-friendly walkthrough of controls, settings, menus, and play modes.
- [Japanese user's guide (日本語)](documentation/ja/BopItAccess-user-guide.html). Other translated guides are available in the language folders under [`documentation/`](documentation/).
- [Detailed feature and control guide](README.txt). Its installation section describes the locally prepared install ZIPs; this GitHub repository provides source only.
- [Technical build history](BopItAccess-build-history.html).
- [Git workflow for this project](GIT-WORKFLOW.md).
- [Third-party notices](THIRD-PARTY-NOTICES.txt).

Translated copies of all six documents above are in [`documentation/`](documentation/) under each supported language code. The source for them is English; `scripts/translate_documents.py` can regenerate the machine-translated drafts after source changes.

## AI transparency

Christopher Shaw directs this project and evaluates its accessibility in the game. OpenAI Codex models have assisted with research, code, and documentation. Published commit messages include a `Co-authored-by` trailer identifying the model that contributed to each change; the historical credits were checked against this project's session records. The earlier build history was reconstructed from saved source archives rather than recorded as commits at the time. AI-assisted contributions can contain mistakes and should be reviewed before use.

## Licensing

A license for the Bop It Access source has not yet been selected. Prism has its own license; see the [third-party notices](THIRD-PARTY-NOTICES.txt). Bop It! and its assets belong to their respective owners and are not included here.
