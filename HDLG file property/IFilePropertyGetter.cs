using Serilog;

namespace HdlgFileProperty
{
    public interface IFilePropertyGetter
    {
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
        Dictionary<string, IConvertible> GetFileProperties(string path);
    }
}
