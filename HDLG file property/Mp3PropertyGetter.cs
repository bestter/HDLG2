using Serilog;
using Serilog.Core;

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
            Logger?.Information($"In {nameof(Mp3PropertyGetter)}.{nameof(GetFileProperties)}: {path}");
            Dictionary<string, IConvertible> properties = new();
            try
            {
                using TagLib.File f = TagLib.File.Create(path);
                if (!f.PossiblyCorrupt)
                {
                    if (!f.Tag.IsEmpty)
                    {
                        properties.Add(nameof(f.Name), f.Name);
                        properties.Add(nameof(f.Tag.Title), f.Tag.Title);

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
            string[] extensions = { ".mp3", ".m4a", ".m4p" };
            return extensions.Contains(extension);
        }
    }
}
