# Bop It Access

Aktuelle Quell-Builds: Mod 0.9.12 (59 Mod-Builds), Installer 0.2.6. Die erste öffentliche Veröffentlichung ist weiterhin geplant.

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

Wählen Sie nach der Bestätigung von Uninstall entweder Uninstall for me oder Uninstall for everyone. Beide Optionen entfernen die gemeinsam genutzten Moddateien aus diesem Spielordner. Die Mod steht somit niemandem mehr zur Verfügung, der diese Installation verwendet. Die Auswahl legt fest, wessen gespeicherte Windows-Einstellungen der Mod entfernt werden: nur die des anfordernden Kontos oder die aller lokalen Windows-Profile, einschließlich abgemeldeter Profile. Die Einstellungen des ursprünglichen Spiels bleiben erhalten. Das .NET-SDK bleibt installiert.

Wenn das Installationsprogramm seine eigene MelonLoader-Installation entfernt und keine anderen Mods sie benötigen, entfernt es auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg sowie die leeren Ordner Plugins, UserLibs und UserData. Einstellungen von Bop It Access, bekannte Protokolle, Anleitungen und Installationsdateien werden entfernt. Andere Mods, bereits vorhandene gemeinsam genutzte Loaderdateien und unbekannte Dateien sind geschützt. Daher kann eine unbekannte Datei dazu führen, dass ein Ordner zurückbleibt; das Installationsprogramm meldet dies in der Diagnose, statt fremde Daten zu löschen.

Die Seite Installierte Apps in Windows verwendet dieselbe Bestätigung, Auswahl der Einstellungen und Bereinigung. Das Installationsprogramm stellt BopItAccess-uninstall.ps1 im Spielordner als Verknüpfung zum installierten Deinstallationsprogramm bereit; künftige Builds aus dem Quellcode enthalten dieses Skript ebenfalls in ihrer Ausgabe. Ein manuell kopiertes Skript installiert das Deinstallationsprogramm selbst nicht. Bei einer älteren manuellen Installation ohne Eigentumsnachweis für die Dateien entfernt das Installationsprogramm eindeutig erkennbare Moddateien und behält gemeinsam genutzte Dateien, deren Herkunft sich nicht feststellen lässt.

Wenn die Bereinigung nicht sicher abgeschlossen werden kann, erklärt das Installationsprogramm dies und behält die für einen erneuten Versuch benötigten Informationen. Bei einer verwalteten Installation bleiben der Windows-Deinstallationseintrag und der Wiederaufnahmepunkt der Bereinigung erhalten, bis die Entfernung erfolgreich ist. Eine ältere manuelle Kopie hat keinen dauerhaften Eigentumsnachweis für die Dateien; versuchen Sie im geöffneten Installationsprogramm erneut, die in den Warnungen genannten Vorgänge auszuführen. Installieren, aktualisieren oder entfernen Sie die Mod nicht, während Bop It! läuft.

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

Die mitgelieferte Vorlage Loader.cfg verbirgt den separaten Startbildschirm und die Konsole von MelonLoader. Der Installer setzt dieselben beiden Standardwerte, bevor Sie das Spiel selbst starten. Der Titelbildschirm des Spiels und der Begrüßungsbildschirm der Mod werden dadurch nicht übersprungen.

Schließen Sie das Spiel und öffnen Sie `UserData/Loader.cfg` im Spielordner. Wenn die Datei bereits vorhanden ist, setzen Sie `disable_start_screen` im vorhandenen Abschnitt `[loader]` auf `true` und `hide_console` im vorhandenen Abschnitt `[console]` auf `true`. Behalten Sie alle anderen Einträge bei. Wenn die Datei fehlt, kopieren Sie die mitgelieferte Vorlage `UserData/Loader.cfg` aus dem Build oder `configuration/Loader.cfg` aus dem Quellcode. Ersetzen Sie niemals eine vorhandene Loader.cfg durch die gesamte Vorlage.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

