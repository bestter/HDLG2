using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Krypton.Toolkit;

namespace HDLG_winforms
{
	public partial class Credit : KryptonForm
	{
		public Credit ()
		{
			InitializeComponent( );
			Icon = AppBranding.LoadApplicationIcon( );
			pictureBox1.BackColor = System.Drawing.Color.FromArgb( 248, 250, 252 );
			pictureBox1.Image = AppBranding.LoadLogoImage( );
			AppUiBootstrap.RemoveFormBranding( this );
		}

		private void Credit_Load (object sender, EventArgs e)
		{
			lblTitle.Values.Text = "HTML Directory List Generator " + typeof( Credit ).Assembly?.GetName( )?.Version?.ToString( );
		}

		private static void OpenUrlSafe (string url)
		{
			if (Uri.TryCreate( url, UriKind.Absolute, out Uri? uriResult ) &&
				(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
			{
				System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show( $"You are about to open an external website:\n\n{url}\n\nAre you sure you want to continue?", "Security Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning );
				if (res != System.Windows.Forms.DialogResult.Yes) return;

				if (RuntimeInformation.IsOSPlatform( OSPlatform.Windows ))
				{
					Process.Start( new ProcessStartInfo
					{
						FileName = "explorer.exe",
						Arguments = $"\"{uriResult.AbsoluteUri}\"",
						UseShellExecute = false,
						WorkingDirectory = Environment.GetFolderPath( Environment.SpecialFolder.System )
					} );
				}
				else if (RuntimeInformation.IsOSPlatform( OSPlatform.Linux ))
				{
					Process.Start( new ProcessStartInfo { FileName = "xdg-open", Arguments = $"\"{uriResult.AbsoluteUri}\"", UseShellExecute = false } );
				}
				else if (RuntimeInformation.IsOSPlatform( OSPlatform.OSX ))
				{
					Process.Start( new ProcessStartInfo { FileName = "open", Arguments = $"\"{uriResult.AbsoluteUri}\"", UseShellExecute = false } );
				}
				else
				{
					throw new PlatformNotSupportedException( "Opening URLs is not supported on this platform." );
				}
			}
			else
			{
				throw new InvalidOperationException( $"Opening URLs with scheme other than http/https is not allowed for security reasons. URL: {url}" );
			}
		}

		private void labelGPL_LinkClicked (object sender, EventArgs e)
		{
			OpenUrlSafe( "https://www.gnu.org/licenses/gpl-3.0.en.html" );
		}
	}
}
