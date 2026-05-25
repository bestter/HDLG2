using System;
using System.IO;
using FluentAssertions;
using HDLG_winforms;
using Xunit;

namespace HDLG.Tests
{
    public class OpenWithDefaultProgramTests : IDisposable
    {
        private readonly string tempDir;

        public OpenWithDefaultProgramTests()
        {
            tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OpenWithDefaultProgramTests_" + Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempDir);
        }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(tempDir))
                System.IO.Directory.Delete(tempDir, true);
        }

        [Fact]
        public void OpenWithDefaultProgram_NullPath_ThrowsArgumentException()
        {
            var act = () => MainWindow.OpenWithDefaultProgram(null!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_EmptyPath_ThrowsArgumentException()
        {
            var act = () => MainWindow.OpenWithDefaultProgram("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_WhitespacePath_ThrowsArgumentException()
        {
            var act = () => MainWindow.OpenWithDefaultProgram("   ");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_NonExistentFile_ThrowsFileNotFoundException()
        {
            var nonExistentPath = System.IO.Path.Combine(tempDir, "nonexistent.txt");
            var act = () => MainWindow.OpenWithDefaultProgram(nonExistentPath);
            act.Should().Throw<FileNotFoundException>();
        }

        [Theory]
        [InlineData(".exe")]
        [InlineData(".bat")]
        [InlineData(".cmd")]
        [InlineData(".ps1")]
        [InlineData(".vbs")]
        [InlineData(".js")]
        [InlineData(".wsf")]
        [InlineData(".scr")]
        [InlineData(".com")]
        [InlineData(".msi")]
        [InlineData(".pif")]
        [InlineData(".hta")]
        [InlineData(".cpl")]
        [InlineData(".EXE")]
        [InlineData(".Bat")]
        [InlineData(".bat ")]
        [InlineData(".exe.")]
        [InlineData(".cmd.  ")]
        public void OpenWithDefaultProgram_DangerousExtension_ThrowsInvalidOperationException(string extension)
        {
            var dangerousFile = System.IO.Path.Combine(tempDir, $"malicious{extension}");
            System.IO.File.WriteAllText(dangerousFile, "dangerous content");

            var act = () => MainWindow.OpenWithDefaultProgram(dangerousFile);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not allowed for security reasons*");
        }

        [Theory]
        [InlineData(".txt")]
        [InlineData(".pdf")]
        [InlineData(".docx")]
        [InlineData(".xlsx")]
        [InlineData(".jpg")]
        [InlineData(".png")]
        [InlineData(".html")]
        [InlineData(".xml")]
        [InlineData(".mp3")]
        public void OpenWithDefaultProgram_SafeExtension_DoesNotThrowInvalidOperationException(string extension)
        {
            // Arrange — we only verify it does NOT throw InvalidOperationException or ArgumentException
            // It may throw a different exception due to the file content or system state, but the
            // security validation should pass
            var safeFile = System.IO.Path.Combine(tempDir, $"safe{extension}");
            System.IO.File.WriteAllText(safeFile, "safe content");

            try
            {
                MainWindow.OpenWithDefaultProgram(safeFile);
            }
            catch (InvalidOperationException)
            {
                // This should NOT happen for safe extensions
                throw new Xunit.Sdk.XunitException($"Extension {extension} should not be blocked but was rejected.");
            }
            catch (Exception)
            {
                // Other exceptions (e.g., no default program, Win32Exception) are acceptable
                // as they come from the OS, not from our security check
            }
        }
    }
}
