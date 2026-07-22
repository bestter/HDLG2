/*
 This file is part of HTML Directory List Generator.

 HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

 HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
 */

namespace HDLG_winforms
{
	/// <summary>
	/// Centralizes WinForms visual theme initialization for the application.
	/// </summary>
	public static class AppUiBootstrap
	{
		private static readonly object configureLock = new( );
		private static bool isConfigured;

		/// <summary>
		/// Applies global UI configuration and High-DPI modes.
		/// </summary>
		public static void Configure ()
		{
			lock (configureLock)
			{
				if (isConfigured)
				{
					return;
				}

				Application.SetHighDpiMode( HighDpiMode.PerMonitorV2 );
				isConfigured = true;
			}
		}

		/// <summary>
		/// Configures standard form appearance with modern background and fonts.
		/// </summary>
		public static void RemoveFormBranding (Form form)
		{
			ArgumentNullException.ThrowIfNull( form );
			form.BackColor = Color.FromArgb( 248, 250, 252 );
			form.Font = new Font( "Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point );
			form.BackgroundImage = null;
		}

		/// <summary>
		/// Returns the active global UI theme name.
		/// </summary>
		public static string ActiveTheme => "MinimalistSlate";
	}
}