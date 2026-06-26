/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Krypton.Toolkit;
using System.Diagnostics;

namespace HDLG_winforms
{
	public partial class Credit : KryptonForm
	{
		public Credit ()
		{
			InitializeComponent( );
			AppUiBootstrap.RemoveFormBranding(this);
		}

		private void Credit_Load (object sender, EventArgs e)
		{
			//Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version;
			lblTitle.Values.Text = "HTML Directory List Generator " + typeof( Credit ).Assembly?.GetName( )?.Version?.ToString( );

		}

		private static void OpenUrlSafe(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
				(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
			{
				DialogResult res = MessageBox.Show($"You are about to open an external website:\n\n{url}\n\nAre you sure you want to continue?", "Security Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (res != DialogResult.Yes) return;

				ProcessStartInfo psInfo = new()
				{
					FileName = uriResult.AbsoluteUri,
					UseShellExecute = true,
					WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System)
				};
				Process.Start(psInfo);
			}
			else
			{
				throw new InvalidOperationException($"Opening URLs with scheme other than http/https is not allowed for security reasons. URL: {url}");
			}
		}

		private void labelIconCredit_LinkClicked (object sender, EventArgs e)
		{
			// Source - https://stackoverflow.com/a
			// Posted by Daniel
			// Retrieved 2026-01-23, License - CC BY-SA 4.0

			OpenUrlSafe("https://www.flaticon.com/free-icons/root-directory");
		}

		private void labelGPL_LinkClicked (object sender, EventArgs e)
		{
			OpenUrlSafe("https://www.gnu.org/licenses/gpl-3.0.en.html");
		}


		private void pictureBox1_Click (object sender, EventArgs e)
		{
			OpenUrlSafe("https://www.gnu.org/licenses/gpl-3.0.en.html");
		}
	}
}
