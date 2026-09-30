Bop It Access 0.1.0 — startup announcement prototype

What this does
--------------
When MelonLoader starts this mod, it initializes Tolk and requests the announcement
“Bop It Access Ready”. Tolk routes that text to an available screen reader. The mod
also enables Tolk's SAPI fallback, which uses the Windows default speech voice if no
supported screen reader is available.

This is only a first-step prototype. It does not yet read menu labels, selection
changes, instructions, or other game interface content.

Install
-------
1. Close Bop It! if it is running.
2. Extract all files in this ZIP into the Bop It! game folder:
   C:\Program Files (x86)\Steam\steamapps\common\Bop It!
   Allow Windows to merge the ZIP's Mods folder with the existing Mods folder.
3. Start Bop It! through Steam as usual.

The ZIP contains Mods\BopItAccess.dll and the Tolk runtime DLLs at the game-folder
root. MelonLoader must already be installed. This prototype was built for the
installed MelonLoader 0.7.3 Open-Beta and Bop It! (Unity 2022.3.50f1, x64).

Check the result
----------------
After starting the game, open Mods\BopItAccess.log in the game folder. A
successful start should include “Tolk initialized” and “Tolk accepted the 'Bop It
Access Ready' announcement.” The latter means Tolk accepted the asynchronous output
request; it does not prove that sound was audible. MelonLoader also reports status
in MelonLoader\Latest.log.

If you use NVDA or JAWS, start it before the game. NVDA was running during the
prototype check. If initialization fails, the log will show the error. Removing
Mods\BopItAccess.dll and the two Tolk DLL files from the game folder uninstalls
this prototype; leave the MelonLoader files in place.

Files and third-party notices
-----------------------------
Tolk is an open-source accessibility library by Christopher T. D. Kager. This
package includes Tolk.dll and nvdaControllerClient64.dll from a public build
bundle, without modifying those binaries. Tolk is licensed under GNU LGPL version
3; the accompanying GNU LGPL and GNU GPL license texts are in
THIRD-PARTY-LICENSES. Tolk source and license: https://github.com/dkager/tolk

The Bop It Access mod source is included in the separate source ZIP.
