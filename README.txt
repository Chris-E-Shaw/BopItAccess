Bop It Access 0.9.10 - Prism Speech and Braille

What this does
--------------
The mod follows the game's Settings > Language selection for speech. It includes
English, French, Italian, German, Spanish (Spain), Spanish (Latin America),
Japanese, Korean, Simplified Chinese, and Brazilian Portuguese. Changing the
game language also changes mod announcements and the in-game user's guide.
The initial translations are machine-generated drafts and need review
by fluent speakers.
Speech is on by default. When the mod loads with speech on, it announces
"Bop It Access speech is ready. The game is still loading. Wait for the title screen or main menu announcement before using the controls." through Prism. If the title screen appears, the mod announces the current Bop input for opening the main menu. It reads
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
On the first-ever use of the mod, the game's native MUSIC, SFX, and VOICE OVER
sliders start at 30. Upgrading keeps previously saved game audio settings.
Once the game reaches its main menu for the first time, a welcome screen takes
focus. Its message can be focused again with Up, and its choices open Mod
Settings, open the user's guide inside the game, or continue to the main menu.
The welcome screen is marked complete only after a choice successfully leaves
it. Closing the game while it is open leaves it ready for the next launch.
Settings indexing now waits for the mod's audio, FPS, and MOD SETTINGS rows before
announcing the first focused item on a newly opened Settings screen.

Below Controls, Settings now has a MOD SETTINGS menu. SPEECH OUTPUT uses the same
saved master switch as F8 or controller Select, including the spoken recovery
instructions when speech is turned off. BRAILLE OUTPUT starts On and is saved
between sessions. Prism sends announcements to a compatible screen reader's
braille output when this setting is On. Turning it Off stops the mod's braille
messages while leaving speech available.
OUTPUT MODE starts at Auto: the mod uses a supported running screen reader
through Prism, then OneCore when none is available, and SAPI if OneCore is unavailable.
OneCore and SAPI remain separate manual choices.
The available output modes are:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Individual readers and engines are available
only when supported by the installed Prism build and the player's system. If a
selected mode is unavailable, an available fallback is used and the
mod announces the fallback once.
Saved SAPI voice IDs are matched to Prism's voice display names. If installed
voices share a display name, the first matching voice may be selected.
MUTE SPEECH IN BACKGROUND is a saved toggle, Off by default. When enabled,
the mod stops speaking as soon as the game loses window focus. Speech created
while the game is in the background is discarded, and announcements resume
with new activity after focus returns. If speech itself is Off when the game
regains focus, the mod gives the current keyboard and controller recovery
instructions once.

The MOD SETTINGS menu also has SPEAK MENU INDEXES, On by default and saved between sessions.
When enabled, a focused menu item includes its position, such as "PLAY, 1 of 6".
This applies across the main and Settings menus, Controls, play modes, Mod
Settings, game-over choices, leaderboards, achievements, credits, and other
supported screens. The count follows the currently available choices. Changing
a slider or toggle while it remains focused still announces only the new value.

FORMAT SPEECH is a saved Mod Settings toggle, On by default. It changes only
the text sent to speech and braille, leaving the game's visible GUI and guide
unchanged. All-capital menu words use natural sentence case: "PASS IT" becomes
"Pass it" and "ONE ON ONE" becomes "One-on-one". The first word after a full stop
is capitalised again. Mixed-case words and common abbreviations such as SAPI,
NVDA, SFX, and FPS are preserved. In an open guide topic, when Speak Menu Indexes is On,
three dots add a pause before the line number if the text ends without terminal
punctuation. Existing full stops, commas, and other terminal punctuation remain.
Turning this toggle Off disables both adjustments and sends the original text.
Your previous Filter Capitalisation preference carries over.

SPEAK CONTROL TYPES is another saved MOD SETTINGS toggle, On by default. When enabled,
the focused item's type follows its name and precedes its value and index:
"MUSIC slider, 30, 1 of 12", "VIBRATION toggle, On, 6 of 12", or
"PLAY button, 1 of 6". Menus also identify tabs, text fields, and readable
list items where relevant. Value changes continue to speak only the new value.
SLIDER RANGES is a saved toggle, Off by default. When enabled, focused sliders
also report their available endpoints after the current value, such as
"MUSIC slider, 30, range 0 to 100, 1 of 12" when indexing and control types
are enabled. Moving a slider still speaks only the new value.
ONE-ON-ONE FEEDBACK is saved and On by default. Announces the active colour at the start and when it changes. A lost life announces the remaining count. A gained life announces the player’s colour and new life total, such as “Green, 3 lives,” so both players know who won the Bop race. These announcements are disabled when One-on-One Feedback is Off. The game’s three-life limit is unchanged. This feature runs only during One-on-One play.

