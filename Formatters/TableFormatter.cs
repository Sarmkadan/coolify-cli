// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace CoolifiCli.Formatters;

/// <summary>
/// Formats data as ASCII tables for console output. Provides automatic column sizing,
/// alignment options, and border styles. Creates human-readable tabular output.
/// </summary>
public class TableFormatter : IOutputFormatter
{
    private readonly TableStyle _style;
    private readonly List<string>? _columnNames;
    private readonly Dictionary<string, int>? _columnWidths;

    public TableFormatter(TableStyle style = TableStyle.Simple, List<string>? columnNames = null)
    {
        _style = style;
        _columnNames = columnNames;
    }

    /// <summary>
    /// Formats a single object as a single-row table.
    /// </summary>
    public string Format(object? data)
    {
        if (data == null)
            return string.Empty;

        var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var rows = new List<List<string>>
        {
            properties.Select(p => p.Name).ToList(),
            properties.Select(p => FormatValue(p.GetValue(data))).ToList()
        };

        return FormatTable(properties.Select(p => p.Name).ToList(), rows);
    }

    /// <summary>
    /// Formats a collection of objects as a multi-row table.
    /// </summary>
    public string FormatCollection<T>(IEnumerable<T> items)
    {
        var itemsList = items.ToList();
        if (itemsList.Count == 0)
            return "No data to display.";

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var headers = properties.Select(p => p.Name).ToList();

        var rows = new List<List<string>>();
        rows.Add(headers); // Header row

        foreach (var item in itemsList)
        {
            var row = properties.Select(p => FormatValue(p.GetValue(item))).ToList();
            rows.Add(row);
        }

        return FormatTable(headers, rows);
    }

    /// <summary>
    /// Formats a dictionary as a two-column key-value table.
    /// </summary>
    public string FormatDictionary(Dictionary<string, object?> data)
    {
        if (data == null || data.Count == 0)
            return "No data to display.";

        var rows = new List<List<string>>
        {
            new List<string> { "Key", "Value" }
        };

        foreach (var kvp in data)
        {
            rows.Add(new List<string> { kvp.Key, FormatValue(kvp.Value) });
        }

        return FormatTable(new List<string> { "Key", "Value" }, rows);
    }

    /// <summary>
    /// Calculates optimal column widths based on content.
    /// </summary>
    private Dictionary<int, int> CalculateColumnWidths(List<List<string>> rows, List<string> headers)
    {
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
    /// Formats the complete table with headers, rows, and borders.
    /// </summary>
    private string FormatTable(List<string> headers, List<List<string>> rows)
    {
        var columnWidths = CalculateColumnWidths(rows, headers);
        var sb = new StringBuilder();

        switch (_style)
        {
            case TableStyle.Simple:
                FormatSimple(sb, headers, rows, columnWidths);
                break;
            case TableStyle.Bordered:
                FormatBordered(sb, headers, rows, columnWidths);
                break;
            case TableStyle.Minimal:
                FormatMinimal(sb, headers, rows, columnWidths);
                break;
            case TableStyle.Grid:
                FormatGrid(sb, headers, rows, columnWidths);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Simple table format with headers separated by dashes.
    /// </summary>
    private void FormatSimple(StringBuilder sb, List<string> headers, List<List<string>> rows, Dictionary<int, int> widths)
    {
        // Header
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(headers[i].PadRight(widths[i]));
            if (i < headers.Count - 1)
                sb.Append("  ");
        }
        sb.AppendLine();

        // Separator
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(new string('-', widths[i]));
            if (i < headers.Count - 1)
                sb.Append("  ");
        }
        sb.AppendLine();

        // Data rows
        foreach (var row in rows.Skip(1))
        {
            for (int i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                sb.Append(value.PadRight(widths[i]));
                if (i < headers.Count - 1)
                    sb.Append("  ");
            }
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Bordered table format with full box drawing characters.
    /// </summary>
    private void FormatBordered(StringBuilder sb, List<string> headers, List<List<string>> rows, Dictionary<int, int> widths)
    {
        var totalWidth = widths.Sum(x => x.Value) + (headers.Count * 3) - 1;
        var borderLine = new string('─', totalWidth);

        // Top border
        sb.AppendLine($"┌{borderLine}┐");

        // Header row
        sb.Append("│ ");
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(headers[i].PadRight(widths[i]));
            sb.Append(" │ ");
        }
        sb.AppendLine();

        // Separator
        sb.AppendLine($"├{new string('─', totalWidth)}┤");

        // Data rows
        foreach (var row in rows.Skip(1))
        {
            sb.Append("│ ");
            for (int i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                sb.Append(value.PadRight(widths[i]));
                sb.Append(" │ ");
            }
            sb.AppendLine();
        }

        // Bottom border
        sb.AppendLine($"└{borderLine}┘");
    }

    /// <summary>
    /// Minimal table format with no borders.
    /// </summary>
    private void FormatMinimal(StringBuilder sb, List<string> headers, List<List<string>> rows, Dictionary<int, int> widths)
    {
        // Header
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(headers[i].PadRight(widths[i]));
            if (i < headers.Count - 1)
                sb.Append(" ");
        }
        sb.AppendLine();

        // Data rows
        foreach (var row in rows.Skip(1))
        {
            for (int i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                sb.Append(value.PadRight(widths[i]));
                if (i < headers.Count - 1)
                    sb.Append(" ");
            }
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Grid table format with full grid lines.
    /// </summary>
    private void FormatGrid(StringBuilder sb, List<string> headers, List<List<string>> rows, Dictionary<int, int> widths)
    {
        var separatorLine = "+" + string.Join("+", widths.Select(x => new string('─', x.Value))) + "+";

        sb.AppendLine(separatorLine);

        // Header
        sb.Append("|");
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append(headers[i].PadRight(widths[i]));
            sb.Append("|");
        }
        sb.AppendLine();

        sb.AppendLine(separatorLine);

        // Data rows
        foreach (var row in rows.Skip(1))
        {
            sb.Append("|");
            for (int i = 0; i < headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                sb.Append(value.PadRight(widths[i]));
                sb.Append("|");
            }
            sb.AppendLine();
        }

        sb.AppendLine(separatorLine);
    }

    /// <summary>
    /// Formats a value for table display, truncating long strings.
    /// </summary>
    private string FormatValue(object? value)
    {
        if (value == null)
            return "-";

        var str = value is DateTime dt
            ? dt.ToString("yyyy-MM-dd HH:mm:ss")
            : value.ToString() ?? string.Empty;

        return str.Length > 50 ? str.Substring(0, 47) + "..." : str;
    }
}

/// <summary>
/// Table styling options.
/// </summary>
public enum TableStyle
{
    Simple,    // Headers with dashes
    Bordered,  // Box drawing characters
    Minimal,   // No borders
    Grid       // Full grid lines
}
