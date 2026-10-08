# Installer diagnostics

Installer preview 0.2.1 records local diagnostic sessions for work on the installation process. The mod remains 0.9.11. This document describes installer recording; it does not replace the game's `Mods/BopItAccess.log` or MelonLoader's `Latest.log`. Installation never launches the game automatically. After a successful install, the separate Play Bop It! The Video Game button offers an explicit user-requested Steam launch. See [Installation flow](INSTALLATION-FLOW.md) for release versus alpha preparation, dependency locations and the first manual launch.

## Find, save and copy a session

Automatic session logs are UTF-8 text files in:

```text
%ProgramData%\BopItAccess\diagnostics
```

Paste that path into File Explorer's address bar and press Enter. `%ProgramData%` is Windows' shared application-data folder; it is usually `C:\ProgramData`, but the installer uses the system's configured location.

Each filename has the form `Installer-YYYYMMDDTHHmmssfffZ-<32-hexadecimal-session-ID>.log`. The date and time are UTC; the suffix distinguishes separate runs. Every entry includes an ISO 8601 UTC timestamp, elapsed milliseconds, the managed thread ID and a category.

The installer's status log remains a selectable, read-only text field. Tab to it to review or select text with the cursor keys. Check Show advanced (Alt+V) to reveal the two diagnostic buttons. They provide the complete current technical transcript, rather than just the shorter status messages:

| Action | Shortcut | Result |
| --- | --- | --- |
| Save diagnostics... | Alt+D | Choose a UTF-8 `.log` or `.txt` file. Save the current transcript there and keep adding new entries until the installer closes. |
| Copy diagnostics | Alt+C | Copy the current diagnostic transcript to the clipboard. |

These exports contain technical context in addition to the shorter visible status messages. The buttons are hidden until Show advanced is checked. Once visible, they become available after the initial checks finish, including when those checks fail, and remain available during subsequent operations. Copy diagnostics posts an accessible confirmation using native Windows UI Automation; Windows/screen-reader support determines whether it is spoken. Save diagnostics accepts `.log` or `.txt` files outside the installer's `%ProgramData%\BopItAccess` state folder and all known selected or installed game folders; Documents is a suitable destination. It saves the current transcript immediately, then continues recording into your chosen file until this installer session closes. Copy diagnostics gives a snapshot at the time you press it and replaces the clipboard's current contents. Files explicitly saved elsewhere are intentional user copies and remain there after uninstall.

If changing the game folder or starting an operation reveals that the active saved recording is inside that game folder, the installer asks you to save elsewhere before proceeding. This prevents an export from interfering with installation or disappearing during removal.

**Before the next installation or uninstall test, choose Save diagnostics.** This keeps an independent, continuing recording available even if successful uninstall removes the automatic logs. The installer now stays open after successful removal, including when opened from Windows Installed Apps. Review the result and use Quit when finished. Automatic managed-install logs and the running uninstall helper are removed after the window closes; a deliberately saved external recording remains.

## What is recorded

- Session start, installer version, operating-system/runtime information, full timestamps and elapsed time.
- Steam discovery, selected installation path, validation and prerequisite checks.
- Operation identity and transitions for install, alpha build, update, abort, rollback, recovery and uninstall.
- State changes and progress milestones, including download/extraction byte counts when available.
- HTTP requests and results, including status and timing; URL query strings, fragments and embedded user information are excluded.
- Compiler standard output and standard error, process completion and exit code.
- For alpha, local proxy-cache checks, offline reference preparation and the Cpp2IL/reference-helper output. These are background build tools, not a game launch.
- .NET runtime reuse or staging, SDK reuse or Microsoft SDK installation, and the subsequent deployment order.
- Managed exceptions with their detailed context instead of only the short message shown to the user.

Successful log entries establish what the installer requested or observed. They do not prove that a screen reader spoke a control, that downloaded software works correctly on that computer, or that a game session succeeded.

## Limits and failure behavior

Recording stays local. The installer does not upload these logs. Installation still makes the network requests needed to fetch GitHub metadata and official dependencies; recording does not add a telemetry service.

The session header records operating-system and runtime information. It does not collect environment-variable or registry dumps, preference-file contents or game-file contents. Recorded paths, status messages, compiler output and errors can still reveal local details.

