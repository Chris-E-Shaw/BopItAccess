Bop It Access 0.9.11 - Prism Sprache und Blindenschrift

Was das bewirkt
--------------
Der Mod folgt den Einstellungen > Sprachauswahl des Spiels für Sprache. Es beinhaltet
Englisch, Französisch, Italienisch, Deutsch, Spanisch (Spanien), Spanisch (Lateinamerika),
Japanisch, Koreanisch, vereinfachtes Chinesisch und brasilianisches Portugiesisch. Ändern der
Die Spielsprache ändert auch Mod-Ankündigungen und das Benutzerhandbuch im Spiel.
Die ersten Übersetzungen sind maschinell erstellte Entwürfe und müssen überprüft werden
von fließenden Sprechern.
Die Sprachausgabe ist standardmäßig aktiviert. Wenn der Mod mit eingeschalteter Sprache geladen wird, wird eine Ankündigung angezeigt
„Bop It Access Rede ist bereit. Das Spiel wird noch geladen. Warten Sie auf den Titelbildschirm oder die Ankündigung des Hauptmenüs, bevor Sie die Steuerelemente verwenden.“ bis Prism. Wenn der Titelbildschirm erscheint, kündigt der Mod die aktuelle Eingabe KLOPFEN zum Öffnen des Hauptmenüs an. Es liest
die fokussierte Hauptmenüschaltfläche und die fokussierte Einstellungszeile. Einstellungswerte sind
gesprochen, wobei der Zeilenname im Fokus steht. Einen Wert ändern, während der Fokus darauf bleibt
Zeile spricht nur den neuen Wert. AUDIO-LATENZ, STEUERUNG und ONLINE GEHEN sind Action
Tasten, daher werden sie ohne den im Spiel bedeutungslosen Platzhalter „0“ gesprochen.
Das Einstellungsmenü verfügt außerdem über einen LIMIT FPS-Schieberegler mit 30, 60, 120, 240 und
UNBEGRENZTE Auswahl. Bei einer Neuinstallation beginnt es bei 60 und merkt sich das
ausgewählten Wert zwischen Sitzungen. Focus spricht den Namen und den Wert; es ändern
spricht nur den neuen Wert. Die Obergrenze ändert die Zielbildrate von Unity während
Spielzeitskala, festes Update-Timing und Audio bleiben unverändert.
Wie bei jeder Frame-Obergrenze bedeutet eine niedrigere Einstellung auch weniger Frame-basierte Eingabeabfragen.
Wenn 30 FPS in einem schnellen Spiel weniger reaktionsschnell erscheinen, wählen Sie 60, 120 oder UNLIMITED.
Der Schalter „STUMM AUDIO IM HINTERGRUND“ wird direkt unter „VOICE OVER in“ angezeigt
Einstellungen. Wenn diese Option aktiviert ist, wird der Ton des Spiels stummgeschaltet, während das Spielfenster deaktiviert ist
Fokussiert und stellt dann den vorherigen Spiel-Audiostatus wieder her, wenn der Fokus zurückkehrt.
Es startet aus und wird zwischen den Sitzungen gespeichert.
Bei der allerersten Verwendung des Mods werden die native MUSIK, SFX und VOICE OVER des Spiels angezeigt
Die Schieberegler beginnen bei 30. Beim Upgrade bleiben die zuvor gespeicherten Audioeinstellungen des Spiels erhalten.
Sobald das Spiel zum ersten Mal das Hauptmenü erreicht, erscheint ein Begrüßungsbildschirm
Fokus. Seine Botschaft kann mit „Up“ wieder fokussiert werden und seine Auswahlmöglichkeiten öffnen Mod
Einstellungen, öffnen Sie das Benutzerhandbuch im Spiel oder fahren Sie mit dem Hauptmenü fort.
Der Begrüßungsbildschirm wird erst dann als abgeschlossen markiert, wenn eine Auswahl erfolgreich abgeschlossen wurde
es. Wenn Sie das Spiel schließen, während es geöffnet ist, ist es für den nächsten Start bereit.
Die Einstellungsindizierung wartet jetzt auf die vorherigen Zeilen Audio, FPS und MOD SETTINGS des Mods
Ankündigung des ersten fokussierten Elements auf einem neu geöffneten Einstellungsbildschirm.

Unter „Steuerelemente“ gibt es in „Einstellungen“ jetzt ein Menü „MOD-EINSTELLUNGEN“. SPEECH OUTPUT nutzt dasselbe
Gespeicherter Hauptschalter als F8 oder Controller-Auswahl, einschließlich der gesprochenen Wiederherstellung
Anweisungen, wenn die Sprachausgabe ausgeschaltet ist. Die BRAILLE-AUSGABE startet und wird gespeichert
zwischen den Sitzungen. Prism sendet Ankündigungen an einen kompatiblen Bildschirmleser
Brailleausgabe, wenn diese Einstellung aktiviert ist. Durch Ausschalten wird die Blindenschrift des Mods gestoppt
Nachrichten, während die Sprache verfügbar bleibt.
OUTPUT MODE: Auto verwendet einen laufenden unterstützten Screenreader, danach OneCore und zuletzt SAPI.
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Die einzelnen Screenreader und Sprach-Engines sind nur verfügbar, wenn die installierte Prism-Version und das System sie unterstützen. Ist der gewählte Modus nicht verfügbar, wechselt der Mod automatisch zu einer verfügbaren Ausgabe und sagt dies einmal an.
Gespeicherte SAPI-Stimmkennungen werden mit den Anzeigenamen in Prism abgeglichen; bei gleichen Namen kann die erste passende Stimme gewählt werden.
„SPRACHE IM HINTERGRUND STUMMSCHALTEN“ ist ein gespeicherter Schalter, standardmäßig deaktiviert. Wenn aktiviert,
Der Mod hört auf zu sprechen, sobald das Spiel den Fensterfokus verliert. Rede geschaffen
während das Spiel im Hintergrund läuft, wird verworfen und die Ankündigungen werden fortgesetzt
mit neuer Aktivität, nachdem der Fokus zurückgekehrt ist. Wenn die Sprache selbst ausgeschaltet ist, wenn das Spiel läuft
Erhält den Fokus wieder, stellt der Mod die aktuelle Tastatur und den Controller wieder her
Anweisungen einmal.

Das Menü MOD-EINSTELLUNGEN verfügt auch über Menüpositionen vorlesen, das standardmäßig aktiviert ist und zwischen Sitzungen gespeichert wird.
Wenn diese Option aktiviert ist, enthält ein fokussierter Menüpunkt seine Position, z. B. „PLAY, 1 von 6“.
Dies gilt für das Haupt- und Einstellungsmenü, die Steuerung, die Wiedergabemodi und Mod
Einstellungen, Game-Over-Auswahl, Bestenlisten, Erfolge, Credits und mehr
unterstützte Bildschirme. Die Zählung richtet sich nach den aktuell verfügbaren Auswahlmöglichkeiten. Verändern
Ein Schieberegler oder Schalter gibt, solange er fokussiert bleibt, immer noch nur den neuen Wert an.

SPRACHE FORMATIEREN ist ein gespeicherter Schalter in den Mod-Einstellungen, standardmäßig Ein. Vollständig großgeschriebene Menüwörter werden mit natürlicher Groß-/Kleinschreibung ausgegeben; gemischte Schreibweisen und Abkürzungen wie SAPI, NVDA, SFX und FPS bleiben erhalten. Wenn im Spielratgeber die Zeilennummer angesagt wird und der Text ohne Satzzeichen endet, werden zuvor drei Punkte für eine Pause eingefügt. Vorhandene Satzzeichen bleiben erhalten. Dies betrifft nur Sprache und Braille; sichtbare Spieltexte und der Ratgeber werden nicht verändert. Aus deaktiviert beide Anpassungen. Die bisherige Einstellung zur Großschreibung bleibt erhalten.

