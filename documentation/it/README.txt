Bop It Access 0.9.12 - Prism Voce e Braille

Build sorgente attuali: mod 0.9.12 (59 build del mod), installer 0.2.6. La prima pubblicazione pubblica è ancora futura.

Cosa fa questo
--------------
La mod segue le Impostazioni del gioco > Selezione della lingua per il parlato. Include
inglese, francese, italiano, tedesco, spagnolo (Spagna), spagnolo (America Latina),
Giapponese, coreano, cinese semplificato e portoghese brasiliano. Modificando il
la lingua del gioco cambia anche gli annunci delle mod e la guida dell'utente nel gioco.
Le traduzioni iniziali sono bozze generate automaticamente e necessitano di revisione
da parlanti fluenti.
La sintesi vocale è attiva per impostazione predefinita. Quando la mod si carica con la voce attiva, annuncia
"Il discorso Bop It Access è pronto. Il gioco è ancora in caricamento. Attendi l'annuncio della schermata del titolo o del menu principale prima di utilizzare i controlli." fino a Prism. Se viene visualizzata la schermata del titolo, la mod annuncia l'attuale input COLPISCI per l'apertura del menu principale. Si legge
il pulsante del menu principale con focus e la riga Impostazioni con focus. I valori delle impostazioni sono
pronunciato con il nome della riga in evidenza. Modificare un valore mentre il focus rimane su quello
riga pronuncia solo il nuovo valore. LATENZA AUDIO, CONTROLLI e VAI ONLINE sono azioni
pulsanti, quindi vengono pronunciati senza il segnaposto insignificante del gioco "0".
Il menu Impostazioni ha anche un cursore LIMITE FPS con 30, 60, 120, 240 e
Scelte ILLIMITATE. Si comincia a 60 per una nuova installazione e si ricorda il
valore selezionato tra le sessioni. Focus parla del nome e del valore; cambiandolo
parla solo del nuovo valore. Il limite modifica il frame rate target di Unity mentre
lasciando intatta la scala temporale del gioco, i tempi di aggiornamento fissi e l'audio.
Come ogni limite di frame, un'impostazione più bassa significa anche meno poll di input basati su frame.
Se 30 FPS ti sembrano meno reattivi in un gioco veloce, scegli 60, 120 o ILLIMITATO.
L'interruttore MUTE AUDIO IN BACKGROUND appare direttamente sotto VOICE OVER in
Impostazioni. Se abilitato, disattiva l'audio del gioco mentre la finestra di gioco non lo è
focalizzato, quindi ripristina lo stato audio del gioco precedente quando ritorna il focus.
Si avvia e viene salvato tra le sessioni.
Al primo utilizzo in assoluto della mod, della MUSICA nativa del gioco, degli effetti sonori e della VOCE OVER
i cursori iniziano da 30. L'aggiornamento mantiene le impostazioni audio del gioco salvate in precedenza.
Una volta che il gioco raggiunge il menu principale per la prima volta, viene visualizzata una schermata di benvenuto
messa a fuoco. Il suo messaggio può essere focalizzato nuovamente con Up, e le sue scelte aprono Mod
Impostazioni, apri la guida dell'utente all'interno del gioco o continua al menu principale.
La schermata di benvenuto viene contrassegnata come completata solo dopo che la scelta è stata completata con successo
esso. Chiudendo il gioco mentre è aperto lo si lascia pronto per il lancio successivo.
L'indicizzazione delle impostazioni ora attende prima le righe audio, FPS e IMPOSTAZIONI MOD del mod
annunciando il primo elemento focalizzato su una schermata Impostazioni appena aperta.

Sotto Controlli, Impostazioni ora ha un menu IMPOSTAZIONI MOD. SPEECH OUTPUT utilizza lo stesso
interruttore principale salvato come F8 o selezione del controller, incluso il ripristino parlato
istruzioni quando la voce è disattivata. L'USCITA BRAILLE si avvia e viene salvata
tra le sessioni. Prism invia annunci a uno screen reader compatibile
output braille quando questa impostazione è attiva. Disattivandolo si interrompe il braille della mod
messaggi lasciando disponibile la voce.
OUTPUT MODE: Auto usa uno screen reader compatibile in esecuzione, poi OneCore e infine SAPI.
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Ogni screen reader e motore vocale è disponibile solo se supportato dalla versione di Prism installata e dal sistema del giocatore. Se la modalità scelta non è disponibile, la mod usa un’uscita disponibile e lo comunica una sola volta.
Le voci SAPI salvate vengono cercate tramite il nome mostrato da Prism; se più voci hanno lo stesso nome, può essere scelta la prima.
MUTE PARLATO IN SFONDO è un'opzione salvata, disattivata per impostazione predefinita. Quando abilitato,
la mod smette di parlare non appena il gioco perde il focus della finestra. Discorso creato
mentre il gioco è in background viene scartato e gli annunci riprendono
con una nuova attività dopo il ritorno dello stato attivo. Se il parlato stesso è disattivato durante il gioco
riacquista la messa a fuoco, la mod ripristina la tastiera e il controller correnti
istruzioni una volta.

Il menu IMPOSTAZIONI MOD dispone anche di Leggi le posizioni nei menu, attivata per impostazione predefinita e salvata tra le sessioni.
Quando abilitata, una voce di menu evidenziata include la sua posizione, ad esempio "PLAY, 1 di 6".
Questo vale per i menu principale e Impostazioni, Controlli, modalità di gioco, Mod
Impostazioni, scelte di game-over, classifiche, risultati, crediti e altro
schermi supportati. Il conteggio segue le scelte attualmente disponibili. Cambiare
un cursore o un interruttore mentre rimane focalizzato annuncia ancora solo il nuovo valore.

FORMATTA IL PARLATO è un’opzione salvata nelle Impostazioni mod, attiva per impostazione predefinita. Rende naturale l’uso delle maiuscole nei nomi dei menu tutti maiuscoli, conservando le parole a maiuscole e minuscole e le abbreviazioni come SAPI, NVDA, SFX e FPS. Nella guida del gioco, quando viene letto il numero di riga e il testo termina senza punteggiatura, aggiunge tre puntini per una pausa prima del numero. La punteggiatura esistente viene conservata. Cambia solo il testo inviato alla voce e al braille; il testo visibile del gioco e la guida rimangono invariati. Disattivandola si disattivano entrambe le modifiche. La precedente preferenza per il filtro delle maiuscole viene mantenuta.

ANNUNCIA TIPI DI CONTROLLO è un'altra opzione salvata per le IMPOSTAZIONI MOD, attivata per impostazione predefinita. Quando abilitato,
il tipo dell'elemento evidenziato segue il nome e precede il valore e l'indice:
"Slider MUSICA, 30, 1 di 12", "Commutazione VIBRAZIONE, On, 6 di 12", o
"Pulsante PLAY, 1 di 6". I menu identificano anche schede, campi di testo e leggibili
elencare gli elementi ove pertinenti. Le modifiche al valore continuano a parlare solo del nuovo valore.
INTERVENTI SLIDER è un interruttore salvato, disattivato per impostazione predefinita. Quando abilitati, cursori focalizzati
riporta anche i loro endpoint disponibili dopo il valore corrente, come ad esempio
"Slider MUSIC, 30, intervallo da 0 a 100, 1 di 12" durante l'indicizzazione e i tipi di controllo
sono abilitati. Lo spostamento di un cursore continua a indicare solo il nuovo valore.
Il feedback uno a uno viene salvato ed è attivo per impostazione predefinita. Annuncia il colore attivo all’inizio e quando cambia. Quando si perde una vita, annuncia il numero di vite rimaste. Quando si guadagna una vita, annuncia il colore del giocatore e il nuovo totale, per esempio “Verde, 3 vite”, così entrambi sanno chi è stato più veloce. Questi annunci sono disattivati quando il feedback uno a uno è disattivato. Il limite di tre vite del gioco resta invariato. Questa funzione opera solo durante una partita uno contro uno.

