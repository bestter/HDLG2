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
        Dictionary<string, IConvertible> GetFileProperties(string path);
    }
}
