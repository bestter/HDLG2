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
		private static readonly string [ ] Spacers = CreateSpacers( );

		private static string [ ] CreateSpacers ()
		{
			var spacers = new string [20];
			for (int i = 0; i < 20; i++)
			{
				spacers [i] = new string( ' ', i );
			}
			return spacers;
		}

		private readonly ILogger log;

		/// <summary>
		/// Css content
		/// </summary>
		private string? CssContent;

		/// <summary>
		/// Cache for HTML encoded property keys to avoid redundant allocations and parsing.
		/// </summary>
		private readonly Dictionary<string, string> _encodedPropertyKeys = new( );

		/// <summary>
		/// Cache for XML encoded property keys to avoid redundant allocations and parsing.
		/// </summary>
		private readonly Dictionary<string, string> _xmlEncodedPropertyKeys = new( );

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
				await writer.WriteElementStringAsync( null, "Directory", null, SanitizeXmlString( directory.Path ) ).ConfigureAwait( false );
				await writer.WriteElementStringAsync( null, "DateTime", null, DateTime.Now.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );

				await writer.WriteElementStringAsync( null, "DirectoriesCount", null, directory.TotalDirectories.ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
				await writer.WriteElementStringAsync( null, "FilesCount", null, directory.TotalFiles.ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );

				await WriteXmlDirectoryAsync( writer, directory ).ConfigureAwait( false );

				await writer.WriteEndElementAsync( ).ConfigureAwait( false );

				await writer.WriteEndDocumentAsync( ).ConfigureAwait( false );
			}
			await sw.FlushAsync( ).ConfigureAwait( false );
		}

		/// <summary>
		/// Write the content of a directory
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="directory"></param>
		/// <returns></returns>
		private async Task WriteXmlDirectoryAsync (XmlWriter writer, HdlgDirectory directory)
		{
			log.Debug( "In {Method} {Type} {Directory}", nameof( WriteXmlDirectoryAsync ), nameof( HdlgDirectory ), directory );
			await writer.WriteStartElementAsync( null, "Directory", null ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Name", null, SanitizeXmlString( directory.Name ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Path", null, SanitizeXmlString( directory.Path ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "CreationTime", null, directory.CreationTime.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
			if (directory.Directories.Count > 0)
			{
				await writer.WriteStartElementAsync( null, "Directories", null ).ConfigureAwait( false );
				for (int i = 0; i < directory.Directories.Count; i++)
				{
					HdlgDirectory d = directory.Directories [i];
					await WriteXmlDirectoryAsync( writer, d ).ConfigureAwait( false );
				}
				await writer.WriteEndElementAsync( ).ConfigureAwait( false );
			}

			if (directory.Files.Count > 0)
			{
				await writer.WriteStartElementAsync( null, "Files", null ).ConfigureAwait( false );
				for (int i = 0; i < directory.Files.Count; i++)
				{
					HdlgFile file = directory.Files [i];
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

			log.Verbose( "{Method} {File}", nameof( WriteXmlFileAsync ), file );

			await writer.WriteStartElementAsync( null, "File", null ).ConfigureAwait( false );

			await writer.WriteElementStringAsync( null, "Name", null, SanitizeXmlString( file.Name ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Path", null, SanitizeXmlString( file.Path ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Extension", null, SanitizeXmlString( file.Extension ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "Size", null, file.Size.ToString( CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
			await writer.WriteElementStringAsync( null, "CreationTime", null, file.CreationTime.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );

			if (file.Properties == null || file.Properties.Count == 0)
			{
				await writer.WriteEndElementAsync( ).ConfigureAwait( false );
				return;
			}

			await writer.WriteStartElementAsync( null, "ExtentedProperties", null ).ConfigureAwait( false );

			// Performance optimization: Type-check and cast IReadOnlyDictionary to Dictionary to allow
			// the foreach loop to use the struct-based enumerator, preventing interface boxing allocations.
			if (file.Properties is Dictionary<string, IConvertible> dictProperties)
			{
				foreach (var property in dictProperties)
				{
					if (string.IsNullOrWhiteSpace( property.Key ) || property.Value == null) continue;

					if (!_xmlEncodedPropertyKeys.TryGetValue( property.Key, out string? encodedKey ))
					{
						encodedKey = XmlConvert.EncodeLocalName( property.Key ) ?? "UnknownProperty";
						_xmlEncodedPropertyKeys [property.Key] = encodedKey;
					}

					if (property.Value is DateTime dtValue)
					{
						await writer.WriteElementStringAsync( null, encodedKey, null, dtValue.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
					}
					else if (property.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
					{
						var value = property.Value.ToString( CultureInfo.InvariantCulture );
						await writer.WriteElementStringAsync( null, encodedKey, null, value ).ConfigureAwait( false );
					}
					else
					{
						var value = property.Value.ToString( CultureInfo.InvariantCulture );
						await writer.WriteElementStringAsync( null, encodedKey, null, SanitizeXmlString( value ) ).ConfigureAwait( false );
					}
				}
			}
			else
			{
				foreach (var property in file.Properties)
				{
					if (string.IsNullOrWhiteSpace( property.Key ) || property.Value == null) continue;

					if (!_xmlEncodedPropertyKeys.TryGetValue( property.Key, out string? encodedKey ))
					{
						encodedKey = XmlConvert.EncodeLocalName( property.Key ) ?? "UnknownProperty";
						_xmlEncodedPropertyKeys [property.Key] = encodedKey;
					}

					if (property.Value is DateTime dtValue)
					{
						await writer.WriteElementStringAsync( null, encodedKey, null, dtValue.ToString( "O", CultureInfo.InvariantCulture ) ).ConfigureAwait( false );
					}
					else if (property.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
					{
						var value = property.Value.ToString( CultureInfo.InvariantCulture );
						await writer.WriteElementStringAsync( null, encodedKey, null, value ).ConfigureAwait( false );
					}
					else
					{
						var value = property.Value.ToString( CultureInfo.InvariantCulture );
						await writer.WriteElementStringAsync( null, encodedKey, null, SanitizeXmlString( value ) ).ConfigureAwait( false );
					}
				}
			}
			// Close ExtentedProperties, then File (both opened above when properties are present).
			await writer.WriteEndElementAsync( ).ConfigureAwait( false );
			await writer.WriteEndElementAsync( ).ConfigureAwait( false );
		}

		private static string SanitizeXmlString (string? xml)
		{
			if (string.IsNullOrEmpty( xml ))
			{
				return xml ?? string.Empty;
			}

			// Performance optimization: Fast-path for valid strings to avoid StringBuilder allocation.
			// Most XML strings (paths, names) are valid, so scanning first saves significant memory overhead.
			int firstIllegalCharIndex = -1;
			for (int i = 0; i < xml.Length; i++)
			{
				if (!IsLegalXmlChar( xml [i] ))
				{
					firstIllegalCharIndex = i;
					break;
				}
			}

			// If no invalid characters are found, immediately return the original string.
			if (firstIllegalCharIndex == -1)
			{
				return xml;
			}

			// Avoid StringBuilder allocations and method overhead by using a simple array buffer.
			char[] buffer = new char[xml.Length];
			xml.CopyTo( 0, buffer, 0, firstIllegalCharIndex );
			int writeIndex = firstIllegalCharIndex;
			for (int i = firstIllegalCharIndex + 1; i < xml.Length; i++)
			{
				char c = xml [i];
				if (IsLegalXmlChar( c ))
				{
					buffer[writeIndex++] = c;
				}
			}
			return new string( buffer, 0, writeIndex );
		}

		private static bool IsLegalXmlChar (int character)
		{
			return character == 0x9 /* == '\t' == 9   */          ||
				   character == 0xA /* == '\n' == 10  */          ||
				   character == 0xD /* == '\r' == 13  */          ||
				  (character >= 0x20 && character <= 0xD7FF) ||
				  (character >= 0xE000 && character <= 0xFFFD) ||
				  (character >= 0x10000 && character <= 0x10FFFF);
		}
		#endregion

		#region HTML

		private static string GetGoogleFontHeader ()
		{
			// Modern 2026 design uses system fonts only for full self-containment, offline support and faster loads.
			// No external Google Fonts to keep the exported HTML self-contained.
			return string.Empty;
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
					log.Warning( "CSS file does not exist at path {DirectoryPath}", directory );
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
			string currentDateTimeFormatted = DateTime.Now.ToString( "F", CultureInfo.CurrentCulture );
			var title = $"HTML Directory list generator  {version} {directory.Path} {currentDateTimeFormatted}";
			var encodedTitle = WebUtility.HtmlEncode( title );
			await sw.WriteLineAsync( "<!DOCTYPE html>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( $"<html lang=\"{CultureInfo.CurrentCulture.TwoLetterISOLanguageName}\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<head>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<meta charset=\"{encoding.WebName}\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"robots\" content=\"noindex, nofollow\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"rating\" content=\"general\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none';\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<meta name=\"referrer\" content=\"no-referrer\">" ).ConfigureAwait( false );
			await sw.WriteAsync( $"<title>{encodedTitle}</title>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( GetGoogleFontHeader( ) ).ConfigureAwait( false );
			await sw.WriteLineAsync( await GetCssAsync( ).ConfigureAwait( false ) ).ConfigureAwait( false );
			await sw.WriteLineAsync( ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</head>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<body>" ).ConfigureAwait( false );

			// 2026 clean light professional layout (responsive via CSS, self-contained)
			await sw.WriteLineAsync( "<div class=\"hdlg\">" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"version\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<h1>{encodedTitle}</h1>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span>Version</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span>{version}</span>" ).ConfigureAwait( false ); // Version strings (e.g. "1.0.0.0") are safe, no need to encode
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await WriteDirectoriesListAsync( sw, directory ).ConfigureAwait( false );

			string encodedRootDirectoryPath = WebUtility.HtmlEncode( directory.Path );
			await sw.WriteLineAsync( "<div class=\"directoryHeader\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span>Directory</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<h2>{encodedRootDirectoryPath}</h2>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"spacer\">&nbsp;</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"dateTime headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">DateTime</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{currentDateTimeFormatted}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"directoriesCount headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">DirectoriesCount</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{directory.TotalDirectories.ToString( CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"filesCount headerContent\">" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "<span class=\"headerContentTitle\">FilesCount</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( $"<span class=\"headerContentData\">{directory.TotalFiles.ToString( CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "<div class=\"directories\">" ).ConfigureAwait( false );

			await WritHtmlDirectoryAsync( sw, directory, 0 ).ConfigureAwait( false );

			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( "</div>" ).ConfigureAwait( false );

			await sw.WriteLineAsync( AppBranding.GetHtmlFooterMarkup( version ) ).ConfigureAwait( false );

			await sw.WriteLineAsync( "</body>" ).ConfigureAwait( false );

			await sw.WriteAsync( "</html>" ).ConfigureAwait( false );

			await sw.FlushAsync( ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoriesListAsync (TextWriter writer, HdlgDirectory directory)
		{
			await writer.WriteLineAsync( "<div class=\"directoryList\" id=\"directoryList\">" ).ConfigureAwait( false );

			await WriteDirectoriesListContainAsync( writer, directory, 0 ).ConfigureAwait( false );

			await writer.WriteLineAsync( "</div>" ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoriesListContainAsync (TextWriter writer, HdlgDirectory directory, int depth)
		{
			string spacer = depth < 20 ? Spacers [depth] : new string( ' ', depth );

			await writer.WriteLineAsync( spacer + "<ul>" ).ConfigureAwait( false );

			await WriteDirectoryListContainAsync( writer, directory, depth ).ConfigureAwait( false );

			await writer.WriteLineAsync( spacer + "</ul>" ).ConfigureAwait( false );
		}

		private static async Task WriteDirectoryListContainAsync (TextWriter writer, HdlgDirectory directory, int depth)
		{
			string spacer = (depth + 1) < 20 ? Spacers [depth + 1] : new string( ' ', depth + 1 );
			// Truncate long directory names with ellipsis + native title hover popup (per user choice: minimal native title + CSS, ~26ch, no JS).
			string dirName = WebUtility.HtmlEncode( directory.Name );
			string dirPath = WebUtility.HtmlEncode( directory.Path );
			await writer.WriteLineAsync( $"{spacer}<li><a href=\"#{dirPath}\" title=\"{dirName}\">{dirName}</a></li>" ).ConfigureAwait( false );

			if (directory.Directories.Count > 0)
			{
				var inDepth = depth + 2;
				for (int i = 0; i < directory.Directories.Count; i++)
				{
					HdlgDirectory d = directory.Directories [i];
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
			log.Debug( "In {Method} {Type} {Directory}", nameof( WritHtmlDirectoryAsync ), nameof( HdlgDirectory ), directory );
			string spacer = depth < 20 ? Spacers [depth] : new string( ' ', depth );
			string encodedPath = WebUtility.HtmlEncode( directory.Path );
			string id = encodedPath; // Re-use cached encoded path
			string name = WebUtility.HtmlEncode( directory.Name );
			string created = directory.CreationTime.ToString( "F", CultureInfo.CurrentCulture );

			// 2026 design: use native <details>/<summary> for clean, accessible, collapsible directory trees.
			await writer.WriteLineAsync( spacer + $"<details class=\"directory\" id=\"{id}\" open>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t<summary>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<span class=\"name\" title=\"{name}\">{name}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<span class=\"path\">{encodedPath}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<span class=\"creationTime\">{created}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<a href=\"#directoryList\" aria-label=\"Back to contents\">⬆️</a>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t</summary>" ).ConfigureAwait( false );

			if (directory.Directories.Count > 0)
			{
				await writer.WriteLineAsync( spacer + "\t<div class=\"directories\">" ).ConfigureAwait( false );
				var inDepth = depth + 1;
				for (int i = 0; i < directory.Directories.Count; i++)
				{
					HdlgDirectory d = directory.Directories [i];
					await WritHtmlDirectoryAsync( writer, d, inDepth ).ConfigureAwait( false );
				}
				await writer.WriteLineAsync( spacer + "\t</div>" ).ConfigureAwait( false );
			}

			if (directory.Files.Count > 0)
			{
				await writer.WriteLineAsync( spacer + "\t<div class=\"files\">" ).ConfigureAwait( false );
				for (int i = 0; i < directory.Files.Count; i++)
				{
					HdlgFile file = directory.Files [i];
					await WriteHtmlFileAsync( writer, file, spacer + "\t" ).ConfigureAwait( false );
				}
				await writer.WriteLineAsync( spacer + "\t</div>" ).ConfigureAwait( false );
			}

			await writer.WriteLineAsync( spacer + "</details>" ).ConfigureAwait( false );
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

			log.Verbose( "{Method} {File}", nameof( WriteHtmlFileAsync ), file );

			// 2026 clean file card using div + flex-friendly structure (styled via modern CSS)
			await writer.WriteLineAsync( spacer + "<div class=\"file\">" ).ConfigureAwait( false );

			string encodedName = WebUtility.HtmlEncode( file.Name );
			string fileUri = "file:///" + string.Join("/", file.Path.Split('\\', '/').Select(p => p.EndsWith(":", StringComparison.Ordinal) ? p : Uri.EscapeDataString(p)));
			string encodedFileUri = WebUtility.HtmlEncode( fileUri );
			await writer.WriteLineAsync( $"{spacer}\t<a href=\"{encodedFileUri}\" download=\"{encodedName}\" referrerpolicy=\"strict-origin\">{encodedName}</a>" ).ConfigureAwait( false );

			await writer.WriteLineAsync( $"{spacer}\t<div class=\"file-meta\">" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<span class=\"size\">{file.Size.ToString( CultureInfo.CurrentCulture )} kb</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t\t<span class=\"creationTime\">{file.CreationTime.ToString( "F", CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( $"{spacer}\t</div>" ).ConfigureAwait( false );

			if (file.Properties == null || file.Properties.Count == 0)
			{
				await writer.WriteLineAsync( spacer + "</div>" ).ConfigureAwait( false );
				return;
			}

			await writer.WriteLineAsync( spacer + "\t<ol class=\"extentedProperties\">" ).ConfigureAwait( false );

			// Performance optimization: Type-check and cast IReadOnlyDictionary to Dictionary to allow
			// the foreach loop to use the struct-based enumerator, preventing interface boxing allocations.
			if (file.Properties is Dictionary<string, IConvertible> dictProperties)
			{
				foreach (var property in dictProperties)
				{
					if (string.IsNullOrWhiteSpace( property.Key ) || property.Value == null) continue;

					if (!_encodedPropertyKeys.TryGetValue( property.Key, out string? encodedKey ))
					{
						encodedKey = WebUtility.HtmlEncode( property.Key );
						_encodedPropertyKeys [property.Key] = encodedKey;
					}
					await writer.WriteLineAsync( spacer + "\t\t<li class=\"extentedProperty\">" ).ConfigureAwait( false );
					await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{encodedKey}</span>" ).ConfigureAwait( false );

					if (property.Value is DateTime dtValue)
					{
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{dtValue.ToString( "F", CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
					}
					else if (property.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
					{
						var value = property.Value.ToString( CultureInfo.CurrentCulture );
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{value}</span>" ).ConfigureAwait( false );
					}
					else
					{
						var value = property.Value.ToString( CultureInfo.CurrentCulture );
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{WebUtility.HtmlEncode( value )}</span>" ).ConfigureAwait( false );
					}

					await writer.WriteLineAsync( spacer + "\t\t</li>" ).ConfigureAwait( false );
				}
			}
			else
			{
				foreach (var property in file.Properties)
				{
					if (string.IsNullOrWhiteSpace( property.Key ) || property.Value == null) continue;

					if (!_encodedPropertyKeys.TryGetValue( property.Key, out string? encodedKey ))
					{
						encodedKey = WebUtility.HtmlEncode( property.Key );
						_encodedPropertyKeys [property.Key] = encodedKey;
					}
					await writer.WriteLineAsync( spacer + "\t\t<li class=\"extentedProperty\">" ).ConfigureAwait( false );
					await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{encodedKey}</span>" ).ConfigureAwait( false );

					if (property.Value is DateTime dtValue)
					{
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{dtValue.ToString( "F", CultureInfo.CurrentCulture )}</span>" ).ConfigureAwait( false );
					}
					else if (property.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal)
					{
						var value = property.Value.ToString( CultureInfo.CurrentCulture );
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{value}</span>" ).ConfigureAwait( false );
					}
					else
					{
						var value = property.Value.ToString( CultureInfo.CurrentCulture );
						await writer.WriteLineAsync( $"{spacer}\t\t\t<span>{WebUtility.HtmlEncode( value )}</span>" ).ConfigureAwait( false );
					}

					await writer.WriteLineAsync( spacer + "\t\t</li>" ).ConfigureAwait( false );
				}
			}
			await writer.WriteLineAsync( spacer + "\t</ol>" ).ConfigureAwait( false );
			await writer.WriteLineAsync( spacer + "</div>" ).ConfigureAwait( false );
		}

		#endregion
	}
}
