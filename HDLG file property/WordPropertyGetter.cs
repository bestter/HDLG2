using DocumentFormat.OpenXml.Packaging;
using Serilog;
using System.Globalization;

namespace HdlgFileProperty
{
    public class WordPropertyGetter : IFilePropertyGetter
    {
        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Dictionary<string, IConvertible> GetFileProperties(string path)
        {
            Dictionary<string, IConvertible> properties = new();
            try
            {
                using WordprocessingDocument wordDoc = WordprocessingDocument.Open(path, false);
                if (!string.IsNullOrWhiteSpace(wordDoc.PackageProperties.Title))
                {
                    properties.Add("Title", wordDoc.PackageProperties.Title);
                }
                DateTime? created = wordDoc.PackageProperties.Created;
                if (created != null)
                {
                    properties.Add("Created", created.Value);
                }
                if (!string.IsNullOrWhiteSpace(wordDoc.PackageProperties.Creator))
                {
                    properties.Add("Creator", wordDoc.PackageProperties.Creator);
                }
            }
            catch (IOException)
            {
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
