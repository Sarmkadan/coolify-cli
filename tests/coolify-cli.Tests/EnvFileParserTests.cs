using FluentAssertions;
using Xunit;
using CoolifyCli.Models;
using CoolifyCli.Services;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for <see cref="EnvFileParser"/>, focusing on preserved line numbers.
/// </summary>
public class EnvFileParserTests
{
    /// <summary>
    /// Tests that line numbers count blank and comment lines, so they match the file on disk.
    /// </summary>
    [Fact]
    public void Parse_ShouldKeepPhysicalLineNumbers_AcrossSkippedLines()
    {
        var lines = new[]
        {
            "# header comment",
            "",
            "FOO=1",
            "   # indented comment",
            "BAR=2"
        };

        var entries = EnvFileParser.Parse(lines);

        entries.Should().Equal(
            new EnvVarEntry("FOO", "1", 3),
            new EnvVarEntry("BAR", "2", 5));
    }

    /// <summary>
    /// Tests that keys and values are trimmed and that only the first '=' separates them.
    /// </summary>
    [Fact]
    public void Parse_ShouldTrimWhitespace_AndSplitOnFirstEquals()
    {
        var entries = EnvFileParser.Parse(["  KEY =  a=b=c  "]);

        entries.Should().ContainSingle()
            .Which.Should().Be(new EnvVarEntry("KEY", "a=b=c", 1));
    }

    /// <summary>
    /// Tests that lines without '=' are skipped rather than turned into entries.
    /// </summary>
    [Fact]
    public void Parse_ShouldSkipLinesWithoutSeparator()
    {
        var entries = EnvFileParser.Parse(["JUST_A_WORD", "KEY=value"]);

        entries.Should().ContainSingle()
            .Which.LineNumber.Should().Be(2);
    }

    /// <summary>
    /// Tests that an empty key is kept so the validator can report it with its line number.
    /// </summary>
    [Fact]
    public void Parse_ShouldKeepEmptyKey_SoValidatorCanReportIt()
    {
        var entries = EnvFileParser.Parse(["=orphan"]);

        entries.Should().ContainSingle()
            .Which.Should().Be(new EnvVarEntry(string.Empty, "orphan", 1));
    }

    /// <summary>
    /// Tests that ParseFile reads from disk with the same line-number behaviour.
    /// </summary>
    [Fact]
    public void ParseFile_ShouldReadEntriesFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"envparser-{Guid.NewGuid():N}.env");
        try
        {
            File.WriteAllText(path, "# c\nA=1\n\nB=2\n");

            var entries = EnvFileParser.ParseFile(path);

            entries.Should().Equal(
                new EnvVarEntry("A", "1", 2),
                new EnvVarEntry("B", "2", 4));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
