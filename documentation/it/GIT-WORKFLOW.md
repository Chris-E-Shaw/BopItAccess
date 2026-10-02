# Flusso di lavoro Git per Bop It Access

Git conserva una cronologia delle modifiche al codice sorgente e ai documenti del progetto. Un **commit** è uno snapshot con nome che puoi controllare o a cui puoi tornare. GitHub pubblica questa cronologia delle fonti in modo che altri possano leggere il codice e creare da soli la mod. Il repository non contiene file mod compilati o versioni GitHub.

## Sulla storia esistente

Gli archivi di origine per le build `v0.1.0` attraverso `v0.6.12` sono stati importati come 37 commit Git successivi. Ogni commit descrive le modifiche all'origine utilizzando la voce corrispondente in [BopItAccess-build-history.html](BopItAccess-build-history.html). Questi commit sono stati creati durante l'importazione Git, quindi i loro timestamp Git **non** sono le date di build originali. I loro argomenti descrivono le modifiche senza numeri di versione; il documento della cronologia delle build registra quale snapshot di origine appartiene a ciascuna versione.

I file ZIP rilasciati, le DLL compilate e l'output di build temporaneo rimangono fuori dalla cronologia dei sorgenti di Git. La pagina della cronologia delle build si collega ai corrispondenti commit di origine GitHub. I tag di versione esistenti rimangono locali e non fanno parte della pubblicazione iniziale GitHub. Nessun tag di versione o release GitHub è stato ancora pubblicato.

I commit Git utilizzano l'indirizzo senza risposta GitHub di Christopher Shaw come autore. I commit scritti con Codex includono anche a `Co-authored-by` trailer che nomina il modello che ha contribuito al lavoro. I record di sessione identificano GPT-6 Luna per la prima build storica e GPT-6 Sol per le successive 36. Se il modello cambia per un commit successivo, utilizza il suo nuovo nome nel trailer di quel commit.

## Comandi utili

Apri PowerShell in questa directory del progetto, quindi esegui:

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

Questi comandi controllano solo il repository; non cambiano la mod o il gioco installato.

## Per ogni futura build

1. Apporta le modifiche all'origine e scegli il numero di versione della build successiva.
2. Costruisci la mod e prepara gli archivi locali come al solito. Includi l'intero `documentation` cartella, con la guida in inglese e ogni sottocartella della lingua tradotta, in ogni archivio di installazione. Copia quella cartella nell'installazione del gioco quando installi una build. La guida in-game legge l'HTML per la lingua di gioco corrente su ogni apertura. Ispezionare il risultato prima di registrare la build come completata. Gli archivi compilati rimangono al di fuori di GitHub.
3. Corri `git status` e `git diff`. Controlla quali file sono cambiati. Metti in scena le modifiche previste alla fonte e alla documentazione, quindi esaminale con `git diff --cached`.
4. Crea un commit di origine descrittivo senza un numero di versione nell'oggetto. Includere un `Co-authored-by` trailer con il nome del modello effettivo quando Codex ha scritto il commit. Ad esempio, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; sostituire `MODEL NAME` con il modello utilizzato per quel commit.
5. Aggiungi una voce per la compilazione `BopItAccess-build-history.html`, utilizzando il formato di stile commit esistente. Descrivi la modifica effettiva, la sua ragione ed eventuali limitazioni rilevanti e collega il commit di origine dal passaggio 4. Aggiorna le copie tradotte corrispondenti prima del confezionamento. Invia la cronologia aggiornata con lo stesso autore e un trailer accurato del coautore. Aggiorna la cartella e l'archivio della documentazione locale se il file della cronologia è già stato copiato al loro interno.
6. Pubblica sia la fonte che i commit della cronologia con `git push origin main` quando è pronto. Questo spinge solo il ramo; non invia tag di versione locale né crea versioni GitHub.

Un piccolo lavoro che non produce una build può avere il proprio commit. Il successivo commit di build può quindi seguirlo. Mantieni i log personali, le installazioni dei giochi, i file binari generati e altri file specifici del computer fuori dai commit. Se le versioni GitHub diventano utili in seguito, decidi i tag e i download compilati in quel momento.

Git non carica automaticamente il nuovo lavoro. Dopo ogni commit locale, spingilo deliberatamente quando è pronto per essere visto da altre persone.
