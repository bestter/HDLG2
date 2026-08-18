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

		private static void OpenUrlSafe (string url)
		{
			if (string.IsNullOrWhiteSpace( url ) ||
				!Uri.TryCreate( url, UriKind.Absolute, out Uri? uriResult ) ||
				(uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps) ||
				!Uri.IsWellFormedUriString( url, UriKind.Absolute ))
			{
				throw new InvalidOperationException( $"Opening URLs with scheme other than http/https is not allowed for security reasons. URL: {url}" );
			}

			if (uriResult != null)
			{
				DialogResult res = MessageBox.Show( $"You are about to open an external website:\n\n{url}\n\nAre you sure you want to continue?", "Security Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning );
				if (res != DialogResult.Yes) return;

				string explorerPath = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Windows ), "explorer.exe" );
				ProcessStartInfo psInfo = new( explorerPath )
				{
					UseShellExecute = false,
					WorkingDirectory = Environment.GetFolderPath( Environment.SpecialFolder.System )
				};
				psInfo.ArgumentList.Add( uriResult.AbsoluteUri );
				Process.Start( psInfo );
			}
			else
			{
				throw new InvalidOperationException( $"Opening URLs with scheme other than http/https is not allowed for security reasons. URL: {url}" );
			}
		}

		private void labelGPL_LinkClicked (object sender, EventArgs e)
		{
			MainWindow.OpenUrlSafe( GplLicenseUri );
		}
	}
}
