Bop It Access 0.9.12 - Prism Parole et Braille

Builds source actuels : mod 0.9.12 (59 builds du mod), installateur 0.2.5. La première publication publique reste à venir.

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

ANNONCER LES TYPES DE COMMANDES est une autre bascule MOD SETTINGS enregistrée, activée par défaut. Lorsqu'il est activé,
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
Sur l’écran de sélection de chanson, le mod annonce le thème actuel
(Shapes, Space, City ou Office) et la difficulté : Classique ou Extrême.
TOURNER change la chanson et annonce seulement le nouveau thème. TIRER
change la difficulté et annonce seulement Classique ou Extrême. L’introduction
explique aussi TOURNER, TIRER, TAPER et Retour. Elle est répétée à chaque
sélection d’un mode et à chaque ouverture de cet écran, avec le thème
et la difficulté actuels.
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
TOURNER pour parcourir les thèmes et TIRER pour choisir Classique ou Extrême. Le
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

Après avoir confirmé Uninstall, choisissez Uninstall for me ou Uninstall for everyone. Les deux options suppriment les fichiers partagés du mod dans ce dossier du jeu ; le mod ne sera donc plus disponible pour aucune personne utilisant cette installation. Ce choix détermine les préférences Windows enregistrées du mod qui sont supprimées : celles du seul compte à l’origine de la demande, ou celles de tous les profils Windows locaux, y compris les profils dont la session est fermée. Les préférences du jeu d’origine sont conservées. Le SDK .NET reste installé.

Lorsque le programme d’installation supprime sa propre installation de MelonLoader et qu’aucun autre mod n’en a besoin, il supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg, ainsi que les dossiers Plugins, UserLibs et UserData lorsqu’ils sont vides. Les paramètres de Bop It Access, les journaux connus, les guides et les fichiers du programme d’installation sont supprimés. Les autres mods, les fichiers partagés du chargeur déjà présents et les fichiers non reconnus sont protégés. Un fichier inconnu peut donc laisser un dossier en place ; le programme d’installation le signale dans les diagnostics au lieu de supprimer des données sans rapport avec le mod.

Si le nettoyage ne peut pas se terminer en toute sécurité, le programme d’installation l’explique et conserve les informations nécessaires à une nouvelle tentative. Pour une installation gérée, son entrée de désinstallation Windows et son point de reprise du nettoyage sont conservés jusqu’à ce que la suppression réussisse. Une ancienne copie manuelle ne dispose pas d’un registre durable de propriété des fichiers ; dans le programme d’installation ouvert, réessayez les opérations signalées par ses avertissements. N’installez pas, ne mettez pas à jour et ne supprimez pas le mod pendant que Bop It! est en cours d’exécution.

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

Le modèle Loader.cfg fourni masque l’écran de démarrage et la console séparés de MelonLoader. L’installateur
applique ces deux valeurs par défaut avant que vous lanciez vous-même le jeu. Elles ne suppriment ni l’écran
titre du jeu ni l’écran de bienvenue du mod.

Le jeu étant fermé, ouvrez UserData/Loader.cfg dans le dossier du jeu. Si ce fichier existe déjà, réglez disable_start_screen sur true dans sa section [loader] existante, et hide_console sur true dans sa section [console] existante. Conservez toutes les autres entrées. Si le fichier n'existe pas, copiez le modèle UserData/Loader.cfg fourni avec la compilation, ou configuration/Loader.cfg depuis le code source. Ne remplacez jamais un Loader.cfg existant par le modèle complet.

[loader]
disable_start_screen = true

[console]
hide_console = true

Le mod ne réinitialise pas ces options à chaque lancement. Pour résoudre un problème, vous pouvez remettre manuellement l’une ou l’autre valeur à false. Si la désinstallation conserve une installation partagée de MelonLoader, elle restaure uniquement les indicateurs ciblés par le programme d’installation qui n’ont pas été modifiés et préserve les autres modifications. Si elle supprime sa propre installation inutilisée de MelonLoader, elle supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg.

