using System;
using System.IO;
using FluentAssertions;
using HDLG_winforms;
using HdlgFileProperty;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class HdlgDirectoryTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IFilePropertyGetter> propertyGetterMock;
        private readonly FilePropertyBrowser propertyBrowser;
        private readonly string baseDirectoryPath;

        public HdlgDirectoryTests()
        {
            loggerMock = new Mock<ILogger>();
            propertyGetterMock = new Mock<IFilePropertyGetter>();
            propertyBrowser = new FilePropertyBrowser(loggerMock.Object, propertyGetterMock.Object);

            baseDirectoryPath = Path.Combine(Path.GetTempPath(), "HdlgDirectoryTests_" + Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(baseDirectoryPath);
        }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(baseDirectoryPath))
            {
                System.IO.Directory.Delete(baseDirectoryPath, true);
            }
        }

        [Fact]
        public void Constructor_ValidDirectory_PopulatesProperties()
        {
            // Act
            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);

            // Assert
            hdlgDirectory.Path.Should().Be(baseDirectoryPath);
            hdlgDirectory.Name.Should().Be(new DirectoryInfo(baseDirectoryPath).Name);
            hdlgDirectory.IsTopDirectory.Should().BeTrue();
            hdlgDirectory.BrowseSubdirectory.Should().BeTrue();
            hdlgDirectory.DirectoriesCount.Should().Be(0);
            hdlgDirectory.FilesCount.Should().Be(0);
        }

        [Fact]
        public void Constructor_NullDirectoryInfo_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HdlgDirectory((DirectoryInfo)null!, true, true, loggerMock.Object));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HdlgDirectory(baseDirectoryPath, true, true, null!));
        }

        [Fact]
        public void Browse_WithoutSubdirectories_OnlyDiscoversTopLevelFiles()
        {
            // Arrange
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "test");
            var subDir = Path.Combine(baseDirectoryPath, "SubDir");
            System.IO.Directory.CreateDirectory(subDir);
            System.IO.File.WriteAllText(Path.Combine(subDir, "file2.txt"), "test");

            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);

            // Act
            hdlgDirectory.Browse(propertyBrowser);

            // Assert
            hdlgDirectory.FilesCount.Should().Be(1);
            hdlgDirectory.Files[0].Name.Should().Be("file1.txt");
            hdlgDirectory.DirectoriesCount.Should().Be(0);
        }

        [Fact]
        public void Browse_WithSubdirectories_DiscoversAllFilesAndSubdirectories()
        {
            // Arrange
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "test");
            var subDir = Path.Combine(baseDirectoryPath, "SubDir");
            System.IO.Directory.CreateDirectory(subDir);
            System.IO.File.WriteAllText(Path.Combine(subDir, "file2.txt"), "test");

            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);

            // Act
            hdlgDirectory.Browse(propertyBrowser);

            // Assert
            hdlgDirectory.FilesCount.Should().Be(1);
            hdlgDirectory.Files[0].Name.Should().Be("file1.txt");
            hdlgDirectory.DirectoriesCount.Should().Be(1);

            var subHdlgDir = hdlgDirectory.Directories[0];
            subHdlgDir.Name.Should().Be("SubDir");
            subHdlgDir.FilesCount.Should().Be(1);
            subHdlgDir.Files[0].Name.Should().Be("file2.txt");
            subHdlgDir.IsTopDirectory.Should().BeFalse();
            subHdlgDir.BrowseSubdirectory.Should().BeTrue();
        }

        [Fact]
        public void Equals_SamePath_ReturnsTrue()
        {
            // Arrange
            var dir1 = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            var dir2 = new HdlgDirectory(baseDirectoryPath, false, false, loggerMock.Object);

            // Act & Assert
            dir1.Equals(dir2).Should().BeTrue();
            (dir1 == dir2).Should().BeTrue();
        }
    }
}
