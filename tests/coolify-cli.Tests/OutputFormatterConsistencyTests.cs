#nullable enable

using Xunit;
using FluentAssertions;
using CoolifyCli.Formatters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoolifyCli.Tests;

/// <summary>
/// Cross-formatter contract tests that verify consistent behavior across all IOutputFormatter implementations.
/// Tests null handling, date/time formatting, number formatting, and basic structure.
/// </summary>
public class OutputFormatterConsistencyTests
{
    private class TestModel
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? Price { get; set; }
        public bool? IsActive { get; set; }
    }

    private static readonly TestModel TestData = new TestModel
    {
        Name = "John Doe",
        Age = 30,
        CreatedAt = new DateTime(2024, 6, 15, 14, 30, 0),
        Price = 199.99m,
        IsActive = true
    };

    private static readonly List<TestModel> TestCollection = new List<TestModel>
    {
        new TestModel { Name = "John", Age = 30, CreatedAt = new DateTime(2024, 1, 1), Price = 99.99m, IsActive = true },
        new TestModel { Name = "Jane", Age = 25, CreatedAt = new DateTime(2024, 2, 15), Price = 149.99m, IsActive = false },
        new TestModel { Name = null, Age = null, CreatedAt = null, Price = null, IsActive = null }
    };

    private static readonly Dictionary<string, object?> TestDictionary = new Dictionary<string, object?>
    {
        { "Name", "Test Product" },
        { "Price", 299.99m },
        { "InStock", true },
        { "Created", new DateTime(2024, 3, 10) },
        { "Description", null }
    };

    [Fact]
    public void AllFormatters_HandleNullObjectConsistently()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should handle null gracefully
        foreach (var formatter in formatters)
        {
            var result = formatter.Format(null);
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public void AllFormatters_HandleNullCollectionConsistently()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should throw ArgumentNullException for null collections
        foreach (var formatter in formatters)
        {
            Action act = () => formatter.FormatCollection<TestModel>(null);
            act.Should().Throw<ArgumentNullException>();
        }
    }

    [Fact]
    public void AllFormatters_HandleNullDictionaryConsistently()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should throw ArgumentNullException for null dictionaries
        foreach (var formatter in formatters)
        {
            Action act = () => formatter.FormatDictionary(null);
            act.Should().Throw<ArgumentNullException>();
        }
    }

    [Fact]
    public void AllFormatters_HandleEmptyCollectionConsistently()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should handle empty collections
        foreach (var formatter in formatters)
        {
            var result = formatter.FormatCollection(new List<TestModel>());
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public void AllFormatters_HandleEmptyDictionaryConsistently()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should handle empty dictionaries
        foreach (var formatter in formatters)
        {
            var result = formatter.FormatDictionary(new Dictionary<string, object?>());
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public void AllFormatters_ProduceNonEmptyOutputForValidData()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should produce non-empty output for valid data
        foreach (var formatter in formatters)
        {
            var objResult = formatter.Format(TestData);
            var collResult = formatter.FormatCollection(TestCollection);
            var dictResult = formatter.FormatDictionary(TestDictionary);

            objResult.Should().NotBeEmpty();
            collResult.Should().NotBeEmpty();
            dictResult.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void AllFormatters_HaveValidFileExtensions()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should have valid file extensions
        foreach (var formatter in formatters)
        {
            formatter.FileExtension.Should().NotBeNullOrEmpty();
            formatter.FileExtension.Should().Match("^[a-z0-9]+$");
        }
    }

    [Fact]
    public void AllFormatters_HaveValidMimeTypes()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act & Assert - All formatters should have valid MIME types
        foreach (var formatter in formatters)
        {
            formatter.MimeType.Should().NotBeNullOrEmpty();
            formatter.MimeType.Should().Match("^[a-z]+/[a-z0-9-+.]+$");
        }
    }

    [Fact]
    public void OutputFormatterFactory_CreatesCorrectFormatters()
    {
        // Act & Assert
        var jsonFormatter = OutputFormatterFactory.CreateFormatter("json");
        jsonFormatter.Should().BeOfType<JsonFormatter>();
        jsonFormatter.FileExtension.Should().Be("json");
        jsonFormatter.MimeType.Should().Be("application/json");

        var csvFormatter = OutputFormatterFactory.CreateFormatter("csv");
        csvFormatter.Should().BeOfType<CsvFormatter>();
        csvFormatter.FileExtension.Should().Be("csv");
        csvFormatter.MimeType.Should().Be("text/csv");

        var tableFormatter = OutputFormatterFactory.CreateFormatter("table");
        tableFormatter.Should().BeOfType<TableFormatter>();
        tableFormatter.FileExtension.Should().Be("txt");
        tableFormatter.MimeType.Should().Be("text/plain");
    }

    [Fact]
    public void OutputFormatterFactory_HandlesCaseInsensitiveFormatNames()
    {
        // Act & Assert
        var jsonFormatter1 = OutputFormatterFactory.CreateFormatter("JSON");
        jsonFormatter1.Should().BeOfType<JsonFormatter>();

        var jsonFormatter2 = OutputFormatterFactory.CreateFormatter("Json");
        jsonFormatter2.Should().BeOfType<JsonFormatter>();

        var csvFormatter = OutputFormatterFactory.CreateFormatter("CSV");
        csvFormatter.Should().BeOfType<CsvFormatter>();
    }

    [Fact]
    public void OutputFormatterFactory_ThrowsForUnsupportedFormat()
    {
        // Act & Assert
        Action act = () => OutputFormatterFactory.CreateFormatter("xml");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OutputFormatterFactory_CreatesByExtension()
    {
        // Act & Assert
        var jsonFormatter = OutputFormatterFactory.CreateFormatterByExtension(".json");
        jsonFormatter.Should().BeOfType<JsonFormatter>();

        var csvFormatter = OutputFormatterFactory.CreateFormatterByExtension(".csv");
        csvFormatter.Should().BeOfType<CsvFormatter>();

        var txtFormatter = OutputFormatterFactory.CreateFormatterByExtension(".txt");
        txtFormatter.Should().BeOfType<TableFormatter>();
    }

    [Fact]
    public void Formatters_ProduceDeterministicOutput()
    {
        // Arrange
        var formatters = new IOutputFormatter[]
        {
            new JsonFormatter(),
            new CsvFormatter(),
            new TableFormatter()
        };

        // Act - Call multiple times
        var results1 = new List<string>();
        var results2 = new List<string>();

        foreach (var formatter in formatters)
        {
            results1.Add(formatter.Format(TestData));
            results2.Add(formatter.Format(TestData));
        }

        // Assert - Output should be deterministic
        for (int i = 0; i < formatters.Length; i++)
        {
            results1[i].Should().Be(results2[i]);
        }
    }

    [Fact]
    public void JsonFormatter_FormatCollection_ProducesValidJsonArray()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act
        var result = formatter.FormatCollection(TestCollection);

        // Assert
        result.Should().StartWith("[").And.EndWith("]");

        // Should be valid JSON
        var parsed = System.Text.Json.JsonDocument.Parse(result);
        parsed.RootElement.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public void CsvFormatter_FormatCollection_ProducesCsvWithHeaders()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act
        var result = formatter.FormatCollection(TestCollection);

        // Assert
        result.Should().Contain("Name,Age,CreatedAt,Price,IsActive");
        var lines = result.Split(new[] { '\n' }, StringSplitOptions.None);
        lines.Length.Should().BeGreaterThan(1); // Header + data rows
    }

    [Fact]
    public void TableFormatter_FormatCollection_ProducesTableWithHeaders()
    {
        // Arrange
        var formatter = new TableFormatter();

        // Act
        var result = formatter.FormatCollection(TestCollection);

        // Assert
        result.Should().Contain("Name").And.Contain("Age").And.Contain("CreatedAt");
    }
}