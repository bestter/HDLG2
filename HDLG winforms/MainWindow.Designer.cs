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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( MainWindow ) );
			gbDirectory = new GroupBox( );
			cbBrowseSubDirectory = new CheckBox( );
			btnChooseFolder = new Button( );
			selectedDirectoryLabel = new Label( );
			selectedDirectoryBrowser = new FolderBrowserDialog( );
			btnStartXml = new Button( );
			saveContentFileDialog = new SaveFileDialog( );
			progressBar1 = new ProgressBar( );
			tableLayoutPanelStart = new TableLayoutPanel( );
			btnStartHtml = new Button( );
			saveFileDialog1 = new SaveFileDialog( );
			saveFileDialogHtml = new SaveFileDialog( );
			menuStrip1 = new MenuStrip( );
			CreditToolStripMenuItem = new ToolStripMenuItem( );
			statusStrip1 = new StatusStrip();
			toolStripStatusLabelBrowseTime = new ToolStripStatusLabel();
			toolStripStatusLabelSaveTime = new ToolStripStatusLabel();
			toolStripStatusLabelTotalTime = new ToolStripStatusLabel();
			toolStripStatusLabelException = new ToolStripStatusLabel();
			gbDirectory.SuspendLayout( );
			tableLayoutPanelStart.SuspendLayout( );
			menuStrip1.SuspendLayout( );
			statusStrip1.SuspendLayout( );
			SuspendLayout( );
			// 
			// gbDirectory
			// 
			gbDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			gbDirectory.Controls.Add( cbBrowseSubDirectory );
			gbDirectory.Controls.Add( btnChooseFolder );
			gbDirectory.Controls.Add( selectedDirectoryLabel );
			gbDirectory.Location = new Point( 95, 176 );
			gbDirectory.Name = "gbDirectory";
			gbDirectory.Size = new Size( 610, 99 );
			gbDirectory.TabIndex = 0;
			gbDirectory.TabStop = false;
			gbDirectory.Text = "Directory";
			// 
			// cbBrowseSubDirectory
			// 
			cbBrowseSubDirectory.AutoSize = true;
			cbBrowseSubDirectory.Checked = true;
			cbBrowseSubDirectory.CheckState = CheckState.Checked;
			cbBrowseSubDirectory.Location = new Point( 398, 22 );
			cbBrowseSubDirectory.Name = "cbBrowseSubDirectory";
			cbBrowseSubDirectory.Size = new Size( 151, 21 );
			cbBrowseSubDirectory.TabIndex = 2;
			cbBrowseSubDirectory.Text = "Browse sub-directory";
			cbBrowseSubDirectory.UseVisualStyleBackColor = true;
			// 
			// btnChooseFolder
			// 
			btnChooseFolder.FlatStyle = FlatStyle.System;
			btnChooseFolder.Location = new Point( 249, 20 );
			btnChooseFolder.Name = "btnChooseFolder";
			btnChooseFolder.Size = new Size( 112, 26 );
			btnChooseFolder.TabIndex = 0;
			btnChooseFolder.Text = "Choose folder";
			btnChooseFolder.UseVisualStyleBackColor = true;
			btnChooseFolder.Click += BtnChooseFolder_Click;
			// 
			// selectedDirectoryLabel
			// 
			selectedDirectoryLabel.AutoSize = true;
			selectedDirectoryLabel.Location = new Point( 255, 63 );
			selectedDirectoryLabel.Name = "selectedDirectoryLabel";
			selectedDirectoryLabel.Size = new Size( 112, 17 );
			selectedDirectoryLabel.TabIndex = 1;
			selectedDirectoryLabel.Text = "Selected directory";
			// 
			// btnStartXml
			// 
			btnStartXml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartXml.FlatStyle = FlatStyle.System;
			btnStartXml.Location = new Point( 3, 3 );
			btnStartXml.Name = "btnStartXml";
			btnStartXml.Size = new Size( 123, 34 );
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
			// progressBar1
			// 
			progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			progressBar1.Location = new Point( 95, 322 );
			progressBar1.Name = "progressBar1";
			progressBar1.Size = new Size( 610, 23 );
			progressBar1.TabIndex = 2;
			// 
			// tableLayoutPanelStart
			// 
			tableLayoutPanelStart.Anchor = AnchorStyles.Top;
			tableLayoutPanelStart.ColumnCount = 2;
			tableLayoutPanelStart.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50F ) );
			tableLayoutPanelStart.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 50F ) );
			tableLayoutPanelStart.Controls.Add( btnStartHtml, 1, 0 );
			tableLayoutPanelStart.Controls.Add( btnStartXml, 0, 0 );
			tableLayoutPanelStart.Location = new Point( 271, 275 );
			tableLayoutPanelStart.Name = "tableLayoutPanelStart";
			tableLayoutPanelStart.RowCount = 1;
			tableLayoutPanelStart.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
			tableLayoutPanelStart.Size = new Size( 259, 40 );
			tableLayoutPanelStart.TabIndex = 6;
			// 
			// btnStartHtml
			// 
			btnStartHtml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartHtml.FlatStyle = FlatStyle.System;
			btnStartHtml.Location = new Point( 132, 3 );
			btnStartHtml.Name = "btnStartHtml";
			btnStartHtml.Size = new Size( 124, 34 );
			btnStartHtml.TabIndex = 7;
			btnStartHtml.Text = "HTML";
			btnStartHtml.UseVisualStyleBackColor = true;
			btnStartHtml.Click += BtnStartHtml_Click;
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
			// menuStrip1
			// 
			menuStrip1.Items.AddRange( new ToolStripItem [ ] { CreditToolStripMenuItem } );
			menuStrip1.Location = new Point( 0, 0 );
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new Size( 800, 24 );
			menuStrip1.TabIndex = 7;
			menuStrip1.Text = "menuStrip1";
			// 
			// CreditToolStripMenuItem
			// 
			CreditToolStripMenuItem.Name = "CreditToolStripMenuItem";
			CreditToolStripMenuItem.Size = new Size( 51, 20 );
			CreditToolStripMenuItem.Text = "Credit";
			CreditToolStripMenuItem.Click += CreditToolStripMenuItem_Click;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] {
            toolStripStatusLabelBrowseTime,
            toolStripStatusLabelSaveTime,
            toolStripStatusLabelTotalTime,
            toolStripStatusLabelException});
			statusStrip1.Location = new Point(0, 450);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(800, 22);
			statusStrip1.TabIndex = 8;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabelBrowseTime
			// 
			toolStripStatusLabelBrowseTime.Name = "toolStripStatusLabelBrowseTime";
			toolStripStatusLabelBrowseTime.Size = new Size(0, 17);
			// 
			// toolStripStatusLabelSaveTime
			// 
			toolStripStatusLabelSaveTime.Name = "toolStripStatusLabelSaveTime";
			toolStripStatusLabelSaveTime.Size = new Size(0, 17);
			// 
			// toolStripStatusLabelTotalTime
			// 
			toolStripStatusLabelTotalTime.Name = "toolStripStatusLabelTotalTime";
			toolStripStatusLabelTotalTime.Size = new Size(0, 17);
			// 
			// toolStripStatusLabelException
			// 
			toolStripStatusLabelException.Name = "toolStripStatusLabelException";
			toolStripStatusLabelException.Size = new Size(0, 17);
			toolStripStatusLabelException.ForeColor = Color.Red;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new SizeF( 7F, 17F );
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size( 800, 472 );
			Controls.Add( tableLayoutPanelStart );
			Controls.Add( progressBar1 );
			Controls.Add( gbDirectory );
			Controls.Add( statusStrip1 );
			Controls.Add( menuStrip1 );
			Font = new Font( "Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point );
			Icon = (Icon) resources.GetObject( "$this.Icon" );
			MainMenuStrip = menuStrip1;
			Name = "MainWindow";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MainWindow";
			Load += MainWindow_Load;
			gbDirectory.ResumeLayout( false );
			gbDirectory.PerformLayout( );
			tableLayoutPanelStart.ResumeLayout( false );
			menuStrip1.ResumeLayout( false );
			menuStrip1.PerformLayout( );
			statusStrip1.ResumeLayout( false );
			statusStrip1.PerformLayout( );
			ResumeLayout( false );
			PerformLayout( );
		}

		#endregion

		private GroupBox gbDirectory;
        private Button btnChooseFolder;
        private FolderBrowserDialog selectedDirectoryBrowser;
        private Label selectedDirectoryLabel;
        private Button btnStartXml;
        private SaveFileDialog saveContentFileDialog;
        private ProgressBar progressBar1;
        private TableLayoutPanel tableLayoutPanelStart;
        private Button btnStartHtml;
        private SaveFileDialog saveFileDialog1;
        private SaveFileDialog saveFileDialogHtml;
        private CheckBox cbBrowseSubDirectory;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem CreditToolStripMenuItem;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabelBrowseTime;
		private ToolStripStatusLabel toolStripStatusLabelSaveTime;
		private ToolStripStatusLabel toolStripStatusLabelTotalTime;
		private ToolStripStatusLabel toolStripStatusLabelException;
	}
}