# Analyse du Projet : HDLG2 (HTML Directory List Generator)

Ce fichier fournit un contexte aux agents IA travaillant sur ce projet.

**Version** : 1.2.0  
**Dernière mise à jour** : 19 mai 2026  
**Propriétaire** : Martin Labelle (@bestter)

---

## Golden Rules – Règles Absolues (Ne jamais transgresser)

1. **Ne modifie jamais les fichiers AGENTS.md, ANTIGRAVITY.md et .editorconfig sans autorisation explicite**  
   Ces fichiers sont la source de vérité pour l'agent IA.  
   **Toute modification nécessite une autorisation claire et explicite du propriétaire du projet** (exemple : « Tu peux réécrire AGENTS.md » ou « Mets à jour la section X »). Sans cette autorisation, tu n'y touches pas.

2. **Minimalisme extrême**  
   Priorise toujours un code fonctionnel, durable et facilement maintenable.  
   **Aucune nouvelle dépendance** (NuGet ou autre) ne doit être ajoutée sans validation explicite, même pour des utilitaires « petits ».

3. **Demande avant d'improviser**  
   Si une fonctionnalité, un pattern ou une décision d'architecture n'est pas clairement documenté dans `AGENTS.md` ou `ANTIGRAVITY.md` → **pose la question** au lieu de deviner.

4. **Respecte .editorconfig + langue dans le code**  
   Avant de générer ou de modifier du code, analyse et respecte **impérativement** les règles des fichiers `.editorconfig` (racine **et** `HDLG winforms\.editorconfig`).  
   Tous les commentaires de code, messages de commit et documentation technique doivent être rédigés **en anglais**, à l'exception des fichiers `AGENTS.md` et `ANTIGRAVITY.md` qui doivent rester en français.

5. **Toute nouvelle fonctionnalité doit être accompagnée de tests**  
   Chaque nouvelle fonctionnalité implémentée doit inclure des tests unitaires dans le projet `HDLG.Tests` qui valident son bon fonctionnement. **Aucune fonctionnalité ne sera considérée comme terminée sans ses tests.**

6. **Toute modification de fonctionnalité existante doit mettre à jour les tests**  
   Lorsqu'une fonctionnalité existante est modifiée, les tests unitaires correspondants doivent être mis à jour pour refléter le nouveau comportement attendu. Les tests obsolètes ou cassés ne sont **jamais acceptables**.

---

## 🔄 Flux de Travail Standard pour les Agents IA

Avant toute modification, suis toujours cet ordre :

