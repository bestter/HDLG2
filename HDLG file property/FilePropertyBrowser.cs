/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */

using System.Globalization;

namespace HdlgFileProperty
{
    public class FilePropertyBrowser
    {
        private readonly List<FilePropertyGetterStatistic> filePropertyGetters;

        private readonly Serilog.ILogger logger;

        private long TotalNumberOfFiles { get; set; }

        public FilePropertyBrowser(Serilog.ILogger logger, params IFilePropertyGetter[] imagePropertyGetters)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(imagePropertyGetters);

            this.logger = logger;
            filePropertyGetters = new(imagePropertyGetters.Length);
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
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
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
