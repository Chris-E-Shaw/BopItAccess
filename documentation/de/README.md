# Bop It Access

Bop It Access ist ein inoffizieller Barrierefreiheits-Mod für die Windows Steam-Version von **Bop It!**. Es verwendet MelonLoader und [Prism](https://github.com/ethindp/prism) um Menüs und Spielbildschirmen Sprach- und Braille-Feedback hinzuzufügen. Zu den aktuellen Funktionen gehören ein Begrüßungsbildschirm für den ersten Start, ein Benutzerhandbuch im Spiel, gesprochene Titel- und Pausenbildschirme, Einstellungen und Steuerungen, Songauswahl, Endergebnisse und Bestenlisten, Erfolge, Credits, Schaltflächenhinweise, On-Demand-Tutorialtext mit aktuellen Steuerungszuweisungen vor einer Runde und Beschreibungen der vier Phasen. Version 0.9.0 verwendet Prism für die Sprach- und Braille-Ausgabe. Der Mod folgt der ausgewählten Sprache des Spiels und enthält eine Anleitung für jede Sprache, die das Spiel anbietet.

## Die Einstellungsdatei bearbeiten

Wenn eine fremde Sprache, lauter Spielton oder eine problematische Stimme die Menüs schwer bedienbar machen, kannst du Einstellungen außerhalb des Spiels ändern. Nach dem Start erstellt der Mod automatisch UserData/BopItAccess.ini im Bop It!-Spielordner aus deinen aktuellen Einstellungen. Diese Textdatei lässt sich etwa mit dem Windows-Editor öffnen.

Die Datei enthält die Sprache, Musik-, Effekt- und Sprachausgabelautstärke, Vibration, Vollbild, Auflösung und Audiolatenz des Spiels; Sprach-, Braille-, Hinweis- und weitere Mod-Einstellungen; getrennte OneCore- und SAPI-Stimmprofile; sowie die für Spieler vorgesehenen Spiel- und Mod-Tastenbelegungen. Verfügbare Auflösungen und installierte Stimmen stehen in Kommentaren.

Schließe das Spiel vor dem Bearbeiten. Suche den passenden Abschnitt, ändere den Wert des vorhandenen Eintrags, speichere die Datei und starte das Spiel erneut. Änderungen werden beim Start eingelesen, nicht sofort während einer laufenden Sitzung. Änderungen über die Spielmenüs aktualisieren die Datei automatisch.

Abschnitts- und Einstellungsnamen bleiben in allen Sprachen Englisch. Für Schalter werden On und Off empfohlen; auch True/False, Yes/No und 1/0 werden akzeptiert. Kommentare erklären die Auswahlmöglichkeiten und Wertebereiche. Fehlende oder ungültige Einträge lassen die jeweilige gespeicherte Einstellung unverändert; andere gültige Änderungen werden trotzdem angewendet. Doppelte Tastenbelegungen werden abgelehnt.

Kommentare und unbekannte Einträge bleiben erhalten. Ändert ein anderes Programm die Datei bei laufendem Spiel, schreibt der Mod für den Rest der Sitzung nicht mehr hinein, um diese Änderungen zu schützen. Schließe und starte das Spiel erneut, um sie zu übernehmen. Du kannst vor Änderungen eine Sicherungskopie anlegen.

Bei Stimmproblemen setze Voice=System default im Abschnitt OneCore oder SAPI. OneCore-Stimmen verwenden Name | Sprache; SAPI akzeptiert den angezeigten Namen einer installierten Stimme oder deren vollständige Registrierungs-ID. Die Datei nennt die verfügbaren Möglichkeiten. OutputMode=Auto versucht einen laufenden unterstützten Screenreader, danach OneCore und zuletzt SAPI.

Das folgende Beispiel stellt Englisch, leiseren Spielton und automatische Sprachausgabe mit den Systemstimmen wieder her. Ändere die entsprechenden Einträge, die bereits in deiner Datei stehen. Dies ist ein Beispielausschnitt und kein zusätzlicher Block zum Anhängen. Behalte deine anderen Einstellungen bei.

```ini
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
```

Installierte Dateien werden in einem Besitzmanifest erfasst, damit Updates vorhandene Dateien bewahren und ein Installationsabbruch eigene Änderungen zurücknehmen kann. **Deinstallieren** und **Installierte Apps** in Windows verwenden denselben Deinstallationscode. Die mitgelieferte `installer/uninstall.ps1` öffnet ein zugängliches Deinstallationsfenster aus Installierte Apps. Nach vollständig erfolgreicher Entfernung werden Deinstallationsstarter, Besitzdatensätze und Windows-Eintrag entfernt. Vorhandene fremde Mods und gemeinsame MelonLoader-Dateien bleiben erhalten. Die Bereinigung verwalteter und älterer manueller Installationen erfasst die bekannten Mod-Protokolle einschließlich `Mods/BopItAccess.log.previous`, `UserData/BopItAccess.ini`, die ältere Datei `UserData/BopItAccess.ini.tmp`, sowie geprüfte `BopItAccess.ini.<GUID>.tmp` in `UserData`Dateireste. Hier muss `<GUID>` aus genau 32 hexadezimalen Zeichen ohne Bindestriche bestehen; beliebige Dateien, die einem weiten Platzhaltermuster entsprechen, werden nicht entfernt.

Bei einer älteren manuell installierten Kopie ohne Besitzmanifest entfernt die Deinstallation identifizierbare Bop It Access-Dateien und lässt gemeinsame Dateien bestehen, deren Herkunft nicht nachgewiesen werden kann. Die Deinstallation entfernt außerdem nur `BopItAccess.*` Einstellungswerte aus jedem lokalen Windows-Benutzerprofil, auch aus abgemeldeten Profilen. Die eigenen Einstellungen des Spiels und das .NET SDK bleiben bestehen. Wenn Windows den Zugriff verweigert oder ein anderer Schritt nicht sicher abgeschlossen werden kann, meldet das Installationsprogramm unvollständige Bereinigung. Bei einer verwalteten Kopie bleiben Windows-Eintrag, Deinstallationsstarter und dauerhafter Wiederaufnahmepunkt bis zum erfolgreichen Abschluss verfügbar, damit verbleibende Schritte erneut versucht werden können. Eine ältere manuelle Installation besitzt keinen solchen dauerhaften Besitznachweis; ihre Warnungen können im geöffneten Installationsprogramm erneut bearbeitet werden.

## Projektstatus

Dieses Projekt ist im Wesentlichen abgeschlossen und es sind keine größeren Inhalte oder Funktionen geplant. Es wird nach Bedarf gewartet, wobei das Feedback der Spieler zu Verbesserungen führt. Das GitHub-Repository enthält Quellcode und technische Dokumentation. **Es gibt noch keine GitHub-Releases.** Die Quelle enthält jetzt auch ein Windows-Installationsprojekt. Bis zur Veröffentlichung einer Version erklärt die Schaltfläche **Installieren**, dass keine Version verfügbar ist; **Alpha installieren** erstellt den neuesten Hauptzweig-Commit aus der Quelle.

Der Commit-Verlauf umfasst rekonstruierte Quell-Snapshots von 37 früheren Builds. Die Commits wurden erstellt, als diese Archive in Git importiert wurden; Ihre Daten sind nicht die ursprünglichen Baudaten. Die [Technische Baugeschichte](BopItAccess-build-history.html) beschreibt die Arbeit hinter jedem Schnappschuss.

## Anforderungen

- Windows x64 und Ihre eigene Installation von Bop It! für Steam.
- MelonLoader im Verzeichnis des Spiels installiert. Die Entwicklung hat MelonLoader **0.7.3 Open-Beta** mit dem x64-Spiel-Build Unity **2022.3.50f1** verwendet. Andere Kombinationen wurden nicht überprüft.
- Ein .NET SDK mit dem **.NET 6 Targeting Pack**, da der Mod auf Ziele zielt `net6.0`.
- Für die Installation ist das offizielle Windows x64 Prism v0.18.3 `prism.dll`. Diese Drittanbieter-Binärdatei befindet sich nicht in diesem Repository.

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
2. Beziehen Sie die offizielle Windows-x64-Version von Prism v0.18.3 (`prism.dll`) von den [Prism-Releases](https://github.com/ethindp/prism/releases) oder erstellen Sie dieselbe Version aus dem Quellcode. Legen Sie `prism.dll` neben die ausführbare Spieldatei im Hauptordner des Spiels, nicht in den Ordner `Mods`.
3. Kopieren Sie den gesamten Build `src\bin\Release\net6.0\documentation\` Ordner in das Spielverzeichnis. Es enthält den englischen Leitfaden im Stammverzeichnis und übersetzte Leitfäden darunter `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, und `pt-BR`. Behalten Sie diese Unterordner und die Begleitdokumente. Der In-Game-Guide liest bei jedem Öffnen den HTML-Code für die aktuelle Spielsprache, sodass beim Ersetzen eines Guides dessen Inhalt aktualisiert wird, ohne dass die DLL neu erstellt werden muss.
4. Starten Sie Ihren Screenreader vor dem Spiel. Ohne laufenden unterstützten Screenreader bevorzugt Prism OneCore und verwendet SAPI, falls OneCore nicht verfügbar ist.

Der Build-Befehl Bop It Access kompiliert nur diesen Mod; Prism wird nicht erstellt oder heruntergeladen. Wenn die Sprachausgabe nicht startet, überprüfen Sie dies `<game directory>\Mods\BopItAccess.log`. Das Protokoll zeichnet die Initialisierung Prism und den Sprachversand auf, obwohl ein erfolgreicher Versand allein nicht beweisen kann, dass Audio gehört wurde.

Beim ersten Start erscheint der Begrüßungsbildschirm, nachdem das Hauptmenü des Spiels fertig ist. Seine Auswahlmöglichkeiten öffnen die Mod-Einstellungen, lesen das Benutzerhandbuch im Spiel oder fahren mit dem Spiel fort. Die Mod-Einstellungen bieten außerdem **Benutzerhandbuch öffnen** und eine bestätigte Aktion **Begrüßungsbildschirm zurücksetzen**, die den Begrüßungsbildschirm beim nächsten Start anzeigt. Verwenden Sie im Leitfaden „Auf/Ab“, um Themen auszuwählen oder Zeilen zu lesen, und „Bestätigen“, um ein Thema zu öffnen. Innerhalb von Tabellen verschiebt „Links“ eine Spalte nach links, „Rechts“ eine Spalte nach rechts und „Auf/Ab“ behält die aktuelle Spalte bei, während die Zeilen gewechselt werden. Spaltenüberschriften beschriften Zellen, anstatt als Datenzeilen zu erscheinen. Der Tisch wird beim Betreten und sein Ende beim Verlassen angekündigt. Zurück hinterlässt ein Thema oder den Leitfaden.

Wählen Sie in der Zeile **Einstellungen > Sprache** des Spiels eine Sprache aus. Mod Speech folgt dieser Auswahl. Der In-Game-Guide verwendet das passende übersetzte HTML-Dokument, mit Englisch als Ersatz, wenn die ausgewählte Kopie fehlt oder nicht lesbar ist. Der gebündelte nicht-englische Text ist ein maschinell übersetzter erster Durchgang; Korrekturen, die fließend sprechen, sind willkommen.

Spielaktionen verwenden die Übersetzungen des Spiels. Shapes, Space, City und Office bleiben feste englische Stufennamen. Für OneCore oder SAPI können Sie in den Mod-Einstellungen eine installierte Stimme für Ihre Spielsprache wählen, wenn die Systemstimme ungeeignet ist. Stimme, Lautstärke, Sprechgeschwindigkeit und Tonhöhe passen die tatsächlich verwendete OneCore- oder SAPI-Ausgabe an, auch im Auto-Modus. Nur unterstützte Regler sind sichtbar; bei anderen Ausgaben werden sie ausgeblendet. Beide Engines speichern ihre Einstellungen getrennt.

Menüpositionen vorlesen. Diese gespeicherte, standardmäßig eingeschaltete Option nennt die Position des ausgewählten Eintrags im Menü. Das Eins-gegen-Eins-Feedback wird gespeichert und ist standardmäßig eingeschaltet. Nennt die aktive Farbe zu Beginn und bei einem Wechsel. Bei einem verlorenen Leben wird die verbleibende Anzahl genannt. Bei einem gewonnenen Leben werden die Spielerfarbe und die neue Anzahl genannt, etwa „Grün, 3 Leben“, damit beide Spieler wissen, wer schneller war. Diese Ansagen entfallen, wenn das Eins-gegen-Eins-Feedback ausgeschaltet ist. Die Grenze von drei Leben bleibt unverändert. Diese Funktion gilt nur während einer Eins-gegen-Eins-Runde. Nach deiner letzten Kalibrierungseingabe sagt der Mod sofort „Fertig!“. Höre auf zu klopfen und warte auf das Messergebnis. Schlägt die Kalibrierung fehl, weil keine Eingabe erfolgte, sagt er „Kalibrierung fehlgeschlagen.“.

**Sprache formatieren**: Macht Texte in Großbuchstaben für Sprache und Braille leichter lesbar. Im Spielratgeber wird vor der Zeilennummer eine Auslassungspause eingefügt, wenn die Zeile ohne Satzzeichen endet. Sichtbare Texte bleiben unverändert. Aus lässt die Ausgabe unverändert.

### MelonLoader Startfenster

Die mitgelieferte Vorlage Loader.cfg verbirgt den separaten Startbildschirm und die Konsole von MelonLoader. Das Installationsprogramm wendet automatisch dieselben beiden Standardeinstellungen an, bevor der Alpha-Build vorbereitet wird, der zum Starten des Spiels erforderlich ist. Diese Einstellungen überspringen weder den Titelbildschirm des Spiels noch den Begrüßungsbildschirm des Mods.

Schließen Sie das Spiel und öffnen Sie `UserData/Loader.cfg` im Spielordner. Wenn die Datei bereits vorhanden ist, setzen Sie `disable_start_screen` im vorhandenen Abschnitt `[loader]` auf `true` und `hide_console` im vorhandenen Abschnitt `[console]` auf `true`. Behalten Sie alle anderen Einträge bei. Wenn die Datei fehlt, kopieren Sie die mitgelieferte Vorlage `UserData/Loader.cfg` aus dem Build oder `configuration/Loader.cfg` aus dem Quellcode. Ersetzen Sie niemals eine vorhandene Loader.cfg durch die gesamte Vorlage.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

Der Mod setzt diese Optionen nicht bei jedem Start zurück. Sie können jeden Wert manuell wieder auf „false“ ändern, wenn Sie die Fenster des Loaders zur Fehlerbehebung benötigen. Durch die Deinstallation des Installationsprogramms werden die ursprünglichen Werte nur dann wiederhergestellt, wenn die tatsächlichen Werte des Installationsprogramms noch vorhanden sind, wodurch andere Änderungen an der Loader-Konfiguration erhalten bleiben.

## Dokumentation

- [Spiel- und Mod-Benutzerhandbuch (Englisch)](BopItAccess-user-guide.html) – eine anfängerfreundliche Anleitung zu Steuerelementen, Einstellungen, Menüs und Spielmodi.
- [Japanisches Benutzerhandbuch (日本語)](../ja/BopItAccess-user-guide.html). Weitere übersetzte Handbücher sind in den Sprachordnern unten verfügbar [`documentation/`](../).
- [Detaillierte Funktions- und Steuerungsanleitung](README.txt). Der Installationsabschnitt beschreibt die lokal vorbereiteten Installations-ZIPs. Dieses GitHub-Repository stellt nur die Quelle bereit.
- [Technische Baugeschichte](BopItAccess-build-history.html).
- [Codeprüfung zur Vorbereitung der Veröffentlichung](BopItAccess-release-review.html) — umgesetzte Feststellungen, geprüfte Dateien, Kompilierungsergebnisse und verbleibende Grenzen.
- [Git-Workflow für dieses Projekt](GIT-WORKFLOW.md).
- [Hinweise Dritter](THIRD-PARTY-NOTICES.txt).

Übersetzte Kopien der oben genannten Dokumente befinden sich in [`documentation/`](../) unter dem jeweiligen unterstützten Sprachcode. Ihre Quelle ist Englisch; `scripts/translate_documents.py` kann die maschinell übersetzten Entwürfe nach Quelländerungen erneut erzeugen.

## Was als nächstes kommt

Dieses Projekt ist im Wesentlichen abgeschlossen und es sind keine größeren Inhalte oder Funktionen geplant. Dieser Mod wird jedoch im Laufe der Zeit bei Bedarf aktiv gepflegt und aktualisiert, wobei das Feedback der Spieler diese Verbesserungen vorantreibt. Zu den möglichen zukünftigen Arbeiten gehören weitere Überprüfungen und Fehlerbehebungen, Codeverfeinerungen und weitere Verbesserungen der Sprachreaktionsfähigkeit. Prism schafft einen möglichen Weg zu anderen Plattformen in der Zukunft, aber dieser Mod unterstützt derzeit nur Windows x64. Das Projekt-Repository ist der Ort, an dem Sie die weitere Entwicklung verfolgen können.

## KI-Transparenzhinweis

Diese Mod wurde mit Vibe Coding erstellt. Der gesamte Code wurde vollständig durch künstliche Intelligenz generiert und erforscht, wobei das technische menschliche Verständnis der zugrunde liegenden Architektur begrenzt war. Bitte verwenden Sie diesen Mod auf eigenes Risiko.

Allerdings wurde jedes einzelne Mod-Feature und jede Designentscheidung von Menschen verfasst und genehmigt. Das Testen wurde nie automatisiert; Es wurde sorgfältig und ausführlich von echten menschlichen Spielern und Testern durchgeführt.

Bitte beachten Sie: Mehrsprachiger Text und Dokumentation wurden von KI generiert und nicht von Muttersprachlern überprüft. Es ist mit einer hohen Übersetzungsungenauigkeit zu rechnen. Ohne Agentencodierung würde dieses Projekt nicht existieren. Vielen Dank, dass Sie ihm eine Chance gegeben haben!

## Danke

Vielen Dank an diejenigen, die diesen Mod vor der Veröffentlichung getestet und dabei geholfen haben, ihn dorthin zu bringen, wo er jetzt ist. Ihr wisst alle, wer ihr seid. An die Spieler, die Feedback geben, den Mod zum ersten Mal ausprobieren oder an mich und dieses Projekt glauben, vielen Dank. Ihre Unterstützung motiviert mich, weiterhin Dinge in einer Welt zu schaffen, die sich verrückt und zutiefst fehlerhaft anfühlen kann. Ich hoffe, dass dieses Projekt es Ihnen leichter macht, das Spiel zu genießen und mit anderen zu spielen. Vielen Dank euch allen. Genießen Sie Bop It!

— Christopher Shaw

## Lizenzierung

Für die Quelle Bop It Access wurde noch keine Lizenz ausgewählt. Prism hat eine eigene Lizenz; siehe die [Hinweise Dritter](THIRD-PARTY-NOTICES.txt). Bop It! und seine Vermögenswerte gehören ihren jeweiligen Eigentümern und sind hier nicht enthalten.
