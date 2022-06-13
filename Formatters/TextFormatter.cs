#nullable enable
using System.Text;

namespace CoolifyCli.Formatters;

/// <summary>
/// Formats data as plain text output with various styling options.
/// Supports colors, alignment, indentation, and structured text layouts.
/// </summary>
public class TextFormatter
{
    private readonly StringBuilder _content = new();
    private int _indentLevel = 0;
    private const int IndentSize = 2;

    /// <summary>
    /// Adds a plain text line.
    /// </summary>
    public TextFormatter WriteLine(string text = "")
    {
        if (string.IsNullOrEmpty(text))
        {
            _content.AppendLine();
        }
        else
        {
            _content.AppendLine(GetIndent() + text);
        }

        return this;
    }

    /// <summary>
    /// Adds a colored text line.
    /// </summary>
    public TextFormatter WriteLineColored(string text, ConsoleColor color)
    {
        var colorCode = GetColorCode(color);
        _content.AppendLine($"{colorCode}{GetIndent()}{text}\x1b[0m");
        return this;
    }

    /// <summary>
    /// Adds a success message (green).
    /// </summary>
    public TextFormatter WriteSuccess(string text)
    {
        return WriteLineColored($"✓ {text}", ConsoleColor.Green);
    }

    /// <summary>
    /// Adds an error message (red).
    /// </summary>
    public TextFormatter WriteError(string text)
    {
        return WriteLineColored($"✗ {text}", ConsoleColor.Red);
    }

    /// <summary>
    /// Adds a warning message (yellow).
    /// </summary>
    public TextFormatter WriteWarning(string text)
    {
        return WriteLineColored($"⚠ {text}", ConsoleColor.Yellow);
    }

    /// <summary>
    /// Adds an info message (cyan).
    /// </summary>
    public TextFormatter WriteInfo(string text)
    {
        return WriteLineColored($"ℹ {text}", ConsoleColor.Cyan);
    }

    /// <summary>
    /// Adds a header with underline.
    /// </summary>
    public TextFormatter WriteHeader(string title)
    {
        WriteLine(title);
        WriteLine(new string('=', title.Length));
        return this;
    }

    /// <summary>
    /// Adds a subheader.
    /// </summary>
    public TextFormatter WriteSubheader(string title)
    {
        WriteLine(title);
        WriteLine(new string('-', title.Length));
        return this;
    }

    /// <summary>
    /// Increases indentation level.
    /// </summary>
    public TextFormatter Indent()
    {
        _indentLevel++;
        return this;
    }

    /// <summary>
    /// Decreases indentation level.
    /// </summary>
    public TextFormatter Outdent()
    {
        if (_indentLevel > 0)
            _indentLevel--;

        return this;
    }

    /// <summary>
    /// Adds a key-value pair.
    /// </summary>
    public TextFormatter WriteKeyValue(string key, object? value)
    {
        WriteLine($"{key}: {value ?? "-"}");
        return this;
    }

    /// <summary>
    /// Adds a list item with bullet.
    /// </summary>
    public TextFormatter WriteListItem(string item)
    {
        WriteLine($"• {item}");
        return this;
    }

    /// <summary>
    /// Adds a numbered list item.
    /// </summary>
    public TextFormatter WriteNumberedItem(int number, string item)
    {
        WriteLine($"{number}. {item}");
        return this;
    }

    /// <summary>
    /// Adds a code block.
    /// </summary>
    public TextFormatter WriteCodeBlock(string code)
    {
        WriteLine("┌─────────────────────────────────┐");
        Indent();

        foreach (var line in code.Split('\n'))
        {
            WriteLine(line);
        }

        Outdent();
        WriteLine("└─────────────────────────────────┘");
        return this;
    }

    /// <summary>
    /// Adds a separator line.
    /// </summary>
    public TextFormatter WriteSeparator()
    {
        WriteLine(new string('-', 40));
        return this;
    }

    /// <summary>
    /// Adds a formatted progress bar.
    /// </summary>
    public TextFormatter WriteProgressBar(int current, int total, string label = "Progress")
    {
        var percent = (double)current / total;
        var filled = (int)(percent * 20);
        var bar = new string('█', filled) + new string('░', 20 - filled);
        WriteLine($"{label}: [{bar}] {percent:P0}");
        return this;
    }

    /// <summary>
    /// Adds a table from columns and rows.
    /// </summary>
    public TextFormatter WriteTable(string[] headers, string[][] rows)
    {
        if (headers.Length == 0 || rows.Length == 0)
            return this;

        // Calculate column widths
        var colWidths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            colWidths[i] = headers[i].Length;
        }

        foreach (var row in rows)
        {
            for (int i = 0; i < headers.Length && i < row.Length; i++)
            {
                colWidths[i] = Math.Max(colWidths[i], row[i].Length);
            }
        }

        // Write header
        var headerLine = GetIndent() + string.Join("  ", headers.Select((h, i) => h.PadRight(colWidths[i])));
        _content.AppendLine(headerLine);

        // Write separator
        var separatorLine = GetIndent() + string.Join("  ", colWidths.Select(w => new string('-', w)));
        _content.AppendLine(separatorLine);

        // Write rows
        foreach (var row in rows)
        {
            var rowLine = GetIndent() + string.Join("  ", row.Select((v, i) =>
                i < colWidths.Length ? v.PadRight(colWidths[i]) : v));
            _content.AppendLine(rowLine);
        }

        return this;
    }

    /// <summary>
    /// Adds a panel/box with a message.
    /// </summary>
    public TextFormatter WritePanel(string content, string? title = null)
    {
        var width = Math.Max(content.Length, title?.Length ?? 0) + 4;

        WriteLine("┌" + new string('─', width - 2) + "┐");

        if (!string.IsNullOrEmpty(title))
        {
            WriteLine($"│ {title.PadRight(width - 4)} │");
            WriteLine("├" + new string('─', width - 2) + "┤");
        }

        foreach (var line in content.Split('\n'))
        {
            WriteLine($"│ {line.PadRight(width - 4)} │");
        }

        WriteLine("└" + new string('─', width - 2) + "┘");

        return this;
    }

    /// <summary>
    /// Clears all content.
    /// </summary>
    public void Clear()
    {
        _content.Clear();
        _indentLevel = 0;
    }

    /// <summary>
    /// Gets the formatted text content.
    /// </summary>
    public override string ToString() => _content.ToString();

    /// <summary>
    /// Writes the content to console.
    /// </summary>
    public void Print()
    {
        Console.Write(_content.ToString());
    }

    /// <summary>
    /// Gets the current indentation as spaces.
    /// </summary>
    private string GetIndent() => new string(' ', _indentLevel * IndentSize);

    /// <summary>
    /// Gets ANSI color code for a console color.
    /// </summary>
    private string GetColorCode(ConsoleColor color) => color switch
    {
        ConsoleColor.Red => "\x1b[31m",
        ConsoleColor.Green => "\x1b[32m",
        ConsoleColor.Yellow => "\x1b[33m",
        ConsoleColor.Blue => "\x1b[34m",
        ConsoleColor.Magenta => "\x1b[35m",
        ConsoleColor.Cyan => "\x1b[36m",
        ConsoleColor.White => "\x1b[37m",
        ConsoleColor.Gray => "\x1b[90m",
        _ => "\x1b[0m"
    };
}
