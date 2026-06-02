using System;
using System.Collections.Generic;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;
using System.Globalization;

namespace HDLG.Tests
{
    public class FilePropertyBrowserTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFilePropertyGetter> propertyGetterMock1;
        private readonly Mock<IFilePropertyGetter> propertyGetterMock2;

        public FilePropertyBrowserTests()
        {
            loggerMock = new Mock<ILogger>();
            propertyGetterMock1 = new Mock<IFilePropertyGetter>();
            propertyGetterMock2 = new Mock<IFilePropertyGetter>();
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FilePropertyBrowser(null!, propertyGetterMock1.Object));
        }

        [Fact]
        public void Constructor_NullPropertyGetters_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FilePropertyBrowser(loggerMock.Object, null!));
        }

        [Fact]
        public void Constructor_ValidArguments_AddsLoggerToGetters()
        {
            // Act
            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object, propertyGetterMock2.Object);

            // Assert
            propertyGetterMock1.Verify(g => g.AddLogger(loggerMock.Object), Times.Once);
            propertyGetterMock2.Verify(g => g.AddLogger(loggerMock.Object), Times.Once);
        }

        [Fact]
        public void GetFileProperty_NullOrWhiteSpacePath_ThrowsArgumentException()
        {
            // Arrange
            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => browser.GetFileProperty(null!));
            Assert.Throws<ArgumentException>(() => browser.GetFileProperty(""));
            Assert.Throws<ArgumentException>(() => browser.GetFileProperty("   "));
        }

        [Fact]
        public void GetFileProperty_PathSupportedByOneGetter_ReturnsPropertiesFromThatGetter()
        {
            // Arrange
            string path = "test.jpg";
            var expectedProperties = new Dictionary<string, IConvertible> { { "Width", 1920 } };

            propertyGetterMock1.Setup(g => g.IsSupportedFile(path)).Returns(true);
            propertyGetterMock1.Setup(g => g.GetFileProperties(path)).Returns(expectedProperties);

            propertyGetterMock2.Setup(g => g.IsSupportedFile(path)).Returns(false);

            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object, propertyGetterMock2.Object);

            // Act
            var result = browser.GetFileProperty(path);

            // Assert
            result.Should().BeEquivalentTo(expectedProperties);
            propertyGetterMock1.Verify(g => g.GetFileProperties(path), Times.Once);
            propertyGetterMock2.Verify(g => g.GetFileProperties(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetFileProperty_PathSupportedByMultipleGetters_CombinesProperties()
        {
            // Arrange
            string path = "test.document";
            var properties1 = new Dictionary<string, IConvertible> { { "Author", "John Doe" } };
            var properties2 = new Dictionary<string, IConvertible> { { "WordCount", 500 } };

            propertyGetterMock1.Setup(g => g.IsSupportedFile(path)).Returns(true);
            propertyGetterMock1.Setup(g => g.GetFileProperties(path)).Returns(properties1);

            propertyGetterMock2.Setup(g => g.IsSupportedFile(path)).Returns(true);
            propertyGetterMock2.Setup(g => g.GetFileProperties(path)).Returns(properties2);

            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object, propertyGetterMock2.Object);

            // Act
            var result = browser.GetFileProperty(path);

            // Assert
            result.Should().ContainKey("Author").WhoseValue.Should().Be("John Doe");
            result.Should().ContainKey("WordCount").WhoseValue.Should().Be(500);
            result.Count.Should().Be(2);
        }

        [Fact]
        public void LogGetterStatistics_NoFilesProcessed_DoesNotLogAverages()
        {
            // Arrange
            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object);

            // Act
            browser.LogGetterStatistics();

            // Assert
            // It should only log the total files line, which is 0
            loggerMock.Verify(l => l.Information("Total number of files {TotalNumberOfFiles}", 0L), Times.Once);
        }

        [Fact]
        public void LogGetterStatistics_FilesProcessed_LogsAveragesAndTotal()
        {
            // Arrange
            string path = "test.file";
            propertyGetterMock1.Setup(g => g.IsSupportedFile(path)).Returns(true);
            propertyGetterMock1.Setup(g => g.GetFileProperties(path)).Returns(new Dictionary<string, IConvertible>());

            var browser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock1.Object);
            browser.GetFileProperty(path);

            // Act
            browser.LogGetterStatistics();

            // Assert
            loggerMock.Verify(l => l.Information(
                "{PropertyGetterType} total runtime: {TotalExecutionTime}. Number of files: {TotalFiles}. Average: {AverageTime}",
                propertyGetterMock1.Object.GetType(),
                It.IsAny<string>(), // TotalExecutionTime
                1L, // TotalFiles
                It.IsAny<string>() // AverageTime
            ), Times.Once);

            loggerMock.Verify(l => l.Information("Total number of files {TotalNumberOfFiles}", 1L), Times.Once);
        }
    }
}
