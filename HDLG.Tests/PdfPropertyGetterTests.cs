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
            PdfPropertyGetterTestSetup.CreatePdfDocs();
        }

        public void Dispose()
        {
            PdfPropertyGetterTestSetup.Cleanup();
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsProperties()
        {
            var getter = new PdfPropertyGetter();
            var properties = getter.GetFileProperties(new FileInfo("test_pdf.pdf"));

            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test Title");
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_ValidFileEmpty_ReturnsEmptyDictionary()
        {
            var getter = new PdfPropertyGetter();
            var properties = getter.GetFileProperties(new FileInfo("test_pdf_empty.pdf"));

            properties.Should().BeEmpty();
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            var properties = getter.GetFileProperties(new FileInfo("nonexistent_pdf.pdf"));

            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            var properties = getter.GetFileProperties(new FileInfo("test_pdf_invalid.pdf"));

            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_EncryptedFile_LogsWarningAndReturnsEmpty()
        {
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            var properties = getter.GetFileProperties(new FileInfo("test_pdf_encrypted.pdf"));

            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("password protected")), It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("test.pdf", true)]
        [InlineData("test.PDF", true)]
        [InlineData("test.txt", false)]
        [InlineData("test.docx", false)]
        public void PdfPropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            var getter = new PdfPropertyGetter();
            var result = getter.IsSupportedFile(new FileInfo(path));
            result.Should().Be(expected);
        }
    }
}
