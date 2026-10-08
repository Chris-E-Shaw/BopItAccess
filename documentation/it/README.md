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

Dopo aver confermato Uninstall, scegli Uninstall for me oppure Uninstall for everyone. Entrambe le opzioni rimuovono i file condivisi della mod da questa cartella del gioco, quindi la mod non sarà più disponibile per nessuno che usi quell’installazione. La scelta determina di chi vengono rimosse le preferenze Windows salvate della mod: solo dell’account che ha richiesto l’operazione, oppure di tutti i profili Windows locali, compresi quelli con sessione disconnessa. Le preferenze del gioco originale vengono conservate. L’SDK .NET rimane installato.

Quando il programma di installazione rimuove la propria installazione di MelonLoader e nessun’altra mod ne ha bisogno, rimuove anche i file noti Loader.cfg e MelonPreferences.cfg e le cartelle Plugins, UserLibs e UserData se vuote. Vengono rimossi le impostazioni di Bop It Access, i registri noti, le guide e i file del programma di installazione. Le altre mod, i file condivisi del loader già presenti e i file non riconosciuti vengono protetti. Questo significa anche che un file sconosciuto può lasciare una cartella sul disco; il programma di installazione lo segnala nella diagnostica invece di eliminare dati estranei.

La pagina App installate di Windows usa gli stessi passaggi di conferma, scelta delle preferenze e pulizia. Il programma di installazione fornisce BopItAccess-uninstall.ps1 nella cartella del gioco come collegamento al programma di disinstallazione installato; anche le future compilazioni dai sorgenti includeranno questo script nei file prodotti. Copiare manualmente uno script non installa il programma di disinstallazione stesso. Per una precedente installazione manuale priva di un registro di proprietà dei file, il programma di installazione rimuove i file identificabili della mod e conserva i file condivisi di cui non è possibile stabilire l’origine.

Se la pulizia non può essere completata in sicurezza, il programma di installazione lo spiega e conserva le informazioni necessarie per riprovare. Per un’installazione gestita, la voce di disinstallazione di Windows e il punto di ripresa della pulizia rimangono finché la rimozione non riesce. Una vecchia copia manuale non ha un registro persistente di proprietà dei file; nel programma di installazione ancora aperto, riprova le operazioni segnalate dagli avvisi. Non installare, aggiornare o rimuovere la mod mentre Bop It! è in esecuzione.

## Stato del progetto

Questo progetto è sostanzialmente completo e non sono previsti contenuti o funzionalità importanti. Verrà mantenuto secondo necessità, con il feedback dei giocatori che guiderà i miglioramenti. Il repository GitHub contiene codice sorgente e documentazione tecnica. **Non sono ancora disponibili versioni GitHub.** Il codice sorgente ora contiene anche un progetto di installazione Windows. Fino alla pubblicazione di una versione, il pulsante **Installa** spiega che non è disponibile alcuna versione; **Installa alpha** crea l'ultimo commit del ramo principale dal sorgente.

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

### MelonLoader finestre di avvio

Il modello Loader.cfg fornito nasconde la schermata iniziale e la console separate di MelonLoader. L’installer applica queste due impostazioni prima che tu avvii personalmente il gioco. Non vengono saltate la schermata del titolo né quella di benvenuto del mod.

A gioco chiuso, apri `UserData/Loader.cfg` nella cartella del gioco. Se il file esiste già, imposta `disable_start_screen` su `true` nella sezione `[loader]` esistente e `hide_console` su `true` nella sezione `[console]` esistente. Mantieni tutte le altre voci. Se il file non esiste, copia il modello `UserData/Loader.cfg` fornito con la compilazione, oppure `configuration/Loader.cfg` dal codice sorgente. Non sostituire mai un Loader.cfg esistente con il modello completo.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

La mod non reimposta queste opzioni a ogni avvio. Per risolvere problemi puoi riportare manualmente una delle due impostazioni a false. Se la disinstallazione conserva un’installazione condivisa di MelonLoader, ripristina solo i flag modificati dal programma di installazione che non sono stati successivamente cambiati e conserva le altre modifiche. Se rimuove la propria installazione inutilizzata di MelonLoader, elimina anche i file noti Loader.cfg e MelonPreferences.cfg.

## Documentazione

