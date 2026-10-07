# Installer installation flow

Installer preview **0.1.8** prepares and deploys Bop It Access without launching `BopIt!.exe`. The mod stays **0.9.11**. The installer process can run dependency installers and background assembly/compiler tools; the player decides when to launch the game.

## Evidence and scope

The user's supplied MelonLoader and mod logs show the game generating its assemblies and then loading a Bop It Access DLL placed in `Mods` before that first launch. This supports deploying the DLL before runtime assembly generation. It does not establish that every new installer branch works: compilation and static review are recorded separately in the technical build history, and actual installation behavior requires human testing.

The installer formerly started the game to generate source-build references. That opened a separate foreground window, introduced game audio before the mod could lower first-run audio settings, and could hide subsequent installer feedback from a keyboard and screen-reader user. Reference preparation now happens outside the game process.

## Common preparation

1. Validate the selected Steam game folder, existing ownership ledger and whether the game is already running. Installation or update requires the game to be closed.
2. Create a unique temporary working folder outside the game directory. Inspect prerequisites for the selected release or alpha action.
3. Reuse the pinned MelonLoader installation when its required files match, or download and extract the verified official x64 MelonLoader **0.7.3 Open-Beta** archive into temporary staging.
4. Prepare the official Windows x64 Prism **0.18.3** payload and notices, verifying the archive and DLL. A matching installed DLL is reused, while notices are still supplied.
5. Download the mod payload: the latest published GitHub release ZIP for Install/Update, or the exact latest `main` commit's source archive for Install alpha. The alpha warning remains required.

No ownership transaction deploys game files until the payload is prepared successfully. A dependency SDK installed during preparation is the existing explicit exception to rollback: it is a shared development component and remains installed.

## Compiled release path

Install/Update use the release's already compiled `Mods/BopItAccess.dll`. They do not compile source, generate build references or require the .NET SDK/targeting pack. Until a release exists, the regular Install action explains that no package is available; Install alpha is available instead.

The runtime requirement is **Windows x64 .NET 6**, not the SDK. Detection checks a real x64 host and the .NET 6 runtime marker files in existing configured/system locations, an older game-root `dotnet` folder, and `MelonLoader/Dependencies/dotnet`. An installed .NET 10 host by itself does not prove that the .NET 6 runtime is present. A hostfxr version reported in a game log does not prove which application runtime was selected.

If the runtime is missing, the installer downloads Microsoft's official **.NET 6.0.36 Windows x64 runtime ZIP**, verifies its SHA-512 checksum, extracts it in temporary staging and validates the required files. It does not install a new system-wide runtime or put a development SDK in the game root. Deployment later copies this runtime into **`MelonLoader/Dependencies/dotnet`**, a location MelonLoader can search. Each introduced/replaced runtime file participates in the same ownership journal, rollback and uninstall rules as other loader files.

## Alpha source path

Alpha requires a compatible x64 SDK, the **.NET 6 runtime**, and the **.NET 6 targeting pack**. Detection checks complete SDK/runtime/reference markers. Existing suitable system SDKs and complete legacy SDKs at `<game>/dotnet` can be reused. The installer does not create that legacy game-root SDK folder anymore.

If no suitable SDK exists, it downloads Microsoft's official **.NET SDK 6.0.428 Windows x64 installer**, verifies its SHA-512 checksum, and runs it quietly with no automatic restart. It rechecks the installed components afterward. A requested Windows restart is reported. SDK components stay installed after abort or uninstall, as previously requested, because they are shared development dependencies.

An abort requested during Microsoft's dependency installation is deferred until that installer finishes safely; it does not terminate an in-progress shared component installation. Abort is then honored before deploying the mod. This can make cancellation take longer than cancellation during a download.

### Embedded-template preflight

Installer 0.1.8 validates its own offline-helper source/project resources at the start of alpha preparation, before prerequisite downloads, SDK installation or reference generation. Both templates must exist and be nonempty. A missing template is an installer packaging error, reported with its resource name and a request to download the latest installer preview, rather than a failure of the player's game.

The installer project declares `WithCulture=false` explicitly on `BuildReferenceGenerator/Program.cs.txt`, `BuildReferenceGenerator/BuildReferenceGenerator.csproj.txt` and `uninstall.ps1`. MSBuild otherwise interprets the `.cs` segment as the Czech culture code and can place the helper source in a satellite assembly even when `LogicalName` is explicit. These files are neutral embedded data and must be available regardless of the Windows display language.

A build-time target runs after `SplitResourcesByCulture` and requires the three exact `LogicalName` values with neutral culture. It fails compilation if a resource is missing, renamed or routed to a culture-specific assembly. This checks packaging while building the installer; the separate runtime preflight checks templates before the alpha operation begins.

The supplied 0.1.7 session completed Cpp2IL with exit code 0, then failed reading `Program.cs.txt` before a deployment transaction existed. That evidence identifies a packaging defect in the background alpha preparation; it does not justify restoring a game pre-run. The single-pass deployment order remains unchanged.

### References without a game launch

