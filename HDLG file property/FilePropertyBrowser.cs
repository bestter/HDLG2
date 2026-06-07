/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */

using System.Globalization;

namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly FilePropertyGetterStatistic[] filePropertyGetters;

        private readonly Serilog.ILogger logger;

        private long TotalNumberOfFiles { get; set; }

        public FilePropertyBrowser(Serilog.ILogger logger, params IFilePropertyGetter[] imagePropertyGetters)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(imagePropertyGetters);

            this.logger = logger;
            filePropertyGetters = new FilePropertyGetterStatistic[imagePropertyGetters.Length];
            TotalNumberOfFiles = 0;

            for (int i = 0; i < imagePropertyGetters.Length; i++)
            {
                var propertyGetter = imagePropertyGetters[i];
                propertyGetter.AddLogger(logger);
                filePropertyGetters[i] = new FilePropertyGetterStatistic(propertyGetter);
            }
        }

        public IReadOnlyDictionary<string, IConvertible>? GetFileProperty(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            TotalNumberOfFiles++;

            IReadOnlyDictionary<string, IConvertible>? firstProperties = null;
            Dictionary<string, IConvertible>? mergedProperties = null;

            for (int i = 0; i < filePropertyGetters.Length; i++)
            {
                var propertyGetters = filePropertyGetters[i];
                if (propertyGetters.FilePropertyGetter.IsSupportedFile(path))
                {
                    propertyGetters.IncrementFile();
                    propertyGetters.StartTimer();
                    var currentProperties = propertyGetters.FilePropertyGetter.GetFileProperties(path);

                    // Performance optimization: Avoid allocating a dictionary enumerator when there are no properties
                    if (currentProperties.Count > 0)
                    {
                        if (firstProperties == null)
                        {
                            // Most common case: only one getter returns properties, so we just hold a reference to it
                            firstProperties = currentProperties;
                        }
                        else
                        {
                            // Rare case: multiple getters returned properties, now we need to merge them
                            if (mergedProperties == null)
                            {
                                mergedProperties = new Dictionary<string, IConvertible>(firstProperties.Count + currentProperties.Count);
                                foreach (var prop in firstProperties)
                                {
                                    mergedProperties.TryAdd(prop.Key, prop.Value);
                                }
                            }

                            foreach (var prop in currentProperties)
                            {
                                mergedProperties.TryAdd(prop.Key, prop.Value);
                            }
                        }
                    }
                    propertyGetters.StopTimer();
                }
            }

            return mergedProperties ?? firstProperties;
        }

        public void LogGetterStatistics()
        {
            foreach (var propertyGetter in filePropertyGetters)
            {
                if (propertyGetter.TotalFiles > 0)
                {
                    var avg = TimeSpan.FromTicks((long)Math.Ceiling(propertyGetter.GetTotalExecutionTime().Ticks / (double)propertyGetter.TotalFiles));
                    logger.Information("{PropertyGetterType} total runtime: {TotalExecutionTime}. Number of files: {TotalFiles}. Average: {AverageTime}",
                        propertyGetter.FilePropertyGetter.GetType(),
                        propertyGetter.GetTotalExecutionTime().ToString("G", CultureInfo.CurrentCulture),
                        propertyGetter.TotalFiles,
                        avg.ToString("G", CultureInfo.CurrentCulture));
                }
            }
            logger.Information("Total number of files {TotalNumberOfFiles}", TotalNumberOfFiles);
        }
    }
}
