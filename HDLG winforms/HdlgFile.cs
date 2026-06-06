/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
namespace HDLG_winforms
{
	public class HdlgFile : IEquatable<HdlgFile>, IComparable, IComparable<HdlgFile>
	{
		public string Name { get; }

		public string Path { get; }

		public string Extension { get; }

		public long Size { get; }

		public DateTime CreationTime { get; }

		public IReadOnlyDictionary<string, IConvertible> Properties { get; }

		private static readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, IConvertible> EmptyProperties = new(new Dictionary<string, IConvertible>());

        public HdlgFile(string path, IReadOnlyDictionary<string, IConvertible>? properties)
            : this(new FileInfo(path ?? throw new ArgumentNullException(nameof(path))), properties)
        {
        }

        public HdlgFile(FileInfo info, IReadOnlyDictionary<string, IConvertible>? properties)
        {
            ArgumentNullException.ThrowIfNull(info);

            Path = info.FullName;

			if (info.Exists)
			{
				Name = info.Name;
				Extension = info.Extension;
				Size = info.Length;
				CreationTime = info.CreationTime;
				Properties = properties != null && properties.Count > 0
					? properties
					: EmptyProperties;
			}
			else
			{
				throw new FileNotFoundException( $"Le fichier {Path} n'existe pas", Path );
			}
		}

		public override string ToString () { return Path; }

		public override int GetHashCode ()
		{
			return Path.GetHashCode( StringComparison.OrdinalIgnoreCase );
		}

		public override bool Equals (object? obj)
		{
			return Equals( obj as HdlgFile );
		}

		public bool Equals (HdlgFile? other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals( this, other ))
			{
				return true;
			}

			return string.Equals( Path, other.Path, StringComparison.OrdinalIgnoreCase );
		}

		public int CompareTo (object? obj)
		{
			if (obj is null)
			{
				return 1;
			}

			if (obj is HdlgFile file)
			{
				return CompareTo( file );
			}

			throw new ArgumentException( "L'objet doit être de type HdlgFile", nameof(obj) );
		}

		public int CompareTo (HdlgFile? other)
		{
			if (other is null)
			{
				return 1;
			}

			return string.Compare( Path, other.Path, StringComparison.OrdinalIgnoreCase );
		}

		public static bool operator == (HdlgFile? left, HdlgFile? right)
		{
			if (left is null)
			{
				return right is null;
			}

			return left.Equals( right );
		}

		public static bool operator != (HdlgFile? left, HdlgFile? right)
		{
			return !(left == right);
		}

		public static bool operator < (HdlgFile? left, HdlgFile? right)
		{
			return Comparer<HdlgFile>.Default.Compare( left, right ) < 0;
		}

		public static bool operator <= (HdlgFile? left, HdlgFile? right)
		{
			return Comparer<HdlgFile>.Default.Compare( left, right ) <= 0;
		}

		public static bool operator > (HdlgFile? left, HdlgFile? right)
		{
			return Comparer<HdlgFile>.Default.Compare( left, right ) > 0;
		}

		public static bool operator >= (HdlgFile? left, HdlgFile? right)
		{
			return Comparer<HdlgFile>.Default.Compare( left, right ) >= 0;
		}
	}
}
