Bop It Access 0.3.0 - source code

Source\BopItAccessMod.cs contains the MelonLoader mod. It announces readiness
through Tolk, reads the six main-menu buttons, and reads Settings focus and
values. Focused settings announce name plus value; changes within the same row
announce only the changed value. Go Online is included when the game shows it.

Source\BopItAccess.csproj targets .NET 6 and references MelonLoader and generated
IL2CPP assemblies from this installation of Bop It!:
  C:\Program Files (x86)\Steam\steamapps\common\Bop It!

To build on this PC, open PowerShell in this source ZIP's extracted folder and run:
  dotnet build .\Source\BopItAccess.csproj -c Release --configfile .\NuGet.Config

The output DLL is in Source\bin\Release\net6.0. A ready-to-install ZIP is supplied
separately as BopItAccess-v0.3.0-install.zip. The install ZIP also contains the
Tolk runtime binaries and license texts.
