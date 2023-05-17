using HdlgFileProperty;
using Serilog.Core;
using System.Collections.ObjectModel;

namespace HDLG_winforms
{

    public class Directory : IEquatable<Directory>, IComparable, IComparable<Directory>
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public DateTime CreationTime { get; private set; }

        private readonly DirectoryInfo directoryInfo;

        private readonly List<Directory> directories = new();

        public bool IsTopDirectory { get; private set; }

        public bool BrowseSubdirectory { get; private set; }

        public ReadOnlyCollection<Directory> Directories => directories.AsReadOnly();

        private readonly List<File> files = new();

        public ReadOnlyCollection<File> Files => files.AsReadOnly();

        public int DirectoriesCount => directories.Count;
        public int FilesCount => files.Count;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger log;

        public Directory(string path, bool isTopDirectory, bool browseSubdirectory, Logger log) : this(new DirectoryInfo(path), isTopDirectory, browseSubdirectory, log)
        {

        }

        public Directory(DirectoryInfo directory, bool isTopDirectory, bool browseSubdirectory, Logger log)
        {
            directoryInfo = directory;
            Path = directory.FullName;
            Name = directory.Name;
            CreationTime = directory.CreationTimeUtc.ToLocalTime();
            IsTopDirectory = isTopDirectory;
            BrowseSubdirectory = browseSubdirectory;
            this.log = log;
        }


        /// <summary>
        /// Browse the content
        /// </summary>
        /// <param name="propertyBrowser"></param>
        public void Browse(FilePropertyBrowser propertyBrowser)
        {
            log.Debug($"Directory: {Path} {nameof(IsTopDirectory)}: {IsTopDirectory} {nameof(BrowseSubdirectory)}: {BrowseSubdirectory}");

            if (BrowseSubdirectory)
            {
                directoryInfo.EnumerateDirectories().ToList().ForEach(d =>
                {
                    directories.Add(new Directory(d.FullName, false, true, log));
                });
                directories.Sort();
            }

            directoryInfo.EnumerateFiles().ToList().ForEach(f =>
            {
                var properties = propertyBrowser.GetFileProperty(f.FullName);
                var file = new File(f.FullName, properties);
                files.Add(file);
            });
            files.Sort();

            foreach (Directory d in directories)
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
            if (obj is Directory directory)
            {
                return directory.Equals(this);
            }
            return false;
        }

        public bool Equals(Directory? other)
        {
            if (other is not null)
            {
                return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public int CompareTo(object? obj)
        {
            if (obj is Directory directory)
            {
                return CompareTo(directory);
            }
            return -1;
        }

        public int CompareTo(Directory? other)
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

        public static bool operator ==(Directory left, Directory right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Directory left, Directory right)
        {
            return !(left == right);
        }

        public static bool operator <(Directory left, Directory right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Directory left, Directory right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Directory left, Directory right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Directory left, Directory right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
