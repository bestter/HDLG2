using System.IO.Compression;
using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using HdlgFileProperty;

namespace HDLG.Tests
{
    public class OpenXmlPackageGuardTests
    {
        [Fact]
        public void ValidateArchiveStructure_TooManyEntries_ThrowsInvalidDataException()
        {
            using MemoryStream package = CreateArchive(
                ("one.bin", ""),
                ("two.bin", ""));

            Action act = () => OpenXmlPackageGuard.ValidateArchiveStructure(package, maxEntries: 1);

            act.Should().Throw<InvalidDataException>().WithMessage("*more than 1 entries*");
        }

        [Fact]
        public void ValidateArchiveStructure_ProcessedXmlExceedsLimit_ThrowsInvalidDataException()
        {
            using MemoryStream package = CreateArchive(
                ("_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"></Relationships>"),
                ("word/_rels/document.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"></Relationships>"));

            Action act = () => OpenXmlPackageGuard.ValidateArchiveStructure(
                package,
                maxPartSizeBytes: 1_000,
                maxProcessedBytes: 100);

            act.Should().Throw<InvalidDataException>().WithMessage("*parsed OpenXML content exceeds*");
        }

        [Fact]
        public void ValidateArchiveStructure_LargeUnparsedBinary_DoesNotCountTowardXmlLimits()
        {
            using MemoryStream package = new();
            using (ZipArchive archive = new(package, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteEntry(archive, "[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"></Types>");
                ZipArchiveEntry binaryEntry = archive.CreateEntry("word/media/large.bin");
                using Stream binaryStream = binaryEntry.Open();
                binaryStream.Write(new byte[2_000]);
            }
            package.Position = 0;

            Action act = () => OpenXmlPackageGuard.ValidateArchiveStructure(
                package,
                maxPartSizeBytes: 100,
                maxProcessedBytes: 100);

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateArchiveStructure_DtdInStructuralXml_ThrowsXmlException()
        {
            using MemoryStream package = CreateArchive(
                ("[Content_Types].xml", "<!DOCTYPE Types [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><Types>&xxe;</Types>"));

            Action act = () => OpenXmlPackageGuard.ValidateArchiveStructure(package);

            act.Should().Throw<System.Xml.XmlException>();
        }

        [Fact]
        public void ExtractProperties_CreatedRawValueExceedsConfiguredLimit_ThrowsInvalidDataException()
        {
            using var testFile = OpenXmlSecurityTestFile.CreateWord(created: DateTime.UtcNow);
            using FileStream stream = new(testFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long processedBytes = OpenXmlPackageGuard.ValidateArchiveStructure(stream);
            using WordprocessingDocument document = WordprocessingDocument.Open(stream, false);

            Action act = () => OpenXmlPackageGuard.ExtractProperties(
                document,
                processedBytes,
                maxPropertyCharacters: 5);

            act.Should().Throw<InvalidDataException>().WithMessage("*core property exceeds*");
        }

        private static MemoryStream CreateArchive(params (string Name, string Content)[] entries)
        {
            MemoryStream package = new();
            using (ZipArchive archive = new(package, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach ((string name, string content) in entries)
                {
                    WriteEntry(archive, name, content);
                }
            }
            package.Position = 0;
            return package;
        }

        private static void WriteEntry(ZipArchive archive, string name, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name);
            using StreamWriter writer = new(entry.Open());
            writer.Write(content);
        }
    }
}
