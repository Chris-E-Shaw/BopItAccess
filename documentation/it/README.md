# Bop It Access

Bop It Access è una mod di accessibilità non ufficiale per la versione Windows Steam di **Bop It!**. Utilizza MelonLoader e [Prism](https://github.com/ethindp/prism) per aggiungere feedback vocale e braille ai menu e alle schermate di gioco. Le funzionalità attuali includono una schermata di benvenuto di prima esecuzione, una guida per l'utente in-game, titolo parlato e schermate di pausa, impostazioni e controlli, selezione di brani, punteggi finali e classifiche, risultati, crediti, suggerimenti sui pulsanti, testo tutorial su richiesta con i controlli correnti assegnazioni prima di un round e descrizioni delle quattro fasi. La versione 0.9.0 utilizza Prism per l'output vocale e braille. La mod segue la lingua selezionata dal gioco e include una guida per ogni lingua offerta dal gioco.

## Modificare il file delle impostazioni

Se una lingua sconosciuta, l’audio troppo forte o una voce problematica rendono difficili da usare i menu, puoi cambiare le impostazioni fuori dal gioco. Dopo l’avvio, il mod crea automaticamente UserData/BopItAccess.ini nella cartella di Bop It!, usando le impostazioni attuali. È un file di testo che puoi aprire con un editor come Blocco note.

Il file include lingua, volumi di musica, effetti e voce, vibrazione, schermo intero, risoluzione e latenza audio del gioco; preferenze di sintesi vocale, braille, suggerimenti e altro del mod; profili vocali separati per OneCore e SAPI; e assegnazioni dei comandi del gioco e del mod destinati ai giocatori. Risoluzioni disponibili e voci installate sono elencate nei commenti.

Chiudi il gioco prima di modificare il file. Trova la sezione interessata e cambia il valore della voce già presente, salva il file e riavvia il gioco. Le modifiche vengono lette all’avvio, non immediatamente durante la partita. Le modifiche fatte nei menu aggiornano automaticamente il file.

I nomi delle sezioni e delle impostazioni restano in inglese in tutte le lingue. On e Off sono i valori consigliati per le opzioni attivabili; sono accettati anche True/False, Yes/No e 1/0. I commenti spiegano le scelte e gli intervalli. Le voci mancanti o non valide lasciano invariata l’impostazione salvata corrispondente, mentre le altre modifiche valide vengono applicate. Le assegnazioni duplicate dei comandi vengono rifiutate.

I commenti e le voci sconosciute vengono conservati. Se un altro programma modifica il file mentre il gioco è aperto, il mod smette di salvarlo per il resto della sessione, per proteggere le modifiche. Chiudi e riapri il gioco per applicarle. Puoi conservare una copia di sicurezza prima di cambiare il file.

Per un problema di voce, imposta Voice=System default nella sezione OneCore o SAPI. Le voci OneCore usano nome | lingua; SAPI accetta il nome visualizzato di una voce installata o il suo identificatore completo nel Registro. Il file elenca le scelte disponibili. OutputMode=Auto prova un lettore di schermo compatibile attivo, poi OneCore e infine SAPI.

L’esempio seguente ripristina l’inglese, un audio di gioco più basso e l’uscita vocale automatica con le voci predefinite del sistema. Modifica le voci corrispondenti già presenti nel file: è un estratto di riferimento, non un altro blocco da aggiungere. Mantieni le altre impostazioni.

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

L’azione Disinstalla del programma di installazione e le App installate di Windows rimuovono anche UserData/BopItAccess.ini e il relativo file .tmp, comprese le vecchie installazioni manuali.

## Stato del progetto

Questo progetto è in fase di sviluppo iniziale. Questo repository contiene il codice sorgente e la documentazione tecnica. **Non ci sono ancora build compilate o versioni GitHub qui.** Per utilizzare la mod da questo repository, creala dal sorgente e fornisci il runtime Prism descritto di seguito.

La cronologia dei commit include snapshot di origine ricostruiti di 37 build precedenti. I commit sono stati creati quando tali archivi sono stati importati in Git; le loro date non sono le date di costruzione originali. Il [storia tecnica della costruzione](BopItAccess-build-history.html) descrive il lavoro dietro ogni istantanea.

## Requisiti

- Windows x64 e la tua installazione di Bop It! per Steam.
- MelonLoader installato nella directory del gioco. Lo sviluppo ha utilizzato MelonLoader **0.7.3 Open-Beta** con la build del gioco x64 Unity **2022.3.50f1**. Altre combinazioni non sono state verificate.
- Un SDK .NET con il **.NET 6 targeting pack**, perché la mod ha come target `net6.0`.
- Per l'installazione, il numero ufficiale Windows x64 Prism v0.18.3 `prism.dll`. Questo file binario di terze parti non è presente in questo repository.

La mod fa riferimento alle DLL generate o installate da MelonLoader nella directory del gioco. Non include né ridistribuisce gli assembly di gioco.

<a id="build-from-source"></a>
## Costruisci dalla fonte

1. Installa MelonLoader, avvia Bop It! una volta, quindi chiudi il gioco. MelonLoader dovrebbe creare `MelonLoader\Il2CppAssemblies` sotto la directory del gioco.
2. Clona o scarica questo repository. Apri PowerShell nella directory principale del repository.
3. Impostato `$gameDir` nella **tua** directory di installazione Bop It!, quindi crea:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   Il percorso di esempio è la solita posizione Windows di Steam. Cambialo se la tua libreria Steam è altrove. Il progetto verifica la presenza del numero MelonLoader richiesto e delle DLL di gioco generate e segnala un percorso mancante prima della compilazione.

4. La DLL mod creata sarà in `src\bin\Release\net6.0\BopItAccess.dll`.

Se l'SDK segnala un pacchetto di targeting .NET 6 mancante, installa un SDK che includa quel pacchetto. Quello del progetto `NuGet.Config` non configura i feed dei pacchetti online.

<a id="install-your-build"></a>
## Installa la tua build

1. Chiudi il gioco. Copia il costruito `BopItAccess.dll` in `<game directory>\Mods\`. Crea il `Mods` directory se MelonLoader non l'ha creata.
2. Ottieni la versione ufficiale di Prism v0.18.3 per Windows x64 (`prism.dll`) dalla [pagina delle versioni di Prism](https://github.com/ethindp/prism/releases), oppure compila la stessa versione dal codice sorgente. Metti `prism.dll` accanto al file eseguibile del gioco, nella cartella principale del gioco, non nella cartella `Mods`.
3. Copia l'intero build `src\bin\Release\net6.0\documentation\` cartella nella directory del gioco. Contiene la guida inglese alla radice e le guide tradotte sotto `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, e `pt-BR`. Conserva quelle sottocartelle e i documenti associati. La guida in-game legge l'HTML per la lingua del gioco corrente ogni volta che si apre, quindi la sostituzione di una guida ne aggiorna il contenuto senza ricostruire la DLL.
4. Avvia lo screen reader prima del gioco. Se non ne è in esecuzione uno compatibile, Prism preferisce OneCore e usa SAPI se OneCore non è disponibile.

Il comando build Bop It Access compila solo questa mod; non crea né scarica Prism. Se la sintesi vocale non si avvia, controlla `<game directory>\Mods\BopItAccess.log`. Il registro registra l'inizializzazione e l'invio vocale Prism, sebbene un invio riuscito da solo non possa dimostrare che l'audio sia stato ascoltato.

Al primo avvio, viene visualizzata la schermata di benvenuto dopo che il menu principale del gioco è pronto. Le sue scelte aprono le Impostazioni Mod, leggi la guida dell'utente nel gioco o continua il gioco. Mod Impostazioni offre anche **Apri Guida per l'utente** e un'azione confermata **Ripristina schermata di benvenuto** che mostra la schermata di benvenuto al prossimo avvio. Nella guida, usa Su/Giù per scegliere gli argomenti o leggere le righe e Conferma per aprire un argomento. All'interno delle tabelle, Sinistra sposta una colonna a sinistra, Destra sposta una colonna a destra e Su/Giù mantiene la colonna corrente mentre si cambiano le righe. Le intestazioni delle colonne etichettano le celle anziché apparire come righe di dati; la tabella viene annunciata in entrata e la sua fine in uscita. Indietro lascia un argomento o la guida.

Scegli una lingua nella riga **Impostazioni > Lingua** del gioco. Il discorso mod segue quella selezione. La guida del gioco utilizza il documento HTML tradotto corrispondente, con l'inglese come fallback se la copia selezionata è mancante o illeggibile. Il testo non inglese fornito in bundle è un primo passaggio tradotto automaticamente; sono gradite correzioni da parte di chi parla fluentemente.

Le azioni usano le traduzioni del gioco. Shapes, Space, City e Office mantengono i nomi inglesi delle scene. Per OneCore o SAPI, scegli nelle Impostazioni mod una voce installata adatta alla lingua del gioco se quella predefinita non va bene. Voce, Volume, Velocità e Tono regolano l’uscita OneCore o SAPI effettivamente in uso, anche in modalità Auto. Appaiono solo i controlli supportati; con le altre uscite vengono nascosti. Ogni motore conserva le proprie impostazioni separatamente.

Leggi le posizioni nei menu. Questa opzione salvata, attiva per impostazione predefinita, legge la posizione dell’elemento nel menu. Il feedback uno a uno viene salvato ed è attivo per impostazione predefinita. Annuncia il colore attivo all’inizio e quando cambia. Quando si perde una vita, annuncia il numero di vite rimaste. Quando si guadagna una vita, annuncia il colore del giocatore e il nuovo totale, per esempio “Verde, 3 vite”, così entrambi sanno chi è stato più veloce. Questi annunci sono disattivati quando il feedback uno a uno è disattivato. Il limite di tre vite del gioco resta invariato. Questa funzione opera solo durante una partita uno contro uno. Dopo l’ultimo input di calibrazione, il mod dice subito “Fatto!”. Smetti di colpire e attendi il risultato misurato. Se la calibrazione fallisce perché non è stato dato alcun input, dice “Calibrazione fallita.”.

**Formatta il parlato**: Rende più naturale il testo tutto maiuscolo per voce e braille. Nella guida del gioco aggiunge una pausa con tre puntini prima del numero di riga, se il testo termina senza punteggiatura. Il testo visibile resta invariato. Disattivala per leggere il testo così com’è.

## Documentazione

- [Guida per l'utente del gioco e della mod (inglese)](BopItAccess-user-guide.html) - una panoramica dettagliata di controlli, impostazioni, menu e modalità di gioco adatta ai principianti.
- [Guida per l'utente giapponese (日本語)](../ja/BopItAccess-user-guide.html). Altre guide tradotte sono disponibili nelle cartelle delle lingue sotto [`documentation/`](../).
- [Guida dettagliata alle funzionalità e ai controlli](README.txt). La sua sezione di installazione descrive gli ZIP di installazione preparati localmente; questo repository GitHub fornisce solo la fonte.
- [Storia tecnica della costruzione](BopItAccess-build-history.html).
- [Flusso di lavoro Git per questo progetto](GIT-WORKFLOW.md).
- [Avvisi di terzi](THIRD-PARTY-NOTICES.txt).

Sono presenti le copie tradotte di tutti e sei i documenti sopra [`documentation/`](../) sotto ciascun codice di lingua supportato. La fonte è l'inglese; `scripts/translate_documents.py` può rigenerare le bozze tradotte automaticamente dopo le modifiche alla fonte.

## Trasparenza dell'IA

Christopher Shaw dirige questo progetto e ne valuta l'accessibilità nel gioco. I modelli OpenAI Codex hanno contribuito alla ricerca, al codice e alla documentazione. I messaggi di commit pubblicati includono a `Co-authored-by` trailer che identifica il modello che ha contribuito a ciascuna modifica; i crediti storici sono stati confrontati con i record delle sessioni di questo progetto. La cronologia di build precedente è stata ricostruita da archivi di origine salvati anziché registrati come commit in quel momento. I contributi assistiti dall'intelligenza artificiale possono contenere errori e devono essere rivisti prima dell'uso.

## Licenza

Non è stata ancora selezionata una licenza per la sorgente Bop It Access. Prism ha la propria licenza; vedere il [avvisi di terzi](THIRD-PARTY-NOTICES.txt). Bop It! e il suo patrimonio appartengono ai rispettivi proprietari e non sono qui inclusi.
