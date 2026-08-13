# Address JSON Export Review Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **How to reload:** In a new Grok session on branch `Improvements20260812`, say: `Continue the plan in Documentation/plans/2026-08-12-address-json-export-review.md`. Also indexed at `.grok/CURRENT-PLAN.md`.
>
> **Source review:** `C:\Users\marti\AppData\Local\Temp\grok-S-1-5-21-4154132604-2431172249-2155633718-1001\grok-review-9e8cd444.md` (temp; this plan is the durable copy of the required work).

**Goal:** Keep the JSON export contract and UI, but delete the third cloned export pipeline in `MainWindow` and close the review gaps that block merge.

**Architecture:** One UI runner owns dialogs, progress, button lock, and exceptions. One browse/save helper owns `HdlgDirectory.BrowseAsync` + timing + `DirectoryBrowser.SaveAs*`. The three buttons stay; they only pass a save dialog, a file extension, and a save delegate. Do not split `DirectoryBrowser.cs` in this plan (810 lines, under the 1k tripwire). Do not revert the already-committed Krypton bump.

**Tech Stack:** C# / .NET 10, WinForms + Krypton, xUnit v3, FluentAssertions, `Utf8JsonWriter`. No new NuGet packages.

**Branch:** `Improvements20260812` (do not commit to `main`).

**Hard constraints from AGENTS.md:**
- English comments, commit messages, and this plan.
- Tabs + CRLF in `HDLG winforms` `.cs` files; `HDLG.Tests` currently uses 4-space indent — match the file you edit.
- `dotnet build HDLG.sln` must be 0 error / 0 warning; `dotnet test HDLG.sln` must be 0 failed.
- **Do not edit `AGENTS.md` or `ANTIGRAVITY.md` unless the owner explicitly says so.** Task 7 is gated.
- No new dependencies.

**Out of scope:**
- Rewriting XML/HTML property writers.
- Splitting `DirectoryBrowser` into per-format types.
- Rewriting git history to unbundle the Krypton bump.

---

## File map

| File | Responsibility in this plan |
|---|---|
| `HDLG.Tests/DirectoryBrowserTests.cs` | Stronger JSON tree contract tests (nested dirs, counts, bool). |
| `HDLG winforms/MainWindow.cs` | Collapse 3 click handlers + 3 `Perform*` methods into `RunExportAsync` + `PerformDirectoryBrowseAsync`. |
| `HDLG winforms/DirectoryBrowser.cs` | Keep public `SaveAsJSONAsync` name; make the sync walk honest (comment + `FileOptions.Asynchronous` not required). Optional: extract private sync `WriteJsonDocument`. |
| `HDLG winforms/MainWindow.Designer.cs` | Same `ButtonStyle` / margins on the 2×2 export buttons. |
| `HDLG.sln` | Map Tests + Benchmark `Release\|x64` to `Release\|x64` (match Debug). |
| `AGENTS.md` | Gated. Only if owner authorizes: sync NuGet table to real csproj versions. |

---

## Review findings this plan covers

| Review issue | Task |
|---|---|
| 1 Blocker: cloned XML/HTML/JSON orchestration | Tasks 2–3 |
| 2 `SaveAsJSONAsync` is async in name only | Task 4 |
| 3 Stale `AGENTS.md` NuGet table | Task 7 (gated) |
| 4 Krypton bump mixed into feature | Out of scope (already committed; do not revert) |
| 5 Incomplete x64 Release mappings | Task 6 |
| 6 Weak JSON happy-path test | Task 1 |
| 7 Designer `ButtonStyle.Cluster` mismatch | Task 5 |

---

### Task 1: Strengthen JSON export tests

**Files:**
- Modify: `HDLG.Tests/DirectoryBrowserTests.cs`
- Do not change production code in this task.

- [x] **Step 1: Add a failing nested-tree + native-bool test**

In `DirectoryBrowserTests`, after `SaveAsJSONAsync_FilesWithProperties_UsesOriginalKeysAndNativeTypes`, add:

```csharp
        [Fact]
        public async Task SaveAsJSONAsync_BrowsedTree_WritesFilesCountsAndNestedDirectories()
        {
            var subDirPath = Path.Combine(baseDirectoryPath, "child");
            System.IO.Directory.CreateDirectory(subDirPath);
            System.IO.File.WriteAllText(Path.Combine(subDirPath, "nested.txt"), "n");

            var properties = new System.Collections.Generic.Dictionary<string, IConvertible>
            {
                { "Flag", true },
            };

            var browserMock = new Mock<HdlgFileProperty.FilePropertyBrowser>(
                loggerMock.Object,
                Array.Empty<HdlgFileProperty.IFilePropertyGetter>());
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.Name == "file1.txt")))
                .ReturnsAsync(properties);
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.Name != "file1.txt")))
                .ReturnsAsync((IReadOnlyDictionary<string, IConvertible>?)null);

            var dir = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            await dir.BrowseAsync(browserMock.Object);

            await directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, dir);

            using var doc = JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(tempJsonFilePath));
            JsonElement root = doc.RootElement;
            root.GetProperty("DirectoriesCount").GetInt64().Should().Be(dir.TotalDirectories);
            root.GetProperty("FilesCount").GetInt64().Should().Be(dir.TotalFiles);

            JsonElement tree = root.GetProperty("Root");
            tree.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("file1.txt");

            JsonElement file1 = tree.GetProperty("Files").EnumerateArray()
                .Single(f => f.GetProperty("Name").GetString() == "file1.txt");
            file1.GetProperty("ExtentedProperties").GetProperty("Flag").GetBoolean().Should().BeTrue();

            JsonElement child = tree.GetProperty("Directories").EnumerateArray()
                .Single(d => d.GetProperty("Name").GetString() == "child");
            child.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("nested.txt");
        }
```

Also change `SaveAsJSONAsync_ValidInputs_GeneratesCompactJsonFile` so it browses first (the current fixture writes `file1.txt` but never calls `BrowseAsync`, so `Files` is empty):

```csharp
            var browser = new HdlgFileProperty.FilePropertyBrowser(loggerMock.Object);
            await testDirectory.BrowseAsync(browser);
            await directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, testDirectory);
```

Then after parsing, add:

```csharp
            tree.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("file1.txt");
```

- [x] **Step 2: Run the new test (expect pass on current writer; the bool/nested path already works)**

Run:

```powershell
dotnet test HDLG.sln --filter "FullyQualifiedName~SaveAsJSONAsync" --no-restore
```

If restore is needed, drop `--no-restore`. Expected: all `SaveAsJSONAsync*` tests pass. If `Flag` bool fails, Task 4 must keep `WriteJsonConvertibleValue` `case bool`.

These tests lock the contract before the MainWindow refactor. They are not expected to fail against current `DirectoryBrowser`; they close the review gap.

- [x] **Step 3: Commit**

```powershell
git add HDLG.Tests/DirectoryBrowserTests.cs
git commit -m "test: lock JSON tree, counts, and bool properties"
```

Use Conventional Commits. End the commit body with:

```
Generated-by: Grok 4.6
```

---

### Task 2: Collapse the three Perform* methods

**Files:**
- Modify: `HDLG winforms/MainWindow.cs` (the three `PerformDirectoryBrowse*Async` methods; XML ~179–214, HTML ~481–513, JSON ~588–621)

- [x] **Step 1: Add one helper and point all three callers at it**

Replace `PerformDirectoryBrowseXmlAsync`, `PerformDirectoryBrowseHtmlAsync`, and `PerformDirectoryBrowseJsonAsync` with:

```csharp
		private async Task<PerformanceCount> PerformDirectoryBrowseAsync (
			string selectedDirectoryPath,
			string saveFilePath,
			Func<DirectoryBrowser, HdlgDirectory, string, Task> save)
		{
			Logger.Debug( "{MethodName} started at {StartTime:T}", nameof( PerformDirectoryBrowseAsync ), DateTime.Now );
			if (string.IsNullOrWhiteSpace( selectedDirectoryPath ))
			{
				Logger.Information( "No {SelectedDirectoryParamName}", nameof( selectedDirectoryPath ) );
				return PerformanceCount.Empty;
			}

			Logger.Information( "{SelectedDirectory}", selectedDirectoryPath );
			HdlgDirectory directory = new( selectedDirectoryPath, true, cbBrowseSubDirectory.Checked, Logger );
			Stopwatch stopwatch = Stopwatch.StartNew( );

			Logger.Debug( "Ready to start {MethodName}", nameof( directory.BrowseAsync ) );
			await directory.BrowseAsync( propertyBrowser ).ConfigureAwait( false );
			Logger.Debug( "{MethodName} of directory {DirectoryName} done", nameof( directory.BrowseAsync ), directory.Name );
			TimeSpan browseTime = stopwatch.Elapsed;
			propertyBrowser.LogGetterStatistics( );

			DirectoryBrowser db = new( Logger );
			await save( db, directory, saveFilePath ).ConfigureAwait( false );
			stopwatch.Stop( );

			var result = new PerformanceCount( )
			{
				BrowseTime = browseTime,
				SaveTime = stopwatch.Elapsed - browseTime,
				TotalTime = stopwatch.Elapsed
			};
			Logger.Information( "Done at {EndTime:T}", DateTime.Now );
			return result;
		}
```

