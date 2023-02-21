using iText.Forms.Fields;
using iText.Forms;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdlgFileProperty
{
    public class PdfPropertyGetter : IFilePropertyGetter
    {
        public Dictionary<string, string> GetFileProperties(string path)
        {
            Dictionary<string, string> properties = new();
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(path)))
            {
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetFormFields();
                if (fields.TryGetValue("name", out PdfFormField toSet))
                {
                    if (toSet != null)
                    {
                        properties.Add("Title", toSet.GetValueAsString());
                    }
                }
            }

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