TIPO DI SUGGERIMENTO ora appare sopra SUGGERIMENTI PULSANTE AUTO-SPEAK nel menu Impostazioni mod.
I SUGGERIMENTI PER IL PULSANTE DI PARLAZIONE AUTOMATICA sono un interruttore salvato ed è attivato per impostazione predefinita. Girandolo
Disattivato sopprime i suggerimenti automatici, mentre PARLARE SUGGERIMENTI rimane disponibile su richiesta.
SUGGERIMENTI PULSANTI RITARDO ha Nessuno, 5 secondi
(Può interrompere il parlato), 10 secondi, 15 secondi, 30 secondi e 60 secondi.
Il valore predefinito è 10 secondi. Con Nessuno, gli input validi per la schermata corrente e
le loro azioni sono incluse nella stringa vocale ordinaria dell'oggetto focalizzato,
dopo un punto fermo. L'azione per il controllo focalizzato viene pronunciata prima del generale
navigazione nel menù. Non esiste un annuncio separato del primo suggerimento. Con un cronometraggio
ritardo, il primo annuncio di suggerimento segue tanta inattività. L'opzione di 5 secondi può
interrompere il discorso già in corso; ritardi più lunghi si mettono in fila dietro di esso. Dopo
dopo l'annuncio del primo suggerimento, un altro ritardo inizia solo quando il giocatore dà
input, a meno che le ripetizioni non siano abilitate. Passare a un altro elemento focalizzato o modificare
un cursore o un interruttore focalizzato conta come input e riavvia il ritardo, anche se il
la scansione del collegamento dell'input del gioco non rileva l'azione del tasto o del controller. Una chiave inutilizzata
ciò non modifica l'interfaccia utente e continua a non riavviarla.
TIPO DI SUGGERIMENTI è uno slider salvato con Automatico, Tastiera, Controller ed Entrambi.
Automatico è l'impostazione predefinita e segue la tastiera o la tastiera utilizzata più di recente
ingresso del controllore. L'uso del mouse conta come la tastiera. Tastiera e controller parlano
solo i suggerimenti di quel dispositivo; Entrambi forniscono entrambi i set di input con dispositivo esplicito
nomi. Il ripristino della voce disattivata include sempre entrambi i dispositivi, quindi il
il giocatore può trovare il controllo che riattiva la voce.

I suggerimenti sui pulsanti mettono l'input prima della sua azione: "Inserisci o Space, attiva l'elemento".
I suggerimenti per un singolo dispositivo omettono il nome del dispositivo. I nomi degli stick del controller vengono pronunciati
per intero, come "Levetta sinistra su e giù". Entrambe le modalità identificano la tastiera
e ingressi del controller. La mod legge i collegamenti attuali del gioco in modo nativo
cambiamenti ricolleganti si riflettono in questi suggerimenti. Quando nessun controller lo è
connesso e il gioco ha nomi di pulsanti frontali diversi su controller diversi
tipi, il suggerimento utilizza il "pulsante conferma" o il "pulsante indietro" anziché presupporre un
Disposizione dell'Xbox. Le righe del punteggio della classifica utilizzano Pagina su e Pagina giù sulla tastiera.
Il controller su/giù legge le righe solo quando nessun controllo della classifica è attivo;
i suggerimenti riportano solo i controlli disponibili per il TIPO DI CONSIGLI selezionato. Ordinario
i suggerimenti sullo schermo includono anche le attuali associazioni SPEAK HINTS e TOGGLE SPEECH.
Entrambi vengono ricercati centralmente, in modo che i futuri controlli globali possano unirsi allo stesso
elenco dei suggerimenti senza modificare ogni schermata separatamente.

RIPETI SUGGERIMENTI PULSANTE è uno slider salvato separato: Off, 2x, 3x, 4x, 5x o
Infinitamente. L'impostazione predefinita è Infinitamente. Il numero rappresenta le letture totali in uno
ciclo: 2x significa il primo suggerimento e una ripetizione; 3x significa il primo suggerimento e
due ripetizioni. Disattivato consente comunque il primo suggerimento automatico o manuale.
REPEAT INTERVAL imposta il ritardo tra le ripetizioni su 15, 30, 45 o
60 secondi e il valore predefinito è 30 secondi. Con RITARDO SUGGERIMENTI PULSANTE impostato su Nessuno,
il timer di ripetizione inizia immediatamente dopo l'immissione. Input, un focus o un valore
modifica oppure un cambio di schermata riavvia il ciclo di suggerimenti per la schermata corrente.
PARLARE SUGGERIMENTI sostituisce il
in attesa del suggerimento automatico per quel ciclo, quindi utilizza l'INTERVALLO DI RIPETIZIONE per qualsiasi
ripetizioni configurate. Funziona anche con SUGGERIMENTI PULSANTE PARLAZIONE AUTOMATICA disattivato.
I suggerimenti vengono soppressi durante
gameplay attivo e fasi di timing della calibrazione audio, dove extra
il discorso potrebbe mascherare un segnale. Un promemoria salvato esistente di 15, 30 o 60 secondi
il ritardo dalla versione 0.6.2 diventa il nuovo valore BUTTON HINTS DELAY.

MOD SETTINGS: Voce, Volume, Velocità e Tono regolano l’uscita OneCore o SAPI effettivamente in uso, anche in modalità Auto. Appaiono solo i controlli supportati; con le altre uscite vengono nascosti. Ogni motore conserva le proprie impostazioni separatamente. Volume: dal 5% al 100% a passi di 5, inizialmente 100%. Velocità e tono: da 0 a 100 a passi di 5, inizialmente 50. Il volume minimo mantiene udibili gli avvisi di ripristino.
Le scelte delle impostazioni del mod vengono ricordate tra una sessione e l'altra. RIPRISTINA MODALITÀ PREDEFINITE
riporta quelle scelte ai valori predefiniti sopra descritti. Premerlo una volta per richiedere
conferma, quindi premerlo nuovamente entro cinque secondi per ripristinarli. In movimento
ad un'altra riga o lasciando passare cinque secondi annulla la richiesta. Questo no
cambia i cursori MUSICA, SFX o VOICE OVER del gioco, LIMITE FPS o personalizzato
associazioni di tastiera e controller. Indietro torna a Impostazioni.
OPEN USER'S GUIDE legge la guida HTML per la lingua attualmente selezionata
nella riga Impostazioni > Lingua del gioco. La guida inglese è a
documentation\BopItAccess-user-guide.html; le guide tradotte sono in lingua
sottocartelle. Se una copia tradotta manca o è illeggibile, la guida in inglese
si apre invece. L'elenco degli argomenti proviene dal sommario del documento e
si ricarica ogni volta che viene aperto. Conferma apre un argomento. Up and Down ne leggono le righe. Nelle tabelle,
Sinistra sposta una colonna a sinistra e Destra sposta una colonna a destra; Continua su e giù
la colonna corrente quando ci si sposta tra le righe. Le intestazioni delle colonne etichettano le celle
invece di apparire come righe di dati. La tabella viene annunciata una volta all'ingresso, e
la sua fine viene annunciata all'uscita. Indietro torna agli argomenti o abbandona la guida.
Durante la lettura, la mod applica il parametro Filtro menu-musica del gioco e
ripristina il valore precedente all'uscita. RESET SCHERMATA DI BENVENUTO richiede a
premendo una seconda volta entro cinque secondi, verrà visualizzata la schermata di benvenuto su
prossimo lancio del gioco. La modifica delle righe o l'attesa di cinque secondi annulla la conferma.
Questo aggiornamento ripristina il layout della riga delle impostazioni native del gioco, quindi su/giù
la navigazione rimane sulle righe Impostazioni dopo l'aggiunta di IMPOSTAZIONI MOD.
Inoltre avvia il sottomenu MOD SETTINGS su SPEECH OUTPUT ogni volta che si apre,
impedendo a una riga INDIETRO precedentemente selezionata di chiudere immediatamente il menu
quando Invio viene utilizzato per riaprirlo.
L'input che apre MOD SETTINGS viene ora ignorato dalle sue righe finché non lo è quell'input
rilasciato, quindi la riapertura del menu non può anche disattivare la sintesi vocale. I mod
aggiunti Controlla anche le righe di associazione in attesa dell'input di apertura
rilasciato prima di accettare una richiesta di riammissione.

