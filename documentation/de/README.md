# Bop It Access

Bop It Access ist ein inoffizieller Barrierefreiheits-Mod für die Windows Steam-Version von **Bop It!**. Es verwendet MelonLoader und Tolk, um Menüs und Spielbildschirmen Sprach- und Braille-Feedback hinzuzufügen. Zu den aktuellen Funktionen gehören ein Begrüßungsbildschirm für den ersten Start, ein Benutzerhandbuch im Spiel, gesprochene Titel- und Pausenbildschirme, Einstellungen und Steuerungen, Songauswahl, Endergebnisse und Bestenlisten, Erfolge, Credits, Schaltflächenhinweise, On-Demand-Tutorialtext mit aktuellen Steuerungszuweisungen vor einer Runde und Beschreibungen der vier Phasen. Version 0.8.0 folgt der ausgewählten Sprache des Spiels und enthält eine Anleitung für jede Sprache, die das Spiel anbietet.

## Projektstatus

Dieses Projekt befindet sich in der frühen Entwicklungsphase. Dieses Repository enthält Quellcode und technische Dokumentation. **Hier gibt es noch keine kompilierten Builds oder GitHub-Releases.** Um den Mod aus diesem Repository zu verwenden, erstellen Sie ihn aus dem Quellcode und stellen Sie die unten beschriebenen Tolk-Laufzeitdateien bereit.

Der Commit-Verlauf umfasst rekonstruierte Quell-Snapshots von 37 früheren Builds. Die Commits wurden erstellt, als diese Archive in Git importiert wurden; Ihre Daten sind nicht die ursprünglichen Baudaten. Die [Technische Baugeschichte](BopItAccess-build-history.html) beschreibt die Arbeit hinter jedem Schnappschuss.

## Anforderungen

- Windows x64 und Ihre eigene Installation von Bop It! für Steam.
- MelonLoader im Verzeichnis des Spiels installiert. Die Entwicklung hat MelonLoader **0.7.3 Open-Beta** mit dem x64-Spiel-Build Unity **2022.3.50f1** verwendet. Andere Kombinationen wurden nicht überprüft.
- Ein .NET SDK mit dem **.NET 6 Targeting Pack**, da der Mod auf Ziele zielt `net6.0`.
- Zur Installation, kompatibles 64-Bit `Tolk.dll` und `nvdaControllerClient64.dll` Laufzeitdateien. Diese Drittanbieter-Binärdateien befinden sich nicht in diesem Repository.

Der Mod verweist auf DLLs, die von MelonLoader im Spielverzeichnis generiert oder installiert wurden. Es umfasst keine Spielassemblys oder verteilt diese weiter.

<a id="build-from-source"></a>
## Aus dem Quellcode erstellen

1. Installieren Sie MelonLoader, starten Sie Bop It! einmal und schließen Sie dann das Spiel. MelonLoader sollte erstellt werden `MelonLoader\Il2CppAssemblies` unterhalb des Spieleverzeichnisses.
2. Klonen Sie dieses Repository oder laden Sie es herunter. Öffnen Sie PowerShell im Stammverzeichnis des Repositorys.
3. Set `$gameDir` in **Ihr** Bop It! Installationsverzeichnis, dann erstellen Sie:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   Der Beispielpfad ist der übliche Speicherort Windows von Windows. Ändern Sie es, wenn sich Ihre Steam-Bibliothek an einem anderen Ort befindet. Das Projekt prüft vor dem Kompilieren, ob die erforderlichen MelonLoader- und generierten Spiel-DLLs vorhanden sind, und meldet einen fehlenden Pfad.

4. Die erstellte Mod-DLL befindet sich unter `src\bin\Release\net6.0\BopItAccess.dll`.

Wenn das SDK ein fehlendes .NET 6-Targeting-Paket meldet, installieren Sie ein SDK, das dieses Paket enthält. Das Projekt `NuGet.Config` konfiguriert keine Online-Paket-Feeds.

<a id="install-your-build"></a>
## Installieren Sie Ihren Build

1. Schließe das Spiel. Kopieren Sie das Gebaute `BopItAccess.dll` hinein `<game directory>\Mods\`. Erstellen Sie die `Mods` Verzeichnis, wenn MelonLoader es nicht erstellt hat.
2. Besorgen Sie sich eine kompatible 64-Bit-Version `Tolk.dll` von einer vertrauenswürdigen Quelle oder [Erstellen Sie es aus der Upstream-Quelle Tolk](https://github.com/dkager/tolk#compiling). Besorgen Sie sich das Passende `nvdaControllerClient64.dll` von [Tolk x64-Bibliotheksverzeichnis](https://github.com/dkager/tolk/tree/master/libs/x64) oder Ihr Tolk-Build. Legen Sie **beide DLLs im Spielverzeichnis** neben der ausführbaren Datei des Spiels ab und nicht darin `Mods`.
3. Kopieren Sie den gesamten Build `src\bin\Release\net6.0\documentation\` Ordner in das Spielverzeichnis. Es enthält den englischen Leitfaden im Stammverzeichnis und übersetzte Leitfäden darunter `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, und `pt-BR`. Behalten Sie diese Unterordner und die Begleitdokumente. Der In-Game-Guide liest bei jedem Öffnen den HTML-Code für die aktuelle Spielsprache, sodass beim Ersetzen eines Guides dessen Inhalt aktualisiert wird, ohne dass die DLL neu erstellt werden muss.
4. Starten Sie Ihren Bildschirmleser, falls Sie einen verwenden, und starten Sie dann Bop It! bis Steam. Der Mod kann die Sprache SAPI verwenden, wenn kein unterstützter Bildschirmleser ausgeführt wird.

