/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
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
                using var reader = new PdfReader(path);
                using PdfDocument pdfDoc = new(reader, documentProperties);
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
            catch (IOException ioe)
            {
                Logger?.Error(ioe, $"Cannot read file {path}");

            }
            catch (iText.Kernel.Exceptions.BadPasswordException e)
            { 
                Logger?.Warning(e, $"File {path} is password protected and cannot be read");
            }

            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToUpperInvariant();
            return extension == ".PDF";
        }
    }
}
