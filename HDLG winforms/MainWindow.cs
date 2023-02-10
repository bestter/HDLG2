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

namespace HDLG_winforms
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
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
            saveContentFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!backgroundWorkerDirectoryBrowse.IsBusy)
            {
                progressBar1.Value = 0;
                labelBrowseTime.Text = string.Empty;
                labelSaveTime.Text = string.Empty;
                labelTotalTime.Text = string.Empty;

                if (!string.IsNullOrWhiteSpace(selectedDirectory))
                {
                    var result = saveContentFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        backgroundWorkerDirectoryBrowse.RunWorkerAsync(selectedDirectory);
                        while (this.backgroundWorkerDirectoryBrowse.IsBusy)
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

        private void BackgroundWorkerDirectoryBrowse_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.Write($"Started at {DateTime.Now.ToLongTimeString()}");
            string? selecteDirectory = e.Argument as string;
            if (!string.IsNullOrWhiteSpace(selecteDirectory))
            {
                Directory directory = new(selecteDirectory);
                Stopwatch stopwatch = Stopwatch.StartNew();
                directory.Browse();
                TimeSpan browseTime = stopwatch.Elapsed;
                
                using (CancellationTokenSource source = new())
                {
                    DirectoryBrowser.SaveAsXMLAsync(saveContentFileDialog.FileName, directory, source.Token).Wait();
                }
                stopwatch.Stop();
                TimeSpan saveTime = stopwatch.Elapsed - browseTime;

                e.Result = new PerformanceCount() { BrowseTime = browseTime, SaveTime = saveTime, TotalTime = stopwatch.Elapsed };
                Debug.Write($"Done at {DateTime.Now.ToLongTimeString()}");
            }
            else
            {
                e.Result = new PerformanceCount() { BrowseTime = TimeSpan.MinValue, SaveTime = TimeSpan.MinValue, TotalTime = TimeSpan.MinValue };
                e.Cancel = true;
            }
        }

        private void BackgroundWorkerDirectoryBrowse_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Write($"Completed at {DateTime.Now.ToLongTimeString()}");
            Cursor.Current = Cursors.Default;
            PerformanceCount? perf = e.Result as PerformanceCount?;
            if (perf != null)
            {
                labelBrowseTime.Text = perf.Value.BrowseTime.ToString("G", CultureInfo.CurrentCulture);
                labelSaveTime.Text = perf.Value.SaveTime.ToString("G", CultureInfo.CurrentCulture);
                labelTotalTime.Text = perf.Value.TotalTime.ToString("G", CultureInfo.CurrentCulture);
            }
        }
    }

}
