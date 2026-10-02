# Bop It Access

Bop It Access è una mod di accessibilità non ufficiale per la versione Windows Steam di **Bop It!**. Utilizza MelonLoader e Tolk per aggiungere feedback vocale e braille ai menu e alle schermate di gioco. Le funzionalità attuali includono una schermata di benvenuto di prima esecuzione, una guida per l'utente in-game, titolo parlato e schermate di pausa, impostazioni e controlli, selezione di brani, punteggi finali e classifiche, risultati, crediti, suggerimenti sui pulsanti, testo tutorial su richiesta con i controlli correnti assegnazioni prima di un round e descrizioni delle quattro fasi. La versione 0.8.0 segue la lingua selezionata dal gioco e include una guida per ogni lingua offerta dal gioco.

## Stato del progetto

Questo progetto è in fase di sviluppo iniziale. Questo repository contiene il codice sorgente e la documentazione tecnica. **Non ci sono ancora build compilate o versioni GitHub qui.** Per utilizzare la mod da questo repository, creala dal sorgente e fornisci i file runtime Tolk descritti di seguito.

La cronologia dei commit include snapshot di origine ricostruiti di 37 build precedenti. I commit sono stati creati quando tali archivi sono stati importati in Git; le loro date non sono le date di costruzione originali. Il [storia tecnica della costruzione](BopItAccess-build-history.html) descrive il lavoro dietro ogni istantanea.

## Requisiti

- Windows x64 e la tua installazione di Bop It! per Steam.
- MelonLoader installato nella directory del gioco. Lo sviluppo ha utilizzato MelonLoader **0.7.3 Open-Beta** con la build del gioco x64 Unity **2022.3.50f1**. Altre combinazioni non sono state verificate.
- Un SDK .NET con il **.NET 6 targeting pack**, perché la mod ha come target `net6.0`.
- Per l'installazione, compatibile a 64 bit `Tolk.dll` e `nvdaControllerClient64.dll` file di esecuzione. Questi file binari di terze parti non sono presenti in questo repository.

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
2. Ottieni una versione compatibile a 64 bit `Tolk.dll` da una fonte attendibile o [costruirlo dalla fonte upstream Tolk](https://github.com/dkager/tolk#compiling). Ottieni l'abbinamento `nvdaControllerClient64.dll` da [Directory della libreria x64 di Tolk](https://github.com/dkager/tolk/tree/master/libs/x64) o la tua build Tolk. Inserisci **entrambe le DLL nella directory del gioco**, accanto all'eseguibile del gioco, anziché all'interno `Mods`.
3. Copia l'intero build `src\bin\Release\net6.0\documentation\` cartella nella directory del gioco. Contiene la guida inglese alla radice e le guide tradotte sotto `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, e `pt-BR`. Conserva quelle sottocartelle e i documenti associati. La guida in-game legge l'HTML per la lingua del gioco corrente ogni volta che si apre, quindi la sostituzione di una guida ne aggiorna il contenuto senza ricostruire la DLL.
4. Avvia il tuo screen reader se ne usi uno, quindi avvia da Bop It! a Steam. La mod può utilizzare il parlato SAPI quando non è in esecuzione alcun screen reader supportato.

Il comando build Bop It Access compila solo questa mod; non crea né scarica Tolk. Se la sintesi vocale non si avvia, controlla `<game directory>\Mods\BopItAccess.log`. Il registro registra se Tolk ha inizializzato e accettato le richieste vocali, sebbene ciò da solo non possa dimostrare che l'audio sia stato ascoltato.

Al primo avvio, viene visualizzata la schermata di benvenuto dopo che il menu principale del gioco è pronto. Le sue scelte aprono le Impostazioni Mod, leggi la guida dell'utente nel gioco o continua il gioco. Mod Impostazioni offre anche **Apri Guida per l'utente** e un'azione confermata **Ripristina schermata di benvenuto** che mostra la schermata di benvenuto al prossimo avvio. Nella guida, usa Su/Giù per scegliere gli argomenti o leggere le righe e Conferma per aprire un argomento. All'interno delle tabelle, Sinistra sposta una colonna a sinistra, Destra sposta una colonna a destra e Su/Giù mantiene la colonna corrente mentre si cambiano le righe. Le intestazioni delle colonne etichettano le celle anziché apparire come righe di dati; la tabella viene annunciata in entrata e la sua fine in uscita. Indietro lascia un argomento o la guida.

Scegli una lingua nella riga **Impostazioni > Lingua** del gioco. Il discorso mod segue quella selezione. La guida del gioco utilizza il documento HTML tradotto corrispondente, con l'inglese come fallback se la copia selezionata è mancante o illeggibile. Il testo non inglese fornito in bundle è un primo passaggio tradotto automaticamente; sono gradite correzioni da parte di chi parla fluentemente.

Il mod utilizza i nomi tradotti del gioco per azioni di gioco. Shapes, Space, City, e Office soggiornano in inglese come titoli fissi. L'output vocale selezionato ha bisogno di una voce per la tua lingua. Per l'output SAPI, scegli una voce installata adatta alla tua lingua se la voce predefinita del sistema suona sbagliata.

## Documentazione

- [Guida per l'utente del gioco e delle mod](BopItAccess-user-guide.html) - una panoramica dettagliata di controlli, impostazioni, menu e modalità di gioco adatta ai principianti.
- [Guida dettagliata alle funzionalità e ai controlli](README.txt). La sua sezione di installazione descrive gli ZIP di installazione preparati localmente; questo repository GitHub fornisce solo la fonte.
- [Storia tecnica della costruzione](BopItAccess-build-history.html).
- [Flusso di lavoro Git per questo progetto](GIT-WORKFLOW.md).
- [Avvisi di terzi](THIRD-PARTY-NOTICES.txt).

Sono presenti le copie tradotte di tutti e sei i documenti sopra [`documentation/`](../) sotto ciascun codice di lingua supportato. La fonte è l'inglese; `scripts/translate_documents.py` può rigenerare le bozze tradotte automaticamente dopo le modifiche alla fonte.

## Trasparenza dell'IA

Christopher Shaw dirige questo progetto e ne valuta l'accessibilità nel gioco. I modelli OpenAI Codex hanno contribuito alla ricerca, al codice e alla documentazione. I messaggi di commit pubblicati includono a `Co-authored-by` trailer che identifica il modello che ha contribuito a ciascuna modifica; i crediti storici sono stati confrontati con i record delle sessioni di questo progetto. La cronologia di build precedente è stata ricostruita da archivi di origine salvati anziché registrati come commit in quel momento. I contributi assistiti dall'intelligenza artificiale possono contenere errori e devono essere rivisti prima dell'uso.

## Licenza

Non è stata ancora selezionata una licenza per la sorgente Bop It Access. Tolk e NVDA Controller Client hanno le proprie licenze; vedere il [avvisi di terzi](THIRD-PARTY-NOTICES.txt). Bop It! e il suo patrimonio appartengono ai rispettivi proprietari e non sono qui inclusi.
