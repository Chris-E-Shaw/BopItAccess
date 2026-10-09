# Bop It Access

Bop It Access est un mod d'accessibilité pour les personnes aveugles, destiné à la version Steam Windows x64 de **Bop It! The Video Game**. Il utilise [MelonLoader](https://github.com/LavaGang/MelonLoader) et [Prism](https://github.com/ethindp/prism) pour rendre les menus et écrans du jeu accessibles par la parole et le braille, avec des commandes et réglages supplémentaires pour jouer plus confortablement.

**La version 1.0 est disponible pour Windows x64.** Le mod et l’installateur utilisent la version source **1.0.0**. Téléchargez l’[installateur](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) pour une installation guidée ou le [ZIP compilé](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) pour une installation manuelle.

## Fonctionnalités

- Lecture des menus, réglages, tutoriels, classements, succès, crédits, écrans de pause et résultats.
- Lecture à la demande des scores, descriptions des décors et conseils tenant compte des commandes actuellement attribuées.
- Parole et guide intégré au jeu dans toutes les langues proposées par le jeu.
- Sortie vers les lecteurs d'écran et le braille, avec OneCore et SAPI comme options de synthèse vocale du système.
- Niveau de détail de la parole, délai et répétition des conseils réglables, ainsi que raccourcis vocaux réattribuables.
- Attribution de commandes natives supplémentaires, limite de fréquence d'images, réglages audio en arrière-plan et fichier de configuration lisible.
- Installateur accessible au clavier et à la manette, pour l'installation, les mises à jour et la désinstallation.

## État du projet

L'essentiel des fonctionnalités est terminé. Le projet sera maintenu selon les besoins, et les retours des joueurs guideront les corrections et améliorations. Windows x64 est actuellement la plateforme prise en charge.