Automatic recording normally keeps at most ten session files, with each session bounded to 8 MiB. Old sessions are removed as newer ones are created; a cleanup failure is reported and can leave older files available. The memory snapshot and exported transcript use the same session limit. On reaching it, the logger writes a limit notice and omits further entries for that run. Save the relevant session before further attempts replace it; start another installer session if a new recording is needed.

Progress entries appear at a new step, at ten-percent milestones, at completion, or after five seconds without another recorded change. They are diagnostic samples, not a record of every visual update.

If automatic disk recording is unavailable, diagnostics remain available in memory for the current session and through Save diagnostics or Copy diagnostics. A logging failure must not prevent installation. If writing to your chosen saved file fails, the installer reports the problem; that file may be incomplete, so choose another destination or use Copy diagnostics. Closing the installer loses memory-only data unless it was saved or copied.

Successful managed uninstall removes the owned diagnostic logs during final cleanup. An incomplete uninstall keeps the recovery state and diagnostic evidence for the remaining work. Successful cleanup of an older manual installation also clears the installer's owned diagnostic session files. The uninstall process does not search other folders for copies that a user chose to export.

## Reviewing a report

1. Note the approximate time, chosen action, expected outcome and what actually happened.
2. Check Show advanced, then choose Save diagnostics before starting the action you want to investigate. The selected file includes earlier entries and continues updating through that action. You can also collect an existing automatic session file, or use Copy diagnostics for a current snapshot.
3. Check the version and session header before reading the final error, cancellation or completion entries.
4. Work backward to the preceding operation, download, prerequisite or compiler entries. Progress is recorded at milestones rather than on every repaint.
5. For an alpha-build failure, include offline reference-tool and compiler output. For a dependency failure, include the Microsoft installer result and diagnostic entries. For game or speech problems after you manually launch it, also collect the separate mod and MelonLoader logs.

## Investigating the deployment sequence

For installer 0.2.1, a complete recording should show preparation before game-file deployment. Alpha either reuses verified matching game references or runs background assembly tools and the compiler. Release installs use an already compiled DLL and do not need an SDK or build references. The installer then deploys MelonLoader, immediately installs `Mods/BopItAccess.dll`, and finishes runtime files if required, Prism, loader defaults, documentation and uninstall support. No step starts `BopIt!.exe`.

If the game is opened independently during preparation, deployment stops. If the game is opened during a later operation and prevents rollback, the installer asks the user to close it and preserves recovery evidence; it does not close the user's game. Record both the installer action and when the game was opened manually.

An SDK used by alpha can already be installed system-wide or in an older game-root `dotnet` folder. New missing SDKs are installed system-wide and intentionally remain after abort or uninstall. A missing release runtime is staged from Microsoft's official runtime-only ZIP and deployed under `MelonLoader/Dependencies/dotnet` with ownership records. It is removed or restored with other owned loader files unless the loader must remain for other mods. Do not infer that .NET 6 is absent because the game-root `dotnet` folder is absent, or that the game runs on .NET 10 because a log reports a .NET 10 `hostfxr.dll`; the loader host and selected application runtime are distinct.

During the first manual game launch, MelonLoader can download dependencies and generate its runtime assemblies before the mod initializes. A minute without mod speech can be expected at that stage. The installer log ends at installation; use MelonLoader's `Latest.log` and `Mods/BopItAccess.log` to distinguish first-launch generation from an installer failure.

Inspect a file before sharing it. It can contain Windows usernames, full folder paths, installed dependency locations, game-library locations and exception details. URL sanitization does not remove that local context or all possible personal information in compiler/error text. Remove personal details if needed, keeping enough path structure and error context to diagnose the issue. Share only the intended exported text; do not include credentials or unrelated game files.

## Diagnosing the alpha template packaging failure

The supplied installer 0.1.7 recording shows Cpp2IL finishing at `2026-10-07T12:43:12.5821241+00:00` with exit code **0**. The next error is `System.IO.InvalidDataException: Missing embedded offline build helper resource: Program.cs.txt`. This isolates that attempt's failure to loading the installer’s own helper source, after Cpp2IL succeeded. It is not evidence of a failed Cpp2IL run or a game launch.

MSBuild inferred `.cs` in `Program.cs.txt` as the Czech culture code and assigned the template to a culture-specific satellite assembly. A `LogicalName` does not disable that inference. Installer 0.1.8 explicitly marks both helper templates and `uninstall.ps1` with `WithCulture=false` so the three resources reside in the neutral main assembly.

