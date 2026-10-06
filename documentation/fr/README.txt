Bop It Access 0.9.6 - Prism Parole et Braille

Qu'est-ce que cela fait
--------------
Le mod suit les paramètres du jeu > Sélection de la langue pour la parole. Il comprend
Anglais, français, italien, allemand, espagnol (Espagne), espagnol (Amérique latine),
Japonais, coréen, chinois simplifié et portugais brésilien. Changer le
la langue du jeu modifie également les annonces de mod et le guide de l'utilisateur du jeu.
Les traductions initiales sont des brouillons générés automatiquement et doivent être révisées.
par des locuteurs fluides.
La parole est activée par défaut. Lorsque le mod se charge avec la parole activée, il annonce
"Le discours Bop It Access est prêt. Le jeu est toujours en cours de chargement. Attendez l'annonce de l'écran titre ou du menu principal avant d'utiliser les commandes." via Prism. Si l'écran titre apparaît, le mod annonce l'entrée actuelle TAPER pour ouvrir le menu principal. Il lit
le bouton du menu principal ciblé et la ligne Paramètres ciblée. Les valeurs des paramètres sont
prononcé avec le nom de la ligne activé. Changer une valeur tout en restant concentré sur cela
la ligne indique uniquement la nouvelle valeur. LATENCE AUDIO, COMMANDES et GO ONLINE sont des actions
boutons, ils sont donc prononcés sans l'espace réservé dénué de sens "0".
Le menu Paramètres comporte également un curseur LIMIT FPS avec 30, 60, 120, 240 et
Choix ILLIMITÉS. Il démarre à 60 pour une nouvelle installation et mémorise le
valeur sélectionnée entre les sessions. Focus parle du nom et de la valeur ; le changer
ne parle que de la nouvelle valeur. Le plafond modifie la fréquence d'images cible de Unity tandis que
laissant l'échelle de temps du jeu, le calendrier de mise à jour fixe et l'audio intacts.
Comme pour toute limite d'image, un paramètre inférieur signifie également moins d'interrogations d'entrée basées sur les images.
Si 30 FPS semblent moins réactifs dans un jeu rapide, choisissez 60, 120 ou ILLIMITÉ.
La bascule MUTE AUDIO IN BACKGROUND apparaît directement sous VOICE OVER dans
Paramètres. Lorsqu'il est activé, il coupe le son du jeu lorsque la fenêtre de jeu n'est pas
concentré, puis restaure l'état audio du jeu précédent lorsque la mise au point revient.
Il démarre Off et est enregistré entre les sessions.
Lors de la toute première utilisation du mod, la MUSIQUE, les SFX et la VOIX OFF natives du jeu
les curseurs commencent à 30. La mise à niveau conserve les paramètres audio du jeu précédemment enregistrés.
Une fois que le jeu atteint son menu principal pour la première fois, un écran de bienvenue apparaît.
se concentrer. Son message peut être à nouveau focalisé avec Up, et ses choix ouvrent Mod
Paramètres, ouvrez le guide de l'utilisateur dans le jeu ou continuez vers le menu principal.
L'écran de bienvenue est marqué comme terminé uniquement après qu'un choix soit terminé avec succès
ça. Fermer le jeu alors qu'il est ouvert le laisse prêt pour le prochain lancement.
L'indexation des paramètres attend désormais les lignes audio, FPS et MOD SETTINGS du mod avant
annonçant le premier élément ciblé sur un écran Paramètres nouvellement ouvert.

Sous Contrôles, Paramètres dispose désormais d’un menu PARAMÈTRES MOD. SPEECH OUTPUT utilise le même
commutateur principal enregistré en tant que F8 ou sélection du contrôleur, y compris la récupération vocale
instructions lorsque la parole est désactivée. La SORTIE BRAILLE démarre et est enregistrée
entre les séances. Prism envoie des annonces à un lecteur d'écran compatible
sortie braille lorsque ce paramètre est activé. Le désactiver arrête le braille du mod
messages tout en laissant la parole disponible.
OUTPUT MODE: Auto utilise un lecteur d’écran compatible en cours d’exécution, puis OneCore, puis SAPI.
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Chaque lecteur d’écran et moteur vocal n’est disponible que si la version de Prism installée et le système du joueur le prennent en charge. Si le mode choisi est indisponible, le mod utilise une sortie disponible et l’annonce une seule fois.
Les voix SAPI enregistrées sont recherchées par le nom affiché dans Prism ; si plusieurs voix portent ce nom, la première peut être choisie.
MUTE PAROLE EN ARRIÈRE-PLAN est une bascule enregistrée, désactivée par défaut. Lorsqu'il est activé,
le mod cesse de parler dès que le jeu perd le focus de la fenêtre. Discours créé
pendant que le jeu est en arrière-plan est ignoré et les annonces reprennent
avec une nouvelle activité après le retour de la concentration. Si la parole elle-même est désactivée lorsque le jeu
retrouve le focus, le mod donne la récupération actuelle du clavier et du contrôleur
instructions une fois.