Après un changement réussi, le mod annonce l'entrée et l'action à laquelle elle est affectée, par exemple « Space attribué à TAPER ».

Choisir le bon téléchargement

La première publication publique sur GitHub prévoit les quatre téléchargements suivants. Ce sont des fichiers à venir, pas encore disponibles ; aucune publication publique ni étiquette n’a été créée. Utilisez jusque-là un installateur fourni ou le code source. Une archive source n’est pas le ZIP d’installation compilé.

https://github.com/Chris-E-Shaw/BopItAccess/releases

- BopItAccess-Installer.exe: L’installateur autonome pour Windows x64. Il trouve le jeu et gère dépendances, installation, mises à jour, diagnostics et suppression. Il n’est pas signé.
- BopItAccess-v1.0.zip: Le paquet compilé du mod pour une installation manuelle sans lancer l’EXE Bop It Access. Il comprend Mods/BopItAccess.dll, prism.dll, toute la documentation et les licences Prism, un modèle Loader.cfg, README.txt et le raccourci de désinstallation. MelonLoader, .NET, les fichiers du jeu et les assemblies générés ne sont pas inclus.
- Source code (zip): Le ZIP du code source de la version, généré automatiquement par GitHub. Pour lire ou compiler le code ; ce n’est pas le paquet compilé du mod.
- Source code (tar.gz): Le même code source dans une archive tar compressée avec gzip. Un autre format source, pas un autre installateur du mod.

Installateur non signé et avertissements Windows 11

Cet installateur n’est pas signé. Un programme non signé ou peu connu peut déclencher SmartScreen ou l’antivirus, y compris de possibles faux positifs ; cela ne prouve pas que toute détection est erronée. Obtenez le fichier seulement auprès du projet officiel Bop It Access ou d’un envoi direct fiable et décidez si vous lui faites confiance. Le ZIP compilé évite cet EXE. Ne désactivez pas l’antivirus et n’excluez pas un lecteur ou tout le dossier du jeu.
https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app

Autoriser une détection Defender précise

Appuyez sur Win+I, puis Confidentialité et sécurité > Sécurité Windows > Ouvrir Sécurité Windows > Protection contre les virus et menaces > Historique de protection (parfois appelé historique des menaces). Développez l’élément correspondant à l’installateur. Avec Tab, allez sur Actions ou Autres actions, appuyez sur Entrée et choisissez Autoriser sur l’appareil ou Autoriser ; acceptez la demande administrateur si nécessaire. Un fichier en quarantaine peut nécessiter Restaurer d’abord, puis une autorisation s’il est détecté à nouveau. S’il a été supprimé, retéléchargez-le depuis le projet officiel. Vérifiez l’élément exact avant d’autoriser.
https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app · https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq

Exclusions Defender facultatives et limitées

Dans Protection contre les virus et menaces, choisissez Gérer les paramètres sous les paramètres de protection, puis Exclusions > Ajouter ou supprimer des exclusions. Acceptez la demande administrateur avec Oui si affichée. Choisissez Ajouter une exclusion > Processus, saisissez exactement BopItAccess-Installer.exe et appuyez sur Entrée. Le nom doit correspondre à l’exécutable réellement lancé.

L’exclusion Processus de Microsoft couvre les fichiers ouverts par ce processus ; elle n’exclut pas l’EXE de l’installateur, ne restaure pas une quarantaine et ne contourne pas SmartScreen. Si Defender détecte l’EXE lui-même et que vous lui faites confiance, une exclusion facultative Fichier ciblant cet EXE téléchargé est l’alternative limitée pertinente. Retirez les exceptions devenues inutiles.
https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview

SmartScreen est un avertissement séparé. Si vous faites confiance à cet EXE et que Windows le propose, choisissez Informations complémentaires > Exécuter quand même. L’autorisation Defender ou l’exclusion Processus ne contourne pas cet avertissement ; une stratégie peut empêcher l’exécution.

Étapes vérifiées le 9 octobre 2026 pour Windows 11 25H2, build 26200.9550. Les libellés peuvent varier ; aucun essai de l’interface n’a été effectué.