`OfflineBuildReferences` accepts the player's own game files and prepares references locally. The current offline alpha route supports the **Unity 2022.3.50f1** game build. A different or unidentifiable Unity version is reported before game-file deployment; there is no fallback that opens the game.

- Reuse complete `MelonLoader/Il2CppAssemblies` proxy DLLs only when the loader's cached `GameAssemblyHash` matches the current `GameAssembly.dll`. Existence alone is insufficient. If the pinned loader was staged separately, copy those matching references into the temporary reference root with the staged loader libraries.
- If matching references are unavailable, download the checksum-pinned **Cpp2IL 2022.1.0-pre-release.21** Windows tool and `Cpp2IL.Plugin.StrippedCodeRegSupport.dll`, plus the matching official MelonLoader Unity dependency archive. These are build tools and support libraries, not a copy of Bop It!.
- Run Cpp2IL on the player's installed game to produce temporary dummy assemblies. Compile the embedded reference helper against MelonLoader's supplied Il2CppInterop/AsmResolver libraries, then run Il2CppInterop generation into a temporary `build-references/MelonLoader/Il2CppAssemblies` tree.
- Verify that every required proxy is a managed assembly. Compile `src/BopItAccess.csproj` against that root with the player's selected SDK.

The tools run without visible helper windows and with redirected stdout/stderr, cancellation and bounded deadlines. Their output is recorded in installer diagnostics. Cancellation terminates and waits for owned build processes before cleanup. It does not terminate a manually started game.

These generated game references are temporary, local build inputs. They are not added to source control, mod release packages or the deployed runtime assembly cache. They do not replace MelonLoader's own first-launch runtime generation. The user owns the game installation that provides the input files.

## Deployment order

After successful preparation, recheck that the game has not been opened independently, then create the durable ownership transaction:

1. Deploy the staged MelonLoader tree if installation or replacement is needed.
2. Create `Mods` if needed and **immediately install `Mods/BopItAccess.dll`**.
3. Deploy the runtime-only tree into `MelonLoader/Dependencies/dotnet` when the release preparation returned a missing runtime.
4. Complete Prism and its notices, targeted loader UI defaults, the full documentation tree and uninstall support. Preserve existing shared configuration entries.
5. Commit the ownership ledger and Windows Installed Apps entry, then show the completion dialog. Explain that the user should launch the game manually and that first-start assembly generation can take time.

The player should keep the game closed throughout installation. If it is opened after preparation, deployment stops. If a later rollback needs the game closed, the installer asks the user to close it and retains recovery information when that cannot finish safely. It does not start or force-close the game.

## First manual launch

Start a screen reader if desired, then launch Bop It! through Steam. MelonLoader may download support tools/libraries and generate its runtime assemblies before loading the already installed mod. Allow about a minute, or longer on some systems. The mod cannot announce its startup message until it has been loaded; silence during generation is not itself proof of a failure.

Wait for the Bop It Access startup announcement, then for the title-screen/main-menu or welcome announcement before using the game controls. The existing first-use audio defaults take effect once the mod and saved game settings are ready. Installer completion does not mean that the game has already reached that state.

If startup fails, collect MelonLoader's `Latest.log` and the mod's `Mods/BopItAccess.log`, alongside the saved installer recording. See [Diagnostics](DIAGNOSTICS.md).

## Ownership, rollback and removal

Staged source, offline tools and generated references are temporary. Game-file changes use the existing transaction journal and backups. An aborted or failed attempt reverses its own deployed files without discarding pre-existing game data or unrelated mods. Actual game-generated runtime assemblies are created only after the player launches the game, outside the installer's build staging.

An introduced loader and its owned runtime files are removed/restored according to the existing manifest rules. If other mods need the loader, preserve shared loader/runtime files. System SDKs, pre-existing runtime installations and legacy `<game>/dotnet` SDKs are not removed. An incomplete legacy SDK directory is left untouched; it no longer blocks a suitable system SDK or a runtime-only preparation.

Automatic diagnostic logs are cleared on successful uninstall; files intentionally exported elsewhere remain. The installer contains neither compiled mod DLLs nor game-generated proxy DLLs. The executable embeds only the small source/project templates needed to compile its local reference helper, along with its own self-contained runtime.

## Validation of preview 0.1.8

Publishing completed with zero warnings or errors. Static PE inspection of the newly built main installer assembly confirmed all three named resources and compared their payloads byte-for-byte with the source files: `uninstall.ps1` (6,021 bytes), `Program.cs.txt` (3,202 bytes), and `BuildReferenceGenerator.csproj.txt` (649 bytes). Static inspection of the final single-file executable’s .NET bundle confirmed its main DLL exactly matches that inspected assembly and that it contains no installer-specific language satellite assembly. The bundle format is 6.0; this is a packaging-format version, not a claim that the installer uses the .NET 6 runtime.

An independent static review found no blocking issues. Validation was limited to compilation and static source/artifact inspection. No automated tests, installer/uninstaller execution, helper execution, offline reference generation or game launch were performed. Actual alpha installation and first manual launch still need a human test. This installer hotfix does not create a new mod build or imply that the mod is installed in any current game directory; the source mod version remains 0.9.11.
