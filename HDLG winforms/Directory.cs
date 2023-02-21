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

        public ReadOnlyCollection<Directory> Directories => directories.AsReadOnly();

        private readonly List<File> files = new();

        public ReadOnlyCollection<File> Files => files.AsReadOnly();

        public int DirectoriesCount => directories.Count;
        public int FilesCount => files.Count;

        private readonly Logger log;

        public Directory(string path, bool isTopDirectory, Logger log) : this(new DirectoryInfo(path), isTopDirectory, log)
        {

        }

        public Directory(DirectoryInfo directory, bool isTopDirectory, Logger log)
        {
            directoryInfo = directory;
            Path = directory.FullName;
            Name = directory.Name;
            CreationTime = directory.CreationTimeUtc.ToLocalTime();
            IsTopDirectory = isTopDirectory;
            this.log = log;
        }


        /// <summary>
        /// Browse the content
        /// </summary>
        /// <param name="propertyBrowser"></param>
        public void Browse(FilePropertyBrowser propertyBrowser)
        {
            log.Debug($"Directory: {Path} {nameof(IsTopDirectory)}: {IsTopDirectory}");

            directoryInfo.EnumerateDirectories().ToList().ForEach(d =>
            {
                directories.Add(new Directory(d.FullName, false, log));
            });
            directories.Sort();

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
            if (other != null)
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
            if (other != null)
            {
                int compareValue = other.IsTopDirectory.CompareTo(IsTopDirectory);
                if (compareValue == 0)
                {
                    compareValue = string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
                }
                return compareValue;
            }
            return -1;
        }
    }
}
