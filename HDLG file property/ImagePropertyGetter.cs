/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace HdlgFileProperty
{
    public class ImagePropertyGetter : IFilePropertyGetter
    {

        public ILogger? Logger { get; private set; }

        public void AddLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public Dictionary<string, IConvertible> GetFileProperties(string path)
        {
            Dictionary<string, IConvertible> properties = new();
            try
            {
                var imageInfo = Image.Identify(path);
                if (imageInfo != null)
                {
                    properties.Add(nameof(imageInfo.Width), imageInfo.Width);
                    properties.Add(nameof(imageInfo.Height), imageInfo.Height);

                    var exifProfile = imageInfo.Metadata?.ExifProfile;
                    if (exifProfile != null)
                    {
                        if (exifProfile.TryGetValue(ExifTag.Model, out var cameraModel) && cameraModel.Value != null)
                        {
                            var modelStr = cameraModel.Value.ToString()?.Trim().Replace("\0", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                            if (!string.IsNullOrWhiteSpace(modelStr))
                            {
                                properties.Add("CameraModel", modelStr);
                            }
                        }
                    }
                }
            }
            catch (UnknownImageFormatException)
            {
                //The stream does not have a valid image format.
            }
            catch (InvalidImageContentException)
            {
                //The image content is corrupted or invalid.
            }
            catch (Exception)
            {
                throw;
            }
            return properties;
        }

        private static readonly HashSet<string> _supportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
{
    // JPEG
    ".JPG", ".JPEG", ".JFIF", ".PJPEG", ".PJP",
    
    // PNG
    ".PNG", ".APNG",
    
    // GIF
    ".GIF",
    
    // WEBP
    ".WEBP",
    
    // BMP
    ".BMP", ".DIB",
    
    // TIFF
    ".TIF", ".TIFF",
    
    // TGA
    ".TGA", ".VDA", ".ICB", ".VST",
    
    // NETPBM
    ".PBM", ".PGM", ".PPM", ".PNM",
    
    // FORMATS SPÉCIALISÉS (GAMING / MODERNE)
    ".DDS", ".QOI"
};

        /// <summary>
        /// Is this file is supported
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsSupportedFile(string path)
        {
            var extension = Path.GetExtension(path);
            return extension != null && _supportedImageExtensions.Contains(extension);
        }
    }
}