Le menu MOD SETTINGS a également Lire les positions dans les menus, activé par défaut et enregistré entre les sessions.
Lorsqu'il est activé, un élément de menu ciblé inclut sa position, telle que « PLAY, 1 of 6 ».
Cela s'applique aux menus principal et paramètres, commandes, modes de lecture, Mod
Paramètres, choix de fin de partie, classements, réalisations, crédits et autres
écrans pris en charge. Le décompte suit les choix actuellement disponibles. Changer
un curseur ou une bascule alors qu'il reste focalisé n'annonce que la nouvelle valeur.

FORMATER LA PAROLE est une option des paramètres du mod, activée par défaut et mémorisée. Elle rend plus naturelle la casse des mots de menu entièrement en majuscules, en conservant les mots à casse mixte et les abréviations comme SAPI, NVDA, SFX et FPS. Dans le guide intégré, si la numérotation est annoncée et que la ligne ne se termine pas par une ponctuation, elle ajoute trois points pour marquer une pause avant le numéro de ligne. La ponctuation existante est conservée. Seuls les textes envoyés à la parole et au braille changent ; les textes visibles du jeu et le guide restent intacts. Désactiver cette option désactive les deux ajustements. Votre ancien choix de filtrage des majuscules est conservé.

READ CONTROL TYPES est une autre bascule MOD SETTINGS enregistrée, activée par défaut. Lorsqu'il est activé,
le type de l'élément ciblé suit son nom et précède sa valeur et son index :
"Curseur MUSIQUE, 30, 1 sur 12", "Bascule VIBRATION, activé, 6 sur 12", ou
"Bouton PLAY, 1 sur 6". Les menus identifient également les onglets, les champs de texte et les
lister les éléments le cas échéant. Les changements de valeur continuent de parler uniquement de la nouvelle valeur.
SLIDER RANGES est une bascule enregistrée, désactivée par défaut. Lorsqu'ils sont activés, les curseurs ciblés
signalent également leurs points de terminaison disponibles après la valeur actuelle, tels que
"Curseur MUSIQUE, 30, plage 0 à 100, 1 sur 12" lors de l'indexation et des types de contrôle
sont activés. Le déplacement d'un curseur indique toujours uniquement la nouvelle valeur.
Les commentaires individuels sont mémorisés et activés par défaut. Annonce la couleur active au début et lorsqu’elle change. Après une vie perdue, annonce le nombre de vies restantes. Après une vie gagnée, annonce la couleur du joueur et son nouveau total, par exemple « Vert, 3 vies », pour indiquer quel joueur a été le plus rapide. Ces annonces sont désactivées lorsque les commentaires individuels sont désactivés. La limite de trois vies du jeu reste inchangée. Cette fonction n’agit que pendant une partie en tête-à-tête.

LE TYPE DE CONSEILS apparaît désormais au-dessus des CONSEILS DU BOUTON AUTO-SPEAK dans le menu Paramètres du module.
CONSEILS SUR LE BOUTON DE PAROLE AUTOMATIQUE est une bascule enregistrée et est activé par défaut. Le tourner
Désactivé supprime les conseils automatiques, tandis que SPEAK HINTS reste disponible sur demande.
CONSEILS DE BOUTON DELAY a Aucun, 5 secondes
(Peut interrompre la parole), 10 secondes, 15 secondes, 30 secondes et 60 secondes.
La valeur par défaut est de 10 secondes. Avec Aucun, les entrées valides pour l'écran actuel et
leurs actions sont incluses dans la chaîne vocale ordinaire de l'élément ciblé,
après un arrêt complet. L'action pour le contrôle ciblé est prononcée avant le général
navigation dans les menus. Il n’y a pas d’annonce distincte de premier indice. Avec un chronométré
retard, la première annonce d’indice fait suite à autant d’inactivité. L'option 5 secondes peut
interrompre un discours déjà en cours ; des retards plus longs font la queue derrière lui. Après
l'annonce du premier indice, un autre délai ne commence que lorsque le joueur donne
entrée, sauf si les répétitions sont activées. Passer à un autre élément ciblé ou modifier
un curseur ou une bascule ciblé compte comme entrée et redémarre le délai, même si le
L'analyse de liaison d'entrée du jeu manque l'action de la touche ou du contrôleur. Une clé inutilisée
cela ne change pas l'interface utilisateur ne la redémarre toujours pas.
HINTS TYPE est un curseur enregistré avec Automatique, Clavier, Contrôleur et les deux.
Automatique est la valeur par défaut et suit le clavier ou le clavier le plus récemment utilisé.
entrée du contrôleur. L'utilisation de la souris compte comme un clavier. Le clavier et le contrôleur parlent
uniquement les indices de cet appareil ; Les deux donnent les deux ensembles d'entrées avec un périphérique explicite
noms. La récupération vocale inclut toujours les deux appareils afin que le
le joueur peut trouver le contrôle qui réactive la parole.

