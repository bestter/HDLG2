/*
 This file is part of HTML Directory List Generator.

 HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

 HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
 */

using Krypton.Toolkit;

namespace HDLG_winforms
{
	/// <summary>
	/// Centralizes WinForms visual theme initialization for the application.
	/// </summary>
	public static class AppUiBootstrap
	{
		private static readonly object configureLock = new();
		private static KryptonManager? uiManager;
		private static bool isConfigured;

		/// <summary>
		/// Applies the global Krypton palette used by all forms.
		/// </summary>
		public static void Configure()
		{
			lock (configureLock)
			{
				if (isConfigured)
				{
					return;
				}

				uiManager = new KryptonManager();
				uiManager.GlobalPaletteMode = PaletteMode.Microsoft365BlueLightMode;
				isConfigured = true;
			}
		}

		/// <summary>
		/// Removes the default Krypton watermark rendered in the form client area.
		/// </summary>
		public static void RemoveFormBranding(KryptonForm form)
		{
			ArgumentNullException.ThrowIfNull(form);

			PaletteBack commonBack = form.StateCommon!.Back!;
			commonBack.Image = null;
			commonBack.ImageStyle = PaletteImageStyle.Inherit;

			form.StateActive!.Back!.Image = null;
			form.StateInactive!.Back!.Image = null;
			form.BackgroundImage = null;
		}

		/// <summary>
		/// Returns the active global palette mode after configuration.
		/// </summary>
		public static PaletteMode ActivePaletteMode =>
			uiManager?.GlobalPaletteMode ?? PaletteMode.Microsoft365BlueLightMode;
	}
}