All'interno di Play, la mod legge Solo, Party, Pass It e One on One quando è focalizzata.
Nella schermata di selezione del brano, il mod annuncia il tema corrente
(Shapes, Space, City o Office) e la difficoltà: Classico o Estremo. RUOTA
cambia il brano e annuncia solo il nuovo tema. TIRA cambia la difficoltà
e annuncia solo Classico o Estremo. L’introduzione spiega anche RUOTA,
TIRA, COLPISCI e Indietro. Si ripete ogni volta che si sceglie una modalità
e si apre la schermata del brano, riportando il tema e la difficoltà
correnti.
Premi PARLARE SUGGERIMENTI (H o premi la levetta destra per impostazione predefinita) su questa schermata per ascoltare
la modalità corrente e il testo tutorial nativo della difficoltà prima di iniziare. Ciascuno
l'azione denominata in quel riferimento include la tastiera attualmente assegnata o
controllo del controller, seguendo TIPO DI CONSIGLI. Vengono letti i controlli riassegnati
gli attacchi del giocatore attivo; Uno contro uno nomina gli input COLPISCI di entrambi i giocatori. Il
la sovrapposizione del tutorial temporizzato durante la riproduzione attiva rimane silenziosa, quindi non può oscurarsi
i comandi vocali del gioco. Il suggerimento annuncia questo utilizzo aggiuntivo di SPEAK HINTS.
Il controllo LEGGI DESCRIZIONI pronuncia una descrizione visiva dell'oggetto selezionato
Shapes, Space, City o Office fase su richiesta. È disponibile in questa schermata
solo prima dell'inizio del gioco. I suoi input predefiniti sono G sulla tastiera e LT
(grilletto sinistro) sul controller. La R è stata sostituita perché è il reset del gioco
Scorciatoia giroscopica. L'introduzione sullo schermo annuncia l'associazione corrente.
L'avvio della riproduzione interrompe qualsiasi discorso rimasto dalla selezione del brano in modo che non possa mascherare il
segnali verbali del gioco. Le descrizioni iniziano con i dettagli della scena anziché con
ripetendo il nome d'arte selezionato.

Nella schermata dei risultati finali, Solo, Party e Passa annunciano il punteggio finale
prima del discorso del menu. Solo legge i pulsanti Replay e Classifica focalizzati
e spiega Indietro. Le altre modalità leggono le opzioni Continua, Replay e
Indietro richiede. Uno contro uno mostra un vincitore anziché un punteggio finale numerico, quindi
la mod annuncia il vincitore mostrato lì. Il discorso sul punteggio ha la priorità
l'annuncio del menu iniziale; i prompt della schermata dei risultati vengono accodati dopo di esso.
I successivi cambiamenti del focus del menu si interrompono a vicenda. Cambiamenti rapidi immediatamente
dopo la fine della partita vengono combinati fino a quando non ha avuto tempo l'annuncio del punteggio breve
per finire, mantenendo l'ultima scelta di menu mirata.
Gli annunci del punteggio più alto in singolo e del grado del gruppo vengono annunciati quando viene segnalato il gioco
un nuovo risultato in classifica.
LEGGI PUNTEGGIO ripete il risultato finale su richiesta solo durante il risultato del game over
lo schermo è visibile. Gli input predefiniti sono T sulla tastiera e pressione della levetta sinistra
controllore. Solo, Party e Pass It ripetono il loro punteggio finale; Uno contro uno
ripete il vincitore mostrato dal gioco. L'azione è disabilitata durante il gioco,
selezione dei brani e tutte le altre schermate. RT (grilletto destro) non è stato utilizzato come
predefinito perché il gioco lo associa già a Reset Gyro e Auto Play.
Le ripetizioni richieste parlano immediatamente e possono essere interrotte dal menu dei risultati
navigazione. Solo l'annuncio automatico del risultato ritarda il menu iniziale
discorso in modo che la partitura venga ascoltata per prima.
Il FEEDBACK UNO A UNO è attivo per impostazione predefinita in Impostazioni > Impostazioni Mod per la voce attiva
colore e vite rimanenti durante quella modalità. I segnali condivisi COLPISCI non passano
stessi identificano un colore, quindi la mod mantiene l'ultimo colore definito.

TOGGLE SPEECH attiva o disattiva tutta la normale voce mod da qualsiasi schermata. Suo
le impostazioni predefinite sono F8 sulla tastiera e Seleziona sul controller. Quando è spento, il mod
interrompe la conversazione corrente e annuncia che la conversazione è disattivata, insieme alla conversazione corrente
controlli della tastiera e del controller per riaccenderlo. Lo stato spento è
salvati tra le sessioni di gioco. Se il gioco inizia con la voce disattivata, la mod dà
quelle istruzioni di ripristino invece del solito messaggio di caricamento. Girando
il discorso riattivato annuncia "Discorso attivato". Gli altri dialoghi mod rimangono silenziosi mentre sono spenti.
Il controllo Attiva/Disattiva voce rimane attivo anche quando la voce è disattivata.

Le classifiche raggiunte dal menu principale, dai risultati in singolo e dai risultati del gruppo
annunciare il brano, il dispositivo, il gruppo e la data selezionati, ove disponibili. Leggono
grado, nome del giocatore e punteggio, inclusi gli stati di caricamento e di risultato vuoto. Pagina su
e Pagina giù leggono le singole righe del punteggio anche quando è attivo un filtro. Nativo
controlli mirati come Locale, Amici, Globale, Oggi, Questo mese, Tutto,
Vengono pronunciati Indietro e Continua. La classifica del Partito riporta anche il suo nome
stato di selezione e conferma.

Questo aggiornamento mantiene silenziose le classifiche dei risultati durante la selezione della modalità e del brano.
I nomi dei filtri della classifica parlano per primi, seguiti dai riepiloghi dei punteggi.
Il pulsante del menu degli obiettivi parla normalmente; prenotare le istruzioni attendere fino a
le sue pagine vengono effettivamente aperte e solo dopo viene annunciata la chiusura.

Il libro degli obiettivi del gioco annuncia la sua pagina visibile e ogni obiettivo
nome, descrizione e stato bloccato o sbloccato. Utilizzare Su e Giù per leggere gli elementi
su una pagina. I controlli Sinistra e Destra del gioco girano le pagine come al solito.

I crediti annunciano la prima riga quando viene aperta. Utilizza il menu Su e Giù del gioco
controlli per la lettura di ciascuna linea di credito. Lo scorrimento automatico visivo continua come
prima. Se questi controlli non sono disponibili, le linee vengono accodate così come appaiono;
se anche questo non funziona, l'intero testo dei titoli di coda viene annunciato una volta.

All'interno dei controlli, la mod recita COLPISCI, COLPISCI (Giocatore 2), SCUOTI, RUOTA, GIRA, TIRA,
e Ripristina impostazioni predefinite. Legge l'associazione corrente per il dispositivo di input attivo,
annuncia i collegamenti modificati e legge il feedback visibile del ricollegamento del gioco.
Annuncia come tornare alle Impostazioni una volta per visita. Quando il ripristino alle impostazioni predefinite cambia a
associazione, segnala che le associazioni sono state ripristinate.

La mod espone una riga RESET GYRO nativa e aggiunge un CHANGE SPEECH OUTPUT
scorciatoia. Reset Gyro si trova con i controlli del gioco; le righe specifiche della mod
rimangono insieme nella parte inferiore del menu, prima di Ripristina impostazioni predefinite. CAMBIARE
SPEECH OUTPUT scorre attraverso le stesse modalità di Impostazioni > Impostazioni Mod > USCITA
MODALITÀ. L'ordine delle modalità è:
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Gli input predefiniti della scorciatoia sono F9 sulla tastiera e il pulsante Fronte ovest (X su Xbox
controllore). Il pulsante Start del controller è riservato al nativo del gioco
Azione del menu. La scelta corrente viene annunciata quando viene utilizzata la scorciatoia e
viene salvato dalla stessa impostazione della modalità di output.