A build-time guard after `SplitResourcesByCulture` requires all three exact neutral resource names and fails compilation if they are missing or classified as culture-specific. Alpha also checks that its embedded templates are available and nonempty before prerequisite downloads, SDK installation or offline reference generation. Diagnostics record that preflight before continuing. A packaging failure stops with an explanatory error before those steps. The supplied failed attempt stopped in temporary alpha preparation, before a deployment transaction modified game files. This fix preserves the flow in [Installation flow](INSTALLATION-FLOW.md): prepare outside the game, deploy MelonLoader and the mod, then leave the first game launch to the user.

## Validation of preview 0.1.8

Installer 0.1.8 published with zero warnings or errors. Static PE inspection confirmed all three resources in the neutral main assembly and exact source payloads: `uninstall.ps1` (6,021 bytes), `Program.cs.txt` (3,202 bytes), and `BuildReferenceGenerator.csproj.txt` (649 bytes). The final single-file executable’s main DLL exactly matches that inspected assembly; no installer-specific language satellite is bundled. An independent static review found no blocking issues.

The build history records compilation and static review separately. No automated tests, offline reference generation, helper execution, installer/uninstaller execution or game launch were performed for this fix. Accessibility and actual installation behavior still require human testing on the target system. The mod source remains 0.9.11; these installer preparation steps do not imply that a mod copy is currently installed.

## Diagnosing the offline helper dependency failure

The next supplied installer 0.1.8 log is `BopItAccess-Installer-20261007T130629247Z-3b70d6999d9447e0bcb3a4abab4a4f80.log`. Its template preflight succeeds. Cpp2IL exits with code 0 at `2026-10-07T13:07:12.9120644+00:00`; the reference helper then builds with zero warnings and zero errors and exits with code 0 at `13:07:15.2574278+00:00`. Interop generation reaches `Pass16ScanMethodRefs`, then reports `FileNotFoundException: Could not load file or assembly 'Iced, Version=1.17.0.0, Culture=neutral, PublicKeyToken=5baba79f4264913b'` and exits with code 1 at `13:07:17.9269869+00:00`. These are UTC timestamps from the recording. The failure occurs during temporary preparation, before deployment changes game files.

The helper project copied loader DLLs through a wildcard `None` item. That puts files beside the program but does not register them as runtime dependencies in its `.deps.json`. The loader supplies Iced 1.21; its presence on disk alone did not let the .NET host resolve the generator's Iced reference. Installer 0.1.9 removes that content-copy wildcard and supplies explicit `Reference` items with `Private=true` for the complete 12-library managed dependency closure. In addition to the original eight references, it includes Iced, MonoMod.Backports, MonoMod.ILHelpers and Microsoft.Extensions.DependencyInjection.Abstractions. The helper uses the DLLs from the verified pinned loader, without another dependency download or a change to the generator.

The helper now compiles before downloading or executing Cpp2IL, its plugin or the Unity dependency archive. The installer statically checks the helper's runtime manifest, requires all 12 DLLs in its output, compares every copied DLL's SHA-256 with the staged loader version, inspects managed assembly metadata and checks that loader-provided assembly dependencies are covered. A missing or mismatched dependency stops preparation with its filename before the expensive assembly-tool steps. Existing template preflight, cancellation, bounded process lifetimes, ownership and manual-launch behavior remain.

For the next human installation attempt, save diagnostics before selecting Install alpha. Expect the helper compilation and the message confirming all 12 dependencies before Cpp2IL downloads or generation. Check the first detailed inner exception if a later generator failure remains. A successful compilation and dependency manifest inspection do not establish that all generator passes execute successfully on the target game.

## Validation of preview 0.1.9

Installer 0.1.9 and its embedded reference helper compiled with zero warnings and zero errors. The helper was compiled against a freshly downloaded official MelonLoader 0.7.3 archive whose pinned SHA-256 was verified. Static inspection found all 12 dependencies registered in the helper’s selected `.deps.json` runtime target. Every output dependency DLL was byte-for-byte identical to the verified archive; the output contained exactly 13 DLLs, including the helper. Static assembly-reference metadata inspection confirmed that the supplied versions were at least the versions requested by those references. Iced 1.21.0.0 is now registered in the manifest; the generator references Iced 1.17.0.0. This metadata inspection does not execute .NET resolution or any generator pass.

