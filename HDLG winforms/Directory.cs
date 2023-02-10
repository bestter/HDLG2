using System.Collections.ObjectModel;

namespace HDLG_winforms
{

    internal class Directory : IEquatable<Directory>, IComparable, IComparable<Directory>
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public DateTime CreationTime { get; private set; }

        private readonly List<Directory> directories = new();

        public ReadOnlyCollection<Directory> Directories => directories.AsReadOnly();

        private readonly List<File> files = new();

        public ReadOnlyCollection<File> Files => files.AsReadOnly();

        public int DirectoriesCount => directories.Count;
        public int FilesCount => files.Count;

        public Directory(string path)
        {
            DirectoryInfo directory = new(path);
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Name = directory.Name;
            CreationTime = directory.CreationTimeUtc.ToLocalTime();
        }

        public void Browse()
        {
            DirectoryInfo directory = new(Path);
            directory.EnumerateDirectories().ToList().ForEach(d =>
            {
                directories.Add(new Directory(d.FullName));
            });
            directories.Sort();

            directory.EnumerateFiles().ToList().ForEach(f =>
            {
                files.Add(new File(f.FullName));
            });
            files.Sort();

            foreach (Directory d in directories)
            {
                d.Browse();
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
                return Path == other.Path;
            }
            return false;
        }

        public int CompareTo(object? obj)
        {
            if (obj is Directory directory)
            {
                return directory.CompareTo(this);
            }
            return -1;
        }

        public int CompareTo(Directory? other)
        {
            if (other != null)
            {
                return string.Compare(other.Path, Path, StringComparison.OrdinalIgnoreCase);
            }
            return -1;
        }
    }
}
