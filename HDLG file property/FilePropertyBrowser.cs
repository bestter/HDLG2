namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly IFilePropertyGetter[] filePropertyGetters;

        public FilePropertyBrowser(params IFilePropertyGetter[] imagePropertyGetters)
        {
            if (imagePropertyGetters is null)
            {
                throw new ArgumentNullException(nameof(imagePropertyGetters));
            }
            filePropertyGetters = new IFilePropertyGetter[imagePropertyGetters.Length];
            imagePropertyGetters.CopyTo(filePropertyGetters, 0);
        }

        public Dictionary<string, IConvertible> GetFileProperty(string path)
        {

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(path));
            }
            Dictionary<string, IConvertible> properties = new();
            foreach (var propertyGetters in filePropertyGetters)
            {
                if (propertyGetters.IsSupportedFile(path))
                {
                    var currentProperties = propertyGetters.GetFileProperties(path);
                    foreach (var currentProperty in currentProperties)
                    {
                        _ = properties.TryAdd(currentProperty.Key, currentProperty.Value);
                    }

                }
            }
            return properties;
        }
    }
}
