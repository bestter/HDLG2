using System;
using FluentAssertions;
using HDLG_winforms;
using Xunit;

namespace HDLG.Tests
{
    public class OpenUrlSafeTests
    {
        [Fact]
        public void OpenUrlSafe_NullUri_ThrowsArgumentNullException()
        {
            var act = () => MainWindow.OpenUrlSafe(null!, _ => { }, _ => true);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void OpenUrlSafe_NullProcessStarter_ThrowsArgumentNullException()
        {
            var uri = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html");
            var act = () => MainWindow.OpenUrlSafe(uri, null!, _ => true);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("file:///C:/Windows/System32/calc.exe")]
        [InlineData("javascript:alert(1)")]
        [InlineData("httpfoo://example.com")]
        [InlineData("ftp://example.com")]
        public void OpenUrlSafe_NonHttpScheme_ThrowsInvalidOperationException(string url)
        {
            var uri = new Uri(url, UriKind.Absolute);
            var act = () => MainWindow.OpenUrlSafe(uri, _ => { }, _ => true);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*http/https*");
        }

        [Fact]
        public void OpenUrlSafe_RelativeUri_ThrowsInvalidOperationException()
        {
            var uri = new Uri("licenses/gpl-3.0.en.html", UriKind.Relative);
            var act = () => MainWindow.OpenUrlSafe(uri, _ => { }, _ => true);
            act.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData("http://example.com/")]
        [InlineData("https://www.gnu.org/licenses/gpl-3.0.en.html")]
        public void OpenUrlSafe_HttpOrHttps_LaunchesAbsoluteUri(string url)
        {
            var uri = new Uri(url);
            string? launched = null;
            MainWindow.OpenUrlSafe(uri, target => launched = target, _ => true);
            launched.Should().Be(uri.AbsoluteUri);
        }

        [Fact]
        public void OpenUrlSafe_HttpsWithUppercaseScheme_LaunchesNormalizedAbsoluteUri()
        {
            var uri = new Uri("HTTPS://WWW.GNU.ORG/licenses/gpl-3.0.en.html");
            string? launched = null;
            string? prompted = null;
            MainWindow.OpenUrlSafe(uri, target => launched = target, shown =>
            {
                prompted = shown;
                return true;
            });
            launched.Should().Be(uri.AbsoluteUri);
            prompted.Should().Be(uri.AbsoluteUri);
            prompted.Should().StartWith("https://");
        }

        [Fact]
        public void OpenUrlSafe_PromptDeclined_DoesNotLaunch()
        {
            bool launched = false;
            MainWindow.OpenUrlSafe(
                new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html"),
                _ => launched = true,
                _ => false);
            launched.Should().BeFalse();
        }
    }
}
