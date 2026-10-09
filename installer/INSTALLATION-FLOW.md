# Installer installation flow

Installer **0.2.7** prepares and deploys Bop It Access without launching `BopIt!.exe`. The mod is **0.9.12** (**59 mod builds**). The installer process can run dependency installers and background assembly/compiler tools; the player decides when to launch the game. After a successful installation, **Play Bop It! The Video Game** (Alt+P) offers a separate, explicitly requested Steam launch; it is never invoked automatically as an installation step.

## Evidence and scope

The user's supplied MelonLoader and mod logs show the game generating its assemblies and then loading a Bop It Access DLL placed in `Mods` before that first launch. This supports deploying the DLL before runtime assembly generation. It does not establish that every new installer branch works: compilation and static review are recorded separately in the technical build history, and actual installation behavior requires human testing.

The installer formerly started the game to generate source-build references. That opened a separate foreground window, introduced game audio before the mod could lower first-run audio settings, and could hide subsequent installer feedback from a keyboard and screen-reader user. Reference preparation now happens outside the game process.

## Common preparation

1. Validate the selected Steam game folder, existing ownership ledger and whether the game is already running. Installation or update requires the game to be closed.
2. Create a unique temporary working folder outside the game directory. Inspect prerequisites for the selected release or alpha action. For alpha, validate and pin the latest main-branch SHA before dependency downloads or shared Microsoft setup.
3. Reuse the pinned MelonLoader installation when its required files match, or download and extract the verified official x64 MelonLoader **0.7.3 Open-Beta** archive into temporary staging.
4. Prepare the official Windows x64 Prism **0.18.3** payload and notices, verifying the archive and DLL. A matching installed DLL is reused, while notices are still supplied.
5. Download the mod payload: the latest published GitHub release ZIP for Install/Update, or the exact latest `main` commit's source archive for Install alpha. The alpha warning remains required.

No ownership transaction deploys game files until the payload is prepared successfully. A dependency SDK installed during preparation is the existing explicit exception to rollback: it is a shared development component and remains installed.

## Compiled release path

Install/Update use the release's already compiled `Mods/BopItAccess.dll`. They do not compile source, generate build references or require the .NET SDK/targeting pack. Until a release exists, the regular Install action explains that no package is available; Show advanced reveals Install alpha for testers instead.

The runtime requirement is **Windows x64 .NET 6**, not the SDK. Detection checks a real x64 host and the .NET 6 runtime marker files in existing configured/system locations, an older game-root `dotnet` folder, and `MelonLoader/Dependencies/dotnet`. An installed .NET 10 host by itself does not prove that the .NET 6 runtime is present. A hostfxr version reported in a game log does not prove which application runtime was selected.

If the runtime is missing, the installer downloads Microsoft's official **.NET 6.0.36 Windows x64 runtime ZIP**, verifies its SHA-512 checksum, extracts it in temporary staging and validates the required files. It does not install a new system-wide runtime or put a development SDK in the game root. Deployment later copies this runtime into **`MelonLoader/Dependencies/dotnet`**, a location MelonLoader can search. Each introduced/replaced runtime file participates in the same ownership journal, rollback and uninstall rules as other loader files.

## Alpha source path

Alpha requires a compatible x64 SDK, the **.NET 6 runtime**, and the **.NET 6 targeting pack**. Detection checks complete SDK/runtime/reference markers. Existing suitable system SDKs and complete legacy SDKs at `<game>/dotnet` can be reused. The installer does not create that legacy game-root SDK folder anymore.

If no suitable SDK exists, it downloads Microsoft's official **.NET SDK 6.0.428 Windows x64 installer**, verifies its SHA-512 checksum, and runs it quietly with no automatic restart. It rechecks the installed components afterward. A requested Windows restart is reported. SDK components stay installed after abort or uninstall, as previously requested, because they are shared development dependencies.

An abort requested during Microsoft's dependency installation is deferred until that installer finishes safely; it does not terminate an in-progress shared component installation. Abort is then honored before deploying the mod. This can make cancellation take longer than cancellation during a download.

### Embedded-template preflight

Installer 0.1.8 validates its own offline-helper source/project resources at the start of alpha preparation, before prerequisite downloads, SDK installation or reference generation. Both templates must exist and be nonempty. A missing template is an installer packaging error, reported with its resource name and a request to download the latest installer, rather than a failure of the player's game.

