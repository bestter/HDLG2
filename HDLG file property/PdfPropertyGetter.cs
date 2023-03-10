using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace HdlgFileProperty
{
    public class PdfPropertyGetter : IFilePropertyGetter
    {
        public Dictionary<string, string> GetFileProperties(string path)
        {
            Dictionary<string, string> properties = new();
            try
            {
                DocumentProperties documentProperties = new();
                using PdfDocument pdfDoc = new(new PdfReader(path), documentProperties);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);
                if (form != null)
                {
                    IDictionary<string, PdfFormField> fields = form.GetFormFields();
                    if (fields.Any() && fields.TryGetValue("name", out PdfFormField toSet))
                    {
                        if (toSet != null)
                        {
                            properties.Add("Title", toSet.GetValueAsString());
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
