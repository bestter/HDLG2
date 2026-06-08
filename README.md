# HTML Directory List Generator (HDLG2)

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)

**HTML Directory List Generator (HDLG2)** is a lightweight, high-performance desktop application built with C# and Windows Forms on .NET 10. It allows users to recursively scan any system directory and export the contents into beautifully structured **HTML** or highly queryable **XML** files, including detailed extraction of extended metadata for media and documents.

---

## ✨ Key Features

- 📁 **Recursive Folder Scanning**: Fast, lazy-loaded interactive UI tree navigation alongside configurable recursive scans.
- 🌐 **Beautiful HTML Export**: Generates a self-contained, fully-styled HTML report featuring:
  - Responsive inline CSS styling (system fonts only for full self-containment, offline support and security; no external Google Fonts).
  - Interactive table of contents with navigable anchor links.
  - Quick-click `file:///` pathways to directly open indexed items.
- 📊 **Structured XML Export**: Employs clean, high-performance streaming writers (`XmlWriter`) for easy data migration and integration.
- 🔍 **Deep Metadata Extraction**: Automatically parses and extracts domain-specific properties:
  - **Images**: Dimensions (Width and Height) and camera model (using *ImageSharp*).
  - **Word & Excel**: Document title, creator, and creation date (using *OpenXML*).
  - **PDF**: Document title (using *PdfPig*).
  - **MP3**: Title, duration, album, year, performers, album artists, composers, and copyright (using *TagLibSharp*).
- ⚡ **Performance Instrumentation**: Measures, records, and displays execution metrics (scantime, compilation, and save-time).
- 🪵 **Structured Logging**: Rolling diagnostic logs written daily to `%LOCALAPPDATA%\HDLG\logs`.

---

## 🏛️ Architecture Overview

The solution consists of three primary layers:

1. **`HDLG winforms` (Desktop GUI App)**: 
   - Manages the Windows Forms layout, progress metrics, and output generation orchestrators.
   - Built on `Microsoft.Extensions.Hosting` utilizing full Dependency Injection (DI) and robust background threading (`Task.Run`) to keep the UI perfectly responsive.
2. **`HdlgFileProperty` (Extraction Engine)**:
   - Houses the core extraction strategy (`IFilePropertyGetter`), delegating specialized tasks to respective metadata engines based on MIME/file formats.
3. **`HDLG.Tests` (Unit Tests)**:
   - xUnit v3-based test suite (`xunit.v3` + runner) with FluentAssertions and Moq, covering export engines, metadata extraction orchestration, directory model logic, property getter contracts, and security helpers (e.g. OpenWithDefaultProgram).

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
- **FilePropertyBrowserTests** — Property extraction orchestration (getter delegation, multi-getter combination, statistics logging).
- **FilePropertyGetterStatisticTests** — Execution statistics validation for getters (elapsed time, file count).
- **HdlgDirectoryTests** — Directory model construction, recursive browse behavior, and equality semantics.
- **HdlgFileTests** — File model validation (construction, properties, size/extension computation).
- **PropertyGetterTests** — File-type support detection for all property getter implementations (Image, MP3, PDF, Word, Excel).
- **OpenWithDefaultProgramTests** — Security validation for `MainWindow.OpenWithDefaultProgram` (dangerous extension blocklist to prevent process injection).

---

## 🪵 Diagnostics & Logs

For troubleshooting, the application records real-time rolling diagnostic logs to the local AppData directory:
```
%LOCALAPPDATA%\HDLG\logs\log-[yyyyMMdd].txt
```

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

## 🎨 Asset & Icon Credits

- **Application Icon**: [Root directory icons](https://www.flaticon.com/free-icons/root-directory) created by *Freepik - Flaticon*.