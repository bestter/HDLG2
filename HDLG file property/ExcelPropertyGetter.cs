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
    public class ExcelPropertyGetter : IFilePropertyGetter
    {
        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyDictionary<string, IConvertible> GetFileProperties(FileInfo fileInfo)
        {
            ArgumentNullException.ThrowIfNull(fileInfo);
            string path = fileInfo.FullName;
            Dictionary<string, IConvertible>? properties = null;
            try
            {
                using FileStream stream = new(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(stream, false);
                var packageProperties = excelDoc.PackageProperties;

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
                Logger?.Warning(ex, "Could not open Excel file or extract properties for {Path}", fileInfo.FullName);
            }
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            catch (Exception ex)
            {
                Logger?.Warning(ex, "Cannot read properties from file {Path}", fileInfo.FullName);
            }
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale

            return properties ?? IFilePropertyGetter.EmptyProperties;
        }

        public bool IsSupportedFile(FileInfo fileInfo)
        {
            if (fileInfo == null) return false;
            string path = fileInfo.FullName;
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            return path.AsSpan().EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
