using System;
using FluentAssertions;
using HDLG_winforms;
using Xunit;

namespace HDLG.Tests
{
    public class PerformanceCountTests
    {
        [Fact]
        public void Empty_ReturnsPerformanceCountWithMinValueTimeSpans()
        {
            // Act
            var empty = PerformanceCount.Empty;

            // Assert
            empty.BrowseTime.Should().Be(TimeSpan.MinValue);
            empty.SaveTime.Should().Be(TimeSpan.MinValue);
            empty.TotalTime.Should().Be(TimeSpan.MinValue);
        }

        [Fact]
        public void FieldAssignments_PersistCorrectly()
        {
            // Arrange
            var browseTime = TimeSpan.FromSeconds(1);
            var saveTime = TimeSpan.FromSeconds(2);
            var totalTime = TimeSpan.FromSeconds(3);

            // Act
            var perfCount = new PerformanceCount
            {
                BrowseTime = browseTime,
                SaveTime = saveTime,
                TotalTime = totalTime
            };

            // Assert
            perfCount.BrowseTime.Should().Be(browseTime);
            perfCount.SaveTime.Should().Be(saveTime);
            perfCount.TotalTime.Should().Be(totalTime);
        }
    }
}
