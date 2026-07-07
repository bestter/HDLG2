using System;
using System.IO;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class ExcelPropertyGetterTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;

        public ExcelPropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            ExcelSetup.CreateExcelDocs();
        }

        public void Dispose()
        {
            ExcelSetup.Cleanup();
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithProperties_ReturnsProperties()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_excel.xlsx"));

            // Assert
            properties.Should().ContainKey("Title");
            properties["Title"].Should().Be("Test Title");
            properties.Should().ContainKey("Creator");
            properties["Creator"].Should().Be("Test Creator");
            properties.Should().ContainKey("Created");
            properties["Created"].Should().BeOfType<DateTime>();
            ((DateTime)properties["Created"]).ToUniversalTime().Should().BeCloseTo(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithoutProperties_ReturnsEmptyDictionary()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_excel_empty.xlsx"));

            // Assert
            properties.Should().BeEmpty();
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("nonexistent_excel.xlsx"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_excel_invalid.xlsx"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
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
            var result = getter.IsSupportedFile(new FileInfo(path));

            // Assert
            result.Should().Be(expected);
        }
    }
}
