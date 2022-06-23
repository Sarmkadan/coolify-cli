#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace CoolifyCli.Formatters;

/// <summary>
/// Formats data as CSV output. Handles proper escaping, quoting, and delimiter handling.
/// Supports custom delimiters, header rows, and field selection.
/// </summary>
public class CsvFormatter : IOutputFormatter
{
    private readonly char _delimiter;
    private readonly bool _includeHeader;
    private readonly List<string>? _selectedFields;

    public CsvFormatter(char delimiter = ',', bool includeHeader = true, List<string>? selectedFields = null)
    {
        _delimiter = delimiter;
        _includeHeader = includeHeader;
        _selectedFields = selectedFields;
    }

    /// <summary>
    /// Formats a single object as a CSV line.
    /// </summary>
    public string Format(object? data)
    {
        if (data is null)
            return string.Empty;

        var properties = GetProperties(data.GetType());
        var values = properties.Select(p => FormatValue(p.GetValue(data)));

        return CombineValues(values);
    }

    /// <summary>
    /// Formats a collection of objects as CSV with header row.
    /// </summary>
    public string FormatCollection<T>(IEnumerable<T> items)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var properties = GetProperties(typeof(T));

        // Write header row if requested
        if (_includeHeader)
        {
            var headers = properties.Select(p => EscapeCsvField(p.Name));
            sb.AppendLine(CombineValues(headers));
        }

        // Write data rows
        foreach (var item in itemsList)
        {
            var values = properties.Select(p => FormatValue(p.GetValue(item)));
            sb.AppendLine(CombineValues(values));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats a dictionary as a CSV line.
    /// </summary>
    public string FormatDictionary(Dictionary<string, object?> data)
    {
        if (data is null || data.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        if (_includeHeader)
        {
            var headers = data.Keys.Select(k => EscapeCsvField(k));
            sb.AppendLine(CombineValues(headers));
        }

        var values = data.Values.Select(FormatValue);
        sb.Append(CombineValues(values));

        return sb.ToString();
    }

    /// <summary>
    /// Converts CSV data to a dictionary with headers as keys.
    /// </summary>
    public List<Dictionary<string, string>> ParseCsv(string csvContent)
    {
        var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var result = new List<Dictionary<string, string>>();

        if (lines.Length < 2)
            return result;

        var headers = lines[0].Split(_delimiter);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var values = lines[i].Split(_delimiter);
            var row = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                row[headers[j].Trim()] = UnescapeCsvField(values[j].Trim());
            }

            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Gets properties from an object type, filtering by selected fields if provided.
    /// </summary>
    private PropertyInfo[] GetProperties(Type type)
    {
        var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (_selectedFields is null || _selectedFields.Count == 0)
            return allProperties;

        return allProperties
            .Where(p => _selectedFields.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>
    /// Formats a single value for CSV output with proper escaping.
    /// </summary>
    private string FormatValue(object? value)
    {
        if (value is null)
            return string.Empty;

        if (value is DateTime dt)
            return EscapeCsvField(dt.ToString("yyyy-MM-dd HH:mm:ss"));

        if (value is bool b)
            return b ? "true" : "false";

        if (value is decimal or double or float)
            return value.ToString() ?? string.Empty;

        return EscapeCsvField(value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Escapes CSV field by quoting if it contains special characters.
    /// </summary>
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Quote field if it contains delimiter, quotes, or newlines
        if (field.Contains(_delimiter) || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    /// <summary>
    /// Unescapes a quoted CSV field.
    /// </summary>
    private string UnescapeCsvField(string field)
    {
        if (field.StartsWith("\"") && field.EndsWith("\""))
        {
            return field.Substring(1, field.Length - 2).Replace("\"\"", "\"");
        }

        return field;
    }

    /// <summary>
    /// Combines values into a CSV line using the configured delimiter.
    /// </summary>
    private string CombineValues(IEnumerable<string> values)
    {
        return string.Join(_delimiter.ToString(), values);
    }
}
