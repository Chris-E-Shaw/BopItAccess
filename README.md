# Bop It Access

Bop It Access is an unofficial accessibility mod for the Windows Steam version of **Bop It!**. It uses MelonLoader and [Prism](https://github.com/ethindp/prism) to add speech and braille feedback to menus and game screens. Current features include a first-run welcome screen, an in-game user's guide, spoken title and pause screens, settings and controls, song selection, final scores and leaderboards, achievements, credits, button hints, on-demand tutorial text with current control assignments before a round, and descriptions of the four stages. Version 0.9.0 uses Prism for speech and braille output. The mod follows the game's selected language and includes a guide for every language the game offers.

## Editing the settings file

If a language, loud game audio, or a troublesome voice makes the menus difficult to use, you can change the settings outside the game. After startup, the mod automatically creates UserData/BopItAccess.ini inside your Bop It! game folder, using your current settings. This is a plain-text file you can open with an editor such as Notepad.

The file includes the game’s language, music, effects and voice-over volumes, vibration, full screen, resolution and audio latency; mod speech, braille, hint and other preferences; separate OneCore and SAPI voice profiles; and player-facing game and mod input bindings. Available resolutions and installed voices are listed in comments.

Close the game before editing. Find the relevant section and change the value of the existing entry, save the file, then launch the game again. Changes are read on launch, not immediately while the game is running. Changes made through the game’s menus update the file automatically.

Section and setting names stay in English in every language so they remain consistent. On and Off are the recommended toggle values; True/False, Yes/No and 1/0 are also accepted. The comments beside settings explain their choices and ranges. Missing entries and invalid values leave the corresponding saved setting unchanged; other valid edits still apply. Duplicate input assignments are rejected.

Comments and unknown entries are kept. If another program changes the file while the game is running, the mod stops saving to that file for the rest of the session to protect those edits. Close and reopen the game to use them. You can keep a backup before making changes.

For a voice problem, set Voice=System default in the OneCore or SAPI section. OneCore voice choices use name | language; SAPI accepts an installed voice’s displayed name or its full registry ID. The file lists available choices. OutputMode=Auto tries a running supported screen reader, then OneCore, then SAPI.

The example below restores English, quieter game audio and speech with automatic output and system-default voices. Change the matching entries already in your file; this is a reference excerpt, not an extra block to append. Leave your other settings in place.

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

## Project status

This project is essentially complete, and no major content or features are planned. It will be maintained as needed, with player feedback guiding improvements. The GitHub repository contains source code and technical documentation. **There are no GitHub releases yet.** The source now also contains a Windows installer project. Until a release is published, its **Install** button explains that no release is available; **Install alpha** builds the latest main-branch commit from source.

The commit history includes reconstructed source snapshots of 37 earlier builds. The commits were created when those archives were imported into Git; their dates are not the original build dates. The [technical build history](BopItAccess-build-history.html) describes the work behind each snapshot.

## Requirements

- Windows x64 and your own installation of Bop It! for Steam.
- MelonLoader installed in the game's directory. Development has used MelonLoader **0.7.3 Open-Beta** with the x64 Unity **2022.3.50f1** game build. Other combinations have not been verified.
- The Windows x64 **.NET 6 runtime** to run the mod. A compatible **.NET SDK and .NET 6 targeting pack** are needed to build from source, including Install alpha, but not to install a compiled release.
- For installation, the official Windows x64 Prism v0.18.3 `prism.dll`. This third-party binary is not in this repository.

The mod references DLLs generated or installed by MelonLoader under the game directory. It does not include or redistribute game assemblies.

## Windows installer preview

The installer source is in [`installer/`](installer/). It is a self-contained, x64 Windows Forms application. Its controls have keyboard access keys and accessible names. The status log is a selectable, read-only text field, and a progress bar shows the current download, extraction, or installation step. It requests administrator permission because Bop It! is commonly installed under Program Files and Windows' Installed Apps entry uses the machine registry.

The installer searches Steam libraries across available drives, and its **Browse** button accepts another game folder. **Install** uses the latest GitHub release ZIP when one exists. **Install alpha** warns before downloading the current main-branch source and compiling it on the player's computer. Installer preview **0.1.7 never launches the game**. Before deployment, alpha reuses complete local game references or generates temporary Cpp2IL/Il2CppInterop build references from the player's own game files without executing the game. After preparing the mod, it deploys MelonLoader and immediately copies `Mods/BopItAccess.dll`, then finishes Prism, configuration, documentation and uninstall support. The player launches Bop It! manually when ready. See [`installer/INSTALLATION-FLOW.md`](installer/INSTALLATION-FLOW.md) for the technical sequence.

A compiled release needs the Windows x64 .NET 6 runtime, not a development SDK. Existing complete runtimes are reused. If no runtime is available, the installer downloads Microsoft’s official .NET 6.0.36 runtime ZIP and places it in MelonLoader/Dependencies/dotnet, a supported loader location. These files are recorded for rollback and uninstall; they are retained if other mods still need the shared loader. Install alpha also needs a compatible SDK and the .NET 6 targeting pack to compile the source. It reuses an installed SDK or a compatible dotnet folder left by an older installer; if necessary, it installs an official Microsoft SDK system-wide. The SDK remains after uninstall or abort. This preview never creates a new dotnet SDK folder at the game root. Official MelonLoader 0.7.3 and Prism 0.18.3 are obtained as needed. Updates still use GitHub; Abort asks for confirmation and reverses this installation’s game-file changes.

Installed files are recorded in an ownership manifest so updates preserve pre-existing files and an aborted installation can reverse its own changes. **Uninstall** and Windows **Installed Apps** use the same uninstall code. The supplied `installer/uninstall.ps1` launches an accessible uninstall window from Installed Apps. After a completely successful removal, it removes the uninstall launcher, ownership records and Windows entry. Existing unrelated mods and shared MelonLoader files are preserved. Both managed-install and older manual-install cleanup cover the known mod logs, including `Mods/BopItAccess.log.previous`, `UserData/BopItAccess.ini`, the older `UserData/BopItAccess.ini.tmp`, and validated `BopItAccess.ini.<GUID>.tmp` remnants in `UserData`. Here `<GUID>` must be exactly 32 hexadecimal characters without hyphens; arbitrary files matching a broad wildcard are not removed.

For an older hand-installed copy that has no ownership manifest, uninstall removes identifiable Bop It Access files and leaves shared files whose origin cannot be proved. Uninstall also removes only `BopItAccess.*` preference values from each local Windows user profile, including profiles that are signed out. The game's own preferences, shared .NET runtime/SDK components and an older game-folder SDK remain. If Windows denies access or another cleanup step cannot safely finish, the installer reports incomplete cleanup. For an installer-managed copy, its Windows entry, uninstall launcher and durable cleanup checkpoint remain available until cleanup succeeds, so the remaining steps can be retried. An older manual installation has no such durable ownership record; its warnings can be retried in the open installer.

Installer preview **0.1.7** records automatic local diagnostic sessions under `%ProgramData%\BopItAccess\diagnostics`. **Save diagnostics...** (Alt+D) saves the current UTF-8 transcript to a chosen `.log` or `.txt` file and keeps adding entries until the installer closes; **Copy diagnostics** (Alt+C) copies a current snapshot. Choose Save diagnostics **before the next installation or uninstall test**, so a complete exported recording remains available even after uninstall removes the automatic logs. Recording includes installer/runtime versions, timestamps, operation/state changes, progress milestones, sanitized HTTP results, compiler output and detailed errors. No diagnostics are uploaded. Logs are limited to ten sessions of up to 8 MiB each, with memory/export fallback if disk recording fails. Successful uninstall clears automatic diagnostics; intentionally exported copies remain. Review logs before sharing because they can contain Windows usernames and full paths. See [`installer/DIAGNOSTICS.md`](installer/DIAGNOSTICS.md) for recording limits and investigation instructions.

To build the installer executable from source on a Windows development machine with the .NET 10 SDK, run:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer-preview --no-restore
```

The executable is `build\installer-preview\BopItAccess.Installer.exe`. It contains the runtime and does not require .NET 10 on the player's computer. It is currently unsigned, so Windows may show its standard unknown-publisher warning. Do not run installation or removal while Bop It! is open.

When publishing the first mod release, attach a ZIP with `Mods/BopItAccess.dll` and the complete `documentation/` directory. The installer fetches Prism and MelonLoader from their official releases rather than from that ZIP. It reads the latest release through GitHub's Releases API, so the installer executable can continue to find later releases without being rebuilt.

On the first manual launch after installing MelonLoader, it may download support files and generate game assemblies. Allow about a minute, or longer on some systems. The mod cannot speak until MelonLoader finishes loading it. Keep the game open and wait for the Bop It Access startup announcement, then for the title-screen or menu announcement before using the controls.

## Build from source

The manual source-build steps below use references generated by a previous game launch. The supplied installer preview can prepare alpha-build references without launching the game; its workflow is described below.

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

The mod uses the game's translated names for gameplay actions. Shapes, Space, City, and Office stay in English as fixed stage titles. The selected speech output needs a voice for your language. In Mod Settings, Voice, Volume, Rate, and Pitch adjust the active OneCore or SAPI engine; each engine keeps its own choices. These rows are hidden when the active output does not offer those controls. Choose an installed voice suited to your language if the system default sounds wrong.

Speak Menu Indexes is the saved menu-position toggle, enabled by default. It adds the focused item’s position within its menu. One-on-One Feedback now identifies a player’s colour and new life total when a life is gained; a loss retains the count-only announcement. Audio calibration says “Done!” as soon as the final input is registered, before the measured result, and “Calibration failed.” if no input was made.

Format Speech makes all-capitals menu text sound more natural and adds a pause before guide line numbers when the line ends without terminal punctuation. It affects speech and braille text only; visible game text and the HTML guide stay unchanged. This setting is On by default and keeps your earlier Filter Capitalisation choice. Switch it Off to receive the original text.

### MelonLoader startup windows

The supplied Loader.cfg template hides MelonLoader’s separate start screen and console. The installer applies the same two defaults before you launch the game yourself. These settings do not skip the game’s title screen or the mod’s welcome screen.

With the game closed, open `UserData/Loader.cfg` in the game folder. If it already exists, set `disable_start_screen` to true in its existing `[loader]` section and `hide_console` to true in its existing `[console]` section. Keep all other entries. If the file does not exist, copy the supplied `UserData/Loader.cfg` template from the build output, or `configuration/Loader.cfg` from the source. Never overwrite an existing Loader.cfg with the whole template.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

The mod does not reset these options on each launch. You may manually change either value back to false if you need the loader’s windows for troubleshooting. Installer uninstall restores the original values only while the installer’s true values are still present, preserving other loader configuration edits.

## Documentation

- [Game and mod user's guide (English)](BopItAccess-user-guide.html) — a beginner-friendly walkthrough of controls, settings, menus, and play modes.
- [Japanese user's guide (日本語)](documentation/ja/BopItAccess-user-guide.html). Other translated guides are available in the language folders under [`documentation/`](documentation/).
- [Detailed feature and control guide](README.txt). Its installation section describes the locally prepared install ZIPs; this GitHub repository provides source only.
- [Technical build history](BopItAccess-build-history.html).
- [Release preparation code review](BopItAccess-release-review.html) — implemented findings, reviewed files, compilation results and remaining limits.
- [Git workflow for this project](GIT-WORKFLOW.md).
- [Third-party notices](THIRD-PARTY-NOTICES.txt).

Translated copies of the documents above are in [`documentation/`](documentation/) under each supported language code. The source for them is English; `scripts/translate_documents.py` can regenerate the machine-translated drafts after source changes.

## What may come next

This project is essentially complete, and no major content or features are planned. However, this mod will be actively maintained and updated over time as needed, with player feedback driving these improvements. Potential future work includes further review and bug fixes, code refinement, and continued improvements to speech responsiveness. Prism creates a possible path to other platforms in the future, but this mod currently only supports Windows x64. The project repository is the place to follow further development.

## AI Transparency Note

This mod is vibe-coded. All code was completely generated and researched by artificial intelligence, with limited technical human understanding of its underlying architecture. Please use this mod at your own risk.

That being said, every single mod feature and design decision was authored and approved by humans. Testing was never automated; it was carefully and extensively performed by real human players and testers.

Please note: Multilingual text and documentation were generated by AI and have not been reviewed by native speakers. High translation inaccuracy is to be expected. Without agentic coding, this project would not exist. Thank you for giving it a chance!

## Thank you

To those who play tested this mod before release and helped get it to where it is now, thank you. Y'all know who you are. To the players who offer feedback, try the mod for the first time, or believe in me and this project, thank you. Your support motivates me to keep making things in a world that can feel crazy and deeply flawed. I hope this project makes it easier for you to enjoy the game and play with others. Thank you all so much. Enjoy Bop It!

— Christopher Shaw

## Licensing

A license for the Bop It Access source has not yet been selected. Prism has its own license; see the [third-party notices](THIRD-PARTY-NOTICES.txt). Bop It! and its assets belong to their respective owners and are not included here.
