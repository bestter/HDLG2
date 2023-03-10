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
            this.gbDirectory = new System.Windows.Forms.GroupBox();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.selectedDirectoryLabel = new System.Windows.Forms.Label();
            this.selectedDirectoryBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.btnStart = new System.Windows.Forms.Button();
            this.saveContentFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.labelBrowseTime = new System.Windows.Forms.Label();
            this.labelSaveTime = new System.Windows.Forms.Label();
            this.backgroundWorkerDirectoryBrowse = new System.ComponentModel.BackgroundWorker();
            this.labelTotalTime = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelException = new System.Windows.Forms.Label();
            this.gbDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDirectory
            // 
            this.gbDirectory.Controls.Add(this.btnChooseFolder);
            this.gbDirectory.Controls.Add(this.selectedDirectoryLabel);
            this.gbDirectory.Location = new System.Drawing.Point(95, 176);
            this.gbDirectory.Name = "gbDirectory";
            this.gbDirectory.Size = new System.Drawing.Size(610, 99);
            this.gbDirectory.TabIndex = 0;
            this.gbDirectory.TabStop = false;
            this.gbDirectory.Text = "Directory";
            // 
            // btnChooseFolder
            // 
            this.btnChooseFolder.Location = new System.Drawing.Point(249, 20);
            this.btnChooseFolder.Name = "btnChooseFolder";
            this.btnChooseFolder.Size = new System.Drawing.Size(112, 23);
            this.btnChooseFolder.TabIndex = 0;
            this.btnChooseFolder.Text = "Choose folder";
            this.btnChooseFolder.UseVisualStyleBackColor = true;
            this.btnChooseFolder.Click += new System.EventHandler(this.BtnChooseFolder_Click);
            // 
            // selectedDirectoryLabel
            // 
            this.selectedDirectoryLabel.AutoSize = true;
            this.selectedDirectoryLabel.Location = new System.Drawing.Point(255, 63);
            this.selectedDirectoryLabel.Name = "selectedDirectoryLabel";
            this.selectedDirectoryLabel.Size = new System.Drawing.Size(101, 15);
            this.selectedDirectoryLabel.TabIndex = 1;
            this.selectedDirectoryLabel.Text = "Selected directory";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(344, 293);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(112, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // saveContentFileDialog
            // 
            this.saveContentFileDialog.DefaultExt = "xml";
            this.saveContentFileDialog.Filter = "Fichiers XML|*.xml|Tous les fichiers|*.*";
            // 
            // labelBrowseTime
            // 
            this.labelBrowseTime.AutoSize = true;
            this.labelBrowseTime.Location = new System.Drawing.Point(682, 411);
            this.labelBrowseTime.Name = "labelBrowseTime";
            this.labelBrowseTime.Size = new System.Drawing.Size(71, 15);
            this.labelBrowseTime.TabIndex = 2;
            this.labelBrowseTime.Text = "BrowseTime";
            // 
            // labelSaveTime
            // 
            this.labelSaveTime.AutoSize = true;
            this.labelSaveTime.Location = new System.Drawing.Point(682, 426);
            this.labelSaveTime.Name = "labelSaveTime";
            this.labelSaveTime.Size = new System.Drawing.Size(57, 15);
            this.labelSaveTime.TabIndex = 3;
            this.labelSaveTime.Text = "SaveTime";
            // 
            // backgroundWorkerDirectoryBrowse
            // 
            this.backgroundWorkerDirectoryBrowse.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDirectoryBrowse_DoWork);
            this.backgroundWorkerDirectoryBrowse.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerDirectoryBrowse_RunWorkerCompleted);
            // 
            // labelTotalTime
            // 
            this.labelTotalTime.AutoSize = true;
            this.labelTotalTime.Location = new System.Drawing.Point(682, 444);
            this.labelTotalTime.Name = "labelTotalTime";
            this.labelTotalTime.Size = new System.Drawing.Size(59, 15);
            this.labelTotalTime.TabIndex = 4;
            this.labelTotalTime.Text = "Total time";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(95, 322);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(610, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // labelException
            // 
            this.labelException.AutoSize = true;
            this.labelException.Location = new System.Drawing.Point(12, 369);
            this.labelException.Name = "labelException";
            this.labelException.Size = new System.Drawing.Size(59, 15);
            this.labelException.TabIndex = 5;
            this.labelException.Text = "Exception";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 472);
            this.Controls.Add(this.labelException);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelTotalTime);
            this.Controls.Add(this.labelSaveTime);
            this.Controls.Add(this.labelBrowseTime);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.gbDirectory);
            this.Name = "MainWindow";
            this.Text = "MainWindow";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.gbDirectory.ResumeLayout(false);
            this.gbDirectory.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GroupBox gbDirectory;
        private Button btnChooseFolder;
        private FolderBrowserDialog selectedDirectoryBrowser;
        private Label selectedDirectoryLabel;
        private Button btnStart;
        private SaveFileDialog saveContentFileDialog;
        private Label labelBrowseTime;
        private Label labelSaveTime;
        private System.ComponentModel.BackgroundWorker backgroundWorkerDirectoryBrowse;
        private Label labelTotalTime;
        private ProgressBar progressBar1;
        private Label labelException;
    }
}