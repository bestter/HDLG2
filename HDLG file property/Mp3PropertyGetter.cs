using Serilog;
using Serilog.Core;
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

                        if (f.Tag.Performers != null && f.Tag.Performers.Any())
                        {
                            properties.Add(nameof(f.Tag.Performers), string.Join(", ", f.Tag.Performers));
                        }

                        if (f.Tag.AlbumArtists != null && f.Tag.AlbumArtists.Any())
                        {
                            properties.Add(nameof(f.Tag.AlbumArtists), string.Join(", ", f.Tag.AlbumArtists));
                        }

                        if (f.Tag.Composers != null && f.Tag.Composers.Any())
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
            catch (TagLib.UnsupportedFormatException ufe)
            {
                Logger?.Warning(ufe, $"File {path} is not supported");
            }
            catch (TagLib.CorruptFileException cfe)
            {
                Logger?.Warning(cfe, $"File {path} is corrupted");
            }
            return properties;
        }

        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var extension = fileInfo.Extension.ToLowerInvariant();
            string[] extensions = { ".mkv", ".ogv", ".avi", ".wmv", ".asf", ".mp4", ".m4p", ".m4v", ".mpeg", ".mpg", ".mpe", ".mpv", ".mpg", ".m2v",
            ".aa", ".aax", ".aac", ".aiff", ".ape", ".dsf", ".flac", ".m4a", ".m4b", ".m4p", ".mp3", ".mpc", ".mpp", ".ogg", ".oga", ".wav", ".wma", ".wv", ".webm"};

            return extensions.Contains(extension);
        }
    }
}
