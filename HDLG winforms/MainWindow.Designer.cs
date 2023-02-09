namespace HDLG_winforms
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

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
            this.gbDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDirectory
            // 
            this.gbDirectory.Controls.Add(this.btnChooseFolder);
            this.gbDirectory.Controls.Add(this.selectedDirectoryLabel);
            this.gbDirectory.Location = new System.Drawing.Point(237, 162);
            this.gbDirectory.Name = "gbDirectory";
            this.gbDirectory.Size = new System.Drawing.Size(326, 99);
            this.gbDirectory.TabIndex = 0;
            this.gbDirectory.TabStop = false;
            this.gbDirectory.Text = "Directory";
            // 
            // btnChooseFolder
            // 
            this.btnChooseFolder.Location = new System.Drawing.Point(107, 20);
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
            this.selectedDirectoryLabel.Location = new System.Drawing.Point(113, 63);
            this.selectedDirectoryLabel.Name = "selectedDirectoryLabel";
            this.selectedDirectoryLabel.Size = new System.Drawing.Size(101, 15);
            this.selectedDirectoryLabel.TabIndex = 1;
            this.selectedDirectoryLabel.Text = "Selected directory";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(363, 293);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // saveContentFileDialog
            // 
            this.saveContentFileDialog.DefaultExt = "xml";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.gbDirectory);
            this.Name = "MainWindow";
            this.Text = "MainWindow";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.gbDirectory.ResumeLayout(false);
            this.gbDirectory.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox gbDirectory;
        private Button btnChooseFolder;
        private FolderBrowserDialog selectedDirectoryBrowser;
        private Label selectedDirectoryLabel;
        private Button btnStart;
        private SaveFileDialog saveContentFileDialog;
    }
}