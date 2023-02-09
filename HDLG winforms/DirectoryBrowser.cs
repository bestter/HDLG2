using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml;

namespace HDLG_winforms
{
    internal class DirectoryBrowser
    {
        
        /// <summary>
        /// Export directory content as XML
        /// </summary>
        /// <param name="filePath">Where to save the data</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task SaveAsXMLAsync(string filePath, Directory directory)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(filePath));
            }

            FileInfo fileInfo = new(filePath);

            XmlWriterSettings settings = new()
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                Async = true,
                IndentChars = "\t"
            };

            using (Stream stream = fileInfo.OpenWrite())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    await writer.WriteStartDocumentAsync();

                    await writer.WriteStartElementAsync(null, "Hdlg", null);
                    await writer.WriteElementStringAsync("null", "Directory", null, directory.Path);
                    await writer.WriteElementStringAsync("null", "DateTime", null, DateTime.Now.ToString("O", CultureInfo.InvariantCulture));

                    await WriteDirectoriesAsync(writer, directory.Directories);

                    await writer.WriteEndElementAsync();

                    await writer.WriteEndDocumentAsync();
                }
                await stream.FlushAsync();

            }

        }

        private static async Task WriteDirectoriesAsync(XmlWriter writer, IEnumerable<Directory> directories)
        {
            if (directories.Any())
            {
                await writer.WriteStartElementAsync(null, "Directories", null);

                foreach (Directory directory in directories)
                {
                    await writer.WriteStartElementAsync(null, "Directory", null);
                    await writer.WriteElementStringAsync(null, "Name", null, directory.Name);
                    await writer.WriteElementStringAsync(null, "Path", null, directory.Path);
                    await writer.WriteElementStringAsync(null, "CreationTime", null, directory.CreationTime.ToString("O", CultureInfo.InvariantCulture));
                    if (directory.Directories.Any())
                    {
                        await WriteDirectoriesAsync(writer, directory.Directories);
                    }

                    if (directory.Files.Any())
                    {
                        foreach (File file in directory.Files)
                        {
                            await file.WriteFileAsync(writer);
                        }
                    }

                    await writer.WriteEndElementAsync();
                }

                await writer.WriteEndElementAsync();
            }
        }


    }
}