Der Build-Befehl Bop It Access kompiliert nur diesen Mod; Tolk wird nicht erstellt oder heruntergeladen. Wenn die Sprachausgabe nicht startet, überprüfen Sie dies `<game directory>\Mods\BopItAccess.log`. Das Protokoll zeichnet auf, ob Tolk Sprachanfragen initialisiert und akzeptiert hat. Dies allein kann jedoch nicht beweisen, dass Audio gehört wurde.

Beim ersten Start erscheint der Begrüßungsbildschirm, nachdem das Hauptmenü des Spiels fertig ist. Seine Auswahlmöglichkeiten öffnen die Mod-Einstellungen, lesen das Benutzerhandbuch im Spiel oder fahren mit dem Spiel fort. Die Mod-Einstellungen bieten außerdem **Benutzerhandbuch öffnen** und eine bestätigte Aktion **Begrüßungsbildschirm zurücksetzen**, die den Begrüßungsbildschirm beim nächsten Start anzeigt. Verwenden Sie im Leitfaden „Auf/Ab“, um Themen auszuwählen oder Zeilen zu lesen, und „Bestätigen“, um ein Thema zu öffnen. Innerhalb von Tabellen verschiebt „Links“ eine Spalte nach links, „Rechts“ eine Spalte nach rechts und „Auf/Ab“ behält die aktuelle Spalte bei, während die Zeilen gewechselt werden. Spaltenüberschriften beschriften Zellen, anstatt als Datenzeilen zu erscheinen. Der Tisch wird beim Betreten und sein Ende beim Verlassen angekündigt. Zurück hinterlässt ein Thema oder den Leitfaden.

Wählen Sie in der Zeile **Einstellungen > Sprache** des Spiels eine Sprache aus. Mod Speech folgt dieser Auswahl. Der In-Game-Guide verwendet das passende übersetzte HTML-Dokument, mit Englisch als Ersatz, wenn die ausgewählte Kopie fehlt oder nicht lesbar ist. Der gebündelte nicht-englische Text ist ein maschinell übersetzter erster Durchgang; Korrekturen, die fließend sprechen, sind willkommen.

Der Mod verwendet die übersetzten Namen des Spiels für Gameplay-Aktionen. Shapes, Space, City und Office bleiben in Englisch als feste Bühne Titel. Die ausgewählte Sprachausgabe benötigt eine Stimme für Ihre Sprache. Wählen Sie für die Ausgabe SAPI eine installierte Stimme aus, die für Ihre Sprache geeignet ist, wenn die Standardsprache des Systems falsch klingt.

## Dokumentation

- [Spiel- und Mod-Benutzerhandbuch](BopItAccess-user-guide.html) – eine anfängerfreundliche Anleitung zu Steuerelementen, Einstellungen, Menüs und Spielmodi.
- [Detaillierte Funktions- und Steuerungsanleitung](README.txt). Der Installationsabschnitt beschreibt die lokal vorbereiteten Installations-ZIPs. Dieses GitHub-Repository stellt nur die Quelle bereit.
- [Technische Baugeschichte](BopItAccess-build-history.html).
- [Git-Workflow für dieses Projekt](GIT-WORKFLOW.md).
- [Hinweise Dritter](THIRD-PARTY-NOTICES.txt).

Übersetzte Kopien aller sechs oben genannten Dokumente liegen vor [`documentation/`](../) unter jedem unterstützten Sprachcode. Die Quelle dafür ist Englisch; `scripts/translate_documents.py` kann die maschinell übersetzten Entwürfe nach Quelländerungen neu generieren.

## KI-Transparenz

Christopher Shaw leitet dieses Projekt und bewertet seine Zugänglichkeit im Spiel. OpenAI Codex Modelle haben bei Recherche, Code und Dokumentation geholfen. Zu den veröffentlichten Commit-Nachrichten gehören a `Co-authored-by` Trailer, der das Modell identifiziert, das zu jeder Änderung beigetragen hat; Die historischen Credits wurden mit den Sitzungsaufzeichnungen dieses Projekts verglichen. Der frühere Build-Verlauf wurde aus gespeicherten Quellarchiven rekonstruiert und nicht als Commits zu diesem Zeitpunkt aufgezeichnet. KI-gestützte Beiträge können Fehler enthalten und sollten vor der Verwendung überprüft werden.

## Lizenzierung

Für die Quelle Bop It Access wurde noch keine Lizenz ausgewählt. Tolk und der Controller-Client NVDA verfügen über eigene Lizenzen; siehe die [Hinweise Dritter](THIRD-PARTY-NOTICES.txt). Bop It! und seine Vermögenswerte gehören ihren jeweiligen Eigentümern und sind hier nicht enthalten.
