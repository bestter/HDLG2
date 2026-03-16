/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
using System.Diagnostics;

namespace HDLG_winforms
{
	public partial class Credit : Form
	{
		public Credit ()
		{
			InitializeComponent( );

		}

		private void Credit_Load (object sender, EventArgs e)
		{
			//Version version = Assembly.GetExecutingAssembly( ).GetName( ).Version;
			lblTitle.Text = "HTML Directory List Generator " + typeof( Credit ).Assembly?.GetName( )?.Version?.ToString( );

		}

		private void labelIconCredit_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Source - https://stackoverflow.com/a
			// Posted by Daniel
			// Retrieved 2026-01-23, License - CC BY-SA 4.0

			ProcessStartInfo psInfo = new( )
			{
				FileName = "https://www.flaticon.com/free-icons/root-directory",
				UseShellExecute = true
			};
			Process.Start( psInfo );

		}

		private void labelGPL_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			ProcessStartInfo psInfo = new( )
			{
				FileName = "https://www.gnu.org/licenses/gpl-3.0.en.html",
				UseShellExecute = true
			};
			Process.Start( psInfo );
		}


		private void pictureBox1_Click (object sender, EventArgs e)
		{
			ProcessStartInfo psInfo = new( )
			{
				FileName = "https://www.gnu.org/licenses/gpl-3.0.en.html",
				UseShellExecute = true
			};
			Process.Start( psInfo );
		}
	}
}
