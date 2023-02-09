using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLG_winforms
{

    internal class Directory: IEquatable<Directory>
    {
        private readonly string _path;

        private readonly List<Directory> directories = new();

        private readonly List<File> files = new();

        public Directory(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public void Browse()
        {
            DirectoryInfo directory = new(_path);
            directory.EnumerateDirectories().ToList().ForEach(d =>
            {
                directories.Add(new Directory(d.FullName));
            });

            directory.EnumerateFiles().ToList().ForEach(f =>
            {
                files.Add(new File(f.FullName));
            });

            foreach (Directory d in directories)
            {
                d.Browse();
            }
        }

        public override string ToString() { return _path; }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
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
                return _path == other._path;
            }
            return false;
        }
    }
}
