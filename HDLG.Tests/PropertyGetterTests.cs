using System;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class PropertyGetterTests
    {
        private readonly Mock<ILogger> loggerMock;

        public PropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void ImagePropertyGetter_AddLogger_SetsLogger()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            getter.AddLogger(loggerMock.Object);

            // Assert
            getter.Logger.Should().Be(loggerMock.Object);
        }

        [Fact]
        public void ImagePropertyGetter_AddLogger_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => getter.AddLogger(null!));
        }

        [Theory]
        [InlineData("test.jpg", true)]
        [InlineData("test.png", true)]
        [InlineData("test.gif", true)]
        [InlineData("test.txt", false)]
        [InlineData("test.doc", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("test", false)]
        [InlineData("test.", false)]
        [InlineData("test.JPG", true)]
        [InlineData("test.tar.jpg", true)]
        public void ImagePropertyGetter_IsSupportedFile_ReturnsExpectedResult(string? path, bool expected)
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path!);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("test.mp3", true)]
        [InlineData("test.flac", true)]
        [InlineData("test.wav", true)]
        [InlineData("test.txt", false)]
        [InlineData("test.jpg", false)]
        public void Mp3PropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsProperties()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test.mp3");

            // Assert
            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test Title");
            properties.Should().ContainKey("Album");
            properties["Album"].Should().Be("Test Album");
            properties.Should().ContainKey("Year");
            properties["Year"].Should().Be(2023u);
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_FileNotFound_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("nonexistent.mp3");

            // Assert
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file"))), Times.Once);
            properties.Should().BeEmpty();
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();
            getter.AddLogger(loggerMock.Object);
            var invalidFile = "test_invalid.mp3";
            System.IO.File.WriteAllText(invalidFile, "invalid mp3 content");

            try
            {
                // Act
                var properties = getter.GetFileProperties(invalidFile);

                // Assert
                loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("is corrupted") || s.Contains("is not supported") || s.Contains("Cannot read properties from file"))), Times.Once);
                properties.Should().BeEmpty();
            }
            finally
            {
                if (System.IO.File.Exists(invalidFile))
                {
                    System.IO.File.Delete(invalidFile);
                }
            }
        }



        [Fact]
        public void PdfPropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsTitle()
        {
            // Arrange
            var getter = new PdfPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test.pdf");

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
            var properties = getter.GetFileProperties("test_empty_title.pdf");

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
            var properties = getter.GetFileProperties("nonexistent.pdf");

            // Assert

            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file"))), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_invalid.pdf");

            // Assert

            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file"))), Times.Once);
        }

        [Fact]
        public void PdfPropertyGetter_GetFileProperties_EncryptedFile_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new PdfPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_encrypted.pdf");

            // Assert
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("password protected"))), Times.Once);
        }

        [Theory]
        [InlineData("test.pdf", true)]
        [InlineData("test.txt", false)]
        public void PdfPropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new PdfPropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

            // Assert
            result.Should().Be(expected);
        }


        [Fact]
        public void WordPropertyGetter_GetFileProperties_ValidFileWithProperties_ReturnsProperties()
        {
            // Arrange
            var getter = new WordPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test_word.docx");

            // Assert
            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test Title");
            properties.Should().ContainKey("Creator");
            properties["Creator"].Should().Be("Test Creator");
            properties.Should().ContainKey("Created");
        }

        [Fact]
        public void WordPropertyGetter_GetFileProperties_ValidFileWithoutProperties_ReturnsEmptyDictionary()
        {
            // Arrange
            var getter = new WordPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test_word_empty.docx");

            // Assert
            properties.Should().BeEmpty();
        }

        [Fact]
        public void WordPropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new WordPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("nonexistent_word.docx");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Word file"))), Times.Once);
        }

        [Fact]
        public void WordPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new WordPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_word_invalid.docx");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Word file"))), Times.Once);
        }

        [Theory]
        [InlineData("test.doc", false)]
        [InlineData("test.docx", true)]
        [InlineData("test.txt", false)]
        public void WordPropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new WordPropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

            // Assert
            result.Should().Be(expected);
        }


        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsTitle()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test.xlsx");

            // Assert
            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test Excel Title");
            properties.Should().ContainKey("Creator");
            properties["Creator"].Should().Be("Test Creator");
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithoutProperties_ReturnsEmptyDictionary()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test_empty.xlsx");

            // Assert
            properties.Should().NotContainKey("Title");
            properties.Should().BeEmpty();
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("nonexistent.xlsx");

            // Assert
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file"))), Times.Once);
            properties.Should().BeEmpty();
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_invalid.xlsx");

            // Assert
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file"))), Times.Once);
            properties.Should().BeEmpty();
        }

        [Theory]
        [InlineData("test.xls", false)]
        [InlineData("test.xlsx", true)]
        [InlineData("test.txt", false)]
        public void ExcelPropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

            // Assert
            result.Should().Be(expected);
        }
    }
}
