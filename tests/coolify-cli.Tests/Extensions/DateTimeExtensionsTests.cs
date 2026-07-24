using System;
using System.Collections.Generic;
using CoolifyCli.Extensions;
using Xunit;

namespace CoolifyCli.Tests.Extensions
{
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void ToRelativeTime_JustNow_WhenSecondsLessThan1()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddSeconds(-1);
            var expected = "just now";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_SecondsAgo_WhenSecondsBetween1And60()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddSeconds(-30);
            var expected = "30 seconds ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_MinutesAgo_WhenMinutesBetween1And60()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddMinutes(-45);
            var expected = "45 minutes ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_HoursAgo_WhenHoursBetween1And24()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddHours(-8);
            var expected = "8 hours ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_DaysAgo_WhenDaysBetween1And7()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddDays(-3);
            var expected = "3 days ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_WeeksAgo_WhenDaysBetween7And30()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddDays(-21);
            var expected = "3 weeks ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_MonthsAgo_WhenDaysBetween30And365()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddDays(-90);
            var expected = "3 months ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_YearsAgo_WhenDaysGreaterThan365()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddDays(-400);
            var expected = "1 year ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_SingularForm_WhenOneUnit()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddSeconds(-1);
            var expected = "1 second ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_PluralForm_WhenMultipleUnits()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddSeconds(-2);
            var expected = "2 seconds ago";

