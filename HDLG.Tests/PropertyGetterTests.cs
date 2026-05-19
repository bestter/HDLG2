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
        public void ImagePropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

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

        [Theory]
        [InlineData("test.doc", true)]
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

        [Theory]
        [InlineData("test.xls", true)]
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
