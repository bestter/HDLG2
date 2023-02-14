using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdlgFileProperty
{
    public interface IFilePropertyGetter
    {
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
        Dictionary<string, string> GetFileProperties(string path);
    }
}