            // Act
            var actual = dateTime.ToRelativeTime();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToReadableDuration_WithDaysHoursMinutesSeconds()
        {
            // Arrange
            var timeSpan = new TimeSpan(3, 4, 30, 15);
            var expected = "3d 4h 30m 15s";

            // Act
            var actual = timeSpan.ToReadableDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToReadableDuration_WithHoursMinutesSeconds()
        {
            // Arrange
            var timeSpan = new TimeSpan(0, 2, 15, 30);
            var expected = "2h 15m 30s";

            // Act
            var actual = timeSpan.ToReadableDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToReadableDuration_WithMinutesSeconds()
        {
            // Arrange
            var timeSpan = new TimeSpan(0, 0, 45, 30);
            var expected = "45m 30s";

            // Act
            var actual = timeSpan.ToReadableDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToReadableDuration_WithSecondsOnly()
        {
            // Arrange
            var timeSpan = new TimeSpan(0, 0, 0, 45);
            var expected = "45s";

            // Act
            var actual = timeSpan.ToReadableDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToReadableDuration_WithZeroValues()
        {
            // Arrange
            var timeSpan = TimeSpan.Zero;
            var expected = "0s";

            // Act
            var actual = timeSpan.ToReadableDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToFormattedDuration_Milliseconds()
        {
            // Arrange
            var timeSpan = TimeSpan.FromMilliseconds(456);
            var expected = "456ms";

            // Act
            var actual = timeSpan.ToFormattedDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToFormattedDuration_Seconds()
        {
            // Arrange
            var timeSpan = TimeSpan.FromSeconds(30.5);
            var expected = "30.50s";

            // Act
            var actual = timeSpan.ToFormattedDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToFormattedDuration_Minutes()
        {
            // Arrange
            var timeSpan = TimeSpan.FromMinutes(2.5);
            var expected = "2.50m";

            // Act
            var actual = timeSpan.ToFormattedDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToFormattedDuration_Hours()
        {
            // Arrange
            var timeSpan = TimeSpan.FromHours(3.5);
            var expected = "3.50h";

            // Act
            var actual = timeSpan.ToFormattedDuration();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MillisecondsToReadable_ConvertsCorrectly()
        {
            // Arrange
            var milliseconds = 250000L; // 250 seconds
            var expected = "4m 10s";

            // Act
            var actual = milliseconds.MillisecondsToReadable();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartOfDay_ReturnsMidnight()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 15, 0, 0, 0);

            // Act
            var actual = dateTime.StartOfDay();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EndOfDay_ReturnsJustBeforeMidnight()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 15, 23, 59, 59, 999);

            // Act
            var actual = dateTime.EndOfDay();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartOfWeek_WithDefaultMonday_ReturnsCorrectDay()
        {
            // Arrange - June 15, 2022 was a Wednesday
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 13, 0, 0, 0); // Monday

            // Act
            var actual = dateTime.StartOfWeek();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartOfWeek_WithSundayStart_ReturnsCorrectDay()
        {
            // Arrange - June 15, 2022 was a Wednesday
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 12, 0, 0, 0); // Sunday

            // Act
            var actual = dateTime.StartOfWeek(DayOfWeek.Sunday);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StartOfMonth_ReturnsFirstDayOfMonth()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 1, 0, 0, 0);

            // Act
            var actual = dateTime.StartOfMonth();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EndOfMonth_ReturnsLastDayOfMonth()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 30, 23, 59, 59, 999);

            // Act
            var actual = dateTime.EndOfMonth();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsPast_ReturnsTrue_WhenDateIsInPast()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddMinutes(-1);

            // Act
            var actual = dateTime.IsPast();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsPast_ReturnsFalse_WhenDateIsInFuture()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddMinutes(1);

            // Act
            var actual = dateTime.IsPast();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsFuture_ReturnsTrue_WhenDateIsInFuture()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddMinutes(1);

            // Act
            var actual = dateTime.IsFuture();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsFuture_ReturnsFalse_WhenDateIsInPast()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddMinutes(-1);

            // Act
            var actual = dateTime.IsFuture();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsToday_ReturnsTrue_WhenDateIsToday()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.Date;

            // Act
            var actual = dateTime.IsToday();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsToday_ReturnsFalse_WhenDateIsNotToday()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddDays(-1);

            // Act
            var actual = dateTime.IsToday();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void ToIso8601String_FormatsCorrectly()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45, DateTimeKind.Utc);
            var expected = "2022-06-15T14:30:45.0000000Z";

            // Act
            var actual = dateTime.ToIso8601String();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BusinessDaysBetween_CalculatesCorrectly()
        {
            // Arrange
            var startDate = new DateTime(2022, 6, 1); // Wednesday
            var endDate = new DateTime(2022, 6, 10); // Friday (next week)
            var expected = 8; // 5 days in first week + 3 days in second week

            // Act
            var actual = startDate.BusinessDaysBetween(endDate);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BusinessDaysBetween_ExcludesWeekends()
        {
            // Arrange
            var startDate = new DateTime(2022, 6, 4); // Saturday
            var endDate = new DateTime(2022, 6, 5); // Sunday
            var expected = 0;

            // Act
            var actual = startDate.BusinessDaysBetween(endDate);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BusinessDaysBetween_InclusiveRange()
        {
            // Arrange
            var startDate = new DateTime(2022, 6, 1); // Wednesday
            var endDate = new DateTime(2022, 6, 1); // Wednesday
            var expected = 1;

            // Act
            var actual = startDate.BusinessDaysBetween(endDate);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RoundToMinute_RoundsDown()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45);
            var expected = new DateTime(2022, 6, 15, 14, 30, 0);

            // Act
            var actual = dateTime.RoundToMinute();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RoundToMinute_RoundsUp()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 59);
            var expected = new DateTime(2022, 6, 15, 14, 30, 0);

            // Act
            var actual = dateTime.RoundToMinute();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FromUnixTimestamp_ConvertsCorrectly()
        {
            // Arrange
            var unixTimestamp = 1655295045L; // June 15, 2022 14:30:45 UTC
            var expected = new DateTime(2022, 6, 15, 14, 30, 45, DateTimeKind.Utc);

            // Act
            var actual = unixTimestamp.FromUnixTimestamp();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToUnixTimestamp_ConvertsCorrectly()
        {
            // Arrange
            var dateTime = new DateTime(2022, 6, 15, 14, 30, 45, DateTimeKind.Utc);
            var expected = 1655295045L;

            // Act
            var actual = dateTime.ToUnixTimestamp();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToRelativeTime_KindLocal_ConvertsCorrectly()
        {
            // Arrange - Test that Local kind is properly converted to UTC
            var localTime = DateTime.Now.AddMinutes(-5);
            var utcTime = DateTime.UtcNow.AddMinutes(-5);

            // Act
            var localResult = localTime.ToRelativeTime();
            var utcResult = utcTime.ToRelativeTime();

            // Assert - Both should produce similar results (accounting for conversion)
            Assert.Equal("5 minutes ago", localResult);
            Assert.Equal("5 minutes ago", utcResult);
        }

        [Fact]
        public void ToRelativeTime_KindUtc_UsesAsIs()
        {
            // Arrange
            var utcTime = DateTime.UtcNow.AddMinutes(-10);

            // Act
            var result = utcTime.ToRelativeTime();

            // Assert
            Assert.Equal("10 minutes ago", result);
        }

        [Fact]
        public void ToRelativeTime_KindUnspecified_TreatsAsUtc()
        {
            // Arrange - Unspecified kind should be treated as UTC
            var unspecifiedTime = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-15), DateTimeKind.Unspecified);

            // Act
            var result = unspecifiedTime.ToRelativeTime();

            // Assert
            Assert.Equal("15 minutes ago", result);
        }

        [Fact]
        public void ToRelativeTime_FutureTime_ShowsInFormat()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddMinutes(5);

            // Act
            var result = futureTime.ToRelativeTime();

            // Assert
            Assert.Equal("in 5 minutes", result);
        }

        [Fact]
        public void ToRelativeTime_FutureSeconds_ShowsInFormat()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddSeconds(30);

            // Act
            var result = futureTime.ToRelativeTime();

            // Assert
            Assert.Equal("in 30 seconds", result);
        }

        [Fact]
        public void ToRelativeTime_FutureHours_ShowsInFormat()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddHours(2);

            // Act
            var result = futureTime.ToRelativeTime();

            // Assert
            Assert.Equal("in 2 hours", result);
        }

        [Fact]
        public void ToRelativeTime_FutureDays_ShowsInFormat()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddDays(3);

            // Act
            var result = futureTime.ToRelativeTime();

            // Assert
            Assert.Equal("in 3 days", result);
        }

        [Fact]
        public void ToRelativeTime_DateTimeOffset_ConvertsCorrectly()
        {
            // Arrange
            var offsetTime = DateTimeOffset.UtcNow.AddMinutes(-7);

            // Act
            var result = offsetTime.ToRelativeTime();

            // Assert
            Assert.Equal("7 minutes ago", result);
        }

        [Fact]
        public void ToRelativeTime_DateTimeOffsetFuture_ShowsInFormat()
        {
            // Arrange
            var offsetTime = DateTimeOffset.UtcNow.AddMinutes(10);

            // Act
            var result = offsetTime.ToRelativeTime();

            // Assert
            Assert.Equal("in 10 minutes", result);
        }

        [Fact]
        public void ToRelativeTime_DateTimeOffsetWithOffset_ConvertsCorrectly()
        {
            // Arrange - Test with a specific offset (e.g., UTC+2)
            var offset = TimeSpan.FromHours(2);
            var offsetTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-12), offset);

            // Act
            var result = offsetTime.ToRelativeTime();

            // Assert - Should still show correct relative time regardless of offset
            Assert.Equal("12 minutes ago", result);
        }

