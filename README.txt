Bop It Access 0.2.0 - source code

Source\BopItAccessMod.cs contains the MelonLoader mod. It announces readiness
through Tolk, watches the game's selected main-menu button, and sends changed
labels to Tolk on a dedicated speech thread.

Source\BopItAccess.csproj targets .NET 6 and references MelonLoader and generated
IL2CPP assemblies from this installation of Bop It!:
  C:\Program Files (x86)\Steam\steamapps\common\Bop It!

To build on this PC, open PowerShell in this source ZIP's extracted folder and run:
  dotnet build .\Source\BopItAccess.csproj -c Release --configfile .\NuGet.Config

The output DLL is in Source\bin\Release\net6.0. A ready-to-install ZIP is supplied
separately as BopItAccess-v0.2.0-install.zip. The install ZIP also contains the
Tolk runtime binaries and license texts.
