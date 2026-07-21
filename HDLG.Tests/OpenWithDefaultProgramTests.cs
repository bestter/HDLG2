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
            var act = () => MainWindow.OpenWithDefaultProgram(null!, _ => { }, _ => true);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_EmptyPath_ThrowsArgumentException()
        {
            var act = () => MainWindow.OpenWithDefaultProgram("", _ => { }, _ => true);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_WhitespacePath_ThrowsArgumentException()
        {
            var act = () => MainWindow.OpenWithDefaultProgram("   ", _ => { }, _ => true);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void OpenWithDefaultProgram_NonExistentFile_ThrowsFileNotFoundException()
        {
            var nonExistentPath = System.IO.Path.Combine(tempDir, "nonexistent.txt");
            var act = () => MainWindow.OpenWithDefaultProgram(nonExistentPath, _ => { }, _ => true);
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
        [InlineData(".jar")]
        [InlineData(".reg")]
        [InlineData(".lnk")]
        [InlineData(".msc")]
        [InlineData(".vbe")]
        [InlineData(".jse")]
        [InlineData(".scf")]
        [InlineData(".ws")]
        [InlineData(".wsh")]
        [InlineData(".EXE")]
        [InlineData(".Bat")]
        [InlineData(".bat ")]
        [InlineData(".exe.")]
        [InlineData(".cmd.  ")]
        [InlineData(".application")]
        [InlineData(".settingcontent-ms")]
        [InlineData(".library-ms")]
        [InlineData(".appx")]
        [InlineData(".msix")]
        [InlineData(".msixbundle")]
        [InlineData(".msp")]
        [InlineData(".chm")]
        [InlineData(".inf")]
        [InlineData(".dll")]
        [InlineData(".ocx")]
        [InlineData(".sys")]
        [InlineData(".drv")]
        [InlineData(".diagcab")]
        [InlineData(".gadget")]
        [InlineData(".workflow")]
        [InlineData(".shb")]
        [InlineData(".sct")]
        [InlineData(".wsc")]
        [InlineData(".shs")]
        [InlineData(".ps1xml")]
        [InlineData(".psc1")]
        [InlineData(".psd1")]
        [InlineData(".vb")]
        [InlineData(".wsb")]
        [InlineData(".xll")]
        [InlineData(".jnlp")]
        [InlineData(".cab")]
        [InlineData(".hlp")]
        [InlineData(".search-ms")]
        [InlineData(".website")]
        [InlineData(".xbap")]
        [InlineData(".mht")]
        [InlineData(".mhtml")]
        public void OpenWithDefaultProgram_DangerousExtension_ThrowsInvalidOperationException(string extension)
        {
            var dangerousFile = System.IO.Path.Combine(tempDir, $"malicious{extension}");
            System.IO.File.WriteAllText(dangerousFile, "dangerous content");

            var act = () => MainWindow.OpenWithDefaultProgram(dangerousFile, _ => { }, _ => true);
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
                MainWindow.OpenWithDefaultProgram(safeFile, _ => { }, _ => true);
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

        [Fact]
        public void OpenWithDefaultProgram_UnknownExtension_ThrowsInvalidOperationException()
        {
            var unknownFile = System.IO.Path.Combine(tempDir, "unknown.xyz123");
            System.IO.File.WriteAllText(unknownFile, "unknown content");

            var act = () => MainWindow.OpenWithDefaultProgram(unknownFile, _ => { }, _ => true);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not allowed for security reasons*");
        }



        [Fact]
        public void OpenWithDefaultProgram_FileChangedDuringPrompt_ThrowsInvalidOperationException()
        {
            var safeFile = System.IO.Path.Combine(tempDir, "toctou.txt");
            System.IO.File.WriteAllText(safeFile, "original content");

            bool executed = false;
            var act = () => MainWindow.OpenWithDefaultProgram(
                safeFile,
                _ => { executed = true; },
                _ =>
                {
                    System.IO.File.WriteAllText(safeFile, "tampered content");
                    return true;
                });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*changed since you reviewed it*");
            executed.Should().BeFalse();
        }

        [Fact]
        public void OpenWithDefaultProgram_FileDeletedDuringPrompt_ThrowsFileNotFoundException()
        {
            var safeFile = System.IO.Path.Combine(tempDir, "deleted.txt");
            System.IO.File.WriteAllText(safeFile, "content");

            bool executed = false;
            var act = () => MainWindow.OpenWithDefaultProgram(
                safeFile,
                _ => { executed = true; },
                _ =>
                {
                    System.IO.File.Delete(safeFile);
                    return true;
                });

            act.Should().Throw<FileNotFoundException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void OpenWithDefaultProgram_ExtensionBecomesDangerousDuringPrompt_ThrowsInvalidOperationException()
        {
            var safeFile = System.IO.Path.Combine(tempDir, "swap.txt");
            System.IO.File.WriteAllText(safeFile, "content");

            bool executed = false;
            var act = () => MainWindow.OpenWithDefaultProgram(
                safeFile,
                _ => { executed = true; },
                _ => true,
                (_, afterUserConfirmation) => afterUserConfirmation ? ".exe" : ".txt");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not allowed for security reasons*");
            executed.Should().BeFalse();
        }


        [Fact]
        public void OpenWithDefaultProgram_FileUnchangedAfterPrompt_Executes()
        {
            var safeFile = System.IO.Path.Combine(tempDir, "stable.txt");
            System.IO.File.WriteAllText(safeFile, "stable content");

            bool executed = false;
            MainWindow.OpenWithDefaultProgram(safeFile, _ => { executed = true; }, _ => true);

            executed.Should().BeTrue();
        }
    }
}
