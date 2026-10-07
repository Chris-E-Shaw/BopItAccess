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

## Windows installer preview 0.2.0

The installer source is in [`installer/`](installer/). It is a self-contained Windows x64 application.

Close Bop It!, open the installer and approve the Windows administrator prompt. The installer welcomes you, looks for the game in Steam libraries on all available drives, and tries to bring its window to the foreground. Check the displayed game folder; use Browse if you need to choose another folder. Tab moves between controls. The status log is a read-only text field: focus it to review messages with cursor keys, select text, or copy it.

Show advanced is unchecked when the installer opens. It reveals Install alpha, Save diagnostics and Copy diagnostics. Install downloads the latest public GitHub release when one exists. There is no public release yet, so testers currently need Show advanced and Install alpha. Alpha asks for confirmation, downloads the latest source and builds it on your computer. Update appears when a newer public release is found for an installed copy.

Status messages explain what is downloading, installing or finishing in plain language. One progress bar shows estimated progress for the whole installation, without resetting for each download or file. It advances in five-percentage-point increments; some preparation stages may take time without a visible change. The welcome message, a newly available update and confirmation that diagnostics were copied are sent through Windows accessibility notifications to your screen reader. Whether these are spoken depends on your screen reader and its Windows notification support.

Installer 0.2.0 never launches Bop It! during installation. Alpha reuses matching local build files or prepares temporary files from your own installed game while it stays closed. The installer then places MelonLoader in the game folder and immediately adds Mods/BopItAccess.dll, followed by Prism, settings, the complete documentation and uninstall support. Wait for the success message, then launch the game yourself through Steam when you are ready.

After a successful installation, Play Bop It! The Video Game appears. Activate it to launch the game yourself through Steam when you are ready. The installer never starts the game automatically during installation.

A compiled release needs the Windows x64 .NET 6 runtime, not a development SDK. Existing complete runtimes are reused. A missing runtime is downloaded from Microsoft and placed in MelonLoader/Dependencies/dotnet. Install alpha also needs a compatible .NET SDK and the .NET 6 targeting pack: an existing SDK is reused or Microsoft’s official SDK is installed system-wide. The installer creates no new SDK folder in the game root. MelonLoader 0.7.3 Open-Beta and Prism 0.18.3 come from their official releases. Shared Microsoft .NET components and SDKs remain installed after abort or uninstall.

Quit closes the installer. If installation is still running, it asks whether to abort and undo the installation before closing; Keep open continues normally. If installation finishes while you are deciding, the dialog updates to say that it is finished, and Quit does not undo the completed installation. Uninstallation, once removal begins, finishes safely before quitting. Abort also asks for confirmation and reverses this attempt’s game-file changes. Canceling during Microsoft .NET setup waits for that shared-component installation to finish safely.

After confirming Uninstall, choose Uninstall for me or Uninstall for everyone. Both remove the shared mod files from this game folder, so the mod will no longer be available to anyone using that installation. The choice controls whose saved Windows mod preferences are removed: your requesting account only, or every local Windows profile, including signed-out profiles. Preferences belonging to the original game are kept. The .NET SDK remains installed.

When the installer removes its own MelonLoader installation and no other mods need it, it also removes the known Loader.cfg and MelonPreferences.cfg files and the empty Plugins, UserLibs and UserData folders. Bop It Access settings, known logs, guides and installer files are removed. Other mods, pre-existing shared loader files and unrecognised files are protected. This also means an unknown file can leave a folder behind; the installer reports that in diagnostics rather than deleting unrelated data.

The installer stays open after uninstall so you can review the result, save diagnostics or install again. Choose Quit when finished. The running uninstall helper and automatic diagnostic files are cleaned up after the window closes. A reinstall in the same window starts a fresh installation record; delayed cleanup cannot remove the new installation.

Windows Installed Apps uses the same confirmation, preference choice and cleanup. The installer supplies BopItAccess-uninstall.ps1 in the game folder as a shortcut to the installed uninstaller; future source builds also include this script in their output. A manually copied script does not install the uninstaller itself. For an older manual installation without an ownership record, the installer removes identifiable mod files and keeps shared files whose origin cannot be established.

If cleanup cannot finish safely, the installer explains that and keeps the information needed to retry. For a managed installation, its Windows uninstall entry and cleanup checkpoint remain until removal succeeds. An older manual copy has no durable ownership record; retry its warnings in the open installer. Do not install, update or remove the mod while Bop It! is running.

### Installer keyboard shortcuts

| Action | Keyboard shortcut | What it does |
| --- | --- | --- |
| Game folder | Alt+G | Focus the game-folder field. |
| Browse | Alt+B | Choose the game folder. |
| Install | Alt+I | Install the latest public release, when available. |
| Install alpha | Alt+A | Confirm and build the latest source; visible with Show advanced. |
| Update | Alt+U | Install a newer public release when offered. |
| Play Bop It! The Video Game | Alt+P | Launch the game through Steam; available after a successful installation. |
| Uninstall | Alt+N | Confirm removal and choose whose Windows mod preferences to remove. |
| Abort | Alt+R | Confirm cancellation of the current installation. |
| Status log | Alt+L | Focus the read-only, selectable status messages. |
| Show advanced | Alt+V | Show or hide alpha installation and diagnostic tools. |
| Save diagnostics | Alt+D | Save and keep recording the full diagnostic session; visible with Show advanced. |
| Copy diagnostics | Alt+C | Copy the full diagnostic snapshot; visible with Show advanced. |
| Quit | Alt+Q | Close, with safe cancellation handling if an operation is running. |

### Using a controller in the installer

The installer supports Xbox-style controllers and other controllers that Windows exposes through XInput. Its controls are separate from the game’s remappable controls. The D-pad or left stick moves between controls; when a text field is focused, directions review its text instead. The bumpers always move to the previous or next focusable control. A activates the focused button or checkbox. Controller input is handled only while this installer or one of its own dialogs is in the foreground.

B goes back or cancels a dialog; on the main installer window it asks to abort an active installation, otherwise it follows Quit. Start follows Quit on the main window and goes back in a dialog. Y toggles Show advanced on the main window. X selects all text in a focused text field. Hold RT while using directions to extend text selection in the installer’s own text fields. Controller navigation in Windows’ native folder and save dialogs still needs human verification. A keyboard remains available for entering a folder or filename. Controllers without XInput support are not covered by this implementation.

### Installer diagnostics

Show advanced reveals Save diagnostics (Alt+D) and Copy diagnostics (Alt+C). Automatic UTF-8 logs are kept locally in %ProgramData%\BopItAccess\diagnostics. Save diagnostics writes the full current session to your chosen .log or .txt file and continues recording until the installer closes; Copy diagnostics copies a snapshot and gives accessible confirmation. Save before an installation or uninstall trial so your recording survives automatic-log cleanup. Technical file, download, compiler and error details are kept here even though the status field uses shorter messages. Nothing is uploaded. Logs may contain Windows usernames and full paths: review them before sharing. Deliberately exported copies remain after uninstall.

On the first manual launch after installing MelonLoader, it may download support files and prepare the game’s assemblies. Allow about a minute, or longer on some systems. The mod cannot speak until MelonLoader loads it. Keep the game open and wait for the Bop It Access startup announcement, then the title-screen, welcome or main-menu announcement before using game controls.

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

The mod does not reset these options on each launch. You may manually change either value back to false for troubleshooting. If uninstall keeps a shared MelonLoader installation, it restores only the installer’s unchanged target flags and preserves other edits. If it removes its own unused MelonLoader installation, it removes the known Loader.cfg and MelonPreferences.cfg files as well.

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
