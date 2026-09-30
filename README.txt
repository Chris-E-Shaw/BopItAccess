Bop It Access 0.5.7 - On-demand stage descriptions

What this does
--------------
When the mod loads, it announces "Bop It Access Ready" through Tolk. It reads
the focused main-menu button and the focused Settings row. Settings values are
spoken with the row name on focus. Changing a value while focus stays on that
row speaks only the new value. AUDIO LATENCY, CONTROLS, and GO ONLINE are action
buttons, so they are spoken without the game's meaningless placeholder "0".

Inside Play, the mod reads Solo, Party, Pass It, and One on One when focused.
On the following song-selection screen, it announces the current theme
(Shapes, Space, City, or Office) and whether Extreme mode is on. Twisting to
change the song speaks only the new theme. Pulling to change difficulty speaks
only the new Extreme state. The screen introduction also explains the Twist,
Pull, Bop, and Back actions.
This full introduction is repeated whenever a mode is selected and the song
screen opens again, with the current theme and Extreme state.
The READ DESCRIPTIONS control speaks a visual description of the selected
Shapes, Space, City, or Office stage on demand. It is available on this screen
only, before gameplay starts. Its default inputs are D on keyboard and LT
(left trigger) on controller. The screen introduction announces the current
binding. If a description is still being spoken when play starts, the mod
stops it so it cannot mask the game's verbal cues.

On the final result screen, Solo, Party, and Pass It announce the final score
before the menu speech. Solo reads the focused Replay and Leaderboard buttons
and explains Back. The other modes read their available Continue, Replay, and
Back prompts. One on One shows a winner rather than a numeric final score, so
the mod announces the winner shown there. Score speech takes priority over
the initial menu announcement; result-screen prompts are queued after it.
Solo high-score and Party rank announcements are spoken when the game reports
a fresh leaderboard result.

The leaderboards reached from the main menu, Solo results, and Party results
announce the selected song, device, group, and date where available. They read
rank, player name, and score, including loading and empty-result states. Page Up
and Page Down read individual score rows even when a filter has focus. Native
focused controls such as Local, Friends, Global, Today, This Month, All Time,
Back, and Continue are spoken. The Party leaderboard also reports its name
selection and confirmation state.

This update keeps result leaderboards silent during mode and song selection.
Leaderboard filter names speak first, with score summaries queued after them.
The achievements menu button speaks normally; book instructions wait until
its pages are actually opened, and closing is announced only after that.

The in-game achievements book announces its visible page and each achievement's
name, description, and locked or unlocked state. Use Up and Down to read items
on a page. The game's Left and Right controls turn pages as usual.

Credits announce the first line when opened. Use the game's Up and Down menu
controls to read each credit line. The visual automatic scroll continues as
before. If those controls are unavailable, lines are queued as they appear;
if that also cannot work, the whole credits text is announced once.

Inside Controls, the mod reads Bop, Bop (Player 2), Flick, Twist, Spin, Pull,
and Reset to Default. It reads the current binding for the active input device,
announces changed bindings, and reads the game's visible rebinding feedback.
It also announces how to return to Settings. When Reset to Default changes a
binding, it reports that the bindings were reset.

The mod adds five rows to the game's Controls menu: Group Previous,
Group Next, Date Previous, Date Next, and READ DESCRIPTIONS. The first four
address the leaderboard filters
reached with O/P and K/L on the default keyboard layout, or the bumpers and
D-pad left/right on a controller. Focus a row to hear its current binding,
then use the game's normal Bop/confirm action to rebind it. The rows scroll
inside the existing Controls panel. READ DESCRIPTIONS can also be rebound for
keyboard and controller. Its binding is saved by the mod, and Reset to Default
restores D and LT. The game's original binding rows and the four leaderboard
rows retain their current rebinding behavior.

Inside Audio Calibration, the mod reads the Calibrate, Back, and Bop controls,
announces the instructions and calibration stages, reads the warmup countdown,
and announces the displayed latency result. It does not speak every beat during
the timing exercise so the beat remains audible.

Other gameplay screens do not yet have speech feedback. Moving the mouse over
an item without giving it Unity UI focus may not speak.

Install
-------
1. Close Bop It! if it is running.
2. Extract all files in this ZIP into the Bop It! game folder:
   C:\Program Files (x86)\Steam\steamapps\common\Bop It!
   Allow Windows to replace Mods\BopItAccess.dll and merge the Mods folder.
3. Start your screen reader, then start Bop It! through Steam as usual.

MelonLoader must already be installed. The ZIP includes Mods\BopItAccess.dll,
Tolk.dll, and nvdaControllerClient64.dll. This mod was built for the installed
MelonLoader 0.7.3 Open-Beta and Bop It! (Unity 2022.3.50f1, x64).

Try the supported screens
-------------------------
Open Play and move among the four modes. Choose one to reach song selection.
Twist to cycle through themes and Pull to switch Extreme mode on or off. The
mod announces each change. Press D or LT to hear the currently selected stage
description. Bop starts the chosen mode; Back returns.
At the end of a game, listen for the score or One on One winner before the
result-screen controls are announced.
Open Leaderboards from the main menu or a result screen. Change a filter to
hear its new selection and read individual scores with Page Up and Page Down.
By default, O/P move between leaderboard groups and K/L move between date
ranges. Their corresponding controller inputs are left/right bumper and
D-pad left/right. The four new Controls rows are intended to reassign them.
Open Achievements and turn pages with Left and Right; use Up and Down for each
entry. Open Credits and use Up and Down to read its lines independently of the
visual scroll.

If speech is missing, check Mods\BopItAccess.log in the game folder. It records
panel detection, selected UI objects, and whether Tolk accepted announcements.
Tolk acceptance does not by itself prove that speech was audible.

To disable the mod, remove Mods\BopItAccess.dll. MelonLoader can remain installed.

Files and third-party notices
-----------------------------
Tolk is an open-source accessibility library by Christopher T. D. Kager. This
package includes Tolk.dll and nvdaControllerClient64.dll from a public build
bundle, without modifying those binaries. Tolk is licensed under GNU LGPL version
3; the accompanying GNU LGPL and GNU GPL license texts are in
THIRD-PARTY-LICENSES. Tolk source and license: https://github.com/dkager/tolk

The Bop It Access mod source is in the separate source ZIP.
