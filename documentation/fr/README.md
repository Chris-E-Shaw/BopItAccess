# Bop It Access

Builds source actuels : mod 0.9.12 (59 builds du mod), installateur 0.2.7. La première publication publique reste à venir.

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

Après avoir confirmé Uninstall, choisissez Uninstall for me ou Uninstall for everyone. Les deux options suppriment les fichiers partagés du mod dans ce dossier du jeu ; le mod ne sera donc plus disponible pour aucune personne utilisant cette installation. Ce choix détermine les préférences Windows enregistrées du mod qui sont supprimées : celles du seul compte à l’origine de la demande, ou celles de tous les profils Windows locaux, y compris les profils dont la session est fermée. Les préférences du jeu d’origine sont conservées. Le SDK .NET reste installé.

Lorsque le programme d’installation supprime sa propre installation de MelonLoader et qu’aucun autre mod n’en a besoin, il supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg, ainsi que les dossiers Plugins, UserLibs et UserData lorsqu’ils sont vides. Les paramètres de Bop It Access, les journaux connus, les guides et les fichiers du programme d’installation sont supprimés. Les autres mods, les fichiers partagés du chargeur déjà présents et les fichiers non reconnus sont protégés. Un fichier inconnu peut donc laisser un dossier en place ; le programme d’installation le signale dans les diagnostics au lieu de supprimer des données sans rapport avec le mod.

La page Applications installées de Windows utilise les mêmes étapes de confirmation, de choix des préférences et de nettoyage. Le programme d’installation fournit BopItAccess-uninstall.ps1 dans le dossier du jeu comme raccourci vers le programme de désinstallation installé ; les futures compilations à partir des sources incluent également ce script dans leurs fichiers de sortie. Copier manuellement ce script n’installe pas le programme de désinstallation lui-même. Pour une ancienne installation manuelle sans registre de propriété des fichiers, le programme d’installation supprime les fichiers du mod identifiables et conserve les fichiers partagés dont l’origine ne peut pas être établie.

Si le nettoyage ne peut pas se terminer en toute sécurité, le programme d’installation l’explique et conserve les informations nécessaires à une nouvelle tentative. Pour une installation gérée, son entrée de désinstallation Windows et son point de reprise du nettoyage sont conservés jusqu’à ce que la suppression réussisse. Une ancienne copie manuelle ne dispose pas d’un registre durable de propriété des fichiers ; dans le programme d’installation ouvert, réessayez les opérations signalées par ses avertissements. N’installez pas, ne mettez pas à jour et ne supprimez pas le mod pendant que Bop It! est en cours d’exécution.

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

Le mod ne réinitialise pas ces options à chaque lancement. Pour résoudre un problème, vous pouvez remettre manuellement l’une ou l’autre valeur à false. Si la désinstallation conserve une installation partagée de MelonLoader, elle restaure uniquement les indicateurs ciblés par le programme d’installation qui n’ont pas été modifiés et préserve les autres modifications. Si elle supprime sa propre installation inutilisée de MelonLoader, elle supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg.

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


## Choisir le bon téléchargement

La première publication publique sur GitHub prévoit les quatre téléchargements suivants. Ce sont des fichiers à venir, pas encore disponibles ; aucune publication publique ni étiquette n’a été créée. Utilisez jusque-là un installateur fourni ou le code source. Une archive source n’est pas le ZIP d’installation compilé.