The installer project declares `WithCulture=false` explicitly on `BuildReferenceGenerator/Program.cs.txt`, `BuildReferenceGenerator/BuildReferenceGenerator.csproj.txt` and `uninstall.ps1`. MSBuild otherwise interprets the `.cs` segment as the Czech culture code and can place the helper source in a satellite assembly even when `LogicalName` is explicit. These files are neutral embedded data and must be available regardless of the Windows display language.

A build-time target runs after `SplitResourcesByCulture` and requires the three exact `LogicalName` values with neutral culture. It fails compilation if a resource is missing, renamed or routed to a culture-specific assembly. This checks packaging while building the installer; the separate runtime preflight checks templates before the alpha operation begins.

The supplied 0.1.7 session completed Cpp2IL with exit code 0, then failed reading `Program.cs.txt` before a deployment transaction existed. That evidence identifies a packaging defect in the background alpha preparation; it does not justify restoring a game pre-run. The single-pass deployment order remains unchanged.

### Helper dependency preparation

Installer 0.1.10 compiles the embedded .NET 6 reference helper before downloading or executing Cpp2IL, the stripped-code plugin or the Unity dependency archive. Its project has explicit private references for all 12 managed dependencies from the staged, verified MelonLoader `net6` directory. This replaces the wildcard content copy, which copied Iced to disk without recording it in `.deps.json`.

The dependencies are AsmResolver, AsmResolver.DotNet, AsmResolver.PE, AsmResolver.PE.File, Il2CppInterop.Generator, Il2CppInterop.Common, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource, Iced, MonoMod.Backports, MonoMod.ILHelpers and Microsoft.Extensions.DependencyInjection.Abstractions. Required DLLs are read from the pinned loader; this does not introduce additional dependency downloads.

Before assembly-tool preparation, check the helper's selected runtime target in `.deps.json`, require all 12 explicit dependencies' runtime entries and regular DLLs, and compare their bytes with the staged loader using SHA-256. Select the highest installed stable .NET 6.0 patch beneath the selected `dotnet.exe`. For references outside the 12 required private dependencies, require registration in that framework's `Microsoft.NETCore.App.deps.json` and compare the DLL's assembly name, version, culture and public-key token to confirm compatibility. Execute the helper using `--fx-version <patch>` and `--roll-forward Disable` so inherited settings cannot choose a different framework. SDK-build arguments are unchanged. Accept compatible framework references even when the loader also contains a copy; do not exempt all `System.*` names. `System.Runtime.CompilerServices.Unsafe` 6 is framework-provided, while `System.Diagnostics.DiagnosticSource` 10 remains explicit because the framework's version 6 is insufficient. Failure reports a missing or incompatible DLL before deployment changes game files. Only after that preflight succeeds does Cpp2IL read the game and Il2CppInterop generate proxies.

The supplied 0.1.8 attempt passed template loading, Cpp2IL and helper compilation, then failed to resolve Iced in `Pass16ScanMethodRefs`. It stopped before deployment. This is a different preparation defect from the 0.1.7 embedded-template failure described above. Neither requires starting the game during installation.

### References without a game launch

`OfflineBuildReferences` accepts the player's own game files and prepares references locally. The current offline alpha route supports the **Unity 2022.3.50f1** game build. A different or unidentifiable Unity version is reported before game-file deployment; there is no fallback that opens the game.

- Reuse complete `MelonLoader/Il2CppAssemblies` proxy DLLs only when the loader's cached `GameAssemblyHash` matches the current `GameAssembly.dll`. Existence alone is insufficient. If the pinned loader was staged separately, copy those matching references into the temporary reference root with the staged loader libraries.
- If matching references are unavailable, compile and validate the embedded helper against the staged MelonLoader dependency set before downloading or running the assembly tools.
- If matching references are unavailable, download the checksum-pinned **Cpp2IL 2022.1.0-pre-release.21** Windows tool and `Cpp2IL.Plugin.StrippedCodeRegSupport.dll`, plus the matching official MelonLoader Unity dependency archive. These are build tools and support libraries, not a copy of Bop It!.
- Run Cpp2IL on the player's installed game to produce temporary dummy assemblies, then use the validated helper to run Il2CppInterop generation into a temporary `build-references/MelonLoader/Il2CppAssemblies` tree.
- Verify that every required proxy is a managed assembly. Compile `src/BopItAccess.csproj` against that root with the player's selected SDK.

