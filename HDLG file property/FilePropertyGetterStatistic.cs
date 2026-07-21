/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */

namespace HdlgFileProperty
{
	internal class FilePropertyGetterStatistic
	{
		public FilePropertyGetterStatistic (IFilePropertyGetter filePropertyGetter)
		{
			FilePropertyGetter = filePropertyGetter ?? throw new ArgumentNullException( nameof( filePropertyGetter ) );
		}

		public IFilePropertyGetter FilePropertyGetter { get; private set; }

		private long _totalExecutionTimeTicks;

		private long _totalFiles;
		public long TotalFiles => System.Threading.Interlocked.Read( ref _totalFiles );

		public void AddExecutionTime (TimeSpan timeSpan)
		{
			System.Threading.Interlocked.Add( ref _totalExecutionTimeTicks, timeSpan.Ticks );
		}

		public TimeSpan GetTotalExecutionTime ()
		{
			return TimeSpan.FromTicks( System.Threading.Interlocked.Read( ref _totalExecutionTimeTicks ) );
		}

		public void IncrementFile ()
		{
			System.Threading.Interlocked.Increment( ref _totalFiles );
		}
	}
}