La mod aggiunge nove righe al menu Controlli del gioco: Gruppo Precedente,
Gruppo successivo, Data precedente, Data successiva, LEGGI DESCRIZIONI, LEGGI PUNTEGGIO,
ALTERNA LA PAROLA, PRONUNCIA SUGGERIMENTI e CAMBIA L'USCITA DELLA PAROLA. I primi quattro
affrontare i filtri della classifica
raggiunto con O/P e K/L sul layout di tastiera predefinito o con i pulsanti e
D-pad sinistro/destro su un controller. Metti a fuoco una riga per ascoltarne la rilegatura corrente,
quindi usa la normale azione COLPISCI/confirm del gioco per ricollegarlo. Le righe scorrono
all'interno del pannello Controlli esistente. LEGGERE LE DESCRIZIONI può anche essere rimbalzato
tastiera e controller. Il suo legame viene salvato dal mod e ripristinato su predefinito
ripristina G e LT. READ SCORE può anche essere rimbalzato per tastiera e controller;
Ripristina impostazioni predefinite ripristina T e la pressione della levetta sinistra. La rilegatura originale del gioco
le righe e le quattro righe della classifica utilizzano lo stesso flusso di riassociazione dei controlli.
TOGGLE SPEECH può essere rimbalzato per tastiera e controller. I suoi legami sono
salvato dal mod e Ripristina impostazioni predefinite ripristina F8 e Seleziona. Se il legame
viene modificato mentre la voce è disattivata, la mod annuncia i nuovi controlli di ripristino.
I SUGGERIMENTI PARLARE possono anche essere rimbalzati. Le sue impostazioni predefinite sono H e premi la levetta destra.
Pronuncia immediatamente il suggerimento della schermata corrente senza programmare un secondo
primo suggerimento automatico. Le ripetizioni configurate possono ancora seguire. È silenzioso
durante il gioco e i segnali di calibrazione audio temporizzati.
CAMBIA USCITA DISCORSO può essere rimbalzato per tastiera e controller; Ripristina a
L'impostazione predefinita ripristina F9 e il pulsante Fronte ovest. Se è già presente una nuova associazione
assegnato a un altro gioco o azione mod, il menu Controlli rifiuta l'
duplica e mantiene l'assegnazione precedente. RESET GYRO può essere rimbalzato
la stessa procedura dei Comandi nativi delle altre azioni di gioco.
Il ritorno dalla selezione del brano al menu principale ripristina anche i suggerimenti del menu principale
quando un gestore di gioco memorizzato nella cache riporta ancora un vecchio stato di gioco. Suggerimento sul pulsante
i timer e la selezione automatica del dispositivo di suggerimento seguono il gioco e la mod assegnati
controlli; i tasti non utilizzati, come un tasto Control non assegnato, non li ripristinano.
Questo aggiornamento impedisce alla riga Controlli obsoleta di annunciare "Riassociazione non riuscita"
ripetutamente dopo la chiusura della scena. Leggi le descrizioni non più temporaneamente
sovrascrive qualsiasi associazione di input del gioco.

All'interno di Calibrazione audio, la mod legge i controlli Calibra, Indietro e COLPISCI,
annuncia le istruzioni e le fasi di calibrazione, legge il conto alla rovescia del riscaldamento,
e annuncia il risultato della latenza visualizzato. Non parla ogni battito durante
l'esercizio di cronometraggio in modo che il ritmo rimanga udibile. Dopo l’ultimo input di calibrazione, il mod dice subito “Fatto!”. Smetti di colpire e attendi il risultato misurato. Se la calibrazione fallisce perché non è stato dato alcun input, dice “Calibrazione fallita.”.

La schermata di pausa annuncia Pausa, il pulsante Riprendi o Menu principale selezionato e i relativi suggerimenti. I cambiamenti di selezione interrompono la voce precedente del menu di pausa. Riprendere o lasciare la partita interrompe la voce rimasta prima di continuare il gioco o il menu principale. Durante una partita, la mod lascia invariati i comandi vocali del gioco e il punteggio in corso.

Installa
-------
Il repository di origine non contiene mod compilati o DLL Prism. Costruisci la mod tramite
seguendo README.md, quindi chiudi il gioco e copia BopItAccess.dll nei suoi Mod
cartella. Ottieni il numero ufficiale Windows x64 Prism v0.18.3 prism.dll da
https://github.com/ethindp/prism/releases e posizionalo accanto all'eseguibile del gioco,
non all'interno dei Mod. Copia la cartella della documentazione della build nella cartella del gioco,
comprese le sottocartelle della lingua tradotta. Avvia il tuo screen reader se tu
utilizzarne uno, quindi avviare da Bop It! a Steam. Prism può utilizzare SAPI quando è supportato
lo screen reader non è in esecuzione. La guida in-game carica l'HTML dal file
cartella della documentazione ogni volta che si apre. Questa mod è stata sviluppata per MelonLoader
0.7.3 Open-Beta e Bop It! (Unity 2022.3.50f1, x64).
Le prime traduzioni non inglesi sono state effettuate con la traduzione automatica
e necessitano di revisione da parte di parlanti fluenti. Si prega di segnalare diciture poco chiare o errate.
I nomi delle azioni di gioco utilizzano i termini tradotti del gioco. Shapes, Space, City,
e Office rimangono inglesi come titoli di scena fissi. Se la voce di sistema di SAPI lo fa
non pronuncia bene la tua lingua, seleziona una voce installata adatta nel Mod
Impostazioni.

Provare i menu e le schermate
-------------------------
Attendi l'annuncio nella schermata del titolo, se appare, quindi utilizza COLPISCI per aprire il
menù principale. Il gioco potrebbe richiedere diversi secondi dopo il messaggio di pronto della mod
accettare questo input.
Al primo avvio, viene visualizzata la schermata di benvenuto prima del menu principale. Selezionalo
messaggio per ascoltare di nuovo l'introduzione. Scegli Apri impostazioni mod, Leggi utente
Guida o Continua al gioco. Speak Hints nomina la sua tastiera attuale e
assegnazioni dei controller nel messaggio di benvenuto indipendentemente dal tipo di suggerimenti.
Apri Play e spostati tra le quattro modalità. Scegline uno per raggiungere la selezione del brano.
RUOTA per scorrere i temi e TIRA per scegliere Classico o Estremo. Il
mod annuncia ogni modifica. Premi G o LT per ascoltare il livello attualmente selezionato
descrizione. Premi H o premi la levetta destra per ascoltare il testo tutorial della modalità,
controlli di azione attualmente assegnati e suggerimenti sui pulsanti. COLPISCI avvia la modalità scelta;
Ritorni indietro. Durante un round, usa
il controllo Menu del gioco per aprire Pausa, quindi spostarsi tra Riprendi e Menu principale.
Alla fine di una partita, ascolta il punteggio o il vincitore uno contro uno prima del
vengono annunciati i controlli della schermata dei risultati. Premi T o premi la levetta sinistra per ripetere l'azione
risultato finale mentre è visibile la schermata di fine partita.
Premi F8 o il controller Seleziona per attivare o disattivare la sintesi vocale da qualsiasi schermata.
Premi F9 o il controller Ovest (X su un controller Xbox) per scorrere il discorso
modalità di uscita. La stessa scelta è disponibile in Impostazioni > Impostazioni Mod > MODALITÀ DI USCITA.
L'USCITA BRAILLE in Impostazioni > Impostazioni Mod è attivata per impostazione predefinita. Visualizzatore Braille di NVDA
può visualizzare il braille e il suo equivalente testuale senza un display fisico.
Per un confronto ON/OFF, utilizzare la modalità braille segui-cursore di NVDA con Mostra
Messaggi abilitati; la sua modalità di visualizzazione-produzione vocale rispecchierebbe anche il parlato
quando l'impostazione USCITA BRAILLE del mod è disattivata.
In Impostazioni > Impostazioni Mod, usa i SUGGERIMENTI PULSANTE AUTO-SPEAK per abilitare o disabilitare
istruzioni automatiche. Premi H o premi la levetta destra per ascoltare il suggerimento attuale
su richiesta.
TIPO SUGGERIMENTI sceglie Automatico, Tastiera, Controller o Entrambi per i suggerimenti.
PULSANTE SUGGERIMENTI RITARDO sceglie se accompagnano il discorso focalizzato o seguono a
periodo di inattività. SUGGERIMENTI PULSANTE RIPETI e INTERVALLO RIPETIZIONE controllano qualsiasi
ulteriori promemoria.
Apri le classifiche dal menu principale o dalla schermata dei risultati. Cambia un filtro in
ascolta la sua nuova selezione e leggi i singoli punteggi con Pagina su e Pagina giù.
Per impostazione predefinita, gli O/P si spostano tra i gruppi della classifica e i K/L si spostano tra le date
intervalli. Gli ingressi del controller corrispondenti sono paraurti sinistro/destro e
D-pad sinistra/destra. Le quattro nuove righe di controlli hanno lo scopo di riassegnarli.
Apri Obiettivi e gira le pagine con Sinistra e Destra; utilizzare Su e Giù per ciascuno
ingresso. Apri Crediti e usa Su e Giù per leggere le sue righe indipendentemente dal
scorrimento visivo.

