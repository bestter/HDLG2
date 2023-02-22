using Serilog.Core;
using System.Globalization;
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

        /// <summary>
        /// Export directory content as XML
        /// </summary>
        /// <param name="filePath">Where to save the data</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task SaveAsXMLAsync(string filePath, Directory directory, CancellationToken cancellationToken)
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
                    await writer.WriteElementStringAsync(null, "filesCount", null, FilesCount(directory).ToString(CultureInfo.InvariantCulture));

                    await WriteDirectoryAsync(writer, directory);

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
        private async Task WriteDirectoryAsync(XmlWriter writer, Directory directory)
        {
            log.Debug($"In {nameof(WriteDirectoryAsync)} {nameof(Directory)} {directory}");
            await writer.WriteStartElementAsync(null, "Directory", null);
            await writer.WriteElementStringAsync(null, "Name", null, directory.Name);
            await writer.WriteElementStringAsync(null, "Path", null, directory.Path);
            await writer.WriteElementStringAsync(null, "CreationTime", null, directory.CreationTime.ToString("O", CultureInfo.InvariantCulture));
            if (directory.Directories.Any())
            {
                await writer.WriteStartElementAsync(null, "Directories", null);
                foreach (Directory d in directory.Directories)
                {
                    await WriteDirectoryAsync(writer, d);
                }
                await writer.WriteEndElementAsync();
            }

            if (directory.Files.Any())
            {
                await writer.WriteStartElementAsync(null, "Files", null);
                foreach (File file in directory.Files)
                {
                    await WriteFileAsync(writer, file);
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
        private async Task WriteFileAsync(XmlWriter writer, File file)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            log.Verbose($"{nameof(WriteFileAsync)} {file}");

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
    }
}
