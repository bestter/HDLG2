#pragma warning disable CA1062
/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using DocumentFormat.OpenXml.Packaging;
using Serilog;

namespace HdlgFileProperty
{
    public class WordPropertyGetter : IFilePropertyGetter
    {
        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyDictionary<string, IConvertible> GetFileProperties(FileInfo fileInfo)
        {
            Dictionary<string, IConvertible>? properties = null;
            try
            {
                using FileStream stream = new(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false);
                var packageProperties = wordDoc.PackageProperties;

                if (!string.IsNullOrWhiteSpace(packageProperties.Title))
                {
                    properties = new Dictionary<string, IConvertible>(3);
                    properties.Add("Title", packageProperties.Title);
                }

                DateTime? created = packageProperties.Created;
                if (created != null)
                {
                    properties ??= new Dictionary<string, IConvertible>(3);
                    properties.Add("Created", created.Value);
                }

                if (!string.IsNullOrWhiteSpace(packageProperties.Creator))
                {
                    properties ??= new Dictionary<string, IConvertible>(3);
                    properties.Add("Creator", packageProperties.Creator);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is OpenXmlPackageException || ex is FileFormatException)
            {
                Logger?.Warning(ex, "Could not open Word file or extract properties for {Path}", fileInfo.FullName);
            }
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Cannot read properties from file {Path}", fileInfo.FullName);
            }
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale

            return (IReadOnlyDictionary<string, IConvertible>?)properties ?? IFilePropertyGetter.EmptyProperties;
        }

        public bool IsSupportedFile(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            return path.AsSpan().EndsWith(".docx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