La [première version publique, v1.0](https://github.com/Chris-E-Shaw/BopItAccess/releases/tag/v1.0), propose quatre téléchargements :

| Téléchargement | Utilisation |
| --- | --- |
| [BopItAccess-Installer.exe](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-Installer.exe) | Installateur accessible qui trouve le jeu, installe les dépendances et le dernier mod stable, et gère les mises à jour et la désinstallation. |
| [BopItAccess-v1.0.zip](https://github.com/Chris-E-Shaw/BopItAccess/releases/download/v1.0/BopItAccess-v1.0.zip) | Mod compilé pour une installation manuelle. |
| [Source code (zip)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.zip) | Fichiers source pour lire ou compiler cette version. |
| [Source code (tar.gz)](https://github.com/Chris-E-Shaw/BopItAccess/archive/refs/tags/v1.0.tar.gz) | Les mêmes sources dans un autre format d’archive. |

Choisissez **Install** dans l’installateur pour obtenir la dernière version stable. **Show advanced > Install alpha** est une option expérimentale et facultative qui compile les dernières sources de `main`, après confirmation. Les archives source de GitHub contiennent le code et la documentation ; utilisez le ZIP compilé ou l’installateur pour installer le mod déjà compilé.

Les commits Git constituent l'historique du projet. Les 37 premiers builds ont été importés sous forme d'instantanés distincts du code source ; les dates de ces commits correspondent à l'importation, et non aux dates des builds d'origine. Ce dépôt ne contient ni fichiers binaires compilés, ni fichiers du jeu, ni assemblages du jeu générés par MelonLoader.

## Documentation

[Lire le guide en anglais](../../BopItAccess-user-guide.html) pour l'installation, les mises à jour, la désinstallation, les commandes, les réglages, les menus et tous les modes de jeu. Le guide intégré utilise automatiquement la langue actuelle du jeu.

## Prérequis

- Windows x64 et votre propre installation Steam légalement acquise de Bop It! The Video Game.
- **MelonLoader 0.7.3 Open-Beta**, x64. Le développement utilise le build du jeu sous Unity 2022.3.50f1.
- L'**environnement d'exécution .NET 6** Windows x64 pour exécuter le mod.
- Le fichier officiel Windows x64 **Prism v0.18.3** `prism.dll`, installé à côté de l'exécutable du jeu.
- Pour compiler le mod depuis le code source : un SDK .NET compatible avec le **pack de ciblage .NET 6** et les références générées par MelonLoader à partir de votre propre jeu.
- Pour compiler l'installateur depuis le code source : le **SDK .NET 10** sous Windows.

L'installateur obtient ses dépendances auprès de leurs sources officielles. L'installation d'une version compilée ne nécessite pas de SDK de développement ; l'installation de l'alpha en nécessite un.

<a id="build-from-source"></a>
## Compiler depuis le code source

1. Installez MelonLoader 0.7.3 Open-Beta dans le dossier du jeu. Lancez le jeu une fois, attendez que MelonLoader prépare ses fichiers, puis fermez-le. Les références générées doivent se trouver dans `MelonLoader\Il2CppAssemblies`, dans le dossier du jeu.
2. Téléchargez ou clonez ce dépôt et ouvrez PowerShell dans son dossier racine.
3. Remplacez le chemin d'exemple ci-dessous par l'emplacement de votre jeu, puis exécutez :

   ```powershell
   $gameDir = 'C:\Program Files (x86)\Steam\steamapps\common\Bop It!'
   dotnet build .\src\BopItAccess.csproj -c Release "-p:BopItGameDir=$gameDir"
   ```

La DLL compilée est `src\bin\Release\net6.0\BopItAccess.dll`. Le projet signale les références au jeu ou au chargeur manquantes avant de compiler. Si le SDK signale l'absence du pack de ciblage .NET 6, installez un SDK qui le contient. Le fichier `NuGet.Config` du projet ne configure aucune source de paquets en ligne.

Le parcours alpha de l'installateur prépare ses références de compilation locales sans lancer le jeu. Ces références sont des éléments temporaires nécessaires à la compilation ; elles ne sont jamais ajoutées aux commits ni incluses dans une version compilée du mod.

<a id="install-your-build"></a>
## Installer votre build

Le jeu étant fermé :

1. Copiez le fichier `BopItAccess.dll` compilé dans le dossier `Mods` du jeu, en créant ce dossier si nécessaire.
2. Téléchargez la version officielle Windows x64 de Prism v0.18.3 depuis les [versions de Prism](https://github.com/ethindp/prism/releases). Placez `prism.dll` à côté de l'exécutable du jeu.
3. Copiez le dossier `src\bin\Release\net6.0\documentation` du build dans le dossier du jeu en conservant tous ses sous-dossiers de langues. Conservez les fichiers de licence Prism applicables lorsque vous distribuez son binaire.
4. Lancez votre lecteur d'écran si vous en utilisez un, puis lancez le jeu par Steam. Attendez l'annonce de démarrage, puis celle du titre, de l'accueil ou du menu principal avant d'utiliser les commandes du jeu.

La compilation du mod ne télécharge ni ne compile Prism. Si la parole ne démarre pas, consultez `Mods\BopItAccess.log` dans le dossier du jeu. Une transmission réussie dans le journal confirme que le mod a envoyé du texte ; elle ne prouve pas que le son a été entendu.

Pour une archive ZIP de version compilée, copiez **tout son contenu** dans le dossier du jeu et fusionnez les dossiers ou remplacez les fichiers lorsque cela vous est demandé. L'archive contient le mod, Prism, les guides et les notices de licence ; MelonLoader et .NET s'installent séparément. Consultez le guide pour les instructions complètes d'installation manuelle.

## Modifier les réglages hors du jeu

Après le démarrage, `UserData\BopItAccess.ini`, dans le dossier du jeu, contient les réglages lisibles du jeu et du mod, les profils de voix et les commandes destinées aux joueurs. Fermez le jeu, ouvrez le fichier dans le Bloc-notes, modifiez les entrées existantes et enregistrez-le. Le mod lit les modifications au prochain lancement. Des commentaires expliquent les choix et plages de valeurs valides.

Par exemple, définissez `Language=en` dans `[Game]` pour rétablir l'anglais, réduisez `MusicVolume`, `SfxVolume` et `VoiceOverVolume`, ou définissez `Voice=System default` dans `[OneCore]` ou `[SAPI]` pour remplacer une voix inadaptée. Définissez `SpeechOutput=On` et `OutputMode=Auto` dans `[Mod]` pour rétablir la parole automatique. Conservez les autres entrées ; n'ajoutez pas de sections en double.

## Compiler l'installateur

Sous Windows avec le SDK .NET 10, exécutez :

```powershell
dotnet restore .\installer\BopItAccess.Installer.csproj --source https://api.nuget.org/v3/index.json
dotnet publish .\installer\BopItAccess.Installer.csproj -c Release -o .\build\installer --no-restore
```

Le résultat est `build\installer\BopItAccess.Installer.exe`. Il s'agit d'un exécutable autonome Windows x64 : les utilisateurs n'ont donc pas besoin de .NET 10 pour l'exécuter. L'installateur fourni n'est pas signé. Le guide décrit ses commandes et les demandes de sécurité de Windows.

`scripts/package-mod.ps1` prépare une archive ZIP de publication à partir d'un mod déjà compilé et correspondant à la version attendue. Elle contient les guides, notices et licences nécessaires aux joueurs. Les README destinés aux développeurs, les notes de travail Git, les références générées du jeu et les installateurs compilés sont exclus de cette archive. La préparation de l'archive ne compile pas le mod et ne publie pas de version sur GitHub.

## Note de transparence sur l'IA

Ce mod est « vibe-coded ». Tout son code a été entièrement généré et documenté par l'intelligence artificielle, avec une compréhension technique humaine limitée de son architecture sous-jacente. Utilisez ce mod à vos propres risques.

Cela dit, chaque fonctionnalité et chaque décision de conception du mod ont été imaginées et approuvées par des humains. Les tests n'ont jamais été automatisés ; ils ont été réalisés avec soin et de manière approfondie par de vrais joueurs et testeurs humains.

Veuillez noter que les textes et la documentation multilingues ont été générés par l'IA et n'ont pas été relus par des locuteurs natifs. Il faut s'attendre à d'importantes imprécisions de traduction. Sans programmation agentique, ce projet n'existerait pas. Merci de lui donner une chance !

## Licence et mentions légales

Le code source propre à Bop It Access et sa documentation sont sous **[licence MIT](../../LICENSE)**. Copyright © 2026 Christopher Shaw. Les dépendances conservent leurs propres licences ; la licence MIT ne les remplace pas et n'accorde aucun droit sur les ressources du jeu. Consultez [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) pour les notices des dépendances.

Bop It Access est un projet non officiel créé par un fan. Il n'est ni créé, ni approuvé, ni soutenu par Hasbro, Alliance, le développeur/éditeur du jeu, Valve, Microsoft, Unity, MelonLoader, Prism ou un fabricant de lecteur d'écran. **Bop It!**, ses personnages, illustrations, sons et marques appartiennent à Hasbro et à leurs ayants droit respectifs. Steam appartient à Valve. Les autres noms de produits, marques et logiciels restent la propriété de leurs détenteurs respectifs.

Vous devez posséder votre propre copie légale du jeu. Ce dépôt ne contient ni le jeu ni ses ressources et n'accorde aucun droit sur ceux-ci. Pour les informations relatives aux droits du jeu d'origine, consultez le [site officiel de Bop It!](https://bopitthevideogame.com/) et sa [page Steam](https://store.steampowered.com/app/3214360/).

## Merci

À ceux qui ont testé ce mod avant sa sortie et l'ont aidé à arriver là où il en est : merci. Vous vous reconnaîtrez. Aux joueurs qui partagent leurs retours, essaient le mod pour la première fois ou croient en moi et en ce projet : merci. Votre soutien me motive à continuer de créer dans ce monde fou où nous vivons. J'espère que ce projet vous permettra de profiter plus facilement du jeu et de jouer avec d'autres. Merci infiniment à tous. Amusez-vous bien avec Bop It !

— Christopher Shaw
