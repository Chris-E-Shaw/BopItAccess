# Bop It Access

Bop It Access est un mod d'accessibilité non officiel pour la version Windows Steam de **Bop It!**. Il utilise MelonLoader et [Prism](https://github.com/ethindp/prism) pour ajouter des commentaires vocaux et braille aux menus et aux écrans de jeu. Les fonctionnalités actuelles incluent un écran de bienvenue de première exécution, un guide de l'utilisateur dans le jeu, des écrans de titre et de pause parlés, des paramètres et des commandes, une sélection de chansons, des scores finaux et des classements, des réalisations, des crédits, des conseils sur les boutons, un texte de didacticiel à la demande avec les affectations de contrôle actuelles avant un tour et des descriptions des quatre étapes. La version 0.9.0 utilise Prism pour la sortie vocale et braille. Le mod suit la langue sélectionnée du jeu et comprend un guide pour chaque langue proposée par le jeu.

## Modifier le fichier des paramètres

Si une langue inconnue, un volume trop élevé ou une voix défectueuse rend les menus difficiles à utiliser, vous pouvez modifier les réglages en dehors du jeu. Après le démarrage, le mod crée automatiquement UserData/BopItAccess.ini dans le dossier de Bop It!, à partir de vos réglages actuels. Ce fichier texte s’ouvre avec un éditeur comme le Bloc-notes.

Le fichier contient la langue du jeu, les volumes de musique, d’effets et de voix, les vibrations, le plein écran, la résolution et la latence audio ; les préférences de parole, de braille et d’indices du mod ; les profils de voix OneCore et SAPI séparés ; et les commandes du jeu et du mod destinées aux joueurs. Les résolutions disponibles et les voix installées sont indiquées dans les commentaires.

Fermez le jeu avant de modifier le fichier. Trouvez la section concernée et changez la valeur de l’entrée existante, enregistrez, puis relancez le jeu. Les changements sont lus au lancement, pas immédiatement pendant une session. Les réglages modifiés dans les menus mettent automatiquement le fichier à jour.

Les noms des sections et des réglages restent en anglais dans toutes les langues. On et Off sont les valeurs conseillées pour les interrupteurs ; True/False, Yes/No et 1/0 sont également acceptés. Les commentaires expliquent les choix et les plages de valeurs. Une entrée absente ou incorrecte conserve le réglage enregistré correspondant, sans empêcher les autres modifications valides. Les affectations de commandes en double sont refusées.

Les commentaires et les entrées inconnues sont conservés. Si un autre programme modifie le fichier pendant le jeu, le mod cesse d’y enregistrer pour le reste de la session afin de protéger vos modifications. Fermez et relancez le jeu pour les appliquer. Vous pouvez conserver une copie de sauvegarde avant de modifier le fichier.

En cas de problème de voix, indiquez Voice=System default dans la section OneCore ou SAPI. Les voix OneCore utilisent le format nom | langue ; SAPI accepte le nom affiché d’une voix installée ou son identifiant complet du Registre. Le fichier liste les choix disponibles. OutputMode=Auto essaie un lecteur d’écran compatible actif, puis OneCore, puis SAPI.

L’exemple ci-dessous rétablit l’anglais, un volume de jeu plus faible et la parole automatique avec les voix par défaut du système. Modifiez les entrées correspondantes déjà présentes dans votre fichier ; cet extrait sert de référence et ne doit pas être ajouté comme un bloc supplémentaire. Conservez vos autres réglages.

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

