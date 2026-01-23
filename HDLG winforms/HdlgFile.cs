/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */
namespace HDLG_winforms
{
	public class HdlgFile : IEquatable<HdlgFile>, IComparable, IComparable<HdlgFile>
	{
		public string Name { get; private set; }

		public string Path { get; private set; }

		public string Extension { get; private set; }

		public long Size { get; private set; }

		public DateTime CreationTime { get; private set; }

		public Dictionary<string, IConvertible> Properties { get; private set; }

		public HdlgFile (string path, Dictionary<string, IConvertible> properties)
		{
			Path = path;
			FileInfo info = new( Path );
			if (info.Exists)
			{
				Name = info.Name;
				Extension = info.Extension;
				Size = info.Length;
				CreationTime = info.CreationTime;
				Properties = new Dictionary<string, IConvertible>( properties );
			}
			else
			{
				throw new NotSupportedException( $"Fichier {Path} n'existe pas" );
			}
		}

		public override string ToString () { return Path; }

		public override int GetHashCode ()
		{
			return Path.GetHashCode( StringComparison.OrdinalIgnoreCase );
		}

		public override bool Equals (object? obj)
		{
			if (obj is HdlgFile file)
			{
				return file.Equals( this );
			}
			return false;
		}

		public bool Equals (HdlgFile? other)
		{
			if (other is not null)
			{
				return string.Equals( Path, other.Path, StringComparison.OrdinalIgnoreCase );
			}
			return false;
		}

		public int CompareTo (object? obj)
		{
			if (obj is HdlgFile file)
			{
				return CompareTo( file );
			}
			return -1;
		}

		public int CompareTo (HdlgFile? other)
		{
			if (other is not null)
			{
				return string.Compare( Path, other.Path, StringComparison.OrdinalIgnoreCase );
			}
			return -1;
		}

		public static bool operator == (HdlgFile left, HdlgFile right)
		{
			if (left is null)
			{
				return right is null;
			}

			return left.Equals( right );
		}

		public static bool operator != (HdlgFile left, HdlgFile right)
		{
			return !(left == right);
		}

		public static bool operator < (HdlgFile left, HdlgFile right)
		{
			return left is null ? right is not null : left.CompareTo( right ) < 0;
		}

		public static bool operator <= (HdlgFile left, HdlgFile right)
		{
			return left is null || left.CompareTo( right ) <= 0;
		}

		public static bool operator > (HdlgFile left, HdlgFile right)
		{
			return left is not null && left.CompareTo( right ) > 0;
		}

		public static bool operator >= (HdlgFile left, HdlgFile right)
		{
			return left is null ? right is null : left.CompareTo( right ) >= 0;
		}
	}
}
