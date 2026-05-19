/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

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
        private Logger Logger;
        #endregion


        /// <summary>
        /// Property browser
        /// </summary>
        private readonly FilePropertyBrowser propertyBrowser;

        //	/// <summary>
        //	/// Logger
        //	/// </summary>
        //	private readonly Logger log = new LoggerConfiguration( )
        //.WriteTo.File( @"logs\Logger.txt", formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day, outputTemplate:
        //	"[{Timestamp:R} {Level:u3}] {Message:lj}{NewLine}{Exception}" ).MinimumLevel.Debug( )
        //.CreateLogger( );

        public MainWindow(ImagePropertyGetter imagePropertyGetter, WordPropertyGetter wordPropertyGetter, ExcelPropertyGetter excelPropertyGetter, PdfPropertyGetter pdfPropertyGetter, Mp3PropertyGetter mp3PropertyGetter, Logger logger)
        {
            InitializeComponent();
            Logger = logger;
            propertyBrowser = new(logger, imagePropertyGetter, wordPropertyGetter, excelPropertyGetter, pdfPropertyGetter, mp3PropertyGetter);
        }

        private string? selectedDirectory;


        private void BtnChooseFolder_Click(object sender, EventArgs e)
        {
            var result = selectedDirectoryBrowser.ShowDialog();
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

        private void MainWindow_Load(object sender, EventArgs e)
        {
            AssemblyName an = typeof(MainWindow).Assembly.GetName();
            Text = $"{an.Name} {an.Version?.ToString()}";
            selectedDirectory = null;
            selectedDirectoryLabel.Text = string.Empty;
            toolStripStatusLabelBrowseTime.Text = string.Empty;
            toolStripStatusLabelSaveTime.Text = string.Empty;
            toolStripStatusLabelTotalTime.Text = string.Empty;
            toolStripStatusLabelException.Text = string.Empty;
            //saveContentFileDiaLogger.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            saveFileDialogHtml.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
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

                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    DirectoryInfo di = new(selectedDirectory);
                    saveContentFileDialog.FileName = $"{di.Name}.xml";
                    var result = saveContentFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        btnStartXml.Enabled = false;
                        btnStartHtml.Enabled = false;
                        if (btnStartUi != null) btnStartUi.Enabled = false;
                        UseWaitCursor = true;
                        Logger.Information($"Start browse with {selectedDirectory}");

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
                Logger.Fatal(ex, $"Error in {nameof(BtnStart_Click)}");
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
                toolStripStatusLabelBrowseTime.Text = $"Browse: {perf.BrowseTime.ToString("G", CultureInfo.CurrentCulture)}";
                toolStripStatusLabelSaveTime.Text = $"Save: {perf.SaveTime.ToString("G", CultureInfo.CurrentCulture)}";
                toolStripStatusLabelTotalTime.Text = $"Total: {perf.TotalTime.ToString("G", CultureInfo.CurrentCulture)}";
            }
        }

        private PerformanceCount PerformDirectoryBrowseXml(string selecteDirectory, string saveFilePath)
        {
            Logger.Debug($"{nameof(PerformDirectoryBrowseXml)} started at {DateTime.Now:T}");
            if (!string.IsNullOrWhiteSpace(selecteDirectory))
            {
                Logger.Information(selecteDirectory);
                HdlgDirectory directory = new(selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger);
                Stopwatch stopwatch = Stopwatch.StartNew();
                Logger.Debug($"Ready to start {nameof(directory.Browse)}");
                directory.Browse(propertyBrowser);
                Logger.Debug($"{nameof(directory.Browse)} of directory {directory.Name} done");
                TimeSpan browseTime = stopwatch.Elapsed;
                propertyBrowser.LogGetterStatistics();

                DirectoryBrowser db = new(Logger);
                Logger.Debug($"Ready to start {nameof(DirectoryBrowser.SaveAsXMLAsync)}");

                db.SaveAsXMLAsync(saveFilePath, directory).Wait();

                Logger.Debug($"{nameof(DirectoryBrowser.SaveAsXMLAsync)} done");
#if DEBUG
                stopwatch.Stop();

                TimeSpan saveTime = stopwatch.Elapsed - browseTime;

                var result = new PerformanceCount() { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
#else
				var result = new PerformanceCount( ) { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
#endif

                Logger.Information($"Done at {DateTime.Now:T}");
                return result;
            }
            else
            {
                Logger.Information($"No {nameof(selecteDirectory)}");
                return new PerformanceCount() { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
            }
        }

        /// <summary>
        /// Open file
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>https://stackoverflow.com/a/54275102/910741</remarks>
        public static void OpenWithDefaultProgram(string path)
        {
            using Process fileopener = new();
            fileopener.StartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };
            fileopener.Start();
        }


        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                Logger.Dispose();
            }

            base.Dispose(disposing);
        }

        private async void BtnStartHtml_Click(object sender, EventArgs e)
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

                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    DirectoryInfo di = new(selectedDirectory);
                    saveFileDialogHtml.FileName = $"{di.Name}.html";
                    var result = saveFileDialogHtml.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        btnStartXml.Enabled = false;
                        btnStartHtml.Enabled = false;
                        if (btnStartUi != null) btnStartUi.Enabled = false;
                        UseWaitCursor = true;
                        Logger.Information($"Start browse with {selectedDirectory}");

                        progressBar1.Style = ProgressBarStyle.Marquee;

                        var perf = await Task.Run(() => PerformDirectoryBrowseHtml(selectedDirectory, saveFileDialogHtml.FileName)).ConfigureAwait(true);

                        progressBar1.Style = ProgressBarStyle.Blocks;
                        progressBar1.Value = 100;

                        UpdateUIWithPerformance(perf);
                        OpenWithDefaultProgram(saveFileDialogHtml.FileName);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031
            {
                toolStripStatusLabelException.Text = ex.Message;
                Logger.Fatal(ex, $"Error in {nameof(BtnStartHtml_Click)}");
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
            Debug.Write($"{nameof(PerformDirectoryBrowseHtml)} started at {DateTime.Now:T}");
            TimeSpan saveTime = TimeSpan.Zero;
            TimeSpan browseTime = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(selecteDirectory))
            {
                Logger.Information(selecteDirectory);
                HdlgDirectory directory = new(selecteDirectory, true, cbBrowseSubDirectory.Checked, Logger);

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    Logger.Debug($"Ready to start {nameof(directory.Browse)}");
                    directory.Browse(propertyBrowser);
                    Logger.Debug($"{nameof(directory.Browse)} of directory {directory.Name} done");
                    browseTime = stopwatch.Elapsed;
                    propertyBrowser.LogGetterStatistics();

                    DirectoryBrowser db = new(Logger);
                    Logger.Debug($"Ready to start {nameof(DirectoryBrowser.SaveAsHTMLAsync)}");

                    db.SaveAsHTMLAsync(saveFileDialogHtml.FileName, directory).Wait();

                    Logger.Debug($"{nameof(DirectoryBrowser.SaveAsHTMLAsync)} done");
                    stopwatch.Stop();
                    saveTime = stopwatch.Elapsed - browseTime;

                    //e.Result = new PerformanceCount( ) { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
                    Logger.Information($"Done at {DateTime.Now.ToLongTimeString()}");
                    stopwatch.Stop();
                    return new PerformanceCount() { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
                }
                finally
                {
                    stopwatch.Stop();

                }
            }
            else
            {
                Logger.Information($"No {nameof(selecteDirectory)}");
                return new PerformanceCount() { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };

            }
        }

        private void BackgroundWorkerDirectoryBrowseHtml_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write($"Completed at {DateTime.Now.ToLongTimeString()}");
            UseWaitCursor = false;
            btnStartXml.Enabled = true;
            btnStartHtml.Enabled = true;
            PerformanceCount? perf = e.Result as PerformanceCount?;
            if (perf != null)
            {
                //labelBrowseTime.Text = perf.Value.BrowseTime.ToString( "G", CultureInfo.CurrentCulture );
                //labelSaveTime.Text = perf.Value.SaveTime.ToString( "G", CultureInfo.CurrentCulture );
                //labelTotalTime.Text = perf.Value.TotalTime.ToString( "G", CultureInfo.CurrentCulture );
            }
            if (saveFileDialogHtml.FileName != null)
            {
                OpenWithDefaultProgram(saveFileDialogHtml.FileName);
            }
        }

        private void SaveFileDialogHtml_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void BtnStartUi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedDirectory))
            {
                MessageBox.Show(this, "Please choose a directory first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UseWaitCursor = true;
                using BrowserForm form = new BrowserForm(selectedDirectory, propertyBrowser, Logger);
                UseWaitCursor = false;
                form.ShowDialog(this);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031
            {
                UseWaitCursor = false;
                toolStripStatusLabelException.Text = ex.Message;
                Logger.Fatal(ex, $"Error opening UI Explorer");
                MessageBox.Show(this, $"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using Credit credit = new Credit();
            credit.ShowDialog(this);
        }
    }
}