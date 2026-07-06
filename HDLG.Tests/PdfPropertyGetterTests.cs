using System;
using System.IO;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class PdfPropertyGetterTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;

        public PdfPropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            PdfSetup.CreatePdfDocs();
        }

        public void Dispose()
        {
            PdfSetup.Cleanup();
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsTitle()
        {
            // Arrange
            var getter = new PdfPropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test.pdf"));

            // Assert
            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test PDF Title");
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_ValidFileWithoutTitle_ReturnsEmptyDictionary()
        {
            // Arrange
            var getter = new PdfPropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_empty_title.pdf"));

            // Assert
            properties.Should().NotContainKey("Title");
            properties.Should().BeEmpty();
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_FileNotFound_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("nonexistent.pdf"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_invalid.pdf"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_EncryptedFile_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_encrypted.pdf"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("password protected")), It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("test.pdf", true)]
        [InlineData("test.txt", false)]
        public void PdfPropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new PdfPropertyGetter();

            // Act
            var result = getter.IsSupportedFile(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path));

            // Assert
            result.Should().Be(expected);
        }
    }
}
