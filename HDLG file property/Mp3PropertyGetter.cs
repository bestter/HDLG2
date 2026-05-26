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

        public Dictionary<string, IConvertible> GetFileProperties(string path)
        {
            Logger?.Verbose($"In {nameof(Mp3PropertyGetter)}.{nameof(GetFileProperties)}: {path}");
            Dictionary<string, IConvertible> properties = new();
            try
            {
                using TagLib.File f = TagLib.File.Create(path);
                if (!f.PossiblyCorrupt)
                {
                    if (!f.Tag.IsEmpty)
                    {
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
                    Logger?.Warning($"File {path} might be corrupted because {string.Join(",", f.CorruptionReasons)}");
                }
            }
            catch(IOException ioe)
            {
                Logger?.Error(ioe, $"Cannot read file {path}");
            }
            catch (TagLib.UnsupportedFormatException ufe)
            {
                Logger?.Warning(ufe, $"File {path} is not supported");
            }
            catch (TagLib.CorruptFileException cfe)
            {
                Logger?.Warning(cfe, $"File {path} is corrupted");
            }
#pragma warning disable CA1031 // Ne pas intercepter les types d'exception générale
            catch (Exception e)
            {
                Logger?.Warning(e, "Cannot read properties from file {Path}", path);
            }
#pragma warning restore CA1031 // Ne pas intercepter les types d'exception générale
            return properties;
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
