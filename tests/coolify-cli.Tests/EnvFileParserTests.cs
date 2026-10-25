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

    /// <summary>
    /// Tests that single- and double-quoted values lose their quotes and keep inner spaces and '#'.
    /// </summary>
    [Fact]
    public void Parse_ShouldUnquoteValues_KeepingInnerContent()
    {
        var entries = EnvFileParser.Parse(["A=\"hello world\"", "B='x # y'", "C=\"\""]);

        entries.Should().Equal(
            new EnvVarEntry("A", "hello world", 1),
            new EnvVarEntry("B", "x # y", 2),
            new EnvVarEntry("C", string.Empty, 3));
    }

    /// <summary>
    /// Tests that an inline comment after a quoted value is ignored.
    /// </summary>
    [Fact]
    public void Parse_ShouldIgnoreTrailingText_AfterClosingQuote()
    {
        var entries = EnvFileParser.Parse(["A=\"one\" # note"]);

        entries.Should().ContainSingle()
            .Which.Value.Should().Be("one");
    }

    /// <summary>
    /// Tests that an unquoted value stops at a '#' preceded by whitespace, but keeps a '#' inside a word.
    /// </summary>
    [Fact]
    public void Parse_ShouldStripInlineComment_FromUnquotedValue()
    {
        var entries = EnvFileParser.Parse(["A=value # comment", "B=abc#def"]);

        entries.Should().Equal(
            new EnvVarEntry("A", "value", 1),
            new EnvVarEntry("B", "abc#def", 2));
    }

    /// <summary>
    /// Tests that an 'export' prefix is dropped from the key.
    /// </summary>
    [Fact]
    public void Parse_ShouldDropExportPrefix_FromKey()
    {
        var entries = EnvFileParser.Parse(["export API_URL=https://example.test"]);

        entries.Should().ContainSingle()
            .Which.Should().Be(new EnvVarEntry("API_URL", "https://example.test", 1));
    }
}
