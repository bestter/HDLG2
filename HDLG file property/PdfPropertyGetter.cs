#pragma warning disable CA1062
/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;
using UglyToad.PdfPig;

namespace HdlgFileProperty
{
    public class PdfPropertyGetter : IFilePropertyGetter
    {
        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyDictionary<string, IConvertible> GetFileProperties(FileInfo fileInfo)
        {
            Dictionary<string, IConvertible>? properties = null;
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            try
            {
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException("File not found", fileInfo.FullName);
                }
                using PdfDocument document = PdfDocument.Open(fileInfo.FullName);
                string? title = document.Information.Title;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    properties = new Dictionary<string, IConvertible>();
                    properties.Add("Title", title);
                }
            }
            catch (IOException ioe)
            {
                Logger?.Error(ioe, "Cannot read file {Path}", fileInfo.FullName);
            }
            catch (Exception e) when (e.GetType().Name.Contains("PdfDocumentEncryptedException", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger?.Warning(e, "File {Path} is password protected and cannot be read", fileInfo.FullName);
            }
            catch (Exception e)
            {
                Logger?.Warning(e, "Cannot read properties from file {Path}", fileInfo.FullName);
            }
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale

            return (IReadOnlyDictionary<string, IConvertible>?)properties ?? IFilePropertyGetter.EmptyProperties;
        }

        public bool IsSupportedFile(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            return path.AsSpan().EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }
    }
}