The tools run without visible helper windows and with redirected stdout/stderr, cancellation and bounded deadlines. Their output is recorded in installer diagnostics. Cancellation terminates and waits for owned build processes before cleanup. It does not terminate a manually started game.

These generated game references are temporary, local build inputs. They are not added to source control, mod release packages or the deployed runtime assembly cache. They do not replace MelonLoader's own first-launch runtime generation. The user owns the game installation that provides the input files.

## Deployment order

After successful preparation, recheck that the game has not been opened independently, then create the durable ownership transaction:

1. Deploy the staged MelonLoader tree if installation or replacement is needed.
2. Create `Mods` if needed and **immediately install `Mods/BopItAccess.dll`**.
3. Deploy the runtime-only tree into `MelonLoader/Dependencies/dotnet` when the release preparation returned a missing runtime.
4. Complete Prism and its notices, targeted loader UI defaults, the full documentation tree and uninstall support. Install the same embedded script both as ProgramData/BopItAccess/uninstall.ps1 and as the recorded game-root BopItAccess-uninstall.ps1 shortcut. Preserve existing shared configuration entries.
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

## Validation of preview 0.1.9

Installer 0.1.9 and its embedded reference helper compiled with zero warnings and zero errors. The helper was compiled against a freshly downloaded official MelonLoader 0.7.3 archive whose pinned SHA-256 was verified. Static inspection found all 12 dependencies registered in the helper’s selected `.deps.json` runtime target. Every output dependency DLL was byte-for-byte identical to the verified archive; the output contained exactly 13 DLLs, including the helper. Static assembly-reference metadata inspection confirmed that the supplied versions were at least the versions requested by those references. Iced 1.21.0.0 is now registered in the manifest; the generator references Iced 1.17.0.0. This metadata inspection does not execute .NET resolution or any generator pass.

Static inspection of the final single-file executable confirmed its main DLL exactly matches the inspected installer assembly and that no installer-specific language satellite is included. Its three neutral resources match the current source files byte-for-byte: `uninstall.ps1` (6,021 bytes), `Program.cs.txt` (3,202 bytes), and `BuildReferenceGenerator.csproj.txt` (529 bytes after removing the wildcard). The production preflight checks runtime-manifest entries, copied payload hashes, managed metadata and coverage of loader-provided dependency names. It does not compare requested version/token identities; the additional version inspection above was a separate static build review.

Validation is limited to compilation and static source/artifact inspection. No automated tests, helper execution, offline reference generation, installer/uninstaller execution or game launch were performed. Actual alpha installation and the first manual game launch still require human testing. The main mod remains 0.9.11 with 58 mod builds; no GitHub Release is published.


## Shared-framework preflight correction

Installer 0.1.9's helper compiled successfully in the supplied recording, but its guard wrongly rejected the duplicate `System.Runtime.CompilerServices.Unsafe.dll` in the full official MelonLoader archive. The .NET 6 runtime already provides a compatible assembly. Installer 0.1.10 validates framework assembly identities using the selected host's runtime and retains manifest and payload checks for explicit private dependencies, rather than assuming that every referenced file in the loader directory must appear in the helper manifest. The 12 required private dependencies, copied-payload verification, early preflight, cancellation, ownership and manual-launch flow remain in place.

## Validation of preview 0.1.10

The self-contained Windows x64 installer 0.1.10 compiled with zero warnings and zero errors. Static inspection used the full official MelonLoader 0.7.3 archive after verifying its pinned checksum, with 47 top-level DLLs in the `net6` directory, rather than the earlier reduced 12-library staging set. For the 12 helper dependencies, all 73 assembly-reference edges were satisfied by either explicit helper libraries (19) or compatible libraries in the selected .NET 6 shared framework (54). None were missing or incompatible. The inspection compared name, culture, public-key token and available version against every requested reference, including explicit private references; production private-reference checks remain manifest, managed-metadata and staged-payload verification. Duplicate `Unsafe` 6 is compatible with the framework, while `DiagnosticSource` 10 remains explicitly supplied. The helper output retains exactly 13 DLLs: its program plus the 12 required private dependencies.