Les conseils sur les boutons placent l'entrée avant son action : "Entrez ou Space, activez l'élément."
Les indications relatives à un seul appareil omettent le nom de l'appareil. Les noms des manettes de contrôle sont prononcés
dans son intégralité, comme « stick gauche haut et bas ». Les deux modes identifient le clavier
et les entrées du contrôleur. Le mod lit les liaisons actuelles du jeu de manière native
les changements de reliure sont reflétés dans ces indices. Lorsqu'aucun contrôleur n'est
connecté et le jeu a différents noms de boutons sur différents contrôleurs
types, l'astuce utilise "bouton de confirmation" ou "bouton de retour" plutôt que de supposer un
Disposition Xbox. Les lignes de score du classement utilisent Page Up et Page Down sur le clavier.
Le contrôleur haut/bas lit les lignes uniquement lorsqu'aucun contrôle de classement n'a le focus ;
les astuces signalent uniquement les commandes disponibles pour le TYPE D'HINTS sélectionné. Ordinaire
les conseils à l'écran incluent également les liaisons actuelles SPEAK HINTS et TOGGLE SPEECH.
Les deux sont recherchés de manière centralisée, afin que les futurs contrôles mondiaux puissent rejoindre le même
liste d'indices sans changer chaque écran séparément.

REPEAT BUTTON HINTS est un curseur enregistré distinct : Désactivé, 2x, 3x, 4x, 5x ou
Infiniment. La valeur par défaut est Infini. Le nombre correspond au total des lectures en un
cycle : 2x signifie le premier indice et une répétition ; 3x signifie le premier indice et
deux répétitions. Désactivé autorise toujours le premier indice automatique ou manuel.
REPEAT INTERVAL définit le délai entre les répétitions sur 15, 30, 45 ou
60 secondes et la valeur par défaut est 30 secondes. Avec BUTTON HINTS DELAY réglé sur Aucun,
la minuterie de répétition démarre immédiatement après la saisie. Entrée, un focus ou une valeur
changement, ou un changement d’écran redémarre le cycle d’indices pour l’écran actuel.
SPEAK HINTS remplace le
en attente d'un indice automatique pour ce cycle, puis utilise REPEAT INTERVAL pour tout
répétitions configurées. Cela fonctionne également avec les CONSEILS DU BOUTON AUTO-SPEAK désactivés.
Les indices sont supprimés pendant
gameplay actif et les phases de beat-timing de Audio Calibration, où des extras
le discours pourrait masquer un indice. Un rappel existant enregistré de 15, 30 ou 60 secondes
le délai de la version 0.6.2 devient la nouvelle valeur BUTTON HINTS DELAY.

MOD SETTINGS: Voix, Volume, Vitesse et Hauteur de voix règlent la sortie OneCore ou SAPI réellement utilisée, même en mode Auto. Seuls les réglages pris en charge sont visibles ; ils sont masqués pour les autres sorties. Chaque moteur mémorise ses choix séparément. Volume : de 5 % à 100 % par pas de 5, valeur initiale 100 %. Vitesse et hauteur : de 0 à 100 par pas de 5, valeur initiale 50. Le volume minimal laisse les messages de récupération audibles.
Les choix de paramètres du module sont mémorisés entre les sessions. RESTAURER LES DÉFAUTS DU MOD
renvoie ces choix aux valeurs par défaut décrites ci-dessus. Appuyez dessus une fois pour demander
confirmation, puis appuyez à nouveau dessus dans les cinq secondes pour les restaurer. Déménagement
vers une autre ligne ou laisser passer cinq secondes annule la demande. Cela ne veut pas dire
modifiez les curseurs MUSIQUE, SFX ou VOICE OVER du jeu, LIMIT FPS ou personnalisé
liaisons du clavier et du contrôleur. Retour revient aux paramètres.
OUVRIR LE GUIDE DE L'UTILISATEUR lit le guide HTML de la langue actuellement sélectionnée dans
la ligne Paramètres > Langue du jeu. Le guide anglais est à
documentation\BopItAccess-user-guide.html ; les guides traduits sont dans la langue
sous-dossiers. Si une copie traduite est manquante ou illisible, le guide anglais
s'ouvre à la place. Sa liste de sujets provient de la table des matières du document et
se recharge à chaque ouverture. Confirmer ouvre un sujet. Up et Down lisent ses lignes. Dans les tableaux,
La gauche déplace une colonne vers la gauche et la droite déplace une colonne vers la droite ; Garder de haut en bas
la colonne actuelle lors du déplacement entre les lignes. Cellules d’étiquette des en-têtes de colonnes
au lieu d'apparaître sous forme de lignes de données. La table est annoncée une fois à l'entrée, et
sa fin est annoncée à la sortie. Retour revient aux sujets ou quitte le guide.
Pendant la lecture, le mod applique le paramètre Filtre menu-musique du jeu et
restaure sa valeur précédente à la sortie. RÉINITIALISER L'ÉCRAN DE BIENVENUE demande un
appuyez une deuxième fois dans les cinq secondes, puis fait apparaître l'écran de bienvenue sur le
prochain lancement du jeu. Changer de ligne ou attendre cinq secondes annule la confirmation.
Cette mise à jour restaure la disposition native des lignes de paramètres du jeu de manière haut/bas.
la navigation reste sur les lignes Paramètres après l’ajout de MOD SETTINGS.
Il démarre également le sous-menu MOD SETTINGS sur SPEECH OUTPUT à chaque ouverture,
empêcher une ligne RETOUR précédemment sélectionnée de fermer immédiatement le menu
lorsque Enter est utilisé pour le rouvrir.
L'entrée qui ouvre MOD SETTINGS est désormais ignorée par ses lignes jusqu'à ce que cette entrée soit
libéré, donc la réouverture du menu ne peut pas également désactiver la parole. Le mod
Les lignes de liaison de contrôles ajoutées attendent également que l'entrée d'ouverture soit
libéré avant d’accepter une demande de reconsolidation.

