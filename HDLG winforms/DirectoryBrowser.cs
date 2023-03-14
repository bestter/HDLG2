using Serilog.Core;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace HDLG_winforms
{
    /// <summary>
    /// Directory browser
    /// </summary>
    public class DirectoryBrowser
    {
        private readonly Logger log;

        public DirectoryBrowser(Logger log)
        {
            this.log = log;
        }

        #region XML

        /// <summary>
        /// Export directory content as XML
        /// </summary>
        /// <param name="filePath">Where to save the data</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task SaveAsXMLAsync(string filePath, Directory directory)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(filePath));
            }

            FileInfo fileInfo = new(filePath);

            var encoding = Encoding.UTF8;

            XmlWriterSettings settings = new()
            {
                Indent = true,
                Encoding = encoding,
                Async = true,
                IndentChars = "\t"
            };

            using (StreamWriter sw = new StreamWriter(fileInfo.OpenWrite(), encoding, 4096, false))
            {
                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    await writer.WriteStartDocumentAsync();

                    await writer.WriteStartElementAsync(null, "Hdlg", null);
                    await writer.WriteAttributeStringAsync(null, "Version", null, typeof(DirectoryBrowser).Assembly.GetName().Version?.ToString());
                    await writer.WriteElementStringAsync(null, "Directory", null, directory.Path);
                    await writer.WriteElementStringAsync(null, "DateTime", null, DateTime.Now.ToString("O", CultureInfo.InvariantCulture));

                    await writer.WriteElementStringAsync(null, "DirectoriesCount", null, DirectoriesCount(directory).ToString(CultureInfo.InvariantCulture));
                    await writer.WriteElementStringAsync(null, "FilesCount", null, FilesCount(directory).ToString(CultureInfo.InvariantCulture));

                    await WriteXmlDirectoryAsync(writer, directory);

                    await writer.WriteEndElementAsync();

                    await writer.WriteEndDocumentAsync();
                }
                await sw.FlushAsync();
            }
        }

        /// <summary>
        /// Count the total numbers of directories into <paramref name="directory"/> and is children
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static long DirectoriesCount(Directory directory)
        {
            long count = directory.DirectoriesCount;
            foreach (Directory d in directory.Directories)
            {
                count += DirectoriesCount(d);
            }
            return count;
        }

        /// <summary>
        /// Count the total numbers of files into <paramref name="directory"/> and is children
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static long FilesCount(Directory directory)
        {
            long count = directory.FilesCount;
            foreach (Directory d in directory.Directories)
            {
                count += FilesCount(d);
            }
            return count;
        }

        /// <summary>
        /// Write the content of a directory
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private async Task WriteXmlDirectoryAsync(XmlWriter writer, Directory directory)
        {
            log.Debug($"In {nameof(WriteXmlDirectoryAsync)} {nameof(Directory)} {directory}");
            await writer.WriteStartElementAsync(null, "Directory", null);
            await writer.WriteElementStringAsync(null, "Name", null, directory.Name);
            await writer.WriteElementStringAsync(null, "Path", null, directory.Path);
            await writer.WriteElementStringAsync(null, "CreationTime", null, directory.CreationTime.ToString("O", CultureInfo.InvariantCulture));
            if (directory.Directories.Any())
            {
                await writer.WriteStartElementAsync(null, "Directories", null);
                foreach (Directory d in directory.Directories)
                {
                    await WriteXmlDirectoryAsync(writer, d);
                }
                await writer.WriteEndElementAsync();
            }

            if (directory.Files.Any())
            {
                await writer.WriteStartElementAsync(null, "Files", null);
                foreach (File file in directory.Files)
                {
                    await WriteXmlFileAsync(writer, file);
                }
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }

        /// <summary>
        /// Write the content of a <paramref name="file"/> to the <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="file">File that content the data</param>
        /// <returns>A task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task WriteXmlFileAsync(XmlWriter writer, File file)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            log.Verbose($"{nameof(WriteXmlFileAsync)} {file}");

            await writer.WriteStartElementAsync(null, "File", null);

            await writer.WriteElementStringAsync(null, "Name", null, file.Name);
            await writer.WriteElementStringAsync(null, "Path", null, file.Path);
            await writer.WriteElementStringAsync(null, "Extension", null, file.Extension);
            await writer.WriteElementStringAsync(null, "Size", null, file.Size.ToString(CultureInfo.InvariantCulture));
            await writer.WriteElementStringAsync(null, "CreationTime", null, file.CreationTime.ToString("O", CultureInfo.InvariantCulture));


            if (file.Properties != null)
            {
                await writer.WriteStartElementAsync(null, "ExtentedProperties", null);
                foreach (var property in file.Properties)
                {
                    if (!string.IsNullOrWhiteSpace(property.Key) && property.Value != null)
                    {
                        await writer.WriteElementStringAsync(null, property.Key, null, property.Value);
                    }
                }
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }
        #endregion

        #region HTML

        public async Task SaveAsHTMLAsync(string filePath, Directory directory)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(filePath));
            }

            FileInfo fileInfo = new(filePath);

            var encoding = Encoding.UTF8;

            XmlWriterSettings settings = new()
            {
                Indent = true,
                Encoding = encoding,
                Async = true,
                IndentChars = "\t"
            };

            string? version = typeof(DirectoryBrowser).Assembly.GetName().Version?.ToString();

            using (StreamWriter sw = new StreamWriter(fileInfo.OpenWrite(), encoding, 4096, false))
            {

                await sw.WriteLineAsync("<!DOCTYPE html>");

                await sw.WriteLineAsync("<html>");
                await sw.WriteLineAsync("<head>");
                await sw.WriteAsync($"<title>HTML Directory list generator {directory.Name} {version}</title>");
                await sw.WriteLineAsync();
                await sw.WriteLineAsync("</head>");

                await sw.WriteLineAsync("<body>");

                await sw.WriteLineAsync("<div class=\"Hdlg\">");

                await sw.WriteLineAsync("<div class=\"version\">");
                await sw.WriteLineAsync("<span>Version</span>");
                await sw.WriteLineAsync($"<span>{version}</span>");
                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("<div class=\"directory\">");
                await sw.WriteLineAsync("<span>Directory</span>");
                await sw.WriteLineAsync($"<span>{directory.Path}</span>");
                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("<div class=\"dateTime\">");
                await sw.WriteLineAsync("<span>DateTime</span>");
                await sw.WriteLineAsync($"<span>{DateTime.Now.ToString("O", CultureInfo.InvariantCulture)}</span>");
                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("<div class=\"directoriesCount\">");
                await sw.WriteLineAsync("<span>DirectoriesCount</span>");
                await sw.WriteLineAsync($"<span>{DirectoriesCount(directory).ToString(CultureInfo.InvariantCulture)}</span>");
                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("<div class=\"filesCount\">");
                await sw.WriteLineAsync("<span>FilesCount</span>");
                await sw.WriteLineAsync($"<span>{FilesCount(directory).ToString(CultureInfo.InvariantCulture)}</span>");
                await sw.WriteLineAsync("</div>");       
                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("<div class=\"directories\">");

                await WritHtmlDirectoryAsync(sw, directory);

                await sw.WriteLineAsync("</div>");

                await sw.WriteLineAsync("</body>");

                await sw.WriteAsync("</html>");

                await sw.FlushAsync();
            }
        }


        /// <summary>
        /// Write the content of a directory
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private async Task WritHtmlDirectoryAsync(TextWriter writer, Directory directory)
        {
            log.Debug($"In {nameof(WritHtmlDirectoryAsync)} {nameof(Directory)} {directory}");


            await writer.WriteLineAsync("<div class=\"directory\">");
            await writer.WriteLineAsync($"<span class=\"name\">{directory.Name}</span>");
            await writer.WriteLineAsync($"<span class=\"path\">{directory.Path}</span>");
            await writer.WriteLineAsync($"<span class=\"creationTime\">{directory.CreationTime.ToString("O", CultureInfo.InvariantCulture)}</span>");

            if (directory.Directories.Any())
            {
                await writer.WriteLineAsync("<div class=\"directories\">");
                foreach (Directory d in directory.Directories)
                {
                    await WritHtmlDirectoryAsync(writer, d);
                }
                await writer.WriteLineAsync("</div>");
            }

            if (directory.Files.Any())
            {
                foreach (File file in directory.Files)
                {

                }
            }

            await writer.WriteLineAsync("</div>");
        }

        /// <summary>
        /// Write the content of a <paramref name="file"/> to the <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="file">File that content the data</param>
        /// <returns>A task</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task WriteHtmlFileAsync(StringWriter writer, File file)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            log.Verbose($"{nameof(WriteHtmlFileAsync)} {file}");

            await writer.WriteLineAsync("<div class=\"file\">");

            await writer.WriteLineAsync($"<span class=\"name\">{file.Name}</span>");
            await writer.WriteLineAsync($"<span class=\"path\">{file.Path}</span>");
            await writer.WriteLineAsync($"<span class=\"extension\">{file.Extension}</span>");
            await writer.WriteLineAsync($"<span class=\"size\">{file.Size.ToString(CultureInfo.InvariantCulture)}</span>");
            await writer.WriteLineAsync($"<span class=\"creationTime\">{file.CreationTime.ToString("O", CultureInfo.InvariantCulture)}</span>");
            

            if (file.Properties != null)
            {
                await writer.WriteLineAsync("<div class=\"extentedProperties\">");
                
                foreach (var property in file.Properties)
                {
                    if (!string.IsNullOrWhiteSpace(property.Key) && property.Value != null)
                    {
                        await writer.WriteLineAsync("<div class=\"extentedProperty\">");
                        await writer.WriteLineAsync($"<span>{property.Key}</span>");
                        await writer.WriteLineAsync($"<span>{property.Value}</span>");
                        await writer.WriteLineAsync("</div>");

                    }
                }
                await writer.WriteLineAsync("</div>");
            }

            await writer.WriteLineAsync("</div>");
        }

        #endregion
    }
}
