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
            selectedDirectory = null;
            selectedDirectoryLabel.Text = string.Empty;
            labelBrowseTime.Text = string.Empty;
            labelSaveTime.Text = string.Empty;
            saveContentFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(selectedDirectory))
            {
                labelBrowseTime.Text = string.Empty;
                labelSaveTime.Text = string.Empty;

                var result = saveContentFileDialog.ShowDialog();
                if (result == DialogResult.OK) { 
                    
                    Directory directory = new(selectedDirectory);
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    directory.Browse();
                    TimeSpan browseTime= stopwatch.Elapsed;

                    await DirectoryBrowser.SaveAsXMLAsync(saveContentFileDialog.FileName, directory);                    
                    stopwatch.Stop();
                    TimeSpan saveTime = stopwatch.Elapsed - browseTime;

                    labelBrowseTime.Text = browseTime.ToString("G", CultureInfo.CurrentUICulture);
                    labelSaveTime.Text = saveTime.ToString("G", CultureInfo.CurrentUICulture);
                }
            }
        }
    }
}
