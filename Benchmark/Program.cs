using System;
using System.Diagnostics;
using System.Linq;
using HDLG_winforms;
using HdlgFileProperty;
using Serilog;

namespace Benchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting benchmark...");

            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error() // keep it quiet
                .CreateLogger();

            string testDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "HdlgBenchmark");
            if (System.IO.Directory.Exists(testDir))
            {
                System.IO.Directory.Delete(testDir, true);
            }
            System.IO.Directory.CreateDirectory(testDir);

            CreateTree(testDir, 4, 5);

            var propertyBrowser = new FilePropertyBrowser(Log.Logger);

            // Warmup
            var warmupDir = new HdlgDirectory(testDir, true, true, Log.Logger);
            await warmupDir.BrowseAsync(propertyBrowser);

            int iterations = 10;
            long totalMs = 0;

            for (int i = 0; i < iterations; i++)
            {
                var dir = new HdlgDirectory(testDir, true, true, Log.Logger);
                long startTimestamp = Stopwatch.GetTimestamp();
                await dir.BrowseAsync(propertyBrowser);
                var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
                totalMs += (long)elapsed.TotalMilliseconds;
                Console.WriteLine($"Iteration {i}: {elapsed.TotalMilliseconds} ms (Dirs: {dir.TotalDirectories}, Files: {dir.TotalFiles})");
            }

            Console.WriteLine($"Average: {totalMs / (double)iterations} ms");

            System.IO.Directory.Delete(testDir, true);
        }

        static void CreateTree(string root, int depth, int branches)
        {
            for (int i = 0; i < 5; i++)
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(root, $"file_{i}.txt"), "test");
            }

            if (depth == 0) return;

            for (int i = 0; i < branches; i++)
            {
                var dir = System.IO.Path.Combine(root, $"dir_{i}");
                System.IO.Directory.CreateDirectory(dir);
                CreateTree(dir, depth - 1, branches);
            }
        }
    }
}
