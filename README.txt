Bop It Access 0.5.2 - source code

Source\BopItAccessMod.cs initializes Tolk and reads the main menu and Settings.
The AUDIO LATENCY and CONTROLS Settings buttons now speak without their
placeholder zero. Source\BopItAccessMod.Controls.cs reads Controls focus,
bindings, and rebinding feedback. Source\BopItAccessMod.Calibration.cs reads
Audio Calibration focus, stages, warmup countdown, and the displayed result.
Source\BopItAccessMod.PlayModes.cs reads Play's four mode buttons.
Source\BopItAccessMod.TrackSelect.cs reads the selected song/theme, Extreme
mode state, and start-screen control hints.
Source\BopItAccessMod.GameOver.cs reads the final result screens. The main
source queues score speech before the initially focused menu item.
Source\BopItAccessMod.ResultRank.cs listens for fresh leaderboard results so
high-score and Party-rank announcements do not use values from a prior game.
Source\BopItAccessMod.Leaderboards.cs reads the combined and Party leaderboard
panels, filters, score rows, name input, and result states.
Source\BopItAccessMod.Achievements.cs reads the in-game achievement book pages.
Source\BopItAccessMod.Credits.cs reads generated credit lines and provides
visual-scroll and full-text fallback paths.

Source\BopItAccess.csproj targets .NET 6 and references MelonLoader and generated
IL2CPP assemblies from this installation of Bop It!:
  C:\Program Files (x86)\Steam\steamapps\common\Bop It!

To build on this PC, open PowerShell in this source ZIP's extracted folder and run:
  dotnet build .\Source\BopItAccess.csproj -c Release --configfile .\NuGet.Config

The output DLL is in Source\bin\Release\net6.0. A ready-to-install ZIP is supplied
separately as BopItAccess-v0.5.2-install.zip. The install ZIP also contains the
Tolk runtime binaries and license texts.
