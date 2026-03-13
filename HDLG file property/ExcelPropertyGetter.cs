/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
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

        public Dictionary<string, IConvertible> GetFileProperties(string path)
        {
            Dictionary<string, IConvertible> properties = [];
            using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(path, false))
            {
                if (!string.IsNullOrWhiteSpace(excelDoc.PackageProperties.Title))
                {
                    properties.Add("Title", excelDoc.PackageProperties.Title);
                }
                DateTime? created = excelDoc.PackageProperties.Created;
                if (created != null)
                {
                    properties.Add("Created", created.Value);
                }
                if (!string.IsNullOrWhiteSpace(excelDoc.PackageProperties.Creator))
                {
                    properties.Add("Creator", excelDoc.PackageProperties.Creator);
                }
            }

            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToUpperInvariant();
            return extension == ".XLSX";
        }
    }
}
