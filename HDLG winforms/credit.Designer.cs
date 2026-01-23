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
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose( );
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( Credit ) );
			lblTitle = new Label( );
			labelCopyright = new Label( );
			labelIconCredit = new Label( );
			labelGPL = new Label( );
			pictureBox1 = new PictureBox( );
			((System.ComponentModel.ISupportInitialize) pictureBox1).BeginInit( );
			SuspendLayout( );
			// 
			// lblTitle
			// 
			lblTitle.AutoSize = true;
			lblTitle.Location = new Point( 282, 45 );
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size( 167, 15 );
			lblTitle.TabIndex = 0;
			lblTitle.Text = "HTML Directory List Generator";
			// 
			// labelCopyright
			// 
			labelCopyright.AutoSize = true;
			labelCopyright.Location = new Point( 289, 106 );
			labelCopyright.Name = "labelCopyright";
			labelCopyright.Size = new Size( 152, 15 );
			labelCopyright.TabIndex = 1;
			labelCopyright.Text = "© Martin Labelle 2023-2026";
			// 
			// labelIconCredit
			// 
			labelIconCredit.AutoSize = true;
			labelIconCredit.Location = new Point( 317, 237 );
			labelIconCredit.Name = "labelIconCredit";
			labelIconCredit.Size = new Size( 96, 15 );
			labelIconCredit.TabIndex = 2;
			labelIconCredit.Text = "Icon by FlatIcons";
			labelIconCredit.Click += labelIconCredit_Click;
			// 
			// labelGPL
			// 
			labelGPL.AutoSize = true;
			labelGPL.Location = new Point( 285, 169 );
			labelGPL.Name = "labelGPL";
			labelGPL.Size = new Size( 160, 15 );
			labelGPL.TabIndex = 3;
			labelGPL.Text = "Distribué sous license GPL V3";
			labelGPL.Click += labelGPL_Click;
			// 
			// pictureBox1
			// 
			pictureBox1.Image = (Image) resources.GetObject( "pictureBox1.Image" );
			pictureBox1.InitialImage = (Image) resources.GetObject( "pictureBox1.InitialImage" );
			pictureBox1.Location = new Point( 451, 142 );
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new Size( 151, 68 );
			pictureBox1.TabIndex = 4;
			pictureBox1.TabStop = false;
			pictureBox1.Click += pictureBox1_Click;
			// 
			// Credit
			// 
			AutoScaleDimensions = new SizeF( 7F, 15F );
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size( 748, 631 );
			Controls.Add( pictureBox1 );
			Controls.Add( labelGPL );
			Controls.Add( labelIconCredit );
			Controls.Add( lblTitle );
			Controls.Add( labelCopyright );
			Icon = (Icon) resources.GetObject( "$this.Icon" );
			Name = "Credit";
			Text = "Crédit";
			Load += Credit_Load;
			((System.ComponentModel.ISupportInitialize) pictureBox1).EndInit( );
			ResumeLayout( false );
			PerformLayout( );
		}

		#endregion

		private Label lblTitle;
		private Label labelCopyright;
		private Label labelIconCredit;
		private Label labelGPL;
		private PictureBox pictureBox1;
	}
}