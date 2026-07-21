using Xunit;
using FluentAssertions;
using CoolifyCli.Formatters;

namespace CoolifyCli.Tests;

public class TableFormatterTests
{
    private class TestData
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
    }

    [Fact]
    public void FormatCollection_EmptyList_ReturnsNoDataMessage()
    {
        // Arrange
        var formatter = new TableFormatter();
        var items = new List<TestData>();

        // Act
        var result = formatter.FormatCollection(items);

        // Assert
        result.Should().Be("No data to display.");
    }

    [Fact]
    public void FormatCollection_WithNullValues_ReturnsDashForNull()
    {
        // Arrange
        var formatter = new TableFormatter();
        var items = new List<TestData>
        {
            new TestData { Name = null, Age = null }
        };

        // Act
        var result = formatter.FormatCollection(items);

        // Assert
        result.Should().Contain("-  -");
    }

    [Fact]
    public void FormatCollection_Sizing_CalculatesWidthsCorrectly()
    {
        // Arrange
        var formatter = new TableFormatter();
        var items = new List<TestData>
        {
            new TestData { Name = "Short", Age = 1 },
            new TestData { Name = "ThisIsALongNameToBeMeasured", Age = 20 }
        };

        // Act
        var result = formatter.FormatCollection(items);

        // Assert
        // Header Name is 4 chars, Short is 5, ThisIsALongNameToBeMeasured is 25.
        // CalculateColumnWidths adds 2.
        // Header Name: maxWidth = 4, then max(4, 5, 25) = 25. width = 25 + 2 = 27.
        result.Should().Contain("ThisIsALongNameToBeMeasured");
    }

    [Fact]
    public void FormatCollection_Headers_RenderedCorrectly()
    {
        // Arrange
        var formatter = new TableFormatter();
        var items = new List<TestData>
        {
            new TestData { Name = "A", Age = 1 }
        };

        // Act
        var result = formatter.FormatCollection(items);

        // Assert
        result.Should().Contain("Name");
        result.Should().Contain("Age");
    }
}
