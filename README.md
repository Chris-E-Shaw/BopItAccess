# Bop It Access

Bop It Access is an unofficial accessibility mod for the Windows Steam version of **Bop It!**. It uses MelonLoader and Tolk to add speech and braille feedback to menus and game screens. Current features include a first-run welcome screen, an in-game user's guide, spoken title and pause screens, settings and controls, song selection, final scores and leaderboards, achievements, credits, button hints, on-demand tutorial text with current control assignments before a round, and descriptions of the four stages.

## Project status

This project is in early development. This repository contains source code and technical documentation. **There are no compiled builds or GitHub releases here yet.** To use the mod from this repository, build it from source and supply the Tolk runtime files described below.

The commit history includes reconstructed source snapshots of 37 earlier builds. The commits were created when those archives were imported into Git; their dates are not the original build dates. The [technical build history](BopItAccess-build-history.html) describes the work behind each snapshot.

## Requirements

- Windows x64 and your own installation of Bop It! for Steam.
- MelonLoader installed in the game's directory. Development has used MelonLoader **0.7.3 Open-Beta** with the x64 Unity **2022.3.50f1** game build. Other combinations have not been verified.
- A .NET SDK with the **.NET 6 targeting pack**, because the mod targets `net6.0`.
- For installation, compatible 64-bit `Tolk.dll` and `nvdaControllerClient64.dll` runtime files. These third-party binaries are not in this repository.

The mod references DLLs generated or installed by MelonLoader under the game directory. It does not include or redistribute game assemblies.

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
2. Obtain a compatible 64-bit `Tolk.dll` from a trusted source or [build it from the upstream Tolk source](https://github.com/dkager/tolk#compiling). Obtain the matching `nvdaControllerClient64.dll` from [Tolk's x64 library directory](https://github.com/dkager/tolk/tree/master/libs/x64) or your Tolk build. Put **both DLLs in the game directory**, beside the game executable, rather than inside `Mods`.
3. Copy the build's `src\bin\Release\net6.0\documentation\` folder into the game directory. It contains `BopItAccess-user-guide.html` and its companion documents. The in-game guide reads this HTML file each time it opens, so replacing the file updates its content without rebuilding the DLL.
4. Start your screen reader if you use one, then launch Bop It! through Steam. The mod can use SAPI speech when no supported screen reader is running.

The Bop It Access build command compiles only this mod; it does not build or download Tolk. If speech does not start, inspect `<game directory>\Mods\BopItAccess.log`. The log records whether Tolk initialized and accepted speech requests, though that alone cannot prove audio was heard.

On a first run, the welcome screen appears after the game's main menu is ready. Its choices open Mod Settings, read the user's guide in-game, or continue to the game. Mod Settings also offers **Open User's Guide** and a confirmed **Reset Welcome Screen** action that shows the welcome screen on the next launch. In the guide, use Up/Down to choose topics or read lines and Confirm to open a topic. Within tables, Left moves one column left, Right moves one column right, and Up/Down keeps the current column while changing rows. Column headings label cells rather than appearing as data rows; the table is announced on entry and its end on exit. Back leaves a topic or the guide.

## Documentation

- [Game and mod user's guide](BopItAccess-user-guide.html) — a beginner-friendly walkthrough of controls, settings, menus, and play modes.
- [Detailed feature and control guide](README.txt). Its installation section describes the locally prepared install ZIPs; this GitHub repository provides source only.
- [Technical build history](BopItAccess-build-history.html).
- [Git workflow for this project](GIT-WORKFLOW.md).
- [Third-party notices](THIRD-PARTY-NOTICES.txt).

## AI transparency

Christopher Shaw directs this project and evaluates its accessibility in the game. OpenAI Codex models have assisted with research, code, and documentation. Published commit messages include a `Co-authored-by` trailer identifying the model that contributed to each change; the historical credits were checked against this project's session records. The earlier build history was reconstructed from saved source archives rather than recorded as commits at the time. AI-assisted contributions can contain mistakes and should be reviewed before use.

## Licensing

A license for the Bop It Access source has not yet been selected. Tolk and the NVDA Controller Client have their own licenses; see the [third-party notices](THIRD-PARTY-NOTICES.txt). Bop It! and its assets belong to their respective owners and are not included here.