[GitHub Releases](https://github.com/Chris-E-Shaw/BopItAccess/releases)

- BopItAccess-Installer.exe: L’installateur autonome pour Windows x64. Il trouve le jeu et gère dépendances, installation, mises à jour, diagnostics et suppression. Il n’est pas signé.
- BopItAccess-v1.0.zip: Le paquet compilé du mod pour une installation manuelle sans lancer l’EXE Bop It Access. Il comprend Mods/BopItAccess.dll, prism.dll, toute la documentation et les licences Prism, un modèle Loader.cfg, README.txt et le raccourci de désinstallation. MelonLoader, .NET, les fichiers du jeu et les assemblies générés ne sont pas inclus.
- Source code (zip): Le ZIP du code source de la version, généré automatiquement par GitHub. Pour lire ou compiler le code ; ce n’est pas le paquet compilé du mod.
- Source code (tar.gz): Le même code source dans une archive tar compressée avec gzip. Un autre format source, pas un autre installateur du mod.

### Installateur non signé et avertissements Windows 11

Cet installateur n’est pas signé. Un programme non signé ou peu connu peut déclencher SmartScreen ou l’antivirus, y compris de possibles faux positifs ; cela ne prouve pas que toute détection est erronée. Obtenez le fichier seulement auprès du projet officiel Bop It Access ou d’un envoi direct fiable et décidez si vous lui faites confiance. Le ZIP compilé évite cet EXE. Ne désactivez pas l’antivirus et n’excluez pas un lecteur ou tout le dossier du jeu.
[Microsoft: unsigned apps and SmartScreen](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/publish-first-app)

### Autoriser une détection Defender précise

Appuyez sur Win+I, puis Confidentialité et sécurité > Sécurité Windows > Ouvrir Sécurité Windows > Protection contre les virus et menaces > Historique de protection (parfois appelé historique des menaces). Développez l’élément correspondant à l’installateur. Avec Tab, allez sur Actions ou Autres actions, appuyez sur Entrée et choisissez Autoriser sur l’appareil ou Autoriser ; acceptez la demande administrateur si nécessaire. Un fichier en quarantaine peut nécessiter Restaurer d’abord, puis une autorisation s’il est détecté à nouveau. S’il a été supprimé, retéléchargez-le depuis le projet officiel. Vérifiez l’élément exact avant d’autoriser.
[Microsoft: Protection history](https://support.microsoft.com/en-us/windows/security/windows-security/protection-history-in-the-windows-security-app) · [Microsoft Defender FAQ](https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq)

### Exclusions Defender facultatives et limitées

Dans Protection contre les virus et menaces, choisissez Gérer les paramètres sous les paramètres de protection, puis Exclusions > Ajouter ou supprimer des exclusions. Acceptez la demande administrateur avec Oui si affichée. Choisissez Ajouter une exclusion > Processus, saisissez exactement BopItAccess-Installer.exe et appuyez sur Entrée. Le nom doit correspondre à l’exécutable réellement lancé.

L’exclusion Processus de Microsoft couvre les fichiers ouverts par ce processus ; elle n’exclut pas l’EXE de l’installateur, ne restaure pas une quarantaine et ne contourne pas SmartScreen. Si Defender détecte l’EXE lui-même et que vous lui faites confiance, une exclusion facultative Fichier ciblant cet EXE téléchargé est l’alternative limitée pertinente. Retirez les exceptions devenues inutiles.
[Microsoft: exclusions overview](https://learn.microsoft.com/en-us/defender-endpoint/microsoft-defender-antivirus-exclusions-overview)

SmartScreen est un avertissement séparé. Si vous faites confiance à cet EXE et que Windows le propose, choisissez Informations complémentaires > Exécuter quand même. L’autorisation Defender ou l’exclusion Processus ne contourne pas cet avertissement ; une stratégie peut empêcher l’exécution.

Étapes vérifiées le 9 octobre 2026 pour Windows 11 25H2, build 26200.9550. Les libellés peuvent varier ; aucun essai de l’interface n’a été effectué.

## Programme d’installation Windows 0.2.7

### Installer, mettre à jour ou supprimer avec l’installateur

1. Fermez le jeu, lancez BopItAccess-Installer.exe et acceptez la demande administrateur Windows. Lisez le champ Welcome and controls, puis vérifiez Game folder ou utilisez Browse. Welcome and controls est en lecture seule, sélectionnable et reçoit le premier focus ; Alt+W y revient.
2. Choisissez Install pour la dernière version publique compilée quand elle sera disponible. Avant la première publication, Show advanced affiche Install alpha, qui demande confirmation et compile les dernières sources sur votre ordinateur. Attendez le message de réussite. Play Bop It! The Video Game lance ensuite le jeu par Steam uniquement sur votre demande.
3. Pour une installation existante, ouvrez l’installateur avec le jeu fermé et vérifiez son état. Update apparaît si une version publique plus récente est trouvée. Choisissez Update et attendez la fin ; vos réglages enregistrés sont conservés. Install alpha est le choix séparé des dernières sources, pas la mise à jour publique.
4. Pour supprimer le mod, choisissez Uninstall, confirmez, puis Uninstall for me ou Uninstall for everyone. Les deux retirent les fichiers partagés du mod de ce dossier du jeu. Le choix détermine si les préférences Windows du mod sont supprimées pour votre compte ou tous les profils locaux ; les préférences originales du jeu restent. Applications installées de Windows suit le même parcours. Vérifiez le résultat avant Quit.

Le tableau suivant décrit toutes les actions et tous les champs de texte de la fenêtre principale. Installer status et Installation progress sont des informations, pas des boutons. Welcome and controls contient les instructions réutilisables ; Status log contient les messages changeants. Abort demande confirmation avant d’annuler une installation active ; Quit suit les mêmes règles d’arrêt sûr. Les dialogues proposent Keep open/Quit, confirmation/annulation et les deux portées de préférences à supprimer. Show advanced change seulement les actions visibles.

Utilisez BopItAccess-Installer-0.2.7.exe ou BopItAccess-Installer.exe fourni par le projet. Les deux noms contiennent le même programme d’installation autonome pour Windows x64. Le projet comprend son code source ; aucun binaire compilé public ni GitHub Release n’est encore publié.

Fermez Bop It!, ouvrez le programme d’installation et approuvez la demande d’autorisation administrateur de Windows. Le programme d’installation vous accueille, recherche le jeu dans les bibliothèques Steam de tous les lecteurs disponibles et tente de placer sa fenêtre au premier plan. Vérifiez le dossier du jeu affiché ; utilisez Browse si vous devez choisir un autre dossier. La touche Tab permet de passer d’une commande à l’autre. Le journal d’état est un champ de texte en lecture seule : placez-y le focus pour consulter les messages avec les touches de déplacement du curseur, sélectionner du texte ou le copier.

Le programme d’installation 0.2.7 demande brièvement l’activation au premier plan et le focus du clavier au démarrage. Si une autre fenêtre reste active à la fin de sa courte observation initiale, son titre et son bouton de barre des tâches clignotent et il demande de passer au programme d’installation avec Alt+Tab. Activez-le avant d’utiliser ses commandes au clavier ou à la manette. Alt+G place le focus sur le champ du dossier du jeu.

Show advanced est décoché à l’ouverture du programme d’installation. Cette case affiche Install alpha, Save diagnostics et Copy diagnostics. Install télécharge la dernière version publique publiée sur GitHub lorsqu’il en existe une. Aucune version publique n’est encore disponible ; les testeurs doivent donc actuellement utiliser Show advanced et Install alpha. L’installation alpha demande confirmation, télécharge les sources les plus récentes et les compile sur votre ordinateur. Update apparaît lorsqu’une version publique plus récente est détectée pour une copie déjà installée.

Les messages d’état expliquent simplement ce qui est en cours de téléchargement, d’installation ou de finalisation. Une seule barre indique la progression estimée de l’ensemble de l’installation, sans revenir à zéro pour chaque téléchargement ou fichier. Elle avance par incréments de cinq points de pourcentage ; certaines étapes de préparation peuvent prendre du temps sans changement visible. La disponibilité d’une nouvelle mise à jour et la confirmation de la copie des diagnostics sont transmis à votre lecteur d’écran par les notifications d’accessibilité de Windows. Leur lecture à voix haute dépend de votre lecteur d’écran et de sa prise en charge des notifications Windows.

Le programme d’installation 0.2.7 ne lance jamais Bop It! pendant l’installation. L’installation alpha réutilise les fichiers de compilation locaux correspondants ou prépare des fichiers temporaires à partir de votre propre jeu installé, qui reste fermé. Le programme d’installation place ensuite MelonLoader dans le dossier du jeu et ajoute immédiatement Mods/BopItAccess.dll, puis Prism, les paramètres, la documentation complète et les éléments nécessaires à la désinstallation. Attendez le message de réussite, puis lancez vous-même le jeu depuis Steam lorsque vous êtes prêt.

Après une installation réussie, Play Bop It! The Video Game apparaît. Activez ce bouton pour lancer vous-même le jeu depuis Steam lorsque vous êtes prêt. Le programme d’installation ne démarre jamais le jeu automatiquement pendant l’installation.

Une version compilée nécessite l’environnement d’exécution .NET 6 pour Windows x64, et non un SDK de développement. Les environnements d’exécution complets déjà présents sont réutilisés. Si l’environnement d’exécution manque, il est téléchargé auprès de Microsoft et placé dans MelonLoader/Dependencies/dotnet. Install alpha nécessite également un SDK .NET compatible et le pack de ciblage .NET 6 : un SDK existant est réutilisé, ou le SDK officiel de Microsoft est installé pour tout le système. Le programme d’installation ne crée aucun nouveau dossier SDK à la racine du jeu. MelonLoader 0.7.3 Open-Beta et Prism 0.18.3 proviennent de leurs versions officielles. Les composants Microsoft .NET partagés et les SDK restent installés après un abandon ou une désinstallation.

Quit ferme le programme d’installation. Si l’installation est encore en cours, il demande s’il faut l’abandonner et annuler ses modifications avant de fermer ; Keep open poursuit normalement l’opération. Si l’installation se termine pendant que vous prenez votre décision, la boîte de dialogue se met à jour pour indiquer qu’elle est terminée, et Quit n’annule pas l’installation achevée. Une fois la suppression commencée, la désinstallation se termine en toute sécurité avant la fermeture. Abort demande également confirmation et annule les modifications des fichiers du jeu effectuées lors de cette tentative. Une annulation pendant l’installation de Microsoft .NET attend que l’installation de ces composants partagés se termine en toute sécurité.

Après avoir confirmé Uninstall, choisissez Uninstall for me ou Uninstall for everyone. Les deux options suppriment les fichiers partagés du mod dans ce dossier du jeu ; le mod ne sera donc plus disponible pour aucune personne utilisant cette installation. Ce choix détermine les préférences Windows enregistrées du mod qui sont supprimées : celles du seul compte à l’origine de la demande, ou celles de tous les profils Windows locaux, y compris les profils dont la session est fermée. Les préférences du jeu d’origine sont conservées. Le SDK .NET reste installé.

Lorsque le programme d’installation supprime sa propre installation de MelonLoader et qu’aucun autre mod n’en a besoin, il supprime également les fichiers connus Loader.cfg et MelonPreferences.cfg, ainsi que les dossiers Plugins, UserLibs et UserData lorsqu’ils sont vides. Les paramètres de Bop It Access, les journaux connus, les guides et les fichiers du programme d’installation sont supprimés. Les autres mods, les fichiers partagés du chargeur déjà présents et les fichiers non reconnus sont protégés. Un fichier inconnu peut donc laisser un dossier en place ; le programme d’installation le signale dans les diagnostics au lieu de supprimer des données sans rapport avec le mod.

Le programme d’installation reste ouvert après la désinstallation pour vous permettre d’examiner le résultat, d’enregistrer les diagnostics ou de réinstaller. Choisissez Quit lorsque vous avez terminé. L’utilitaire de désinstallation en cours d’exécution et les fichiers de diagnostic automatiques sont nettoyés après la fermeture de la fenêtre. Une réinstallation dans la même fenêtre ouvre un nouveau dossier de suivi de l’installation ; le nettoyage différé ne peut pas supprimer la nouvelle installation.

La page Applications installées de Windows utilise les mêmes étapes de confirmation, de choix des préférences et de nettoyage. Le programme d’installation fournit BopItAccess-uninstall.ps1 dans le dossier du jeu comme raccourci vers le programme de désinstallation installé ; les futures compilations à partir des sources incluent également ce script dans leurs fichiers de sortie. Copier manuellement ce script n’installe pas le programme de désinstallation lui-même. Pour une ancienne installation manuelle sans registre de propriété des fichiers, le programme d’installation supprime les fichiers du mod identifiables et conserve les fichiers partagés dont l’origine ne peut pas être établie.

Si le nettoyage ne peut pas se terminer en toute sécurité, le programme d’installation l’explique et conserve les informations nécessaires à une nouvelle tentative. Pour une installation gérée, son entrée de désinstallation Windows et son point de reprise du nettoyage sont conservés jusqu’à ce que la suppression réussisse. Une ancienne copie manuelle ne dispose pas d’un registre durable de propriété des fichiers ; dans le programme d’installation ouvert, réessayez les opérations signalées par ses avertissements. N’installez pas, ne mettez pas à jour et ne supprimez pas le mod pendant que Bop It! est en cours d’exécution.

### Raccourcis clavier du programme d’installation

| Action | Raccourci clavier | Effet |
| --- | --- | --- |
| Welcome and controls | Alt+W | Le champ Welcome and controls séparé liste les raccourcis de lecture du texte à la manette ; Alt+W y revient et Alt+L ouvre le Status log changeant. Les deux champs sont en lecture seule, sélectionnables et consultables. Show advanced annonce coché ou décoché. Tout sélectionner confirme la réussite ou un champ vide. Au clavier, Ctrl+A sélectionne tout le texte et Ctrl+C copie la sélection. |
| Dossier du jeu | Alt+G | Placer le focus sur le champ du dossier du jeu. |
| Browse | Alt+B | Choisir le dossier du jeu. |
| Install | Alt+I | Installer la dernière version publique lorsqu’elle est disponible. |
| Install alpha | Alt+A | Confirmer et compiler les sources les plus récentes ; visible avec Show advanced. |
| Update | Alt+U | Installer une version publique plus récente lorsqu’elle est proposée. |
| Play Bop It! The Video Game | Alt+P | Lancer le jeu depuis Steam ; disponible après une installation réussie. |
| Uninstall | Alt+N | Confirmer la suppression et choisir les comptes dont les préférences Windows du mod seront supprimées. |
| Abort | Alt+R | Confirmer l’annulation de l’installation en cours. |
| Journal d’état | Alt+L | Placer le focus sur les messages d’état en lecture seule dont le texte peut être sélectionné. |
| Show advanced | Alt+V | Afficher ou masquer l’installation alpha et les outils de diagnostic. |
| Save diagnostics | Alt+D | Enregistrer la session de diagnostic complète et poursuivre son enregistrement ; visible avec Show advanced. |
| Copy diagnostics | Alt+C | Copier l’instantané complet des diagnostics ; visible avec Show advanced. |
| Quit | Alt+Q | Fermer, avec une gestion sûre de l’annulation si une opération est en cours. |

### Utiliser une manette dans le programme d’installation

Le programme d’installation prend en charge les manettes de type Xbox et les autres manettes que Windows rend accessibles par XInput. Ses commandes sont distinctes des commandes personnalisables du jeu. La croix directionnelle ou le stick gauche permet de passer d’une commande à l’autre ; lorsqu’un champ de texte a le focus, les directions servent à parcourir son texte. Les boutons de tranche passent toujours à la commande précédente ou suivante pouvant recevoir le focus. A active le bouton ou la case à cocher ayant le focus. Les commandes de la manette sont traitées uniquement lorsque ce programme d’installation ou l’une de ses propres boîtes de dialogue est au premier plan.

B revient en arrière ou annule une boîte de dialogue ; dans la fenêtre principale du programme d’installation, il demande l’abandon d’une installation en cours, sinon il correspond à Quit. Start correspond à Quit dans la fenêtre principale et revient en arrière dans une boîte de dialogue. Y (le bouton supérieur en façade) sélectionne tout le texte lorsqu’un champ de texte du programme d’installation a le focus. Hors des champs de texte de la fenêtre principale, Y bascule Show advanced. Dans le journal d’état ou un autre champ de texte du programme d’installation, la croix directionnelle ou le stick gauche agit comme les touches fléchées : Gauche/Droite se déplace par caractère et Haut/Bas par ligne. Maintenez LT comme Ctrl : Gauche/Droite se déplace par mot et Haut/Bas par paragraphe. Maintenez RT comme Maj pour étendre la sélection ; maintenez LT et RT ensemble pour sélectionner des mots ou des paragraphes. X copie uniquement le texte sélectionné ; sélectionnez d’abord la partie souhaitée. Ctrl+C au clavier continue de copier la sélection. Lorsqu’aucun texte n’est sélectionné, le programme d’installation envoie aussi des notifications accessibles pour le caractère, le mot, la ligne ou le paragraphe à la position du curseur. Le programme d’installation envoie une confirmation accessible quand le texte est copié et signale une sélection vide ou un échec de copie. L’annonce vocale dépend de la prise en charge des notifications Windows par votre lecteur d’écran. La navigation à la manette dans les boîtes de dialogue natives de Windows de sélection de dossier et d’enregistrement nécessite encore une vérification humaine. Un clavier reste disponible pour saisir un dossier ou un nom de fichier. Les manettes sans prise en charge de XInput ne sont pas couvertes par cette implémentation.

Le champ Welcome and controls séparé liste les raccourcis de lecture du texte à la manette ; Alt+W y revient et Alt+L ouvre le Status log changeant. Les deux champs sont en lecture seule, sélectionnables et consultables. Show advanced annonce coché ou décoché. Tout sélectionner confirme la réussite ou un champ vide. Au clavier, Ctrl+A sélectionne tout le texte et Ctrl+C copie la sélection.

Quand Welcome and controls (Alt+W) ou Status log (Alt+L) reçoit le focus, le champ porte un nom court. Les instructions restent consultables dans Welcome and controls. L’activation d’un bouton de l’installateur envoie immédiatement un retour au lecteur d’écran et demande d’interrompre la parole précédente de l’installateur. Les confirmations, annulations et erreurs sont toujours annoncées. Tous les messages d’état ne sont pas lus automatiquement. La manière dont ces retours sont lus dépend de la prise en charge des notifications Windows par votre lecteur d’écran.

### Diagnostics du programme d’installation

Show advanced affiche Save diagnostics (Alt+D) et Copy diagnostics (Alt+C). Les journaux automatiques en UTF-8 sont conservés localement dans %ProgramData%\BopItAccess\diagnostics. Save diagnostics écrit l’intégralité de la session en cours dans le fichier .log ou .txt que vous choisissez et continue de l’enregistrer jusqu’à la fermeture du programme d’installation ; Copy diagnostics copie un instantané et fournit une confirmation accessible. Enregistrez avant un essai d’installation ou de désinstallation pour que votre enregistrement soit conservé après le nettoyage des journaux automatiques. Les détails techniques relatifs aux fichiers, aux téléchargements, à la compilation et aux erreurs y sont conservés, même si le champ d’état affiche des messages plus courts. Rien n’est envoyé en ligne. Les journaux peuvent contenir des noms d’utilisateur Windows et des chemins complets : relisez-les avant de les partager. Les copies exportées volontairement restent présentes après la désinstallation.

Au premier lancement manuel après l’installation de MelonLoader, celui-ci peut télécharger des fichiers de prise en charge et préparer les assemblies du jeu. Prévoyez environ une minute, voire davantage sur certains systèmes. Le mod ne peut pas parler tant que MelonLoader ne l’a pas chargé. Gardez le jeu ouvert et attendez l’annonce de démarrage de Bop It Access, puis l’annonce de l’écran titre, de bienvenue ou du menu principal avant d’utiliser les commandes du jeu.


### Installer le ZIP compilé sans l’EXE Bop It Access

Lorsque BopItAccess-v1.0.zip sera publié, ce parcours utilisera la DLL déjà compilée et ne nécessitera pas le SDK .NET. Il faut toujours votre jeu acheté pour Windows x64, MelonLoader officiel x64 0.7.3 Open-Beta et le runtime .NET 6 Windows x64. Suivez les instructions officielles de MelonLoader et Microsoft ; le ZIP ne fournit pas ces prérequis.

[MelonLoader](https://github.com/LavaGang/MelonLoader#how-to-use-the-installer) · [.NET 6 Windows x64 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

1. Installez le jeu par Steam et trouvez son dossier. Fermez Bop It! avant de modifier les fichiers ; utilisez au besoin la fonction Steam pour parcourir les fichiers installés.
2. Installez MelonLoader officiel x64 dans ce dossier et vérifiez que le runtime .NET 6 x64 est installé. Ne lancez pas encore le jeu : placez d’abord le mod.
3. Extrayez BopItAccess-v1.0.zip compilé dans un dossier temporaire. Copiez Mods/BopItAccess.dll dans Mods du jeu, en créant ou fusionnant ce dossier sans supprimer d’autres mods. Copiez prism.dll à côté de BopIt!.exe.
4. Copiez entièrement documentation et THIRD-PARTY-LICENSES, avec les sous-dossiers de langues et les notices/licences Prism. Copiez README.txt et BopItAccess-uninstall.ps1 du paquet. Ce script ouvre seulement un désinstallateur géré par l’installateur ; le copier ne crée ni désinstallateur fonctionnel ni inscription dans Applications installées.
5. Pour UserData/Loader.cfg : copiez le modèle si le fichier manque. Sinon, fusionnez seulement [loader] disable_start_screen=true et [console] hide_console=true dans les bonnes sections et conservez les autres réglages. N’écrasez pas une configuration existante avec le modèle.
6. Démarrez votre lecteur d’écran si utilisé, puis lancez le jeu par Steam. MelonLoader peut télécharger des fichiers auxiliaires et générer les assemblies au premier lancement, avec le mod déjà dans Mods. Attendez les annonces de démarrage du mod et du menu avant de jouer.

Pour une mise à jour manuelle, fermez le jeu et recopiez les fichiers du mod, Prism, documentation et licences du nouveau paquet aux mêmes endroits. Gardez BopItAccess.ini, les autres mods et les fichiers étrangers ; fusionnez Loader.cfg comme indiqué. Pour désactiver/supprimer le mod manuel, retirez seulement Mods/BopItAccess.dll. Pour un nettoyage plus complet, retirez uniquement les fichiers copiés pour ce mod et UserData/BopItAccess.ini ou son fichier .tmp ; gardez Prism/MelonLoader s’ils sont partagés. Des préférences Windows peuvent rester. Le ZIP manuel n’a ni registre de propriété ni désinstallateur enregistré. Si vous choisissez ensuite l’installateur, Uninstall peut reconnaître une ancienne copie manuelle et nettoyer les préférences en protégeant les fichiers d’origine inconnue. Les journaux du mod sont Mods/BopItAccess.log et Mods/BopItAccess.log.previous ; ne retirez que ces journaux connus lors du nettoyage.

Conditionnement avancé : scripts/package-mod.ps1 empaquette un mod déjà compilé correspondant et les fichiers connus de documentation/configuration/Prism. Il vérifie les versions source/DLL et exclut les fichiers du jeu, générés ou anciens ; il ne compile pas. Archive et préparation restent locales.