Se manca la voce, controlla Mods\BopItAccess.log nella cartella del gioco. Registra
rilevamento del pannello, oggetti dell'interfaccia utente selezionati e inizializzazione e invio Prism.
Il successo dell'invio non dimostra di per sé che il discorso fosse udibile.

Per disabilitare la mod, rimuovere Mods\BopItAccess.dll. MelonLoader può rimanere installato.

File e avvisi di terze parti
-----------------------------
Prism è una libreria di accessibilità open source di Ethan Dupuy e collaboratori.
È concesso in licenza sotto la Mozilla Public License, versione 2.0. Questa fonte
il repository non include prism.dll. Fonte, versioni e licenza:
https://github.com/ethindp/prism
Vedere THIRD-PARTY-NOTICES.txt per gli attuali avvisi sulle dipendenze.

Modificare il file delle impostazioni
-------------------------------------
Se una lingua sconosciuta, l’audio troppo forte o una voce problematica rendono difficili da usare i menu, puoi cambiare le impostazioni fuori dal gioco. Dopo l’avvio, il mod crea automaticamente UserData/BopItAccess.ini nella cartella di Bop It!, usando le impostazioni attuali. È un file di testo che puoi aprire con un editor come Blocco note.

Il file include lingua, volumi di musica, effetti e voce, vibrazione, schermo intero, risoluzione e latenza audio del gioco; preferenze di sintesi vocale, braille, suggerimenti e altro del mod; profili vocali separati per OneCore e SAPI; e assegnazioni dei comandi del gioco e del mod destinati ai giocatori. Risoluzioni disponibili e voci installate sono elencate nei commenti.

Chiudi il gioco prima di modificare il file. Trova la sezione interessata e cambia il valore della voce già presente, salva il file e riavvia il gioco. Le modifiche vengono lette all’avvio, non immediatamente durante la partita. Le modifiche fatte nei menu aggiornano automaticamente il file.

I nomi delle sezioni e delle impostazioni restano in inglese in tutte le lingue. On e Off sono i valori consigliati per le opzioni attivabili; sono accettati anche True/False, Yes/No e 1/0. I commenti spiegano le scelte e gli intervalli. Le voci mancanti o non valide lasciano invariata l’impostazione salvata corrispondente, mentre le altre modifiche valide vengono applicate. Le assegnazioni duplicate dei comandi vengono rifiutate.

I commenti e le voci sconosciute vengono conservati. Se un altro programma modifica il file mentre il gioco è aperto, il mod smette di salvarlo per il resto della sessione, per proteggere le modifiche. Chiudi e riapri il gioco per applicarle. Puoi conservare una copia di sicurezza prima di cambiare il file.

Per un problema di voce, imposta Voice=System default nella sezione OneCore o SAPI. Le voci OneCore usano nome | lingua; SAPI accetta il nome visualizzato di una voce installata o il suo identificatore completo nel Registro. Il file elenca le scelte disponibili. OutputMode=Auto prova un lettore di schermo compatibile attivo, poi OneCore e infine SAPI.

L’esempio seguente ripristina l’inglese, un audio di gioco più basso e l’uscita vocale automatica con le voci predefinite del sistema. Modifica le voci corrispondenti già presenti nel file: è un estratto di riferimento, non un altro blocco da aggiungere. Mantieni le altre impostazioni.

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

Dopo aver confermato Uninstall, scegli Uninstall for me oppure Uninstall for everyone. Entrambe le opzioni rimuovono i file condivisi della mod da questa cartella del gioco, quindi la mod non sarà più disponibile per nessuno che usi quell’installazione. La scelta determina di chi vengono rimosse le preferenze Windows salvate della mod: solo dell’account che ha richiesto l’operazione, oppure di tutti i profili Windows locali, compresi quelli con sessione disconnessa. Le preferenze del gioco originale vengono conservate. L’SDK .NET rimane installato.

Quando il programma di installazione rimuove la propria installazione di MelonLoader e nessun’altra mod ne ha bisogno, rimuove anche i file noti Loader.cfg e MelonPreferences.cfg e le cartelle Plugins, UserLibs e UserData se vuote. Vengono rimossi le impostazioni di Bop It Access, i registri noti, le guide e i file del programma di installazione. Le altre mod, i file condivisi del loader già presenti e i file non riconosciuti vengono protetti. Questo significa anche che un file sconosciuto può lasciare una cartella sul disco; il programma di installazione lo segnala nella diagnostica invece di eliminare dati estranei.

Se la pulizia non può essere completata in sicurezza, il programma di installazione lo spiega e conserva le informazioni necessarie per riprovare. Per un’installazione gestita, la voce di disinstallazione di Windows e il punto di ripresa della pulizia rimangono finché la rimozione non riesce. Una vecchia copia manuale non ha un registro persistente di proprietà dei file; nel programma di installazione ancora aperto, riprova le operazioni segnalate dagli avvisi. Non installare, aggiornare o rimuovere la mod mentre Bop It! è in esecuzione.

Nota sulla trasparenza dell'IA
--------------------

Questa mod è stata realizzata con il « vibe coding ». Tutto il codice è stato completamente generato e ricercato dall'intelligenza artificiale, con una comprensione umana limitata della sua architettura sottostante. Si prega di utilizzare questa mod a proprio rischio.

Detto questo, ogni singola funzionalità della mod e decisione progettuale è stata creata e approvata da esseri umani. I test non sono mai stati automatizzati; sono stati eseguiti con attenzione e in modo approfondito da veri giocatori e tester umani.

Nota: il testo e la documentazione multilingue sono stati generati dall'intelligenza artificiale e non sono stati revisionati da madrelingua. È prevedibile un'elevata imprecisione della traduzione. Senza la codifica degli agenti, questo progetto non esisterebbe. Grazie per avergli dato una possibilità!

Cosa potrebbe accadere dopo
------------------

Questo progetto è sostanzialmente completo e non sono previsti contenuti o funzionalità importanti. Tuttavia, questa mod verrà mantenuta e aggiornata attivamente nel tempo secondo necessità, con il feedback dei giocatori che guida questi miglioramenti. Il potenziale lavoro futuro include ulteriori revisioni e correzioni di bug, perfezionamento del codice e continui miglioramenti alla reattività vocale. Prism crea un possibile percorso verso altre piattaforme in futuro, ma questa mod attualmente supporta solo Windows x64. Il repository del progetto è il luogo in cui seguire gli ulteriori sviluppi.

Grazie
---------

A coloro che hanno testato questa mod prima del rilascio e hanno contribuito a portarla dov'è ora, grazie. Sapete tutti chi siete. Ai giocatori che offrono feedback, provano la mod per la prima volta o credono in me e in questo progetto, grazie. Il tuo sostegno mi motiva a continuare a creare cose in un mondo che può sembrare folle e profondamente imperfetto. Spero che questo progetto ti renda più facile goderti il ​​gioco e giocare con gli altri. Grazie mille a tutti. Divertitevi Bop It!

