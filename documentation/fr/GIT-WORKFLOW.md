# Méthode de travail Git pour Bop It Access

Git conserve l'historique du code source et de la documentation de ce projet. Un **commit** est un instantané nommé que vous pouvez consulter ou retrouver. GitHub publie ces commits pour permettre aux autres de lire les changements et de compiler eux-mêmes le projet. Un commit ne crée pas automatiquement une version publique.

## À propos de l'historique existant

Les 37 premiers builds du code source, de `0.1.0` à `0.6.12`, ont été importés en commits distincts à partir des archives disponibles et de leurs notes de changement d'origine. Leurs horodatages Git correspondent à l'importation et non aux dates des builds d'origine. Le travail ultérieur est enregistré directement dans les commits du code source. Git et GitHub constituent désormais l'historique des changements du projet ; aucun document d'historique des builds séparé n'est tenu à jour.

L'adresse GitHub sans réponse de Christopher Shaw est utilisée pour l'auteur des commits. Les commits assistés par l'IA incluent une ligne `Co-authored-by` indiquant le modèle qui a réellement contribué. Les comptes rendus de session identifient GPT-6 Luna pour le premier build historique et GPT-6 Sol pour les 36 suivants. Utilisez le nom actuel du modèle ayant contribué pour les futurs commits.

Les DLL compilées, installateurs, ZIP de publication, assemblages générés du jeu, journaux personnels et fichiers de compilation temporaires restent hors de l'historique du code source Git. Les étiquettes de version locales ne sont pas publiées automatiquement. Créer une version GitHub est une étape séparée et volontaire.

## Commandes utiles

Ouvrez PowerShell dans le dépôt, puis exécutez :

```powershell
git status                 # See changed, added and untracked files
git diff                   # Inspect changes that are not staged
git log --oneline           # Browse commits
git show --stat HEAD~1      # Inspect the previous commit's changed files
```

`git status` affiche les fichiers modifiés, ajoutés et non suivis. `git diff` montre les changements non indexés. `git log --oneline` permet de parcourir les commits. `git show --stat HEAD~1` affiche les fichiers modifiés par le commit précédent.

Ces commandes consultent le dépôt sans modifier le mod installé ni le jeu.

## Pour chaque changement futur

1. Effectuez les changements souhaités dans le code source et la documentation. Pour un nouveau build, mettez sa version à jour.
2. Mettez à jour chaque guide traduit concerné. Conservez la documentation des joueurs et les notices de licence avec les fichiers compilés ; les README de développement et cette méthode de travail ne sont pas destinés aux versions pour joueurs.
3. Compilez lorsque le changement nécessite un nouveau binaire et préparez les fichiers locaux. Les essais en jeu sont réalisés par des joueurs humains lorsqu'ils sont demandés ; une compilation réussie ne constitue pas une vérification de l'exécution.
4. Exécutez `git status` et `git diff`. Ajoutez les fichiers voulus à l'index, puis examinez `git diff --cached`. N'ajoutez pas de binaires générés, de journaux privés ni de références du jeu aux changements indexés.
5. Créez un commit descriptif dont le titre ne contient aucun numéro de version. Dans son corps, expliquez les changements et leur raison, ainsi que les vérifications et limites pertinentes. Mentionnez le véritable modèle d'IA dans une ligne de coauteur s'il a contribué :

   ```text
   Co-authored-by: MODEL NAME <noreply@openai.com>
   ```

   Remplacez `MODEL NAME` par le modèle qui a écrit le travail. Gardez Christopher Shaw comme auteur, avec l'adresse `336230252+Chris-E-Shaw@users.noreply.github.com`.
6. Lorsque les changements sont prêts à être publiés, exécutez `git push origin main`. Cela publie les commits de la branche sans envoyer les étiquettes locales ni créer une version.

Un changement cohérent peut réunir son code source et sa documentation dans un même commit. Des commits distincts restent utiles pour des changements indépendants. Git n'envoie pas automatiquement le travail : publiez-le volontairement lorsqu'il est prêt à être lu par d'autres.
