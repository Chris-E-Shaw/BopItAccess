# Git-Workflow für Bop It Access

Git führt einen Verlauf der Änderungen am Quellcode und an den Dokumenten des Projekts. Ein **Commit** ist ein benannter Snapshot, den Sie überprüfen oder zu dem Sie zurückkehren können. GitHub veröffentlicht diesen Quellverlauf, damit andere den Code lesen und den Mod selbst erstellen können. Das Repository enthält keine kompilierten Mod-Dateien oder GitHub-Releases.

## Über die bestehende Geschichte

Die Quellarchive für Builds `v0.1.0` durch `v0.6.12` wurden als 37 aufeinanderfolgende Git-Commits importiert. Jeder Commit beschreibt die Quelländerungen mithilfe des entsprechenden Eintrags in [BopItAccess-build-history.html](BopItAccess-build-history.html). Diese Commits wurden während des Git-Imports erstellt, daher sind ihre Git-Zeitstempel **nicht** die ursprünglichen Erstellungsdaten. Ihre Themen beschreiben die Änderungen ohne Versionsnummern; Im Build-History-Dokument wird aufgezeichnet, welcher Quell-Snapshot zu jeder Version gehört.

Release-ZIP-Dateien, kompilierte DLLs und temporäre Build-Ausgaben bleiben außerhalb des Quellverlaufs von Git. Die Seite mit dem Build-Verlauf enthält Links zu den entsprechenden Quell-Commits GitHub. Vorhandene Versions-Tags bleiben lokal und sind nicht Teil der ursprünglichen GitHub-Veröffentlichung. Es sind noch keine GitHub Versions-Tags oder Releases veröffentlicht.

Git-Commits verwenden Christopher Shaws No-Reply-Adresse GitHub als Autor. Mit Codex geschriebene Commits umfassen auch a `Co-authored-by` Trailer mit Nennung des Modells, das zu der Arbeit beigetragen hat. Sitzungsdatensätze identifizieren GPT-6 Luna für den ersten historischen Build und GPT-6 Sol für die folgenden 36. Wenn sich das Modell für einen späteren Commit ändert, verwenden Sie seinen neuen Namen im Trailer dieses Commits.

## Nützliche Befehle

Öffnen Sie PowerShell in diesem Projektverzeichnis und führen Sie dann Folgendes aus:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

Diese Befehle prüfen nur das Repository; Sie ändern weder den Mod noch das installierte Spiel.

## Für jeden zukünftigen Build

1. Nehmen Sie die Quelländerungen vor und wählen Sie die nächste Build-Versionsnummer.
2. Erstellen Sie den Mod und bereiten Sie die lokalen Archive wie gewohnt vor. Schließen Sie das Ganze ein `documentation` Ordner mit der englischen Anleitung und jedem übersetzten Sprachunterordner in jedem Installationsarchiv. Kopieren Sie diesen Ordner in die Spielinstallation, wenn Sie einen Build installieren. Der In-Game-Guide liest bei jeder Eröffnung den HTML-Code für die aktuelle Spielsprache vor. Überprüfen Sie das Ergebnis, bevor Sie den Build als abgeschlossen protokollieren. Zusammengestellte Archive bleiben außerhalb von GitHub.
3. Lauf `git status` und `git diff`. Überprüfen Sie, welche Dateien geändert wurden. Stellen Sie die beabsichtigten Quell- und Dokumentationsänderungen bereit und überprüfen Sie sie dann mit `git diff --cached`.
4. Erstellen Sie einen beschreibenden Quell-Commit ohne eine Versionsnummer im Betreff. Fügen Sie ein `Co-authored-by` Trailer mit dem tatsächlichen Modellnamen, als Codex den Commit schrieb. Zum Beispiel, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; ersetzen `MODEL NAME` mit dem für diesen Commit verwendeten Modell.
5. Fügen Sie einen Eintrag für den Build hinzu `BopItAccess-build-history.html`, unter Verwendung des vorhandenen Commit-Formats. Beschreiben Sie die tatsächliche Änderung, ihren Grund und alle relevanten Einschränkungen und verknüpfen Sie den Quell-Commit aus Schritt 4. Aktualisieren Sie die entsprechenden übersetzten Kopien vor dem Verpacken. Übermitteln Sie den aktualisierten Verlauf mit demselben Autor und einem genauen Co-Autor-Trailer. Aktualisieren Sie den lokalen Dokumentationsordner und das Archiv, wenn die Verlaufsdatei bereits dorthin kopiert wurde.
6. Veröffentlichen Sie sowohl Quell- als auch Verlaufs-Commits mit `git push origin main` wenn es fertig ist. Dadurch wird nur der Zweig gepusht; Es werden keine lokalen Versions-Tags übertragen oder GitHub-Releases erstellt.

Kleinere Arbeiten, die keinen Build erzeugen, können einen eigenen Commit haben. Darauf kann dann der nächste Build-Commit folgen. Halten Sie persönliche Protokolle, Spielinstallationen, generierte Binärdateien und andere maschinenspezifische Dateien von Commits fern. Wenn GitHub-Releases später nützlich werden, entscheiden Sie sich zu diesem Zeitpunkt für Tags und kompilierte Downloads.

Git lädt neue Arbeiten nicht automatisch hoch. Pushen Sie es nach jedem lokalen Commit bewusst, wenn es für andere Leute sichtbar ist.