STEUERELEMENTTYPEN ANSAGEN ist ein weiterer gespeicherter MOD-EINSTELLUNGS-Schalter, der standardmäßig aktiviert ist. Wenn aktiviert,
Der Typ des fokussierten Elements folgt seinem Namen und geht seinem Wert und Index voraus:
„MUSIK-Schieberegler, 30, 1 von 12“, „VIBRATION umschalten, Ein, 6 von 12“ oder
„PLAY-Taste, 1 von 6“. Menüs identifizieren auch Registerkarten, Textfelder und sind lesbar
Listen Sie Elemente auf, sofern relevant. Wertänderungen sprechen weiterhin nur für den neuen Wert.
SLIDER RANGES ist ein gespeicherter Schalter, standardmäßig deaktiviert. Wenn aktiviert, werden die Schieberegler fokussiert
Melden Sie auch ihre verfügbaren Endpunkte nach dem aktuellen Wert, z
„MUSIK-Schieberegler, 30, Bereich 0 bis 100, 1 von 12“ beim Indizieren und Kontrollieren von Typen
sind aktiviert. Beim Verschieben eines Schiebereglers wird weiterhin nur der neue Wert angezeigt.
Das Eins-gegen-Eins-Feedback wird gespeichert und ist standardmäßig eingeschaltet. Nennt die aktive Farbe zu Beginn und bei einem Wechsel. Bei einem verlorenen Leben wird die verbleibende Anzahl genannt. Bei einem gewonnenen Leben werden die Spielerfarbe und die neue Anzahl genannt, etwa „Grün, 3 Leben“, damit beide Spieler wissen, wer schneller war. Diese Ansagen entfallen, wenn das Eins-gegen-Eins-Feedback ausgeschaltet ist. Die Grenze von drei Leben bleibt unverändert. Diese Funktion gilt nur während einer Eins-gegen-Eins-Runde.

HINTS TYPE erscheint jetzt über den AUTO-SPEAK-BUTTON-HINTS im Mod-Einstellungen-Menü.
AUTO-SPEAK-TASTE HINTS ist ein gespeicherter Schalter und standardmäßig aktiviert. Drehen
„Aus“ unterdrückt automatische Hinweise, während SPEAK HINTS bei Bedarf verfügbar bleibt.
TASTENHINWEISE VERZÖGERUNG hat Keine, 5 Sekunden
(Kann die Rede unterbrechen), 10 Sekunden, 15 Sekunden, 30 Sekunden und 60 Sekunden.
Der Standardwert beträgt 10 Sekunden. Mit „Keine“ werden die gültigen Eingaben für den aktuellen Bildschirm und angezeigt
ihre Aktionen sind in der gewöhnlichen Sprachzeichenfolge des fokussierten Elements enthalten,
nach einem Vollstopp. Die Aktion zur fokussierten Kontrolle wird vorab allgemein gesprochen
Menünavigation. Es gibt keine separate Ankündigung des ersten Hinweises. Mit einer zeitgesteuerten
Verzögerung, die erste Hinweisankündigung folgt auf so viel Inaktivität. Die 5-Sekunden-Option kann
eine bereits laufende Rede unterbrechen; Dahinter warten längere Verzögerungen. Nachher
Nach der ersten Hinweisansage beginnt eine weitere Verzögerung erst, wenn der Spieler etwas gibt
Eingabe, es sei denn, Wiederholungen sind aktiviert. Zu einem anderen fokussierten Element wechseln oder etwas ändern
Ein fokussierter Schieberegler oder Schalter zählt als Eingabe und startet die Verzögerung neu, auch wenn der
Beim Eingabebindungsscan des Spiels fehlt die Tasten- oder Controller-Aktion. Ein unbenutzter Schlüssel
Das ändert nichts an der Benutzeroberfläche und startet sie immer noch nicht neu.
HINTS TYPE ist ein gespeicherter Schieberegler mit den Optionen „Automatisch“, „Tastatur“, „Controller“ und „Beide“.
Automatisch ist die Standardeinstellung und folgt der zuletzt verwendeten Tastatur oder
Controller-Eingang. Die Verwendung einer Maus zählt als Tastatur. Tastatur und Controller sprechen
nur die Hinweise zu diesem Gerät; Beide geben beide Eingabesätze mit explizitem Gerät an
Namen. Die Speech-Off-Wiederherstellung umfasst immer beide Geräte
Der Spieler kann die Steuerung finden, die die Sprache wieder einschaltet.

Schaltflächenhinweise stellen die Eingabe vor ihre Aktion: „Enter oder Space, Element aktivieren.“
Bei Hinweisen zu einzelnen Geräten wird der Gerätename weggelassen. Controller-Stick-Namen werden gesprochen
vollständig, wie zum Beispiel „Linker Stick nach oben und unten“. Beide Modi identifizieren die Tastatur
und Controller-Eingänge. Der Mod liest die aktuellen Bindungen des Spiels also nativ
Neubindungsänderungen werden in diesen Hinweisen widergespiegelt. Wenn kein Controller vorhanden ist
verbunden und das Spiel hat auf verschiedenen Controllern unterschiedliche Namen der Gesichtstasten
Typen verwendet der Hinweis „Bestätigungsschaltfläche“ oder „Zurückschaltfläche“, anstatt eine anzunehmen
Xbox-Layout. In den Bewertungszeilen der Bestenliste wird „Bild nach oben“ und „Bild nach unten“ auf der Tastatur verwendet.
Controller nach oben/unten liest Zeilen nur, wenn kein Leaderboard-Steuerelement den Fokus hat;
Hinweise melden nur die Steuerelemente, die für den ausgewählten HINWEISTYP verfügbar sind. Gewöhnlich
Bildschirmhinweise umfassen auch die aktuellen SPEAK HINTS- und TOGGLE SPEECH-Bindungen.
Beide werden zentral nachgeschlagen, sodass zukünftige globale Kontrollen denselben beitreten können
Hinweisliste, ohne jeden Bildschirm einzeln zu ändern.

HINWEISE ZUR WIEDERHOLUNGSTASTE ist ein separater gespeicherter Schieberegler: Aus, 2x, 3x, 4x, 5x oder
Unendlich. Der Standardwert ist „Unendlich“. Die Zahl ist die Gesamtzahl der Messwerte in einem
Zyklus: 2x bedeutet der erste Hinweis und eine Wiederholung; 3x bedeutet den ersten Hinweis und
zwei Wiederholungen. Aus ermöglicht weiterhin den ersten automatischen oder manuellen Hinweis.
WIEDERHOLUNGSINTERVALL stellt die Verzögerung zwischen Wiederholungen auf 15, 30, 45 oder ein
60 Sekunden und standardmäßig 30 Sekunden. Wenn BUTTON HINTS DELAY auf „Keine“ eingestellt ist,
Der Wiederholungstimer beginnt unmittelbar nach der Eingabe. Eingabe, ein Fokus oder Wert
oder ein Bildschirmwechsel startet den Hinweiszyklus für den aktuellen Bildschirm neu.
SPEAK HINTS ersetzt das
wartet auf einen automatischen Hinweis für diesen Zyklus und verwendet dann das WIEDERHOLUNGSINTERVALL für jeden
konfigurierte Wiederholungen. Dies funktioniert auch bei ausgeschalteter AUTO-SPEAK-TASTE.
Hinweise werden währenddessen unterdrückt
Aktives Gameplay und die Beat-Timing-Phasen der Audiokalibrierung, wo extra
Sprache könnte einen Hinweis maskieren. Eine bereits gespeicherte 15-, 30- oder 60-Sekunden-Erinnerung
Verzögerung ab Version 0.6.2 wird zum neuen BUTTON HINTS DELAY-Wert.

