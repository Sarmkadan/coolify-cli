#nullable enable
using CoolifyCli.Extensions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the StringExtensions class.
/// </summary>
public class StringExtensionsTests
{
    /// <summary>
    /// Tests the ToPascalCase method with a hyphen-delimited input string.
    /// </summary>
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

    /// <summary>
    /// Tests the Truncate method when the input string exceeds the specified maximum length.
    /// </summary>
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

    /// <summary>
    /// Tests the Truncate method when the input string is within the specified maximum length.
    /// </summary>
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

    /// <summary>
    /// Tests the MaskSensitive method with a long API key.
    /// </summary>
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

    /// <summary>
    /// Tests the SplitTrimmed method with a whitespace-padded input string.
    /// </summary>
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