Dans Play, le mod lit Solo, Party, Pass It et One on One lorsqu'il est concentré.
Sur l'écran de sélection de chanson suivant, il annonce le thème actuel
(Shapes, Space, City ou Office) et si le mode Extrême est activé. Se tordre vers
changer la chanson ne parle que du nouveau thème. Tirer pour changer la difficulté parle
seulement le nouvel état Extrême. L'introduction de l'écran explique également le TOURNER,
TIRER, TAPER et actions Retour.
Cette introduction complète est répétée chaque fois qu'un mode est sélectionné et que la chanson
L'écran s'ouvre à nouveau, avec le thème actuel et l'état Extrême.
Appuyez sur SPEAK HINTS (H ou stick droit par défaut) sur cet écran pour entendre
le mode actuel et le texte du didacticiel natif de la difficulté avant de commencer. Chacun
l'action nommée dans cette référence inclut son clavier ou son
contrôle du contrôleur, en suivant le TYPE d'indices. Les contrôles réaffectés sont lus à partir de
les fixations du joueur actif ; One on One nomme les entrées TAPER des deux joueurs. Le
la superposition du didacticiel chronométré pendant le jeu actif reste silencieuse afin de ne pas obscurcir
les commandes vocales du jeu. L'indice annonce cette utilisation supplémentaire de SPEAK HINTS.
Le contrôle LIRE DESCRIPTIONS prononce une description visuelle de la zone sélectionnée.
Étape Shapes, Space, City ou Office sur demande. Il est disponible sur cet écran
seulement, avant le début du jeu. Ses entrées par défaut sont G au clavier et LT
(gâchette gauche) sur le contrôleur. R a été remplacé car c'est le Reset du jeu
Raccourci gyroscopique. L'introduction à l'écran annonce la liaison actuelle.
Le démarrage de la lecture arrête toute parole laissée par la sélection de la chanson afin qu'elle ne puisse pas masquer le
les signaux verbaux du jeu. Les descriptions commencent par les détails de la scène plutôt que par
en répétant le nom de scène sélectionné.

Sur l'écran des résultats finaux, Solo, Party et Pass It annoncent le score final
avant le discours du menu. Solo lit les boutons Replay et Leaderboard ciblés
et explique Back. Les autres modes lisent leurs options Continuer, Rejouer et
Retour aux invites. Un contre un montre un gagnant plutôt qu'un score final numérique, donc
le mod annonce le gagnant qui y est affiché. Le discours de score a la priorité sur
l'annonce initiale du menu ; les invites de l'écran de résultat sont mises en file d'attente après.
Les changements de focus de menu ultérieurs s'interrompent les uns les autres. Des changements rapides immédiatement
après la fin du jeu sont combinés jusqu'à ce que la courte annonce du score ait eu le temps
pour finir, en conservant le dernier choix de menu ciblé.
Les annonces des meilleurs scores en solo et du classement du groupe sont prononcées lorsque le jeu rapporte
un nouveau résultat de classement.
LIRE LE SCORE répète le résultat final à la demande uniquement pendant le résultat du game-over
l'écran est visible. Les entrées par défaut sont T sur le clavier et le stick gauche appuie sur
contrôleur. Solo, Party et Pass It répètent leur partition finale ; Un contre un
répète le gagnant montré par le jeu. L'action est désactivée pendant le jeu,
sélection de chansons et tous les autres écrans. RT (gâchette droite) n'a pas été utilisé comme
par défaut car le jeu le lie déjà à Reset Gyro et Auto Play.
Les répétitions demandées parlent immédiatement et peuvent être interrompues par le menu des résultats
navigation. Seule l'annonce automatique des résultats retarde le menu initial
discours afin que la partition soit entendue en premier.
ONE-ON-ONE FEEDBACK est activé par défaut dans Paramètres > Paramètres de module pour les actifs parlés.
couleur et vies restantes pendant ce mode. Les signaux TAPER partagés ne passent pas par
identifient eux-mêmes une couleur, donc le mod conserve la dernière couleur définie.

