namespace HDLG_winforms
{
	partial class Credit
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

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			headerGroupAbout = new ModernCardPanel();
			tableLayoutPanel1 = new TableLayoutPanel();
			lblTitle = new Label();
			labelCopyright = new Label();
			labelGPL = new LinkLabel();
			pictureBox1 = new PictureBox();
			headerGroupAbout.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			// 
			// headerGroupAbout
			// 
			headerGroupAbout.Dock = DockStyle.Fill;
			headerGroupAbout.Heading = "About";
			headerGroupAbout.Description = "HTML Directory List Generator";
			headerGroupAbout.Location = new Point(0, 0);
			headerGroupAbout.Name = "headerGroupAbout";
			headerGroupAbout.Size = new Size(440, 330);
			headerGroupAbout.TabIndex = 0;
			headerGroupAbout.ContentPanel.Controls.Add(tableLayoutPanel1);
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 1;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Controls.Add(lblTitle, 0, 0);
			tableLayoutPanel1.Controls.Add(labelCopyright, 0, 1);
			tableLayoutPanel1.Controls.Add(labelGPL, 0, 2);
			tableLayoutPanel1.Controls.Add(pictureBox1, 0, 3);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.Padding = new Padding(16);
			tableLayoutPanel1.RowCount = 4;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
			tableLayoutPanel1.Size = new Size(438, 274);
			tableLayoutPanel1.TabIndex = 0;
			// 
			// lblTitle
			// 
			lblTitle.Anchor = AnchorStyles.None;
			lblTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point);
			lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
			lblTitle.Location = new Point(16, 20);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(406, 34);
			lblTitle.TabIndex = 0;
			lblTitle.Text = "HTML Directory List Generator";
			lblTitle.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// labelCopyright
			// 
			labelCopyright.Anchor = AnchorStyles.None;
			labelCopyright.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			labelCopyright.ForeColor = Color.FromArgb(100, 116, 139);
			labelCopyright.Location = new Point(16, 64);
			labelCopyright.Name = "labelCopyright";
			labelCopyright.Size = new Size(406, 24);
			labelCopyright.TabIndex = 1;
			labelCopyright.Text = "Copyright Martin Labelle 2023-2026";
			labelCopyright.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// labelGPL
			// 
			labelGPL.Anchor = AnchorStyles.None;
			labelGPL.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			labelGPL.Location = new Point(16, 96);
			labelGPL.Name = "labelGPL";
			labelGPL.Size = new Size(406, 24);
			labelGPL.TabIndex = 2;
			labelGPL.TabStop = true;
			labelGPL.Text = "Distributed under the GPLv3 license";
			labelGPL.TextAlign = ContentAlignment.MiddleCenter;
			labelGPL.LinkClicked += labelGPL_LinkClicked;
			// 
			// pictureBox1
			// 
			pictureBox1.Anchor = AnchorStyles.None;
			pictureBox1.Location = new Point(59, 128);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(320, 130);
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 3;
			pictureBox1.TabStop = false;
			// 
			// Credit
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(440, 330);
			Controls.Add(headerGroupAbout);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Credit";
			StartPosition = FormStartPosition.CenterParent;
			Text = "About";
			Load += Credit_Load;
			headerGroupAbout.ResumeLayout(false);
			tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private ModernCardPanel headerGroupAbout;
		private TableLayoutPanel tableLayoutPanel1;
		private Label lblTitle;
		private Label labelCopyright;
		private LinkLabel labelGPL;
		private PictureBox pictureBox1;
	}
}