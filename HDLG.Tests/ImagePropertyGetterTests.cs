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
            var files = new[]
            {
                "test.jpg",
                "test_no_exif.jpg",
                "test.png",
                "test_invalid.jpg",
                "test_corrupted.png",
                "test_valid_exif.jpg",
                "test_valid_no_exif.jpg",
                "test_corrupted.jpg"
            };

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithExif_ReturnsProperties()
        {
            // Arrange
            var getter = new ImagePropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test_valid_exif.jpg");

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
            var properties = getter.GetFileProperties("test_valid_no_exif.jpg");

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
            var properties = getter.GetFileProperties("test.png");

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
            var properties = getter.GetFileProperties("test_invalid.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Unsupported image format for file: {FilePath}", "test_invalid.jpg"), Times.Once);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_CorruptedImage_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_corrupted.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Invalid image content for file: {FilePath}", "test_corrupted.jpg"), Times.Once);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("nonexistent_image.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), "Cannot read properties from file: {FilePath}", "nonexistent_image.jpg"), Times.Once);
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
