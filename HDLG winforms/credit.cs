/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Krypton.Toolkit;

namespace HDLG_winforms
{
	public partial class Credit : KryptonForm
	{
		private static readonly Uri GplLicenseUri = new( "https://www.gnu.org/licenses/gpl-3.0.en.html" );

		public Credit ()
		{
			InitializeComponent( );
			Icon = AppBranding.LoadApplicationIcon( );
			pictureBox1.BackColor = Color.FromArgb( 248, 250, 252 );
			pictureBox1.Image = AppBranding.LoadLogoImage( );
			AppUiBootstrap.RemoveFormBranding( this );
		}

		private void Credit_Load (object sender, EventArgs e)
		{
			//Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version;
			lblTitle.Values.Text = "HTML Directory List Generator " + typeof( Credit ).Assembly?.GetName( )?.Version?.ToString( );

		}

		private void labelGPL_LinkClicked (object sender, EventArgs e)
		{
			MainWindow.OpenUrlSafe( GplLicenseUri );
		}
	}
}
