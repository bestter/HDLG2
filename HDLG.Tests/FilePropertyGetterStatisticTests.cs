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
        public async System.Threading.Tasks.Task IncrementFile_ShouldIncreaseTotalFilesConcurrently()
        {
            // Arrange
            var mockGetter = new Mock<IFilePropertyGetter>();
            var statistic = new FilePropertyGetterStatistic(mockGetter.Object);
            int numberOfTasks = 100;
            int incrementsPerTask = 1000;

            // Act
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
            for (int i = 0; i < numberOfTasks; i++)
            {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < incrementsPerTask; j++)
                    {
                        statistic.IncrementFile();
                    }
                }));
            }

            await System.Threading.Tasks.Task.WhenAll(tasks);

            // Assert
            statistic.TotalFiles.Should().Be(numberOfTasks * incrementsPerTask);
        }

        [Fact]
        public void GetTotalExecutionTime_WhenNoExecutionTimeAdded_ShouldReturnZero()
        {
            // Arrange
            var mockGetter = new Mock<IFilePropertyGetter>();
            var statistic = new FilePropertyGetterStatistic(mockGetter.Object);

            // Act
            var totalExecutionTime = statistic.GetTotalExecutionTime();

            // Assert
            totalExecutionTime.Should().Be(TimeSpan.Zero);
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
