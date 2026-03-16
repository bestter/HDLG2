namespace HDLG_winforms
{
	partial class Credit
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Credit));
			lblTitle = new Label();
			labelCopyright = new Label();
			labelIconCredit = new LinkLabel();
			labelGPL = new LinkLabel();
			pictureBox1 = new PictureBox();
			tableLayoutPanel1 = new TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			tableLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// lblTitle
			// 
			lblTitle.Anchor = AnchorStyles.None;
			lblTitle.AutoSize = true;
			lblTitle.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
			lblTitle.Location = new Point(23, 10);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(313, 30);
			lblTitle.TabIndex = 0;
			lblTitle.Text = "HTML Directory List Generator";
			// 
			// labelCopyright
			// 
			labelCopyright.Anchor = AnchorStyles.None;
			labelCopyright.AutoSize = true;
			labelCopyright.Location = new Point(92, 55);
			labelCopyright.Name = "labelCopyright";
			labelCopyright.Size = new Size(176, 17);
			labelCopyright.TabIndex = 1;
			labelCopyright.Text = "© Martin Labelle 2023-2026";
			// 
			// labelIconCredit
			// 
			labelIconCredit.Anchor = AnchorStyles.None;
			labelIconCredit.AutoSize = true;
			labelIconCredit.Location = new Point(123, 219);
			labelIconCredit.Name = "labelIconCredit";
			labelIconCredit.Size = new Size(114, 17);
			labelIconCredit.TabIndex = 2;
			labelIconCredit.TabStop = true;
			labelIconCredit.Text = "Icon by FlatIcons";
			labelIconCredit.LinkClicked += labelIconCredit_LinkClicked;
			// 
			// labelGPL
			// 
			labelGPL.Anchor = AnchorStyles.None;
			labelGPL.AutoSize = true;
			labelGPL.Location = new Point(81, 95);
			labelGPL.Name = "labelGPL";
			labelGPL.Size = new Size(198, 17);
			labelGPL.TabIndex = 3;
			labelGPL.TabStop = true;
			labelGPL.Text = "Distribué sous licence GPL V3";
			labelGPL.LinkClicked += labelGPL_LinkClicked;
			// 
			// pictureBox1
			// 
			pictureBox1.Anchor = AnchorStyles.None;
			pictureBox1.Cursor = Cursors.Hand;
			pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
			pictureBox1.InitialImage = (Image)resources.GetObject("pictureBox1.InitialImage");
			pictureBox1.Location = new Point(104, 131);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size(151, 68);
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			pictureBox1.TabIndex = 4;
			pictureBox1.TabStop = false;
			pictureBox1.Click += pictureBox1_Click;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 1;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Controls.Add(lblTitle, 0, 0);
			tableLayoutPanel1.Controls.Add(labelCopyright, 0, 1);
			tableLayoutPanel1.Controls.Add(labelGPL, 0, 2);
			tableLayoutPanel1.Controls.Add(pictureBox1, 0, 3);
			tableLayoutPanel1.Controls.Add(labelIconCredit, 0, 4);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.Padding = new Padding(10);
			tableLayoutPanel1.RowCount = 5;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel1.Size = new Size(360, 260);
			tableLayoutPanel1.TabIndex = 5;
			// 
			// Credit
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(360, 260);
			Controls.Add(tableLayoutPanel1);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "Credit";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Crédits";
			Load += Credit_Load;
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Label lblTitle;
		private Label labelCopyright;
		private LinkLabel labelIconCredit;
		private LinkLabel labelGPL;
		private PictureBox pictureBox1;
		private TableLayoutPanel tableLayoutPanel1;
	}
}