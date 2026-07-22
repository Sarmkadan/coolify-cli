using Xunit;
using FluentAssertions;
using CoolifyCli.Formatters;
using CoolifyCli.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CoolifyCli.Tests;

public class JsonFormatterTests
{
    private class TestModel
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
        public decimal? Salary { get; set; }
    }

    private class NestedModel
    {
        public string? Title { get; set; }
        public TestModel? Details { get; set; }
        public List<string>? Tags { get; set; }
    }

    private class ComplexModel
    {
        public string? Id { get; set; }
        public ApiResponse<TestModel>? Response { get; set; }
        public Dictionary<string, object?>? Metadata { get; set; }
    }

    [Fact]
    public void Format_NullObject_ReturnsNullString()
    {
        var formatter = new JsonFormatter();
        var result = formatter.Format(null);
        result.Should().Be("null");
    }

    [Fact]
    public void Format_SimpleObject_GeneratesValidJson()
    {
        var formatter = new JsonFormatter();
        var model = new TestModel { Name = "John", Age = 30, IsActive = true };
        var result = formatter.Format(model);

        result.Should().NotBeEmpty().And.StartWith("{").And.EndWith("}");

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Name").GetString().Should().Be("John");
        parsed.RootElement.GetProperty("Age").GetInt32().Should().Be(30);
        parsed.RootElement.GetProperty("IsActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Format_ObjectWithNullProperties_ExcludesNullFields()
    {
        var formatter = new JsonFormatter();
        var model = new TestModel { Name = "Jane", Age = null, IsActive = false };
        var result = formatter.Format(model);

        result.Should().NotContain("Age").And.Contain("Jane").And.Contain("IsActive");

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Name").GetString().Should().Be("Jane");
        parsed.RootElement.GetProperty("IsActive").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public void Format_NestedObject_GeneratesValidNestedJson()
    {
        var formatter = new JsonFormatter();
        var nested = new NestedModel
        {
            Title = "Test",
            Details = new TestModel { Name = "Nested", Age = 25 },
            Tags = new List<string> { "tag1", "tag2" }
        };
        var result = formatter.Format(nested);

        result.Should().NotBeEmpty();

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Title").GetString().Should().Be("Test");
        parsed.RootElement.GetProperty("Details").GetProperty("Name").GetString().Should().Be("Nested");
        parsed.RootElement.GetProperty("Details").GetProperty("Age").GetInt32().Should().Be(25);
        parsed.RootElement.GetProperty("Tags").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void Format_CollectionOfObjects_GeneratesValidJsonArray()
    {
        var formatter = new JsonFormatter();
        var items = new List<TestModel>
        {
            new TestModel { Name = "John", Age = 30 },
            new TestModel { Name = "Jane", Age = 25 }
        };
        var result = formatter.FormatCollection(items);

        result.Should().NotBeEmpty().And.StartWith("[").And.EndWith("]");

        var parsed = JsonDocument.Parse(result);
        var array = parsed.RootElement;
        array.GetArrayLength().Should().Be(2);
        array[0].GetProperty("Name").GetString().Should().Be("John");
        array[1].GetProperty("Name").GetString().Should().Be("Jane");
    }

    [Fact]
    public void Format_EmptyCollection_ReturnsEmptyArray()
    {
        var formatter = new JsonFormatter();
        var items = new List<TestModel>();
        var result = formatter.FormatCollection(items);
        result.Should().Be("[]");
    }

    [Fact]
    public void Format_Dictionary_GeneratesValidJsonObject()
    {
        var formatter = new JsonFormatter();
        var dict = new Dictionary<string, object?>
        {
            { "name", "Test" },
            { "count", 42 },
            { "active", true }
        };
        var result = formatter.FormatDictionary(dict);

        result.Should().NotBeEmpty();

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("name").GetString().Should().Be("Test");
        parsed.RootElement.GetProperty("count").GetInt32().Should().Be(42);
        parsed.RootElement.GetProperty("active").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Format_EmptyDictionary_ReturnsEmptyObject()
    {
        var formatter = new JsonFormatter();
        var dict = new Dictionary<string, object?>();
        var result = formatter.FormatDictionary(dict);
        result.Should().Be("{}");
    }

    [Fact]
    public void Format_PrettyPrintEnabled_IndentsJson()
    {
        var formatter = new JsonFormatter(prettyPrint: true);
        var model = new TestModel { Name = "John", Age = 30 };
        var result = formatter.Format(model);

        result.Should().NotBeEmpty().And.Contain("\n").And.Contain("  ");
    }

    [Fact]
    public void Format_PrettyPrintDisabled_CompactJson()
    {
        var formatter = new JsonFormatter(prettyPrint: false);
        var model = new TestModel { Name = "John", Age = 30 };
        var result = formatter.Format(model);

        result.Should().NotContain("\n").And.NotContain("  ");
    }

    [Fact]
    public void ReformatJson_ValidJsonString_ReformatsWithFormatterOptions()
    {
        var formatter = new JsonFormatter();
        var jsonString = "{\"name\":\"John\",\"age\":30}";
        var result = formatter.ReformatJson(jsonString);

        result.Should().NotBeEmpty();
        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("name").GetString().Should().Be("John");
        parsed.RootElement.GetProperty("age").GetInt32().Should().Be(30);
    }

    [Fact]
    public void ReformatJson_InvalidJson_ThrowsInvalidOperationException()
    {
        var formatter = new JsonFormatter();
        var invalidJson = "{invalid}";
        Action act = () => formatter.ReformatJson(invalidJson);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Minify_ValidJsonString_RemovesWhitespace()
    {
        var formatter = new JsonFormatter();
        var jsonString = "{ \"name\": \"John\", \"age\": 30 }";
        var result = formatter.Minify(jsonString);

        result.Should().Be("{\"name\":\"John\",\"age\":30}");
        result.Should().NotContain("\n").And.NotContain(" ");
    }

    [Fact]
    public void Minify_InvalidJson_ThrowsInvalidOperationException()
    {
        var formatter = new JsonFormatter();
        var invalidJson = "{invalid}";
        Action act = () => formatter.Minify(invalidJson);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Prettify_CompactJsonString_AddsIndentation()
    {
        var formatter = new JsonFormatter();
        var jsonString = "{\"name\":\"John\",\"age\":30}";
        var result = formatter.Prettify(jsonString);

        result.Should().Contain("\n").And.Contain("  ").And.Contain("name").And.Contain("John");
    }

    [Fact]
    public void Prettify_InvalidJson_ThrowsInvalidOperationException()
    {
        var formatter = new JsonFormatter();
        var invalidJson = "{invalid}";
        Action act = () => formatter.Prettify(invalidJson);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ExtractField_ValidJsonPath_ReturnsFieldValue()
    {
        var formatter = new JsonFormatter();
        var jsonString = "{\"user\":{\"name\":\"John\",\"age\":30}}";
        var result = formatter.ExtractField(jsonString, "user.name");

        result.Should().NotBeNull();
        result.Should().Be("\"John\"");
    }

    [Fact]
    public void ExtractField_InvalidPath_ReturnsNull()
    {
        var formatter = new JsonFormatter();
        var jsonString = "{\"user\":{\"name\":\"John\"}}";
        var result = formatter.ExtractField(jsonString, "user.email");
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractField_InvalidJsonString_ReturnsNull()
    {
        var formatter = new JsonFormatter();
        var invalidJson = "{invalid}";
        var result = formatter.ExtractField(invalidJson, "user.name");
        result.Should().BeNull();
    }

    [Fact]
    public void Format_WithIncludeFields_FiltersToOnlyIncludedFields()
    {
        var formatter = new JsonFormatter(includeFields: new List<string> { "Name", "Age" });
        var model = new TestModel { Name = "John", Age = 30, IsActive = true };
        var result = formatter.Format(model);

        result.Should().NotContain("IsActive").And.Contain("John").And.Contain("30");

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Name").GetString().Should().Be("John");
        parsed.RootElement.GetProperty("Age").GetInt32().Should().Be(30);
        parsed.RootElement.TryGetProperty("IsActive", out _).Should().BeFalse();
    }

    [Fact]
    public void Format_WithExcludeFields_FiltersOutExcludedFields()
    {
        var formatter = new JsonFormatter(excludeFields: new List<string> { "Age", "Salary" });
        var model = new TestModel { Name = "Jane", Age = 25, IsActive = true };
        var result = formatter.Format(model);

        result.Should().NotContain("Age").And.Contain("Jane").And.Contain("IsActive");

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Name").GetString().Should().Be("Jane");
        parsed.RootElement.TryGetProperty("Age", out _).Should().BeFalse();
        parsed.RootElement.GetProperty("IsActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Format_WithIncludeAndExcludeFields_IncludeTakesPrecedence()
    {
        var formatter = new JsonFormatter(
            includeFields: new List<string> { "Name", "Age" },
            excludeFields: new List<string> { "Age" }
        );
        var model = new TestModel { Name = "John", Age = 30, IsActive = true };
        var result = formatter.Format(model);

        result.Should().Contain("Name").And.NotContain("Age").And.NotContain("IsActive");
    }

    [Fact]
    public void Format_ComplexNestedStructure_HandlesDeepNesting()
    {
        var formatter = new JsonFormatter();
        var complex = new ComplexModel
        {
            Id = "123",
            Response = ApiResponse<TestModel>.SuccessResponse(
                new TestModel { Name = "Test", Age = 40 },
                "Success"
            ),
            Metadata = new Dictionary<string, object?>
            {
                { "created", DateTime.UtcNow },
                { "version", "1.0" }
            }
        };
        var result = formatter.Format(complex);

        result.Should().NotBeEmpty();

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Id").GetString().Should().Be("123");
        parsed.RootElement.GetProperty("Response").GetProperty("Success").GetBoolean().Should().BeTrue();
        parsed.RootElement.GetProperty("Response").GetProperty("Data").GetProperty("Name").GetString().Should().Be("Test");
    }

    [Fact]
    public void FormatCollection_WithNullCollection_ReturnsEmptyArray()
    {
        var formatter = new JsonFormatter();
        List<TestModel>? items = null;
        var result = formatter.FormatCollection(items);
        result.Should().Be("[]");
    }

    [Fact]
    public void FormatDictionary_WithNullDictionary_ReturnsEmptyObject()
    {
        var formatter = new JsonFormatter();
        Dictionary<string, object?>? dict = null;
        var result = formatter.FormatDictionary(dict);
        result.Should().Be("{}");
    }

    [Fact]
    public void Format_RealWorldApiResponse_HandlesApiResponseModel()
    {
        var formatter = new JsonFormatter();
        var response = ApiResponse<TestModel>.SuccessResponse(
            new TestModel { Name = "John", Age = 30 },
            "User retrieved successfully"
        );
        var result = formatter.Format(response);

        result.Should().NotBeEmpty();

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Success").GetBoolean().Should().BeTrue();
        parsed.RootElement.GetProperty("Data").GetProperty("Name").GetString().Should().Be("John");
        parsed.RootElement.GetProperty("Message").GetString().Should().Be("User retrieved successfully");
        parsed.RootElement.GetProperty("StatusCode").GetInt32().Should().Be(200);
    }

    [Fact]
    public void Format_RealWorldApiPaginatedResponse_HandlesPaginatedResponse()
    {
        var formatter = new JsonFormatter();
        var response = new ApiPaginatedResponse<TestModel>
        {
            Data = new List<TestModel>
            {
                new TestModel { Name = "John", Age = 30 },
                new TestModel { Name = "Jane", Age = 25 }
            },
            PageNumber = 1,
            PageSize = 20,
            TotalRecords = 2,
            TotalPages = 1
        };
        var result = formatter.Format(response);

        result.Should().NotBeEmpty();

        var parsed = JsonDocument.Parse(result);
        parsed.RootElement.GetProperty("Success").GetBoolean().Should().BeTrue();
        parsed.RootElement.GetProperty("Data").GetArrayLength().Should().Be(2);
        parsed.RootElement.GetProperty("PageNumber").GetInt32().Should().Be(1);
        parsed.RootElement.GetProperty("TotalRecords").GetInt64().Should().Be(2);
    }
}