MOD SETTINGS: Stimme, Lautstärke, Sprechgeschwindigkeit und Tonhöhe passen die tatsächlich verwendete OneCore- oder SAPI-Ausgabe an, auch im Auto-Modus. Nur unterstützte Regler sind sichtbar; bei anderen Ausgaben werden sie ausgeblendet. Beide Engines speichern ihre Einstellungen getrennt. Lautstärke: 5 % bis 100 % in Fünferschritten, standardmäßig 100 %. Geschwindigkeit und Tonhöhe: 0 bis 100 in Fünferschritten, standardmäßig 50. Die Mindestlautstärke hält Wiederherstellungshinweise hörbar.
Die Auswahl der Mod-Einstellungen wird zwischen den Sitzungen gespeichert. MOD-STANDARDS WIEDERHERSTELLEN
setzt diese Auswahl auf die oben beschriebenen Standardwerte zurück. Drücken Sie einmal darauf, um eine Anfrage zu stellen
Bestätigung, und drücken Sie dann innerhalb von fünf Sekunden erneut darauf, um sie wiederherzustellen. Umzug
Wenn Sie in eine andere Zeile wechseln oder fünf Sekunden verstreichen lassen, wird die Anfrage abgebrochen. Dies ist nicht der Fall
Ändern Sie die MUSIK-, SFX- oder VOICE-OVER-Schieberegler des Spiels, LIMIT FPS oder benutzerdefiniert
Tastatur- und Controller-Anbindungen. Zurück kehrt zu den Einstellungen zurück.
OPEN USER'S GUIDE liest den HTML-Leitfaden für die aktuell ausgewählte Sprache
in der Zeile „Einstellungen“ > „Sprache“ des Spiels. Der englische Reiseführer ist unter
documentation\BopItAccess-user-guide.html; Übersetzte Reiseführer sind in der entsprechenden Sprache
Unterordner. Wenn eine übersetzte Kopie fehlt oder unleserlich ist, der englische Leitfaden
wird stattdessen geöffnet. Die Themenliste stammt aus dem Inhaltsverzeichnis des Dokuments und
wird bei jedem Öffnen neu geladen. Bestätigen öffnet ein Thema. Up and Down liest seine Zeilen. In Tabellen,
„Links“ verschiebt eine Spalte nach links und „Rechts“ verschiebt eine Spalte nach rechts; Auf und ab halten
Die aktuelle Spalte beim Wechseln zwischen Zeilen. Spaltenüberschriften beschriften Zellen
anstatt als Datenzeilen zu erscheinen. Der Tisch wird einmalig beim Einlass bekannt gegeben und
sein Ende wird beim Verlassen angekündigt. Zurück kehrt zu den Themen zurück oder verlässt die Anleitung.
Während des Lesens wendet der Mod die Menümusik-Filterparameter des Spiels an und
stellt beim Beenden seinen vorherigen Wert wieder her. Der Willkommensbildschirm zum Zurücksetzen fragt nach einem
Drücken Sie innerhalb von fünf Sekunden ein zweites Mal, dann erscheint der Begrüßungsbildschirm auf dem
nächster Spielstart. Wenn Sie Zeilen ändern oder fünf Sekunden warten, wird die Bestätigung abgebrochen.
Dieses Update stellt das native Einstellungszeilenlayout des Spiels wieder her, also nach oben/unten
Die Navigation bleibt in den Einstellungszeilen, nachdem MOD SETTINGS hinzugefügt wurde.
Außerdem wird bei jedem Öffnen das Untermenü „MOD-EINSTELLUNGEN“ für SPRACHAUSGABE gestartet.
Dadurch wird verhindert, dass eine zuvor ausgewählte ZURÜCK-Zeile das Menü sofort schließt
wenn die Eingabetaste zum erneuten Öffnen verwendet wird.
Die Eingabe, die MOD SETTINGS öffnet, wird nun von seinen Zeilen ignoriert, bis diese Eingabe erfolgt
freigegeben, sodass die Sprachausgabe durch erneutes Öffnen des Menüs nicht ausgeschaltet werden kann. Die Mods
Hinzugefügte Steuerelemente, die Zeilen binden, warten ebenfalls auf die Eröffnungseingabe
freigegeben, bevor eine erneute Bindungsanfrage angenommen wird.

In Play liest der Mod „Solo“, „Party“, „Pass It“ und „One on One“, wenn er fokussiert ist.
Auf dem folgenden Songauswahlbildschirm nennt der Mod das aktuelle Thema
(Shapes, Space, City oder Office) und die Schwierigkeit: Klassisch oder Extrem.
DREHEN ändert den Song; gesprochen wird nur das neue Thema. ZIEHEN ändert
die Schwierigkeit; gesprochen wird nur Klassisch oder Extrem. Die Einführung
erklärt außerdem DREHEN, ZIEHEN, KLOPFEN und Zurück. Sie wird bei jeder
Modusauswahl und jedem erneuten Öffnen dieses Bildschirms wiederholt und
nennt dabei das aktuelle Thema und die aktuelle Schwierigkeit.
Drücken Sie auf diesem Bildschirm SPEAK HINTS (standardmäßig H oder drücken Sie den rechten Stick), um zu hören
Lesen Sie den nativen Tutorialtext des aktuellen Modus und der Schwierigkeit, bevor Sie beginnen. Jeder
Die benannte Aktion in dieser Referenz umfasst die aktuell zugewiesene Tastatur oder
Controller-Steuerung, folgende HINWEISE TYP. Neu zugewiesene Steuerelemente werden gelesen
die Bindungen des aktiven Spielers; One on One benennt die KLOPFEN Eingaben beider Spieler. Die
Die zeitgesteuerte Einblendung des Tutorials während des aktiven Spiels bleibt stumm, sodass sie nicht verdeckt wird
die gesprochenen Befehle des Spiels. Der Hinweis kündigt diese zusätzliche Verwendung von SPEAK HINTS an.
Das Steuerelement BESCHREIBUNGEN LESEN gibt eine visuelle Beschreibung des ausgewählten Elements aus
Shapes, Space, City oder Office Bühne auf Anfrage. Es ist auf diesem Bildschirm verfügbar
nur, bevor das Spiel beginnt. Die Standardeingaben sind G auf der Tastatur und LT
(linker Auslöser) am Controller. R wurde ersetzt, da es sich um den Reset des Spiels handelt
Gyro-Abkürzung. Die Bildschirmeinleitung kündigt die aktuelle Bindung an.
Beim Starten der Wiedergabe werden alle von der Songauswahl verbliebenen Sprachausgaben gestoppt, so dass diese nicht überdeckt werden können
verbale Hinweise des Spiels. Beschreibungen beginnen eher mit den Szenendetails als
Wiederholen des ausgewählten Künstlernamens.

Auf dem Endergebnisbildschirm geben Solo, Party und Pass It das Endergebnis bekannt
vor der Menürede. Solo liest die fokussierten Wiedergabe- und Bestenlisten-Schaltflächen
und erklärt Zurück. Die anderen Modi lesen ihre verfügbaren Fortsetzungs-, Wiederholungs- und Wiedergabemodi
Zurück-Eingabeaufforderungen. One on One zeigt also eher einen Gewinner als ein numerisches Endergebnis
Der Mod gibt den dort angezeigten Gewinner bekannt. Partiturrede hat Vorrang vor
die anfängliche Menüankündigung; Eingabeaufforderungen auf dem Ergebnisbildschirm werden danach in die Warteschlange gestellt.
Nachfolgende Menüfokusänderungen unterbrechen sich gegenseitig. Schnelle Änderungen sofort
nach dem Spielende werden kombiniert, bis die kurze Ergebnisankündigung Zeit hatte
Zum Schluss behalten Sie die aktuellste fokussierte Menüauswahl bei.
Solo-Highscore- und Gruppenrangansagen werden gesprochen, wenn das Spiel berichtet
ein neues Bestenlistenergebnis.
READ SCORE wiederholt das Endergebnis auf Anfrage nur während des Game-Over-Ergebnisses
Der Bildschirm ist sichtbar. Die Standardeingaben sind T auf der Tastatur und gedrückter linker Stick
Controller. Solo, Party und Pass It wiederholen ihr Endergebnis; Eins zu Eins
wiederholt den vom Spiel angezeigten Gewinner. Die Aktion ist während des Spiels deaktiviert.
Songauswahl und alle anderen Bildschirme. RT (rechter Auslöser) wurde nicht verwendet
Standardmäßig, da das Spiel es bereits an „Reset Gyro“ und „Auto Play“ bindet.
Gewünschte Wiederholungen sprechen sofort und können per Ergebnismenü unterbrochen werden
Navigation. Lediglich die automatische Ergebnisansage verzögert das Einstiegsmenü
Rede, so dass die Partitur zuerst gehört wird.
EINS-ZU-EINS-FEEDBACK ist in den Einstellungen > Mod-Einstellungen für gesprochenes Aktiv standardmäßig aktiviert
Farbe und verbleibende Leben in diesem Modus. Geteilte KLOPFEN Hinweise nicht von
identifizieren selbst eine Farbe, daher behält der Mod die letzte bestimmte Farbe bei.

