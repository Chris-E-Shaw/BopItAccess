Bop It Access 0.6.7 - One-on-One Feedback

What this does
--------------
Speech is on by default. When the mod loads with speech on, it announces
"Bop It Access speech is ready. The game is still loading. Wait for the main menu announcement before using the controls." through Tolk. It reads
the focused main-menu button and the focused Settings row. Settings values are
spoken with the row name on focus. Changing a value while focus stays on that
row speaks only the new value. AUDIO LATENCY, CONTROLS, and GO ONLINE are action
buttons, so they are spoken without the game's meaningless placeholder "0".
The Settings menu also has a LIMIT FPS slider with 30, 60, 120, 240, and
UNLIMITED choices. It starts at 60 for a new installation and remembers the
selected value between sessions. Focus speaks the name and value; changing it
speaks only the new value. The cap changes Unity's target frame rate while
leaving game time scale, fixed update timing, and audio untouched.
Like any frame cap, a lower setting also means fewer frame-based input polls.
If 30 FPS feels less responsive in a fast game, choose 60, 120, or UNLIMITED.
The MUTE AUDIO IN BACKGROUND toggle appears directly below VOICE OVER in
Settings. When enabled, it mutes game audio while the game window is not
focused, then restores the previous game audio state when focus returns.
It starts Off and is saved between sessions.
Settings indexing now waits for the mod's audio, FPS, and SPEECH rows before
announcing the first focused item on a newly opened Settings screen.

Below Controls, Settings now has a SPEECH menu. SPEECH OUTPUT uses the same
saved master switch as F8 or controller Select, including the spoken recovery
instructions when speech is turned off. OUTPUT MODE starts at Auto: the mod
speaks through a detected screen reader, or uses SAPI when none is running.
SAPI can be selected directly. The menu also lists JAWS, Window-Eyes, NVDA,
System Access, and ZoomText. Direct NVDA output is available when NVDA is
running. The other named readers are used when Tolk detects them as the active
driver; if the chosen reader is unavailable, the mod announces a SAPI fallback.
Tolk's 64-bit build does not support SuperNova, so it is not listed.
MUTE SPEECH IN BACKGROUND is a saved toggle, Off by default. When enabled,
the mod stops speaking as soon as the game loses window focus. Speech created
while the game is in the background is discarded, and announcements resume
with new activity after focus returns. If speech itself is Off when the game
regains focus, the mod gives the current keyboard and controller recovery
instructions once.

The SPEECH menu also has INDEXING, off by default and saved between sessions.
When enabled, a focused menu item includes its position, such as "PLAY, 1 of 6".
This applies across the main and Settings menus, Controls, play modes, speech
settings, game-over choices, leaderboards, achievements, credits, and other
supported screens. The count follows the currently available choices. Changing
a slider or toggle while it remains focused still announces only the new value.

READ CONTROL TYPES is another saved SPEECH toggle, off by default. When enabled,
the focused item's type follows its name and precedes its value and index:
"MUSIC slider, 30, 1 of 11", "VIBRATION toggle, On, 5 of 11", or
"PLAY button, 1 of 6". Menus also identify tabs, text fields, and readable
list items where relevant. Value changes continue to speak only the new value.
SLIDER RANGES is a saved toggle, Off by default. When enabled, focused sliders
also report their available endpoints after the current value, such as
"MUSIC slider, 30, range 0 to 100, 1 of 12" when indexing and control types
are enabled. Moving a slider still speaks only the new value.
ONE-ON-ONE FEEDBACK is a saved toggle, Off by default. When enabled, it
announces the active colour at the start of a One on One round and when that
colour changes. A lost life announces the remaining count, such as "2 lives"
or "1 life". If both players lose a life on the same shared Bop cue, both
counts are identified by colour. The feature runs only during One on One play.

READ BUTTON HINTS is a saved SPEECH toggle and is On by default. Turning it
Off suppresses all button-hint speech. BUTTON HINTS DELAY has None, 5 seconds
(May interrupt speech), 10 seconds, 15 seconds, 30 seconds, and 60 seconds.
It defaults to None. With None, the valid inputs for the current screen and
their actions are included in the focused item's ordinary speech string,
after a full stop. The action for the focused control is spoken before general
menu navigation. There is no separate first hint announcement. With a timed
delay, the first hint announcement follows that much inactivity. The 5-second option can
interrupt speech already in progress; longer delays queue behind it. After
the first hint announcement, another delay starts only when the player gives
input, unless repeats are enabled.
HINTS TYPE is a saved slider with Automatic, Keyboard, Controller, and Both.
Automatic is the default and follows the most recently used keyboard or
controller input. Mouse use counts as keyboard. Keyboard and Controller speak
only that device's hints; Both gives the full dual-device instructions from
previous versions. Speech-off recovery always includes both devices so the
player can find the control that turns speech back on.

Button hints now name keyboard and controller inputs for menu selection,
navigation, Back, sliders, song selection, achievement pages, and other
supported controls. The mod reads the game's current bindings so native
rebinding changes are reflected in these hints. When no controller is
connected and the game has different face-button names on different controller
types, the hint uses "confirm button" or "back button" rather than assuming an
Xbox layout. Leaderboard score rows use Page Up and Page Down on keyboard.
Controller up/down reads rows only when no leaderboard control has focus;
hints report only the controls available for the selected HINTS TYPE.

