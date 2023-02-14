using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly IFilePropertyGetter[] filePropertyGetters;

        public FilePropertyBrowser(IFilePropertyGetter imagePropertyGetter)
        {

            filePropertyGetters = new IFilePropertyGetter[] { imagePropertyGetter };
        }

        public Dictionary<string,string> GetFileProperty(string path)
        {

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(path));
            }
            Dictionary<string,string> properties = new();
            foreach (var propertyGetters in filePropertyGetters)
            {
                if (propertyGetters.IsSupportedFile(path))
                {
                    var currentProperties = propertyGetters.GetFileProperties(path);
                    foreach (var currentProperty in currentProperties)
                    {
                        properties.TryAdd(currentProperty.Key, currentProperty.Value);
                    }

                }
            }
            return properties;
        }
    }
}
