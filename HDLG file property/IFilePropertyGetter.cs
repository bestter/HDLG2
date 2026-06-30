/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using Serilog;

namespace HdlgFileProperty
{
    public interface IFilePropertyGetter
    {
        public static readonly IReadOnlyDictionary<string, IConvertible> EmptyProperties = System.Collections.ObjectModel.ReadOnlyDictionary<string, IConvertible>.Empty;

        /// <summary>
        /// Add valid <see cref="ILogger"/> to this property getter
        /// </summary>
        /// <param name="logger"></param>
        void AddLogger(ILogger logger);

        /// <summary>
        /// Did this file is supported
        /// </summary>
        /// <param name="path">The full file path</param>
        /// <returns></returns>
        bool IsSupportedFile(string path);

        /// <summary>
        /// Get the file properties
        /// </summary>
        /// <param name="path">The full file path</param>
        /// <returns></returns>
        IReadOnlyDictionary<string, IConvertible> GetFileProperties(string path);
    }
}