TOGGLE SPEECH schaltet die gesamte normale Mod-Sprache auf jedem Bildschirm ein oder aus. Es ist
Die Standardeinstellungen sind F8 auf der Tastatur und Auswählen auf dem Controller. Im ausgeschalteten Zustand ist der Mod
stoppt die aktuelle Sprachausgabe und gibt zusammen mit der aktuellen Meldung bekannt, dass die Sprachausgabe deaktiviert ist
Tastatur- und Controller-Bedienelemente zum erneuten Einschalten. Der Aus-Zustand ist
zwischen Spielsitzungen gespeichert. Wenn das Spiel mit ausgeschalteter Sprache beginnt, gibt der Mod nach
diese Wiederherstellungsanweisungen anstelle der üblichen Lademeldung. Drehen
„Speech Back On“ kündigt „Speech On“ an. Die Sprache anderer Mods bleibt im ausgeschalteten Zustand stumm.
Die Steuerung „Sprache umschalten“ bleibt auch dann aktiv, wenn die Sprachausgabe ausgeschaltet ist.

Zu den Bestenlisten gelangen Sie über das Hauptmenü, Solo-Ergebnisse und Party-Ergebnisse
Ansage des ausgewählten Titels, Geräts, der Gruppe und des Datums, sofern verfügbar. Sie lesen
Rang, Spielername und Punktestand, einschließlich Lade- und Leerergebnisstatus. Seite nach oben
und „Bild nach unten“ lesen einzelne Partiturzeilen, auch wenn ein Filter den Fokus hat. Einheimisch
fokussierte Steuerelemente wie „Lokal“, „Freunde“, „Global“, „Heute“, „Dieser Monat“, „Alle Zeiten“
Zurück und Weiter werden gesprochen. Die Parteibestenliste gibt auch ihren Namen bekannt
Auswahl- und Bestätigungsstatus.

Dieses Update sorgt dafür, dass die Ergebnis-Bestenlisten während der Modus- und Songauswahl stumm bleiben.
Die Namen der Bestenlistenfilter werden zuerst gesprochen, die Ergebniszusammenfassungen werden danach in die Warteschlange gestellt.
Die Menüschaltfläche „Erfolge“ spricht normal; Buchanweisungen warten bis
seine Seiten werden tatsächlich geöffnet und das Schließen wird erst danach angekündigt.

Das Erfolgsbuch im Spiel gibt seine sichtbare Seite und die einzelnen Erfolge bekannt
Name, Beschreibung und gesperrter oder entsperrter Status. Verwenden Sie „Auf“ und „Ab“, um Elemente zu lesen
auf einer Seite. Die linken und rechten Steuerelemente des Spiels blättern wie gewohnt um.

Credits kündigen beim Öffnen die erste Zeile an. Verwenden Sie das Auf- und Ab-Menü des Spiels
Steuerelemente zum Lesen jeder Kreditlinie. Der visuelle automatische Bildlauf wird fortgesetzt
vor. Wenn diese Steuerelemente nicht verfügbar sind, werden die Leitungen bei ihrem Erscheinen in die Warteschlange gestellt.
Sollte das auch nicht klappen, wird der gesamte Abspanntext einmalig angesagt.

In Controls lautet der Mod KLOPFEN, KLOPFEN (Spieler 2), SCHNIPSEN, DREHEN, KREISEN, ZIEHEN,
und Auf Standard zurücksetzen. Es liest die aktuelle Bindung für das aktive Eingabegerät,
kündigt geänderte Bindungen an und liest das sichtbare Neubindungs-Feedback des Spiels.
Es wird einmal pro Besuch angesagt, wie man zu den Einstellungen zurückkehrt. Wenn sich „Auf Standard zurücksetzen“ ändert, a
Bindung meldet es, dass die Bindungen zurückgesetzt wurden.

Der Mod stellt eine native RESET GYRO-Zeile bereit und fügt eine CHANGE SPEECH OUTPUT hinzu
Verknüpfung. Reset Gyro übernimmt die Steuerung des Spiels; die modspezifischen Zeilen
bleiben am unteren Rand des Menüs zusammen, bevor sie auf die Standardeinstellungen zurückgesetzt werden. VERÄNDERUNG
SPEECH OUTPUT durchläuft dieselben Modi wie Settings > Mod Settings > OUTPUT
MODUS. Die Modusreihenfolge ist:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Die Standardeingaben der Verknüpfung sind F9 auf der Tastatur und die West-Face-Taste (X auf einer Xbox).
Controller). Die Starttaste des Controllers ist dem nativen Controller des Spiels vorbehalten
Menüaktion. Die aktuelle Auswahl wird bekannt gegeben, wenn die Tastenkombination verwendet wird und
wird durch die gleiche Ausgabemodus-Einstellung gespeichert.

Der Mod fügt dem Steuerungsmenü des Spiels neun Zeilen hinzu: Group Previous,
Nächste Gruppe, Vorheriges Datum, Nächstes Datum, BESCHREIBUNGEN LESEN, PARTITIERUNG LESEN,
Schalten Sie die Sprache um, sprechen Sie Hinweise und ändern Sie die Sprachausgabe. Die ersten vier
Behandeln Sie die Bestenlistenfilter
erreicht mit O/P und K/L auf dem Standard-Tastaturlayout oder den Bumpern und
D-Pad links/rechts auf einem Controller. Fokussieren Sie eine Zeile, um deren aktuelle Bindung zu hören.
Verwenden Sie dann die normale Aktion KLOPFEN/Bestätigen des Spiels, um es erneut zu binden. Die Zeilen scrollen
innerhalb des vorhandenen Steuerfelds. LESEN SIE BESCHREIBUNGEN, es kann auch ein Rebound für durchgeführt werden
Tastatur und Controller. Seine Bindung wird vom Mod gespeichert und auf die Standardwerte zurückgesetzt
stellt G und LT wieder her. READ SCORE kann auch für Tastatur und Controller reboundiert werden;
Beim Zurücksetzen auf Standard werden T und das Drücken des linken Steuerknüppels wiederhergestellt. Die Originalbindung des Spiels
Die Zeilen und die vier Bestenlistenzeilen verwenden denselben Controls-Rebinding-Ablauf.
TOGGLE SPEECH kann für Tastatur und Controller zurückgesetzt werden. Seine Bindungen sind
vom Mod gespeichert und „Auf Standard zurücksetzen“ stellt F8 und Auswählen wieder her. Wenn die Bindung
geändert wird, während die Sprachausgabe ausgeschaltet ist, kündigt der Mod die neuen Wiederherstellungssteuerungen an.
SPEAK HINTS kann auch reboundiert werden. Die Standardeinstellungen sind H und das Drücken des rechten Sticks.
Der Hinweis des aktuellen Bildschirms wird sofort vorgelesen, ohne dass eine Sekunde eingeplant werden muss
automatischer erster Hinweis. Die konfigurierten Wiederholungen können weiterhin folgen. Es ist still
während des Spiels und die zeitgesteuerten Audiokalibrierungshinweise.
SPRACHAUSGABE ÄNDERN kann für Tastatur und Controller zurückgesetzt werden; Zurücksetzen auf
Die Standardeinstellung stellt F9 und die Schaltfläche „Westseite“ wieder her. Liegt bereits eine neue Bindung vor
einem anderen Spiel oder einer anderen Mod-Aktion zugewiesen wurde, lehnt das Steuerungsmenü dies ab
dupliziert und behält die vorherige Zuordnung bei. RESET GYRO kann durch zurückprallen
die gleiche native Steuerungsprozedur wie die anderen Spielaktionen.
Wenn Sie von der Songauswahl zum Hauptmenü zurückkehren, werden auch die Hinweise zum Hauptmenü wiederhergestellt
wenn ein zwischengespeicherter Spielmanager immer noch einen alten Spielstatus meldet. Schaltflächenhinweis
Timer und automatische Hinweise zur Geräteauswahl folgen dem zugewiesenen Spiel und Mod
Kontrollen; Nicht verwendete Tasten, wie z. B. eine nicht zugewiesene Steuertaste, setzen sie nicht zurück.
Dieses Update verhindert, dass in der veralteten Steuerelementzeile „Neubindung fehlgeschlagen“ gemeldet wird.
wiederholt, nachdem die Szene geschlossen wurde. Beschreibungen nicht mehr vorübergehend lesen
überschreibt alle spieleigenen Eingabebindungen.