Programme d’installation Windows 0.2.5
--------------------------------------

Installer, mettre à jour ou supprimer avec l’installateur

1. Fermez le jeu, lancez BopItAccess-Installer.exe et acceptez la demande administrateur Windows. Lisez le champ Welcome and controls, puis vérifiez Game folder ou utilisez Browse. Welcome and controls est en lecture seule, sélectionnable et reçoit le premier focus ; Alt+W y revient.
2. Choisissez Install pour la dernière version publique compilée quand elle sera disponible. Avant la première publication, Show advanced affiche Install alpha, qui demande confirmation et compile les dernières sources sur votre ordinateur. Attendez le message de réussite. Play Bop It! The Video Game lance ensuite le jeu par Steam uniquement sur votre demande.
3. Pour une installation existante, ouvrez l’installateur avec le jeu fermé et vérifiez son état. Update apparaît si une version publique plus récente est trouvée. Choisissez Update et attendez la fin ; vos réglages enregistrés sont conservés. Install alpha est le choix séparé des dernières sources, pas la mise à jour publique.
4. Pour supprimer le mod, choisissez Uninstall, confirmez, puis Uninstall for me ou Uninstall for everyone. Les deux retirent les fichiers partagés du mod de ce dossier du jeu. Le choix détermine si les préférences Windows du mod sont supprimées pour votre compte ou tous les profils locaux ; les préférences originales du jeu restent. Applications installées de Windows suit le même parcours. Vérifiez le résultat avant Quit.

Le tableau suivant décrit toutes les actions et tous les champs de texte de la fenêtre principale. Installer status et Installation progress sont des informations, pas des boutons. Welcome and controls contient les instructions réutilisables ; Status log contient les messages changeants. Abort demande confirmation avant d’annuler une installation active ; Quit suit les mêmes règles d’arrêt sûr. Les dialogues proposent Keep open/Quit, confirmation/annulation et les deux portées de préférences à supprimer. Show advanced change seulement les actions visibles.

Utilisez BopItAccess-Installer-0.2.5.exe ou BopItAccess-Installer.exe fourni par le projet. Les deux noms contiennent le même programme d’installation autonome pour Windows x64. Le projet comprend son code source ; aucun binaire compilé public ni GitHub Release n’est encore publié.

Fermez Bop It!, ouvrez le programme d’installation et approuvez la demande d’autorisation administrateur de Windows. Le programme d’installation vous accueille, recherche le jeu dans les bibliothèques Steam de tous les lecteurs disponibles et tente de placer sa fenêtre au premier plan. Vérifiez le dossier du jeu affiché ; utilisez Browse si vous devez choisir un autre dossier. La touche Tab permet de passer d’une commande à l’autre. Le journal d’état est un champ de texte en lecture seule : placez-y le focus pour consulter les messages avec les touches de déplacement du curseur, sélectionner du texte ou le copier.

Le programme d’installation 0.2.5 demande brièvement l’activation au premier plan et le focus du clavier au démarrage. Si une autre fenêtre reste active à la fin de sa courte observation initiale, son titre et son bouton de barre des tâches clignotent et il demande de passer au programme d’installation avec Alt+Tab. Activez-le avant d’utiliser ses commandes au clavier ou à la manette. Alt+G place le focus sur le champ du dossier du jeu.

Show advanced est décoché à l’ouverture du programme d’installation. Cette case affiche Install alpha, Save diagnostics et Copy diagnostics. Install télécharge la dernière version publique publiée sur GitHub lorsqu’il en existe une. Aucune version publique n’est encore disponible ; les testeurs doivent donc actuellement utiliser Show advanced et Install alpha. L’installation alpha demande confirmation, télécharge les sources les plus récentes et les compile sur votre ordinateur. Update apparaît lorsqu’une version publique plus récente est détectée pour une copie déjà installée.

