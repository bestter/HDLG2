using Serilog.Core;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace HDLG_winforms
{
    /// <summary>
    /// Directory browser
    /// </summary>
    public class DirectoryBrowser
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger log;

        /// <summary>
        /// Css content
        /// </summary>
        private string? CssContent;

        public DirectoryBrowser(Logger log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            CssContent = null;
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
            using FileStream fileStream = new(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using StreamWriter sw = new (fileStream, encoding, 4096, false);
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
                        DateTime? dtValue = property.Value as DateTime?;
                        if (dtValue != null && dtValue.HasValue)
                        {
                            await writer.WriteElementStringAsync(null, property.Key, null, dtValue.Value.ToString("O", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            var value = property.Value.ToString(CultureInfo.InvariantCulture);
                            await writer.WriteElementStringAsync(null, property.Key, null,value);
                        }
                    }
                }
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }
        #endregion

        #region HTML

        private static string GetGoogleFontHeader()
        {
            StringBuilder builder = new();
            builder.AppendLine(@"<link rel=""preconnect"" href=""https://fonts.googleapis.com"">");
            builder.AppendLine(@"<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>");
            builder.AppendLine(@"<link href=""https://fonts.googleapis.com/css2?family=Roboto+Serif:ital,opsz,wght@0,8..144,400;0,8..144,700;1,8..144,400;1,8..144,700&family=Source+Sans+Pro:ital,wght@0,400;0,700;1,400;1,700&display=swap"" rel=""stylesheet"">");
            return builder.ToString().Trim();
        }

        private string GetCss()
        {
            if (string.IsNullOrEmpty(CssContent))
            {
                var directory = Path.GetDirectoryName(Application.ExecutablePath);
                FileInfo cssFile = new(Path.Combine(directory ?? string.Empty, "hdlg.css"));
                if (cssFile.Exists)
                {
                    StringBuilder stringBuilder = new();

                    using FileStream stream = cssFile.OpenRead();
                    using StreamReader reader = new(stream);
                    var iCss = reader.ReadToEnd().Trim();

                    if (!string.IsNullOrWhiteSpace(iCss))
                    {
                        stringBuilder.AppendLine("<style>");
                        stringBuilder.AppendLine(iCss);
                        stringBuilder.AppendLine("</style>");
                        var css = stringBuilder.ToString().Trim();
                        CssContent = css;
                    }
                }
                else
                {
                    log.Warning($"CSS file does not exist at path {directory}");
                }
            }
            return CssContent ?? string.Empty;

        }

        public async Task SaveAsHTMLAsync(string filePath, Directory directory)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"'{nameof(filePath)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(filePath));
            }

            FileInfo fileInfo = new(filePath);

            var encoding = Encoding.UTF8;
            
            string? version = typeof(DirectoryBrowser).Assembly.GetName().Version?.ToString();

            using FileStream fileStream = new(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using StreamWriter sw = new(fileStream, encoding, 4096, false);            
            var title = $"HTML Directory list generator {directory.Path} {version} {DateTimeOffset.Now.ToString("F", CultureInfo.CurrentCulture)}";
            await sw.WriteLineAsync("<!DOCTYPE html>");

            await sw.WriteLineAsync($"<html lang=\"{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}\">");
            await sw.WriteLineAsync("<head>");
            await sw.WriteLineAsync($"<meta charset=\"{encoding.WebName}\">");
            await sw.WriteLineAsync("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            await sw.WriteLineAsync("<meta name=\"robots\" content=\"noindex, nofollow\">");
            await sw.WriteLineAsync("<meta name=\"rating\" content=\"general\">");
            await sw.WriteAsync($"<title>{title}</title>");
            await sw.WriteLineAsync(GetGoogleFontHeader());
            await sw.WriteLineAsync(GetCss());
            await sw.WriteLineAsync();
            await sw.WriteLineAsync("</head>");

            await sw.WriteLineAsync("<body>");

            await sw.WriteLineAsync("<div class=\"Hdlg\">");

            await sw.WriteLineAsync("<div class=\"version\">");
            await sw.WriteLineAsync($"<h1>{title}</h1>");
            await sw.WriteLineAsync("<span>Version</span>");
            await sw.WriteLineAsync($"<span>{version}</span>");
            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("<div class=\"directoryHeader\">");
            await sw.WriteLineAsync("<span>Directory</span>");
            await sw.WriteLineAsync($"<h2>{directory.Path}</h2>");
            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("<div class=\"dateTime headerContent\">");
            await sw.WriteLineAsync("<span class=\"headerContentTitle\">DateTime</span>");
            await sw.WriteLineAsync($"<span class=\"headerContentData\">{DateTime.Now.ToString("F", CultureInfo.CurrentCulture)}</span>");
            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("<div class=\"directoriesCount headerContent\">");
            await sw.WriteLineAsync("<span class=\"headerContentTitle\">DirectoriesCount</span>");
            await sw.WriteLineAsync($"<span class=\"headerContentData\">{DirectoriesCount(directory).ToString(CultureInfo.CurrentCulture)}</span>");
            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("<div class=\"filesCount headerContent\">");
            await sw.WriteLineAsync("<span class=\"headerContentTitle\">FilesCount</span>");
            await sw.WriteLineAsync($"<span class=\"headerContentData\">{FilesCount(directory).ToString(CultureInfo.CurrentCulture)}</span>");
            await sw.WriteLineAsync("</div>");
            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("<div class=\"directories\">");

            await WritHtmlDirectoryAsync(sw, directory);

            await sw.WriteLineAsync("</div>");

            await sw.WriteLineAsync("</body>");

            await sw.WriteAsync("</html>");

            await sw.FlushAsync();
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
            await writer.WriteLineAsync($"<span class=\"creationTime\">{directory.CreationTime.ToString("F", CultureInfo.CurrentCulture)}</span>");

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
                await writer.WriteLineAsync("<div class=\"files\">");
                foreach (File file in directory.Files)
                {
                    await WriteHtmlFileAsync(writer, file);
                }
                await writer.WriteLineAsync("</div>");
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
        private async Task WriteHtmlFileAsync(TextWriter writer, File file)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            log.Verbose($"{nameof(WriteHtmlFileAsync)} {file}");

            await writer.WriteLineAsync("<details class=\"file\">");

            await writer.WriteLineAsync("<summary>");
            await writer.WriteLineAsync($"<a href=\"{file.Path}\">{file.Name}</a>");                        
            await writer.WriteLineAsync("</summary>");

            await writer.WriteLineAsync("<div>");
            await writer.WriteLineAsync($"<span class=\"size\">{file.Size.ToString(CultureInfo.CurrentCulture)} kb</span>");
            await writer.WriteLineAsync($"<span class=\"creationTime\">{file.CreationTime.ToString("F", CultureInfo.CurrentCulture)}</span>");
            if (file.Properties != null)
            {
                await writer.WriteLineAsync("<p class=\"extentedProperties\">");
                
                foreach (var property in file.Properties)
                {
                    if (!string.IsNullOrWhiteSpace(property.Key) && property.Value != null)
                    {
                        await writer.WriteLineAsync("<p class=\"extentedProperty\">");
                        await writer.WriteLineAsync($"<span>{property.Key}</span>");

                        DateTime? dtValue = property.Value as DateTime?;
                        if (dtValue != null && dtValue.HasValue)
                        {
                            await writer.WriteLineAsync($"<span>{dtValue.Value.ToString("F", CultureInfo.CurrentCulture)}</span>");
                        }
                        else
                        {
                            var value = property.Value.ToString(CultureInfo.CurrentCulture);
                            await writer.WriteLineAsync($"<span>{value}</span>");
                        }
                        
                        await writer.WriteLineAsync("</p>");

                    }
                }
                await writer.WriteLineAsync("</p>");
            }

            await writer.WriteLineAsync("</div>");
            await writer.WriteLineAsync("</details>");
        }

        #endregion
    }
}