Innerhalb der Audiokalibrierung liest der Mod die Steuerelemente „Kalibrieren“, „Zurück“ und „KLOPFEN“.
kündigt die Anweisungen und Kalibrierungsschritte an, liest den Aufwärm-Countdown vor,
und gibt das angezeigte Latenzergebnis bekannt. Es wird nicht bei jedem Schlag gesprochen
Machen Sie die Timing-Übung, damit der Beat hörbar bleibt. Nach deiner letzten Kalibrierungseingabe sagt der Mod sofort „Fertig!“. Höre auf zu klopfen und warte auf das Messergebnis. Schlägt die Kalibrierung fehl, weil keine Eingabe erfolgte, sagt er „Kalibrierung fehlgeschlagen.“.

Der Pausenbildschirm sagt Pause, die ausgewählte Schaltfläche Fortsetzen oder Hauptmenü und die zugehörigen Tastenhinweise an. Ein Auswahlwechsel unterbricht die vorherige Pausenansage. Beim Fortsetzen oder Verlassen einer Runde wird verbleibende Pausensprache gestoppt, bevor das Spiel oder das Hauptmenü weitergeht. Während einer laufenden Runde lässt die Mod die Sprachbefehle und den laufenden Punktestand des Spiels unverändert.

Installieren
-------
Das Quell-Repository enthält keinen kompilierten Mod oder Prism DLL. Baue den Mod nach
Folgen Sie README.md, schließen Sie dann das Spiel und kopieren Sie BopItAccess.dll in seine Mods
Ordner. Beziehen Sie das offizielle Windows x64 Prism v0.18.3 prism.dll von
https://github.com/ethindp/prism/releases und platzieren Sie es neben der ausführbaren Datei des Spiels.
nicht innerhalb von Mods. Kopieren Sie den Dokumentationsordner des Builds in den Spielordner.
einschließlich der Unterordner für die übersetzte Sprache. Starten Sie ggf. Ihren Screenreader
Verwenden Sie eine und starten Sie dann Bop It! bis Steam. Prism kann SAPI verwenden, wenn eine unterstützt wird
Der Bildschirmleser läuft nicht. Der In-Game-Guide lädt den HTML-Code aus dem
Dokumentationsordner, wann immer er geöffnet wird. Dieser Mod wurde für MelonLoader entwickelt
0.7.3 Open-Beta und Bop It! (Unity 2022.3.50f1, x64).
Die ersten nicht-englischen Übersetzungen wurden mit maschineller Übersetzung erstellt
und benötigen eine Überprüfung durch fließende Redner. Bitte melden Sie unklare oder fehlerhafte Formulierungen.
Namen von Gameplay-Aktionen verwenden die übersetzten Begriffe des Spiels. Shapes, Space, City,
und Office bleiben als feste Bühnentitel englisch. Wenn die Systemstimme von SAPI dies tut
Wenn Sie Ihre Sprache nicht gut aussprechen, wählen Sie in Mod eine geeignete installierte Stimme aus
Einstellungen.

Probieren Sie die Menüs und Bildschirme aus
-------------------------
Warten Sie auf die Ankündigung auf dem Titelbildschirm, falls diese erscheint, und öffnen Sie sie dann mit KLOPFEN
Hauptmenü. Das Spiel kann einige Sekunden dauern, nachdem der Mod die Bereitschaftsmeldung erhalten hat
Akzeptieren Sie diese Eingabe.
Beim ersten Start erscheint der Begrüßungsbildschirm vor dem Hauptmenü. Wählen Sie es aus
Nachricht, um die Einleitung noch einmal zu hören. Wählen Sie „Mod-Einstellungen öffnen“, „Benutzer lesen“.
Anleitung oder Weiter zum Spiel. Speak Hints benennt seine aktuelle Tastatur und
Controller-Zuweisungen in der Willkommensnachricht unabhängig vom Hinweistyp.
Öffnen Sie Play und wechseln Sie zwischen den vier Modi. Wählen Sie eines aus, um zur Songauswahl zu gelangen.
DREHEN, um durch die Themen zu blättern, und ZIEHEN, um Klassisch oder Extrem zu wählen. Die
Mod kündigt jede Änderung an. Drücken Sie G oder LT, um die aktuell ausgewählte Stufe anzuhören
Beschreibung. Drücken Sie H oder drücken Sie den rechten Stick, um den Tutorialtext des Modus zu hören.
aktuell zugewiesene Aktionssteuerungen und Schaltflächenhinweise. KLOPFEN startet den gewählten Modus;
Zurück kehrt zurück. Während einer Runde verwenden
Drücken Sie die Menüsteuerung des Spiels, um die Pause zu öffnen, und wechseln Sie dann zwischen Fortsetzen und Hauptmenü.
Hören Sie am Ende eines Spiels auf den Punktestand oder den Eins-gegen-Eins-Gewinner, bevor der
Ergebnisbildschirmkontrollen werden angekündigt. Drücken Sie T oder den linken Stick, um den Vorgang zu wiederholen
Endergebnis, während der Game-Over-Bildschirm sichtbar ist.
Drücken Sie F8 oder Controller Select, um die Mod-Sprache von jedem Bildschirm aus ein- oder auszuschalten.
Drücken Sie F9 oder den West-Controller (X auf einem Xbox-Controller), um die Sprache zu durchlaufen
Ausgabemodus. Die gleiche Auswahl ist unter Einstellungen > Mod-Einstellungen > AUSGABEMODUS verfügbar.
Die BRAILLE-AUSGABE unter „Einstellungen“ > „Mod-Einstellungen“ ist standardmäßig aktiviert. NVDA's Braille-Viewer
kann die Blindenschrift und ihr Textäquivalent ohne physische Anzeige anzeigen.
Für einen EIN/AUS-Vergleich verwenden Sie den Braille-Modus „Folgen der Cursor“ von NVDA mit „Anzeigen“.
Nachrichten aktiviert; Sein Anzeige-Sprachausgabemodus würde sogar Sprache widerspiegeln
wenn die BRAILLE-AUSGABE-Einstellung des Mods auf „Aus“ steht.
Verwenden Sie unter „Einstellungen“ > „Mod-Einstellungen“ die Hinweise zur AUTO-SPEAK-TASTE, um sie zu aktivieren oder zu deaktivieren
automatische Anweisungen. Drücken Sie H oder drücken Sie den rechten Stick, um den aktuellen Hinweis zu hören
auf Anfrage.
HINWEISTYP wählt für diese Hinweise „Automatisch“, „Tastatur“, „Controller“ oder „Beide“ aus.
TASTENHINWEISE VERZÖGERUNG wählt aus, ob sie die Fokusrede begleiten oder einer folgen
Zeitraum der Inaktivität. HINWEISE ZUR WIEDERHOLUNGSTASTE und WIEDERHOLUNGSINTERVALL steuern beliebig
zusätzliche Erinnerungen.
Öffnen Sie Bestenlisten über das Hauptmenü oder einen Ergebnisbildschirm. Ändern Sie einen Filter in
Hören Sie sich die neue Auswahl an und lesen Sie einzelne Partituren mit Page Up und Page Down.
Standardmäßig verschieben sich O/P zwischen Bestenlistengruppen und K/L zwischen Datum
Bereiche. Ihre entsprechenden Controller-Eingänge sind linker/rechter Stoßfänger und
Steuerkreuz links/rechts. Die vier neuen Controls-Zeilen sollen diese neu zuordnen.
Öffnen Sie „Erfolge“ und blättern Sie mit Links und Rechts um. Verwenden Sie jeweils „Up“ und „Down“.
Eintrag. Öffnen Sie den Abspann und verwenden Sie „Auf“ und „Ab“, um die Zeilen unabhängig davon zu lesen
visuelle Schriftrolle.

Wenn die Sprache fehlt, überprüfen Sie Mods\BopItAccess.log im Spielordner. Es zeichnet auf
Panel-Erkennung, ausgewählte UI-Objekte sowie Prism Initialisierung und Versand.
Ein erfolgreicher Versand allein beweist nicht, dass Sprache hörbar war.

Um den Mod zu deaktivieren, entfernen Sie Mods\BopItAccess.dll.. MelonLoader kann installiert bleiben.

