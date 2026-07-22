namespace HDLG_winforms
{
	partial class BrowserForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			headerGroupExplorer = new ModernCardPanel();
			splitContainer1 = new SplitContainer();
			treeView1 = new TreeView();
			panelRight = new Panel();
			listViewProperties = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			panelHeaderRight = new Panel();
			lblSelectedFileName = new Label();
			btnOpenFile = new ModernButton();
			((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			panelRight.SuspendLayout();
			panelHeaderRight.SuspendLayout();
			headerGroupExplorer.SuspendLayout();
			SuspendLayout();
			// 
			// headerGroupExplorer
			// 
			headerGroupExplorer.Dock = DockStyle.Top;
			headerGroupExplorer.Heading = "Directory Explorer";
			headerGroupExplorer.Description = "Browse folders and inspect file properties.";
			headerGroupExplorer.Location = new Point(0, 0);
			headerGroupExplorer.Name = "headerGroupExplorer";
			headerGroupExplorer.Size = new Size(980, 56);
			headerGroupExplorer.TabIndex = 1;
			// 
			// splitContainer1
			// 
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer1.Location = new Point(0, 56);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Orientation = Orientation.Vertical;
			// 
			// splitContainer1.Panel1
			// 
			splitContainer1.Panel1.Controls.Add(treeView1);
			splitContainer1.Panel1.Padding = new Padding(12);
			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(panelRight);
			splitContainer1.Size = new Size(980, 584);
			splitContainer1.SplitterDistance = 320;
			splitContainer1.SplitterWidth = 6;
			splitContainer1.TabIndex = 0;
			// 
			// treeView1
			// 
			treeView1.BorderStyle = BorderStyle.FixedSingle;
			treeView1.Dock = DockStyle.Fill;
			treeView1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			treeView1.FullRowSelect = true;
			treeView1.HideSelection = false;
			treeView1.ItemHeight = 28;
			treeView1.Location = new Point(12, 12);
			treeView1.Name = "treeView1";
			treeView1.ShowLines = false;
			treeView1.Size = new Size(296, 560);
			treeView1.TabIndex = 0;
			treeView1.BeforeExpand += TreeView1_BeforeExpand;
			treeView1.AfterSelect += TreeView1_AfterSelect;
			// 
			// panelRight
			// 
			panelRight.Controls.Add(listViewProperties);
			panelRight.Controls.Add(panelHeaderRight);
			panelRight.Dock = DockStyle.Fill;
			panelRight.Location = new Point(0, 0);
			panelRight.Name = "panelRight";
			panelRight.Padding = new Padding(16);
			panelRight.Size = new Size(654, 584);
			panelRight.TabIndex = 0;
			// 
			// panelHeaderRight
			// 
			panelHeaderRight.Controls.Add(btnOpenFile);
			panelHeaderRight.Controls.Add(lblSelectedFileName);
			panelHeaderRight.Dock = DockStyle.Top;
			panelHeaderRight.Location = new Point(16, 16);
			panelHeaderRight.Name = "panelHeaderRight";
			panelHeaderRight.Size = new Size(622, 56);
			panelHeaderRight.TabIndex = 2;
			// 
			// lblSelectedFileName
			// 
			lblSelectedFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblSelectedFileName.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
			lblSelectedFileName.ForeColor = Color.FromArgb(15, 23, 42);
			lblSelectedFileName.Location = new Point(0, 10);
			lblSelectedFileName.Name = "lblSelectedFileName";
			lblSelectedFileName.Size = new Size(424, 34);
			lblSelectedFileName.TabIndex = 0;
			lblSelectedFileName.Text = "Select a file to view properties";
			lblSelectedFileName.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// btnOpenFile
			// 
			btnOpenFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnOpenFile.Enabled = false;
			btnOpenFile.IsPrimary = true;
			btnOpenFile.Location = new Point(472, 8);
			btnOpenFile.Name = "btnOpenFile";
			btnOpenFile.Size = new Size(150, 40);
			btnOpenFile.TabIndex = 1;
			btnOpenFile.Text = "Open File";
			btnOpenFile.Click += BtnOpenFile_Click;
			// 
			// listViewProperties
			// 
			listViewProperties.BorderStyle = BorderStyle.FixedSingle;
			listViewProperties.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
			listViewProperties.Dock = DockStyle.Fill;
			listViewProperties.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			listViewProperties.FullRowSelect = true;
			listViewProperties.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			listViewProperties.Location = new Point(16, 72);
			listViewProperties.Name = "listViewProperties";
			listViewProperties.Size = new Size(622, 496);
			listViewProperties.TabIndex = 1;
			listViewProperties.UseCompatibleStateImageBehavior = false;
			listViewProperties.View = View.Details;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Property";
			columnHeader1.Width = 220;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Value";
			columnHeader2.Width = 360;
			// 
			// BrowserForm
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(980, 640);
			Controls.Add(splitContainer1);
			Controls.Add(headerGroupExplorer);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			MinimumSize = new Size(800, 520);
			Name = "BrowserForm";
			ShowIcon = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Directory Explorer";
			Load += BrowserForm_Load;
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
			splitContainer1.ResumeLayout(false);
			panelRight.ResumeLayout(false);
			panelHeaderRight.ResumeLayout(false);
			headerGroupExplorer.ResumeLayout(false);
			ResumeLayout(false);
		}

		private SplitContainer splitContainer1;
		private TreeView treeView1;
		private Panel panelRight;
		private Panel panelHeaderRight;
		private Label lblSelectedFileName;
		private ModernButton btnOpenFile;
		private ListView listViewProperties;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ModernCardPanel headerGroupExplorer;
	}
}