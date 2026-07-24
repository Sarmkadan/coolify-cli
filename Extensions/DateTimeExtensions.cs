#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoolifyCli.Extensions;

/// <summary>
/// Supplies holiday information for business-day calculations. Implementations decide
/// which calendar dates are treated as non-working days in addition to weekends.
/// </summary>
public interface IHolidayProvider
{
    /// <summary>
    /// Determines whether the given date is a holiday and should be skipped by
    /// business-day calculations.
    /// </summary>
    /// <param name="date">The date to check. Only the date component is considered.</param>
    /// <returns><see langword="true"/> if <paramref name="date"/> is a holiday; otherwise, <see langword="false"/>.</returns>
    bool IsHoliday(DateTime date);
}

/// <summary>
/// An <see cref="IHolidayProvider"/> that never reports a holiday. Used as the default
/// when no holiday calendar is supplied, so business-day calculations only skip weekends.
/// </summary>
public sealed class NoHolidayProvider : IHolidayProvider
{
    /// <summary>
    /// Gets the shared singleton instance of <see cref="NoHolidayProvider"/>.
    /// </summary>
    public static NoHolidayProvider Instance { get; } = new();

    /// <inheritdoc />
    public bool IsHoliday(DateTime date) => false;
}

/// <summary>
/// An <see cref="IHolidayProvider"/> backed by a fixed set of holiday dates.
/// </summary>
public sealed class FixedHolidayProvider : IHolidayProvider
{
    private readonly HashSet<DateTime> _holidays;

    /// <summary>
    /// Initializes a new instance of <see cref="FixedHolidayProvider"/> with the given holiday dates.
    /// </summary>
    /// <param name="holidays">The collection of holiday dates. Only the date component of each value is used.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="holidays"/> is null.</exception>
    public FixedHolidayProvider(IEnumerable<DateTime> holidays)
    {
        ArgumentNullException.ThrowIfNull(holidays);

        _holidays = new HashSet<DateTime>();
        foreach (var holiday in holidays)
            _holidays.Add(holiday.Date);
    }

    /// <inheritdoc />
    public bool IsHoliday(DateTime date) => _holidays.Contains(date.Date);
}

