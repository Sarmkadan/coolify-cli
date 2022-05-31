// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Utilities;

using CoolifiCli.Models;

/// <summary>
/// Helper utilities for CLI output formatting and user interaction.
/// </summary>
public static class CliHelpers
{
    /// <summary>
    /// Prints a section header with decorative line.
    /// </summary>
    /// <param name="title">Section title.</param>
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a subheader with decorative line.
    /// </summary>
    /// <param name="title">Subheader title.</param>
    public static void PrintSubheader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n{title}");
        Console.WriteLine(new string('-', title.Length));
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a success message in green.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an error message in red.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a warning message in yellow.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prints an info message in blue.
    /// </summary>
    /// <param name="message">Message to print.</param>
    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Prompts user for confirmation (yes/no).
    /// </summary>
    /// <param name="prompt">Confirmation prompt.</param>
    /// <returns>True if user confirms.</returns>
    public static bool PromptConfirmation(string prompt)
    {
        Console.Write($"{prompt} (y/n): ");
        var response = Console.ReadLine()?.ToLower();
        return response == "y" || response == "yes";
    }

    /// <summary>
    /// Prompts user for input.
    /// </summary>
    /// <param name="prompt">Input prompt.</param>
    /// <returns>User input string.</returns>
    public static string? PromptInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine();
    }

    /// <summary>
    /// Prompts user for secret input (masks typed characters).
    /// </summary>
    /// <param name="prompt">Input prompt.</param>
    /// <returns>User input string.</returns>
    public static string PromptSecretInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        var result = string.Empty;

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace && result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }
            else if (key.Key != ConsoleKey.Backspace)
            {
                result += key.KeyChar;
            }
        }

        return result;
    }

    /// <summary>
    /// Formats bytes into human-readable size string.
    /// </summary>
    /// <param name="bytes">Size in bytes.</param>
    /// <returns>Formatted size string.</returns>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Formats a timespan into human-readable string.
    /// </summary>
    /// <param name="timespan">TimeSpan to format.</param>
    /// <returns>Formatted timespan string.</returns>
    public static string FormatTimeSpan(TimeSpan timespan)
    {
        if (timespan.TotalSeconds < 60)
            return $"{timespan.TotalSeconds:F0}s";

        if (timespan.TotalMinutes < 60)
            return $"{timespan.TotalMinutes:F0}m {timespan.Seconds}s";

        if (timespan.TotalHours < 24)
            return $"{timespan.Hours}h {timespan.Minutes}m";

        return $"{timespan.Days}d {timespan.Hours}h";
    }

    /// <summary>
    /// Gets colored status indicator for deployment status.
    /// </summary>
    /// <param name="status">Deployment status.</param>
    /// <returns>Colored status string.</returns>
    public static string GetStatusIndicator(DeploymentStatus status)
    {
        return status switch
        {
            DeploymentStatus.Deployed => GetColoredText("●", ConsoleColor.Green),
            DeploymentStatus.InProgress => GetColoredText("◐", ConsoleColor.Yellow),
            DeploymentStatus.Failed => GetColoredText("●", ConsoleColor.Red),
            DeploymentStatus.Pending => GetColoredText("○", ConsoleColor.Gray),
            DeploymentStatus.Rollback => GetColoredText("↻", ConsoleColor.Magenta),
            _ => GetColoredText("?", ConsoleColor.White)
        };
    }

    /// <summary>
    /// Gets colored health status indicator.
    /// </summary>
    /// <param name="status">Health status.</param>
    /// <returns>Colored status string.</returns>
    public static string GetHealthIndicator(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => GetColoredText("●", ConsoleColor.Green),
            HealthStatus.Degraded => GetColoredText("●", ConsoleColor.Yellow),
            HealthStatus.Unhealthy => GetColoredText("●", ConsoleColor.Red),
            HealthStatus.Critical => GetColoredText("●", ConsoleColor.DarkRed),
            _ => GetColoredText("?", ConsoleColor.White)
        };
    }

    /// <summary>
    /// Returns colored text for console output.
    /// </summary>
    /// <param name="text">Text to color.</param>
    /// <param name="color">Console color.</param>
    /// <returns>Colored text that can be printed to console.</returns>
    private static string GetColoredText(string text, ConsoleColor color)
    {
        // Note: This returns the text as-is; actual coloring is done at output time
        return text;
    }

    /// <summary>
    /// Prints a formatted table with specified columns.
    /// </summary>
    /// <param name="headers">Column headers.</param>
    /// <param name="rows">Table rows (list of column values).</param>
    public static void PrintTable(string[] headers, List<string[]> rows)
    {
        if (headers == null || headers.Length == 0)
            return;

        // Calculate column widths
        int[] columnWidths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            columnWidths[i] = headers[i].Length;
        }

        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length && i < columnWidths.Length; i++)
            {
                columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
            }
        }

        // Print headers
        for (int i = 0; i < headers.Length; i++)
        {
            Console.Write(headers[i].PadRight(columnWidths[i] + 2));
        }
        Console.WriteLine();

        // Print separator
        for (int i = 0; i < headers.Length; i++)
        {
            Console.Write(new string('-', columnWidths[i] + 2));
        }
        Console.WriteLine();

        // Print rows
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length && i < columnWidths.Length; i++)
            {
                Console.Write(row[i].PadRight(columnWidths[i] + 2));
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Creates a progress bar display.
    /// </summary>
    /// <param name="percentage">Progress percentage (0-100).</param>
    /// <param name="width">Width of the progress bar.</param>
    /// <returns>Progress bar string.</returns>
    public static string GetProgressBar(int percentage, int width = 20)
    {
        var filled = (int)((percentage / 100.0) * width);
        var empty = width - filled;

        return $"[{new string('█', filled)}{new string('░', empty)}] {percentage}%";
    }
}
