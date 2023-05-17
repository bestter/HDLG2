namespace HDLG_winforms
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gbDirectory = new GroupBox();
            btnChooseFolder = new Button();
            selectedDirectoryLabel = new Label();
            selectedDirectoryBrowser = new FolderBrowserDialog();
            btnStartXml = new Button();
            saveContentFileDialog = new SaveFileDialog();
            labelBrowseTime = new Label();
            labelSaveTime = new Label();
            backgroundWorkerDirectoryBrowseXml = new System.ComponentModel.BackgroundWorker();
            labelTotalTime = new Label();
            progressBar1 = new ProgressBar();
            labelException = new Label();
            tableLayoutPanelStart = new TableLayoutPanel();
            btnStartHtml = new Button();
            backgroundWorkerDirectoryBrowseHtml = new System.ComponentModel.BackgroundWorker();
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialogHtml = new SaveFileDialog();
            cbBrowseSubDirectory = new CheckBox();
            gbDirectory.SuspendLayout();
            tableLayoutPanelStart.SuspendLayout();
            SuspendLayout();
            // 
            // gbDirectory
            // 
            gbDirectory.Controls.Add(cbBrowseSubDirectory);
            gbDirectory.Controls.Add(btnChooseFolder);
            gbDirectory.Controls.Add(selectedDirectoryLabel);
            gbDirectory.Location = new Point(95, 176);
            gbDirectory.Name = "gbDirectory";
            gbDirectory.Size = new Size(610, 99);
            gbDirectory.TabIndex = 0;
            gbDirectory.TabStop = false;
            gbDirectory.Text = "Directory";
            // 
            // btnChooseFolder
            // 
            btnChooseFolder.Location = new Point(249, 20);
            btnChooseFolder.Name = "btnChooseFolder";
            btnChooseFolder.Size = new Size(112, 23);
            btnChooseFolder.TabIndex = 0;
            btnChooseFolder.Text = "Choose folder";
            btnChooseFolder.UseVisualStyleBackColor = true;
            btnChooseFolder.Click += BtnChooseFolder_Click;
            // 
            // selectedDirectoryLabel
            // 
            selectedDirectoryLabel.AutoSize = true;
            selectedDirectoryLabel.Location = new Point(255, 63);
            selectedDirectoryLabel.Name = "selectedDirectoryLabel";
            selectedDirectoryLabel.Size = new Size(101, 15);
            selectedDirectoryLabel.TabIndex = 1;
            selectedDirectoryLabel.Text = "Selected directory";
            // 
            // btnStartXml
            // 
            btnStartXml.Location = new Point(3, 3);
            btnStartXml.Name = "btnStartXml";
            btnStartXml.Size = new Size(112, 23);
            btnStartXml.TabIndex = 1;
            btnStartXml.Text = "XML";
            btnStartXml.UseVisualStyleBackColor = true;
            btnStartXml.Click += BtnStart_Click;
            // 
            // saveContentFileDialog
            // 
            saveContentFileDialog.DefaultExt = "xml";
            saveContentFileDialog.Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*";
            // 
            // labelBrowseTime
            // 
            labelBrowseTime.AutoSize = true;
            labelBrowseTime.Location = new Point(682, 411);
            labelBrowseTime.Name = "labelBrowseTime";
            labelBrowseTime.Size = new Size(71, 15);
            labelBrowseTime.TabIndex = 2;
            labelBrowseTime.Text = "BrowseTime";
            // 
            // labelSaveTime
            // 
            labelSaveTime.AutoSize = true;
            labelSaveTime.Location = new Point(682, 426);
            labelSaveTime.Name = "labelSaveTime";
            labelSaveTime.Size = new Size(57, 15);
            labelSaveTime.TabIndex = 3;
            labelSaveTime.Text = "SaveTime";
            // 
            // backgroundWorkerDirectoryBrowseXml
            // 
            backgroundWorkerDirectoryBrowseXml.DoWork += BackgroundWorkerDirectoryBrowse_DoWork;
            backgroundWorkerDirectoryBrowseXml.RunWorkerCompleted += BackgroundWorkerDirectoryBrowse_RunWorkerCompleted;
            // 
            // labelTotalTime
            // 
            labelTotalTime.AutoSize = true;
            labelTotalTime.Location = new Point(682, 444);
            labelTotalTime.Name = "labelTotalTime";
            labelTotalTime.Size = new Size(59, 15);
            labelTotalTime.TabIndex = 4;
            labelTotalTime.Text = "Total time";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(95, 322);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(610, 23);
            progressBar1.TabIndex = 2;
            // 
            // labelException
            // 
            labelException.AutoSize = true;
            labelException.Location = new Point(12, 369);
            labelException.Name = "labelException";
            labelException.Size = new Size(59, 15);
            labelException.TabIndex = 5;
            labelException.Text = "Exception";
            // 
            // tableLayoutPanelStart
            // 
            tableLayoutPanelStart.ColumnCount = 2;
            tableLayoutPanelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelStart.Controls.Add(btnStartHtml, 1, 0);
            tableLayoutPanelStart.Controls.Add(btnStartXml, 0, 0);
            tableLayoutPanelStart.Location = new Point(271, 281);
            tableLayoutPanelStart.Name = "tableLayoutPanelStart";
            tableLayoutPanelStart.RowCount = 1;
            tableLayoutPanelStart.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelStart.Size = new Size(259, 30);
            tableLayoutPanelStart.TabIndex = 6;
            // 
            // btnStartHtml
            // 
            btnStartHtml.Location = new Point(132, 3);
            btnStartHtml.Name = "btnStartHtml";
            btnStartHtml.Size = new Size(112, 23);
            btnStartHtml.TabIndex = 7;
            btnStartHtml.Text = "HTML";
            btnStartHtml.UseVisualStyleBackColor = true;
            btnStartHtml.Click += BtnStartHtml_Click;
            // 
            // backgroundWorkerDirectoryBrowseHtml
            // 
            backgroundWorkerDirectoryBrowseHtml.DoWork += BackgroundWorkerDirectoryBrowseHtml_DoWork;
            backgroundWorkerDirectoryBrowseHtml.RunWorkerCompleted += BackgroundWorkerDirectoryBrowseHtml_RunWorkerCompleted;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*";
            // 
            // saveFileDialogHtml
            // 
            saveFileDialogHtml.Filter = "Fichiers HTML|*.html|Tous les fichiers|*.*";
            saveFileDialogHtml.FileOk += SaveFileDialogHtml_FileOk;
            // 
            // cbBrowseSubDirectory
            // 
            cbBrowseSubDirectory.AutoSize = true;
            cbBrowseSubDirectory.Checked = true;
            cbBrowseSubDirectory.CheckState = CheckState.Checked;
            cbBrowseSubDirectory.Location = new Point(398, 22);
            cbBrowseSubDirectory.Name = "cbBrowseSubDirectory";
            cbBrowseSubDirectory.Size = new Size(138, 19);
            cbBrowseSubDirectory.TabIndex = 2;
            cbBrowseSubDirectory.Text = "Browse sub-directory";
            cbBrowseSubDirectory.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 472);
            Controls.Add(tableLayoutPanelStart);
            Controls.Add(labelException);
            Controls.Add(progressBar1);
            Controls.Add(labelTotalTime);
            Controls.Add(labelSaveTime);
            Controls.Add(labelBrowseTime);
            Controls.Add(gbDirectory);
            Name = "MainWindow";
            Text = "MainWindow";
            Load += MainWindow_Load;
            gbDirectory.ResumeLayout(false);
            gbDirectory.PerformLayout();
            tableLayoutPanelStart.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox gbDirectory;
        private Button btnChooseFolder;
        private FolderBrowserDialog selectedDirectoryBrowser;
        private Label selectedDirectoryLabel;
        private Button btnStartXml;
        private SaveFileDialog saveContentFileDialog;
        private Label labelBrowseTime;
        private Label labelSaveTime;
        private System.ComponentModel.BackgroundWorker backgroundWorkerDirectoryBrowseXml;
        private Label labelTotalTime;
        private ProgressBar progressBar1;
        private Label labelException;
        private TableLayoutPanel tableLayoutPanelStart;
        private Button btnStartHtml;
        private System.ComponentModel.BackgroundWorker backgroundWorkerDirectoryBrowseHtml;
        private SaveFileDialog saveFileDialog1;
        private SaveFileDialog saveFileDialogHtml;
        private CheckBox cbBrowseSubDirectory;
    }
}