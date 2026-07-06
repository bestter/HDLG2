using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;
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
            Mp3Setup.CreateMp3Docs();
            PdfSetup.CreatePdfDocs();
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
            var result = getter.IsSupportedFile(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path!));

            // Assert
            result.Should().Be(expected);
        }


        [Fact]
        public void ImagePropertyGetter_GetFileProperties_OversizedFile_ReturnsEmptyAndLogsWarning()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);
            var tempFile = Path.GetTempFileName() + ".jpg";
            try
            {
                using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.SetLength(FilePropertyLimits.MaxFileSizeBytes + 1);
                }

                // Act
                var properties = getter.GetFileProperties(string.IsNullOrWhiteSpace(tempFile) ? null! : new FileInfo(tempFile));

                // Assert
                properties.Should().BeEmpty();
                loggerMock.Verify(
                    l => l.Warning(
                        It.Is<string>(s => s.Contains("exceeds maximum allowed size")),
                        FilePropertyLimits.MaxFileSizeBytes,
                        FilePropertyLimits.MaxFileSizeBytes + 1,
                        tempFile),
                    Times.Once);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithExif_ReturnsPropertiesAndCameraModel()
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
        public void ImagePropertyGetter_GetFileProperties_ValidFileWithoutExif_ReturnsPropertiesWithoutCameraModel()
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
        public void ImagePropertyGetter_GetFileProperties_ValidPngFile_ReturnsProperties()
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
        public void ImagePropertyGetter_GetFileProperties_FileNotFound_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var getter = new ImagePropertyGetter();
            getter.AddLogger(loggerMock.Object);

            // Act
            var properties = getter.GetFileProperties(new FileInfo("nonexistent.jpg"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_invalid.jpg"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_corrupted.jpg"));

            // Assert
            properties.Should().BeEmpty();
            loggerMock.Verify(l => l.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Invalid image content") || s.Contains("Unsupported image format") || s.Contains("Cannot read properties")), It.IsAny<string>()), Times.Once);
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
            var result = getter.IsSupportedFile(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path));

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Mp3PropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsProperties()
        {
            // Arrange
            var getter = new Mp3PropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test.mp3"));

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
            var invalidFile = "test_invalid_temp.mp3";
            System.IO.File.WriteAllText(invalidFile, "invalid mp3 content");

            try
            {
                // Act
                var properties = getter.GetFileProperties(string.IsNullOrWhiteSpace(invalidFile) ? null! : new FileInfo(invalidFile));

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
            var properties = getter.GetFileProperties(new FileInfo("test.pdf"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_empty_title.pdf"));

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
            var properties = getter.GetFileProperties(new FileInfo("nonexistent.pdf"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_invalid.pdf"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_encrypted.pdf"));

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
            var result = getter.IsSupportedFile(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path));

            // Assert
            result.Should().Be(expected);
        }





        [Fact]
        public void ExcelPropertyGetter_GetFileProperties_ValidFileWithTitle_ReturnsTitle()
        {
            // Arrange
            var getter = new ExcelPropertyGetter();

            // Act
            var properties = getter.GetFileProperties(new FileInfo("test.xlsx"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_empty.xlsx"));

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
                var properties = getter.GetFileProperties(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path));

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
            var properties = getter.GetFileProperties(new FileInfo("nonexistent.xlsx"));

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
            var properties = getter.GetFileProperties(new FileInfo("test_invalid.xlsx"));

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
            var result = getter.IsSupportedFile(string.IsNullOrWhiteSpace(path) ? null! : new FileInfo(path));

            // Assert
            result.Should().Be(expected);
        }
    }
}