Les messages d’état expliquent simplement ce qui est en cours de téléchargement, d’installation ou de finalisation. Une seule barre indique la progression estimée de l’ensemble de l’installation, sans revenir à zéro pour chaque téléchargement ou fichier. Elle avance par incréments de cinq points de pourcentage ; certaines étapes de préparation peuvent prendre du temps sans changement visible. Le message d’accueil, la disponibilité d’une nouvelle mise à jour et la confirmation de la copie des diagnostics sont transmis à votre lecteur d’écran par les notifications d’accessibilité de Windows. Leur lecture à voix haute dépend de votre lecteur d’écran et de sa prise en charge des notifications Windows.

Le programme d’installation 0.2.5 ne lance jamais Bop It! pendant l’installation. L’installation alpha réutilise les fichiers de compilation locaux correspondants ou prépare des fichiers temporaires à partir de votre propre jeu installé, qui reste fermé. Le programme d’installation place ensuite MelonLoader dans le dossier du jeu et ajoute immédiatement Mods/BopItAccess.dll, puis Prism, les paramètres, la documentation complète et les éléments nécessaires à la désinstallation. Attendez le message de réussite, puis lancez vous-même le jeu depuis Steam lorsque vous êtes prêt.

Après une installation réussie, Play Bop It! The Video Game apparaît. Activez ce bouton pour lancer vous-même le jeu depuis Steam lorsque vous êtes prêt. Le programme d’installation ne démarre jamais le jeu automatiquement pendant l’installation.

Une version compilée nécessite l’environnement d’exécution .NET 6 pour Windows x64, et non un SDK de développement. Les environnements d’exécution complets déjà présents sont réutilisés. Si l’environnement d’exécution manque, il est téléchargé auprès de Microsoft et placé dans MelonLoader/Dependencies/dotnet. Install alpha nécessite également un SDK .NET compatible et le pack de ciblage .NET 6 : un SDK existant est réutilisé, ou le SDK officiel de Microsoft est installé pour tout le système. Le programme d’installation ne crée aucun nouveau dossier SDK à la racine du jeu. MelonLoader 0.7.3 Open-Beta et Prism 0.18.3 proviennent de leurs versions officielles. Les composants Microsoft .NET partagés et les SDK restent installés après un abandon ou une désinstallation.

Quit ferme le programme d’installation. Si l’installation est encore en cours, il demande s’il faut l’abandonner et annuler ses modifications avant de fermer ; Keep open poursuit normalement l’opération. Si l’installation se termine pendant que vous prenez votre décision, la boîte de dialogue se met à jour pour indiquer qu’elle est terminée, et Quit n’annule pas l’installation achevée. Une fois la suppression commencée, la désinstallation se termine en toute sécurité avant la fermeture. Abort demande également confirmation et annule les modifications des fichiers du jeu effectuées lors de cette tentative. Une annulation pendant l’installation de Microsoft .NET attend que l’installation de ces composants partagés se termine en toute sécurité.

Après avoir confirmé Uninstall, choisissez Uninstall for me ou Uninstall for everyone. Les deux options suppriment les fichiers partagés du mod dans ce dossier du jeu ; le mod ne sera donc plus disponible pour aucune personne utilisant cette installation. Ce choix détermine les préférences Windows enregistrées du mod qui sont supprimées : celles du seul compte à l’origine de la demande, ou celles de tous les profils Windows locaux, y compris les profils dont la session est fermée. Les préférences du jeu d’origine sont conservées. Le SDK .NET reste installé.

Lorsque le programme d’installation supprime sa propre installation de MelonLoader et qu’aucun autre mod n’en a besoin, il supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg, ainsi que les dossiers Plugins, UserLibs et UserData lorsqu’ils sont vides. Les paramètres de Bop It Access, les journaux connus, les guides et les fichiers du programme d’installation sont supprimés. Les autres mods, les fichiers partagés du chargeur déjà présents et les fichiers non reconnus sont protégés. Un fichier inconnu peut donc laisser un dossier en place ; le programme d’installation le signale dans les diagnostics au lieu de supprimer des données sans rapport avec le mod.

