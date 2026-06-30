using FluentAssertions;
using HDLG_winforms;
using System.Drawing;

namespace HDLG.Tests
{
	[Collection(nameof(WinFormsUiTestCollection))]
	public class AppLogoRendererTests
	{
		[Fact]
		public void LoadLogoImage_ProducesColoredPixels()
		{
			using var bitmap = (Bitmap)AppBranding.LoadLogoImage();

			bitmap.Width.Should().BeGreaterThanOrEqualTo(320);
			bitmap.Height.Should().BeGreaterThanOrEqualTo(160);
			CountAccentPixels(bitmap).Should().BeGreaterThan(100);
		}

		[Fact]
		public void LoadApplicationIcon_ReturnsValidIcon()
		{
			using var icon = AppBranding.LoadApplicationIcon();

			icon.Width.Should().BeGreaterThan(0);
			icon.Height.Should().BeGreaterThan(0);
		}

		[Fact]
		public void ExportPackagedLogoAssets_WhenRequested()
		{
			if (!string.Equals(Environment.GetEnvironmentVariable("HDLG_EXPORT_LOGO"), "1", StringComparison.Ordinal))
			{
				return;
			}

			string assetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HDLG winforms", "Assets"));
			System.IO.Directory.CreateDirectory(assetsDir);

			using (var logo = (Bitmap)AppBranding.LoadLogoImage())
			{
				logo.Save(Path.Combine(assetsDir, "hdlg-logo.png"), System.Drawing.Imaging.ImageFormat.Png);
			}

			using (var icon = AppBranding.LoadApplicationIcon())
			using (var stream = System.IO.File.OpenWrite(Path.Combine(assetsDir, "hdlg-icon.ico")))
			{
				icon.Save(stream);
			}
		}

		private static int CountAccentPixels(Bitmap bitmap)
		{
			int count = 0;
			var accent = Color.FromArgb(255, 2, 132, 200);

			for (int y = 0; y < bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					if (bitmap.GetPixel(x, y).ToArgb() == accent.ToArgb())
					{
						count++;
					}
				}
			}

			return count;
		}
	}
}