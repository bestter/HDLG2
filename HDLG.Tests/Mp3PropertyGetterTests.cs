using System;
using System.IO;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class Mp3PropertyGetterTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;

        public Mp3PropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            Mp3Setup.CreateMp3Docs();
        }

        public void Dispose()
        {
            Mp3Setup.Cleanup();
        }

        [Fact]
        public void Mp3PropertyGetter_AddLogger_SetsLogger()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act
            getter.AddLogger(loggerMock.Object);

            // Assert
            getter.Logger.Should().BeSameAs(loggerMock.Object);
        }

        [Fact]
        public void Mp3PropertyGetter_AddLogger_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => getter.AddLogger(null!));
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_AllProperties_ReturnsCompleteDictionary()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();
            var testFile = "test_full.mp3";

            // Create a test file with all properties filled to verify they are extracted correctly
            System.IO.File.Copy("test.mp3", testFile, true);
            using (var f = TagLib.File.Create(testFile))
            {
                f.Tag.Title = "Test Complete Title";
                f.Tag.Album = "Test Complete Album";
                f.Tag.Year = 2024;
                f.Tag.Performers = new[] { "John Doe", "Jane Doe" };
                f.Tag.AlbumArtists = new[] { "Band A", "Band B" };
                f.Tag.Composers = new[] { "Mozart", "Beethoven" };
                f.Tag.Copyright = "2024 Test Corp";
                f.Save();
            }

            try
            {
                // Act
                var properties = getter.GetFileProperties(new FileInfo(testFile));

                // Assert
                properties.Should().ContainKey("Title");
                properties["Title"].Should().Be("Test Complete Title");

                properties.Should().ContainKey("Duration");
                // Duration format checks can be tricky due to locale, so we just verify it exists
                // and has a reasonable value type
                properties["Duration"].Should().NotBeNull();

                properties.Should().ContainKey("Album");
                properties["Album"].Should().Be("Test Complete Album");

                properties.Should().ContainKey("Year");
                properties["Year"].Should().Be(2024u);

                properties.Should().ContainKey("Performers");
                properties["Performers"].Should().Be("John Doe, Jane Doe");

                properties.Should().ContainKey("AlbumArtists");
                properties["AlbumArtists"].Should().Be("Band A, Band B");

                properties.Should().ContainKey("Composers");
                properties["Composers"].Should().Be("Mozart, Beethoven");

                properties.Should().ContainKey("Copyright");
                properties["Copyright"].Should().Be("2024 Test Corp");
            }
            finally
            {
                if (System.IO.File.Exists(testFile))
                {
                    System.IO.File.Delete(testFile);
                }
            }
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
            var testFile = "test_valid_title.mp3";

            System.IO.File.Copy("test.mp3", testFile, true);
            using (var f = TagLib.File.Create(testFile))
            {
                f.Tag.Title = "Test Title";
                f.Tag.Album = "Test Album";
                f.Tag.Year = 0;
                f.Save();
            }

            try
            {
                // Act
                var properties = getter.GetFileProperties(new FileInfo(testFile));

                // Assert
                properties.Should().ContainKey("Title");
                properties["Title"].Should().Be("Test Title");
                properties.Should().ContainKey("Album");
                properties["Album"].Should().Be("Test Album");
                properties.Should().ContainKey("Year");
                properties["Year"].Should().Be(0u);
            }
            finally
            {
                if (System.IO.File.Exists(testFile))
                {
                    System.IO.File.Delete(testFile);
                }
            }
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_FileNotFound_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("nonexistent.mp3"));

            // Assert
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file")), It.IsAny<string>()), Times.Once);
            properties.Should().BeEmpty();
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();
            getter.AddLogger(loggerMock.Object);
            // using the invalid file from setup instead of creating it inline
            var invalidFile = "test_invalid.mp3";

            try
            {
                // Act
                var properties = getter.GetFileProperties(new FileInfo(invalidFile));

                // Assert
                loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("is corrupted") || s.Contains("is not supported") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
                properties.Should().BeEmpty();
            }
            finally
            {
                // No need to delete, Cleanup handles it or it stays for next test
            }
        }
    }
}