Static inspection of the final single-file installer confirms that its main DLL matches the inspected compiled assembly and its three neutral embedded resources match their source files; no installer-specific language satellite is included. Source review confirms helper execution is pinned to the inspected framework patch and SDK build arguments are unchanged. These are compilation, source review and static package/metadata inspections. No automated tests, helper execution, offline reference generation, installer/uninstaller run or game launch were performed. They do not establish that a clean alpha installation completes on the target system.

The mod remains 0.9.11 with 58 mod builds. No GitHub Release is published.


## Accessible delivery interface in preview 0.2.0

Show advanced starts unchecked and hides alpha installation and both diagnostic buttons. Normal actions retain keyboard access keys and accessible names. **Play Bop It! The Video Game** appears after a successful installation and launches through Steam only when the user activates it. On first showing the window, attempt foreground activation and focus the game-folder field; Windows may deny the request. Post a welcome via native UI Automation notifications. The same notification utility confirms a copied diagnostic snapshot and a newly available update. Actual reader output depends on Windows and the screen reader, so posting success is not audible-output verification.

The status text field remains non-editable and supports caret review, selection and copying. User-facing statuses now explain checks, downloads, installation, completion and rollback using short sentences. All technical per-file/compiler/error context remains in diagnostics. Weighted whole-operation progress replaces per-step resets: publish in five-percentage-point increments and reach 100 only after successful commit/removal. Unknown-duration stages hold their estimate rather than providing a time guarantee.

Quit and the title-bar close action share a live confirmation during an operation. Quitting an active install cancels owned work, waits for safe Microsoft shared-component completion if necessary, and reverses game-file changes before closing. While the confirmation is open, update it when the backend finishes and never abort an already committed install. Suppress a competing success popup until the user finishes deciding. Irreversible uninstall removal finishes safely before shutdown.

### Preference scope and self-cleanup

After confirming uninstall, offer Uninstall for me, Uninstall for everyone or Cancel. Shared game files are removed for everyone in either confirmed scope. The scope changes only which Windows profiles lose mod-specific registry values; native game preferences and shared .NET SDK/runtime components remain. Carry the requesting user SID across UAC and persist it and the scope once removal starts, so retry does not silently target another account.

When the installer-owned loader is removed and no third-party mod needs it, validate and remove the known Loader.cfg/MelonPreferences.cfg and prune empty Plugins, UserLibs and UserData. Preserve unknown contents and pre-existing/shared loader files; rejected links or inaccessible cleanup must be reported rather than followed. An older manual install without a trustworthy loader ownership record retains dependencies whose origin cannot be established.

Both the normal installer and the Installed Apps uninstall window remain open after success. Final self-cleanup waits for Quit. A completed manifest no longer marks the game as installed. Before reinstalling in the same window, discard the old completed file-ownership baseline. Preserve a validated completed-uninstall tombstone for final cleanup if the new installation later aborts, but an active transaction or a newly committed manifest prevents delayed deletion of the new installation.

The root shortcut BopItAccess-uninstall.ps1 delegates to ProgramData's installed script; it never recursively deletes the game directory. The source mod project also links that script into future output so the same design is carried forward. Copying only this shortcut into a manual build does not install the ProgramData launcher. No mod DLL is rebuilt here.

### Controller navigation

Use XInput for Xbox-style and compatible controllers, with polling gated to the foreground installer and owned dialogs. The D-pad and left stick navigate controls or review focused text. Bumpers navigate focus independently of text review, including from text fields; A activates, B goes back/cancels or requests main-window abort and Start quits/back. Y selects all in a focused installer text field; outside text fields on the main window, it toggles Show advanced. In managed text fields, LT+directions moves by word with Left/Right or paragraph with Up/Down; RT+directions extends selection; LT+RT selects words or paragraphs. X copies the current selection, with accessible success/empty-selection/failure feedback. Held inputs at dialog transitions or reconnect are absorbed, stick direction uses hysteresis, and repeat has a bounded delay. Native Windows folder/save dialogs are supported by local-window messaging but need human verification; input is never sent to unrelated applications. Non-XInput devices are outside current coverage.

### Human evidence and validation

