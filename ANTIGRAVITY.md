# Directives IA : HTML Directory List Generator (Winforms/C#)

## 1. Contexte du Projet
Tu agis en tant qu'expert C# .NET et Winforms. L'objectif de cette application est de lire des répertoires locaux et de générer des listes HTML propres et performantes.

## 2. Règles de Lecture et d'Analyse (Économie de Tokens)
* **Ignore le code autogénéré :** Ne lis et ne modifie JAMAIS les fichiers `*.Designer.cs` ou `*.resx` à moins que je te le demande explicitement.
* **Focus ciblé :** Concentre-toi uniquement sur le "Code-Behind" (ex: `Form1.cs`) et les classes de logique métier.
* **Pas de scan global :** Si je te parle d'un bug de génération HTML, regarde en priorité la méthode qui construit la chaîne de caractères, ne scanne pas l'interface utilisateur.

## 3. Génération de Code et Bonnes Pratiques C#
* **Performance HTML :** Pour toute génération de HTML, privilégie systématiquement l'utilisation de `StringBuilder` plutôt que la concaténation classique avec `+` ou `+=` pour éviter d'allouer trop de mémoire.
* **Séparation des préoccupations :** Garde la logique de parcours de fichiers (`System.IO`) et de génération HTML séparée du fil d'exécution de l'interface utilisateur (UI Thread) pour éviter que l'application Winforms ne gèle (utilise `async/await` et `Task.Run` si nécessaire).
* **Code minimaliste :** Si je te demande de modifier une méthode, renvoie **uniquement** la méthode modifiée ou le bloc de code concerné, pas la classe entière. Utilise `// ... code existant ...` pour indiquer les parties inchangées.

## 4. Prévention des Boucles d'Erreurs
* Si une solution proposée génère une erreur de compilation Winforms ou lève une exception d'accès refusé (`UnauthorizedAccessException` lors de la lecture des dossiers), propose UNE correction. Si elle échoue encore, arrête-toi, explique le problème brièvement et demande-moi d'inspecter les permissions locales. Ne boucle pas.