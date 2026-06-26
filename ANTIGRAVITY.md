# Instructions de Gestion des Ressources IA

## 1. Analyse de Contexte Minimale
* Ne relis pas l'intégralité du projet pour des modifications locales. 
* Avant de scanner un dossier, demande-moi si c'est nécessaire.
* Priorise la lecture des fichiers `.cs` spécifiquement mentionnés.

## 2. Optimisation des Réponses
* **Code seulement :** Si la modification est évidente, fournis uniquement le snippet de code sans explications exhaustives.
* **Pas de réécriture complète :** Ne réécris pas un fichier entier si seulement deux lignes changent. Utilise des commentaires `// ... reste du code` pour économiser les tokens.

## 3. Prévention du "Looping"
* Si tu ne trouves pas la solution après 2 tentatives de correction d'erreur, arrête-toi et demande-moi des précisions au lieu de brûler des crédits en bouclant.

## 4. Priorité au Local
* Utilise les définitions locales (types, interfaces) avant de chercher à indexer les bibliothèques externes.

## 5. Conventions de Tests
* Les tests unitaires se trouvent dans le projet `HDLG.Tests` (xUnit).
* Utilise **FluentAssertions** pour les assertions et **Moq** pour le mocking d'interfaces.
* Nettoie toujours les fichiers et répertoires temporaires via `IDisposable`.
* Tout nouveau code doit être accompagné de tests unitaires. Ne crée pas de tests si la tâche ne concerne que de la documentation ou de la configuration.
* Les tests UI WinForms (`WinFormsUiTests`) s'exécutent sur thread **STA** et vérifient la structure des contrôles Krypton sans interaction graphique utilisateur.