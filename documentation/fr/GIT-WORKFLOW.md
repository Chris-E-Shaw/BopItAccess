# Flux de travail Git pour Bop It Access

Git conserve un historique des modifications apportées au code source et aux documents du projet. Un **commit** est un instantané nommé que vous pouvez inspecter ou auquel vous pouvez revenir. GitHub publie cet historique des sources afin que d'autres puissent lire le code et créer eux-mêmes le mod. Le référentiel ne contient pas de fichiers mod compilés ni de versions GitHub.

## À propos de l'histoire existante

Les archives sources des builds `v0.1.0` à travers `v0.6.12` ont été importés sous forme de 37 commits Git successifs. Chaque commit décrit les modifications de la source à l'aide de l'entrée correspondante dans [BopItAccess-build-history.html](BopItAccess-build-history.html). Ces commits ont été créés lors de l'importation Git, donc leurs horodatages Git ne correspondent **pas** aux dates de construction d'origine. Leurs sujets décrivent les modifications sans numéros de version ; le document d'historique de construction enregistre quel instantané source appartient à chaque version.

Les fichiers ZIP de publication, les DLL compilées et les résultats de build temporaires restent en dehors de l'historique des sources de Git. La page d'historique de construction renvoie aux validations sources GitHub correspondantes. Les balises de version existantes restent locales et ne font pas partie de la publication GitHub initiale. Aucune balise de version ou version GitHub n'est encore publiée.

Git s'engage à utiliser l'adresse de non-réponse GitHub de Christopher Shaw comme auteur. Les commits écrits avec Codex incluent également un `Co-authored-by` bande-annonce nommant le modèle qui a contribué au travail. Les enregistrements de session identifient GPT-6 Luna pour la première version historique et GPT-6 Sol pour les 36 suivantes. Si le modèle change pour une validation ultérieure, utilisez son nouveau nom dans la fin de cette validation.

## Commandes utiles

Ouvrez PowerShell dans ce répertoire de projet, puis exécutez :

```powershell
git status                         # See changed, added, and untracked files
git diff                           # See changes that have not been staged
git log --oneline                   # Browse source commits
git show --stat HEAD~1             # See files changed in the preceding commit
```

Ces commandes inspectent uniquement le référentiel ; ils ne changent ni le mod ni le jeu installé.

## Pour chaque future build

1. Apportez les modifications à la source et choisissez le numéro de version de build suivant.
2. Construisez le mod et préparez les archives locales comme d'habitude. Inclure l'intégralité `documentation` dossier, avec le guide en anglais et chaque sous-dossier de langue traduite, dans chaque archive d'installation. Copiez ce dossier dans l'installation du jeu lors de l'installation d'une version. Le guide du jeu lit le code HTML de la langue actuelle du jeu à chaque ouverture. Inspectez le résultat avant d’enregistrer la construction comme terminée. Les archives compilées restent en dehors du GitHub.
3. Courir `git status` et `git diff`. Vérifiez quels fichiers ont été modifiés. Organisez les modifications prévues dans la source et la documentation, puis examinez-les avec `git diff --cached`.
4. Créez un commit source descriptif sans numéro de version dans son sujet. Inclure un `Co-authored-by` bande-annonce avec le nom réel du modèle lorsque Codex a écrit le commit. Par exemple, `git commit -m "feat(speech): add example setting" -m "Co-authored-by: MODEL NAME <noreply@openai.com>"`; remplacer `MODEL NAME` avec le modèle utilisé pour ce commit.
5. Ajouter une entrée pour la construction à `BopItAccess-build-history.html`, en utilisant le format de style commit existant. Décrivez le changement réel, sa raison et toutes les limitations pertinentes, et liez la validation source de l'étape 4. Mettez à jour les copies traduites correspondantes avant l'empaquetage. Validez l’historique mis à jour avec le même auteur et une bande-annonce précise du co-auteur. Actualisez le dossier de documentation local et l'archive si le fichier historique y a déjà été copié.
6. Publiez les commits source et historique avec `git push origin main` quand il est prêt. Cela pousse uniquement la branche ; il ne pousse pas les balises de version locale et ne crée pas de versions GitHub.

Un petit travail qui ne produit pas de build peut avoir son propre commit. Le prochain commit de build peut alors le suivre. Conservez les journaux personnels, les installations de jeux, les binaires générés et autres fichiers spécifiques à la machine hors des validations. Si les versions GitHub deviennent utiles plus tard, décidez des balises et des téléchargements compilés à ce moment-là.

Git ne télécharge pas automatiquement les nouveaux travaux. Après chaque commit local, poussez-le délibérément lorsqu'il est prêt à être vu par d'autres personnes.