TOGGLE SPEECH active ou désactive tous les discours de mod ordinaires depuis n'importe quel écran. C'est
les valeurs par défaut sont F8 sur le clavier et Select sur le contrôleur. Lorsqu'il est éteint, le mod
arrête la parole en cours et annonce que la parole est désactivée, ainsi que la parole en cours
commandes du clavier et du contrôleur pour le rallumer. L'état éteint est
sauvegardés entre les sessions de jeu. Si le jeu commence avec la parole désactivée, le mod donne
ces instructions de récupération au lieu de son message de chargement habituel. Tournant
le discours de retour annonce « Discours sur ». Les autres discours du mod restent silencieux lorsqu'ils sont éteints.
La commande Toggle Speech reste active même lorsque la parole est désactivée.

Les classements accessibles depuis le menu principal, les résultats Solo et les résultats Party
annoncer la chanson, l'appareil, le groupe et la date sélectionnés, le cas échéant. Ils lisent
rang, nom du joueur et score, y compris les états de chargement et de résultat vide. Page précédente
et Page Down lit les lignes de score individuelles même lorsqu'un filtre a le focus. Natif
contrôles ciblés tels que Local, Amis, Global, Aujourd'hui, Ce mois-ci, Tous les temps,
Retour et Continuer sont prononcés. Le classement du Parti indique également son nom
état de sélection et de confirmation.

Cette mise à jour maintient les classements de résultats silencieux pendant la sélection du mode et de la chanson.
Les noms des filtres du classement parlent en premier, suivis par les résumés des scores.
Le bouton du menu des réalisations parle normalement ; instructions du livre attendre jusqu'à
ses pages sont effectivement ouvertes et la fermeture n'est annoncée qu'après cela.

Le livre des succès du jeu annonce sa page visible et le résultat de chaque succès.
nom, description et état verrouillé ou déverrouillé. Utilisez Haut et Bas pour lire les éléments
sur une page. Les commandes gauche et droite du jeu tournent les pages comme d'habitude.

Les crédits annoncent la première ligne à l'ouverture. Utilisez le menu Haut et Bas du jeu
contrôles pour lire chaque ligne de crédit. Le défilement visuel automatique continue comme
avant. Si ces contrôles ne sont pas disponibles, les lignes sont mises en file d'attente au fur et à mesure de leur apparition ;
si cela ne fonctionne pas non plus, le texte complet du générique est annoncé une seule fois.

À l'intérieur des contrôles, le mod lit TAPER, TAPER (Joueur 2), BALAYER, TOURNER, PIVOTER, TIRER,
et Réinitialiser aux valeurs par défaut. Il lit la liaison actuelle pour le périphérique d'entrée actif,
annonce les liaisons modifiées et lit les commentaires de reliure visibles du jeu.
Il annonce comment revenir aux paramètres une fois par visite. Lorsque la réinitialisation aux valeurs par défaut modifie un
liaison, il signale que les liaisons ont été réinitialisées.

Le mod expose une ligne native RESET GYRO et ajoute une SORTIE CHANGEMENT DE PAROLE
raccourci. Reset Gyro se trouve avec les commandes du jeu ; les lignes spécifiques au mod
restent ensemble en bas du menu, avant de réinitialiser les paramètres par défaut. CHANGEMENT
SORTIE VOCALE passe par les mêmes modes que Paramètres > Paramètres du module > SORTIE
MODE. L'ordre des modes est :
Auto, OneCore, SAPI, NVDA, JAWS, UI Automation, ZDSR, ZoomText,
Boy PC Reader, PC Talker, Sense Reader, System Access, Window-Eyes.
Les entrées par défaut du raccourci sont F9 sur le clavier et le bouton de la face ouest (X sur une Xbox
contrôleur). Le bouton Démarrer du contrôleur est réservé par le natif du jeu
Actions des menus. Le choix en cours est annoncé lorsque le raccourci est utilisé et
est enregistré par le même paramètre de mode de sortie.