Dateien und Hinweise Dritter
-----------------------------
Prism ist eine Open-Source-Barrierefreiheitsbibliothek von Ethan Dupuy und Mitwirkenden.
Es ist unter der Mozilla Public License, Version 2.0, lizenziert. Diese Quelle
Das Repository enthält nicht prism.dll. Quelle, Veröffentlichungen und Lizenz:
https://github.com/ethindp/prism
Die aktuellen Abhängigkeitshinweise finden Sie unter THIRD-PARTY-NOTICES.txt.

Die Einstellungsdatei bearbeiten
--------------------------------
Wenn eine fremde Sprache, lauter Spielton oder eine problematische Stimme die Menüs schwer bedienbar machen, kannst du Einstellungen außerhalb des Spiels ändern. Nach dem Start erstellt der Mod automatisch UserData/BopItAccess.ini im Bop It!-Spielordner aus deinen aktuellen Einstellungen. Diese Textdatei lässt sich etwa mit dem Windows-Editor öffnen.

Die Datei enthält die Sprache, Musik-, Effekt- und Sprachausgabelautstärke, Vibration, Vollbild, Auflösung und Audiolatenz des Spiels; Sprach-, Braille-, Hinweis- und weitere Mod-Einstellungen; getrennte OneCore- und SAPI-Stimmprofile; sowie die für Spieler vorgesehenen Spiel- und Mod-Tastenbelegungen. Verfügbare Auflösungen und installierte Stimmen stehen in Kommentaren.

Schließe das Spiel vor dem Bearbeiten. Suche den passenden Abschnitt, ändere den Wert des vorhandenen Eintrags, speichere die Datei und starte das Spiel erneut. Änderungen werden beim Start eingelesen, nicht sofort während einer laufenden Sitzung. Änderungen über die Spielmenüs aktualisieren die Datei automatisch.

Abschnitts- und Einstellungsnamen bleiben in allen Sprachen Englisch. Für Schalter werden On und Off empfohlen; auch True/False, Yes/No und 1/0 werden akzeptiert. Kommentare erklären die Auswahlmöglichkeiten und Wertebereiche. Fehlende oder ungültige Einträge lassen die jeweilige gespeicherte Einstellung unverändert; andere gültige Änderungen werden trotzdem angewendet. Doppelte Tastenbelegungen werden abgelehnt.

Kommentare und unbekannte Einträge bleiben erhalten. Ändert ein anderes Programm die Datei bei laufendem Spiel, schreibt der Mod für den Rest der Sitzung nicht mehr hinein, um diese Änderungen zu schützen. Schließe und starte das Spiel erneut, um sie zu übernehmen. Du kannst vor Änderungen eine Sicherungskopie anlegen.

Bei Stimmproblemen setze Voice=System default im Abschnitt OneCore oder SAPI. OneCore-Stimmen verwenden Name | Sprache; SAPI akzeptiert den angezeigten Namen einer installierten Stimme oder deren vollständige Registrierungs-ID. Die Datei nennt die verfügbaren Möglichkeiten. OutputMode=Auto versucht einen laufenden unterstützten Screenreader, danach OneCore und zuletzt SAPI.

Das folgende Beispiel stellt Englisch, leiseren Spielton und automatische Sprachausgabe mit den Systemstimmen wieder her. Ändere die entsprechenden Einträge, die bereits in deiner Datei stehen. Dies ist ein Beispielausschnitt und kein zusätzlicher Block zum Anhängen. Behalte deine anderen Einstellungen bei.

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

Wählen Sie nach der Bestätigung von Uninstall entweder Uninstall for me oder Uninstall for everyone. Beide Optionen entfernen die gemeinsam genutzten Moddateien aus diesem Spielordner. Die Mod steht somit niemandem mehr zur Verfügung, der diese Installation verwendet. Die Auswahl legt fest, wessen gespeicherte Windows-Einstellungen der Mod entfernt werden: nur die des anfordernden Kontos oder die aller lokalen Windows-Profile, einschließlich abgemeldeter Profile. Die Einstellungen des ursprünglichen Spiels bleiben erhalten. Das .NET-SDK bleibt installiert.

Wenn das Installationsprogramm seine eigene MelonLoader-Installation entfernt und keine anderen Mods sie benötigen, entfernt es auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg sowie die leeren Ordner Plugins, UserLibs und UserData. Einstellungen von Bop It Access, bekannte Protokolle, Anleitungen und Installationsdateien werden entfernt. Andere Mods, bereits vorhandene gemeinsam genutzte Loaderdateien und unbekannte Dateien sind geschützt. Daher kann eine unbekannte Datei dazu führen, dass ein Ordner zurückbleibt; das Installationsprogramm meldet dies in der Diagnose, statt fremde Daten zu löschen.

Wenn die Bereinigung nicht sicher abgeschlossen werden kann, erklärt das Installationsprogramm dies und behält die für einen erneuten Versuch benötigten Informationen. Bei einer verwalteten Installation bleiben der Windows-Deinstallationseintrag und der Wiederaufnahmepunkt der Bereinigung erhalten, bis die Entfernung erfolgreich ist. Eine ältere manuelle Kopie hat keinen dauerhaften Eigentumsnachweis für die Dateien; versuchen Sie im geöffneten Installationsprogramm erneut, die in den Warnungen genannten Vorgänge auszuführen. Installieren, aktualisieren oder entfernen Sie die Mod nicht, während Bop It! läuft.

Hinweis zur KI-Transparenz
--------------------

Diese Mod wurde mit Vibe Coding erstellt. Der gesamte Code wurde vollständig durch künstliche Intelligenz generiert und erforscht, wobei das technische menschliche Verständnis der zugrunde liegenden Architektur begrenzt war. Bitte verwenden Sie diesen Mod auf eigenes Risiko.

Allerdings wurde jedes einzelne Mod-Feature und jede Designentscheidung von Menschen verfasst und genehmigt. Das Testen wurde nie automatisiert; Es wurde sorgfältig und ausführlich von echten menschlichen Spielern und Testern durchgeführt.

Bitte beachten Sie: Mehrsprachiger Text und Dokumentation wurden von KI generiert und nicht von Muttersprachlern überprüft. Es ist mit einer hohen Übersetzungsungenauigkeit zu rechnen. Ohne Agentencodierung würde dieses Projekt nicht existieren. Vielen Dank, dass Sie ihm eine Chance gegeben haben!

Was kann als nächstes kommen?
------------------

Dieses Projekt ist im Wesentlichen abgeschlossen und es sind keine größeren Inhalte oder Funktionen geplant. Dieser Mod wird jedoch im Laufe der Zeit bei Bedarf aktiv gepflegt und aktualisiert, wobei das Feedback der Spieler diese Verbesserungen vorantreibt. Zu den möglichen zukünftigen Arbeiten gehören weitere Überprüfungen und Fehlerbehebungen, Codeverfeinerungen und weitere Verbesserungen der Sprachreaktionsfähigkeit. Prism schafft einen möglichen Weg zu anderen Plattformen in der Zukunft, aber dieser Mod unterstützt derzeit nur Windows x64. Das Projekt-Repository ist der Ort, an dem Sie die weitere Entwicklung verfolgen können.

Vielen Dank
---------

Vielen Dank an diejenigen, die diesen Mod vor der Veröffentlichung getestet und dabei geholfen haben, ihn dorthin zu bringen, wo er jetzt ist. Ihr wisst alle, wer ihr seid. An die Spieler, die Feedback geben, den Mod zum ersten Mal ausprobieren oder an mich und dieses Projekt glauben, vielen Dank. Ihre Unterstützung motiviert mich, weiterhin Dinge in einer Welt zu schaffen, die sich verrückt und zutiefst fehlerhaft anfühlen kann. Ich hoffe, dass dieses Projekt es Ihnen leichter macht, das Spiel zu genießen und mit anderen zu spielen. Vielen Dank euch allen. Genießen Sie Bop It!

— Christopher Shaw

MelonLoader Startfenster
---------------------------

Die mitgelieferte Vorlage Loader.cfg verbirgt den separaten Startbildschirm und die Konsole von MelonLoader.
Der Installer setzt dieselben beiden Standardwerte, bevor Sie das Spiel selbst starten. Der Titelbildschirm
des Spiels und der Begrüßungsbildschirm der Mod werden dadurch nicht übersprungen.

