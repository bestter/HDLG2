using HdlgFileProperty;
using Serilog.Core;
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

        public ReadOnlyCollection<HdlgDirectory> Directories => directories.AsReadOnly();

        private readonly List<HdlgFile> files = new();

        public ReadOnlyCollection<HdlgFile> Files => files.AsReadOnly();

        public int DirectoriesCount => directories.Count;
        public int FilesCount => files.Count;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger log;

        public HdlgDirectory(string path, bool isTopDirectory, bool browseSubdirectory, Logger log) : this(new DirectoryInfo(path), isTopDirectory, browseSubdirectory, log)
        {

        }

        public HdlgDirectory(DirectoryInfo directory, bool isTopDirectory, bool browseSubdirectory, Logger log)
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
			log.Debug($"Directory: {Path} {nameof(IsTopDirectory)}: {IsTopDirectory} {nameof(BrowseSubdirectory)}: {BrowseSubdirectory}");

            if (BrowseSubdirectory)
            {
                directoryInfo.EnumerateDirectories().ToList().ForEach(d =>
                {
                    directories.Add(new HdlgDirectory(d.FullName, false, true, log));
                });
                directories.Sort();
            }

            directoryInfo.EnumerateFiles().ToList().ForEach(f =>
            {
                var properties = propertyBrowser.GetFileProperty(f.FullName);
                var file = new HdlgFile(f.FullName, properties);
                files.Add(file);
            });
            files.Sort();

            foreach (HdlgDirectory d in directories)
            {
                d.Browse(propertyBrowser);
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
                return directory.Equals(this);
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
            if (obj is HdlgDirectory directory)
            {
                return CompareTo(directory);
            }
            return -1;
        }

        public int CompareTo(HdlgDirectory? other)
        {
            if (other is null)
            {
                return -1;
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
