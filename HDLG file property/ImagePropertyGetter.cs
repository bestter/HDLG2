using System.Drawing;
using System.Globalization;

namespace HdlgFileProperty
{
    public class ImagePropertyGetter : IFilePropertyGetter
    {
        public Dictionary<string, string> GetFileProperties(string path)
        {
            Dictionary<string, string> properties = new();
            try
            {
                var img = Image.FromStream(File.OpenRead(path), false, false);

                properties.Add(nameof(img.Width), img.Width.ToString(CultureInfo.InvariantCulture));
                properties.Add(nameof(img.Height), img.Height.ToString(CultureInfo.InvariantCulture));
            }
            catch (ArgumentException)
            {
                //The stream does not have a valid image format.
            }
            catch (OutOfMemoryException)
            {
                //The stream does not have a valid image format.
            }
            catch (Exception)
            {
                throw;
            }
            return properties;
        }

        /// <summary>
        /// Is this file is supported
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsSupportedFile(string path)
        {
            FileInfo fileInfo = new(path);
            var isSupported = fileInfo.Extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" => true,
                _ => false,
            };
            return isSupported;
        }
    }
}