Die Mod setzt diese Optionen nicht bei jedem Start zurück. Zur Fehlersuche können Sie einen der beiden Werte manuell wieder auf false setzen. Wenn die Deinstallation eine gemeinsam genutzte MelonLoader-Installation beibehält, stellt sie nur die seit ihrer Einrichtung unveränderten Ziel-Flags des Installationsprogramms wieder her und bewahrt andere Änderungen. Wenn sie ihre eigene ungenutzte MelonLoader-Installation entfernt, entfernt sie auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg.

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


## Den richtigen Download wählen

Die erste öffentliche GitHub-Veröffentlichung soll die vier folgenden Downloads enthalten. Sie sind geplant und noch nicht verfügbar; eine öffentliche Veröffentlichung oder ein Tag wurde noch nicht publiziert. Verwenden Sie bis dahin einen bereitgestellten Installer oder den Quellcodeweg. Ein Quellarchiv ist nicht das kompilierte Installations-ZIP.

[GitHub Releases](https://github.com/Chris-E-Shaw/BopItAccess/releases)

- BopItAccess-Installer.exe: Das eigenständige Windows-x64-Installationsprogramm. Es findet das Spiel und verwaltet Abhängigkeiten, Installation, Updates, Diagnosen und Entfernung. Es ist nicht signiert.
- BopItAccess-v1.0.zip: Das kompilierte Mod-Paket für die manuelle Installation ohne Bop-It-Access-EXE. Enthält Mods/BopItAccess.dll, prism.dll, alle Dokumente und Prism-Lizenzen, eine Loader.cfg-Vorlage, README.txt und die Deinstallationsverknüpfung. MelonLoader, .NET, Spieldateien und generierte Spielassemblies sind nicht enthalten.
- Source code (zip): Das von GitHub automatisch erzeugte ZIP des Release-Quellcodes. Zum Lesen oder Kompilieren gedacht; kein kompiliertes Mod-Paket.
- Source code (tar.gz): Derselbe Quellcode als gzip-komprimiertes tar-Archiv. Ein alternatives Quellformat, kein weiteres Mod-Installationsprogramm.

### Nicht signierter Installer und Windows-11-Sicherheitsabfragen

Dieser Installer ist nicht signiert. Ein nicht signiertes oder unbekanntes Programm kann SmartScreen- oder Antiviruswarnungen auslösen, auch mögliche Fehlalarme; nicht jede Erkennung ist deshalb falsch. Beziehen Sie die Datei nur vom offiziellen Bop-It-Access-Projekt oder einer vertrauenswürdigen direkten Quelle und entscheiden Sie, ob Sie ihr vertrauen. Der kompilierte ZIP-Weg vermeidet diese Installer-EXE. Schalten Sie den Virenschutz nicht ab und schließen Sie kein ganzes Laufwerk oder Spielverzeichnis aus.
[Microsoft: unsigned apps and SmartScreen](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app)

### Eine bestimmte Defender-Erkennung zulassen

Drücken Sie Win+I und wählen Sie Datenschutz und Sicherheit > Windows-Sicherheit > Windows-Sicherheit öffnen > Viren- & Bedrohungsschutz > Schutzverlauf (teils auch Bedrohungsverlauf genannt). Öffnen Sie den Eintrag dieses Installers. Gehen Sie mit Tab zu Aktionen oder Weitere Aktionen, drücken Sie Enter und wählen Sie Auf Gerät zulassen oder Zulassen; bestätigen Sie gegebenenfalls die Administratorabfrage. Ein Quarantäneeintrag kann zuerst Wiederherstellen erfordern und bei erneuter Erkennung anschließend Zulassen. Wurde die Datei entfernt, laden Sie eine neue Kopie vom offiziellen Projekt. Prüfen Sie vor dem Zulassen den genauen Eintrag.
[Microsoft: Protection history](https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app) · [Microsoft Defender FAQ](https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq)

### Optionale, eng begrenzte Defender-Ausschlüsse

Wählen Sie unter Viren- & Bedrohungsschutz Einstellungen verwalten im Bereich der Schutzeinstellungen, dann Ausschlüsse > Ausschlüsse hinzufügen oder entfernen. Bestätigen Sie bei Bedarf die Administratorabfrage mit Ja. Wählen Sie Ausschluss hinzufügen > Prozess, geben Sie genau BopItAccess-Installer.exe ein und drücken Sie Enter. Der Prozessname muss der tatsächlich gestarteten Datei entsprechen.

Microsofts Prozessausschluss betrifft Dateien, die dieser Prozess öffnet; er schließt nicht die Installer-EXE selbst aus, stellt keine Quarantänedatei wieder her und umgeht SmartScreen nicht. Erkennt Defender die EXE selbst und vertrauen Sie ihr, ist ein optionaler Dateiausschluss für genau diese heruntergeladene EXE die passende engere Alternative. Entfernen Sie unnötige Ausnahmen später.
[Microsoft: exclusions overview](https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview)

SmartScreen ist eine separate Warnung. Vertrauen Sie genau dieser EXE und bietet Windows es an, wählen Sie Weitere Informationen > Trotzdem ausführen. Defender-Zulassung oder Prozessausschluss umgeht diese Abfrage nicht; eine Richtlinie kann die Ausführung verhindern.

Schritte geprüft am 9. Oktober 2026 für Windows 11 25H2, Build 26200.9550. Beschriftungen können abweichen; kein UI-Durchlauf wurde durchgeführt.

## Windows-Installationsprogramm 0.2.6

### Mit dem Installer installieren, aktualisieren oder entfernen

1. Schließen Sie das Spiel, starten Sie BopItAccess-Installer.exe und bestätigen Sie die Windows-Administratorabfrage. Lesen Sie das Welcome and controls-Textfeld und prüfen Sie dann den erkannten Game folder oder verwenden Sie Browse. Welcome and controls ist schreibgeschützt, auswählbar und erhält zuerst den Fokus; Alt+W führt dorthin zurück.
2. Wählen Sie Install für die neueste öffentliche kompilierte Version, sobald sie verfügbar ist. Bis zur ersten Veröffentlichung zeigt Show advanced die Aktion Install alpha, die nach Bestätigung den neuesten Quellcode auf Ihrem Computer kompiliert. Warten Sie auf die Erfolgsmeldung. Play Bop It! The Video Game startet danach nur auf Ihren Wunsch über Steam.
3. Öffnen Sie bei einer vorhandenen Installation den Installer mit geschlossenem Spiel und prüfen Sie den Status. Update erscheint, wenn eine neuere öffentliche Version gefunden wird. Wählen Sie Update und warten Sie bis zum Abschluss; gespeicherte Einstellungen bleiben erhalten. Install alpha ist die separate Wahl für den neuesten Quellcode, nicht das Update einer öffentlichen Version.
4. Wählen Sie zum Entfernen Uninstall, bestätigen Sie und wählen Sie dann Uninstall for me oder Uninstall for everyone. Beide entfernen die gemeinsam verwendeten Mod-Dateien aus diesem Spielordner. Die Wahl betrifft gespeicherte Windows-Mod-Voreinstellungen Ihres Kontos oder aller lokalen Profile; originale Spieleinstellungen bleiben erhalten. Windows Installierte Apps verwendet denselben Ablauf. Prüfen Sie das Ergebnis vor Quit.

Die folgende Tabelle nennt jede Aktion und jedes Textfeld im Hauptfenster. Installer status und Installation progress sind Informationen, keine Schaltflächen. Welcome and controls enthält die wiederverwendbaren Anweisungen; Status log die wechselnden Vorgangsmeldungen. Abort fragt vor dem Rückgängigmachen einer laufenden Installation; Quit verwendet dieselben sicheren Abbruchregeln. Dialoge bieten Keep open/Quit, Bestätigung/Abbrechen und die beiden Deinstallationsbereiche. Show advanced ändert nur, welche Aktionen sichtbar sind.

Verwenden Sie die bereitgestellte BopItAccess-Installer-0.2.6.exe oder BopItAccess-Installer.exe. Beide Namen enthalten dasselbe eigenständige Windows-x64-Installationsprogramm. Der Quellcode ist im Projekt enthalten; eine öffentliche kompilierte Datei oder ein GitHub Release wurde noch nicht veröffentlicht.

Schließen Sie Bop It!, öffnen Sie das Installationsprogramm und bestätigen Sie die Windows-Abfrage für Administratorrechte. Das Installationsprogramm begrüßt Sie, sucht in den Steam-Bibliotheken auf allen verfügbaren Laufwerken nach dem Spiel und versucht, sein Fenster in den Vordergrund zu bringen. Prüfen Sie den angezeigten Spielordner; verwenden Sie Browse, wenn Sie einen anderen Ordner auswählen müssen. Mit Tab wechseln Sie zwischen den Bedienelementen. Das Statusprotokoll ist ein schreibgeschütztes Textfeld: Setzen Sie den Fokus darauf, um Meldungen mit den Cursortasten zu lesen, Text auszuwählen oder ihn zu kopieren.

Das Installationsprogramm 0.2.6 fordert beim Start kurz Vordergrundaktivierung und Tastaturfokus an. Ist am Ende der begrenzten Startbeobachtung noch ein anderes Fenster aktiv, blinken sein Fenstertitel und seine Taskleistenschaltfläche, und es fordert zum Wechsel mit Alt+Tab auf. Aktivieren Sie das Installationsprogramm, bevor Sie seine Tastatur- oder Controller-Steuerung verwenden. Alt+G setzt den Fokus auf das Spielordnerfeld.

Show advanced ist beim Öffnen des Installationsprogramms nicht aktiviert. Damit werden Install alpha, Save diagnostics und Copy diagnostics eingeblendet. Install lädt die neueste öffentliche GitHub-Version herunter, sofern eine vorhanden ist. Es gibt noch keine öffentliche Version, daher benötigen Tester derzeit Show advanced und Install alpha. Die Alpha-Installation bittet um Bestätigung, lädt den neuesten Quellcode herunter und kompiliert ihn auf Ihrem Computer. Update erscheint, wenn für eine installierte Kopie eine neuere öffentliche Version gefunden wird.

Die Statusmeldungen erklären in verständlicher Sprache, was heruntergeladen, installiert oder abgeschlossen wird. Eine einzige Fortschrittsanzeige zeigt den geschätzten Fortschritt der gesamten Installation, ohne für jeden Download oder jede Datei zurückgesetzt zu werden. Sie steigt in Schritten von fünf Prozentpunkten; manche Vorbereitungsschritte können Zeit benötigen, ohne dass eine sichtbare Änderung erfolgt. Die Begrüßung, ein neu verfügbares Update und die Bestätigung, dass Diagnosedaten kopiert wurden, werden über die Windows-Benachrichtigungen zur Barrierefreiheit an Ihren Screenreader gesendet. Ob sie vorgelesen werden, hängt von Ihrem Screenreader und seiner Unterstützung für Windows-Benachrichtigungen ab.

Das Installationsprogramm 0.2.6 startet Bop It! während der Installation niemals. Die Alpha-Installation verwendet passende lokale Builddateien erneut oder bereitet temporäre Dateien aus Ihrem eigenen installierten Spiel vor, während dieses geschlossen bleibt. Anschließend legt das Installationsprogramm MelonLoader im Spielordner ab und fügt sofort Mods/BopItAccess.dll hinzu, gefolgt von Prism, Einstellungen, der vollständigen Dokumentation und der Unterstützung für die Deinstallation. Warten Sie auf die Erfolgsmeldung und starten Sie das Spiel anschließend selbst über Steam, wenn Sie bereit sind.

Nach einer erfolgreichen Installation erscheint Play Bop It! The Video Game. Aktivieren Sie diese Schaltfläche, um das Spiel selbst über Steam zu starten, wenn Sie bereit sind. Das Installationsprogramm startet das Spiel während der Installation niemals automatisch.

Eine kompilierte Version benötigt die .NET-6-Laufzeitumgebung für Windows x64, kein Entwicklungs-SDK. Vorhandene vollständige Laufzeitumgebungen werden erneut verwendet. Eine fehlende Laufzeitumgebung wird von Microsoft heruntergeladen und unter MelonLoader/Dependencies/dotnet abgelegt. Install alpha benötigt außerdem ein kompatibles .NET-SDK und das .NET-6-Targeting-Pack: Ein vorhandenes SDK wird erneut verwendet oder das offizielle SDK von Microsoft systemweit installiert. Das Installationsprogramm erstellt keinen neuen SDK-Ordner im Stammordner des Spiels. MelonLoader 0.7.3 Open-Beta und Prism 0.18.3 stammen aus ihren offiziellen Veröffentlichungen. Gemeinsam genutzte Microsoft-.NET-Komponenten und SDKs bleiben nach einem Abbruch oder einer Deinstallation installiert.

Quit schließt das Installationsprogramm. Wenn die Installation noch läuft, fragt es, ob diese vor dem Schließen abgebrochen und rückgängig gemacht werden soll; Keep open setzt den Vorgang normal fort. Wenn die Installation abgeschlossen wird, während Sie sich entscheiden, zeigt der Dialog diesen Abschluss an, und Quit macht die abgeschlossene Installation nicht rückgängig. Sobald die Entfernung begonnen hat, wird die Deinstallation vor dem Beenden sicher abgeschlossen. Auch Abort bittet um Bestätigung und macht die Änderungen dieses Versuchs an den Spieldateien rückgängig. Bei einem Abbruch während der Einrichtung von Microsoft .NET wird gewartet, bis die Installation dieser gemeinsam genutzten Komponenten sicher abgeschlossen ist.

Wählen Sie nach der Bestätigung von Uninstall entweder Uninstall for me oder Uninstall for everyone. Beide Optionen entfernen die gemeinsam genutzten Moddateien aus diesem Spielordner. Die Mod steht somit niemandem mehr zur Verfügung, der diese Installation verwendet. Die Auswahl legt fest, wessen gespeicherte Windows-Einstellungen der Mod entfernt werden: nur die des anfordernden Kontos oder die aller lokalen Windows-Profile, einschließlich abgemeldeter Profile. Die Einstellungen des ursprünglichen Spiels bleiben erhalten. Das .NET-SDK bleibt installiert.

Wenn das Installationsprogramm seine eigene MelonLoader-Installation entfernt und keine anderen Mods sie benötigen, entfernt es auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg sowie die leeren Ordner Plugins, UserLibs und UserData. Einstellungen von Bop It Access, bekannte Protokolle, Anleitungen und Installationsdateien werden entfernt. Andere Mods, bereits vorhandene gemeinsam genutzte Loaderdateien und unbekannte Dateien sind geschützt. Daher kann eine unbekannte Datei dazu führen, dass ein Ordner zurückbleibt; das Installationsprogramm meldet dies in der Diagnose, statt fremde Daten zu löschen.

Das Installationsprogramm bleibt nach der Deinstallation geöffnet, damit Sie das Ergebnis prüfen, Diagnosedaten speichern oder erneut installieren können. Wählen Sie Quit, wenn Sie fertig sind. Das laufende Hilfsprogramm zur Deinstallation und die automatischen Diagnosedateien werden nach dem Schließen des Fensters entfernt. Eine erneute Installation im selben Fenster beginnt mit einem neuen Installationsdatensatz; die verzögerte Bereinigung kann die neue Installation nicht entfernen.

Die Seite Installierte Apps in Windows verwendet dieselbe Bestätigung, Auswahl der Einstellungen und Bereinigung. Das Installationsprogramm stellt BopItAccess-uninstall.ps1 im Spielordner als Verknüpfung zum installierten Deinstallationsprogramm bereit; künftige Builds aus dem Quellcode enthalten dieses Skript ebenfalls in ihrer Ausgabe. Ein manuell kopiertes Skript installiert das Deinstallationsprogramm selbst nicht. Bei einer älteren manuellen Installation ohne Eigentumsnachweis für die Dateien entfernt das Installationsprogramm eindeutig erkennbare Moddateien und behält gemeinsam genutzte Dateien, deren Herkunft sich nicht feststellen lässt.

Wenn die Bereinigung nicht sicher abgeschlossen werden kann, erklärt das Installationsprogramm dies und behält die für einen erneuten Versuch benötigten Informationen. Bei einer verwalteten Installation bleiben der Windows-Deinstallationseintrag und der Wiederaufnahmepunkt der Bereinigung erhalten, bis die Entfernung erfolgreich ist. Eine ältere manuelle Kopie hat keinen dauerhaften Eigentumsnachweis für die Dateien; versuchen Sie im geöffneten Installationsprogramm erneut, die in den Warnungen genannten Vorgänge auszuführen. Installieren, aktualisieren oder entfernen Sie die Mod nicht, während Bop It! läuft.

### Tastenkombinationen des Installationsprogramms

| Aktion | Tastenkombination | Funktion |
| --- | --- | --- |
| Welcome and controls | Alt+W | Das separate Welcome and controls-Textfeld nennt die Controller-Textbefehle; Alt+W führt dorthin zurück und Alt+L zum wechselnden Status log. Beide Felder sind schreibgeschützt, auswählbar und überprüfbar. Show advanced meldet aktiviert oder deaktiviert. Alles auswählen bestätigt Erfolg oder ein leeres Feld. Strg+A wählt mit der Tastatur den gesamten Text aus; Strg+C kopiert die Auswahl. |
| Spielordner | Alt+G | Den Fokus auf das Spielordnerfeld setzen. |
| Browse | Alt+B | Den Spielordner auswählen. |
| Install | Alt+I | Die neueste öffentliche Version installieren, sofern verfügbar. |
| Install alpha | Alt+A | Den neuesten Quellcode bestätigen und kompilieren; mit Show advanced sichtbar. |
| Update | Alt+U | Eine neuere öffentliche Version installieren, wenn sie angeboten wird. |
| Play Bop It! The Video Game | Alt+P | Das Spiel über Steam starten; nach einer erfolgreichen Installation verfügbar. |
| Uninstall | Alt+N | Die Entfernung bestätigen und auswählen, wessen Windows-Einstellungen der Mod entfernt werden sollen. |
| Abort | Alt+R | Den Abbruch der aktuellen Installation bestätigen. |
| Statusprotokoll | Alt+L | Den Fokus auf die schreibgeschützten Statusmeldungen mit auswählbarem Text setzen. |
| Show advanced | Alt+V | Die Alpha-Installation und Diagnosewerkzeuge ein- oder ausblenden. |
| Save diagnostics | Alt+D | Die vollständige Diagnosesitzung speichern und weiter aufzeichnen; mit Show advanced sichtbar. |
| Copy diagnostics | Alt+C | Die vollständige Diagnosemomentaufnahme kopieren; mit Show advanced sichtbar. |
| Quit | Alt+Q | Schließen und einen Abbruch sicher behandeln, falls ein Vorgang läuft. |

### Einen Controller im Installationsprogramm verwenden

Das Installationsprogramm unterstützt Controller im Xbox-Stil und andere Controller, die Windows über XInput bereitstellt. Seine Steuerung ist unabhängig von der frei belegbaren Steuerung des Spiels. Mit dem Steuerkreuz oder dem linken Stick wechseln Sie zwischen den Bedienelementen; wenn ein Textfeld den Fokus hat, dienen die Richtungen stattdessen zum Lesen seines Texts. Die Schultertasten wechseln immer zum vorherigen oder nächsten Bedienelement, das den Fokus erhalten kann. A aktiviert die Schaltfläche oder das Kontrollkästchen mit dem Fokus. Controller-Eingaben werden nur verarbeitet, solange dieses Installationsprogramm oder einer seiner eigenen Dialoge im Vordergrund ist.

B geht zurück oder bricht einen Dialog ab; im Hauptfenster des Installationsprogramms fordert es den Abbruch einer laufenden Installation an, andernfalls entspricht es Quit. Start entspricht im Hauptfenster Quit und geht in einem Dialog zurück. Y (die obere Aktionstaste) wählt den gesamten Text aus, wenn ein Textfeld des Installationsprogramms den Fokus hat. Außerhalb der Textfelder im Hauptfenster schaltet Y Show advanced um. Im Statusprotokoll oder einem anderen Textfeld des Installationsprogramms wirken Steuerkreuz und linker Stick wie die Pfeiltasten: Links/Rechts bewegt sich um ein Zeichen, Auf/Ab um eine Zeile. Halten Sie LT wie Strg gedrückt: Links/Rechts bewegt sich um ein Wort, Auf/Ab um einen Absatz. Halten Sie RT wie Umschalt gedrückt, um die Auswahl zu erweitern; LT und RT zusammen wählen Wörter oder Absätze aus. X kopiert nur den ausgewählten Text; wählen Sie zuerst den gewünschten Teil aus. Strg+C auf der Tastatur kopiert die Auswahl weiterhin. Wenn kein Text ausgewählt ist, sendet das Installationsprogramm außerdem barrierefreie Benachrichtigungen zum Zeichen, Wort, zur Zeile oder zum Absatz an der Textmarke. Das Installationsprogramm sendet eine barrierefreie Bestätigung, wenn Text kopiert wurde, und meldet eine leere Auswahl oder einen Kopierfehler. Ob dies vorgelesen wird, hängt von der Unterstützung Ihres Screenreaders für Windows-Benachrichtigungen ab. Die Controller-Navigation in den nativen Windows-Dialogen zur Ordnerauswahl und zum Speichern muss noch von Menschen geprüft werden. Zur Eingabe eines Ordners oder Dateinamens steht weiterhin eine Tastatur zur Verfügung. Controller ohne XInput-Unterstützung werden von dieser Implementierung nicht abgedeckt.

Das separate Welcome and controls-Textfeld nennt die Controller-Textbefehle; Alt+W führt dorthin zurück und Alt+L zum wechselnden Status log. Beide Felder sind schreibgeschützt, auswählbar und überprüfbar. Show advanced meldet aktiviert oder deaktiviert. Alles auswählen bestätigt Erfolg oder ein leeres Feld. Strg+A wählt mit der Tastatur den gesamten Text aus; Strg+C kopiert die Auswahl.

Das Installationsprogramm 0.2.6 fordert für jede ausgegebene Sprachnachricht die Ersetzung früherer Installer-Sprachausgabe an, einschließlich Textprüfung, Alles auswählen, aktiviertem/deaktiviertem Show advanced, Copy diagnostics und weiteren Bestätigungen. LB/RB sagt weiterhin das neue Bedienelement mit Fokus an. Die Häufigkeit der Statusansagen bleibt gleich; nicht jeder Protokolleintrag wird automatisch gesprochen. Die tatsächliche Unterbrechung hängt von der Windows-Benachrichtigungsunterstützung des Screenreaders ab und erfordert noch menschliche Prüfung.

### Diagnose des Installationsprogramms

Show advanced blendet Save diagnostics (Alt+D) und Copy diagnostics (Alt+C) ein. Automatische UTF-8-Protokolle werden lokal unter %ProgramData%\BopItAccess\diagnostics gespeichert. Save diagnostics schreibt die gesamte aktuelle Sitzung in die von Ihnen gewählte .log- oder .txt-Datei und zeichnet weiter auf, bis das Installationsprogramm geschlossen wird; Copy diagnostics kopiert eine Momentaufnahme und gibt eine barrierefreie Bestätigung. Speichern Sie vor einem Installations- oder Deinstallationsversuch, damit Ihre Aufzeichnung nach der Bereinigung der automatischen Protokolle erhalten bleibt. Technische Einzelheiten zu Dateien, Downloads, Compiler und Fehlern werden hier aufbewahrt, auch wenn das Statusfeld kürzere Meldungen verwendet. Es werden keine Daten hochgeladen. Protokolle können Windows-Benutzernamen und vollständige Pfade enthalten: Prüfen Sie sie vor dem Weitergeben. Bewusst exportierte Kopien bleiben nach der Deinstallation erhalten.

Beim ersten manuellen Start nach der Installation von MelonLoader kann dieser Unterstützungsdateien herunterladen und die Assemblies des Spiels vorbereiten. Rechnen Sie mit etwa einer Minute, auf manchen Systemen auch länger. Die Mod kann erst sprechen, nachdem MelonLoader sie geladen hat. Lassen Sie das Spiel geöffnet und warten Sie zuerst auf die Startansage von Bop It Access und anschließend auf die Ansage des Titelbildschirms, der Begrüßung oder des Hauptmenüs, bevor Sie die Spielsteuerung verwenden.


### Das kompilierte ZIP ohne die Bop-It-Access-EXE installieren

Wenn BopItAccess-v1.0.zip veröffentlicht ist, verwendet dieser Weg die bereits kompilierte DLL und benötigt kein .NET SDK. Sie brauchen weiterhin Ihr gekauftes Windows-x64-Spiel, den offiziellen x64 MelonLoader 0.7.3 Open-Beta und die Windows-x64-.NET-6-Laufzeit. Folgen Sie den offiziellen Downloadanweisungen von MelonLoader und Microsoft; das ZIP enthält diese Voraussetzungen nicht.

[MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer) · [.NET 6 Windows x64 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

1. Installieren Sie das Spiel über Steam und finden Sie seinen Installationsordner. Schließen Sie Bop It! vor Dateiänderungen; verwenden Sie bei Bedarf Steams Funktion zum Durchsuchen installierter Dateien.
2. Installieren Sie den offiziellen x64 MelonLoader in diesen Spielordner und stellen Sie sicher, dass die .NET-6-x64-Laufzeit installiert ist. Starten Sie das Spiel noch nicht: legen Sie zuerst den Mod ab.
3. Entpacken Sie das kompilierte BopItAccess-v1.0.zip in einen temporären Ordner. Kopieren Sie Mods/BopItAccess.dll in den Mods-Ordner des Spiels; erstellen oder ergänzen Sie ihn, ohne andere Mods zu löschen. Kopieren Sie prism.dll neben BopIt!.exe.
4. Kopieren Sie den gesamten documentation-Ordner und THIRD-PARTY-LICENSES einschließlich aller Sprachunterordner und Prism-Hinweise/-Lizenzen. Kopieren Sie README.txt und BopItAccess-uninstall.ps1 aus dem Paket. Das Skript ist nur eine Verknüpfung zu einem vom Installer verwalteten Uninstaller; Kopieren erstellt keinen funktionsfähigen Uninstaller oder Eintrag unter Windows Installierte Apps.
5. Behandeln Sie UserData/Loader.cfg sorgfältig: fehlt die Datei, kopieren Sie die Vorlage. Existiert sie, ergänzen Sie nur [loader] disable_start_screen=true und [console] hide_console=true in den passenden Abschnitten und behalten Sie alle anderen Einstellungen. Überschreiben Sie keine vorhandene Konfiguration mit der Vorlage.
6. Starten Sie bei Bedarf Ihren Screenreader und das Spiel über Steam. MelonLoader kann beim ersten Start Hilfsdateien herunterladen und Assemblies erzeugen, während der Mod bereits in Mods liegt. Warten Sie vor Spielaktionen auf Mod-Start- und Menüansagen.

Schließen Sie für ein manuelles Update das Spiel und kopieren Sie Mod-, Prism-, Dokumentations- und Lizenzdateien des neueren Pakets an dieselben Stellen. Behalten Sie BopItAccess.ini, andere Mods und fremde Dateien; ergänzen Sie Loader.cfg wie oben. Zum Deaktivieren/Entfernen des manuellen Mods löschen Sie nur Mods/BopItAccess.dll. Entfernen Sie für weitere Bereinigung nur die für diesen Mod kopierten Dateien und UserData/BopItAccess.ini oder dessen .tmp-Datei; behalten Sie gemeinsam benötigte Prism-/MelonLoader-Dateien. Gespeicherte Windows-Voreinstellungen können bleiben. Ein manuelles ZIP hat kein Eigentumsprotokoll und keinen registrierten Uninstaller. Falls Sie später den Installer nutzen, kann Uninstall eine ältere manuelle Kopie erkennen und Voreinstellungen bereinigen, während Dateien unbekannter Herkunft geschützt bleiben. Mod-Protokolle sind Mods/BopItAccess.log und Mods/BopItAccess.log.previous; entfernen Sie bei Bereinigung nur diese bekannten Mod-Protokolle.

Erweiterte Paketierung: scripts/package-mod.ps1 verpackt einen bereits kompilierten passenden Mod und bekannte Dokumentations-/Konfigurations-/Prism-Dateien. Es prüft Quell-/DLL-Versionen und schließt Spiel-, generierte und alte Nutzdaten aus; es kompiliert nicht. Archiv und Staging bleiben lokal.
