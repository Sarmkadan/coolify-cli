#nullable enable

using CoolifyCli.Extensions;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Provides unit tests for the DateTime extension methods in the CoolifyCli.Extensions namespace.
/// Tests cover relative time formatting, duration formatting, date manipulation, and business day calculations.
/// </summary>
public class DateTimeExtensionsTests
{
    // ---- ToRelativeTime ------------------------------------------------------

    /// <summary>
    /// Tests that a DateTime within the last 500 milliseconds returns "just now".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenJustNow_ReturnsJustNow()
    {
        var result = DateTime.UtcNow.AddMilliseconds(-500).ToRelativeTime();

        result.Should().Be("just now");
    }

    /// <summary>
    /// Tests that a DateTime approximately 30 seconds in the past returns "30 seconds ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenFewSecondsAgo_ReturnsSecondsAgo()
    {
        var result = DateTime.UtcNow.AddSeconds(-30).ToRelativeTime();

        result.Should().Be("30 seconds ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 1 minute in the past returns a string starting with "1 minute" and ending with "ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenAboutOneMinuteAgo_ReturnsMinuteAgo()
    {
        var result = DateTime.UtcNow.AddMinutes(-1).ToRelativeTime();

        result.Should().StartWith("1 minute");
        result.Should().EndWith("ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 3 hours in the past returns "3 hours ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenSeveralHoursAgo_ReturnsHoursAgo()
    {
        var result = DateTime.UtcNow.AddHours(-3).ToRelativeTime();

        result.Should().Be("3 hours ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 5 days in the past returns "5 days ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenSeveralDaysAgo_ReturnsDaysAgo()
    {
        var result = DateTime.UtcNow.AddDays(-5).ToRelativeTime();

        result.Should().Be("5 days ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 2 weeks in the past returns "2 weeks ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenTwoWeeksAgo_ReturnsWeeksAgo()
    {
        var result = DateTime.UtcNow.AddDays(-14).ToRelativeTime();

        result.Should().Be("2 weeks ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 3 months in the past returns "3 months ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenSeveralMonthsAgo_ReturnsMonthsAgo()
    {
        var result = DateTime.UtcNow.AddDays(-90).ToRelativeTime();

        result.Should().Be("3 months ago");
    }

    /// <summary>
    /// Tests that a DateTime approximately 2 years in the past returns "2 years ago".
    /// </summary>
    [Fact]
    public void ToRelativeTime_WhenOverAYearAgo_ReturnsYearsAgo()
    {
        var result = DateTime.UtcNow.AddDays(-730).ToRelativeTime();

        result.Should().Be("2 years ago");
    }

    // ---- ToReadableDuration --------------------------------------------------

    /// <summary>
    /// Tests that a TimeSpan with only seconds component returns a string with just the seconds value followed by 's'.
    /// </summary>
    [Fact]
    public void ToReadableDuration_WithOnlySeconds_ShowsSecondsComponent()
    {
        var ts = TimeSpan.FromSeconds(45);

        ts.ToReadableDuration().Should().Be("45s");
    }

    /// <summary>
    /// Tests that a TimeSpan with hours, minutes, and seconds components returns a formatted string with all components.
    /// </summary>
    [Fact]
    public void ToReadableDuration_WithHoursMinutesSeconds_ShowsAllComponents()
    {
        var ts = new TimeSpan(2, 30, 15);

        ts.ToReadableDuration().Should().Be("2h 30m 15s");
    }

    /// <summary>
    /// Tests that a TimeSpan with days and hours components returns a formatted string including the day component.
    /// </summary>
    [Fact]
    public void ToReadableDuration_WithDays_IncludesDayComponent()
    {
        var ts = new TimeSpan(3, 1, 0, 0);

        ts.ToReadableDuration().Should().Be("3d 1h");
    }

    /// <summary>
    /// Tests that a zero TimeSpan returns "0s" as its readable duration representation.
    /// </summary>
    [Fact]
    public void ToReadableDuration_WithZeroTimeSpan_ShowsZeroSeconds()
    {
        var ts = TimeSpan.Zero;

        ts.ToReadableDuration().Should().Be("0s");
    }

    // ---- ToFormattedDuration -------------------------------------------------

    /// <summary>
    /// Tests that a TimeSpan in the sub-second range returns a string showing milliseconds.
    /// </summary>
    [Fact]
    public void ToFormattedDuration_SubSecondRange_ShowsMilliseconds()
    {
        var ts = TimeSpan.FromMilliseconds(500);

        ts.ToFormattedDuration().Should().Be("500ms");
    }

    /// <summary>
    /// Tests that a TimeSpan in the second range returns a string showing seconds with two decimal places.
    /// </summary>
    [Fact]
    public void ToFormattedDuration_SecondRange_ShowsSeconds()
    {
        var ts = TimeSpan.FromSeconds(10);

        ts.ToFormattedDuration().Should().Be("10.00s");
    }

    /// <summary>
    /// Tests that a TimeSpan in the minute range returns a string showing minutes with two decimal places.
    /// </summary>
    [Fact]
    public void ToFormattedDuration_MinuteRange_ShowsMinutes()
    {
        var ts = TimeSpan.FromMinutes(5);

        ts.ToFormattedDuration().Should().Be("5.00m");
    }

    // ---- StartOfDay / EndOfDay -----------------------------------------------

    /// <summary>
    /// Tests that StartOfDay() returns a DateTime with the same date but time set to midnight (00:00:00).
    /// </summary>
    [Fact]
    public void StartOfDay_ReturnsDateWithZeroTime()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);

        var start = dt.StartOfDay();

        start.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
    }

    /// <summary>
    /// Tests that EndOfDay() returns a DateTime with the same date but time set to just before midnight (23:59:59.9999999).
    /// </summary>
    [Fact]
    public void EndOfDay_ReturnsLastTickOfDay()
    {
        var dt = new DateTime(2024, 6, 15, 8, 0, 0);

        var end = dt.EndOfDay();

        end.Date.Should().Be(new DateTime(2024, 6, 15));
        end.TimeOfDay.Should().BeGreaterThan(TimeSpan.FromHours(23));
    }

    // ---- StartOfWeek ---------------------------------------------------------

    /// <summary>
    /// Tests that StartOfWeek() with DayOfWeek.Monday returns the preceding Monday for a given Wednesday.
    /// </summary>
    [Fact]
    public void StartOfWeek_WithMondayStart_ReturnsPrecedingMonday()
    {
        var wednesday = new DateTime(2024, 6, 19); // Wednesday

        var monday = wednesday.StartOfWeek(DayOfWeek.Monday);

        monday.DayOfWeek.Should().Be(DayOfWeek.Monday);
        monday.Should().Be(new DateTime(2024, 6, 17));
    }

    // ---- StartOfMonth / EndOfMonth -------------------------------------------

    /// <summary>
    /// Tests that StartOfMonth() returns the first day of the month (day 1) for any DateTime in that month.
    /// </summary>
    [Fact]
    public void StartOfMonth_ReturnsFirstDayOfMonth()
    {
        var dt = new DateTime(2024, 3, 20);

        dt.StartOfMonth().Should().Be(new DateTime(2024, 3, 1));
    }

    /// <summary>
    /// Tests that EndOfMonth() returns the last day of the month, correctly handling month boundaries including leap years.
    /// </summary>
    [Fact]
    public void EndOfMonth_ReturnsLastDayOfMonth()
    {
        var dt = new DateTime(2024, 2, 10); // Feb in leap year

        var end = dt.EndOfMonth();

        end.Month.Should().Be(2);
        end.Day.Should().Be(29);
    }

    // ---- IsPast / IsFuture ---------------------------------------------------

    /// <summary>
    /// Tests that IsPast() returns true for a DateTime that is in the past.
    /// </summary>
    [Fact]
    public void IsPast_ForPastDateTime_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(-1).IsPast().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsFuture() returns true for a DateTime that is in the future.
    /// </summary>
    [Fact]
    public void IsFuture_ForFutureDateTime_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(1).IsFuture().Should().BeTrue();
    }

    // ---- ToIso8601String -----------------------------------------------------

    /// <summary>
    /// Tests that ToIso8601String() produces a string that can be parsed back to the original DateTime using roundtrip formatting.
    /// </summary>
    [Fact]
    public void ToIso8601String_ProducesRoundtrippableString()
    {
        var original = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var iso = original.ToIso8601String();
        var parsed = DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind);

        parsed.ToUniversalTime().Should().Be(original);
    }

    // ---- Unix timestamp ------------------------------------------------------

    /// <summary>
    /// Tests that ToUnixTimestamp() and FromUnixTimestamp() can roundtrip a DateTime through Unix timestamp format.
    /// </summary>
    [Fact]
    public void ToUnixTimestamp_AndBack_RoundtripsCorrectly()
    {
        var original = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var ts = original.ToUnixTimestamp();
        var restored = ts.FromUnixTimestamp();

        restored.Should().Be(original);
    }

    // ---- BusinessDaysBetween -------------------------------------------------

    /// <summary>
    /// Tests that BusinessDaysBetween() counts only weekdays (Monday-Friday) when the date range spans a full workweek.
    /// </summary>
    [Fact]
    public void BusinessDaysBetween_WorkweekOnly_CountsFiveBusinessDays()
    {
        var monday = new DateTime(2024, 6, 17);
        var friday = new DateTime(2024, 6, 21);

        var count = monday.BusinessDaysBetween(friday);

        count.Should().Be(5);
    }

    /// <summary>
    /// Tests that BusinessDaysBetween() excludes weekends (Saturday and Sunday) when the date range spans a weekend.
    /// </summary>
    [Fact]
    public void BusinessDaysBetween_SpanningWeekend_ExcludesSaturdayAndSunday()
    {
        var friday = new DateTime(2024, 6, 21);
        var monday = new DateTime(2024, 6, 24);

        var count = friday.BusinessDaysBetween(monday);

        count.Should().Be(2); // Friday + Monday
    }

    // ---- RoundToMinute -------------------------------------------------------

    /// <summary>
    /// Tests that RoundToMinute() truncates seconds and sub-seconds, rounding down to the nearest minute.
    /// </summary>
    [Fact]
    public void RoundToMinute_TruncatesSecondsAndSubSeconds()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 45);

        var rounded = dt.RoundToMinute();

        rounded.Second.Should().Be(0);
        rounded.Minute.Should().Be(30);
    }
}
