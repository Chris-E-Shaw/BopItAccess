# Git-Arbeitsablauf für Bop It Access

Git bewahrt die Geschichte des Quellcodes und der Dokumentation dieses Projekts auf. Ein **Commit** ist ein benannter Stand, den du ansehen oder zu dem du zurückkehren kannst. GitHub veröffentlicht diese Commits, damit andere die Änderungen lesen und das Projekt selbst erstellen können. Ein Commit erstellt nicht automatisch eine öffentliche Veröffentlichung.

## Zur bestehenden Geschichte

Die ersten 37 Quellcode-Builds von `0.1.0` bis `0.6.12` wurden anhand der verfügbaren Quellcodearchive und ihrer ursprünglichen Änderungshinweise als einzelne Commits importiert. Ihre Git-Zeitstempel beziehen sich auf den Import, nicht auf die ursprünglichen Builddaten. Spätere Arbeiten werden direkt in Quellcode-Commits erfasst. Git und GitHub bilden jetzt die Änderungsgeschichte des Projekts; ein separates Buildgeschichtsdokument wird nicht mehr gepflegt.

Als Commitautor wird Christopher Shaws GitHub-No-Reply-Adresse verwendet. KI-unterstützte Commits enthalten eine `Co-authored-by`-Zeile mit dem tatsächlichen Modell, das mitgewirkt hat. Die Sitzungsaufzeichnungen nennen GPT-6 Luna für den ersten historischen Build und GPT-6 Sol für die folgenden 36. Verwende bei zukünftigen Commits den aktuellen Namen des mitwirkenden Modells.

Kompilierte DLLs, Installer, Veröffentlichungs-ZIPs, erzeugte Spielassemblies, persönliche Protokolle und vorübergehende Buildausgaben bleiben außerhalb der Git-Quellcodegeschichte. Lokale Versionstags werden nicht automatisch veröffentlicht. Eine GitHub-Veröffentlichung zu erstellen ist ein gesonderter, bewusster Schritt.

## Nützliche Befehle

Öffne PowerShell im Repository und führe Folgendes aus:

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

`git status` zeigt geänderte, hinzugefügte und noch nicht erfasste Dateien. `git diff` zeigt Änderungen außerhalb des Staging-Bereichs. `git log --oneline` zeigt die Commits. `git show --stat HEAD~1` zeigt die vom vorherigen Commit geänderten Dateien.

Diese Befehle zeigen Informationen zum Repository an, ohne die installierte Mod oder das Spiel zu verändern.

## Für jede zukünftige Änderung

1. Nimm die vorgesehenen Änderungen am Quellcode und an der Dokumentation vor. Aktualisiere bei einem neuen Build die Version.
2. Aktualisiere jede betroffene übersetzte Anleitung. Lege Spielerdokumentation und Lizenzhinweise der kompilierten Ausgabe bei; Entwickler-READMEs und dieser Arbeitsablauf gehören nicht zu Spieler-Veröffentlichungen.
3. Kompiliere, wenn die Änderung eine neue Binärdatei erfordert, und bereite die lokalen Dateien vor. Spieltests werden auf Anfrage von menschlichen Spielern durchgeführt; behaupte keine Laufzeitprüfung allein aufgrund einer erfolgreichen Kompilierung.
4. Führe `git status` und `git diff` aus. Nimm die vorgesehenen Dateien in den Staging-Bereich auf und prüfe anschließend `git diff --cached`. Erzeugte Binärdateien, private Protokolle und Spielreferenzen dürfen nicht unter den vorgemerkten Änderungen sein.
5. Erstelle einen beschreibenden Commit, dessen Betreff keine Versionsnummer enthält. Erkläre im Nachrichtentext, was sich geändert hat und warum, sowie relevante Prüfungen und Einschränkungen. Wenn eine KI mitgewirkt hat, nenne das tatsächliche Modell in einer Co-Autor-Zeile:

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Ersetze `MODEL NAME` durch das Modell, das die Arbeit geschrieben hat. Behalte Christopher Shaw als Autor mit der Adresse `336230252+Chris-E-Shaw@users.noreply.github.com` bei.
6. Wenn die Änderungen veröffentlicht werden können, führe `git push origin main` aus. Dadurch werden die Branch-Commits veröffentlicht, ohne lokale Tags zu übertragen oder eine Veröffentlichung zu erstellen.

Eine zusammenhängende Änderung kann ihren Quellcode und ihre Dokumentation im selben Commit enthalten. Separate Commits sind weiterhin nützlich, wenn sie unabhängige Änderungen beschreiben. Git lädt Arbeit nicht automatisch hoch: Übertrage sie bewusst, wenn sie für andere lesbar sein soll.
