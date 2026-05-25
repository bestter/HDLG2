using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using HDLG_winforms;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class DirectoryBrowserTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly DirectoryBrowser directoryBrowser;
        private readonly string tempXmlFilePath;
        private readonly string tempHtmlFilePath;
        private readonly HdlgDirectory testDirectory;
        private readonly string baseDirectoryPath;

        public DirectoryBrowserTests()
        {
            loggerMock = new Mock<ILogger>();
            directoryBrowser = new DirectoryBrowser(loggerMock.Object);

            tempXmlFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".xml");
            tempHtmlFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".html");

            baseDirectoryPath = Path.Combine(Path.GetTempPath(), "DirectoryBrowserTests_" + Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(baseDirectoryPath);

            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "content");

            testDirectory = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);
        }

        public void Dispose()
        {
            if (System.IO.File.Exists(tempXmlFilePath))
                System.IO.File.Delete(tempXmlFilePath);

            if (System.IO.File.Exists(tempHtmlFilePath))
                System.IO.File.Delete(tempHtmlFilePath);

            if (System.IO.Directory.Exists(baseDirectoryPath))
                System.IO.Directory.Delete(baseDirectoryPath, true);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectoryBrowser(null!));
        }

        [Fact]
        public async Task SaveAsXMLAsync_NullFilePath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsXMLAsync(null!, testDirectory));
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsXMLAsync("", testDirectory));
        }

        [Fact]
        public async Task SaveAsXMLAsync_NullDirectory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => directoryBrowser.SaveAsXMLAsync(tempXmlFilePath, null!));
        }

        [Fact]
        public async Task SaveAsXMLAsync_ValidInputs_GeneratesXmlFile()
        {
            // Act
            await directoryBrowser.SaveAsXMLAsync(tempXmlFilePath, testDirectory);

            // Assert
            System.IO.File.Exists(tempXmlFilePath).Should().BeTrue();

            var xmlContent = await System.IO.File.ReadAllTextAsync(tempXmlFilePath);
            xmlContent.Should().Contain("<Hdlg");
            xmlContent.Should().Contain($"<Directory>{testDirectory.Path}</Directory>");
            xmlContent.Should().Contain("</Hdlg>");
        }

        [Fact]
        public async Task SaveAsHTMLAsync_NullFilePath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsHTMLAsync(null!, testDirectory));
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsHTMLAsync("", testDirectory));
        }

        [Fact]
        public async Task SaveAsHTMLAsync_NullDirectory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => directoryBrowser.SaveAsHTMLAsync(tempHtmlFilePath, null!));
        }

        [Fact]
        public async Task SaveAsHTMLAsync_ValidInputs_GeneratesHtmlFile()
        {
            // Act
            await directoryBrowser.SaveAsHTMLAsync(tempHtmlFilePath, testDirectory);

            // Assert
            System.IO.File.Exists(tempHtmlFilePath).Should().BeTrue();

            var htmlContent = await System.IO.File.ReadAllTextAsync(tempHtmlFilePath);
            htmlContent.Should().Contain("<!DOCTYPE html>");
            htmlContent.Should().Contain($"<html lang=\"{System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName}\">");
            htmlContent.Should().Contain($"<h2>{WebUtility.HtmlEncode( testDirectory.Path )}</h2>");
            htmlContent.Should().Contain("</html>");

            // 2026 modern responsive HTML assertions (updated for prettier redesign using details/summary + clean cards)
            htmlContent.Should().Contain("<details class=\"directory\"");
            htmlContent.Should().Contain("<summary>");
            htmlContent.Should().Contain("class=\"hdlg\"");
            // Note: .file markup only emitted when the HdlgDirectory instance contains files (this basic test setup does not populate via Browse)
        }

        [Fact]
        public async Task SaveAsHTMLAsync_DirectoryWithSpecialChars_EncodesHtmlContent()
        {
            // Arrange — create a directory with HTML-dangerous characters in the name
            var dangerousDirName = "test&dir<xss>";
            var dangerousDirPath = Path.Combine(baseDirectoryPath, dangerousDirName);

            // On Windows, < and > are not valid directory name chars, so use only &
            var safeDangerousDirName = "test&dir";
            var safeDangerousDirPath = Path.Combine(baseDirectoryPath, safeDangerousDirName);
            System.IO.Directory.CreateDirectory(safeDangerousDirPath);
            System.IO.File.WriteAllText(Path.Combine(safeDangerousDirPath, "a&b.txt"), "content");

            var dirWithSubDirs = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            var browser = new HdlgFileProperty.FilePropertyBrowser(loggerMock.Object);
            dirWithSubDirs.Browse(browser);

            var htmlPath = Path.Combine(Path.GetTempPath(), "test_xss_" + Guid.NewGuid().ToString() + ".html");

            try
            {
                // Act
                await directoryBrowser.SaveAsHTMLAsync(htmlPath, dirWithSubDirs);

                // Assert
                var htmlContent = await System.IO.File.ReadAllTextAsync(htmlPath);

                // The & character must be encoded as &amp; in the HTML output (including in new title= hover popup attrs)
                htmlContent.Should().Contain(WebUtility.HtmlEncode(safeDangerousDirName));
                htmlContent.Should().NotContain($"<span class=\"name\" title=\"{safeDangerousDirName}\">");
                htmlContent.Should().Contain($"<span class=\"name\" title=\"{WebUtility.HtmlEncode( safeDangerousDirName )}\">");

                // Long name handling: title= provides the full name for native hover popup (both in tree and TOC).
                // TOC ("main directory menu") uses 23ch max-width + ellipsis (reduced by 3ch for better fit).
                htmlContent.Should().Contain($"title=\"{WebUtility.HtmlEncode( safeDangerousDirName )}\"");
                htmlContent.Should().Contain($"<a href=\"#{WebUtility.HtmlEncode( baseDirectoryPath )}\" title=\"{WebUtility.HtmlEncode( System.IO.Path.GetFileName( baseDirectoryPath ) )}\">");

                // File name with & must also be encoded
                htmlContent.Should().Contain(WebUtility.HtmlEncode("a&b.txt"));
            }
            finally
            {
                if (System.IO.File.Exists(htmlPath))
                    System.IO.File.Delete(htmlPath);
            }
        }
    }
}
