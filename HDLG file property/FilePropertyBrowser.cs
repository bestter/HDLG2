using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly IFilePropertyGetter[] filePropertyGetters;

        public FilePropertyBrowser(Serilog.ILogger logger, params IFilePropertyGetter[] imagePropertyGetters)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (imagePropertyGetters is null)
            {
                throw new ArgumentNullException(nameof(imagePropertyGetters));
            }
            filePropertyGetters = new IFilePropertyGetter[imagePropertyGetters.Length];
            imagePropertyGetters.CopyTo(filePropertyGetters, 0);
            foreach (var imagePropertyGetter in imagePropertyGetters)
            {
                imagePropertyGetter.AddLogger(logger);
            }
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
