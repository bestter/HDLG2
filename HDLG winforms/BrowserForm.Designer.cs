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
            splitContainer1 = new SplitContainer();
            treeView1 = new TreeView();
            panelRight = new Panel();
            listViewProperties = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            panelHeaderRight = new Panel();
            lblSelectedFileName = new Label();
            btnOpenFile = new Button();
            panelTop = new Panel();
            lblTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelRight.SuspendLayout();
            panelHeaderRight.SuspendLayout();
            panelTop.SuspendLayout();
            SuspendLayout();
            
            // panelTop
            panelTop.BackColor = Color.FromArgb(32, 33, 36);
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(950, 60);
            panelTop.TabIndex = 1;
            
            // lblTitle
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(180, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Directory Explorer";
            
            // splitContainer1
            splitContainer1.BackColor = Color.FromArgb(220, 222, 224); // splitter color
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 60);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.SplitterWidth = 2;
            
            // splitContainer1.Panel1
            splitContainer1.Panel1.BackColor = Color.FromArgb(248, 249, 250);
            splitContainer1.Panel1.Controls.Add(treeView1);
            splitContainer1.Panel1.Padding = new Padding(10);
            
            // splitContainer1.Panel2
            splitContainer1.Panel2.BackColor = Color.White;
            splitContainer1.Panel2.Controls.Add(panelRight);
            splitContainer1.Size = new Size(950, 540);
            splitContainer1.SplitterDistance = 300;
            splitContainer1.TabIndex = 0;
            
            // treeView1
            treeView1.BackColor = Color.FromArgb(248, 249, 250);
            treeView1.BorderStyle = BorderStyle.None;
            treeView1.Dock = DockStyle.Fill;
            treeView1.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            treeView1.FullRowSelect = true;
            treeView1.HideSelection = false;
            treeView1.ItemHeight = 28;
            treeView1.Location = new Point(10, 10);
            treeView1.Name = "treeView1";
            treeView1.ShowLines = false;
            treeView1.Size = new Size(280, 520);
            treeView1.TabIndex = 0;
            treeView1.BeforeExpand += TreeView1_BeforeExpand;
            treeView1.AfterSelect += TreeView1_AfterSelect;
            
            // panelRight
            panelRight.Controls.Add(listViewProperties);
            panelRight.Controls.Add(panelHeaderRight);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(0, 0);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(20);
            panelRight.Size = new Size(648, 540);
            panelRight.TabIndex = 0;
            
            // panelHeaderRight
            panelHeaderRight.Controls.Add(btnOpenFile);
            panelHeaderRight.Controls.Add(lblSelectedFileName);
            panelHeaderRight.Dock = DockStyle.Top;
            panelHeaderRight.Location = new Point(20, 20);
            panelHeaderRight.Name = "panelHeaderRight";
            panelHeaderRight.Size = new Size(608, 60);
            panelHeaderRight.TabIndex = 2;
            
            // lblSelectedFileName
            lblSelectedFileName.AutoEllipsis = true;
            lblSelectedFileName.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
            lblSelectedFileName.ForeColor = Color.FromArgb(32, 33, 36);
            lblSelectedFileName.Location = new Point(0, 10);
            lblSelectedFileName.Name = "lblSelectedFileName";
            lblSelectedFileName.Size = new Size(400, 40);
            lblSelectedFileName.TabIndex = 0;
            lblSelectedFileName.Text = "Select a file to view properties";
            lblSelectedFileName.TextAlign = ContentAlignment.MiddleLeft;
            
            // btnOpenFile
            btnOpenFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOpenFile.BackColor = Color.FromArgb(26, 115, 232);
            btnOpenFile.Cursor = Cursors.Hand;
            btnOpenFile.Enabled = false;
            btnOpenFile.FlatAppearance.BorderSize = 0;
            btnOpenFile.FlatStyle = FlatStyle.Flat;
            btnOpenFile.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnOpenFile.ForeColor = Color.White;
            btnOpenFile.Location = new Point(468, 10);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(140, 40);
            btnOpenFile.TabIndex = 1;
            btnOpenFile.Text = "Open File";
            btnOpenFile.UseVisualStyleBackColor = false;
            btnOpenFile.Click += BtnOpenFile_Click;
            
            // listViewProperties
            listViewProperties.BorderStyle = BorderStyle.None;
            listViewProperties.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listViewProperties.Dock = DockStyle.Fill;
            listViewProperties.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            listViewProperties.FullRowSelect = true;
            listViewProperties.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewProperties.Location = new Point(20, 80);
            listViewProperties.Name = "listViewProperties";
            listViewProperties.Size = new Size(608, 440);
            listViewProperties.TabIndex = 1;
            listViewProperties.UseCompatibleStateImageBehavior = false;
            listViewProperties.View = View.Details;
            
            // columnHeader1
            columnHeader1.Text = "Property";
            columnHeader1.Width = 200;
            
            // columnHeader2
            columnHeader2.Text = "Value";
            columnHeader2.Width = 380;
            
            // BrowserForm
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(950, 600);
            Controls.Add(splitContainer1);
            Controls.Add(panelTop);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
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
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ResumeLayout(false);
        }

        private SplitContainer splitContainer1;
        private TreeView treeView1;
        private Panel panelRight;
        private Panel panelHeaderRight;
        private Label lblSelectedFileName;
        private Button btnOpenFile;
        private ListView listViewProperties;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Panel panelTop;
        private Label lblTitle;
    }
}
