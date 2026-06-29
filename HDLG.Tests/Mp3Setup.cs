using System;
using System.IO;

namespace HDLG.Tests
{
    internal static class Mp3Setup
    {
        private static bool _initialized = false;
        private static readonly object _lock = new();

        public static void CreateMp3Docs()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;

                // Base64 encoding of a small dummy MP3 file.
                // Replace with actual base64 of test files if needed or generate it dynamically.
                string testMp3Base64 = "SUQzBAAAAAABLlRJVDIAAAAMAAADVGVzdCBUaXRsZQBUUEUxAAAADQAAA1Rlc3QgQXJ0aXN0AFRBTEIAAAAMAAADVGVzdCBBbGJ1bQB0ZW5jAAAADAAAA1Rlc3QgRW5jb2RlciAA";
                // Using a slightly more complete dummy for 'test_full.mp3' or we can just copy
                string testMp3InvalidBase64 = "aW52YWxpZCBkYXRh";

                File.WriteAllBytes("test.mp3", Convert.FromBase64String(testMp3Base64));
                File.WriteAllBytes("test_invalid.mp3", Convert.FromBase64String(testMp3InvalidBase64));

                _initialized = true;
            }
        }

        public static void Cleanup()
        {
            // Optional: delete temporary files
        }
    }
}