Les fichiers installés sont consignés dans un manifeste de propriété afin que les mises à jour préservent les fichiers préexistants et qu’une installation annulée puisse revenir sur ses propres modifications. **Désinstaller** et les **Applications installées** de Windows utilisent le même code de désinstallation. Le fichier fourni `installer/uninstall.ps1` ouvre une fenêtre accessible de désinstallation depuis les Applications installées. Après une suppression entièrement réussie, il supprime le lanceur de désinstallation, les registres de propriété et l’entrée Windows. Les autres mods préexistants et les fichiers MelonLoader partagés sont conservés. Le nettoyage des installations gérées comme des anciennes installations manuelles couvre les journaux connus du mod, dont `Mods/BopItAccess.log.previous`, `UserData/BopItAccess.ini`, l’ancien fichier `UserData/BopItAccess.ini.tmp`, ainsi que les résidus validés de `BopItAccess.ini.<GUID>.tmp` dans `UserData`. Ici, `<GUID>` doit comporter exactement 32 caractères hexadécimaux sans tirets ; les fichiers quelconques correspondant à un joker large ne sont pas supprimés.

Pour une ancienne copie installée manuellement sans manifeste de propriété, la désinstallation supprime les fichiers Bop It Access identifiables et laisse les fichiers partagés dont l’origine ne peut pas être prouvée. La désinstallation supprime également uniquement les valeurs de préférences `BopItAccess.*` de chaque profil utilisateur Windows local, y compris les profils déconnectés. Les préférences propres au jeu et le SDK .NET restent en place. Si Windows refuse l’accès ou si une autre étape de nettoyage ne peut pas se terminer en sûreté, l’installateur signale un nettoyage incomplet. Pour une copie gérée par l’installateur, son entrée Windows, son lanceur de désinstallation et son point de reprise durable restent disponibles jusqu’à la réussite du nettoyage, afin de réessayer les étapes restantes. Une ancienne installation manuelle n’a pas ce registre de propriété durable ; ses avertissements permettent une nouvelle tentative dans l’installateur ouvert.

## Statut du projet

Ce projet est pour l’essentiel terminé et aucun contenu ou fonctionnalité majeur n’est prévu. Il sera maintenu selon les besoins, les commentaires des joueurs guidant les améliorations. Le référentiel GitHub contient le code source et la documentation technique. **Il n'y a pas encore de versions GitHub.** La source contient désormais également un projet d'installation Windows. Jusqu'à ce qu'une version soit publiée, son bouton **Installer** explique qu'aucune version n'est disponible ; **Install alpha** crée le dernier commit de branche principale à partir des sources.

L'historique des validations comprend des instantanés source reconstruits de 37 versions antérieures. Les commits ont été créés lorsque ces archives ont été importées dans Git ; leurs dates ne sont pas les dates de construction d'origine. Le [historique de construction technique](BopItAccess-build-history.html) décrit le travail derrière chaque instantané.

## Exigences

- Windows x64 et votre propre installation de Bop It! pour Steam.
- MelonLoader installé dans le répertoire du jeu. Le développement a utilisé MelonLoader **0.7.3 Open-Beta** avec la version de jeu x64 Unity **2022.3.50f1**. D'autres combinaisons n'ont pas été vérifiées.
- Un SDK .NET avec le **pack de ciblage .NET 6**, car le mod cible `net6.0`.
- Pour l'installation, le officiel Windows x64 Prism v0.18.3 `prism.dll`. Ce binaire tiers n'est pas dans ce référentiel.

Le mod fait référence aux DLL générées ou installées par MelonLoader sous le répertoire du jeu. Il n’inclut ni ne redistribue les assemblages de jeux.

<a id="build-from-source"></a>
## Construire à partir des sources

1. Installez MelonLoader, démarrez Bop It! une fois, puis fermez le jeu. MelonLoader devrait créer `MelonLoader\Il2CppAssemblies` sous le répertoire du jeu.
2. Clonez ou téléchargez ce référentiel. Ouvrez PowerShell dans le répertoire racine du référentiel.
3. Ensemble `$gameDir` dans **votre** répertoire d'installation Bop It!, puis compilez :

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

   L'exemple de chemin est l'emplacement Windows habituel de Steam. Changez-le si votre bibliothèque Steam se trouve ailleurs. Le projet vérifie les MelonLoader requis et les DLL de jeu générées et signale un chemin manquant avant la compilation.

4. La DLL du mod construite sera à `src\bin\Release\net6.0\BopItAccess.dll`.

