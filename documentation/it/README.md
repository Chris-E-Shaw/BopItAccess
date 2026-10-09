# Bop It Access

Bop It Access è una mod di accessibilità per persone cieche, destinata alla versione Steam per Windows x64 di **Bop It! The Video Game**. Utilizza [MelonLoader](https://github.com/LavaGang/MelonLoader) e [Prism](https://github.com/ethindp/prism) per rendere accessibili con voce e braille i menu e le schermate del gioco, con comandi e impostazioni aggiuntivi per un'esperienza più confortevole.

Versioni attuali del codice sorgente: **mod 0.9.13** e **programma di installazione 0.2.8**. La prima versione pubblica è in arrivo.

## Funzionalità

- Voce per menu, impostazioni, tutorial, classifiche, obiettivi, crediti, schermate di pausa e risultati.
- Lettura su richiesta di punteggi, descrizioni degli scenari e suggerimenti che rispettano le assegnazioni correnti dei comandi.
- Voce e guida integrata in tutte le lingue offerte dal gioco.
- Uscita per lettori di schermo e braille, con OneCore e SAPI come opzioni di sintesi vocale del sistema.
- Livello di dettaglio della voce, tempi e ripetizioni dei suggerimenti regolabili, oltre a scorciatoie vocali riassegnabili.
- Assegnazioni aggiuntive dei comandi del gioco, limite alla frequenza dei fotogrammi, controlli audio in background e file delle impostazioni leggibile.
- Programma di installazione accessibile con tastiera e controller, per installare, aggiornare e disinstallare.

## Stato del progetto

Le funzionalità principali sono sostanzialmente complete. Il progetto sarà mantenuto secondo necessità, con i riscontri dei giocatori a guidare correzioni e miglioramenti. La piattaforma attualmente supportata è Windows x64.

Questo repository contiene codice sorgente e documentazione. **Non è ancora stata pubblicata una versione pubblica.** La prossima pubblicazione offrirà `BopItAccess-Installer.exe` e un archivio compilato `BopItAccess-v1.0.zip`. Gli archivi del codice sorgente ZIP e TAR.GZ generati automaticamente da GitHub contengono il codice, non una mod pronta da installare. Fino alla prima pubblicazione, l'opzione **Mostra opzioni avanzate > Installa alpha** del programma di installazione compila il codice sorgente più recente da `main`.

I commit Git costituiscono la cronologia del progetto. Le prime 37 build sono state importate come istantanee separate del codice sorgente; le date di quei commit indicano l'importazione, non le date delle build originali. Questo repository non contiene file binari compilati, file del gioco o assembly del gioco generati da MelonLoader.

## Documentazione

[Leggi la guida utente in inglese](../../BopItAccess-user-guide.html) per installazione, aggiornamenti, disinstallazione, comandi, impostazioni, menu e tutte le modalità di gioco. La guida integrata usa automaticamente la lingua corrente del gioco.

## Requisiti

- Windows x64 e una propria installazione Steam acquistata legalmente di Bop It! The Video Game.
- **MelonLoader 0.7.3 Open-Beta**, x64. Lo sviluppo utilizza la build del gioco con Unity 2022.3.50f1.
- Il **runtime .NET 6** per Windows x64, per eseguire la mod.
- Il file ufficiale Windows x64 **Prism v0.18.3** `prism.dll`, installato accanto all'eseguibile del gioco.
- Per compilare la mod dal codice sorgente: un SDK .NET compatibile con il **targeting pack .NET 6** e i riferimenti generati da MelonLoader a partire dal proprio gioco.
- Per compilare il programma di installazione dal codice sorgente: l'**SDK .NET 10** su Windows.

Il programma di installazione ottiene le dipendenze dalle loro fonti ufficiali. Installare una versione compilata non richiede un SDK di sviluppo; Installa alpha lo richiede.

<a id="build-from-source"></a>
## Compilare dal codice sorgente

1. Installa MelonLoader 0.7.3 Open-Beta nella cartella del gioco. Avvia il gioco una volta, attendi che MelonLoader prepari i suoi file e poi chiudilo. I riferimenti generati dovrebbero trovarsi in `MelonLoader\Il2CppAssemblies`, nella cartella del gioco.
2. Scarica o clona questo repository e apri PowerShell nella sua cartella principale.
3. Sostituisci il percorso di esempio qui sotto con quello del tuo gioco, poi esegui:

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

La DLL compilata è `src\bin\Release\net6.0\BopItAccess.dll`. Il progetto segnala eventuali riferimenti al gioco o al loader mancanti prima di compilare. Se l'SDK segnala l'assenza del targeting pack .NET 6, installa un SDK che lo contenga. Il file `NuGet.Config` del progetto non configura fonti di pacchetti online.

Il percorso alpha del programma di installazione prepara i propri riferimenti locali di compilazione senza avviare il gioco. Questi riferimenti sono elementi temporanei necessari alla compilazione; non vengono mai inclusi nei commit o in una versione compilata della mod.

<a id="install-your-build"></a>
## Installare la propria build

Con il gioco chiuso:

1. Copia il file `BopItAccess.dll` compilato nella cartella `Mods` del gioco, creandola se necessario.
2. Scarica Prism v0.18.3 ufficiale per Windows x64 dalle [versioni di Prism](https://github.com/ethindp/prism/releases). Metti `prism.dll` accanto all'eseguibile del gioco.
3. Copia la cartella `src\bin\Release\net6.0\documentation` della build nella cartella del gioco, mantenendo tutte le sottocartelle delle lingue. Conserva i file di licenza Prism pertinenti quando distribuisci il suo binario.
4. Avvia il tuo lettore di schermo, se ne usi uno, poi avvia il gioco tramite Steam. Attendi l'annuncio di avvio e poi quello del titolo, della schermata di benvenuto o del menu principale prima di usare i comandi del gioco.

La compilazione della mod non scarica né compila Prism. Se la voce non parte, consulta `Mods\BopItAccess.log` nella cartella del gioco. Un invio riuscito nel registro conferma che la mod ha inviato testo; non può dimostrare che l'audio sia stato ascoltato.

Per lo ZIP di una versione compilata, copia **tutto il suo contenuto** nella cartella del gioco e unisci le cartelle o sostituisci i file quando richiesto. Lo ZIP include la mod, Prism, le guide e gli avvisi di licenza; MelonLoader e .NET si installano separatamente. Consulta la guida utente per le istruzioni complete di installazione manuale.

## Modificare le impostazioni fuori dal gioco

Dopo l'avvio, `UserData\BopItAccess.ini`, nella cartella del gioco, contiene impostazioni leggibili del gioco e della mod, profili delle voci e assegnazioni dei comandi destinati ai giocatori. Chiudi il gioco, apri il file nel Blocco note, modifica le voci esistenti e salvalo. La mod legge le modifiche al successivo avvio. I commenti spiegano le opzioni e gli intervalli validi.

Per esempio, imposta `Language=en` in `[Game]` per ripristinare l'inglese, riduci `MusicVolume`, `SfxVolume` e `VoiceOverVolume`, oppure imposta `Voice=System default` in `[OneCore]` o `[SAPI]` per sostituire una voce inadatta. Imposta `SpeechOutput=On` e `OutputMode=Auto` in `[Mod]` per ripristinare la voce automatica. Mantieni le altre voci; non aggiungere sezioni duplicate.

## Compilare il programma di installazione

Su Windows con l'SDK .NET 10, esegui:

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

Il risultato è `build\installer\BopItAccess.Installer.exe`. È un eseguibile autonomo per Windows x64, quindi gli utenti non hanno bisogno di .NET 10 per eseguirlo. Il programma di installazione fornito non è firmato. La guida utente illustra i suoi comandi e le richieste di sicurezza di Windows.

`scripts/package-mod.ps1` prepara uno ZIP di pubblicazione da una mod già compilata e della versione corrispondente. Include guide, avvisi e licenze necessari ai giocatori. I README per sviluppatori, le note sul flusso di lavoro Git, i riferimenti generati del gioco e gli installer compilati sono esclusi dallo ZIP. La preparazione del pacchetto non compila la mod e non pubblica una versione su GitHub.

## Nota di trasparenza sull'IA

Questa mod è stata realizzata con il «vibe coding». Tutto il codice è stato generato e ricercato interamente dall'intelligenza artificiale, con una comprensione tecnica umana limitata dell'architettura sottostante. Usa questa mod a tuo rischio.

Detto questo, ogni singola funzionalità e decisione di progettazione della mod è stata ideata e approvata da esseri umani. I test non sono mai stati automatizzati: sono stati eseguiti con cura e in modo approfondito da veri giocatori e tester umani.

Nota: i testi e la documentazione multilingue sono stati generati dall'IA e non sono stati revisionati da madrelingua. Sono da aspettarsi notevoli imprecisioni nelle traduzioni. Senza la programmazione tramite agenti di IA, questo progetto non esisterebbe. Grazie per dargli una possibilità!

## Licenza e note legali

Il codice sorgente proprio di Bop It Access e la sua documentazione sono distribuiti sotto la **[licenza MIT](../../LICENSE)**. Copyright © 2026 Christopher Shaw. Le dipendenze mantengono le proprie licenze; la licenza MIT non le sostituisce e non concede diritti sulle risorse del gioco. Consulta [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) per gli avvisi relativi alle dipendenze.

Bop It Access è un progetto non ufficiale creato da un fan. Non è realizzato, approvato o sostenuto da Hasbro, Alliance, lo sviluppatore/editore del gioco, Valve, Microsoft, Unity, MelonLoader, Prism o alcun produttore di lettori di schermo. **Bop It!** e i relativi personaggi, illustrazioni, suoni e marchi appartengono a Hasbro e ai rispettivi titolari dei diritti. Steam appartiene a Valve. Gli altri nomi di prodotti, marchi e software rimangono proprietà dei rispettivi titolari.

Devi procurarti una tua copia legale del gioco. Questo repository non include il gioco o le sue risorse e non concede alcun diritto su di essi. Per le informazioni sui diritti del gioco originale, consulta il [sito ufficiale di Bop It!](https://bopitthevideogame.com/) e la [pagina Steam](https://store.steampowered.com/app/3214360/).

## Grazie

A chi ha provato questa mod prima del rilascio e ha contribuito a portarla fin qui: grazie. Sapete chi siete. Ai giocatori che offrono riscontri, provano la mod per la prima volta o credono in me e in questo progetto: grazie. Il vostro sostegno mi motiva a continuare a creare in questo mondo folle in cui viviamo. Spero che questo progetto vi aiuti a godervi il gioco e a giocare con gli altri. Grazie di cuore a tutti. Buon divertimento con Bop It!

— Christopher Shaw