HINTS TYPE now appears above AUTO-SPEAK BUTTON HINTS in the Mod Settings menu.
AUTO-SPEAK BUTTON HINTS is a saved toggle and is On by default. Turning it
Off suppresses automatic hints, while SPEAK HINTS remains available on demand.
BUTTON HINTS DELAY has None, 5 seconds
(May interrupt speech), 10 seconds, 15 seconds, 30 seconds, and 60 seconds.
It defaults to 10 seconds. With None, the valid inputs for the current screen and
their actions are included in the focused item's ordinary speech string,
after a full stop. The action for the focused control is spoken before general
menu navigation. There is no separate first hint announcement. With a timed
delay, the first hint announcement follows that much inactivity. The 5-second option can
interrupt speech already in progress; longer delays queue behind it. After
the first hint announcement, another delay starts only when the player gives
input, unless repeats are enabled. Moving to another focused item or changing
a focused slider or toggle counts as input and restarts the delay, even if the
game's input binding scan misses the key or controller action. An unused key
that does not change the UI still does not restart it.
HINTS TYPE is a saved slider with Automatic, Keyboard, Controller, and Both.
Automatic is the default and follows the most recently used keyboard or
controller input. Mouse use counts as keyboard. Keyboard and Controller speak
only that device's hints; Both gives both sets of inputs with explicit device
names. Speech-off recovery always includes both devices so the
player can find the control that turns speech back on.

Button hints put the input before its action: "Enter or Space, activate item."
Single-device hints omit the device name. Controller stick names are spoken
in full, such as "Left stick Up and Down". Both mode identifies keyboard
and controller inputs. The mod reads the game's current bindings so native
rebinding changes are reflected in these hints. When no controller is
connected and the game has different face-button names on different controller
types, the hint uses "confirm button" or "back button" rather than assuming an
Xbox layout. Leaderboard score rows use Page Up and Page Down on keyboard.
Controller up/down reads rows only when no leaderboard control has focus;
hints report only the controls available for the selected HINTS TYPE. Ordinary
screen hints also include the current SPEAK HINTS and TOGGLE SPEECH bindings.
Both are looked up centrally, so future global controls can join the same
hint list without changing every screen separately.

REPEAT BUTTON HINTS is a separate saved slider: Off, 2x, 3x, 4x, 5x, or
Infinitely. The default is Infinitely. The number is the total readings in one
cycle: 2x means the first hint and one repeat; 3x means the first hint and
two repeats. Off still allows the first automatic or manual hint.
REPEAT INTERVAL sets the delay between repeats to 15, 30, 45, or
60 seconds and defaults to 30 seconds. With BUTTON HINTS DELAY set to None,
the repeat timer begins immediately after input. Input, a focus or value
change, or a screen change restarts the hint cycle for the current screen.
SPEAK HINTS replaces the
pending automatic hint for that cycle, then uses REPEAT INTERVAL for any
configured repeats. This also works with AUTO-SPEAK BUTTON HINTS Off.
Hints are suppressed during
active gameplay and the beat-timing phases of Audio Calibration, where extra
speech could mask a cue. An existing saved 15-, 30-, or 60-second reminder
delay from version 0.6.2 becomes the new BUTTON HINTS DELAY value.

