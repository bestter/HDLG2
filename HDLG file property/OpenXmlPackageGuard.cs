/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace HdlgFileProperty
{
    /// <summary>
    /// Reads Title, Creator, and Created from an OpenXML package.
    /// Only the root relationships part and the core-properties part are inflated.
    /// </summary>
    internal static class OpenXmlPackageGuard
    {
        private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private const string CorePropertiesRelationshipType = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";

        internal static IReadOnlyDictionary<string, IConvertible> ReadCoreProperties(
            Stream packageStream,
            long maxFileSizeBytes = FilePropertyLimits.MaxFileSizeBytes,
            int maxEntries = FilePropertyLimits.MaxOpenXmlEntries,
            long maxPartSizeBytes = FilePropertyLimits.MaxOpenXmlPartSizeBytes,
            long maxProcessedBytes = FilePropertyLimits.MaxOpenXmlProcessedBytes,
            int maxPropertyCharacters = FilePropertyLimits.MaxOpenXmlPropertyCharacters)
        {
            ArgumentNullException.ThrowIfNull(packageStream);

            try
            {
                return ReadCorePropertiesCore(
                    packageStream,
                    maxFileSizeBytes,
                    maxEntries,
                    maxPartSizeBytes,
                    maxProcessedBytes,
                    maxPropertyCharacters);
            }
            catch (XmlException ex)
            {
                throw new InvalidDataException("The OpenXML package contains invalid XML.", ex);
            }
        }

        private static IReadOnlyDictionary<string, IConvertible> ReadCorePropertiesCore(
            Stream packageStream,
            long maxFileSizeBytes,
            int maxEntries,
            long maxPartSizeBytes,
            long maxProcessedBytes,
            int maxPropertyCharacters)
        {
            if (!packageStream.CanRead || !packageStream.CanSeek)
            {
                throw new InvalidDataException("The OpenXML package stream must be readable and seekable.");
            }

            if (packageStream.Length > maxFileSizeBytes)
            {
                throw new InvalidDataException($"The OpenXML package exceeds the maximum allowed size of {maxFileSizeBytes} bytes.");
            }

            long processedBytes = 0;
            packageStream.Position = 0;
            try
            {
                using ZipArchive archive = new(packageStream, ZipArchiveMode.Read, leaveOpen: true);
                if (archive.Entries.Count > maxEntries)
                {
                    throw new InvalidDataException($"The OpenXML package contains more than {maxEntries} entries.");
                }

                ZipArchiveEntry? relationshipsEntry = archive.GetEntry("_rels/.rels") ?? archive.GetEntry("/_rels/.rels");
                if (relationshipsEntry == null)
                {
                    return IFilePropertyGetter.EmptyProperties;
                }

                string? coreEntryName;
                using (Stream relationshipsStream = relationshipsEntry.Open())
                using (MemoryStream relationshipsBuffer = ReadBoundedPart(
                    relationshipsStream,
                    relationshipsEntry.Length,
                    maxPartSizeBytes,
                    maxProcessedBytes,
                    ref processedBytes))
                {
                    coreEntryName = FindCorePropertiesEntryName(relationshipsBuffer);
                }

                if (coreEntryName == null)
                {
                    return IFilePropertyGetter.EmptyProperties;
                }

                // The target is validated before this lookup, so an illegal path is never inflated.
                ZipArchiveEntry? coreEntry = archive.GetEntry(coreEntryName);
                if (coreEntry == null || coreEntry.Name.Length == 0)
                {
                    throw new InvalidDataException("The OpenXML core-properties part is missing.");
                }

                using Stream coreStream = coreEntry.Open();
                using MemoryStream coreBuffer = ReadBoundedPart(
                    coreStream,
                    coreEntry.Length,
                    maxPartSizeBytes,
                    maxProcessedBytes,
                    ref processedBytes);
                return ExtractCoreProperties(coreBuffer, maxPropertyCharacters);
            }
            finally
            {
                packageStream.Position = 0;
            }
        }

        private static string? FindCorePropertiesEntryName(Stream relationshipsXml)
        {
            using XmlReader reader = XmlReader.Create(relationshipsXml, CreateXmlReaderSettings());
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element
                    || reader.LocalName != "Relationship"
                    || reader.NamespaceURI != RelationshipsNamespace)
                {
                    continue;
                }

                if (!string.Equals(reader.GetAttribute("Type"), CorePropertiesRelationshipType, StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(reader.GetAttribute("TargetMode"), "External", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("The OpenXML core-properties target is external.");
                }

                return ResolveEntryName(reader.GetAttribute("Target"));
            }

            return null;
        }

        private static string ResolveEntryName(string? target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new InvalidDataException("The OpenXML core-properties target is missing.");
            }

            string decoded;
            try
            {
                decoded = Uri.UnescapeDataString(target);
            }
            catch (UriFormatException ex)
            {
                throw new InvalidDataException("The OpenXML core-properties target is outside the package.", ex);
            }

            if (decoded.Contains(':', StringComparison.Ordinal)
                || decoded.Contains('\\', StringComparison.Ordinal)
                || decoded.Contains("//", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The OpenXML core-properties target is outside the package.");
            }

            string relative = decoded.TrimStart('/');
            if (relative.Length == 0)
            {
                throw new InvalidDataException("The OpenXML core-properties target is outside the package.");
            }

            string[] segments = relative.Split('/');
            foreach (string segment in segments)
            {
                if (segment.Length == 0 || segment == "." || segment == "..")
                {
                    throw new InvalidDataException("The OpenXML core-properties target is outside the package.");
                }
            }

            return string.Join('/', segments);
        }

        private static MemoryStream ReadBoundedPart(
            Stream source,
            long? declaredLength,
            long maxPartSizeBytes,
            long maxProcessedBytes,
            ref long processedBytes)
        {
            if (declaredLength > maxPartSizeBytes)
            {
                throw new InvalidDataException($"An OpenXML part exceeds the maximum allowed size of {maxPartSizeBytes} bytes.");
            }

            int initialCapacity = declaredLength is > 0 and <= int.MaxValue ? (int)declaredLength.Value : 0;
            MemoryStream buffer = new(initialCapacity);
            byte[] chunk = new byte[81_920];
            long partBytes = 0;

            try
            {
                while (true)
                {
                    int bytesRead = source.Read(chunk, 0, chunk.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    partBytes += bytesRead;
                    processedBytes += bytesRead;
                    if (partBytes > maxPartSizeBytes)
                    {
                        throw new InvalidDataException($"An OpenXML part exceeds the maximum allowed size of {maxPartSizeBytes} bytes.");
                    }

                    if (processedBytes > maxProcessedBytes)
                    {
                        throw new InvalidDataException($"The parsed OpenXML content exceeds the maximum allowed size of {maxProcessedBytes} bytes.");
                    }

                    buffer.Write(chunk, 0, bytesRead);
                }

                buffer.Position = 0;
                return buffer;
            }
            catch
            {
                buffer.Dispose();
                throw;
            }
        }

        private static IReadOnlyDictionary<string, IConvertible> ExtractCoreProperties(
            Stream xmlStream,
            int maxPropertyCharacters)
        {
            const string DublinCoreNamespace = "http://purl.org/dc/elements/1.1/";
            const string DublinCoreTermsNamespace = "http://purl.org/dc/terms/";

            string? title = null;
            string? creator = null;
            DateTime? created = null;

            using XmlReader reader = XmlReader.Create(xmlStream, CreateXmlReaderSettings());
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.Depth != 1)
                {
                    continue;
                }

                string localName = reader.LocalName;
                string namespaceUri = reader.NamespaceURI;
                string value = ReadElementValueBounded(reader, maxPropertyCharacters);

                if (namespaceUri == DublinCoreNamespace && localName == "title")
                {
                    title = value;
                }
                else if (namespaceUri == DublinCoreNamespace && localName == "creator")
                {
                    creator = value;
                }
                else if (namespaceUri == DublinCoreTermsNamespace && localName == "created" && !string.IsNullOrWhiteSpace(value))
                {
                    if (!DateTime.TryParse(
                        value,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                        out DateTime parsedCreated))
                    {
                        throw new InvalidDataException("The OpenXML Created property is not a valid date and time.");
                    }

                    created = parsedCreated;
                }
            }

            Dictionary<string, IConvertible>? properties = null;
            if (!string.IsNullOrWhiteSpace(title))
            {
                properties = new Dictionary<string, IConvertible>(3)
                {
                    { "Title", title }
                };
            }

            if (created != null)
            {
                properties ??= new Dictionary<string, IConvertible>(3);
                properties.Add("Created", created.Value);
            }

            if (!string.IsNullOrWhiteSpace(creator))
            {
                properties ??= new Dictionary<string, IConvertible>(3);
                properties.Add("Creator", creator);
            }

            return properties ?? IFilePropertyGetter.EmptyProperties;
        }

        private static string ReadElementValueBounded(XmlReader reader, int maxPropertyCharacters)
        {
            if (reader.IsEmptyElement)
            {
                return string.Empty;
            }

            int elementDepth = reader.Depth;
            int propertyCharacters = 0;
            char[] valueBuffer = new char[1_024];
            StringBuilder value = new(Math.Min(maxPropertyCharacters, valueBuffer.Length));

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == elementDepth)
                {
                    break;
                }

                if (!IsTextNode(reader.NodeType))
                {
                    continue;
                }

                int charactersRead;
                while ((charactersRead = reader.ReadValueChunk(valueBuffer, 0, valueBuffer.Length)) > 0)
                {
                    propertyCharacters += charactersRead;
                    if (propertyCharacters > maxPropertyCharacters)
                    {
                        throw new InvalidDataException($"An OpenXML core property exceeds the maximum allowed length of {maxPropertyCharacters} characters.");
                    }

                    value.Append(valueBuffer, 0, charactersRead);
                }
            }

            return value.ToString();
        }

        private static XmlReaderSettings CreateXmlReaderSettings()
        {
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                MaxCharactersInDocument = FilePropertyLimits.MaxOpenXmlCharacters,
                XmlResolver = null
            };
        }

        private static bool IsTextNode(XmlNodeType nodeType)
        {
            return nodeType is XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.Whitespace or XmlNodeType.SignificantWhitespace;
        }
    }
}
