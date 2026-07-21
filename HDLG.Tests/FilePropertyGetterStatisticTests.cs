using System;
using FluentAssertions;
using HdlgFileProperty;
using Moq;
using Xunit;

namespace HDLG.Tests
{
    public class FilePropertyGetterStatisticTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenFilePropertyGetterIsNull()
        {
            // Act
            Action act = () => new FilePropertyGetterStatistic(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("filePropertyGetter");
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var mockGetter = new Mock<IFilePropertyGetter>();

            // Act
            var statistic = new FilePropertyGetterStatistic(mockGetter.Object);

            // Assert
            statistic.FilePropertyGetter.Should().BeSameAs(mockGetter.Object);
            statistic.TotalFiles.Should().Be(0);
            statistic.GetTotalExecutionTime().Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void IncrementFile_ShouldIncreaseTotalFiles()
        {
            // Arrange
            var mockGetter = new Mock<IFilePropertyGetter>();
            var statistic = new FilePropertyGetterStatistic(mockGetter.Object);

            // Act
            statistic.IncrementFile();
            statistic.IncrementFile();

            // Assert
            statistic.TotalFiles.Should().Be(2);
        }

        [Fact]
        public void StartAndStopTimer_ShouldRecordExecutionTime()
        {
            // Arrange
            var mockGetter = new Mock<IFilePropertyGetter>();
            var statistic = new FilePropertyGetterStatistic(mockGetter.Object);

            // Act
            var sw = System.Diagnostics.Stopwatch.StartNew();
            System.Threading.Thread.Sleep(10); // Sleep briefly to ensure elapsed time > 0
            sw.Stop();
            statistic.AddExecutionTime(sw.Elapsed);

            // Assert
            statistic.GetTotalExecutionTime().Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
