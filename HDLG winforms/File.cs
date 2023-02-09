using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLG_winforms
{
    internal class File
    {
        public string Path { get; private set; }

        public string Extension { get; private set; }

        public long Size { get; private set; }

        public DateTime CreationTime { get; private set; }

        public File(string path)
        {
            Path = path;
            FileInfo info = new(Path);
            if (info.Exists)
            {
                Extension = info.Extension;
                Size = info.Length;
                CreationTime = info.CreationTime;
            }
            else
            {
                throw new NotSupportedException($"Fichier {Path} n'existe pas");
            }
        }

        public override string ToString() { return Path; }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
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
                return Path == other.Path;
            }
            return false;
        }
    }
}