— Christopher Shaw

MelonLoader finestre di avvio
---------------------------

Il modello Loader.cfg fornito nasconde la schermata iniziale e la console separate di MelonLoader. L’installer
applica queste due impostazioni prima che tu avvii personalmente il gioco. Non vengono saltate la schermata
del titolo né quella di benvenuto del mod.

A gioco chiuso, apri UserData/Loader.cfg nella cartella del gioco. Se il file esiste già, imposta disable_start_screen su true nella sezione [loader] esistente e hide_console su true nella sezione [console] esistente. Mantieni tutte le altre voci. Se il file non esiste, copia il modello UserData/Loader.cfg fornito con la compilazione, oppure configuration/Loader.cfg dal codice sorgente. Non sostituire mai un Loader.cfg esistente con il modello completo.

[loader]
disable_start_screen = true

[console]
hide_console = true

La mod non reimposta queste opzioni a ogni avvio. Per risolvere problemi puoi riportare manualmente una delle due impostazioni a false. Se la disinstallazione conserva un’installazione condivisa di MelonLoader, ripristina solo i flag modificati dal programma di installazione che non sono stati successivamente cambiati e conserva le altre modifiche. Se rimuove la propria installazione inutilizzata di MelonLoader, elimina anche i file noti Loader.cfg e MelonPreferences.cfg.

Dopo una modifica riuscita, il mod annuncia l'input e l'azione a cui è assegnato, ad esempio "Space assegnato a COLPISCI".

Scegliere il download giusto

La prima pubblicazione pubblica su GitHub prevede i quattro download seguenti. Sono file futuri, non ancora disponibili; non è stata pubblicata alcuna release pubblica o tag. Nel frattempo usa un installer fornito o il codice sorgente. Un archivio sorgente non è lo ZIP di installazione compilato.

https://github.com/Chris-E-Shaw/BopItAccess/releases

- BopItAccess-Installer.exe: L’installer autonomo per Windows x64. Trova il gioco e gestisce dipendenze, installazione, aggiornamenti, diagnostica e rimozione. Non è firmato.
- BopItAccess-v1.0.zip: Il pacchetto compilato del mod per installazione manuale senza eseguire l’EXE Bop It Access. Include Mods/BopItAccess.dll, prism.dll, tutti i documenti e le licenze Prism, un modello Loader.cfg, README.txt e il collegamento di disinstallazione. Non include MelonLoader, .NET, file del gioco o assembly generati.
- Source code (zip): Lo ZIP del codice sorgente della release generato automaticamente da GitHub. Serve a leggere o compilare il codice; non è il pacchetto compilato del mod.
- Source code (tar.gz): Lo stesso codice sorgente come archivio tar compresso con gzip. Un formato sorgente alternativo, non un altro installer del mod.

Installer non firmato e avvisi di sicurezza Windows 11

Questo installer non è firmato. Un programma non firmato o poco conosciuto può generare avvisi SmartScreen o antivirus, inclusi possibili falsi positivi; non dimostra che ogni rilevamento sia errato. Ottienilo soltanto dal progetto ufficiale Bop It Access o da una consegna diretta affidabile e decidi se fidarti del file. Lo ZIP compilato evita questo EXE. Non disattivare l’antivirus né escludere un’intera unità o cartella del gioco.
https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app

Consentire uno specifico rilevamento Defender

Premi Win+I e apri Privacy e sicurezza > Sicurezza di Windows > Apri Sicurezza di Windows > Protezione da virus e minacce > Cronologia della protezione (talvolta detta cronologia delle minacce). Espandi la voce relativa all’installer. Con Tab raggiungi Azioni o Altre azioni, premi Invio e scegli Consenti nel dispositivo o Consenti; approva la richiesta amministratore se appare. Un file in quarantena può richiedere prima Ripristina e poi il consenso se rilevato nuovamente. Se rimosso, scaricalo di nuovo dal progetto ufficiale. Controlla la voce esatta prima di consentire.
https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app · https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq

Esclusioni Defender facoltative e limitate

In Protezione da virus e minacce scegli Gestisci impostazioni sotto le impostazioni di protezione, poi Esclusioni > Aggiungi o rimuovi esclusioni. Approva con Sì la richiesta amministratore se presente. Scegli Aggiungi un’esclusione > Processo, digita esattamente BopItAccess-Installer.exe e premi Invio. Il nome deve corrispondere all’eseguibile effettivamente avviato.

L’esclusione Processo Microsoft riguarda i file aperti da quel processo; non esclude l’EXE dell’installer, ripristina file in quarantena o evita SmartScreen. Se Defender rileva l’EXE stesso e ti fidi, un’esclusione File facoltativa per quel preciso EXE scaricato è l’alternativa limitata pertinente. Rimuovi le eccezioni non più necessarie.
https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview

SmartScreen è un avviso separato. Se ti fidi di quel preciso EXE e Windows lo offre, scegli Ulteriori informazioni > Esegui comunque. Il consenso Defender o l’esclusione Processo non evita questo avviso; un criterio può impedire l’esecuzione.

Passaggi verificati il 9 ottobre 2026 per Windows 11 25H2, build 26200.9550. Le etichette possono variare; nessuna prova dell’interfaccia è stata eseguita.

Programma di installazione Windows 0.2.6
----------------------------------------

Installare, aggiornare o rimuovere con l’installer

1. Chiudi il gioco, avvia BopItAccess-Installer.exe e approva la richiesta amministratore Windows. Leggi il campo Welcome and controls, poi controlla Game folder o usa Browse. Welcome and controls è di sola lettura, selezionabile e riceve il primo focus; Alt+W vi ritorna.
2. Scegli Install per l’ultima release pubblica compilata quando disponibile. Prima della prima pubblicazione, Show advanced mostra Install alpha, che chiede conferma e compila le ultime sorgenti sul computer. Attendi il messaggio di successo. Play Bop It! The Video Game avvia poi il gioco con Steam solo quando lo scegli.
3. Per un’installazione esistente, apri l’installer con il gioco chiuso e controlla lo stato. Update appare se trova una release pubblica più recente. Scegli Update e attendi il termine; mantiene le impostazioni salvate. Install alpha è la scelta separata per le ultime sorgenti, non l’aggiornamento pubblico.
4. Per rimuovere il mod, scegli Uninstall e conferma, poi Uninstall for me oppure Uninstall for everyone. Entrambi rimuovono i file condivisi del mod da questa cartella del gioco. La scelta determina se eliminare le preferenze Windows del mod per il tuo account o tutti i profili locali; le preferenze originali del gioco restano. App installate di Windows usa lo stesso flusso. Controlla il risultato prima di Quit.

La tabella seguente elenca ogni azione e campo di testo della finestra principale. Installer status e Installation progress sono informazioni, non pulsanti. Welcome and controls contiene istruzioni riutilizzabili; Status log contiene i messaggi variabili. Abort chiede prima di annullare un’installazione attiva; Quit usa le stesse regole di cancellazione sicura. I dialoghi includono Keep open/Quit, conferma/annulla e i due ambiti delle preferenze da rimuovere. Show advanced cambia solo le azioni visibili.

Usa BopItAccess-Installer-0.2.6.exe oppure BopItAccess-Installer.exe fornito dal progetto. Entrambi i nomi contengono lo stesso programma di installazione autonomo per Windows x64. Il progetto include il codice sorgente; non è ancora pubblicato alcun binario compilato pubblico o GitHub Release.

Chiudi Bop It!, apri il programma di installazione e approva la richiesta di autorizzazione come amministratore di Windows. Il programma di installazione ti dà il benvenuto, cerca il gioco nelle librerie Steam di tutte le unità disponibili e tenta di portare la propria finestra in primo piano. Controlla la cartella del gioco visualizzata; usa Browse se devi scegliere un’altra cartella. Tab passa da un controllo all’altro. Il registro di stato è un campo di testo di sola lettura: portaci il focus per esaminare i messaggi con i tasti di spostamento del cursore, selezionare il testo o copiarlo.

