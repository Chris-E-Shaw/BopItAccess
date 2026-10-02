# Bop It Access

Bop It Access est un mod d'accessibilité non officiel pour la version Windows Steam de **Bop It!**. Il utilise MelonLoader et [Prism](https://github.com/ethindp/prism) pour ajouter des commentaires vocaux et braille aux menus et aux écrans de jeu. Les fonctionnalités actuelles incluent un écran de bienvenue de première exécution, un guide de l'utilisateur dans le jeu, des écrans de titre et de pause parlés, des paramètres et des commandes, une sélection de chansons, des scores finaux et des classements, des réalisations, des crédits, des conseils sur les boutons, un texte de didacticiel à la demande avec les affectations de contrôle actuelles avant un tour et des descriptions des quatre étapes. La version 0.9.0 utilise Prism pour la sortie vocale et braille. Le mod suit la langue sélectionnée du jeu et comprend un guide pour chaque langue proposée par le jeu.

## Statut du projet

Ce projet est en début de développement. Ce référentiel contient le code source et la documentation technique. **Il n'y a pas encore de versions compilées ni de versions GitHub ici.** Pour utiliser le mod de ce référentiel, construisez-le à partir des sources et fournissez le runtime Prism décrit ci-dessous.

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
4. Si vous utilisez un lecteur d’écran, démarrez-le avant le jeu, puis lancez le jeu depuis Steam. Si aucun lecteur d’écran compatible n’est actif, Prism peut utiliser la voix SAPI.

La commande build Bop It Access compile uniquement ce mod ; il ne construit pas et ne télécharge pas Prism. Si la parole ne démarre pas, inspectez `<game directory>\Mods\BopItAccess.log`. Le journal enregistre l'initialisation et l'envoi de la parole de Prism, bien qu'un envoi réussi ne puisse à lui seul prouver que l'audio a été entendu.

Lors d'une première exécution, l'écran de bienvenue apparaît une fois le menu principal du jeu prêt. Ses choix ouvrent les paramètres du module, lisent le guide de l'utilisateur dans le jeu ou continuent le jeu. Les paramètres du module proposent également **Ouvrir le guide de l'utilisateur** et une action confirmée **Réinitialiser l'écran de bienvenue** qui affiche l'écran de bienvenue au prochain lancement. Dans le guide, utilisez Haut/Bas pour choisir des sujets ou lire des lignes et Confirmer pour ouvrir un sujet. Dans les tableaux, Gauche déplace une colonne vers la gauche, Droite déplace une colonne vers la droite et Haut/Bas conserve la colonne actuelle tout en changeant de ligne. Les en-têtes de colonnes étiquettent les cellules plutôt que d'apparaître sous forme de lignes de données ; le tableau est annoncé à l'entrée et sa fin à la sortie. Retour laisse un sujet ou le guide.

Choisissez une langue dans la ligne **Paramètres > Langue** du jeu. Le discours du mod suit cette sélection. Le guide du jeu utilise le document HTML traduit correspondant, avec l'anglais comme solution de secours si la copie sélectionnée est manquante ou illisible. Le texte non anglais groupé est une première passe traduite automatiquement ; les corrections pour le locuteur courant sont les bienvenues.

Le mod utilise les noms traduits du jeu pour les actions de jeu. Shapes, Space, City et Office restent en anglais sous forme de titres d'étape fixes. La sortie vocale sélectionnée nécessite une voix pour votre langue. Pour la sortie SAPI, choisissez une voix installée adaptée à votre langue si la voix par défaut du système sonne mal.

## Documentation

- [Guide d'utilisation du jeu et du mod (anglais)](BopItAccess-user-guide.html) - une présentation pas à pas conviviale des commandes, des paramètres, des menus et des modes de jeu.
- [Guide de l'utilisateur japonais (日本語)](../ja/BopItAccess-user-guide.html). D'autres guides traduits sont disponibles dans les dossiers de langue sous [`documentation/`](../).
- [Guide détaillé des fonctionnalités et des commandes](README.txt). Sa section d'installation décrit les ZIP d'installation préparés localement ; ce référentiel GitHub fournit uniquement la source.
- [Historique de construction technique](BopItAccess-build-history.html).
- [Workflow Git pour ce projet](GIT-WORKFLOW.md).
- [Avis de tiers](THIRD-PARTY-NOTICES.txt).

Des copies traduites des six documents ci-dessus sont en [`documentation/`](../) sous chaque code de langue pris en charge. Leur source est l'anglais ; `scripts/translate_documents.py` peut régénérer les brouillons traduits automatiquement après des modifications de source.

## Transparence de l'IA

Christopher Shaw dirige ce projet et évalue son accessibilité dans le jeu. Les modèles OpenAI Codex ont aidé à la recherche, au code et à la documentation. Les messages de validation publiés incluent un `Co-authored-by` une bande-annonce identifiant le modèle qui a contribué à chaque changement ; les crédits historiques ont été vérifiés par rapport aux enregistrements de session de ce projet. L'historique de construction antérieur a été reconstruit à partir des archives sources enregistrées plutôt que enregistré en tant que validation à l'époque. Les contributions assistées par l'IA peuvent contenir des erreurs et doivent être examinées avant utilisation.

## Licence

Une licence pour la source Bop It Access n'a pas encore été sélectionnée. Prism possède sa propre licence ; voir le [avis de tiers](THIRD-PARTY-NOTICES.txt). Bop It! et ses actifs appartiennent à leurs propriétaires respectifs et ne sont pas inclus ici.
