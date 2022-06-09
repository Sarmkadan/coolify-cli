// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
#nullable enable

using CoolifyCli.Extensions;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class DateTimeExtensionsTests
{
    // ---- ToRelativeTime ------------------------------------------------------

    [Fact]
    public void ToRelativeTime_WhenJustNow_ReturnsJustNow()
    {
        var result = DateTime.UtcNow.AddMilliseconds(-500).ToRelativeTime();

        result.Should().Be("just now");
    }

    [Fact]
    public void ToRelativeTime_WhenFewSecondsAgo_ReturnsSecondsAgo()
    {
        var result = DateTime.UtcNow.AddSeconds(-30).ToRelativeTime();

        result.Should().Be("30 seconds ago");
    }

    [Fact]
    public void ToRelativeTime_WhenAboutOneMinuteAgo_ReturnsMinuteAgo()
    {
        var result = DateTime.UtcNow.AddMinutes(-1).ToRelativeTime();

        result.Should().StartWith("1 minute");
        result.Should().EndWith("ago");
    }

    [Fact]
    public void ToRelativeTime_WhenSeveralHoursAgo_ReturnsHoursAgo()
    {
        var result = DateTime.UtcNow.AddHours(-3).ToRelativeTime();

        result.Should().Be("3 hours ago");
    }

    [Fact]
    public void ToRelativeTime_WhenSeveralDaysAgo_ReturnsDaysAgo()
    {
        var result = DateTime.UtcNow.AddDays(-5).ToRelativeTime();

        result.Should().Be("5 days ago");
    }

    [Fact]
    public void ToRelativeTime_WhenTwoWeeksAgo_ReturnsWeeksAgo()
    {
        var result = DateTime.UtcNow.AddDays(-14).ToRelativeTime();

        result.Should().Be("2 weeks ago");
    }

    [Fact]
    public void ToRelativeTime_WhenSeveralMonthsAgo_ReturnsMonthsAgo()
    {
        var result = DateTime.UtcNow.AddDays(-90).ToRelativeTime();

        result.Should().Be("3 months ago");
    }

    [Fact]
    public void ToRelativeTime_WhenOverAYearAgo_ReturnsYearsAgo()
    {
        var result = DateTime.UtcNow.AddDays(-730).ToRelativeTime();

        result.Should().Be("2 years ago");
    }

    // ---- ToReadableDuration --------------------------------------------------

    [Fact]
    public void ToReadableDuration_WithOnlySeconds_ShowsSecondsComponent()
    {
        var ts = TimeSpan.FromSeconds(45);

        ts.ToReadableDuration().Should().Be("45s");
    }

    [Fact]
    public void ToReadableDuration_WithHoursMinutesSeconds_ShowsAllComponents()
    {
        var ts = new TimeSpan(2, 30, 15);

        ts.ToReadableDuration().Should().Be("2h 30m 15s");
    }

    [Fact]
    public void ToReadableDuration_WithDays_IncludesDayComponent()
    {
        var ts = new TimeSpan(3, 1, 0, 0);

        ts.ToReadableDuration().Should().Be("3d 1h");
    }

    [Fact]
    public void ToReadableDuration_WithZeroTimeSpan_ShowsZeroSeconds()
    {
        var ts = TimeSpan.Zero;

        ts.ToReadableDuration().Should().Be("0s");
    }

    // ---- ToFormattedDuration -------------------------------------------------

    [Fact]
    public void ToFormattedDuration_SubSecondRange_ShowsMilliseconds()
    {
        var ts = TimeSpan.FromMilliseconds(500);

        ts.ToFormattedDuration().Should().Be("500ms");
    }

    [Fact]
    public void ToFormattedDuration_SecondRange_ShowsSeconds()
    {
        var ts = TimeSpan.FromSeconds(10);

        ts.ToFormattedDuration().Should().Be("10.00s");
    }

    [Fact]
    public void ToFormattedDuration_MinuteRange_ShowsMinutes()
    {
        var ts = TimeSpan.FromMinutes(5);

        ts.ToFormattedDuration().Should().Be("5.00m");
    }

    // ---- StartOfDay / EndOfDay -----------------------------------------------

    [Fact]
    public void StartOfDay_ReturnsDateWithZeroTime()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);

        var start = dt.StartOfDay();

        start.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
    }

    [Fact]
    public void EndOfDay_ReturnsLastTickOfDay()
    {
        var dt = new DateTime(2024, 6, 15, 8, 0, 0);

        var end = dt.EndOfDay();

        end.Date.Should().Be(new DateTime(2024, 6, 15));
        end.TimeOfDay.Should().BeGreaterThan(TimeSpan.FromHours(23));
    }

    // ---- StartOfWeek ---------------------------------------------------------

    [Fact]
    public void StartOfWeek_WithMondayStart_ReturnsPrecedingMonday()
    {
        var wednesday = new DateTime(2024, 6, 19); // Wednesday

        var monday = wednesday.StartOfWeek(DayOfWeek.Monday);

        monday.DayOfWeek.Should().Be(DayOfWeek.Monday);
        monday.Should().Be(new DateTime(2024, 6, 17));
    }

    // ---- StartOfMonth / EndOfMonth -------------------------------------------

    [Fact]
    public void StartOfMonth_ReturnsFirstDayOfMonth()
    {
        var dt = new DateTime(2024, 3, 20);

        dt.StartOfMonth().Should().Be(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void EndOfMonth_ReturnsLastDayOfMonth()
    {
        var dt = new DateTime(2024, 2, 10); // Feb in leap year

        var end = dt.EndOfMonth();

        end.Month.Should().Be(2);
        end.Day.Should().Be(29);
    }

    // ---- IsPast / IsFuture ---------------------------------------------------

    [Fact]
    public void IsPast_ForPastDateTime_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(-1).IsPast().Should().BeTrue();
    }

    [Fact]
    public void IsFuture_ForFutureDateTime_ReturnsTrue()
    {
        DateTime.UtcNow.AddDays(1).IsFuture().Should().BeTrue();
    }

    // ---- ToIso8601String -----------------------------------------------------

    [Fact]
    public void ToIso8601String_ProducesRoundtrippableString()
    {
        var original = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var iso = original.ToIso8601String();
        var parsed = DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind);

        parsed.ToUniversalTime().Should().Be(original);
    }

    // ---- Unix timestamp ------------------------------------------------------

    [Fact]
    public void ToUnixTimestamp_AndBack_RoundtripsCorrectly()
    {
        var original = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var ts = original.ToUnixTimestamp();
        var restored = ts.FromUnixTimestamp();

        restored.Should().Be(original);
    }

    // ---- BusinessDaysBetween -------------------------------------------------

    [Fact]
    public void BusinessDaysBetween_WorkweekOnly_CountsFiveBusinessDays()
    {
        var monday = new DateTime(2024, 6, 17);
        var friday = new DateTime(2024, 6, 21);

        var count = monday.BusinessDaysBetween(friday);

        count.Should().Be(5);
    }

    [Fact]
    public void BusinessDaysBetween_SpanningWeekend_ExcludesSaturdayAndSunday()
    {
        var friday = new DateTime(2024, 6, 21);
        var monday = new DateTime(2024, 6, 24);

        var count = friday.BusinessDaysBetween(monday);

        count.Should().Be(2); // Friday + Monday
    }

    // ---- RoundToMinute -------------------------------------------------------

    [Fact]
    public void RoundToMinute_TruncatesSecondsAndSubSeconds()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 45);

        var rounded = dt.RoundToMinute();

        rounded.Second.Should().Be(0);
        rounded.Minute.Should().Be(30);
    }
}