Il programma di installazione 0.2.6 richiede brevemente attivazione in primo piano e focus della tastiera all’avvio. Se al termine della sua breve osservazione iniziale è ancora attiva un’altra finestra, fa lampeggiare il titolo e il pulsante sulla barra delle applicazioni e chiede di passare al programma con Alt+Tab. Attivalo prima di usare i suoi comandi da tastiera o controller. Alt+G porta il focus sul campo della cartella del gioco.

Show advanced è deselezionato all’apertura del programma di installazione. Mostra Install alpha, Save diagnostics e Copy diagnostics. Install scarica l’ultima versione pubblica di GitHub quando disponibile. Non esiste ancora una versione pubblica, quindi al momento chi esegue i test deve usare Show advanced e Install alpha. L’installazione alpha chiede conferma, scarica i sorgenti più recenti e li compila sul tuo computer. Update compare quando viene trovata una versione pubblica più recente per una copia installata.

I messaggi di stato spiegano con parole semplici cosa viene scaricato, installato o completato. Una sola barra mostra l’avanzamento stimato dell’intera installazione, senza azzerarsi per ogni download o file. Avanza a incrementi di cinque punti percentuali; alcune fasi di preparazione possono richiedere tempo senza cambiamenti visibili. Il messaggio di benvenuto, la disponibilità di un nuovo aggiornamento e la conferma della copia della diagnostica vengono inviati al lettore di schermo tramite le notifiche di accessibilità di Windows. La loro lettura ad alta voce dipende dal lettore di schermo e dal suo supporto alle notifiche di Windows.

Il programma di installazione 0.2.6 non avvia mai Bop It! durante l’installazione. L’installazione alpha riutilizza i file locali di compilazione corrispondenti oppure prepara file temporanei dalla tua copia del gioco installata, che rimane chiusa. Il programma di installazione colloca quindi MelonLoader nella cartella del gioco e aggiunge subito Mods/BopItAccess.dll, seguito da Prism, impostazioni, documentazione completa e supporto alla disinstallazione. Attendi il messaggio di riuscita, poi avvia tu il gioco tramite Steam quando sei pronto.

Dopo un’installazione riuscita compare Play Bop It! The Video Game. Attiva questo pulsante per avviare tu il gioco tramite Steam quando sei pronto. Il programma di installazione non avvia mai automaticamente il gioco durante l’installazione.

Una versione compilata richiede il runtime .NET 6 per Windows x64, non un SDK di sviluppo. I runtime completi già presenti vengono riutilizzati. Un runtime mancante viene scaricato da Microsoft e collocato in MelonLoader/Dependencies/dotnet. Install alpha richiede anche un SDK .NET compatibile e il targeting pack di .NET 6: viene riutilizzato un SDK esistente oppure installato l’SDK ufficiale di Microsoft a livello di sistema. Il programma di installazione non crea nuove cartelle SDK nella cartella principale del gioco. MelonLoader 0.7.3 Open-Beta e Prism 0.18.3 provengono dalle rispettive versioni ufficiali. I componenti condivisi di Microsoft .NET e gli SDK rimangono installati dopo l’interruzione o la disinstallazione.

Quit chiude il programma di installazione. Se l’installazione è ancora in corso, chiede se interromperla e annullarne le modifiche prima di chiudere; Keep open prosegue normalmente. Se l’installazione termina mentre stai decidendo, la finestra di dialogo si aggiorna per indicarlo e Quit non annulla l’installazione completata. Una volta iniziata la rimozione, la disinstallazione viene completata in sicurezza prima dell’uscita. Anche Abort chiede conferma e annulla le modifiche ai file del gioco effettuate durante questo tentativo. L’annullamento durante l’installazione di Microsoft .NET attende che l’installazione di quei componenti condivisi termini in sicurezza.

Dopo aver confermato Uninstall, scegli Uninstall for me oppure Uninstall for everyone. Entrambe le opzioni rimuovono i file condivisi della mod da questa cartella del gioco, quindi la mod non sarà più disponibile per nessuno che usi quell’installazione. La scelta determina di chi vengono rimosse le preferenze Windows salvate della mod: solo dell’account che ha richiesto l’operazione, oppure di tutti i profili Windows locali, compresi quelli con sessione disconnessa. Le preferenze del gioco originale vengono conservate. L’SDK .NET rimane installato.

Quando il programma di installazione rimuove la propria installazione di MelonLoader e nessun’altra mod ne ha bisogno, rimuove anche i file noti Loader.cfg e MelonPreferences.cfg e le cartelle Plugins, UserLibs e UserData se vuote. Vengono rimossi le impostazioni di Bop It Access, i registri noti, le guide e i file del programma di installazione. Le altre mod, i file condivisi del loader già presenti e i file non riconosciuti vengono protetti. Questo significa anche che un file sconosciuto può lasciare una cartella sul disco; il programma di installazione lo segnala nella diagnostica invece di eliminare dati estranei.

Il programma di installazione rimane aperto dopo la disinstallazione, così puoi esaminare il risultato, salvare la diagnostica o installare di nuovo. Scegli Quit quando hai finito. L’utilità di disinstallazione in esecuzione e i file di diagnostica automatici vengono eliminati dopo la chiusura della finestra. Una reinstallazione nella stessa finestra avvia un nuovo registro dell’installazione; la pulizia differita non può rimuovere la nuova installazione.

La pagina App installate di Windows usa gli stessi passaggi di conferma, scelta delle preferenze e pulizia. Il programma di installazione fornisce BopItAccess-uninstall.ps1 nella cartella del gioco come collegamento al programma di disinstallazione installato; anche le future compilazioni dai sorgenti includeranno questo script nei file prodotti. Copiare manualmente uno script non installa il programma di disinstallazione stesso. Per una precedente installazione manuale priva di un registro di proprietà dei file, il programma di installazione rimuove i file identificabili della mod e conserva i file condivisi di cui non è possibile stabilire l’origine.

Se la pulizia non può essere completata in sicurezza, il programma di installazione lo spiega e conserva le informazioni necessarie per riprovare. Per un’installazione gestita, la voce di disinstallazione di Windows e il punto di ripresa della pulizia rimangono finché la rimozione non riesce. Una vecchia copia manuale non ha un registro persistente di proprietà dei file; nel programma di installazione ancora aperto, riprova le operazioni segnalate dagli avvisi. Non installare, aggiornare o rimuovere la mod mentre Bop It! è in esecuzione.

Scorciatoie da tastiera del programma di installazione
------------------------------------------------------

Welcome and controls: Alt+W. Il campo Welcome and controls separato elenca le scorciatoie di lettura con controller; Alt+W vi ritorna e Alt+L apre il Status log variabile. Entrambi sono di sola lettura, selezionabili e consultabili. Show advanced annuncia selezionato o non selezionato. Seleziona tutto conferma il successo o un campo vuoto. Da tastiera, Ctrl+A seleziona tutto il testo e Ctrl+C copia la selezione.
Cartella del gioco: Alt+G. Assegnare il focus al campo della cartella del gioco.
Browse: Alt+B. Scegliere la cartella del gioco.
Install: Alt+I. Installare l’ultima versione pubblica quando disponibile.
Install alpha: Alt+A. Confermare e compilare i sorgenti più recenti; visibile con Show advanced.
Update: Alt+U. Installare una versione pubblica più recente quando viene proposta.
Play Bop It! The Video Game: Alt+P. Avviare il gioco tramite Steam; disponibile dopo un’installazione riuscita.
Uninstall: Alt+N. Confermare la rimozione e scegliere di chi rimuovere le preferenze Windows della mod.
Abort: Alt+R. Confermare l’annullamento dell’installazione corrente.
Registro di stato: Alt+L. Assegnare il focus ai messaggi di stato di sola lettura con testo selezionabile.
Show advanced: Alt+V. Mostrare o nascondere l’installazione alpha e gli strumenti di diagnostica.
Save diagnostics: Alt+D. Salvare la sessione diagnostica completa e continuare a registrarla; visibile con Show advanced.
Copy diagnostics: Alt+C. Copiare l’istantanea diagnostica completa; visibile con Show advanced.
Quit: Alt+Q. Chiudere, gestendo l’annullamento in sicurezza se un’operazione è in corso.

