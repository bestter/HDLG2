namespace HDLG_winforms
{
    public class File : IEquatable<File>, IComparable, IComparable<File>
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public string Extension { get; private set; }

        public long Size { get; private set; }

        public DateTime CreationTime { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        public File(string path, Dictionary<string, string> properties)
        {
            Path = path;
            FileInfo info = new(Path);
            if (info.Exists)
            {
                Name = info.Name;
                Extension = info.Extension;
                Size = info.Length;
                CreationTime = info.CreationTime;
                Properties = new Dictionary<string, string>(properties);
            }
            else
            {
                throw new NotSupportedException($"Fichier {Path} n'existe pas");
            }
        }

        public override string ToString() { return Path; }

        public override int GetHashCode()
        {
            return Path.GetHashCode(System.StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            if (obj is File file)
            {
                return file.Equals(this);
            }
            return false;
        }

        public bool Equals(File? other)
        {
            if (other != null)
            {
                return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public int CompareTo(object? obj)
        {
            if (obj is File file)
            {
                return CompareTo(file);
            }
            return -1;
        }

        public int CompareTo(File? other)
        {
            if (other != null)
            {
                return string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return -1;
        }
    }
}
