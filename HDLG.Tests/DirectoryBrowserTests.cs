using System;
using System.IO;
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
            htmlContent.Should().Contain($"<h2>{testDirectory.Path}</h2>");
            htmlContent.Should().Contain("</html>");
        }
    }
}