REPEAT BUTTON HINTS is a separate saved slider: Off, 2x, 3x, 4x, 5x, or
Infinitely. It defaults to Off. When enabled, it sends that many extra hint
announcements after the first one; for example, 2x means two additional
reminders. REPEAT INTERVAL sets the delay between them to 15, 30, 45, or
60 seconds and defaults to 15 seconds. With BUTTON HINTS DELAY set to None,
the repeat timer begins immediately after input. Input or a screen change
restarts the hint cycle for the current screen. Hints are suppressed during
active gameplay and the beat-timing phases of Audio Calibration, where extra
speech could mask a cue. An existing saved 15-, 30-, or 60-second reminder
delay from version 0.6.2 becomes the new BUTTON HINTS DELAY value.

The SPEECH menu also has VOICE, VOLUME, RATE, and PITCH controls. Voice
lists the system default and installed 64-bit SAPI voices. Volume starts at
100; Rate and Pitch start at 50. Volume ranges from 5 to 100 so SAPI recovery
notices remain audible. Rate and Pitch range from 0 to 100. All three move in
steps of five. They apply to SAPI mode and Auto's SAPI fallback.
Speech settings are remembered between sessions. Back returns to Settings.
This update restores the game's native Settings row layout so up/down
navigation remains on Settings rows after SPEECH is added.
It also starts the SPEECH submenu on SPEECH OUTPUT each time it opens,
preventing a previously selected BACK row from closing the menu immediately
when Enter is used to reopen it.
The input that opens SPEECH is now ignored by its rows until that input is
released, so reopening the menu cannot also toggle speech off. The mod's
added Controls binding rows likewise wait for the opening input to be
released before accepting a rebinding request.

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
only, before gameplay starts. Its default inputs are R on keyboard and LT
(left trigger) on controller. The screen introduction announces the current
binding. Starting play stops any speech left from song selection so it cannot
mask the game's verbal cues. Descriptions begin with the scene details rather
than repeating the selected stage name.
R is also listed as Reset Gyro in the game's input asset. The mod does not
change that or any other native binding, and its own action is disabled during
gameplay.

On the final result screen, Solo, Party, and Pass It announce the final score
before the menu speech. Solo reads the focused Replay and Leaderboard buttons
and explains Back. The other modes read their available Continue, Replay, and
Back prompts. One on One shows a winner rather than a numeric final score, so
the mod announces the winner shown there. Score speech takes priority over
the initial menu announcement; result-screen prompts are queued after it.
Subsequent menu focus changes interrupt one another. Rapid changes immediately
after game over are combined until the short score announcement has had time
to finish, keeping the latest focused menu choice.
Solo high-score and Party rank announcements are spoken when the game reports
a fresh leaderboard result.
READ SCORE repeats the final result on demand only while the game-over result
screen is visible. The default inputs are T on keyboard and left stick press on
controller. Solo, Party, and Pass It repeat their final score; One on One
repeats the winner shown by the game. The action is disabled during gameplay,
song selection, and all other screens. RT (right trigger) was not used as the
default because the game already binds it to Reset Gyro and Auto Play.
Requested repeats speak immediately and can be interrupted by result-menu
navigation. Only the automatic result announcement delays the initial menu
speech so the score is heard first.
ONE-ON-ONE FEEDBACK can be enabled in Settings > Speech for spoken active
colour and remaining lives during that mode. Shared Bop cues do not by
themselves identify one colour, so the mod retains the last definite colour.

TOGGLE SPEECH turns all ordinary mod speech on or off from any screen. Its
defaults are F8 on keyboard and Select on controller. When turned off, the mod
stops current speech and announces that speech is off, along with the current
keyboard and controller controls for turning it back on. The off state is
saved between game sessions. If the game starts with speech off, the mod gives
those recovery instructions instead of its usual loading message. Turning
speech back on announces "Speech on." Other mod speech stays silent while off.
The Toggle Speech control remains active even while speech is off.

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
It announces how to return to Settings once per visit. When Reset to Default changes a
binding, it reports that the bindings were reset.

The mod adds seven rows to the game's Controls menu: Group Previous,
Group Next, Date Previous, Date Next, READ DESCRIPTIONS, READ SCORE, and
TOGGLE SPEECH. The first four
address the leaderboard filters
reached with O/P and K/L on the default keyboard layout, or the bumpers and
D-pad left/right on a controller. Focus a row to hear its current binding,
then use the game's normal Bop/confirm action to rebind it. The rows scroll
inside the existing Controls panel. READ DESCRIPTIONS can also be rebound for
keyboard and controller. Its binding is saved by the mod, and Reset to Default
restores R and LT. READ SCORE can also be rebound for keyboard and controller;
Reset to Default restores T and left stick press. The game's original binding rows and the four leaderboard
rows retain their current rebinding behavior.
TOGGLE SPEECH can be rebound for keyboard and controller. Its bindings are
saved by the mod, and Reset to Default restores F8 and Select. If the binding
is changed while speech is off, the mod announces the new recovery controls.
This update stops the stale Controls row from announcing "Rebinding failed"
repeatedly after its scene closes. Read Descriptions no longer temporarily
overrides any of the game's own input bindings.

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
mod announces each change. Press R or LT to hear the currently selected stage
description. Bop starts the chosen mode; Back returns.
At the end of a game, listen for the score or One on One winner before the
result-screen controls are announced. Press T or left stick press to repeat the
final result while the game-over screen is visible.
Press F8 or controller Select to turn mod speech off or on from any screen.
In Settings > Speech, use READ BUTTON HINTS to enable or disable instructions.
HINTS TYPE chooses Automatic, Keyboard, Controller, or Both for those hints.
BUTTON HINTS DELAY chooses whether they accompany focus speech or follow a
period of inactivity. REPEAT BUTTON HINTS and REPEAT INTERVAL control any
additional reminders.
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
