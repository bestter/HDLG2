using FluentAssertions;
using HDLG_winforms;
using Krypton.Toolkit;

namespace HDLG.Tests
{
    [Collection(nameof(WinFormsUiTestCollection))]
    public class AppUiBootstrapTests
    {
        [Fact]
        public void Configure_SetsMicrosoft365BlueLightPalette()
        {
            AppUiBootstrap.Configure();

            AppUiBootstrap.ActivePaletteMode.Should().Be(PaletteMode.Microsoft365BlueLightMode);
        }

        [Fact]
        public void RemoveFormBranding_ClearsKryptonWatermarkImage()
        {
            AppUiBootstrap.Configure();
            using var form = new Credit();

            form.StateCommon!.Back!.Image.Should().BeNull();
            form.StateActive!.Back!.Image.Should().BeNull();
            form.StateInactive!.Back!.Image.Should().BeNull();
            form.BackgroundImage.Should().BeNull();
        }
    }
}