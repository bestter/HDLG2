/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using HdlgFileProperty;
using Serilog;
using Serilog.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace HDLG_winforms
{
	public partial class MainWindow : Form
	{
		#region PropertyGetter
		public ImagePropertyGetter ImagePropertyGetter;

		public WordPropertyGetter WordPropertyGetter;

		public ExcelPropertyGetter ExcelPropertyGetter;

		public PdfPropertyGetter PdfPropertyGetter;

		public Mp3PropertyGetter Mp3PropertyGetter;

		private ILogger Logger;
		#endregion

		/// <summary>
		/// Property browser
		/// </summary>
		private readonly FilePropertyBrowser propertyBrowser;

		public MainWindow (ImagePropertyGetter imagePropertyGetter, WordPropertyGetter wordPropertyGetter, ExcelPropertyGetter excelPropertyGetter, PdfPropertyGetter pdfPropertyGetter, Mp3PropertyGetter mp3PropertyGetter, ILogger logger)
		{
			InitializeComponent( );
			ImagePropertyGetter = imagePropertyGetter;
			WordPropertyGetter = wordPropertyGetter;
			ExcelPropertyGetter = excelPropertyGetter;
			PdfPropertyGetter = pdfPropertyGetter;
			Mp3PropertyGetter = mp3PropertyGetter;
			Logger = logger;
			propertyBrowser = new( logger, imagePropertyGetter, wordPropertyGetter, excelPropertyGetter, pdfPropertyGetter, mp3PropertyGetter );
		}

		private string? selectedDirectory;

		private void BtnChooseFolder_Click (object sender, EventArgs e)
		{
			var result = selectedDirectoryBrowser.ShowDialog( );
			if (result == DialogResult.OK)
			{
				selectedDirectoryLabel.Text = selectedDirectoryBrowser.SelectedPath;
				selectedDirectory = selectedDirectoryBrowser.SelectedPath;
			}
			else
			{
				selectedDirectoryLabel.Text = string.Empty;
				selectedDirectory = null;
			}
		}

		private void MainWindow_Load (object sender, EventArgs e)
		{
			AssemblyName an = typeof( MainWindow ).Assembly.GetName( );
			Text = $"{an.Name} {an.Version?.ToString( )}";
			selectedDirectory = null;
			selectedDirectoryLabel.Text = string.Empty;
			toolStripStatusLabelBrowseTime.Text = string.Empty;
			toolStripStatusLabelSaveTime.Text = string.Empty;
			toolStripStatusLabelTotalTime.Text = string.Empty;
			toolStripStatusLabelException.Text = string.Empty;
			saveContentFileDialog.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
			saveFileDialogHtml.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		}

		private async void BtnStart_Click (object sender, EventArgs e)
		{
			try
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

				if (!string.IsNullOrWhiteSpace( selectedDirectory ))
				{
					DirectoryInfo di = new( selectedDirectory );
					saveContentFileDialog.FileName = $"{di.Name}.xml";
					var result = saveContentFileDialog.ShowDialog( );
					if (result == DialogResult.OK)
					{
						btnStartXml.Enabled = false;
						btnStartHtml.Enabled = false;
						if (btnStartUi != null) btnStartUi.Enabled = false;
						UseWaitCursor = true;
						Logger.Information( "Start browse with {SelectedDirectory}", selectedDirectory );

						// Use an indeterminate progress bar if supported, or leave it at 0
						progressBar1.Style = ProgressBarStyle.Marquee;

						// Exécuter le travail dans un thread de fond sans bloquer l'UI
						var perf = await Task.Run(() => PerformDirectoryBrowseXmlAsync(selectedDirectory, saveContentFileDialog.FileName)).ConfigureAwait(true);

						progressBar1.Style = ProgressBarStyle.Blocks;
						progressBar1.Value = 100;

						// Mettre à jour l'UI après le traitement
						UpdateUIWithPerformance(perf);
						OpenWithDefaultProgram(saveContentFileDialog.FileName);
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Access denied in {MethodName}", nameof( BtnStart_Click ) );
			}
			catch (System.Security.SecurityException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Security exception in {MethodName}", nameof( BtnStart_Click ) );
			}
			catch (IOException ex)
			{
				toolStripStatusLabelException.Text = "An IO error occurred";
				Logger.Error( ex, "IO Error in {MethodName}", nameof( BtnStart_Click ) );
			}
			catch (Exception ex)
			{
				toolStripStatusLabelException.Text = "An error occurred";
				Logger.Error( ex, "Error in {MethodName}", nameof( BtnStart_Click ) );
				throw;
			}
			finally
			{
				btnStartXml.Enabled = true;
				btnStartHtml.Enabled = true;
				if (btnStartUi != null) btnStartUi.Enabled = true;
				UseWaitCursor = false;
			}
		}

		private void UpdateUIWithPerformance(PerformanceCount perf)
		{
			if (perf.TotalTime != TimeSpan.MinValue)
			{
				toolStripStatusLabelBrowseTime.Text = $"Browse: {perf.BrowseTime.ToString( "G", CultureInfo.CurrentCulture )}";
				toolStripStatusLabelSaveTime.Text = $"Save: {perf.SaveTime.ToString( "G", CultureInfo.CurrentCulture )}";
				toolStripStatusLabelTotalTime.Text = $"Total: {perf.TotalTime.ToString( "G", CultureInfo.CurrentCulture )}";
			}
		}

		private async Task<PerformanceCount> PerformDirectoryBrowseXmlAsync(string selecteDirectory, string saveFilePath)
		{
			Logger.Debug( "{MethodName} started at {StartTime:T}", nameof( PerformDirectoryBrowseXmlAsync ), DateTime.Now );
			if (!string.IsNullOrWhiteSpace( selecteDirectory ))
			{
				Logger.Information( "{SelectedDirectory}", selecteDirectory );
				HdlgDirectory directory = new( selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger );
				Stopwatch stopwatch = Stopwatch.StartNew( );

				Logger.Debug( "Ready to start {MethodName}", nameof( directory.Browse ) );
				directory.Browse( propertyBrowser );
				Logger.Debug( "{MethodName} of directory {DirectoryName} done", nameof( directory.Browse ), directory.Name );
				TimeSpan browseTime = stopwatch.Elapsed;
				propertyBrowser.LogGetterStatistics( );

				DirectoryBrowser db = new( Logger );
				Logger.Debug( "Ready to start {MethodName}", nameof( DirectoryBrowser.SaveAsXMLAsync ) );

				await db.SaveAsXMLAsync( saveFilePath, directory ).ConfigureAwait( false );

				Logger.Debug( "{MethodName} done", nameof( DirectoryBrowser.SaveAsXMLAsync ) );
				stopwatch.Stop( );

				TimeSpan saveTime = stopwatch.Elapsed - browseTime;

				var result = new PerformanceCount( ) { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };

				Logger.Information( "Done at {EndTime:T}", DateTime.Now );
				return result;
			}
			else
			{
				Logger.Information( "No {SelectedDirectoryParamName}", nameof( selecteDirectory ) );
				return PerformanceCount.Empty;
			}
		}

		/// <summary>
		/// Dangerous file extensions that must not be opened directly to prevent process injection
		/// </summary>
		private static readonly HashSet<string> DangerousExtensions = new( StringComparer.OrdinalIgnoreCase )
		{
			".exe", ".bat", ".cmd", ".ps1", ".vbs", ".js", ".wsf", ".scr", ".com", ".msi", ".pif", ".hta", ".cpl",
			".jar", ".reg", ".lnk", ".msc", ".vbe", ".jse", ".scf", ".ws", ".wsh",
			".iso", ".img", ".vhd", ".vhdx", ".url", ".appref-ms", ".theme", ".themepack",
			".application", ".settingcontent-ms", ".library-ms", ".appx", ".msix", ".msixbundle", ".msp", ".chm"
		};

		/// <summary>
		/// Safe file extensions that can be opened directly without prompting the user.
		/// </summary>
		private static readonly HashSet<string> SafeExtensions = new( StringComparer.OrdinalIgnoreCase )
		{
			".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".rtf", ".csv",
			".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".svg", ".ico",
			".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac",
			".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".flv",
			".html", ".htm", ".xml", ".json", ".yaml", ".yml", ".md", ".log",
			".zip", ".rar", ".7z", ".tar", ".gz", ".bz2",
			".cs", ".cpp", ".h", ".c", ".java", ".py", ".ts", ".jsx", ".tsx", ".css"
		};

		/// <summary>
		/// Open file with the default program
		/// </summary>
		/// <param name="path"></param>
		/// <remarks>https://stackoverflow.com/a/54275102/910741</remarks>
		/// <exception cref="ArgumentException">Thrown when path is null or whitespace</exception>
		/// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
		/// <exception cref="InvalidOperationException">Thrown when the file has a dangerous extension</exception>
		public static void OpenWithDefaultProgram(string path)
		{
			OpenWithDefaultProgram(path, p =>
			{
				using Process fileopener = new( );
				fileopener.StartInfo = new ProcessStartInfo( p )
				{
					UseShellExecute = true,
					WorkingDirectory = Environment.GetFolderPath( Environment.SpecialFolder.System )
				};
				fileopener.Start( );
			}, ext => {
				DialogResult res = MessageBox.Show( $"The file extension '{ext}' is not in the safe allowlist.\n\nAre you sure you want to open this file?", "Security Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning );
				return res == DialogResult.Yes;
			});
		}

		public static void OpenWithDefaultProgram(string path, Action<string> processStarter, Func<string, bool>? promptUnknownExtension = null)
		{
			ArgumentNullException.ThrowIfNull( processStarter );
			ArgumentException.ThrowIfNullOrWhiteSpace( path );

			if (!System.IO.File.Exists( path ))
			{
				throw new FileNotFoundException( "The specified file was not found.", path );
			}

			string fullPath = System.IO.Path.GetFullPath( path );

			string extension = System.IO.Path.GetExtension( fullPath.TrimEnd(' ', '.') );
			if (DangerousExtensions.Contains( extension ))
			{
				throw new InvalidOperationException( $"Opening files with extension '{extension}' is not allowed for security reasons." );
			}

			if (!SafeExtensions.Contains( extension ))
			{
				if (promptUnknownExtension == null)
				{
					throw new InvalidOperationException( $"Opening files with unknown extension '{extension}' is not allowed for security reasons." );
				}
				else if (!promptUnknownExtension(extension))
				{
					return;
				}
			}

			processStarter( fullPath );
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				components?.Dispose( );

				(Logger as IDisposable)?.Dispose( );
			}

			base.Dispose( disposing );
		}

		private async void BtnStartHtml_Click (object sender, EventArgs e)
		{
			try
			{
				progressBar1.Value = 0;
				toolStripStatusLabelBrowseTime.Text = string.Empty;
				toolStripStatusLabelSaveTime.Text = string.Empty;
				toolStripStatusLabelTotalTime.Text = string.Empty;
				toolStripStatusLabelException.Text = string.Empty;

				if (!string.IsNullOrWhiteSpace( selectedDirectory ))
				{
					DirectoryInfo di = new( selectedDirectory );
					saveFileDialogHtml.FileName = $"{di.Name}.html";
					var result = saveFileDialogHtml.ShowDialog( );
					if (result == DialogResult.OK)
					{
						btnStartXml.Enabled = false;
						btnStartHtml.Enabled = false;
						if (btnStartUi != null) btnStartUi.Enabled = false;
						UseWaitCursor = true;
						Logger.Information( "Start browse with {SelectedDirectory}", selectedDirectory );

						progressBar1.Style = ProgressBarStyle.Marquee;

						var perf = await Task.Run(() => PerformDirectoryBrowseHtmlAsync(selectedDirectory, saveFileDialogHtml.FileName)).ConfigureAwait(true);

						progressBar1.Style = ProgressBarStyle.Blocks;
						progressBar1.Value = 100;

						UpdateUIWithPerformance(perf);
						OpenWithDefaultProgram(saveFileDialogHtml.FileName);
					}
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Access denied in {MethodName}", nameof( BtnStartHtml_Click ) );
			}
			catch (System.Security.SecurityException ex)
			{
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Security exception in {MethodName}", nameof( BtnStartHtml_Click ) );
			}
			catch (IOException ex)
			{
				toolStripStatusLabelException.Text = "An IO error occurred";
				Logger.Error( ex, "IO Error in {MethodName}", nameof( BtnStartHtml_Click ) );
			}
			catch (Exception ex)
			{
				toolStripStatusLabelException.Text = "An error occurred";
				Logger.Error( ex, "Error in {MethodName}", nameof( BtnStartHtml_Click ) );
				throw;
			}
			finally
			{
				btnStartXml.Enabled = true;
				btnStartHtml.Enabled = true;
				if (btnStartUi != null) btnStartUi.Enabled = true;
				UseWaitCursor = false;
			}
		}

		private async Task<PerformanceCount> PerformDirectoryBrowseHtmlAsync(string selecteDirectory, string saveFilePath)
		{
			Debug.Write( $"{nameof( PerformDirectoryBrowseHtmlAsync )} started at {DateTime.Now:T}" );
			if (!string.IsNullOrWhiteSpace( selecteDirectory ))
			{
				Logger.Information( "{SelectedDirectory}", selecteDirectory );
				HdlgDirectory directory = new( selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger );
				Stopwatch stopwatch = Stopwatch.StartNew( );
				Logger.Debug( "Ready to start {MethodName}", nameof( directory.Browse ) );
				directory.Browse( propertyBrowser );
				Logger.Debug( "{MethodName} of directory {DirectoryName} done", nameof( directory.Browse ), directory.Name );
				TimeSpan browseTime = stopwatch.Elapsed;
				propertyBrowser.LogGetterStatistics( );

				DirectoryBrowser db = new( Logger );
				Logger.Debug( "Ready to start {MethodName}", nameof( DirectoryBrowser.SaveAsHTMLAsync ) );

				await db.SaveAsHTMLAsync( saveFilePath, directory ).ConfigureAwait( false );

				Logger.Debug( "{MethodName} done", nameof( DirectoryBrowser.SaveAsHTMLAsync ) );
				stopwatch.Stop( );
				TimeSpan saveTime = stopwatch.Elapsed - browseTime;

				var result = new PerformanceCount( ) { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
				Logger.Information( "Done at {EndTime:T}", DateTime.Now );
				return result;
			}
			else
			{
				Logger.Information( "No {SelectedDirectoryParamName}", nameof( selecteDirectory ) );
				return PerformanceCount.Empty;
			}
		}

		private void SaveFileDialogHtml_FileOk (object sender, CancelEventArgs e)
		{

		}

		private void BtnStartUi_Click (object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace( selectedDirectory ))
			{
				MessageBox.Show( this, "Please choose a directory first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning );
				return;
			}

			try
			{
				UseWaitCursor = true;
				using BrowserForm form = new BrowserForm( selectedDirectory, propertyBrowser, Logger );
				UseWaitCursor = false;
				form.ShowDialog( this );
			}
			catch (UnauthorizedAccessException ex)
			{
				UseWaitCursor = false;
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Access denied opening UI Explorer" );
				MessageBox.Show( this, "Error: Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			catch (System.Security.SecurityException ex)
			{
				UseWaitCursor = false;
				toolStripStatusLabelException.Text = "Access Denied";
				Logger.Warning( ex, "Security exception opening UI Explorer" );
				MessageBox.Show( this, "Error: Access Denied", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			catch (IOException ex)
			{
				UseWaitCursor = false;
				toolStripStatusLabelException.Text = "An IO error occurred";
				Logger.Error( ex, "IO Error opening UI Explorer" );
				MessageBox.Show( this, "An IO error occurred", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			catch (Exception ex)
			{
				UseWaitCursor = false;
				toolStripStatusLabelException.Text = "An error occurred";
				Logger.Error( ex, "Error opening UI Explorer" );
				throw;
			}
		}

		private void CreditToolStripMenuItem_Click (object sender, EventArgs e)
		{
			using Credit credit = new Credit( );
			credit.ShowDialog( this );
		}
	}
}