Static inspection of the final single-file executable confirmed its main DLL exactly matches the inspected installer assembly and that no installer-specific language satellite is included. Its three neutral resources match the current source files byte-for-byte: `uninstall.ps1` (6,021 bytes), `Program.cs.txt` (3,202 bytes), and `BuildReferenceGenerator.csproj.txt` (529 bytes after removing the wildcard). The production preflight checks runtime-manifest entries, copied payload hashes, managed metadata and coverage of loader-provided dependency names. It does not compare requested version/token identities; the additional version inspection above was a separate static build review.

No automated tests, helper execution, offline reference generation, installer/uninstaller execution or game launch were performed for this fix. Compilation and static inspection are distinct from a completed clean installation. The mod source remains 0.9.11 with 58 mod builds; there is no new mod build or GitHub Release.


## Diagnosing the shared-runtime dependency false positive

The supplied installer 0.1.9 recording is `BopItAccess-Installer-20261007T132500652Z-714ae19e402341de834423e7878d1539.log`. The embedded helper builds with zero warnings and zero errors. Its build exits with code **0** at `2026-10-07T13:25:42.8551316+00:00`. At `13:25:43.2299888+00:00`, the new preflight raises `InvalidDataException: Offline helper has an unregistered loader dependency: System.Runtime.CompilerServices.Unsafe.dll`. These are UTC timestamps from the recording. The helper itself has not run; Cpp2IL and deployment have not started. The recording establishes a preflight rejection, not a failed generator pass or an additional missing external library.

The 0.1.9 guard classifies every referenced DLL found in the loader directory as an external helper dependency. The full verified official MelonLoader archive also contains `System.Runtime.CompilerServices.Unsafe.dll`, version 6.0.0.0. The selected .NET 6 framework supplies an assembly with the same name, version, culture and public-key token. A duplicate loader copy therefore does not establish that the helper needs its own runtime-manifest entry for this library. The earlier static review used a reduced 12-library staging set and did not cover this duplicate from the complete archive.

Installer 0.1.10 selects the highest installed stable .NET 6.0 patch beneath the selected `dotnet.exe`. Framework references must appear in that runtime's `Microsoft.NETCore.App.deps.json` and have a regular DLL whose assembly name, culture and public-key token match and whose version is at least the requested version. Identity reads inspect metadata without loading or running those assemblies. This is not a blanket exemption for `System.*`: `System.Diagnostics.DiagnosticSource` 10 remains one of the 12 required private dependencies because the framework's version 6 is insufficient. Helper execution is explicitly bound to the inspected patch with `--fx-version <patch>` and `--roll-forward Disable`, preventing inherited roll-forward settings from selecting another framework. SDK-build arguments remain unchanged.

All 12 explicit dependencies still require runtime-manifest entries, regular managed DLLs and SHA-256 equality with the staged loader. Missing or incompatible references report their filename before Cpp2IL begins. Review the complete official loader archive and the selected .NET 6 runtime together; a trimmed helper dependency set cannot reproduce the false positive. Save diagnostics before the next human Install alpha attempt. A static identity check does not demonstrate runtime assembly resolution or successful generation for the game.

## Validation of preview 0.1.10

The self-contained Windows x64 installer 0.1.10 compiled with zero warnings and zero errors. Static inspection used the full official MelonLoader 0.7.3 archive after verifying its pinned checksum, with 47 top-level DLLs in the `net6` directory, rather than the earlier reduced 12-library staging set. For the 12 helper dependencies, all 73 assembly-reference edges were satisfied by either explicit helper libraries (19) or compatible libraries in the selected .NET 6 shared framework (54). None were missing or incompatible. The inspection compared name, culture, public-key token and available version against every requested reference, including explicit private references; production private-reference checks remain manifest, managed-metadata and staged-payload verification. Duplicate `Unsafe` 6 is compatible with the framework, while `DiagnosticSource` 10 remains explicitly supplied. The helper output retains exactly 13 DLLs: its program plus the 12 required private dependencies.

Static inspection of the final single-file installer confirms that its main DLL matches the inspected compiled assembly and its three neutral embedded resources match their source files; no installer-specific language satellite is included. Source review confirms helper execution is pinned to the inspected framework patch and SDK build arguments are unchanged. These are compilation, source review and static package/metadata inspections. No automated tests, helper execution, offline reference generation, installer/uninstaller run or game launch were performed. They do not establish that a clean alpha installation completes on the target system.

