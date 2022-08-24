namespace CoolifyCli.Models;

public static class LogEntryExtensions
{
    /// <summary>
    /// Determines if a log entry is related to an application crash.
    /// </summary>
    /// <param name="logEntry">The log entry to check.</param>
    /// <returns>true if the log entry is related to an application crash; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEntry"/> is null.</exception>
    public static bool IsCrashRelated(this LogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);

        return logEntry.Level == LogLevel.Error
            && (logEntry.ExitCode.HasValue && logEntry.ExitCode > 0
                || !string.IsNullOrEmpty(logEntry.StackTrace));
    }

    /// <summary>
    /// Formats a log entry with its metadata into a human-readable string.
    /// </summary>
    /// <param name="logEntry">The log entry to format.</param>
    /// <returns>A formatted string representation of the log entry with its metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEntry"/> is null.</exception>
    public static string FormatWithMetadata(this LogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);

        var metadataString = string.Join(Environment.NewLine, logEntry.Metadata.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        return $"{logEntry.ToString()}{Environment.NewLine}Metadata:{Environment.NewLine}{metadataString}";
    }
}
