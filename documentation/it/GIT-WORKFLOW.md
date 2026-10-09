# Flusso di lavoro Git per Bop It Access

Git conserva la cronologia del codice sorgente e della documentazione di questo progetto. Un **commit** è un'istantanea con un nome che puoi esaminare o ripristinare. GitHub pubblica questi commit, così altri possono leggere le modifiche e compilare il progetto autonomamente. Un commit non crea automaticamente una versione pubblica.

## Informazioni sulla cronologia esistente

Le prime 37 build del codice sorgente, da `0.1.0` a `0.6.12`, sono state importate come commit separati usando gli archivi disponibili e le note originali delle modifiche. I loro timestamp Git indicano l'importazione, non le date delle build originali. Il lavoro successivo viene registrato direttamente nei commit del codice sorgente. Git e GitHub sono ora la cronologia delle modifiche del progetto; non viene più mantenuto un documento separato con la cronologia delle build.

L'indirizzo GitHub senza risposta di Christopher Shaw è usato per l'autore dei commit. I commit con assistenza dell'IA includono una riga `Co-authored-by` che indica il modello che ha realmente contribuito. I resoconti delle sessioni identificano GPT-6 Luna per la prima build storica e GPT-6 Sol per le 36 successive. Usa il nome attuale del modello che ha contribuito per i commit futuri.

DLL compilate, programmi di installazione, ZIP di pubblicazione, assembly generati del gioco, registri personali e file temporanei di compilazione restano fuori dalla cronologia Git del codice sorgente. I tag di versione locali non vengono pubblicati automaticamente. Creare una versione su GitHub è un passaggio separato e intenzionale.

## Comandi utili

Apri PowerShell nel repository, poi esegui:

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

`git status` mostra i file modificati, aggiunti e non tracciati. `git diff` mostra le modifiche non preparate. `git log --oneline` permette di consultare i commit. `git show --stat HEAD~1` mostra i file modificati dal commit precedente.

Questi comandi esaminano il repository senza modificare la mod installata o il gioco.

## Per ogni modifica futura

1. Apporta le modifiche previste al codice sorgente e alla documentazione. Per una nuova build, aggiorna la versione.
2. Aggiorna ogni guida tradotta interessata. Mantieni la documentazione per i giocatori e gli avvisi di licenza insieme ai file compilati; i README per sviluppatori e questo flusso di lavoro non sono file per le versioni destinate ai giocatori.
3. Compila quando la modifica richiede un nuovo binario e prepara i file locali. I test di gioco sono eseguiti da giocatori umani quando richiesti; non affermare di aver verificato il funzionamento solo sulla base della compilazione.
4. Esegui `git status` e `git diff`. Aggiungi i file previsti all'area di staging, poi esamina `git diff --cached`. Non includere binari generati, registri privati o riferimenti del gioco nelle modifiche preparate.
5. Crea un commit descrittivo il cui titolo non contenga un numero di versione. Nel testo, spiega cosa è cambiato e perché, oltre alle verifiche e limitazioni pertinenti. Se ha contribuito un'IA, indica il modello effettivo in una riga di coautore:

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Sostituisci `MODEL NAME` con il modello che ha scritto il lavoro. Mantieni Christopher Shaw come autore, usando `336230252+Chris-E-Shaw@users.noreply.github.com`.
6. Quando le modifiche sono pronte da pubblicare, esegui `git push origin main`. Il comando pubblica i commit del ramo senza inviare i tag locali o creare una versione.

Una modifica coerente può includere codice sorgente e documentazione nello stesso commit. Commit separati restano utili per modifiche indipendenti. Git non carica il lavoro automaticamente: invialo intenzionalmente quando è pronto per essere letto da altri.
