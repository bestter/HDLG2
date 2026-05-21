/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
using HdlgFileProperty;
using Serilog;
using Serilog.Core;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
						Logger.Information( $"Start browse with {selectedDirectory}" );

						// Use an indeterminate progress bar if supported, or leave it at 0
						progressBar1.Style = ProgressBarStyle.Marquee;

						// Exécuter le travail dans un thread de fond sans bloquer l'UI
						var perf = await Task.Run(() => PerformDirectoryBrowseXml(selectedDirectory, saveContentFileDialog.FileName)).ConfigureAwait(true);

						progressBar1.Style = ProgressBarStyle.Blocks;
						progressBar1.Value = 100;

						// Mettre à jour l'UI après le traitement
						UpdateUIWithPerformance(perf);
						OpenWithDefaultProgram(saveContentFileDialog.FileName);
					}
				}
			}
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
			catch (Exception ex)
#pragma warning restore CA1031
			{
				toolStripStatusLabelException.Text = ex.Message;
				Logger.Fatal( ex, $"Error in {nameof( BtnStart_Click )}" );
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

		private PerformanceCount PerformDirectoryBrowseXml(string selecteDirectory, string saveFilePath)
		{
			Logger.Debug( $"{nameof( PerformDirectoryBrowseXml )} started at {DateTime.Now:T}" );
			if (!string.IsNullOrWhiteSpace( selecteDirectory ))
			{
				Logger.Information( selecteDirectory );
				HdlgDirectory directory = new( selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger );
#if DEBUG
				Stopwatch stopwatch = Stopwatch.StartNew( );
#endif

				Logger.Debug( $"Ready to start {nameof( directory.Browse )}" );
				directory.Browse( propertyBrowser );
				Logger.Debug( $"{nameof( directory.Browse )} of directory {directory.Name} done" );
#if DEBUG
				TimeSpan browseTime = stopwatch.Elapsed;
#endif
				propertyBrowser.LogGetterStatistics( );

				DirectoryBrowser db = new( Logger );
				Logger.Debug( $"Ready to start {nameof( DirectoryBrowser.SaveAsXMLAsync )}" );

				db.SaveAsXMLAsync( saveFilePath, directory ).Wait( );

				Logger.Debug( $"{nameof( DirectoryBrowser.SaveAsXMLAsync )} done" );
#if DEBUG
				stopwatch.Stop( );

				TimeSpan saveTime = stopwatch.Elapsed - browseTime;

				var result = new PerformanceCount( ) { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
#else
				var result = new PerformanceCount( ) { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
#endif

				Logger.Information( $"Done at {DateTime.Now:T}" );
				return result;
			}
			else
			{
				Logger.Information( $"No {nameof( selecteDirectory )}" );
				return new PerformanceCount( ) { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
			}
		}

		/// <summary>
		/// Dangerous file extensions that must not be opened directly to prevent process injection
		/// </summary>
		private static readonly HashSet<string> DangerousExtensions = new( StringComparer.OrdinalIgnoreCase )
		{
			".exe", ".bat", ".cmd", ".ps1", ".vbs", ".js", ".wsf", ".scr", ".com", ".msi", ".pif", ".hta", ".cpl"
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
			ArgumentException.ThrowIfNullOrWhiteSpace( path );

			if (!System.IO.File.Exists( path ))
			{
				throw new FileNotFoundException( "The specified file was not found.", path );
			}

			string extension = System.IO.Path.GetExtension( path );
			if (DangerousExtensions.Contains( extension ))
			{
				throw new InvalidOperationException( $"Opening files with extension '{extension}' is not allowed for security reasons." );
			}

			using Process fileopener = new( );
			fileopener.StartInfo = new ProcessStartInfo( path )
			{
				UseShellExecute = true
			};
			fileopener.Start( );
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
						Logger.Information( $"Start browse with {selectedDirectory}" );

						progressBar1.Style = ProgressBarStyle.Marquee;

						var perf = await Task.Run(() => PerformDirectoryBrowseHtml(selectedDirectory, saveFileDialogHtml.FileName)).ConfigureAwait( true );

						progressBar1.Style = ProgressBarStyle.Blocks;
						progressBar1.Value = 100;

						UpdateUIWithPerformance(perf);
						OpenWithDefaultProgram(saveFileDialogHtml.FileName);
					}
				}
			}
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
			catch (Exception ex)
#pragma warning restore CA1031
			{
				toolStripStatusLabelException.Text = ex.Message;
				Logger.Fatal( ex, $"Error in {nameof( BtnStartHtml_Click )}" );
			}
			finally
			{
				btnStartXml.Enabled = true;
				btnStartHtml.Enabled = true;
				if (btnStartUi != null) btnStartUi.Enabled = true;
				UseWaitCursor = false;
			}
		}

		private PerformanceCount PerformDirectoryBrowseHtml(string selecteDirectory, string saveFilePath)
		{
			Debug.Write( $"{nameof( PerformDirectoryBrowseHtml )} started at {DateTime.Now:T}" );
			if (!string.IsNullOrWhiteSpace( selecteDirectory ))
			{
				Logger.Information( selecteDirectory );
				HdlgDirectory directory = new( selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger );
				Stopwatch stopwatch = Stopwatch.StartNew( );
				Logger.Debug( $"Ready to start {nameof( directory.Browse )}" );
				directory.Browse( propertyBrowser );
				Logger.Debug( $"{nameof( directory.Browse )} of directory {directory.Name} done" );
				TimeSpan browseTime = stopwatch.Elapsed;
				propertyBrowser.LogGetterStatistics( );

				DirectoryBrowser db = new( Logger );
				Logger.Debug( $"Ready to start {nameof( DirectoryBrowser.SaveAsHTMLAsync )}" );

				db.SaveAsHTMLAsync( saveFilePath, directory ).Wait( );

				Logger.Debug( $"{nameof( DirectoryBrowser.SaveAsHTMLAsync )} done" );
				stopwatch.Stop( );
				TimeSpan saveTime = stopwatch.Elapsed - browseTime;

				var result = new PerformanceCount( ) { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
				Logger.Information( $"Done at {DateTime.Now:T}" );
				return result;
			}
			else
			{
				Logger.Information( $"No {nameof( selecteDirectory )}" );
				return new PerformanceCount( ) { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
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
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
			catch (Exception ex)
#pragma warning restore CA1031
			{
				UseWaitCursor = false;
				toolStripStatusLabelException.Text = ex.Message;
				Logger.Fatal( ex, $"Error opening UI Explorer" );
				MessageBox.Show( this, $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		private void CreditToolStripMenuItem_Click (object sender, EventArgs e)
		{
			using Credit credit = new Credit( );
			credit.ShowDialog( this );
		}
	}
}
