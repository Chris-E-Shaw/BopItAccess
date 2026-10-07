# Installer diagnostics

Installer preview 0.1.7 records local diagnostic sessions for work on the installation process. The mod remains 0.9.11. This document describes installer recording; it does not replace the game's `Mods/BopItAccess.log` or MelonLoader's `Latest.log`. The installer no longer launches the game. See [Installation flow](INSTALLATION-FLOW.md) for release versus alpha preparation, dependency locations and the first manual launch.

## Find, save and copy a session

Automatic session logs are UTF-8 text files in:

```text
%ProgramData%\BopItAccess\diagnostics
```

Paste that path into File Explorer's address bar and press Enter. `%ProgramData%` is Windows' shared application-data folder; it is usually `C:\ProgramData`, but the installer uses the system's configured location.

Each filename has the form `Installer-YYYYMMDDTHHmmssfffZ-<32-hexadecimal-session-ID>.log`. The date and time are UTC; the suffix distinguishes separate runs. Every entry includes an ISO 8601 UTC timestamp, elapsed milliseconds, the managed thread ID and a category.

The installer's status log remains a selectable, read-only text field. Tab to it to review or select text with the cursor keys. Two buttons beside the status area provide the complete current diagnostic transcript:

| Action | Shortcut | Result |
| --- | --- | --- |
| Save diagnostics... | Alt+D | Choose a UTF-8 `.log` or `.txt` file. Save the current transcript there and keep adding new entries until the installer closes. |
| Copy diagnostics | Alt+C | Copy the current diagnostic transcript to the clipboard. |

These exports contain technical context in addition to the shorter visible status messages. The buttons become available after the initial checks finish, including when those checks fail, and remain available during subsequent operations. Save diagnostics accepts `.log` or `.txt` files outside the installer's `%ProgramData%\BopItAccess` state folder and all known selected or installed game folders; Documents is a suitable destination. It saves the current transcript immediately, then continues recording into your chosen file until this installer session closes. Copy diagnostics gives a snapshot at the time you press it and replaces the clipboard's current contents. Files explicitly saved elsewhere are intentional user copies and remain there after uninstall.

If changing the game folder or starting an operation reveals that the active saved recording is inside that game folder, the installer asks you to save elsewhere before proceeding. This prevents an export from interfering with installation or disappearing during removal.

**Before the next installation or uninstall test, choose Save diagnostics.** This keeps an independent, continuing recording available even if successful uninstall removes the automatic logs. The Windows uninstall window can close after you dismiss its success message, so save before starting removal. When testing from the regular installer window, leave it open until the final result has been recorded, then review or share the saved file.

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
2. Choose Save diagnostics before starting the action you want to investigate. The selected file includes earlier entries and continues updating through that action. You can also collect an existing automatic session file, or use Copy diagnostics for a current snapshot.
3. Check the version and session header before reading the final error, cancellation or completion entries.
4. Work backward to the preceding operation, download, prerequisite or compiler entries. Progress is recorded at milestones rather than on every repaint.
5. For an alpha-build failure, include offline reference-tool and compiler output. For a dependency failure, include the Microsoft installer result and diagnostic entries. For game or speech problems after you manually launch it, also collect the separate mod and MelonLoader logs.

## Investigating the deployment sequence

For installer 0.1.7, a complete recording should show preparation before game-file deployment. Alpha either reuses verified matching game references or runs background assembly tools and the compiler. Release installs use an already compiled DLL and do not need an SDK or build references. The installer then deploys MelonLoader, immediately installs `Mods/BopItAccess.dll`, and finishes runtime files if required, Prism, loader defaults, documentation and uninstall support. No step starts `BopIt!.exe`.

If the game is opened independently during preparation, deployment stops. If the game is opened during a later operation and prevents rollback, the installer asks the user to close it and preserves recovery evidence; it does not close the user's game. Record both the installer action and when the game was opened manually.

An SDK used by alpha can already be installed system-wide or in an older game-root `dotnet` folder. New missing SDKs are installed system-wide and intentionally remain after abort or uninstall. A missing release runtime is staged from Microsoft's official runtime-only ZIP and deployed under `MelonLoader/Dependencies/dotnet` with ownership records. It is removed or restored with other owned loader files unless the loader must remain for other mods. Do not infer that .NET 6 is absent because the game-root `dotnet` folder is absent, or that the game runs on .NET 10 because a log reports a .NET 10 `hostfxr.dll`; the loader host and selected application runtime are distinct.

During the first manual game launch, MelonLoader can download dependencies and generate its runtime assemblies before the mod initializes. A minute without mod speech can be expected at that stage. The installer log ends at installation; use MelonLoader's `Latest.log` and `Mods/BopItAccess.log` to distinguish first-launch generation from an installer failure.

Inspect a file before sharing it. It can contain Windows usernames, full folder paths, installed dependency locations, game-library locations and exception details. URL sanitization does not remove that local context or all possible personal information in compiler/error text. Remove personal details if needed, keeping enough path structure and error context to diagnose the issue. Share only the intended exported text; do not include credentials or unrelated game files.

## Validation of this preview

The project records compilation and static review separately in the build history. No installer, uninstall or game execution is implied by preparing these diagnostics. Accessibility and actual installation behavior still require human testing on the target system.
