using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            saveContentFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(selectedDirectory))
            {
                var result = saveContentFileDialog.ShowDialog();
                if (result == DialogResult.OK) { 
                    Directory directory = new(selectedDirectory);
                    directory.Browse();

                    await DirectoryBrowser.SaveAsXMLAsync(saveContentFileDialog.FileName, directory);
                }
            }
        }
    }
}
