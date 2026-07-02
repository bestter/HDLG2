#pragma warning disable CA1062
/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;
using System.Globalization;

namespace HdlgFileProperty
{
    public class Mp3PropertyGetter : IFilePropertyGetter
    {
        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyDictionary<string, IConvertible> GetFileProperties(FileInfo fileInfo)
        {
            Logger?.Verbose("In {Class}.{Method}: {Path}", nameof(Mp3PropertyGetter), nameof(GetFileProperties), fileInfo.FullName);
            Dictionary<string, IConvertible>? properties = null;
            try
            {
                using TagLib.File f = TagLib.File.Create(fileInfo.FullName);
                if (!f.PossiblyCorrupt)
                {
                    if (!f.Tag.IsEmpty)
                    {
                        properties = new Dictionary<string, IConvertible>();
                        properties.Add(nameof(f.Tag.Title), f.Tag.Title);

                        properties.Add(nameof(f.Properties.Duration), f.Properties.Duration.ToString("G", CultureInfo.CurrentCulture));

                        properties.Add(nameof(f.Tag.Album), f.Tag.Album);
                        properties.Add(nameof(f.Tag.Year), f.Tag.Year);

                        if (f.Tag.Performers is { Length: > 0 })
                        {
                            properties.Add(nameof(f.Tag.Performers), string.Join(", ", f.Tag.Performers));
                        }

                        if (f.Tag.AlbumArtists is { Length: > 0 })
                        {
                            properties.Add(nameof(f.Tag.AlbumArtists), string.Join(", ", f.Tag.AlbumArtists));
                        }

                        if (f.Tag.Composers is { Length: > 0 })
                        {
                            properties.Add(nameof(f.Tag.Composers), string.Join(", ", f.Tag.Composers));
                        }

                        if (!string.IsNullOrWhiteSpace(f.Tag.Copyright))
                        {
                            properties.Add(nameof(f.Tag.Copyright), f.Tag.Copyright);
                        }
                    }
                }
                else
                {
                    // Performance optimization: Avoid eager string.Join allocation, let Serilog format the collection
                    Logger?.Warning("File {Path} might be corrupted because {CorruptionReasons}", fileInfo.FullName, f.CorruptionReasons);
                }
            }
            catch (IOException ioe)
            {
                Logger?.Error(ioe, "Cannot read file {Path}", fileInfo.FullName);
            }
            catch (TagLib.UnsupportedFormatException ufe)
            {
                Logger?.Warning(ufe, "File {Path} is not supported", fileInfo.FullName);
            }
            catch (TagLib.CorruptFileException cfe)
            {
                Logger?.Warning(cfe, "File {Path} is corrupted", fileInfo.FullName);
            }
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            catch (Exception e)
            {
                Logger?.Warning(e, "Cannot read properties from file {Path}", fileInfo.FullName);
            }
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale
            return (IReadOnlyDictionary<string, IConvertible>?)properties ?? IFilePropertyGetter.EmptyProperties;
        }

        private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".MKV", ".OGV", ".AVI", ".WMV", ".ASF", ".MP4", ".M4P", ".M4V", ".MPEG", ".MPG", ".MPE", ".MPV", ".M2V",
            ".AA", ".AAX", ".AAC", ".AIFF", ".APE", ".DSF", ".FLAC", ".M4A", ".M4B", ".MP3", ".MPC", ".MPP", ".OGG", ".OGA", ".WAV", ".WMA", ".WV", ".WEBM"
        };

        public bool IsSupportedFile(string path)
        {
            var extension = Path.GetExtension(path.AsSpan());
            // Optimization: _supportedExtensions uses StringComparer.OrdinalIgnoreCase,
            // so ToUpperInvariant() is an unnecessary allocation.
            return !extension.IsEmpty && _supportedExtensions.GetAlternateLookup<ReadOnlySpan<char>>().Contains(extension);
        }
    }
}
