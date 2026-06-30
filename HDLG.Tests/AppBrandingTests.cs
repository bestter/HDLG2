using FluentAssertions;
using HDLG_winforms;

namespace HDLG.Tests
{
	public class AppBrandingTests
	{
		[Fact]
		public void InlineLogoSvgMarkup_ContainsConceptCMarkWithoutGridLines()
		{
			string svg = AppBranding.InlineLogoSvgMarkup;

			svg.Should().Contain("viewBox=\"0 0 320 160\"");
			svg.Should().Contain("fill=\"#0284C8\"");
			svg.Should().NotContain("<line");
			svg.Should().NotContain("stroke=");
		}

		[Fact]
		public void GetHtmlFooterMarkup_IncludesLogoAndVersion()
		{
			string footer = AppBranding.GetHtmlFooterMarkup("1.4.0.0");

			footer.Should().Contain("class=\"hdlg-footer\"");
			footer.Should().Contain("class=\"hdlg-footer-logo\"");
			footer.Should().Contain("<svg");
			footer.Should().Contain("HTML Directory List Generator");
			footer.Should().Contain("1.4.0.0");
		}
	}
}