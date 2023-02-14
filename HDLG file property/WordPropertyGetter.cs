﻿using DocumentFormat.OpenXml.Packaging;
using System.Globalization;

namespace HdlgFileProperty
{
    public class WordPropertyGetter : IFilePropertyGetter
    {
        public Dictionary<string, string> GetFileProperties(string path)
        {
            Dictionary<string, string> properties = new();
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(path, true))
            {
                properties.Add("Title", wordDoc.PackageProperties.Title);
                DateTime? created = wordDoc.PackageProperties.Created;
                if (created != null)
                {
                    properties.Add("Created", created.Value.ToString("O", CultureInfo.InvariantCulture));
                }
                properties.Add("Creator", wordDoc.PackageProperties.Creator);
            }

            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToLowerInvariant();
            return extension == ".docx";
        }
    }
}
