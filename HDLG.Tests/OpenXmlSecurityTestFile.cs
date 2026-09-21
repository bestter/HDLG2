using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using HdlgFileProperty;

namespace HDLG.Tests
{
    internal sealed class OpenXmlSecurityTestFile : IDisposable
    {
        private OpenXmlSecurityTestFile(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static OpenXmlSecurityTestFile CreateWord(
            string? title = "Safe Title",
            string? creator = "Safe Creator",
            DateTime? created = null,
            long embeddedBinaryBytes = 0)
        {
            string path = CreatePath(".docx");
            using (WordprocessingDocument document = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();
                document.PackageProperties.Title = title;
                document.PackageProperties.Creator = creator;
                document.PackageProperties.Created = created;

                if (embeddedBinaryBytes > 0)
                {
                    EmbeddedPackagePart binaryPart = mainPart.AddEmbeddedPackagePart("application/octet-stream");
                    using Stream stream = binaryPart.GetStream(FileMode.Create, FileAccess.Write);
                    WriteRepeatedBytes(stream, embeddedBinaryBytes);
                }
            }

            return new OpenXmlSecurityTestFile(path);
        }

        public static OpenXmlSecurityTestFile CreateExcel(
            string? title = "Safe Title",
            string? creator = "Safe Creator",
            DateTime? created = null,
            long embeddedBinaryBytes = 0)
        {
            string path = CreatePath(".xlsx");
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                document.PackageProperties.Title = title;
                document.PackageProperties.Creator = creator;
                document.PackageProperties.Created = created;

                if (embeddedBinaryBytes > 0)
                {
                    ExtendedPart binaryPart = workbookPart.AddExtendedPart(
                        "urn:hdlg:test:large-binary",
                        "application/octet-stream",
                        ".bin");
                    using Stream stream = binaryPart.GetStream(FileMode.Create, FileAccess.Write);
                    WriteRepeatedBytes(stream, embeddedBinaryBytes);
                }
            }

            return new OpenXmlSecurityTestFile(path);
        }

        public static OpenXmlSecurityTestFile CreatePackageWithOversizedContentTypes(string extension)
        {
            string path = CreatePath(extension);
            using FileStream fileStream = new(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            using System.IO.Compression.ZipArchive archive = new(fileStream, System.IO.Compression.ZipArchiveMode.Create);
            System.IO.Compression.ZipArchiveEntry entry = archive.CreateEntry("[Content_Types].xml");
            using Stream entryStream = entry.Open();
            using StreamWriter writer = new(entryStream);
            writer.Write("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
            WriteRepeatedCharacters(writer, FilePropertyLimits.MaxOpenXmlPartSizeBytes + 1);
            writer.Write("</Types>");
            return new OpenXmlSecurityTestFile(path);
        }

        public static OpenXmlSecurityTestFile CreateSizedFile(string extension, long length)
        {
            string path = CreatePath(extension);
            using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            stream.SetLength(length);
            return new OpenXmlSecurityTestFile(path);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        private static string CreatePath(string extension)
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"hdlg-openxml-{Guid.NewGuid():N}{extension}");
        }

        private static void WriteRepeatedBytes(Stream stream, long count)
        {
            byte[] buffer = new byte[81_920];
            while (count > 0)
            {
                int bytesToWrite = (int)Math.Min(buffer.Length, count);
                stream.Write(buffer, 0, bytesToWrite);
                count -= bytesToWrite;
            }
        }

        private static void WriteRepeatedCharacters(TextWriter writer, long count)
        {
            char[] buffer = new char[81_920];
            Array.Fill(buffer, ' ');
            while (count > 0)
            {
                int charactersToWrite = (int)Math.Min(buffer.Length, count);
                writer.Write(buffer, 0, charactersToWrite);
                count -= charactersToWrite;
            }
        }
    }
}