Si le SDK signale un pack de ciblage .NET 6 manquant, installez un SDK incluant ce pack. Le projet `NuGet.Config` ne configure pas les flux de packages en ligne.

<a id="install-your-build"></a>
## Installez votre build

1. Fermez le jeu. Copiez le construit `BopItAccess.dll` dans `<game directory>\Mods\`. Créer le `Mods` répertoire si MelonLoader ne l'a pas créé.
2. Obtenez la version officielle de Prism v0.18.3 pour Windows x64 (`prism.dll`) sur la [page des versions de Prism](https://github.com/ethindp/prism/releases), ou compilez cette même version depuis les sources. Placez `prism.dll` à côté du fichier exécutable du jeu, dans le dossier principal du jeu, pas dans le dossier `Mods`.
3. Copiez l'intégralité de la build `src\bin\Release\net6.0\documentation\` dossier dans le répertoire du jeu. Il contient le guide anglais à sa racine et des guides traduits sous `fr`, `it`, `de`, `es`, `es-MX`, `ja`, `ko`, `zh`, et `pt-BR`. Conservez ces sous-dossiers et les documents qui les accompagnent. Le guide du jeu lit le code HTML de la langue du jeu actuelle à chaque ouverture. Ainsi, le remplacement d'un guide met à jour son contenu sans reconstruire la DLL.
4. Démarrez votre lecteur d’écran avant le jeu. Sans lecteur compatible en cours d’exécution, Prism privilégie OneCore, puis SAPI si OneCore est indisponible.

La commande build Bop It Access compile uniquement ce mod ; il ne construit pas et ne télécharge pas Prism. Si la parole ne démarre pas, inspectez `<game directory>\Mods\BopItAccess.log`. Le journal enregistre l'initialisation et l'envoi de la parole de Prism, bien qu'un envoi réussi ne puisse à lui seul prouver que l'audio a été entendu.

Lors d'une première exécution, l'écran de bienvenue apparaît une fois le menu principal du jeu prêt. Ses choix ouvrent les paramètres du module, lisent le guide de l'utilisateur dans le jeu ou continuent le jeu. Les paramètres du module proposent également **Ouvrir le guide de l'utilisateur** et une action confirmée **Réinitialiser l'écran de bienvenue** qui affiche l'écran de bienvenue au prochain lancement. Dans le guide, utilisez Haut/Bas pour choisir des sujets ou lire des lignes et Confirmer pour ouvrir un sujet. Dans les tableaux, Gauche déplace une colonne vers la gauche, Droite déplace une colonne vers la droite et Haut/Bas conserve la colonne actuelle tout en changeant de ligne. Les en-têtes de colonnes étiquettent les cellules plutôt que d'apparaître sous forme de lignes de données ; le tableau est annoncé à l'entrée et sa fin à la sortie. Retour laisse un sujet ou le guide.

Choisissez une langue dans la ligne **Paramètres > Langue** du jeu. Le discours du mod suit cette sélection. Le guide du jeu utilise le document HTML traduit correspondant, avec l'anglais comme solution de secours si la copie sélectionnée est manquante ou illisible. Le texte non anglais groupé est une première passe traduite automatiquement ; les corrections pour le locuteur courant sont les bienvenues.

Les actions de jeu utilisent les traductions du jeu. Shapes, Space, City et Office gardent leurs noms de scène anglais. Avec OneCore ou SAPI, choisissez dans les paramètres du mod une voix installée pour la langue du jeu si la voix par défaut ne convient pas. Voix, Volume, Vitesse et Hauteur de voix règlent la sortie OneCore ou SAPI réellement utilisée, même en mode Auto. Seuls les réglages pris en charge sont visibles ; ils sont masqués pour les autres sorties. Chaque moteur mémorise ses choix séparément.

Lire les positions dans les menus. Cette option mémorisée, activée par défaut, annonce la position de l’élément dans son menu. Les commentaires individuels sont mémorisés et activés par défaut. Annonce la couleur active au début et lorsqu’elle change. Après une vie perdue, annonce le nombre de vies restantes. Après une vie gagnée, annonce la couleur du joueur et son nouveau total, par exemple « Vert, 3 vies », pour indiquer quel joueur a été le plus rapide. Ces annonces sont désactivées lorsque les commentaires individuels sont désactivés. La limite de trois vies du jeu reste inchangée. Cette fonction n’agit que pendant une partie en tête-à-tête. Après votre dernière entrée de calibrage, le mod dit immédiatement « Terminé ! » ; cessez de taper et attendez le résultat mesuré. Si le calibrage échoue parce qu’aucune entrée n’a été effectuée, il dit « Échec du calibrage. ».

**Formater la parole**: Rend les textes tout en majuscules plus naturels pour la parole et le braille. Dans le guide intégré, ajoute des points de suspension avant le numéro de ligne si le texte se termine sans ponctuation. Le texte visible reste inchangé. Désactivez cette option pour conserver le texte tel quel.

### MelonLoader fenêtres de démarrage

Le modèle Loader.cfg fourni masque l’écran de démarrage et la console séparés de MelonLoader. L’installateur applique ces deux valeurs par défaut avant que vous lanciez vous-même le jeu. Elles ne suppriment ni l’écran titre du jeu ni l’écran de bienvenue du mod.

Le jeu étant fermé, ouvrez `UserData/Loader.cfg` dans le dossier du jeu. Si ce fichier existe déjà, réglez `disable_start_screen` sur `true` dans sa section `[loader]` existante, et `hide_console` sur `true` dans sa section `[console]` existante. Conservez toutes les autres entrées. Si le fichier n'existe pas, copiez le modèle `UserData/Loader.cfg` fourni avec la compilation, ou `configuration/Loader.cfg` depuis le code source. Ne remplacez jamais un Loader.cfg existant par le modèle complet.

```ini
[loader]
disable_start_screen = true

