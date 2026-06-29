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
    }
}