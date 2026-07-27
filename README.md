# HTML Directory List Generator (HDLG2)

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Version](https://img.shields.io/badge/Version-1.4.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)

**HTML Directory List Generator (HDLG2)** is a lightweight, high-performance desktop application built with C# and Windows Forms on .NET 10. It allows users to recursively scan any system directory and export the contents into beautifully structured **HTML** or highly queryable **XML** files, including detailed extraction of extended metadata for media and documents.

---

## ✨ Key Features

- 📁 **Recursive Folder Scanning**: Fast, lazy-loaded interactive UI tree navigation alongside configurable recursive scans.
- 🌐 **Beautiful HTML Export**: Generates a self-contained, fully-styled HTML report featuring:
  - Responsive inline CSS styling (system fonts only for full self-containment, offline support and security; no external Google Fonts).
  - Interactive table of contents with navigable anchor links.
  - Quick-click `file:///` pathways to directly open indexed items.
  - Branded footer with inline SVG logo and generator attribution.
- 📊 **Structured XML Export**: Employs clean, high-performance streaming writers (`XmlWriter`) for easy data migration and integration.
- 🔍 **Deep Metadata Extraction**: Automatically parses and extracts domain-specific properties:
  - **Images**: Dimensions (Width and Height) and camera model (using *ImageSharp*).
  - **Word & Excel**: Document title, creator, and creation date (using *OpenXML*).
  - **PDF**: Document title (using *PdfPig*).
  - **MP3**: Title, duration, album, year, performers, album artists, composers, and copyright (using *TagLibSharp*).
- ⚡ **Performance Instrumentation**: Measures, records, and displays execution metrics (scantime, compilation, and save-time).
- 🛡️ **DoS Hardening (Property Extraction)**: Configurable safeguards in `FilePropertyLimits` — rejects files exceeding 100 MB, enforces a 30-second timeout per property getter in `FilePropertyBrowser`, and caps image dimensions at 32 768 px via `ImagePropertyGetter` (with `DecoderOptions.MaxFrames = 1`).
- 🪵 **Structured Logging**: Rolling diagnostic logs written daily to `%LOCALAPPDATA%\HDLG\logs`.
- 🎨 **Modern WinForms UI (v1.4)**: Fluent-style desktop interface powered by **Krypton.Toolkit** (Microsoft 365 Blue Light palette), with a dashboard layout on the main window and harmonized explorer/about dialogs.
- 🏷️ **HDLG Monogram Branding**: Original geometric logo (Concept C, 2×2 layout, accent `#0284C8`) in the About dialog, application icon, and HTML export footer (inline SVG).

---

## 🏛️ Architecture Overview

The solution consists of three primary layers:

1. **`HDLG winforms` (Desktop GUI App)**: 
   - Manages the Windows Forms layout, progress metrics, and output generation orchestrators.
   - Built on `Microsoft.Extensions.Hosting` utilizing full Dependency Injection (DI) and robust background threading (`Task.Run`) to keep the UI perfectly responsive.
   - UI theme initialized via `AppUiBootstrap` using **Krypton.Toolkit** controls (`KryptonForm`, `KryptonHeaderGroup`, `KryptonTreeView`, etc.).
2. **`HdlgFileProperty` (Extraction Engine)**:
   - Houses the core extraction strategy (`IFilePropertyGetter`), delegating specialized tasks to respective metadata engines based on MIME/file formats.
   - `FilePropertyBrowser` orchestrates getters with file-size checks and per-getter timeouts; `FilePropertyLimits` centralizes the configurable thresholds.
3. **`HDLG.Tests` (Unit Tests)**:
   - xUnit v3-based test suite (`xunit.v3` + runner) with FluentAssertions and Moq, covering export engines, metadata extraction orchestration, directory model logic, property getter contracts, security helpers (e.g. OpenWithDefaultProgram), UI bootstrap, and structural WinForms UI tests.

---

## ⚙️ Prerequisites & Installation

- **Operating System**: Windows 10 (Build 26100 or higher) / Windows 11
- **Runtime**: [.NET 10.0 SDK or Desktop Runtime](https://dotnet.microsoft.com/download)

---

## 🛠️ Building the Project

You can compile the solution quickly via CLI or your preferred IDE:

### 1. Standard CLI Build
```powershell
dotnet build HDLG.sln
```

### 2. Using the helper batch file
Run the shortcut compilation script at the root directory:
```powershell
.\build.bat
```

### 3. Running Tests
Execute the full test suite:
```powershell
dotnet test HDLG.sln
```

The `HDLG.Tests` project covers:
- **DirectoryBrowserTests** — XML and HTML export validation (parameter guards, output structure).
- **FilePropertyBrowserTests** — Property extraction orchestration (getter delegation, multi-getter combination, oversized-file rejection, timeout behavior, statistics logging).
- **FilePropertyGetterStatisticTests** — Execution statistics validation for getters (elapsed time, file count).
- **HdlgDirectoryTests** — Directory model construction, recursive browse behavior, and equality semantics.
- **HdlgFileTests** — File model validation (construction, properties, size/extension computation).
- **PropertyGetterTests** — Image, MP3, and PDF getter contracts (support detection, property extraction, oversized-image rejection).
- **WordPropertyGetterTests** — Word document property extraction and error handling.
- **ExcelPropertyGetterTests** — Excel workbook property extraction and error handling.
- **OpenWithDefaultProgramTests** — Security validation for `MainWindow.OpenWithDefaultProgram` (dangerous extension blocklist to prevent process injection).
- **AppUiBootstrapTests** — Validates Krypton global palette initialization and watermark removal.
- **AppBrandingTests** — Validates inline SVG markup and HTML footer generation.
- **AppLogoRendererTests** — Validates packaged logo/icon asset loading.
- **WinFormsUiTests** — Structural UI tests (STA thread) verifying Krypton controls on `MainWindow`, `BrowserForm`, and `Credit`.

---

## 🪵 Diagnostics & Logs

For troubleshooting, the application records real-time rolling diagnostic logs to the local AppData directory:
```
%LOCALAPPDATA%\HDLG\logs\log.txt
```
Daily rolling produces dated files (e.g. `log20260626.txt`) alongside the active `log.txt`.

---

## 📄 License & Header Boilerplate

This project is licensed under the terms of the **GNU General Public License Version 3** (GPLv3).

```
This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
```

---

## 🎨 Branding & Assets

The application uses an original **HDLG** monogram (Concept C: 2×2 letter grid without visible grid lines, geometric style, Krypton palette `#0284C8`).

| Asset | Path | Usage |
|---|---|---|
| Wordmark (SVG source) | `HDLG winforms/Assets/hdlg-logo.svg` | Canonical vector; HTML inline SVG via `AppBranding` |
| Wordmark (PNG) | `HDLG winforms/Assets/hdlg-logo.png` | About dialog, NuGet package icon |
| App icon (SVG source) | `HDLG winforms/Assets/hdlg-app-icon.svg` | Pixel-aligned icon for 16–48 px Windows chrome |
| App icon (ICO) | `HDLG winforms/Assets/hdlg-icon.ico` | Executable and window icons |

Regenerate raster assets after editing SVG sources (requires [Inkscape](https://inkscape.org/) installed):

```powershell
.\scripts\GenerateAppLogoAssets.ps1
```

Optional maintainer export via tests: `$env:HDLG_EXPORT_LOGO='1'; dotnet test HDLG.sln --filter ExportPackagedLogoAssets_WhenRequested`