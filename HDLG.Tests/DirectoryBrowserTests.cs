using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using HDLG_winforms;
using Moq;
using Serilog;
using Xunit;

namespace HDLG.Tests
{
    public class DirectoryBrowserTests : IDisposable
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly DirectoryBrowser directoryBrowser;
        private readonly string tempXmlFilePath;
        private readonly string tempHtmlFilePath;
        private readonly string tempJsonFilePath;
        private readonly HdlgDirectory testDirectory;
        private readonly string baseDirectoryPath;

        public DirectoryBrowserTests()
        {
            loggerMock = new Mock<ILogger>();
            directoryBrowser = new DirectoryBrowser(loggerMock.Object);

            tempXmlFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".xml");
            tempHtmlFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".html");
            tempJsonFilePath = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".json");

            baseDirectoryPath = Path.Combine(Path.GetTempPath(), "DirectoryBrowserTests_" + Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(baseDirectoryPath);

            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "file1.txt"), "content");

            testDirectory = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);
        }

        public void Dispose()
        {
            if (System.IO.File.Exists(tempXmlFilePath))
                System.IO.File.Delete(tempXmlFilePath);

            if (System.IO.File.Exists(tempHtmlFilePath))
                System.IO.File.Delete(tempHtmlFilePath);

            if (System.IO.File.Exists(tempJsonFilePath))
                System.IO.File.Delete(tempJsonFilePath);

            if (System.IO.Directory.Exists(baseDirectoryPath))
                System.IO.Directory.Delete(baseDirectoryPath, true);
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectoryBrowser(null!));
        }

        [Fact]
        public async Task SaveAsXMLAsync_NullFilePath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsXMLAsync(null!, testDirectory));
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsXMLAsync("", testDirectory));
        }

        [Fact]
        public async Task SaveAsXMLAsync_NullDirectory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => directoryBrowser.SaveAsXMLAsync(tempXmlFilePath, null!));
        }

        [Fact]
        public async Task SaveAsXMLAsync_ValidInputs_GeneratesXmlFile()
        {
            // Act
            await directoryBrowser.SaveAsXMLAsync(tempXmlFilePath, testDirectory);

            // Assert
            System.IO.File.Exists(tempXmlFilePath).Should().BeTrue();

            var xmlContent = await System.IO.File.ReadAllTextAsync(tempXmlFilePath);
            xmlContent.Should().Contain("<Hdlg");
            xmlContent.Should().Contain($"<Directory>{testDirectory.Path}</Directory>");
            xmlContent.Should().Contain("</Hdlg>");
        }

        [Fact]
        public async Task SaveAsXMLAsync_FileWithInvalidXmlCharacters_SanitizesAndSucceeds()
        {
            // Arrange
            var testFilePath = Path.Combine(baseDirectoryPath, "badxml.mp3");
            System.IO.File.WriteAllText(testFilePath, "dummy");

            var properties = new System.Collections.Generic.Dictionary<string, IConvertible>
            {
                { "Camera Model", "Nikon" }, // Space is invalid in XML element name
                { "InvalidVal", "Test\x0BText" } // \x0B is invalid XML character
            };

            var browserMock = new Mock<HdlgFileProperty.FilePropertyBrowser>(loggerMock.Object, new HdlgFileProperty.IFilePropertyGetter[0]);
            browserMock.Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && (f.FullName == testFilePath || f.FullName.EndsWith("badxml.mp3"))))).ReturnsAsync(properties);

            var dir = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);
            await dir.BrowseAsync(browserMock.Object);

            var xmlPath = Path.Combine(Path.GetTempPath(), "test_badxml_" + Guid.NewGuid().ToString() + ".xml");

            try
            {
                // Act
                await directoryBrowser.SaveAsXMLAsync(xmlPath, dir);

                // Assert
                System.IO.File.Exists(xmlPath).Should().BeTrue();

                var xmlContent = await System.IO.File.ReadAllTextAsync(xmlPath);

                // Should use encoded local name for 'Camera Model'
                xmlContent.Should().Contain("<Camera_x0020_Model>Nikon</Camera_x0020_Model>");

                // Should sanitize value by removing the invalid XML character (\x0B)
                xmlContent.Should().Contain("<InvalidVal>TestText</InvalidVal>");
            }
            finally
            {
                if (System.IO.File.Exists(xmlPath))
                    System.IO.File.Delete(xmlPath);
            }
        }

        [Fact]
        public async Task SaveAsXMLAsync_FilesWithProperties_AreSiblingsNotNested()
        {
            // Arrange: one file with extended properties, one without — both must be sibling <File> nodes.
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "with_props.txt"), "a");
            System.IO.File.WriteAllText(Path.Combine(baseDirectoryPath, "without_props.txt"), "b");

            var properties = new System.Collections.Generic.Dictionary<string, IConvertible>
            {
                { "Author", "Bob" }
            };

            var browserMock = new Mock<HdlgFileProperty.FilePropertyBrowser>(loggerMock.Object, Array.Empty<HdlgFileProperty.IFilePropertyGetter>());
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.IsAny<FileInfo>()))
                .ReturnsAsync((FileInfo fi) => fi.Name == "with_props.txt" ? properties : null);

            var dir = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);
            await dir.BrowseAsync(browserMock.Object);

            var xmlPath = Path.Combine(Path.GetTempPath(), "test_xml_structure_" + Guid.NewGuid().ToString() + ".xml");
            try
            {
                // Act
                await directoryBrowser.SaveAsXMLAsync(xmlPath, dir);

                // Assert
                var doc = XDocument.Load(xmlPath);
                var files = doc.Descendants("File").ToList();

                files.Should().HaveCountGreaterThanOrEqualTo(2);
                foreach (var file in files)
                {
                    file.Parent.Should().NotBeNull();
                    file.Parent!.Name.LocalName.Should().Be("Files",
                        "each <File> must close properly so siblings are not nested inside another <File>");
                }
                files.Count(f => f.Element("ExtentedProperties") != null).Should().Be(1);

                var withProps = files.Single(f => (string?)f.Element("Name") == "with_props.txt");
                withProps.Element("ExtentedProperties").Should().NotBeNull();
                ((string?)withProps.Element("ExtentedProperties")!.Element("Author")).Should().Be("Bob");
            }
            finally
            {
                if (System.IO.File.Exists(xmlPath))
                    System.IO.File.Delete(xmlPath);
            }
        }

        [Fact]
        public async Task SaveAsHTMLAsync_NullFilePath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsHTMLAsync(null!, testDirectory));
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsHTMLAsync("", testDirectory));
        }

        [Fact]
        public async Task SaveAsHTMLAsync_NullDirectory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => directoryBrowser.SaveAsHTMLAsync(tempHtmlFilePath, null!));
        }


        [Fact]
        public async Task SaveAsHTMLAsync_FileLocked_ThrowsIOException()
        {
            // Arrange
            // Lock the file by opening it with exclusive access
            using var fileStream = new FileStream(tempHtmlFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => directoryBrowser.SaveAsHTMLAsync(tempHtmlFilePath, testDirectory));
        }

        [Fact]
        public async Task SaveAsHTMLAsync_ValidInputs_GeneratesHtmlFile()
        {
            // Act
            await directoryBrowser.SaveAsHTMLAsync(tempHtmlFilePath, testDirectory);

            // Assert
            System.IO.File.Exists(tempHtmlFilePath).Should().BeTrue();

            var htmlContent = await System.IO.File.ReadAllTextAsync(tempHtmlFilePath);
            htmlContent.Should().Contain("<!DOCTYPE html>");
            htmlContent.Should().Contain($"<html lang=\"{System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName}\">");
            htmlContent.Should().Contain($"<h2>{WebUtility.HtmlEncode(testDirectory.Path)}</h2>");
            htmlContent.Should().Contain("</html>");

            htmlContent.Should().Contain("<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none';\">");

            // 2026 modern responsive HTML assertions (updated for prettier redesign using details/summary + clean cards)
            htmlContent.Should().Contain("<details class=\"directory\"");
            htmlContent.Should().Contain("<summary>");
            htmlContent.Should().Contain("class=\"hdlg\"");
            htmlContent.Should().Contain("class=\"hdlg-footer\"");
            htmlContent.Should().Contain("HTML Directory List Generator");
            // Note: .file markup only emitted when the HdlgDirectory instance contains files (this basic test setup does not populate via Browse)
        }

        [Fact]
        public async Task SaveAsHTMLAsync_DirectoryWithSpecialChars_EncodesHtmlContent()
        {
            // Arrange — create a directory with HTML-dangerous characters in the name
            var dangerousDirName = "test&dir<xss>";
            var dangerousDirPath = Path.Combine(baseDirectoryPath, dangerousDirName);

            // On Windows, < and > are not valid directory name chars, so use only &
            var safeDangerousDirName = "test&dir";
            var safeDangerousDirPath = Path.Combine(baseDirectoryPath, safeDangerousDirName);
            System.IO.Directory.CreateDirectory(safeDangerousDirPath);
            System.IO.File.WriteAllText(Path.Combine(safeDangerousDirPath, "a&b.txt"), "content");

            var dirWithSubDirs = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            var browser = new HdlgFileProperty.FilePropertyBrowser(loggerMock.Object);
            await dirWithSubDirs.BrowseAsync(browser);

            var htmlPath = Path.Combine(Path.GetTempPath(), "test_xss_" + Guid.NewGuid().ToString() + ".html");

            try
            {
                // Act
                await directoryBrowser.SaveAsHTMLAsync(htmlPath, dirWithSubDirs);

                // Assert
                var htmlContent = await System.IO.File.ReadAllTextAsync(htmlPath);

                // The & character must be encoded as &amp; in the HTML output (including in new title= hover popup attrs)
                htmlContent.Should().Contain(WebUtility.HtmlEncode(safeDangerousDirName));
                htmlContent.Should().NotContain($"<span class=\"name\" title=\"{safeDangerousDirName}\">");
                htmlContent.Should().Contain($"<span class=\"name\" title=\"{WebUtility.HtmlEncode(safeDangerousDirName)}\">");

                // Long name handling: title= provides the full name for native hover popup (both in tree and TOC).
                // TOC ("main directory menu") uses 23ch max-width + ellipsis (reduced by 3ch for better fit).
                htmlContent.Should().Contain($"title=\"{WebUtility.HtmlEncode(safeDangerousDirName)}\"");
                htmlContent.Should().Contain($"<a href=\"#{WebUtility.HtmlEncode(baseDirectoryPath)}\" title=\"{WebUtility.HtmlEncode(System.IO.Path.GetFileName(baseDirectoryPath))}\">");

                // File name with & must also be encoded
                htmlContent.Should().Contain(WebUtility.HtmlEncode("a&b.txt"));
            }
            finally
            {
                if (System.IO.File.Exists(htmlPath))
                    System.IO.File.Delete(htmlPath);
            }
        }

        [Fact]
        public async Task SaveAsJSONAsync_NullFilePath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsJSONAsync(null!, testDirectory));
            await Assert.ThrowsAsync<ArgumentException>(() => directoryBrowser.SaveAsJSONAsync("", testDirectory));
        }

        [Fact]
        public async Task SaveAsJSONAsync_NullDirectory_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, null!));
        }

        [Fact]
        public async Task SaveAsJSONAsync_ValidInputs_GeneratesCompactJsonFile()
        {
            var browser = new HdlgFileProperty.FilePropertyBrowser(loggerMock.Object);
            await testDirectory.BrowseAsync(browser);
            await directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, testDirectory);

            System.IO.File.Exists(tempJsonFilePath).Should().BeTrue();
            var json = await System.IO.File.ReadAllTextAsync(tempJsonFilePath);
            json.Should().NotContain("\n  ");
            using var doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            root.TryGetProperty("Hdlg", out _).Should().BeFalse();
            root.GetProperty("Version").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("Directory").GetString().Should().Be(testDirectory.Path);
            root.GetProperty("DateTime").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("DirectoriesCount").ValueKind.Should().Be(JsonValueKind.Number);
            root.GetProperty("FilesCount").ValueKind.Should().Be(JsonValueKind.Number);
            JsonElement tree = root.GetProperty("Root");
            tree.GetProperty("Name").GetString().Should().Be(testDirectory.Name);
            tree.GetProperty("Path").GetString().Should().Be(testDirectory.Path);
            tree.GetProperty("Directories").ValueKind.Should().Be(JsonValueKind.Array);
            tree.GetProperty("Files").ValueKind.Should().Be(JsonValueKind.Array);
            tree.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("file1.txt");
        }

        [Fact]
        public async Task SaveAsJSONAsync_FileLocked_ThrowsIOException()
        {
            using var fileStream = new FileStream(tempJsonFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            await Assert.ThrowsAsync<IOException>(() => directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, testDirectory));
        }

        [Fact]
        public async Task SaveAsJSONAsync_FilesWithProperties_UsesOriginalKeysAndNativeTypes()
        {
            var testFilePath = Path.Combine(baseDirectoryPath, "photo.jpg");
            System.IO.File.WriteAllText(testFilePath, "dummy");

            var properties = new System.Collections.Generic.Dictionary<string, IConvertible>
            {
                { "Camera Model", "Nikon" },
                { "Width", 1920 },
                { "Taken", new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Unspecified) },
                { "   ", "skip-me" },
            };

            var browserMock = new Mock<HdlgFileProperty.FilePropertyBrowser>(loggerMock.Object, Array.Empty<HdlgFileProperty.IFilePropertyGetter>());
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.FullName == testFilePath)))
                .ReturnsAsync(properties);
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.FullName != testFilePath)))
                .ReturnsAsync((IReadOnlyDictionary<string, IConvertible>?)null);

            var dir = new HdlgDirectory(baseDirectoryPath, true, false, loggerMock.Object);
            await dir.BrowseAsync(browserMock.Object);

            var jsonPath = Path.Combine(Path.GetTempPath(), "test_json_props_" + Guid.NewGuid().ToString() + ".json");
            try
            {
                await directoryBrowser.SaveAsJSONAsync(jsonPath, dir);

                using var doc = JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(jsonPath));
                JsonElement files = doc.RootElement.GetProperty("Root").GetProperty("Files");
                files.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);

                JsonElement photo = default;
                foreach (JsonElement file in files.EnumerateArray())
                {
                    if (file.GetProperty("Name").GetString() == "photo.jpg")
                    {
                        photo = file;
                        break;
                    }
                }
                photo.ValueKind.Should().Be(JsonValueKind.Object);

                photo.GetProperty("Size").ValueKind.Should().Be(JsonValueKind.Number);
                JsonElement ext = photo.GetProperty("ExtentedProperties");
                ext.TryGetProperty("Camera Model", out JsonElement camera).Should().BeTrue();
                camera.GetString().Should().Be("Nikon");
                ext.TryGetProperty("Camera_x0020_Model", out _).Should().BeFalse();
                ext.GetProperty("Width").GetInt32().Should().Be(1920);
                ext.GetProperty("Taken").GetString().Should().Be(new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Unspecified).ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                ext.TryGetProperty("   ", out _).Should().BeFalse();

                foreach (JsonElement file in files.EnumerateArray())
                {
                    if (file.GetProperty("Name").GetString() != "photo.jpg")
                    {
                        file.GetProperty("ExtentedProperties").EnumerateObject().Should().BeEmpty();
                    }
                }
            }
            finally
            {
                if (System.IO.File.Exists(jsonPath))
                    System.IO.File.Delete(jsonPath);
            }
        }

        [Fact]
        public async Task SaveAsJSONAsync_BrowsedTree_WritesFilesCountsAndNestedDirectories()
        {
            var subDirPath = Path.Combine(baseDirectoryPath, "child");
            System.IO.Directory.CreateDirectory(subDirPath);
            System.IO.File.WriteAllText(Path.Combine(subDirPath, "nested.txt"), "n");

            var properties = new System.Collections.Generic.Dictionary<string, IConvertible>
            {
                { "Flag", true },
            };

            var browserMock = new Mock<HdlgFileProperty.FilePropertyBrowser>(
                loggerMock.Object,
                Array.Empty<HdlgFileProperty.IFilePropertyGetter>());
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.Name == "file1.txt")))
                .ReturnsAsync(properties);
            browserMock
                .Setup(b => b.GetFilePropertyAsync(It.Is<FileInfo>(f => f != null && f.Name != "file1.txt")))
                .ReturnsAsync((IReadOnlyDictionary<string, IConvertible>?)null);

            var dir = new HdlgDirectory(baseDirectoryPath, true, true, loggerMock.Object);
            await dir.BrowseAsync(browserMock.Object);

            await directoryBrowser.SaveAsJSONAsync(tempJsonFilePath, dir);

            using var doc = JsonDocument.Parse(await System.IO.File.ReadAllTextAsync(tempJsonFilePath));
            JsonElement root = doc.RootElement;
            root.GetProperty("DirectoriesCount").GetInt64().Should().Be(dir.TotalDirectories);
            root.GetProperty("FilesCount").GetInt64().Should().Be(dir.TotalFiles);

            JsonElement tree = root.GetProperty("Root");
            tree.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("file1.txt");

            JsonElement file1 = tree.GetProperty("Files").EnumerateArray()
                .Single(f => f.GetProperty("Name").GetString() == "file1.txt");
            file1.GetProperty("ExtentedProperties").GetProperty("Flag").GetBoolean().Should().BeTrue();

            JsonElement child = tree.GetProperty("Directories").EnumerateArray()
                .Single(d => d.GetProperty("Name").GetString() == "child");
            child.GetProperty("Files").EnumerateArray()
                .Select(f => f.GetProperty("Name").GetString())
                .Should().Contain("nested.txt");
        }
    }
}