        [Fact]
        public void ToRelativeTime_NullDateTime_ThrowsArgumentNullException()
        {
            // Arrange
            DateTime? nullDateTime = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullDateTime!.Value.ToRelativeTime());
        }

        [Fact]
        public void ToRelativeTime_NullDateTimeOffset_ThrowsArgumentNullException()
        {
            // Arrange
            DateTimeOffset? nullDateTimeOffset = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullDateTimeOffset!.Value.ToRelativeTime());
        }

        [Fact]
        public void AddBusinessDays_ZeroDays_ReturnsSameDate()
        {
            // Arrange - start on a Saturday, which is not itself a business day
            var saturday = new DateTime(2022, 6, 4);

            // Act
            var actual = saturday.AddBusinessDays(0);

            // Assert
            Assert.Equal(saturday, actual);
        }

        [Fact]
        public void AddBusinessDays_StartOnSaturday_NegativeThreeDays_WalksBackward()
        {
            // Arrange - Saturday June 4, 2022; stepping back 3 business days should
            // skip the weekend entirely and land on Wednesday June 1, 2022
            // (Fri Jun 3, Thu Jun 2, Wed Jun 1)
            var saturday = new DateTime(2022, 6, 4);
            var expected = new DateTime(2022, 6, 1);

            // Act
            var actual = saturday.AddBusinessDays(-3);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddBusinessDays_StartOnSaturday_PositiveThreeDays_WalksForward()
        {
            // Arrange - Saturday June 4, 2022; stepping forward 3 business days should
            // skip the weekend and land on Wednesday June 8, 2022
            // (Mon Jun 6, Tue Jun 7, Wed Jun 8)
            var saturday = new DateTime(2022, 6, 4);
            var expected = new DateTime(2022, 6, 8);

            // Act
            var actual = saturday.AddBusinessDays(3);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddBusinessDays_WithHolidayProvider_SkipsHolidays()
        {
            // Arrange - Wednesday June 1, 2022; June 2 is a holiday, so adding 1
            // business day should skip both the holiday and land past it
            var start = new DateTime(2022, 6, 1);
            var holidayProvider = new FixedHolidayProvider(new[] { new DateTime(2022, 6, 2) });
            var expected = new DateTime(2022, 6, 3);

            // Act
            var actual = start.AddBusinessDays(1, holidayProvider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BusinessDaysBetween_WithHolidayProvider_ExcludesHolidays()
        {
            // Arrange - Monday June 6 through Friday June 10, 2022, with Wednesday
            // June 8 marked as a holiday
            var startDate = new DateTime(2022, 6, 6);
            var endDate = new DateTime(2022, 6, 10);
            var holidayProvider = new FixedHolidayProvider(new[] { new DateTime(2022, 6, 8) });
            var expected = 4;

            // Act
            var actual = startDate.BusinessDaysBetween(endDate, holidayProvider);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsBusinessDay_ReturnsFalse_ForWeekend()
        {
            // Arrange
            var saturday = new DateTime(2022, 6, 4);

            // Act
            var actual = saturday.IsBusinessDay();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsBusinessDay_ReturnsFalse_ForHoliday()
        {
            // Arrange
            var wednesday = new DateTime(2022, 6, 1);
            var holidayProvider = new FixedHolidayProvider(new[] { wednesday });

            // Act
            var actual = wednesday.IsBusinessDay(holidayProvider);

            // Assert
            Assert.False(actual);
        }
    }
}