Schließen Sie das Spiel und öffnen Sie UserData/Loader.cfg im Spielordner. Wenn die Datei bereits vorhanden ist, setzen Sie disable_start_screen im vorhandenen Abschnitt [loader] auf true und hide_console im vorhandenen Abschnitt [console] auf true. Behalten Sie alle anderen Einträge bei. Wenn die Datei fehlt, kopieren Sie die mitgelieferte Vorlage UserData/Loader.cfg aus dem Build oder configuration/Loader.cfg aus dem Quellcode. Ersetzen Sie niemals eine vorhandene Loader.cfg durch die gesamte Vorlage.

[loader]
disable_start_screen = true

[console]
hide_console = true

Die Mod setzt diese Optionen nicht bei jedem Start zurück. Zur Fehlersuche können Sie einen der beiden Werte manuell wieder auf false setzen. Wenn die Deinstallation eine gemeinsam genutzte MelonLoader-Installation beibehält, stellt sie nur die seit ihrer Einrichtung unveränderten Ziel-Flags des Installationsprogramms wieder her und bewahrt andere Änderungen. Wenn sie ihre eigene ungenutzte MelonLoader-Installation entfernt, entfernt sie auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg.

Nach einer erfolgreichen Änderung kündigt der Mod die Eingabe und die ihr zugewiesene Aktion an, zum Beispiel „Space zugewiesen an KLOPFEN“.

Vorabversion des Windows-Installationsprogramms 0.2.2
-----------------------------------------------------

Schließen Sie Bop It!, öffnen Sie das Installationsprogramm und bestätigen Sie die Windows-Abfrage für Administratorrechte. Das Installationsprogramm begrüßt Sie, sucht in den Steam-Bibliotheken auf allen verfügbaren Laufwerken nach dem Spiel und versucht, sein Fenster in den Vordergrund zu bringen. Prüfen Sie den angezeigten Spielordner; verwenden Sie Browse, wenn Sie einen anderen Ordner auswählen müssen. Mit Tab wechseln Sie zwischen den Bedienelementen. Das Statusprotokoll ist ein schreibgeschütztes Textfeld: Setzen Sie den Fokus darauf, um Meldungen mit den Cursortasten zu lesen, Text auszuwählen oder ihn zu kopieren.

Das Installationsprogramm 0.2.2 fordert beim Start kurz Vordergrundaktivierung und Tastaturfokus an. Ist am Ende der begrenzten Startbeobachtung noch ein anderes Fenster aktiv, blinken sein Fenstertitel und seine Taskleistenschaltfläche, und es fordert zum Wechsel mit Alt+Tab auf. Aktivieren Sie das Installationsprogramm, bevor Sie seine Tastatur- oder Controller-Steuerung verwenden. Alt+G setzt den Fokus auf das Spielordnerfeld.

Show advanced ist beim Öffnen des Installationsprogramms nicht aktiviert. Damit werden Install alpha, Save diagnostics und Copy diagnostics eingeblendet. Install lädt die neueste öffentliche GitHub-Version herunter, sofern eine vorhanden ist. Es gibt noch keine öffentliche Version, daher benötigen Tester derzeit Show advanced und Install alpha. Die Alpha-Installation bittet um Bestätigung, lädt den neuesten Quellcode herunter und kompiliert ihn auf Ihrem Computer. Update erscheint, wenn für eine installierte Kopie eine neuere öffentliche Version gefunden wird.

Die Statusmeldungen erklären in verständlicher Sprache, was heruntergeladen, installiert oder abgeschlossen wird. Eine einzige Fortschrittsanzeige zeigt den geschätzten Fortschritt der gesamten Installation, ohne für jeden Download oder jede Datei zurückgesetzt zu werden. Sie steigt in Schritten von fünf Prozentpunkten; manche Vorbereitungsschritte können Zeit benötigen, ohne dass eine sichtbare Änderung erfolgt. Die Begrüßung, ein neu verfügbares Update und die Bestätigung, dass Diagnosedaten kopiert wurden, werden über die Windows-Benachrichtigungen zur Barrierefreiheit an Ihren Screenreader gesendet. Ob sie vorgelesen werden, hängt von Ihrem Screenreader und seiner Unterstützung für Windows-Benachrichtigungen ab.

Das Installationsprogramm 0.2.2 startet Bop It! während der Installation niemals. Die Alpha-Installation verwendet passende lokale Builddateien erneut oder bereitet temporäre Dateien aus Ihrem eigenen installierten Spiel vor, während dieses geschlossen bleibt. Anschließend legt das Installationsprogramm MelonLoader im Spielordner ab und fügt sofort Mods/BopItAccess.dll hinzu, gefolgt von Prism, Einstellungen, der vollständigen Dokumentation und der Unterstützung für die Deinstallation. Warten Sie auf die Erfolgsmeldung und starten Sie das Spiel anschließend selbst über Steam, wenn Sie bereit sind.

Nach einer erfolgreichen Installation erscheint Play Bop It! The Video Game. Aktivieren Sie diese Schaltfläche, um das Spiel selbst über Steam zu starten, wenn Sie bereit sind. Das Installationsprogramm startet das Spiel während der Installation niemals automatisch.

Eine kompilierte Version benötigt die .NET-6-Laufzeitumgebung für Windows x64, kein Entwicklungs-SDK. Vorhandene vollständige Laufzeitumgebungen werden erneut verwendet. Eine fehlende Laufzeitumgebung wird von Microsoft heruntergeladen und unter MelonLoader/Dependencies/dotnet abgelegt. Install alpha benötigt außerdem ein kompatibles .NET-SDK und das .NET-6-Targeting-Pack: Ein vorhandenes SDK wird erneut verwendet oder das offizielle SDK von Microsoft systemweit installiert. Das Installationsprogramm erstellt keinen neuen SDK-Ordner im Stammordner des Spiels. MelonLoader 0.7.3 Open-Beta und Prism 0.18.3 stammen aus ihren offiziellen Veröffentlichungen. Gemeinsam genutzte Microsoft-.NET-Komponenten und SDKs bleiben nach einem Abbruch oder einer Deinstallation installiert.

Quit schließt das Installationsprogramm. Wenn die Installation noch läuft, fragt es, ob diese vor dem Schließen abgebrochen und rückgängig gemacht werden soll; Keep open setzt den Vorgang normal fort. Wenn die Installation abgeschlossen wird, während Sie sich entscheiden, zeigt der Dialog diesen Abschluss an, und Quit macht die abgeschlossene Installation nicht rückgängig. Sobald die Entfernung begonnen hat, wird die Deinstallation vor dem Beenden sicher abgeschlossen. Auch Abort bittet um Bestätigung und macht die Änderungen dieses Versuchs an den Spieldateien rückgängig. Bei einem Abbruch während der Einrichtung von Microsoft .NET wird gewartet, bis die Installation dieser gemeinsam genutzten Komponenten sicher abgeschlossen ist.

Wählen Sie nach der Bestätigung von Uninstall entweder Uninstall for me oder Uninstall for everyone. Beide Optionen entfernen die gemeinsam genutzten Moddateien aus diesem Spielordner. Die Mod steht somit niemandem mehr zur Verfügung, der diese Installation verwendet. Die Auswahl legt fest, wessen gespeicherte Windows-Einstellungen der Mod entfernt werden: nur die des anfordernden Kontos oder die aller lokalen Windows-Profile, einschließlich abgemeldeter Profile. Die Einstellungen des ursprünglichen Spiels bleiben erhalten. Das .NET-SDK bleibt installiert.

Wenn das Installationsprogramm seine eigene MelonLoader-Installation entfernt und keine anderen Mods sie benötigen, entfernt es auch die bekannten Dateien Loader.cfg und MelonPreferences.cfg sowie die leeren Ordner Plugins, UserLibs und UserData. Einstellungen von Bop It Access, bekannte Protokolle, Anleitungen und Installationsdateien werden entfernt. Andere Mods, bereits vorhandene gemeinsam genutzte Loaderdateien und unbekannte Dateien sind geschützt. Daher kann eine unbekannte Datei dazu führen, dass ein Ordner zurückbleibt; das Installationsprogramm meldet dies in der Diagnose, statt fremde Daten zu löschen.

