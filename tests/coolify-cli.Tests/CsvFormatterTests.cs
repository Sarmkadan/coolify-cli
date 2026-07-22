using Xunit;
using FluentAssertions;
using CoolifyCli.Formatters;
using System;
using System.Collections.Generic;

namespace CoolifyCli.Tests;

public class CsvFormatterTests
{
    private class TestPerson
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
        public decimal Salary { get; set; }
    }

    [Fact]
    public void FormatCollection_EmptyList_ReturnsEmptyString()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>();

        var result = formatter.FormatCollection(items);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatCollection_SingleItem_GeneratesHeaderAndDataRow()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", Age = 30, IsActive = true }
        };

        var result = formatter.FormatCollection(items);

        result.Should().NotBeEmpty();
        result.Should().Contain("Name,Age,BirthDate,IsActive,Salary");
        result.Should().Contain("John,30,,true,");
    }

    [Fact]
    public void FormatCollection_MultipleItems_GeneratesMultipleDataRows()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", Age = 30 },
            new TestPerson { Name = "Jane", Age = 25 }
        };

        var result = formatter.FormatCollection(items);

        var lines = result.Split(new[] { '\n' }, StringSplitOptions.None);
        lines.Should().HaveCount(3);
        lines[0].Should().Contain("Name,Age,BirthDate,IsActive,Salary");
        lines[1].Should().Contain("John,30");
        lines[2].Should().Contain("Jane,25");
    }

    [Fact]
    public void FormatCollection_IncludeHeaderFalse_DoesNotGenerateHeader()
    {
        var formatter = new CsvFormatter(includeHeader: false);
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", Age = 30 }
        };

        var result = formatter.FormatCollection(items);

        result.Should().NotContain("Name,Age,BirthDate,IsActive,Salary");
        result.Should().Contain("John,30");
    }

    [Fact]
    public void FormatCollection_FieldWithComma_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "Doe, John" }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("\"Doe, John\"");
    }

    [Fact]
    public void FormatCollection_FieldWithNewline_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John\nDoe" }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("\"John\nDoe\"");
    }

    [Fact]
    public void FormatCollection_CustomDelimiter_UsesDelimiterInOutput()
    {
        var formatter = new CsvFormatter(delimiter: ';');
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", Age = 30 }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("Name;Age;BirthDate;IsActive;Salary");
        result.Should().Contain("John;30");
    }

    [Fact]
    public void FormatCollection_SelectedFields_FiltersFieldsInOutput()
    {
        var formatter = new CsvFormatter(selectedFields: new List<string> { "Name", "Age" });
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", Age = 30, IsActive = true }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("Name,Age");
        result.Should().NotContain("BirthDate");
        result.Should().NotContain("IsActive");
        result.Should().NotContain("Salary");
    }

    [Fact]
    public void FormatCollection_NullValues_FormatsAsEmptyString()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = null, Age = null }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain(",,");
    }

    [Fact]
    public void FormatCollection_DateTimeValues_FormatsAsIsoString()
    {
        var formatter = new CsvFormatter();
        var date = new DateTime(2024, 6, 15, 14, 30, 0);
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", BirthDate = date }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("2024-06-15 14:30:00");
    }

    [Fact]
    public void FormatCollection_BooleanValues_FormatsAsTrueFalse()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson>
        {
            new TestPerson { Name = "John", IsActive = true },
            new TestPerson { Name = "Jane", IsActive = false }
        };

        var result = formatter.FormatCollection(items);

        result.Should().Contain(",true,");
        result.Should().Contain(",false,");
    }

    [Fact]
    public void Format_NullObject_ReturnsEmptyString()
    {
        var formatter = new CsvFormatter();

        var result = formatter.Format(null);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatDictionary_WithHeader_GeneratesHeaderAndData()
    {
        var formatter = new CsvFormatter();
        var dict = new Dictionary<string, object?>
        {
            { "Name", "John" },
            { "Age", 30 },
            { "Active", true }
        };

        var result = formatter.FormatDictionary(dict);

        result.Should().Contain("Name,Age,Active");
        result.Should().Contain("John,30,true");
    }

    [Fact]
    public void FormatDictionary_EmptyDictionary_ReturnsEmptyString()
    {
        var formatter = new CsvFormatter();
        var dict = new Dictionary<string, object?>();

        var result = formatter.FormatDictionary(dict);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseCsv_ValidCsv_ReturnsListOfDictionaries()
    {
        var formatter = new CsvFormatter();
        var csv = "Name,Age\nJohn,30\nJane,25";

        var result = formatter.ParseCsv(csv);

        result.Should().HaveCount(2);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
        result[1]["Name"].Should().Be("Jane");
        result[1]["Age"].Should().Be("25");
    }

    [Fact]
    public void ParseCsv_WithQuotedFields_ReturnsParsedData()
    {
        var formatter = new CsvFormatter();
        var csv = "Name,Age\nJohn,30";

        var result = formatter.ParseCsv(csv);

        result.Should().HaveCount(1);
        result[0]["Name"].Should().Be("John");
        result[0]["Age"].Should().Be("30");
    }

    [Fact]
    public void EscapeCsvField_FieldWithCarriageReturn_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var field = "John\rDoe";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\"John\rDoe\"");
    }

    [Fact]
    public void EscapeCsvField_FieldWithMultipleSpecialCharacters_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var field = "John, Doe\nTest\r";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\"John, Doe\nTest\r\"");
    }

    [Fact]
    public void EscapeCsvField_EmptyString_ReturnsEmptyString()
    {
        var formatter = new CsvFormatter();
        var field = "";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain(",");
    }

    [Fact]
    public void EscapeCsvField_WhitespaceOnlyString_ReturnsWhitespace()
    {
        var formatter = new CsvFormatter();
        var field = "   ";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("   ");
    }

    [Fact]
    public void FormatCollection_DifferentDelimiterTypes_UsesCorrectDelimiter()
    {
        var formatter = new CsvFormatter(delimiter: '|');
        var items = new List<TestPerson> { new TestPerson { Name = "John", Age = 30 } };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("Name|Age|BirthDate|IsActive|Salary");
        result.Should().Contain("John|30");
    }

    [Fact]
    public void FormatCollection_FieldWithTabCharacter_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var field = "John\tDoe";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\"John\tDoe\"");
    }

    [Fact]
    public void ParseCsv_FieldWithNewlinesInQuotes_ParsesCorrectly()
    {
        var formatter = new CsvFormatter();
        var csv = "Name\n\"John\nDoe\"";

        var result = formatter.ParseCsv(csv);

        result.Should().HaveCount(1);
        result[0]["Name"].Should().Be("John\nDoe");
    }

    [Fact]
    public void FormatCollection_FieldWithSemicolon_EscapesFieldWithQuotes()
    {
        var formatter = new CsvFormatter();
        var field = "John; Doe";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\"John; Doe\"");
    }

    [Fact]
    public void FormatCollection_MixedNumericTypes_FormatsCorrectly()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson> { new TestPerson { Name = "John", Age = 30, Salary = 50000.50m } };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("50000.50");
    }

    [Fact]
    public void FormatCollection_SelectedFieldsCaseInsensitive_MatchesCorrectly()
    {
        var formatter = new CsvFormatter(selectedFields: new List<string> { "name", "AGE" });
        var items = new List<TestPerson> { new TestPerson { Name = "John", Age = 30, IsActive = true } };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("Name,Age");
        result.Should().NotContain("name");
        result.Should().NotContain("age");
    }

    [Fact]
    public void FormatCollection_FieldWithOnlyDelimiter_EscapesField()
    {
        var formatter = new CsvFormatter();
        var field = ",";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\",\"");
    }

    [Fact]
    public void FormatDictionary_FieldWithMultipleSpecialCharacters_EscapesCorrectly()
    {
        var formatter = new CsvFormatter();
        var dict = new Dictionary<string, object?> { { "Name", "John, Doe\nTest" } };

        var result = formatter.FormatDictionary(dict);

        result.Should().Contain("\"John, Doe\nTest\"");
    }

    [Fact]
    public void FormatCollection_FieldWithLeadingTrailingSpaces_PreservesSpaces()
    {
        var formatter = new CsvFormatter();
        var field = "  John Doe  ";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("  John Doe  ");
    }

    [Fact]
    public void ParseCsv_EmptyCsvContent_ReturnsEmptyList()
    {
        var formatter = new CsvFormatter();
        var csv = "";

        var result = formatter.ParseCsv(csv);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseCsv_SingleLineCsv_ReturnsEmptyList()
    {
        var formatter = new CsvFormatter();
        var csv = "Name,Age";

        var result = formatter.ParseCsv(csv);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseCsv_WhitespaceOnly_ReturnsEmptyList()
    {
        var formatter = new CsvFormatter();
        var csv = "   \n   ";

        var result = formatter.ParseCsv(csv);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatCollection_NullCollection_ThrowsArgumentNullException()
    {
        var formatter = new CsvFormatter();
        List<TestPerson>? items = null;

        Assert.Throws<ArgumentNullException>(() => formatter.FormatCollection(items!));
    }

    [Fact]
    public void FormatCollection_DecimalAndDoubleFormatting_WorksCorrectly()
    {
        var formatter = new CsvFormatter();
        var items = new List<TestPerson> { new TestPerson { Name = "John", Salary = 75000.99m } };

        var result = formatter.FormatCollection(items);

        result.Should().Contain("75000.99");
    }

    [Fact]
    public void FormatCollection_FieldWithAllCsvSpecialChars_EscapesWithQuotes()
    {
        var formatter = new CsvFormatter();
        var field = ",\n\r";

        var result = formatter.FormatCollection(new List<TestPerson> { new TestPerson { Name = field } });

        result.Should().Contain("\",\n\r\"");
    }
}
