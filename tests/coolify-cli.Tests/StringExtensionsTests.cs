#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifiCli.Extensions;
using FluentAssertions;
using Xunit;

namespace CoolifiCli.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToPascalCase_WithHyphenDelimitedWords_ReturnsConcatenatedPascalWords()
    {
        // Arrange
        var input = "deploy-my-app";

        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be("DeployMyApp");
    }

    [Fact]
    public void Truncate_WhenInputExceedsMaxLength_TruncatesAndAddsEllipsis()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = input.Truncate(5);

        // Assert
        result.Should().Be("Hello...");
    }

    [Fact]
    public void Truncate_WhenInputWithinMaxLength_ReturnsOriginalString()
    {
        // Arrange
        var input = "Hi";

        // Act
        var result = input.Truncate(10);

        // Assert
        result.Should().Be("Hi");
    }

    [Fact]
    public void MaskSensitive_WithLongApiKey_ExposesOnlyEdgeCharacters()
    {
        // Arrange
        var apiKey = "abcd1234efgh5678";

        // Act
        var result = apiKey.MaskSensitive(showChars: 4);

        // Assert
        result.Should().Be("abcd********5678");
    }

    [Fact]
    public void SplitTrimmed_WithWhitespacePaddedParts_ReturnsCleanSegments()
    {
        // Arrange
        var input = " api , web , worker ";

        // Act
        var result = input.SplitTrimmed(',');

        // Assert
        result.Should().Equal("api", "web", "worker");
    }
}
