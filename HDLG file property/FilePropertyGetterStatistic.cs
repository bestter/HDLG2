using System.Diagnostics;

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
