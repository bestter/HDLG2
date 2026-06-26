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
			headerGroupAbout = new Krypton.Toolkit.KryptonHeaderGroup();
			tableLayoutPanel1 = new TableLayoutPanel();
			lblTitle = new Krypton.Toolkit.KryptonLabel();
			labelCopyright = new Krypton.Toolkit.KryptonWrapLabel();
			labelGPL = new Krypton.Toolkit.KryptonLinkLabel();
			pictureBox1 = new PictureBox();
			((System.ComponentModel.ISupportInitialize)headerGroupAbout).BeginInit();
			headerGroupAbout.Panel.SuspendLayout();
			headerGroupAbout.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			SuspendLayout();
			//
			// headerGroupAbout
			//
			headerGroupAbout.Dock = DockStyle.Fill;
			headerGroupAbout.Location = new Point(0, 0);
			headerGroupAbout.Name = "headerGroupAbout";
			headerGroupAbout.Size = new Size(440, 330);
			headerGroupAbout.TabIndex = 0;
			headerGroupAbout.ValuesPrimary.Heading = "About";
			headerGroupAbout.ValuesPrimary.Description = "HTML Directory List Generator";
			headerGroupAbout.Panel.Controls.Add(tableLayoutPanel1);
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
			tableLayoutPanel1.Size = new Size(438, 230);
			tableLayoutPanel1.TabIndex = 0;
			//
			// lblTitle
			//
			lblTitle.Anchor = AnchorStyles.None;
			lblTitle.LabelStyle = Krypton.Toolkit.LabelStyle.TitlePanel;
			lblTitle.Location = new Point(62, 20);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(314, 34);
			lblTitle.TabIndex = 0;
			lblTitle.Values.Text = "HTML Directory List Generator";
			//
			// labelCopyright
			//
			labelCopyright.Anchor = AnchorStyles.None;
			labelCopyright.LabelStyle = Krypton.Toolkit.LabelStyle.NormalControl;
			labelCopyright.Location = new Point(108, 64);
			labelCopyright.Name = "labelCopyright";
			labelCopyright.Size = new Size(222, 24);
			labelCopyright.TabIndex = 1;
			labelCopyright.Text = "Copyright Martin Labelle 2023-2026";
			//
			// labelGPL
			//
			labelGPL.Anchor = AnchorStyles.None;
			labelGPL.Location = new Point(96, 96);
			labelGPL.Name = "labelGPL";
			labelGPL.Size = new Size(246, 24);
			labelGPL.TabIndex = 2;
			labelGPL.Values.Text = "Distributed under the GPLv3 license";
			labelGPL.LinkClicked += labelGPL_LinkClicked;
			//
			// pictureBox1
			//
			pictureBox1.Anchor = AnchorStyles.None;
			pictureBox1.Location = new Point(59, 118);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(320, 160);
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
			headerGroupAbout.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)headerGroupAbout).EndInit();
			headerGroupAbout.ResumeLayout(false);
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonHeaderGroup headerGroupAbout;
		private TableLayoutPanel tableLayoutPanel1;
		private Krypton.Toolkit.KryptonLabel lblTitle;
		private Krypton.Toolkit.KryptonWrapLabel labelCopyright;
		private Krypton.Toolkit.KryptonLinkLabel labelGPL;
		private PictureBox pictureBox1;
	}
}