/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */

namespace HdlgFileProperty
{
    /// <summary>
    /// Configurable limits that protect property extraction from denial-of-service attacks.
    /// </summary>
    public static class FilePropertyLimits
    {
        /// <summary>
        /// Maximum file size (in bytes) allowed for property extraction.
        /// </summary>
        public const long MaxFileSizeBytes = 100L * 1024 * 1024;

        /// <summary>
        /// Maximum width or height (in pixels) allowed when identifying images.
        /// </summary>
        public const int MaxImageDimension = 32_768;

        /// <summary>
        /// Maximum time allowed for a single property getter invocation.
        /// </summary>
        public static readonly TimeSpan PropertyExtractionTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Maximum characters allowed when parsing OpenXML parts to prevent XXE/Zip bomb attacks.
        /// </summary>
        public const long MaxOpenXmlCharacters = 10_000_000L;

        /// <summary>
        /// Maximum number of entries allowed in an OpenXML package.
        /// </summary>
        public const int MaxOpenXmlEntries = 10_000;

        /// <summary>
        /// Maximum decompressed size allowed for an OpenXML part that is parsed by the property extractors.
        /// </summary>
        public const long MaxOpenXmlPartSizeBytes = 10L * 1024 * 1024;

        /// <summary>
        /// Maximum cumulative decompressed size allowed for OpenXML parts parsed during property extraction.
        /// </summary>
        public const long MaxOpenXmlProcessedBytes = 100L * 1024 * 1024;

        /// <summary>
        /// Maximum characters allowed in a single OpenXML core property value.
        /// </summary>
        public const int MaxOpenXmlPropertyCharacters = 4_096;
    }
}
