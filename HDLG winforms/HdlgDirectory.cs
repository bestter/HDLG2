/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using HdlgFileProperty;
using Serilog;
using System.Collections.ObjectModel;

namespace HDLG_winforms
{

    public class HdlgDirectory : IEquatable<HdlgDirectory>, IComparable, IComparable<HdlgDirectory>
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public DateTime CreationTime { get; private set; }

        private readonly DirectoryInfo directoryInfo;

		private readonly List<HdlgDirectory> directories = [];

        public bool IsTopDirectory { get; private set; }

        public bool BrowseSubdirectory { get; private set; }

        public IReadOnlyList<HdlgDirectory> Directories => directories;

        private readonly List<HdlgFile> files = [];

        public IReadOnlyList<HdlgFile> Files => files;

        public int DirectoriesCount => directories.Count;
        public int FilesCount => files.Count;

        /// <summary>
        /// Total number of directories in this subtree (computed once during Browse for performance).
        /// </summary>
        public long TotalDirectories { get; private set; }

        /// <summary>
        /// Total number of files in this subtree (computed once during Browse for performance).
        /// </summary>
        public long TotalFiles { get; private set; }

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger log;

        public HdlgDirectory(string path, bool isTopDirectory, bool browseSubdirectory, ILogger log) : this(new DirectoryInfo(path), isTopDirectory, browseSubdirectory, log)
        {

        }

        public HdlgDirectory(DirectoryInfo directory, bool isTopDirectory, bool browseSubdirectory, ILogger log)
        {
            directoryInfo = directory ?? throw new ArgumentNullException( nameof( directory ) );
            Path = directory.FullName;
            Name = directory.Name;
            CreationTime = directory.CreationTimeUtc.ToLocalTime();
            IsTopDirectory = isTopDirectory;
            BrowseSubdirectory = browseSubdirectory;
            this.log = log ?? throw new ArgumentNullException( nameof( log ) );
        }


        /// <summary>
        /// Browse the content
        /// </summary>
        /// <param name="propertyBrowser"></param>
        public void Browse(FilePropertyBrowser propertyBrowser)
        {
			ArgumentNullException.ThrowIfNull( propertyBrowser );
			log.Debug("Directory: {Path} {IsTopDirectoryName}: {IsTopDirectory} {BrowseSubdirectoryName}: {BrowseSubdirectory}", Path, nameof(IsTopDirectory), IsTopDirectory, nameof(BrowseSubdirectory), BrowseSubdirectory);

            if (BrowseSubdirectory)
            {
                foreach (var d in directoryInfo.EnumerateDirectories())
                {
                    if ((d.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    directories.Add(new HdlgDirectory(d, false, true, log));
                }
                directories.Sort();
            }

            foreach (var f in directoryInfo.EnumerateFiles())
            {
                var properties = propertyBrowser.GetFileProperty(f.FullName);
                var file = new HdlgFile(f, properties);
                files.Add(file);
            }
            files.Sort();

            foreach (HdlgDirectory d in directories)
            {
                d.Browse(propertyBrowser);
            }

            // Compute subtree totals once (post-order) so export counts become O(1) with no extra recursion.
            TotalDirectories = directories.Count;
            foreach (HdlgDirectory d in directories)
            {
                TotalDirectories += d.TotalDirectories;
            }
            TotalFiles = files.Count;
            foreach (HdlgDirectory d in directories)
            {
                TotalFiles += d.TotalFiles;
            }
        }



        public override string ToString() { return Path; }

        public override int GetHashCode()
        {
            return Path.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            if (obj is HdlgDirectory directory)
            {
                return Equals(directory);
            }
            return false;
        }

        public bool Equals(HdlgDirectory? other)
        {
            if (other is not null)
            {
                return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is HdlgDirectory directory)
            {
                return CompareTo(directory);
            }
            throw new ArgumentException("Object is not a HdlgDirectory");
        }

        public int CompareTo(HdlgDirectory? other)
        {
            if (other is null)
            {
                return 1;
            }
            int compareValue = other.IsTopDirectory.CompareTo(IsTopDirectory);
            if (compareValue == 0)
            {
                compareValue = string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return compareValue;
        }

        public static bool operator ==(HdlgDirectory left, HdlgDirectory right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(HdlgDirectory left, HdlgDirectory right)
        {
            return !(left == right);
        }

        public static bool operator <(HdlgDirectory left, HdlgDirectory right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(HdlgDirectory left, HdlgDirectory right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(HdlgDirectory left, HdlgDirectory right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(HdlgDirectory left, HdlgDirectory right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
