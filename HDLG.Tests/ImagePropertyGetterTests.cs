using System;
using System.IO;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class ImagePropertyGetterTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;

        public ImagePropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            ImageSetup.CreateImages();
        }

        public void Dispose()
        {
            // Do not delete files during parallel test execution to prevent race conditions.
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithExif_ReturnsProperties()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_valid_exif.jpg"));

            // Assert
            properties.Should().ContainKey("Width");
            properties["Width"].Should().Be(100);
            properties.Should().ContainKey("Height");
            properties["Height"].Should().Be(50);
            properties.Should().ContainKey("CameraModel");
            properties["CameraModel"].Should().Be("TestCameraModel");
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidFileNoExif_ReturnsPropertiesWithoutCameraModel()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_valid_no_exif.jpg"));

            // Assert
            properties.Should().ContainKey("Width");
            properties["Width"].Should().Be(100);
            properties.Should().ContainKey("Height");
            properties["Height"].Should().Be(50);
            properties.Should().NotContainKey("CameraModel");
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidPng_ReturnsProperties()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test.png"));

            // Assert
            properties.Should().ContainKey("Width");
            properties["Width"].Should().Be(1);
            properties.Should().ContainKey("Height");
            properties["Height"].Should().Be(1);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_InvalidFile_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_invalid.jpg"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Unsupported image format for file: {FilePath}", It.Is<string>(s => s.EndsWith("test_invalid.jpg"))), Times.Once);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_CorruptedImage_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test_corrupted.jpg"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Invalid image content for file: {FilePath}", It.Is<string>(s => s.EndsWith("test_corrupted.jpg"))), Times.Once);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("nonexistent_image.jpg"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Cannot read properties from file: {FilePath}", It.Is<string>(s => s.EndsWith("nonexistent_image.jpg"))), Times.Once);
        }

        [Theory]
        [InlineData("test.jpg", true)]
        [InlineData("test.jpeg", true)]
        [InlineData("test.png", true)]
        [InlineData("test.gif", true)]
        [InlineData("test.bmp", true)]
        [InlineData("test.webp", true)]
        [InlineData("test.tiff", true)]
        [InlineData("test.dds", true)]
        [InlineData("test.qoi", true)]
        [InlineData("test.txt", false)]
        [InlineData("test.mp3", false)]
        [InlineData("", false)]
        [InlineData("test_no_extension", false)]
        public void ImagePropertyGetter_IsSupportedFile_ReturnsExpectedResult(string path, bool expected)
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var result = getter.IsSupportedFile(path);

            // Assert
            result.Should().Be(expected);
        }
    }
}