Temporary wrappers so Task 3 can land separately (delete them in Task 3):

```csharp
		private Task<PerformanceCount> PerformDirectoryBrowseXmlAsync (string selectedDirectoryPath, string saveFilePath)
		{
			return PerformDirectoryBrowseAsync( selectedDirectoryPath, saveFilePath,
				(browser, directory, path) => browser.SaveAsXMLAsync( path, directory ) );
		}

		private Task<PerformanceCount> PerformDirectoryBrowseHtmlAsync (string selectedDirectoryPath, string saveFilePath)
		{
			return PerformDirectoryBrowseAsync( selectedDirectoryPath, saveFilePath,
				(browser, directory, path) => browser.SaveAsHTMLAsync( path, directory ) );
		}

		private Task<PerformanceCount> PerformDirectoryBrowseJsonAsync (string selectedDirectoryPath, string saveFilePath)
		{
			return PerformDirectoryBrowseAsync( selectedDirectoryPath, saveFilePath,
				(browser, directory, path) => browser.SaveAsJSONAsync( path, directory ) );
		}
```

Preserve behavior:
- Still honor `cbBrowseSubDirectory.Checked`.
- Still return `PerformanceCount.Empty` on blank path.
- Do not keep the HTML-only `Debug.Write` start line; use `Logger.Debug` like XML/JSON.

- [x] **Step 2: Build and test**

```powershell
dotnet build HDLG.sln --no-restore
dotnet test HDLG.sln --no-restore
```

Expected: 0 build warnings/errors, 0 failed tests.

- [x] **Step 3: Commit**

```powershell
git add "HDLG winforms/MainWindow.cs"
git commit -m "refactor: share browse and save timing across export formats"
```

---

### Task 3: Collapse the three click handlers (blocker)

**Files:**
- Modify: `HDLG winforms/MainWindow.cs` (`BtnStart_Click`, `BtnStartHtml_Click`, `BtnStartJson_Click`, and the Task 2 wrappers)

- [x] **Step 1: Add the UI runner**

Insert above `BtnStart_Click`:

```csharp
		private void ResetExportStatusLabels ()
		{
			progressBar1.Value = 0;
			toolStripStatusLabelBrowseTime.Text = string.Empty;
			toolStripStatusLabelSaveTime.Text = string.Empty;
			toolStripStatusLabelTotalTime.Text = string.Empty;
			toolStripStatusLabelException.Text = string.Empty;

#if !DEBUG
			toolStripStatusLabelBrowseTime.Visible = false;
			toolStripStatusLabelSaveTime.Visible = false;
			toolStripStatusLabelTotalTime.Visible = false;
#endif
		}

		private void SetExportControlsEnabled (bool enabled)
		{
			btnStartXml.Enabled = enabled;
			btnStartHtml.Enabled = enabled;
			btnStartJson.Enabled = enabled;
			if (btnStartUi != null)
			{
				btnStartUi.Enabled = enabled;
			}
		}

		private async Task RunExportAsync (
			SaveFileDialog dialog,
			string extensionWithDot,
			Func<DirectoryBrowser, HdlgDirectory, string, Task> save)
		{
			try
			{
				ResetExportStatusLabels( );

				if (string.IsNullOrWhiteSpace( selectedDirectory ))
				{
					return;
				}

				DirectoryInfo di = new( selectedDirectory );
				dialog.FileName = $"{di.Name}{extensionWithDot}";
				if (dialog.ShowDialog( ) != DialogResult.OK)
				{
					return;
				}

				SetExportControlsEnabled( false );
				UseWaitCursor = true;
				Logger.Information( "Start browse with {SelectedDirectory}", selectedDirectory );
				progressBar1.Style = ProgressBarStyle.Marquee;

				string directoryPath = selectedDirectory;
				string savePath = dialog.FileName;
				var perf = await Task.Run( () => PerformDirectoryBrowseAsync( directoryPath, savePath, save ) ).ConfigureAwait( true );

				progressBar1.Style = ProgressBarStyle.Blocks;
				progressBar1.Value = 100;
				UpdateUIWithPerformance( perf );
				OpenWithDefaultProgram( savePath );
			}
			catch (UnauthorizedAccessException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Access denied in {MethodName}", nameof( RunExportAsync ) );
			}
			catch (System.Security.SecurityException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Security exception in {MethodName}", nameof( RunExportAsync ) );
			}
			catch (IOException ex)
			{
				toolStripStatusLabelException.Text = "An IO error occurred";
				Logger.Error( ex, "IO Error in {MethodName}", nameof( RunExportAsync ) );
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				toolStripStatusLabelException.Text = "An unexpected error occurred";
				Logger.Error( ex, "Error in {MethodName}", nameof( RunExportAsync ) );
			}
#pragma warning restore CA1031 // Do not catch general exception types
			finally
			{
				SetExportControlsEnabled( true );
				UseWaitCursor = false;
			}
		}
```

Intentional small consistency change: `#if !DEBUG` hide of timing labels now applies to HTML and JSON as well (previously XML-only). Do not preserve that XML-only special case.

- [x] **Step 2: Replace the three click handlers**

```csharp
		private async void BtnStart_Click (object sender, EventArgs e)
		{
			await RunExportAsync(
				saveContentFileDialog,
				".xml",
				(browser, directory, path) => browser.SaveAsXMLAsync( path, directory ) ).ConfigureAwait( true );
		}

		private async void BtnStartHtml_Click (object sender, EventArgs e)
		{
			await RunExportAsync(
				saveFileDialogHtml,
				".html",
				(browser, directory, path) => browser.SaveAsHTMLAsync( path, directory ) ).ConfigureAwait( true );
		}

		private async void BtnStartJson_Click (object sender, EventArgs e)
		{
			await RunExportAsync(
				saveFileDialogJson,
				".json",
				(browser, directory, path) => browser.SaveAsJSONAsync( path, directory ) ).ConfigureAwait( true );
		}
```

Delete `PerformDirectoryBrowseXmlAsync`, `PerformDirectoryBrowseHtmlAsync`, and `PerformDirectoryBrowseJsonAsync`.

Leave `BtnStartUi_Click` and `BtnAbout_Click` unchanged.

Leave the empty `SaveFileDialogHtml_FileOk` alone (pre-existing; not this review).

- [x] **Step 3: Build and test**

```powershell
dotnet build HDLG.sln
dotnet test HDLG.sln
```

Expected: 0 errors, 0 warnings, 0 failed tests. `WinFormsUiTests` still finds `btnStartXml`, `btnStartHtml`, `btnStartJson`.

- [x] **Step 4: Commit**

```powershell
git add "HDLG winforms/MainWindow.cs"
git commit -m "refactor: run XML HTML and JSON export through one UI path"
```

---

### Task 4: Make the JSON save boundary honest

**Files:**
- Modify: `HDLG winforms/DirectoryBrowser.cs` (`SaveAsJSONAsync` ~675–701)
- Modify: `HDLG.Tests/DirectoryBrowserTests.cs` only if the public method is renamed (this plan does **not** rename it)

- [x] **Step 1: Keep the public name; pull the sync walk into a private method**

Replace `SaveAsJSONAsync` body so the public method is a thin async flush around a named sync walk:

```csharp
		public async Task SaveAsJSONAsync (string filePath, HdlgDirectory directory)
		{
			if (string.IsNullOrWhiteSpace( filePath ))
			{
				throw new ArgumentException( $"'{nameof( filePath )}' cannot be null or whitespace.", nameof( filePath ) );
			}

			ArgumentNullException.ThrowIfNull( directory );

			FileInfo fileInfo = new( filePath );
			using FileStream fileStream = new( fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None );
			using Utf8JsonWriter writer = new( fileStream, new JsonWriterOptions { Indented = false } );
			WriteJsonDocument( writer, directory );
			await writer.FlushAsync( ).ConfigureAwait( false );
		}

		/// <summary>
		/// Walks the directory tree synchronously into <paramref name="writer"/>.
		/// Callers that must not block the UI should run <see cref="SaveAsJSONAsync"/> on a worker thread.
		/// </summary>
		private void WriteJsonDocument (Utf8JsonWriter writer, HdlgDirectory directory)
		{
			string? version = typeof( DirectoryBrowser ).Assembly.GetName( ).Version?.ToString( );

			writer.WriteStartObject( );
			writer.WriteString( "Version", version );
			writer.WriteString( "Directory", directory.Path );
			writer.WriteString( "DateTime", DateTime.Now.ToString( "O", CultureInfo.InvariantCulture ) );
			writer.WriteNumber( "DirectoriesCount", directory.TotalDirectories );
			writer.WriteNumber( "FilesCount", directory.TotalFiles );
			writer.WritePropertyName( "Root" );
			WriteJsonDirectory( writer, directory );
			writer.WriteEndObject( );
		}
```

Do not rename `SaveAsJSONAsync`. Tests, `MainWindow`, README, and AGENTS already use that name. Do not add `FileOptions.Asynchronous`; the UI already wraps browse+save in `Task.Run`.

- [x] **Step 2: Re-run JSON tests**

```powershell
dotnet test HDLG.sln --filter "FullyQualifiedName~SaveAsJSONAsync"
```

Expected: pass, including Task 1 tests.

- [x] **Step 3: Commit**

```powershell
git add "HDLG winforms/DirectoryBrowser.cs"
git commit -m "refactor: isolate synchronous JSON tree walk from async flush"
```

---

### Task 5: Even the 2×2 export buttons

**Files:**
- Modify: `HDLG winforms/MainWindow.Designer.cs` (`btnStartHtml` ~195–205, `btnStartJson` ~207–216, `btnStartUi` ~218–227)

- [x] **Step 1: Match style and margins**

On `btnStartHtml`, remove the one-off cluster style:

```csharp
			btnStartHtml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartHtml.Location = new Point(279, 19);
			btnStartHtml.Margin = new Padding(3, 3, 6, 3);
			btnStartHtml.Name = "btnStartHtml";
			btnStartHtml.Size = new Size(258, 58);
			btnStartHtml.TabIndex = 1;
			btnStartHtml.Values.Text = "Export HTML";
			btnStartHtml.Click += BtnStartHtml_Click;
```

Do **not** leave `btnStartHtml.ButtonStyle = Krypton.Toolkit.ButtonStyle.Cluster;`.

On `btnStartJson` and `btnStartUi`, keep Anchor fill. Align JSON margin with XML (`3, 3, 6, 3`). Leave `btnStartUi` right-column margin as `3, 3, 3, 3`. Designer `Size`/`Location` values are initial only; `TableLayoutPanel` stretch is what matters.

- [x] **Step 2: Build WinForms UI tests**

```powershell
dotnet test HDLG.sln --filter "FullyQualifiedName~MainWindow_InitializesWithKryptonDashboardControls"
```

Expected: pass.

- [x] **Step 3: Commit**

```powershell
git add "HDLG winforms/MainWindow.Designer.cs"
git commit -m "fix: use the same button style on the export grid"
```

---

### Task 6: Finish x64 Release mappings

**Files:**
- Modify: `HDLG.sln` (Tests `{6D8E0960-F219-4A21-9CF6-43880D3FF354}`, Benchmark `{42730287-9B8A-7ED6-2F08-B7B524CBA6F7}`)

- [x] **Step 1: Point Release|x64 at Release|x64**

Replace these two pairs:

```
{6D8E0960-F219-4A21-9CF6-43880D3FF354}.Release|x64.ActiveCfg = Release|Any CPU
{6D8E0960-F219-4A21-9CF6-43880D3FF354}.Release|x64.Build.0 = Release|Any CPU
```

with:

```
{6D8E0960-F219-4A21-9CF6-43880D3FF354}.Release|x64.ActiveCfg = Release|x64
{6D8E0960-F219-4A21-9CF6-43880D3FF354}.Release|x64.Build.0 = Release|x64
```

Replace these two pairs:

```
{42730287-9B8A-7ED6-2F08-B7B524CBA6F7}.Release|x64.ActiveCfg = Release|Any CPU
{42730287-9B8A-7ED6-2F08-B7B524CBA6F7}.Release|x64.Build.0 = Release|Any CPU
```

with:

```
{42730287-9B8A-7ED6-2F08-B7B524CBA6F7}.Release|x64.ActiveCfg = Release|x64
{42730287-9B8A-7ED6-2F08-B7B524CBA6F7}.Release|x64.Build.0 = Release|x64
```

Both projects already declare `<Platforms>AnyCPU;x64</Platforms>`.

- [x] **Step 2: Build Release x64**

```powershell
dotnet build HDLG.sln -c Release -p:Platform=x64
dotnet test HDLG.sln -c Release -p:Platform=x64 --no-build
```

Expected: both succeed. If `--no-build` cannot find test assemblies, drop `--no-build` and re-run test.

- [x] **Step 3: Commit**

```powershell
git add HDLG.sln
git commit -m "chore: map Tests and Benchmark Release x64 to x64"
```

---

### Task 7: Sync AGENTS.md NuGet table (gated)

**Do not start this task unless the owner says one of:** `Tu peux réécrire AGENTS.md` / `Mets à jour la section NuGet` / `You can update AGENTS.md`.

**Files:**
- Modify: `AGENTS.md` (NuGet tables ~124–155)
- Modify: `ANTIGRAVITY.md` only if the owner also authorizes that file

If authorized, set the table to the versions in the csproj files on this branch:

| Package | Version to write | Source |
|---|---|---|
| `Krypton.Toolkit` | `105.26.7.201` | `HDLG winforms/HDLG winforms.csproj` |
| `Microsoft.Extensions.Hosting` | `10.0.11` | same |
| `Serilog` | `4.4.0` | `HDLG file property/HdlgFileProperty.csproj` |
| `SixLabors.ImageSharp` | `3.1.12` | same |
| `PdfPig` | `0.1.15` | same |
| `System.Drawing.Common` | `10.0.11` | same |
| `Microsoft.AspNetCore.TestHost` | `10.0.11` | `HDLG.Tests/HDLG.Tests.csproj` |
| `Microsoft.NET.Test.Sdk` | `18.8.1` | same |

Commit message if done: `docs: sync AGENTS.md NuGet versions with project files`.

If not authorized: skip and leave a note in the session that Task 7 is still open.

**Skipped pending owner authorization.** `AGENTS.md` / `ANTIGRAVITY.md` were not authorized; NuGet table not synced.

---

### Task 8: Full verification

- [x] **Step 1: Build Debug and run the full suite**

```powershell
dotnet build HDLG.sln
dotnet test HDLG.sln
```

Expected: 0 error, 0 warning, 0 failed.

- [x] **Step 2: Confirm the clone is gone**

In `HDLG winforms/MainWindow.cs` there must be exactly one of each:
- `RunExportAsync`
- `PerformDirectoryBrowseAsync`
- `SetExportControlsEnabled`

There must be **zero** of:
- `PerformDirectoryBrowseXmlAsync`
- `PerformDirectoryBrowseHtmlAsync`
- `PerformDirectoryBrowseJsonAsync`

The three click handlers must each be a single `await RunExportAsync(...)`.

- [x] **Step 3: Mark this plan complete**

Check every `- [ ]` in this file to `- [x]`. Update `.grok/CURRENT-PLAN.md` status to `complete`.

---

## Self-review

**Spec coverage:** Review issues 1, 2, 3, 5, 6, 7 each map to a task. Issue 4 is explicitly out of scope.

**Placeholders:** None. Method bodies, filters, and commit titles are written out.

**Type consistency:** `Func<DirectoryBrowser, HdlgDirectory, string, Task>` is the save delegate in Tasks 2 and 3. Public export methods stay `SaveAsXMLAsync` / `SaveAsHTMLAsync` / `SaveAsJSONAsync`.

**Reload phrase for the next session:** `Continue the plan in Documentation/plans/2026-08-12-address-json-export-review.md`
