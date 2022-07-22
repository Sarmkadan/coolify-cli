#nullable enable

using System.Reflection;
using System.Text;

namespace CoolifyCli.Formatters;

/// <summary>
/// Extension methods for <see cref="TableFormatter"/> providing additional formatting capabilities.
/// </summary>
public static class TableFormatterExtensions
{
    /// <summary>
    /// Formats a collection of objects with custom column selection.
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="formatter">The table formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <param name="columns">The property names to include as columns</param>
    /// <returns>A formatted table string</returns>
    public static string FormatCollection<T>(this TableFormatter formatter, IEnumerable<T> items, IEnumerable<string> columns)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return "No data to display.";

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var selectedProperties = properties.Where(p => columns.Contains(p.Name)).ToList();

        if (selectedProperties.Count == 0)
            return "No valid columns specified.";

        var headers = selectedProperties.Select(p => p.Name).ToList();

        var rows = new List<List<string>>();
        rows.Add(headers); // Header row

        foreach (var item in itemsList)
        {
            var row = selectedProperties.Select(p => formatter.FormatValue(p.GetValue(item))).ToList();
            rows.Add(row);
        }

        return formatter.FormatTable(headers, rows);
    }

    /// <summary>
    /// Formats a collection with custom headers (different from property names).
    /// </summary>
    /// <typeparam name="T">The type of objects in the collection</typeparam>
    /// <param name="formatter">The table formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <param name="customHeaders">Custom header names for each column</param>
    /// <returns>A formatted table string</returns>
    public static string FormatCollection<T>(this TableFormatter formatter, IEnumerable<T> items, params string[] customHeaders)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return "No data to display.";

        if (customHeaders == null || customHeaders.Length == 0)
            return formatter.FormatCollection(items);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var headers = customHeaders.ToList();

        var rows = new List<List<string>>();
        rows.Add(headers); // Header row

        foreach (var item in itemsList)
        {
            var row = new List<string>();
            foreach (var header in customHeaders)
            {
                // Try to find a property that matches the header (case-insensitive)
                var property = properties.FirstOrDefault(p => string.Equals(p.Name, header, StringComparison.OrdinalIgnoreCase));
                var value = property?.GetValue(item);
                row.Add(formatter.FormatValue(value));
            }
            rows.Add(row);
        }

        return formatter.FormatTable(headers, rows);
    }

    /// <summary>
    /// Formats a dictionary with custom value formatting.
    /// </summary>
    /// <param name="formatter">The table formatter instance</param>
    /// <param name="data">The dictionary to format</param>
    /// <param name="formatValue">Optional custom formatting function for values</param>
    /// <returns>A formatted table string</returns>
    public static string FormatDictionary(this TableFormatter formatter, Dictionary<string, object?> data, Func<object?, string>? formatValue = null)
    {
        if (data is null || data.Count == 0)
            return "No data to display.";

        var rows = new List<List<string>>
        {
            new List<string> { "Key", "Value" }
        };

        foreach (var kvp in data)
        {
            var value = formatValue?.Invoke(kvp.Value) ?? formatter.FormatValue(kvp.Value);
            rows.Add(new List<string> { kvp.Key, value });
        }

        return formatter.FormatTable(new List<string> { "Key", "Value" }, rows);
    }

    /// <summary>
    /// Formats a collection as a single-column table with custom header.
    /// </summary>
    /// <param name="formatter">The table formatter instance</param>
    /// <param name="items">The collection of items to format</param>
    /// <param name="header">The header for the single column</param>
    /// <returns>A formatted single-column table string</returns>
    public static string FormatSingleColumn<T>(this TableFormatter formatter, IEnumerable<T> items, string header)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return "No data to display.";

        var rows = new List<List<string>>
        {
            new List<string> { header }
        };

        foreach (var item in itemsList)
        {
            rows.Add(new List<string> { formatter.FormatValue(item) });
        }

        return formatter.FormatTable(new List<string> { header }, rows);
    }

    /// <summary>
    /// Helper method to format table (exposed for extension methods).
    /// </summary>
    private static string FormatTable(this TableFormatter formatter, List<string> headers, List<List<string>> rows)
    {
        // Use reflection to access the private FormatTable method
        var method = typeof(TableFormatter).GetMethod(
            "FormatTable",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(List<string>), typeof(List<List<string>>) },
            null);

        if (method != null)
        {
            return (string)method.Invoke(formatter, new object[] { headers, rows })!;
        }

        // Fallback implementation if reflection fails
        var columnWidths = formatter.CalculateColumnWidths(rows, headers);
        var sb = new StringBuilder();

        // Header
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(headers[i].PadRight(columnWidths[i]));
            if (i < headers.Count - 1) sb.Append(" ");
        }
        sb.AppendLine();

        // Separator
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(new string('-', columnWidths[i]));
            if (i < headers.Count - 1) sb.Append(" ");
        }
        sb.AppendLine();

        // Data rows
        foreach (var row in rows.Skip(1))
        {
            for (int i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                sb.Append(value.PadRight(columnWidths[i]));
                if (i < headers.Count - 1) sb.Append(" ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Calculates optimal column widths for a table (exposed for extension methods).
    /// </summary>
    private static Dictionary<int, int> CalculateColumnWidths(this TableFormatter formatter, List<List<string>> rows, List<string> headers)
    {
        // Use reflection to access the private CalculateColumnWidths method
        var method = typeof(TableFormatter).GetMethod(
            "CalculateColumnWidths",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(List<List<string>>), typeof(List<string>) },
            null);

        if (method != null)
        {
            return (Dictionary<int, int>)method.Invoke(formatter, new object[] { rows, headers })!;
        }

        // Fallback implementation if reflection fails
        var widths = new Dictionary<int, int>();

        for (int col = 0; col < headers.Count; col++)
        {
            var maxWidth = headers[col].Length;

            foreach (var row in rows.Skip(1))
            {
                if (col < row.Count)
                {
                    maxWidth = Math.Max(maxWidth, row[col].Length);
                }
            }

            widths[col] = Math.Min(maxWidth + 2, 50); // Max 50 chars per column
        }

        return widths;
    }

    /// <summary>
    /// Formats a value using the same logic as TableFormatter.FormatValue.
    /// </summary>
    private static string FormatValue(this TableFormatter formatter, object? value)
    {
        if (value is null)
            return "-";

        var str = value is DateTime dt
            ? dt.ToString("yyyy-MM-dd HH:mm:ss")
            : value.ToString() ?? string.Empty;

        return str.Length > 50 ? str.Substring(0, 47) + "..." : str;
    }
}