[console]
hide_console = true
```

Le mod ne réinitialise pas ces options à chaque lancement. Vous pouvez redéfinir manuellement l’une ou l’autre valeur sur false si vous avez besoin des fenêtres du chargeur pour le dépannage. La désinstallation du programme d'installation restaure les valeurs d'origine uniquement tant que les vraies valeurs du programme d'installation sont toujours présentes, préservant ainsi les autres modifications de configuration du chargeur.

## Documentation

- [Guide d'utilisation du jeu et du mod (anglais)](BopItAccess-user-guide.html) - une présentation pas à pas conviviale des commandes, des paramètres, des menus et des modes de jeu.
- [Guide de l'utilisateur japonais (日本語)](../ja/BopItAccess-user-guide.html). D'autres guides traduits sont disponibles dans les dossiers de langue sous [`documentation/`](../).
- [Guide détaillé des fonctionnalités et des commandes](README.txt). Sa section d'installation décrit les ZIP d'installation préparés localement ; ce référentiel GitHub fournit uniquement la source.
- [Historique de construction technique](BopItAccess-build-history.html).
- [Revue du code pour préparer la publication](BopItAccess-release-review.html) — constats traités, fichiers examinés, résultats de compilation et limites restantes.
- [Workflow Git pour ce projet](GIT-WORKFLOW.md).
- [Avis de tiers](THIRD-PARTY-NOTICES.txt).

Les versions traduites des documents ci-dessus se trouvent dans [`documentation/`](../) sous le code de chaque langue prise en charge. Leur source est en anglais ; `scripts/translate_documents.py` permet de régénérer les brouillons traduits automatiquement après modification de la source.

## Que pourrait-il arriver ensuite

Ce projet est pour l’essentiel terminé et aucun contenu ou fonctionnalité majeur n’est prévu. Cependant, ce mod sera activement maintenu et mis à jour au fil du temps selon les besoins, les commentaires des joueurs étant à l'origine de ces améliorations. Les travaux futurs potentiels incluent une révision plus approfondie et des corrections de bugs, un raffinement du code et des améliorations continues de la réactivité vocale. Prism crée un chemin possible vers d'autres plates-formes dans le futur, mais ce mod ne prend actuellement en charge que Windows x64. Le référentiel du projet est l'endroit idéal pour suivre le développement ultérieur.

## Note de transparence sur l'IA

Ce mod a été réalisé par « vibe coding ». Tout le code a été entièrement généré et étudié par l’intelligence artificielle, avec une compréhension humaine technique limitée de son architecture sous-jacente. Veuillez utiliser ce mod à vos propres risques.

Cela étant dit, chaque fonctionnalité du mod et chaque décision de conception ont été rédigées et approuvées par des humains. Les tests n’ont jamais été automatisés ; ils ont été réalisés avec soin et de manière approfondie par de vrais joueurs et testeurs humains.

Remarque : les textes et la documentation multilingues ont été générés par l'IA et n'ont pas été révisés par des locuteurs natifs. Il faut s’attendre à une grande imprécision de traduction. Sans codage agent, ce projet n’existerait pas. Merci de lui avoir donné une chance !

## Merci

À ceux qui ont testé ce mod avant sa sortie et qui ont contribué à l'amener là où il est actuellement, merci. Vous savez tous qui vous êtes. Aux joueurs qui donnent leur avis, essaient le mod pour la première fois, ou croient en moi et en ce projet, merci. Votre soutien me motive à continuer à créer des choses dans un monde qui peut sembler fou et profondément imparfait. J'espère que ce projet vous permettra d'apprécier plus facilement le jeu et de jouer avec les autres. Merci beaucoup à tous. Profitez du Bop It!

— Christopher Shaw

## Licence

Une licence pour la source Bop It Access n'a pas encore été sélectionnée. Prism possède sa propre licence ; voir le [avis de tiers](THIRD-PARTY-NOTICES.txt). Bop It! et ses actifs appartiennent à leurs propriétaires respectifs et ne sont pas inclus ici.

## Installation et premier démarrage

L’aperçu 0.1.7 du programme d’installation ne lance jamais Bop It! pendant l’installation. Install télécharge une version publique compilée lorsqu’elle existe. Install alpha télécharge le dernier code source, demande confirmation et le compile avant de copier les fichiers. Alpha réutilise des références locales complètes ou prépare des références de compilation temporaires à partir de votre jeu installé, sans l’exécuter. Après cette préparation, l’installateur place MelonLoader dans le dossier du jeu et immédiatement BopItAccess.dll dans Mods. Il termine ensuite les fichiers de Prism, de configuration, de documentation et de désinstallation. Attendez le message de réussite, puis lancez vous-même le jeu par Steam lorsque vous êtes prêt.

Une version compilée nécessite l’environnement d’exécution Windows x64 de .NET 6, pas de SDK de développement. Les environnements complets existants sont réutilisés. S’il en manque un, l’installateur télécharge le ZIP officiel Microsoft .NET 6.0.36 et le place dans MelonLoader/Dependencies/dotnet, un emplacement pris en charge. Ces fichiers sont enregistrés pour annulation et désinstallation ; ils sont conservés si d’autres mods utilisent le chargeur partagé. Install alpha nécessite aussi un SDK compatible et le pack de ciblage .NET 6. Il réutilise un SDK installé ou un ancien dossier dotnet compatible ; si nécessaire, il installe un SDK Microsoft officiel pour tout le système. Le SDK reste après désinstallation ou annulation. Cet aperçu ne crée aucun nouveau dossier SDK dotnet à la racine du jeu. MelonLoader 0.7.3 et Prism 0.18.3 officiels sont récupérés si nécessaire. Les mises à jour passent toujours par GitHub. Abort demande confirmation et annule les modifications de cette installation aux fichiers du jeu.

Au premier lancement manuel après l’installation de MelonLoader, des fichiers de support peuvent être téléchargés et les assemblages du jeu générés. Comptez environ une minute, parfois davantage. Le mod ne peut pas parler avant que MelonLoader ait fini de le charger. Gardez le jeu ouvert et attendez l’annonce de démarrage de Bop It Access, puis celle de l’écran titre ou du menu, avant d’utiliser les commandes.
