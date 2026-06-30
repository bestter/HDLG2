/*
 This file is part of HTML Directory List Generator.

 HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

 HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HDLG_winforms
{
	/// <summary>
	/// Renders the HDLG monogram using the same geometry as the inline SVG logo.
	/// </summary>
	internal static class AppLogoRenderer
	{
		private static readonly Color AccentColor = Color.FromArgb(255, 2, 132, 200);

		private const float SourceWidth = 320f;
		private const float SourceHeight = 160f;

		/// <summary>
		/// Renders the wordmark bitmap at the requested size.
		/// </summary>
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns the returned bitmap.")]
		public static Bitmap RenderWordmark(int width, int height)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using var graphics = Graphics.FromImage(bitmap);
			ConfigureGraphics(graphics);
			graphics.Clear(Color.Transparent);

			float scaleX = width / SourceWidth;
			float scaleY = height / SourceHeight;
			DrawLogo(graphics, scaleX, scaleY);

			return bitmap;
		}

		/// <summary>
		/// Renders a square application icon with padding around the wordmark.
		/// </summary>
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns the returned bitmap.")]
		public static Bitmap RenderIcon(int size)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

			var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
			using var graphics = Graphics.FromImage(bitmap);
			ConfigureGraphics(graphics);
			graphics.Clear(Color.Transparent);

			float padding = size * 0.12f;
			float availableWidth = size - (padding * 2f);
			float availableHeight = size - (padding * 2f);
			float scale = Math.Min(availableWidth / SourceWidth, availableHeight / SourceHeight);

			float drawWidth = SourceWidth * scale;
			float drawHeight = SourceHeight * scale;
			float offsetX = (size - drawWidth) / 2f;
			float offsetY = (size - drawHeight) / 2f;

			graphics.TranslateTransform(offsetX, offsetY);
			DrawLogo(graphics, scale, scale);

			return bitmap;
		}

		/// <summary>
		/// Creates a multi-size icon for the application executable and window chrome.
		/// </summary>
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns the returned icon.")]
		public static Icon CreateApplicationIcon()
		{
			int[] sizes = [16, 32, 48, 256];
			using var memoryStream = new MemoryStream();
			using var writer = new BinaryWriter(memoryStream);

			writer.Write((ushort)0);
			writer.Write((ushort)1);
			writer.Write((ushort)sizes.Length);

			var pngEntries = new List<byte[]>(sizes.Length);
			foreach (int size in sizes)
			{
				using var bitmap = RenderIcon(size);
				using var pngStream = new MemoryStream();
				bitmap.Save(pngStream, ImageFormat.Png);
				pngEntries.Add(pngStream.ToArray());
			}

			int offset = 6 + (sizes.Length * 16);
			foreach ((int size, byte[] pngBytes) in sizes.Zip(pngEntries))
			{
				writer.Write((byte)(size >= 256 ? 0 : (byte)size));
				writer.Write((byte)(size >= 256 ? 0 : (byte)size));
				writer.Write((byte)0);
				writer.Write((byte)0);
				writer.Write((ushort)1);
				writer.Write((ushort)32);
				writer.Write(pngBytes.Length);
				writer.Write(offset);
				offset += pngBytes.Length;
			}

			foreach (byte[] pngBytes in pngEntries)
			{
				writer.Write(pngBytes);
			}

			memoryStream.Position = 0;
			return new Icon(memoryStream);
		}

		private static void ConfigureGraphics(Graphics graphics)
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		}

		private static void DrawLogo(Graphics graphics, float scaleX, float scaleY)
		{
			using var brush = new SolidBrush(AccentColor);

			FillRoundedRect(graphics, brush, scaleX, scaleY, 24f, 24f, 10f, 52f, 2f);
			FillRoundedRect(graphics, brush, scaleX, scaleY, 58f, 24f, 10f, 52f, 2f);
			FillRoundedRect(graphics, brush, scaleX, scaleY, 24f, 45f, 44f, 10f, 2f);

			FillRoundedRect(graphics, brush, scaleX, scaleY, 186f, 24f, 10f, 52f, 2f);
			FillLetterD(graphics, brush, scaleX, scaleY);

			FillRoundedRect(graphics, brush, scaleX, scaleY, 24f, 96f, 10f, 52f, 2f);
			FillRoundedRect(graphics, brush, scaleX, scaleY, 24f, 138f, 44f, 10f, 2f);

			FillLetterG(graphics, brush, scaleX, scaleY);
		}

		private static void FillRoundedRect(
			Graphics graphics,
			Brush brush,
			float scaleX,
			float scaleY,
			float x,
			float y,
			float width,
			float height,
			float radius)
		{
			using var path = CreateRoundedRectPath(
				x * scaleX,
				y * scaleY,
				width * scaleX,
				height * scaleY,
				radius * Math.Min(scaleX, scaleY));
			graphics.FillPath(brush, path);
		}

		private static void FillLetterD(Graphics graphics, Brush brush, float scaleX, float scaleY)
		{
			using var path = new GraphicsPath(FillMode.Alternate);
			AddLetterDBowl(path, scaleX, scaleY);
			AddScaledRoundedRectangle(path, scaleX, scaleY, 206f, 34f, 32f, 32f, 8f);
			graphics.FillPath(brush, path);
		}

		private static void FillLetterG(Graphics graphics, Brush brush, float scaleX, float scaleY)
		{
			FillRoundedRect(graphics, brush, scaleX, scaleY, 186f, 96f, 10f, 52f, 2f);

			using var path = new GraphicsPath(FillMode.Alternate);
			AddLetterGBowl(path, scaleX, scaleY);
			AddScaledRoundedRectangle(path, scaleX, scaleY, 206f, 106f, 30f, 20f, 4f);
			graphics.FillPath(brush, path);

			FillRoundedRect(graphics, brush, scaleX, scaleY, 228f, 122f, 24f, 10f, 2f);
		}

		private static void AddLetterDBowl(GraphicsPath path, float scaleX, float scaleY)
		{
			float left = 196f * scaleX;
			float top = 24f * scaleY;
			float right = 252f * scaleX;
			float bottom = 76f * scaleY;
			float midY = 50f * scaleY;

			path.AddLine(left, top, right - (20f * scaleX), top);
			path.AddArc(right - (40f * scaleX), top, 40f * scaleX, (bottom - top), 270f, 180f);
			path.AddLine(right - (20f * scaleX), bottom, left, bottom);
			path.AddLine(left, bottom, left, top);
			path.CloseFigure();
		}

		private static void AddLetterGBowl(GraphicsPath path, float scaleX, float scaleY)
		{
			float left = 186f * scaleX;
			float top = 96f * scaleY;
			float right = 252f * scaleX;
			float bottom = 148f * scaleY;
			float notchTop = 106f * scaleY;

			path.AddLine(left, top, right - (20f * scaleX), top);
			path.AddArc(right - (40f * scaleX), top, 40f * scaleX, (bottom - top), 270f, 180f);
			path.AddLine(right - (20f * scaleX), bottom, left, bottom);
			path.AddLine(left, bottom, left, notchTop);
			path.AddLine(left, notchTop, right - (36f * scaleX), notchTop);
			path.AddLine(right - (36f * scaleX), notchTop, right - (36f * scaleX), top);
			path.CloseFigure();
		}

		private static void AddScaledRectangle(GraphicsPath path, float scaleX, float scaleY, float x, float y, float width, float height)
		{
			path.AddRectangle(new RectangleF(x * scaleX, y * scaleY, width * scaleX, height * scaleY));
		}

		private static void AddScaledRoundedRectangle(
			GraphicsPath path,
			float scaleX,
			float scaleY,
			float x,
			float y,
			float width,
			float height,
			float radius)
		{
			using var rounded = CreateRoundedRectPath(
				x * scaleX,
				y * scaleY,
				width * scaleX,
				height * scaleY,
				radius * Math.Min(scaleX, scaleY));
			path.AddPath(rounded, false);
		}

		private static GraphicsPath CreateRoundedRectPath(float x, float y, float width, float height, float radius)
		{
			var path = new GraphicsPath();
			float diameter = radius * 2f;

			if (diameter <= 0f || diameter > width || diameter > height)
			{
				path.AddRectangle(new RectangleF(x, y, width, height));
				return path;
			}

			path.AddArc(x, y, diameter, diameter, 180f, 90f);
			path.AddArc(x + width - diameter, y, diameter, diameter, 270f, 90f);
			path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0f, 90f);
			path.AddArc(x, y + height - diameter, diameter, diameter, 90f, 90f);
			path.CloseFigure();
			return path;
		}
	}
}