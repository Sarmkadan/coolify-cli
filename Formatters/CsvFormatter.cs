#nullable enable
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

    /// <summary>
    /// Gets the file extension for CSV output.
    /// </summary>
    public string FileExtension => "csv";

    /// <summary>
    /// Gets the MIME type for CSV output.
    /// </summary>
    public string MimeType => "text/csv";

    public CsvFormatter(char delimiter = ',', bool includeHeader = true, List<string>? selectedFields = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(delimiter, '\0');
        ArgumentOutOfRangeException.ThrowIfEqual(char.IsWhiteSpace(delimiter), true);

        _delimiter = delimiter;
        _includeHeader = includeHeader;
        _selectedFields = selectedFields?.Count > 0 ? selectedFields : null;
    }

    /// <summary>
    /// Gets the delimiter character used by this formatter.
    /// </summary>
    public char Delimiter => _delimiter;

    /// <summary>
    /// Gets a value indicating whether to include header row in output.
    /// </summary>
    public bool IncludeHeader => _includeHeader;

    /// <summary>
    /// Gets the list of selected fields, or null if all fields should be included.
    /// </summary>
    public IReadOnlyList<string>? SelectedFields => _selectedFields;

    /// <summary>
    /// Formats a single object as a CSV line.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public string Format(object? data)
    {
        if (data is null)
            return string.Empty;

        var properties = GetProperties(data.GetType());
        var values = properties.Select(p => FormatValue(p.GetValue(data)));

        return CombineValues(values);
    }

    /// <summary>
    /// Formats a collection of objects as CSV with header row and streams directly to the writer.
    /// </summary>
    /// <typeparam name="T">The type of objects to format</typeparam>
    /// <param name="items">The collection of items to format</param>
    /// <param name="writer">The text writer to write the CSV output to</param>
    /// <exception cref="ArgumentNullException">Thrown when items or writer is null.</exception>
    public void Format<T>(IEnumerable<T> items, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(writer);

        var properties = GetProperties(typeof(T));

        // Write header row if requested
        if (_includeHeader)
        {
            var headers = properties.Select(p => EscapeCsvField(p.Name));
            writer.WriteLine(CombineValues(headers));
        }

        // Write data rows - stream each row as we process it
        foreach (var item in items)
        {
            var values = properties.Select(p => FormatValue(p.GetValue(item)));
            writer.WriteLine(CombineValues(values));
        }
    }

    /// <summary>
    /// Formats a collection of objects as CSV with header row.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
    public string FormatCollection<T>(IEnumerable<T>? items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return string.Empty;

        using var writer = new StringWriter();
        Format(itemsList, writer);
        return writer.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats a dictionary as a CSV line.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public string FormatDictionary(Dictionary<string, object?>? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count == 0)
            return string.Empty;

        using var writer = new StringWriter();
        FormatDictionary(data, writer);
        return writer.ToString().TrimEnd();
    }

    /// <summary>
    /// Formats a dictionary as CSV and streams directly to the writer.
    /// </summary>
    /// <param name="data">The dictionary to format</param>
    /// <param name="writer">The text writer to write the CSV output to</param>
    /// <exception cref="ArgumentNullException">Thrown when data or writer is null.</exception>
    public void FormatDictionary(Dictionary<string, object?> data, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(writer);

        if (_includeHeader && data.Count > 0)
        {
            var headers = data.Keys.Select(k => EscapeCsvField(k));
            writer.WriteLine(CombineValues(headers));
        }

        var values = data.Values.Select(FormatValue);
        writer.Write(CombineValues(values));
    }

    /// <summary>
    /// Converts CSV data to a dictionary with headers as keys.
    /// </summary>
    public List<Dictionary<string, string>> ParseCsv(string csvContent)
    {
        var result = new List<Dictionary<string, string>>();
        var records = ParseCsvRecords(csvContent);

        if (records.Count < 2)
            return result;

        var headers = records[0];

        for (int i = 1; i < records.Count; i++)
        {
            var values = records[i];
            if (values.Count == 1 && string.IsNullOrWhiteSpace(values[0]))
                continue;

            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Count && j < values.Count; j++)
            {
                row[headers[j].Trim()] = values[j].Trim();
            }

            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Parses CSV content into a list of records, each being a list of field values.
    /// Handles quoted fields with embedded newlines, commas, and escaped quotes.
    /// </summary>
    private List<List<string>> ParseCsvRecords(string csvContent)
    {
        var records = new List<List<string>>();
        var currentRecord = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < csvContent.Length)
        {
            var ch = csvContent[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < csvContent.Length && csvContent[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = false;
                        i++;
                    }
                }
                else
                {
                    currentField.Append(ch);
                    i++;
                }
            }
            else
            {
                if (ch == '"')
                {
                    inQuotes = true;
                    i++;
                }
                else if (ch == _delimiter)
                {
                    currentRecord.Add(currentField.ToString());
                    currentField.Clear();
                    i++;
                }
                else if (ch == '\r' || ch == '\n')
                {
                    currentRecord.Add(currentField.ToString());
                    currentField.Clear();
                    records.Add(currentRecord);
                    currentRecord = new List<string>();

                    if (ch == '\r' && i + 1 < csvContent.Length && csvContent[i + 1] == '\n')
                        i += 2;
                    else
                        i++;
                }
                else
                {
                    currentField.Append(ch);
                    i++;
                }
            }
        }

        if (currentField.Length > 0 || currentRecord.Count > 0)
        {
            currentRecord.Add(currentField.ToString());
            records.Add(currentRecord);
        }

        return records;
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
        if (field.Contains(_delimiter) || field.Contains('\"') || field.Contains('\n') || field.Contains('\r') || field.Contains('\t') || field.Contains(';'))
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