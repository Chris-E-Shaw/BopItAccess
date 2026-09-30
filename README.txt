Bop It Access 0.4.0 - source code

Source\BopItAccessMod.cs initializes Tolk and reads the main menu and Settings.
The AUDIO LATENCY and CONTROLS Settings buttons now speak without their
placeholder zero. Source\BopItAccessMod.Controls.cs reads Controls focus,
bindings, and rebinding feedback. Source\BopItAccessMod.Calibration.cs reads
Audio Calibration focus, stages, warmup countdown, and the displayed result.

Source\BopItAccess.csproj targets .NET 6 and references MelonLoader and generated
IL2CPP assemblies from this installation of Bop It!:
  C:\Program Files (x86)\Steam\steamapps\common\Bop It!

To build on this PC, open PowerShell in this source ZIP's extracted folder and run:
  dotnet build .\Source\BopItAccess.csproj -c Release --configfile .\NuGet.Config

The output DLL is in Source\bin\Release\net6.0. A ready-to-install ZIP is supplied
separately as BopItAccess-v0.4.0-install.zip. The install ZIP also contains the
Tolk runtime binaries and license texts.
