using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Serilog;

namespace HdlgFileProperty
{
    public class PdfPropertyGetter : IFilePropertyGetter
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
                DocumentProperties documentProperties = new();
                using PdfDocument pdfDoc = new(new PdfReader(path), documentProperties);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);
                if (form != null)
                {
                    IDictionary<string, PdfFormField> fields = form.GetAllFormFields();
                    if (fields.Any())
                    {
                        if (fields.TryGetValue("name", out var toSet))
                        { 
                            if (toSet != null)
                            {
                                properties.Add("Title", toSet.GetValueAsString());
                            }
                        }
                    }
                }
            }
            catch (System.IO.IOException)
            { }
            catch (iText.Kernel.Exceptions.BadPasswordException)
            { }

            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToLowerInvariant();
            return extension == ".pdf";
        }
    }
}
