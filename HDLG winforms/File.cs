namespace HDLG_winforms
{
    public class File : IEquatable<File>, IComparable, IComparable<File>
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public string Extension { get; private set; }

        public long Size { get; private set; }

        public DateTime CreationTime { get; private set; }

        public Dictionary<string, IConvertible> Properties { get; private set; }

        public File(string path, Dictionary<string, IConvertible> properties)
        {
            Path = path;
            FileInfo info = new(Path);
            if (info.Exists)
            {
                Name = info.Name;
                Extension = info.Extension;
                Size = info.Length;
                CreationTime = info.CreationTime;
                Properties = new Dictionary<string, IConvertible>(properties);
            }
            else
            {
                throw new NotSupportedException($"Fichier {Path} n'existe pas");
            }
        }

        public override string ToString() { return Path; }

        public override int GetHashCode()
        {
            return Path.GetHashCode(StringComparison.OrdinalIgnoreCase);
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
            if (other is not null)
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
            if (other is not null)
            {
                return string.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return -1;
        }

        public static bool operator ==(File left, File right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(File left, File right)
        {
            return !(left == right);
        }

        public static bool operator <(File left, File right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(File left, File right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(File left, File right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(File left, File right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
