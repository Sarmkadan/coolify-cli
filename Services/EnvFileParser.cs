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

            entries.Add(new EnvVarEntry(
                line[..separator].Trim(),
                line[(separator + 1)..].Trim(),
                lineNumber));
        }

        return entries;
    }
}