Le programme d’installation reste ouvert après la désinstallation pour vous permettre d’examiner le résultat, d’enregistrer les diagnostics ou de réinstaller. Choisissez Quit lorsque vous avez terminé. L’utilitaire de désinstallation en cours d’exécution et les fichiers de diagnostic automatiques sont nettoyés après la fermeture de la fenêtre. Une réinstallation dans la même fenêtre ouvre un nouveau dossier de suivi de l’installation ; le nettoyage différé ne peut pas supprimer la nouvelle installation.

La page Applications installées de Windows utilise les mêmes étapes de confirmation, de choix des préférences et de nettoyage. Le programme d’installation fournit BopItAccess-uninstall.ps1 dans le dossier du jeu comme raccourci vers le programme de désinstallation installé ; les futures compilations à partir des sources incluent également ce script dans leurs fichiers de sortie. Copier manuellement ce script n’installe pas le programme de désinstallation lui-même. Pour une ancienne installation manuelle sans registre de propriété des fichiers, le programme d’installation supprime les fichiers du mod identifiables et conserve les fichiers partagés dont l’origine ne peut pas être établie.

Si le nettoyage ne peut pas se terminer en toute sécurité, le programme d’installation l’explique et conserve les informations nécessaires à une nouvelle tentative. Pour une installation gérée, son entrée de désinstallation Windows et son point de reprise du nettoyage sont conservés jusqu’à ce que la suppression réussisse. Une ancienne copie manuelle ne dispose pas d’un registre durable de propriété des fichiers ; dans le programme d’installation ouvert, réessayez les opérations signalées par ses avertissements. N’installez pas, ne mettez pas à jour et ne supprimez pas le mod pendant que Bop It! est en cours d’exécution.

Raccourcis clavier du programme d’installation
----------------------------------------------

Welcome and controls: Alt+W. Le champ Welcome and controls séparé liste les raccourcis de lecture du texte à la manette ; Alt+W y revient et Alt+L ouvre le Status log changeant. Les deux champs sont en lecture seule, sélectionnables et consultables. Show advanced annonce coché ou décoché. Tout sélectionner confirme la réussite ou un champ vide. Au clavier, Ctrl+A sélectionne tout le texte et Ctrl+C copie la sélection.
Dossier du jeu: Alt+G. Placer le focus sur le champ du dossier du jeu.
Browse: Alt+B. Choisir le dossier du jeu.
Install: Alt+I. Installer la dernière version publique lorsqu’elle est disponible.
Install alpha: Alt+A. Confirmer et compiler les sources les plus récentes ; visible avec Show advanced.
Update: Alt+U. Installer une version publique plus récente lorsqu’elle est proposée.
Play Bop It! The Video Game: Alt+P. Lancer le jeu depuis Steam ; disponible après une installation réussie.
Uninstall: Alt+N. Confirmer la suppression et choisir les comptes dont les préférences Windows du mod seront supprimées.
Abort: Alt+R. Confirmer l’annulation de l’installation en cours.
Journal d’état: Alt+L. Placer le focus sur les messages d’état en lecture seule dont le texte peut être sélectionné.
Show advanced: Alt+V. Afficher ou masquer l’installation alpha et les outils de diagnostic.
Save diagnostics: Alt+D. Enregistrer la session de diagnostic complète et poursuivre son enregistrement ; visible avec Show advanced.
Copy diagnostics: Alt+C. Copier l’instantané complet des diagnostics ; visible avec Show advanced.
Quit: Alt+Q. Fermer, avec une gestion sûre de l’annulation si une opération est en cours.

Utiliser une manette dans le programme d’installation
-----------------------------------------------------

Le programme d’installation prend en charge les manettes de type Xbox et les autres manettes que Windows rend accessibles par XInput. Ses commandes sont distinctes des commandes personnalisables du jeu. La croix directionnelle ou le stick gauche permet de passer d’une commande à l’autre ; lorsqu’un champ de texte a le focus, les directions servent à parcourir son texte. Les boutons de tranche passent toujours à la commande précédente ou suivante pouvant recevoir le focus. A active le bouton ou la case à cocher ayant le focus. Les commandes de la manette sont traitées uniquement lorsque ce programme d’installation ou l’une de ses propres boîtes de dialogue est au premier plan.

