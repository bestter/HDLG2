/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>. 
 */

using HdlgFileProperty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdlgFileProperty
{
    internal class FilePropertyGetterStatistic
    {
        public FilePropertyGetterStatistic(IFilePropertyGetter filePropertyGetter)
        {
            FilePropertyGetter = filePropertyGetter ?? throw new ArgumentNullException(nameof(filePropertyGetter));
            Stopwatch = new Stopwatch();
        }

        public IFilePropertyGetter FilePropertyGetter { get; private set; }

        private Stopwatch Stopwatch { get; set; }

        public long TotalFiles { get; private set; }

        public void StartTimer()
        {
            Stopwatch.Start();
        }
        public void StopTimer()
        {
            Stopwatch.Stop();
        }

        public TimeSpan GetTotalExecutionTime()
        {
            return Stopwatch.Elapsed.Duration();
        }

        public void IncrementFile()
        {
            TotalFiles++;
        }
    }
}
