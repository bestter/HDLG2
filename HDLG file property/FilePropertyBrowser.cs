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

		private readonly long maxFileSizeBytes;

		private readonly TimeSpan propertyExtractionTimeout;

		private long TotalNumberOfFiles { get; set; }

		public FilePropertyBrowser(Serilog.ILogger logger, params IFilePropertyGetter[] imagePropertyGetters)
			: this(logger, FilePropertyLimits.MaxFileSizeBytes, FilePropertyLimits.PropertyExtractionTimeout, imagePropertyGetters)
		{
		}

		public FilePropertyBrowser(
			Serilog.ILogger logger,
			long maxFileSizeBytes,
			TimeSpan propertyExtractionTimeout,
			params IFilePropertyGetter[] imagePropertyGetters)
		{
			ArgumentNullException.ThrowIfNull(logger);
			ArgumentNullException.ThrowIfNull(imagePropertyGetters);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxFileSizeBytes);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(propertyExtractionTimeout, TimeSpan.Zero);

			this.logger = logger;
			this.maxFileSizeBytes = maxFileSizeBytes;
			this.propertyExtractionTimeout = propertyExtractionTimeout;
			filePropertyGetters = new FilePropertyGetterStatistic[imagePropertyGetters.Length];
			TotalNumberOfFiles = 0;

			for (int i = 0; i < imagePropertyGetters.Length; i++)
			{
				var propertyGetter = imagePropertyGetters[i];
				propertyGetter.AddLogger(logger);
				filePropertyGetters[i] = new FilePropertyGetterStatistic(propertyGetter);
			}
		}

		public virtual IReadOnlyDictionary<string, IConvertible>? GetFileProperty(string path)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(path);
			return GetFileProperty(new FileInfo(path));
		}

		public virtual IReadOnlyDictionary<string, IConvertible>? GetFileProperty(FileInfo fileInfo)
		{
			ArgumentNullException.ThrowIfNull(fileInfo);
			string path = fileInfo.FullName;
			TotalNumberOfFiles++;

			IReadOnlyDictionary<string, IConvertible>? firstProperties = null;
			Dictionary<string, IConvertible>? mergedProperties = null;
			bool? fileSizeAllowed = null;

			for (int i = 0; i < filePropertyGetters.Length; i++)
			{
				var propertyGetters = filePropertyGetters[i];
				if (propertyGetters.FilePropertyGetter.IsSupportedFile(fileInfo.FullName))
				{
					fileSizeAllowed ??= IsFileSizeWithinLimit(fileInfo);
					if (fileSizeAllowed == false)
					{
						continue;
					}

					propertyGetters.IncrementFile();
					propertyGetters.StartTimer();
					var currentProperties = GetFilePropertiesWithTimeout(
						propertyGetters.FilePropertyGetter,
						fileInfo.FullName,
						propertyGetters.FilePropertyGetter.GetType());

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
								AddProperties(mergedProperties, firstProperties);
							}
							AddProperties(mergedProperties, currentProperties);
						}
					}
					propertyGetters.StopTimer();
				}
			}

			return mergedProperties ?? firstProperties;
		}

		private bool IsFileSizeWithinLimit(FileInfo fileInfo)
		{
			try
			{
				if (!fileInfo.Exists)
				{
					return true;
				}

				var fileLength = fileInfo.Length;
				if (fileLength > maxFileSizeBytes)
				{
					logger.Warning(
						"File exceeds maximum allowed size ({MaxFileSizeBytes} bytes, actual {ActualFileSizeBytes} bytes), skipping property extraction: {FilePath}",
						maxFileSizeBytes,
						fileLength,
						fileInfo.FullName);
					return false;
				}
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				logger.Warning(ex, "Cannot determine file size, skipping property extraction: {FilePath}", fileInfo.FullName);
				return false;
			}

			return true;
		}

		private IReadOnlyDictionary<string, IConvertible> GetFilePropertiesWithTimeout(
			IFilePropertyGetter getter,
			string path,
			Type getterType)
		{
			using var cts = new CancellationTokenSource(propertyExtractionTimeout);
			var task = Task.Run(() => getter.GetFileProperties(path), cts.Token);

			try
			{
				if (!task.Wait(propertyExtractionTimeout))
				{
					cts.Cancel();
					logger.Warning(
						"Property extraction timed out after {TimeoutSeconds}s for {PropertyGetterType}: {FilePath}",
						propertyExtractionTimeout.TotalSeconds,
						getterType,
						path);
					return IFilePropertyGetter.EmptyProperties;
				}

				if (task.IsFaulted)
				{
					logger.Warning(
						task.Exception!.GetBaseException(),
						"Property extraction failed for {PropertyGetterType}: {FilePath}",
						getterType,
						path);
					return IFilePropertyGetter.EmptyProperties;
				}

				if (task.IsCanceled)
				{
					logger.Warning(
						"Property extraction was canceled for {PropertyGetterType}: {FilePath}",
						getterType,
						path);
					return IFilePropertyGetter.EmptyProperties;
				}

				return task.Result;
			}
			catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
			{
				logger.Warning(
					"Property extraction timed out after {TimeoutSeconds}s for {PropertyGetterType}: {FilePath}",
					propertyExtractionTimeout.TotalSeconds,
					getterType,
					path);
				return IFilePropertyGetter.EmptyProperties;
			}
		}

		private static void AddProperties(Dictionary<string, IConvertible> target, IReadOnlyDictionary<string, IConvertible> source)
		{
			if (source is Dictionary<string, IConvertible> sourceDict)
			{
				foreach (var prop in sourceDict)
				{
					target.TryAdd(prop.Key, prop.Value);
				}
			}
			else
			{
				foreach (var prop in source)
				{
					target.TryAdd(prop.Key, prop.Value);
				}
			}
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