Le mod ajoute neuf lignes au menu Contrôles du jeu : Groupe Précédent,
Groupe suivant, Date précédente, Date suivante, LIRE LES DESCRIPTIONS, LIRE LE SCORE,
BASCULER LA PAROLE, PARLER DES CONSEILS et CHANGER LA SORTIE DE LA PAROLE. Les quatre premiers
répondre aux filtres du classement
atteint avec O/P et K/L sur la disposition du clavier par défaut, ou les pare-chocs et
D-pad gauche/droite sur une manette. Concentrez une ligne pour entendre sa liaison actuelle,
puis utilisez l'action normale TAPER/confirmation du jeu pour le relier. Les lignes défilent
à l’intérieur du panneau Contrôles existant. LIRE LES DESCRIPTIONS peut également être rebondi pour
clavier et contrôleur. Sa liaison est enregistrée par le mod et réinitialisée aux valeurs par défaut
restaure G et LT. READ SCORE peut également être rebondi pour le clavier et le contrôleur ;
La réinitialisation aux valeurs par défaut rétablit T et appuyez sur le joystick gauche. La reliure originale du jeu
Les lignes et les quatre lignes du classement utilisent le même flux de reliure de contrôles.
TOGGLE SPEECH peut être rebondi pour le clavier et le contrôleur. Ses fixations sont
enregistré par le mod, et Réinitialiser aux valeurs par défaut restaure F8 et Select. Si la liaison
est modifié alors que la parole est désactivée, le mod annonce les nouveaux contrôles de récupération.
PARLER DES CONSEILS peut également être rebondi. Ses valeurs par défaut sont H et le stick droit.
Il prononce immédiatement l'indice de l'écran actuel sans en programmer une seconde.
premier indice automatique. Les répétitions configurées peuvent toujours suivre. C'est silencieux
pendant le jeu et les signaux d'étalonnage audio chronométrés.
CHANGEMENT DE SORTIE PAROLE peut être rebondi pour le clavier et le contrôleur ; Réinitialiser à
Par défaut, restaure F9 et le bouton de la face Ouest. Si une nouvelle liaison est déjà
affecté à un autre jeu ou action de mod, le menu Contrôles rejette le
dupliquer et conserver l'affectation précédente. RESET GYRO peut être rebondi en
la même procédure de contrôles natifs que les autres actions du jeu.
Le retour de la sélection de chansons au menu principal restaure les indications du menu principal, même
lorsqu'un gestionnaire de jeu en cache signale toujours un ancien état de jeu. Astuce pour les boutons
les minuteries et la sélection automatique du périphérique d'indice suivent le jeu et le mod attribués
contrôles; les touches inutilisées telles qu'une touche de contrôle non attribuée ne les réinitialisent pas.
Cette mise à jour empêche la ligne de contrôles obsolète d'annoncer « Échec de la reliure ».
à plusieurs reprises après la fin de la scène. Lire les descriptions n'est plus temporairement
remplace toutes les propres liaisons d'entrée du jeu.

Dans Audio Calibration, le mod lit les commandes Calibrate, Back et TAPER,
annonce les instructions et les étapes d'étalonnage, lit le compte à rebours de l'échauffement,
et annonce le résultat de latence affiché. Il ne parle pas à chaque battement pendant
l'exercice de chronométrage pour que le rythme reste audible. Après votre dernière entrée de calibrage, le mod dit immédiatement « Terminé ! » ; cessez de taper et attendez le résultat mesuré. Si le calibrage échoue parce qu’aucune entrée n’a été effectuée, il dit « Échec du calibrage. ».

L'écran de pause annonce Pause, le bouton Reprendre ou Menu principal qui a le focus, ainsi que ses aides de touches. Un changement de focus interrompt la lecture précédente du menu de pause. Reprendre ou quitter la partie arrête la parole restante avant de poursuivre le jeu ou le menu principal. Pendant une partie, le mod laisse les commandes vocales du jeu et le score en cours inchangés.

Installer
-------
Le référentiel source ne contient aucun mod compilé ni DLL Prism. Construisez le mod en
suivant README.md, puis fermez le jeu et copiez BopItAccess.dll dans ses Mods
dossier. Obtenez le Windows x64 officiel Prism v0.18.3 prism.dll auprès de
https://github.com/ethindp/prism/releases et placez-le à côté de l'exécutable du jeu,
pas à l'intérieur des Mods. Copiez le dossier de documentation du build dans le dossier du jeu,
y compris ses sous-dossiers de langue traduite. Démarrez votre lecteur d'écran si vous
utilisez-en un, puis démarrez Bop It! à Steam. Prism peut utiliser SAPI lorsqu'un
le lecteur d'écran ne fonctionne pas. Le guide du jeu charge le HTML à partir du
dossier de documentation à chaque ouverture. Ce mod a été développé pour MelonLoader
0.7.3 bêta ouverte et Bop It! (Unity 2022.3.50f1, x64).
Les premières traductions non anglaises ont été réalisées avec la traduction automatique
et doivent être révisés par des locuteurs parlant couramment. Veuillez signaler une formulation peu claire ou incorrecte.
Les noms des actions de gameplay utilisent les termes traduits du jeu. Shapes, Space, City,
et Office restent en anglais sous forme de titres de scène fixes. Si la voix système de SAPI le fait
ne prononcez pas bien votre langue, sélectionnez une voix installée appropriée dans Mod
Paramètres.