1. Lire intégralement `AGENTS.md` (et `ANTIGRAVITY.md` si présent).
2. Analyser le besoin et **imaginer le design** (surtout pour l'UI).
3. Implémenter **uniquement** selon les règles définies dans ce document.
4. Exécuter `dotnet build HDLG.sln` et obtenir **0 erreur, 0 warning** de build.
5. Exécuter `dotnet test HDLG.sln` et obtenir **0 échec** de test.
6. Si le moindre doute existe sur l'architecture, le design ou une décision non documentée → **poser la question immédiatement**.

---

## 📌 Vue d'ensemble du Projet

- **Nom du projet** : HDLG2 (HTML Directory List Generator)
- **Type d'application** : Application de bureau (Windows Forms)
- **Langage principal** : C# (.NET 10)
- **Framework cible** : `net10.0-windows10.0.26100.0`
- **Licence** : GNU General Public License v3.0 (GPLv3)
- **Objectif** : Fournir une interface graphique utilisateur (GUI) permettant de parcourir le contenu d'un répertoire (et ses sous-répertoires) et de générer un listing structuré au format **XML** ou **HTML**, avec extraction des propriétés étendues des fichiers (images, documents Word/Excel, PDF, MP3).

---

## 📁 Structure de la Solution

La solution `HDLG.sln` contient **trois projets** :

### Projet 1 : `HDLG winforms` (Application WinForms principale)

| Fichier | Rôle |
|---|---|
| **`Program.cs`** | Point d'entrée de l'application. Configure l'injection de dépendances (DI) via `Microsoft.Extensions.Hosting`, initialise Serilog, et enregistre les gestionnaires d'exceptions globales (thread UI + threads d'arrière-plan). |
| **`MainWindow.cs`** | Fenêtre principale. Permet de sélectionner un répertoire, lancer le parcours en XML ou HTML via des `BackgroundWorker`, afficher les temps de performance (browse, save, total). |
| **`MainWindow.Designer.cs`** | Code généré par le Designer Windows Forms. **Ne jamais modifier manuellement.** |
| **`BrowserForm.cs`** | Formulaire de navigation arborescente (TreeView) avec chargement paresseux (lazy loading) des répertoires/fichiers. Affiche les propriétés d'un fichier sélectionné dans un `ListView`. |
| **`BrowserForm.Designer.cs`** | Code généré par le Designer. **Ne jamais modifier manuellement.** |
| **`DirectoryBrowser.cs`** | Cœur logique de l'export. Contient `SaveAsXMLAsync()` (génération XML via `XmlWriter`) et `SaveAsHTMLAsync()` (génération HTML avec CSS embarqué et Google Fonts). |
| **`Directory.cs`** | Modèle de données (legacy) représentant un répertoire. Implémente `IEquatable`, `IComparable`. Parcourt récursivement les sous-répertoires et fichiers. |
| **`HdlgDirectory.cs`** | Modèle de données (version refactorisée) d'un répertoire. Même rôle que `Directory.cs` mais avec un code plus propre (utilisation de `IReadOnlyList`, `ArgumentNullException.ThrowIfNull`, etc.). |
| **`File.cs`** | Modèle de données (legacy) d'un fichier. Contient les métadonnées (nom, chemin, extension, taille, date de création, propriétés étendues). |
| **`HdlgFile.cs`** | Modèle de données (version refactorisée) d'un fichier. Version améliorée de `File.cs`. |
| **`PerformanceCount.cs`** | Structure pour stocker les métriques de performance (temps de parcours, sauvegarde, total). |
| **`credit.cs`** | Formulaire « À propos » affichant la version, la licence GPLv3, et les crédits des icônes (Flaticon). |
| **`hdlg.css`** | Feuille de style CSS embarquée dans les fichiers HTML générés (utilise Google Fonts Roboto Serif et Source Sans Pro). |

### Projet 2 : `HdlgFileProperty` (Bibliothèque d'extraction de propriétés)

| Fichier | Rôle |
|---|---|
| **`IFilePropertyGetter.cs`** | Interface définissant le contrat pour les extracteurs de propriétés : `IsSupportedFile()` et `GetFileProperties()`. |
| **`FilePropertyBrowser.cs`** | Orchestrateur qui délègue l'extraction des propriétés au bon `IFilePropertyGetter` en fonction du type de fichier. Collecte aussi des statistiques de performance par getter. |
| **`ImagePropertyGetter.cs`** | Extraction de propriétés d'images (via `SixLabors.ImageSharp` et `System.Drawing.Common`). |
| **`WordPropertyGetter.cs`** | Extraction de propriétés de documents Word (via `DocumentFormat.OpenXml`). |
| **`ExcelPropertyGetter.cs`** | Extraction de propriétés de fichiers Excel (via `DocumentFormat.OpenXml`). |
| **`PdfPropertyGetter.cs`** | Extraction de propriétés de fichiers PDF (via `iText`). |
| **`Mp3PropertyGetter.cs`** | Extraction de propriétés de fichiers MP3 (via `TagLibSharp`). |
| **`FilePropertyGetterStatistic.cs`** | Wrapper autour d'un `IFilePropertyGetter` pour mesurer le temps d'exécution et compter les fichiers traités. |

### Projet 3 : `HDLG.Tests` (Tests unitaires – xUnit)

| Fichier | Rôle |
|---|---|
| **`DirectoryBrowserTests.cs`** | Tests de `DirectoryBrowser` : validation des paramètres (null/empty), génération XML (structure, balises attendues) et génération HTML (DOCTYPE, structure, contenu). Utilise des fichiers temporaires nettoyés via `IDisposable`. |
| **`FilePropertyBrowserTests.cs`** | Tests de `FilePropertyBrowser` : validation du constructeur (null logger, null getters), délégation correcte aux `IFilePropertyGetter` via mocks Moq, combinaison de propriétés de multiples getters, et vérification des statistiques de logging. |
| **`HdlgDirectoryTests.cs`** | Tests de `HdlgDirectory` : construction avec propriétés valides, validation des paramètres null, parcours avec/sans sous-répertoires, et vérification de l'égalité par chemin. Utilise des répertoires temporaires sur le système de fichiers. |
| **`PropertyGetterTests.cs`** | Tests des implémentations `IFilePropertyGetter` : `ImagePropertyGetter`, `Mp3PropertyGetter`, `PdfPropertyGetter`, `WordPropertyGetter`, `ExcelPropertyGetter`. Vérifie `AddLogger()`, la validation null, et `IsSupportedFile()` via `[Theory]`/`[InlineData]`. |

---

## 📦 Dépendances NuGet

### `HDLG winforms`
| Package | Version | Usage |
|---|---|---|
| `Microsoft.Extensions.Hosting` | 10.0.8 | Hébergement et injection de dépendances |
| `Microsoft.Extensions.DependencyInjection` | 10.0.8 | Conteneur IoC |
| `Microsoft.Extensions.Logging` | 10.0.8 | Abstraction de logging |
| `Newtonsoft.Json` | 13.0.4 | Sérialisation JSON |
| `Serilog.Sinks.File` | 7.0.0 | Journalisation vers fichiers |

### `HdlgFileProperty`
| Package | Version | Usage |
|---|---|---|
| `DocumentFormat.OpenXml` | 3.5.1 | Lecture de documents Office (Word, Excel) |
| `itext` | 9.6.0 | Lecture de propriétés PDF |
| `itext.bouncy-castle-adapter` | 9.6.0 | Adaptateur cryptographique pour iText |
| `Newtonsoft.Json` | 13.0.4 | Sérialisation JSON |
| `Serilog` | 4.3.1 | Logging |
| `SixLabors.ImageSharp` | 3.1.12 | Traitement d'images |
| `System.Drawing.Common` | 10.0.8 | API graphique Windows |
| `TagLibSharp` | 2.3.0 | Lecture de métadonnées audio (MP3) |

### `HDLG.Tests`
| Package | Version | Usage |
|---|---|---|
| `coverlet.collector` | 6.0.4 | Collecte de couverture de code |
| `FluentAssertions` | 8.10.0 | Assertions lisibles et expressives |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | Infrastructure de test .NET |
| `Moq` | 4.20.72 | Mocking d'interfaces pour tests isolés |
| `xunit` | 2.9.3 | Framework de tests unitaires |
| `xunit.runner.visualstudio` | 3.1.4 | Runner Visual Studio pour xUnit |

---

## 🎨 Stratégie UI/UX (Windows Forms – Designer-First)

Pour toute modification de l'interface utilisateur :

1. **Les fichiers `.Designer.cs` ne doivent JAMAIS être modifiés manuellement.**  
   Le layout est géré exclusivement par le Designer Visual Studio via `InitializeComponent()`.

2. **La logique événementielle reste dans les fichiers `.cs` correspondants** (ex: `MainWindow.cs`, `BrowserForm.cs`).

3. **Injection de dépendances** : Les formulaires reçoivent leurs dépendances via le constructeur (DI configurée dans `Program.cs`). Ne jamais instancier manuellement les services.

4. **Opérations longues** : Utiliser `BackgroundWorker` pour les tâches de parcours et d'export afin de ne pas bloquer le thread UI. L'utilisation de `Application.DoEvents()` existe actuellement pour la barre de progression.

---

## ⚙️ Fonctionnalités Clés Implémentées

1. **Parcours récursif de répertoires** : Navigation dans un répertoire sélectionné et ses sous-répertoires (optionnel via checkbox).
2. **Export XML** : Génération asynchrone d'un fichier XML structuré (`XmlWriter`) contenant l'arborescence complète avec métadonnées.
3. **Export HTML** : Génération asynchrone d'un fichier HTML avec CSS embarqué, Google Fonts, table des matières avec ancres navigables, et liens `file:///` vers les fichiers.
4. **Extraction de propriétés étendues** : Pour chaque fichier, extraction automatique des métadonnées spécifiques selon le type (dimensions d'image, auteur Word/Excel, tags MP3, etc.).
5. **Navigation arborescente** (`BrowserForm`) : Exploration interactive du système de fichiers avec lazy loading et affichage des propriétés.
6. **Métriques de performance** : Mesure et affichage des temps de parcours, sauvegarde et total.
7. **Logging structuré** : Journalisation via Serilog dans `%LOCALAPPDATA%\HDLG\logs\log.txt` (rolling quotidien).
8. **Gestion d'exceptions globale** : Intercepteurs pour les exceptions du thread UI et des threads d'arrière-plan.

---

## 🛠️ Directives de Développement (Pour les agents)

- **Architecture** : La solution suit un modèle à deux couches : l'application WinForms (`HDLG winforms`) qui gère l'UI et l'orchestration, et la bibliothèque (`HdlgFileProperty`) qui gère l'extraction de propriétés. Cette séparation doit être maintenue.
- **Pattern Strategy** : L'extraction de propriétés utilise le pattern Strategy via l'interface `IFilePropertyGetter`. Pour ajouter le support d'un nouveau type de fichier, créer une nouvelle implémentation de cette interface dans le projet `HdlgFileProperty` et l'enregistrer dans le DI de `Program.cs`.
- **Modèles en doublon** : Il existe actuellement deux versions de modèles (`Directory.cs`/`File.cs` et `HdlgDirectory.cs`/`HdlgFile.cs`). Les versions `Hdlg*` sont la version refactorisée et doivent être privilégiées pour tout nouveau développement.
- **Logging** : Utiliser exclusivement Serilog via l'injection du `Logger`. Ne pas créer de nouvelles instances de logger en dehors de `Program.cs`.
- **Build** : Le projet se compile via `dotnet build HDLG.sln`. Un fichier `build.bat` est fourni à la racine pour simplifier la commande.
- **CI/CD** : GitHub Actions (`.github/workflows/dotnet-desktop.yml`) exécute le build sur push/PR vers `main` en configurations Debug et Release. Dependabot est activé pour les mises à jour NuGet et GitHub Actions.
- **Tests** : Le projet `HDLG.Tests` (xUnit) contient les tests unitaires de la solution. Les tests utilisent **FluentAssertions** pour des assertions expressives et **Moq** pour le mocking d'interfaces. Pour exécuter les tests : `dotnet test HDLG.sln`. Tout nouveau code doit être accompagné de tests unitaires correspondants dans ce projet.
- **Encodage des fichiers** : Les fichiers `.cs` et `.vb` utilisent des **tabulations** pour l'indentation (`indent_style = tab`, `tab_width = 4`) et les fins de ligne **CRLF** (`end_of_line = crlf`).
