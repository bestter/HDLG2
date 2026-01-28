/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;
using System.Drawing;

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
                using var file = File.OpenRead(path);
                var img = Image.FromStream(file, false, false);

                properties.Add(nameof(img.Width), img.Width);
                properties.Add(nameof(img.Height), img.Height);
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