The mod source remains 0.9.11 with 58 mod builds. There is no new mod build or GitHub Release. The installer leaves the game’s first launch to the player.


## Delivery changes in preview 0.2.0

The supplied human recording is `BopItAccess-Installer-20261007T134543496Z-87f95935135b492da6301206f7b8df9a.log`. It shows the interop generator exiting **0** at `2026-10-07T13:46:48.2388349+00:00`, followed by a committed `0.9.11-alpha.e2c0a77` installation at `13:46:58.0283274+00:00`. Uninstall completes at `13:48:27.4053045+00:00`, and the window closes with exit code **0** at `13:48:33.2343540+00:00`. These are UTC timestamps from the log. The user reports both operations succeeded but found `Plugins`, `UserLibs`, `UserData` and `MelonPreferences.cfg` left behind.

The regular uninstall success handler deliberately called Close after the success dialog. The supplied session records no crash. Remove that unconditional close, route explicit Quit and the window close button through one safe shutdown path, and treat a completed uninstall manifest as uninstalled when publishing UI state. An active installer can then review diagnostics or reinstall without closing.

### Status versus diagnostics

`InstallerService.Log` always records the original technical message. `InstallerFeedback.UserStatus` maps meaningful stages and warnings to short user-facing messages; compiler stdout/stderr, checksum checks, file paths and repeated per-file messages remain in diagnostics. `USER-STATUS` records concise download and completion notices when produced by progress callbacks. The original process and exception records remain available for investigation.

The UI requests native Windows UI Automation notifications for its welcome, a newly available update and clipboard-copy confirmation. `ACCESSIBILITY` records whether Windows accepted posting the notification, not whether a screen reader spoke it. No Prism, Tolk or secondary speech engine is bundled into the installer. Foreground activation is attempted once when shown; Windows can deny it, and that request is not repeatedly used to steal focus while the user works elsewhere.

The visual progress bar now represents weighted progress across the whole operation, using five-percentage-point increments. It never resets between downloads, extraction, reference generation, compilation and deployment. A stage without a measurable fraction holds its current estimate; 100 is published only after commit or complete removal. This is an estimate of work stages, not a time-to-finish promise. Underlying byte/file counts and process stage transitions remain in diagnostic `PROGRESS` entries with their existing sampling rules.

### Uninstall scope and remaining records

After normal confirmation, the user chooses `CurrentUser` or `AllUsers` preference cleanup. Both remove shared files from the selected game installation; scope applies to `BopItAccess.*` registry preferences in Windows user profiles. The original requesting SID is carried across elevation, including an Apps & Features script launch. Once removal starts, the manifest records the scope/SID for consistent retry. Native game PlayerPrefs and shared Microsoft .NET/SDK installations remain untouched.

When removing its installer-owned loader and no other mods need it, cleanup validates the known `Loader.cfg` and `MelonPreferences.cfg` and prunes empty `Plugins`, `UserLibs` and `UserData` folders. Linked paths are rejected; unknown files, pre-existing shared support and unrelated mods remain protected. The short status can explain a preservation limit; full paths and retained items remain in diagnostics.

Deferred cleanup waits until the installer exits. A same-window reinstall clears the completed ownership baseline before creating a fresh transaction, retaining a completed-uninstall tombstone when necessary. Cleanup selects the live manifest before the tombstone, refuses an active transaction or a new installation, and cannot delete newer state because of a previous uninstall. This also permits cleanup after an aborted reinstall. The game-root `BopItAccess-uninstall.ps1` is recorded transactionally and delegates to the ProgramData launcher. Future mod source builds link the same script into output; no new mod DLL is built for this installer update.

### Quit and controller reports

An installation Quit prompt is live: it checks completion while the user decides, updates its wording when the backend stops, and avoids a competing success popup. A confirmed quit cancels an active install and waits for rollback, but never cancels a completed transaction. Uninstall file removal and Microsoft shared-component setup finish safely before shutdown. Preserve the log if these operations fail rather than describing an incomplete rollback as complete.