Das Installationsprogramm bleibt nach der Deinstallation geöffnet, damit Sie das Ergebnis prüfen, Diagnosedaten speichern oder erneut installieren können. Wählen Sie Quit, wenn Sie fertig sind. Das laufende Hilfsprogramm zur Deinstallation und die automatischen Diagnosedateien werden nach dem Schließen des Fensters entfernt. Eine erneute Installation im selben Fenster beginnt mit einem neuen Installationsdatensatz; die verzögerte Bereinigung kann die neue Installation nicht entfernen.

Die Seite Installierte Apps in Windows verwendet dieselbe Bestätigung, Auswahl der Einstellungen und Bereinigung. Das Installationsprogramm stellt BopItAccess-uninstall.ps1 im Spielordner als Verknüpfung zum installierten Deinstallationsprogramm bereit; künftige Builds aus dem Quellcode enthalten dieses Skript ebenfalls in ihrer Ausgabe. Ein manuell kopiertes Skript installiert das Deinstallationsprogramm selbst nicht. Bei einer älteren manuellen Installation ohne Eigentumsnachweis für die Dateien entfernt das Installationsprogramm eindeutig erkennbare Moddateien und behält gemeinsam genutzte Dateien, deren Herkunft sich nicht feststellen lässt.

Wenn die Bereinigung nicht sicher abgeschlossen werden kann, erklärt das Installationsprogramm dies und behält die für einen erneuten Versuch benötigten Informationen. Bei einer verwalteten Installation bleiben der Windows-Deinstallationseintrag und der Wiederaufnahmepunkt der Bereinigung erhalten, bis die Entfernung erfolgreich ist. Eine ältere manuelle Kopie hat keinen dauerhaften Eigentumsnachweis für die Dateien; versuchen Sie im geöffneten Installationsprogramm erneut, die in den Warnungen genannten Vorgänge auszuführen. Installieren, aktualisieren oder entfernen Sie die Mod nicht, während Bop It! läuft.

Tastenkombinationen des Installationsprogramms
----------------------------------------------

Spielordner: Alt+G. Den Fokus auf das Spielordnerfeld setzen.
Browse: Alt+B. Den Spielordner auswählen.
Install: Alt+I. Die neueste öffentliche Version installieren, sofern verfügbar.
Install alpha: Alt+A. Den neuesten Quellcode bestätigen und kompilieren; mit Show advanced sichtbar.
Update: Alt+U. Eine neuere öffentliche Version installieren, wenn sie angeboten wird.
Play Bop It! The Video Game: Alt+P. Das Spiel über Steam starten; nach einer erfolgreichen Installation verfügbar.
Uninstall: Alt+N. Die Entfernung bestätigen und auswählen, wessen Windows-Einstellungen der Mod entfernt werden sollen.
Abort: Alt+R. Den Abbruch der aktuellen Installation bestätigen.
Statusprotokoll: Alt+L. Den Fokus auf die schreibgeschützten Statusmeldungen mit auswählbarem Text setzen.
Show advanced: Alt+V. Die Alpha-Installation und Diagnosewerkzeuge ein- oder ausblenden.
Save diagnostics: Alt+D. Die vollständige Diagnosesitzung speichern und weiter aufzeichnen; mit Show advanced sichtbar.
Copy diagnostics: Alt+C. Die vollständige Diagnosemomentaufnahme kopieren; mit Show advanced sichtbar.
Quit: Alt+Q. Schließen und einen Abbruch sicher behandeln, falls ein Vorgang läuft.

Einen Controller im Installationsprogramm verwenden
---------------------------------------------------

Das Installationsprogramm unterstützt Controller im Xbox-Stil und andere Controller, die Windows über XInput bereitstellt. Seine Steuerung ist unabhängig von der frei belegbaren Steuerung des Spiels. Mit dem Steuerkreuz oder dem linken Stick wechseln Sie zwischen den Bedienelementen; wenn ein Textfeld den Fokus hat, dienen die Richtungen stattdessen zum Lesen seines Texts. Die Schultertasten wechseln immer zum vorherigen oder nächsten Bedienelement, das den Fokus erhalten kann. A aktiviert die Schaltfläche oder das Kontrollkästchen mit dem Fokus. Controller-Eingaben werden nur verarbeitet, solange dieses Installationsprogramm oder einer seiner eigenen Dialoge im Vordergrund ist.

B geht zurück oder bricht einen Dialog ab; im Hauptfenster des Installationsprogramms fordert es den Abbruch einer laufenden Installation an, andernfalls entspricht es Quit. Start entspricht im Hauptfenster Quit und geht in einem Dialog zurück. Y (die obere Aktionstaste) wählt den gesamten Text aus, wenn ein Textfeld des Installationsprogramms den Fokus hat. Außerhalb der Textfelder im Hauptfenster schaltet Y Show advanced um. Im Statusprotokoll oder einem anderen Textfeld des Installationsprogramms wirken Steuerkreuz und linker Stick wie die Pfeiltasten: Links/Rechts bewegt sich um ein Zeichen, Auf/Ab um eine Zeile. Halten Sie LT wie Strg gedrückt: Links/Rechts bewegt sich um ein Wort, Auf/Ab um einen Absatz. Halten Sie RT wie Umschalt gedrückt, um die Auswahl zu erweitern; LT und RT zusammen wählen Wörter oder Absätze aus. X kopiert nur den ausgewählten Text; wählen Sie zuerst den gewünschten Teil aus. Strg+C auf der Tastatur kopiert die Auswahl weiterhin. Wenn kein Text ausgewählt ist, sendet das Installationsprogramm außerdem barrierefreie Benachrichtigungen zum Zeichen, Wort, zur Zeile oder zum Absatz an der Textmarke. Das Installationsprogramm sendet eine barrierefreie Bestätigung, wenn Text kopiert wurde, und meldet eine leere Auswahl oder einen Kopierfehler. Ob dies vorgelesen wird, hängt von der Unterstützung Ihres Screenreaders für Windows-Benachrichtigungen ab. Die Controller-Navigation in den nativen Windows-Dialogen zur Ordnerauswahl und zum Speichern muss noch von Menschen geprüft werden. Zur Eingabe eines Ordners oder Dateinamens steht weiterhin eine Tastatur zur Verfügung. Controller ohne XInput-Unterstützung werden von dieser Implementierung nicht abgedeckt.

Die Begrüßungsmeldung im Statusprotokoll nennt die Controller-Tastenkombinationen zur Textprüfung; mit Alt+L kehren Sie zum Protokoll zurück. Bei einer Änderung sendet Show advanced eine Windows-Benachrichtigung zur Barrierefreiheit, die angibt, ob es aktiviert oder deaktiviert ist. Auch die Auswahl des gesamten Texts wird barrierefrei bestätigt; bei einem leeren Feld wird dies gemeldet.

Diagnose des Installationsprogramms
-----------------------------------

Show advanced blendet Save diagnostics (Alt+D) und Copy diagnostics (Alt+C) ein. Automatische UTF-8-Protokolle werden lokal unter %ProgramData%\BopItAccess\diagnostics gespeichert. Save diagnostics schreibt die gesamte aktuelle Sitzung in die von Ihnen gewählte .log- oder .txt-Datei und zeichnet weiter auf, bis das Installationsprogramm geschlossen wird; Copy diagnostics kopiert eine Momentaufnahme und gibt eine barrierefreie Bestätigung. Speichern Sie vor einem Installations- oder Deinstallationsversuch, damit Ihre Aufzeichnung nach der Bereinigung der automatischen Protokolle erhalten bleibt. Technische Einzelheiten zu Dateien, Downloads, Compiler und Fehlern werden hier aufbewahrt, auch wenn das Statusfeld kürzere Meldungen verwendet. Es werden keine Daten hochgeladen. Protokolle können Windows-Benutzernamen und vollständige Pfade enthalten: Prüfen Sie sie vor dem Weitergeben. Bewusst exportierte Kopien bleiben nach der Deinstallation erhalten.

Beim ersten manuellen Start nach der Installation von MelonLoader kann dieser Unterstützungsdateien herunterladen und die Assemblies des Spiels vorbereiten. Rechnen Sie mit etwa einer Minute, auf manchen Systemen auch länger. Die Mod kann erst sprechen, nachdem MelonLoader sie geladen hat. Lassen Sie das Spiel geöffnet und warten Sie zuerst auf die Startansage von Bop It Access und anschließend auf die Ansage des Titelbildschirms, der Begrüßung oder des Hauptmenüs, bevor Sie die Spielsteuerung verwenden.
