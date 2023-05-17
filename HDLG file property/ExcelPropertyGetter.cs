using DocumentFormat.OpenXml.Packaging;
using Serilog;
using System.Globalization;

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
            Dictionary<string, IConvertible> properties = new();
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
            var extension = fileInfo.Extension.ToLowerInvariant();
            return extension == ".xlsx";
        }
    }
}
