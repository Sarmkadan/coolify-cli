#nullable enable
using CoolifyCli.Models;

namespace CoolifyCli.Services;

/// <summary>
/// Parses <c>.env</c>-style files (<c>KEY=VALUE</c> lines) into entries that remember their line numbers,
/// so validation errors can point at the exact line that caused them.
/// </summary>
public static class EnvFileParser
{
    /// <summary>
    /// Reads and parses an environment file from disk.
    /// </summary>
    /// <param name="path">Path to the file. The file must exist.</param>
    /// <returns>Entries in file order. Blank lines, comments and lines without <c>=</c> are skipped.</returns>
    public static IReadOnlyList<EnvVarEntry> ParseFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Parse(File.ReadAllLines(path));
    }

    /// <summary>
    /// Parses already-read <c>.env</c> lines.
    /// </summary>
    /// <remarks>
    /// A leading <c>export </c> on the key is dropped. A value wrapped in matching single or double
    /// quotes is taken literally (no escape processing). An unquoted value ends at a <c>#</c> that is
    /// preceded by whitespace.
    /// </remarks>
    /// <param name="lines">The lines to parse, in file order.</param>
    /// <returns>Entries in file order, each tagged with its 1-based line number.</returns>
    public static IReadOnlyList<EnvVarEntry> Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var entries = new List<EnvVarEntry>();
        var lineNumber = 0;

        foreach (var rawLine in lines)
        {
            lineNumber++;

            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = line.IndexOf('=');
            if (separator < 0)
                continue;

            var key = line[..separator].Trim();
            if (key.StartsWith("export ", StringComparison.Ordinal))
                key = key["export ".Length..].Trim();

            entries.Add(new EnvVarEntry(
                key,
                ParseValue(line[(separator + 1)..].Trim()),
                lineNumber));
        }

        return entries;
    }

    /// <summary>
    /// Extracts the value from the raw text after the first <c>=</c>, removing quotes or an inline comment.
    /// </summary>
    /// <param name="raw">The trimmed raw value text.</param>
    /// <returns>The unquoted value. An unterminated quote is kept as written.</returns>
    private static string ParseValue(string raw)
    {
        if (raw.Length >= 2 && raw[0] is '"' or '\'')
        {
            var closing = raw.IndexOf(raw[0], 1);
            if (closing > 0)
                return raw[1..closing];

            return raw;
        }

        for (var i = 1; i < raw.Length; i++)
        {
            if (raw[i] == '#' && char.IsWhiteSpace(raw[i - 1]))
                return raw[..i].TrimEnd();
        }

        return raw;
    }
}
