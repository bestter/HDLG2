using FluentAssertions;
using HDLG_winforms;

namespace HDLG.Tests
{
    [Collection(nameof(WinFormsUiTestCollection))]
    public class AppUiBootstrapTests
    {
        [Fact]
        public void Configure_InitializesActiveTheme()
        {
            AppUiBootstrap.Configure();

            AppUiBootstrap.ActiveTheme.Should().Be("MinimalistSlate");
        }

        [Fact]
        public void RemoveFormBranding_ConfiguresFormTheme()
        {
            AppUiBootstrap.Configure();
            using var form = new Credit();

            form.BackColor.Should().Be(System.Drawing.Color.FromArgb(248, 250, 252));
            form.BackgroundImage.Should().BeNull();
        }
    }
}