- [Guida per l'utente del gioco e della mod (inglese)](BopItAccess-user-guide.html) - una panoramica dettagliata di controlli, impostazioni, menu e modalità di gioco adatta ai principianti.
- [Guida per l'utente giapponese (日本語)](../ja/BopItAccess-user-guide.html). Altre guide tradotte sono disponibili nelle cartelle delle lingue sotto [`documentation/`](../).
- [Guida dettagliata alle funzionalità e ai controlli](README.txt). La sua sezione di installazione descrive gli ZIP di installazione preparati localmente; questo repository GitHub fornisce solo la fonte.
- [Storia tecnica della costruzione](BopItAccess-build-history.html).
- [Revisione del codice in preparazione della pubblicazione](BopItAccess-release-review.html) — problemi risolti, file esaminati, risultati della compilazione e limiti rimanenti.
- [Flusso di lavoro Git per questo progetto](GIT-WORKFLOW.md).
- [Avvisi di terzi](THIRD-PARTY-NOTICES.txt).

Le copie tradotte dei documenti sopra si trovano in [`documentation/`](../) sotto il codice di ogni lingua supportata. La sorgente è in inglese; `scripts/translate_documents.py` può rigenerare le bozze tradotte automaticamente dopo le modifiche alla sorgente.

## Cosa potrebbe accadere dopo

Questo progetto è sostanzialmente completo e non sono previsti contenuti o funzionalità importanti. Tuttavia, questa mod verrà mantenuta e aggiornata attivamente nel tempo secondo necessità, con il feedback dei giocatori che guida questi miglioramenti. Il potenziale lavoro futuro include ulteriori revisioni e correzioni di bug, perfezionamento del codice e continui miglioramenti alla reattività vocale. Prism crea un possibile percorso verso altre piattaforme in futuro, ma questa mod attualmente supporta solo Windows x64. Il repository del progetto è il luogo in cui seguire gli ulteriori sviluppi.

## Nota sulla trasparenza dell'IA

Questa mod è stata realizzata con il « vibe coding ». Tutto il codice è stato completamente generato e ricercato dall'intelligenza artificiale, con una comprensione umana limitata della sua architettura sottostante. Si prega di utilizzare questa mod a proprio rischio.

Detto questo, ogni singola funzionalità della mod e decisione progettuale è stata creata e approvata da esseri umani. I test non sono mai stati automatizzati; sono stati eseguiti con attenzione e in modo approfondito da veri giocatori e tester umani.

Nota: il testo e la documentazione multilingue sono stati generati dall'intelligenza artificiale e non sono stati revisionati da madrelingua. È prevedibile un'elevata imprecisione della traduzione. Senza la codifica degli agenti, questo progetto non esisterebbe. Grazie per avergli dato una possibilità!

## Grazie

A coloro che hanno testato questa mod prima del rilascio e hanno contribuito a portarla dov'è ora, grazie. Sapete tutti chi siete. Ai giocatori che offrono feedback, provano la mod per la prima volta o credono in me e in questo progetto, grazie. Il tuo sostegno mi motiva a continuare a creare cose in un mondo che può sembrare folle e profondamente imperfetto. Spero che questo progetto ti renda più facile goderti il ​​gioco e giocare con gli altri. Grazie mille a tutti. Divertitevi Bop It!

— Christopher Shaw

## Licenza

Non è stata ancora selezionata una licenza per la sorgente Bop It Access. Prism ha la propria licenza; vedere il [avvisi di terzi](THIRD-PARTY-NOTICES.txt). Bop It! e il suo patrimonio appartengono ai rispettivi proprietari e non sono qui inclusi.


## Programma di installazione Windows 0.2.4

Usa BopItAccess-Installer-0.2.4.exe oppure BopItAccess-Installer.exe fornito dal progetto. Entrambi i nomi contengono lo stesso programma di installazione autonomo per Windows x64. Il progetto include il codice sorgente; non è ancora pubblicato alcun binario compilato pubblico o GitHub Release.

Chiudi Bop It!, apri il programma di installazione e approva la richiesta di autorizzazione come amministratore di Windows. Il programma di installazione ti dà il benvenuto, cerca il gioco nelle librerie Steam di tutte le unità disponibili e tenta di portare la propria finestra in primo piano. Controlla la cartella del gioco visualizzata; usa Browse se devi scegliere un’altra cartella. Tab passa da un controllo all’altro. Il registro di stato è un campo di testo di sola lettura: portaci il focus per esaminare i messaggi con i tasti di spostamento del cursore, selezionare il testo o copiarlo.

Il programma di installazione 0.2.4 richiede brevemente attivazione in primo piano e focus della tastiera all’avvio. Se al termine della sua breve osservazione iniziale è ancora attiva un’altra finestra, fa lampeggiare il titolo e il pulsante sulla barra delle applicazioni e chiede di passare al programma con Alt+Tab. Attivalo prima di usare i suoi comandi da tastiera o controller. Alt+G porta il focus sul campo della cartella del gioco.

Show advanced è deselezionato all’apertura del programma di installazione. Mostra Install alpha, Save diagnostics e Copy diagnostics. Install scarica l’ultima versione pubblica di GitHub quando disponibile. Non esiste ancora una versione pubblica, quindi al momento chi esegue i test deve usare Show advanced e Install alpha. L’installazione alpha chiede conferma, scarica i sorgenti più recenti e li compila sul tuo computer. Update compare quando viene trovata una versione pubblica più recente per una copia installata.

I messaggi di stato spiegano con parole semplici cosa viene scaricato, installato o completato. Una sola barra mostra l’avanzamento stimato dell’intera installazione, senza azzerarsi per ogni download o file. Avanza a incrementi di cinque punti percentuali; alcune fasi di preparazione possono richiedere tempo senza cambiamenti visibili. Il messaggio di benvenuto, la disponibilità di un nuovo aggiornamento e la conferma della copia della diagnostica vengono inviati al lettore di schermo tramite le notifiche di accessibilità di Windows. La loro lettura ad alta voce dipende dal lettore di schermo e dal suo supporto alle notifiche di Windows.

Il programma di installazione 0.2.4 non avvia mai Bop It! durante l’installazione. L’installazione alpha riutilizza i file locali di compilazione corrispondenti oppure prepara file temporanei dalla tua copia del gioco installata, che rimane chiusa. Il programma di installazione colloca quindi MelonLoader nella cartella del gioco e aggiunge subito Mods/BopItAccess.dll, seguito da Prism, impostazioni, documentazione completa e supporto alla disinstallazione. Attendi il messaggio di riuscita, poi avvia tu il gioco tramite Steam quando sei pronto.

Dopo un’installazione riuscita compare Play Bop It! The Video Game. Attiva questo pulsante per avviare tu il gioco tramite Steam quando sei pronto. Il programma di installazione non avvia mai automaticamente il gioco durante l’installazione.

Una versione compilata richiede il runtime .NET 6 per Windows x64, non un SDK di sviluppo. I runtime completi già presenti vengono riutilizzati. Un runtime mancante viene scaricato da Microsoft e collocato in MelonLoader/Dependencies/dotnet. Install alpha richiede anche un SDK .NET compatibile e il targeting pack di .NET 6: viene riutilizzato un SDK esistente oppure installato l’SDK ufficiale di Microsoft a livello di sistema. Il programma di installazione non crea nuove cartelle SDK nella cartella principale del gioco. MelonLoader 0.7.3 Open-Beta e Prism 0.18.3 provengono dalle rispettive versioni ufficiali. I componenti condivisi di Microsoft .NET e gli SDK rimangono installati dopo l’interruzione o la disinstallazione.

Quit chiude il programma di installazione. Se l’installazione è ancora in corso, chiede se interromperla e annullarne le modifiche prima di chiudere; Keep open prosegue normalmente. Se l’installazione termina mentre stai decidendo, la finestra di dialogo si aggiorna per indicarlo e Quit non annulla l’installazione completata. Una volta iniziata la rimozione, la disinstallazione viene completata in sicurezza prima dell’uscita. Anche Abort chiede conferma e annulla le modifiche ai file del gioco effettuate durante questo tentativo. L’annullamento durante l’installazione di Microsoft .NET attende che l’installazione di quei componenti condivisi termini in sicurezza.

Dopo aver confermato Uninstall, scegli Uninstall for me oppure Uninstall for everyone. Entrambe le opzioni rimuovono i file condivisi della mod da questa cartella del gioco, quindi la mod non sarà più disponibile per nessuno che usi quell’installazione. La scelta determina di chi vengono rimosse le preferenze Windows salvate della mod: solo dell’account che ha richiesto l’operazione, oppure di tutti i profili Windows locali, compresi quelli con sessione disconnessa. Le preferenze del gioco originale vengono conservate. L’SDK .NET rimane installato.

Quando il programma di installazione rimuove la propria installazione di MelonLoader e nessun’altra mod ne ha bisogno, rimuove anche i file noti Loader.cfg e MelonPreferences.cfg e le cartelle Plugins, UserLibs e UserData se vuote. Vengono rimossi le impostazioni di Bop It Access, i registri noti, le guide e i file del programma di installazione. Le altre mod, i file condivisi del loader già presenti e i file non riconosciuti vengono protetti. Questo significa anche che un file sconosciuto può lasciare una cartella sul disco; il programma di installazione lo segnala nella diagnostica invece di eliminare dati estranei.

Il programma di installazione rimane aperto dopo la disinstallazione, così puoi esaminare il risultato, salvare la diagnostica o installare di nuovo. Scegli Quit quando hai finito. L’utilità di disinstallazione in esecuzione e i file di diagnostica automatici vengono eliminati dopo la chiusura della finestra. Una reinstallazione nella stessa finestra avvia un nuovo registro dell’installazione; la pulizia differita non può rimuovere la nuova installazione.

La pagina App installate di Windows usa gli stessi passaggi di conferma, scelta delle preferenze e pulizia. Il programma di installazione fornisce BopItAccess-uninstall.ps1 nella cartella del gioco come collegamento al programma di disinstallazione installato; anche le future compilazioni dai sorgenti includeranno questo script nei file prodotti. Copiare manualmente uno script non installa il programma di disinstallazione stesso. Per una precedente installazione manuale priva di un registro di proprietà dei file, il programma di installazione rimuove i file identificabili della mod e conserva i file condivisi di cui non è possibile stabilire l’origine.

Se la pulizia non può essere completata in sicurezza, il programma di installazione lo spiega e conserva le informazioni necessarie per riprovare. Per un’installazione gestita, la voce di disinstallazione di Windows e il punto di ripresa della pulizia rimangono finché la rimozione non riesce. Una vecchia copia manuale non ha un registro persistente di proprietà dei file; nel programma di installazione ancora aperto, riprova le operazioni segnalate dagli avvisi. Non installare, aggiornare o rimuovere la mod mentre Bop It! è in esecuzione.

### Scorciatoie da tastiera del programma di installazione

| Azione | Scorciatoia da tastiera | Funzione |
| --- | --- | --- |
| Cartella del gioco | Alt+G | Assegnare il focus al campo della cartella del gioco. |
| Browse | Alt+B | Scegliere la cartella del gioco. |
| Install | Alt+I | Installare l’ultima versione pubblica quando disponibile. |
| Install alpha | Alt+A | Confermare e compilare i sorgenti più recenti; visibile con Show advanced. |
| Update | Alt+U | Installare una versione pubblica più recente quando viene proposta. |
| Play Bop It! The Video Game | Alt+P | Avviare il gioco tramite Steam; disponibile dopo un’installazione riuscita. |
| Uninstall | Alt+N | Confermare la rimozione e scegliere di chi rimuovere le preferenze Windows della mod. |
| Abort | Alt+R | Confermare l’annullamento dell’installazione corrente. |
| Registro di stato | Alt+L | Assegnare il focus ai messaggi di stato di sola lettura con testo selezionabile. |
| Show advanced | Alt+V | Mostrare o nascondere l’installazione alpha e gli strumenti di diagnostica. |
| Save diagnostics | Alt+D | Salvare la sessione diagnostica completa e continuare a registrarla; visibile con Show advanced. |
| Copy diagnostics | Alt+C | Copiare l’istantanea diagnostica completa; visibile con Show advanced. |
| Quit | Alt+Q | Chiudere, gestendo l’annullamento in sicurezza se un’operazione è in corso. |

### Uso di un controller nel programma di installazione

Il programma di installazione supporta i controller di tipo Xbox e gli altri controller che Windows rende disponibili tramite XInput. I suoi comandi sono separati dai comandi rimappabili del gioco. La croce direzionale o la levetta sinistra passa da un controllo all’altro; quando un campo di testo ha il focus, le direzioni servono invece a scorrerne il testo. I pulsanti dorsali passano sempre al controllo precedente o successivo che può ricevere il focus. A attiva il pulsante o la casella di controllo con il focus. L’input del controller viene gestito solo quando questo programma di installazione o una delle sue finestre di dialogo è in primo piano.

B torna indietro o annulla una finestra di dialogo; nella finestra principale del programma di installazione chiede di interrompere un’installazione in corso, altrimenti esegue Quit. Start esegue Quit nella finestra principale e torna indietro in una finestra di dialogo. Y (il pulsante frontale superiore) seleziona tutto il testo quando un campo di testo del programma di installazione ha il focus. Fuori dai campi di testo della finestra principale, Y attiva o disattiva Show advanced. Nel registro di stato o in un altro campo di testo del programma di installazione, la croce direzionale o la levetta sinistra funziona come le frecce: Sinistra/Destra si sposta per caratteri e Su/Giù per righe. Tieni premuto LT come Ctrl: Sinistra/Destra si sposta per parole e Su/Giù per paragrafi. Tieni premuto RT come Maiusc per estendere la selezione; tieni premuti LT e RT insieme per selezionare parole o paragrafi. X copia solo il testo selezionato; seleziona prima la parte desiderata. Ctrl+C sulla tastiera continua a copiare la selezione. Quando non è selezionato del testo, il programma di installazione invia anche notifiche accessibili per il carattere, la parola, la riga o il paragrafo nella posizione del cursore. Il programma di installazione invia una conferma accessibile quando il testo viene copiato e segnala una selezione vuota o un errore di copia. L’annuncio vocale dipende dal supporto del lettore di schermo per le notifiche di Windows. La navigazione con controller nelle finestre di dialogo native di Windows per le cartelle e il salvataggio richiede ancora una verifica umana. È sempre possibile usare una tastiera per inserire una cartella o un nome di file. Questa implementazione non copre i controller privi del supporto a XInput.

Il messaggio di benvenuto nel registro di stato elenca i comandi del controller per leggere il testo; usa Alt+L per tornare al registro. Modificare Show advanced invia una notifica di accessibilità di Windows che indica se la casella è selezionata o deselezionata. Anche la selezione di tutto il testo dà una conferma accessibile, oppure segnala che il campo è vuoto.

Il programma di installazione 0.2.4 richiede che ogni annuncio vocale emesso sostituisca la voce precedente dell’installer, inclusi lettura del testo, Seleziona tutto, stato selezionato/non selezionato di Show advanced, Copy diagnostics e altre conferme. LB/RB continua ad annunciare il nuovo controllo con il focus. La frequenza degli annunci di stato resta invariata; non ogni voce del registro viene letta automaticamente. L’interruzione effettiva dipende dal supporto delle notifiche Windows del lettore di schermo e richiede ancora una verifica umana.

### Diagnostica del programma di installazione

Show advanced mostra Save diagnostics (Alt+D) e Copy diagnostics (Alt+C). I registri automatici UTF-8 vengono conservati localmente in %ProgramData%\BopItAccess\diagnostics. Save diagnostics scrive l’intera sessione corrente nel file .log o .txt scelto e continua a registrare fino alla chiusura del programma di installazione; Copy diagnostics copia un’istantanea e fornisce una conferma accessibile. Salva prima di una prova di installazione o disinstallazione, così la registrazione sopravvive alla pulizia dei registri automatici. Qui vengono conservati i dettagli tecnici relativi a file, download, compilatore ed errori, anche se il campo di stato usa messaggi più brevi. Non viene caricato nulla online. I registri possono contenere nomi utente Windows e percorsi completi: controllali prima di condividerli. Le copie esportate intenzionalmente rimangono dopo la disinstallazione.

Al primo avvio manuale dopo l’installazione di MelonLoader, quest’ultimo può scaricare file di supporto e preparare gli assembly del gioco. Attendi circa un minuto, o più a lungo su alcuni sistemi. La mod non può parlare finché MelonLoader non la carica. Lascia aperto il gioco e attendi l’annuncio di avvio di Bop It Access, seguito dall’annuncio della schermata del titolo, di benvenuto o del menu principale prima di usare i comandi del gioco.
