# Analyse du Projet : HDLG2 (HTML Directory List Generator)

Ce fichier fournit un contexte aux agents IA travaillant sur ce projet.

**Version** : 1.4.0.0  
**Dernière mise à jour** : 22 juillet 2026 — Audit complet et synchronisation de la documentation, modernisation UI WinForms via `AppUiBootstrap` (palette `MinimalistSlate`, fond slate `#F8FAFC`, Segoe UI, contrôles modernes `ModernControls` / `ModernCardPanel`), monogramme HDLG original (`AppBranding`, assets SVG/Inkscape, pied de page HTML), suite complète de 240 tests unitaires et UI (`HDLG.Tests` avec support thread STA xUnit v3).
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
| **`Program.cs`** | Point d'entrée de l'application. Configure l'injection de dépendances (DI) via `Microsoft.Extensions.Hosting`, initialise Serilog, appelle `AppUiBootstrap.Configure()`, et enregistre les gestionnaires d'exceptions globales (thread UI + threads d'arrière-plan). |
| **`AppUiBootstrap.cs`** | Initialise la configuration UI globale (High-DPI PerMonitorV2, thème `MinimalistSlate`) ; `RemoveFormBranding()` applique le fond slate `#F8FAFC` et la police Segoe UI. |
| **`AppBranding.cs`** | Centralise le markup SVG inline (exports HTML), le pied de page HTML, et le chargement des assets logo/icône (`Assets/hdlg-logo.png`, `Assets/hdlg-icon.ico`). |
| **`AppLogoRenderer.cs`** | Rendu bitmap de secours du monogramme (géométrie alignée sur le SVG) si les assets empaquetés sont absents. |
| **`Assets/`** | Sources SVG (`hdlg-logo.svg`, `hdlg-app-icon.svg`) et exports PNG/ICO générés via `scripts/GenerateAppLogoAssets.ps1` (Inkscape). |
| **`MainWindow.cs`** | Fenêtre principale (`Form`). Layout dashboard avec panneaux `ModernCardPanel`, sélection de répertoire, parcours XML/HTML via `Task.Run`, lancement de l'UI Explorer, affichage des métriques de performance. |
| **`MainWindow.Designer.cs`** | Layout WinForms de la fenêtre principale (contrôles `ModernCardPanel`, `ModernButton`, `ProgressBar`, `StatusStrip`, etc.). |
| **`BrowserForm.cs`** | Formulaire de navigation arborescente (`TreeView`) avec chargement paresseux (lazy loading) des répertoires/fichiers. Affiche les propriétés d'un fichier sélectionné dans un `ListView`. |
| **`BrowserForm.Designer.cs`** | Layout WinForms de l'explorateur (contrôles `ModernCardPanel`, `TreeView`, `ListView`, `SplitContainer`). |
| **`ModernControls.cs`** | Composants UI personnalisés (`ModernCardPanel` avec en-tête et description, `ModernButton` avec effets de survol/clic). |
| **`DirectoryBrowser.cs`** | Cœur logique de l'export. Contient `SaveAsXMLAsync()` (génération XML via `XmlWriter`) et `SaveAsHTMLAsync()` (génération HTML self-contained avec CSS embarqué ; polices système uniquement, CSP strict sans Google Fonts externes). |
| **`Directory.cs`** | Modèle de données (legacy) représentant un répertoire. Implémente `IEquatable`, `IComparable`. |
| **`HdlgDirectory.cs`** | Modèle de données refactorisé d'un répertoire (`IReadOnlyList`, `ArgumentNullException.ThrowIfNull`, etc.). |
| **`File.cs`** | Modèle de données (legacy) d'un fichier (métadonnées, taille, date, propriétés étendues). |
| **`HdlgFile.cs`** | Modèle de données refactorisé d'un fichier. |
| **`PerformanceCount.cs`** | Structure pour stocker les métriques de performance (browse, save, total). |
| **`credit.cs`** | Formulaire « About » (`Form`) affichant la version, la licence GPLv3, et le monogramme HDLG (`AppBranding.LoadLogoImage()`). |
| **`hdlg.css`** | Feuille de style CSS embarquée dans les fichiers HTML générés (polices système uniquement pour self-containment). |

### Projet 2 : `HdlgFileProperty` (Bibliothèque d'extraction de propriétés)

| Fichier | Rôle |
|---|---|
| **`IFilePropertyGetter.cs`** | Interface définissant le contrat pour les extracteurs de propriétés : `IsSupportedFile()` et `GetFileProperties()`. |
| **`FilePropertyLimits.cs`** | Constantes configurables de protection anti-DoS : `MaxFileSizeBytes` (100 Mo), `MaxImageDimension` (32 768 px), `PropertyExtractionTimeout` (30 s). |
| **`FilePropertyBrowser.cs`** | Orchestrateur qui délègue l'extraction des propriétés au bon `IFilePropertyGetter` en fonction du type de fichier. Vérifie la taille du fichier (`FileInfo.Length`), applique un timeout par getter (`Task.Run` + `CancellationTokenSource`), collecte les statistiques de performance par getter. |
| **`ImagePropertyGetter.cs`** | Extraction de propriétés d'images (via `SixLabors.ImageSharp` uniquement). Rejette les fichiers trop volumineux, utilise `DecoderOptions { MaxFrames = 1 }` pour `Identify`, et refuse les dimensions dépassant `MaxImageDimension`. |
| **`WordPropertyGetter.cs`** | Extraction de propriétés de documents Word (via `DocumentFormat.OpenXml`). |
| **`ExcelPropertyGetter.cs`** | Extraction de propriétés de fichiers Excel (via `DocumentFormat.OpenXml`). |
| **`PdfPropertyGetter.cs`** | Extraction de propriétés de fichiers PDF (via `PdfPig`). |
| **`Mp3PropertyGetter.cs`** | Extraction de propriétés de fichiers MP3 (via `TagLibSharp`). |
| **`FilePropertyGetterStatistic.cs`** | Wrapper autour d'un `IFilePropertyGetter` pour mesurer le temps d'exécution et compter les fichiers traités. |

### Projet 3 : `HDLG.Tests` (Tests unitaires – xUnit)

| Fichier | Rôle |
|---|---|
| **`DirectoryBrowserTests.cs`** | Tests de `DirectoryBrowser` : validation des paramètres (null/empty), génération XML (structure, balises attendues) et génération HTML (DOCTYPE, structure, CSP, contenu). |
| **`FilePropertyBrowserTests.cs`** | Tests de `FilePropertyBrowser` : validation du constructeur (null logger, null getters), délégation aux `IFilePropertyGetter` via Moq, combinaison multi-getters, rejet des fichiers trop volumineux, timeout, et logging. |
| **`HdlgDirectoryTests.cs`** | Tests de `HdlgDirectory` : construction, validation null, parcours avec/sans sous-répertoires, égalité par chemin. |
| **`HdlgFileTests.cs`** | Tests de `HdlgFile` : construction, métadonnées, calculs de taille et extension. |
| **`PropertyGetterTests.cs`** | Tests des implémentations `IFilePropertyGetter` : `ImagePropertyGetter`, `Mp3PropertyGetter`, `PdfPropertyGetter`. |
| **`WordPropertyGetterTests.cs`** | Tests de `WordPropertyGetter` : extraction des propriétés, gestion des fichiers invalides/manquants, logging. |
| **`ExcelPropertyGetterTests.cs`** | Tests de `ExcelPropertyGetter` : extraction des propriétés, gestion des fichiers invalides/manquants, logging. |
| **`FilePropertyGetterStatisticTests.cs`** | Tests de `FilePropertyGetterStatistic` : validation des statistiques d'exécution d'un getter (temps écoulé, nombre de fichiers traités). |
| **`OpenWithDefaultProgramTests.cs`** | Tests de `MainWindow.OpenWithDefaultProgram` (sécurité : validation des extensions dangereuses pour prévenir l'injection de processus). |
| **`AppUiBootstrapTests.cs`** | Tests du bootstrap UI (`MinimalistSlate`, High-DPI, suppression de branding legacy). |
| **`AppBrandingTests.cs`** | Tests du markup SVG inline et du pied de page HTML généré. |
| **`AppLogoRendererTests.cs`** | Tests de chargement des assets logo/icône empaquetés. |
| **`BrowserFormLoadTests.cs`** | Tests de chargement et lazy-loading dans `BrowserForm`. |
| **`PerformanceCountTests.cs`** | Tests de calcul et formatage des métriques de temps d'exécution (`PerformanceCount`). |
| **`WinFormsUiTestCollection.cs`** | Collection xUnit sérialisée pour éviter les conflits GDI+ entre tests WinForms. |
| **`WinFormsUiTests.cs`** | Tests UI structurels (thread STA) : instanciation des formulaires et présence des contrôles modernes clés (`MainWindow`, `BrowserForm`, `Credit`). |
| **`ExcelSetup.cs` / `ImageSetup.cs` / `WordSetup.cs` / `WordPropertyGetterTestSetup.cs`** | Classes utilitaires de création de fixtures de test temporaires isolées. |

---

## 📦 Dépendances NuGet

### `HDLG winforms`

| Package | Version | Usage |
|---|---|---|
| `Microsoft.Extensions.Hosting` | 10.0.10 | Hébergement et injection de dépendances (transitive : DependencyInjection + Logging) |
| `Serilog.Sinks.File` | 7.0.0 | Journalisation vers fichiers |

### `HdlgFileProperty`

| Package | Version | Usage |
|---|---|---|
| `DocumentFormat.OpenXml` | 3.5.1 | Lecture de documents Office (Word, Excel) |
| `PdfPig` | 0.1.15 | Lecture de propriétés PDF |
| `Serilog` | 4.4.0 | Logging |
| `SixLabors.ImageSharp` | 3.1.12 | Traitement d'images |
| `System.Drawing.Common` | 10.0.10 | API graphique Windows |
| `TagLibSharp` | 2.3.0 | Lecture de métadonnées audio (MP3) |

### `HDLG.Tests`

| Package                     | Version | Usage                                  |
| -----------------------------| ---------| ----------------------------------------|
| `coverlet.collector`        | 10.0.1  | Collecte de couverture de code         |
| `FluentAssertions`          | 8.10.0  | Assertions lisibles et expressives     |
| `Microsoft.AspNetCore.TestHost` | 10.0.10 | Hébergement de test ASP.NET Core (référencé par le projet de tests) |
| `Microsoft.NET.Test.Sdk`    | 18.8.1  | Infrastructure de test .NET            |
| `Moq`                       | 4.20.72 | Mocking d'interfaces pour tests isolés |
| `Serilog`                   | 4.4.0   | Logging dans les tests                 |
| `TagLibSharp`               | 2.3.0   | Création de fixtures audio pour les tests |
| `xunit.v3`                  | 3.2.2   | Framework de tests unitaires (v3)      |
| `xunit.runner.visualstudio` | 3.1.5   | Runner Visual Studio pour xUnit        |

---

## 🎨 Stratégie UI/UX (Windows Forms)

Pour toute modification de l'interface utilisateur :

1. **Thème global** : Toute form hérite de `Form` et s'appuie sur `AppUiBootstrap.Configure()` et `AppUiBootstrap.RemoveFormBranding()` (palette `MinimalistSlate`, fond `#F8FAFC`, police Segoe UI). Ne pas hardcoder de couleurs inline sauf nécessité documentée.

2. **Composants UI modernes** : Les cartes avec en-tête et description utilisent `ModernCardPanel` (défini dans `ModernControls.cs`). Les boutons d'action stylisés utilisent `ModernButton`.

3. **Fichiers `.Designer.cs`** : Le layout (contrôles, ancres, docking) est défini dans les `.Designer.cs`. La logique métier reste dans les `.cs` non-Designer.

4. **La logique événementielle reste dans les fichiers `.cs` correspondants** (ex: `MainWindow.cs`, `BrowserForm.cs`).

5. **Injection de dépendances** : Les formulaires reçoivent leurs dépendances via le constructeur (DI configurée dans `Program.cs`). Ne jamais instancier manuellement les services.

6. **Opérations longues** : Utiliser `Task.Run` + `ConfigureAwait(true)` pour les tâches de parcours et d'export afin de ne pas bloquer le thread UI. La barre de progression utilise `ProgressBar` en mode `Marquee` pendant le traitement.

7. **Palette visuelle** : Alignée sur `hdlg.css` (fond `#F8FAFC`, accent `#0284C8`, texte `#0F172A`).

8. **Branding** : Monogramme HDLG original (Concept C, grille 2×2 sans lignes visibles). Wordmark via `hdlg-logo.svg` (About + HTML) ; icône Windows via `hdlg-app-icon.svg` (optimisée 16–48 px). Régénérer PNG/ICO avec `scripts/GenerateAppLogoAssets.ps1` (Inkscape requis). Ne pas réutiliser d'assets Flaticon.

---

## ⚙️ Fonctionnalités Clés Implémentées

1. **Parcours récursif de répertoires** : Navigation dans un répertoire sélectionné et ses sous-répertoires (optionnel via checkbox).
2. **Export XML** : Génération asynchrone d'un fichier XML structuré (`XmlWriter`) contenant l'arborescence complète avec métadonnées.
3. **Export HTML** : Génération asynchrone d'un fichier HTML self-contained avec CSS embarqué (polices système, CSP strict sans Google Fonts pour offline et mitigation XSS), table des matières avec ancres navigables, et liens `file:///` vers les fichiers.
4. **Extraction de propriétés étendues** : Pour chaque fichier, extraction automatique des métadonnées spécifiques selon le type (dimensions d'image, auteur Word/Excel, tags MP3, etc.).
5. **Navigation arborescente** (`BrowserForm`) : Exploration interactive du système de fichiers avec lazy loading et affichage des propriétés.
6. **Métriques de performance** : Mesure et affichage des temps de parcours, sauvegarde et total.
7. **Logging structuré** : Journalisation via Serilog dans `%LOCALAPPDATA%\HDLG\logs\log.txt` (rolling quotidien).
8. **Gestion d'exceptions globale** : Intercepteurs pour les exceptions du thread UI et des threads d'arrière-plan.
9. **Protection anti-DoS (extraction de propriétés)** : Limites de taille de fichier (100 Mo), timeout par getter (30 s), et plafond de dimensions image (32 768 px) pour mitiger les attaques par déni de service lors du parsing de fichiers non fiables.
10. **Interface modernisée (v1.4)** : `MainWindow` en layout dashboard avec `ModernCardPanel`, section Source Directory / Export, bouton About intégré, `BrowserForm` et `Credit` harmonisés.
11. **Branding HDLG** : Monogramme original dans About, icône application, et pied de page des exports HTML (SVG inline self-contained).

---

## 🛠️ Directives de Développement (Pour les agents)

- **Architecture** : La solution suit un modèle à deux couches : l'application WinForms (`HDLG winforms`) qui gère l'UI et l'orchestration, et la bibliothèque (`HdlgFileProperty`) qui gère l'extraction de propriétés. Cette séparation doit être maintenue.
- **Pattern Strategy** : L'extraction de propriétés utilise le pattern Strategy via l'interface `IFilePropertyGetter`. Pour ajouter le support d'un nouveau type de fichier, créer une nouvelle implémentation de cette interface dans le projet `HdlgFileProperty` et l'enregistrer dans le DI de `Program.cs`.
- **Modèles en doublon** : Il existe actuellement deux versions de modèles (`Directory.cs`/`File.cs` et `HdlgDirectory.cs`/`HdlgFile.cs`). Les versions `Hdlg*` sont la version refactorisée et doivent être privilégiées pour tout nouveau développement.
- **Logging** : Utiliser exclusivement Serilog via l'injection du `Logger`. Ne pas créer de nouvelles instances de logger en dehors de `Program.cs`.
- **Build** : Le projet se compile via `dotnet build HDLG.sln`. Un fichier `build.bat` est fourni à la racine pour simplifier la commande.
- **CI/CD** : GitHub Actions (`.github/workflows/dotnet-desktop.yml`) exécute le build (via msbuild) sur push/PR vers `main` en configurations Debug et Release. Dependabot est activé pour les mises à jour NuGet.
- **Tests** : Le projet `HDLG.Tests` (xUnit v3) contient les 240 tests unitaires et UI de la solution. Les tests utilisent **FluentAssertions** pour des assertions expressives et **Moq** pour le mocking d'interfaces. Pour exécuter les tests : `dotnet test HDLG.sln`. Tout nouveau code doit être accompagné de tests unitaires correspondants dans ce projet.
- **Encodage des fichiers** : Les fichiers `.cs` et `.vb` utilisent des **tabulations** pour l'indentation (`indent_style = tab`, `tab_width = 4`) et les fins de ligne **CRLF** (`end_of_line = crlf`).

## 🐙 Conventions Git et Historique

- **Gestion des branches (Branching) :** Ne pousse **jamais** de code directement sur les branches principales (`main` ou `master`). Crée toujours une branche de travail distincte, courte et descriptive (ex: `feature/nom-de-la-fonctionnalite`, `bugfix/nom-du-bug`, `refactor/nom-du-composant`).
- **Pull Requests (PR) :** Tout changement doit obligatoirement passer par la création d'une Pull Request. Aucune fusion (merge) ne doit être effectuée sans une revue préalable.
- **Commits atomiques :** Chaque commit doit représenter une unité de travail cohérente et complète. Évite les commits qui mélangent plusieurs changements non liés.
- **Format des messages :** Utilise la spécification *Conventional Commits* (`feat:`, `fix:`, `refactor:`, `chore:`, etc.) pour structurer les titres de tes commits.
- **Signature de l'IA :** Signe toujours tes commits avec ton nom et ton nom de modèle exact à la fin de la description (ex: `Generated-by: Hermes 2 Pro` ou `Generated-by: OpenClaw`). Cette traçabilité est essentielle pour auditer avec précision le code produit par les différents agents locaux ou distants.
- **Langue :** Conformément à la Règle Dorée #4, l'intégralité des messages de commit (titre et description) doit être rédigée **strictement en anglais**.
