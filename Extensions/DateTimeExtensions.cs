#nullable enable
namespace CoolifyCli.Extensions;

/// <summary>
/// Extension methods for DateTime and TimeSpan manipulation.
/// Provides utilities for formatting and relative time calculations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to a human-readable relative time string (e.g., "2 hours ago").
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 1)
            return "just now";

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} seconds ago";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours > 1 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays > 1 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) > 1 ? "s" : "")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")} ago";
    }

    /// <summary>
    /// Formats a TimeSpan as a human-readable duration string (e.g., "2h 30m 15s").
    /// </summary>
    public static string ToReadableDuration(this TimeSpan timeSpan)
    {
        var parts = new List<string>();

        if (timeSpan.Days > 0)
            parts.Add($"{timeSpan.Days}d");

        if (timeSpan.Hours > 0)
            parts.Add($"{timeSpan.Hours}h");

        if (timeSpan.Minutes > 0)
            parts.Add($"{timeSpan.Minutes}m");

        if (timeSpan.Seconds > 0 || parts.Count == 0)
            parts.Add($"{timeSpan.Seconds}s");

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Formats a TimeSpan as milliseconds with appropriate unit (ms, s, m, h).
    /// </summary>
    public static string ToFormattedDuration(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMilliseconds < 1000)
            return $"{timeSpan.TotalMilliseconds:F0}ms";

        if (timeSpan.TotalSeconds < 60)
            return $"{timeSpan.TotalSeconds:F2}s";

        if (timeSpan.TotalMinutes < 60)
            return $"{timeSpan.TotalMinutes:F2}m";

        return $"{timeSpan.TotalHours:F2}h";
    }

    /// <summary>
    /// Converts milliseconds to a formatted duration string.
    /// </summary>
    public static string MillisecondsToReadable(this long milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds).ToReadableDuration();
    }

    /// <summary>
    /// Gets the start of the day for a DateTime.
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day for a DateTime.
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week for a DateTime.
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startDayOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the start of the month for a DateTime.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month for a DateTime.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }

    /// <summary>
    /// Checks if a DateTime is in the past.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is in the future.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a DateTime is today.
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Formats a DateTime as ISO 8601 string for API calls.
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O");
    }

    /// <summary>
    /// Gets the number of business days between two dates.
    /// </summary>
    public static int BusinessDaysBetween(this DateTime startDate, DateTime endDate)
    {
        var businessDays = 0;
        var current = startDate;

        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// Rounds a DateTime to the nearest minute.
    /// </summary>
    public static DateTime RoundToMinute(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
            dateTime.Hour, dateTime.Minute, 0);
    }

    /// <summary>
    /// Converts a Unix timestamp (seconds since epoch) to DateTime.
    /// </summary>
    public static DateTime FromUnixTimestamp(this long unixTimestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(unixTimestamp);
    }

    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var timeSpan = dateTime.ToUniversalTime() - epoch;
        return (long)timeSpan.TotalSeconds;
    }
}