The MOD SETTINGS menu has four voice adjustment controls: VOICE, VOLUME, RATE,
and PITCH. They adjust the OneCore or SAPI engine actually in use, including
when OUTPUT MODE is Auto. Only controls supported
by the active engine appear; other speech outputs hide them. Each engine keeps
its own voice and adjustment values. Voice lists the system default and the
voices available to that engine. Volume starts at 100%; Rate and Pitch start
at 50. Volume ranges from 5% to 100% so recovery notices remain audible.
Rate and Pitch range from 0 to 100. All three move in steps of five.
The earlier trim-silence experiment remains hidden and inactive.
Mod Settings choices are remembered between sessions. RESTORE MOD DEFAULTS
returns those choices to the defaults described above. Press it once to request
confirmation, then press it again within five seconds to restore them. Moving
to another row or letting five seconds pass cancels the request. This does not
change the game's MUSIC, SFX, or VOICE OVER sliders, LIMIT FPS, or custom
keyboard and controller bindings. Back returns to Settings.
OPEN USER'S GUIDE reads the HTML guide for the language currently selected in
the game's Settings > Language row. The English guide is at
documentation\BopItAccess-user-guide.html; translated guides are in language
subfolders. If a translated copy is missing or unreadable, the English guide
opens instead. Its topic list comes from the document's table of contents and
reloads whenever opened. Confirm opens a topic. Up and Down read its lines. In tables,
Left moves one column left and Right moves one column right; Up and Down keep
the current column when moving between rows. Column headings label cells
instead of appearing as data rows. The table is announced once on entry, and
its end is announced on exit. Back returns to topics or leaves the guide.
While reading, the mod applies the game's menu-music Filter parameter and
restores its previous value on exit. RESET WELCOME SCREEN asks for a
second press within five seconds, then makes the welcome screen appear on the
next game launch. Changing rows or waiting five seconds cancels confirmation.
This update restores the game's native Settings row layout so up/down
navigation remains on Settings rows after MOD SETTINGS is added.
It also starts the MOD SETTINGS submenu on SPEECH OUTPUT each time it opens,
preventing a previously selected BACK row from closing the menu immediately
when Enter is used to reopen it.
The input that opens MOD SETTINGS is now ignored by its rows until that input is
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
Press SPEAK HINTS (H or right stick press by default) on this screen to hear
the current mode and difficulty's native tutorial text before starting. Each
named action in that reference includes its currently assigned keyboard or
controller control, following HINTS TYPE. Reassigned controls are read from
the active player's bindings; One on One names both players' Bop inputs. The
timed tutorial overlay during active play stays silent so it cannot obscure
the game's spoken commands. The hint announces this extra use of SPEAK HINTS.
The READ DESCRIPTIONS control speaks a visual description of the selected
Shapes, Space, City, or Office stage on demand. It is available on this screen
only, before gameplay starts. Its default inputs are G on keyboard and LT
(left trigger) on controller. R was replaced because it is the game's Reset
Gyro shortcut. The screen introduction announces the current binding.
Starting play stops any speech left from song selection so it cannot mask the
game's verbal cues. Descriptions begin with the scene details rather than
repeating the selected stage name.

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
ONE-ON-ONE FEEDBACK is On by default in Settings > Mod Settings for spoken active
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

The mod exposes a native RESET GYRO row and adds a CHANGE SPEECH OUTPUT
shortcut. Reset Gyro sits with the game's controls; the mod-specific rows
remain together at the bottom of the menu, before Reset to Default. CHANGE
SPEECH OUTPUT cycles through the same modes as Settings > Mod Settings > OUTPUT
MODE. The mode order is:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
The shortcut's default inputs are F9 on keyboard and the West face button (X on an Xbox
controller). The controller Start button is reserved by the game's native
Menu action. The current choice is announced when the shortcut is used and
is saved by the same Output Mode setting.

The mod adds nine rows to the game's Controls menu: Group Previous,
Group Next, Date Previous, Date Next, READ DESCRIPTIONS, READ SCORE,
TOGGLE SPEECH, SPEAK HINTS, and CHANGE SPEECH OUTPUT. The first four
address the leaderboard filters
reached with O/P and K/L on the default keyboard layout, or the bumpers and
D-pad left/right on a controller. Focus a row to hear its current binding,
then use the game's normal Bop/confirm action to rebind it. The rows scroll
inside the existing Controls panel. READ DESCRIPTIONS can also be rebound for
keyboard and controller. Its binding is saved by the mod, and Reset to Default
restores G and LT. READ SCORE can also be rebound for keyboard and controller;
Reset to Default restores T and left stick press. The game's original binding
rows and the four leaderboard rows use the same Controls rebinding flow.
TOGGLE SPEECH can be rebound for keyboard and controller. Its bindings are
saved by the mod, and Reset to Default restores F8 and Select. If the binding
is changed while speech is off, the mod announces the new recovery controls.
SPEAK HINTS can also be rebound. Its defaults are H and right stick press.
It speaks the current screen's hint immediately without scheduling a second
automatic first hint. The configured repeats can still follow. It is silent
during gameplay and the timed Audio Calibration cues.
CHANGE SPEECH OUTPUT can be rebound for keyboard and controller; Reset to
Default restores F9 and the West face button. If a new binding is already
assigned to another game or mod action, the Controls menu rejects the
duplicate and keeps the previous assignment. RESET GYRO can be rebound by
the same native Controls procedure as the other game actions.
Returning from song selection to the main menu restores main-menu hints even
when a cached game manager still reports an old gameplay state. Button hint
timers and Automatic hint device selection follow assigned game and mod
controls; unused keys such as an unassigned Control key do not reset them.
This update stops the stale Controls row from announcing "Rebinding failed"
repeatedly after its scene closes. Read Descriptions no longer temporarily
overrides any of the game's own input bindings.

