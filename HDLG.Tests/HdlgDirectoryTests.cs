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
        public async Task Browse_WithoutSubdirectories_OnlyDiscoversTopLevelFiles()
        {
            // Arrange
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "test");
            var subDir = Path.Combine(baseDirectoryPath, "SubDir");
            System.IO.Directory.CreateDirectory(subDir);
            System.IO.File.WriteAllText(Path.Combine(subDir, "file2.txt"), "test");

            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);

            // Act
            await hdlgDirectory.BrowseAsync(propertyBrowser);

            // Assert
            hdlgDirectory.FilesCount.Should().Be(1);
            hdlgDirectory.Files[0].Name.Should().Be("file1.txt");
            hdlgDirectory.DirectoriesCount.Should().Be(0);
            hdlgDirectory.TotalDirectories.Should().Be(0);
            hdlgDirectory.TotalFiles.Should().Be(1);
        }

        [Fact]
        public async Task Browse_WithDeepRecursion_DiscoversAllLevels()
        {
            // Arrange
            int maxDepth = 10;
            string currentPath = baseDirectoryPath;

            // Create a deeply nested structure: Base -> Depth1 -> Depth2 ... -> Depth10
            for (int i = 1; i <= maxDepth; i++)
            {
                System.IO.File.WriteAllText(Path.Combine(currentPath, $"file_{i}.txt"), $"content {i}");
                currentPath = Path.Combine(currentPath, $"Depth{i}");
                System.IO.Directory.CreateDirectory(currentPath);
            }
            // Add a file in the deepest directory
            System.IO.File.WriteAllText(Path.Combine(currentPath, $"file_{maxDepth + 1}.txt"), "deepest content");

            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);

            // Act
            await hdlgDirectory.BrowseAsync(propertyBrowser);

            // Assert
            hdlgDirectory.TotalDirectories.Should().Be(maxDepth);
            hdlgDirectory.TotalFiles.Should().Be(maxDepth + 1);

            // Verify the structure manually by traversing down
            var currentHdlgDir = hdlgDirectory;
            for (int i = 1; i <= maxDepth; i++)
            {
                currentHdlgDir.FilesCount.Should().Be(1);
                currentHdlgDir.Files[0].Name.Should().Be($"file_{i}.txt");
                currentHdlgDir.DirectoriesCount.Should().Be(1);
                currentHdlgDir.Directories[0].Name.Should().Be($"Depth{i}");

                currentHdlgDir = currentHdlgDir.Directories[0];
            }

            currentHdlgDir.FilesCount.Should().Be(1);
            currentHdlgDir.Files[0].Name.Should().Be($"file_{maxDepth + 1}.txt");
            currentHdlgDir.DirectoriesCount.Should().Be(0);
        }

        [Fact]
        public async Task Browse_WithSubdirectories_DiscoversAllFilesAndSubdirectories()
        {
            // Arrange
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "test");
            var subDir = Path.Combine(baseDirectoryPath, "SubDir");
            System.IO.Directory.CreateDirectory(subDir);
            System.IO.File.WriteAllText(Path.Combine(subDir, "file2.txt"), "test");

            var hdlgDirectory = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);

            // Act
            await hdlgDirectory.BrowseAsync(propertyBrowser);

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

            hdlgDirectory.TotalDirectories.Should().Be(1);
            hdlgDirectory.TotalFiles.Should().Be(2);
            subHdlgDir.TotalDirectories.Should().Be(0);
            subHdlgDir.TotalFiles.Should().Be(1);
        }

        [Fact]
        public async Task Equals_SamePath_ReturnsTrue()
        {
            // Arrange
            var dir1 = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            var dir2 = new HdlgDirectory(baseDirectoryPath, false, false, loggerMock.Object);

            // Act & Assert
            dir1.Equals(dir2).Should().BeTrue();
            (dir1 == dir2).Should().BeTrue();
        }

        [Fact]
        public async Task Browse_UnauthorizedAccessException_LogsWarning()
        {
            // Arrange
            var restrictedDirPath = Path.Combine(baseDirectoryPath, "RestrictedDir");
            System.IO.Directory.CreateDirectory(restrictedDirPath);

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var di = new DirectoryInfo(restrictedDirPath);
                    var ds = di.GetAccessControl();
                    ds.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                        Environment.UserName,
                        System.Security.AccessControl.FileSystemRights.ReadData,
                        System.Security.AccessControl.AccessControlType.Deny));
                    di.SetAccessControl(ds);
                }
                else
                {
                    System.IO.File.SetUnixFileMode(restrictedDirPath, System.IO.UnixFileMode.None);
                }

                var hdlgDirectory = new HdlgDirectory(restrictedDirPath, true, true, loggerMock.Object);

                // Act
                await hdlgDirectory.BrowseAsync(propertyBrowser);

                // Assert
                loggerMock.Verify(
                    l => l.Warning(
                        It.IsAny<UnauthorizedAccessException>(),
                        "Access denied to directory: {Path}",
                        restrictedDirPath),
                    Times.Once);
            }
            finally
            {
                if (OperatingSystem.IsWindows())
                {
                    var di = new DirectoryInfo(restrictedDirPath);
                    var ds = di.GetAccessControl();
                    ds.RemoveAccessRule(new System.Security.AccessControl.FileSystemAccessRule(
                        Environment.UserName,
                        System.Security.AccessControl.FileSystemRights.ReadData,
                        System.Security.AccessControl.AccessControlType.Deny));
                    di.SetAccessControl(ds);
                }
                else
                {
                    System.IO.File.SetUnixFileMode(restrictedDirPath, System.IO.UnixFileMode.UserRead | System.IO.UnixFileMode.UserWrite | System.IO.UnixFileMode.UserExecute);
                }
            }
        }
    }
}
