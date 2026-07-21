#nullable enable

namespace CoolifyCli.Tests;

using CoolifyCli.Formatters;
using CoolifyCli.Infrastructure;
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

public class TemplateDiffFormatterTests
{
    private static readonly IacTemplateOptions TextOptions = new() { OutputFormat = "text" };
    private static readonly IacTemplateOptions JsonOptions = new() { OutputFormat = "json" };

    [Fact]
    public void FormatDiff_WithIdenticalTemplates_ProducesNoDiffLines()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [],
            Modified = [],
            Removed = [],
            Unchanged = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "web-app", ChangeDescription = null },
                new TemplateDiffEntry { ResourceType = "Database", Name = "postgres-db", ChangeDescription = null }
            ]
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("IN SYNC");
        result.Should().NotContain("ADDED");
        result.Should().NotContain("MODIFIED");
        result.Should().NotContain("ORPHAN");
        result.Should().Contain("=2 unchanged");
    }

    [Fact]
    public void FormatDiff_WithAddedResources_MarksWithPlusSign()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "new-app", ChangeDescription = "New application" },
                new TemplateDiffEntry { ResourceType = "Database", Name = "new-db", ChangeDescription = null }
            ],
            Modified = [],
            Removed = [],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().Contain("+ ADDED");
        result.Should().Contain("+ Application       new-app");
        result.Should().Contain("+ Database          new-db");
        result.Should().Contain("(New application)");
    }

    [Fact]
    public void FormatDiff_WithRemovedResources_MarksWithExclamationSign()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [],
            Modified = [],
            Removed = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "old-app", ChangeDescription = null },
                new TemplateDiffEntry { ResourceType = "Database", Name = "old-db", ChangeDescription = "Deprecated database" }
            ],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().Contain("! ORPHAN");
        result.Should().Contain("! Application       old-app");
        result.Should().Contain("! Database          old-db");
        result.Should().Contain("(Deprecated database)");
    }

    [Fact]
    public void FormatDiff_WithModifiedResources_MarksWithTildeSign()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [],
            Modified = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "web-app", ChangeDescription = "Updated port configuration" },
                new TemplateDiffEntry { ResourceType = "Database", Name = "postgres-db", ChangeDescription = "Changed version from 14 to 15" }
            ],
            Removed = [],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().Contain("~ MODIFIED");
        result.Should().Contain("~ Application       web-app");
        result.Should().Contain("~ Database          postgres-db");
        result.Should().Contain("(Updated port configuration)");
        result.Should().Contain("(Changed version from 14 to 15)");
    }

    [Fact]
    public void FormatDiff_WithMixedChanges_ShowsAllCategories()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "new-service", ChangeDescription = null }
            ],
            Modified = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "web-app", ChangeDescription = "Updated config" }
            ],
            Removed = [
                new TemplateDiffEntry { ResourceType = "Database", Name = "old-db", ChangeDescription = null }
            ],
            Unchanged = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "api-app", ChangeDescription = null }
            ]
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().Contain("+1 added ~1 modified !1 orphaned =1 unchanged");
        result.Should().Contain("+ ADDED");
        result.Should().Contain("~ MODIFIED");
        result.Should().Contain("! ORPHAN");
        result.Should().Contain("= IN SYNC ");
    }

    [Fact]
    public void FormatDiff_WithTextFormat_ReturnsHumanReadableOutput()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [new TemplateDiffEntry { ResourceType = "Application", Name = "app1", ChangeDescription = null }],
            Modified = [],
            Removed = [],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().StartWith("\n Infrastructure Diff\n");
        result.Should().Contain("─"); // Ruler line
        result.Should().Contain("ADDED");
        result.Should().Contain("added 1");
    }

    [Fact]
    public void FormatDiff_WithJsonFormat_ReturnsStructuredOutput()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "app1", ChangeDescription = "New app" },
                new TemplateDiffEntry { ResourceType = "Database", Name = "db1", ChangeDescription = null }
            ],
            Modified = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "app2", ChangeDescription = "Updated" }
            ],
            Removed = [
                new TemplateDiffEntry { ResourceType = "Database", Name = "db2", ChangeDescription = null }
            ],
            Unchanged = [
                new TemplateDiffEntry { ResourceType = "Application", Name = "app3", ChangeDescription = null }
            ]
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, JsonOptions);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().StartWith("{");
        result.Should().Contain("\"added\":");
        result.Should().Contain("\"modified\":");
        result.Should().Contain("\"orphaned\":");
        result.Should().Contain("\"unchanged\":");
        result.Should().Contain("\"hasChanges\":true");

        // Verify it's valid JSON
        var jsonDocument = System.Text.Json.JsonDocument.Parse(result);
        jsonDocument.RootElement.TryGetProperty("added", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("modified", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("orphaned", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("unchanged", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("summary", out _).Should().BeTrue();
    }

    [Fact]
    public void FormatDiff_WithEmptyLists_ProducesValidOutput()
    {
        // Arrange
        var diff = new TemplateDiffResult
        {
            Added = [],
            Modified = [],
            Removed = [],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("0 added 0 modified 0 orphaned 0 unchanged");
        result.Should().Contain("hasChanges: false");
    }

    [Fact]
    public void FormatDiff_OrderingIsStable()
    {
        // Arrange - Add resources in specific order
        var diff = new TemplateDiffResult
        {
            Added = [
                new TemplateDiffEntry { ResourceType = "Database", Name = "db-zebra", ChangeDescription = null },
                new TemplateDiffEntry { ResourceType = "Application", Name = "app-alpha", ChangeDescription = null },
                new TemplateDiffEntry { ResourceType = "Database", Name = "db-alpha", ChangeDescription = null },
                new TemplateDiffEntry { ResourceType = "Application", Name = "app-zebra", ChangeDescription = null }
            ],
            Modified = [],
            Removed = [],
            Unchanged = []
        };

        // Act
        var result = TemplateDiffFormatter.FormatDiff(diff, TextOptions);

        // Assert - Order should be preserved as-is
        var lines = result.Split('\n');
        var entryLines = lines.Where(l => l.Trim().StartsWith("+ Application") || l.Trim().StartsWith("+ Database")).ToList();
        entryLines.Should().HaveCount(4);

        // Verify order is preserved
        entryLines[0].Should().Contain("app-alpha");
        entryLines[1].Should().Contain("app-zebra");
        entryLines[2].Should().Contain("db-alpha");
        entryLines[3].Should().Contain("db-zebra");
    }

    [Fact]
    public void FormatApplyResult_WithSuccess_RendersSuccessStatus()
    {
        // Arrange
        var result = new TemplateApplyResult
        {
            Operations = [
                new TemplateApplyOperation { ResourceType = "Application", ResourceName = "app1", Action = "Create", Succeeded = true },
                new TemplateApplyOperation { ResourceType = "Database", ResourceName = "db1", Action = "Create", Succeeded = true }
            ],
            Duration = TimeSpan.FromSeconds(10.5)
        };

        // Act
        var output = TemplateDiffFormatter.FormatApplyResult(result, TextOptions);

        // Assert
        output.Should().Contain("✓");
        output.Should().Contain("Apply Result ✓ 10.5s");
        output.Should().Contain("2 succeeded 0 failed");
    }

    [Fact]
    public void FormatApplyResult_WithFailure_RendersFailureStatus()
    {
        // Arrange
        var result = new TemplateApplyResult
        {
            Operations = [
                new TemplateApplyOperation { ResourceType = "Application", ResourceName = "app1", Action = "Create", Succeeded = true },
                new TemplateApplyOperation { ResourceType = "Database", ResourceName = "db1", Action = "Create", Succeeded = false, Message = "Connection timeout" }
            ],
            Duration = TimeSpan.FromSeconds(5.2)
        };

        // Act
        var output = TemplateDiffFormatter.FormatApplyResult(result, TextOptions);

        // Assert
        output.Should().Contain("✗");
        output.Should().Contain("Apply Result ✗ 5.2s");
        output.Should().Contain("1 succeeded 1 failed");
        output.Should().Contain("Connection timeout");
    }

    [Fact]
    public void FormatApplyResult_WithJsonFormat_ReturnsStructuredOutput()
    {
        // Arrange
        var result = new TemplateApplyResult
        {
            Operations = [
                new TemplateApplyOperation { ResourceType = "Application", ResourceName = "app1", Action = "Create", Succeeded = true },
                new TemplateApplyOperation { ResourceType = "Database", ResourceName = "db1", Action = "Create", Succeeded = false }
            ],
            Duration = TimeSpan.FromSeconds(7.333)
        };

        // Act
        var output = TemplateDiffFormatter.FormatApplyResult(result, JsonOptions);

        // Assert
        output.Should().NotBeNullOrWhiteSpace();
        output.Should().StartWith("{");
        output.Should().Contain("\"success\":false");
        output.Should().Contain("\"durationSeconds\":7.333");
        output.Should().Contain("\"succeededCount\":1");
        output.Should().Contain("\"failedCount\":1");

        // Verify it's valid JSON
        var jsonDocument = System.Text.Json.JsonDocument.Parse(output);
        jsonDocument.RootElement.TryGetProperty("success", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("durationSeconds", out _).Should().BeTrue();
    }

    [Fact]
    public void FormatValidationResult_WithValidTemplate_RendersSuccess()
    {
        // Arrange
        var result = new TemplateValidationResult
        {
            Errors = [],
            Warnings = [],
            TemplateName = "test-template"
        };

        // Act
        var output = TemplateDiffFormatter.FormatValidationResult(result, TextOptions);

        // Assert
        output.Should().Contain("✓ PASSED");
        output.Should().Contain("test-template");
        output.Should().Contain("0 error(s) 0 warning(s)");
        output.Should().Contain("Template is valid with no warnings.");
    }

    [Fact]
    public void FormatValidationResult_WithErrorsAndWarnings_RendersBoth()
    {
        // Arrange
        var result = new TemplateValidationResult
        {
            Errors = ["Missing required field 'name'", "Invalid repository URL"],
            Warnings = ["No health check configured", "Using default branch 'main'"],
            TemplateName = "invalid-template"
        };

        // Act
        var output = TemplateDiffFormatter.FormatValidationResult(result, TextOptions);

        // Assert
        output.Should().Contain("✗ FAILED");
        output.Should().Contain("invalid-template");
        output.Should().Contain("✗ Missing required field 'name'");
        output.Should().Contain("✗ Invalid repository URL");
        output.Should().Contain("! No health check configured");
        output.Should().Contain("! Using default branch 'main'");
        output.Should().Contain("2 error(s) 2 warning(s)");
    }

    [Fact]
    public void FormatValidationResult_WithJsonFormat_ReturnsStructuredOutput()
    {
        // Arrange
        var result = new TemplateValidationResult
        {
            Errors = ["Error 1", "Error 2"],
            Warnings = ["Warning 1"],
            TemplateName = "validation-test"
        };

        // Act
        var output = TemplateDiffFormatter.FormatValidationResult(result, JsonOptions);

        // Assert
        output.Should().NotBeNullOrWhiteSpace();
        output.Should().StartWith("{");
        output.Should().Contain("\"isValid\":false");
        output.Should().Contain("\"templateName\":\"validation-test\"");
        output.Should().Contain("\"errors\":[");
        output.Should().Contain("\"warnings\":[");

        // Verify it's valid JSON
        var jsonDocument = System.Text.Json.JsonDocument.Parse(output);
        jsonDocument.RootElement.TryGetProperty("isValid", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("templateName", out _).Should().BeTrue();
    }
}