Inside Audio Calibration, the mod reads the Calibrate, Back, and Bop controls,
announces the instructions and calibration stages, reads the warmup countdown,
and announces the displayed latency result. It does not speak every beat during
the timing exercise so the beat remains audible. After your final calibration input, the mod immediately says “Done!”; stop bopping and wait for the measured result. If calibration fails because no input was made, it says “Calibration failed.”

The pause screen announces Paused, the focused Resume or Main Menu button, and its button hints. Focus changes interrupt earlier pause-menu speech; resuming or leaving the round clears any remaining pause speech before play or the main menu continues. During an active round, the mod leaves the game's verbal commands and in-progress score alone.

Install
-------
The source repository contains no compiled mod or Prism DLL. Build the mod by
following README.md, then close the game and copy BopItAccess.dll into its Mods
folder. Obtain the official Windows x64 Prism v0.18.3 prism.dll from
https://github.com/ethindp/prism/releases and place it beside the game executable,
not inside Mods. Copy the build's documentation folder into the game folder,
including its translated language subfolders. Start your screen reader if you
use one, then start Bop It! through Steam. Without a supported running screen
reader, Prism prefers OneCore speech, then SAPI if OneCore is unavailable.
The in-game guide loads the HTML from the
documentation folder whenever it opens. This mod was developed for MelonLoader
0.7.3 Open-Beta and Bop It! (Unity 2022.3.50f1, x64).
The first non-English translations were made with machine translation
and need review by fluent speakers. Please report unclear or incorrect wording.
Gameplay action names use the game's translated terms. Shapes, Space, City,
and Office remain English as fixed stage titles. If OneCore's or SAPI's system voice does
not pronounce your language well, select a suitable installed voice in Mod
Settings.

Try the menus and screens
-------------------------
Wait for the title-screen announcement if it appears, then use Bop to open the
main menu. The game may take several seconds after the mod's ready message to
accept this input.
On a first run, the welcome screen appears before the main menu. Select its
message to hear the introduction again. Choose Open Mod Settings, Read User's
Guide, or Continue to Game. Speak Hints names its current keyboard and
controller assignments in the welcome message regardless of Hints Type.
Open Play and move among the four modes. Choose one to reach song selection.
Twist to cycle through themes and Pull to switch Extreme mode on or off. The
mod announces each change. Press G or LT to hear the currently selected stage
description. Press H or right stick press to hear the mode's tutorial text,
current assigned action controls, and button hints. Bop starts the chosen mode;
Back returns. During a round, use
the game's Menu control to open Pause, then move between Resume and Main Menu.
At the end of a game, listen for the score or One on One winner before the
result-screen controls are announced. Press T or left stick press to repeat the
final result while the game-over screen is visible.
Press F8 or controller Select to turn mod speech off or on from any screen.
Press F9 or controller West (X on an Xbox controller) to cycle the speech
output mode. The same choice is available in Settings > Mod Settings > OUTPUT MODE.
BRAILLE OUTPUT in Settings > Mod Settings is On by default. NVDA's Braille Viewer
can display the braille and its text equivalent without a physical display.
For an ON/OFF comparison, use NVDA's follow-cursors braille mode with Show
Messages enabled; its display-speech-output mode would mirror speech even
when the mod's BRAILLE OUTPUT setting is Off.
In Settings > Mod Settings, use AUTO-SPEAK BUTTON HINTS to enable or disable
automatic instructions. Press H or right stick press to hear the current hint
on demand.
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
panel detection, selected UI objects, and Prism initialization and dispatch.
Successful dispatch does not by itself prove that speech was audible.

To disable the mod, remove Mods\BopItAccess.dll. MelonLoader can remain installed.

Files and third-party notices
-----------------------------
Prism is an open-source accessibility library by Ethan Dupuy and contributors.
It is licensed under the Mozilla Public License, version 2.0. This source
repository does not include prism.dll. Source, releases, and license:
https://github.com/ethindp/prism
See THIRD-PARTY-NOTICES.txt for the current dependency notices.