Essayez les menus et les écrans
-------------------------
Attendez l'annonce sur l'écran titre si elle apparaît, puis utilisez TAPER pour ouvrir le
menu principal. Le jeu peut prendre plusieurs secondes après le message prêt du mod pour
accepter cette entrée.
Lors d'une première exécution, l'écran de bienvenue apparaît avant le menu principal. Sélectionnez son
message pour réentendre l’introduction. Choisissez Ouvrir les paramètres du module, lire les paramètres de l'utilisateur
Guider ou Continuer le jeu. Speak Hints nomme son clavier actuel et
affectations de contrôleur dans le message de bienvenue, quel que soit le type d'indices.
Ouvrez Play et déplacez-vous entre les quatre modes. Choisissez-en un pour accéder à la sélection de chansons.
TOURNER pour parcourir les thèmes et TIRER pour activer ou désactiver le mode Extrême. Le
le mod annonce chaque changement. Appuyez sur G ou LT pour entendre l'étape actuellement sélectionnée
descriptif. Appuyez sur H ou sur le joystick droit pour entendre le texte du didacticiel du mode,
les contrôles d'action actuellement attribués et les conseils sur les boutons. TAPER démarre le mode choisi ;
Le retour revient. Durant un tour, utilisez
Utilisez la commande Menu du jeu pour ouvrir Pause, puis déplacez-vous entre Reprise et Menu principal.
À la fin d'une partie, écoutez le score ou le vainqueur du One contre One avant le
les contrôles de l’écran de résultats sont annoncés. Appuyez sur T ou sur le joystick gauche pour répéter le
résultat final pendant que l'écran de fin de jeu est visible.
Appuyez sur F8 ou sur Sélectionner le contrôleur pour activer ou désactiver la parole mod depuis n'importe quel écran.
Appuyez sur F9 ou sur la manette Ouest (X sur une manette Xbox) pour faire défiler le discours
mode de sortie. Le même choix est disponible dans Paramètres > Paramètres du module > MODE DE SORTIE.
La SORTIE BRAILLE dans Paramètres > Paramètres du module est activée par défaut. Visionneuse braille de NVDA
peut afficher le braille et son équivalent texte sans affichage physique.
Pour une comparaison ON/OFF, utilisez le mode braille de suivi des curseurs du NVDA avec Show
Messages activés ; son mode d'affichage-parole-sortie refléterait la parole même
lorsque le paramètre BRAILLE OUTPUT du mod est désactivé.
Dans Paramètres > Paramètres du module, utilisez les CONSEILS DU BOUTON AUTO-SPEAK pour activer ou désactiver
instructions automatiques. Appuyez sur H ou sur le joystick droit pour entendre l'indice actuel.
sur demande.
HINTS TYPE choisit Automatique, Clavier, Contrôleur ou Les deux pour ces astuces.
BUTTON HINTS DELAY choisit s'ils accompagnent le discours de concentration ou suivent un
période d'inactivité. LES CONSEILS DU BOUTON DE RÉPÉTITION et l'INTERVALLE DE RÉPÉTITION contrôlent tout
rappels supplémentaires.
Ouvrez les classements depuis le menu principal ou un écran de résultats. Changer un filtre pour
écoutez sa nouvelle sélection et lisez les partitions individuelles avec Page Up et Page Down.
Par défaut, O/P se déplace entre les groupes du classement et K/L se déplace entre les dates.
gammes. Leurs entrées de contrôleur correspondantes sont le pare-chocs gauche/droit et
D-pad gauche/droite. Les quatre nouvelles lignes de contrôles sont destinées à les réaffecter.
Ouvrez les réalisations et tournez les pages avec gauche et droite ; utilisez Up et Down pour chacun
entrée. Ouvrez les crédits et utilisez Up et Down pour lire ses lignes indépendamment du
défilement visuel.

S'il manque la parole, vérifiez Mods\BopItAccess.log dans le dossier du jeu. Il enregistre
détection du panneau, objets d'interface utilisateur sélectionnés et initialisation et envoi Prism.
Une expédition réussie ne prouve pas en soi que la parole était audible.

Pour désactiver le mod, supprimez Mods\BopItAccess.dll., MelonLoader peut rester installé.

Fichiers et avis de tiers
-----------------------------
Prism est une bibliothèque d'accessibilité open source créée par Ethan Dupuy et ses contributeurs.
Il est sous licence Mozilla Public License, version 2.0. Cette source
le référentiel n'inclut pas prism.dll. Source, versions et licence :
https://github.com/ethindp/prism
Voir THIRD-PARTY-NOTICES.txt pour les avis de dépendance actuels.

Modifier le fichier des paramètres
----------------------------------
Si une langue inconnue, un volume trop élevé ou une voix défectueuse rend les menus difficiles à utiliser, vous pouvez modifier les réglages en dehors du jeu. Après le démarrage, le mod crée automatiquement UserData/BopItAccess.ini dans le dossier de Bop It!, à partir de vos réglages actuels. Ce fichier texte s’ouvre avec un éditeur comme le Bloc-notes.

Le fichier contient la langue du jeu, les volumes de musique, d’effets et de voix, les vibrations, le plein écran, la résolution et la latence audio ; les préférences de parole, de braille et d’indices du mod ; les profils de voix OneCore et SAPI séparés ; et les commandes du jeu et du mod destinées aux joueurs. Les résolutions disponibles et les voix installées sont indiquées dans les commentaires.

Fermez le jeu avant de modifier le fichier. Trouvez la section concernée et changez la valeur de l’entrée existante, enregistrez, puis relancez le jeu. Les changements sont lus au lancement, pas immédiatement pendant une session. Les réglages modifiés dans les menus mettent automatiquement le fichier à jour.

