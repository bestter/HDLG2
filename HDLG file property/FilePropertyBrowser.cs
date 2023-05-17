using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Intrinsics.X86;

namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly List<FilePropertyGetterStatistic> filePropertyGetters;

        private readonly Serilog.ILogger logger;

        private long TotalNumberOfFiles { get; set; }

        public FilePropertyBrowser(Serilog.ILogger logger, params IFilePropertyGetter[] imagePropertyGetters)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;

            if (imagePropertyGetters is null)
            {
                throw new ArgumentNullException(nameof(imagePropertyGetters));
            }
            filePropertyGetters = new();
            TotalNumberOfFiles = 0;

            foreach (var propertyGetter in imagePropertyGetters)
            {
                propertyGetter.AddLogger(logger);
                FilePropertyGetterStatistic filePropertyGetterStatistic = new(propertyGetter);
                filePropertyGetters.Add(filePropertyGetterStatistic);
            }
        }

        public Dictionary<string, IConvertible> GetFileProperty(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(path));
            }
            TotalNumberOfFiles++;
            Dictionary<string, IConvertible> properties = new();
            foreach (var propertyGetters in filePropertyGetters)
            {
                if (propertyGetters.FilePropertyGetter.IsSupportedFile(path))
                {
                    propertyGetters.IncrementFile();
                    propertyGetters.StartTimer();
                    var currentProperties = propertyGetters.FilePropertyGetter.GetFileProperties(path);
                    foreach (var currentProperty in currentProperties)
                    {
                        _ = properties.TryAdd(currentProperty.Key, currentProperty.Value);
                    }
                    propertyGetters.StopTimer();

                }
            }
            return properties;
        }

        public void LogGetterStatistics()
        {
            foreach (var propertyGetter in filePropertyGetters)
            {
                if (propertyGetter.TotalFiles > 0)
                {
                    var avg = TimeSpan.FromTicks((long)Math.Ceiling(propertyGetter.GetTotalExecutionTime().Ticks / (double)propertyGetter.TotalFiles));
                    logger.Information($"{propertyGetter.FilePropertyGetter.GetType()} total runtime: {propertyGetter.GetTotalExecutionTime().ToString("G", CultureInfo.CurrentCulture)}. Number of files: {propertyGetter.TotalFiles}. Average: {avg.ToString("G", CultureInfo.CurrentCulture)}");
                }
            }
            logger.Information($"Total number of files {TotalNumberOfFiles}");
        }
    }
}
