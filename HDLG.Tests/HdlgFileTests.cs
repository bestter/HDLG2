using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using HDLG_winforms;
using Xunit;

namespace HDLG.Tests
{
    public class HdlgFileTests : IDisposable
    {
        private readonly string baseDirectoryPath;
        private readonly string existingFilePath;

        public HdlgFileTests()
        {
            baseDirectoryPath = Path.Combine(Path.GetTempPath(), "HdlgFileTests_" + Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(baseDirectoryPath);
            existingFilePath = Path.Combine(baseDirectoryPath, "testFile.txt");
            System.IO.File.WriteAllText(existingFilePath, "test content");
        }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(baseDirectoryPath))
            {
                System.IO.Directory.Delete(baseDirectoryPath, true);
            }
        }

        [Fact]
        public void Constructor_NullPath_ThrowsArgumentNullException()
        {
            Action action = () => new HdlgFile((string)null!, null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_NullFileInfo_ThrowsArgumentNullException()
        {
            Action action = () => new HdlgFile((FileInfo)null!, null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_NonExistentFile_ThrowsFileNotFoundException()
        {
            string nonExistentPath = Path.Combine(baseDirectoryPath, "doesNotExist.txt");
            Action action = () => new HdlgFile(nonExistentPath, null);
            action.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void Constructor_ExistingFile_PopulatesProperties()
        {
            var properties = new Dictionary<string, IConvertible> { { "TestProp", "TestValue" } };
            var hdlgFile = new HdlgFile(existingFilePath, properties);

            hdlgFile.Path.Should().Be(existingFilePath);
            hdlgFile.Name.Should().Be("testFile.txt");
            hdlgFile.Extension.Should().Be(".txt");
            hdlgFile.Size.Should().Be(12); // "test content" is 12 bytes
            hdlgFile.CreationTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            hdlgFile.Properties.Should().ContainKey("TestProp").WhoseValue.Should().Be("TestValue");
        }

        [Fact]
        public void Constructor_NullProperties_SetsEmptyReadOnlyDictionary()
        {
            var hdlgFile = new HdlgFile(existingFilePath, null);
            hdlgFile.Properties.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_EmptyProperties_SetsEmptyReadOnlyDictionary()
        {
            var hdlgFile = new HdlgFile(existingFilePath, new Dictionary<string, IConvertible>());
            hdlgFile.Properties.Should().BeEmpty();
        }

        [Fact]
        public void ToString_ReturnsFilePath()
        {
            var hdlgFile = new HdlgFile(existingFilePath, null);
            hdlgFile.ToString().Should().Be(existingFilePath);
        }

        [Fact]
        public void GetHashCode_SamePathDifferentCase_ReturnsSameHashCode()
        {
            var file1 = new HdlgFile(existingFilePath, null);

            int expectedHash = existingFilePath.GetHashCode(StringComparison.OrdinalIgnoreCase);

            file1.GetHashCode().Should().Be(expectedHash);
        }
        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            file1.Equals(file1).Should().BeTrue();
            file1.Equals((object)file1).Should().BeTrue();
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            HdlgFile? nullFile = null;
            bool result = file1.Equals(nullFile);
            result.Should().BeFalse();
            bool resultObj = file1.Equals((object)null!);
            resultObj.Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            file1.Equals(new object()).Should().BeFalse();
        }

        [Fact]
        public void Equals_SamePath_ReturnsTrue()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            var file2 = new HdlgFile(existingFilePath, null);

            file1.Equals(file2).Should().BeTrue();
            file1.Equals((object)file2).Should().BeTrue();
        }

        [Fact]
        public void CompareTo_Null_ReturnsGreaterThanZero()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            file1.CompareTo(null).Should().BePositive();
            file1.CompareTo((object?)null).Should().BePositive();
        }

        [Fact]
        public void CompareTo_InvalidType_ThrowsArgumentException()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            Action action = () => file1.CompareTo(new object());
            action.Should().Throw<ArgumentException>();
        }
        [Fact]
        public void CompareTo_SamePath_ReturnsZero()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            var file2 = new HdlgFile(existingFilePath, null);

            file1.CompareTo(file2).Should().Be(0);
            file1.CompareTo((object)file2).Should().Be(0);
        }

        [Fact]
        public void CompareTo_DifferentPaths_ReturnsCorrectOrder()
        {
            var pathA = Path.Combine(baseDirectoryPath, "a.txt");
            var pathB = Path.Combine(baseDirectoryPath, "b.txt");

            System.IO.File.WriteAllText(pathA, "a");
            System.IO.File.WriteAllText(pathB, "b");

            var fileA = new HdlgFile(pathA, null);
            var fileB = new HdlgFile(pathB, null);

            fileA.CompareTo(fileB).Should().BeNegative();
            fileA.CompareTo((object)fileB).Should().BeNegative();
            fileB.CompareTo(fileA).Should().BePositive();
            fileB.CompareTo((object)fileA).Should().BePositive();
        }

        [Fact]
        public void EqualityOperators_SamePath_ReturnsTrue()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            var file2 = new HdlgFile(existingFilePath, null);

            (file1 == file2).Should().BeTrue();
            (file1 != file2).Should().BeFalse();
        }

        [Fact]
        public void EqualityOperators_Nulls_ReturnsExpected()
        {
            var file1 = new HdlgFile(existingFilePath, null);
            HdlgFile? nullFile1 = null;
            HdlgFile? nullFile2 = null;

            (file1 == nullFile1).Should().BeFalse();
            (nullFile1 == file1).Should().BeFalse();
            (nullFile1 == nullFile2).Should().BeTrue();

            (file1 != nullFile1).Should().BeTrue();
            (nullFile1 != file1).Should().BeTrue();
            (nullFile1 != nullFile2).Should().BeFalse();
        }

        [Fact]
        public void ComparisonOperators_DifferentPaths_ReturnsCorrectOrder()
        {
            var pathA = Path.Combine(baseDirectoryPath, "a.txt");
            var pathB = Path.Combine(baseDirectoryPath, "b.txt");

            System.IO.File.WriteAllText(pathA, "a");
            System.IO.File.WriteAllText(pathB, "b");

            var fileA = new HdlgFile(pathA, null);
            var fileB = new HdlgFile(pathB, null);
            var fileA2 = new HdlgFile(pathA, null);

            (fileA < fileB).Should().BeTrue();
            (fileA <= fileB).Should().BeTrue();
            (fileA <= fileA2).Should().BeTrue();

            (fileB > fileA).Should().BeTrue();
            (fileB >= fileA).Should().BeTrue();
            (fileA >= fileA2).Should().BeTrue();
        }
    }
}