Installer 0.2.0 compiled with zero warnings and zero errors. PowerShell AST parsing found no errors. Static inspection confirms all three neutral embedded resources match their source bytes, the published single-file bundle’s main assembly exactly matches the inspected DLL, and no installer-specific language satellite assemblies are included. These checks do not execute the UI, controller handling, uninstall or game.

The supplied installer 0.1.10 recording shows successful alpha deployment and uninstall, followed by normal exit code 0. The tester separately identified leftover loader configuration/folders. Static source inspection identifies the old unconditional close and stale completed-manifest state. Installer 0.2.0 addresses those findings and the requested interface changes. Compilation and static review are the only validation performed for this update: no automated tests, installer/uninstaller execution, helper generation or game launch. Actual screen-reader notifications, focus, Quit completion races, both preference scopes and controller/native-dialog interaction still need human checks. The mod remains 0.9.11 with 58 mod builds; no GitHub Release is published.

## Startup focus and controller text review in preview 0.2.1

The tester confirms that preview **0.2.0** works apart from initial foreground and keyboard focus. Preview **0.2.1** adds a bounded initial activation/focus attempt and the requested controller text controls. That prior human report does not validate the new behavior.

Preview 0.2.1 made at most eight activation attempts on a 125 ms timer within an absolute two-second lifetime. It confirmed actual foreground status before setting keyboard focus to the game-folder field or enabled status fallback, and stopped after stable focus for 250 ms or fresh input, deliberate control changes, operations or owned dialogs. It used a temporary topmost pulse. The later human report confirms that startup activation still failed; preview 0.2.2 replaces that handling as described below.

In a focused installer text field, D-pad/left-stick directions act as arrow keys. Left/Right moves by character and Up/Down by visual line. LT acts as Ctrl, moving by word with Left/Right or by hard-newline paragraph with Up/Down. RT acts as Shift to extend selection; LT+RT acts as Ctrl+Shift to select words or paragraphs while retaining the selection anchor through direction changes. Bumpers always move focus to the previous/next control, including from the read-only status field.

X copies only the selection in a focused installer text field and preserves that selection. It does not select all; select the desired text first. Keyboard Ctrl+C keeps its native behavior. Copy diagnostics (Alt+C) remains the action for the entire technical snapshot. Native UI Automation notifications confirm a successful copy, an empty selection or failure. With no selection, caret review also posts the current character, word, visual line or paragraph; RT selection changes use native selection events. These handlers do not record reviewed/copied contents in diagnostics, and a successfully posted notification is not proof of speech.

Controller input is still restricted to the foreground installer or its own dialogs. Native folder/save dialogs retain scoped local arrow navigation; X uses `WM_COPY` and verifies the clipboard sequence/owner. Trigger modifier navigation is limited to the installer's own managed text fields. Native-dialog interaction and non-XInput coverage retain the human-review limits described above.

Validation for this update is compilation and static inspection only. No automated tests, installer/uninstaller, controller or screen-reader execution, helper/reference generation or game launch is performed. Startup focus, word/paragraph review, selection and actual accessible feedback await human review. The main mod remains **0.9.11**, with **58 mod builds**; no new mod DLL, tag or GitHub Release is created.

## Select All, explicit checkbox state and foreground attention in preview 0.2.2

During preparation of preview **0.2.2**, the user had confirmed **0.2.1** controller text navigation, selection and copying, while startup foreground/keyboard focus still failed from File Explorer. At publication, **0.2.2** activation and attention behavior awaited human review. The later confirmation is recorded below under 0.2.3.

At the earliest managed startup, request `AllowSetForegroundWindow` for the installer's own PID and record grant/denial. This preserves existing legitimate permission when possible, without creating missing permission or changing Windows policy. Record the earliest managed foreground executable name/handle, administrator-token state and native process age, using executable names only and omitting other applications' titles, paths and command lines. Process context does not establish the exact reason for a foreground denial or directly observe preceding UAC interaction.