/// <summary>
/// Extension methods for <see cref="DateTime"/> and <see cref="TimeSpan"/> manipulation.
/// Provides utilities for formatting, relative time calculations, and date arithmetic.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to a human-readable relative time string (e.g., "2 hours ago" or "in 5 minutes").
    /// </summary>
    /// <remarks>
    /// The method normalizes the input <see cref="DateTime"/> to UTC for consistent comparison:
    /// <list type="bullet">
    /// <item><description><see cref="DateTimeKind.Local"/> values are converted to UTC using <see cref="DateTime.ToUniversalTime()"/></description></item>
    /// <item><description><see cref="DateTimeKind.Utc"/> values are used as-is</description></item>
    /// <item><description><see cref="DateTimeKind.Unspecified"/> values are treated as UTC (no conversion applied)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="dateTime">The date and time to convert to a relative string.</param>
    /// <returns>A human-readable string representing the relative time, or "just now" if within 1 second.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dateTime"/> is null.</exception>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        var utcNow = DateTime.UtcNow;
        var utcDateTime = dateTime.Kind == DateTimeKind.Local
            ? dateTime.ToUniversalTime()
            : dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime; // Unspecified treated as UTC

        var timeSpan = utcNow - utcDateTime;

        return timeSpan.TotalSeconds switch
        {
            < 1 => "just now",
            > -1 and < 60 => timeSpan.TotalSeconds > 0
                ? $"{(int)timeSpan.TotalSeconds} second{(timeSpan.TotalSeconds > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalSeconds)} second{(-timeSpan.TotalSeconds > 1 ? "s" : "")}",
            > -60 and < 3600 => timeSpan.TotalMinutes > 0
                ? $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalMinutes)} minute{(-timeSpan.TotalMinutes > 1 ? "s" : "")}",
            > -86400 and < 86400 => timeSpan.TotalHours > 0
                ? $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalHours)} hour{(-timeSpan.TotalHours > 1 ? "s" : "")}",
            > -604800 and < 604800 => timeSpan.TotalDays > 0
                ? $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays)} day{(-timeSpan.TotalDays > 1 ? "s" : "")}",
            > -2592000 and < 2592000 => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 7)} week{((int)(-timeSpan.TotalDays / 7) > 1 ? "s" : "")}",
            > -31536000 and < 31536000 => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 30)} month{((int)(-timeSpan.TotalDays / 30) > 1 ? "s" : "")}",
            _ => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 365)} year{((int)(-timeSpan.TotalDays / 365) > 1 ? "s" : "")}"
        };
    }

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to a human-readable relative time string (e.g., "2 hours ago" or "in 5 minutes").
    /// </summary>
    /// <param name="dateTimeOffset">The date and time with offset to convert to a relative string.</param>
    /// <returns>A human-readable string representing the relative time, or "just now" if within 1 second.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dateTimeOffset"/> is null.</exception>
    public static string ToRelativeTime(this DateTimeOffset dateTimeOffset)
    {
        ArgumentNullException.ThrowIfNull(dateTimeOffset);

        var utcNow = DateTimeOffset.UtcNow;
        var timeSpan = utcNow - dateTimeOffset;

        return timeSpan.TotalSeconds switch
        {
            < 1 => "just now",
            > -1 and < 60 => timeSpan.TotalSeconds > 0
                ? $"{(int)timeSpan.TotalSeconds} second{(timeSpan.TotalSeconds > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalSeconds)} second{(-timeSpan.TotalSeconds > 1 ? "s" : "")}",
            > -60 and < 3600 => timeSpan.TotalMinutes > 0
                ? $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalMinutes)} minute{(-timeSpan.TotalMinutes > 1 ? "s" : "")}",
            > -86400 and < 86400 => timeSpan.TotalHours > 0
                ? $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalHours)} hour{(-timeSpan.TotalHours > 1 ? "s" : "")}",
            > -604800 and < 604800 => timeSpan.TotalDays > 0
                ? $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays)} day{(-timeSpan.TotalDays > 1 ? "s" : "")}",
            > -2592000 and < 2592000 => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 7)} week{((int)(-timeSpan.TotalDays / 7) > 1 ? "s" : "")}",
            > -31536000 and < 31536000 => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 30)} month{((int)(-timeSpan.TotalDays / 30) > 1 ? "s" : "")}",
            _ => timeSpan.TotalDays > 0
                ? $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")} ago"
                : $"in {(int)(-timeSpan.TotalDays / 365)} year{((int)(-timeSpan.TotalDays / 365) > 1 ? "s" : "")}"
        };
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a human-readable duration string (e.g., "2h 30m 15s").
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>A human-readable duration string.</returns>
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
    /// Formats a <see cref="TimeSpan"/> as milliseconds with appropriate unit (ms, s, m, h).
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>A formatted duration string with appropriate unit.</returns>
    public static string ToFormattedDuration(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalMilliseconds < 1000)
            return string.Create(CultureInfo.InvariantCulture, $"{timeSpan.TotalMilliseconds:F0}ms");

        if (timeSpan.TotalSeconds < 60)
            return string.Create(CultureInfo.InvariantCulture, $"{timeSpan.TotalSeconds:F2}s");

        if (timeSpan.TotalMinutes < 60)
            return string.Create(CultureInfo.InvariantCulture, $"{timeSpan.TotalMinutes:F2}m");

        return string.Create(CultureInfo.InvariantCulture, $"{timeSpan.TotalHours:F2}h");
    }

    /// <summary>
    /// Converts milliseconds to a formatted duration string.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to convert.</param>
    /// <returns>A human-readable duration string.</returns>
    public static string MillisecondsToReadable(this long milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds).ToReadableDuration();
    }

    /// <summary>
    /// Gets the start of the day for a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to get the start of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing the start of the day (midnight).</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day for a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to get the end of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing the end of the day (just before midnight).</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week for a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to get the start of week for.</param>
    /// <param name="startDayOfWeek">The day of week to consider as the start of the week. Defaults to Monday.</param>
    /// <returns>A <see cref="DateTime"/> representing the start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startDayOfWeek)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the start of the month for a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to get the start of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month for a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time to get the end of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the month.</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1).EndOfDay();
    }

    /// <summary>
    /// Checks if a <see cref="DateTime"/> is in the past.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the date is in the past; otherwise, <see langword="false"/>.</returns>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a <see cref="DateTime"/> is in the future.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the date is in the future; otherwise, <see langword="false"/>.</returns>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a <see cref="DateTime"/> is today.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the date is today; otherwise, <see langword="false"/>.</returns>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Formats a <see cref="DateTime"/> as ISO 8601 string for API calls.
    /// </summary>
    /// <param name="dateTime">The date and time to format.</param>
    /// <returns>An ISO 8601 formatted date-time string.</returns>
    public static string ToIso8601String(this DateTime dateTime) =>
        dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the number of business days between two dates.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <param name="holidayProvider">
    /// An optional <see cref="IHolidayProvider"/> whose holiday dates are also excluded from the count.
    /// When omitted, only weekends (Saturday and Sunday) are excluded.
    /// </param>
    /// <returns>The number of business days (Monday-Friday, excluding holidays) between the two dates, inclusive of both boundaries.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startDate"/> is later than <paramref name="endDate"/>.</exception>
    public static int BusinessDaysBetween(this DateTime startDate, DateTime endDate, IHolidayProvider? holidayProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startDate, endDate);

        var holidays = holidayProvider ?? NoHolidayProvider.Instance;
        var businessDays = 0;
        var current = startDate.Date;
        var last = endDate.Date;

        while (current <= last)
        {
            if (current.IsBusinessDay(holidays))
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// Determines whether the given date is a business day: not a weekend and not reported as a holiday.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="holidayProvider">
    /// An optional <see cref="IHolidayProvider"/> used to exclude holidays. When omitted, only weekends are excluded.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="date"/> is a weekday and not a holiday; otherwise, <see langword="false"/>.</returns>
    public static bool IsBusinessDay(this DateTime date, IHolidayProvider? holidayProvider = null)
    {
        var holidays = holidayProvider ?? NoHolidayProvider.Instance;

        return date.DayOfWeek != DayOfWeek.Saturday
            && date.DayOfWeek != DayOfWeek.Sunday
            && !holidays.IsHoliday(date);
    }

    /// <summary>
    /// Adds (or subtracts, for negative values) a number of business days to a <see cref="DateTime"/>,
    /// skipping weekends and any holidays reported by <paramref name="holidayProvider"/>.
    /// </summary>
    /// <remarks>
    /// Semantics:
    /// <list type="bullet">
    /// <item><description><paramref name="businessDays"/> of 0 returns <paramref name="date"/> unchanged, even if it falls on a weekend or holiday.</description></item>
    /// <item><description>Positive values walk forward in time; negative values walk backward. Weekends/holidays are never counted as a step, so the start date itself is never counted as one of the stepped days.</description></item>
    /// <item><description>The starting date's own weekend/holiday status does not affect the count of days walked - only the days landed on while stepping are checked.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="date">The date to offset.</param>
    /// <param name="businessDays">The number of business days to add. Negative values move backward in time.</param>
    /// <param name="holidayProvider">
    /// An optional <see cref="IHolidayProvider"/> whose holiday dates are also skipped.
    /// When omitted, only weekends (Saturday and Sunday) are skipped.
    /// </param>
    /// <returns>The resulting <see cref="DateTime"/>, offset by the requested number of business days.</returns>
    public static DateTime AddBusinessDays(this DateTime date, int businessDays, IHolidayProvider? holidayProvider = null)
    {
        if (businessDays == 0)
            return date;

        var holidays = holidayProvider ?? NoHolidayProvider.Instance;
        var direction = Math.Sign(businessDays);
        var remaining = Math.Abs(businessDays);
        var current = date;

        while (remaining > 0)
        {
            current = current.AddDays(direction);
            if (current.IsBusinessDay(holidays))
                remaining--;
        }

        return current;
    }

    /// <summary>
    /// Rounds a <see cref="DateTime"/> to the nearest minute.
    /// </summary>
    /// <param name="dateTime">The date and time to round.</param>
    /// <returns>A <see cref="DateTime"/> with seconds and sub-seconds set to zero.</returns>
    public static DateTime RoundToMinute(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
    }

    /// <summary>
    /// Converts a Unix timestamp (seconds since epoch) to <see cref="DateTime"/>.
    /// </summary>
    /// <param name="unixTimestamp">The Unix timestamp in seconds.</param>
    /// <returns>A <see cref="DateTime"/> representing the timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the timestamp is outside the valid range for DateTime conversion.</exception>
    public static DateTime FromUnixTimestamp(this long unixTimestamp)
    {
        var maxDate = DateTime.MaxValue.AddYears(-1);
        var minDate = DateTime.MinValue.AddYears(1);
        var maxSeconds = (long)(maxDate - DateTime.UnixEpoch).TotalSeconds;
        var minSeconds = (long)(minDate - DateTime.UnixEpoch).TotalSeconds;

        ArgumentOutOfRangeException.ThrowIfLessThan(unixTimestamp, minSeconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(unixTimestamp, maxSeconds);

        return DateTime.UnixEpoch.AddSeconds(unixTimestamp);
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> to Unix timestamp (seconds since epoch).
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    public static long ToUnixTimestamp(this DateTime dateTime) =>
        (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
}