B revient en arrière ou annule une boîte de dialogue ; dans la fenêtre principale du programme d’installation, il demande l’abandon d’une installation en cours, sinon il correspond à Quit. Start correspond à Quit dans la fenêtre principale et revient en arrière dans une boîte de dialogue. Y (le bouton supérieur en façade) sélectionne tout le texte lorsqu’un champ de texte du programme d’installation a le focus. Hors des champs de texte de la fenêtre principale, Y bascule Show advanced. Dans le journal d’état ou un autre champ de texte du programme d’installation, la croix directionnelle ou le stick gauche agit comme les touches fléchées : Gauche/Droite se déplace par caractère et Haut/Bas par ligne. Maintenez LT comme Ctrl : Gauche/Droite se déplace par mot et Haut/Bas par paragraphe. Maintenez RT comme Maj pour étendre la sélection ; maintenez LT et RT ensemble pour sélectionner des mots ou des paragraphes. X copie uniquement le texte sélectionné ; sélectionnez d’abord la partie souhaitée. Ctrl+C au clavier continue de copier la sélection. Lorsqu’aucun texte n’est sélectionné, le programme d’installation envoie aussi des notifications accessibles pour le caractère, le mot, la ligne ou le paragraphe à la position du curseur. Le programme d’installation envoie une confirmation accessible quand le texte est copié et signale une sélection vide ou un échec de copie. L’annonce vocale dépend de la prise en charge des notifications Windows par votre lecteur d’écran. La navigation à la manette dans les boîtes de dialogue natives de Windows de sélection de dossier et d’enregistrement nécessite encore une vérification humaine. Un clavier reste disponible pour saisir un dossier ou un nom de fichier. Les manettes sans prise en charge de XInput ne sont pas couvertes par cette implémentation.

Le champ Welcome and controls séparé liste les raccourcis de lecture du texte à la manette ; Alt+W y revient et Alt+L ouvre le Status log changeant. Les deux champs sont en lecture seule, sélectionnables et consultables. Show advanced annonce coché ou décoché. Tout sélectionner confirme la réussite ou un champ vide. Au clavier, Ctrl+A sélectionne tout le texte et Ctrl+C copie la sélection.

Le programme d’installation 0.2.5 demande que chaque annonce vocale émise remplace la parole précédente de l’installateur, notamment la lecture du texte, Tout sélectionner, l’état coché/décoché de Show advanced, Copy diagnostics et les autres confirmations. LB/RB continue d’annoncer la nouvelle commande ayant le focus. La fréquence des annonces d’état reste identique ; chaque entrée du journal n’est pas lue automatiquement. L’interruption réelle dépend de la prise en charge des notifications Windows par le lecteur d’écran et attend encore une vérification humaine.

Diagnostics du programme d’installation
---------------------------------------

Show advanced affiche Save diagnostics (Alt+D) et Copy diagnostics (Alt+C). Les journaux automatiques en UTF-8 sont conservés localement dans %ProgramData%\BopItAccess\diagnostics. Save diagnostics écrit l’intégralité de la session en cours dans le fichier .log ou .txt que vous choisissez et continue de l’enregistrer jusqu’à la fermeture du programme d’installation ; Copy diagnostics copie un instantané et fournit une confirmation accessible. Enregistrez avant un essai d’installation ou de désinstallation pour que votre enregistrement soit conservé après le nettoyage des journaux automatiques. Les détails techniques relatifs aux fichiers, aux téléchargements, à la compilation et aux erreurs y sont conservés, même si le champ d’état affiche des messages plus courts. Rien n’est envoyé en ligne. Les journaux peuvent contenir des noms d’utilisateur Windows et des chemins complets : relisez-les avant de les partager. Les copies exportées volontairement restent présentes après la désinstallation.