Observe startup on a 125 ms timer for at most two seconds. Request activation once per distinct foreground window, at most three requests; confirm foreground before setting keyboard focus and require 250 ms of stable foreground/control focus before completing. Stop for fresh user input, deliberate enabled-control focus changes, operations and owned popups. Remove the temporary topmost pulse. If the deadline or transition limit leaves another window active, flash the caption/taskbar three times and append visible Alt+Tab guidance, using an important, recent native UI Automation notification to supersede lengthy welcome speech. In 0.2.2, the full welcome was kept in the log; current 0.2.7 keeps it in the separate **Welcome and controls** field (Alt+W), while Status log (Alt+L) holds operation messages. A denied second launch uses the same attention guidance and keeps an existing popup's focused control.

Y, the top face button, selects all in a focused installer text field, including local native edit controls. Confirm “All text selected.” or “No text to select.” Managed selection retains its start anchor and active end so subsequent RT directions can adjust it. Outside text fields on the main window, Y toggles Show advanced; it does not change advanced controls in other dialogs. X continues copying only the current selection. Foreground scoping remains required.

In 0.2.2, the welcome message included all controller text-review shortcuts: character/line arrows, LT words/paragraphs, RT selection, both triggers, Y Select All and X Copy. Show advanced uses an explicit checkbox role. On every state change, from any input method, append visible checked/unchecked status and post important native accessibility confirmation. Notifications depend on Windows/screen-reader support; static posting paths do not prove spoken output.

Installer **0.2.2** publishes with zero warnings and zero errors; the whitespace diff check is clean. Static PE resource inspection confirms three exact source matches, the executable bundle's main DLL matches the inspected DLL, and no installer-specific language satellite resources are included. Validation was compilation and static inspection only: no automated tests, installer/uninstaller, controller or screen-reader execution, helper/reference generation or game launch. At publication, startup focus, Select All and state/attention feedback awaited human review. The main mod remained **0.9.11**, with **58 mod builds**; no new mod DLL, tag or GitHub Release was created.

## Replacing controller review with focus announcements in preview 0.2.3

The user confirms preview **0.2.2** startup focus and all new controls. The remaining bug is continued D-pad status-text speech after LB/RB moves focus. That human confirmation concerns 0.2.2. The **0.2.3** interruption change awaits human verification.

In 0.2.3, routine controller review used normal `MostRecent` when `important=false` and `replacePending=true`. A verified successful bumper move from a control that posted controller speech queued a deferred `ImportantMostRecent` destination announcement. It identifies the accessible name, role and relevant state, including checkbox/radio state and single-line values. A multiline field supplies its name/type and selection count if present, without repeating its contents or total character count.

Before posting, validate generation, foreground, actual managed/native focus, destination handle identity, enabled/visible state and ownership. Rapid bumper movement retains the latest valid destination. Keep a successfully announced destination as the controller speech source so further bumper moves replace its announcement too. Other focus/window changes invalidate stale work. Do not add an announcement when navigation leaves focus at the same boundary control. Important confirmations and attention keep their processing priority. Native dialog navigation remains scoped to local messages.

The replacement is intended to interrupt old review or focus-announcement speech. Diagnostics record whether posting succeeded and the control type, without destination labels/values or reviewed text. Actual speech and timing depend on Windows and the installed reader and require human review.

Installer **0.2.3** publishes with zero warnings and zero errors; the whitespace diff check is clean. Static PE inspection confirms all three required resource payloads exactly match source, the single-file bundle includes the matching main DLL, and no installer-specific language satellite resources are present. Validation is compilation and static inspection only: no automated tests, installer/uninstaller, controller or screen-reader execution, helper/reference generation or game launch. The main mod remains **0.9.11**, with **58 mod builds**; no new mod DLL, tag or GitHub Release is created.

## Recent announcement replacement in installer 0.2.4

Historical delivery record: installer **0.2.4** compiled to `build/installer/BopItAccess.Installer.exe`. The copies `BopItAccess-Installer-0.2.4.exe` and the identical `BopItAccess-Installer.exe` were delivered separately in `C:\Users\Chris\Documents\Codex`, without a preview label. No public binary, GitHub Release or tag was published. The current supplied build is **0.2.7**, with the separate Welcome field described below.

Every speech announcement emitted by the installer requests replacement of earlier installer speech. The central policy uses `ImportantMostRecent` for important feedback and `MostRecent` for ordinary feedback, including controller text review; there is no per-call `replacePending` opt-in. Select All, selected-text copy, Show advanced state, diagnostics confirmations, welcome/update/dialog messages and attention all use that policy. Status-log announcement frequency is unchanged, so appending a status line does not necessarily speak it.

