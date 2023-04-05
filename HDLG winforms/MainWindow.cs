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
        #endregion

        /// <summary>
        /// Property browser
        /// </summary>
        private readonly FilePropertyBrowser propertyBrowser;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger log = new LoggerConfiguration()
    .WriteTo.File(@"logs\log.txt", formatProvider: CultureInfo.CurrentCulture, rollingInterval: RollingInterval.Day).MinimumLevel.Debug()
    .CreateLogger();


        public MainWindow(ImagePropertyGetter imagePropertyGetter, WordPropertyGetter wordPropertyGetter, ExcelPropertyGetter excelPropertyGetter, PdfPropertyGetter pdfPropertyGetter)
        {
            InitializeComponent();
            ImagePropertyGetter = imagePropertyGetter;
            WordPropertyGetter = wordPropertyGetter;
            ExcelPropertyGetter = excelPropertyGetter;
            PdfPropertyGetter = pdfPropertyGetter;
            propertyBrowser = new(imagePropertyGetter, wordPropertyGetter, excelPropertyGetter, pdfPropertyGetter);
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
            saveFileDialogHtml.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorkerDirectoryBrowseXml.IsBusy)
                {
                    progressBar1.Value = 0;
                    labelBrowseTime.Text = string.Empty;
                    labelSaveTime.Text = string.Empty;
                    labelTotalTime.Text = string.Empty;
                    labelException.Text = string.Empty;

                    if (!string.IsNullOrWhiteSpace(selectedDirectory))
                    {
                        DirectoryInfo di = new(selectedDirectory);
                        saveContentFileDialog.FileName = $"{di.Name}.xml";
                        var result = saveContentFileDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            btnStartXml.Enabled = false;
                            btnStartHtml.Enabled = false;
                            UseWaitCursor = true;
                            log.Information($"Start browse with {selectedDirectory}");
                            backgroundWorkerDirectoryBrowseXml.RunWorkerAsync(selectedDirectory);
                            while (backgroundWorkerDirectoryBrowseXml.IsBusy)
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

                db.SaveAsXMLAsync(saveContentFileDialog.FileName, directory).Wait();

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
            UseWaitCursor = false;
            btnStartXml.Enabled = true;
            PerformanceCount? perf = e.Result as PerformanceCount?;
            if (perf != null)
            {
                labelBrowseTime.Text = perf.Value.BrowseTime.ToString("G", CultureInfo.CurrentCulture);
                labelSaveTime.Text = perf.Value.SaveTime.ToString("G", CultureInfo.CurrentCulture);
                labelTotalTime.Text = perf.Value.TotalTime.ToString("G", CultureInfo.CurrentCulture);
            }
            if (saveContentFileDialog.FileName != null)
            {
                OpenWithDefaultProgram(saveContentFileDialog.FileName);
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

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
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

                log.Dispose();
            }

            base.Dispose(disposing);
        }

        private void btnStartHtml_Click(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorkerDirectoryBrowseHtml.IsBusy)
                {
                    progressBar1.Value = 0;
                    labelBrowseTime.Text = string.Empty;
                    labelSaveTime.Text = string.Empty;
                    labelTotalTime.Text = string.Empty;
                    labelException.Text = string.Empty;

                    if (!string.IsNullOrWhiteSpace(selectedDirectory))
                    {
                        DirectoryInfo di = new(selectedDirectory);
                        saveFileDialogHtml.FileName = $"{di.Name}.html";
                        var result = saveFileDialogHtml.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            btnStartXml.Enabled = false;
                            btnStartHtml.Enabled = false;
                            UseWaitCursor = true;
                            log.Information($"Start browse with {selectedDirectory}");
                            backgroundWorkerDirectoryBrowseHtml.RunWorkerAsync(selectedDirectory);
                            while (backgroundWorkerDirectoryBrowseHtml.IsBusy)
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

        private void backgroundWorkerDirectoryBrowseHtml_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.Write($"{nameof(backgroundWorkerDirectoryBrowseHtml_DoWork)} started at {DateTime.Now.ToLongTimeString()}");
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
                log.Debug($"Ready to start {nameof(DirectoryBrowser.SaveAsHTMLAsync)}");

                db.SaveAsHTMLAsync(saveFileDialogHtml.FileName, directory).Wait();

                log.Debug($"{nameof(DirectoryBrowser.SaveAsHTMLAsync)} done");
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

        private void backgroundWorkerDirectoryBrowseHtml_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write($"Completed at {DateTime.Now.ToLongTimeString()}");
            UseWaitCursor = false;
            btnStartXml.Enabled = true;
            btnStartHtml.Enabled = true;
            PerformanceCount? perf = e.Result as PerformanceCount?;
            if (perf != null)
            {
                labelBrowseTime.Text = perf.Value.BrowseTime.ToString("G", CultureInfo.CurrentCulture);
                labelSaveTime.Text = perf.Value.SaveTime.ToString("G", CultureInfo.CurrentCulture);
                labelTotalTime.Text = perf.Value.TotalTime.ToString("G", CultureInfo.CurrentCulture);
            }
            if (saveFileDialogHtml.FileName != null)
            {
                OpenWithDefaultProgram(saveFileDialogHtml.FileName);
            }
        }

        private void saveFileDialogHtml_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
