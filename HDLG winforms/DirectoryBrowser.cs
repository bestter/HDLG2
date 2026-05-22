/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;
using System.Globalization;
using System.Net;
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
		private readonly ILogger log;

		/// <summary>
		/// Css content
		/// </summary>
		private string? CssContent;

		public DirectoryBrowser (ILogger log)
		{
			this.log = log ?? throw new ArgumentNullException( nameof( log ) );
			CssContent = null;
		}

		#region XML

		/// <summary>
		/// Export directory content as XML
		/// </summary>
		/// <param name="filePath">Where to save the data</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public async Task SaveAsXMLAsync (string filePath, HdlgDirectory directory)
		{
			if (string.IsNullOrWhiteSpace( filePath ))
			{
				throw new ArgumentException( $"'{nameof( filePath )}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof( filePath ) );
			}

			ArgumentNullException.ThrowIfNull( directory );
			FileInfo fileInfo = new( filePath );

			var encoding = Encoding.UTF8;

			XmlWriterSettings settings = new( )
			{
				Indent = true,
				Encoding = encoding,
				Async = true,
				IndentChars = "\t"
			};
			using FileStream fileStream = new( fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None );
			using StreamWriter sw = new( fileStream, encoding, 4096, false );
			using (XmlWriter writer = XmlWriter.Create( sw, settings ))
			{
				await writer.WriteStartDocumentAsync( ).ConfigureAwait( false );

				await writer.WriteStartElementAsync( null, "Hdlg", null ).ConfigureAwait( false );
				await writer.WriteAttributeStringAsync( null, "Version", null, typeof( DirectoryBrowser ).Assembly.GetName( ).Version?.ToString( ) ).ConfigureAwait( false );
				await writer.WriteElementStringAsync( null, "Directory", null, directory.Path ).ConfigureAwait( false );
				await writer.WriteElementStringAsync( null, "DateTime", null, DateTime.Now.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );

				await writer.WriteElementStringAsync( null, "DirectoriesCount", null, DirectoriesCount( directory ).ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
				await writer.WriteElementStringAsync( null, "FilesCount", null, FilesCount( directory ).ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );

				await WriteXmlDirectoryAsync( writer, directory ).ConfigureAwait( false );

				await writer.WriteEndElementAsync( ).ConfigureAwait( false );

				await writer.WriteEndDocumentAsync( ).ConfigureAwait( false );
			}
			await sw.FlushAsync( ).ConfigureAwait( false );
		}

		/// <summary>
		/// Count the total numbers of directories into <paramref name="directory"/> and is children
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		private static long DirectoriesCount (HdlgDirectory directory)
		{
			long count = directory.DirectoriesCount;
			foreach (HdlgDirectory d in directory.Directories)
			{
				count += DirectoriesCount( d );
			}
			return count;
		}

		/// <summary>
		/// Count the total numbers of files into <paramref name="directory"/> and is children
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		private static long FilesCount (HdlgDirectory directory)
		{
			long count = directory.FilesCount;
			foreach (HdlgDirectory d in directory.Directories)
			{
				count += FilesCount( d );
			}
			return count;
		}

		/// <summary>
		/// Write the content of a directory
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="directory"></param>
		/// <returns></returns>
		private async Task WriteXmlDirectoryAsync (XmlWriter writer, HdlgDirectory directory)
		{
			log.Debug( $"In {nameof( WriteXmlDirectoryAsync )} {nameof( HdlgDirectory )} {directory}" );
			await writer.WriteStartElementAsync( null, "Directory", null ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Name", null, directory.Name ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Path", null, directory.Path ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "CreationTime", null, directory.CreationTime.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
			if (directory.Directories.Any( ))
			{
				await writer.WriteStartElementAsync( null, "Directories", null ).ConfigureAwait( false );
				foreach (HdlgDirectory d in directory.Directories)
				{
					await WriteXmlDirectoryAsync( writer, d ).ConfigureAwait( false );
				}
				await writer.WriteEndElementAsync( ).ConfigureAwait( false );
			}

			if (directory.Files.Any( ))
			{
				await writer.WriteStartElementAsync( null, "Files", null ).ConfigureAwait( false );
				foreach (HdlgFile file in directory.Files)
				{
					await WriteXmlFileAsync( writer, file ).ConfigureAwait( false );
				}
				await writer.WriteEndElementAsync( ).ConfigureAwait( false );
			}

			await writer.WriteEndElementAsync( ).ConfigureAwait( false );
		}

		/// <summary>
		/// Write the content of a <paramref name="file"/> to the <paramref name="writer"/>
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		/// <param name="file">File that content the data</param>
		/// <returns>A task</returns>
		/// <exception cref="ArgumentNullException"></exception>
		private async Task WriteXmlFileAsync (XmlWriter writer, HdlgFile file)
		{
			if (writer is null)
			{
				throw new ArgumentNullException( nameof( writer ) );
			}

			log.Verbose( $"{nameof( WriteXmlFileAsync )} {file}" );

			await writer.WriteStartElementAsync( null, "File", null ).ConfigureAwait( false );

			await writer.WriteElementStringAsync( null, "Name", null, file.Name ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Path", null, file.Path ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Extension", null, file.Extension ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Size", null, file.Size.ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "CreationTime", null, file.CreationTime.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );


			if (file.Properties != null)
			{
				await writer.WriteStartElementAsync( null, "ExtentedProperties", null ).ConfigureAwait( false );
				foreach (var property in file.Properties)
				{
					if (!string.IsNullOrWhiteSpace( property.Key ) && property.Value != null)
					{
						if (property.Value is DateTime dtValue)
						{
							await writer.WriteElementStringAsync( null, property.Key, null, dtValue.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
						}
						else
						{
							var value = property.Value.ToString( CultureInfo.InvariantCulture );
							await writer.WriteElementStringAsync( null, property.Key, null, value ).ConfigureAwait( false );
						}
					}
				}
				await writer.WriteEndElementAsync( ).ConfigureAwait( false );
			}

			await writer.WriteEndElementAsync( ).ConfigureAwait( false );
		}
		#endregion

		#region HTML

		private static string GetGoogleFontHeader ()
		{
			return @"<link rel=""preconnect"" href=""https://fonts.googleapis.com"">
<link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
<link href=""https://fonts.googleapis.com/css2?family=Roboto+Serif:ital,opsz,wght@0,8..144,400;0,8..144,700;1,8..144,400;1,8..144,700&family=Source+Sans+Pro:ital,wght@0,400;0,700;1,400;1,700&display=swap"" rel=""stylesheet"">";
		}

		private async Task<string> GetCssAsync ()
		{
			if (string.IsNullOrEmpty( CssContent ))
			{
				var directory = Path.GetDirectoryName( Application.ExecutablePath );
				FileInfo cssFile = new( Path.Combine( directory ?? string.Empty, "hdlg.css" ) );
				if (cssFile.Exists)
				{
					using FileStream stream = new( cssFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true );
					using StreamReader reader = new( stream );
					var iCss = (await reader.ReadToEndAsync( ).ConfigureAwait( false )).Trim( );

					if (!string.IsNullOrWhiteSpace( iCss ))
					{
						CssContent = $"<style>\n{iCss}\n</style>";
					}
				}
				else
				{
					log.Warning( $"CSS file does not exist at path {directory}" );
				}
			}
			return CssContent ?? string.Empty;

		}

		public async Task SaveAsHTMLAsync (string filePath, HdlgDirectory directory)
		{
			if (string.IsNullOrWhiteSpace( filePath ))
			{
				throw new ArgumentException( $"'{nameof( filePath )}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof( filePath ) );
			}

			ArgumentNullException.ThrowIfNull( directory );

			FileInfo fileInfo = new( filePath );

			var encoding = Encoding.UTF8;

			string? version = typeof( DirectoryBrowser ).Assembly.GetName( ).Version?.ToString( );

			using FileStream fileStream = new( fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None );
			using StreamWriter sw = new( fileStream, encoding, 4096, false );
			var titleRaw = $"HTML Directory list generator  {version} {directory.Path} {DateTimeOffset.Now.ToString( "F", CultureInfo.CurrentCulture )}";
			var title = WebUtility.HtmlEncode( titleRaw );
			await sw.WriteLineAsync( "<!DOCTYPE html>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( $"<html lang=\"{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<head>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<meta charset=\"{encoding.WebName}\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"robots\" content=\"noindex, nofollow\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"rating\" content=\"general\">" ).ConfigureAwait( false );
			await sw.WriteAsync( $"<title>{title}</title>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( GetGoogleFontHeader( ) ).ConfigureAwait( false );
			await sw.WriteLineAsync( await GetCssAsync( ).ConfigureAwait( false ) ).ConfigureAwait( false );
			await sw.WriteLineAsync( ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</head>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<body>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"Hdlg\">" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"version\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<h1>{title}</h1>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span>Version</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span>{WebUtility.HtmlEncode( version )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await WriteDirectoriesListAsync( sw, directory ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"directoryHeader\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span>Directory</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<h2>{WebUtility.HtmlEncode( directory.Path )}</h2>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"spacer\">&nbsp;</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"dateTime headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">DateTime</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{DateTime.Now.ToString( "F", CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"directoriesCount headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">DirectoriesCount</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{DirectoriesCount( directory ).ToString( CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"filesCount headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">FilesCount</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{FilesCount( directory ).ToString( CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"directories\">" ).ConfigureAwait( false );

			await WritHtmlDirectoryAsync( sw, directory, 0 ).ConfigureAwait( false );

			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "</body>" ).ConfigureAwait( false );

			await sw.WriteAsync( "</html>" ).ConfigureAwait( false );

			await sw.FlushAsync( ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoriesListAsync (TextWriter writer, HdlgDirectory directory)
		{
			await writer.WriteLineAsync( "<div class=\"directoryList\" id=\"directoryList\">" ).ConfigureAwait( false );

			await WriteDirectoriesListContainAsync( writer, directory, 0 ).ConfigureAwait( false );

			await writer.WriteLineAsync( "</div" ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoriesListContainAsync (TextWriter writer, HdlgDirectory directory, int depth)
		{
			string spacer = new string( ' ', depth );

			await writer.WriteLineAsync( spacer + "<ul>" ).ConfigureAwait( false );

			await WriteDirectoryListContainAsync( writer, directory, depth ).ConfigureAwait( false );

			await writer.WriteLineAsync( spacer + "</ul>" ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoryListContainAsync (TextWriter writer, HdlgDirectory directory, int depth)
		{
			string spacer = new string( ' ', depth + 1 );
			await writer.WriteLineAsync( $"{spacer}<li><a href=\"#{WebUtility.HtmlEncode( directory.Path )}\">{WebUtility.HtmlEncode( directory.Name )}</a></li>" ).ConfigureAwait( false );

			if (directory.Directories.Count > 0)
			{
				var inDepth = depth + 2;
				foreach (HdlgDirectory d in directory.Directories)
				{
					await WriteDirectoriesListContainAsync( writer, d, inDepth ).ConfigureAwait( false );
				}

			}
		}

		/// <summary>
		/// Write the content of a directory
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="directory"></param>
		/// <returns></returns>
		private async Task WritHtmlDirectoryAsync (TextWriter writer, HdlgDirectory directory, int depth)
		{
			log.Debug( $"In {nameof( WritHtmlDirectoryAsync )} {nameof( HdlgDirectory )} {directory}" );
			string spacer = new string( ' ', depth );

			await writer.WriteLineAsync( spacer + $"<div class=\"directory\" id=\"{WebUtility.HtmlEncode( directory.Path )}\">" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}<span class=\"name\">{WebUtility.HtmlEncode( directory.Name )}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}<span class=\"path\">{WebUtility.HtmlEncode( directory.Path )}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}<span class=\"creationTime\">{WebUtility.HtmlEncode( directory.CreationTime.ToString( "F", CultureInfo.CurrentCulture ) )}</span>" ).ConfigureAwait( false );

			await writer.WriteLineAsync( $"{spacer}<a href=\"#directoryList\">⬆️</a>" ).ConfigureAwait( false );

			if (directory.Directories.Any( ))
			{
				await writer.WriteLineAsync( spacer + "<div class=\"directories\">" ).ConfigureAwait( false );
				var inDepth = depth + 1;
				foreach (HdlgDirectory d in directory.Directories)
				{
					await WritHtmlDirectoryAsync( writer, d, inDepth ).ConfigureAwait( false );
				}
				await writer.WriteLineAsync( spacer + "</div>" ).ConfigureAwait( false );
			}

			if (directory.Files.Any( ))
			{
				await writer.WriteLineAsync( spacer + "<div class=\"files\">" ).ConfigureAwait( false );
				foreach (HdlgFile file in directory.Files)
				{
					await WriteHtmlFileAsync( writer, file, spacer ).ConfigureAwait( false );
				}
				await writer.WriteLineAsync( spacer + "</div>" ).ConfigureAwait( false );
			}

			await writer.WriteLineAsync( spacer + "</div>" ).ConfigureAwait( false );
		}

		/// <summary>
		/// Write the content of a <paramref name="file"/> to the <paramref name="writer"/>
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		/// <param name="file">File that content the data</param>
		/// <returns>A task</returns>
		/// <exception cref="ArgumentNullException"></exception>
		private async Task WriteHtmlFileAsync (TextWriter writer, HdlgFile file, string spacer)
		{
			if (writer is null)
			{
				throw new ArgumentNullException( nameof( writer ) );
			}

			log.Verbose( $"{nameof( WriteHtmlFileAsync )} {file}" );

			await writer.WriteLineAsync( spacer + "<ul class=\"file\">" ).ConfigureAwait( false );


			await writer.WriteLineAsync( $"{spacer}<li><a href=\"file:///{Uri.EscapeDataString( file.Path )}\" download=\"{WebUtility.HtmlEncode( file.Name )}\" referrerpolicy=\"strict-origin\">{WebUtility.HtmlEncode( file.Name )}</a></li>" ).ConfigureAwait( false );


			await writer.WriteLineAsync( $"{spacer}<li class=\"size\">{WebUtility.HtmlEncode( file.Size.ToString( CultureInfo.CurrentCulture ) )} kb</li>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}<li class=\"creationTime\">{WebUtility.HtmlEncode( file.CreationTime.ToString( "F", CultureInfo.CurrentCulture ) )}</li>" ).ConfigureAwait( false );
			if (file.Properties != null && file.Properties.Count > 0)
			{
				await writer.WriteLineAsync( spacer + "<li>" ).ConfigureAwait( false );
				await writer.WriteLineAsync( spacer + "<ol class=\"extentedProperties\">" ).ConfigureAwait( false );

				foreach (var property in file.Properties)
				{
					if (!string.IsNullOrWhiteSpace( property.Key ) && property.Value != null)
					{
						await writer.WriteLineAsync( spacer + "\t<li class=\"extentedProperty\">" ).ConfigureAwait( false );
						await writer.WriteLineAsync( $"{spacer}\t\t<span>{WebUtility.HtmlEncode( property.Key )}</span>" ).ConfigureAwait( false );

						if (property.Value is DateTime dtValue)
						{
							await writer.WriteLineAsync( $"{spacer}\t\t<span>{WebUtility.HtmlEncode( dtValue.ToString( "F", CultureInfo.CurrentCulture ) )}</span>" ).ConfigureAwait( false );
						}
						else
						{
							var value = property.Value.ToString( CultureInfo.CurrentCulture );
							await writer.WriteLineAsync( $"{spacer}\t\t<span>{WebUtility.HtmlEncode( value )}</span>" ).ConfigureAwait( false );
						}

						await writer.WriteLineAsync( spacer + "\t</li>" ).ConfigureAwait( false );

					}
				}
				await writer.WriteLineAsync( spacer + "</ol>" ).ConfigureAwait( false );
				await writer.WriteLineAsync( spacer + "</li>" ).ConfigureAwait( false );
			}

			await writer.WriteLineAsync( spacer + "</ul>" ).ConfigureAwait( false );
		}

		#endregion
	}
}