Les noms des sections et des réglages restent en anglais dans toutes les langues. On et Off sont les valeurs conseillées pour les interrupteurs ; True/False, Yes/No et 1/0 sont également acceptés. Les commentaires expliquent les choix et les plages de valeurs. Une entrée absente ou incorrecte conserve le réglage enregistré correspondant, sans empêcher les autres modifications valides. Les affectations de commandes en double sont refusées.

Les commentaires et les entrées inconnues sont conservés. Si un autre programme modifie le fichier pendant le jeu, le mod cesse d’y enregistrer pour le reste de la session afin de protéger vos modifications. Fermez et relancez le jeu pour les appliquer. Vous pouvez conserver une copie de sauvegarde avant de modifier le fichier.

En cas de problème de voix, indiquez Voice=System default dans la section OneCore ou SAPI. Les voix OneCore utilisent le format nom | langue ; SAPI accepte le nom affiché d’une voix installée ou son identifiant complet du Registre. Le fichier liste les choix disponibles. OutputMode=Auto essaie un lecteur d’écran compatible actif, puis OneCore, puis SAPI.

L’exemple ci-dessous rétablit l’anglais, un volume de jeu plus faible et la parole automatique avec les voix par défaut du système. Modifiez les entrées correspondantes déjà présentes dans votre fichier ; cet extrait sert de référence et ne doit pas être ajouté comme un bloc supplémentaire. Conservez vos autres réglages.

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

L’action Désinstaller de l’installeur et les Applications installées de Windows suppriment aussi UserData/BopItAccess.ini et son fichier .tmp, y compris pour les anciennes installations manuelles.

Note de transparence sur l'IA
--------------------

Ce mod a été réalisé par « vibe coding ». Tout le code a été entièrement généré et étudié par l’intelligence artificielle, avec une compréhension humaine technique limitée de son architecture sous-jacente. Veuillez utiliser ce mod à vos propres risques.

Cela étant dit, chaque fonctionnalité du mod et chaque décision de conception ont été rédigées et approuvées par des humains. Les tests n’ont jamais été automatisés ; ils ont été réalisés avec soin et de manière approfondie par de vrais joueurs et testeurs humains.

Remarque : les textes et la documentation multilingues ont été générés par l'IA et n'ont pas été révisés par des locuteurs natifs. Il faut s’attendre à une grande imprécision de traduction. Sans codage agent, ce projet n’existerait pas. Merci de lui avoir donné une chance !

Que peut-il arriver ensuite
------------------

Ce projet est pour l’essentiel terminé et aucun contenu ou fonctionnalité majeur n’est prévu. Cependant, ce mod sera activement maintenu et mis à jour au fil du temps selon les besoins, les commentaires des joueurs étant à l'origine de ces améliorations. Les travaux futurs potentiels incluent une révision plus approfondie et des corrections de bugs, un raffinement du code et des améliorations continues de la réactivité vocale. Prism crée un chemin possible vers d'autres plates-formes dans le futur, mais ce mod ne prend actuellement en charge que Windows x64. Le référentiel du projet est l'endroit idéal pour suivre le développement ultérieur.

Merci
---------

À ceux qui ont testé ce mod avant sa sortie et qui ont contribué à l'amener là où il est actuellement, merci. Vous savez tous qui vous êtes. Aux joueurs qui donnent leur avis, essaient le mod pour la première fois, ou croient en moi et en ce projet, merci. Votre soutien me motive à continuer à créer des choses dans un monde qui peut sembler fou et profondément imparfait. J'espère que ce projet vous permettra d'apprécier plus facilement le jeu et de jouer avec les autres. Merci beaucoup à tous. Profitez du Bop It!

— Christopher Shaw

MelonLoader fenêtres de démarrage
---------------------------

Le modèle Loader.cfg fourni masque l’écran de démarrage et la console séparés de MelonLoader. Le programme d'installation applique automatiquement les deux mêmes valeurs par défaut, avant toute préparation de build alpha nécessaire au démarrage du jeu. Ces paramètres ne sautent pas l’écran titre du jeu ni l’écran de bienvenue du mod.

Le jeu étant fermé, ouvrez UserData/Loader.cfg dans le dossier du jeu. Si ce fichier existe déjà, réglez disable_start_screen sur true dans sa section [loader] existante, et hide_console sur true dans sa section [console] existante. Conservez toutes les autres entrées. Si le fichier n'existe pas, copiez le modèle UserData/Loader.cfg fourni avec la compilation, ou configuration/Loader.cfg depuis le code source. Ne remplacez jamais un Loader.cfg existant par le modèle complet.

[loader]
disable_start_screen = true

[console]
hide_console = true

Le mod ne réinitialise pas ces options à chaque lancement. Vous pouvez redéfinir manuellement l’une ou l’autre valeur sur false si vous avez besoin des fenêtres du chargeur pour le dépannage. La désinstallation du programme d'installation restaure les valeurs d'origine uniquement tant que les vraies valeurs du programme d'installation sont toujours présentes, préservant ainsi les autres modifications de configuration du chargeur.

Après un changement réussi, le mod annonce l'entrée et l'action à laquelle elle est affectée, par exemple « Space attribué à TAPER ».
