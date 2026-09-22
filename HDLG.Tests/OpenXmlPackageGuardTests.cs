using System.IO.Compression;
using System.Text;
using System.Xml;
using FluentAssertions;
using HdlgFileProperty;

namespace HDLG.Tests
{
    public class OpenXmlPackageGuardTests
    {
        private const string CorePropertiesXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
            + "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\">"
            + "<dc:title>Safe Title</dc:title>"
            + "<dc:creator>Safe Creator</dc:creator>"
            + "<dcterms:created>2023-01-01T12:00:00Z</dcterms:created>"
            + "</cp:coreProperties>";

        [Theory]
        [InlineData("docProps/core.xml")]
        [InlineData("/docProps/core.xml")]
        public void ReadCoreProperties_CorePropertiesTarget_ReturnsTitleCreatorAndCreated(string target)
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", RelationshipPart(target)),
                ("docProps/core.xml", CorePropertiesXml));

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(package);

            properties["Title"].Should().Be("Safe Title");
            properties["Creator"].Should().Be("Safe Creator");
            ((DateTime)properties["Created"]).Should().Be(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ReadCoreProperties_MissingRootRelationships_ReturnsEmpty()
        {
            using MemoryStream package = CreateArchive(("[Content_Types].xml", "<Types></Types>"));

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(package);

            properties.Should().BeEmpty();
        }

        [Fact]
        public void ReadCoreProperties_NoCorePropertiesRelationship_ReturnsEmpty()
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"></Relationships>"));

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(package);

            properties.Should().BeEmpty();
        }

        [Fact]
        public void ReadCoreProperties_TooManyEntries_ThrowsInvalidDataException()
        {
            using MemoryStream package = CreateArchive(
                ("one.bin", ""),
                ("two.bin", ""));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(package, maxEntries: 1);

            act.Should().Throw<InvalidDataException>().WithMessage("*more than 1 entries*");
        }

        [Fact]
        public void ReadCoreProperties_RelationshipsPartExceedsLimit_ThrowsInvalidDataException()
        {
            string relationships = RelationshipPart("docProps/core.xml") + new string(' ', 500);
            using MemoryStream package = CreateArchive(("_rels/.rels", relationships));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(
                package,
                maxPartSizeBytes: 100,
                maxProcessedBytes: 10_000);

            act.Should().Throw<InvalidDataException>().WithMessage("*part exceeds*");
        }

        [Fact]
        public void ReadCoreProperties_ProcessedContentExceedsLimit_ThrowsInvalidDataException()
        {
            string relationships = RelationshipPart("docProps/core.xml");
            int relationshipsBytes = Encoding.UTF8.GetByteCount(relationships);
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", relationships),
                ("docProps/core.xml", CorePropertiesXml));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(
                package,
                maxPartSizeBytes: 10_000,
                maxProcessedBytes: relationshipsBytes);

            act.Should().Throw<InvalidDataException>().WithMessage("*parsed OpenXML content exceeds*");
        }

        [Fact]
        public void ReadCoreProperties_DtdInRelationships_ThrowsInvalidDataException()
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", "<!DOCTYPE Relationships [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><Relationships>&xxe;</Relationships>"));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(package);

            act.Should().Throw<InvalidDataException>()
                .WithMessage("*invalid XML*")
                .WithInnerException<XmlException>();
        }

        [Fact]
        public void ReadCoreProperties_TitleAtLimit_ReturnsTitle()
        {
            string title = new('A', FilePropertyLimits.MaxOpenXmlPropertyCharacters);
            using OpenXmlSecurityTestFile testFile = OpenXmlSecurityTestFile.CreateWord(title: title);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(stream);

            properties["Title"].Should().Be(title);
        }

        [Fact]
        public void ReadCoreProperties_TitleOverLimit_ThrowsInvalidDataException()
        {
            string title = new('A', FilePropertyLimits.MaxOpenXmlPropertyCharacters + 1);
            using OpenXmlSecurityTestFile testFile = OpenXmlSecurityTestFile.CreateWord(title: title);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(stream);

            act.Should().Throw<InvalidDataException>().WithMessage("*core property exceeds*");
        }

        [Fact]
        public void ReadCoreProperties_MissingCorePropertiesPart_ThrowsInvalidDataException()
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", RelationshipPart("docProps/core.xml")));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(package);

            act.Should().Throw<InvalidDataException>().WithMessage("*core-properties part is missing*");
        }

        [Theory]
        [InlineData("../huge.bin")]
        [InlineData("%2E%2E/huge.bin")]
        [InlineData("docProps\\core.xml")]
        [InlineData("http://evil.example/core.xml")]
        [InlineData("//server/core.xml")]
        public void ReadCoreProperties_IllegalTarget_ThrowsWithoutReadingThePart(string target)
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", RelationshipPart(target)),
                ("huge.bin", new string('B', 5_000)));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(
                package,
                maxPartSizeBytes: 2_000,
                maxProcessedBytes: 2_000);

            act.Should().Throw<InvalidDataException>().WithMessage("*outside the package*");
        }

        [Fact]
        public void ReadCoreProperties_ExternalTarget_ThrowsWithoutReadingThePart()
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", RelationshipPart("huge.bin", "External")),
                ("huge.bin", new string('B', 5_000)));

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(
                package,
                maxPartSizeBytes: 2_000,
                maxProcessedBytes: 2_000);

            act.Should().Throw<InvalidDataException>().WithMessage("*external*");
        }

        [Fact]
        public void ReadCoreProperties_LargeUnparsedBinary_ReturnsProperties()
        {
            using OpenXmlSecurityTestFile testFile = OpenXmlSecurityTestFile.CreateWord(embeddedBinaryBytes: 131_072);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(
                stream,
                maxPartSizeBytes: 65_536,
                maxProcessedBytes: 65_536);

            properties["Title"].Should().Be("Safe Title");
            properties["Creator"].Should().Be("Safe Creator");
        }

        [Fact]
        public void ReadCoreProperties_OversizedUnreadContentTypes_ReturnsProperties()
        {
            using OpenXmlSecurityTestFile testFile = OpenXmlSecurityTestFile.CreatePackageWithOversizedContentTypes(".docx", 8_192);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            IReadOnlyDictionary<string, IConvertible> properties = OpenXmlPackageGuard.ReadCoreProperties(
                stream,
                maxPartSizeBytes: 4_096,
                maxProcessedBytes: 64 * 1024);

            properties["Title"].Should().Be("Safe Title");
            properties["Creator"].Should().Be("Safe Creator");
        }

        [Fact]
        public void ReadCoreProperties_PhysicalFileOverLimit_ThrowsInvalidDataException()
        {
            using OpenXmlSecurityTestFile testFile = OpenXmlSecurityTestFile.CreateSizedFile(
                ".docx",
                FilePropertyLimits.MaxFileSizeBytes + 1);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Action act = () => OpenXmlPackageGuard.ReadCoreProperties(stream);

            act.Should().Throw<InvalidDataException>().WithMessage("*exceeds the maximum allowed size*");
        }

        private static string RelationshipPart(string target, string? targetMode = null)
        {
            string mode = targetMode == null ? string.Empty : $" TargetMode=\"{targetMode}\"";
            return "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                + $"<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"{target}\"{mode}/>"
                + "</Relationships>";
        }

        private static MemoryStream CreateArchive(params (string Name, string Content)[] entries)
        {
            MemoryStream package = new();
            using (ZipArchive archive = new(package, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach ((string name, string content) in entries)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(name);
                    using Stream stream = entry.Open();
                    byte[] bytes = Encoding.UTF8.GetBytes(content);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            package.Position = 0;
            return package;
        }
    }
}