Every successful managed controller focus move queues the guarded concise destination announcement, even without prior D-pad review. The callback validates the actual destination and skips when a newer announcement has posted; worker requests reject stale foreground or teardown and coalesce rapid updates. Announcements normally require their own form to be active. Copy/select feedback in owned native dialogs uses exact owner-window scope, and deliberate startup/duplicate-launch attention retains its exception. Destination descriptions keep accessible name, role and relevant state; multiline contents and total character count are omitted.

These requests aim to interrupt earlier speech; native automatic focus, checkbox and selection events remain reader/Windows controlled. At that publication, human verification of interruption and event timing was still needed. Compilation and static inspection succeeded with zero warnings and errors: all three required embedded resources match source bytes, the bundle contains the matching main DLL, and no installer-specific language satellite resources are present. No automated tests or installer, uninstaller, controller, screen-reader or game execution was performed. At that publication, the main mod remained **0.9.11**, with **58 mod builds**; no new mod DLL was built.

## Supplied installer and manual package contract in 0.2.5

Installer **0.2.5** gives initial keyboard/controller focus to the permanent read-only, selectable **Welcome and controls** field above Game folder. Alt+W returns to it; its accessible name is **Installer welcome and controls**. The changing Status log remains Alt+L and no longer receives the welcome text. After successful startup focus confirmation, one guarded full welcome announcement uses the existing recent-replacement policy. The welcome field supports the same review, selection and copy controls as other installer text fields.

The compiled release asset must match the public release tag: `v1.0` selects `BopItAccess-v1.0.zip`, later `v1.1` selects `BopItAccess-v1.1.zip`, with optional leading v/V normalized. The standalone `BopItAccess-Installer.exe` and GitHub Source code (zip)/(tar.gz) remain separate choices for users; none is a compiled mod package. Public assets and a release/tag remain upcoming.

Manual ZIP users install official x64 MelonLoader 0.7.3 Open-Beta and the Windows x64 .NET 6 runtime separately; no SDK is needed for the compiled route. With the game closed, place the ZIP’s mod in Mods and Prism beside the game EXE, copy complete documentation/licences, and merge only the two Loader.cfg flags when that configuration exists. Put the mod in place before the first game launch generates assemblies. The ZIP omits loader/runtime/game/generated/source payloads. Source or Install alpha still needs the SDK and targeting pack.

The manual route creates no installer ownership record, helper or Windows uninstall entry. Its script only points to an installer-managed helper if one exists. A manual update preserves settings and unrelated files. Deleting only `Mods/BopItAccess.dll` disables the mod; fuller manual cleanup removes only identifiable mod files/settings/logs and may leave Windows preferences or shared loader/Prism files. The installer can later offer legacy-copy removal and the two preference scopes.

`scripts/package-mod.ps1` prepares the known compiled mod/configuration/documentation/Prism payload from an already built matching DLL, checks source/DLL versions, and keeps staging/archive local; it compiles nothing. The main mod is **0.9.12**, with **59 mod builds**. The new history entries record compile/static evidence separately from pending human verification. No automated tests or installer/game/UI execution is performed, and no GitHub Release or tag is published.

Static inspection of the 0.2.5 executable confirmed source revision fabc32d48754bf53f266656d67b356bc9d0841d3, three exact embedded-resource source matches, the matching bundled main DLL and no installer-specific language satellites.

The local compiled **0.9.12** ZIP has now been created successfully with `scripts/package-mod.ps1`. It contains the **70 documentation files**, the mod and Prism DLLs, the declared configuration/shortcut files and Prism notices/licences. This local archive is separate from the still-upcoming public v1.0 release. Package creation and static inspection do not verify installation or speech; no automated tests or app/game execution was performed.

## Source-reference preparation and metadata bounds in 0.2.6

The **0.2.5** tester log downloaded the Microsoft SDK, MelonLoader and Prism successfully, then failed at the latest-source lookup: `/commits/main` returned commit metadata and patches exceeding the one-megabyte client buffer. That response-limit failure does not demonstrate that Internet access was unavailable.