XInput controller support is limited to compatible Xbox-style devices. Inputs are gated to the installer or its own foreground dialogs; held buttons are absorbed at window changes and connection, sticks use hysteresis, and directions have bounded repeat. Reports should identify the controller, focused field/dialog, whether a button was tapped or held, and the expected action. D-pad/left stick navigate or review text; bumpers move focus even from text fields, A activates, B cancels/back or requests main-window abort, Start quits/back and Y changes advanced visibility. In managed text fields, LT+directions moves by word with Left/Right or paragraph with Up/Down; RT+directions extends selection; LT+RT selects words or paragraphs. X copies the current selection, with accessible success/empty-selection/failure feedback. Native Windows folder/save-dialog behavior still requires human verification.

### Validation boundary

Installer 0.2.0 compiled with zero warnings and zero errors. PowerShell AST parsing found no errors. Static inspection confirms all three neutral embedded resources match their source bytes, the published single-file bundle’s main assembly exactly matches the inspected DLL, and no installer-specific language satellite assemblies are included. These checks do not execute the UI, controller handling, uninstall or game.

The supplied recording and user report establish successful installation and removal with **0.1.10**. They do not verify the new **0.2.0** interface, scoped cleanup or controller behavior. Validation of this update is compilation and static review only: no automated tests, installer/uninstaller execution, reference generation or game launch. Screen-reader notifications, foreground behavior, live Quit races, text selection, native dialogs and both preference scopes remain human verification tasks. The mod remains **0.9.11**, with **58 mod builds**; no new mod DLL or GitHub Release is created.

## Focus and selected-text review in preview 0.2.1

The user has since confirmed that installer **0.2.0** works apart from its initial foreground and keyboard focus. That human report applies to the prior preview. The startup and controller text changes in **0.2.1** still require human review.

The installer makes at most eight startup activation attempts on a 125 ms timer, with an absolute two-second lifetime. It confirms actual foreground status before giving the game-folder field keyboard focus; if initialization disables that field, it chooses the enabled status-field fallback. It waits for stable foreground and control focus for 250 ms, then stops. New user input, a deliberate enabled-control focus change, an operation or an owned dialog ends the startup attempts. A single temporary visibility pulse requests and then removes topmost status. Windows foreground restrictions can still refuse activation. If necessary, switch to the installer with Alt+Tab and focus the game-folder field with Alt+G.

`FOCUS` diagnostics record attempts, the actual foreground window/process and keyboard focus, API outcomes and the reason retries ended. Successful activation diagnostics cannot establish that a screen reader announced the field. Report whether the window became foreground, which control held keyboard focus, and whether an unrelated foreground window or deliberate focus change was involved.

In the status field and the installer's other text fields, D-pad/left-stick directions use arrow-key semantics: Left/Right moves by character and Up/Down by visual line. LT supplies Ctrl for word movement with Left/Right and hard-newline paragraph movement with Up/Down. RT supplies Shift to extend selection; both triggers supply Ctrl+Shift to select words or paragraphs. The selection anchor survives reversing direction. The bumpers move focus to the previous/next control even while text is focused. Reports should distinguish character, line, word and paragraph movement and state which triggers were held.

X copies the current selected text in the focused installer field and leaves the selection intact. It no longer selects the whole field, so select the desired part before copying. Keyboard Ctrl+C retains its native behavior. This selected-text copy is separate from Copy diagnostics (Alt+C), which copies the full technical snapshot. Selected-text copy sends native UI Automation feedback: “Text copied to clipboard.”, “No text selected.” or “Could not copy text.” In native folder/save edit controls, X uses local `WM_COPY` and checks the changed clipboard sequence and owner before claiming success; modifier-based word/paragraph review is scoped to the installer's managed text fields.

When the caret is collapsed, controller text review posts native UI Automation notifications for the character, word, visual line or paragraph at the caret. Selection changes with RT use native selection events for added or removed text. The review/copy handlers do not record the reviewed or copied contents in diagnostics; the original technical transcript still contains its normal status messages and paths. `ACCESSIBILITY` records posting success, which does not prove that a screen reader spoke. Report which field was focused, whether it had a selection and which copy action was used. Do not paste sensitive text into a report without reviewing it.

Validation for this update is compilation and static inspection only. No automated tests, installer/uninstaller, controller or screen-reader execution, reference generation or game launch is performed. Native Windows folder/save-dialog interaction and actual accessible feedback remain human review tasks. The main mod remains **0.9.11**, with **58 mod builds**; no new mod DLL, tag or GitHub Release is created.
