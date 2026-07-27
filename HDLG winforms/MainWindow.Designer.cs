namespace HDLG_winforms
{
	partial class MainWindow
	{
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
			panelRoot = new TableLayoutPanel();
			panelHeader = new Panel();
			lblAppTitle = new Krypton.Toolkit.KryptonLabel();
			btnAbout = new Krypton.Toolkit.KryptonButton();
			headerGroupDirectory = new Krypton.Toolkit.KryptonHeaderGroup();
			panelDirectoryContent = new Panel();
			btnChooseFolder = new Krypton.Toolkit.KryptonButton();
			cbBrowseSubDirectory = new Krypton.Toolkit.KryptonCheckBox();
			selectedDirectoryLabel = new Krypton.Toolkit.KryptonWrapLabel();
			headerGroupExport = new Krypton.Toolkit.KryptonHeaderGroup();
			tableLayoutPanelStart = new TableLayoutPanel();
			btnStartXml = new Krypton.Toolkit.KryptonButton();
			btnStartHtml = new Krypton.Toolkit.KryptonButton();
			btnStartUi = new Krypton.Toolkit.KryptonButton();
			progressBar1 = new Krypton.Toolkit.KryptonProgressBar();
			statusStrip1 = new Krypton.Toolkit.KryptonStatusStrip();
			toolStripStatusLabelBrowseTime = new ToolStripStatusLabel();
			toolStripStatusLabelSaveTime = new ToolStripStatusLabel();
			toolStripStatusLabelTotalTime = new ToolStripStatusLabel();
			toolStripStatusLabelException = new ToolStripStatusLabel();
			selectedDirectoryBrowser = new FolderBrowserDialog();
			saveContentFileDialog = new SaveFileDialog();
			saveFileDialog1 = new SaveFileDialog();
			saveFileDialogHtml = new SaveFileDialog();
			panelRoot.SuspendLayout();
			panelHeader.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)headerGroupDirectory).BeginInit();
			headerGroupDirectory.Panel.SuspendLayout();
			headerGroupDirectory.SuspendLayout();
			panelDirectoryContent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)headerGroupExport).BeginInit();
			headerGroupExport.Panel.SuspendLayout();
			headerGroupExport.SuspendLayout();
			tableLayoutPanelStart.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			//
			// panelRoot
			//
			panelRoot.ColumnCount = 1;
			panelRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			panelRoot.Controls.Add(panelHeader, 0, 0);
			panelRoot.Controls.Add(headerGroupDirectory, 0, 1);
			panelRoot.Controls.Add(headerGroupExport, 0, 2);
			panelRoot.Controls.Add(progressBar1, 0, 3);
			panelRoot.Dock = DockStyle.Fill;
			panelRoot.Location = new Point(0, 0);
			panelRoot.Name = "panelRoot";
			panelRoot.Padding = new Padding(24, 20, 24, 16);
			panelRoot.RowCount = 4;
			panelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			panelRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
			panelRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
			panelRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
			panelRoot.Size = new Size(884, 520);
			panelRoot.TabIndex = 0;
			//
			// panelHeader
			//
			panelHeader.Controls.Add(lblAppTitle);
			panelHeader.Controls.Add(btnAbout);
			panelHeader.Dock = DockStyle.Fill;
			panelHeader.Location = new Point(27, 23);
			panelHeader.Name = "panelHeader";
			panelHeader.Size = new Size(830, 50);
			panelHeader.TabIndex = 0;
			//
			// lblAppTitle
			//
			lblAppTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblAppTitle.LabelStyle = Krypton.Toolkit.LabelStyle.TitlePanel;
			lblAppTitle.Location = new Point(0, 8);
			lblAppTitle.Name = "lblAppTitle";
			lblAppTitle.Size = new Size(650, 34);
			lblAppTitle.TabIndex = 0;
			lblAppTitle.Values.Text = "HTML Directory List Generator";
			//
			// btnAbout
			//
			btnAbout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnAbout.Location = new Point(710, 6);
			btnAbout.Name = "btnAbout";
			btnAbout.Size = new Size(120, 36);
			btnAbout.TabIndex = 1;
			btnAbout.Values.Text = "About";
			btnAbout.Click += BtnAbout_Click;
			//
			// headerGroupDirectory
			//
			headerGroupDirectory.Dock = DockStyle.Fill;
			headerGroupDirectory.Location = new Point(27, 79);
			headerGroupDirectory.Name = "headerGroupDirectory";
			headerGroupDirectory.Size = new Size(830, 180);
			headerGroupDirectory.TabIndex = 1;
			headerGroupDirectory.ValuesPrimary.Heading = "Source Directory";
			headerGroupDirectory.ValuesPrimary.Description = "Choose a folder to scan and optionally include subdirectories.";
			headerGroupDirectory.Panel.Controls.Add(panelDirectoryContent);
			//
			// panelDirectoryContent
			//
			panelDirectoryContent.Controls.Add(btnChooseFolder);
			panelDirectoryContent.Controls.Add(cbBrowseSubDirectory);
			panelDirectoryContent.Controls.Add(selectedDirectoryLabel);
			panelDirectoryContent.Dock = DockStyle.Fill;
			panelDirectoryContent.Location = new Point(0, 0);
			panelDirectoryContent.Name = "panelDirectoryContent";
			panelDirectoryContent.Padding = new Padding(12, 8, 12, 12);
			panelDirectoryContent.Size = new Size(828, 130);
			panelDirectoryContent.TabIndex = 0;
			//
			// btnChooseFolder
			//
			btnChooseFolder.Location = new Point(12, 12);
			btnChooseFolder.Name = "btnChooseFolder";
			btnChooseFolder.Size = new Size(150, 34);
			btnChooseFolder.TabIndex = 0;
			btnChooseFolder.Values.Text = "Choose folder";
			btnChooseFolder.Click += BtnChooseFolder_Click;
			//
			// cbBrowseSubDirectory
			//
			cbBrowseSubDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			cbBrowseSubDirectory.Checked = true;
			cbBrowseSubDirectory.CheckState = CheckState.Checked;
			cbBrowseSubDirectory.Location = new Point(560, 16);
			cbBrowseSubDirectory.Name = "cbBrowseSubDirectory";
			cbBrowseSubDirectory.Size = new Size(240, 24);
			cbBrowseSubDirectory.TabIndex = 1;
			cbBrowseSubDirectory.Values.Text = "Include subdirectories";
			//
			// selectedDirectoryLabel
			//
			selectedDirectoryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			selectedDirectoryLabel.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
			selectedDirectoryLabel.Location = new Point(12, 56);
			selectedDirectoryLabel.Name = "selectedDirectoryLabel";
			selectedDirectoryLabel.Size = new Size(788, 52);
			selectedDirectoryLabel.TabIndex = 2;
			selectedDirectoryLabel.Text = "No directory selected";
			//
			// headerGroupExport
			//
			headerGroupExport.Dock = DockStyle.Fill;
			headerGroupExport.Location = new Point(27, 259);
			headerGroupExport.Name = "headerGroupExport";
			headerGroupExport.Size = new Size(830, 140);
			headerGroupExport.TabIndex = 2;
			headerGroupExport.ValuesPrimary.Heading = "Export";
			headerGroupExport.ValuesPrimary.Description = "Generate a listing or explore files interactively.";
			headerGroupExport.Panel.Controls.Add(tableLayoutPanelStart);
			//
			// tableLayoutPanelStart
			//
			tableLayoutPanelStart.ColumnCount = 3;
			tableLayoutPanelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			tableLayoutPanelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
			tableLayoutPanelStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
			tableLayoutPanelStart.Controls.Add(btnStartXml, 0, 0);
			tableLayoutPanelStart.Controls.Add(btnStartHtml, 1, 0);
			tableLayoutPanelStart.Controls.Add(btnStartUi, 2, 0);
			tableLayoutPanelStart.Dock = DockStyle.Fill;
			tableLayoutPanelStart.Location = new Point(0, 0);
			tableLayoutPanelStart.Name = "tableLayoutPanelStart";
			tableLayoutPanelStart.Padding = new Padding(12, 16, 12, 12);
			tableLayoutPanelStart.RowCount = 1;
			tableLayoutPanelStart.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanelStart.Size = new Size(828, 90);
			tableLayoutPanelStart.TabIndex = 0;
			//
			// btnStartXml
			//
			btnStartXml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartXml.Location = new Point(15, 19);
			btnStartXml.Margin = new Padding(3, 3, 6, 3);
			btnStartXml.Name = "btnStartXml";
			btnStartXml.Size = new Size(258, 58);
			btnStartXml.TabIndex = 0;
			btnStartXml.Values.Text = "Export XML";
			btnStartXml.Click += BtnStart_Click;
			//
			// btnStartHtml
			//
			btnStartHtml.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartHtml.ButtonStyle = Krypton.Toolkit.ButtonStyle.Cluster;
			btnStartHtml.Location = new Point(279, 19);
			btnStartHtml.Margin = new Padding(3, 3, 6, 3);
			btnStartHtml.Name = "btnStartHtml";
			btnStartHtml.Size = new Size(258, 58);
			btnStartHtml.TabIndex = 1;
			btnStartHtml.Values.Text = "Export HTML";
			btnStartHtml.Click += BtnStartHtml_Click;
			//
			// btnStartUi
			//
			btnStartUi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			btnStartUi.Location = new Point(543, 19);
			btnStartUi.Margin = new Padding(3, 3, 3, 3);
			btnStartUi.Name = "btnStartUi";
			btnStartUi.Size = new Size(270, 58);
			btnStartUi.TabIndex = 2;
			btnStartUi.Values.Text = "UI Explorer";
			btnStartUi.Click += BtnStartUi_Click;
			//
			// progressBar1
			//
			progressBar1.Dock = DockStyle.Fill;
			progressBar1.Location = new Point(27, 399);
			progressBar1.Name = "progressBar1";
			progressBar1.Size = new Size(830, 26);
			progressBar1.TabIndex = 3;
			//
			// statusStrip1
			//
			statusStrip1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
			statusStrip1.Items.AddRange(new ToolStripItem[] {
			toolStripStatusLabelBrowseTime,
			toolStripStatusLabelSaveTime,
			toolStripStatusLabelTotalTime,
			toolStripStatusLabelException});
			statusStrip1.Location = new Point(0, 520);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(884, 24);
			statusStrip1.TabIndex = 1;
			statusStrip1.Text = "statusStrip1";
			//
			// toolStripStatusLabelBrowseTime
			//
			toolStripStatusLabelBrowseTime.Name = "toolStripStatusLabelBrowseTime";
			toolStripStatusLabelBrowseTime.Size = new Size(0, 19);
			//
			// toolStripStatusLabelSaveTime
			//
			toolStripStatusLabelSaveTime.Name = "toolStripStatusLabelSaveTime";
			toolStripStatusLabelSaveTime.Size = new Size(0, 19);
			//
			// toolStripStatusLabelTotalTime
			//
			toolStripStatusLabelTotalTime.Name = "toolStripStatusLabelTotalTime";
			toolStripStatusLabelTotalTime.Size = new Size(0, 19);
			//
			// toolStripStatusLabelException
			//
			toolStripStatusLabelException.ForeColor = Color.Red;
			toolStripStatusLabelException.Name = "toolStripStatusLabelException";
			toolStripStatusLabelException.Size = new Size(0, 19);
			//
			// saveContentFileDialog
			//
			saveContentFileDialog.DefaultExt = "xml";
			saveContentFileDialog.Filter = "XML files|*.xml|All files|*.*";
			//
			// saveFileDialog1
			//
			saveFileDialog1.DefaultExt = "xml";
			saveFileDialog1.Filter = "XML files|*.xml|All files|*.*";
			//
			// saveFileDialogHtml
			//
			saveFileDialogHtml.Filter = "HTML files|*.html|All files|*.*";
			saveFileDialogHtml.FileOk += SaveFileDialogHtml_FileOk;
			//
			// MainWindow
			//
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(884, 544);
			Controls.Add(panelRoot);
			Controls.Add(statusStrip1);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(720, 480);
			Name = "MainWindow";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "HTML Directory List Generator";
			Load += MainWindow_Load;
			panelRoot.ResumeLayout(false);
			panelHeader.ResumeLayout(false);
			panelHeader.PerformLayout();
			headerGroupDirectory.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)headerGroupDirectory).EndInit();
			headerGroupDirectory.ResumeLayout(false);
			panelDirectoryContent.ResumeLayout(false);
			panelDirectoryContent.PerformLayout();
			headerGroupExport.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)headerGroupExport).EndInit();
			headerGroupExport.ResumeLayout(false);
			tableLayoutPanelStart.ResumeLayout(false);
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TableLayoutPanel panelRoot;
		private Panel panelHeader;
		private Krypton.Toolkit.KryptonLabel lblAppTitle;
		private Krypton.Toolkit.KryptonButton btnAbout;
		private Krypton.Toolkit.KryptonHeaderGroup headerGroupDirectory;
		private Panel panelDirectoryContent;
		private Krypton.Toolkit.KryptonButton btnChooseFolder;
		private Krypton.Toolkit.KryptonCheckBox cbBrowseSubDirectory;
		private Krypton.Toolkit.KryptonWrapLabel selectedDirectoryLabel;
		private Krypton.Toolkit.KryptonHeaderGroup headerGroupExport;
		private TableLayoutPanel tableLayoutPanelStart;
		private Krypton.Toolkit.KryptonButton btnStartXml;
		private Krypton.Toolkit.KryptonButton btnStartHtml;
		private Krypton.Toolkit.KryptonButton btnStartUi;
		private Krypton.Toolkit.KryptonProgressBar progressBar1;
		private Krypton.Toolkit.KryptonStatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabelBrowseTime;
		private ToolStripStatusLabel toolStripStatusLabelSaveTime;
		private ToolStripStatusLabel toolStripStatusLabelTotalTime;
		private ToolStripStatusLabel toolStripStatusLabelException;
		private FolderBrowserDialog selectedDirectoryBrowser;
		private SaveFileDialog saveContentFileDialog;
		private SaveFileDialog saveFileDialog1;
		private SaveFileDialog saveFileDialogHtml;
	}
}