Editing the settings file
-------------------------
If a language, loud game audio, or a troublesome voice makes the menus difficult to use, you can change the settings outside the game. After startup, the mod automatically creates UserData/BopItAccess.ini inside your Bop It! game folder, using your current settings. This is a plain-text file you can open with an editor such as Notepad.

The file includes the game’s language, music, effects and voice-over volumes, vibration, full screen, resolution and audio latency; mod speech, braille, hint and other preferences; separate OneCore and SAPI voice profiles; and player-facing game and mod input bindings. Available resolutions and installed voices are listed in comments.

Close the game before editing. Find the relevant section and change the value of the existing entry, save the file, then launch the game again. Changes are read on launch, not immediately while the game is running. Changes made through the game’s menus update the file automatically.

Section and setting names stay in English in every language so they remain consistent. On and Off are the recommended toggle values; True/False, Yes/No and 1/0 are also accepted. The comments beside settings explain their choices and ranges. Missing entries and invalid values leave the corresponding saved setting unchanged; other valid edits still apply. Duplicate input assignments are rejected.

Comments and unknown entries are kept. If another program changes the file while the game is running, the mod stops saving to that file for the rest of the session to protect those edits. Close and reopen the game to use them. You can keep a backup before making changes.

For a voice problem, set Voice=System default in the OneCore or SAPI section. OneCore voice choices use name | language; SAPI accepts an installed voice’s displayed name or its full registry ID. The file lists available choices. OutputMode=Auto tries a running supported screen reader, then OneCore, then SAPI.

The example below restores English, quieter game audio and speech with automatic output and system-default voices. Change the matching entries already in your file; this is a reference excerpt, not an extra block to append. Leave your other settings in place.

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

Uninstall in the installer and Windows Installed Apps remove the known mod logs, including Mods/BopItAccess.log.previous, UserData/BopItAccess.ini, the older UserData/BopItAccess.ini.tmp, and validated BopItAccess.ini.<GUID>.tmp remnants in UserData. <GUID> means exactly 32 hexadecimal characters without hyphens; unrelated files are preserved. Native game preferences, unrelated mods and the .NET SDK remain. If cleanup is incomplete, the status log reports it. For an installer-managed copy, the Windows entry, uninstall launcher and cleanup checkpoint remain available for retry until cleanup succeeds. Older manual installations have no durable ownership record, so their warnings can be retried in the open installer.

AI Transparency Note
--------------------

This mod is vibe-coded. All code was completely generated and researched by artificial intelligence, with limited technical human understanding of its underlying architecture. Please use this mod at your own risk.

That being said, every single mod feature and design decision was authored and approved by humans. Testing was never automated; it was carefully and extensively performed by real human players and testers.

Please note: Multilingual text and documentation were generated by AI and have not been reviewed by native speakers. High translation inaccuracy is to be expected. Without agentic coding, this project would not exist. Thank you for giving it a chance!

What may come next
------------------

This project is essentially complete, and no major content or features are planned. However, this mod will be actively maintained and updated over time as needed, with player feedback driving these improvements. Potential future work includes further review and bug fixes, code refinement, and continued improvements to speech responsiveness. Prism creates a possible path to other platforms in the future, but this mod currently only supports Windows x64. The project repository is the place to follow further development.

Thank you
---------

To those who play tested this mod before release and helped get it to where it is now, thank you. Y'all know who you are. To the players who offer feedback, try the mod for the first time, or believe in me and this project, thank you. Your support motivates me to keep making things in a world that can feel crazy and deeply flawed. I hope this project makes it easier for you to enjoy the game and play with others. Thank you all so much. Enjoy Bop It!

— Christopher Shaw

MelonLoader startup windows
---------------------------

The supplied Loader.cfg template hides MelonLoader’s separate start screen and console. The installer applies the same two defaults automatically, before any alpha build preparation that needs to start the game. These settings do not skip the game’s title screen or the mod’s welcome screen.

With the game closed, open UserData/Loader.cfg in the game folder. If it already exists, set disable_start_screen to true in its existing [loader] section and hide_console to true in its existing [console] section. Keep all other entries. If the file does not exist, copy the supplied UserData/Loader.cfg template from the build output, or configuration/Loader.cfg from the source. Never overwrite an existing Loader.cfg with the whole template.

[loader]
disable_start_screen = true

[console]
hide_console = true

The mod does not reset these options on each launch. You may manually change either value back to false if you need the loader’s windows for troubleshooting. Installer uninstall restores the original values only while the installer’s true values are still present, preserving other loader configuration edits.

After a successful change, the mod announces the input and the action it is assigned to, for example, “Space assigned to Bop.”
