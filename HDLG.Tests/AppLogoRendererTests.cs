using FluentAssertions;
using HDLG_winforms;
using System.Drawing;

namespace HDLG.Tests
{
	[Collection( nameof( WinFormsUiTestCollection ) )]
	public class AppLogoRendererTests
	{
		[Fact]
		public void LoadLogoImage_ProducesColoredPixels ()
		{
			using var bitmap = (Bitmap)AppBranding.LoadLogoImage( );

			bitmap.Width.Should( ).BeGreaterThanOrEqualTo( 320 );
			bitmap.Height.Should( ).BeGreaterThanOrEqualTo( 160 );
			CountAccentPixels( bitmap ).Should( ).BeGreaterThan( 100 );
		}

		[Fact]
		public void LoadApplicationIcon_ReturnsValidIcon ()
		{
			using var icon = AppBranding.LoadApplicationIcon( );

			icon.Width.Should( ).BeGreaterThan( 0 );
			icon.Height.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public void CreateApplicationIcon_ReturnsIcon ()
		{
			using var icon = AppLogoRenderer.CreateApplicationIcon( );

			icon.Should( ).NotBeNull( );
			icon.Width.Should( ).BeGreaterThan( 0 );
			icon.Height.Should( ).BeGreaterThan( 0 );

			// Verify we can access different sizes
			using var icon16 = new Icon( icon, new Size( 16, 16 ) );
			icon16.Width.Should( ).Be( 16 );
			icon16.Height.Should( ).Be( 16 );
		}

		[Theory]
		[InlineData( 16 )]
		[InlineData( 32 )]
		[InlineData( 256 )]
		public void RenderIcon_ProducesBitmapOfRequestedSize (int size)
		{
			using var bitmap = AppLogoRenderer.RenderIcon( size );

			bitmap.Should( ).NotBeNull( );
			bitmap.Width.Should( ).Be( size );
			bitmap.Height.Should( ).Be( size );
			bitmap.PixelFormat.Should( ).Be( System.Drawing.Imaging.PixelFormat.Format32bppArgb );
		}

		[Fact]
		public void RenderIcon_ThrowsForInvalidSize ()
		{
			Action act = () => AppLogoRenderer.RenderIcon( 0 );
			act.Should( ).Throw<ArgumentOutOfRangeException>( );
		}

		[Fact]
		public void RenderWordmark_ProducesBitmapOfRequestedSize ()
		{
			using var bitmap = AppLogoRenderer.RenderWordmark( 320, 160 );

			bitmap.Should( ).NotBeNull( );
			bitmap.Width.Should( ).Be( 320 );
			bitmap.Height.Should( ).Be( 160 );
		}

		[Theory]
		[InlineData( 0, 100 )]
		[InlineData( 100, 0 )]
		[InlineData( -1, 100 )]
		public void RenderWordmark_ThrowsForInvalidDimensions (int width, int height)
		{
			Action act = () => AppLogoRenderer.RenderWordmark( width, height );
			act.Should( ).Throw<ArgumentOutOfRangeException>( );
		}

		[Fact]
		public void ExportPackagedLogoAssets_WhenRequested ()
		{
			if (!string.Equals( Environment.GetEnvironmentVariable( "HDLG_EXPORT_LOGO" ), "1", StringComparison.Ordinal ))
			{
				return;
			}

			string assetsDir = Path.GetFullPath( Path.Combine( AppContext.BaseDirectory, "..", "..", "..", "..", "HDLG winforms", "Assets" ) );
			System.IO.Directory.CreateDirectory( assetsDir );

			using (var logo = (Bitmap)AppBranding.LoadLogoImage( ))
			{
				logo.Save( Path.Combine( assetsDir, "hdlg-logo.png" ), System.Drawing.Imaging.ImageFormat.Png );
			}

			using (var icon = AppBranding.LoadApplicationIcon( ))
			using (var stream = System.IO.File.OpenWrite( Path.Combine( assetsDir, "hdlg-icon.ico" ) ))
			{
				icon.Save( stream );
			}
		}

		private static int CountAccentPixels (Bitmap bitmap)
		{
			int count = 0;
			var accent = Color.FromArgb( 255, 2, 132, 200 );

			for (int y = 0; y < bitmap.Height; y++)
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					if (bitmap.GetPixel( x, y ).ToArgb( ) == accent.ToArgb( ))
					{
						count++;
					}
				}
			}

			return count;
		}
	}
}
