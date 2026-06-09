using DocumentFormat.OpenXml.Packaging;
using System;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class PropertyGetterTests : IDisposable
    {
        public void Dispose()
        {
            WordSetup.Cleanup();
        }

        private readonly Mock<ILogger> loggerMock;

        public PropertyGetterTests()
        {
            loggerMock = new Mock<ILogger>();
            ImageSetup.CreateImages();
            WordSetup.CreateWordDocs();
            WordPropertyGetterTestSetup.EnsureTestFilesExist();
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


        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithExif_ReturnsPropertiesAndCameraModel()
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
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithoutExif_ReturnsPropertiesWithoutCameraModel()
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
        public void ImagePropertyGetter_GetFileProperties_ValidPngFile_ReturnsProperties()
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
        public void ImagePropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("nonexistent.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public void ImagePropertyGetter_GetFileProperties_InvalidFileFormat_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_invalid.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Unsupported image format") || s.Contains("Cannot read properties")), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_CorruptedImageContent_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_corrupted.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Invalid image content") || s.Contains("Unsupported image format") || s.Contains("Cannot read properties")), It.IsAny<string>()), Times.Once);
        }



        [Fact]
        public void Mp3PropertyGetter_AddLogger_SetsLogger()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act
            getter.AddLogger(loggerMock.Object);

            // Assert
            getter.Logger.Should().Be(loggerMock.Object);
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
                var properties = getter.GetFileProperties(testFile);

                // Assert
                properties.Should().ContainKey("Title");
                properties["Title"].Should().Be("Test Complete Title");

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

                properties.Should().ContainKey("Duration");
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
        public void ImagePropertyGetter_GetFileProperties_CorruptedImage_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties("test_corrupted.jpg");

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Invalid image content")), It.IsAny<string>()), Times.Once);
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
            properties["Year"].Should().Be(0u);
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
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file")), It.IsAny<string>()), Times.Once);
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
                loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("is corrupted") || s.Contains("is not supported") || s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
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

            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read file")), It.IsAny<string>()), Times.Once);
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

            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Cannot read properties from file")), It.IsAny<string>()), Times.Once);
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
            properties.Should().ContainKey("Created");
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithoutProperties_ReturnsCreatedOnly()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties("test_empty.xlsx");

            // Assert
            properties.Should().NotContainKey("Title");
            properties.Should().NotContainKey("Creator");
            properties.Should().ContainKey("Created");
        }

        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_FileWithNoProperties_ReturnsEmptyDictionary()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();
            var path = "test_gen.xlsx";
            using (var spreadsheetDocument = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(path, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                var worksheetPart = workbookpart.AddNewPart<DocumentFormat.OpenXml.Packaging.WorksheetPart>();
                worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new DocumentFormat.OpenXml.Spreadsheet.SheetData());
                var sheets = workbookpart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());
                var sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = spreadsheetDocument.WorkbookPart!.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
                sheets.Append(sheet);
                workbookpart.Workbook.Save();
            }

            try
            {
                // Act
                var properties = getter.GetFileProperties(path);

                // Assert
                properties.Should().BeEmpty();
            }
            finally
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
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
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file")), It.IsAny<string>()), Times.Once);
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
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Could not open Excel file")), It.IsAny<string>()), Times.Once);
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