For Install alpha, **0.2.6** first verifies the embedded templates and resolves `/git/ref/heads/main` to the expected main ref, a `commit` object and a 40-character ASCII hexadecimal SHA. The 1–3% **Checking the latest alpha version** phase occurs before dependency downloads and shared Microsoft installation. Reuse that exact SHA for the later source archive instead of resolving it after those side effects. Existing preparation, deployment and rollback rules remain.

Metadata requests use `ResponseHeadersRead`, an **8 MiB** declared/streamed-byte limit and one **30-second deadline** across response headers, body and JSON parsing. HTTPS, JSON media type, body completeness, parse validity and object shape are checked; user cancellation is preserved separately from timeout. Release metadata uses the same bounded path, with the normal no-public-release case retained. Binary package downloads keep their separate policy.

Failure feedback identifies HTTP status classes, transport/TLS/proxy failures, response limits, invalid metadata and timeouts without treating every request exception as an offline connection. Startup update checks use that classification, with full technical exceptions in diagnostics. This changes metadata preparation and failure accuracy, not the player’s controls or installation choices.

Human verification of the new lookup and installation remains pending. No automated tests or installer, uninstaller, game, controller or screen-reader execution is performed. Only the installer changes: the main mod remains **0.9.12**, with **59 mod builds**, and no new mod DLL, public release or tag is created.

Installer **0.2.6** published with zero warnings and errors from source revision c46a77811be8a1175d3dd6305f21568bc59d616b. Static inspection confirms that all three embedded resources exactly match source, the single-file bundle contains the matching inspected main DLL, and no installer-specific language satellite resources are present. Independent read-only review accepted the change. These are compilation and static results, not runtime verification; the mod DLL remains unchanged.

## Concise text-field focus and button feedback in 0.2.7

The user confirms that installer **0.2.6** installs successfully. The next installer refinement gives the welcome and status fields the short accessible names **Welcome and controls.** and **Status log.**, with empty redundant descriptions. The full welcome instructions remain in the selectable text itself. Native read-only and multiline state, Alt+W/Alt+L, cursor review, text selection and keyboard/controller copying remain available.

`InstallerReviewTextBox` retains the native edit role, read-only/multiline state and complete native Value/TextPattern behavior. Accessibility descriptions are empty. Both keyboard and managed controller focus request the short `ImportantMostRecent` summary **Name. Multi line. Read-only. Alt+W/L.** instead of automatically replacing speech with the full welcome text. The startup field remains Welcome and controls. Guard focus callbacks against stale focus, changed caret or selection, newer speech, disposed controls and a nonforeground installer; managed controller navigation avoids a duplicate summary. Manual KeyDown and MouseDown invalidate queued focus summaries so boundary text review, Ctrl+C and same-visit startup retries cannot be overwritten by stale summaries. Windows and the screen reader control the native role, wording and order, so an exact spoken phrase is not guaranteed.

`ReportAction` uses interrupting feedback for deliberate operation starts, Browse and Save opening/cancellation/results, validation, Abort and Quit. Play feedback is posted before the explicit Steam launch, while the installer still has focus. Dialog choice buttons announce synchronously while visible and stop refresh before closing; cancellation and close paths also report feedback. Existing Copy diagnostics/Save success and Show advanced checked-state announcements remain. Retain operation-specific failures and confirmation dialogs. Do not force action announcements over unrelated foreground applications or automatically speak every status-log entry.

Installer **0.2.7** published with zero warnings and errors from source revision 4632c06b3701f0fe8e5224241b395422a0f498ba. Static PE inspection confirms that all three embedded resources match source exactly: uninstall.ps1 **8,810 bytes**, Program.cs.txt **3,202 bytes**, and BuildReferenceGenerator.csproj.txt **529 bytes**. The **254-file** single-file bundle contains the matching inspected **323,584-byte** main DLL and no installer-specific language satellite assemblies. FileVersion is **0.2.7.0**; ProductVersion includes the same source revision. The final executable SHA-256 is **B019270FAA5E8DD8DF92125EE2B8A476AD60571F7A439A3A72DC17ADA53A4A45**. These are compilation and static inspection results. No automated tests or installer, uninstaller, controller, screen-reader or game execution is performed. Actual notification timing and reader wording await human verification. The main mod remains **0.9.12**, with **59 mod builds** and its DLL unchanged; no public release or tag is created.
