using System;
using System.IO;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class WordPropertyGetterTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;

        public WordPropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            WordSetup.CreateWordDocs();
        }

        public void Dispose()
        {
            WordSetup.Cleanup();
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
            properties["Created"].Should().BeOfType<DateTime>();
            ((DateTime)properties["Created"]).Should().BeCloseTo(new DateTime(2023, 1, 1, 12, 0, 0), TimeSpan.FromSeconds(1));
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
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Word file") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
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
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Word file") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
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
    }
}