Uso di un controller nel programma di installazione
---------------------------------------------------

Il programma di installazione supporta i controller di tipo Xbox e gli altri controller che Windows rende disponibili tramite XInput. I suoi comandi sono separati dai comandi rimappabili del gioco. La croce direzionale o la levetta sinistra passa da un controllo all’altro; quando un campo di testo ha il focus, le direzioni servono invece a scorrerne il testo. I pulsanti dorsali passano sempre al controllo precedente o successivo che può ricevere il focus. A attiva il pulsante o la casella di controllo con il focus. L’input del controller viene gestito solo quando questo programma di installazione o una delle sue finestre di dialogo è in primo piano.

B torna indietro o annulla una finestra di dialogo; nella finestra principale del programma di installazione chiede di interrompere un’installazione in corso, altrimenti esegue Quit. Start esegue Quit nella finestra principale e torna indietro in una finestra di dialogo. Y (il pulsante frontale superiore) seleziona tutto il testo quando un campo di testo del programma di installazione ha il focus. Fuori dai campi di testo della finestra principale, Y attiva o disattiva Show advanced. Nel registro di stato o in un altro campo di testo del programma di installazione, la croce direzionale o la levetta sinistra funziona come le frecce: Sinistra/Destra si sposta per caratteri e Su/Giù per righe. Tieni premuto LT come Ctrl: Sinistra/Destra si sposta per parole e Su/Giù per paragrafi. Tieni premuto RT come Maiusc per estendere la selezione; tieni premuti LT e RT insieme per selezionare parole o paragrafi. X copia solo il testo selezionato; seleziona prima la parte desiderata. Ctrl+C sulla tastiera continua a copiare la selezione. Quando non è selezionato del testo, il programma di installazione invia anche notifiche accessibili per il carattere, la parola, la riga o il paragrafo nella posizione del cursore. Il programma di installazione invia una conferma accessibile quando il testo viene copiato e segnala una selezione vuota o un errore di copia. L’annuncio vocale dipende dal supporto del lettore di schermo per le notifiche di Windows. La navigazione con controller nelle finestre di dialogo native di Windows per le cartelle e il salvataggio richiede ancora una verifica umana. È sempre possibile usare una tastiera per inserire una cartella o un nome di file. Questa implementazione non copre i controller privi del supporto a XInput.

Il campo Welcome and controls separato elenca le scorciatoie di lettura con controller; Alt+W vi ritorna e Alt+L apre il Status log variabile. Entrambi sono di sola lettura, selezionabili e consultabili. Show advanced annuncia selezionato o non selezionato. Seleziona tutto conferma il successo o un campo vuoto. Da tastiera, Ctrl+A seleziona tutto il testo e Ctrl+C copia la selezione.

Il programma di installazione 0.2.6 richiede che ogni annuncio vocale emesso sostituisca la voce precedente dell’installer, inclusi lettura del testo, Seleziona tutto, stato selezionato/non selezionato di Show advanced, Copy diagnostics e altre conferme. LB/RB continua ad annunciare il nuovo controllo con il focus. La frequenza degli annunci di stato resta invariata; non ogni voce del registro viene letta automaticamente. L’interruzione effettiva dipende dal supporto delle notifiche Windows del lettore di schermo e richiede ancora una verifica umana.

Diagnostica del programma di installazione
------------------------------------------

Show advanced mostra Save diagnostics (Alt+D) e Copy diagnostics (Alt+C). I registri automatici UTF-8 vengono conservati localmente in %ProgramData%\BopItAccess\diagnostics. Save diagnostics scrive l’intera sessione corrente nel file .log o .txt scelto e continua a registrare fino alla chiusura del programma di installazione; Copy diagnostics copia un’istantanea e fornisce una conferma accessibile. Salva prima di una prova di installazione o disinstallazione, così la registrazione sopravvive alla pulizia dei registri automatici. Qui vengono conservati i dettagli tecnici relativi a file, download, compilatore ed errori, anche se il campo di stato usa messaggi più brevi. Non viene caricato nulla online. I registri possono contenere nomi utente Windows e percorsi completi: controllali prima di condividerli. Le copie esportate intenzionalmente rimangono dopo la disinstallazione.

Al primo avvio manuale dopo l’installazione di MelonLoader, quest’ultimo può scaricare file di supporto e preparare gli assembly del gioco. Attendi circa un minuto, o più a lungo su alcuni sistemi. La mod non può parlare finché MelonLoader non la carica. Lascia aperto il gioco e attendi l’annuncio di avvio di Bop It Access, seguito dall’annuncio della schermata del titolo, di benvenuto o del menu principale prima di usare i comandi del gioco.


Installare lo ZIP compilato senza l’EXE Bop It Access

Quando BopItAccess-v1.0.zip sarà pubblicato, questo percorso userà la DLL già compilata e non richiederà il .NET SDK. Occorrono comunque il gioco acquistato Windows x64, MelonLoader ufficiale x64 0.7.3 Open-Beta e il runtime .NET 6 Windows x64. Segui le istruzioni ufficiali di MelonLoader e Microsoft; lo ZIP non fornisce questi prerequisiti.

https://github.com/LavaGang/MelonLoader#how-to-use-the-installer
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

1. Installa il gioco con Steam e trova la sua cartella. Chiudi Bop It! prima di cambiare i file; usa se necessario la funzione Steam per sfogliare i file installati.
2. Installa MelonLoader ufficiale x64 in quella cartella e assicurati che il runtime .NET 6 x64 sia installato. Non avviare ancora il gioco: colloca prima il mod.
3. Estrai BopItAccess-v1.0.zip compilato in una cartella temporanea. Copia Mods/BopItAccess.dll in Mods del gioco, creando o unendo la cartella senza eliminare altri mod. Copia prism.dll accanto a BopIt!.exe.
4. Copia per intero documentation e THIRD-PARTY-LICENSES, incluse tutte le lingue e le note/licenze Prism. Copia README.txt e BopItAccess-uninstall.ps1 del pacchetto. Lo script è soltanto un collegamento a un uninstaller gestito dall’installer; copiarlo non crea un uninstaller funzionante o una registrazione in App installate.
5. Per UserData/Loader.cfg: se assente, copia il modello. Se esiste, unisci solo [loader] disable_start_screen=true e [console] hide_console=true nelle sezioni corrispondenti e mantieni le altre impostazioni. Non sovrascrivere una configurazione esistente con il modello.
6. Avvia il lettore di schermo se usato, poi il gioco tramite Steam. MelonLoader può scaricare file di supporto e generare assembly al primo avvio, con il mod già in Mods. Attendi gli annunci di avvio del mod e del menu prima di usare i controlli.

Per un aggiornamento manuale, chiudi il gioco e copia mod, Prism, documenti e licenze del nuovo pacchetto negli stessi percorsi. Mantieni BopItAccess.ini, altri mod e file estranei; unisci Loader.cfg come sopra. Per disattivare/rimuovere il mod manuale, elimina solo Mods/BopItAccess.dll. Per una pulizia ulteriore, rimuovi soltanto i file copiati per questo mod e UserData/BopItAccess.ini o il suo .tmp; mantieni Prism/MelonLoader se condivisi. Le preferenze Windows possono restare. Lo ZIP manuale non ha registro di proprietà né uninstaller registrato. Se poi scegli l’installer, Uninstall può riconoscere una vecchia copia manuale e pulire le preferenze proteggendo i file di provenienza sconosciuta. I log del mod sono Mods/BopItAccess.log e Mods/BopItAccess.log.previous; nella pulizia elimina soltanto questi log noti.

Pacchettizzazione avanzata: scripts/package-mod.ps1 impacchetta un mod già compilato corrispondente e file noti di documentazione/configurazione/Prism. Verifica versioni sorgente/DLL ed esclude file del gioco, generati o precedenti; non compila. Archivio e preparazione restano locali.
