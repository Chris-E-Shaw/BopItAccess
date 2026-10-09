# Bop It Access

Bop It Access ist eine Mod für blinde Spieler der Windows-x64-Steam-Version von **Bop It! The Video Game**. Sie verwendet [MelonLoader](https://github.com/LavaGang/MelonLoader) und [Prism](https://github.com/ethindp/prism), um die Menüs und Bildschirme des Spiels mit Sprach- und Brailleausgabe zugänglich zu machen. Zusätzliche Steuerungsmöglichkeiten und Einstellungen sorgen für ein angenehmeres Spielerlebnis.

Aktuelle Quellcodeversionen: **Mod 0.9.13** und **Installer 0.2.8**. Die erste öffentliche Veröffentlichung steht bevor.

## Funktionen

- Sprachausgabe für Menüs, Einstellungen, Tutorials, Bestenlisten, Erfolge, den Abspann, Pausenbildschirme und Ergebnisse.
- Auf Wunsch vorgelesene Punktzahlen, Bühnenbeschreibungen und Hinweise, die die aktuellen Tastenbelegungen berücksichtigen.
- Sprachausgabe und eine im Spiel verfügbare Anleitung in allen vom Spiel angebotenen Sprachen.
- Ausgabe über Screenreader und Braille, mit OneCore und SAPI als Optionen für die Systemsprachausgabe.
- Einstellbarer Umfang der Sprachausgabe, Wartezeiten und Wiederholungen für Hinweise sowie neu belegbare Sprachbefehle.
- Zusätzliche Belegungen für Spielaktionen, eine Bildratenbegrenzung, Einstellungen für Audio im Hintergrund und eine lesbare Einstellungsdatei.
- Ein zugänglicher Installer mit Tastatur- und Controllerunterstützung für Installation, Updates und Deinstallation.

## Projektstatus

Die wichtigsten Funktionen sind im Wesentlichen fertig. Das Projekt wird bei Bedarf weiter gepflegt; Rückmeldungen von Spielern helfen dabei, Fehler zu beheben und Verbesserungen vorzunehmen. Derzeit wird Windows x64 unterstützt.

Dieses Repository enthält Quellcode und Dokumentation. **Es gibt noch keine öffentliche Veröffentlichung.** Die kommende Veröffentlichung wird `BopItAccess-Installer.exe` und eine kompilierte `BopItAccess-v1.0.zip` anbieten. Die von GitHub automatisch erzeugten Quellcodearchive ZIP und TAR.GZ enthalten Quellcode, keine direkt installierbare Mod. Bis zur ersten Veröffentlichung erstellt die Installeroption **Erweiterte Optionen anzeigen > Alpha installieren** die neueste Version aus dem Quellcode auf `main`.

Die Git-Commits bilden die Projektgeschichte. Die ersten 37 Builds wurden als einzelne Quellcodestände importiert; die Datumsangaben dieser Commits beziehen sich auf den Import, nicht auf die ursprünglichen Builddaten. Kompilierte Binärdateien, Spieldateien und von MelonLoader erzeugte Spielassemblies sind nicht in diesem Repository enthalten.

## Dokumentation

[Die englische Benutzeranleitung lesen](../../BopItAccess-user-guide.html): Sie erklärt Installation, Updates, Deinstallation, Steuerung, Einstellungen, Menüs und alle Spielmodi. Die Anleitung im Spiel verwendet automatisch die aktuelle Spielsprache.

## Voraussetzungen

- Windows x64 und eine eigene, legal erworbene Steam-Installation von Bop It! The Video Game.
- **MelonLoader 0.7.3 Open-Beta**, x64. Bei der Entwicklung wird die Spielversion mit Unity 2022.3.50f1 verwendet.
- Die Windows-x64-**.NET 6-Laufzeit**, um die Mod auszuführen.
- Die offizielle Windows-x64-**Prism v0.18.3**-Datei `prism.dll` neben der ausführbaren Spieldatei.
- Zum Erstellen der Mod aus dem Quellcode: ein kompatibles .NET SDK mit dem **.NET 6-Targeting Pack** und den Referenzen, die MelonLoader aus dem eigenen Spiel erzeugt.
- Zum Erstellen des Installers aus dem Quellcode: das **.NET 10 SDK** unter Windows.

Der Installer lädt seine Abhängigkeiten aus offiziellen Quellen herunter. Für die Installation einer kompilierten Veröffentlichung ist kein Entwicklungs-SDK nötig; für „Alpha installieren“ schon.

<a id="build-from-source"></a>
## Aus dem Quellcode erstellen

1. Installiere MelonLoader 0.7.3 Open-Beta im Spielordner. Starte das Spiel einmal, warte, bis MelonLoader seine Dateien vorbereitet hat, und schließe es dann. Die erzeugten Referenzen sollten sich unter `MelonLoader\Il2CppAssemblies` im Spielordner befinden.
2. Lade dieses Repository herunter oder klone es und öffne PowerShell im Stammordner des Repositorys.
3. Ersetze den Beispielpfad unten durch den Speicherort deines Spiels und führe die folgenden Befehle aus:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

Die kompilierte DLL liegt unter `src\bin\Release\net6.0\BopItAccess.dll`. Das Projekt meldet fehlende Spiel- oder Loaderreferenzen vor dem Kompilieren. Falls das SDK ein fehlendes .NET 6-Targeting Pack meldet, installiere ein SDK, das dieses Paket enthält. Die Datei `NuGet.Config` des Projekts richtet keine Online-Paketquellen ein.

Der Alpha-Installationsweg des Installers bereitet seine lokalen Buildreferenzen vor, ohne das Spiel zu starten. Diese Referenzen sind vorübergehende Eingaben für den Build; sie werden nie in Git aufgenommen oder mit einer kompilierten Mod veröffentlicht.

<a id="install-your-build"></a>
## Deinen Build installieren

Schließe das Spiel und gehe dann wie folgt vor:

1. Kopiere die erstellte `BopItAccess.dll` in den Ordner `Mods` des Spiels. Erstelle diesen Ordner bei Bedarf.
2. Lade die offizielle Windows-x64-Version Prism v0.18.3 von den [Prism-Veröffentlichungen](https://github.com/ethindp/prism/releases) herunter. Lege `prism.dll` neben die ausführbare Spieldatei.
3. Kopiere den Ordner `src\bin\Release\net6.0\documentation` des Builds in den Spielordner und behalte alle Sprachunterordner bei. Wenn du die Prism-Binärdatei weitergibst, füge die zugehörigen Lizenzdateien bei.
4. Starte deinen Screenreader, falls du einen verwendest, und starte dann das Spiel über Steam. Warte auf die Startansage und anschließend auf die Ansage des Titelbildschirms, der Begrüßung oder des Hauptmenüs, bevor du die Spielsteuerung verwendest.

Der Mod-Build lädt Prism weder herunter noch kompiliert er es. Wenn die Sprachausgabe nicht startet, prüfe `Mods\BopItAccess.log` im Spielordner. Eine erfolgreiche Ausgabe im Protokoll bestätigt, dass die Mod Text gesendet hat; sie kann nicht nachweisen, dass die Sprachausgabe gehört wurde.

Bei einer kompilierten Veröffentlichungs-ZIP kopierst du **den gesamten Inhalt** in den Spielordner und führst Ordner zusammen oder ersetzt Dateien, wenn du dazu aufgefordert wirst. Die ZIP enthält die Mod, Prism, Anleitungen und Lizenzhinweise; MelonLoader und .NET werden separat installiert. Die vollständige Anleitung zur manuellen Installation findest du in der Benutzeranleitung.

## Einstellungen außerhalb des Spiels bearbeiten

Nach dem Start enthält `UserData\BopItAccess.ini` im Spielordner lesbare Spiel- und Modeinstellungen, Stimmenprofile und für Spieler vorgesehene Tastenbelegungen. Schließe das Spiel, öffne die Datei im Editor, bearbeite die vorhandenen Einträge und speichere sie. Die Mod liest Änderungen beim nächsten Start ein. Kommentare erklären gültige Werte und Bereiche.

Setze beispielsweise `Language=en` in `[Game]`, um Englisch wiederherzustellen, verringere `MusicVolume`, `SfxVolume` und `VoiceOverVolume` oder setze `Voice=System default` in `[OneCore]` oder `[SAPI]`, um eine ungeeignete Stimme zu ersetzen. Setze `SpeechOutput=On` und `OutputMode=Auto` in `[Mod]`, um automatische Sprachausgabe wiederherzustellen. Lass die anderen Einträge bestehen und füge keine doppelten Abschnitte hinzu.

## Den Installer erstellen

Führe unter Windows mit dem .NET 10 SDK Folgendes aus:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

Das Ergebnis ist `build\installer\BopItAccess.Installer.exe`. Es ist eine eigenständige ausführbare Windows-x64-Datei, für deren Nutzung kein .NET 10 installiert sein muss. Der bereitgestellte Installer ist nicht signiert. Die Benutzeranleitung erklärt seine Bedienelemente und die Sicherheitsabfragen von Windows.

`scripts/package-mod.ps1` erstellt eine Veröffentlichungs-ZIP aus einer bereits kompilierten, passenden Mod. Sie enthält die Anleitungen, Hinweise und Lizenzen, die Spieler benötigen. Entwickler-READMEs, Git-Arbeitsabläufe, erzeugte Spielreferenzen und kompilierte Installerdateien werden nicht in diese ZIP aufgenommen. Das Verpacken kompiliert die Mod nicht und erstellt keine GitHub-Veröffentlichung.

## Transparenzhinweis zur KI

Diese Mod wurde durch „Vibe Coding“ entwickelt. Sämtlicher Code wurde vollständig von künstlicher Intelligenz erzeugt und recherchiert; das technische Verständnis der zugrunde liegenden Architektur auf menschlicher Seite ist begrenzt. Bitte verwende diese Mod auf eigenes Risiko.

Dennoch wurden jede einzelne Modfunktion und jede Designentscheidung von Menschen entworfen und genehmigt. Tests wurden nie automatisiert, sondern sorgfältig und ausführlich von echten menschlichen Spielern und Testern durchgeführt.

Bitte beachte: Die mehrsprachigen Texte und die Dokumentation wurden von KI erzeugt und nicht von Muttersprachlern geprüft. Es ist mit erheblichen Übersetzungsungenauigkeiten zu rechnen. Ohne agentengestützte Programmierung würde dieses Projekt nicht existieren. Danke, dass du ihm eine Chance gibst!

## Lizenz und rechtliche Hinweise

Der eigene Quellcode und die Dokumentation von Bop It Access stehen unter der **[MIT-Lizenz](../../LICENSE)**. Copyright © 2026 Christopher Shaw. Abhängigkeiten behalten ihre eigenen Lizenzen; die MIT-Lizenz lizenziert sie nicht neu und gewährt keine Rechte an Spielinhalten. Hinweise zu Abhängigkeiten findest du in [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).

Bop It Access ist ein inoffizielles Fanprojekt. Es wurde weder von Hasbro, dem Entwickler/Herausgeber Alliance, Valve, Microsoft, Unity, MelonLoader, Prism noch von einem Screenreaderhersteller erstellt, genehmigt oder unterstützt. **Bop It!** und die zugehörigen Figuren, Grafiken, Klänge und Marken gehören Hasbro und den jeweiligen Rechteinhabern. Steam gehört Valve. Andere Produktnamen, Marken und Software bleiben Eigentum ihrer jeweiligen Inhaber.

Du musst eine eigene, legal erworbene Kopie des Spiels besitzen. Dieses Repository enthält weder das Spiel noch dessen Inhalte und gewährt keinerlei Rechte daran. Informationen zu den Rechten am Originalspiel findest du auf der [offiziellen Bop-It!-Spielwebsite](https://bopitthevideogame.com/) und auf der [Steam-Produktseite](https://store.steampowered.com/app/3214360/).

## Danke

An alle, die diese Mod vor der Veröffentlichung getestet und dazu beigetragen haben, sie so weit zu bringen: Danke. Ihr wisst, wer gemeint ist. An die Spieler, die Rückmeldungen geben, die Mod zum ersten Mal ausprobieren oder an mich und dieses Projekt glauben: Danke. Eure Unterstützung motiviert mich, in dieser verrückten Welt, in der wir leben, weiter Dinge zu erschaffen. Ich hoffe, dieses Projekt macht es euch leichter, das Spiel zu genießen und mit anderen zu spielen. Vielen lieben Dank euch allen. Viel Spaß mit Bop It!

— Christopher Shaw
