﻿using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace HdlgFileProperty
{
    public class ExcelPropertyGetter : IFilePropertyGetter
    {
        public Dictionary<string, string> GetFileProperties(string path)
        {
            Dictionary<string, string> properties = new();
            using (SpreadsheetDocument excelDoc = SpreadsheetDocument.Open(path, true))
            {
                properties.Add("Title", excelDoc.PackageProperties.Title);
                DateTime? created = excelDoc.PackageProperties.Created;
                if (created != null)
                {
                    properties.Add("Created", created.Value.ToString("O", CultureInfo.InvariantCulture));
                }
                properties.Add("Creator", excelDoc.PackageProperties.Creator);
            }

            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToLowerInvariant();
            return extension == ".xlsx";
        }
    }
}