Au premier lancement manuel après l’installation de MelonLoader, celui-ci peut télécharger des fichiers de prise en charge et préparer les assemblies du jeu. Prévoyez environ une minute, voire davantage sur certains systèmes. Le mod ne peut pas parler tant que MelonLoader ne l’a pas chargé. Gardez le jeu ouvert et attendez l’annonce de démarrage de Bop It Access, puis l’annonce de l’écran titre, de bienvenue ou du menu principal avant d’utiliser les commandes du jeu.


Installer le ZIP compilé sans l’EXE Bop It Access

Lorsque BopItAccess-v1.0.zip sera publié, ce parcours utilisera la DLL déjà compilée et ne nécessitera pas le SDK .NET. Il faut toujours votre jeu acheté pour Windows x64, MelonLoader officiel x64 0.7.3 Open-Beta et le runtime .NET 6 Windows x64. Suivez les instructions officielles de MelonLoader et Microsoft ; le ZIP ne fournit pas ces prérequis.

https://github.com/LavaGang/MelonLoader#how-to-use-the-installer
https://dotnet.microsoft.com/en-us/download/dotnet/6.0

1. Installez le jeu par Steam et trouvez son dossier. Fermez Bop It! avant de modifier les fichiers ; utilisez au besoin la fonction Steam pour parcourir les fichiers installés.
2. Installez MelonLoader officiel x64 dans ce dossier et vérifiez que le runtime .NET 6 x64 est installé. Ne lancez pas encore le jeu : placez d’abord le mod.
3. Extrayez BopItAccess-v1.0.zip compilé dans un dossier temporaire. Copiez Mods/BopItAccess.dll dans Mods du jeu, en créant ou fusionnant ce dossier sans supprimer d’autres mods. Copiez prism.dll à côté de BopIt!.exe.
4. Copiez entièrement documentation et THIRD-PARTY-LICENSES, avec les sous-dossiers de langues et les notices/licences Prism. Copiez README.txt et BopItAccess-uninstall.ps1 du paquet. Ce script ouvre seulement un désinstallateur géré par l’installateur ; le copier ne crée ni désinstallateur fonctionnel ni inscription dans Applications installées.
5. Pour UserData/Loader.cfg : copiez le modèle si le fichier manque. Sinon, fusionnez seulement [loader] disable_start_screen=true et [console] hide_console=true dans les bonnes sections et conservez les autres réglages. N’écrasez pas une configuration existante avec le modèle.
6. Démarrez votre lecteur d’écran si utilisé, puis lancez le jeu par Steam. MelonLoader peut télécharger des fichiers auxiliaires et générer les assemblies au premier lancement, avec le mod déjà dans Mods. Attendez les annonces de démarrage du mod et du menu avant de jouer.

Pour une mise à jour manuelle, fermez le jeu et recopiez les fichiers du mod, Prism, documentation et licences du nouveau paquet aux mêmes endroits. Gardez BopItAccess.ini, les autres mods et les fichiers étrangers ; fusionnez Loader.cfg comme indiqué. Pour désactiver/supprimer le mod manuel, retirez seulement Mods/BopItAccess.dll. Pour un nettoyage plus complet, retirez uniquement les fichiers copiés pour ce mod et UserData/BopItAccess.ini ou son fichier .tmp ; gardez Prism/MelonLoader s’ils sont partagés. Des préférences Windows peuvent rester. Le ZIP manuel n’a ni registre de propriété ni désinstallateur enregistré. Si vous choisissez ensuite l’installateur, Uninstall peut reconnaître une ancienne copie manuelle et nettoyer les préférences en protégeant les fichiers d’origine inconnue. Les journaux du mod sont Mods/BopItAccess.log et Mods/BopItAccess.log.previous ; ne retirez que ces journaux connus lors du nettoyage.

Conditionnement avancé : scripts/package-mod.ps1 empaquette un mod déjà compilé correspondant et les fichiers connus de documentation/configuration/Prism. Il vérifie les versions source/DLL et exclut les fichiers du jeu, générés ou anciens ; il ne compile pas. Archive et préparation restent locales.
