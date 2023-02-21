using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using HdlgFileProperty;
using Serilog;
using Serilog.Core;

namespace HDLG_winforms
{
    public partial class MainWindow : Form
    {
        public ImagePropertyGetter ImagePropertyGetter;

        public WordPropertyGetter WordPropertyGetter;

        public ExcelPropertyGetter ExcelPropertyGetter;

        private readonly FilePropertyBrowser propertyBrowser;
        readonly Logger log = new LoggerConfiguration()
    .WriteTo.File(@"logs\log.txt", formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day).MinimumLevel.Debug()
    .CreateLogger();


        public MainWindow(ImagePropertyGetter imagePropertyGetter, WordPropertyGetter wordPropertyGetter, ExcelPropertyGetter excelPropertyGetter)
        {
            InitializeComponent();
            ImagePropertyGetter = imagePropertyGetter;
            WordPropertyGetter = wordPropertyGetter;
            ExcelPropertyGetter = excelPropertyGetter;
            propertyBrowser = new(imagePropertyGetter, wordPropertyGetter, excelPropertyGetter);
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
            labelBrowseTime.Text = string.Empty;
            labelSaveTime.Text = string.Empty;
            labelTotalTime.Text = string.Empty;
            labelException.Text = string.Empty;
            saveContentFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorkerDirectoryBrowse.IsBusy)
                {
                    progressBar1.Value = 0;
                    labelBrowseTime.Text = string.Empty;
                    labelSaveTime.Text = string.Empty;
                    labelTotalTime.Text = string.Empty;
                    labelException.Text = string.Empty;

                    if (!string.IsNullOrWhiteSpace(selectedDirectory))
                    {
                        var result = saveContentFileDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            btnStart.Enabled = false;
                            Cursor.Current = Cursors.WaitCursor;
                            log.Information($"Start browse with {selectedDirectory}");
                            backgroundWorkerDirectoryBrowse.RunWorkerAsync(selectedDirectory);
                            while (backgroundWorkerDirectoryBrowse.IsBusy)
                            {
                                progressBar1.Increment(1);
                                // Keep UI messages moving, so the form remains 
                                // responsive during the asynchronous operation.
                                Application.DoEvents();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                labelException.Text = ex.Message;
                log.Fatal(ex, $"Error in {nameof(BtnStart_Click)}");
            }
        }

        private void BackgroundWorkerDirectoryBrowse_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.Write($"{nameof(BackgroundWorkerDirectoryBrowse_DoWork)} started at {DateTime.Now.ToLongTimeString()}");
            string? selecteDirectory = e.Argument as string;
            if (!string.IsNullOrWhiteSpace(selecteDirectory))
            {
                log.Information(selecteDirectory);
                Directory directory = new(selecteDirectory, true, log);
                Stopwatch stopwatch = Stopwatch.StartNew();
                log.Debug($"Ready to start {nameof(directory.Browse)}");
                directory.Browse(propertyBrowser);
                log.Debug($"{nameof(directory.Browse)} of directory {directory.Name} done");
                TimeSpan browseTime = stopwatch.Elapsed;

                DirectoryBrowser db = new(log);
                log.Debug($"Ready to start {nameof(DirectoryBrowser.SaveAsXMLAsync)}");
                using (CancellationTokenSource source = new())
                {
                    db.SaveAsXMLAsync(saveContentFileDialog.FileName, directory, source.Token).Wait();
                }
                log.Debug($"{nameof(DirectoryBrowser.SaveAsXMLAsync)} done");
                stopwatch.Stop();
                TimeSpan saveTime = stopwatch.Elapsed - browseTime;

                e.Result = new PerformanceCount() { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
                log.Information($"Done at {DateTime.Now.ToLongTimeString()}");
            }
            else
            {
                log.Information($"No {nameof(selecteDirectory)}");
                e.Result = new PerformanceCount() { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
                e.Cancel = true;
            }
        }

        private void BackgroundWorkerDirectoryBrowse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write($"Completed at {DateTime.Now.ToLongTimeString()}");
            Cursor.Current = Cursors.Default;
            btnStart.Enabled = true;
            PerformanceCount? perf = e.Result as PerformanceCount?;
            if (perf != null)
            {
                labelBrowseTime.Text = perf.Value.BrowseTime.ToString("G", CultureInfo.CurrentCulture);
                labelSaveTime.Text = perf.Value.SaveTime.ToString("G", CultureInfo.CurrentCulture);
                labelTotalTime.Text = perf.Value.TotalTime.ToString("G", CultureInfo.CurrentCulture);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                log.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
